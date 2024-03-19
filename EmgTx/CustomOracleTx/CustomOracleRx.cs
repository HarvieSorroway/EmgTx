using CustomOracleTx;
using HUD;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;



namespace CustomOracleTx
{
    /// <summary>
    /// 管理类
    /// </summary>
    public static class CustomOracleRx
    {
        public static List<CustomOracleTx> customOraclesTreatments = new List<CustomOracleTx>();
        public static Dictionary<Oracle.OracleID, CustomOracleTx> idAndTreatments = new Dictionary<Oracle.OracleID, CustomOracleTx>();
        public static void ApplyTreatment(CustomOracleTx customOracle)
        {
            if (customOraclesTreatments.Contains(customOracle)) return;
            customOraclesTreatments.Add(customOracle);
            idAndTreatments.Add(customOracle.OracleID, customOracle);
            OracleHoox.HookOn();
        }
    }

    public class CustomOracleTx
    {
        /// <summary>
        /// 迭代器生成的房间
        /// </summary>
        public virtual string LoadRoom => "";

        /// <summary>
        /// 生成的迭代器ID
        /// </summary>
        public virtual Oracle.OracleID OracleID => Oracle.OracleID.SS;

        public virtual Oracle.OracleID InheritOracleID => null;

        /// <summary>
        /// 当前迭代器的gravity
        /// </summary>
        public float gravity = 0.9f;

        /// <summary>
        /// 迭代器在房价中的初始位置
        /// </summary>
        public Vector2 startPos = new Vector2(350f, 350f);

        /// <summary>
        /// 最后一次实例化的Oracle弱引用
        /// </summary>
        public WeakReference<Oracle> oracleRef = new WeakReference<Oracle>(null);

        /// <summary>
        /// 拓展类
        /// </summary>
        public static ConditionalWeakTable<Oracle, CustomOralceEX> oracleEx = new ConditionalWeakTable<Oracle, CustomOralceEX>();
        /// <summary>
        /// 自定义珍珠的注册类，可以选择不使用该类型
        /// </summary>
        public CustomOraclePearlTx pearlRegistry;

        /// <summary>
        /// 加载迭代器的行为和其他模块，比如OracleArm
        /// </summary>
        /// <param name="oracle">迭代器本身</param>
        /// <param name="room">迭代器所在的房间</param>
        public virtual void LoadBehaviourAndSurroundings(ref Oracle oracle, Room room)
        {
            oracleEx.Add(oracle, new CustomOralceEX(oracle));
        }

        /// <summary>
        /// 生成珍珠阵列，与Oracle的同名方法相同
        /// </summary>
        public virtual void SetUpMarbles(Oracle oracle)
        {
            if (!oracleEx.TryGetValue(oracle, out var customOralceEX))
                return;

            var marbles = customOralceEX.customMarbles;
            var room = oracle.room;

            Vector2 vector = new Vector2(200f, 100f);

            for (int i = 0; i < 6; i++)
            {
                Vector2 ps = new Vector2(vector.x + 300f, vector.y + 200f) + Custom.RNV() * 20f;
                int color;
                switch (i)
                {
                    default:
                        color = 0;
                        break;
                    case 5:
                        color = 2;
                        break;
                    case 2:
                    case 3:
                        color = 1;
                        break;
                }
                oracle.CreateMarble(oracle, ps, 0, 35f, color);
            }
            for (int j = 0; j < 2; j++)
            {
                oracle.CreateMarble(oracle, new Vector2(vector.x + 300f, vector.y + 200f) + Custom.RNV() * 20f, 1, 100f, (j == 1) ? 2 : 0);
            }
            oracle.CreateMarble(null, new Vector2(vector.x + 20f, vector.y + 200f), 0, 0f, 1);
            Vector2 vector2 = new Vector2(vector.x + 80f, vector.y + 30f);
            Vector2 vector3 = Custom.DegToVec(-32.7346f);
            Vector2 vector4 = Custom.PerpendicularVector(vector3);

            for (int k = 0; k < 3; k++)
            {
                for (int l = 0; l < 5; l++)
                {
                    if (k != 2 || l != 2)
                    {
                        oracle.CreateMarble(null, vector2 + vector4 * k * 17f + vector3 * l * 17f, 0, 0f, ((k != 2 || l != 0) && (k != 1 || l != 3)) ? 1 : 2);
                    }
                }
            }

            oracle.CreateMarble(null, new Vector2(vector.x + 287f, vector.y + 218f), 0, 0f, 1);

            var centerPearl = marbles[marbles.Count - 1];

            for (int i = 0; i < 6; i++)
            {
                int color;
                switch (i)
                {
                    default:
                        color = 1;
                        break;
                    case 5:
                        color = 2;
                        break;
                    case 2:
                    case 3:
                        color = 0;
                        break;
                }
                oracle.CreateMarble(centerPearl, new Vector2(vector.x + 287f + 14f * i, vector.y + 218f), 0, 14f * i, color);
            }

            oracle.CreateMarble(marbles[marbles.Count - 1], new Vector2(vector.x + 440f, vector.y + 477f), 0, 14f, 0);

            oracle.CreateMarble(null, new Vector2(vector.x + 450f, vector.y + 467f), 0, 0f, 2);
            oracle.CreateMarble(marbles[marbles.Count - 1], new Vector2(vector.x + 440f, vector.y + 477f), 0, 38f, 1);
            oracle.CreateMarble(marbles[marbles.Count - 2], new Vector2(vector.x + 440f, vector.y + 477f), 0, 38f, 2);
            oracle.CreateMarble(null, new Vector2(vector.x + 117f, vector.y), 0, 0f, 2);

            oracle.CreateMarble(null, new Vector2(vector.x + 547f, vector.y + 374f), 0, 0f, 0);
            oracle.CreateMarble(null, new Vector2(vector.x + 114f, vector.y + 500f), 0, 0f, 2);
            oracle.CreateMarble(null, new Vector2(vector.x + 108f, vector.y + 511f), 0, 0f, 2);
            oracle.CreateMarble(null, new Vector2(vector.x + 551f, vector.y + 131f), 0, 0f, 1);
            oracle.CreateMarble(null, new Vector2(vector.x + 560f, vector.y + 124f), 0, 0f, 1);
            oracle.CreateMarble(null, new Vector2(vector.x + 520f, vector.y + 134f), 0, 0f, 0);

            oracle.CreateMarble(null, new Vector2(vector.x + 109f, vector.y + 352f), 0, 0f, 0);

            oracle.CreateMarble(marbles[marbles.Count - 1], new Vector2(vector.x + 109f, vector.y + 352f), 0, 42f, 1);
            marbles[marbles.Count - 1].orbitSpeed = 0.8f;

            oracle.CreateMarble(marbles[marbles.Count - 1], new Vector2(vector.x + 109f, vector.y + 352f), 0, 12f, 0);
        }

