namespace ProjectWitchcraft.Core
{
    // How an entity behaves across save/load. Declared per entity (serialized on the Entity component).
    public enum EntityPersistencePolicy
    {
        // Saved exactly as left — position and current health restored on load.
        Persistent,
        // Saved position, but returns to full state (health) on load. Fits the target dummy.
        RespawnInPlace,
        // Never saved; repopulated by spawners. Skipped entirely by the save system.
        Transient,
    }
}
