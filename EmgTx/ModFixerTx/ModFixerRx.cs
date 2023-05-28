using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModFixerTx
{
    public static class ModFixerRx
    {
        public static List<ModFixerTx> treatments = new List<ModFixerTx>();
        public static void ApplyTreatment(ModFixerTx treatment,bool lateApply = true)
        {
            ModFixerHoox.HookOn();

            if(!treatments.Contains(treatment))
                treatments.Add(treatment);

            if (lateApply)
                return;

            if (treatment.hookOn)
                return;

            try
            {
                foreach (var mod in ModManager.ActiveMods)
                {
                    if (mod.id == treatment._id)
                    {
                        treatment.Apply();
                        treatment.hookOn = true;
                    }
                }
                if (!treatment.hookOn)
                    EmgTxCustom.Log($"{treatment._id} hook dont have active mod");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
