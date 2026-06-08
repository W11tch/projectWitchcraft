using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "ProjectWitchcraft/Equipment/Weapon")]
    public class WeaponItemData : ItemData
    {
        [Header("Weapon")]
        [SerializeField] private bool _isTwoHanded;
        [SerializeField] private int _maxDurability = 100;
        [SerializeField] private List<EquipmentAffix> _affixes = new();
        [SerializeField] private AttackDefinition _attackDefinition;
        [Tooltip("Base swing rate for this weapon (attacks per second), before the wielder's AttackSpeed multiplier.")]
        [SerializeField] private float _baseAttacksPerSecond = 1f;

        public bool IsTwoHanded => _isTwoHanded;
        public int MaxDurability => _maxDurability;
        public IReadOnlyList<EquipmentAffix> Affixes => _affixes;
        public AttackDefinition AttackDefinition => _attackDefinition;
        public float BaseAttacksPerSecond => _baseAttacksPerSecond;

        public override float GetMaxDurability() => _maxDurability;
    }
}
