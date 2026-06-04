using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.UI
{
    // Attach to the damage-numbers Canvas. That Canvas must be Screen Space - Overlay
    // with its GraphicRaycaster component deleted (not disabled — deleted).
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private FloatingDamageNumber _prefab;
        [SerializeField] private int _poolSize = 12;

        private readonly Queue<FloatingDamageNumber> _pool = new();

        private void Awake()
        {
            for (int i = 0; i < _poolSize; i++)
                CreatePooled();
        }

        private void OnEnable()  => EventManager.AddListener<DamagedEvent>(OnDamaged);
        private void OnDisable() => EventManager.RemoveListener<DamagedEvent>(OnDamaged);

        private void OnDamaged(DamagedEvent e) => Spawn(e.Hit.Damage, e.Hit.WorldPosition);

        private void Spawn(float damage, Vector3 worldPos)
        {
            FloatingDamageNumber n = _pool.Count > 0 ? _pool.Dequeue() : CreatePooled();
            n.Spawn(damage, worldPos);
        }

        private FloatingDamageNumber CreatePooled()
        {
            FloatingDamageNumber n = Instantiate(_prefab, transform);
            n.gameObject.SetActive(false);
            n.OnFinished = Return;
            _pool.Enqueue(n);
            return n;
        }

        private void Return(FloatingDamageNumber n) => _pool.Enqueue(n);
    }
}
