// Located at: Assets/Scripts/UI/UI_InventoryDisplay.cs
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.UI
{
    /// <summary>
    /// Manages the creation and updating of the inventory and hotbar UI visuals.
    /// </summary>
    public class UI_InventoryDisplay : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private GameObject _inventorySlotPrefab;

        [Header("UI Containers")]
        [SerializeField] private Transform _hotbarContainer;
        [SerializeField] private Transform _inventoryContainer;

        private List<UI_InventorySlot> _hotbarSlotsUI = new List<UI_InventorySlot>();
        private List<UI_InventorySlot> _inventorySlotsUI = new List<UI_InventorySlot>();

        private void OnEnable()
        {
            EventManager.AddListener<InventoryChangedEvent>(OnInventoryChanged);
            if (_hotbarSlotsUI.Count == 0)
            {
                CreateSlots();
            }
            UpdateAllSlots();
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<InventoryChangedEvent>(OnInventoryChanged);
        }


        // ... (keep the rest of the script the same) ...

        private void CreateSlots()
        {
            // --- Defensive Checks ---
            if (_inventoryManager == null) { Debug.LogError("ERROR: _inventoryManager is NULL in UI_InventoryDisplay!"); return; }
            if (_inventoryManager.HotbarSlots == null) { Debug.LogError("ERROR: _inventoryManager.HotbarSlots is NULL! This is likely an execution order problem."); return; }
            if (_inventorySlotPrefab == null) { Debug.LogError("ERROR: _inventorySlotPrefab is NULL in UI_InventoryDisplay!"); return; }
            if (_hotbarContainer == null) { Debug.LogError("ERROR: _hotbarContainer is NULL in UI_InventoryDisplay!"); return; }
            // --- End of Checks ---

            // Create Hotbar Slots
            for (int i = 0; i < _inventoryManager.HotbarSlots.Count; i++)
            {
                var slotObject = Instantiate(_inventorySlotPrefab, _hotbarContainer);
                if (slotObject == null) { Debug.LogError($"ERROR: Failed to Instantiate slot prefab for hotbar slot {i}!"); continue; }

                var uiSlot = slotObject.GetComponent<UI_InventorySlot>();
                if (uiSlot == null) { Debug.LogError($"ERROR: The instantiated prefab for hotbar slot {i} is MISSING the UI_InventorySlot script!"); continue; }

                uiSlot.Initialize(_inventoryManager, i, InventoryType.PlayerHotbar); // This was line 43
                _hotbarSlotsUI.Add(uiSlot);
            }

            // Create Inventory Slots
            if (_inventoryContainer == null) { Debug.LogError("ERROR: _inventoryContainer is NULL in UI_InventoryDisplay!"); return; }
            for (int i = 0; i < _inventoryManager.InventorySlots.Count; i++)
            {
                var slotObject = Instantiate(_inventorySlotPrefab, _inventoryContainer);
                var uiSlot = slotObject.GetComponent<UI_InventorySlot>();
                if (uiSlot == null) { Debug.LogError($"ERROR: The instantiated prefab for inventory slot {i} is MISSING the UI_InventorySlot script!"); continue; }

                uiSlot.Initialize(_inventoryManager, i, InventoryType.PlayerInventory);
                _inventorySlotsUI.Add(uiSlot);
            }
        }

        // ... (keep the rest of the script the same) ...

        private void OnInventoryChanged(InventoryChangedEvent e)
        {
            UpdateAllSlots();
        }

        private void UpdateAllSlots()
        {
            for (int i = 0; i < _hotbarSlotsUI.Count; i++)
            {
                _hotbarSlotsUI[i].UpdateSlot(_inventoryManager.HotbarSlots[i]);
            }

            for (int i = 0; i < _inventorySlotsUI.Count; i++)
            {
                _inventorySlotsUI[i].UpdateSlot(_inventoryManager.InventorySlots[i]);
            }
        }
    }
}