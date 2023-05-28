using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModFixerTx
{
    public class ModFixerHoox
    {
        static bool hookOn;
        public static void HookOn()
        {
            if (hookOn) 
                return;

            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            hookOn = true;
        }

        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig.Invoke(self);

            EmgTxCustom.Log("Active mod ids : ");
            foreach (var mod in ModManager.ActiveMods)
            {
                EmgTxCustom.Log(mod.id);
            }

            if (ModFixerRx.treatments.Count > 0)
            {
                foreach(var treatment in ModFixerRx.treatments)
                {
                    ModFixerRx.ApplyTreatment(treatment, false);
                }
            }
        }
    }
}
