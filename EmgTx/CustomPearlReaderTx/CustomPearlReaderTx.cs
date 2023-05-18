using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomPearlReader
{
    /// <summary>
    /// 该类的灵感来自SlimeCubed的PearlLexicon
    /// 为各种意义上的远程阅读珍珠提供支持
    /// </summary>
    public class CustomPearlReaderTx
    {
        public List<string> pearlConvs = new List<string>();

        public static List<string> convIDNames;
        public static List<Conversation.ID> convIDs;

        public bool setup = false;

        public SLOracleBehaviorHasMark simulateSLOracleBehaviour;

        public virtual void Init()
        {
            SetupBehavior();
            if (setup) return;

            List<string> names = Conversation.ID.values.entries;
            List<Conversation.ID> idArray = new List<Conversation.ID>();
            foreach (var name in names)
            {
                idArray.Add(new Conversation.ID(name));
            }

            convIDNames = names;
            convIDs = idArray;
            setup = true;   
        }

        /// <summary>
        /// 初始化虚拟 SLOracleBehaviorHasMark
        /// </summary>
        public virtual void SetupBehavior()
        {
            try
            {
                simulateSLOracleBehaviour = GetUninit<SLOracleBehaviorHasMark>();
                simulateSLOracleBehaviour.oracle = GetUninit<Oracle>();
                simulateSLOracleBehaviour.oracle.ID = GetUninit<Oracle.OracleID>();
                simulateSLOracleBehaviour.oracle.room = GetUninit<Room>();
                simulateSLOracleBehaviour.oracle.room.game = GetUninit<RainWorldGame>();
                simulateSLOracleBehaviour.oracle.room.game.rainWorld = UnityEngine.Object.FindObjectOfType<RainWorld>();
                simulateSLOracleBehaviour.oracle.room.game.session = GetUninit<SandboxGameSession>();
                simulateSLOracleBehaviour.currentConversation = GetUninit<SLOracleBehaviorHasMark.MoonConversation>();
                simulateSLOracleBehaviour.currentConversation.dialogBox = GetUninit<HUD.DialogBox>();

                simulateSLOracleBehaviour.DEBUGSTATE = new SLOrcacleState(true, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 寻找对话
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Conversation.ID FindConvo(DataPearl.AbstractDataPearl.DataPearlType type)
        {
            Init();
            string text = type.ToString();

            foreach (string str in convPrefix)
            {
                for (int j = 0; j < convIDNames.Count; j++)
                {
                    if (convIDNames[j] == str + text) return convIDs[j];
                }
            }
            for (int k = 0; k < convIDNames.Count; k++)
            {
                if (convIDNames[k].EndsWith(text)) return convIDs[k];
            }
            for (int l = 0; l < convIDNames.Count; l++)
            {
                if (convIDNames[l].Contains(text)) return convIDs[l];
            }

            Debug.LogException(new Exception(string.Format("Failed to get conversation for pearl: {0}", type)));
            return convIDs[0];
        }

        public Conversation.DialogueEvent[] GetEvent(Conversation.ID id, SLOracleBehaviorHasMark.MiscItemType miscItemType, bool skipIntro = false)
        {
            CustomPearlReaderHoox.SkipIntro = skipIntro;
            Conversation.DialogueEvent[] result = new SLOracleBehaviorHasMark.MoonConversation(id, simulateSLOracleBehaviour, miscItemType).events.ToArray();
            CustomPearlReaderHoox.SkipIntro = false;

            return result;
        }

        /// <summary>
        /// 读取珍珠的方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="skipIntro"></param>
        /// <returns></returns>
        public Conversation.DialogueEvent[] TalkAboutPearl(DataPearl.AbstractDataPearl.DataPearlType type, bool skipIntro = false)
        {
            return GetEvent(FindConvo(type),SLOracleBehaviorHasMark.MiscItemType.NA, skipIntro);
        }

        /// <summary>
        /// 读取物品的方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="skipIntro"></param>
        /// <returns></returns>
        public Conversation.DialogueEvent[] TalkAboutThisObject(PhysicalObject item, bool skipIntro = false)
        {
            return GetEvent(Conversation.ID.Moon_Misc_Item, simulateSLOracleBehaviour.TypeOfMiscItem(item), skipIntro);
        }

        public static T GetUninit<T>()
        {
            return (T)(FormatterServices.GetSafeUninitializedObject(typeof(T)));
        }

        public virtual void SetNeuronsLeft(int left)
        {
            simulateSLOracleBehaviour.State.neuronsLeft = left;
        }

        private static string[] convPrefix = new string[]
        {
            "Moon_Pearl",
            "Moon_Pearl_",
            "Moon_",
            "Pearl_",
            ""
        };
    }
}
