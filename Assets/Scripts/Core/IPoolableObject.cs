// Located at: Assets/Scripts/Core/IPoolableObject.cs
namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// An interface for any GameObject that can be managed by the ObjectPooler.
    /// It provides a consistent way for the pooler to know the object's original pool tag.
    /// </summary>
    public interface IPoolableObject
    {
        string PoolTag { get; }
    }
}
