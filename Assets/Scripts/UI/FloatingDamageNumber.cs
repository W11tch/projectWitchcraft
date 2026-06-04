using System.Collections;
using UnityEngine;
using TMPro;

namespace ProjectWitchcraft.UI
{
    public class FloatingDamageNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        private RectTransform _rect;
        public System.Action<FloatingDamageNumber> OnFinished;

        private void Awake() => _rect = GetComponent<RectTransform>();

        public void Spawn(float damage, Vector3 worldPos)
        {
            _text.text = Mathf.RoundToInt(damage).ToString();
            _text.color = Color.white;
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FloatAndFade(worldPos));
        }

        private IEnumerator FloatAndFade(Vector3 worldPos)
        {
            float elapsed  = 0f;
            float duration = 0.8f;
            float risePixels = 55f;
            float xOffset  = Random.Range(-14f, 14f);
            Camera cam     = Camera.main;

            while (elapsed < duration)
            {
                float t = elapsed / duration;

                Vector2 screen = cam.WorldToScreenPoint(worldPos);
                screen.x += xOffset;
                screen.y += Mathf.Lerp(0f, risePixels, t);
                _rect.position = screen;

                Color c = _text.color;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                _text.color = c;

                elapsed += Time.deltaTime;
                yield return null;
            }

            gameObject.SetActive(false);
            OnFinished?.Invoke(this);
        }
    }
}
