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
using Microsoft.Win32;
using static CustomDreamTx.CustomDreamRx;
using Menu;

namespace CustomDreamTx
{
    internal static class CustomDreamHoox
    {
        

        #region Hook
        static bool isLoaded = false;
        public delegate SlugcatStats orig_slugcatStats(Player self);

        public static SlugcatStats Player_slugcatStats_get(orig_slugcatStats orig, Player self)
        {
            if (self.abstractCreature.world.game.session is DreamGameSession)
                return self.abstractCreature.world.game.session.characterStats;
            return orig(self);
        }



        public static void Log(string message)
        {
            Debug.Log("[DreamNutils] " + message);
        }
        public static void LogException(Exception e)
        {
            Debug.LogError("[DreamNutils] ERROR!");
            Debug.LogException(e);
        }

        public static void OnModInit()
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

                On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues;
                On.WorldLoader.CreatingWorld += WorldLoader_CreatingWorld;

                On.CreatureCommunities.LikeOfPlayer += CreatureCommunities_LikeOfPlayer;
                On.CreatureCommunities.SetLikeOfPlayer += CreatureCommunities_SetLikeOfPlayer;
                On.CreatureCommunities.InfluenceLikeOfPlayer += CreatureCommunities_InfluenceLikeOfPlayer;

                Hook slugcatStateHook = new Hook(
                    typeof(Player).GetProperty("slugcatStats", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                    typeof(CustomDreamHoox).GetMethod("Player_slugcatStats_get", BindingFlags.Static | BindingFlags.Public));
                isLoaded = true;
            }
        }

        #region CreatureCommunities

        private static void CreatureCommunities_InfluenceLikeOfPlayer(On.CreatureCommunities.orig_InfluenceLikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID commID, int region, int playerNumber, float influence, float interRegionBleed, float interCommunityBleed)
        {
            if (self.session is DreamGameSession)
                region = -1;
            orig(self, commID, region, playerNumber, influence, interRegionBleed, interCommunityBleed);
        }

        private static void CreatureCommunities_SetLikeOfPlayer(On.CreatureCommunities.orig_SetLikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID commID, int region, int playerNumber, float newLike)
        {
            if (self.session is DreamGameSession)
                region = -1;
            orig(self, commID, region, playerNumber, newLike);
        }

        private static float CreatureCommunities_LikeOfPlayer(On.CreatureCommunities.orig_LikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID commID, int region, int playerNumber)
        {
            if (self.session is DreamGameSession)
                region = -1;
            return orig(self, commID, region, playerNumber);
        }


        #endregion

        #region spawn

        private static void WorldLoader_CreatingWorld(On.WorldLoader.orig_CreatingWorld orig, WorldLoader self)
        {
            orig(self);
            if (self.game != null && self.game.session is DreamGameSession dream && dream.owner.AllowDefaultSpawn)
                self.GeneratePopulation();
            Debug.Log("[Nutils] End Generate population");
        }

