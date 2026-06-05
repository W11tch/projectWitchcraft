// Located at: Assets/Scripts/UI/SaveLoadUI.cs
using UnityEngine;
using UnityEngine.UI;
using ProjectWitchcraft.Managers;
using ProjectWitchcraft.Core;

namespace ProjectWitchcraft.UI
{
    public class SaveLoadUI : MonoBehaviour
    {
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button clearButton;

        private void OnEnable()
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            clearButton.onClick.AddListener(OnClearButtonClicked);
        }

        private void OnDisable()
        {
            saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            loadButton.onClick.RemoveListener(OnLoadButtonClicked);
            clearButton.onClick.RemoveListener(OnClearButtonClicked);
        }

        private void OnSaveButtonClicked()
        {
            EventManager.TriggerEvent(new SaveRequestEvent());
        }

        private void OnLoadButtonClicked()
        {
            EventManager.TriggerEvent(new LoadRequestEvent());
        }

        private void OnClearButtonClicked()
        {
            SaveManager.Instance.ClearSave();
        }
    }
}