        /// <summary>
        /// 在珍珠阵列中创建单颗珍珠，与Oracle的同名方法相同
        /// </summary>
        /// <param name="self"></param>
        /// <param name="orbitObj"></param>
        /// <param name="ps"></param>
        /// <param name="circle"></param>
        /// <param name="dist"></param>
        /// <param name="color"></param>
        public virtual void CreateMarble(Oracle self, PhysicalObject orbitObj, Vector2 ps, int circle, float dist, int color)
        {
            if (self.pearlCounter == 0)
            {
                self.pearlCounter = 1;
            }
            if (pearlRegistry == null)
                return;
            if (!oracleEx.TryGetValue(self, out var customOracleEX))
                return;

            AbstractPhysicalObject abstractPhysicalObject = pearlRegistry.GetAbstractCustomOraclePearl(self.room.world, null, self.room.GetWorldCoordinate(ps), self.room.game.GetNewID(), -1, -1, null, color, self.pearlCounter);
            CustomOrbitableOraclePearl customPearl = pearlRegistry.RealizeDataPearl(abstractPhysicalObject, self.room.world);

            self.pearlCounter++;
            self.room.abstractRoom.entities.Add(abstractPhysicalObject);

            customPearl.oracle = self;
            customPearl.firstChunk.HardSetPosition(ps);
            customPearl.orbitObj = orbitObj;
            if (orbitObj == null)
            {
                customPearl.hoverPos = new Vector2?(ps);
            }
            customPearl.orbitCircle = circle;
            customPearl.orbitDistance = dist;
            customPearl.marbleColor = (abstractPhysicalObject as CustomOrbitableOraclePearl.AbstractCustomOraclePearl).color;
            customPearl.marbleIndex = customOracleEX.customMarbles.Count;

            self.room.AddObject(customPearl);
            customOracleEX.customMarbles.Add(customPearl);
        }

        public override string ToString()
        {
            return base.ToString() + " " + OracleID.ToString() + " " + LoadRoom.ToString();
        }

        public virtual OracleGraphics InitCustomOracleGraphic(PhysicalObject ow)
        {
            return new OracleGraphics(ow);
        }

        public class CustomOralceEX
        {
            public WeakReference<Oracle> oracleRef = new WeakReference<Oracle>(null);
            public List<CustomOrbitableOraclePearl> customMarbles = new List<CustomOrbitableOraclePearl>();

            public CustomOralceEX(Oracle oracle)
            {
                oracleRef = new WeakReference<Oracle>(oracle);
            }
        }
    }

    public class CustomOracleGraphic : OracleGraphics
    {
        public bool callBaseApplyPalette = false;
        public bool callBaseInitiateSprites = false;
        public bool callBaseDrawSprites = false;
        public bool callBaseUpdate = false;

        public CustomOracleStateViz customCOracleStateViz;

        public CustomOracleGraphic(PhysicalObject ow) : base(ow)
        {
            //customCOracleStateViz = new CustomOracleStateViz(oracle);
        }

        #region 默认方法
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
        }

        /// <summary>
        /// 默认的OracleGraphics绘制函数
        /// </summary>
        /// <param name="sLeaser"></param>
        /// <param name="rCam"></param>
        /// <param name="timeStacker"></param>
        /// <param name="camPos"></param>
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (oracle == null || oracle.room == null)
            {
                return;
            }
            Vector2 bodyPos = Vector2.Lerp(owner.firstChunk.lastPos, owner.firstChunk.pos, timeStacker);
            Vector2 bodyDir = Custom.DirVec(Vector2.Lerp(owner.bodyChunks[1].lastPos, owner.bodyChunks[1].pos, timeStacker), bodyPos);
            Vector2 perpendicularBodyDir = Custom.PerpendicularVector(bodyDir);
            Vector2 lookDir = Vector2.Lerp(lastLookDir, base.lookDir, timeStacker);
            Vector2 headPos = Vector2.Lerp(head.lastPos, head.pos, timeStacker);

            for (int i = 0; i < owner.bodyChunks.Length; i++)
            {
                sLeaser.sprites[firstBodyChunkSprite + i].x = Mathf.Lerp(owner.bodyChunks[i].lastPos.x, owner.bodyChunks[i].pos.x, timeStacker) - camPos.x;
                sLeaser.sprites[firstBodyChunkSprite + i].y = Mathf.Lerp(owner.bodyChunks[i].lastPos.y, owner.bodyChunks[i].pos.y, timeStacker) - camPos.y;
            }

            sLeaser.sprites[firstBodyChunkSprite].rotation = Custom.AimFromOneVectorToAnother(bodyPos, headPos) - Mathf.Lerp(14f, 0f, Mathf.Lerp(lastBreatheFac, breathFac, timeStacker));
            sLeaser.sprites[firstBodyChunkSprite + 1].rotation = Custom.VecToDeg(bodyDir);

