using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreSimconnect
{
    [Route("api/[controller]")]
    public class SimConnectController : Controller
    {
        private SimConnectApi simApi;

        public SimConnectController(SimConnectApi simApi)
        {
            this.simApi = simApi;
        }

        /// <summary>
        /// Returns the list of SimConnect values with their names and values
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<KeyValuePair<string,double>> GetValues()
        {
            return simApi.GetNamesAndValues();
        }

        /// <summary>
        /// Returns a value for a given name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public double Get(int id)
        {
            return simApi.GetValue(id);
        }

        /// <summary>
        /// Returns the status of the connection to the simulator
        /// </summary>
        /// <returns></returns>
        [HttpGet("/status")]
        public SimConnectApiStatus GetStatus()
        {
            return simApi.Status;
        }
    }
}
