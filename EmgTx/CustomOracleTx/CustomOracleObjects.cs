using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomOracleTx
{
    /// <summary>
    /// 注册类
    /// </summary>
    public class CustomOraclePearlTx
    {
        public readonly AbstractPhysicalObject.AbstractObjectType pearlObjectType;
        public readonly DataPearl.AbstractDataPearl.DataPearlType dataPearlType;
        public readonly Conversation.ID pearlConvID;

        /// <summary>
        /// 是否为特殊珍珠
        /// </summary>
        public virtual bool IsSignificantPearl => false;

        /// <summary>
        /// 简化注册方法，不用手动赋值EnumExt
        /// </summary>
        /// <param name="name"></param>
        public CustomOraclePearlTx(string name) : this(new AbstractPhysicalObject.AbstractObjectType(name, true), new DataPearl.AbstractDataPearl.DataPearlType(name, true), new Conversation.ID(name, true))
        {
        }

        public CustomOraclePearlTx(AbstractPhysicalObject.AbstractObjectType pearlObjectType, DataPearl.AbstractDataPearl.DataPearlType dataPearlType, Conversation.ID pearlConvID)
        {
            this.pearlObjectType = pearlObjectType;
            this.dataPearlType = dataPearlType;
            this.pearlConvID = pearlConvID;
        }

        /// <summary>
        /// 创建 CustomPearl 的实例，需要手动实现
        /// </summary>
        /// <param name="abstractPhysicalObject"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public virtual CustomOrbitableOraclePearl RealizeDataPearl(AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            return null;
        }

        /// <summary>
        /// 获取AbstractCustomOraclePearl的实例
        /// 如果你没有继承新的 AbstractCustomOraclePearl，或者没有保存新的数据，那么可以不重写该方法
        /// </summary>
        /// <param name="world"></param>
        /// <param name="realizedObject"></param>
        /// <param name="pos"></param>
        /// <param name="ID"></param>
        /// <param name="originRoom"></param>
        /// <param name="placedObjectIndex"></param>
        /// <param name="consumableData"></param>
        /// <param name="color"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public virtual CustomOrbitableOraclePearl.AbstractCustomOraclePearl GetAbstractCustomOraclePearl(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData, int color, int number)
        {
            return new CustomOrbitableOraclePearl.AbstractCustomOraclePearl(pearlObjectType, dataPearlType, world, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData, color, number);
        }

        /// <summary>
        /// 从存档字符串中创建一个 AbstractCustomOraclePearl 实例
        /// 如果你没有继承新的 AbstractCustomOraclePearl，或者没有保存新的数据，那么可以不重写该方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="world"></param>
        /// <param name="id"></param>
        /// <param name="pos"></param>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        public virtual CustomOrbitableOraclePearl.AbstractCustomOraclePearl AbstractPhysicalObjectFromString(AbstractPhysicalObject.AbstractObjectType type, World world, EntityID id, WorldCoordinate pos, string[] dataArray)
        {
            if (type != pearlObjectType)
                return null;
            return new CustomOrbitableOraclePearl.AbstractCustomOraclePearl(pearlObjectType, dataPearlType, world, null, pos, id, int.Parse(dataArray[3]), int.Parse(dataArray[4]), null, int.Parse(dataArray[6]), int.Parse(dataArray[7]));
        }

        /// <summary>
        /// TODO : 以后再来写，哈哈
        /// Moon 阅读珍珠时触发的对话, 你可以重写该方法来实现自定义的对话，默认方法包含了珍珠对话的开头
        /// </summary>
        /// <param name="self"></param>
        /// <param name="saveFile"></param>
        /// <param name="oneRandomLine"></param>
        /// <param name="randomSeed"></param>
        public virtual void LoadSLPearlConversation(SLOracleBehaviorHasMark.MoonConversation self, SlugcatStats.Name saveFile, bool oneRandomLine, int randomSeed)
        {
            switch (Random.Range(0, 5))
            {
                case 0:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You would like me to read this?"), 10));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("It's still warm... this was in use recently."), 10));
                    break;
                case 1:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("A pearl... This one is crystal clear - it was used just recently."), 10));
                    break;
                case 2:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Would you like me to read this pearl?"), 10));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Strange... it seems to have been used not too long ago."), 10));
                    break;
                case 3:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("This pearl has been written to just now!"), 10));
                    break;
                default:
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Let's see... A pearl..."), 10));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("And this one is fresh! It was not long ago this data was written to it!"), 10));
                    break;
            }
        }
    }

    public class CustomOrbitableOraclePearl : DataPearl
    {
        public Vector2? hoverPos;

        public float orbitAngle;
        public float orbitSpeed;
        public float orbitDistance;
        public float orbitFlattenAngle;
        public float orbitFlattenFac;

        public int orbitCircle;
        public int marbleColor;
        public int marbleIndex;

        public bool lookForMarbles;

        public GlyphLabel label;
        public Oracle oracle;
        public PhysicalObject orbitObj;
        public List<CustomOrbitableOraclePearl> otherMarbles;
        public bool NotCarried => grabbedBy.Count == 0;

        public CustomOrbitableOraclePearl(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            otherMarbles = new List<CustomOrbitableOraclePearl>();
            orbitAngle = Random.value * 360f;
            orbitSpeed = 3f;
            orbitDistance = 50f;
            collisionLayer = 0;
            orbitFlattenAngle = Random.value * 360f;
            orbitFlattenFac = 0.5f + Random.value * 0.5f;
        }

        public override void NewRoom(Room newRoom)
        {
            base.NewRoom(newRoom);
            UpdateOtherMarbles();
        }

        /// <summary>
        /// Marble的方法，用于模拟轨道行为
        /// </summary>
        public virtual void UpdateOtherMarbles()
        {
            for (int i = 0; i < otherMarbles.Count; i++)
            {
                otherMarbles[i].otherMarbles.Remove(this);
            }
            otherMarbles.Clear();
            for (int j = 0; j < room.physicalObjects[collisionLayer].Count; j++)
            {
                if (room.physicalObjects[collisionLayer][j] is CustomOrbitableOraclePearl && room.physicalObjects[collisionLayer][j] != this)
                {
                    if (!(room.physicalObjects[this.collisionLayer][j] as CustomOrbitableOraclePearl).otherMarbles.Contains(this))
                    {
                        (room.physicalObjects[collisionLayer][j] as CustomOrbitableOraclePearl).otherMarbles.Add(this);
                    }
                    if (!otherMarbles.Contains(room.physicalObjects[collisionLayer][j] as CustomOrbitableOraclePearl))
                    {
                        otherMarbles.Add(room.physicalObjects[collisionLayer][j] as CustomOrbitableOraclePearl);
                    }
                }
            }
        }

        /// <summary>
        /// Update 方法，一般不需要重写
        /// </summary>
        /// <param name="eu"></param>
        public override void Update(bool eu)
        {
            if (!lookForMarbles)
            {
                UpdateOtherMarbles();
                lookForMarbles = true;
            }
            if (oracle != null && oracle.room != room)
            {
                oracle = null;
            }
            abstractPhysicalObject.destroyOnAbstraction = (oracle != null);
            if (label != null)
            {
                label.setPos = new Vector2?(firstChunk.pos);
                if (label.room != room)
                {
                    label.Destroy();
                }
            }
            else
            {
                label = new GlyphLabel(firstChunk.pos, GlyphLabel.RandomString(1, 1, 12842 + (abstractPhysicalObject as AbstractCustomOraclePearl).number, false));
                room.AddObject(label);
            }
            base.Update(eu);
            float num = orbitAngle;
            float num2 = orbitSpeed;
            float num3 = orbitDistance;
            float axis = orbitFlattenAngle;
            float num4 = orbitFlattenFac;
            if (room.gravity < 1f && NotCarried && oracle != null)
            {
                if (ModManager.MSC && oracle != null && oracle.marbleOrbiting)
                {
                    int listCount = 1;
                    if (CustomOracleTx.oracleEx.TryGetValue(oracle, out var customOralceEX))
                    {
                        listCount = customOralceEX.customMarbles.Count;
                    }
                    float num5 = (float)marbleIndex / (float)listCount;
                    num = 360f * num5 + (float)oracle.behaviorTime * 0.1f;
                    Vector2 vector = new Vector2(oracle.room.PixelWidth / 2f, oracle.room.PixelHeight / 2f) + Custom.DegToVec(num) * 275f;
                    firstChunk.vel *= Custom.LerpMap(firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    firstChunk.vel += Vector2.ClampMagnitude(vector - firstChunk.pos, 100f) / 100f * 0.4f * (1f - room.gravity);
                }
                else
                {
                    Vector2 vector2 = base.firstChunk.pos;
                    if (orbitObj != null)
                    {
                        int num6 = 0;
                        int num7 = 0;
                        int number = abstractPhysicalObject.ID.number;
                        for (int i = 0; i < otherMarbles.Count; i++)
                        {
                            if (otherMarbles[i].orbitObj == orbitObj && otherMarbles[i].NotCarried && Custom.DistLess(otherMarbles[i].firstChunk.pos, orbitObj.firstChunk.pos, otherMarbles[i].orbitDistance * 4f) && otherMarbles[i].orbitCircle == orbitCircle)
                            {
                                num3 += otherMarbles[i].orbitDistance;
                                if (otherMarbles[i].abstractPhysicalObject.ID.number < this.abstractPhysicalObject.ID.number)
                                {
                                    num7++;
                                }
                                num6++;
                                if (otherMarbles[i].abstractPhysicalObject.ID.number < number)
                                {
                                    number = otherMarbles[i].abstractPhysicalObject.ID.number;
                                    num = otherMarbles[i].orbitAngle;
                                    num2 = otherMarbles[i].orbitSpeed;
                                    axis = otherMarbles[i].orbitFlattenAngle;
                                    num4 = otherMarbles[i].orbitFlattenFac;
                                }
                            }
                        }
                        num3 /= (1 + num6);
                        num += (num7 * (360f / (num6 + 1)));
                        Vector2 vector3 = orbitObj.firstChunk.pos;
                        if (orbitObj is Oracle && orbitObj.graphicsModule != null)
                        {
                            vector3 = (orbitObj.graphicsModule as OracleGraphics).halo.Center(1f);
                        }
                        vector2 = vector3 + Custom.FlattenVectorAlongAxis(Custom.DegToVec(num), axis, num4) * num3 * Mathf.Lerp(1f / num4, 1f, 0.5f);
                    }
                    else if (hoverPos != null)
                    {
                        vector2 = hoverPos.Value;
                    }
                    firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 1f, 6f, 0.999f, 0.9f);
                    firstChunk.vel += Vector2.ClampMagnitude(vector2 - firstChunk.pos, 100f) / 100f * 0.4f * (1f - this.room.gravity);
                }
            }
            orbitAngle += num2 * ((orbitCircle % 2 == 0) ? 1f : -1f);
        }

        /// <summary>
        /// 给你的珍珠赋上颜色
        /// </summary>
        /// <param name="sLeaser"></param>
        /// <param name="rCam"></param>
        /// <param name="palette"></param>
        public virtual void DataPearlApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }


        public class AbstractCustomOraclePearl : AbstractDataPearl
        {
            public int color;
            public int number;
            public AbstractCustomOraclePearl(
                AbstractObjectType objectType,
                DataPearlType dataPearlType,

                World world,
                PhysicalObject realizedObject,
                WorldCoordinate pos,
                EntityID ID,
                int originRoom,
                int placedObjectIndex,
                PlacedObject.ConsumableObjectData consumableData,
                int color,
                int number) : base(world, objectType, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData, dataPearlType)
            {
                this.color = color;
                this.number = number;
            }

            public override string ToString()
            {
                string baseString = base.ToString();
                baseString += string.Format(CultureInfo.InvariantCulture, "<oA>{0}<oA>{1}", color, number);
                baseString = SaveState.SetCustomData(this, baseString);

                return baseString;
            }
        }
    }

    public class CustomOraclePearlRx
    {
        public static List<CustomOraclePearlTx> treatments = new List<CustomOraclePearlTx>();
        public static Dictionary<AbstractPhysicalObject.AbstractObjectType, CustomOraclePearlTx> typeToTreatment = new Dictionary<AbstractPhysicalObject.AbstractObjectType, CustomOraclePearlTx>();
        //public static Dictionary<Conversation.ID, CustomOraclePearlTx> idToTreatment = new Dictionary<Conversation.ID, CustomOraclePearlTx>();
        public static Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomOraclePearlTx> pearlTypeToTreatment = new Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomOraclePearlTx>();

        public static void ApplyTreatment(CustomOraclePearlTx pearlTreatment)
        {
            try
            {
                if (treatments.Contains(pearlTreatment)) return;
                treatments.Add(pearlTreatment);
                typeToTreatment.Add(pearlTreatment.pearlObjectType, pearlTreatment);
                pearlTypeToTreatment.Add(pearlTreatment.dataPearlType, pearlTreatment);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                EmgTxCustom.Log($"Exception when apply treatment for : {pearlTreatment.pearlObjectType}-{pearlTreatment.pearlConvID}-{pearlTreatment.dataPearlType}");
            }

            CustomOraclePearlHoox.HookOn();
        }
    }

    public class CustomOraclePearlHoox
    {
        static bool inited = false;

        #region Hooks
        public static void HookOn()
        {
            if (inited) return;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;
            On.DataPearl.ApplyPalette += DataPearl_ApplyPalette;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;

            On.SLOracleBehaviorHasMark.GrabObject += SLOracleBehaviorHasMark_GrabObject;
            On.Conversation.DataPearlToConversation += Conversation_DataPearlToConversation;

            inited = true;
        }

        private static Conversation.ID Conversation_DataPearlToConversation(On.Conversation.orig_DataPearlToConversation orig, DataPearl.AbstractDataPearl.DataPearlType type)
        {
            var result = orig.Invoke(type);
            if (CustomOraclePearlRx.pearlTypeToTreatment.TryGetValue(type, out var treatment))
            {
                result = treatment.pearlConvID;
            }
            return result;
        }

        private static void SLOracleBehaviorHasMark_GrabObject(On.SLOracleBehaviorHasMark.orig_GrabObject orig, SLOracleBehaviorHasMark self, PhysicalObject item)
        {
            if (self.throwAwayObjects)
            {
                return;
            }
            if (item is DataPearl dataPearl && CustomOraclePearlRx.typeToTreatment.TryGetValue(dataPearl.AbstractPearl.type, out var registry) && !registry.IsSignificantPearl)
            {
                #region BaseFunction
                bool flag = true;
                int num = 0;
                while (flag && num < self.pickedUpItemsThisRealization.Count)
                {
                    if (item.abstractPhysicalObject.ID == self.pickedUpItemsThisRealization[num])
                    {
                        flag = false;
                    }
                    num++;
                }
                if (flag)
                {
                    self.pickedUpItemsThisRealization.Add(item.abstractPhysicalObject.ID);
                }
                if (item.graphicsModule != null)
                {
                    item.graphicsModule.BringSpritesToFront();
                }
                if (item is IDrawable)
                {
                    for (int i = 0; i < self.oracle.abstractPhysicalObject.world.game.cameras.Length; i++)
                    {
                        self.oracle.abstractPhysicalObject.world.game.cameras[i].MoveObjectToContainer(item as IDrawable, null);
                    }
                }
                self.holdingObject = item;
                #endregion

                self.isRepeatedDiscussion = false;
                if (self.State.HaveIAlreadyDescribedThisItem(item.abstractPhysicalObject.ID))
                {
                    if (!ModManager.MMF)
                    {
                        self.AlreadyDiscussedItem(item is DataPearl);
                        return;
                    }
                    self.isRepeatedDiscussion = true;
                }

                self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(registry.pearlConvID, self, SLOracleBehaviorHasMark.MiscItemType.NA);

                if (!self.isRepeatedDiscussion)
                {
                    SLOrcacleState state2 = self.State;
                    num = state2.totalItemsBrought;
                    state2.totalItemsBrought = num + 1;
                    self.State.AddItemToAlreadyTalkedAbout(item.abstractPhysicalObject.ID);
                }
                self.talkedAboutThisSession.Add(item.abstractPhysicalObject.ID);
            }
            else
                orig.Invoke(self, item);
        }

        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            orig.Invoke(self);
            foreach(var treatment in CustomOraclePearlRx.treatments)
            {
                if(treatment.pearlConvID == self.id)
                {
                    treatment.LoadSLPearlConversation(self, self.currentSaveFile, true, (self.myBehavior is SLOracleBehaviorHasMark && (self.myBehavior as SLOracleBehaviorHasMark).holdingObject != null) ? (self.myBehavior as SLOracleBehaviorHasMark).holdingObject.abstractPhysicalObject.ID.RandomSeed : Random.Range(0, 100000));
                    EmgTxCustom.Log($"Custom oracle pearl conversation loaded : {treatment.pearlObjectType}-{treatment.pearlConvID}-{treatment.dataPearlType}");
                    return;
                }
            }   
        }

        private static void DataPearl_ApplyPalette(On.DataPearl.orig_ApplyPalette orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);
            if (self is CustomOrbitableOraclePearl)
            {
                (self as CustomOrbitableOraclePearl).DataPearlApplyPalette(sLeaser, rCam, palette);
            }
        }

        private static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
        {
            orig.Invoke(self);
            if (CustomOraclePearlRx.typeToTreatment.TryGetValue(self.type, out var treatment))
            {
                self.realizedObject = treatment.RealizeDataPearl(self, self.world);
            }
        }

        private static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            var result = orig.Invoke(world, objString);
            AbstractPhysicalObject abstractPhysicalObject;
            try
            {
                string[] array = Regex.Split(objString, "<oA>");
                EntityID id = EntityID.FromString(array[0]);
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = new AbstractPhysicalObject.AbstractObjectType(array[1], false);
                WorldCoordinate pos = WorldCoordinate.FromString(array[2]);

                foreach (var treatment in CustomOraclePearlRx.treatments)
                {
                    abstractPhysicalObject = treatment.AbstractPhysicalObjectFromString(abstractObjectType, world, id, pos, array);
                    if (abstractPhysicalObject != null)
                    {
                        result = abstractPhysicalObject;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                EmgTxCustom.Log(string.Concat(new string[]
                {
                    "[EXCEPTION] AbstractPhysicalObjectFromString: ",
                    objString,
                    " -- ",
                    ex.Message,
                    " -- ",
                    ex.StackTrace
                }));
                result = null;
            }

            return result;
        }
        #endregion

        public static AbstractPhysicalObject GetAbstractCustomPearlOfType(AbstractPhysicalObject.AbstractObjectType abstractObjectType, World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData, int color, int number)
        {
            if (CustomOraclePearlRx.typeToTreatment.TryGetValue(abstractObjectType, out var registry))
            {
                return registry.GetAbstractCustomOraclePearl(world, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData, color, number);
            }
            return null;
        }

        public static CustomOrbitableOraclePearl GetRealizedCustomPearlOfType(AbstractPhysicalObject.AbstractObjectType abstractObjectType, AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            if (CustomOraclePearlRx.typeToTreatment.TryGetValue(abstractObjectType, out var registry))
            {
                return registry.RealizeDataPearl(abstractPhysicalObject, world);
            }
            return null;
        }
    }
}
