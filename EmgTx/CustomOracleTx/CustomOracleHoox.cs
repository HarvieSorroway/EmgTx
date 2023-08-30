using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;

namespace CustomOracleTx
{
    internal static class OracleHoox
    {
        static bool hookOn = false;
        public static void HookOn()
        {
            if (hookOn) return;

            OracleHook();
            OracleGraphicHoox();
            OracleArmHoox.HookOn();

            OracleGraphicsModuleHooks.HookOn();
            OracleGraphicsPropertiesHoox.HookOn();

            On.Room.ReadyForAI += Room_ReadyForAI;
            hookOn = true;
        }

        public static void OracleHook()
        {
            IL.Oracle.ctor += Oracle_ctor;

            On.Oracle.InitiateGraphicsModule += Oracle_InitiateGraphicsModule;

            On.Oracle.CreateMarble += Oracle_CreateMarble;
            On.Oracle.SetUpMarbles += Oracle_SetUpMarbles;
        }

        public static void OracleGraphicHoox()
        {
            IL.OracleGraphics.ctor += OracleGraphics_ctor;
            IL.OracleGraphics.InitiateSprites += OracleGraphics_InitiateSprites;
            IL.OracleGraphics.ApplyPalette += OracleGraphics_ApplyPalette;
        }

        private static void OracleGraphics_InitiateSprites(ILContext il)
        {
            ILCursor c1 = new ILCursor(il);//用于找到 IL_05e7,跳过默认的sprite初始化
            ILCursor c2 = new ILCursor(il);//用于插入委托

            ILLabel skipLabel = null;
            try
            {
                if (c1.TryGotoNext(MoveType.After,
                    i => i.MatchCall<OracleGraphics>("get_IsPebbles"),
                    i => i.Match(OpCodes.Brtrue_S),
                    i => i.MatchLdarg(0),
                    i => i.MatchCall<OracleGraphics>("get_IsPastMoon")
                ))
                {
                    skipLabel = c1.MarkLabel();
                }

            }
            catch (Exception ex1)
            {
                Debug.LogException(ex1);
                return;
            }
            try
            {
                c2.Emit(OpCodes.Ldarg, 0);
                c2.EmitDelegate<Func<OracleGraphics, bool>>((graphic) =>
                {
                    if (graphic is CustomOracleGraphic)
                    {
                        EmgTxCustom.Log("Skip current OracleGraphic InitiateSprites ? " + (!(graphic as CustomOracleGraphic).callBaseInitiateSprites).ToString());
                        return !(graphic as CustomOracleGraphic).callBaseInitiateSprites;
                    }
                    return false;
                });
                c2.Emit(OpCodes.Brtrue_S, skipLabel);
            }
            catch (Exception ex2)
            {
                Debug.LogException(ex2);
                return;
            }
        }

        private static void OracleGraphics_ApplyPalette(ILContext il)
        {
            ILCursor c1 = new ILCursor(il);//用于找到 ret
            ILCursor c2 = new ILCursor(il);//用于找到基类构造函数，并在其后插入委托

            ILLabel retLabel = null;
            try
            {
                c1.Index = il.Instrs.Count - 1;
                retLabel = c1.MarkLabel();
            }
            catch (Exception ex1)
            {
                Debug.LogException(ex1);
                return;
            }

            try
            {
                if (c2.TryGotoNext(MoveType.After,
                    i => i.MatchCall<GraphicsModule>("ApplyPalette")
                ))
                {
                    c2.Emit(OpCodes.Ldarg_0);
                    c2.EmitDelegate<Func<OracleGraphics, bool>>((graphic) =>
                    {
                        if (graphic is CustomOracleGraphic)
                        {
                            EmgTxCustom.Log("Skip current OracleGraphic ApplyPalette ? " + (!(graphic as CustomOracleGraphic).callBaseApplyPalette).ToString());
                            return !(graphic as CustomOracleGraphic).callBaseApplyPalette;
                        }
                        return false;
                    });
                    c2.Emit(OpCodes.Brtrue_S, retLabel);
                }
            }
            catch (Exception ex2)
            {
                Debug.LogException(ex2);
                return;
            }
        }

