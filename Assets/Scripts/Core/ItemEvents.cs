using ProjectWitchcraft.Core;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    public struct InventoryChangedEvent
    {
        public ItemData itemData;
        public int NewAmount;
    }

    public struct ItemDroppedInWorldEvent
    {
        public ItemData itemData;
        public int quantity;
        public Vector3 position;
    }
}
