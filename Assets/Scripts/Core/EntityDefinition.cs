using UnityEngine;

namespace ProjectWitchcraft.Core
{
    // Save-system identity for a spawnable world entity (creatures, characters, plants, the dummy).
    // Holds the prefab to instantiate; resolved by AssetGuid through AssetRegistry on load.
    // The single definition type — per-entity data lives on the prefab's components for now.
    [CreateAssetMenu(fileName = "New Entity", menuName = "ProjectWitchcraft/Entity Definition")]
    public class EntityDefinition : GuidAsset
    {
        [Header("Identity")]
        [SerializeField] private GameObject _prefab;
        public GameObject Prefab => _prefab;
    }
}
