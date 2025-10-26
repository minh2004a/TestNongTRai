using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Save
{
    /// <summary>
    /// Hệ thống Save/Load game
    /// Quản lý việc lưu và load tất cả game data
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "savegame";
        [SerializeField] private string saveFileExtension = ".json";
        [SerializeField] private bool useEncryption = false;
        [SerializeField] private bool prettyPrint = true;
        [SerializeField] private int maxAutoSaveSlots = 3;

        [Header("Auto Save")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 phút

        [Header("References")]
        [SerializeField] private Items.InventoryManager inventoryManager;

        [Header("Runtime Data")]
        [SerializeField] private string currentSaveSlot = "slot1";
        [SerializeField] private float timeSinceLastAutoSave = 0f;
        [SerializeField] private bool isSaving = false;
        [SerializeField] private bool isLoading = false;

        // Singleton
        private static SaveSystem _instance;
        public static SaveSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SaveSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SaveSystem");
                        _instance = go.AddComponent<SaveSystem>();
                    }
                }
                return _instance;
            }
        }

        // Properties
        public string CurrentSaveSlot => currentSaveSlot;
        public bool IsSaving => isSaving;
        public bool IsLoading => isLoading;
        public string SaveDirectory => Application.persistentDataPath + "/Saves/";

        // Events
        public event Action<string> OnSaveStarted;
        public event Action<string, bool> OnSaveCompleted; // saveSlot, success
        public event Action<string> OnLoadStarted;
        public event Action<string, bool> OnLoadCompleted; // saveSlot, success
        public event Action<string> OnSaveDeleted;
        public event Action OnAutoSave;

        private void Awake()
        {
            // Singleton setup
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Update()
        {
            // Auto save timer
            if (enableAutoSave && !isSaving && !isLoading)
            {
                timeSinceLastAutoSave += Time.deltaTime;

                if (timeSinceLastAutoSave >= autoSaveInterval)
                {
                    AutoSave();
                    timeSinceLastAutoSave = 0f;
                }
            }
        }

        private void Initialize()
        {
            // Validate references
            if (inventoryManager == null)
            {
                inventoryManager = FindObjectOfType<Items.InventoryManager>();
                if (inventoryManager == null)
                {
                    Debug.LogWarning("[SaveSystem] InventoryManager not found!");
                }
            }

            // Tạo save directory nếu chưa có
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
                Debug.Log($"[SaveSystem] Created save directory: {SaveDirectory}");
            }

        }

        #region Save Operations

        /// <summary>
        /// Save game vào slot hiện tại
        /// </summary>
        public bool SaveGame()
        {
            return SaveGame(currentSaveSlot);
        }

        /// <summary>
        /// Save game vào slot cụ thể
        /// </summary>
        public bool SaveGame(string saveSlot)
        {
            if (isSaving)
            {
                Debug.LogWarning("[SaveSystem] Already saving!");
                return false;
            }

            isSaving = true;
            OnSaveStarted?.Invoke(saveSlot);

            try
            {
                // Tạo save data
                SaveData saveData = CreateSaveData();

                // Serialize to JSON
                string json = JsonUtility.ToJson(saveData, prettyPrint);

                // Encrypt nếu cần
                if (useEncryption)
                {
                    json = EncryptData(json);
                }

                // Write to file
                string filePath = GetSaveFilePath(saveSlot);
                File.WriteAllText(filePath, json);

                Debug.Log($"[SaveSystem] Game saved to: {filePath}");
                OnSaveCompleted?.Invoke(saveSlot, true);

                isSaving = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
                OnSaveCompleted?.Invoke(saveSlot, false);

                isSaving = false;
                return false;
            }
        }

        /// <summary>
        /// Auto save
        /// </summary>
        private void AutoSave()
        {
            Debug.Log("[SaveSystem] Auto saving...");

            string autoSaveSlot = $"autosave_{DateTime.Now:yyyyMMdd_HHmmss}";
            bool success = SaveGame(autoSaveSlot);

            if (success)
            {
                OnAutoSave?.Invoke();
                CleanupOldAutoSaves();
            }
        }

        /// <summary>
        /// Tạo SaveData từ game state hiện tại
        /// </summary>
        private SaveData CreateSaveData()
        {
            SaveData data = new SaveData
            {
                saveVersion = "1.0",
                saveSlot = currentSaveSlot,
                saveTimestamp = DateTime.Now.Ticks,
                playTime = Time.time
            };

            // Save inventory data
            if (inventoryManager != null && inventoryManager.IsInitialized)
            {
                data.inventoryData = inventoryManager.SaveInventory();
            }
            else
            {
                Debug.LogWarning("[SaveSystem] Cannot save inventory - manager not initialized!");
            }

            // TODO: Save other game systems
            // data.playerData = playerManager.SavePlayer();
            // data.worldData = worldManager.SaveWorld();
            // data.farmData = farmManager.SaveFarm();

            return data;
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Load game từ slot hiện tại
        /// </summary>
        public bool LoadGame()
        {
            return LoadGame(currentSaveSlot);
        }

        /// <summary>
        /// Load game từ slot cụ thể
        /// </summary>
        public bool LoadGame(string saveSlot)
        {
            if (isLoading)
            {
                Debug.LogWarning("[SaveSystem] Already loading!");
                return false;
            }

            string filePath = GetSaveFilePath(saveSlot);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveSystem] Save file not found: {filePath}");
                OnLoadCompleted?.Invoke(saveSlot, false);
                return false;
            }

            isLoading = true;
            OnLoadStarted?.Invoke(saveSlot);

            try
            {
                // Read file
                string json = File.ReadAllText(filePath);

                // Decrypt nếu cần
                if (useEncryption)
                {
                    json = DecryptData(json);
                }

                // Deserialize
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                if (saveData == null)
                {
                    throw new Exception("Failed to deserialize save data!");
                }

                // Validate save data
                if (!saveData.Validate())
                {
                    throw new Exception("Save data validation failed!");
                }

                // Apply save data
                ApplySaveData(saveData);

                Debug.Log($"[SaveSystem] Game loaded from: {filePath}");
                OnLoadCompleted?.Invoke(saveSlot, true);

                isLoading = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed: {e.Message}");
                OnLoadCompleted?.Invoke(saveSlot, false);

                isLoading = false;
                return false;
            }
        }

        /// <summary>
        /// Apply loaded data vào game
        /// </summary>
        private void ApplySaveData(SaveData data)
        {
            // Load inventory
            if (data.inventoryData != null && inventoryManager != null)
            {
                inventoryManager.LoadInventory(data.inventoryData);
            }

            // TODO: Load other systems
            // if (data.playerData != null) playerManager.LoadPlayer(data.playerData);
            // if (data.worldData != null) worldManager.LoadWorld(data.worldData);
            // if (data.farmData != null) farmManager.LoadFarm(data.farmData);

            Debug.Log($"[SaveSystem] Applied save data: {data}");
        }

        #endregion

        #region Save Management

        /// <summary>
        /// Kiểm tra save file có tồn tại không
        /// </summary>
        public bool SaveExists(string saveSlot)
        {
            string filePath = GetSaveFilePath(saveSlot);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public bool DeleteSave(string saveSlot)
        {
            string filePath = GetSaveFilePath(saveSlot);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveSystem] Save file not found: {filePath}");
                return false;
            }

            try
            {
                File.Delete(filePath);
                Debug.Log($"[SaveSystem] Deleted save: {filePath}");
                OnSaveDeleted?.Invoke(saveSlot);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy tất cả save files
        /// </summary>
        public List<SaveFileInfo> GetAllSaves()
        {
            List<SaveFileInfo> saves = new List<SaveFileInfo>();

            if (!Directory.Exists(SaveDirectory))
            {
                return saves;
            }

            string[] files = Directory.GetFiles(SaveDirectory, $"*{saveFileExtension}");

            foreach (string file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);

                    if (useEncryption)
                    {
                        json = DecryptData(json);
                    }

                    SaveData data = JsonUtility.FromJson<SaveData>(json);

                    if (data != null)
                    {
                        saves.Add(new SaveFileInfo
                        {
                            saveSlot = data.saveSlot,
                            filePath = file,
                            saveTime = new DateTime(data.saveTimestamp),
                            playTime = data.playTime,
                            saveVersion = data.saveVersion
                        });
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveSystem] Failed to read save file {file}: {e.Message}");
                }
            }

            return saves;
        }

        /// <summary>
        /// Set current save slot
        /// </summary>
        public void SetCurrentSaveSlot(string saveSlot)
        {
            currentSaveSlot = saveSlot;
            Debug.Log($"[SaveSystem] Current save slot set to: {saveSlot}");
        }

        /// <summary>
        /// Clean up old auto saves (giữ lại maxAutoSaveSlots mới nhất)
        /// </summary>
        private void CleanupOldAutoSaves()
        {
            try
            {
                string[] autoSaves = Directory.GetFiles(SaveDirectory, $"autosave_*{saveFileExtension}");

                if (autoSaves.Length <= maxAutoSaveSlots)
                    return;

                // Sort by creation time
                Array.Sort(autoSaves, (a, b) => File.GetCreationTime(b).CompareTo(File.GetCreationTime(a)));

                // Delete old auto saves
                for (int i = maxAutoSaveSlots; i < autoSaves.Length; i++)
                {
                    File.Delete(autoSaves[i]);
                    Debug.Log($"[SaveSystem] Deleted old auto save: {autoSaves[i]}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to cleanup auto saves: {e.Message}");
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Lấy đường dẫn save file
        /// </summary>
        private string GetSaveFilePath(string saveSlot)
        {
            string fileName = $"{saveFileName}_{saveSlot}{saveFileExtension}";
            return Path.Combine(SaveDirectory, fileName);
        }

        /// <summary>
        /// Encrypt data (simple XOR - thay thế bằng encryption tốt hơn nếu cần)
        /// </summary>
        private string EncryptData(string data)
        {
            // TODO: Implement proper encryption
            // Đây chỉ là placeholder - dùng XOR encryption đơn giản
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Decrypt data
        /// </summary>
        private string DecryptData(string data)
        {
            // TODO: Implement proper decryption
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data));
        }

        #endregion

        #region Quick Save/Load

        /// <summary>
        /// Quick save (F5)
        /// </summary>
        public void QuickSave()
        {
            SaveGame("quicksave");
        }

        /// <summary>
        /// Quick load (F9)
        /// </summary>
        public void QuickLoad()
        {
            if (SaveExists("quicksave"))
            {
                LoadGame("quicksave");
            }
            else
            {
                Debug.LogWarning("[SaveSystem] No quick save found!");
            }
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Save Game")]
        private void DebugSaveGame()
        {
            SaveGame();
        }

        [ContextMenu("Load Game")]
        private void DebugLoadGame()
        {
            LoadGame();
        }

        [ContextMenu("Delete Current Save")]
        private void DebugDeleteSave()
        {
            DeleteSave(currentSaveSlot);
        }

        [ContextMenu("List All Saves")]
        private void DebugListSaves()
        {
            List<SaveFileInfo> saves = GetAllSaves();
            Debug.Log($"=== SAVE FILES ({saves.Count}) ===");

            foreach (var save in saves)
            {
                Debug.Log($"  {save.saveSlot} - {save.saveTime:yyyy-MM-dd HH:mm:ss} - Play Time: {save.playTime:F1}s");
            }
        }

        [ContextMenu("Open Save Directory")]
        private void DebugOpenSaveDirectory()
        {
            Application.OpenURL(SaveDirectory);
        }

        #endregion
    }

    #region Save Data Structures

    /// <summary>
    /// Master save data container
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public string saveVersion;
        public string saveSlot;
        public long saveTimestamp;
        public float playTime;

        // Game data
        public Items.InventoryData inventoryData;
        // TODO: Add other save data
        // public PlayerData playerData;
        // public WorldData worldData;
        // public FarmData farmData;

        public DateTime SaveTime => new DateTime(saveTimestamp);

        public bool Validate()
        {
            if (string.IsNullOrEmpty(saveVersion))
            {
                Debug.LogWarning("[SaveData] Invalid save version!");
                return false;
            }

            if (inventoryData != null && !inventoryData.Validate())
            {
                Debug.LogWarning("[SaveData] Invalid inventory data!");
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"[SaveData] Version: {saveVersion}, Slot: {saveSlot}, Time: {SaveTime:yyyy-MM-dd HH:mm:ss}, PlayTime: {playTime:F1}s";
        }
    }

    /// <summary>
    /// Save file info (metadata)
    /// </summary>
    [System.Serializable]
    public class SaveFileInfo
    {
        public string saveSlot;
        public string filePath;
        public DateTime saveTime;
        public float playTime;
        public string saveVersion;

        public override string ToString()
        {
            return $"[{saveSlot}] {saveTime:yyyy-MM-dd HH:mm:ss} - {playTime:F1}s";
        }
    }

    #endregion
}