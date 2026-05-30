using UnityEngine;

namespace ProjectWitchcraft.Entities
{
    // Base NPC entity. AI state machine and loot table added in Phase 3.
    public class NPC : Character
    {
        [SerializeField] private NPCDefinition _definition;
        public NPCDefinition Definition => _definition;

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
