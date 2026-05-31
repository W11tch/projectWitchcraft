using UnityEngine;

namespace ProjectWitchcraft.Core
{
    public enum DamageType { Physical }

    public struct HitData
    {
        public float Damage;
        public DamageType Type;
        public object Source;         // item, spell, or any object that caused the hit
        public Vector3 WorldPosition; // impact point, used by VFX/floating numbers
        public Vector3 Direction;     // normalized direction of impact, used by stagger
    }
}
