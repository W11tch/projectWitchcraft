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

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("Attack action performed.");
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            // --- MODIFIED DEBUG LINE ---
            Debug.Log($"OnInteract called. Current Game State is: {GameManager.Instance.CurrentState}");
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
            if (GameManager.Instance.CurrentState != GameState.Playing || !context.performed) return;

            if (int.TryParse(context.control.name, out int keyNumber))
            {
                int slotIndex = keyNumber == 0 ? 9 : keyNumber - 1;

                var inventoryManager = InventoryManager.Instance;
                if (slotIndex < 0 || slotIndex >= inventoryManager.HotbarSlots.Count) return;

                var slot = inventoryManager.HotbarSlots[slotIndex];
                if (slot.IsEmpty)
                {
                    EventManager.TriggerEvent(new CancelActionTriggeredEvent());
                    return;
                }

                if (slot.item is PlaceableItemData placeableItem)
                {
                    EventManager.TriggerEvent(new PlacementModeRequestedEvent { ItemData = placeableItem });
                }
                else
                {
                    Debug.Log($"Selected non-placeable item: {slot.item.Name}");
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