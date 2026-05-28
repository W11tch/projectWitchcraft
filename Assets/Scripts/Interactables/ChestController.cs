using UnityEngine;
using System.Collections.Generic;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.Interactables
{
    public class ChestController : MonoBehaviour, IInteractable, IChestInventory
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
                Inventory.Add(new InventorySlot());

            UniqueID = System.Guid.NewGuid().ToString();
        }

        #region IInteractable
        public string InteractionPrompt => $"Open {_chestName}";

        public bool Interact()
        {
            EventManager.TriggerEvent(new OpenContainerUIEvent
            {
                ContainerName = _chestName,
                ContainerInventory = this.Inventory
            });
            GameManager.Instance.UpdateState(GameState.InMenu, MenuContext.Container);
            return true;
        }
        #endregion

        // Called by ChunkManager during save.
        public ChestSaveData GetSaveData()
        {
            var data = new ChestSaveData { uniqueId = UniqueID };
            foreach (var slot in Inventory)
                data.slots.Add(new SlotData
                {
                    itemGuid = slot.IsEmpty ? "" : slot.item.AssetGuid,
                    quantity = slot.quantity
                });
            return data;
        }

        // Called by ChunkManager during load.
        public void ApplySaveData(ChestSaveData data, AssetRegistry registry)
        {
            UniqueID = data.uniqueId;
            for (int i = 0; i < Inventory.Count && i < data.slots.Count; i++)
            {
                var slotData = data.slots[i];
                if (string.IsNullOrEmpty(slotData.itemGuid) || slotData.quantity <= 0)
                {
                    Inventory[i].Clear();
                    continue;
                }
                Inventory[i].item = registry.GetItemByGuid(slotData.itemGuid);
                Inventory[i].quantity = slotData.quantity;
                if (Inventory[i].item == null) Inventory[i].Clear();
            }
        }
    }
}
