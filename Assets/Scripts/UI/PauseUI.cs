// Located at: Assets/Scripts/UI/PauseUI.cs
using UnityEngine;
using ProjectWitchcraft.Core;
using ProjectWitchcraft.Managers;

namespace ProjectWitchcraft.UI
{
    public class PauseUI : MonoBehaviour
    {
        private GameObject _pausePanel;

        private void Awake()
        {
            _pausePanel = this.gameObject;
            _pausePanel.SetActive(false);
            EventManager.AddListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            bool shouldBeActive = (e.NewState == GameState.Paused);
            _pausePanel.SetActive(shouldBeActive);
        }
    }
}