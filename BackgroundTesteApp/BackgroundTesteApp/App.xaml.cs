using BackgroundTesteApp.Services;
using Matcha.BackgroundService;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BackgroundTesteApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new Inicio();
        }

        protected override void OnStart()
        {
            StartBackgroundService();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private static void StartBackgroundService()
        {
            //Monitoring notifications every minute
            BackgroundAggregatorService.Add(() => new PeriodicMonitoringService(1));

            //Start Background Service
            BackgroundAggregatorService.StartBackgroundService();
        }
    }
}
