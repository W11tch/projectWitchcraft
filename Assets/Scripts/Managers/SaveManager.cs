using UnityEngine;
using ProjectWitchcraft.Core;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
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
            StartCoroutine(LoadGameCoroutine());
        }

        private IEnumerator LoadGameCoroutine()
        {
            GameManager.Instance.UpdateState(GameState.Loading);

            // File read and JSON deserialization on a background thread.
            string json = null;
            SaveData saveData = null;
            System.Exception loadError = null;

            var task = Task.Run(() =>
            {
                json = File.ReadAllText(_savePath);
                saveData = JsonConvert.DeserializeObject<SaveData>(json, JsonSettings);
            });

            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"[SaveManager] Load failed: {task.Exception?.InnerException?.Message}");
                GameManager.Instance.UpdateState(GameState.Playing);
                yield break;
            }

            if (saveData == null)
            {
                Debug.LogError("[SaveManager] Deserialized save data is null.");
                GameManager.Instance.UpdateState(GameState.Playing);
                yield break;
            }

            // Fast listeners: inventory, player position, equipment, world items.
            EventManager.TriggerEvent(new ApplySaveDataEvent { SaveData = saveData });

            // Heavy listener: placed objects — spread across frames.
            yield return ChunkManager.Instance.RestoreObjectsCoroutine(saveData.placedObjects);

            GameManager.Instance.UpdateState(GameState.Playing);
            EventManager.TriggerEvent(new GameLoadedEvent());
        }
    }
}
