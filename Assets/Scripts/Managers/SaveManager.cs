// Located at: Assets/Scripts/Managers/SaveManager.cs
using UnityEngine;
using ProjectWitchcraft.Core;
using System.IO;

namespace ProjectWitchcraft.Managers
{
    public class SaveManager : Singleton<SaveManager>
    {
        private string _savePath;
        private const string SAVE_FILE_NAME = "savegame.json";

        protected override void Awake()
        {
            base.Awake();
            _savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        private void OnEnable()
        {
            EventManager.AddListener<SaveRequestEvent>(HandleSaveRequest);
            EventManager.AddListener<LoadRequestEvent>(HandleLoadRequest);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<SaveRequestEvent>(HandleSaveRequest);
            EventManager.RemoveListener<LoadRequestEvent>(HandleLoadRequest);
        }

        private void HandleSaveRequest(SaveRequestEvent e)
        {
            SaveGame();
        }

        private void HandleLoadRequest(LoadRequestEvent e)
        {
            LoadGame();
        }

        public void SaveGame()
        {
            Debug.Log("Saving game to: " + _savePath);
            SaveData saveData = new SaveData();

            EventManager.TriggerEvent(new GatherSaveDataEvent { SaveData = saveData });

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(_savePath, json);

            EventManager.TriggerEvent(new GameSavedEvent());
            Debug.Log("Game Saved!");
        }

        public void LoadGame()
        {
            if (File.Exists(_savePath))
            {
                Debug.Log("Loading game from: " + _savePath);
                string json = File.ReadAllText(_savePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                EventManager.TriggerEvent(new ApplySaveDataEvent { SaveData = saveData });

                EventManager.TriggerEvent(new GameLoadedEvent());
                Debug.Log("Game Loaded!");
            }
            else
            {
                Debug.LogWarning("No save file found to load.");
            }
        }
    }
}