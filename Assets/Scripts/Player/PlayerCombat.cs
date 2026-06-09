using System;
using System.Collections;
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
        [SerializeField] private LayerMask _blockerLayer;
        [SerializeField] private StatDefinition _damageStatDefinition;
        [SerializeField] private StatDefinition _moveSpeedStat;
        [SerializeField] private StatDefinition _attackSpeedStat;
        [SerializeField] private Animator _animator;
        [Tooltip("Logs playerMult/rate/cycle each swing to verify the attack-speed stat is being read at runtime.")]
        [SerializeField] private bool _logAttackTiming;

        private PlayerController _controller;
        private CharacterStats _stats;
        private AttackVFX _attackVFX;
        private CharacterController _body;
        private float _nextAttackTime;
        private bool _warnedAttackSpeedZero;
        private Coroutine _swingRoutine;
        private readonly HashSet<Collider> _hitThisSwing = new();
        // Dedicated source token so the swing slow can be removed precisely without touching
        // any other modifiers this component might apply.
        private readonly object _swingSlowSource = new object();

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _stats = GetComponent<CharacterStats>();
            _attackVFX = GetComponent<AttackVFX>();
            _body = GetComponent<CharacterController>();
        }

        public void TryAttack()
        {
            if (Time.time < _nextAttackTime) return;

            WeaponItemData weapon = GetEquippedWeapon();
            AttackDefinition def = weapon != null ? weapon.AttackDefinition : null;
            if (def == null) return;

            // Cadence comes from attack speed: weapon's base attacks/sec scaled by the wielder's
            // AttackSpeed stat (a multiplier). The whole swing scales with the cycle — the active
            // hit window is a fraction of it, the rest is recovery before the next swing.
            float playerMult = _stats != null && _attackSpeedStat != null
                ? _stats.GetStatValue(_attackSpeedStat)
                : 1f;
            if (playerMult <= 0f)
            {
                // AttackSpeed is a multiplier (base 1.0 = 100%). A value of 0 means it's missing
                // from the player's CharacterStats.baseStats — percentage affixes then multiply
                // against 0 and do nothing. Warn once instead of silently masking the misconfig.
                if (!_warnedAttackSpeedZero)
                {
                    Debug.LogWarning("[Combat] AttackSpeed resolved to <= 0 — is it in the player's " +
                                     "CharacterStats.baseStats with value 1.0, and assigned to PlayerCombat._attackSpeedStat? " +
                                     "Falling back to 1.0.", this);
                    _warnedAttackSpeedZero = true;
                }
                playerMult = 1f;
            }

            float rate = Mathf.Max(0.01f, weapon.BaseAttacksPerSecond * playerMult);
            float cycle = 1f / rate;
            float windupDuration = cycle * def.WindupFraction;
            float activeWindow = cycle * def.ActiveFraction;

            _nextAttackTime = Time.time + cycle;

            if (_logAttackTiming)
                Debug.Log($"[Combat] attack timing — playerMult={playerMult:0.###} rate={rate:0.###}/s cycle={cycle:0.###}s (windup={windupDuration:0.###}s active={activeWindow:0.###}s)");

            if (_animator != null && !string.IsNullOrEmpty(def.AnimationTrigger))
                _animator.SetTrigger(def.AnimationTrigger);

            if (_swingRoutine != null) StopCoroutine(_swingRoutine);
            _swingRoutine = StartCoroutine(SwingRoutine(def, weapon.DamageType, windupDuration, activeWindow));
        }

        // Drives the attack over its active window: the overlap is re-checked each frame so the
        // hitbox tracks the player's live position, while committing to the facing locked at swing
        // start. Movement is slowed for the duration via a temporary MoveSpeed debuff. The window
        // length is derived from attack speed (see TryAttack).
        private IEnumerator SwingRoutine(AttackDefinition def, DamageType damageType, float windupDuration, float activeWindow)
        {
            _hitThisSwing.Clear();

            float baseDamage = _stats != null && _damageStatDefinition != null
                ? _stats.GetStatValue(_damageStatDefinition)
                : 10f;

            RemoveSwingSlow();
            ApplySwingSlow(def);

            // Lock the swing direction at the instant of the attack (committed through windup too).
            // The hitbox still tracks the player's live position over the window, but commits to this
            // facing (no mid-swing re-aim).
            Quaternion swingRotation = transform.rotation;

            // Windup: a pre-hit telegraph delay. No hits register and the VFX arc is held back so it
            // appears when hits actually land. The move-slow already applied keeps the player committed.
            if (windupDuration > 0f)
                yield return new WaitForSeconds(windupDuration);

            // AttackVFX lives on the player root, so it self-drives its pose (live position + this
            // locked rotation) for its whole life, fade included — no need to feed it each frame.
            _attackVFX?.Play(def, swingRotation, activeWindow);

            float elapsed = 0f;
            do
            {
                ResolveHits(def, baseDamage, damageType, swingRotation);
                elapsed += Time.deltaTime;
                yield return null;
            }
            while (elapsed < activeWindow);

            RemoveSwingSlow();
            _swingRoutine = null;
        }

        // Runs one frame's overlap, filters, and applies damage. The hit volume tracks the player's
        // live position but uses the locked swingRotation, so the swing follows movement without
        // re-aiming toward the mouse mid-swing.
        private void ResolveHits(AttackDefinition def, float baseDamage, DamageType damageType, Quaternion swingRotation)
        {
            Vector3 worldCenter  = transform.position + swingRotation * def.HitOffset;
            Vector3 swingForward = swingRotation * Vector3.forward;

            // Capsule = a forward thrust zone: horizontal, parallel to facing, length HitBoxSize.z,
            // thickness HitRadius. Endpoints computed once for both the debug flash and the overlap.
            float capHalf = Mathf.Max(0f, def.HitBoxSize.z * 0.5f - def.HitRadius);
            Vector3 capP0 = worldCenter - swingForward * capHalf;
            Vector3 capP1 = worldCenter + swingForward * capHalf;

            // Flash hit volume in Scene view this frame (duration 0 → redraws each frame, follows the swing).
            Color debugColor = new Color(1f, 0.2f, 0.2f);
            switch (def.ShapeType)
            {
                case AttackDefinition.HitShapeType.Box:
                    DrawBox(worldCenter, def.HitBoxSize * 0.5f, swingRotation, debugColor, 0f);
                    break;
                case AttackDefinition.HitShapeType.Capsule:
                    DrawSphere(capP0, def.HitRadius, debugColor, 0f);
                    DrawSphere(capP1, def.HitRadius, debugColor, 0f);
                    Debug.DrawLine(capP0, capP1, debugColor, 0f);
                    break;
                default:
                    DrawSphere(worldCenter, def.HitRadius, debugColor, 0f);
                    break;
            }

            if (def.ArcAngleDegrees < 360f)
            {
                float halfArc    = def.ArcAngleDegrees * 0.5f;
                float debugReach = def.ShapeType == AttackDefinition.HitShapeType.Box ? def.HitBoxSize.z : def.HitRadius;
                Debug.DrawRay(transform.position, Quaternion.Euler(0f, -halfArc, 0f) * swingForward * debugReach, Color.yellow, 0f);
                Debug.DrawRay(transform.position, Quaternion.Euler(0f,  halfArc, 0f) * swingForward * debugReach, Color.yellow, 0f);
            }

            Collider[] hits = def.ShapeType switch
            {
                AttackDefinition.HitShapeType.Box    => Physics.OverlapBox(worldCenter, def.HitBoxSize * 0.5f, swingRotation, _enemyLayer),
                AttackDefinition.HitShapeType.Sphere => Physics.OverlapSphere(worldCenter, def.HitRadius, _enemyLayer),
                AttackDefinition.HitShapeType.Capsule => Physics.OverlapCapsule(capP0, capP1, def.HitRadius, _enemyLayer),
                _ => Array.Empty<Collider>()
            };

            // LoS origin = player body center. Cast from here (never inside cover, since the
            // CharacterController collides with blocks) rather than the forward-pushed swing
            // center, which can sit inside a block the player is hugging.
            Vector3 losOrigin = _body != null ? _body.bounds.center : transform.position;

            foreach (Collider col in hits)
            {
                // _hitThisSwing persists across the whole window → each enemy is hit once per swing.
                if (!_hitThisSwing.Add(col)) continue;

                Vector3 toEnemy = col.transform.position - transform.position;
                toEnemy.y = 0f;

                if (def.ArcAngleDegrees < 360f &&
                    Vector3.Angle(swingForward, toEnemy.normalized) > def.ArcAngleDegrees * 0.5f)
                {
                    // Not in arc this frame; allow a later frame of the window to re-test it.
                    _hitThisSwing.Remove(col);
                    continue;
                }

                // Line-of-sight: skip enemies hidden behind a blocker (walls, blocks, chests, upper placeables).
                // Cast from the player body center to the enemy's collider center.
                bool blocked = Physics.Linecast(losOrigin, col.bounds.center, _blockerLayer);
                Debug.DrawLine(losOrigin, col.bounds.center, blocked ? Color.red : Color.green, 0f);
                if (blocked)
                {
                    // Blocked this frame; allow a later frame of the window to re-test it.
                    _hitThisSwing.Remove(col);
                    continue;
                }

                var damageable = col.GetComponent<IDamageable>();
                if (damageable == null) continue;

                damageable.TakeDamage(new HitData
                {
                    // baseDamage already resolves to weapon base (Override) + equipment flat/additive/
                    // multiplicative affixes — see EquipmentManager.ApplyModifiers / CharacterStats.
                    Damage        = baseDamage,
                    Type          = damageType,
                    Source        = this,
                    // Anchor floating damage numbers above the enemy's head (collider top), not its
                    // center. The damage number is the only reader of WorldPosition, so VFX are unaffected.
                    WorldPosition = new Vector3(col.bounds.center.x, col.bounds.max.y, col.bounds.center.z),
                    Direction     = toEnemy.sqrMagnitude > 0f ? toEnemy.normalized : swingForward
                });
            }
        }

        private void ApplySwingSlow(AttackDefinition def)
        {
            if (_stats == null || _moveSpeedStat == null) return;
            if (def.MoveSpeedFractionWhileSwinging >= 1f) return;

            _stats.AddBonuses(_swingSlowSource, new List<CharacterStats.StatBonus>
            {
                new CharacterStats.StatBonus
                {
                    Stat = _moveSpeedStat,
                    Modifier = new StatModifier(
                        def.MoveSpeedFractionWhileSwinging - 1f,
                        CalculationStage.Multiplicative,
                        _swingSlowSource)
                }
            });
        }

        private void RemoveSwingSlow()
        {
            _stats?.RemoveModifiersFromSource(_swingSlowSource);
        }

        private void OnDisable()
        {
            // A swing interrupted by death/disable must not leave the player permanently slowed.
            if (_swingRoutine != null)
            {
                StopCoroutine(_swingRoutine);
                _swingRoutine = null;
            }
            RemoveSwingSlow();
        }

        private WeaponItemData GetEquippedWeapon()
        {
            int index = _controller != null ? _controller.ActiveHotbarIndex : 0;
            var slots = InventoryManager.Instance?.HotbarSlots;
            if (slots == null || index >= slots.Count) return null;

            var slot = slots[index];
            if (slot.IsEmpty) return null;

            return slot.Instance.Definition as WeaponItemData;
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
