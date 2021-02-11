using Matcha.BackgroundService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTesteApp.Services
{
    class PeriodicMonitoringService : IPeriodicTask
    {
        public TimeSpan Interval { get; set; }

        public PeriodicMonitoringService(int seconds)
        {
            Interval = TimeSpan.FromSeconds(seconds);
        }

        public async Task<bool> StartJob()
        {
            BackgroundTestService.GetInstance();

            Console.WriteLine("TESTE: " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"));

            //Return true para continuar
            return true;

            //Return false para parar
            //return false;
        }
    }
}
