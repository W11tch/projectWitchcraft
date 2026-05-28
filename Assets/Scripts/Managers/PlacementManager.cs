// Located at: Assets/Scripts/Managers/PlacementManager.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using ProjectWitchcraft.BuildingSystem;
using ProjectWitchcraft.Core;
using UnityEngine.InputSystem;

namespace ProjectWitchcraft.Managers
{
    public class PlacementManager : Singleton<PlacementManager>
    {
        [Header("Dependencies")]
        public ObjectPooler objectPooler;
        [SerializeField] private ChunkManager worldGridManager;
        [SerializeField] private InventoryManager inventoryManager;
        [Header("Configuration")]
        [SerializeField] private Transform placedObjectsParent;
        public Transform PlacedObjectsParent => placedObjectsParent;
        [SerializeField] private LayerMask buildingBlockLayer;
        [SerializeField] private LayerMask groundLayer;
        private PlaceableObject _previewObject;
        private PlaceableItemData _currentItemData;
        private bool _isPlacing = false;
        private int _previewLayer;
        private bool _isPointerOverUI = false;
        private bool _isDestroyModeActive = true;
        private bool _isFrozen = false;
        private Camera _mainCamera;
        protected override void Awake()
        {
            base.Awake();
            _mainCamera = GameReferences.Instance.MainCamera;
            _previewLayer = LayerMask.NameToLayer("Preview");
        }
        private void OnEnable()
        {
            EventManager.AddListener<GameStateChangedEvent>(OnGameStateChanged);
            EventManager.AddListener<PlacementModeRequestedEvent>(OnPlacementModeRequested);
            EventManager.AddListener<PlaceActionTriggeredEvent>(HandlePlaceAction);
            EventManager.AddListener<RotateActionTriggeredEvent>(HandleRotateAction);
            EventManager.AddListener<CancelActionTriggeredEvent>(HandleCancelAction);
            EventManager.AddListener<DestroyActionTriggeredEvent>(HandleDestroyAction);
            EventManager.AddListener<ToggleDestroyModeEvent>(OnToggleDestroyMode);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
            EventManager.RemoveListener<PlacementModeRequestedEvent>(OnPlacementModeRequested);
            EventManager.RemoveListener<PlaceActionTriggeredEvent>(HandlePlaceAction);
            EventManager.RemoveListener<RotateActionTriggeredEvent>(HandleRotateAction);
            EventManager.RemoveListener<CancelActionTriggeredEvent>(HandleCancelAction);
            EventManager.RemoveListener<DestroyActionTriggeredEvent>(HandleDestroyAction);
            EventManager.RemoveListener<ToggleDestroyModeEvent>(OnToggleDestroyMode);
        }
        private void OnToggleDestroyMode(ToggleDestroyModeEvent e)
        {
            _isDestroyModeActive = e.IsDestroyModeActive;
        }

        private void Update()
        {
            _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

            if (_isPlacing)
            {
                UpdatePreview();
            }
        }

        private void OnPlacementModeRequested(PlacementModeRequestedEvent eventData)
        {
            PlaceableItemData itemData = eventData.ItemData;
            if (itemData?.placedPrefab == null) return;

            if (_isPlacing && _currentItemData == itemData)
            {
                StopPlacement();
                return;
            }

            StartPlacement(itemData);
        }

        private void StartPlacement(PlaceableItemData itemData)
        {
            StopPlacement();
            _isPlacing = true;
            _currentItemData = itemData;

            Vector3 initialSpawnPosition = new Vector3(0, -1000f, 0);
            _previewObject = objectPooler.SpawnFromPool(_currentItemData.placedPrefab.name, initialSpawnPosition, Quaternion.identity).GetComponent<PlaceableObject>();

            if (_previewObject == null)
            {
                _isPlacing = false;
                return;
            }
            _previewObject.gameObject.layer = _previewLayer;
            foreach (Transform child in _previewObject.transform)
            {
                child.gameObject.layer = _previewLayer;
            }
            _previewObject.GetComponent<VisualsController>()?.SetIsTransparent(true, 0.5f);

            UpdatePreview();
        }

