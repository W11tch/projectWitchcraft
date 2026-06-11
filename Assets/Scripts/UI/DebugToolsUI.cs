using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ProjectWitchcraft.Managers;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Entities;

namespace ProjectWitchcraft.UI
{
    public class DebugToolsUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AssetRegistry _assetRegistry;

        [Header("UI & Settings")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private EntityDefinition _targetDummyDefinition;
        [SerializeField] private int amountToAdd = 100;
        [SerializeField] private int amountToDamage = 50;
        [SerializeField] private int amountToDamagePlayer = 25;
        [SerializeField] private Toggle flyModeToggle;
        [SerializeField] private Toggle destroyModeToggle;
        [SerializeField] private Toggle killModeToggle;
        [SerializeField] private LayerMask _entityLayer;

        // True while waiting for the next world click to place a target dummy.
        private bool _isPlacingDummy;
        // Set by the "Enable Kill Key" toggle; gates the K-key entity despawn.
        private bool _isKillModeActive;
        // Cached once per frame in Update — read in InputAction-driven handlers, since calling
        // EventSystem.IsPointerOverGameObject() during input event processing warns and is stale.
        private bool _isPointerOverUI;

        private void OnEnable()  => EventManager.AddListener<KillActionTriggeredEvent>(OnKillAction);
        private void OnDisable() => EventManager.RemoveListener<KillActionTriggeredEvent>(OnKillAction);

        private void Start()
        {
            if (debugPanel != null) debugPanel.SetActive(false);

            if (flyModeToggle != null)
            {
                flyModeToggle.isOn = false;
                OnFlyModeToggled(false);
                flyModeToggle.onValueChanged.AddListener(OnFlyModeToggled);
            }

            if (destroyModeToggle != null)
            {
                destroyModeToggle.isOn = true;
                OnDestroyModeToggled(true);
                destroyModeToggle.onValueChanged.AddListener(OnDestroyModeToggled);
            }

            if (killModeToggle != null)
            {
                killModeToggle.isOn = false;
                OnKillModeToggled(false);
                killModeToggle.onValueChanged.AddListener(OnKillModeToggled);
            }
        }

        private void Update()
        {
            _isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            if (Keyboard.current.backquoteKey.wasPressedThisFrame) ToggleDebugPanel();

            if (_isPlacingDummy) HandleDummyPlacement();
        }

        // While armed by the Spawn Target Dummy button: left-click in the world spawns the dummy at
        // the cursor's ground point; right-click or ESC cancels. Clicks on UI are ignored.
        private void HandleDummyPlacement()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.rightButton.wasPressedThisFrame ||
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                _isPlacingDummy = false;
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (_isPointerOverUI) return;

            if (TryGetGroundPoint(out Vector3 point))
            {
                // The dummy is a ground entity — refuse cells occupied by an upper-level placed object
                // (stay armed so the user can pick another spot).
                if (ChunkManager.Instance != null && ChunkManager.Instance.HasUpperObject(point))
                {
                    Debug.Log("[DebugTools] Can't spawn the target dummy on an upper-level object — pick another spot.");
                    return;
                }

                EntityManager.Instance.Spawn(_targetDummyDefinition, point, Quaternion.identity, settleOnGround: true);
                _isPlacingDummy = false;
            }
        }

        private bool TryGetGroundPoint(out Vector3 point)
        {
            point = default;
            var cam = GameReferences.Instance != null ? GameReferences.Instance.MainCamera : Camera.main;
            if (cam == null) return false;

            var ground = new Plane(Vector3.up, Vector3.zero);
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (ground.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            return false;
        }

        public void ToggleDebugPanel()
        {
            if (debugPanel != null) debugPanel.SetActive(!debugPanel.activeSelf);
        }

        public void OnFlyModeToggled(bool isFlyModeOn)
        {
            EventManager.TriggerEvent(new ToggleFlyModeEvent { IsFlyModeActive = isFlyModeOn });
        }

        public void OnDestroyModeToggled(bool isDestroyModeOn)
        {
            EventManager.TriggerEvent(new ToggleDestroyModeEvent { IsDestroyModeActive = isDestroyModeOn });
        }

        public void OnKillModeToggled(bool isKillModeOn)
        {
            _isKillModeActive = isKillModeOn;
        }

        // K-key handler (wired via PlayerInput → PlayerController.OnKillEntity → KillActionTriggeredEvent).
        // Raycasts the Entity layer under the cursor and despawns the hit entity through the seam.
        private void OnKillAction(KillActionTriggeredEvent e)
        {
            if (!_isKillModeActive) return;
            if (_isPointerOverUI) return;

            var cam = GameReferences.Instance != null ? GameReferences.Instance.MainCamera : Camera.main;
            if (cam == null || Mouse.current == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hit, 100f, _entityLayer))
            {
                var entity = hit.collider.GetComponentInParent<Entity>();
                if (entity != null) EntityManager.Instance.Despawn(entity);
            }
        }

        public void GiveAllItems()
        {
            if (_assetRegistry == null) { Debug.LogError("[DebugTools] AssetRegistry not assigned."); return; }
            var inv = InventoryManager.Instance;
            if (inv == null) { Debug.LogError("[DebugTools] InventoryManager.Instance is null."); return; }

            foreach (var itemData in _assetRegistry.AllItemsOfAllTypes)
            {
                if (itemData != null)
                    inv.AddItem(itemData, Mathf.Min(amountToAdd, itemData.maxStackSize));
            }
        }

        public void HurtPlayer()
        {
            var player = GameReferences.Instance?.PlayerTransform;
            if (player == null) { Debug.LogWarning("[DebugTools] PlayerTransform not registered."); return; }
            var damageable = player.GetComponent<IDamageable>();
            if (damageable == null) { Debug.LogWarning("[DebugTools] Player has no IDamageable component."); return; }
            damageable.TakeDamage(new HitData
            {
                Damage = amountToDamagePlayer,
                Type = DamageType.Physical,
                WorldPosition = player.position,
                Direction = Vector3.zero
            });
        }

        public void GetFullHealth()
        {
            var player = GameReferences.Instance?.PlayerTransform;
            if (player == null) { Debug.LogWarning("[DebugTools] PlayerTransform not registered."); return; }
            var health = player.GetComponent<HealthComponent>();
            if (health == null) { Debug.LogWarning("[DebugTools] Player has no HealthComponent."); return; }
            health.Heal(health.MaxHealth);
        }

        // Arms placement: the next left-click in the world spawns the dummy at the cursor.
        public void SpawnTargetDummy()
        {
            if (_targetDummyDefinition == null) { Debug.LogError("[DebugTools] Target dummy definition not assigned."); return; }
            _isPlacingDummy = true;
            Debug.Log("[DebugTools] Target dummy placement armed — left-click to spawn, right-click/ESC to cancel.");
        }

        public void DamageAllEquipment()
        {
            EquipmentManager.Instance?.DamageAll(amountToDamage);
        }

        public void RepairEverything()
        {
            var inv = InventoryManager.Instance;
            if (inv == null) return;
            RepairSlotList(inv.HotbarSlots);
            RepairSlotList(inv.InventorySlots);
            if (inv.ExternalInventory != null)
                RepairSlotList(inv.ExternalInventory);

            EquipmentManager.Instance.RepairAll();
        }

        private static void RepairSlotList(System.Collections.Generic.IReadOnlyList<InventorySlot> slots)
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.Instance.HasDurability && slot.Instance.Definition != null)
                    slot.Instance.CurrentDurability = slot.Instance.Definition.GetMaxDurability();
            }
        }
    }
}
