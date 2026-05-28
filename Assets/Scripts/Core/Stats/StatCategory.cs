// Located at: Assets/Scripts/Core/Stats/StatCategory.cs
namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// Defines categories for stats to allow for easy grouping and display in the UI.
    /// </summary>
    public enum StatCategory
    {
        /// <summary>
        /// Stats related to combat, such as health and damage.
        /// </summary>
        Combat,

        /// <summary>
        /// Stats related to resource gathering and crafting.
        /// </summary>
        Gathering,

        /// <summary>
        /// General-purpose stats, such as movement speed.
        /// </summary>
        Utility
    }
}