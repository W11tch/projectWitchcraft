using ProjectWitchcraft.Core;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    public struct InventoryChangedEvent
    {
        public ItemData itemData;
        public int NewAmount;
    }

    public struct EquipmentChangedEvent
    {
        public EquipmentSlot Slot;
        public ItemInstance NewItem; // null when slot was cleared
    }

    public struct ItemDroppedInWorldEvent
    {
        public ItemInstance itemInstance;
        public int quantity;
        public Vector3 position;
    }
}
