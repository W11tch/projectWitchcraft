// Located at: Assets/Scripts/Entities/HitFlash.cs
using System.Collections;
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.Entities
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private float _flashDuration = 0.12f;
        [SerializeField] private float _bounceHeight = 0.12f;
        [SerializeField] private float _bounceDuration = 0.15f;

        private SpriteRenderer _spriteRenderer;
        private Transform _entityRoot;
        private Vector3 _originalLocalPosition;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalLocalPosition = transform.localPosition;
            _entityRoot = transform.parent;
        }

        private void OnEnable()  => EventManager.AddListener<DamagedEvent>(OnDamaged);
        private void OnDisable() => EventManager.RemoveListener<DamagedEvent>(OnDamaged);

        private void OnDamaged(DamagedEvent e)
        {
            if (_entityRoot == null || e.Target != _entityRoot) return;
            StopAllCoroutines();
            transform.localPosition = _originalLocalPosition;
            _spriteRenderer.color = Color.white;
            StartCoroutine(FlashCoroutine());
            StartCoroutine(BounceCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            _spriteRenderer.color = _flashColor;
            float elapsed = 0f;
            while (elapsed < _flashDuration)
            {
                _spriteRenderer.color = Color.Lerp(_flashColor, Color.white, elapsed / _flashDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _spriteRenderer.color = Color.white;
        }

        private IEnumerator BounceCoroutine()
        {
            float half = _bounceDuration * 0.5f;
            float elapsed = 0f;
            while (elapsed < half)
            {
                float y = Mathf.Lerp(0f, _bounceHeight, elapsed / half);
                transform.localPosition = _originalLocalPosition + new Vector3(0f, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < half)
            {
                float y = Mathf.Lerp(_bounceHeight, 0f, elapsed / half);
                transform.localPosition = _originalLocalPosition + new Vector3(0f, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = _originalLocalPosition;
        }
    }
}
