using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CoreSimconnect
{
    public enum SimConnectApiStatus
    {
        Disconnected,
        Connected
    }

    public class SimConnectApi
    {
        /// <summary>
        /// Contains the list of all the SimConnect properties we will read, the unit is separated by coma by our own code.
        /// </summary>
        Dictionary<int, string> simConnectProperties = new Dictionary<int, string>
        {
            {1,"PLANE LONGITUDE,degree" },
            {2,"PLANE LATITUDE,degree" },
            {3,"PLANE HEADING DEGREES MAGNETIC,degree" },
            {4,"PLANE ALTITUDE,feet" },
            {5,"AIRSPEED INDICATED,knots" },
        };

        Dictionary<int, double> simValues = new Dictionary<int, double>();

        /// User-defined win32 event => put basically any number?
        public const int WM_USER_SIMCONNECT = 0x0402;

        SimConnect sim;

        public IntPtr WindowHandle { get; }

        public SimConnectApi()
        {
            var w = MessagePumpWindow.GetWindow();
            WindowHandle = w.Hwnd;
            w.WndProcHandle += W_WndProcHandle;

            var runner = new Thread((obj)=>
            {
                while(true)
                {
                    Thread.Sleep(1000);
                    Timer_Tick(null, null);
                    //Microsoft.AspNetCore.Components.Dispatcher.
                    //Application.Current.MainWindow.BeginInvoke(Timer_Tick);
                }
            });
            runner.IsBackground=true;
            runner.Start();
        }

        private IntPtr W_WndProcHandle(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (msg == WM_USER_SIMCONNECT)
                    ReceiveSimConnectMessage();
            }
            catch
            {
                Disconnect();
            }

            return IntPtr.Zero;
        }

        public void ReceiveSimConnectMessage()
        {
            sim?.ReceiveMessage();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (sim == null) // We are not connected, let's try to connect
                Connect();
            else // We are connected, let's try to grab the data from the Sim
            {
                try
                {
                    foreach (var toConnect in simConnectProperties)
                        sim.RequestDataOnSimObjectType((DUMMYENUM)toConnect.Key, (DUMMYENUM)toConnect.Key, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                }
                catch
                {
                    Disconnect();
                }
            }
        }

        private void Connect()
        {
            /// The constructor is similar to SimConnect_Open in the native API
            try
            {
                // Pass the self defined ID which will be returned on WndProc
                sim = new SimConnect("CoreSimconnect", WindowHandle, WM_USER_SIMCONNECT, null, 0);
                sim.OnRecvOpen += Sim_OnRecvOpen;
                sim.OnRecvQuit += Sim_OnRecvQuit;
                sim.OnRecvSimobjectDataBytype += Sim_OnRecvSimobjectDataBytype;
            }
            catch
            {
                sim = null;
            }
        }

        private void Sim_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            lock (simValues)
            {
                simValues.Clear();
                foreach (var toConnect in simConnectProperties)
                {
                    var values = toConnect.Value.Split(new char[] { ',' });
                    /// Define a data structure
                    sim.AddToDataDefinition((DUMMYENUM)toConnect.Key, values[0], values[1], SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    /// IMPORTANT: Register it with the simconnect managed wrapper marshaller
                    /// If you skip this step, you will only receive a uint in the .dwData field.
                    sim.RegisterDataDefineStruct<double>((DUMMYENUM)toConnect.Key);
                    simValues.Add(toConnect.Key, 0);
                }
            }
        }

        private void Sim_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            int iRequest = (int)data.dwRequestID;
            double dValue = (double)data.dwData[0];

            lock (simValues)
                simValues[iRequest] = dValue;
        }

        public IEnumerable<KeyValuePair<string, double>> GetNamesAndValues()
        {
            lock (simValues)
                return simConnectProperties.Select(row => new KeyValuePair<string, double>(row.Value.Split(new char[] { ',' })[0], simValues.ContainsKey(row.Key) ? simValues[row.Key] : 0));
        }

        public double GetValue(int id)
        {
            lock (simValues)
                return simValues.ContainsKey(id) ? simValues[id] : 0;
        }

        private void Sim_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            sim = null;
        }


        /// <summary>
        /// Let's disconnect from SimConnect
        /// </summary>
        public void Disconnect()
        {
            if (sim != null)
            {
                sim.Dispose();
                sim = null;
                lock (simValues)
                    simValues.Clear();
            }
        }

        public SimConnectApiStatus Status => (sim == null ? SimConnectApiStatus.Disconnected : SimConnectApiStatus.Connected);
    }
}
