using UnityEngine;

namespace ProjectWitchcraft.Core
{
    public class ItemData : GuidAsset
    {
        [Header("Item Information")]
        public string Name;
        public Sprite Icon;
        [Tooltip("The maximum number of this item that can be held in a single inventory slot.")]
        public int maxStackSize = 9999;
        [TextArea]
        public string description;

        // -1 means no durability. Override in durable item types (EquipmentItemData, WeaponItemData, ToolItemData).
        public virtual float GetMaxDurability() => -1f;
    }
}
