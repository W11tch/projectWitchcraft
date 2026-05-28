using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Factions
{
    [CreateAssetMenu(fileName = "New Faction", menuName = "ProjectWitchcraft/Faction Definition")]
    public class FactionDefinition : ScriptableObject
    {
        [SerializeField] private string _factionName;
        [SerializeField] private Color _factionColor = Color.white;

        // Default standing toward other factions (-1 = hostile, 0 = neutral, 1 = allied).
        [SerializeField] private List<FactionRelation> _defaultRelations = new();

        public string FactionName => _factionName;
        public Color FactionColor => _factionColor;

        public float GetDefaultRelation(FactionDefinition other)
        {
            foreach (var rel in _defaultRelations)
                if (rel.faction == other) return rel.standing;
            return 0f; // neutral by default
        }

        [System.Serializable]
        public class FactionRelation
        {
            public FactionDefinition faction;
            [Range(-1f, 1f)] public float standing;
        }
    }
}
