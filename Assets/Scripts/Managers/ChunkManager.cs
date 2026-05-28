using System.Collections.Generic;
using UnityEngine;
using ProjectWitchcraft.BuildingSystem;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.Managers
{
    public class ChunkManager : Singleton<ChunkManager>
    {
        [Header("Configuration")]
        [SerializeField] private float cellSize = 1f;
        public float CellSize => cellSize;
        public const int ChunkSize = Chunk.Size;

        [Header("Dependencies")]
        [SerializeField] private AssetRegistry _assetRegistry;

        private readonly Dictionary<ChunkCoord, Chunk> _chunks = new();

        private void OnEnable()
        {
            EventManager.AddListener<GatherSaveDataEvent>(OnGatherSaveData);
            EventManager.AddListener<ApplySaveDataEvent>(OnApplySaveData);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<GatherSaveDataEvent>(OnGatherSaveData);
            EventManager.RemoveListener<ApplySaveDataEvent>(OnApplySaveData);
        }

        private void OnDrawGizmos()
        {
            if (_chunks == null || _chunks.Count == 0) return;
            Gizmos.color = Color.yellow;
            foreach (var (coord, chunk) in _chunks)
            {
                for (int lx = 0; lx < ChunkSize; lx++)
                for (int ly = 0; ly < ChunkSize; ly++)
                {
                    if (chunk.GetCell(lx, ly) == null) continue;
                    int gx = coord.X * ChunkSize + lx;
                    int gy = coord.Y * ChunkSize + ly;
                    var center = new Vector3(gx * cellSize + cellSize * 0.5f, 0, gy * cellSize + cellSize * 0.5f);
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }

        // --- Coordinate helpers ---
        private ChunkCoord GlobalToChunkCoord(int gx, int gy)
            => new ChunkCoord(Mathf.FloorToInt((float)gx / ChunkSize), Mathf.FloorToInt((float)gy / ChunkSize));

        private Vector2Int GlobalToLocalCoord(int gx, int gy)
        {
            var cc = GlobalToChunkCoord(gx, gy);
            return new Vector2Int(gx - cc.X * ChunkSize, gy - cc.Y * ChunkSize);
        }

        private Chunk GetOrCreateChunk(ChunkCoord coord)
        {
            if (!_chunks.TryGetValue(coord, out var chunk))
            {
                chunk = new Chunk(coord);
                _chunks[coord] = chunk;
            }
            return chunk;
        }

        private GridCell GetOrCreateCell(Vector2Int globalCoords)
        {
            var cc = GlobalToChunkCoord(globalCoords.x, globalCoords.y);
            var local = GlobalToLocalCoord(globalCoords.x, globalCoords.y);
            return GetOrCreateChunk(cc).GetOrCreateCell(local.x, local.y);
        }

        // --- Public API (same surface as old WorldGridManager) ---

        public Vector2Int WorldToGridCoords(Vector3 worldPosition)
            => new Vector2Int(Mathf.FloorToInt(worldPosition.x / cellSize), Mathf.FloorToInt(worldPosition.z / cellSize));

        public GridCell GetGridData(Vector3 worldPosition) => GetGridData(WorldToGridCoords(worldPosition));

        public GridCell GetGridData(Vector2Int globalCoords)
        {
            var cc = GlobalToChunkCoord(globalCoords.x, globalCoords.y);
            var local = GlobalToLocalCoord(globalCoords.x, globalCoords.y);
            return _chunks.TryGetValue(cc, out var chunk) ? chunk.GetCell(local.x, local.y) : null;
        }

        public bool IsTileWalkable(Vector3 worldPosition) => GetGridData(worldPosition)?.groundObject != null;

        public Vector3 SnapToGridCenter(Vector3 worldPosition)
        {
            float x = Mathf.Floor(worldPosition.x / cellSize) * cellSize + cellSize * 0.5f;
            float z = Mathf.Floor(worldPosition.z / cellSize) * cellSize + cellSize * 0.5f;
            return new Vector3(x, 0, z);
        }

        public Vector3 SnapToGrid(Vector3 worldPosition, Vector3Int objectSize)
        {
            float sx = objectSize.x % 2 == 0
                ? Mathf.Round(worldPosition.x / cellSize) * cellSize
                : Mathf.Floor(worldPosition.x / cellSize) * cellSize + cellSize * 0.5f;
            float sz = objectSize.z % 2 == 0
                ? Mathf.Round(worldPosition.z / cellSize) * cellSize
                : Mathf.Floor(worldPosition.z / cellSize) * cellSize + cellSize * 0.5f;
            return new Vector3(sx, 0, sz);
        }

        public List<Vector2Int> GetGridPositionsForObject(Vector3 worldPosition, Vector3Int size)
        {
            var positions = new List<Vector2Int>();
            var origin = WorldToGridCoords(worldPosition);
            int startX = origin.x - Mathf.FloorToInt(size.x / 2f);
            int startY = origin.y - Mathf.FloorToInt(size.z / 2f);
            for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.z; y++)
                positions.Add(new Vector2Int(startX + x, startY + y));
            return positions;
        }

        public void PlaceObject(PlaceableObject obj)
        {
            var rules = obj.ItemData.placementRules;
            var gridPositions = GetGridPositionsForObject(obj.transform.position, obj.Size);
            var primaryCell = GetGridData(gridPositions[0]);

            PlacementLayer finalLayer = rules.Layer;
            if (finalLayer == PlacementLayer.Any)
                finalLayer = primaryCell?.groundObject == null ? PlacementLayer.Ground : PlacementLayer.Upper;

            foreach (var pos in gridPositions)
            {
                var cell = GetOrCreateCell(pos);
                if (finalLayer == PlacementLayer.Ground) cell.groundObject = obj;
                else cell.upperObject = obj;
            }
        }

        public void RemoveObject(PlaceableObject obj)
        {
            foreach (var pos in GetGridPositionsForObject(obj.transform.position, obj.Size))
            {
                var cell = GetGridData(pos);
                if (cell == null) continue;
                if (cell.groundObject == obj) cell.groundObject = null;
                if (cell.upperObject == obj) cell.upperObject = null;
            }
        }

        public void ClearAll()
        {
            if (PlacementManager.Instance == null) return;
            foreach (var chunk in _chunks.Values)
            {
                for (int lx = 0; lx < ChunkSize; lx++)
                for (int ly = 0; ly < ChunkSize; ly++)
                {
                    var cell = chunk.GetCell(lx, ly);
                    if (cell == null) continue;
                    if (cell.groundObject != null)
                        PlacementManager.Instance.objectPooler.ReturnToPool(cell.groundObject.gameObject);
                    if (cell.upperObject != null)
                        PlacementManager.Instance.objectPooler.ReturnToPool(cell.upperObject.gameObject);
                }
            }
            _chunks.Clear();
        }

        // --- Save / Load ---

        private void OnGatherSaveData(GatherSaveDataEvent e)
        {
            e.SaveData.placedObjects.Clear();
            var processed = new HashSet<PlaceableObject>();
            foreach (var chunk in _chunks.Values)
            {
                for (int lx = 0; lx < ChunkSize; lx++)
                for (int ly = 0; ly < ChunkSize; ly++)
                {
                    var cell = chunk.GetCell(lx, ly);
                    if (cell == null) continue;
                    TrySaveObject(e.SaveData, cell.groundObject, processed);
                    TrySaveObject(e.SaveData, cell.upperObject, processed);
                }
            }
        }

        private static void TrySaveObject(SaveData saveData, PlaceableObject obj, HashSet<PlaceableObject> processed)
        {
            if (obj == null || !processed.Add(obj)) return;

            var data = new ObjectData { visualRotationIndex = obj.VisualRotationIndex };
            data.itemGuid = obj.ItemData.AssetGuid;
            data.Position = obj.transform.position;
            data.Rotation = obj.transform.rotation;

            // Embed chest data directly so load doesn't need a UniqueID lookup.
            var chest = obj.GetComponent<IChestInventory>();
            if (chest != null)
                data.chest = chest.GetSaveData();

            saveData.placedObjects.Add(data);
        }

        private void OnApplySaveData(ApplySaveDataEvent e)
        {
            if (_assetRegistry == null)
            {
                Debug.LogError("[ChunkManager] AssetRegistry not assigned. Cannot load placed objects.");
                return;
            }

            ClearAll();
            var parent = PlacementManager.Instance?.PlacedObjectsParent;

            foreach (var data in e.SaveData.placedObjects)
            {
                var itemData = _assetRegistry.GetPlaceableByGuid(data.itemGuid);
                if (itemData == null)
                {
                    Debug.LogError($"[ChunkManager] No PlaceableItemData with GUID '{data.itemGuid}' in AssetRegistry.");
                    continue;
                }

                var go = PlacementManager.Instance.objectPooler.SpawnFromPool(
                    itemData.placedPrefab.name, data.Position, data.Rotation);
                if (go == null) continue;

                var obj = go.GetComponent<PlaceableObject>();
                if (obj == null) continue;

                obj.VisualRotationIndex = data.visualRotationIndex;
                obj.transform.SetParent(parent);
                obj.GetComponent<VisualsController>()?.SetIsTransparent(false);
                obj.Placed = true;
                PlaceObject(obj);

                // Chest data is embedded alongside the object — no ID lookup needed.
                if (data.chest != null)
                    go.GetComponent<IChestInventory>()?.ApplySaveData(data.chest, _assetRegistry);
            }
        }
    }
}
