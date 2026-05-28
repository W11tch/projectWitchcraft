// Located at: Assets/Scripts/UI/DebugToolsUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using ProjectWitchcraft.Managers;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.UI
{
    public class DebugToolsUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private ItemDatabase itemDatabase; // Renamed

        [Header("UI & Settings")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private int amountToAdd = 100;
        [SerializeField] private Toggle flyModeToggle;
        [SerializeField] private Toggle destroyModeToggle;

        private void Start()
        {
            if (debugPanel != null) { debugPanel.SetActive(false); }

            if (flyModeToggle != null)
            {
                flyModeToggle.isOn = false;
                OnFlyModeToggled(false);
                flyModeToggle.onValueChanged.AddListener(OnFlyModeToggled);
            }

            if (destroyModeToggle != null)
            {
                destroyModeToggle.isOn = true;
                OnDestroyModeToggled(true);
                destroyModeToggle.onValueChanged.AddListener(OnDestroyModeToggled);
            }
        }

        private void Update()
        {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame) { ToggleDebugPanel(); }
        }

        public void ToggleDebugPanel()
        {
            if (debugPanel != null) { debugPanel.SetActive(!debugPanel.activeSelf); }
        }

        public void OnFlyModeToggled(bool isFlyModeOn)
        {
            EventManager.TriggerEvent(new ToggleFlyModeEvent { IsFlyModeActive = isFlyModeOn });
        }

        public void OnDestroyModeToggled(bool isDestroyModeOn)
        {
            EventManager.TriggerEvent(new ToggleDestroyModeEvent { IsDestroyModeActive = isDestroyModeOn });
        }

        public void GiveAllItems() // Renamed from GiveAllResources
        {
            if (inventoryManager == null || itemDatabase == null) return;
            foreach (var itemData in itemDatabase.AllItems)
            {
                if (itemData != null) { inventoryManager.AddItem(itemData, amountToAdd); }
            }
        }
    }
}