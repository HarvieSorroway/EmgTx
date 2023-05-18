using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmgTx.CustomDreamTx
{
    /// <summary>
    /// DreamNutils类包含了部分梦境注册信息
    /// </summary>
    public class CustomDreamRx
    {
        /// <summary>
        /// 获取梦境场景的Session
        /// </summary>
        /// <param name="game">当前game</param>
        /// <param name="name">梦境前战役所使用的猫猫的名字</param>
        /// <returns></returns>
        public virtual DreamGameSession GetSession(RainWorldGame game, SlugcatStats.Name name)
        {
            return new DreamGameSession(game, name, this);
        }

        /// <summary>
        /// 判断本轮回是否有梦，一般可以获取game.Players进行具体判断
        /// </summary>
        /// <param name="game">当前game</param>
        /// <param name="malnourished">是否是饥饿状态</param>
        /// <returns></returns>
        public virtual bool HasDreamThisCycle(RainWorldGame game, bool malnourished)
        {
            return false;
        }


        /// <summary>
        /// 退出梦境，一般情况下不需要重写。
        /// </summary>
        /// <param name="game">当前game</param>
        /// <param name="survived">是否成功度过雨循环</param>
        /// <param name="newMalnourished">是否饥饿</param>
        public virtual void ExitDream(RainWorldGame game, bool survived, bool newMalnourished)
        {
            game.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;

            if (game.manager.musicPlayer != null)
                game.manager.musicPlayer.FadeOutAllSongs(SongFadeOut);

            if (!survived)
                game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.DeathScreen, SleepFadeIn);
            else
                game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen, DeathFadeIn);
        }

        /// <summary>
        /// 梦境中加载的第一个房间
        /// 如果IsSingleWorld为true，则会从world文件夹下搜索
        /// 如果IsSingleWorld为false，则会在levels文件夹下搜索
        /// </summary>
        public virtual string FirstRoom => "accelerator";

        /// <summary>
        /// 如果为单房间(IsSingleWorld为true)情况下，是否在竞技场隐藏该房间(FirstRoom)
        /// </summary>
        public virtual bool HiddenRoomInArena => false;

        /// <summary>
        /// 是否为单房间模式
        /// </summary>
        public virtual bool IsSingleWorld => true;

        /// <summary>
        /// 在多房间模式下是否显示HUD界面（不会显示MAP）
        /// </summary>
        public virtual bool HasHUD => true;

        /// <summary>
        /// 梦中死亡是否计入保存
        /// 如果为true则梦中死亡不影响正常存档
        /// </summary>
        public virtual bool ForceSave => false;

        /// <summary>
        /// 进入雨眠界面的淡出时长
        /// </summary>
        public virtual float SleepFadeIn => 3f;

        /// <summary>
        /// 进入死亡界面的淡出时长
        /// </summary>
        public virtual float DeathFadeIn => 3f;

        /// <summary>
        /// 梦境结束时歌曲淡出时长
        /// </summary>
        public virtual float SongFadeOut => 20f;

        /// <summary>
        /// 存档用函数，会调用ExitDream
        /// </summary>
        /// <param name="game"></param>
        /// <param name="asDeath"></param>
        /// <param name="asQuit"></param>
        /// <param name="newMalnourished"></param>
        public void ExitDream_Base(RainWorldGame game, bool asDeath, bool asQuit, bool newMalnourished)
        {
            var survived = !(asDeath || asQuit) || ForceSave;
            var oldGame = game.manager.oldProcess as RainWorldGame;
            if (oldGame == null)
                DreamSessionHoox.LogException(new Exception("[DreamGameSession] OldProcess is not a RainWorldGame Class!"));

            //progression会在切换process时清空(PostSwitchMainProcess)，需重新赋值
            oldGame.rainWorld.progression.currentSaveState = oldGame.GetStorySession.saveState;
            oldGame.GetStorySession.saveState.SessionEnded(oldGame, survived, newMalnourished);

            ExitDream(game, survived, newMalnourished);
            return;
        }
    }

}
