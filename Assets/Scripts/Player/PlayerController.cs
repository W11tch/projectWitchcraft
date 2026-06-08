// Located at: Assets/Scripts/Player/PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using ProjectWitchcraft.Managers;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public int ActiveHotbarIndex { get; private set; } = 0;

        private PlayerCombat _combat;

        private void Awake()
        {
            _combat = GetComponent<PlayerCombat>();
        }

        private void OnEnable()
        {
            EventManager.AddListener<InventoryChangedEvent>(OnInventoryChanged);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<InventoryChangedEvent>(OnInventoryChanged);
        }

        // Covers startup/after-load and any change to the active-slot weapon (pickup, move, consume).
        private void OnInventoryChanged(InventoryChangedEvent e) => RefreshActiveWeapon();

        // Applies the affixes of the weapon in the active hotbar slot (and removes the previous one's).
        private void RefreshActiveWeapon()
        {
            var slots = InventoryManager.Instance?.HotbarSlots;
            ItemInstance weapon = null;
            if (slots != null && ActiveHotbarIndex >= 0 && ActiveHotbarIndex < slots.Count)
            {
                var slot = slots[ActiveHotbarIndex];
                if (!slot.IsEmpty) weapon = slot.Instance;
            }
            EquipmentManager.Instance?.SetActiveWeapon(weapon);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            Debug.Log($"[Combat] OnAttack called — phase={context.phase}  _combat={(_combat != null ? "OK" : "NULL")}");
            if (context.performed)
                _combat?.TryAttack();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            // If a menu is open, the interact key should close it.
            // Otherwise, it should trigger an interaction.

            var gm = GameManager.Instance;
            if (gm.CurrentState == GameState.InMenu) { gm.UpdateState(GameState.Playing); }
            else if (gm.CurrentState == GameState.Playing)
            { 
                EventManager.TriggerEvent(new InteractActionTriggeredEvent()); 
            }

        }

        // This method is now connected to the new hotbar system.
        public void OnSelectHotbarSlot(InputAction.CallbackContext context)
        {
            var state = GameManager.Instance.CurrentState;
            if ((state != GameState.Playing && state != GameState.Building) || !context.performed) return;

            if (int.TryParse(context.control.name, out int keyNumber))
            {
                int slotIndex = keyNumber == 0 ? 9 : keyNumber - 1;
                ActiveHotbarIndex = slotIndex;
                RefreshActiveWeapon();

                var inventoryManager = InventoryManager.Instance;
                if (slotIndex < 0 || slotIndex >= inventoryManager.HotbarSlots.Count) return;

                var slot = inventoryManager.HotbarSlots[slotIndex];

                // Determine 2H state regardless of whether the slot is empty.
                bool isTwoHanded = slot.Instance?.Definition is WeaponItemData weapon && weapon.IsTwoHanded;
                EquipmentManager.Instance.SetOffHandActive(!isTwoHanded);

                if (slot.IsEmpty)
                {
                    EventManager.TriggerEvent(new CancelActionTriggeredEvent());
                    return;
                }

                if (slot.Instance?.Definition is PlaceableItemData placeableItem)
                {
                    EventManager.TriggerEvent(new PlacementModeRequestedEvent { ItemData = placeableItem });
                }
                else
                {
                    EventManager.TriggerEvent(new CancelActionTriggeredEvent());
                }
            }
        }

        public void OnPlace(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                EventManager.TriggerEvent(new PlaceActionTriggeredEvent());
            }
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                EventManager.TriggerEvent(new RotateActionTriggeredEvent());
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                EventManager.TriggerEvent(new CancelActionTriggeredEvent());
            }
        }

        public void OnDestroyObject(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                EventManager.TriggerEvent(new DestroyActionTriggeredEvent());
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var gm = GameManager.Instance;
            if (gm.CurrentState == GameState.Playing) { gm.UpdateState(GameState.Paused); }
            else if (gm.CurrentState == GameState.Paused) { gm.UpdateState(GameState.Playing); }
            else if (gm.CurrentState == GameState.InMenu) { gm.UpdateState(GameState.Paused); }
        }

        public void OnToggleInventory(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var gm = GameManager.Instance;
            if (gm.CurrentState == GameState.Playing)
            {
                gm.UpdateState(GameState.InMenu, MenuContext.PlayerInventory);
            }
            else if (gm.CurrentState == GameState.InMenu) 
            { 
                gm.UpdateState(GameState.Playing);
            }
        }
    }
}