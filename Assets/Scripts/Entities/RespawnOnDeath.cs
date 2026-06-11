using System.Collections;
using UnityEngine;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.Entities
{
    // Capability component: when this entity dies, it comes back to full health in place after a
    // delay (instead of staying dead). Used by the target dummy; reusable for any practice/endless
    // entity. Orthogonal to EntityPersistencePolicy.RespawnInPlace, which is about save/load restore.
    [RequireComponent(typeof(HealthComponent))]
    public class RespawnOnDeath : MonoBehaviour
    {
        [SerializeField] private float _respawnDelay = 2f;

        private HealthComponent _health;

        private void Awake() => _health = GetComponent<HealthComponent>();

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
