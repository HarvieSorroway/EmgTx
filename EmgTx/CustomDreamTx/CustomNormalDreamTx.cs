
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDreamTx
{
    /// <summary>
    /// 自定义梦境，目前仅支持RainWorldGame类型的梦境（类似炸猫）
    /// Dream的生命周期：从 DreamScreen.Update 开始，到梦境结束后切换为 SleepAndDeathScreen ProcessManager.PostSwitchProcess 结束
    /// </summary>
    public class CustomNormalDreamTx
    {
        public bool dreamStarted = false;
        public bool dreamFinished = false;
        public bool currentDreamActivate = false;

        public DreamsState.DreamID activateDreamID;
        public SlugcatStats.Name focusSlugcat;

        /// <summary>
        /// 当前梦境是演出型梦境，还是cg梦境
        /// </summary>
        public virtual bool IsPerformDream => true;

        public CustomNormalDreamTx(SlugcatStats.Name focusSlugcat)
        {
            this.focusSlugcat = focusSlugcat;
        }

        /// <summary>
        /// 当梦境激活时，该方法会调用
        /// </summary>
        /// <param name="dreamID"></param>
        public virtual void ActivateThisDream(DreamsState.DreamID dreamID)
        {
            activateDreamID = dreamID;
            currentDreamActivate = true;
            CustomDreamRx.currentActivateNormalDream = this;

            EmgTxCustom.Log(ToString() + " dream activate, id : " + dreamID.ToString());
        }

        /// <summary>
        /// 梦境播放完成，在preSwitchProcess中清理状态
        /// </summary>
        public virtual void CleanUpThisDream()
        {
            dreamStarted = false;
            dreamFinished = false;
            currentDreamActivate = false;
            CustomDreamRx.currentActivateNormalDream = null;

            EmgTxCustom.Log(ToString() + "clean up dream");
        }

        /// <summary>
        /// 结束梦境调用的方法，仅当作为演出型梦境时被才需要被调用。
        /// 如何结束梦境取决于用户，需要手动调用。
        /// </summary>
        /// <param name="game"></param>
        public virtual void EndDream(RainWorldGame game)
        {
            EmgTxCustom.Log(ToString() + " try to end dream. already ended : " + dreamFinished.ToString());
            if (dreamFinished) return;

            dreamFinished = true;

            game.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
            List<AbstractCreature> collection = new List<AbstractCreature>(game.session.Players);
            game.session = new StoryGameSession(focusSlugcat, game);
            game.session.Players = new List<AbstractCreature>(collection);


            if (game.manager.musicPlayer != null)
            {
                game.manager.musicPlayer.FadeOutAllSongs(20f);
            }

            game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen, 5f);
        }

        /// <summary>
        /// 根据 DreamID 决定需要展示的 SceneID,仅当作为cg梦境的时候才需要重写该方法
        /// </summary>
        /// <param name="dreamID"></param>
        /// <returns></returns>
        public virtual MenuScene.SceneID SceneFromDream(DreamsState.DreamID dreamID)
        {
            return MenuScene.SceneID.Empty;
        }


        /// <summary>
        /// 决定该轮回结束后启用的梦境ID
        /// </summary>
        /// <param name="upcomingDream"></param>
        public virtual void DecideDreamID(
            SaveState saveState,
            string currentRegion,
            string denPosition,
            ref int cyclesSinceLastDream,
            ref int cyclesSinceLastFamilyDream,
            ref int cyclesSinceLastGuideDream,
            ref int inGWOrSHCounter,
            ref DreamsState.DreamID upcomingDream,
            ref DreamsState.DreamID eventDream,
            ref bool everSleptInSB,
            ref bool everSleptInSB_S01,
            ref bool guideHasShownHimselfToPlayer,
            ref int guideThread,
            ref bool guideHasShownMoonThisRound,
            ref int familyThread)
        {
            if (dreamFinished) return;
        }

        /// <summary>
        /// 获取建立梦境的参数
        /// </summary>
        /// <returns></returns>
        public virtual CustomDreamRx.BuildDreamWorldParams GetBuildDreamWorldParams()
        {
            throw new NotImplementedException("Nooooo! you must implement this!");
        }

    }
}
