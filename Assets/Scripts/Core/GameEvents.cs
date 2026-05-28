// Located at: Assets/Scripts/Core/GameEvents.cs
namespace ProjectWitchcraft.Core
{
    // The GameState enum now lives here, in the Core assembly.
    public enum GameState { PreGame, Playing, Paused, InMenu }

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
        InfoInteraction
    }

    public struct GameStateChangedEvent
    {
        // Provides context for why the state changed, especially for entering the InMenu state.
        public MenuContext Context;
        public GameState NewState;
    }
}