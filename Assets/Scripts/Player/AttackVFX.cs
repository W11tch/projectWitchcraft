// Located at: Assets/Scripts/Player/AttackVFX.cs
using System.Collections;
using UnityEngine;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.Player
{
    public class AttackVFX : MonoBehaviour
    {
        [SerializeField] private int _segments = 16;
        // Assign a URP Unlit or Sprites/Default material with transparency support.
        [SerializeField] private Material _lineMaterial;

        private LineRenderer _line;
        private Material _materialInstance;

        private void Awake()
        {
            _line = gameObject.AddComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _line.receiveShadows = false;
            if (_lineMaterial != null)
                _line.material = new Material(_lineMaterial);
            _materialInstance = _line.material;
            _line.enabled = false;
        }

        public void Play(Transform attacker, AttackDefinition def)
        {
            StopAllCoroutines();

            if (def.ShapeType == AttackDefinition.HitShapeType.Box)
                DrawBox(attacker, def);
            else
                DrawArc(attacker, def);

            _line.startWidth = def.ArcWidth;
            _line.endWidth   = def.ArcWidth;
            ApplyColor(def.ArcColor);
            _line.enabled = true;

            StartCoroutine(FadeOut(def.ArcColor, def.ArcFadeDuration));
        }

        private void ApplyColor(Color c)
        {
            _line.startColor = c;
            _line.endColor   = c;
            _materialInstance.color = c;
        }

        // Draws the exact XZ footprint of the OverlapBox.
        private void DrawBox(Transform attacker, AttackDefinition def)
        {
            Vector3 o = def.HitOffset;
            Vector3 h = def.HitBoxSize * 0.5f;

            Vector3 nearLeft  = attacker.TransformPoint(o + new Vector3(-h.x, 0f, -h.z));
            Vector3 nearRight = attacker.TransformPoint(o + new Vector3( h.x, 0f, -h.z));
            Vector3 farLeft   = attacker.TransformPoint(o + new Vector3(-h.x, 0f,  h.z));
            Vector3 farRight  = attacker.TransformPoint(o + new Vector3( h.x, 0f,  h.z));

            _line.positionCount = 5;
            _line.SetPosition(0, nearLeft);
            _line.SetPosition(1, farLeft);
            _line.SetPosition(2, farRight);
            _line.SetPosition(3, nearRight);
            _line.SetPosition(4, nearLeft);
        }

        // Draws the sector boundary: origin → left edge → arc → right edge → origin.
        private void DrawArc(Transform attacker, AttackDefinition def)
        {
            Vector3 origin  = attacker.TransformPoint(def.HitOffset);
            float halfArc   = def.ArcAngleDegrees * 0.5f;
            float radius    = def.HitRadius;
            bool fullCircle = def.ArcAngleDegrees >= 360f;

            if (fullCircle)
            {
                _line.positionCount = _segments + 1;
                for (int i = 0; i <= _segments; i++)
                {
                    float angle = i / (float)_segments * 360f;
                    Vector3 dir = Quaternion.Euler(0f, angle, 0f) * attacker.forward;
                    _line.SetPosition(i, origin + dir * radius);
                }
            }
            else
            {
                // Sector: origin, arc points, back to origin.
                _line.positionCount = _segments + 3;
                _line.SetPosition(0, origin);
                for (int i = 0; i <= _segments; i++)
                {
                    float t     = i / (float)_segments;
                    float angle = Mathf.Lerp(-halfArc, halfArc, t);
                    Vector3 dir = Quaternion.Euler(0f, angle, 0f) * attacker.forward;
                    _line.SetPosition(i + 1, origin + dir * radius);
                }
                _line.SetPosition(_segments + 2, origin);
            }
        }

        private IEnumerator FadeOut(Color baseColor, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float a = Mathf.Lerp(1f, 0f, elapsed / duration);
                ApplyColor(new Color(baseColor.r, baseColor.g, baseColor.b, a));
                elapsed += Time.deltaTime;
                yield return null;
            }
            _line.enabled = false;
        }
    }
}
