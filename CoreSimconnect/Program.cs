using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreSimconnect
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var coreRunner = new Thread((_) =>
              {
                  CreateHostBuilder(args).Build().Run();
              });
            coreRunner.Start();
            MessagePumpWindow.MessageLoop();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
