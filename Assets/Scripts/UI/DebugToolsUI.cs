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
        [SerializeField] private AssetRegistry _assetRegistry;

        [Header("UI & Settings")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private int amountToAdd = 100;
        [SerializeField] private int amountToDamage = 50;
        [SerializeField] private Toggle flyModeToggle;
        [SerializeField] private Toggle destroyModeToggle;

        private void Start()
        {
            if (debugPanel != null) debugPanel.SetActive(false);

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
            if (Keyboard.current.backquoteKey.wasPressedThisFrame) ToggleDebugPanel();
        }

        public void ToggleDebugPanel()
        {
            if (debugPanel != null) debugPanel.SetActive(!debugPanel.activeSelf);
        }

        public void OnFlyModeToggled(bool isFlyModeOn)
        {
            EventManager.TriggerEvent(new ToggleFlyModeEvent { IsFlyModeActive = isFlyModeOn });
        }

        public void OnDestroyModeToggled(bool isDestroyModeOn)
        {
            EventManager.TriggerEvent(new ToggleDestroyModeEvent { IsDestroyModeActive = isDestroyModeOn });
        }

        public void GiveAllItems()
        {
            if (_assetRegistry == null) { Debug.LogError("[DebugTools] AssetRegistry not assigned."); return; }
            var inv = InventoryManager.Instance;
            if (inv == null) { Debug.LogError("[DebugTools] InventoryManager.Instance is null."); return; }

            foreach (var itemData in _assetRegistry.AllItemsOfAllTypes)
            {
                if (itemData != null)
                    inv.AddItem(itemData, Mathf.Min(amountToAdd, itemData.maxStackSize));
            }
        }

        public void DamageAllEquipment()
        {
            EquipmentManager.Instance?.DamageAll(amountToDamage);
        }

        public void RepairEverything()
        {
            var inv = InventoryManager.Instance;
            if (inv == null) return;
            RepairSlotList(inv.HotbarSlots);
            RepairSlotList(inv.InventorySlots);
            if (inv.ExternalInventory != null)
                RepairSlotList(inv.ExternalInventory);

            EquipmentManager.Instance.RepairAll();
        }

        private static void RepairSlotList(System.Collections.Generic.IReadOnlyList<InventorySlot> slots)
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.Instance.HasDurability && slot.Instance.Definition != null)
                    slot.Instance.CurrentDurability = slot.Instance.Definition.GetMaxDurability();
            }
        }
    }
}
