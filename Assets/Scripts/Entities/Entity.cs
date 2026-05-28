using UnityEngine;

namespace ProjectWitchcraft.Entities
{
    // Base for everything that lives in the world and needs a persistent identity.
    public abstract class Entity : MonoBehaviour
    {
        [SerializeField] private string _uniqueId;
        public string UniqueId => _uniqueId;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(_uniqueId))
                _uniqueId = System.Guid.NewGuid().ToString();
        }
    }
}
