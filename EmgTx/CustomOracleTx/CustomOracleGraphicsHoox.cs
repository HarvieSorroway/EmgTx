using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomOracleTx
{
    public class OracleGraphicsModuleHooks
    {
        public static void HookOn()
        {
            IL.OracleGraphics.ArmJointGraphics.ApplyPalette += ArmJointGraphics_ApplyPalette;
            On.OracleGraphics.ArmJointGraphics.BaseColor += ArmJointGraphics_BaseColor;
            On.OracleGraphics.ArmJointGraphics.HighLightColor += ArmJointGraphics_HighLightColor;

            On.OracleGraphics.UbilicalCord.ApplyPalette += UbilicalCord_ApplyPalette1;
            On.OracleGraphics.Gown.Color += Gown_Color;
        }


        private static Color Gown_Color(On.OracleGraphics.Gown.orig_Color orig, OracleGraphics.Gown self, float f)
        {
            if (self.owner is CustomOracleGraphic)
            {
                return (self.owner as CustomOracleGraphic).Gown_Color(self, f);
            }
            return orig.Invoke(self, f);
        }


        private static void UbilicalCord_ApplyPalette1(On.OracleGraphics.UbilicalCord.orig_ApplyPalette orig, OracleGraphics.UbilicalCord self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);
            if (self.owner is CustomOracleGraphic)
            {
                for (int j = 0; j < self.smallCords.GetLength(0); j++)
                {
                    if (self.smallCoordColors[j] == 0)
                    {
                        sLeaser.sprites[self.SmallCordSprite(j)].color = self.owner.armJointGraphics[0].metalColor;
                    }
                    else if (self.smallCoordColors[j] == 1)
                    {
                        sLeaser.sprites[self.SmallCordSprite(j)].color = Color.Lerp((self.owner as CustomOracleGraphic).UbilicalCord_WireCol_1(self), self.owner.armJointGraphics[0].metalColor, 0.5f);
                    }
                    else if (self.smallCoordColors[j] == 2)
                    {
                        sLeaser.sprites[self.SmallCordSprite(j)].color = Color.Lerp((self.owner as CustomOracleGraphic).UbilicalCord_WireCol_2(self), self.owner.armJointGraphics[0].metalColor, 0.5f);
                    }
                }
            }
        }


        #region ArmJointGraphics
        private static void ArmJointGraphics_ApplyPalette(ILContext il)
        {
            ILCursor c1 = new ILCursor(il);
            try
            {
                if (c1.TryGotoNext(MoveType.After,
                    i => i.MatchCallvirt<Texture2D>("GetPixel"),
                    i => i.MatchLdcR4(0.12f),
                    i => i.Match(OpCodes.Call),
                    i => i.Match(OpCodes.Ldfld)
                ))
                {
                    c1.Emit(OpCodes.Ldarg, 0);
                    c1.Emit(OpCodes.Ldarg, 1);
                    c1.Emit(OpCodes.Ldarg, 2);
                    c1.Emit(OpCodes.Ldarg, 3);
                    c1.EmitDelegate<Action<OracleGraphics.ArmJointGraphics, RoomCamera.SpriteLeaser, RoomCamera, RoomPalette>>((self, sLeaser, rCam, paltte) =>
                    {
                        if (self.owner is CustomOracleGraphic)
                        {
                            self.metalColor = (self.owner as CustomOracleGraphic).ArmJoint_MetalColor(self, sLeaser, rCam, paltte);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static Color ArmJointGraphics_BaseColor(On.OracleGraphics.ArmJointGraphics.orig_BaseColor orig, OracleGraphics.ArmJointGraphics self, UnityEngine.Vector2 ps)
        {
            if (self.owner is CustomOracleGraphic)
            {
                return (self.owner as CustomOracleGraphic).ArmJoint_BaseColor(self, ps);
            }
            return orig.Invoke(self, ps);
        }

        private static Color ArmJointGraphics_HighLightColor(On.OracleGraphics.ArmJointGraphics.orig_HighLightColor orig, OracleGraphics.ArmJointGraphics self, Vector2 ps)
        {
            if (self.owner is CustomOracleGraphic)
            {
                return (self.owner as CustomOracleGraphic).ArmJoint_HighLightColor(self, ps);
            }
            return orig.Invoke(self, ps);
        }
        #endregion
    }
}
