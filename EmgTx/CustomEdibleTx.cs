using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using UnityEngine;

namespace EmgTx
{
    /// <summary>
    /// 用于添加或禁止猫食用部分物品
    /// </summary>
    public class CustomEdibleTx
    {
        public SlugcatStats.Name name;

        public FoodData[] edibleDatas;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">猫的名字</param>
        /// <param name="edibleDatas">可食用与禁止食用配置，可传递多组</param>
        public CustomEdibleTx(SlugcatStats.Name name,params FoodData[] edibleDatas)
        {
            this.name = name;
            this.edibleDatas = edibleDatas;
        }

        public class FoodData
        {
            public AbstractPhysicalObject.AbstractObjectType edibleType;
            public AbstractPhysicalObject.AbstractObjectType forbidType;
            public int food;
            public int qFood;
            
            /// <summary>
            /// 添加新的禁止项
            /// </summary>
            /// <param name="forbidType">禁止的食物的类型</param>
            public FoodData(AbstractPhysicalObject.AbstractObjectType forbidType)
            {
                this.forbidType = forbidType;
                food = qFood = -1;
            }


            /// <summary>
            /// 添加新的可食用项
            /// </summary>
            /// <param name="edibleType">可食用的物体类型</param>
            /// <param name="food">食用回复的整数饱食度</param>
            /// <param name="quarterFood">食用回复的小数饱食度(1/4格)</param>
            public FoodData(AbstractPhysicalObject.AbstractObjectType edibleType, int food, int quarterFood)
            {
                this.edibleType = edibleType;
                this.food = food;
                this.qFood = quarterFood;
            }
        }
    }

    public static class CustomEdibleRx
    {
        public static void ApplyTreatment(CustomEdibleTx treatment)
        {
            CustomEdibleHoox.HookOn();
            if (!edibleTxes.ContainsKey(treatment.name))
            {
                edibleTxes.Add(treatment.name,treatment);
            }
            else
            {
                EmgTxCustom.Log("Already apply for cat :{0}",treatment.name);
            }
        }

        public static Dictionary<SlugcatStats.Name, CustomEdibleTx> edibleTxes =
            new Dictionary<SlugcatStats.Name, CustomEdibleTx>();
    }

    static class CustomEdibleHoox
    {
        static bool hookOn;
        public static void HookOn()
        {
            if (!hookOn)
            {
                IL.Player.GrabUpdate += Player_GrabUpdate_EdibleIL;
                On.Player.BiteEdibleObject += Player_BiteEdibleObject;
                hookOn = true;
            }
        }


        private static void Player_GrabUpdate_EdibleIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                ILLabel label = c.DefineLabel();
                ILLabel label2 = c.DefineLabel();
                c.GotoNext(MoveType.Before, i => i.MatchLdarg(0),
                                           i => i.MatchCall<Creature>("get_grasps"),
                                           i => i.MatchLdloc(13),
                                           i => i.MatchLdelemRef(),
                                           i => i.MatchLdfld<Creature.Grasp>("grabbed"),
                                           i => i.MatchIsinst<IPlayerEdible>());
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_S, (byte)13);
                c.EmitDelegate<Func<Player,int, bool>>(EdibleForCat);
                c.Emit(OpCodes.Brtrue_S, label);

                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_S, (byte)13);
                c.EmitDelegate<Func<Player, int, bool>>((self, index) =>
                {
                    if (CustomEdibleRx.edibleTxes.ContainsKey(self.slugcatStats.name) &&
                        CustomEdibleRx.edibleTxes[self.slugcatStats.name].edibleDatas.Any(i =>
                            i.forbidType == self.grasps[index].grabbed.abstractPhysicalObject.type))
                        return false;
                    return true;
                });
                c.Emit(OpCodes.Brfalse_S, label2);
                c.GotoNext(MoveType.Before, i => i.MatchLdloc(13),
                                            i => i.MatchStloc(6),
                                            i => i.MatchLdloc(13));
                c.MarkLabel(label);
                c.GotoNext(MoveType.After, i => i.MatchStloc(6));
                c.MarkLabel(label2);

            }
            catch (Exception e)
            {
               Debug.LogException(e);
            }
        }

        static bool EdibleForCat(Player player,int index)
        {
            if (!CustomEdibleRx.edibleTxes.ContainsKey(player.slugcatStats.name))
                return false;
            var grasp = player.grasps[index];

            if (grasp != null)
            {
                if (CustomEdibleRx.edibleTxes[player.slugcatStats.name].edibleDatas.
                    Any(i => i.edibleType == grasp.grabbed.abstractPhysicalObject.type))
                    return true;
            }

            return false;
        }
        private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player self, bool eu)
        {
            bool canBitOther = self.grasps.All(i => !(i?.grabbed is IPlayerEdible));
            orig(self, eu);
            if (canBitOther)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null && CustomEdibleRx.edibleTxes.ContainsKey(self.slugcatStats.name) &&
                        CustomEdibleRx.edibleTxes[self.slugcatStats.name].edibleDatas.
                            Any(d => d.edibleType == self.grasps[i].grabbed.abstractPhysicalObject.type))
                    {
                        var data = CustomEdibleRx.edibleTxes[self.slugcatStats.name].edibleDatas.
                            First(d => d.edibleType == self.grasps[i].grabbed.abstractPhysicalObject.type);
                        if (self.SessionRecord != null)
                        {
                            self.SessionRecord.AddEat(self.grasps[i].grabbed);
                        }
                        (self.graphicsModule as PlayerGraphics)?.BiteFly(i);
                        self.AddFood(data.food);
                        for (int j = 0; j < data.qFood; j++)
                            self.AddQuarterFood();
                        var obj = self.grasps[i].grabbed;
                        self.grasps[i].Release();
                        obj.Destroy();
                    }
                }
            }
        }

    }
}
