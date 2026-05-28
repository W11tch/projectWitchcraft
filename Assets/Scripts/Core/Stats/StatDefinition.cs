// Located at: Assets/Scripts/Core/Stats/StatDefinition.cs
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// A ScriptableObject that defines the static properties of a stat.
    /// Each unique stat in the game (e.g., Health, Damage, Mining Speed) should have its own StatDefinition asset.
    /// </summary>
    [CreateAssetMenu(fileName = "New StatDefinition", menuName = "Witchcraft/Stats/Stat Definition")]

    public class StatDefinition : ScriptableObject
    {
        [Tooltip("The display name of the stat.")]
        public string statName;

        [Tooltip("The description of the stat, used for tooltips.")]
        [TextArea]
        public string description;

        [Tooltip("The icon to display next to the stat in the UI.")]
        public Sprite icon;

        [Tooltip("The category this stat belongs to, used for UI grouping.")]
        public StatCategory category;
    }

}