            for (int j = 0; j < armJointGraphics.Length; j++)
            {
                armJointGraphics[j].DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            //CustomOracleGraphcis Updates
            CustomOracleBehaviour oracleBehaviour = oracle.oracleBehavior as CustomOracleBehaviour;
            if (oracleBehaviour != null)
            {
                CustomOracleDrawBehaviour(sLeaser, rCam, timeStacker, camPos, oracleBehaviour);
            }
            //end

            sLeaser.sprites[fadeSprite].x = headPos.x - camPos.x;
            sLeaser.sprites[fadeSprite].y = headPos.y - camPos.y;
            sLeaser.sprites[neckSprite].x = bodyPos.x - camPos.x;
            sLeaser.sprites[neckSprite].y = bodyPos.y - camPos.y;
            sLeaser.sprites[neckSprite].rotation = Custom.AimFromOneVectorToAnother(bodyPos, headPos);
            sLeaser.sprites[neckSprite].scaleY = Vector2.Distance(bodyPos, headPos);

            if (gown != null)
            {
                gown.DrawSprite(robeSprite, sLeaser, rCam, timeStacker, camPos);
            }
            if (halo != null)
            {
                halo.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
            if (armBase != null)
            {
                armBase.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            Vector2 chinDir = Custom.DirVec(headPos, bodyPos);
            Vector2 perpendicularChinDir = Custom.PerpendicularVector(chinDir);
            sLeaser.sprites[HeadSprite].x = headPos.x - camPos.x;
            sLeaser.sprites[HeadSprite].y = headPos.y - camPos.y;
            sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(chinDir);
            Vector2 relativeLookDir = RelativeLookDir(timeStacker);
            Vector2 chinPos = Vector2.Lerp(headPos, bodyPos, 0.15f);
            chinPos += perpendicularChinDir * relativeLookDir.x * 2f;
            sLeaser.sprites[ChinSprite].x = chinPos.x - camPos.x;
            sLeaser.sprites[ChinSprite].y = chinPos.y - camPos.y;

            float eyesOpen = Mathf.Lerp(lastEyesOpen, base.eyesOpen, timeStacker);

            for (int k = 0; k < 2; k++)
            {
                float leftOrRight = ((k == 0) ? (-1f) : 1f);
                Vector2 eyeSpritePos = headPos + perpendicularChinDir * Mathf.Clamp(relativeLookDir.x * 3f + 2.5f * leftOrRight, -5f, 5f) + chinDir * (1f - relativeLookDir.y * 3f);
                sLeaser.sprites[EyeSprite(k)].rotation = Custom.VecToDeg(chinDir);
                sLeaser.sprites[EyeSprite(k)].scaleX = 1f + ((k == 0) ? Mathf.InverseLerp(-1f, -0.5f, relativeLookDir.x) : Mathf.InverseLerp(1f, 0.5f, relativeLookDir.x)) + (1f - eyesOpen);
                sLeaser.sprites[EyeSprite(k)].scaleY = Mathf.Lerp(1f, IsPebbles ? 2f : 3f, eyesOpen);
                sLeaser.sprites[EyeSprite(k)].x = eyeSpritePos.x - camPos.x;
                sLeaser.sprites[EyeSprite(k)].y = eyeSpritePos.y - camPos.y;
                sLeaser.sprites[EyeSprite(k)].alpha = 0.5f + 0.5f * eyesOpen;
                int side = ((k < 1 != relativeLookDir.x < 0f) ? 1 : 0);
                Vector2 phonePos = headPos + perpendicularChinDir * Mathf.Clamp(Mathf.Lerp(7f, 5f, Mathf.Abs(relativeLookDir.x)) * leftOrRight, -11f, 11f);
                for (int l = 0; l < 2; l++)
                {
                    sLeaser.sprites[PhoneSprite(side, l)].rotation = Custom.VecToDeg(chinDir);
                    sLeaser.sprites[PhoneSprite(side, l)].scaleY = 5.5f * ((l == 0) ? 1f : 0.8f) / 20f;
                    sLeaser.sprites[PhoneSprite(side, l)].scaleX = Mathf.Lerp(3.5f, 5f, Mathf.Abs(relativeLookDir.x)) * ((l == 0) ? 1f : 0.8f) / 20f;
                    sLeaser.sprites[PhoneSprite(side, l)].x = phonePos.x - camPos.x;
                    sLeaser.sprites[PhoneSprite(side, l)].y = phonePos.y - camPos.y;
                }
                sLeaser.sprites[PhoneSprite(side, 2)].x = phonePos.x - camPos.x;
                sLeaser.sprites[PhoneSprite(side, 2)].y = phonePos.y - camPos.y;
                sLeaser.sprites[PhoneSprite(side, 2)].rotation = Custom.AimFromOneVectorToAnother(bodyPos, phonePos - chinDir * 40f - lookDir * 10f);
                Vector2 handPos = Vector2.Lerp(hands[k].lastPos, hands[k].pos, timeStacker);
                Vector2 shoulderPos = bodyPos + perpendicularBodyDir * 4f * ((k == 1) ? (-1f) : 1f);

                if (IsMoon)
                {
                    shoulderPos += bodyDir * 3f;
                }

                Vector2 cB = handPos + Custom.DirVec(handPos, shoulderPos) * 3f + bodyDir;
                Vector2 cA = shoulderPos + perpendicularBodyDir * 5f * ((k == 1) ? (-1f) : 1f);
                sLeaser.sprites[HandSprite(k, 0)].x = handPos.x - camPos.x;
                sLeaser.sprites[HandSprite(k, 0)].y = handPos.y - camPos.y;
                Vector2 vector14 = shoulderPos - perpendicularBodyDir * 2f * ((k == 1) ? (-1f) : 1f);

                float armWidth = (IsPebbles ? 4f : 2f);

                for (int m = 0; m < 7; m++)
                {
                    float f3 = (float)m / 6f;
                    Vector2 vector15 = Custom.Bezier(shoulderPos, cA, handPos, cB, f3);
                    Vector2 vector16 = Custom.DirVec(vector14, vector15);
                    Vector2 vector17 = Custom.PerpendicularVector(vector16) * ((k == 0) ? (-1f) : 1f);
                    float num4 = Vector2.Distance(vector14, vector15);
                    (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4, vector15 - vector16 * num4 * 0.3f - vector17 * armWidth - camPos);
                    (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 1, vector15 - vector16 * num4 * 0.3f + vector17 * armWidth - camPos);
                    (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 2, vector15 - vector17 * armWidth - camPos);
                    (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 3, vector15 + vector17 * armWidth - camPos);
                    vector14 = vector15;
                }
                handPos = Vector2.Lerp(feet[k].lastPos, feet[k].pos, timeStacker);
                shoulderPos = Vector2.Lerp(oracle.bodyChunks[1].lastPos, oracle.bodyChunks[1].pos, timeStacker);
                Vector2 b = Vector2.Lerp(knees[k, 1], knees[k, 0], timeStacker);
                cB = Vector2.Lerp(handPos, b, 0.9f);
                cA = Vector2.Lerp(shoulderPos, b, 0.9f);
                sLeaser.sprites[FootSprite(k, 0)].x = handPos.x - camPos.x;
                sLeaser.sprites[FootSprite(k, 0)].y = handPos.y - camPos.y;
                vector14 = shoulderPos - perpendicularBodyDir * 2f * ((k == 1) ? (-1f) : 1f);
                armWidth = 4f;
                float num5 = 4f;
                for (int n = 0; n < 7; n++)
                {
                    float f4 = (float)n / 6f;
                    armWidth = (IsPebbles ? 2f : Mathf.Lerp(4f, 2f, Mathf.Pow(f4, 0.5f)));
                    Vector2 armMidPos = Custom.Bezier(shoulderPos, cA, handPos, cB, f4);
                    Vector2 vector19 = Custom.DirVec(vector14, armMidPos);
                    Vector2 vector20 = Custom.PerpendicularVector(vector19) * ((k == 0) ? (-1f) : 1f);
                    float num6 = Vector2.Distance(vector14, armMidPos);
                    (sLeaser.sprites[FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4, armMidPos - vector19 * num6 * 0.3f - vector20 * (num5 + armWidth) * 0.5f - camPos);
                    (sLeaser.sprites[FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 1, armMidPos - vector19 * num6 * 0.3f + vector20 * (num5 + armWidth) * 0.5f - camPos);
                    (sLeaser.sprites[FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 2, armMidPos - vector20 * armWidth - camPos);
                    (sLeaser.sprites[FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 3, armMidPos + vector20 * armWidth - camPos);
                    vector14 = armMidPos;
                    num5 = armWidth;
                }
            }

            if (umbCord != null)
            {
                umbCord.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
            else if (discUmbCord != null)
            {
                discUmbCord.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            #region base.DrawSprites():
            if (this.DEBUGLABELS != null && this.DEBUGLABELS.Length != 0)
            {
                foreach (DebugLabel debugLabel in this.DEBUGLABELS)
                {
                    if (debugLabel.relativePos)
                    {
                        debugLabel.label.x = this.owner.bodyChunks[0].pos.x + debugLabel.pos.x - camPos.x;
                        debugLabel.label.y = this.owner.bodyChunks[0].pos.y + debugLabel.pos.y - camPos.y;
                    }
                    else
                    {
                        debugLabel.label.x = debugLabel.pos.x;
                        debugLabel.label.y = debugLabel.pos.y;
                    }
                }
            }
            if (this.owner.slatedForDeletetion || this.owner.room != rCam.room || this.dispose)
            {
                sLeaser.CleanSpritesAndRemove();
            }
            if (sLeaser.sprites[0].isVisible == this.culled)
            {
                for (int j = 0; j < sLeaser.sprites.Length; j++)
                {
                    sLeaser.sprites[j].isVisible = !this.culled;
                }
            }
            #endregion
        }

        /// <summary>
        /// OracleGrpahics的Update函数
        /// </summary>
        public override void Update()
        {
            if (customCOracleStateViz != null)
                customCOracleStateViz.Update();

            //GraphicsModule的Update函数
            #region base.Update();
            lastCulled = culled;
            culled = ShouldBeCulled;
            if (!culled && lastCulled)
            {
                Reset();
            }
            #endregion

            if (oracle == null || oracle.room == null)
            {
                return;
            }
            CustomOracleBehaviour oracleBehaviour = oracle.oracleBehavior as CustomOracleBehaviour;
            if (oracleBehaviour == null) return;

            breathe += 1f / Mathf.Lerp(10f, 60f, oracle.health);
            lastBreatheFac = breathFac;
            breathFac = Mathf.Lerp(0.5f + 0.5f * Mathf.Sin(breathe * 3.1415927f * 2f), 1f, Mathf.Pow(oracle.health, 2f));

            if (gown != null)
                gown.Update();

            if (halo != null)
                halo.Update();

            if (armBase != null)
                armBase.Update();

            lastLookDir = lookDir;
            if (oracle.Consious)
            {
                lookDir = Vector2.ClampMagnitude(oracle.oracleBehavior.lookPoint - oracle.firstChunk.pos, 100f) / 100f;
                lookDir = Vector2.ClampMagnitude(lookDir + randomTalkVector * averageVoice * 0.3f, 1f);
            }
            head.Update();
            head.ConnectToPoint(oracle.firstChunk.pos + Custom.DirVec(oracle.bodyChunks[1].pos, oracle.firstChunk.pos) * 6f, 8f, true, 0f, oracle.firstChunk.vel, 0.5f, 0.01f);

            if (oracle.Consious)
            {
                if (oracleBehaviour.CouldCloseEyes && oracle.oracleBehavior.EyesClosed)
                {
                    head.vel += Custom.DegToVec(-90f);
                }
                else
                {
                    head.vel += Custom.DirVec(oracle.bodyChunks[1].pos, oracle.firstChunk.pos) * breathFac;
                    head.vel += lookDir * 0.5f * breathFac;
                }
            }
            else
            {
                head.vel += Custom.DirVec(oracle.bodyChunks[1].pos, oracle.firstChunk.pos) * 0.75f;
                GenericBodyPart genericBodyPart = head;
                genericBodyPart.vel.y = genericBodyPart.vel.y - 0.7f;
            }

            for (int i = 0; i < 2; i++)
            {
                feet[i].Update();
                feet[i].ConnectToPoint(oracle.bodyChunks[1].pos, IsMoon ? 20f : 10f, false, 0f, oracle.bodyChunks[1].vel, 0.3f, 0.01f);
                if (IsMoon)
                {
                    GenericBodyPart genericBodyPart2 = feet[i];
                    genericBodyPart2.vel.y = genericBodyPart2.vel.y - 0.5f;
                }
                feet[i].vel += Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos) * 0.3f;
                feet[i].vel += Custom.PerpendicularVector(Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos)) * 0.15f * ((i == 0) ? -1f : 1f);
                hands[i].Update();
                hands[i].ConnectToPoint(oracle.firstChunk.pos, 15f, false, 0f, oracle.firstChunk.vel, 0.3f, 0.01f);
                GenericBodyPart genericBodyPart3 = hands[i];
                genericBodyPart3.vel.y = genericBodyPart3.vel.y - 0.5f;
                hands[i].vel += Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos) * 0.3f;
                hands[i].vel += Custom.PerpendicularVector(Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos)) * 0.3f * ((i == 0) ? -1f : 1f);
                knees[i, 1] = knees[i, 0];

                CustomOracleUpdateBehaviour(i, oracleBehaviour);
            }

            for (int j = 0; j < armJointGraphics.Length; j++)
            {
                armJointGraphics[j].Update();
            }

            if (umbCord != null)
                umbCord.Update();
            else
                discUmbCord?.Update();

            if (oracle.oracleBehavior.voice != null && oracle.oracleBehavior.voice.currentSoundObject != null && oracle.oracleBehavior.voice.currentSoundObject.IsPlaying)
            {
                if (oracle.oracleBehavior.voice.currentSoundObject.IsLoaded)
                {
                    oracle.oracleBehavior.voice.currentSoundObject.GetSpectrumData(voiceFreqSamples, 0, FFTWindow.BlackmanHarris);
                    averageVoice = 0f;
                    for (int k = 0; k < voiceFreqSamples.Length; k++)
                    {
                        averageVoice += voiceFreqSamples[k];
                    }
                    averageVoice /= (float)voiceFreqSamples.Length;
                    averageVoice = Mathf.InverseLerp(0f, 0.00014f, averageVoice);
                    if (averageVoice > 0.7f && Random.value < averageVoice / 14f)
                    {
                        randomTalkVector = Custom.RNV();
                    }
                }
            }
            else
            {
                randomTalkVector *= 0.9f;
                if (averageVoice > 0f)
                {
                    for (int l = 0; l < voiceFreqSamples.Length; l++)
                    {
                        voiceFreqSamples[l] = 0f;
                    }
                    averageVoice = 0f;
                }
            }
            lastEyesOpen = eyesOpen;
            eyesOpen = (oracle.oracleBehavior.EyesClosed ? 0f : 1f);
            if (owner.room.game.cameras[0].AboutToSwitchRoom && lightsource != null)
            {
                lightsource.RemoveFromRoom();
                return;
            }
            if (IsPebbles)
            {
                if (lightsource == null)
                {
                    lightsource = new LightSource(oracle.firstChunk.pos, false, Custom.HSL2RGB(0.1f, 1f, 0.5f), oracle);
                    lightsource.affectedByPaletteDarkness = 0f;
                    oracle.room.AddObject(lightsource);
                    return;
                }
                if (IsRottedPebbles)
                {
                    lightsource.setAlpha = 0.5f;
                }
                else
                {
                    lightsource.setAlpha = oracleBehaviour.working;
                }
                lightsource.setRad = 400f;
                lightsource.setPos = oracle.firstChunk.pos;
            }
        }

        #endregion

        #region ArmJoinGraphics修改

        /// <summary>
        /// 用于在ApplyPalette阶段修改 ArmJointGraphics 的 metalColor
        /// </summary>
        /// <param name="armJointGraphics"> ArmJointGraphics 实例 </param>
        /// <param name="sLeaser"></param>
        /// <param name="rCam"></param>
        /// <param name="palette"></param>
        /// <returns></returns>
        public virtual Color ArmJoint_MetalColor(ArmJointGraphics armJointGraphics, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            return Color.Lerp(palette.blackColor, palette.texture.GetPixel(5, 5), 0.12f);
        }

        /// <summary>
        /// 用于替代 ArmJointGraphics 中的 BaseColor 方法
        /// </summary>
        /// <param name="armJointGraphics">ArmJointGraphics 实例</param>
        /// <param name="pos"> 在房间中的位置，但该位置信息不是每次都会用到 </param>
        /// <returns></returns>
        public virtual Color ArmJoint_BaseColor(ArmJointGraphics armJointGraphics, Vector2 pos)
        {
            return Color.Lerp(Custom.HSL2RGB(0.025f, Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.05f, 0.7f - 0.5f * owner.room.Darkness(armJointGraphics.myJoint.pos), Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.1f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.5f);
        }

        /// <summary>
        /// 用于替代 ArmJointGraphics 中的 HighLightColor 方法
        /// </summary>
        /// <param name="armJointGraphics">ArmJointGraphics 实例</param>
        /// <param name="pos"> 在房间中的位置，但该位置信息不是每次都会用到 </param>
        /// <returns></returns>
        public virtual Color ArmJoint_HighLightColor(ArmJointGraphics armJointGraphics, Vector2 pos)
        {
            return Color.Lerp(Custom.HSL2RGB(0.025f, Mathf.Lerp(0.5f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.15f, 0.85f - 0.65f * owner.room.Darkness(armJointGraphics.myJoint.pos), Mathf.Pow(1f, 0.45f))), new Color(0f, 0f, 0.15f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.4f);
        }
        #endregion

        #region UbilicalCord修改
        /// <summary>
        /// UbilicalCord 中的第一种电线颜色
        /// </summary>
        /// <param name="ubilicalCord"> UbilicalCord 实例 </param>
        /// <returns></returns>
        public virtual Color UbilicalCord_WireCol_1(UbilicalCord ubilicalCord)
        {
            return new Color(1f, 0f, 0f);
        }

        /// <summary>
        /// UbilicalCord 中的第二种电线颜色
        /// </summary>
        /// <param name="ubilicalCord"> UbilicalCord 实例 </param>
        /// <returns></returns>
        public virtual Color UbilicalCord_WireCol_2(UbilicalCord ubilicalCord)
        {
            return new Color(0f, 0f, 1f);
        }
        #endregion

        #region 自定义方法

        /// <summary>
        /// 针对 CustomOracleBehaviour 特定部分起效的更新方法，如果不需要修改 Update 方法的话，可以重写这个
        /// </summary>
        /// <param name="bodySide">代表更新迭代器哪一边的身体，0代表左边，1代表右边</param>
        /// <param name="oracleBehaviour"></param>
        public virtual void CustomOracleUpdateBehaviour(int bodySide, CustomOracleBehaviour oracleBehaviour)
        {
            hands[bodySide].vel += randomTalkVector * averageVoice * 0.8f;
            if (oracle.oracleBehavior.player != null && bodySide == 0 && oracleBehaviour.HandTowardsPlayer())
            {
                hands[0].vel += Custom.DirVec(hands[0].pos, oracle.oracleBehavior.player.mainBodyChunk.pos) * 3f;
            }
            knees[bodySide, 0] = (feet[bodySide].pos + oracle.bodyChunks[1].pos) / 2f + Custom.PerpendicularVector(Custom.DirVec(oracle.firstChunk.pos, oracle.bodyChunks[1].pos)) * 4f * ((bodySide == 0) ? -1f : 1f);
        }

        /// <summary>
        /// 针对 CustomOracleBehaviour 特定部分起效的绘制方法。如果你不需要修改 DrawSprites 方法，可以重写这个
        /// </summary>
        /// <param name="sLeaser"></param>
        /// <param name="rCam"></param>
        /// <param name="timeStacker"></param>
        /// <param name="camPos"></param>
        /// <param name="oracleBehaviour"></param>
        public virtual void CustomOracleDrawBehaviour(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, CustomOracleBehaviour oracleBehaviour)
        {
            if (killSprite > 0)
            {
                if (oracleBehaviour.killFac > 0)
                {
                    sLeaser.sprites[killSprite].isVisible = true;

                    if (oracleBehaviour.player != null)
                    {
                        sLeaser.sprites[killSprite].x = Mathf.Lerp(oracleBehaviour.player.mainBodyChunk.lastPos.x, oracleBehaviour.player.mainBodyChunk.pos.x, timeStacker) - camPos.x;
                        sLeaser.sprites[killSprite].y = Mathf.Lerp(oracleBehaviour.player.mainBodyChunk.lastPos.y, oracleBehaviour.player.mainBodyChunk.pos.y, timeStacker) - camPos.y;
                    }

                    float num = Mathf.Lerp(oracleBehaviour.lastKillFac, oracleBehaviour.killFac, timeStacker);
                    sLeaser.sprites[killSprite].scale = Mathf.Lerp(200f, 2f, Mathf.Pow(num, 0.5f));
                    sLeaser.sprites[killSprite].alpha = Mathf.Pow(num, 3f);
                }
                else
                    sLeaser.sprites[killSprite].isVisible = false;
            }
        }

        /// <summary>
        /// 决定袍子颜色的函数，默认是fp的袍子
        /// </summary>
        /// <param name="gown"></param>
        /// <param name="f">顶点位于袍子上的位置，越接近0越靠上</param>
        /// <returns></returns>
        public virtual Color Gown_Color(Gown gown, float f)
        {
            return Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.02f, Mathf.Pow(f, 2f)), Mathf.Lerp(1f, 0.8f, f), 0.5f);
        }
        #endregion
    }

    public class CustomOracleBehaviour : OracleBehavior
    {
        #region 默认字段和属性
        public Vector2 lastPos;
        public Vector2 nextPos;
        public Vector2 lastPosHandle;
        public Vector2 nextPosHandle;
        public Vector2 currentGetTo;

        public float pathProgression;

        public float investigateAngle;

        public float invstAngSpeed;

        public Vector2 baseIdeal;

        public float working;

        public float getToWorking;

        public float unconciousTick;

        public float killFac;
        public float lastKillFac;

        public bool floatyMovement;

        public int discoverCounter;

        public int throwOutCounter;

        public int playerOutOfRoomCounter;

        public CustomOrbitableOraclePearl investigateCustomMarble;

        public CustomSubBehaviour currSubBehavior;

        public List<CustomSubBehaviour> allSubBehaviors;

        public CustomOracleConversation conversation;

        public CustomMovementBehavior movementBehavior;

        public CustomAction action;
        public CustomAction afterGiveMarkAction;

        public float lastKillFacOverseer;

        public float killFacOverseer;

        public bool pearlPickupReaction = true;

        public bool lastPearlPickedUp = true;

        public bool restartConversationAfterCurrentDialoge;

        public bool playerEnteredWithMark;

        public int timeSinceSeenPlayer = -1;

        public override Vector2 BaseGetToPos => baseIdeal;
        public override Vector2 OracleGetToPos
        {
            get
            {
                Vector2 v = currentGetTo;
                if (floatyMovement && Custom.DistLess(this.oracle.firstChunk.pos, this.nextPos, 50f))
                {
                    v = nextPos;
                }
                return ClampVectorInRoom(v);
            }
        }

        public override Vector2 GetToDir
        {
            get
            {
                if (movementBehavior == CustomMovementBehavior.Idle)
                {
                    return Custom.DegToVec(this.investigateAngle);
                }
                if (this.movementBehavior == CustomMovementBehavior.Investigate)
                {
                    return -Custom.DegToVec(this.investigateAngle);
                }
                return new Vector2(0f, 1f);
            }
        }
        #endregion

        #region 自定义字段和属性
        /// <summary>
        /// 迭代器不在工作的时候，房间的色板
        /// </summary>
        public virtual int NotWorkingPalette => 25;

        /// <summary>
        /// 迭代器在工作时，房间的色板
        /// </summary>
        public virtual int GetWorkingPalette => 26;

        public virtual bool CouldCloseEyes => false;

        #endregion

        public override DialogBox dialogBox
        {
            get
            {
                if (oracle.room.game.cameras[0].hud.dialogBox == null)
                {
                    oracle.room.game.cameras[0].hud.InitDialogBox();
                    oracle.room.game.cameras[0].hud.dialogBox.defaultYPos = -10f;
                }
                return oracle.room.game.cameras[0].hud.dialogBox;
            }
        }

        public CustomOracleBehaviour(Oracle oracle) : base(oracle)
        {
            currentGetTo = oracle.firstChunk.pos;
            lastPos = oracle.firstChunk.pos;
            nextPos = oracle.firstChunk.pos;

            pathProgression = 1f;
            investigateAngle = Random.value * 360f;

            action = CustomAction.General_Idle;

            allSubBehaviors = new List<CustomSubBehaviour>();
            currSubBehavior = new NoSubBehavior(this);
            allSubBehaviors.Add(currSubBehavior);

            working = 1f;
            getToWorking = 1f;

            movementBehavior = CustomMovementBehavior.Idle;
            playerEnteredWithMark = oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark;
        }

        public override void Update(bool eu)
        {
            if (timeSinceSeenPlayer >= 0)
            {
                timeSinceSeenPlayer++;
            }
            base.Update(eu);

            if (conversation != null)
            {
                if (restartConversationAfterCurrentDialoge && conversation.paused && dialogBox.messages.Count == 0 && player.room == oracle.room)
                {
                    conversation.paused = false;
                    restartConversationAfterCurrentDialoge = false;
                    conversation.RestartCurrent();
                }
            }
            else
            {
                restartConversationAfterCurrentDialoge = false;
            }

            if (!oracle.Consious) return;
            if (oracle.slatedForDeletetion) return;

            unconciousTick = 0f;
            currSubBehavior.Update();

            if (conversation != null)
                conversation.Update();

            if (!currSubBehavior.CurrentlyCommunicating)
                pathProgression = UpdatePathProgression();

            currentGetTo = Custom.Bezier(lastPos, ClampVectorInRoom(lastPos + lastPosHandle), nextPos, ClampVectorInRoom(nextPos + nextPosHandle), pathProgression);
            floatyMovement = false;
            investigateAngle += invstAngSpeed;
            inActionCounter++;

            if (player != null && player.room == oracle.room)
            {
                PlayerStayInRoomUpdate();
                playerOutOfRoomCounter = 0;
            }
            else
            {
                PlayerOutOfRoomUpdate();
                killFac = 0f;
                playerOutOfRoomCounter++;
            }

            if (pathProgression >= 1f && consistentBasePosCounter > 100 && !oracle.arm.baseMoving)
                allStillCounter++;
            else
                allStillCounter = 0;

            lastKillFac = killFac;

            GeneralActionUpdate();

            Move();
            if (working != getToWorking)
            {
                working = Custom.LerpAndTick(working, getToWorking, 0.05f, 1f / 30f);
            }

            for (int i = 0; i < oracle.room.game.cameras.Length; i++)
            {
                if (oracle.room.game.cameras[i].room == oracle.room && !oracle.room.game.cameras[i].AboutToSwitchRoom && oracle.room.game.cameras[i].paletteBlend != working)
                {
                    oracle.room.game.cameras[i].ChangeBothPalettes(NotWorkingPalette, GetWorkingPalette, working);
                }
            }

            if (!currSubBehavior.Gravity)
            {
                oracle.room.gravity = Custom.LerpAndTick(oracle.room.gravity, 0f, 0.05f, 0.02f);
            }
            else if (!ModManager.MSC || oracle.room.world.name != "HR" || !oracle.room.game.IsStorySession || !oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.ripMoon || oracle.ID != Oracle.OracleID.SS)
            {
                oracle.room.gravity = 1f - working;
            }
        }

        #region 默认帮助方法
        public Vector2 ClampVectorInRoom(Vector2 v)
        {
            Vector2 vector = v;
            vector.x = Mathf.Clamp(vector.x, this.oracle.arm.cornerPositions[0].x + 10f, this.oracle.arm.cornerPositions[1].x - 10f);
            vector.y = Mathf.Clamp(vector.y, this.oracle.arm.cornerPositions[2].y + 10f, this.oracle.arm.cornerPositions[1].y - 10f);
            return vector;
        }

        public virtual float BasePosScore(Vector2 tryPos)
        {
            if (player == null)
            {
                return Vector2.Distance(tryPos, oracle.room.MiddleOfTile(24, 5));
            }
            if (movementBehavior == CustomMovementBehavior.ShowMedia)
            {
                return 0f - Vector2.Distance(player.DangerPos, tryPos);
            }
            return Mathf.Abs(Vector2.Distance(nextPos, tryPos) - 200f) + Custom.LerpMap(Vector2.Distance(player.DangerPos, tryPos), 40f, 300f, 800f, 0f);
        }

        public virtual float CommunicatePosScore(Vector2 tryPos)
        {
            if (oracle.room.GetTile(tryPos).Solid || player == null)
            {
                return float.MaxValue;
            }
            float num = Mathf.Abs(Vector2.Distance(tryPos, player.DangerPos) - ((movementBehavior == CustomMovementBehavior.Talk) ? 250f : 400f));
            num -= (float)Custom.IntClamp(oracle.room.aimap.getTerrainProximity(tryPos), 0, 8) * 10f;
            if (movementBehavior == CustomMovementBehavior.ShowMedia)
            {
                num += (float)(Custom.IntClamp(oracle.room.aimap.getTerrainProximity(tryPos), 8, 16) - 8) * 10f;
            }
            return num;
        }

        #endregion

        #region 自定义帮助方法
        /// <summary>
        /// 更新迭代器当前的路径百分比
        /// </summary>
        /// <returns></returns>
        public virtual float UpdatePathProgression()
        {
            return Mathf.Min(1f, pathProgression + 1f / Mathf.Lerp(40f + pathProgression * 80f, Vector2.Distance(lastPos, nextPos) / 5f, 0.5f));
        }
        #endregion

        #region 默认功能方法
        public virtual void NewAction(CustomAction newAction)
        {

        }

        public void InitateConversation(Conversation.ID convoId, CustomConversationBehaviour convBehav)
        {
            if (conversation != null)
            {
                conversation.Interrupt("...", 0);
                conversation.Destroy();
            }
            conversation = new CustomOracleConversation(this, convBehav, convoId, dialogBox);
        }

        /// <summary>
        /// 设定迭代器的新位置，房间坐标
        /// </summary>
        /// <param name="dst"></param>
        public virtual void SetNewDestination(Vector2 dst)
        {
            lastPos = currentGetTo;
            nextPos = dst;
            lastPosHandle = Custom.RNV() * Mathf.Lerp(0.3f, 0.65f, Random.value) * Vector2.Distance(lastPos, nextPos);
            nextPosHandle = -GetToDir * Mathf.Lerp(0.3f, 0.65f, Random.value) * Vector2.Distance(lastPos, nextPos);
            pathProgression = 0f;
        }

        /// <summary>
        /// 见到玩家时的反应，用于初始化 CustomAction
        /// </summary>
        public virtual void SeePlayer()
        {
            if (timeSinceSeenPlayer < 0)
                timeSinceSeenPlayer = 0;

            //TODO : coop 支持，但真的不想写所以先去他嘛的
        }

        /// <summary>
        /// 玩家进入房间时调用的方法，手动调用
        /// </summary>
        public virtual void SlugcatEnterRoomReaction()
        {
            getToWorking = 0f;
            oracle.room.PlaySound(SoundID.SS_AI_Exit_Work_Mode, 0f, 1f, 1f);
            if (oracle.graphicsModule != null)
            {
                (oracle.graphicsModule as CustomOracleGraphic).halo.ChangeAllRadi();
                (oracle.graphicsModule as CustomOracleGraphic).halo.connectionsFireChance = 1f;
            }
        }

        /// <summary>
        /// 根据不同的movementBehaviour来决定迭代器的行为模式
        /// </summary>
        public virtual void Move()
        {
            if (movementBehavior == CustomMovementBehavior.Idle)
            {
                invstAngSpeed = 1f;
                if (CustomOracleTx.oracleEx.TryGetValue(oracle, out var customOralceEX))
                {
                    if (investigateCustomMarble == null && customOralceEX.customMarbles.Count > 0)
                    {
                        investigateCustomMarble = customOralceEX.customMarbles[Random.Range(0, customOralceEX.customMarbles.Count)];
                    }
                    if (investigateCustomMarble != null && (investigateCustomMarble.orbitObj == oracle || Custom.DistLess(new Vector2(250f, 150f), investigateCustomMarble.firstChunk.pos, 100f)))
                    {
                        investigateCustomMarble = null;
                    }
                    if (investigateCustomMarble != null)
                    {
                        lookPoint = investigateCustomMarble.firstChunk.pos;
                        if (Custom.DistLess(nextPos, investigateCustomMarble.firstChunk.pos, 100f))
                        {
                            floatyMovement = true;
                            nextPos = investigateCustomMarble.firstChunk.pos - Custom.DegToVec(investigateAngle) * 50f;
                        }
                        else
                        {
                            SetNewDestination(investigateCustomMarble.firstChunk.pos - Custom.DegToVec(investigateAngle) * 50f);
                        }
                        if (pathProgression == 1f && Random.value < 0.005f)
                        {
                            investigateCustomMarble = null;
                        }
                    }
                }
            }
            else if (movementBehavior == CustomMovementBehavior.KeepDistance)
            {
                if (player == null)
                {
                    movementBehavior = CustomMovementBehavior.Idle;
                }
                else
                {
                    lookPoint = player.DangerPos;
                    Vector2 vector = new Vector2(Random.value * oracle.room.PixelWidth, Random.value * oracle.room.PixelHeight);
                    if (!oracle.room.GetTile(vector).Solid && oracle.room.aimap.getTerrainProximity(vector) > 2 && Vector2.Distance(vector, player.DangerPos) > Vector2.Distance(nextPos, player.DangerPos) + 100f)
                    {
                        SetNewDestination(vector);
                    }
                }
            }
            else if (movementBehavior == CustomMovementBehavior.Investigate)
            {
                if (player == null)
                {
                    movementBehavior = CustomMovementBehavior.Idle;
                }
                else
                {
                    lookPoint = player.DangerPos;
                    if (investigateAngle < -90f || investigateAngle > 90f || oracle.room.aimap.getTerrainProximity(nextPos) < 2f)
                    {
                        investigateAngle = Mathf.Lerp(-70f, 70f, Random.value);
                        invstAngSpeed = Mathf.Lerp(0.4f, 0.8f, Random.value) * ((Random.value < 0.5f) ? (-1f) : 1f);
                    }
                    Vector2 vector = player.DangerPos + Custom.DegToVec(investigateAngle) * 150f;
                    if (oracle.room.aimap.getTerrainProximity(vector) >= 2f)
                    {
                        if (pathProgression > 0.9f)
                        {
                            if (Custom.DistLess(oracle.firstChunk.pos, vector, 30f))
                            {
                                floatyMovement = true;
                            }
                            else if (!Custom.DistLess(nextPos, vector, 30f))
                            {
                                SetNewDestination(vector);
                            }
                        }
                        nextPos = vector;
                    }
                }
            }
            else if (movementBehavior == CustomMovementBehavior.Talk)
            {
                if (player == null)
                {
                    movementBehavior = CustomMovementBehavior.Idle;
                }
                else
                {
                    lookPoint = player.DangerPos;
                    Vector2 vector = new Vector2(Random.value * oracle.room.PixelWidth, Random.value * oracle.room.PixelHeight);
                    if (CommunicatePosScore(vector) + 40f < CommunicatePosScore(nextPos) && !Custom.DistLess(vector, nextPos, 30f))
                    {
                        SetNewDestination(vector);
                    }
                }
            }
            else if (movementBehavior == CustomMovementBehavior.ShowMedia)
            {
                //pass
            }
            if (currSubBehavior != null && currSubBehavior.LookPoint.HasValue)
            {
                lookPoint = currSubBehavior.LookPoint.Value;
            }
            consistentBasePosCounter++;
            if (oracle.room.readyForAI)
            {
                Vector2 vector = new Vector2(Random.value * oracle.room.PixelWidth, Random.value * oracle.room.PixelHeight);
                if (!oracle.room.GetTile(vector).Solid && BasePosScore(vector) + 40f < BasePosScore(baseIdeal))
                {
                    baseIdeal = vector;
                    consistentBasePosCounter = 0;
                }
            }
            else
            {
                baseIdeal = nextPos;
            }
        }

        /// <summary>
        /// 决定迭代器是否要将手伸向玩家
        /// </summary>
        /// <returns></returns>
        public virtual bool HandTowardsPlayer()
        {
            return action == CustomAction.General_GiveMark;
        }

        /// <summary>
        /// 当迭代器被武器击中时的反应
        /// </summary>
        public virtual void ReactToHitWeapon()
        {
        }
        #endregion

        #region 自定义功能方法
        public virtual void PlayerStayInRoomUpdate()
        {

        }

        public virtual void PlayerOutOfRoomUpdate()
        {

        }

        public virtual void GeneralActionUpdate()
        {
            if (action == CustomAction.General_Idle)
            {
                if (movementBehavior != CustomMovementBehavior.Idle)
                {
                    movementBehavior = CustomMovementBehavior.Idle;
                }
                throwOutCounter = 0;
                if (player != null && player.room == oracle.room)
                {
                    discoverCounter++;
                    if (oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 && (discoverCounter > 220 || Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) || !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(1).StartTile), 150f)))
                    {
                        SeePlayer();
                    }
                }
            }
            else if (action == CustomAction.General_GiveMark)
            {
                movementBehavior = CustomMovementBehavior.KeepDistance;
                if (inActionCounter > 30 && inActionCounter < 300)
                {
                    player.Stun(20);
                    player.mainBodyChunk.vel += Vector2.ClampMagnitude(oracle.room.MiddleOfTile(24, 14) - player.mainBodyChunk.pos, 40f) / 40f * 2.8f * Mathf.InverseLerp(30f, 160f, (float)inActionCounter);
                }
                if (inActionCounter == 30)
                {
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, 0f, 1f, 1f);
                }
                if (inActionCounter == 300)
                {
                    player.mainBodyChunk.vel += Custom.RNV() * 10f;
                    player.bodyChunks[1].vel += Custom.RNV() * 10f;
                    player.Stun(40);
                    (oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark = true;
                    (oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 9;
                    (oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = (oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap;
                    for (int l = 0; l < oracle.room.game.cameras.Length; l++)
                    {
                        if (oracle.room.game.cameras[l].hud.karmaMeter != null)
                        {
                            oracle.room.game.cameras[l].hud.karmaMeter.UpdateGraphic();
                        }
                    }
                    for (int m = 0; m < 20; m++)
                    {
                        oracle.room.AddObject(new Spark(player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                    }
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f);
                }
                if (inActionCounter > 300 && player.graphicsModule != null)
                {
                    (player.graphicsModule as PlayerGraphics).markAlpha = Mathf.Max((player.graphicsModule as PlayerGraphics).markAlpha, Mathf.InverseLerp(500f, 300f, (float)inActionCounter));
                }
                if (inActionCounter >= 500)
                {
                    NewAction(afterGiveMarkAction);
                    if (conversation != null)
                    {
                        conversation.paused = false;
                    }
                }
            }
        }

        public virtual void AddConversationEvents(CustomOracleConversation conv, Conversation.ID id)
        {

        }
        #endregion

        public void UnlockShortcuts()
        {
            oracle.room.lockedShortcuts.Clear();
        }

        public class NoSubBehavior : CustomSubBehaviour
        {
            public NoSubBehavior(CustomOracleBehaviour owner) : base(owner, CustomSubBehaviour.CustomSubBehaviourID.General)
            {
            }
        }

        #region 基类
        /// <summary>
        /// 对话类的抽象类，与 SSOracleBehaviour.ConversationBehavior 对应
        /// </summary>
        public abstract class CustomConversationBehaviour : CustomTalkBehaviour
        {
            public CustomConversationBehaviour(CustomOracleBehaviour owner, CustomSubBehaviour.CustomSubBehaviourID ID, Conversation.ID convoID) : base(owner, ID)
            {
                this.convoID = convoID;
            }
            public Conversation.ID convoID;
        }

        /// <summary>
        /// 子行为的基类，与SSOracleBehaviour.SubBehavior对应
        /// </summary>
        public abstract class CustomSubBehaviour
        {
            public CustomSubBehaviourID ID;

            public CustomOracleBehaviour owner;

            public int inActionCounter => owner.inActionCounter;
            public virtual bool CurrentlyCommunicating => false;
            public virtual bool Gravity => true;
            public virtual float LowGravity => -1f;

            public virtual Vector2? LookPoint => null;

            public CustomAction action => owner.action;
            public Oracle oracle => owner.oracle;
            public Player player => owner.player;
            public CustomMovementBehavior movementBehavior
            {
                get
                {
                    return owner.movementBehavior;
                }
                set
                {
                    owner.movementBehavior = value;
                }
            }

            public CustomSubBehaviour(CustomOracleBehaviour owner, CustomSubBehaviourID ID)
            {
                this.owner = owner;
                this.ID = ID;
            }

            public virtual void Update()
            {
            }

            public virtual void NewAction(CustomAction oldAction, CustomAction newAction)
            {
            }

            public virtual void Activate(CustomAction oldAction, CustomAction newAction)
            {
                NewAction(oldAction, newAction);
            }

            public virtual void Deactivate()
            {
                owner.UnlockShortcuts();
            }

            /// <summary>
            /// 子行为ID，结构与其他的ExtEnum类似
            /// </summary>
            public class CustomSubBehaviourID : ExtEnum<CustomSubBehaviourID>
            {
                public CustomSubBehaviourID(string value, bool register = false) : base(value, register)
                {
                }

                public static readonly CustomSubBehaviourID General = new CustomSubBehaviourID("General", true);
            }

        }

        /// <summary>
        /// 具有对话和行为的行为基类，与 SSOracleBehaviour.TalkBehavior 对应
        /// </summary>
        public abstract class CustomTalkBehaviour : CustomSubBehaviour
        {
            public DialogBox dialogBox
            {
                get
                {
                    return this.owner.dialogBox;
                }
            }

            public string Translate(string s)
            {
                return this.owner.Translate(s);
            }

            public override bool CurrentlyCommunicating
            {
                get
                {
                    return this.dialogBox.ShowingAMessage;
                }
            }

            public CustomTalkBehaviour(CustomOracleBehaviour owner, CustomSubBehaviourID ID) : base(owner, ID)
            {
            }

            public override void NewAction(CustomAction oldAction, CustomAction newAction)
            {
                base.NewAction(oldAction, newAction);
                this.communicationIndex = 0;
            }

            public int communicationIndex;

            public int communicationPause;
        }

        /// <summary>
        /// 与 PebblesConversavtion 对应
        /// </summary>
        public class CustomOracleConversation : Conversation
        {
            public CustomOracleBehaviour owner;

            public CustomConversationBehaviour convBehav;

            public bool waitForStill;

            public int age;

            public CustomOracleConversation(CustomOracleBehaviour owner, CustomConversationBehaviour convBehav, ID id, DialogBox dialogBox) : base(owner, id, dialogBox)
            {
                this.owner = owner;
                this.convBehav = convBehav;
                AddEvents();
            }

            public override void Update()
            {
                age++;
                if (waitForStill)
                {
                    if (!convBehav.CurrentlyCommunicating && convBehav.communicationPause > 0)
                    {
                        convBehav.communicationPause--;
                    }
                    if (!convBehav.CurrentlyCommunicating && convBehav.communicationPause < 1 && owner.allStillCounter > 20)
                    {
                        waitForStill = false;
                        return;
                    }
                }
                else
                {
                    base.Update();
                }
            }

            public string Translate(string s)
            {
                return owner.Translate(s);
            }

            public override void AddEvents()
            {
                owner.AddConversationEvents(this, id);
            }

            public delegate void AddEventDelegate(ID id, CustomOracleBehaviour owner);

            public class PauseAndWaitForStillEvent : DialogueEvent
            {
                public PauseAndWaitForStillEvent(Conversation owner, CustomConversationBehaviour _convBehav, int pauseFrames) : base(owner, 0)
                {
                    convBehav = _convBehav;
                    if (convBehav == null && owner is CustomOracleConversation)
                    {
                        convBehav = (owner as CustomOracleConversation).convBehav;
                    }
                    this.pauseFrames = pauseFrames;
                }

                public override void Activate()
                {
                    base.Activate();
                    convBehav.communicationPause = pauseFrames;
                    (owner as CustomOracleConversation).waitForStill = true;
                }

                public CustomConversationBehaviour convBehav;

                public int pauseFrames;
            }
        }
        #endregion

        #region CustomEnums
        public class CustomAction : ExtEnum<CustomAction>
        {
            public CustomAction(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly CustomAction General_Idle = new CustomAction("General_Idle", true);
            public static readonly CustomAction General_GiveMark = new CustomAction("General_GiveMark", true);
        }

        public class CustomMovementBehavior : ExtEnum<CustomMovementBehavior>
        {
            public CustomMovementBehavior(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly CustomMovementBehavior Idle = new CustomMovementBehavior("Idle", true);

            public static readonly CustomMovementBehavior KeepDistance = new CustomMovementBehavior("KeepDistance", true);

            public static readonly CustomMovementBehavior Investigate = new CustomMovementBehavior("Investigate", true);

            public static readonly CustomMovementBehavior Talk = new CustomMovementBehavior("Talk", true);

            public static readonly CustomMovementBehavior ShowMedia = new CustomMovementBehavior("ShowMedia", true);
        }
        #endregion
    }
}

/// <summary>
/// 可视化调试类
/// </summary>
public class CustomOracleStateViz
{
    Oracle oracle;

    public FLabel label;
    public CustomOracleStateViz(Oracle oracle)
    {
        this.oracle = oracle;
        InitSprites();
    }

    public virtual void InitSprites()
    {
        label = new FLabel(Custom.GetFont(), "") { anchorX = 0f, anchorY = 1f, scale = 1.1f, isVisible = true, alpha = 1f };
        Futile.stage.AddChild(label);
    }

    public virtual void Update()
    {
        label.SetPosition(new Vector2(400f, 300f));
        string text = string.Format("Oracle : {0}\n", oracle.ID);

        for (int i = 0; i < oracle.bodyChunks.Length; i++)
        {
            text += string.Format("Bodychunk{0} pos : {1}\n", i, oracle.bodyChunks[i].pos);
        }


        CustomOracleGraphic customOracleGraphic = oracle.graphicsModule as CustomOracleGraphic;
        CustomOracleBehaviour behaviour = (oracle.oracleBehavior as CustomOracleBehaviour);
        if (customOracleGraphic != null)
        {
            text += string.Format("getToPos {0}\n", behaviour.OracleGetToPos);
            text += string.Format("idealPos {0}\n", behaviour.baseIdeal);
            text += string.Format("progression {0:f2}\n", behaviour.pathProgression);
            text += string.Format("Action : {0} inActionCounter {1}\n", behaviour.action, behaviour.inActionCounter);
            text += string.Format("Conversation : {0}", behaviour.conversation?.id);
        }
        label.text = text;
    }

    public virtual void ClearSprites()
    {
        label.RemoveFromContainer();
    }
}
