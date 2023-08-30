using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevInterface;

namespace EmgTx.CustomObjectDevExt
{
    public abstract class CustomDevObjectTx
    {
        public readonly PlacedObject.Type placedType;
        public readonly AbstractPhysicalObject.AbstractObjectType objectType;

        /// <summary>
        /// 在开发者工具物品页面中的分类
        /// </summary>
        public virtual ObjectsPage.DevObjectCategories Categorie => ObjectsPage.DevObjectCategories.Unsorted;
        public CustomDevObjectTx(PlacedObject.Type placedType,AbstractPhysicalObject.AbstractObjectType objectType)
        {
            this.placedType = placedType;
            this.objectType = objectType;
        }

        /// <summary>
        /// 当物品新建数据时的替代方法
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public virtual PlacedObject.Data GenerateEmptyDate(PlacedObject self)
        {
            return null;
        }

        /// <summary>
        /// 当房间首次加载时，根据PlacedObject来创建物品实例的方法
        /// </summary>
        /// <param name="room"></param>
        /// <param name="placedObject"></param>
        /// <returns></returns>
        public virtual AbstractPhysicalObject CreateAbstractPhysicalObjectOnRoomLoad(Room room, PlacedObject placedObject, int placedObjectIndex)
        {
            return null;
        }


        /// <summary>
        /// 当开发者工具创建该物品的开发界面时，为该物品创建代表物品
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tp"></param>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public virtual PlacedObjectRepresentation CreateObjectRep(ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            return null;
        }
    }

    public class CustomConsumableDevObjectTx : CustomDevObjectTx
    {
        public virtual int MinRegen => 1;
        public virtual int MaxRegen => 5;

        public CustomConsumableDevObjectTx(PlacedObject.Type placedType, AbstractPhysicalObject.AbstractObjectType objectType) : base(placedType, objectType)
        {
        }

        public override ObjectsPage.DevObjectCategories Categorie => ObjectsPage.DevObjectCategories.Consumable;

        public override AbstractPhysicalObject CreateAbstractPhysicalObjectOnRoomLoad(Room room, PlacedObject placedObject, int placedObjectIndex)
        {
            if (!(room.game.session is StoryGameSession) || !(room.game.session as StoryGameSession).saveState.ItemConsumed(room.world, false, room.abstractRoom.index, placedObjectIndex))
            {
                AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(room.world, objectType, null, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), room.abstractRoom.index, placedObjectIndex, placedObject.data as PlacedObject.ConsumableObjectData);
                (abstractPhysicalObject as AbstractConsumable).isConsumed = false;
                return abstractPhysicalObject;
            }
            return null;
        }

        public override PlacedObject.Data GenerateEmptyDate(PlacedObject self)
        {
            var result = new PlacedObject.ConsumableObjectData(self);
            result.minRegen = MinRegen;
            result.maxRegen = MaxRegen;

            return result;
        }

        public override PlacedObjectRepresentation CreateObjectRep(ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            return new ConsumableRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());
        }

    }
}
