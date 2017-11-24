using System;
using CitizenFX.Core;

namespace XikeonBrServer
{
    static class Notification
    {
        static public void Send(Player player, string message)
        {
            BaseScript.TriggerClientEvent(player, "xbr:showNotification", message);
        }

        static public void SendDetails(Player player, string pic, string title, string subtitle, string message) //(target, pic, title, subtitle, message)
        {
            if (player == null)
            {
                BaseScript.TriggerClientEvent("xbr:showNotificationDetails", pic, title, subtitle, message);
            }
            else
            {
                BaseScript.TriggerClientEvent(player, "xbr:showNotificationDetailsPlayer", player, pic, title, subtitle, message);
            }
        }
    }
}
