// Located at: Assets/Scripts/Managers/GameManager.cs
using ProjectWitchcraft.Core;
using UnityEngine;

namespace ProjectWitchcraft.Managers
{

    public class GameManager : Singleton<GameManager>
    {
        public GameState CurrentState { get; private set; }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            UpdateState(GameState.Playing);
        }

        public void UpdateState(GameState newState, MenuContext context = MenuContext.None)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.InMenu:
                    Time.timeScale = 1f;
                    break;
            }
            EventManager.TriggerEvent(new GameStateChangedEvent { NewState = newState, Context = context });
        }
    }
}