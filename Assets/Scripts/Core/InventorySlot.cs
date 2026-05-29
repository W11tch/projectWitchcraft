using ProjectWitchcraft.Core;

[System.Serializable]
public class InventorySlot
{
    public ItemInstance Instance;
    public int quantity;

    public bool IsEmpty => Instance == null || Instance.Definition == null || quantity <= 0;

    public InventorySlot() { }

    // Convenience constructor — creates a fresh ItemInstance for the given definition.
    public InventorySlot(ItemData item, int quantity)
    {
        Instance = item != null ? new ItemInstance(item) : null;
        this.quantity = quantity;
    }

    // Use this when moving an existing instance between slots to preserve runtime state (durability, etc.).
    public InventorySlot(ItemInstance instance, int quantity)
    {
        Instance = instance;
        this.quantity = quantity;
    }

    public void Clear()
    {
        Instance = null;
        quantity = 0;
    }

    public void AddQuantity(int amount) => quantity += amount;
}
