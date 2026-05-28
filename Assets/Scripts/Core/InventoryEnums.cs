namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// Defines the different types of inventories that can be interacted with.
    /// This is placed in Core so both Managers and UI can reference it without circular dependencies.
    /// </summary>
    public enum InventoryType { PlayerHotbar, PlayerInventory, Container }
}
