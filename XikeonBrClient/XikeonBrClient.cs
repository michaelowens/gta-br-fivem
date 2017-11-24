using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Dynamic;
using XikeonBrClient.Managers;

namespace XikeonBrClient
{
    public class XikeonBrClient : BaseScript
    {
        bool firstSpawn = true;
        bool playerInLobby = false;

        public XikeonBrClient()
        {
            Debug.WriteLine("XikeonBrClient initialized");
            Tick += OnTick;
            EventHandlers.Add("onClientMapStart", new Action<string>(OnClientMapStart));
            EventHandlers.Add("sendMotd", new Action<string>(ReceivedMotd));
            EventHandlers.Add("playerSpawned", new Action<Player, System.Object, CallbackDelegate>(OnPlayerSpawned));
            EventHandlers.Add("xbr:showNotification", new Action<string>(Notification.OnNotification));
            EventHandlers.Add("xbr:showNotificationDetails", new Action<string, string, string, string>(Notification.OnNotificationDetails));
            EventHandlers.Add("xbr:showNotificationDetailsPlayer", new Action<ExpandoObject, string, string, string, string>(Notification.OnNotificationDetails));
            EventHandlers.Add("xbr:setTime", new Action<int, int, int>(OnSetTime));
            EventHandlers.Add("getMapDirectives", new Action<CallbackDelegate>(SpawnManager.OnGetMapDirectives));
        }

        private void OnClientMapStart(string resourceName)
        {
            Debug.WriteLine("onClientMapStart: {0}", resourceName);
            //Exports["spawnmanager"].setAutoSpawn(false);
            //Exports["spawnmanager"].spawnPlayer(false);
            SpawnManager.SpawnPlayer();
        }

        private async Task OnTick()
        {
            NetworkOverrideClockTime(XikeonBrShared.Time.h, XikeonBrShared.Time.m, XikeonBrShared.Time.s);
            await Delay(250);
        }

        private void ReceivedMotd(string motd)
        {
            TriggerEvent("chatMessage", "SYSTEM", new[] { 255, 0, 0 }, motd);
        }

        private void OnSetTime(int h, int m, int s)
        {
            XikeonBrShared.Time.Set(h, m, s);
        }

        private void OnPlayerSpawned([FromSource]Player player, System.Object playerName, CallbackDelegate kickReason)
        {
            if (firstSpawn)
            {
                firstSpawn = false;
                TriggerServerEvent("xbr:playerFirstSpawned", player);
            }

            playerInLobby = true;
            //NetworkOverrideClockTime(12, 0, 0);
        }
    }
}
