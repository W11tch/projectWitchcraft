using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Stats;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private StatDefinition _damageStatDefinition;
        [SerializeField] private Animator _animator;

        private PlayerController _controller;
        private CharacterStats _stats;
        private AttackVFX _attackVFX;
        private float _nextAttackTime;
        private readonly HashSet<Collider> _hitThisSwing = new();

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _stats = GetComponent<CharacterStats>();
            _attackVFX = GetComponent<AttackVFX>();
        }

        public void TryAttack()
        {
            if (Time.time < _nextAttackTime) return;

            AttackDefinition def = GetEquippedWeaponDefinition();
            if (def == null) return;

            _nextAttackTime = Time.time + def.Cooldown;

            if (_animator != null && !string.IsNullOrEmpty(def.AnimationTrigger))
                _animator.SetTrigger(def.AnimationTrigger);

            Vector3 worldCenter = transform.TransformPoint(def.HitOffset);

            // Flash hit volume in Scene view for 0.4s
            Color debugColor = new Color(1f, 0.2f, 0.2f);
            if (def.ShapeType == AttackDefinition.HitShapeType.Box)
                DrawBox(worldCenter, def.HitBoxSize * 0.5f, transform.rotation, debugColor, 0.4f);
            else
                DrawSphere(worldCenter, def.HitRadius, debugColor, 0.4f);

            if (def.ArcAngleDegrees < 360f)
            {
                float halfArc    = def.ArcAngleDegrees * 0.5f;
                float debugReach = def.ShapeType == AttackDefinition.HitShapeType.Box ? def.HitBoxSize.z : def.HitRadius;
                Debug.DrawRay(transform.position, Quaternion.Euler(0f, -halfArc, 0f) * transform.forward * debugReach, Color.yellow, 0.4f);
                Debug.DrawRay(transform.position, Quaternion.Euler(0f,  halfArc, 0f) * transform.forward * debugReach, Color.yellow, 0.4f);
            }

            Collider[] hits = def.ShapeType switch
            {
                AttackDefinition.HitShapeType.Box    => Physics.OverlapBox(worldCenter, def.HitBoxSize * 0.5f, transform.rotation, _enemyLayer),
                AttackDefinition.HitShapeType.Sphere => Physics.OverlapSphere(worldCenter, def.HitRadius, _enemyLayer),
                AttackDefinition.HitShapeType.Capsule => Physics.OverlapCapsule(
                    worldCenter - Vector3.up * (def.HitRadius * 0.5f),
                    worldCenter + Vector3.up * (def.HitRadius * 0.5f),
                    def.HitRadius, _enemyLayer),
                _ => Array.Empty<Collider>()
            };

            float baseDamage = _stats != null && _damageStatDefinition != null
                ? _stats.GetStatValue(_damageStatDefinition)
                : 10f;

            _hitThisSwing.Clear();

            foreach (Collider col in hits)
            {
                if (!_hitThisSwing.Add(col)) continue;

                Vector3 toEnemy = col.transform.position - transform.position;
                toEnemy.y = 0f;

                if (def.ArcAngleDegrees < 360f &&
                    Vector3.Angle(transform.forward, toEnemy.normalized) > def.ArcAngleDegrees * 0.5f)
                    continue;

                var damageable = col.GetComponent<IDamageable>();
                if (damageable == null) continue;

                damageable.TakeDamage(new HitData
                {
                    Damage        = baseDamage * def.DamageMultiplier,
                    Type          = def.DamageType,
                    Source        = this,
                    WorldPosition = col.bounds.center,
                    Direction     = toEnemy.sqrMagnitude > 0f ? toEnemy.normalized : transform.forward
                });
            }

            _attackVFX?.Play(transform, def);
        }

        private AttackDefinition GetEquippedWeaponDefinition()
        {
            int index = _controller != null ? _controller.ActiveHotbarIndex : 0;
            var slots = InventoryManager.Instance?.HotbarSlots;
            if (slots == null || index >= slots.Count) return null;

            var slot = slots[index];
            if (slot.IsEmpty) return null;

            return (slot.Instance.Definition as WeaponItemData)?.AttackDefinition;
        }

        private static void DrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color color, float duration)
        {
            Vector3 e = halfExtents;
            Vector3[] c = new Vector3[8];
            c[0] = center + rotation * new Vector3(-e.x, -e.y, -e.z);
            c[1] = center + rotation * new Vector3( e.x, -e.y, -e.z);
            c[2] = center + rotation * new Vector3( e.x, -e.y,  e.z);
            c[3] = center + rotation * new Vector3(-e.x, -e.y,  e.z);
            c[4] = center + rotation * new Vector3(-e.x,  e.y, -e.z);
            c[5] = center + rotation * new Vector3( e.x,  e.y, -e.z);
            c[6] = center + rotation * new Vector3( e.x,  e.y,  e.z);
            c[7] = center + rotation * new Vector3(-e.x,  e.y,  e.z);
            Debug.DrawLine(c[0], c[1], color, duration); Debug.DrawLine(c[1], c[2], color, duration);
            Debug.DrawLine(c[2], c[3], color, duration); Debug.DrawLine(c[3], c[0], color, duration);
            Debug.DrawLine(c[4], c[5], color, duration); Debug.DrawLine(c[5], c[6], color, duration);
            Debug.DrawLine(c[6], c[7], color, duration); Debug.DrawLine(c[7], c[4], color, duration);
            Debug.DrawLine(c[0], c[4], color, duration); Debug.DrawLine(c[1], c[5], color, duration);
            Debug.DrawLine(c[2], c[6], color, duration); Debug.DrawLine(c[3], c[7], color, duration);
        }

        private static void DrawSphere(Vector3 center, float radius, Color color, float duration)
        {
            const int seg = 16;
            for (int i = 0; i < seg; i++)
            {
                float a0 = i       / (float)seg * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)seg * Mathf.PI * 2f;
                Debug.DrawLine(
                    center + new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f) * radius,
                    center + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f) * radius,
                    color, duration);
                Debug.DrawLine(
                    center + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius,
                    center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius,
                    color, duration);
            }
        }
    }
}
