using UnityEngine;
using ProjectWitchcraft.Stats;
using ProjectWitchcraft.Factions;

namespace ProjectWitchcraft.Entities
{
    // Base for any living thing: player, enemy, boss.
    // Requires CharacterStats and FactionMember on the same GameObject.
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(FactionMember))]
    public abstract class Character : Entity
    {
        protected CharacterStats Stats { get; private set; }
        protected FactionMember Faction { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Stats = GetComponent<CharacterStats>();
            Faction = GetComponent<FactionMember>();
        }

        // Stubs — implemented fully in Phase 3 when HealthComponent is added.
        public virtual void TakeDamage(float amount, Entity source) { }
        public virtual void Die() { }
    }
}
