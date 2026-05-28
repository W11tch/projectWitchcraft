// Located at: Assets/Scripts/Core/SaveLoadEvents.cs
namespace ProjectWitchcraft.Core
{
    // --- UI -> System Events ---

    /// <summary>
    /// Fired when the player clicks a "Save" button.
    /// </summary>
    public struct SaveRequestEvent { }

    /// <summary>
    /// Fired when the player clicks a "Load" button.
    /// </summary>
    public struct LoadRequestEvent { }

    // --- System -> System Events ---

    /// <summary>
    /// Fired by the SaveManager to tell all listening systems to provide their data.
    /// </summary>
    public struct GatherSaveDataEvent
    {
        public SaveData SaveData;
    }

    /// <summary>
    /// Fired by the SaveManager to tell all listening systems to load their state from the provided data.
    /// </summary>
    public struct ApplySaveDataEvent
    {
        public SaveData SaveData;
    }

    // --- System -> UI Events ---

    /// <summary>
    /// Fired by the SaveManager after a save operation is complete.
    /// </summary>
    public struct GameSavedEvent { }

    /// <summary>
    /// Fired by the SaveManager after a load operation is complete.
    /// </summary>
    public struct GameLoadedEvent { }

    /// <summary>
    /// Fired when save data is cleared (for future use).
    /// </summary>
    public struct SaveDataClearedEvent { }
}