        private void StopPlacement()
        {
            _isPlacing = false;
            if (_previewObject != null)
            {
                objectPooler.ReturnToPool(_previewObject.gameObject);
            }
            _previewObject = null;
            _currentItemData = null;
        }

        private void HandlePlaceAction(PlaceActionTriggeredEvent e)
        {
            if (_isFrozen || !_isPlacing || _isPointerOverUI || _previewObject == null) return;

            Vector3 finalPosition = _previewObject.transform.position;
            if (IsPlacementValid(finalPosition, _previewObject.Size))
            {
                inventoryManager.RemoveItem(_currentItemData, 1);

                var finalObject = objectPooler.SpawnFromPool(_currentItemData.placedPrefab.name, finalPosition, _previewObject.transform.rotation).GetComponent<PlaceableObject>();

                // Copy the visual rotation state from the preview object to the final placed object.
                finalObject.VisualRotationIndex = _previewObject.VisualRotationIndex;

                finalObject.transform.SetParent(placedObjectsParent);
                finalObject.GetComponent<VisualsController>()?.SetIsTransparent(false);
                finalObject.Placed = true;

                worldGridManager.PlaceObject(finalObject);

                // Temporarily disable the collider to prevent the physics bump.
                StartCoroutine(EnableColliderAfterFrame(finalObject));
            }
        }

