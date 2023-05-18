using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomPearlReader
{
    public static class CustomPearlReaderRx
    {
        public static void ApplyTreatment(CustomPearlReaderTx treatment)
        {
            treatment.Init();
            CustomPearlReaderHoox.HookOn();
        }
    }
    
    public static class CustomPearlReaderHoox //try to learn something in PearlLexicon :P
    {
        public static bool SkipIntro = false;
        static bool inited;
        public static void HookOn()
        {
            if(inited) return;
            Hook rainWorldGame_get_GetStoryGameSession_Hook = new Hook(typeof(RainWorldGame).GetProperty("GetStorySession", propFlags).GetGetMethod(), typeof(CustomPearlReaderHoox).GetMethod("RainWorldGame_get_GetStorySession", methodFlags));
            On.SLOracleBehaviorHasMark.MoonConversation.PearlIntro += MoonConversation_PearlIntro;
            inited = true;
        }

        private static void MoonConversation_PearlIntro(On.SLOracleBehaviorHasMark.MoonConversation.orig_PearlIntro orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            if (SkipIntro) return;
            orig.Invoke(self);
        }

        //to fix exception in MoonConversation.ctor
        public static StoryGameSession RainWorldGame_get_GetStorySession(orig_RainWorldGame_GetStorySession orig, RainWorldGame self)
        {
            StoryGameSession result = orig.Invoke(self);
            if (self.session == null || !(self.session is StoryGameSession))
            {
                result = CustomPearlReaderTx.GetUninit<StoryGameSession>();
            }
            return result;
        }
        public delegate StoryGameSession orig_RainWorldGame_GetStorySession(RainWorldGame self);
        static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
        static BindingFlags methodFlags = BindingFlags.Static | BindingFlags.Public;
    }
}
