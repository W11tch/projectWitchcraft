namespace ProjectWitchcraft.Core
{
    // Implemented by any world object that has a persistent inventory (e.g. ChestController).
    // Used by ChunkManager to save/load chest contents without creating a circular assembly dependency.
    public interface IChestInventory
    {
        string UniqueID { get; }
        ChestSaveData GetSaveData();
        void ApplySaveData(ChestSaveData data, AssetRegistry registry);
    }
}
