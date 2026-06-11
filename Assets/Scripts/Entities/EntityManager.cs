using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.Entities
{
    // Central lifetime owner for spawnable world entities (NPCs/creatures/target dummies).
    // The single seam for spawning and despawning — pooling can later replace the bodies of
    // Spawn/Despawn without touching call sites. Drives its own save/load via the SaveData
    // events so SaveManager (in Managers) never has to reference the Entities assembly.
    //
    // The Player is NOT an Entity (its own category), so it never registers here and is never
    // cleared on load.
    //
    // No input/UI dependencies live here (the Entities assembly doesn't reference Unity.InputSystem):
    // the debug kill-mode raycast lives in DebugToolsUI and calls Despawn.
    public class EntityManager : Singleton<EntityManager>
    {
        [Header("Dependencies")]
        [SerializeField] private AssetRegistry _assetRegistry;
        [SerializeField] private Transform _entitiesParent;

        [Header("Ground Settle")]
        [Tooltip("Layers an entity settles onto when spawned with settleOnGround (ground + building blocks).")]
        [SerializeField] private LayerMask _groundLayer;
        [Tooltip("How far above the spawn point to start the downward settle raycast.")]
        [SerializeField] private float _settleCastHeight = 50f;

        // How many entities to rebuild per frame on load, mirroring ChunkManager's restore.
        private const int EntitiesPerFrame = 20;

        private readonly HashSet<Entity> _live = new();

        // Captured on ApplySaveData, consumed on GameLoadedEvent (after terrain is restored).
        private List<EntitySaveData> _pendingEntities;

        private void OnEnable()
        {
            EventManager.AddListener<GatherSaveDataEvent>(OnGatherSaveData);
            EventManager.AddListener<ApplySaveDataEvent>(OnApplySaveData);
            EventManager.AddListener<GameLoadedEvent>(OnGameLoaded);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<GatherSaveDataEvent>(OnGatherSaveData);
            EventManager.RemoveListener<ApplySaveDataEvent>(OnApplySaveData);
            EventManager.RemoveListener<GameLoadedEvent>(OnGameLoaded);
        }

        // --- Registry (called by Entity.OnEnable/OnDisable) ---

        public void Register(Entity entity)
        {
            if (entity != null) _live.Add(entity);
        }

        public void Unregister(Entity entity)
        {
            if (entity != null) _live.Remove(entity);
        }

        // --- Spawn / Despawn seam ---

        public Entity Spawn(EntityDefinition definition, Vector3 position, Quaternion rotation, bool settleOnGround = false)
        {
            if (definition == null) { Debug.LogError("[EntityManager] Spawn called with null definition."); return null; }
            if (definition.Prefab == null) { Debug.LogError($"[EntityManager] Definition '{definition.name}' has no prefab."); return null; }

            // Resolve ground height BEFORE instantiating so the raycast can't hit the new entity.
            if (settleOnGround) position = SettleOnGround(position);

            // Instantiate/Destroy for now; pooling is a later optimization behind this same seam.
            GameObject go = Instantiate(definition.Prefab, position, rotation);
            if (_entitiesParent != null) go.transform.SetParent(_entitiesParent);

            var entity = go.GetComponent<Entity>();
            if (entity == null)
                Debug.LogError($"[EntityManager] Prefab '{definition.Prefab.name}' has no Entity component.", go);
            // Registration happens in Entity.OnEnable.
            return entity;
        }

        // Snaps the Y of a position down onto the ground/block surface. Returns the input unchanged
        // if nothing is hit (e.g. spawned over a void).
        private Vector3 SettleOnGround(Vector3 position)
        {
            Vector3 origin = position + Vector3.up * _settleCastHeight;
            if (Physics.Raycast(origin, Vector3.down, out var hit, _settleCastHeight * 2f, _groundLayer))
                position.y = hit.point.y;
            return position;
        }

        public void Despawn(Entity entity)
        {
            if (entity == null) return;
            // OnDisable auto-unregisters. Destroy for now; return-to-pool later.
            Destroy(entity.gameObject);
        }

        private void DespawnAll()
        {
            // Copy first — Despawn triggers OnDisable which mutates _live.
            var snapshot = new List<Entity>(_live);
            foreach (var entity in snapshot)
                Despawn(entity);
            _live.Clear();
        }

        // --- Save / Load ---

        private void OnGatherSaveData(GatherSaveDataEvent e)
        {
            e.SaveData.entities.Clear();
            foreach (var entity in _live)
            {
                if (entity == null || entity.PersistencePolicy == EntityPersistencePolicy.Transient) continue;
                if (entity.Definition == null)
                {
                    Debug.LogWarning($"[EntityManager] '{entity.name}' has no EntityDefinition; skipping save.", entity);
                    continue;
                }

                var data = new EntitySaveData
                {
                    definitionGuid = entity.Definition.AssetGuid,
                    uniqueId = entity.UniqueId,
                    Position = entity.transform.position,
                    Rotation = entity.transform.rotation,
                };

                var health = entity.GetComponent<HealthComponent>();
                if (health != null) data.currentHealth = health.CurrentHealth;

                e.SaveData.entities.Add(data);
            }
        }

        // Only capture here — terrain isn't restored yet. Actual rebuild happens on GameLoadedEvent.
        private void OnApplySaveData(ApplySaveDataEvent e)
        {
            _pendingEntities = e.SaveData.entities;
        }

        // Fired after ChunkManager.RestoreObjectsCoroutine, so the ground exists and entities settle.
        private void OnGameLoaded(GameLoadedEvent e)
        {
            var pending = _pendingEntities;
            _pendingEntities = null;
            StartCoroutine(RestoreEntitiesCoroutine(pending));
        }

        private IEnumerator RestoreEntitiesCoroutine(List<EntitySaveData> entities)
        {
            DespawnAll();

            if (entities == null) yield break;
            if (_assetRegistry == null) { Debug.LogError("[EntityManager] AssetRegistry not assigned."); yield break; }

            int spawned = 0;
            foreach (var data in entities)
            {
                var definition = _assetRegistry.GetEntityByGuid(data.definitionGuid);
                if (definition == null)
                {
                    Debug.LogWarning($"[EntityManager] No EntityDefinition for GUID '{data.definitionGuid}'; skipping.");
                    continue;
                }

                // Saved transform is authoritative — terrain is deterministic so the saved Y already
                // matches the restored ground. (Settle is for spawner/biome entities with no saved Y.)
                var entity = Spawn(definition, data.Position, data.Rotation);
                if (entity == null) continue;

                entity.SetUniqueId(data.uniqueId);

                // Persistent entities restore their saved health; RespawnInPlace keeps full health.
                if (entity.PersistencePolicy == EntityPersistencePolicy.Persistent)
                    entity.GetComponent<HealthComponent>()?.RestoreHealth(data.currentHealth);

                spawned++;
                if (spawned % EntitiesPerFrame == 0)
                    yield return null;
            }
        }
    }
}
