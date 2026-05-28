// Located at: Assets/Scripts/UI/UI_InventorySlot.cs
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectWitchcraft.UI
{
    public class UI_InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _quantityText;
        private InventoryManager _inventoryManager;
        private int _slotIndex;
        private InventoryType _slotType; // **MODIFIED**: This now uses the enum from the Core assembly.
        public void Initialize(InventoryManager manager, int index, InventoryType type)
        {
            _inventoryManager = manager;
            _slotIndex = index;
            _slotType = type;
        }
        public void UpdateSlot(InventorySlot slot)
        {
            if (slot.IsEmpty)
            {
                _itemIcon.enabled = false;
                _quantityText.enabled = false;
            }
            else
            {
                _itemIcon.sprite = slot.item.Icon;
                _itemIcon.enabled = true;
                _quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
                _quantityText.enabled = slot.quantity > 1;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging) return;

            if (!_inventoryManager.HeldSlot.IsEmpty)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    _inventoryManager.DropHeldItemOnSlot(_slotIndex, _slotType);
                }
                else if (eventData.button == PointerEventData.InputButton.Right)
                {
                    _inventoryManager.PlaceOneFromHeldStack(_slotIndex, _slotType);
                }
            }
            else
            {
                if (eventData.button == PointerEventData.InputButton.Right)
                {
                    _inventoryManager.SplitStack(_slotIndex, _slotType);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_inventoryManager.HeldSlot.IsEmpty)
            {
                _inventoryManager.PickupSlotContents(_slotIndex, _slotType);
            }
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            // **THE FIX:** Check if the drag ended without dropping on a valid slot.
            // The 'pointerCurrentRaycast' is null if the cursor is not over any UI element.
            if (eventData.pointerCurrentRaycast.gameObject == null)
            {
                _inventoryManager.DropHeldItem();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            _inventoryManager.DropHeldItemOnSlot(_slotIndex, _slotType);
        }
    }
}