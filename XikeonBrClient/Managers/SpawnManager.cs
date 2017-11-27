using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XikeonBrClient.Models;
using System.Threading;
using CitizenFX.Core.UI;

namespace XikeonBrClient.Managers
{
    static class SpawnManager
    {
        static Random rnd = new Random();
        static bool spawnLock = false;
        private static List<SpawnPoint> spawnpoints = new List<SpawnPoint>();

        public static void OnGetMapDirectives(CallbackDelegate add)
        {
            Debug.WriteLine("OnGetMapDirectives: add spawnpoint");

            Func<dynamic, string, Action<dynamic>> OnSpawnpoint = (state, model) =>
            {
                Debug.WriteLine("OnSpawnpoint: add point");

                return new Action<dynamic>(opts =>
                {
                    Debug.WriteLine("OnSpawnpoint: Add point2");
                    IDictionary<string, object> optsDict = opts;
                    string s = XikeonBrShared.Utils.ExpandoObjectToString(opts);

                    try
                    {
                        double x, y, z, heading;
                        int modelKey;

                        if (optsDict.ContainsKey("x"))
                        {
                            x = (double)optsDict["x"];
                            y = (double)optsDict["y"];
                            z = (double)optsDict["z"];
                        } else {
                            // Debug.WriteLine("SpawnManager: using 0/1/2");
                            // TODO: test if this is being used/works
                            x = (double)optsDict.ElementAt(0).Value;
                            y = (double)optsDict.ElementAt(1).Value;
                            z = (double)optsDict.ElementAt(2).Value;
                        }

                        x = x + 0.0001;
                        y = y + 0.0001;
                        z = z + 0.0001;

                        heading = 0;
                        if (optsDict.ContainsKey("heading")) {
                            heading = (double)optsDict["heading"] + 0.01;
                        }

                        // recalculate the model for storage
                        // TODO: if (not is number model)
                        modelKey = GetHashKey(model);

                        AddSpawnPoint(new SpawnPoint
                        {
                            X = x,
                            Y = y,
                            Z = z,
                            Heading = heading,
                            Model = modelKey
                        });

                        //state = new ExpandoObject();

                        //IDictionary<string, Object> stateDict = state;

                        state.x = x;
                        state.y = y;
                        state.z = z;
                        state.model = modelKey;

                        Debug.WriteLine("Saved state: {0}", XikeonBrShared.Utils.ExpandoObjectToString(state));

                        /*stateDict.Add("x", x);
                        stateDict.Add("y", y);
                        stateDict.Add("z", z);
                        stateDict.Add("model", modelKey);*/
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("OnSpawnpoint: Add point2 - error: ", e.ToString());
                        throw;
                    }
                });
            };

            Action<ExpandoObject> removeCB = (state) =>
            {
                // TODO: this is broken and crashes
                /*Debug.WriteLine("SpawnManager: removeCB");
                Debug.WriteLine(XikeonBrShared.Utils.ExpandoObjectToString(state));
                IDictionary<string, Object> stateDict = state;
                Debug.WriteLine("SpawnManager: removeCB - stateDict: {0}", stateDict);
                Debug.WriteLine("SpawnManager: removeCB - stateDict.x: {0}", stateDict["x"]);*/

                //Debug.WriteLine("removing key {0}", state.key);
                // loop over spawnpoints, remove where x,y,z&model match and return (early exit loop? can only be 1)
                //spawnpoints.RemoveAll(sp => sp.X == (double)stateDict["x"] && sp.Y == (double)stateDict["y"] && sp.Z == (double)stateDict["z"] && sp.Model == (int)stateDict["model"]);
            };

            add("spawnpoint", OnSpawnpoint, removeCB);
        }

        public static int AddSpawnPoint(SpawnPoint sp) {
            if (!IsModelInCdimage((uint)sp.Model))
            {
                Debug.WriteLine("Model not found in cd image: {0}", sp.Model);
                return -1;
            }

            sp.Idx = spawnpoints.Count;
            spawnpoints.Add(sp);
            Debug.WriteLine("Spawnpoint added: {0}", sp.Idx);

            return sp.Idx;
        }

        public static void FreezePlayer(int id, bool freeze)
        {
            Debug.WriteLine("Freezing player: {0}", freeze);
            //SetPlayerControl(id, !freeze, 0); // Camera looking around
            Game.Player.CanControlCharacter = !freeze;
            int ped = GetPlayerPed(id);

            if (!IsEntityVisible(ped))
            {
                SetEntityVisible(ped, !freeze, false); // TODO: check 3rd param
            }

            if (!IsPedInAnyVehicle(ped, false)) // || freeze? - TODO: check 2nd param
            {
                SetEntityCollision(ped, !freeze, true); // TODO: check 3rd param
            }

            FreezeEntityPosition(ped, freeze);
            // SetCharNeverTargeted(ped, true)
            SetPlayerInvincible(ped, freeze);

            if (!IsPedFatallyInjured(ped) && freeze)
            {
                ClearPedTasksImmediately(ped);
            }
        }

        public static void SpawnPlayer()
        {
            SpawnPlayer(rnd.Next(spawnpoints.Count));
        }

        public static async void SpawnPlayer(int spawnIdx)
        {
            if (spawnLock)
            {
                Debug.WriteLine("Spawning locked...");
                return;
            }

            spawnLock = true;

            //DoScreenFadeOut(500);
            Screen.Fading.FadeOut(500);
            Debug.WriteLine("Spawning player... Spawn Idx: {0}", spawnIdx);

            //while (IsScreenFadingOut())
            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(100);
            }

            SpawnPoint sp = spawnpoints.ElementAt(spawnIdx);

            // TODO: freeze player
            int pid = PlayerId();
            FreezePlayer(pid, true);
            
            // change the player model
            Model m = new Model(sp.Model);
            await Game.Player.ChangeModel(m);
            Game.Player.Character.Style.SetDefaultClothes();
            

            // TODO: Set model as not used ?

            RequestCollisionAtCoord((float)sp.X, (float)sp.Y, (float)sp.Z);
            
            // ResurrectNetworkPlayer(GetPlayerId(), spawn.x, spawn.y, spawn.z, spawn.heading)
            int ped = GetPlayerPed(-1);
            
            // V requires setting coords as well
            SetEntityCoordsNoOffset(ped, (float)sp.X, (float)sp.Y, (float)sp.Z, false, false, false); //, true);
            NetworkResurrectLocalPlayer((float)sp.X, (float)sp.Y, (float)sp.Z, (float)sp.Heading, true, true); //, false);;

            ClearPedTasksImmediately(ped);
            RemoveAllPedWeapons(ped, true);
            ClearPlayerWantedLevel(pid);

            while (!HasCollisionLoadedAroundEntity(ped))
            {
                await BaseScript.Delay(10);
            }

            ShutdownLoadingScreen();
            DoScreenFadeIn(500);
            
            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(100);
            }

            // TODO: unfreeze player
            FreezePlayer(pid, false);

            BaseScript.TriggerServerEvent("playerSpawned", sp);

            spawnLock = false;
        }
    }
}