        private static void GeneratePopulation(this WorldLoader self)
        {

            Debug.Log("Generate Nutils population for : " + self.world.region.name);
            for (int l = 0; l < self.spawners.Count; l++)
            {

                if (self.spawners[l] is World.SimpleSpawner simpleSpawner)
                {
                    int num = simpleSpawner.amount;

                    if (num > 0)
                    {
                        self.creatureStats[simpleSpawner.creatureType.Index + 4] += (float)num;
                        AbstractRoom abstractRoom = self.world.GetAbstractRoom(simpleSpawner.den);
                        if (abstractRoom != null && simpleSpawner.den.abstractNode < abstractRoom.nodes.Length && (abstractRoom.nodes[simpleSpawner.den.abstractNode].type == AbstractRoomNode.Type.Den || abstractRoom.nodes[simpleSpawner.den.abstractNode].type == AbstractRoomNode.Type.GarbageHoles))
                        {
                            if (StaticWorld.GetCreatureTemplate(simpleSpawner.creatureType).quantified)
                            {
                                abstractRoom.AddQuantifiedCreature(simpleSpawner.den.abstractNode, simpleSpawner.creatureType, simpleSpawner.amount);
                            }
                            else
                            {
                                for (int m = 0; m < num; m++)
                                {
                                    try
                                    {
                                        AbstractCreature abstractCreature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(simpleSpawner.creatureType), null,
                                            simpleSpawner.den, self.world.game.GetNewID(simpleSpawner.SpawnerID));
                                        abstractCreature.spawnData = simpleSpawner.spawnDataString;
                                        abstractCreature.nightCreature = simpleSpawner.nightCreature;
                                        abstractCreature.setCustomFlags();
                                        abstractRoom.MoveEntityToDen(abstractCreature);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogException(e);
                                        Debug.Log(string.Format("[Nutils] invaild Creature {0}", simpleSpawner.creatureType));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (self.spawners[l] is World.Lineage lineage2)
                {
                    try
                    {
                        self.creatureStats[lineage2.creatureTypes[0] + 4] += 1f;
                        if (true)
                        {
                            AbstractRoom abstractRoom2 = self.world.GetAbstractRoom(lineage2.den);
                            CreatureTemplate.Type type = new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(lineage2.creatureTypes[0]));
                            if (StaticWorld.GetCreatureTemplate(type) != null && abstractRoom2 != null && lineage2.den.abstractNode < abstractRoom2.nodes.Length && (abstractRoom2.nodes[lineage2.den.abstractNode].type == AbstractRoomNode.Type.Den || abstractRoom2.nodes[lineage2.den.abstractNode].type == AbstractRoomNode.Type.GarbageHoles))
                            {

                                AbstractCreature creature = new AbstractCreature(self.world,
                                    StaticWorld.GetCreatureTemplate(type), null, lineage2.den,
                                    self.world.game.GetNewID(lineage2.SpawnerID))
                                {
                                    spawnData = lineage2.spawnData[0],
                                    nightCreature = lineage2.nightCreature
                                };
                                creature.setCustomFlags();
                                abstractRoom2.MoveEntityToDen(creature);
                            }
                            else if (type == null || StaticWorld.GetCreatureTemplate(type) != null)
                            {
                                Debug.Log("add NONE creature to respawns for lineage " + lineage2.SpawnerID.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.Log(string.Format("[Nutils] invaild lineage Creature"));
                    }
                }
            }
            if (RainWorld.ShowLogs)
            {
                Debug.Log("==== WORLD CREATURE DENSITY STATS ====");
                string str = "Config: region: ";
                string name = self.world.name;
                string str2 = " slugcatIndex: ";
                SlugcatStats.Name name2 = self.playerCharacter;
                Debug.Log(str + name + str2 + ((name2 != null) ? name2.ToString() : null));
                Debug.Log("ROOMS: " + self.creatureStats[0].ToString() + " SPAWNERS: " + self.creatureStats[1].ToString());
                Debug.Log("Room to spawner density: " + (self.creatureStats[1] / self.creatureStats[0]).ToString());
                Debug.Log("Creature spawn counts: ");
                for (int n = 0; n < ExtEnum<CreatureTemplate.Type>.values.entries.Count; n++)
                {
                    if (self.creatureStats[4 + n] > 0f)
                    {
                        Debug.Log(string.Concat(new string[]
                        {
                        ExtEnum<CreatureTemplate.Type>.values.entries[n],
                        " spawns: ",
                        self.creatureStats[4 + n].ToString(),
                        " Spawner Density: ",
                        (self.creatureStats[4 + n] / self.creatureStats[1]).ToString(),
                        " Room Density: ",
                        (self.creatureStats[4 + n] / self.creatureStats[0]).ToString()
                        }));
                    }
                }
                Debug.Log("================");
            }
        }

        private static void WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues(On.WorldLoader.orig_ctor_RainWorldGame_Name_bool_string_Region_SetupValues orig, WorldLoader self, RainWorldGame game, SlugcatStats.Name playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
        {

            if (game != null && game.session is DreamGameSession dream1 && dream1.owner.OverrideDefaultSpawn)
                orig(self, game, dream1.owner.DefaultSpawnName, singleRoomWorld, worldName, region, setupValues);
            orig(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);
        }

        #endregion

        #region hud
        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            if (cam.game.session is DreamGameSession)
            {
                if (!(cam.game.session as DreamGameSession).owner.HasHUD)
                    return;
                self.AddPart(new TextPrompt(self));
                self.AddPart(new KarmaMeter(self, self.fContainers[1], new IntVector2((self.owner as Player).Karma, (self.owner as Player).KarmaCap), (self.owner as Player).KarmaIsReinforced));
                self.AddPart(new FoodMeter(self, (self.owner as Player).slugcatStats.maxFood, (self.owner as Player).slugcatStats.foodToHibernate, null, 0));
                self.AddPart(new RainMeter(self, self.fContainers[1]));
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

        #endregion

        #region world
        private static void OverWorld_LoadFirstWorld(On.OverWorld.orig_LoadFirstWorld orig, OverWorld self)
        {
            if (self.game.session is DreamGameSession session)
            {
                var text = self.FIRSTROOM = self.game.startingRoom = session.owner.FirstRoom;
                if (!session.owner.IsSingleWorld)
                    self.LoadWorld(text.Split('_')[0].ToUpper(), self.game.session.characterStats.name, session.owner.IsSingleWorld);
                else
                    self.LoadWorld(self.game.startingRoom, self.game.session.characterStats.name, session.owner.IsSingleWorld);

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
        #endregion

        #region Game

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

        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            if (self.session is DreamGameSession session)
            {
                session.Base_Update();
            }
            orig(self);
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
                        if (activeDream != null)
                        {

                            self.session = activeDream.GetSession(self, game.session.characterStats.name);
                            self.rainWorld.setup.worldCreaturesSpawn = activeDream.AllowDefaultSpawn;
                            activeDream = null;
                        }
                    }
                });
            }
        }

        static private void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
        {
            if (!(self.session is DreamGameSession))
            {
                foreach (var dream in sessionDreamTreatments)
                {
                    if (dream.HasDreamThisCycle(self, malnourished))
                    {
                        activeDream = dream;
                        break;
                    }
                }
                if (activeDream != null)
                {
                    Log("Try entering dream");
                    self.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
                    ma = malnourished;
                    self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                    return;
                }
            }
            orig(self, malnourished);
        }


        private static bool MultiplayerUnlocks_IsLevelUnlocked(On.MultiplayerUnlocks.orig_IsLevelUnlocked orig, MultiplayerUnlocks self, string levelName)
        {
            foreach (var data in sessionDreamTreatments)
            {
                if (data.IsSingleWorld && data.HiddenRoomInArena && data.FirstRoom.ToLower() == levelName.ToLower())
                    return false;
            }
            return orig(self, levelName);
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

        #endregion

        

        #endregion
        #region NormalDreamHoox
        static bool normalDreamRegisted;
        public static void NormalDreamHooksOn()
        {
            if (normalDreamRegisted) return;

            DreamFix.InitIL();
            DreamFix.Init();

            On.DreamsState.StaticEndOfCycleProgress += DreamsState_StaticEndOfCycleProgress;
            On.Menu.DreamScreen.Update += DreamScreen_Update;
            On.Menu.DreamScreen.SceneFromDream += DreamScreen_SceneFromDream;

            On.OverWorld.LoadFirstWorld += OverWorld_LoadFirstWorld_NormalDream;
            On.WorldLoader.CreatingWorld += WorldLoader_CreatingWorld_NormalDream;


            On.RainWorldGame.GameOver += RainWorldGame_GameOver;
            On.RainWorldGame.Win += RainWorldGame_Win_NormalDream;
            On.RainWorldGame.SpawnPlayers_bool_bool_bool_bool_WorldCoordinate += RainWorldGame_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate;

            On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;

            On.StoryGameSession.TimeTick += StoryGameSession_TimeTick;

            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgression_GetOrInitiateSaveState;
            On.PlayerProgression.SaveDeathPersistentDataOfCurrentState += PlayerProgression_SaveDeathPersistentDataOfCurrentState;

            On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;

            normalDreamRegisted = true;
        }

        private static AbstractCreature RainWorldGame_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate(On.RainWorldGame.orig_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate orig, RainWorldGame self, bool player1, bool player2, bool player3, bool player4, WorldCoordinate location)
        {
            if (currentActivateNormalDream != null && currentActivateNormalDream.dreamStarted)
            {
                var param = currentActivateNormalDream.GetBuildDreamWorldParams();
                if (param.overridePlayerPos != null)
                {
                    location.x = param.overridePlayerPos.Value.x;
                    location.y = param.overridePlayerPos.Value.y;
                }
            }
            return orig.Invoke(self, player1, player2, player3, player4, location);
        }

        private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            orig.Invoke(self, ID);
            if (currentActivateNormalDream != null && currentActivateNormalDream.dreamStarted && currentActivateNormalDream.dreamFinished)
            {
                currentActivateNormalDream.CleanUpThisDream();
            }
            if (currentActivateNormalDream != null && currentActivateNormalDream.dreamStarted && ID != ProcessManager.ProcessID.SleepScreen && currentActivateNormalDream.dreamStarted && ID != ProcessManager.ProcessID.Game)
            {
                currentActivateNormalDream.CleanUpThisDream();
            }
        }

        private static void PlayerProgression_SaveDeathPersistentDataOfCurrentState(On.PlayerProgression.orig_SaveDeathPersistentDataOfCurrentState orig, PlayerProgression self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            EmgTxCustom.Log(String.Format("Inside PlayerProgression_SaveDeathPersistentDataOfCurrentState, {0}", currentActivateNormalDream));
            if (currentActivateNormalDream != null && currentActivateNormalDream.IsPerformDream)
            {
                EmgTxCustom.Log(currentActivateNormalDream.dreamStarted.ToString());
                if (currentActivateNormalDream.dreamStarted)
                {
                    EmgTxCustom.Log(string.Format("PlayerProgression_SaveDeathPersistentDataOfCurrentState, block save : die {0}, quit {1}", saveAsIfPlayerDied, saveAsIfPlayerQuit));
                    return;
                }

            }
            orig.Invoke(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
        }

        private static void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            if (normalDreamDataBridge.sleepDeathScreenDataPackage != null)
            {
                package = normalDreamDataBridge.sleepDeathScreenDataPackage;
                normalDreamDataBridge.sleepDeathScreenDataPackage = null;
            }

            orig.Invoke(self, package);
        }

        private static SaveState PlayerProgression_GetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
        {
            EmgTxCustom.Log(String.Format("Inside PlayerProgression_GetOrInitiateSaveState, {0}", currentActivateNormalDream));
            if (currentActivateNormalDream != null)
            {
                saveAsDeathOrQuit = !currentActivateNormalDream.dreamStarted;
                EmgTxCustom.Log("Dream started , progression getOrInititateSaveState saveAsDeathOrQuit " + saveAsDeathOrQuit.ToString());
            }
            return orig.Invoke(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
        }

        private static void StoryGameSession_TimeTick(On.StoryGameSession.orig_TimeTick orig, StoryGameSession self, float dt)
        {
            if (currentActivateNormalDream != null && currentActivateNormalDream.dreamFinished)
                return;
            orig.Invoke(self, dt);
        }

        private static void RainWorldGame_Win_NormalDream(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
        {
            if (currentActivateNormalDream != null && currentActivateNormalDream.dreamStarted && currentActivateNormalDream.IsPerformDream)
            {
                currentActivateNormalDream.EndDream(self);
                return;
            }
            orig.Invoke(self, malnourished);
        }

        private static void RainWorldGame_GameOver(On.RainWorldGame.orig_GameOver orig, RainWorldGame self, Creature.Grasp dependentOnGrasp)
        {
            if (currentActivateNormalDream != null && currentActivateNormalDream.dreamStarted && currentActivateNormalDream.IsPerformDream)
            {
                currentActivateNormalDream.EndDream(self);
                return;
            }
            orig.Invoke(self, dependentOnGrasp);
        }

        #region WorldLoader & OverWorld
        private static void OverWorld_LoadFirstWorld_NormalDream(On.OverWorld.orig_LoadFirstWorld orig, OverWorld self)
        {
            if (currentActivateNormalDream != null && currentActivateNormalDream.IsPerformDream)
            {
                var param = currentActivateNormalDream.GetBuildDreamWorldParams();

                string room = param.firstRoom;
                string region = room.Split('_').First().ToUpper();

                self.game.startingRoom = room;
                self.LoadWorld(region, self.PlayerCharacterNumber, param.singleRoomWorld);
                self.FIRSTROOM = room;

                EmgTxCustom.Log("OverWorld load room");
                return;
            }
            orig.Invoke(self);
        }
        private static void WorldLoader_CreatingWorld_NormalDream(On.WorldLoader.orig_CreatingWorld orig, WorldLoader self)
        {
            if (self.game != null && self.game.session is StoryGameSession && currentActivateNormalDream != null && currentActivateNormalDream.IsPerformDream)
            {
                self.world.spawners = self.spawners.ToArray();
                List<World.Lineage> list = new List<World.Lineage>();
                for (int i = 0; i < self.spawners.Count; i++)
                {
                    if (self.spawners[i] is World.Lineage)
                    {
                        list.Add(self.spawners[i] as World.Lineage);
                    }
                }
                self.world.lineages = list.ToArray();
                if (self.loadContext == WorldLoader.LoadingContext.FASTTRAVEL || self.loadContext == WorldLoader.LoadingContext.MAPMERGE)
                {
                    self.world.LoadWorldForFastTravel(self.playerCharacter, self.abstractRooms, self.swarmRoomsList.ToArray(), self.sheltersList.ToArray(), self.gatesList.ToArray());
                }
                else
                {
                    self.world.LoadWorld(self.playerCharacter, self.abstractRooms, self.swarmRoomsList.ToArray(), self.sheltersList.ToArray(), self.gatesList.ToArray());
                    self.creatureStats[0] = (float)self.world.NumberOfRooms;
                    self.creatureStats[1] = (float)self.world.spawners.Length;
                }
                self.fliesMigrationBlockages = new int[self.tempBatBlocks.Count, 2];
                for (int j = 0; j < self.tempBatBlocks.Count; j++)
                {
                    int num = (self.world.GetAbstractRoom(self.tempBatBlocks[j].fromRoom) == null) ? -1 : self.world.GetAbstractRoom(self.tempBatBlocks[j].fromRoom).index;
                    int num2 = (self.world.GetAbstractRoom(self.tempBatBlocks[j].destRoom) == null) ? -1 : self.world.GetAbstractRoom(self.tempBatBlocks[j].destRoom).index;
                    self.fliesMigrationBlockages[j, 0] = num;
                    self.fliesMigrationBlockages[j, 1] = num2;
                }

                return;
            }
            orig.Invoke(self);
        }
        #endregion

        #region DreamScreen
        private static void DreamScreen_Update(On.Menu.DreamScreen.orig_Update orig, Menu.DreamScreen self)
        {
            orig.Invoke(self);
            if (self.manager.fadeToBlack > 0.9f && currentActivateNormalDream != null && currentActivateNormalDream.IsPerformDream)
            {
                self.manager.fadeToBlack = 1f;
                self.scene.RemoveSprites();

                currentActivateNormalDream.dreamStarted = true;
                normalDreamDataBridge.sleepDeathScreenDataPackage = self.fromGameDataPackage;
                self.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
                self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
            }
        }

        private static MenuScene.SceneID DreamScreen_SceneFromDream(On.Menu.DreamScreen.orig_SceneFromDream orig, DreamScreen self, DreamsState.DreamID dreamID)
        {
            MenuScene.SceneID sceneID = orig.Invoke(self, dreamID);
            if (sceneID == MenuScene.SceneID.Empty && currentActivateNormalDream != null && !currentActivateNormalDream.IsPerformDream)
            {
                sceneID = currentActivateNormalDream.SceneFromDream(dreamID);
                currentActivateNormalDream.dreamStarted = true;
                currentActivateNormalDream.dreamFinished = true;
            }
            return sceneID;
        }

        private static void DreamsState_StaticEndOfCycleProgress(On.DreamsState.orig_StaticEndOfCycleProgress orig, SaveState saveState, string currentRegion, string denPosition, ref int cyclesSinceLastDream, ref int cyclesSinceLastFamilyDream, ref int cyclesSinceLastGuideDream, ref int inGWOrSHCounter, ref DreamsState.DreamID upcomingDream, ref DreamsState.DreamID eventDream, ref bool everSleptInSB, ref bool everSleptInSB_S01, ref bool guideHasShownHimselfToPlayer, ref int guideThread, ref bool guideHasShownMoonThisRound, ref int familyThread)
        {
            orig.Invoke(saveState, currentRegion, denPosition, ref cyclesSinceLastDream, ref cyclesSinceLastFamilyDream, ref cyclesSinceLastGuideDream, ref inGWOrSHCounter, ref upcomingDream, ref eventDream, ref everSleptInSB, ref everSleptInSB_S01, ref guideHasShownHimselfToPlayer, ref guideThread, ref guideHasShownMoonThisRound, ref familyThread);
            if (normalDreamTreatments.Count > 0)
            {
                foreach (var dream in normalDreamTreatments)
                {
                    if (dream.focusSlugcat != saveState.saveStateNumber)
                    {
                        EmgTxCustom.Log(string.Format("{0} not focus on this slugcat : {1}, skip decide dream", dream, saveState.saveStateNumber));
                        continue;
                    }

                    dream.DecideDreamID(saveState, currentRegion, denPosition, ref cyclesSinceLastDream, ref cyclesSinceLastFamilyDream, ref cyclesSinceLastGuideDream, ref inGWOrSHCounter, ref upcomingDream, ref eventDream, ref everSleptInSB, ref everSleptInSB_S01, ref guideHasShownHimselfToPlayer, ref guideThread, ref guideHasShownMoonThisRound, ref familyThread);
                    if (upcomingDream != null)
                    {
                        dream.ActivateThisDream(upcomingDream);
                        break;
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}
