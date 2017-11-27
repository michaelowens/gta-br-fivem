using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Dynamic;
using XikeonBrClient.Managers;
using CitizenFX.Core.UI;
using XikeonBrClient.System;

namespace XikeonBrClient
{
    public class XikeonBrClient : BaseScript
    {
        bool firstSpawn = true;
        bool playerInLobby = false;

        SpawnManager spawnManager;
        BaseEvents baseEvents;

        public XikeonBrClient()
        {
            Debug.WriteLine("XikeonBrClient initialized");

            baseEvents = new BaseEvents();
            Tick += baseEvents.OnTick;

            spawnManager = new SpawnManager();
            Tick += spawnManager.OnTick;
            EventHandlers.Add("getMapDirectives", new Action<CallbackDelegate>(spawnManager.OnGetMapDirectives));

            Tick += OnTick;
            EventHandlers.Add("onClientMapStart", new Action<string>(OnClientMapStart));
            EventHandlers.Add("sendMotd", new Action<string>(ReceivedMotd));
            EventHandlers.Add("playerSpawned", new Action<Player, Object, CallbackDelegate>(OnPlayerSpawned));
            EventHandlers.Add("xbr:showNotification", new Action<string>(Notification.OnNotification));
            EventHandlers.Add("xbr:showNotificationDetails", new Action<string, string, string, string>(Notification.OnNotificationDetails));
            EventHandlers.Add("xbr:showNotificationDetailsPlayer", new Action<ExpandoObject, string, string, string, string>(Notification.OnNotificationDetails));
            EventHandlers.Add("xbr:setTime", new Action<int, int, int>(OnSetTime));
            EventHandlers.Add("xbr:freezePlayer", new Action<int, bool>(OnFreezePlayer));
            EventHandlers.Add("xbr:playerRestart", new Action(OnPlayerRestart));
            // EventHandlers.Add("xbr:onPlayerDied", new Action<int, double, double, double>(OnPlayerDied));
        }

        private void OnClientMapStart(string resourceName)
        {
            Debug.WriteLine("onClientMapStart: {0}", resourceName);
            //Exports["spawnmanager"].setAutoSpawn(false);
            //Exports["spawnmanager"].spawnPlayer(false);
            spawnManager.SpawnPlayer();
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

        private void OnFreezePlayer(int player, bool freeze)
        {
            spawnManager.FreezePlayer(player, freeze);
        }

        private void OnPlayerRestart()
        {
            Debug.WriteLine("restarting player");
            spawnManager.SpawnPlayer();
        }

        private void OnPlayerSpawned([FromSource]Player player, Object playerName, CallbackDelegate kickReason)
        {
            baseEvents.IsDead = false;
            baseEvents.PlayerSpawned = true;

            if (firstSpawn)
            {
                firstSpawn = false;
                TriggerServerEvent("xbr:playerFirstSpawned", player);
            }

            playerInLobby = true;
            
            // Parachute
            Debug.WriteLine("Give parachute...");
            int ped = GetPlayerPed(PlayerId());
            uint item = (uint)GetHashKey("gadget_parachute");

            GiveWeaponToPed(ped, item, 1, false, true);

            // Spawn test weapon
            Debug.WriteLine("Spawn test weapon...");
            string weapon = "PICKUP_WEAPON_SNIPERRIFLE";
            int hk = GetHashKey(weapon);
            Vector3 pos = GetEntityCoords(ped, true);
            //int p = CreatePortablePickup((uint)hk, pos.X, pos.Y + 5f, 33f, true, (uint)hk);
            //FreezeEntityPosition(p, true);
            CreatePickup((uint)hk, pos.X, pos.Y + 5f, 33f, 8, 100, false, (uint)hk); // 8 = ON GROUND
            CreatePickup((uint)hk, pos.X, pos.Y + 3f, 33f, 512, 100, false, (uint)hk); // 512 = SPIN AROUND
            //SetEntityHasGravity(p, true);
            //CreatePickupWithAmmo()

            //NetworkOverrideClockTime(12, 0, 0);
        }
    }
}
