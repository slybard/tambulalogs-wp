using System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Windows.Phone.Devices.Power;

namespace TambulaLogs.Utils
{
    public static class LogUtils
    {
        #region PROPERTIES
        /// <summary>
        /// delegate to handle callbacks - //NOTE: consider going with actions
        /// </summary>
        public delegate void LogDelegate();

        /// <summary>
        /// Empty Message Delegate
        /// </summary>
        public static readonly LogDelegate EmptyMessageDelegate = () => { };

        /// <summary>
        /// Task Name
        /// </summary>
        public const string TaskName = "TambulaScheduledTaskAgent";

        /// <summary>
        /// Tambula task description
        /// </summary>
        public const string TaskDesc = "Log Tambula Agent phone location and battery power";

        #endregion

        #region DEVICE DATA
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceInfo()
        {
            var deviceid = (byte[]) DeviceExtendedProperties.GetValue("DeviceUniqueId"); //is object(byte[]). MUST cast to byte[]
            //return BitConverter.ToString(deviceid); //return as string

            return Convert.ToBase64String(deviceid); //base64string
        }

        /// <summary>
        /// return battery level
        /// </summary>
        /// <returns></returns>
        public static string GetBatteryLevel()
        {
            return Battery.GetDefault().RemainingChargePercent.ToString();
        }
        #endregion

        #region MESSAGES

        /// <summary>
        /// Handle Pushing Custom message boxes with messages and respective actions based on selection
        /// </summary>
        /// <param name="message"></param>
        /// <param name="yesMethodDelegate"></param>
        /// <param name="noMethodDelegate"></param>
        /// <param name="withOneButton"></param>
        /// <param name="fullscreen"></param>
        /// <returns></returns>
        public static CustomMessageBox ShowMessage(string message, LogDelegate yesMethodDelegate, LogDelegate noMethodDelegate, bool withOneButton = false, bool fullscreen = false)
        {
            var msg = new CustomMessageBox
            {
                Caption = "Tambula Logs",
                Message = message,
                LeftButtonContent = withOneButton ? "OK" : "Yes",
                IsFullScreen = fullscreen
            };

            if (true != withOneButton) //show second button
            {
                msg.RightButtonContent = "No";
            }

            msg.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        yesMethodDelegate();
                        break;
                    default:
                        noMethodDelegate();
                        break;
                }
            };

            msg.Show();

            return msg;

        }
        #endregion

    }
}
