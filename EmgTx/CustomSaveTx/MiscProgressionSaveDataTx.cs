using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PlayerProgression;

namespace EmgTx.CustomSaveTx
{
    internal class MiscProgressionSaveDataRx
    {
        static bool hooked;
        public static List<MiscProgressionSaveDataTx> treatments = new List<MiscProgressionSaveDataTx>();

        public static void HookOn()
        {
            On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;
            On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
        }

        private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, MiscProgressionData self)
        {
            string result = orig.Invoke(self);

            foreach (var treatment in treatments)
            {
                string appendent = $"<mpdA>{treatment.Header.ToUpper()}<mpdB>{treatment.SaveToString()}";
                result += appendent;
            }
            return result;
        }

        private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, MiscProgressionData self, string s)
        {
            orig.Invoke(self, s);
            string[] array = Regex.Split(s, "<mpdA>");

            foreach (var treatment in treatments)
            {
                treatment.Reset();
            }

            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = Regex.Split(array[i], "<mpdB>");
                string header = array2[0].ToUpper();

                foreach (var treatment in treatments)
                {
                    if (treatment.Header == header)
                    {
                        treatment.FromString(array2[1]);

                        if (self.unrecognizedSaveStrings.Contains(array[i]))
                        {
                            self.unrecognizedSaveStrings.Remove(array[i]);
                        }
                        break;
                    }
                }
            }
        }

        public static T GetTreatmentOfType<T>() where T : MiscProgressionSaveDataTx
        {
            foreach (var treatment in treatments)
            {
                if (treatment is T tUnit)
                    return tUnit;
            }
            return null;
        }

        public static void ApplyTreatment(MiscProgressionSaveDataTx treatment)
        {
            foreach (var unit in treatments)
            {
                if (unit.Header.ToUpper() == treatment.Header.ToUpper())
                {
                    throw new Exception($"MiscProgressionSaveDataTx of header:{unit.Header.ToUpper()} already exist");
                }
            }
            treatments.Add(treatment);
            if (!hooked)
            {
                HookOn();
                hooked = true;
            }
        }
    }

    /// <summary>
    /// 用来在MiscProgression中保存自定义数据的方法。
    /// </summary>
    internal class MiscProgressionSaveDataTx
    {
        /// <summary>
        /// 标头，仅使用大写
        /// </summary>
        public virtual string Header => "";

        /// <summary>
        /// 当数据重载的时候会触发该方法，你可以控制base.FromString的调用位置来决定该事件触发的时机。
        /// </summary>
        public event Action OnSaveUnitLoad;
        public MiscProgressionSaveDataTx()
        {
        }

        /// <summary>
        /// 清空现有的数据
        /// </summary>
        public virtual void Reset()
        {

        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="data"></param>
        public virtual void FromString(string data)
        {
            if (OnSaveUnitLoad != null)
            {
                OnSaveUnitLoad.Invoke();
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <returns></returns>
        public virtual string SaveToString()
        {
            return "";
        }
    }
}
