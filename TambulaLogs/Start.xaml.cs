#define DEBUG_LOG_AGENT

using System;
using System.Windows;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Controls;
using TambulaLogs.Utils;
using Microsoft.Phone.Scheduler;

namespace TambulaLogs
{
    public partial class Start : PhoneApplicationPage
    {
        #region DEPENDENCY PROPERTIES
        /// <summary>
        /// Latitude
        /// </summary>
        public static readonly DependencyProperty Lat = DependencyProperty.Register("Latitude", typeof(double), typeof(Start), new PropertyMetadata(0.0));
        public double Latitude
        {

            get { return (double) GetValue(Lat); }

            set
            {
                SetValue(Lat, value);
            }
        }

        /// <summary>
        /// Longitude
        /// </summary>
        public static readonly DependencyProperty Long = DependencyProperty.Register("Longitude", typeof(double), typeof(Start), new PropertyMetadata(0.0));
        public  double Longitude
        {
            get { return (double) GetValue(Long); }

            set
            {
                SetValue(Long, value);
            }
        }

        /// <summary>
        /// Battery Level
        /// </summary>
        public static readonly DependencyProperty Bat = DependencyProperty.Register("BatteryLevel", typeof(string), typeof(Start), new PropertyMetadata("battery_level"));
        public string BatteryLevel
        {
            get { return (string) GetValue(Bat); }

            set
            {
                SetValue(Bat, value);
            }
        }

        /// <summary>
        /// Device ID
        /// </summary>
        public static readonly DependencyProperty Id = DependencyProperty.Register("DeviceID", typeof(string), typeof(Start), new PropertyMetadata("device_id"));
        public string DeviceID
        {
            get { return (string) GetValue(Id); }

            set
            {
                SetValue(Id, value);
            }
        }
        #endregion

         PeriodicTask PhonePing;

        public Start()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        #region UPDATE FOREGROUND APP WITH DATA
        /// <summary>
        /// Run on start
        /// </summary>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateData();
        }

        /// <summary>
        /// Update data on home page
        /// </summary>
        private async void UpdateData()
        {
            //gps data 
            var gps = new Geolocator { DesiredAccuracyInMeters = 1, ReportInterval = 1, MovementThreshold = 3 };
            if (gps.LocationStatus == PositionStatus.Disabled) return;
            var position = await gps.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));

            //Update Geo DPs
            Latitude = position.Coordinate.Point.Position.Latitude;
            Longitude = position.Coordinate.Point.Position.Longitude;

            //update level Dp
            BatteryLevel = LogUtils.GetBatteryLevel();

            //update level DP
            DeviceID = LogUtils.GetDeviceInfo();

            StartLogAgent(); //-- NOTE: do better - switch to actual event which fires after last UI thread updated - otherwise, chaining could have issues in slower devices
        }
        #endregion

        #region BACKGROUND AGENT INIT
        /// <summary>
        /// Start Phone Ping background agent
        /// </summary>
        private void StartLogAgent()
        {
            
            PhonePing = ScheduledActionService.Find(LogUtils.TaskName) as PeriodicTask; //check if task already running

            if (PhonePing != null)
            {
                RemoveAgent(LogUtils.TaskName);
            }

            PhonePing = new PeriodicTask(LogUtils.TaskName) { Description = LogUtils.TaskDesc }; //create new instance

            try
            {
                ScheduledActionService.Add(PhonePing); //register background agent
            }
#if DEBUG_LOG_AGENT
            catch (InvalidOperationException e)  // -- TODO: REMOVE FOR STORE SUBMISSION --ONLY CATCHING FOR DEBUG or switch to friendlier error messaging
            {
                if (e.Message.Contains("BNS Error: The action is disabled"))
                {
                    MessageBox.Show("Background Agents disabled by user");
                }

                if (e.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added"))
                {
                    MessageBox.Show("Phone has registered maximum Background Agents. Disable some to enable Tambula Log Background Service");
                }
            }
            catch (SchedulerServiceException se)
            {
                MessageBox.Show(se.ToString());
            }
#endif
        }

        /// <summary>
        /// Unregister Background Agent
        /// </summary>
        /// <param name="backgroundAgentName"></param>
        private void RemoveAgent(string backgroundAgentName)
        {
            try
            {
                ScheduledActionService.Remove(backgroundAgentName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

    }
}