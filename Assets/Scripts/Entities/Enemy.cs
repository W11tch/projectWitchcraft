using UnityEngine;

namespace ProjectWitchcraft.Entities
{
    // Base enemy entity. AI state machine and loot table added in Phase 3.
    public class Enemy : Character
    {
        [SerializeField] private EnemyDefinition _definition;
        public EnemyDefinition Definition => _definition;

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
