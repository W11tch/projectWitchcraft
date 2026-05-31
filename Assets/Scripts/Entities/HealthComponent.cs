using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Stats;

namespace ProjectWitchcraft.Entities
{
    // Owns health state for any entity. Max health is read live from CharacterStats
    // so buffs and debuffs apply automatically. Fires DamagedEvent / DiedEvent
    // through EventManager — VFX, UI, and AI react without direct coupling.
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private StatDefinition _healthStat;

        private CharacterStats _characterStats;

        // Always computed from stats — reflects any runtime buffs / debuffs.
        // Returns 0 if setup is incomplete so the broken state is visible in the display.
        public float MaxHealth => _characterStats != null && _healthStat != null
            ? _characterStats.GetStatValue(_healthStat)
            : 0f;

        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        private void Awake()
        {
            _characterStats = GetComponent<CharacterStats>();
            CurrentHealth = MaxHealth;

            if (_healthStat == null)
                Debug.LogError($"[HealthComponent] _healthStat not assigned on '{name}'. Drag a Health StatDefinition into the slot.", this);
            else if (CurrentHealth <= 0f)
                Debug.LogWarning($"[HealthComponent] MaxHealth is 0 on '{name}'. Add a base value for the Health stat in the CharacterStats component.", this);

            if (_characterStats != null)
                _characterStats.OnStatChanged += OnCharacterStatChanged;
        }

        private void OnDestroy()
        {
            if (_characterStats != null)
                _characterStats.OnStatChanged -= OnCharacterStatChanged;
        }

        private void OnCharacterStatChanged(StatDefinition stat, float newValue)
        {
            // No stat filter — MaxHealth is dynamic. Any stat change may have lowered
            // the cap, so always check regardless of which stat changed.
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;
        }

        public void TakeDamage(HitData hit)
        {
            if (!IsAlive) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - hit.Damage);

            EventManager.TriggerEvent(new DamagedEvent
            {
                Hit = hit,
                RemainingHealth = CurrentHealth,
                MaxHealth = MaxHealth,
                Target = transform
            });

            if (CurrentHealth <= 0f)
            {
                EventManager.TriggerEvent(new DiedEvent
                {
                    FinalBlow = hit,
                    Target = transform
                });
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            EventManager.TriggerEvent(new HealedEvent
            {
                NewHealth = CurrentHealth,
                MaxHealth = MaxHealth,
                Target = transform
            });
        }
    }
}
