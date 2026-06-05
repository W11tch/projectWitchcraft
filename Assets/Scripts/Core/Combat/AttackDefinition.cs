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

        [Header("Timing")]
        public float Cooldown = 0.5f;

        [Header("Animation")]
        public string AnimationTrigger = "Attack";

        [Header("VFX")]
        public Color ArcColor = Color.white;
        public float ArcWidth = 0.05f;
        public float ArcFadeDuration = 0.25f;
    }
}
