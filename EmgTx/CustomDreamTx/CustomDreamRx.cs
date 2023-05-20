using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDreamTx
{
    public static class CustomDreamRx
    {
        #region NormalDreamTx
        public static List<CustomNormalDreamTx> normalDreamTreatments = new List<CustomNormalDreamTx>();
        public static CustomNormalDreamTx currentActivateNormalDream;
        public static DataBridge normalDreamDataBridge = new DataBridge();

        public class BuildDreamWorldParams
        {
            public string firstRoom;
            public bool singleRoomWorld;

            public SlugcatStats.Name playAs;

            public IntVector2? overridePlayerPos;
        }

        public class DataBridge
        {
            public KarmaLadderScreen.SleepDeathScreenDataPackage sleepDeathScreenDataPackage;
        }

        /// <summary>
        /// 注册普通梦境的方法
        /// </summary>
        /// <param name="treatment"></param>
        public static void ApplyTreatment(CustomNormalDreamTx treatment)
        {
            normalDreamTreatments.Add(treatment);
            CustomDreamHoox.NormalDreamHooksOn();
        }
        #endregion
    }
}
