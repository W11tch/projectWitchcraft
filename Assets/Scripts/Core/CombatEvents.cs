using UnityEngine;

namespace ProjectWitchcraft.Core
{
    public struct DamagedEvent
    {
        public HitData Hit;
        public float RemainingHealth;
        public float MaxHealth;
        public Transform Target;
    }

    public struct DiedEvent
    {
        public HitData FinalBlow;
        public Transform Target;
    }

    public struct HealedEvent
    {
        public float NewHealth;
        public float MaxHealth;
        public Transform Target;
    }
}
