// Located at: Assets/Scripts/Managers/InventoryManager.cs
using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core;
using System.Linq;
using UnityEngine.InputSystem;

namespace ProjectWitchcraft.Managers
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        [Header("Data")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private AssetRegistry _assetRegistry;
        [Header("Settings")]
        [SerializeField] private int hotbarSize = 10;
        [SerializeField] private int inventorySize = 30;

        [Header("Drop Settings")]
        [SerializeField] private LayerMask _groundLayer;
        // Controls how far from the player the item is dropped
        [SerializeField] private float _dropDistance = 1.5f;

        private List<InventorySlot> _hotbarSlots;
        private List<InventorySlot> _inventorySlots;
        // A reference to the inventory of an external container (like a chest) that is currently open.
        private List<InventorySlot> _externalInventory;
        // A property to check if an external inventory is currently open.
        public bool IsExternalInventoryOpen => _externalInventory != null;

        private InventorySlot _heldSlot = new InventorySlot();

        public IReadOnlyList<InventorySlot> HotbarSlots => _hotbarSlots;
        public IReadOnlyList<InventorySlot> InventorySlots => _inventorySlots;
        public IReadOnlyList<InventorySlot> ExternalInventory => _externalInventory;
        public InventorySlot HeldSlot => _heldSlot;

        // Called by a UI controller (like ChestUIController) when a container is opened.
        // <param name="externalInventory">The inventory list of the container being opened.</param>
        public void OpenExternalInventory(List<InventorySlot> externalInventory)
        {
            _externalInventory = externalInventory;
        }

        // Called by a UI controller when a container UI is closed.
        public void CloseExternalInventory()
        {
            // If the player is still holding an item when the container is closed,
            // try to return it to their inventory. If there's no space, drop it.
            if (!_heldSlot.IsEmpty)
            {
                AddItem(_heldSlot.item, _heldSlot.quantity);
                if (!_heldSlot.IsEmpty) // If item could not be fully added
                {
                    DropHeldItem();
                }
            }
            _externalInventory = null;
        }

        public void DropHeldItem()
        {
            if (_heldSlot.IsEmpty) return;

            Transform playerTransform = GameReferences.Instance.PlayerTransform;
            if (playerTransform == null)
            {
                Debug.LogError("[InventoryManager] Cannot drop item: Player transform not registered in GameReferences.");
                return;
            }

            Vector3 dropPosition;
            Vector3 playerPosition = playerTransform.position;

            Ray ray = GameReferences.Instance.MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
            {
                // Drop at cursor position, clamped to _dropDistance from the player.
                Vector3 toHit = hit.point - playerPosition;
                toHit.y = 0;
                dropPosition = toHit.magnitude > _dropDistance
                    ? playerPosition + toHit.normalized * _dropDistance
                    : hit.point;
            }
            else
            {
                // Fallback: cursor pointing at sky — drop in front of the player.
                Vector3 forwardDirection = playerTransform.forward;
                forwardDirection.y = 0;
                dropPosition = playerPosition + forwardDirection.normalized * _dropDistance;
            }

            EventManager.TriggerEvent(new ItemDroppedInWorldEvent
            {
                itemData = _heldSlot.item,
                quantity = _heldSlot.quantity,
                position = dropPosition
            });

            _heldSlot.Clear();
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }

        protected override void Awake()
        {
            base.Awake();
            if (itemDatabase != null) itemDatabase.Initialize();
            else Debug.LogError("ItemDatabase is not assigned in the InventoryManager Inspector!", this.gameObject);
            _hotbarSlots = new List<InventorySlot>(hotbarSize);
            _inventorySlots = new List<InventorySlot>(inventorySize);
            for (int i = 0; i < hotbarSize; i++) _hotbarSlots.Add(new InventorySlot());
            for (int i = 0; i < inventorySize; i++) _inventorySlots.Add(new InventorySlot());
        }
        private void OnEnable()
        {
            EventManager.AddListener<GatherSaveDataEvent>(OnGatherSaveData);
            EventManager.AddListener<ApplySaveDataEvent>(OnApplySaveData);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<GatherSaveDataEvent>(OnGatherSaveData);
            EventManager.RemoveListener<ApplySaveDataEvent>(OnApplySaveData);
        }
        public int AddItem(ItemData itemData, int amount)
        {
            if (itemData == null || amount <= 0) return amount;
            int amountRemaining = amount;
            foreach (var slot in _hotbarSlots.Concat(_inventorySlots).Where(s => !s.IsEmpty && s.item == itemData))
            {
                amountRemaining = AddToStack(slot, amountRemaining);
                if (amountRemaining == 0) break;
            }
            if (amountRemaining > 0)
            {
                foreach (var slot in _hotbarSlots.Concat(_inventorySlots).Where(s => s.IsEmpty))
                {
                    amountRemaining = CreateNewStack(slot, itemData, amountRemaining);
                    if (amountRemaining == 0) break;
                }
            }
            if (amountRemaining < amount)
            {
                EventManager.TriggerEvent(new InventoryChangedEvent());
            }
            return amountRemaining;
        }
        private int AddToStack(InventorySlot slot, int amount)
        {
            int spaceAvailable = slot.item.maxStackSize - slot.quantity;
            int amountToAdd = Mathf.Min(amount, spaceAvailable);
            slot.AddQuantity(amountToAdd);
            return amount - amountToAdd;
        }
        private int CreateNewStack(InventorySlot slot, ItemData item, int amount)
        {
            int amountToAdd = Mathf.Min(amount, item.maxStackSize);
            slot.item = item;
            slot.quantity = amountToAdd;
            return amount - amountToAdd;
        }
        public void RemoveItem(ItemData itemData, int amount)
        {
            if (!HasItem(itemData, amount)) return;
            int amountToRemove = amount;
            foreach (var slot in _hotbarSlots.Concat(_inventorySlots).Where(s => !s.IsEmpty && s.item == itemData))
            {
                int amountToRemoveFromSlot = Mathf.Min(amountToRemove, slot.quantity);
                slot.quantity -= amountToRemoveFromSlot;
                amountToRemove -= amountToRemoveFromSlot;
                if (slot.quantity <= 0) slot.Clear();
                if (amountToRemove == 0) break;
            }
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }
        public bool HasItem(ItemData itemData, int amount)
        {
            return GetItemAmount(itemData) >= amount;
        }
        public int GetItemAmount(ItemData itemData)
        {
            if (itemData == null) return 0;
            return _hotbarSlots.Concat(_inventorySlots)
                .Where(s => !s.IsEmpty && s.item == itemData)
                .Sum(s => s.quantity);
        }
        // This logic needs to handle different slot types
        public void PickupSlotContents(int fromIndex, InventoryType fromType)
        {
            if (!_heldSlot.IsEmpty) return;
            var fromList = GetListFromType(fromType);
            if (fromList == null) return;

            InventorySlot fromSlot = fromList[fromIndex];
            if (fromSlot.IsEmpty) return;

            _heldSlot = new InventorySlot(fromSlot.item, fromSlot.quantity);
            fromSlot.Clear();
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }
        public void DropHeldItemOnSlot(int toIndex, InventoryType toType)
        {
            if (_heldSlot.IsEmpty) return;
            var toList = GetListFromType(toType);
            if (toList == null) return;

            InventorySlot toSlot = toList[toIndex];

            if (toSlot.IsEmpty)
            {
                toList[toIndex] = new InventorySlot(_heldSlot.item, _heldSlot.quantity);
                _heldSlot.Clear();
            }
            else if (toSlot.item == _heldSlot.item)
            {
                int spaceInToStack = toSlot.item.maxStackSize - toSlot.quantity;
                int amountToMove = Mathf.Min(_heldSlot.quantity, spaceInToStack);
                toSlot.AddQuantity(amountToMove);
                _heldSlot.quantity -= amountToMove;
                if (_heldSlot.quantity <= 0) _heldSlot.Clear();
            }
            else
            {
                InventorySlot temp = new InventorySlot(toSlot.item, toSlot.quantity);
                toList[toIndex] = new InventorySlot(_heldSlot.item, _heldSlot.quantity);
                _heldSlot = temp;
            }
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }
        public void SplitStack(int fromIndex, InventoryType fromType)
        {
            if (!_heldSlot.IsEmpty) return;
            var fromList = GetListFromType(fromType);
            if (fromList == null) return;
            InventorySlot fromSlot = fromList[fromIndex];
            if (fromSlot.IsEmpty || fromSlot.quantity < 2) return;
            int halfAmount = Mathf.CeilToInt(fromSlot.quantity / 2f);
            fromSlot.quantity -= halfAmount;
            _heldSlot = new InventorySlot(fromSlot.item, halfAmount);
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }
        public void PlaceHeldItem(int toIndex, InventoryType toType)
        {
            DropHeldItemOnSlot(toIndex, toType);
        }
        public void PlaceOneFromHeldStack(int toIndex, InventoryType toType)
        {
            if (_heldSlot.IsEmpty) return;
            var toList = GetListFromType(toType);
            if (toList == null) return;

            InventorySlot toSlot = toList[toIndex];

            if (toSlot.IsEmpty)
            {
                toList[toIndex] = new InventorySlot(_heldSlot.item, 1);
                _heldSlot.quantity--;
            }
            else if (toSlot.item == _heldSlot.item && toSlot.quantity < toSlot.item.maxStackSize)
            {
                toSlot.AddQuantity(1);
                _heldSlot.quantity--;
            }
            if (_heldSlot.quantity <= 0) _heldSlot.Clear();
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }

                private List<InventorySlot> GetListFromType(InventoryType type)
        {
            switch (type)
            {
                case InventoryType.PlayerHotbar:
                    return _hotbarSlots;
                case InventoryType.PlayerInventory:
                    return _inventorySlots;
                case InventoryType.Container:
                    return _externalInventory;
                default:
                    return null;
            }
        }

        private void OnGatherSaveData(GatherSaveDataEvent e)
        {
            e.SaveData.player.hotbar.Clear();
            e.SaveData.player.inventory.Clear();

            foreach (var slot in _hotbarSlots)
                e.SaveData.player.hotbar.Add(MakeSlotData(slot));
            foreach (var slot in _inventorySlots)
                e.SaveData.player.inventory.Add(MakeSlotData(slot));
        }

        private static SlotData MakeSlotData(InventorySlot slot) => new SlotData
        {
            itemGuid = slot.IsEmpty ? "" : slot.item.AssetGuid,
            quantity = slot.quantity
        };

        private void OnApplySaveData(ApplySaveDataEvent e)
        {
            if (_assetRegistry == null)
            {
                Debug.LogError("[InventoryManager] AssetRegistry not assigned. Cannot load inventory.");
                return;
            }

            ApplySlotList(_hotbarSlots, e.SaveData.player.hotbar);
            ApplySlotList(_inventorySlots, e.SaveData.player.inventory);
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }

        private void ApplySlotList(List<InventorySlot> slots, List<SlotData> dataList)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (i >= dataList.Count) { slots[i].Clear(); continue; }
                var data = dataList[i];
                if (string.IsNullOrEmpty(data.itemGuid) || data.quantity <= 0) { slots[i].Clear(); continue; }
                slots[i].item = _assetRegistry.GetItemByGuid(data.itemGuid);
                slots[i].quantity = data.quantity;
                if (slots[i].item == null) slots[i].Clear();
            }
        }
    }
}