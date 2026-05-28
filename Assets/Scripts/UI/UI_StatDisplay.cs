// Located at: Assets/Scripts/UI/UI_StatDisplay.cs
using ProjectWitchcraft.Core;
using TMPro;
using UnityEngine;

namespace ProjectWitchcraft.UI
{
    /// <summary>
    /// Controls a single UI element that displays the name and value of a stat.
    /// </summary>
    public class UI_StatDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statNameText;
        [SerializeField] private TextMeshProUGUI statValueText;

        private StatDefinition _statDef;

        /// <summary>
        /// Sets the static information for this display element.
        /// </summary>
        public void SetStat(StatDefinition statDef)
        {
            _statDef = statDef;
            statNameText.text = $"{_statDef.statName}:";
        }

        /// <summary>
        /// Updates the displayed value of the stat.
        /// </summary>
        public void UpdateValue(float value)
        {
            // You can customize formatting here
            // ex with one decimal
            // statValueText.text = value.ToString("F1");
            // Actually shows an integrer
            statValueText.text = Mathf.RoundToInt(value).ToString();
        }

        /// <summary>
        /// Returns the StatDefinition this UI element is currently displaying.
        /// Used by the UI_StatsPanel to know which stat value to fetch.
        /// </summary>
        public StatDefinition GetStatDefinition()
        {
            return _statDef;
        }
    }
}