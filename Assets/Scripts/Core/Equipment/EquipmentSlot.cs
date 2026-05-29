namespace ProjectWitchcraft.Core
{
    public enum EquipmentSlot
    {
        // Physical character slots (used as dictionary keys in EquipmentManager and slot types in UI).
        Head,
        Chest,
        Legs,
        Boots,
        Amulet,
        Charm,
        OffHand,
        Ring1,
        Ring2,

        // Item-facing category — set this on RingItemData. Accepted by either Ring1 or Ring2 slot.
        Ring
    }
}
