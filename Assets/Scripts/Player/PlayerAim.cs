// Located at: Assets/Scripts/Player/PlayerAim.cs
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAim : MonoBehaviour
    {
        private Camera _camera;
        private bool _isAimingFrozen;

        public Vector3 AimDirection => transform.forward;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void OnEnable()
        {
            EventManager.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void Update()
        {
            if (_isAimingFrozen || _camera == null) return;

            var groundPlane = new Plane(Vector3.up, transform.position);
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                Vector3 lookTarget = new Vector3(worldPoint.x, transform.position.y, worldPoint.z);
                if ((lookTarget - transform.position).sqrMagnitude > 0.001f)
                    transform.LookAt(lookTarget);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            _isAimingFrozen = e.NewState == GameState.Paused
                || e.NewState == GameState.InMenu
                || e.NewState == GameState.Loading
                || e.NewState == GameState.Cinematic;
        }
    }
}
