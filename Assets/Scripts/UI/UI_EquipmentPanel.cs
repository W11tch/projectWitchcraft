using UnityEngine;
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

        private void Awake()
        {
            gameObject.SetActive(false);
            EventManager.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void Start()
        {
            if (_slotPrefab == null || _container == null) return;

            foreach (var slotType in SlotOrder)
            {
                var go = Instantiate(_slotPrefab, _container);
                go.GetComponent<UI_EquipmentSlot>().Initialize(slotType);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            bool shouldShow = e.NewState == GameState.InMenu && e.Context != MenuContext.Container;
            gameObject.SetActive(shouldShow);
        }
    }
}
