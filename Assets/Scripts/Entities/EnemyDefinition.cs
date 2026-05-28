using UnityEngine;
using ProjectWitchcraft.Factions;

namespace ProjectWitchcraft.Entities
{
    [CreateAssetMenu(fileName = "New Enemy", menuName = "ProjectWitchcraft/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _enemyName;
        [SerializeField] private FactionDefinition _faction;

        [Header("Awareness")]
        [SerializeField] private float _visionRange = 8f;
        [SerializeField] private float _visionAngle = 90f;
        [SerializeField] private float _hearingRange = 4f;

        [Header("Behaviour Weights")]
        [Range(0f, 1f)] [SerializeField] private float _aggressionWeight = 0.5f;

        public string EnemyName => _enemyName;
        public FactionDefinition Faction => _faction;
        public float VisionRange => _visionRange;
        public float VisionAngle => _visionAngle;
        public float HearingRange => _hearingRange;
        public float AggressionWeight => _aggressionWeight;
    }
}
