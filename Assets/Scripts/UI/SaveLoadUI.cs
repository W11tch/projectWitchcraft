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
            // FIX: This now fires the correct event to REQUEST a save.
            EventManager.TriggerEvent(new SaveRequestEvent());
            Debug.Log("UI: Save button clicked, SaveRequestEvent fired.");
        }

        private void OnLoadButtonClicked()
        {
            // FIX: This now fires the correct event to REQUEST a load.
            EventManager.TriggerEvent(new LoadRequestEvent());
            Debug.Log("UI: Load button clicked, LoadRequestEvent fired.");
        }

        private void OnClearButtonClicked()
        {
            // This event is not fully implemented in SaveManager yet, but we'll use the correct request.
            // For now, it will likely do nothing, which is fine.
            // EventManager.TriggerEvent(new ClearSaveDataRequestEvent()); 
            Debug.Log("UI: Clear button clicked.");
        }
    }
}