// Located at: Assets/Scripts/UI/UI_DraggedItem.cs
using ProjectWitchcraft.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectWitchcraft.UI
{
    public class UI_DraggedItem : MonoBehaviour
    {
        private Image _icon;
        private TextMeshProUGUI _quantityText;
        private RectTransform _rectTransform;
        private InventoryManager _inventoryManager;

        private void Start()
        {
            _icon = GetComponentInChildren<Image>();
            _quantityText = GetComponentInChildren<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();
            _inventoryManager = InventoryManager.Instance;

            // Hide on start by disabling the components, not the GameObject
            _icon.enabled = false;
            _quantityText.enabled = false;
        }

        private void Update()
        {
            // If the inventory manager is holding an item, show it and follow the mouse
            if (!_inventoryManager.HeldSlot.IsEmpty)
            {
                // **FIX:** Enable the component directly
                _icon.enabled = true;
                _rectTransform.position = Input.mousePosition;
                _icon.sprite = _inventoryManager.HeldSlot.item.Icon;

                if (_inventoryManager.HeldSlot.quantity > 1)
                {
                    _quantityText.text = _inventoryManager.HeldSlot.quantity.ToString();
                    _quantityText.enabled = true;
                }
                else
                {
                    _quantityText.enabled = false;
                }
            }
            else
            {
                // Otherwise, hide the components
                _icon.enabled = false;
                _quantityText.enabled = false;
            }
        }
    }
}