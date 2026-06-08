using UnityEngine;

namespace ProjectWitchcraft.Core
{
    [CreateAssetMenu(menuName = "Witchcraft/Combat/Attack Definition")]
    public class AttackDefinition : ScriptableObject
    {
        public enum HitShapeType { Box, Sphere, Capsule }

        [Header("Shape")]
        public HitShapeType ShapeType = HitShapeType.Box;
        public Vector3 HitBoxSize = new Vector3(1.5f, 1f, 1.5f);
        public float HitRadius = 1.2f;
        public Vector3 HitOffset = new Vector3(0f, 0f, 0.6f);
        [Range(0f, 360f)]
        public float ArcAngleDegrees = 110f;

        [Header("Damage")]
        public float DamageMultiplier = 1f;
        public DamageType DamageType = DamageType.Physical;

        [Header("Swing")]
        [Range(0f, 1f)]
        [Tooltip("Portion of the swing cycle spent winding up before any hit registers (0-1). A pre-hit telegraph delay; 0 = hits start instantly at swing start. Windup + Active must not exceed 1; the remainder is recovery. Scales with attack speed.")]
        public float WindupFraction = 0f;
        [Range(0f, 1f)]
        [Tooltip("Portion of the swing cycle that is the live hit window (0-1), starting after the windup. The cycle length comes from attack speed (weapon base APS x player AttackSpeed); this fraction also drives the VFX hold. Overlap is re-checked each frame over the window; each enemy is still hit once per swing.")]
        public float ActiveFraction = 0.4f;
        [Range(0f, 1f)]
        [Tooltip("Move speed multiplier while swinging (1 = no slow). Applied as a temporary multiplicative MoveSpeed debuff for the active window.")]
        public float MoveSpeedFractionWhileSwinging = 0.5f;

        [Header("Animation")]
        public string AnimationTrigger = "Attack";

        [Header("VFX")]
        public Color ArcColor = Color.white;
        public float ArcWidth = 0.05f;
        public float ArcFadeDuration = 0.25f;

        private void OnValidate()
        {
            // Windup + Active can't overrun the cycle; trim Active so the remainder stays as recovery.
            if (WindupFraction + ActiveFraction > 1f)
                ActiveFraction = 1f - WindupFraction;
        }
    }
}
