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

        public bool IsTwoHanded => _isTwoHanded;
        public int MaxDurability => _maxDurability;
        public IReadOnlyList<EquipmentAffix> Affixes => _affixes;

        public override float GetMaxDurability() => _maxDurability;
    }
}
