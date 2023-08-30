using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmgTx.CustomObjectDevExt
{
    public static class CustomDevObjectRx
    {
        public static Dictionary<PlacedObject.Type, CustomDevObjectTx> customDevObjectTxs = new Dictionary<PlacedObject.Type, CustomDevObjectTx>();


        public static void ApplyTreatment(CustomDevObjectTx treatment)
        {
            customDevObjectTxs.Add(treatment.placedType, treatment);
            CustomDevObjectHoox.HookOn();
        }
    }
}
