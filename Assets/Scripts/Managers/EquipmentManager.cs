using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Stats;

namespace ProjectWitchcraft.Managers
{
    public class EquipmentManager : Singleton<EquipmentManager>
    {
        [SerializeField] private AssetRegistry _assetRegistry;
        [Tooltip("The Damage stat. A held weapon's base damage is applied to it as an Override, replacing the player's base before flat/additive/multiplicative affixes stack.")]
        [SerializeField] private StatDefinition _damageStat;

        private readonly Dictionary<EquipmentSlot, ItemInstance> _equippedItems = new();
        // The weapon currently selected in the active hotbar slot. Weapons aren't slot-equippable
        // (WeaponItemData isn't EquipmentItemData), so their affixes are applied through this path
        // instead of _equippedItems, using the instance as the modifier source.
        private ItemInstance _activeWeapon;
        private bool _isOffHandActive = true;

        // Cached reference — fetched lazily once Player is in the scene.
        private CharacterStats _playerStats;

        public bool IsOffHandActive => _isOffHandActive;

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

        // Equips an item. Returns false if the item type doesn't match the slot.
        // displaced is set to the item that was already in the slot (null if the slot was empty).
        public bool TryEquip(ItemInstance instance, EquipmentSlot slot, out ItemInstance displaced)
        {
            displaced = null;
            if (instance == null || instance.IsEmpty) return false;
            if (!IsValidForSlot(instance, slot)) return false;

            if (_equippedItems.TryGetValue(slot, out var existing) && existing != null)
            {
                RemoveModifiers(existing);
                displaced = existing;
            }

            _equippedItems[slot] = instance;

            if (IsSlotActive(slot))
                ApplyModifiers(instance);

            EventManager.TriggerEvent(new EquipmentChangedEvent { Slot = slot, NewItem = instance });
            return true;
        }

        // Unequips the item in the given slot. Returns the item so the caller can return it to inventory.
        public ItemInstance Unequip(EquipmentSlot slot)
        {
            if (!_equippedItems.TryGetValue(slot, out var instance) || instance == null) return null;

            RemoveModifiers(instance);
            _equippedItems.Remove(slot);
            EventManager.TriggerEvent(new EquipmentChangedEvent { Slot = slot, NewItem = null });
            return instance;
        }

        // Applies the affixes of the weapon held in the active hotbar slot, removing those of the
        // previously held weapon. Pass null (or a non-weapon) to clear. Idempotent — safe to call
        // on every InventoryChangedEvent.
        public void SetActiveWeapon(ItemInstance weapon)
        {
            if (weapon == null || weapon.IsEmpty || weapon.Definition is not WeaponItemData)
                weapon = null;

            if (ReferenceEquals(weapon, _activeWeapon)) return;

            if (_activeWeapon != null)
                RemoveModifiers(_activeWeapon);

            _activeWeapon = weapon;

            if (_activeWeapon != null)
                ApplyModifiers(_activeWeapon);
        }

        // Called by PlayerController when the active hotbar item changes.
        public void SetOffHandActive(bool active)
        {
            if (_isOffHandActive == active) return;
            _isOffHandActive = active;

            if (_equippedItems.TryGetValue(EquipmentSlot.OffHand, out var offHandItem) && offHandItem != null)
            {
                if (active)
                    ApplyModifiers(offHandItem);
                else
                    RemoveModifiers(offHandItem);

                EventManager.TriggerEvent(new EquipmentChangedEvent { Slot = EquipmentSlot.OffHand, NewItem = offHandItem });
            }
        }

        public ItemInstance GetEquippedItem(EquipmentSlot slot)
        {
            _equippedItems.TryGetValue(slot, out var item);
            return item;
        }

        public bool IsSlotActive(EquipmentSlot slot)
        {
            return slot != EquipmentSlot.OffHand || _isOffHandActive;
        }

        // Debug helper — restores all equipped item durability to max.
        public void RepairAll()
        {
            foreach (var instance in _equippedItems.Values)
            {
                if (instance != null && instance.HasDurability && instance.Definition != null)
                    instance.CurrentDurability = instance.Definition.GetMaxDurability();
            }
        }

