using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomDreamTx
{
    /// <summary>
    /// CustomDreamRx类包含了部分梦境注册信息
    /// </summary>
    public class CustomSessionDreamTx

    {
        /// <summary>
        /// 添加原有世界的生物生成
        /// </summary>
        public virtual bool AllowDefaultSpawn => false;

        /// <summary>
        /// 覆盖生成
        /// </summary>
        public virtual bool OverrideDefaultSpawn => true;

        /// <summary>
        /// 覆盖所使用的继承世界
        /// </summary>
        public virtual SlugcatStats.Name DefaultSpawnName => SlugcatStats.Name.White;

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
            var oldgame = game.manager.oldProcess as RainWorldGame;
            if (oldgame == null)
                DreamSessionHoox.LogException(new Exception("[DreamGameSession] OldPrcess is not a RainWorldGame Class!"));

            //progression会在切换process时清空(PostSwitchMainProcess)，需重新赋值
            oldgame.rainWorld.progression.currentSaveState = oldgame.GetStorySession.saveState;
            oldgame.GetStorySession.saveState.SessionEnded(oldgame, survived, newMalnourished);

            ExitDream(game, survived, newMalnourished);
            return;
        }
    }

    public class DreamGameSession : GameSession
    {
        /// <summary>
        /// 获取DreamNutils，获取一部分参数
        /// </summary>
        public CustomSessionDreamTx owner;

        /// <summary>
        /// 标志结束session，会在QuitGame被设置为true
        /// </summary>
        public bool EndSession { get; set; }


        /// <summary>
        /// session运行时间
        /// 初始化后SessionCounter会每帧+1，可用作梦中计时器
        /// </summary>
        protected int SessionCounter { get; set; }

        /// <summary>
        /// 在初始化结束后会每帧刷新
        /// SessionCounter会每帧+1，可用作梦中计时器
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// 在世界加载后调用，放置巢穴(den)内的生物或offscreen的生物用
        /// </summary>
        public virtual void PostWorldLoaded()
        {
        }

        /// <summary>
        /// 在初始房间实例化后调用，一般可以放置shortcut内生成的生物。
        /// 玩家放置也在这个函数内进行
        /// </summary>
        public virtual void PostFirstRoomRealized()
        {

        }

        #region 生物生成API

        public AbstractCreature SpawnCreatureOffScreen(CreatureTemplate.Type type)
        {
            AbstractCreature abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(game.world.offScreenDen.index, 0, 0, 0), game.GetNewID());
            game.world.GetAbstractRoom(0).MoveEntityToDen(abstractCreature);
            game.world.offScreenDen.AddEntity(abstractCreature);
            return abstractCreature;
        }
        public AbstractCreature SpawnCreatureInShortCut(CreatureTemplate.Type type, string roomName, int suggestExits)
        {
            int roomID = game.world.GetAbstractRoom(roomName).index;
            return SpawnCreatureInShortCut(type, roomID, suggestExits);
        }
        public AbstractCreature SpawnCreatureInShortCut(CreatureTemplate.Type type, int roomID, int suggestExits)
        {
            if (game.world.GetAbstractRoom(roomID).realizedRoom == null)
            {
                DreamSessionHoox.LogException(new Exception("Can't spawn creature in non-activate room short cut"));
                return null;
            }
            int exits = game.world.GetAbstractRoom(roomID).exits;

            int node = Mathf.Min(suggestExits, exits);
            AbstractCreature abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(roomID, -1, -1, -1), game.GetNewID());
            game.world.GetAbstractRoom(roomID).AddEntity(abstractCreature);
            abstractCreature.RealizeInRoom();
            ShortcutHandler.ShortCutVessel shortCutVessel = new ShortcutHandler.ShortCutVessel(new IntVector2(-1, -1), abstractCreature.realizedCreature, game.world.GetAbstractRoom(roomID), 0);
            shortCutVessel.entranceNode = node;
            shortCutVessel.room = game.world.GetAbstractRoom(roomID);
            abstractCreature.pos.room = game.world.offScreenDen.index;
            game.shortcuts.betweenRoomsWaitingLobby.Add(shortCutVessel);
            return abstractCreature;
        }
        public AbstractCreature SpawnCreatureInDen(CreatureTemplate.Type type, string roomName, int suggestDen)
        {
            int roomID = game.world.GetAbstractRoom(roomName).index;
            return SpawnCreatureInDen(type, roomID, suggestDen);
        }
        public AbstractCreature SpawnCreatureInDen(CreatureTemplate.Type type, int roomID, int suggestDen)
        {

            var room = game.world.GetAbstractRoom(roomID);
            int dens = room.dens;
            suggestDen = Mathf.Min(suggestDen, dens);
            int denNodeIndex = -1;
            for (int i = 0; i < room.nodes.Length; i++)
            {
                if (room.nodes[i].type == AbstractRoomNode.Type.Den)
                {
                    denNodeIndex = i;
                    if (suggestDen == 0)
                        break;
                    suggestDen--;
                }
            }
            AbstractCreature abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(roomID, -1, -1, denNodeIndex), game.GetNewID());
            abstractCreature.remainInDenCounter = 20;
            room.MoveEntityToDen(abstractCreature);
            return abstractCreature;
        }


        public AbstractCreature SpawnPlayerInShortCut(string roomName, int suggestShortCut)
        {
            int roomID = game.world.GetAbstractRoom(roomName).index;
            return SpawnPlayerInShortCut(roomID, suggestShortCut);
        }
        public AbstractCreature SpawnPlayerInShortCut(int roomID, int suggestShortCut)
        {
            return SpawnPlayerInShortCut(roomID, suggestShortCut, characterStats.name);
        }
        public AbstractCreature SpawnPlayerInShortCut(string roomName, int suggestShortCut, SlugcatStats.Name name)
        {
            int roomID = game.world.GetAbstractRoom(roomName).index;
            return SpawnPlayerInShortCut(roomID, suggestShortCut, name);
        }
        public AbstractCreature SpawnPlayerInShortCut(int roomID, int suggestShortCut, SlugcatStats.Name name)
        {
            if (game.world.GetAbstractRoom(roomID).realizedRoom == null)
            {
                DreamSessionHoox.LogException(new Exception("Can't spawn player in non-activate room"));
                return null;
            }
            int exits = game.world.GetAbstractRoom(roomID).exits;
            suggestShortCut = Mathf.Min(suggestShortCut, exits);

            AbstractCreature abstractCreature = null;
            if (Players.Count == 0)
            {
                abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate("Slugcat"), null, new WorldCoordinate(roomID, -1, -1, -1), new EntityID(-1, 0));
                game.cameras[0].followAbstractCreature = abstractCreature;
            }
            else
                abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate("Slugcat"), null, new WorldCoordinate(roomID, -1, -1, -1), game.GetNewID());
            abstractCreature.state = new PlayerState(abstractCreature, 0, name, false);

            abstractCreature.Realize();
            ShortcutHandler.ShortCutVessel shortCutVessel = new ShortcutHandler.ShortCutVessel(new IntVector2(-1, -1), abstractCreature.realizedCreature, game.world.GetAbstractRoom(roomID), 0);
            shortCutVessel.entranceNode = suggestShortCut;
            shortCutVessel.room = game.world.GetAbstractRoom(roomID);
            abstractCreature.pos.room = game.world.offScreenDen.index;
            game.shortcuts.betweenRoomsWaitingLobby.Add(shortCutVessel);
            AddPlayer(abstractCreature);
            return abstractCreature;
        }


        public AbstractCreature SpawnPlayerInRoom(string roomName, IntVector2 pos)
        {
            int roomID = game.world.GetAbstractRoom(roomName).index;
            return SpawnPlayerInRoom(roomID, pos);
        }
        public AbstractCreature SpawnPlayerInRoom(int roomID, IntVector2 pos)
        {
            return SpawnPlayerInRoom(roomID, pos, characterStats.name);
        }
        public AbstractCreature SpawnPlayerInRoom(string roomName, IntVector2 pos, SlugcatStats.Name name)
        {
            int roomID = game.world.GetAbstractRoom(roomName).index;
            return SpawnPlayerInRoom(roomID, pos, name);
        }
        public AbstractCreature SpawnPlayerInRoom(int roomID, IntVector2 pos, SlugcatStats.Name name)
        {
            if (game.world.GetAbstractRoom(roomID).realizedRoom == null)
            {
                DreamSessionHoox.LogException(new Exception("Can't spawn player in non-activate room"));
                return null;
            }

            AbstractCreature abstractCreature = null;
            if (Players.Count == 0)
            {
                abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate("Slugcat"), null, new WorldCoordinate(roomID, pos.x, pos.y, -1), new EntityID(-1, 0));
                game.cameras[0].followAbstractCreature = abstractCreature;
            }
            else
                abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate("Slugcat"), null, new WorldCoordinate(roomID, pos.x, pos.y, -1), game.GetNewID());
            abstractCreature.state = new PlayerState(abstractCreature, 0, name, false);
            game.world.GetAbstractRoom(roomID).AddEntity(abstractCreature);
            abstractCreature.RealizeInRoom();
            AddPlayer(abstractCreature);
            return abstractCreature;
        }
        #endregion

        #region 基类
        /// <summary>
        /// 基本的update，不许你们动xx
        /// </summary>
        public void Base_Update()
        {
            if (EndSession) return;
            if (!isRealized && game.cameras[0].room != null && game.cameras[0].room.shortCutsReady)
            {
                PostFirstRoomRealized();
                DreamSessionHoox.Log("Dream session room realized");
                isRealized = true;
            }
            if (!isLoaded && game.world != null)
            {
                PostWorldLoaded();
                DreamSessionHoox.Log("Dream session world loaded");
                isLoaded = true;
            }

            if (isRealized)
            {
                Update();
                SessionCounter++;
            }
        }

        public DreamGameSession(RainWorldGame game, SlugcatStats.Name name, CustomSessionDreamTx ow) : base(game)
        {
            owner = ow;
            characterStats = new SlugcatStats(name, false);
        }

        /// <summary>
        /// 初始房间是否实例化
        /// </summary>
        protected bool isRealized;


        /// <summary>
        /// 世界是否加载完毕
        /// </summary>
        protected bool isLoaded;

        #endregion
    }
}
