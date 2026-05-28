// Located at: Assets/Scripts/Core/Stats/StatDatabase.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    /// <summary>
    /// A ScriptableObject that acts as a central database for all StatDefinition assets in the game.
    /// This allows UI and other systems to easily access a list of all possible stats.
    /// </summary>
    [CreateAssetMenu(fileName = "StatDatabase", menuName = "ProjectWitchcraft/Stats/Stat Database")]

    public class StatDatabase : ScriptableObject
    {
        [Tooltip("A list of all stats that should be displayed in the player's stat panel, in order.")]
        public List<StatDefinition> playerStats;
    }
}
