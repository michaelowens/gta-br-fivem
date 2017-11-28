using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrClient
{
    class Crouch : BaseScript
    {
        public int Key = (int)Control.Duck;
        public string AnimationName = "move_ped_crouched";
        public bool Crouched = false;

        public Crouch()
        {
            Tick += OnTick;
        }

        public async Task OnTick()
        {
            int ped = GetPlayerPed(-1);

            if (DoesEntityExist(ped) && !IsEntityDead(ped))
            {
                DisableControlAction(0, Key, true);

                if (!IsPauseMenuActive() && IsDisabledControlJustPressed(0, Key))
                {
                    RequestAnimSet(AnimationName);

                    while (!HasAnimSetLoaded(AnimationName))
                    {
                        await Delay(100);
                    }

                    if (Crouched)
                    {
                        ResetPedMovementClipset(ped, 0);
                    } else
                    {
                        SetPedMovementClipset(ped, AnimationName, 0.25f);
                    }

                    Crouched = !Crouched;
                }
            }
        }
    }
}
