using DevInterface;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EmgTx.CustomObjectDevExt
{
    internal static class CustomDevObjectHoox
    {
        static bool hookOn = false;

        public static void HookOn()
        {
            if (hookOn)
                return;

            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;

            On.Room.Loaded += Room_Loaded;

            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;

            hookOn = true;
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            if (CustomDevObjectRx.customDevObjectTxs.TryGetValue(self.type, out var tx))
                self.data = tx.GenerateEmptyDate(self);
            else
                orig.Invoke(self);
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            bool orig_firstTimeRealized = self.abstractRoom.firstTimeRealized;
            orig.Invoke(self);
            if (orig_firstTimeRealized)
            {
                for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
                {
                    if (self.roomSettings.placedObjects[i].active)
                    {
                        if(CustomDevObjectRx.customDevObjectTxs.TryGetValue(self.roomSettings.placedObjects[i].type, out var tx))
                        {
                            AbstractPhysicalObject abstractPhysicalObject = tx.CreateAbstractPhysicalObjectOnRoomLoad(self, self.roomSettings.placedObjects[i], i);

                            if(abstractPhysicalObject != null)
                                self.abstractRoom.AddEntity(abstractPhysicalObject);
                        }
                    }
                }
            }
        }

        private static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
        {
            if (CustomDevObjectRx.customDevObjectTxs.TryGetValue(type, out var tx))
                return tx.Categorie;
            else
                return orig.Invoke(self, type);
        }

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, DevInterface.ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            if (CustomDevObjectRx.customDevObjectTxs.TryGetValue(tp, out var tx))
            {
                if (pObj == null)
                {
                    pObj = new PlacedObject(tp, null);
                    pObj.pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f;
                    self.RoomSettings.placedObjects.Add(pObj);
                }

                PlacedObjectRepresentation rep = tx.CreateObjectRep(self, tp, pObj);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }
            else
                orig.Invoke(self, tp, pObj);
        }
    }
}
