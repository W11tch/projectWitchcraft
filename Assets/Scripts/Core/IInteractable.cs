// Located at: Assets/Scripts/Core/IInteractable.cs
namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// An interface for any object that the player can interact with in the world.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// A display name that can be used by UI prompts.
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// The method to be called when the player interacts with this object.
        /// </summary>
        /// <returns>True if the interaction was successful, false otherwise.</returns>
        bool Interact();
    }
}