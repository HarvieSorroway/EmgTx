using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmgTx
{
    using BepInEx.Logging;
    using Menu;
    using Mono.Cecil.Cil;
    using MonoMod.Cil;
    using System;
    using System.Collections.Generic;

    namespace EmgTx
    {

        public static class SlideSceneHoox
        {
            /// <summary>
            /// 添加新的SlideShow
            /// </summary>
            /// <param name="ID">新的过场ID</param>
            /// <param name="music">过场时的背景音乐</param>
            /// <param name="buildSlideAction">创建新SlideShow的函数</param>
            public static void AddSlideShow(SlideShow.SlideShowID ID, string music,  Action<SlideShow> buildSlideAction)
            {
                OnModsInit();
                SlideShowArg arg;
                arg.music = music;
                arg.buildSlideAction = buildSlideAction;
                slideArgs.Add(ID,arg);
            }


            /// <summary>
            /// 添加单个场景（slide Show是由多个场景依次播放的）
            /// </summary>
            /// <param name="id">过场ID</param>
            /// <param name="action">创建单个scene的函数</param>
            public static void AddSingleScene(MenuScene.SceneID id, Action<MenuScene> action)
            {
                OnModsInit();
                sceneArgs.Add(id, action);
            }

            /// <summary>
            /// 将对应名字猫的入场slideShow改为对应ID的
            /// </summary>
            /// <param name="name"></param>
            /// <param name="ID"></param>
            public static void AddIntro(SlugcatStats.Name name, SlideShow.SlideShowID ID)
            {
                OnModsInit();
                introArgs.Add(name,ID);
            }


            /// <summary>
            /// 将对应名字猫的飞升slideShow改为对应ID的
            /// </summary>
            /// <param name="name"></param>
            /// <param name="ID"></param>
            public static void AddOutro(SlugcatStats.Name name, SlideShow.SlideShowID ID)
            {
                OnModsInit();
                outroArgs.Add(name,ID);
            }

            #region Hook
            static bool loaded = false;
            static SlideSceneHoox()
            {
                slideArgs = new Dictionary<SlideShow.SlideShowID, SlideShowArg>();
                sceneArgs = new Dictionary<MenuScene.SceneID, Action<MenuScene>>();
                introArgs = new Dictionary<SlugcatStats.Name, SlideShow.SlideShowID>();
                outroArgs = new Dictionary<SlugcatStats.Name, SlideShow.SlideShowID>();
            }


            static void OnModsInit()
            {
                if (!loaded)
                {

                    IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGameIL;
                    IL.RainWorldGame.ExitToVoidSeaSlideShow += RainWorldGame_ExitToVoidSeaSlideShowIL;
                    On.Menu.SlideShow.ctor += SlideShow_ctor;
                    On.Menu.SlideShow.NextScene += SlideShow_NextScene;
                    On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
                    loaded = true;
                }

            }


            private static void RainWorldGame_ExitToVoidSeaSlideShowIL(ILContext il)
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After, i => i.MatchLdfld<MainLoopProcess>("manager"),
                                                 i => i.MatchLdsfld<SlideShow.SlideShowID>("WhiteOutro")))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<SlideShow.SlideShowID, RainWorldGame, SlideShow.SlideShowID>>((id, game) =>
                    {
                        if(outroArgs.ContainsKey(game.session.characterStats.name))
                            return outroArgs[game.session.characterStats.name];
                        return id;
                    });
                }
            }

            private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
            {
                orig(self);
                if (self.sceneID != null && sceneArgs.ContainsKey(self.sceneID))
                    sceneArgs[self.sceneID](self);
            }


            private static void SlideShow_NextScene(On.Menu.SlideShow.orig_NextScene orig, SlideShow self)
            {
                if (self.preloadedScenes.Length == 0)
                    return;
                orig(self);
            }


            private static void SlideShow_ctor(On.Menu.SlideShow.orig_ctor orig, SlideShow self, ProcessManager manager, SlideShow.SlideShowID slideShowID)
            {
                //处理音乐部分

                if (slideArgs.ContainsKey(slideShowID))
                {
                    self.waitForMusic = slideArgs[slideShowID].music;
                    self.stall = true;
                    manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                }

                orig(self, manager, slideShowID);

                self.processAfterSlideShow = ProcessManager.ProcessID.Game;
                if (slideArgs.ContainsKey(slideShowID))
                {
                    var arg = slideArgs[slideShowID];
                    if (arg.buildSlideAction == null)
                        return;
                    arg.buildSlideAction(self);
                    self.preloadedScenes = new SlideShowMenuScene[self.playList.Count];
                    for (int num10 = 0; num10 < self.preloadedScenes.Length; num10++)
                    {
                        self.preloadedScenes[num10] = new SlideShowMenuScene(self, self.pages[0], self.playList[num10].sceneID);
                        self.preloadedScenes[num10].Hide();
                    }
                }
                self.NextScene();
            }

            private static void SlugcatSelectMenu_StartGameIL(ILContext il)
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                    i => i.MatchLdfld<MainLoopProcess>("manager"),
                    i => i.MatchLdsfld<ProcessManager.ProcessID>("Game")))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldarg_1);
                    c.EmitDelegate<Func<ProcessManager.ProcessID, SlugcatSelectMenu, SlugcatStats.Name, ProcessManager.ProcessID>>((id, self, name) =>
                    {
                        if (introArgs.ContainsKey(name))
                        {
                            self.manager.nextSlideshow = introArgs[name];
                            return ProcessManager.ProcessID.SlideShow;
                        }
                        return id;
                    });

                }

            }

            static Dictionary<SlideShow.SlideShowID, SlideShowArg> slideArgs;
            static Dictionary<MenuScene.SceneID, Action<MenuScene>> sceneArgs;

            static Dictionary<SlugcatStats.Name, SlideShow.SlideShowID> introArgs;
            static Dictionary<SlugcatStats.Name, SlideShow.SlideShowID> outroArgs;
            #endregion
        }
        public struct SlideShowArg
        {
            public string music;
            public Action<SlideShow> buildSlideAction;
        }


    }

}
