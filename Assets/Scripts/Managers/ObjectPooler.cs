// Located at: Assets/Scripts/Managers/ObjectPooler.cs
using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core; // Added to use IPoolableObject
using ProjectWitchcraft.BuildingSystem; // **FIX**: Added this using statement.

namespace ProjectWitchcraft.Managers
{
    public class ObjectPooler : Singleton<ObjectPooler>
    {
        [System.Serializable]
        public class Pool
        {
            // **FIX**: The manual 'tag' field has been removed.
            // The prefab's name will now be used as the tag automatically.
            public GameObject prefab;
            public int size;
            public bool allowGrowth = true;
        }

        public List<Pool> pools;
        private Dictionary<string, Queue<GameObject>> _poolDictionary;
        private Dictionary<string, Pool> _poolInfoDictionary;

        // --- MODIFIED: Awake is now an override to resolve the compiler warning ---
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// It overrides the base Singleton Awake method to set up the pool dictionaries.
        /// </summary>
        protected override void Awake()
        {
            // Call the base Singleton's Awake method to ensure the instance is set up correctly.
            base.Awake();

            _poolDictionary = new Dictionary<string, Queue<GameObject>>();
            _poolInfoDictionary = new Dictionary<string, Pool>();

            foreach (Pool pool in pools)
            {
                // **FIX**: Use the prefab's name as the unique tag for the pool.
                // This removes the need for a manual tag and prevents typos.
                if (pool.prefab == null)
                {
                    Debug.LogError("A pool in the ObjectPooler has a null prefab!", this);
                    continue;
                }
                string poolTag = pool.prefab.name;

                if (_poolDictionary.ContainsKey(poolTag))
                {
                    Debug.LogWarning($"Pool with tag '{poolTag}' already exists. Skipping.");
                    continue;
                }

                _poolInfoDictionary.Add(poolTag, pool);

                Queue<GameObject> objectPool = new Queue<GameObject>();
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = CreateNewObjectForPool(pool.prefab);
                    objectPool.Enqueue(obj);
                }
                _poolDictionary.Add(poolTag, objectPool);
            }
        }

        // --- REMOVED: The Start() method is no longer needed as the logic has been moved to Awake() ---

        private GameObject CreateNewObjectForPool(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            return obj;
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
                return null;
            }

            if (_poolDictionary[tag].Count == 0)
            {
                if (_poolInfoDictionary[tag].allowGrowth)
                {
                    Debug.Log($"Pool with tag '{tag}' is empty. Creating a new object.");
                    GameObject newObj = CreateNewObjectForPool(_poolInfoDictionary[tag].prefab);
                    _poolDictionary[tag].Enqueue(newObj);
                }
                else
                {
                    Debug.LogWarning($"Pool with tag {tag} is empty and not allowed to grow.");
                    return null;
                }
            }

            GameObject objectToSpawn = _poolDictionary[tag].Dequeue();

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            return objectToSpawn;
        }

        public void ReturnToPool(GameObject objectToReturn)
        {
            if (objectToReturn == null) return;

            // **THE FIX**: Instead of getting the generic interface, we get the specific PlaceableObject component.
            // This is a more direct and reliable way to find the script and its PoolTag.
            var poolable = objectToReturn.GetComponent<PlaceableObject>();

            if (poolable != null && _poolDictionary.ContainsKey(poolable.PoolTag))
            {
                _poolDictionary[poolable.PoolTag].Enqueue(objectToReturn);
                objectToReturn.SetActive(false);
            }
            else
            {
                // The warning message is now more descriptive to help with future debugging.
                string tagInfo = poolable != null ? $"with tag '{poolable.PoolTag}'" : "because it is missing the PlaceableObject component";
                Debug.LogWarning($"Object {objectToReturn.name} {tagInfo} could not be returned to a pool and will be destroyed instead.");
                Destroy(objectToReturn);
            }
        }
    }
}