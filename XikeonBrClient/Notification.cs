using System;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Dynamic;

namespace XikeonBrClient
{
    static class Notification
    {
        /**
         * Events
         */
        static public void OnNotification(string message)
        {
            Notification.ShowNotification(message);
        }

        static public void OnNotificationDetails(string pic, string title, string subtitle, string message)
        {
            Notification.ShowNotificationDetails(pic, pic, true, 2, title, subtitle, message);
        }

        static public void OnNotificationDetails(ExpandoObject player, string pic, string title, string subtitle, string message)
        {
            Notification.ShowNotificationDetails(pic, pic, true, 2, title, subtitle, message);
        }

        /**
         * Functions
         */
        static public void ShowNotification(string text)
        {
            SetNotificationTextEntry("STRING");
            AddTextComponentString(text);
            DrawNotification(true, false);
        }

        static public void ShowNotificationDetails(string picName1, string picName2, bool flash, int iconType, string sender, string subject, string message)
        {
            SetNotificationTextEntry("STRING");
            AddTextComponentString(message);
            SetNotificationMessage(picName1, picName2, flash, iconType, sender, subject);
            DrawNotification(true, false);
        }
    }
}
