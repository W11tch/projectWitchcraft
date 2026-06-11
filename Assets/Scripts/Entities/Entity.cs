using UnityEngine;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.Entities
{
    // The single concrete type for every non-player thing that lives in the world:
    // creatures, characters, plants, props, the target dummy. Capabilities (health, faction,
    // AI, growth, respawn, ...) are added as sibling components — never as subclasses.
    //
    // Players are NOT entities (own category, driven by input/networking). Chests are placed
    // objects (BuildingSystem) and items are ItemData — neither carry this component.
    public class Entity : MonoBehaviour
    {
        [SerializeField] private string _uniqueId;
        public string UniqueId => _uniqueId;

        [Header("Persistence")]
        [SerializeField] private EntityDefinition _definition;
        public EntityDefinition Definition => _definition;

        [SerializeField] private EntityPersistencePolicy _persistencePolicy = EntityPersistencePolicy.Persistent;
        public EntityPersistencePolicy PersistencePolicy => _persistencePolicy;

        // Set when an entity is rebuilt from save data so its identity carries across loads.
        public void SetUniqueId(string id) => _uniqueId = id;

        private void Awake()
        {
            if (string.IsNullOrEmpty(_uniqueId))
                _uniqueId = System.Guid.NewGuid().ToString();
        }

        private void OnEnable()
        {
            EntityManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            // Instance may already be torn down during application/scene shutdown.
            EntityManager.InstanceIfExists?.Unregister(this);
        }
    }
}
