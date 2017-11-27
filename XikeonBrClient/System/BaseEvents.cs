﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core.UI;
using XikeonBrClient.Managers;
using CitizenFX.Core.Native;

namespace XikeonBrClient.System
{
    public class BaseEvents
    {
        public bool IsDead = false;
        public bool PlayerSpawned = false;
        public bool HasBeenDead = false;
        public int DiedAt = -1;
        private bool Restarting = false;
        private bool RanDeathAnimation = false;

        public BaseEvents() {
            Debug.WriteLine("LOADED BASE_EVENTS");
        }

        public void DrawDeadMessage()
        {
            if (IsDead)
            {
                float x = Screen.Width / 2;
                float y = Screen.Height / 2;
                float width = 350;
                float height = 120;
                //DrawRect(x - (width / 2), y - (height / 2), width, height, 255, 0, 0, 255);

                Rectangle rect = new Rectangle(new PointF(x - (width / 2), y - (height / 2)), new SizeF(width, height), Color.FromArgb(175, 0, 0 ,0));
                rect.Draw();

                Text text = new Text("You died!", new PointF(x, rect.Position.Y + 15f), 1.0f);
                text.Alignment = Alignment.Center;
                text.WrapWidth = width;
                //Debug.WriteLine("WrapWidth:{0} - {1}", text.Width, (x - (text.Width)));
                //text.Position.X = x - (text.WrapWidth / 2);
                //text.Caption = "You died!";
                text.Draw();

                string btn = GetControlInstructionalButton(0, (int)Control.Reload, true);
                Text restartText = new Text(string.Format("Press {0} to restart!", btn), new PointF(x, rect.Position.Y + height - 50f), .5f);
                restartText.Alignment = Alignment.Center;
                restartText.WrapWidth = width;
                restartText.Draw();

                Scaleform buttons = new Scaleform("instructional_buttons");
                buttons.CallFunction("CLEAR_ALL");
                buttons.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
                buttons.CallFunction("CREATE_CONTAINER");
                buttons.CallFunction("SET_DATA_SLOT", 0, Function.Call<string>((Hash)0x0499D7B09FC9B407, 2, (int)Control.Reload, 0), "Restart");
                buttons.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", 1);
                buttons.Render2D();

                Screen.Hud.CursorSprite = CursorSprite.MiddleFinger;
                Screen.Hud.ShowCursorThisFrame();

                if (!RanDeathAnimation && PlayerSpawned)
                {
                    Debug.WriteLine("DEATH FAIL OUT");
                    RanDeathAnimation = true;
                    Screen.Effects.Start(ScreenEffect.DeathFailOut, 0, true);
                }
            }
        }

        public async Task OnTick()
        {
            int pid = PlayerId();

            if (NetworkIsPlayerActive(pid))
            {
                int ped = GetPlayerPed(pid);

                if (IsPedFatallyInjured(ped) && !IsDead)
                {
                    IsDead = true;
                    if (DiedAt == -1)
                    {
                        DiedAt = GetGameTimer();
                    }

                    uint killerWeapon = new uint();
                    int killerEntityId = NetworkGetEntityKillerOfPlayer(pid, ref killerWeapon);
                    int killerEntityType = GetEntityType(killerEntityId);
                    int killerType = -1;
                    bool killerInVehicle = false;
                    string killerVehicleName = "";
                    int killerVehicleSeat = 0;

                    if (killerEntityType == 1)
                    {
                        killerType = GetPedType(killerEntityId);

                        if (IsPedInAnyVehicle(killerEntityId, false))
                        {
                            killerInVehicle = true;
                            killerVehicleName = GetDisplayNameFromVehicleModel((uint)GetEntityModel(GetVehiclePedIsIn(killerEntityId, false)));
                            killerVehicleSeat = GetLastPedInVehicleSeat(killerEntityId, 0);
                        } else
                        {
                            killerInVehicle = false;
                        }
                    }

                    int killerId = GetPlayerByEntityID(killerEntityId);
                    if (killerEntityId != ped && killerEntityId != -1 && NetworkIsPlayerActive(killerId))
                    {
                        killerId = GetPlayerServerId(killerId);
                    } else
                    {
                        killerId = -1;
                    }

                    Vector3 coords = GetEntityCoords(ped, true);
                    if (killerEntityId == ped || killerEntityId == -1)
                    {
                        Debug.WriteLine("emit {0} - killerType:{1} - location:{2},{3},{4}", "xbr:onPlayerDied", killerType, coords.X, coords.Y, coords.Z);
                        BaseScript.TriggerEvent("xbr:onPlayerDied", killerType, coords.X, coords.Y, coords.Z);
                        BaseScript.TriggerServerEvent("xbr:onPlayerDied", killerType, coords.X, coords.Y, coords.Z);
                    } else
                    {
                        Debug.WriteLine("emit {0} - killerId:{1} - killerType:{2} - location:{3},{4},{5}", "xbr:onPlayerKilled", killerId, killerType, coords.X, coords.Y, coords.Z);
                        BaseScript.TriggerEvent("xbr:onPlayerKilled", killerId, killerType, coords.X, coords.Y, coords.Z);
                        BaseScript.TriggerServerEvent("xbr:onPlayerKilled", killerId, killerType, coords.X, coords.Y, coords.Z);
                    }
                } else if (!IsPedFatallyInjured(ped))
                {
                    Restarting = false;
                    IsDead = false;
                    DiedAt = -1;
                }

                // Check if the player has to respawn in order to trigger an event
                if (!HasBeenDead && DiedAt > 0)
                {
                    Vector3 coords = GetEntityCoords(ped, true);
                    BaseScript.TriggerEvent("xbr:onPlayerWasted", coords.X, coords.Y, coords.Z);
                    BaseScript.TriggerServerEvent("xbr:onPlayerWasted", coords.X, coords.Y, coords.Z);
                } else if (HasBeenDead && DiedAt <= 0)
                {
                    HasBeenDead = false;
                }
            }

            DrawDeadMessage();

            if (IsDead && !Restarting && IsControlPressed(0, (int)Control.Reload))
            {
                Restarting = true;
                RanDeathAnimation = false;
                BaseScript.TriggerEvent("xbr:playerRestart");
                BaseScript.TriggerServerEvent("xbr:playerRestart");
                Screen.Effects.Stop(ScreenEffect.DeathFailOut);
            }

            if (IsDead)
            {
                PlayerSpawned = false;
            }
        }

        public int GetPlayerByEntityID(int id)
        {
            for (int i = 0; i < 32; i++)
            {
                if (NetworkIsPlayerActive(i) && GetPlayerPed(i) == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
