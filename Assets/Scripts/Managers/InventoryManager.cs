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
        [SerializeField] private float _dropDistance = 1.5f;

        private List<InventorySlot> _hotbarSlots;
        private List<InventorySlot> _inventorySlots;
        private List<InventorySlot> _externalInventory;
        public bool IsExternalInventoryOpen => _externalInventory != null;

        private InventorySlot _heldSlot = new InventorySlot();

        public IReadOnlyList<InventorySlot> HotbarSlots => _hotbarSlots;
        public IReadOnlyList<InventorySlot> InventorySlots => _inventorySlots;
        public IReadOnlyList<InventorySlot> ExternalInventory => _externalInventory;
        public InventorySlot HeldSlot => _heldSlot;

        public void OpenExternalInventory(List<InventorySlot> externalInventory)
        {
            _externalInventory = externalInventory;
        }

        public void CloseExternalInventory()
        {
            if (!_heldSlot.IsEmpty)
            {
                AddItem(_heldSlot.Instance?.Definition, _heldSlot.quantity);
                if (!_heldSlot.IsEmpty)
                    DropHeldItem();
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
                Vector3 toHit = hit.point - playerPosition;
                toHit.y = 0;
                dropPosition = toHit.magnitude > _dropDistance
                    ? playerPosition + toHit.normalized * _dropDistance
                    : hit.point;
            }
            else
            {
                Plane groundPlane = new Plane(Vector3.up, playerPosition);
                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector3 toHit = hitPoint - playerPosition;
                    toHit.y = 0;
                    dropPosition = toHit.magnitude > _dropDistance
                        ? playerPosition + toHit.normalized * _dropDistance
                        : new Vector3(hitPoint.x, playerPosition.y, hitPoint.z);
                }
                else
                {
                    dropPosition = playerPosition + playerTransform.forward.normalized * _dropDistance;
                }
            }

            EventManager.TriggerEvent(new ItemDroppedInWorldEvent
            {
                itemInstance = _heldSlot.Instance,
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

            foreach (var slot in _hotbarSlots.Concat(_inventorySlots).Where(s => !s.IsEmpty && s.Instance.Definition == itemData))
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
                EventManager.TriggerEvent(new InventoryChangedEvent());

            return amountRemaining;
        }

        // Puts an item directly into the held slot (e.g. when swapping out an equipped item).
        public void SetHeldItem(ItemInstance instance, int quantity = 1)
        {
            _heldSlot = new InventorySlot(instance, quantity);
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }

        // Use this for durable items (equipment, weapons, tools) to preserve the existing instance.
        public int AddItemInstance(ItemInstance instance, int amount)
        {
            if (instance == null || instance.IsEmpty || amount <= 0) return amount;
            int remaining = amount;
            foreach (var slot in _hotbarSlots.Concat(_inventorySlots).Where(s => s.IsEmpty))
            {
                slot.Instance = instance;
                slot.quantity = 1;
                remaining--;
                if (remaining == 0) break;
            }
            if (remaining < amount)
                EventManager.TriggerEvent(new InventoryChangedEvent());
            return remaining;
        }

        private int AddToStack(InventorySlot slot, int amount)
        {
            int spaceAvailable = slot.Instance.Definition.maxStackSize - slot.quantity;
            int amountToAdd = Mathf.Min(amount, spaceAvailable);
            slot.AddQuantity(amountToAdd);
            return amount - amountToAdd;
        }

        private int CreateNewStack(InventorySlot slot, ItemData item, int amount)
        {
            int amountToAdd = Mathf.Min(amount, item.maxStackSize);
            slot.Instance = new ItemInstance(item);
            slot.quantity = amountToAdd;
            return amount - amountToAdd;
        }

        public void RemoveItem(ItemData itemData, int amount)
        {
            if (!HasItem(itemData, amount)) return;
            int amountToRemove = amount;
            foreach (var slot in _hotbarSlots.Concat(_inventorySlots).Where(s => !s.IsEmpty && s.Instance.Definition == itemData))
            {
                int amountToRemoveFromSlot = Mathf.Min(amountToRemove, slot.quantity);
                slot.quantity -= amountToRemoveFromSlot;
                amountToRemove -= amountToRemoveFromSlot;
                if (slot.quantity <= 0) slot.Clear();
                if (amountToRemove == 0) break;
            }
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }

        public bool HasItem(ItemData itemData, int amount) => GetItemAmount(itemData) >= amount;

        public int GetItemAmount(ItemData itemData)
        {
            if (itemData == null) return 0;
            return _hotbarSlots.Concat(_inventorySlots)
                .Where(s => !s.IsEmpty && s.Instance.Definition == itemData)
                .Sum(s => s.quantity);
        }

        public void PickupSlotContents(int fromIndex, InventoryType fromType)
        {
            if (!_heldSlot.IsEmpty) return;
            var fromList = GetListFromType(fromType);
            if (fromList == null) return;

            InventorySlot fromSlot = fromList[fromIndex];
            if (fromSlot.IsEmpty) return;

            _heldSlot = new InventorySlot(fromSlot.Instance, fromSlot.quantity);
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
                toList[toIndex] = new InventorySlot(_heldSlot.Instance, _heldSlot.quantity);
                _heldSlot.Clear();
            }
            else if (toSlot.Instance.Definition == _heldSlot.Instance.Definition)
            {
                int spaceInToStack = toSlot.Instance.Definition.maxStackSize - toSlot.quantity;
                int amountToMove = Mathf.Min(_heldSlot.quantity, spaceInToStack);
                toSlot.AddQuantity(amountToMove);
                _heldSlot.quantity -= amountToMove;
                if (_heldSlot.quantity <= 0) _heldSlot.Clear();
            }
            else
            {
                InventorySlot temp = new InventorySlot(toSlot.Instance, toSlot.quantity);
                toList[toIndex] = new InventorySlot(_heldSlot.Instance, _heldSlot.quantity);
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
            _heldSlot = new InventorySlot(fromSlot.Instance.Definition, halfAmount);
            EventManager.TriggerEvent(new InventoryChangedEvent());
        }

        public void PlaceHeldItem(int toIndex, InventoryType toType) => DropHeldItemOnSlot(toIndex, toType);

        public void PlaceOneFromHeldStack(int toIndex, InventoryType toType)
        {
            if (_heldSlot.IsEmpty) return;
            var toList = GetListFromType(toType);
            if (toList == null) return;

            InventorySlot toSlot = toList[toIndex];

            if (toSlot.IsEmpty)
            {
                toList[toIndex] = new InventorySlot(_heldSlot.Instance.Definition, 1);
                _heldSlot.quantity--;
            }
            else if (toSlot.Instance.Definition == _heldSlot.Instance.Definition && toSlot.quantity < toSlot.Instance.Definition.maxStackSize)
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
                case InventoryType.PlayerHotbar: return _hotbarSlots;
                case InventoryType.PlayerInventory: return _inventorySlots;
                case InventoryType.Container: return _externalInventory;
                default: return null;
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
            itemGuid = slot.IsEmpty ? "" : slot.Instance.Definition.AssetGuid,
            quantity = slot.quantity,
            durability = slot.IsEmpty ? -1f : slot.Instance.CurrentDurability
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
                var itemDef = _assetRegistry.GetItemByGuid(data.itemGuid);
                if (itemDef == null) { slots[i].Clear(); continue; }
                slots[i].Instance = new ItemInstance(itemDef) { CurrentDurability = data.durability };
                slots[i].quantity = data.quantity;
            }
        }
    }
}
