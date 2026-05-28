using UnityEngine;

namespace ProjectWitchcraft.Core
{
    // Central cache for frequently accessed scene references.
    // Add this to a persistent scene object alongside the other managers.
    public class GameReferences : Singleton<GameReferences>
    {
        public Camera MainCamera { get; private set; }
        public Transform PlayerTransform { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            MainCamera = Camera.main;
        }

        // Called by the Player's root MonoBehaviour on Awake.
        public void RegisterPlayer(Transform playerTransform)
        {
            PlayerTransform = playerTransform;
        }
    }
}
