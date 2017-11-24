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

        //public SpawnManager()
        //{
        //    EventHandlers.Add("getMapDirectives", new Action<CallbackDelegate>(OnGetMapDirectives));
        //}

        public static void OnGetMapDirectives(CallbackDelegate add)
        {
            Debug.WriteLine("OnGetMapDirectives: add spawnpoint");

            Func<ExpandoObject, string, Action<ExpandoObject>> OnSpawnpoint = (state, model) =>
            {
                Debug.WriteLine("OnSpawnpoint: add point");

                return new Action<ExpandoObject>(opts =>
                {
                    IDictionary<string, object> optsDict = opts;
                    string s = XikeonBrShared.Utils.ExpandoObjectToString(opts);

                    try
                    {
                        double x, y, z, heading;
                        int modelKey;

                        if (optsDict.ContainsKey("x"))
                        {
                            Debug.WriteLine("SpawnManager: using x/y/z");
                            x = (double)optsDict["x"];
                            y = (double)optsDict["y"];
                            z = (double)optsDict["z"];
                        } else {
                            Debug.WriteLine("SpawnManager: using 0/1/2");
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
                        //if (not is number model)
                        modelKey = GetHashKey(model);

                        AddSpawnPoint(new SpawnPoint
                        {
                            X = x,
                            Y = y,
                            Z = z,
                            Heading = heading,
                            Model = modelKey
                        });

                        Debug.WriteLine("Adding spawn point: {0},{1},{2}", x,y,z);
                        Debug.WriteLine("Original state object: {0}", XikeonBrShared.Utils.ExpandoObjectToString(state));

                        IDictionary<string, Object> stateDict = state as IDictionary<string, Object>;

                        stateDict.Add("x", x);
                        stateDict.Add("y", y);
                        stateDict.Add("z", z);
                        stateDict.Add("model", modelKey);

                        Debug.WriteLine("Updated state object: {0}", XikeonBrShared.Utils.ExpandoObjectToString(state));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                        throw;
                    }

                    //state.add("key", opts);
                });
            };

            Action<dynamic> removeCB = (state) =>
            {
                IDictionary<string, Object> stateDict = state as IDictionary<string, Object>;

                Debug.WriteLine("removing key {0}", state.key);
                // loop over spawnpoints, remove where x,y,z&model match and return (early exit loop? can only be 1)
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
            Debug.WriteLine("Got spawnpoint: {0}", sp.Idx);

            // freeze player
                
            // change the player model
            Debug.WriteLine("Request model: {0}, uint: {1}", sp.Model, (uint)sp.Model);
            //RequestModel((uint)sp.Model);
            //Model m = new Model(sp.Model);
            Model m = new Model(sp.Model);
            Debug.WriteLine("Fetched model: {0}", m);
            await Game.Player.ChangeModel(m);
            Debug.WriteLine("Changed model");
            Game.Player.Character.Style.SetDefaultClothes();
            Debug.WriteLine("Changed clothes");
            /*while (!HasModelLoaded((uint)sp.Model))
            {
                //RequestModel((uint)sp.Model);
                Thread.Sleep(10);
            }
            Debug.WriteLine("model loaded: {0}", sp.Model);*/

            //Game.Player.Character.Style.SetDefaultClothes();

            int pid = PlayerId();
            Debug.WriteLine("Got player id");
            //SetPlayerModel(pid, (uint)sp.Model);
            Debug.WriteLine("Player model set");

            // release the player model
            //SetModelAsNoLongerNeeded((uint)sp.Model);
            //Debug.WriteLine("Released model");

            RequestCollisionAtCoord((float)sp.X, (float)sp.Y, (float)sp.Z);
            Debug.WriteLine("Requested collisions model");

            // ResurrectNetworkPlayer(GetPlayerId(), spawn.x, spawn.y, spawn.z, spawn.heading)
            int ped = GetPlayerPed(-1);
            Debug.WriteLine("Got player ped");

            // V requires setting coords as well
            SetEntityCoordsNoOffset(ped, (float)sp.X, (float)sp.Y, (float)sp.Z, false, false, false); //, true);
            Debug.WriteLine("SetEntityCoordsNoOffset");
            NetworkResurrectLocalPlayer((float)sp.X, (float)sp.Y, (float)sp.Z, (float)sp.Heading, true, true); //, false);
            Debug.WriteLine("NetworkResurrectLocalPlayer");

            ClearPedTasksImmediately(ped);
            Debug.WriteLine("ClearPedTasksImmediately");
            RemoveAllPedWeapons(ped, true);
            Debug.WriteLine("RemoveAllPedWeapons");
            ClearPlayerWantedLevel(pid);
            Debug.WriteLine("ClearPlayerWantedLevel");

            while (!HasCollisionLoadedAroundEntity(ped))
            {
                await BaseScript.Delay(10);
            }
            Debug.WriteLine("HasCollisionLoadedAroundEntity");

            ShutdownLoadingScreen();
            Debug.WriteLine("ShutdownLoadingScreen");
            DoScreenFadeIn(500);
            Debug.WriteLine("DoScreenFadeIn");

            //while (IsScreenFadingIn())
            //{
            //    Thread.Sleep(10);
            //}
            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(100);
            }
            Debug.WriteLine("Screen faded in");

            // unfreeze player
            BaseScript.TriggerServerEvent("playerSpawned", sp);

            spawnLock = false;
        }
    }
}
