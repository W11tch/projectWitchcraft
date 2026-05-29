using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "ProjectWitchcraft/Equipment/Equipment Item")]
    public class EquipmentItemData : ItemData
    {
        [Header("Equipment")]
        [SerializeField] private EquipmentSlot _slot;
        [SerializeField] private int _maxDurability = 100;
        [SerializeField] private List<EquipmentAffix> _affixes = new();

        public EquipmentSlot Slot => _slot;
        public int MaxDurability => _maxDurability;
        public IReadOnlyList<EquipmentAffix> Affixes => _affixes;

        public override float GetMaxDurability() => _maxDurability;
    }
}
