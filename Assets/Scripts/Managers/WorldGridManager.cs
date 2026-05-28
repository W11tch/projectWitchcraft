// Located at: Assets/Scripts/Managers/WorldGridManager.cs
using ProjectWitchcraft.BuildingSystem;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.Managers
{
    [Serializable]
    public class GridCell
    {
        public PlaceableObject groundObject = null;
        public PlaceableObject upperObject = null;
    }

    public class WorldGridManager : Singleton<WorldGridManager>
    {
        [Header("Configuration")]
        [SerializeField] private float cellSize = 1f;

        private Dictionary<Vector2Int, GridCell> _grid = new Dictionary<Vector2Int, GridCell>();
        private Transform _placedObjectsParent;

        private void OnDrawGizmos()
        {
            if (_grid == null || _grid.Count == 0) return;
            Gizmos.color = Color.yellow;
            foreach (var pair in _grid)
            {
                if (pair.Value.groundObject != null || pair.Value.upperObject != null)
                {
                    Vector3 cellCenter = new Vector3(pair.Key.x * cellSize + (cellSize / 2.0f), 0, pair.Key.y * cellSize + (cellSize / 2.0f));
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            var placementManager = FindAnyObjectByType<PlacementManager>();
            if (placementManager != null)
            {
                _placedObjectsParent = placementManager.PlacedObjectsParent;
            }
        }

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

        private void OnGatherSaveData(GatherSaveDataEvent e)
        {
            e.SaveData.placedObjects.Clear();
            var processedObjects = new HashSet<PlaceableObject>();
            foreach (var cell in _grid.Values)
            {
                if (cell.groundObject != null && processedObjects.Add(cell.groundObject))
                {
                    e.SaveData.placedObjects.Add(new ObjectData 
                    { 
                        itemDataName = cell.groundObject.ItemData.name, 
                        position = cell.groundObject.transform.position, 
                        rotation = cell.groundObject.transform.rotation,
                        visualRotationIndex = cell.groundObject.VisualRotationIndex
                    });
                }
                if (cell.upperObject != null && processedObjects.Add(cell.upperObject))
                {
                    e.SaveData.placedObjects.Add(new ObjectData 
                    { 
                        itemDataName = cell.upperObject.ItemData.name, 
                        position = cell.upperObject.transform.position, 
                        rotation = cell.upperObject.transform.rotation,
                        visualRotationIndex = cell.upperObject.VisualRotationIndex
                    });
                }
            }
        }

        private void OnApplySaveData(ApplySaveDataEvent e)
        {
            ClearAllPlacedObjects();
            foreach (var objectData in e.SaveData.placedObjects)
            {
                PlaceableItemData itemData = Resources.Load<PlaceableItemData>($"Items/Placeable/{objectData.itemDataName}");
                if (itemData != null)
                {
                    var placedObject = PlacementManager.Instance.objectPooler.SpawnFromPool(itemData.placedPrefab.name, objectData.position, objectData.rotation).GetComponent<PlaceableObject>();
                    if (placedObject != null)
                    {
                        // **FIX**: Apply the saved visual rotation index after spawning.
                        placedObject.VisualRotationIndex = objectData.visualRotationIndex;

                        placedObject.transform.SetParent(_placedObjectsParent);
                        placedObject.GetComponent<VisualsController>()?.SetIsTransparent(false);
                        placedObject.Placed = true;
                        // Let the grid manager figure out where it belongs
                        PlaceObject(placedObject);
                    }
                }
            }
        }

        private void ClearAllPlacedObjects()
        {
            if (PlacementManager.Instance == null) return;
            foreach (var cell in _grid.Values)
            {
                if (cell.groundObject != null) { PlacementManager.Instance.objectPooler.ReturnToPool(cell.groundObject.gameObject); }
                if (cell.upperObject != null) { PlacementManager.Instance.objectPooler.ReturnToPool(cell.upperObject.gameObject); }
            }
            _grid.Clear();
        }

        public bool IsTileWalkable(Vector3 worldPosition)
        {
            var cell = GetGridData(worldPosition);
            return cell?.groundObject != null;
        }

        public Vector3 SnapToGridCenter(Vector3 worldPosition)
        {
            float x = Mathf.Floor(worldPosition.x / cellSize) * cellSize + (cellSize / 2.0f);
            float z = Mathf.Floor(worldPosition.z / cellSize) * cellSize + (cellSize / 2.0f);
            return new Vector3(x, 0, z);
        }

        // Snaps a world position to the grid, accounting for the size of the object.
        // Odd-sized objects are centered on cells, even-sized objects are centered on grid lines.
        public Vector3 SnapToGrid(Vector3 worldPosition, Vector3Int objectSize)
        {
            float snappedX, snappedZ;

            // --- X-Axis Snapping ---
            if (objectSize.x % 2 == 0)
            {
                // EVEN size: Snap to the nearest grid line.
                snappedX = Mathf.Round(worldPosition.x / cellSize) * cellSize;
            }
            else
            {
                // ODD size: Snap to the nearest cell center (your old logic).
                snappedX = Mathf.Floor(worldPosition.x / cellSize) * cellSize + (cellSize / 2.0f);
            }

            // --- Z-Axis Snapping ---
            if (objectSize.z % 2 == 0)
            {
                // EVEN size: Snap to the nearest grid line.
                snappedZ = Mathf.Round(worldPosition.z / cellSize) * cellSize;
            }
            else
            {
                // ODD size: Snap to the nearest cell center.
                snappedZ = Mathf.Floor(worldPosition.z / cellSize) * cellSize + (cellSize / 2.0f);
            }

            return new Vector3(snappedX, 0, snappedZ);
        }

        public Vector2Int WorldToGridCoords(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / cellSize);
            int y = Mathf.FloorToInt(worldPosition.z / cellSize);
            return new Vector2Int(x, y);
        }

        public GridCell GetGridData(Vector3 worldPosition)
        {
            var gridCoords = WorldToGridCoords(worldPosition);
            _grid.TryGetValue(gridCoords, out var cell);
            return cell;
        }

        public GridCell GetGridData(Vector2Int gridCoords)
        {
            _grid.TryGetValue(gridCoords, out var cell);
            return cell;
        }

        private GridCell GetOrCreateGridCell(Vector2Int gridCoords)
        {
            if (!_grid.TryGetValue(gridCoords, out var cell))
            {
                cell = new GridCell();
                _grid[gridCoords] = cell;
            }
            return cell;
        }

        public List<Vector2Int> GetGridPositionsForObject(Vector3 worldPosition, Vector3Int size)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            Vector2Int origin = WorldToGridCoords(worldPosition);

            // --- REPLACE WITH THIS NEW, CORRECTED CODE ---
            int startX = origin.x - Mathf.FloorToInt(size.x / 2f);
            int startY = origin.y - Mathf.FloorToInt(size.z / 2f);

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.z; y++)
                {
                    positions.Add(new Vector2Int(startX + x, startY + y));
                }
            }
            return positions;
        }

        /// <summary>
        /// Places an object in the grid. This method now contains the logic to determine
        /// whether the object should be a ground or upper object based on its rules.
        /// </summary>
        public void PlaceObject(PlaceableObject obj)
        {
            var rules = obj.ItemData.placementRules;
            List<Vector2Int> gridPositions = GetGridPositionsForObject(obj.transform.position, obj.Size);

            // Need to check the state of the *first* tile to decide the layer for Any objects.
            // This assumes multi-tile objects are uniform.
            var primaryCell = GetGridData(gridPositions[0]);

            PlacementLayer finalLayer = rules.Layer;
            if (finalLayer == PlacementLayer.Any)
            {
                finalLayer = (primaryCell?.groundObject == null) ? PlacementLayer.Ground : PlacementLayer.Upper;
            }

            foreach (var pos in gridPositions)
            {
                var cell = GetOrCreateGridCell(pos);
                if (finalLayer == PlacementLayer.Ground)
                {
                    cell.groundObject = obj;
                }
                else
                {
                    cell.upperObject = obj;
                }
            }
        }

        public void RemoveObject(PlaceableObject obj)
        {
            List<Vector2Int> gridPositions = GetGridPositionsForObject(obj.transform.position, obj.Size);

            foreach (var pos in gridPositions)
            {
                if (_grid.TryGetValue(pos, out var cell))
                {
                    if (cell.groundObject == obj) cell.groundObject = null;
                    if (cell.upperObject == obj) cell.upperObject = null;
                }
            }
        }
    }
}