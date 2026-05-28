// Located at: Assets/Scripts/Core/ItemEvents.cs
using ProjectWitchcraft.Core;
using UnityEngine; // Added for Vector3

/// <summary>
/// Fired whenever the player's inventory changes (items added, removed, or moved).
/// If itemData is null, it signifies a major change requiring a full UI refresh.
/// </summary>
public struct InventoryChangedEvent
{
    public ItemData itemData;
    public int NewAmount;
}


/// <summary>
/// Fired by the InventoryManager when the player drops an item into the world.
/// </summary>
public struct ItemDroppedInWorldEvent
{
    public ItemData itemData;
    public int quantity;
    public Vector3 position;
}