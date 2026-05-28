using UnityEngine;
using ProjectWitchcraft.Core;
using System.IO;
using Newtonsoft.Json;

namespace ProjectWitchcraft.Managers
{
    public class SaveManager : Singleton<SaveManager>
    {
        private string _savePath;
        private const string SaveFileName = "savegame.json";

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        protected override void Awake()
        {
            base.Awake();
            _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        }

        private void OnEnable()
        {
            EventManager.AddListener<SaveRequestEvent>(OnSaveRequested);
            EventManager.AddListener<LoadRequestEvent>(OnLoadRequested);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<SaveRequestEvent>(OnSaveRequested);
            EventManager.RemoveListener<LoadRequestEvent>(OnLoadRequested);
        }

        private void OnSaveRequested(SaveRequestEvent e) => SaveGame();
        private void OnLoadRequested(LoadRequestEvent e) => LoadGame();

        public void SaveGame()
        {
            var saveData = new SaveData();
            EventManager.TriggerEvent(new GatherSaveDataEvent { SaveData = saveData });

            try
            {
                string json = JsonConvert.SerializeObject(saveData, JsonSettings);
                File.WriteAllText(_savePath, json);
                EventManager.TriggerEvent(new GameSavedEvent());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveManager] Save failed: {ex.Message}");
            }
        }

        public void LoadGame()
        {
            if (!File.Exists(_savePath)) return;

            try
            {
                string json = File.ReadAllText(_savePath);
                var saveData = JsonConvert.DeserializeObject<SaveData>(json, JsonSettings);
                if (saveData == null)
                {
                    Debug.LogError("[SaveManager] Deserialized save data is null.");
                    return;
                }
                EventManager.TriggerEvent(new ApplySaveDataEvent { SaveData = saveData });
                EventManager.TriggerEvent(new GameLoadedEvent());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveManager] Load failed: {ex.Message}");
            }
        }
    }
}
