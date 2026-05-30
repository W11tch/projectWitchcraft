using System.Collections.Generic;
using UnityEngine;

namespace ProjectWitchcraft.Entities
{
    // Boss entity — multi-phase NPC. Phase logic added in Phase 4.
    public class Boss : NPC
    {
        [SerializeField] private List<BossPhaseDefinition> _phases = new();
        public IReadOnlyList<BossPhaseDefinition> Phases => _phases;

        private int _currentPhaseIndex;
        public int CurrentPhaseIndex => _currentPhaseIndex;
    }
}
