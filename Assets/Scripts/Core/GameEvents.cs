// Located at: Assets/Scripts/Core/GameEvents.cs
namespace ProjectWitchcraft.Core
{
    public enum GameState
    {
        Loading,    // scene loading / world gen — no player input
        Playing,    // normal exploration
        Building,   // placement mode active, player can still move
        InMenu,     // any UI panel open (inventory, chest, etc.)
        Combat,     // reserved for future combat-specific behaviour
        Paused,     // Time.timeScale = 0
        Cinematic,  // cutscene — all input suppressed
    }

    // Defines the reason why the game entered the InMenu state.
    public enum MenuContext
    {
        // The menu was opened for a generic reason or the context is not specified.
        None,
        // The menu was opened by the player directly (e.g., hitting Tab).
        PlayerInventory,
        // The menu was opened to interact with a container (e.g., a chest).
        Container,
        // The menu was opened for an interaction that requires seeing player stats (e.g., an enchanting table).
        InfoInteraction,
        // The character equipment panel is open.
        Equipment
    }

    public struct GameStateChangedEvent
    {
        // Provides context for why the state changed, especially for entering the InMenu state.
        public MenuContext Context;
        public GameState NewState;
    }
}