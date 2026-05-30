using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.UI
{
    public class UI_EquipmentSlot : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IDropHandler, IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler
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

        // Drag out to unequip — mirrors InventorySlot.OnBeginDrag.
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_inventoryManager.HeldSlot.IsEmpty) return;

            var unequipped = EquipmentManager.Instance.Unequip(_slotType);
            if (unequipped == null) return;

            _inventoryManager.SetHeldItem(unequipped);
        }

        public void OnDrag(PointerEventData eventData) { }

        // If drag ends over nothing, drop the held item into the world.
        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == null)
                _inventoryManager.DropHeldItem();
        }

        // Drop a held item onto this slot to equip it.
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

        // Click to place a held item — mirrors inventory click-to-place.
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (_inventoryManager.HeldSlot.IsEmpty) return;

            OnDrop(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var item = EquipmentManager.Instance.GetEquippedItem(_slotType);
            if (item == null || item.IsEmpty) return;

            var held = _inventoryManager.HeldSlot;
            if (!held.IsEmpty)
                UI_ItemTooltip.Instance?.ShowWithComparison(item, held.Instance);
            else
                UI_ItemTooltip.Instance?.Show(item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UI_ItemTooltip.Instance?.Hide();
        }
    }
}
