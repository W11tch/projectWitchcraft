using System;

namespace ProjectWitchcraft.Core
{
    [Serializable]
    public class ItemInstance
    {
        public ItemData Definition;
        // -1 means this item type has no durability (stackable resources, etc.)
        public float CurrentDurability = -1f;

        public bool HasDurability => CurrentDurability >= 0f;
        public bool IsEmpty => Definition == null;

        public ItemInstance() { }

        public ItemInstance(ItemData definition)
        {
            Definition = definition;
            CurrentDurability = definition != null ? definition.GetMaxDurability() : -1f;
        }

        // Copy constructor — used when moving instances between slots.
        public ItemInstance(ItemInstance source)
        {
            Definition = source.Definition;
            CurrentDurability = source.CurrentDurability;
        }
    }
}
