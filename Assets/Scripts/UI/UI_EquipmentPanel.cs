using UnityEngine;
using UnityEngine.UI;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.UI
{
    public class UI_EquipmentPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _container;

        private static readonly EquipmentSlot[] SlotOrder =
        {
            EquipmentSlot.Head,
            EquipmentSlot.Chest,
            EquipmentSlot.Legs,
            EquipmentSlot.Boots,
            EquipmentSlot.Amulet,
            EquipmentSlot.Charm,
            EquipmentSlot.OffHand,
            EquipmentSlot.Ring1,
            EquipmentSlot.Ring2,
        };

        private bool _slotsCreated;

        private void Awake()
        {
            EventManager.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnEnable()
        {
            if (!_slotsCreated)
            {
                if (_slotPrefab == null || _container == null) return;
                foreach (var slotType in SlotOrder)
                {
                    var go = Instantiate(_slotPrefab, _container);
                    go.GetComponent<UI_EquipmentSlot>().Initialize(slotType);
                }
                _slotsCreated = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_container);
                return; // Initialize() already refreshed each slot
            }

            // Sync all slots with current state — covers events that fired while panel was inactive
            foreach (Transform child in _container)
            {
                var slot = child.GetComponent<UI_EquipmentSlot>();
                slot?.Refresh(EquipmentManager.Instance?.GetEquippedItem(slot.SlotType));
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            bool shouldShow = e.NewState == GameState.InMenu && e.Context != MenuContext.Container;
            gameObject.SetActive(shouldShow);
        }
    }
}
