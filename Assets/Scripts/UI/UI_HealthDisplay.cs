using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Entities;
using ProjectWitchcraft.Stats;

namespace ProjectWitchcraft.UI
{
    // Reusable health display widget. Drop it anywhere in the UI hierarchy,
    // drag a HealthComponent into the Target slot, and it stays in sync.
    //
    // Poll Every Frame: tick for player HUD (1-2 instances) — always accurate, no event
    // ordering issues. Leave off for boss bars / NPC displays where event-based is preferred.
    public class UI_HealthDisplay : MonoBehaviour
    {
        [SerializeField] private HealthComponent _target;

        [Header("Text (optional)")]
        [SerializeField] private TextMeshProUGUI _currentMaxText;

        [Header("Fill bar (optional)")]
        [SerializeField] private Image _fillBar;

        [Header("Polling")]
        [Tooltip("Read health every frame. Use for the player HUD (1-2 instances). " +
                 "Leave off for boss bars or displays with many simultaneous instances.")]
        [SerializeField] private bool _pollEveryFrame;

        private CharacterStats _characterStats;

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            if (_pollEveryFrame) Refresh();
        }

        private void OnEnable()
        {
            if (!_pollEveryFrame)
            {
                EventManager.AddListener<DamagedEvent>(OnDamaged);
                EventManager.AddListener<DiedEvent>(OnDied);
                EventManager.AddListener<HealedEvent>(OnHealed);

                _characterStats = _target != null ? _target.GetComponent<CharacterStats>() : null;
                if (_characterStats != null)
                    _characterStats.OnStatChanged += OnAnyStatChanged;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (!_pollEveryFrame)
            {
                EventManager.RemoveListener<DamagedEvent>(OnDamaged);
                EventManager.RemoveListener<DiedEvent>(OnDied);
                EventManager.RemoveListener<HealedEvent>(OnHealed);

                if (_characterStats != null)
                    _characterStats.OnStatChanged -= OnAnyStatChanged;
            }
        }

        private void OnDamaged(DamagedEvent e)
        {
            if (_target == null || e.Target != _target.transform) return;
            Apply(e.RemainingHealth, e.MaxHealth);
        }

        private void OnDied(DiedEvent e)
        {
            if (_target == null || e.Target != _target.transform) return;
            Apply(0f, _target.MaxHealth);
        }

        private void OnHealed(HealedEvent e)
        {
            if (_target == null || e.Target != _target.transform) return;
            Apply(e.NewHealth, e.MaxHealth);
        }

        private void OnAnyStatChanged(StatDefinition stat, float newValue)
        {
            if (_target == null) return;
            Apply(_target.CurrentHealth, _target.MaxHealth);
        }

        private void Refresh()
        {
            if (_target == null) return;
            Apply(_target.CurrentHealth, _target.MaxHealth);
        }

        private void Apply(float current, float max)
        {
            if (_currentMaxText != null)
                _currentMaxText.text = $"{current:F0} / {max:F0}";

            if (_fillBar != null && max > 0f)
                _fillBar.fillAmount = current / max;
        }
    }
}