        // This coroutine waits one frame, then re-enables the collider.
        private IEnumerator EnableColliderAfterFrame(PlaceableObject obj)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
                // Wait until the end of the current frame.
                yield return new WaitForEndOfFrame();
                col.enabled = true;
            }
        }

        private void HandleRotateAction(RotateActionTriggeredEvent e)
        {
            if (_isFrozen || !_isPlacing) return;
            _previewObject?.Rotate();
        }

        private void HandleCancelAction(CancelActionTriggeredEvent e)
        {
            if (!_isPlacing) return;
            StopPlacement();
        }

        private void HandleDestroyAction(DestroyActionTriggeredEvent e)
        {
            if (_isFrozen || !_isDestroyModeActive || _isPointerOverUI) return;

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hit, 100f, buildingBlockLayer))
            {
                var objToDestroy = hit.collider.GetComponentInParent<PlaceableObject>();
                if (objToDestroy != null)
                {
                    var gridData = worldGridManager.GetGridData(objToDestroy.transform.position);
                    if (gridData != null && gridData.groundObject == objToDestroy && gridData.upperObject != null)
                    {
                        return;
                    }

                    worldGridManager.RemoveObject(objToDestroy);
                    objectPooler.ReturnToPool(objToDestroy.gameObject);
                }
            }
        }
        private Vector3 GetMouseWorldPosition()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, groundLayer))
            {
                return hitInfo.point;
            }
            return new Vector3(0, -999, 0);
        }
        private void UpdatePreview()
        {
            if (_previewObject == null) return;

            Vector3 mousePos = GetMouseWorldPosition();
            if (mousePos.y < -900)
            {
                _previewObject.transform.position = new Vector3(0, -1000f, 0);
                return;
            }
            Vector3 snappedPos = worldGridManager.SnapToGrid(mousePos, _previewObject.Size);

            Vector3 finalPreviewPosition = GetPreviewPosition(snappedPos);
            _previewObject.transform.position = finalPreviewPosition;

            bool canPlace = IsPlacementValid(finalPreviewPosition, _previewObject.Size);
            _previewObject.GetComponent<VisualsController>()?.SetIsTransparent(true, canPlace ? 0.5f : 0.2f);
        }
        private Vector3 GetPreviewPosition(Vector3 snappedPos)
        {
            if (_currentItemData == null) return snappedPos;

            var rules = _currentItemData.placementRules;
            List<Vector2Int> gridPositions = worldGridManager.GetGridPositionsForObject(snappedPos, _previewObject.Size);

            // Determine if this is an upper layer placement.
            bool placeOnUpper = (rules.Layer == PlacementLayer.Upper);
            if (rules.Layer == PlacementLayer.Any)
            {
                // For 'Any', it can only be 'Upper' if ALL tiles have ground objects.
                bool canBeUpper = true;
                foreach (var pos in gridPositions)
                {
                    if (worldGridManager.GetGridData(pos)?.groundObject == null)
                    {
                        canBeUpper = false;
                        break;
                    }
                }
                placeOnUpper = canBeUpper;
            }

            if (placeOnUpper)
            {
                // Now that we know it's an Upper object, find the highest point to place it on.
                float highestPoint = float.MinValue;
                foreach (var pos in gridPositions)
                {
                    GridCell cell = worldGridManager.GetGridData(pos);
                    // If any cell is missing a ground object, the placement is invalid. Sink the preview.
                    if (cell?.groundObject == null)
                    {
                        snappedPos.y = -1000f;
                        return snappedPos;
                    }

                    // Also check if the upper slot is already blocked by an object that can't be stacked on.
                    if (cell.upperObject != null && !cell.groundObject.ItemData.placementRules.AllowsStackingOnTop)
                    {
                        snappedPos.y = -1000f; // Sink the preview
                        return snappedPos;
                    }

                    var groundCollider = cell.groundObject.GetComponent<Collider>();
                    if (groundCollider != null)
                    {
                        float topOfGroundObject = groundCollider.bounds.center.y + groundCollider.bounds.extents.y;
                        if (topOfGroundObject > highestPoint)
                        {
                            highestPoint = topOfGroundObject;
                        }
                    }
                }

                Collider objectCollider = _previewObject.GetComponent<Collider>();
                float halfHeight = objectCollider != null ? objectCollider.bounds.extents.y : 0.5f;
                snappedPos.y = highestPoint + halfHeight;
            }
            else // It's a Ground layer placement
            {
                Collider objectCollider = _previewObject.GetComponent<Collider>();
                float halfHeight = objectCollider != null ? objectCollider.bounds.extents.y : 0.5f;
                // This is the original line from your script. I have reverted it back,
                // as you mentioned it was intentional for your 3D objects.
                snappedPos.y = 0.01f - halfHeight;
            }
            return snappedPos;
        }
        private bool IsPlacementValid(Vector3 position, Vector3Int size)
        {
            if (_currentItemData == null || !inventoryManager.HasItem(_currentItemData, 1))
            {
                return false;
            }

            if (position.y < -900f) return false;
            List<Vector2Int> gridPositions = worldGridManager.GetGridPositionsForObject(position, size);
            foreach (var gridPos in gridPositions)
            {
                GridCell cell = worldGridManager.GetGridData(gridPos);
                var rules = _currentItemData.placementRules;
                switch (rules.Layer)
                {
                    case PlacementLayer.Ground:
                        if (cell?.groundObject != null) return false;
                        break;

                    case PlacementLayer.Upper:
                        if (cell?.groundObject == null) return false;
                        if (cell.upperObject != null && !cell.upperObject.ItemData.placementRules.AllowsStackingOnTop) return false;
                        break;

                    case PlacementLayer.Any:
                        bool groundOccupied = cell?.groundObject != null;
                        bool upperIsBlocked = cell?.upperObject != null && !cell.upperObject.ItemData.placementRules.AllowsStackingOnTop;
                        if (groundOccupied && upperIsBlocked) return false;
                        break;
                }
            }

            return true;
        }
        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            _isFrozen = (e.NewState == GameState.Paused || e.NewState == GameState.InMenu);

            if (_isFrozen)
            {
                StopPlacement();
            }
        }
    }
}