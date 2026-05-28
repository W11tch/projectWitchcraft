// Located at: Assets/Scripts/Core/PlacementEvents.cs
using ProjectWitchcraft.Core;

/// <summary>
/// Fired when the player selects a placeable item from the hotbar,
/// requesting to enter placement mode.
/// </summary>
public struct PlacementModeRequestedEvent
{
    public PlaceableItemData ItemData;
}