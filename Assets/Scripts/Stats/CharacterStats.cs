using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectWitchcraft.Core;

/// <summary>
/// Manages all stats for a character. This component should be attached to any GameObject
/// that needs to have stats, such as the player or enemies.
/// </summary>

namespace ProjectWitchcraft.Stats
{
    public class CharacterStats : MonoBehaviour
    {
        // A dictionary to hold the base values for each stat.
        // This can be configured in the Inspector.
        [System.Serializable]
        public class StatBaseValue
        {
            public StatDefinition Stat;
            public float Value;
        }

        [Tooltip("The list of base stats for this character.")]
        [SerializeField] private List<StatBaseValue> baseStats = new List<StatBaseValue>();

        // The core of the system: a dictionary that holds all active modifiers for each stat.
        private readonly Dictionary<StatDefinition, List<StatModifier>> _statModifiers = new Dictionary<StatDefinition, List<StatModifier>>();

        // A cache to store the final calculated values of stats. This improves performance
        // by avoiding recalculation every time a stat is accessed within the same frame.
        private readonly Dictionary<StatDefinition, float> _cachedStatValues = new Dictionary<StatDefinition, float>();
        private bool _isCacheDirty = true;

        /// <summary>
        /// A helper class to associate a StatDefinition with a list of its modifiers.
        /// </summary>
        public class StatBonus
        {
            public StatDefinition Stat;
            public StatModifier Modifier;
        }

        /// <summary>
        /// Adds a list of mixed stat bonuses from a single source.
        /// This is the primary method for applying stats from items or buffs.
        /// </summary>
        /// <param name="source">The object providing the modifiers (e.g., an item, a buff).</param>
        /// <param name="bonuses">The list of stat bonuses to add.</param>
        public void AddBonuses(object source, List<StatBonus> bonuses)
        {
            foreach (var bonus in bonuses)
            {
                // Ensure the modifier's source is correctly set
                if (bonus.Modifier.Source != source)
                {
                    Debug.LogError("Modifier source mismatch. The source object provided to AddBonuses must be the same as the source object on the StatModifier.", this);
                    continue;
                }
                AddModifier(bonus.Stat, bonus.Modifier);
            }
        }

        /// <summary>
        /// Removes all modifiers provided by a specific source.
        /// </summary>
        /// <param name="source">The source of the modifiers to remove.</param>
        public void RemoveModifiersFromSource(object source)
        {
            foreach (var statMods in _statModifiers.Values)
            {
                // We iterate backwards because we are removing items from the list.
                for (int i = statMods.Count - 1; i >= 0; i--)
                {
                    if (statMods[i].Source == source)
                    {
                        statMods.RemoveAt(i);
                        _isCacheDirty = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the final calculated value of a stat after applying all modifiers.
        /// </summary>
        /// <param name="stat">The StatDefinition to calculate the value for.</param>
        /// <returns>The final stat value.</returns>
        public float GetStatValue(StatDefinition stat)
        {
            if (!_isCacheDirty && _cachedStatValues.TryGetValue(stat, out float cachedValue))
            {
                return cachedValue;
            }

            float value = CalculateStatValue(stat);
            _cachedStatValues[stat] = value;
            return value;
        }

        public void LateUpdate()
        {
            // Invalidate the cache at the end of each frame.
            _isCacheDirty = true;
        }

        private void AddModifier(StatDefinition stat, StatModifier mod)
        {
            if (!_statModifiers.ContainsKey(stat))
            {
                _statModifiers[stat] = new List<StatModifier>();
            }
            _statModifiers[stat].Add(mod);
            _isCacheDirty = true;
        }

        private float CalculateStatValue(StatDefinition stat)
        {
            float finalValue = GetBaseStatValue(stat);

            if (!_statModifiers.TryGetValue(stat, out var relevantModifiers) || relevantModifiers.Count == 0)
            {
                return finalValue;
            }

            // Sort modifiers by calculation stage order to ensure correct application.
            relevantModifiers.Sort((a, b) => ((int)a.Stage).CompareTo((int)b.Stage));

            // --- Stage: Override ---
            // Find the highest override value, if any.
            float overrideValue = float.MinValue;
            bool hasOverride = false;
            foreach (var mod in relevantModifiers)
            {
                if (mod.Stage == CalculationStage.Override)
                {
                    if (mod.Value > overrideValue)
                    {
                        overrideValue = mod.Value;
                        hasOverride = true;
                    }
                }
            }
            if (hasOverride)
            {
                finalValue = overrideValue;
            }

            // --- Stage: Flat ---
            foreach (var mod in relevantModifiers)
            {
                if (mod.Stage == CalculationStage.Flat)
                {
                    finalValue += mod.Value;
                }
            }

            // --- Stage: Additive ---
            float additivePercentSum = 0;
            foreach (var mod in relevantModifiers)
            {
                if (mod.Stage == CalculationStage.Additive)
                {
                    additivePercentSum += mod.Value;
                }
            }
            finalValue *= (1 + additivePercentSum);

            // --- Stage: Multiplicative ---
            foreach (var mod in relevantModifiers)
            {
                if (mod.Stage == CalculationStage.Multiplicative)
                {
                    finalValue *= (1 + mod.Value);
                }
            }

            return finalValue;
        }

        private float GetBaseStatValue(StatDefinition stat)
        {
            var baseStat = baseStats.FirstOrDefault(s => s.Stat == stat);
            return baseStat?.Value ?? 0f;
        }
    }
}