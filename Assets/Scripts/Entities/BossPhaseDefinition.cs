using UnityEngine;

namespace ProjectWitchcraft.Entities
{
    [CreateAssetMenu(fileName = "New Boss Phase", menuName = "ProjectWitchcraft/Boss Phase Definition")]
    public class BossPhaseDefinition : ScriptableObject
    {
        [Tooltip("Transition into this phase when HP drops below this fraction (0–1).")]
        [Range(0f, 1f)] [SerializeField] private float _healthThreshold = 0.5f;

        public float HealthThreshold => _healthThreshold;
        // Attack patterns and VFX overrides added in Phase 4.
    }
}
