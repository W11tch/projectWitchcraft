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
        private AttackDefinition _def;
        // Facing is locked at swing start; position is read live from this transform each frame
        // (AttackVFX is on the player root) so the arc follows the player for its whole life.
        private Quaternion _lockedRotation;

        private void Awake()
        {
            _line = gameObject.AddComponent<LineRenderer>();
            // World space + an explicit pose driven each frame by PlayerCombat. This lets the arc
            // follow the player's position while keeping the direction locked to swing start.
            _line.useWorldSpace = true;
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _line.receiveShadows = false;
            if (_lineMaterial != null)
                _line.material = new Material(_lineMaterial);
            _materialInstance = _line.material;
            _line.enabled = false;
        }

        public void Play(AttackDefinition def, Quaternion rotation, float holdDuration)
        {
            StopAllCoroutines();

            _def = def;
            _lockedRotation = rotation;
            Redraw(transform.position, _lockedRotation);

            _line.startWidth = def.ArcWidth;
            _line.endWidth   = def.ArcWidth;
            ApplyColor(def.ArcColor);
            _line.enabled = true;

            StartCoroutine(HoldThenFade(def.ArcColor, holdDuration, def.ArcFadeDuration));
        }

        private void Redraw(Vector3 position, Quaternion rotation)
        {
            if (_def.ShapeType == AttackDefinition.HitShapeType.Box)
                DrawBox(position, rotation, _def);
            else if (_def.ShapeType == AttackDefinition.HitShapeType.Capsule)
                DrawCapsule(position, rotation, _def);
            else
                DrawArc(position, rotation, _def);
        }

        private void ApplyColor(Color c)
        {
            _line.startColor = c;
            _line.endColor   = c;
            _materialInstance.color = c;
        }

        // Draws the exact XZ footprint of the OverlapBox in world space, at the given pose.
        private void DrawBox(Vector3 position, Quaternion rotation, AttackDefinition def)
        {
            Vector3 o = def.HitOffset;
            Vector3 h = def.HitBoxSize * 0.5f;

            Vector3 nearLeft  = position + rotation * (o + new Vector3(-h.x, 0f, -h.z));
            Vector3 nearRight = position + rotation * (o + new Vector3( h.x, 0f, -h.z));
            Vector3 farLeft   = position + rotation * (o + new Vector3(-h.x, 0f,  h.z));
            Vector3 farRight  = position + rotation * (o + new Vector3( h.x, 0f,  h.z));

            _line.positionCount = 5;
            _line.SetPosition(0, nearLeft);
            _line.SetPosition(1, farLeft);
            _line.SetPosition(2, farRight);
            _line.SetPosition(3, nearRight);
            _line.SetPosition(4, nearLeft);
        }

        // Draws the true stadium (capsule) footprint in world space, matching the physics capsule:
        // cap centers at ±half along forward, radius = HitRadius. half = max(0, HitBoxSize.z/2 - r),
        // so the total length equals HitBoxSize.z and the ends are rounded, not square.
        private void DrawCapsule(Vector3 position, Quaternion rotation, AttackDefinition def)
        {
            Vector3 o = def.HitOffset;
            float r    = def.HitRadius;
            float half = Mathf.Max(0f, def.HitBoxSize.z * 0.5f - r);

            int segPerCap = Mathf.Max(2, _segments / 2);
            _line.positionCount = (segPerCap + 1) * 2 + 1;

            int idx = 0;
            // Far cap (+z): sweep left edge → front tip → right edge.
            for (int i = 0; i <= segPerCap; i++)
            {
                float a = Mathf.Lerp(-90f, 90f, i / (float)segPerCap) * Mathf.Deg2Rad;
                Vector3 local = new Vector3(r * Mathf.Sin(a), 0f, half + r * Mathf.Cos(a));
                _line.SetPosition(idx++, position + rotation * (o + local));
            }
            // Near cap (-z): sweep right edge → back tip → left edge. The gaps between the caps are
            // the straight sides, drawn implicitly as the lines connecting consecutive points.
            for (int i = 0; i <= segPerCap; i++)
            {
                float b = Mathf.Lerp(90f, -90f, i / (float)segPerCap) * Mathf.Deg2Rad;
                Vector3 local = new Vector3(r * Mathf.Sin(b), 0f, -half - r * Mathf.Cos(b));
                _line.SetPosition(idx++, position + rotation * (o + local));
            }
            // Close the loop back to the first point (far cap, left edge).
            Vector3 firstLocal = new Vector3(-r, 0f, half);
            _line.SetPosition(idx, position + rotation * (o + firstLocal));
        }

        // Draws the sector boundary in world space at the given pose:
        // origin → left edge → arc → right edge → origin.
        private void DrawArc(Vector3 position, Quaternion rotation, AttackDefinition def)
        {
            Vector3 origin  = position + rotation * def.HitOffset;
            Vector3 forward = rotation * Vector3.forward;
            float halfArc   = def.ArcAngleDegrees * 0.5f;
            float radius    = def.HitRadius;
            bool fullCircle = def.ArcAngleDegrees >= 360f;

            if (fullCircle)
            {
                _line.positionCount = _segments + 1;
                for (int i = 0; i <= _segments; i++)
                {
                    float angle = i / (float)_segments * 360f;
                    Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward;
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
                    Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward;
                    _line.SetPosition(i + 1, origin + dir * radius);
                }
                _line.SetPosition(_segments + 2, origin);
            }
        }

        // Holds the arc at full opacity for the active swing window, then fades it out. The arc is
        // redrawn at the player's live position (with the locked rotation) every frame of both
        // phases, so it stays attached to the player while fading instead of freezing in the world.
        private IEnumerator HoldThenFade(Color baseColor, float holdDuration, float fadeDuration)
        {
            ApplyColor(baseColor);

            float held = 0f;
            while (held < holdDuration)
            {
                Redraw(transform.position, _lockedRotation);
                held += Time.deltaTime;
                yield return null;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                Redraw(transform.position, _lockedRotation);
                float a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                ApplyColor(new Color(baseColor.r, baseColor.g, baseColor.b, a));
                elapsed += Time.deltaTime;
                yield return null;
            }
            _line.enabled = false;
        }
    }
}
