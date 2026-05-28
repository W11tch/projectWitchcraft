// Located at: Assets/Scripts/Core/InventorySlot.cs
using ProjectWitchcraft.Core;

/// <summary>
/// Represents a single slot within an inventory. It holds a reference
/// to the item data and the quantity of that item in this slot.
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public ItemData item;   // The ScriptableObject defining the item in this slot.
    public int quantity;    // The number of items in this slot.

    // A flag to easily check if the slot is empty.
    public bool IsEmpty => item == null || quantity <= 0;

    /// <summary>
    /// Creates an empty inventory slot.
    /// </summary>
    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }

    /// <summary>
    /// Creates an inventory slot with a specific item and quantity.
    /// </summary>
    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    /// <summary>
    /// Clears the slot, making it empty.
    /// </summary>
    public void Clear()
    {
        item = null;
        quantity = 0;
    }

    /// <summary>
    /// Adds a given amount to the slot's quantity.
    /// </summary>
    public void AddQuantity(int amount)
    {
        quantity += amount;
    }
}