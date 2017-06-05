using System;
using System.Diagnostics;
using System.Windows;
using Windows.Devices.Geolocation;
using Windows.Phone.Devices.Power;
using Windows.Web.Http;
using Microsoft.Phone.Info;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using TambulaScheduledTaskAgent.Models;

namespace TambulaScheduledTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe -- exception event handler 
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }
        
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region RUN TASK
        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        protected override async void OnInvoke(ScheduledTask task)
        {

            //gps data
            var gps = new Geolocator { DesiredAccuracyInMeters = 1, ReportInterval = 1, MovementThreshold = 3 };
            var position = await gps.GetGeopositionAsync();

            //get battery level
            var battery = Battery.GetDefault().RemainingChargePercent.ToString();

            //device id
            var id = Convert.ToBase64String((byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId"));

            //new device data object
            var devicedata = new DeviceData
            {
                action = "device_ping",
                latitude =
                ((position.Coordinate.Point.Position.Latitude) > 0.0
                    ? position.Coordinate.Point.Position.Latitude
                    : 0.0),
                longitude =
                ((position.Coordinate.Point.Position.Longitude) > 0.0
                    ? position.Coordinate.Point.Position.Longitude
                    : 0.0),
                battery_level = ((!string.IsNullOrEmpty(battery)) ? battery : "00"),
                device_id = ((!string.IsNullOrEmpty(id)) ? id : "00")
            };


            if (!CheckInternet()) return;

            var json = JsonConvert.SerializeObject(devicedata); //serialize object

            var httpClient = new HttpClient();
            var resp = await httpClient.PostAsync(new Uri(DeviceData.BASE_URL), new HttpStringContent(json));

            if (resp.Content.ToString().Contains("successful"))
            {
                var toast = new ShellToast
                {
                    Title = "Tambula Log",
                    Content = "Log updated"
                };
                toast.Show();
            }

            NotifyComplete();
        }
        #endregion

        #region CHECK CONNECTIVITY
        /// <summary>
        /// Check internet
        /// </summary>
        /// <returns></returns>
        private static bool CheckInternet()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }
        #endregion
    }
}