// Located at: Assets/Scripts/Core/ItemData.cs
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// The base ScriptableObject for all items in the game.
    /// Contains common data that every item shares.
    /// </summary>
    public class ItemData : ScriptableObject
    {
        [Header("Item Information")]
        public string Name;
        public Sprite Icon;
        [Tooltip("The maximum number of this item that can be held in a single inventory slot.")]
        public int maxStackSize = 9999;
        [TextArea]
        public string description;
    }
}