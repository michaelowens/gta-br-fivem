using System;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.ComponentModel;

namespace XikeonBrServer
{
    public class XikeonBrServer : BaseScript
    {
        public static string Motd = "Welcome to the server!";
        public XikeonBrShared.Sync sync;

        public XikeonBrServer()
        {
            Debug.WriteLine("XikeonBrServer initialized");
            EventHandlers.Add("playerConnecting", new Action<string, CallbackDelegate>(OnPlayerConnecting));
            EventHandlers.Add("xbr:playerFirstSpawned", new Action<Player>(OnPlayerFirstSpawned));
            EventHandlers.Add("rconCommand", new Action<string, List<dynamic>>(OnRconCommand));

            sync = new XikeonBrShared.Sync(OnSyncPropertyChange);
        }

        private void OnSyncPropertyChange(Object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("[Server] Shared.Sync: property changed: {0} - {1}", e.PropertyName, sender.GetType().GetProperty(e.PropertyName).GetValue(sender));
            BaseScript.TriggerClientEvent("xbr:sync", sender.ToString(), e.PropertyName, sender.GetType().GetProperty(e.PropertyName).GetValue(sender));
        }

        private void OnPlayerConnecting(string playerName, CallbackDelegate kickReason)
        {
            // if gameStarted then kick
            if (sync.status.GameStarted)
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
                    sync.status.GameStarted = true;
                    CancelEvent();
                    break;
                case "stop_game":
                    Debug.WriteLine($"Stopping game");
                    sync.status.GameStarted = false;
                    CancelEvent();
                    break;
                case "time":
                    Debug.WriteLine("Setting time to {0}:{1}:{2}", args[0], args[1], args[2]);
                    XikeonBrShared.Time.Set(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
                    BaseScript.TriggerClientEvent("xbr:setTime", XikeonBrShared.Time.h, XikeonBrShared.Time.m, XikeonBrShared.Time.s);
                    CancelEvent();
                    break;
                case "countdown":
                    int countdownSeconds = 10;
                    if (args.Count > 0)
                    {
                        countdownSeconds = Convert.ToInt32(args[0]);
                    }

                    sync.status.Countdown = countdownSeconds;
                    sync.status.ShowCountdown = true;
                    Debug.WriteLine("Countdown from {0}", countdownSeconds);
                    //BaseScript.TriggerClientEvent("xbr:countdown", countdownSeconds);
                    CancelEvent();
                    break;
                case "freeze":
                    bool freeze = true;
                    if (args.Count > 1)
                    {
                        freeze = Convert.ToBoolean(args[1]);
                    }

                    Debug.WriteLine("Freezing Player: {0} - {1}", args[0], GetPlayerName(args[0]));
                    BaseScript.TriggerClientEvent("xbr:freezePlayer", args[0], freeze);
                    CancelEvent();
                    break;
                default:
                    break;
            }
        }
    }
}
