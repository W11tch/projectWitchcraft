// Located at: Assets/Scripts/Interactables/ChestController.cs
using UnityEngine;
using System.Collections.Generic;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.Interactables
{
    public class ChestController : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private string _chestName = "Chest";
        [SerializeField] private int _inventorySize = 20;

        public List<InventorySlot> Inventory { get; private set; }
        public string UniqueID { get; private set; }

        private void Awake()
        {
            Inventory = new List<InventorySlot>(_inventorySize);
            for (int i = 0; i < _inventorySize; i++)
            {
                Inventory.Add(new InventorySlot());
            }

            // A simple way to give chests a unique ID for saving.
            UniqueID = $"{transform.position.x}_{transform.position.y}_{transform.position.z}";
        }

        #region IInteractable Implementation
        public string InteractionPrompt => $"Open {_chestName}";

        public bool Interact()
        {

            // Fire event for UI to open
            EventManager.TriggerEvent(new OpenContainerUIEvent
            {
                ContainerName = _chestName,
                ContainerInventory = this.Inventory
            });

            // Set game state to InMenu to pause player actions
            GameManager.Instance.UpdateState(GameState.InMenu, MenuContext.Container);

            Debug.Log($"Interacted with chest: {UniqueID}");
            return true;
        }
        #endregion
    }
}