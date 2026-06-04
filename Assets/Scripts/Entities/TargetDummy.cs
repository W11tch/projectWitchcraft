using System.Collections;
using UnityEngine;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.Entities
{
    public class TargetDummy : NPC
    {
        [SerializeField] private float _respawnDelay = 2f;

        private HealthComponent _health;

        protected override void Awake()
        {
            base.Awake();
            _health = GetComponent<HealthComponent>();
        }

        private void OnEnable()  => EventManager.AddListener<DiedEvent>(OnDied);
        private void OnDisable() => EventManager.RemoveListener<DiedEvent>(OnDied);

        private void OnDied(DiedEvent e)
        {
            if (e.Target != transform) return;
            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(_respawnDelay);
            if (_health != null)
                _health.ResetHealth();
        }
    }
}
