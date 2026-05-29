using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Core
{
    [CreateAssetMenu(fileName = "New Tool", menuName = "ProjectWitchcraft/Equipment/Tool")]
    public class ToolItemData : ItemData
    {
        [Header("Tool")]
        [SerializeField] private int _maxDurability = 100;
        [SerializeField] private List<EquipmentAffix> _affixes = new();

        public int MaxDurability => _maxDurability;
        public IReadOnlyList<EquipmentAffix> Affixes => _affixes;

        public override float GetMaxDurability() => _maxDurability;
    }
}