        private static void OracleGraphics_ctor(ILContext il)
        {
            ILCursor c1 = new ILCursor(il);//用于找到 ret
            ILCursor c2 = new ILCursor(il);//用于找到基类构造函数，并在其后插入委托

            ILLabel retLabel = null;
            try
            {
                c1.Index = il.Instrs.Count - 1;
                retLabel = c1.MarkLabel();
            }
            catch (Exception ex1)
            {
                Debug.LogException(ex1);
                return;
            }

            try
            {
                if (c2.TryGotoNext(MoveType.After,
                    i => i.MatchCall<GraphicsModule>(".ctor"),
                    i => i.Match(OpCodes.Call),
                    i => i.MatchStloc(0)
                ))
                {
                    c2.Emit(OpCodes.Ldarg_0);
                    c2.Emit(OpCodes.Ldarg_1);
                    c2.EmitDelegate<Func<OracleGraphics, PhysicalObject, bool>>((graphic, ow) =>
                    {
                        if (graphic is CustomOracleGraphic)
                        {
                            EmgTxCustom.Log("Skip current OracleGraphic ctor");
                            return true;
                        }
                        return false;
                    });
                    c2.Emit(OpCodes.Brtrue_S, retLabel);
                }
            }
            catch (Exception ex2)
            {
                Debug.LogException(ex2);
                return;
            }
        }

        #region Oracle

        private static void Oracle_SetUpMarbles(On.Oracle.orig_SetUpMarbles orig, Oracle self)
        {
            if (CustomOracleRx.idAndTreatments.TryGetValue(self.ID, out var module))
            {
                module.SetUpMarbles(self);
                return;
            }
            orig.Invoke(self);
        }

        private static void Oracle_CreateMarble(On.Oracle.orig_CreateMarble orig, Oracle self, PhysicalObject orbitObj, Vector2 ps, int circle, float dist, int color)
        {
            if (CustomOracleRx.idAndTreatments.TryGetValue(self.ID, out var module))
            {
                module.CreateMarble(self, orbitObj, ps, circle, dist, color);
                return;
            }
            orig.Invoke(self, orbitObj, ps, circle, dist, color);
        }


