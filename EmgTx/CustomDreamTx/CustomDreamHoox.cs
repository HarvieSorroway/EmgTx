using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HUD;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace EmgTx.CustomDreamTx
{
    public class DreamGameSession : GameSession
    {
        /// <summary>
        /// 获取DreamNutils，获取一部分参数
        /// </summary>
        public CustomDreamRx owner;

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
            AbstractCreature abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(0, -1, -1, -1), game.GetNewID());
            abstractCreature.state = new HealthState(abstractCreature);

            abstractCreature.Realize();
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
            AbstractCreature abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(roomID, denNodeIndex, -1, -1), game.GetNewID());
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
            abstractCreature.state = new PlayerState(abstractCreature, 0, characterStats.name, false);

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

            abstractCreature.state = new PlayerState(abstractCreature, 0, characterStats.name, false);
            abstractCreature.Realize();
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

        public DreamGameSession(RainWorldGame game, SlugcatStats.Name name, CustomDreamRx ow) : base(game)
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

    public static class DreamSessionHoox
    {
        /// <summary>
        /// 注册梦到游戏
        /// </summary>
        /// <param name="customDream">梦的参数类DreamNutils</param>
        public static void RegisterDream(CustomDreamRx customDream)
        {
            OnModInit();
            dreams.Add(customDream);
        }

        #region Hook
        static bool isLoaded = false;
        static DreamSessionHoox()
        {
            dreams = new List<CustomDreamRx>();
        }
        public delegate SlugcatStats orig_slugcatStats(Player self);
        public static SlugcatStats Player_slugcatStats_get(orig_slugcatStats orig, Player self)
        {
            if (self.abstractCreature.world.game.session is DreamGameSession)
                return self.abstractCreature.world.game.session.characterStats;
            return orig(self);
        }



        public static void Log(string message)
        {
            Debug.Log("[CustomDreamRx] " + message);
        }
        public static void LogException(Exception e)
        {
            Debug.LogError("[CustomDreamRx] ERROR!");
            Debug.LogException(e);
        }

        static void OnModInit()
        {
            if (!isLoaded)
            {
                IL.RainWorldGame.ctor += RainWorldGame_ctorIL;
                IL.World.ctor += World_ctorIL;
                IL.RegionGate.ctor += RegionGate_ctorIL;
                IL.RainWorldGame.Update += RainWorldGame_UpdateIL;

                On.RainWorldGame.ExitGame += RainWorldGame_ExitGame;
                On.RainWorldGame.Win += RainWorldGame_Win;
                On.RainWorldGame.Update += RainWorldGame_Update;
                On.ProcessManager.PreSwitchMainProcess += ProcessManager_PreSwitchMainProcess;
                On.OverWorld.LoadFirstWorld += OverWorld_LoadFirstWorld;
                On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
                On.MultiplayerUnlocks.IsLevelUnlocked += MultiplayerUnlocks_IsLevelUnlocked;

                Hook slugcatStateHook = new Hook(
                    typeof(Player).GetProperty("slugcatStats", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                    typeof(DreamSessionHoox).GetMethod("Player_slugcatStats_get", BindingFlags.Static | BindingFlags.Public));
                isLoaded = true;
            }
        }

        private static void RainWorldGame_UpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<AbstractSpaceVisualizer>("ChangeRoom"),
                                             instr => instr.MatchLdarg(0),
                                             instr => instr.MatchCallOrCallvirt<RainWorldGame>("get_IsStorySession")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, RainWorldGame, bool>>((a, game) =>
                {
                    if (game.session is DreamGameSession)
                        return true;
                    return a;
                }); ;
            }
        }

        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            if (cam.game.session is DreamGameSession session && self.owner is Player player)
            {
                if (!session.owner.HasHUD)
                    return;
                self.AddPart(new TextPrompt(self));
                self.AddPart(new KarmaMeter(self, self.fContainers[1], new IntVector2(player.Karma, player.KarmaCap), (self.owner as Player).KarmaIsReinforced));
                self.AddPart(new FoodMeter(self, player.slugcatStats.maxFood, player.slugcatStats.foodToHibernate, null, 0));
                //self.AddPart(new Map(self, new Map.MapData(cam.room.world, cam.room.game.rainWorld)));
                self.AddPart(new RainMeter(self, self.fContainers[1]));
                if (ModManager.MSC)
                {
                    self.AddPart(new AmmoMeter(self, null, self.fContainers[1]));
                    self.AddPart(new HypothermiaMeter(self, self.fContainers[1]));
                    if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
                    {
                        self.AddPart(new GourmandMeter(self, self.fContainers[1]));
                    }
                }
                if (ModManager.MMF && MMF.cfgBreathTimeVisualIndicator.Value)
                {
                    self.AddPart(new BreathMeter(self, self.fContainers[1]));
                    if (ModManager.CoopAvailable && cam.room.game.session != null)
                    {
                        for (int i = 1; i < cam.room.game.session.Players.Count; i++)
                        {
                            self.AddPart(new BreathMeter(self, self.fContainers[1], cam.room.game.session.Players[i]));
                        }
                    }
                }
                if (ModManager.MMF && MMF.cfgThreatMusicPulse.Value)
                {
                    self.AddPart(new ThreatPulser(self, self.fContainers[1]));
                }
                if (ModManager.MMF && MMF.cfgSpeedrunTimer.Value)
                {
                    self.AddPart(new SpeedRunTimer(self, null, self.fContainers[1]));
                }
                if (cam.room.abstractRoom.shelter)
                {
                    self.karmaMeter.fade = 1f;
                    self.rainMeter.fade = 1f;
                    self.foodMeter.fade = 1f;
                }
                return;
            }
            orig(self, cam);
        }

        private static bool MultiplayerUnlocks_IsLevelUnlocked(On.MultiplayerUnlocks.orig_IsLevelUnlocked orig, MultiplayerUnlocks self, string levelName)
        {
            foreach (var data in dreams)
            {
                if (data.IsSingleWorld && data.HiddenRoomInArena && data.FirstRoom.ToLower() == levelName.ToLower())
                    return false;
            }
            return orig(self, levelName);
        }

        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            if (self.session is DreamGameSession session)
            {
                session.Base_Update();
            }
            orig(self);
        }

        private static void OverWorld_LoadFirstWorld(On.OverWorld.orig_LoadFirstWorld orig, OverWorld self)
        {
            if (self.game.session is DreamGameSession session)
            {
                var text = self.FIRSTROOM = self.game.startingRoom = session.owner.FirstRoom;
                if (!session.owner.IsSingleWorld)
                    self.LoadWorld(text.Split('_')[0].ToUpper(), self.game.session.characterStats.name, (self.game.session as DreamGameSession).owner.IsSingleWorld);
                else
                    self.LoadWorld(self.game.startingRoom, self.game.session.characterStats.name, (self.game.session as DreamGameSession).owner.IsSingleWorld);

                return;
            }
            orig(self);
        }

        private static void RegionGate_ctorIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            try
            {
                if (c.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
                                                 i => i.MatchCall<RegionGate>("Reset")))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    var label = c.DefineLabel();
                    c.EmitDelegate<Func<RegionGate, bool>>((gate) =>
                    {
                        if (gate is WaterGate waterGate)
                            waterGate.waterLeft = 1f;
                        else if (gate is ElectricGate electricGate)
                            electricGate.batteryLeft = 1f;
                        return gate.room.world.game.session is DreamGameSession;
                    });
                    c.Emit(OpCodes.Brtrue_S, label);
                    c.GotoNext(MoveType.Before, i => i.MatchLdarg(0),
                                                i => i.MatchLdarg(0),
                                                i => i.MatchNewobj<RegionGateGraphics>());
                    c.MarkLabel(label);
                }
                else
                    LogException(new Exception("RegionGate_ctor IL hook failed!"));
            }
            catch (Exception e)
            {
                LogException(new Exception("RegionGate_ctor IL hook failed!"));
                LogException(e);
            }

        }

        private static void World_ctorIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After, i => i.MatchCallvirt<RainWorldGame>("get_GetArenaGameSession")))
                {
                    c.GotoPrev(MoveType.After, i => i.MatchLdarg(1));

                    var notArena = c.DefineLabel();
                    var arena = c.DefineLabel();
                    c.EmitDelegate<Func<RainWorldGame, bool>>((game) => game.IsArenaSession);
                    c.Emit(OpCodes.Brfalse_S, notArena);

                    c.Emit(OpCodes.Ldarg_1);
                    c.GotoNext(MoveType.After, i => i.MatchStfld<World>("rainCycle"));
                    c.Emit(OpCodes.Br_S, arena);
                    c.MarkLabel(notArena);

                    c.EmitDelegate<Action<World, World>>((self, world) =>
                    {
                        //我无聊 好吧是为了清除返回值
                        self.rainCycle = new RainCycle(world, 100);
                    });
                    c.MarkLabel(arena);
                }
                else
                    LogException(new Exception("World_ctor IL hook failed!"));
            }
            catch (Exception e)
            {
                LogException(new Exception("RegionGate_ctor IL hook failed!"));
                LogException(e);
            }
        }

        private static void RainWorldGame_ExitGame(On.RainWorldGame.orig_ExitGame orig, RainWorldGame self, bool asDeath, bool asQuit)
        {
            if (self.session is DreamGameSession session)
            {
                session.EndSession = true;
                session.owner.ExitDream_Base(self, asDeath, asQuit, ma);
                return;
            }
            orig(self, asDeath, asQuit);
        }
        private static void ProcessManager_PreSwitchMainProcess(On.ProcessManager.orig_PreSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {

            if (self.oldProcess is RainWorldGame && self.currentMainLoop is RainWorldGame && (self.currentMainLoop as RainWorldGame).session is DreamGameSession)
            {
                //切换回story进行数据传输
                var game = self.oldProcess;
                self.oldProcess = self.currentMainLoop;
                self.currentMainLoop = game;

                //手动删除梦境
                self.oldProcess.ShutDownProcess();
                self.oldProcess.processActive = false;

                //清除恼人的coop控件
                if (!game.processActive && ModManager.JollyCoop)
                {
                    foreach (var camera in (game as RainWorldGame).cameras)
                    {
                        if (camera.hud?.jollyMeter != null)
                        {
                            camera.hud.parts.Remove(camera.hud.jollyMeter);
                            camera.hud.jollyMeter = null;
                        }
                    }
                }
            }
            orig(self, ID);
        }

        private static void RainWorldGame_ctorIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before, i => i.MatchNewobj<OverWorld>(),
                                              i => i.MatchStfld<RainWorldGame>("overWorld")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<RainWorldGame>>((self) =>
                {
                    if (self.manager.oldProcess is RainWorldGame game)
                    {
                        if (activeCustomDream != null)
                        {
                            self.session = activeCustomDream.GetSession(self, game.session.characterStats.name);
                            activeCustomDream = null;
                        }
                    }
                });
            }
        }

        private static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
        {
            if (!(self.session is DreamGameSession))
            {
                foreach (var dream in dreams)
                {
                    if (dream.HasDreamThisCycle(self, malnourished))
                    {
                        activeCustomDream = dream;
                        break;
                    }
                }
                if (activeCustomDream != null)
                {
                    Log("Try entering customDream");
                    self.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
                    ma = malnourished;
                    self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                    return;
                }
            }
            orig(self, malnourished);
        }
        static List<CustomDreamRx> dreams;
        static bool ma;
        static CustomDreamRx activeCustomDream;
        #endregion
    }
}
