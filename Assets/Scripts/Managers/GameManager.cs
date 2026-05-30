using System.Collections.Generic;
using ProjectWitchcraft.Core;
using UnityEngine;

namespace ProjectWitchcraft.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public GameState CurrentState { get; private set; } = GameState.Loading;

        private static readonly HashSet<(GameState, GameState)> ValidTransitions = new()
        {
            // Loading is reachable from any state (triggered by SaveManager).
            (GameState.Loading,    GameState.Playing),
            (GameState.Playing,    GameState.Loading),
            (GameState.Building,   GameState.Loading),
            (GameState.InMenu,     GameState.Loading),
            (GameState.Combat,     GameState.Loading),
            (GameState.Paused,     GameState.Loading),
            (GameState.Cinematic,  GameState.Loading),

            (GameState.Playing,    GameState.Building),
            (GameState.Playing,    GameState.InMenu),
            (GameState.Playing,    GameState.Combat),
            (GameState.Playing,    GameState.Paused),
            (GameState.Playing,    GameState.Cinematic),

            (GameState.Building,   GameState.Playing),
            (GameState.Building,   GameState.InMenu),
            (GameState.Building,   GameState.Paused),

            (GameState.InMenu,     GameState.Playing),
            (GameState.InMenu,     GameState.Building),
            (GameState.InMenu,     GameState.Paused),

            (GameState.Combat,     GameState.Playing),
            (GameState.Combat,     GameState.InMenu),
            (GameState.Combat,     GameState.Paused),

            (GameState.Paused,     GameState.Playing),
            (GameState.Paused,     GameState.Building),
            (GameState.Paused,     GameState.InMenu),

            (GameState.Cinematic,  GameState.Playing),
        };

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UpdateState(GameState.Playing);
        }

        public void UpdateState(GameState newState, MenuContext context = MenuContext.None)
        {
            if (CurrentState == newState) return;

            if (!ValidTransitions.Contains((CurrentState, newState)))
            {
                Debug.LogError($"[GameManager] Invalid state transition: {CurrentState} → {newState}");
                return;
            }

            CurrentState = newState;

            Time.timeScale = newState == GameState.Paused ? 0f : 1f;

            EventManager.TriggerEvent(new GameStateChangedEvent { NewState = newState, Context = context });
        }
    }
}
