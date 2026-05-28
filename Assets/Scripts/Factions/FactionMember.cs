using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Factions
{
    // Tracks which faction this entity belongs to and its current standing with all factions.
    // Standing changes at runtime as the entity takes actions (killing, trading, etc.).
    public class FactionMember : MonoBehaviour
    {
        [SerializeField] private FactionDefinition _faction;
        public FactionDefinition Faction => _faction;

        // Runtime standing overrides on top of the faction's defaults.
        private readonly Dictionary<FactionDefinition, float> _standingOverrides = new();

        public float GetStanding(FactionDefinition toward)
        {
            if (_standingOverrides.TryGetValue(toward, out float standing))
                return standing;
            return _faction != null ? _faction.GetDefaultRelation(toward) : 0f;
        }

        public void ModifyStanding(FactionDefinition toward, float delta)
        {
            float current = GetStanding(toward);
            _standingOverrides[toward] = Mathf.Clamp(current + delta, -1f, 1f);
        }

        public bool IsHostileTo(FactionDefinition toward) => GetStanding(toward) < -0.25f;
        public bool IsFriendlyTo(FactionDefinition toward) => GetStanding(toward) > 0.25f;
    }
}
