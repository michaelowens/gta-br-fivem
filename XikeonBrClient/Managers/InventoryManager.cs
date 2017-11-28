using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrClient.Managers
{
    public class InventoryManager
    {
        public InventoryManager() { }

        public async Task OnTick()
        {
            FindNearbyItemsOnGround(GetPlayerPed(-1));
            await BaseScript.Delay(1000);
        }

        public void FindNearbyItemsOnGround(int ped)
        {
            Vector3 pos = GetEntityCoords(ped, true);

            bool hit = false;
            Vector3 endCoords = new Vector3();
            Vector3 surfaceNormal = new Vector3();
            int entityHit = -1;
            int ray = StartShapeTestCapsule(pos.X, pos.Y, pos.Z, pos.X, pos.Y, pos.Z, 10f, -1, ped, 7);
            GetShapeTestResult(ray, ref hit, ref endCoords, ref surfaceNormal, ref entityHit);

            Debug.WriteLine("Got inventory scan hit: {0} - Type:{1}", entityHit, GetEntityType(entityHit));
        }
    }
}
