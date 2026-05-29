using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.UI
{
    public class UI_EquipmentSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [SerializeField] private Image _icon;

        private EquipmentSlot _slotType;
        private InventoryManager _inventoryManager;

        public EquipmentSlot SlotType => _slotType;

        public void Initialize(EquipmentSlot slotType)
        {
            _slotType = slotType;
            _inventoryManager = InventoryManager.Instance;
            EventManager.AddListener<EquipmentChangedEvent>(OnEquipmentChanged);
            Refresh(EquipmentManager.Instance.GetEquippedItem(slotType));
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<EquipmentChangedEvent>(OnEquipmentChanged);
        }

        private void OnEquipmentChanged(EquipmentChangedEvent e)
        {
            if (e.Slot == _slotType)
                Refresh(e.NewItem);
        }

        public void Refresh(ItemInstance item)
        {
            if (_icon == null) return;
            if (item == null || item.IsEmpty)
            {
                _icon.sprite = null;
                _icon.enabled = false;
            }
            else
            {
                _icon.sprite = item.Definition.Icon;
                _icon.enabled = true;
            }
        }

        public void SetActive(bool active)
        {
            if (_icon != null)
                _icon.color = active ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var heldSlot = _inventoryManager.HeldSlot;
            if (heldSlot.IsEmpty) return;

            if (!EquipmentManager.Instance.TryEquip(heldSlot.Instance, _slotType, out var displaced))
                return;

            heldSlot.Clear();

            if (displaced != null)
                _inventoryManager.SetHeldItem(displaced);
            else
                EventManager.TriggerEvent(new InventoryChangedEvent());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;

            var unequipped = EquipmentManager.Instance.Unequip(_slotType);
            if (unequipped == null) return;

            int remaining = _inventoryManager.AddItemInstance(unequipped, 1);
            if (remaining > 0)
                _inventoryManager.SetHeldItem(unequipped);
        }
    }
}
