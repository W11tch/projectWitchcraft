// Located at: Assets/Scripts/Player/PlayerMovement.cs
using UnityEngine;
using ProjectWitchcraft.Managers;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Stats;

namespace ProjectWitchcraft.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerController))]
    public class PlayerMovement : MonoBehaviour
    {
        private bool _isMovementFrozen = false;

        [Header("Movement Settings")]
        [Tooltip("The StatDefinition asset that represents the player's move speed.")]
        [SerializeField] private StatDefinition _moveSpeedStat;
        private float _gravity = -9.81f;

        [Tooltip("Controls the size of the character's collision footprint. A smaller value allows the character to get closer to edges.")]
        [SerializeField] private float footprintSizeMultiplier = 0.1f;

        public bool IsFlyModeActive { get; set; } = false;

        private CharacterController _characterController;
        private PlayerController _playerController;
        private WorldGridManager _worldGridManager;
        private Vector3 _playerVelocity;
        private CharacterStats _characterStats;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _playerController = GetComponent<PlayerController>();
            // Get the CharacterStats component attached to the player.
            _characterStats = GetComponent<CharacterStats>();
            if (_characterStats == null)
            {
                Debug.LogError("PlayerMovement requires a CharacterStats component, but none was found.", this);
            }
        }

        private void Start()
        {
            _worldGridManager = WorldGridManager.Instance;
        }

        private void OnEnable()
        {
            EventManager.AddListener<ToggleFlyModeEvent>(OnToggleFlyMode);
            EventManager.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<ToggleFlyModeEvent>(OnToggleFlyMode);
            EventManager.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnToggleFlyMode(ToggleFlyModeEvent e)
        {
            IsFlyModeActive = e.IsFlyModeActive;
        }

        private void Update()
        {
            if (_isMovementFrozen) return;
            HandleMovement();
        }

        private void HandleMovement()
        {
            bool isGrounded = _characterController.isGrounded;
            if (isGrounded && _playerVelocity.y < 0)
            {
                _playerVelocity.y = -2f;
            }

            Vector2 input = _playerController.MoveInput;
            Vector3 move = new Vector3(input.x, 0, input.y);

            if (!IsFlyModeActive)
            {
                if (input.magnitude > 0.1f && !IsPathClear(move.normalized))
                {
                    move = Vector3.zero;
                }
            }


            _playerVelocity.y += _gravity * Time.deltaTime;

            // We now get the current move speed from the CharacterStats component every frame.
            // This ensures any buffs or debuffs are applied in real-time.
            float currentMoveSpeed = _characterStats.GetStatValue(_moveSpeedStat);
            _characterController.Move(move * currentMoveSpeed * Time.deltaTime);
            _characterController.Move(_playerVelocity * Time.deltaTime);
        }

        private bool IsPathClear(Vector3 direction)
        {
            float checkDistance = _characterController.radius + 0.1f;

            Vector3 halfWidth = transform.right * (_characterController.radius * footprintSizeMultiplier);
            Vector3 halfDepth = transform.forward * (_characterController.radius * footprintSizeMultiplier);

            Vector3 frontRight = transform.position + halfWidth + halfDepth;
            Vector3 frontLeft = transform.position - halfWidth + halfDepth;
            Vector3 backRight = transform.position + halfWidth - halfDepth;
            Vector3 backLeft = transform.position - halfWidth - halfDepth;

            if (!_worldGridManager.IsTileWalkable(frontRight + direction * checkDistance)) return false;
            if (!_worldGridManager.IsTileWalkable(frontLeft + direction * checkDistance)) return false;
            if (!_worldGridManager.IsTileWalkable(backRight + direction * checkDistance)) return false;
            if (!_worldGridManager.IsTileWalkable(backLeft + direction * checkDistance)) return false;

            return true;
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            _isMovementFrozen = (e.NewState == GameState.Paused || e.NewState == GameState.InMenu);
        }
    }
}