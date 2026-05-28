// Located at: Assets/Scripts/Core/DebugEvents.cs
namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// Fired by the debug UI to enable or disable the destroy functionality.
    /// </summary>
    public struct ToggleDestroyModeEvent
    {
        public bool IsDestroyModeActive;
    }

    /// <summary>
    /// Fired by UI or debug tools to request a change in the player's fly mode status.
    /// </summary>
    public struct ToggleFlyModeEvent
    {
        public bool IsFlyModeActive;
    }
}