        // Debug helper — reduces all equipped item durability by the given amount.
        public void DamageAll(int amount)
        {
            foreach (var instance in _equippedItems.Values)
            {
                if (instance != null && instance.HasDurability)
                    instance.CurrentDurability = Mathf.Max(0, instance.CurrentDurability - amount);
            }
        }

        private bool IsValidForSlot(ItemInstance instance, EquipmentSlot slot)
        {
            if (instance.Definition is EquipmentItemData equip)
            {
                if (equip.Slot == slot) return true;
                // Ring items can go in either ring slot.
                if (equip.Slot == EquipmentSlot.Ring && (slot == EquipmentSlot.Ring1 || slot == EquipmentSlot.Ring2))
                    return true;
            }
            return false;
        }

        private void ApplyModifiers(ItemInstance instance)
        {
            var bonuses = new List<CharacterStats.StatBonus>();

            // A held weapon's base damage overrides the player's base Damage stat; the affix
            // flat/additive/multiplicative stages then stack on top (see CharacterStats pipeline).
            if (instance.Definition is WeaponItemData weapon && _damageStat != null)
            {
                bonuses.Add(new CharacterStats.StatBonus
                {
                    Stat = _damageStat,
                    Modifier = new StatModifier(weapon.BaseDamage, CalculationStage.Override, instance)
                });
            }

            var affixes = GetAffixes(instance);
            if (affixes != null)
            {
                foreach (var affix in affixes)
                {
                    bonuses.Add(new CharacterStats.StatBonus
                    {
                        Stat = affix.Stat,
                        Modifier = new StatModifier(affix.Value, affix.Stage, instance)
                    });
                }
            }

            if (bonuses.Count == 0) return;
            GetPlayerStats()?.AddBonuses(instance, bonuses);
        }

        private void RemoveModifiers(ItemInstance instance)
        {
            GetPlayerStats()?.RemoveModifiersFromSource(instance);
        }

        private static IReadOnlyList<EquipmentAffix> GetAffixes(ItemInstance instance)
        {
            if (instance.Definition is EquipmentItemData equip) return equip.Affixes;
            if (instance.Definition is WeaponItemData weapon) return weapon.Affixes;
            if (instance.Definition is ToolItemData tool) return tool.Affixes;
            return null;
        }

        private CharacterStats GetPlayerStats()
        {
            if (_playerStats == null && GameReferences.Instance?.PlayerTransform != null)
                _playerStats = GameReferences.Instance.PlayerTransform.GetComponent<CharacterStats>();
            return _playerStats;
        }

        private void OnGatherSaveData(GatherSaveDataEvent e)
        {
            e.SaveData.player.equipment.slots.Clear();
            e.SaveData.player.equipment.isOffHandActive = _isOffHandActive;

            foreach (var (slot, instance) in _equippedItems)
            {
                if (instance == null || instance.IsEmpty) continue;
                e.SaveData.player.equipment.slots[slot.ToString()] = new ItemInstanceSaveData
                {
                    itemGuid = instance.Definition.AssetGuid,
                    durability = instance.CurrentDurability
                };
            }
        }

        private void OnApplySaveData(ApplySaveDataEvent e)
        {
            UnequipAll();
            _isOffHandActive = e.SaveData.player.equipment.isOffHandActive;

            foreach (var (slotName, data) in e.SaveData.player.equipment.slots)
            {
                if (!System.Enum.TryParse<EquipmentSlot>(slotName, out var slot)) continue;
                if (_assetRegistry == null)
                {
                    Debug.LogError("[EquipmentManager] AssetRegistry not assigned. Cannot restore equipment.");
                    break;
                }
                var itemDef = _assetRegistry.GetItemByGuid(data.itemGuid);
                if (itemDef == null) continue;
                var instance = new ItemInstance(itemDef) { CurrentDurability = data.durability };
                _equippedItems[slot] = instance;
                if (IsSlotActive(slot))
                    ApplyModifiers(instance);
                EventManager.TriggerEvent(new EquipmentChangedEvent { Slot = slot, NewItem = instance });
            }
        }

        private void UnequipAll()
        {
            foreach (var (slot, instance) in _equippedItems)
            {
                if (instance != null)
                    RemoveModifiers(instance);
                EventManager.TriggerEvent(new EquipmentChangedEvent { Slot = slot, NewItem = null });
            }
            _equippedItems.Clear();
        }
    }
}
