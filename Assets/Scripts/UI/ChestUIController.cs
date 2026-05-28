using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.UI
{
    /// <summary>
    /// Manages the UI panel for a chest or any other item container.
    /// It listens for an event to open, then populates itself with the container's inventory data.
    /// </summary>
    public class ChestUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _containerNameText;
        [SerializeField] private Transform _slotsContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject _slotPrefab;

        private List<UI_InventorySlot> _uiSlots = new List<UI_InventorySlot>();
        private InventoryManager _inventoryManager;

        // NEW: A reference to the inventory we are currently displaying.
        private List<InventorySlot> _currentInventory;

        private void Awake()
        {
            // Ensure the panel is hidden on start.
            _panel.SetActive(false);

            // Cache the reference to the InventoryManager.
            _inventoryManager = InventoryManager.Instance;

            // Subscribe to events.
            EventManager.AddListener<OpenContainerUIEvent>(OnOpenContainer);
            // Subscribe to game state changes.
            EventManager.AddListener<GameStateChangedEvent>(OnGameStateChanged);
            // NEW: Subscribe to inventory change events.
            EventManager.AddListener<InventoryChangedEvent>(OnInventoryChanged);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks.
            EventManager.RemoveListener<OpenContainerUIEvent>(OnOpenContainer);
            // Unsubscribe from game state changes.
            EventManager.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
            // NEW: Unsubscribe from inventory change events.
            EventManager.RemoveListener<InventoryChangedEvent>(OnInventoryChanged);
        }

        //private void Start()
        //{
        //}

        /// <summary>
        /// Handles the event to open the container UI.
        /// </summary>
        /// <param name="e">The event data containing the container's details.</param>
        private void OnOpenContainer(OpenContainerUIEvent e)
        {
            // Check if the text component is assigned before trying to use it.
            if (_containerNameText != null)
            {
                _containerNameText.text = e.ContainerName;
            }

            _currentInventory = e.ContainerInventory;
            // Tell the InventoryManager that this container is now open.
            _inventoryManager.OpenExternalInventory(_currentInventory);

            // Clear any old slots before creating new ones.
            foreach (Transform child in _slotsContainer)
            {
                Destroy(child.gameObject);
            }
            _uiSlots.Clear();

            // Create and initialize a UI slot for each item slot in the container.
            for (int i = 0; i < e.ContainerInventory.Count; i++)
            {
                GameObject slotGO = Instantiate(_slotPrefab, _slotsContainer);
                var uiSlot = slotGO.GetComponent<UI_InventorySlot>();
                // Initialize the slot as a 'Container' type.
                uiSlot.Initialize(_inventoryManager, i, InventoryType.Container);
                _uiSlots.Add(uiSlot);
            }

            // Update all slots with the current item data and show the panel.
            UpdateAllSlots(_currentInventory);
            _panel.SetActive(true);
        }

        // NEW METHOD: This will be called whenever any inventory changes.
        private void OnInventoryChanged(InventoryChangedEvent e)
        {
            // If our panel is active, we must update because we don't know WHICH inventory changed.
            if (_panel.activeSelf)
            {
                UpdateAllSlots(_currentInventory);
            }
        }

        /// <summary>
        /// Handles the generic event to close the top UI panel.
        /// </summary>
        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            // If the new state is NOT InMenu and our panel is active, close it.
            // This is called by the 'E' and the 'Tab' key via ExitMenuMode().
            if (e.NewState != GameState.InMenu && _panel.activeSelf)
            {
                ClosePanel();
            }
        }

        /// <summary>
        /// Closes the UI panel and cleans up the state.
        /// </summary>
        public void ClosePanel()
        {
            _panel.SetActive(false);
            // Tell the InventoryManager that no external container is open anymore.
            _inventoryManager.CloseExternalInventory();

            // Clear the reference to the inventory we were displaying.
            _currentInventory = null;

        }

        /// <summary>
        /// Updates the visual representation of all slots.
        /// </summary>
        /// <param name="inventory">The list of inventory slots to display.</param>
        private void UpdateAllSlots(List<InventorySlot> inventory)
        {
            for (int i = 0; i < _uiSlots.Count; i++)
            {
                _uiSlots[i].UpdateSlot(inventory[i]);
            }
        }
    }
}
