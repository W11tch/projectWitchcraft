// Located at: Assets/Scripts/UI/UI_StatPanel.cs
using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Stats;

namespace ProjectWitchcraft.UI
{
    /// <summary>
    /// Manages the player's stat display panel in the UI.
    /// It populates itself with all stats found in the StatDatabase.
    /// </summary>
    public class UI_StatsPanel : MonoBehaviour
    {
        [Header("Data Sources")]
        [Tooltip("Reference to the StatDatabase asset that lists all player stats.")]
        [SerializeField] private StatDatabase statDatabase;

        [Tooltip("The CharacterStats component of the entity whose stats should be displayed. Drag the Player here.")]
        [SerializeField] private CharacterStats characterStats;

        [Header("UI Prefabs and Containers")]
        [Tooltip("The prefab for a single stat display row.")]
        [SerializeField] private GameObject statDisplayPrefab;

        [Tooltip("The parent transform where stat display rows will be instantiated.")]
        [SerializeField] private Transform statContainer;

        private readonly List<UI_StatDisplay> _statDisplays = new List<UI_StatDisplay>();

        private void Start()
        {
            // --- CHANGE EXPLANATION ---
            // Replaced FindObjectOfType with a direct serialized reference.
            // This is more performant, flexible, and better for decoupling.
            // You must now drag the Player's CharacterStats component into the inspector.
            if (characterStats == null)
            {
                Debug.LogError("CharacterStats reference is not set in the inspector for UI_StatsPanel.", this);
                gameObject.SetActive(false);
                return;
            }

            CreateStatDisplays();
        }

        private void Update()
        {
            if (characterStats == null) return;

            // Update the value of each stat every frame.
            foreach (var display in _statDisplays)
            {
                var statDef = display.GetStatDefinition();
                if (statDef != null)
                {
                    display.UpdateValue(characterStats.GetStatValue(statDef));
                }
            }
        }

        private void CreateStatDisplays()
        {
            // Clear any existing displays
            foreach (Transform child in statContainer)
            {
                Destroy(child.gameObject);
            }
            _statDisplays.Clear();

            // Create a new display for each stat in the database
            foreach (var statDef in statDatabase.playerStats)
            {
                GameObject statDisplayGO = Instantiate(statDisplayPrefab, statContainer);
                UI_StatDisplay statDisplay = statDisplayGO.GetComponent<UI_StatDisplay>();
                if (statDisplay != null)
                {
                    statDisplay.SetStat(statDef);
                    _statDisplays.Add(statDisplay);
                }
            }
        }
    }
}
