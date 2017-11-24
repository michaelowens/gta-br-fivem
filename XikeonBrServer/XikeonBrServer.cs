using System;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.Native;
using System.Collections.Generic;

namespace XikeonBrServer
{
    public class XikeonBrServer : BaseScript
    {
        public static string Motd = "Welcome to the server!";

        public XikeonBrServer()
        {
            Debug.WriteLine("XikeonBrServer initialized");
            EventHandlers.Add("playerConnecting", new Action<string, CallbackDelegate>(OnPlayerConnecting));
            EventHandlers.Add("xbr:playerFirstSpawned", new Action<Player>(OnPlayerFirstSpawned));
            EventHandlers.Add("rconCommand", new Action<string, List<dynamic>>(OnRconCommand));
        }

        private void OnPlayerConnecting(string playerName, CallbackDelegate kickReason)
        {
            // if gameStarted then kick
            if (XikeonBrShared.Status.gameStarted)
            {
                Debug.WriteLine("Game in progress... Kicking player: {0}", playerName);
                kickReason("Game in progress... Try again later.");
                CancelEvent();
            }
            else
            {
                // TriggerClientEvent(player, "sendMotd", Motd);
                Debug.WriteLine($"{playerName} connected (server).");
            }
        }

        private void OnPlayerFirstSpawned([FromSource]Player player)
        {
            BaseScript.TriggerClientEvent("xbr:setTime", XikeonBrShared.Time.h, XikeonBrShared.Time.m, XikeonBrShared.Time.s);
            // TriggerClientEvent(player, "sendMotd", Motd);
            Debug.WriteLine($"{player.Name} requested the motd");
            Notification.SendDetails(player, "CHAR_ALL_PLAYERS_CONF", "GTA: Battle Royale", "", "Welcome to ~r~GTA: Battle Royale~s~!~n~This gamemode is currently in very early development stages.");
        }

        private void OnRconCommand(string command, List<dynamic> args)
        {
            Debug.WriteLine($"rcon command: {command}");

            switch (command)
            {
                case "testnotif":
                    Debug.WriteLine($"Sending test notification to all players");
                    Notification.SendDetails(null, "CHAR_ALL_PLAYERS_CONF", "GTA: Battle Royale", "", "Welcome to ~r~GTA: Battle Royale~s~!~n~This gamemode is currently in very early development stages.");
                    CancelEvent();
                    break;
                case "start_game":
                    Debug.WriteLine($"Starting game");
                    XikeonBrShared.Status.gameStarted = true;
                    CancelEvent();
                    break;
                case "stop_game":
                    Debug.WriteLine($"Stopping game");
                    XikeonBrShared.Status.gameStarted = false;
                    CancelEvent();
                    break;
                case "time":
                    Debug.WriteLine("Setting time to {0}:{1}:{2}", args[0], args[1], args[2]);
                    XikeonBrShared.Time.Set(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
                    BaseScript.TriggerClientEvent("xbr:setTime", XikeonBrShared.Time.h, XikeonBrShared.Time.m, XikeonBrShared.Time.s);
                    CancelEvent();
                    break;
                default:
                    break;
            }
        }
    }
}
