using System;
using System.Collections.Generic;
using UnityEngine;
using HUD;
using MoreSlugcats;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod;
using HunterExpansion.CustomOracle;
using Menu;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace CustomDreamTx
{
    internal class DreamFix
    {
        public static void InitIL()
        {
            //这是在修梦境结束时扔出珍珠使游戏崩溃的问题
            IL.DataPearl.Update += DataPearl_UpdateIL;
            //这是在修多人联机时，梦境结束会卡在雨眠界面，雨眠cg疯狂抖动的问题
            IL.RainWorldGame.CommunicateWithUpcomingProcess += RainWorldGame_CommunicateWithUpcomingProcessIL;
        }

        public static void Init()
        {
            //这是在修Emgtx的bug，解决与速通计时器的冲突
            On.MoreSlugcats.SpeedRunTimer.Update += SpeedRunTimer_Update;
        }

        #region IL Hooks
        public static void DataPearl_UpdateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //对self.room.game.GetStorySession.playerSessionRecords[num]进行null检查
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Br_S),
                    (i) => i.Match(OpCodes.Ldc_I4_0),
                    (i) => i.Match(OpCodes.Stloc_2),
                    (i) => i.Match(OpCodes.Ldloc_2)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, DataPearl, bool>>((flag, self) =>
                    {
                        return flag && (self.room.game.GetStorySession.playerSessionRecords[(self.grabbedBy[0].grabber as Player).playerState.playerNumber] != null);
                    });
                    c.Emit(OpCodes.Stloc_2);
                    c.Emit(OpCodes.Ldloc_2);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void RainWorldGame_CommunicateWithUpcomingProcessIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                ILCursor find = new ILCursor(il);
                ILLabel pos = null;
                //找到循环结束的地方
                if (find.TryGotoNext(MoveType.After,
                    (i) => i.MatchCallvirt(out var method) && method.Name == "AddRange"))//(i) => i.MatchCallvirt<List<PlayerSessionRecord.KillRecord>>("AddRange")
                {
                    pos = find.MarkLabel();
                }
                //当self.GetStorySession.playerSessionRecords[i] == null时，需要跳过这一循环

                //对self.GetStorySession.playerSessionRecords[i]进行null检查
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.MatchLdsfld<ModManager>("CoopAvailable"),
                    (i) => i.Match(OpCodes.Brfalse_S),
                    (i) => i.Match(OpCodes.Ldc_I4_1),
                    (i) => i.MatchStloc(8),
                    (i) => i.Match(OpCodes.Br_S),
                    (i) => i.Match(OpCodes.Ldarg_0)))
                {
                    if (pos != null)
                    {
                        c.Emit(OpCodes.Ldloc_S, (byte)8);//找到i的本地变量
                        c.EmitDelegate<Func<RainWorldGame, int, bool>>((self, i) =>
                        {
                            return (self.GetStorySession.playerSessionRecords[i] != null);
                        });
                        c.Emit(OpCodes.Brfalse_S, pos);
                        c.Emit(OpCodes.Ldarg_0);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion
        public static void SpeedRunTimer_Update(On.MoreSlugcats.SpeedRunTimer.orig_Update orig, SpeedRunTimer self)
        {
            if (self.ThePlayer().abstractCreature.world.game.GetStorySession.playerSessionRecords[0] == null)
            {
                return;
            }
            orig(self);
        }
    }
}
