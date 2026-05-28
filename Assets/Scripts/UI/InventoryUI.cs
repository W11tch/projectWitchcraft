// Located at: Assets/Scripts/UI/InventoryUI.cs
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("The panel that holds the player's stats and equipment displays. This will be shown when opening the inventory directly.")]
        [SerializeField] private GameObject characterInfoPanel;

        private GameObject _panel;

        private void Awake()
        {
            _panel = this.gameObject;
            _panel.SetActive(false); // Hide on start
            EventManager.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            // Show the panel ONLY when the game state is InMenu
            bool shouldBeActive = (e.NewState == GameState.InMenu);
            _panel.SetActive(shouldBeActive);

            // We only need to check the context if the panel is being activated.
            if (shouldBeActive)
            {
                // If the context is Container, hide the character info panel.
                if (e.Context == MenuContext.Container)
                {
                    characterInfoPanel.SetActive(false);
                }
                else // For any other context, show it.
                {
                    characterInfoPanel.SetActive(true);
                }
            }
            else
            {
                // If the main inventory panel is being hidden, always hide the character info panel too.
                characterInfoPanel.SetActive(false);
            }
        }
    }
}