        private static void Oracle_ctor(ILContext il)
        {
            ILCursor c1 = new ILCursor(il);//用于找到 IL_0301, 跳过默认的ID加载
            ILCursor c2 = new ILCursor(il);//用于找到Bodychunks初始化，并插入委托

            ILCursor c4 = new ILCursor(il);//用于找到 IL_0736, 返回函数
            ILCursor c5 = new ILCursor(il);//用于加载OracleBehaviour和周边物品

            ILLabel skipLabel = null;
            ILLabel retLabel = null;
            try
            {
                if (c1.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(0),
                    x => x.MatchLdarg(0),
                    x => x.Match(OpCodes.Call),
                    x => x.MatchLdlen(),
                    x => x.MatchConvI4(),
                    x => x.Match(OpCodes.Blt_S)
                 ))
                {
                    skipLabel = c1.MarkLabel();
                    if (skipLabel == null)
                    {
                        EmgTxCustom.Log("Can't find skip label!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }

            try
            {
                if (skipLabel != null && c2.TryGotoNext(MoveType.After,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdcI4(2),
                    i => i.MatchNewarr<BodyChunk>(),
                    i => i.Match(OpCodes.Call)
                ))
                {
                    c2.Emit(OpCodes.Ldarg_0);
                    c2.Emit(OpCodes.Ldarg, 2);
                    c2.EmitDelegate<Func<Oracle, Room, bool>>((self, room) =>
                    {
                        foreach (var customOracle in CustomOracleRx.customOraclesTreatments)
                        {
                            if (room.abstractRoom.name == customOracle.LoadRoom)
                            {
                                EmgTxCustom.Log(String.Format("Load Custom Oracle\nroom : {0}\nID : {1}", customOracle.LoadRoom, customOracle.OracleID));
                                self.ID = customOracle.OracleID;
                                customOracle.oracleRef.SetTarget(self);

                                for (int i = 0; i < self.bodyChunks.Length; i++)
                                {
                                    self.bodyChunks[i] = new BodyChunk(self, i, customOracle.startPos, 6f, 0.5f);
                                }

                                return true;
                            }
                        }
                        return false;
                    });
                    c2.Emit(OpCodes.Brtrue_S, skipLabel);
                }
            }
            catch (Exception ex2)
            {
                Debug.LogException(ex2);
                return;
            }

            try
            {
                c4.Index = il.Instrs.Count - 1;
                retLabel = c4.MarkLabel();
            }
            catch (Exception ex4)
            {
                Debug.LogException(ex4);
            }

            try
            {
                if (c5.TryGotoNext(MoveType.After,
                    i => i.MatchCall<PhysicalObject>("set_buoyancy")
                ))
                {
                    c5.Emit(OpCodes.Ldarg_0);
                    c5.Emit(OpCodes.Ldarg, 2);
                    c5.EmitDelegate<Func<Oracle, Room, bool>>((self, room) =>
                    {
                        foreach (var customOracle in CustomOracleRx.customOraclesTreatments)
                        {
                            if (room.abstractRoom.name == customOracle.LoadRoom)
                            {
                                self.gravity = customOracle.gravity;
                                EmgTxCustom.Log("Load custom oracle baviour and surroundings");
                                customOracle.LoadBehaviourAndSurroundings(ref self, room);

                                return true;
                            }
                        }
                        return false;
                    });
                }
                c5.Emit(OpCodes.Brtrue_S, retLabel);
            }
            catch (Exception ex5)
            {
                Debug.LogException(ex5);
            }
        }

        private static void Oracle_InitiateGraphicsModule(On.Oracle.orig_InitiateGraphicsModule orig, Oracle self)
        {
            foreach (var customOracle in CustomOracleRx.customOraclesTreatments)
            {
                if (customOracle.oracleRef.TryGetTarget(out var oracle) && oracle == self)
                {
                    if (self.graphicsModule == null) self.graphicsModule = customOracle.InitCustomOracleGraphic(self);
                    return;
                }
            }
            orig.Invoke(self);
        }
        #endregion

        private static void Room_ReadyForAI(On.Room.orig_ReadyForAI orig, Room self)
        {
            orig.Invoke(self);
            if (self.game != null && self.game.IsStorySession)
            {
                foreach (var customOracle in CustomOracleRx.customOraclesTreatments)
                {
                    if (customOracle.LoadRoom == self.abstractRoom.name)
                    {
                        Oracle oracle = new Oracle(new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(self.abstractRoom.index, 15, 15, -1), self.game.GetNewID()), self);
                        self.AddObject(oracle);
                        EmgTxCustom.Log("Create Oracle for ID : {0}", customOracle.OracleID);
                    }
                }
            }
        }

        public static Func<Oracle.OracleID, Oracle.OracleID, bool> JudgeOracleID = (orig, origCondition) =>
        {
            if (CustomOracleRx.idAndTreatments.TryGetValue(orig, out var customOracle) && customOracle.InheritOracleID != null)
            {
                //Plugin.Log(String.Format("CustomOracle : {0}, origCondition : {1}, inheritOracleID : {2}", orig, origCondition, customOracle.InheritOracleID));
                return (orig == origCondition) || (customOracle.InheritOracleID == origCondition);
            }
            return orig == origCondition;
        };

        public static void AddCustomIDJudgement(ILContext il)
        {
            ILCursor c1 = new ILCursor(il);

            while (c1.TryGotoNext(MoveType.After,
                     i => i.MatchLdarg(0),
                     i => i.MatchLdfld<Oracle.OracleArm>("oracle"),
                     i => i.MatchLdfld<Oracle>("ID"),
                     i => i.Match(OpCodes.Ldsfld)
            ))
            {
                c1.Remove();
                c1.EmitDelegate(JudgeOracleID);
            }
        }
    }


    public class OracleArmHoox
    {
        public static void HookOn()
        {
            IL.Oracle.OracleArm.Update += OracleArm_Update;
            IL.Oracle.OracleArm.BaseDir += OracleArm_BaseDir;
            IL.Oracle.OracleArm.OnFramePos += OracleArm_OnFramePos;
        }

        private static void OracleArm_OnFramePos(ILContext il)
        {
            try
            {
                OracleHoox.AddCustomIDJudgement(il);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void OracleArm_BaseDir(ILContext il)
        {
            try
            {
                OracleHoox.AddCustomIDJudgement(il);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void OracleArm_Update(ILContext il)
        {
            try
            {
                OracleHoox.AddCustomIDJudgement(il);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

    }

    public class OracleGraphicsPropertiesHoox
    {
        static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
        static BindingFlags methodFlags = BindingFlags.Static | BindingFlags.Public;

        public static void HookOn()
        {
            Hook isPebblesHook = new Hook(typeof(OracleGraphics).GetProperty("IsPebbles", propFlags).GetGetMethod(), typeof(OracleGraphicsPropertiesHoox).GetMethod("get_IsPebbles"));
            Hook isIsMoonHook = new Hook(typeof(OracleGraphics).GetProperty("IsPebbles", propFlags).GetGetMethod(), typeof(OracleGraphicsPropertiesHoox).GetMethod("get_IsMoon"));
        }

        public static bool get_IsPebbles(Func<OracleGraphics, bool> orig, OracleGraphics self)
        {
            bool result = orig.Invoke(self);
            if (CustomOracleRx.idAndTreatments.TryGetValue(self.oracle.ID, out var customOracle) && customOracle.InheritOracleID != null)
            {
                result |= customOracle.InheritOracleID == Oracle.OracleID.SS;
            }
            return result;
        }

        public static bool get_IsMoon(Func<OracleGraphics, bool> orig, OracleGraphics self)
        {
            bool result = orig.Invoke(self);
            if (CustomOracleRx.idAndTreatments.TryGetValue(self.oracle.ID, out var customOracle) && customOracle.InheritOracleID != null)
            {
                result |= customOracle.InheritOracleID == Oracle.OracleID.SL;
            }
            return result;
        }
    }
}
