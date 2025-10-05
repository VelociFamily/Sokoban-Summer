using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Lightweight JSON-backed save system with atomic writes and backup.
    /// Stores small data models under Application.persistentDataPath/Saves.
    /// </summary>
    public class SaveService
    {
        private static SaveService _instance;
        public static SaveService Instance => _instance ??= new SaveService();

        private readonly string _rootDir;

        private SaveService()
        {
            _rootDir = Path.Combine(Application.persistentDataPath, "Saves");
            try
            {
                if (!Directory.Exists(_rootDir)) Directory.CreateDirectory(_rootDir);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to ensure save directory: {_rootDir}. {e}");
            }
        }

        private string PathFor(string key) => Path.Combine(_rootDir, key + ".json");
        private string BackupPathFor(string key) => Path.Combine(_rootDir, key + ".bak");
        private string TempPathFor(string key) => Path.Combine(_rootDir, key + ".tmp");

        public bool Exists(string key) => File.Exists(PathFor(key));

        public bool TryLoad<T>(string key, out T data) where T : class
        {
            data = null;
            var path = PathFor(key);
            try
            {
                if (!File.Exists(path))
                {
                    // Try backup if primary is missing
                    var bak = BackupPathFor(key);
                    if (!File.Exists(bak)) return false;
                    var bakJson = File.ReadAllText(bak, Encoding.UTF8);
                    data = JsonUtility.FromJson<T>(bakJson);
                    return data != null;
                }

                var json = File.ReadAllText(path, Encoding.UTF8);
                data = JsonUtility.FromJson<T>(json);
                if (data != null) return true;

                // Attempt backup on parse failure
                var backup = BackupPathFor(key);
                if (File.Exists(backup))
                {
                    var bakJson = File.ReadAllText(backup, Encoding.UTF8);
                    data = JsonUtility.FromJson<T>(bakJson);
                    return data != null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to load '{key}': {e}");
            }
            return false;
        }

        public T LoadOrCreate<T>(string key, Func<T> createDefault) where T : class
        {
            if (TryLoad<T>(key, out var existing) && existing != null) return existing;
            var fresh = createDefault != null ? createDefault() : Activator.CreateInstance<T>();
            Save(key, fresh);
            return fresh;
        }

        public void Save<T>(string key, T data) where T : class
        {
            if (data == null)
            {
                Debug.LogWarning($"[SaveService] Attempted to save null data for key '{key}'");
                return;
            }

            var finalPath = PathFor(key);
            var tempPath = TempPathFor(key);
            var backupPath = BackupPathFor(key);

            try
            {
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(tempPath, json, Encoding.UTF8);

                // Create/refresh backup if final exists
                if (File.Exists(finalPath))
                {
                    try
                    {
                        File.Copy(finalPath, backupPath, true);
                    }
                    catch (Exception be)
                    {
                        Debug.LogWarning($"[SaveService] Could not create backup for '{key}': {be.Message}");
                    }
                }

                // Atomic replace when available, otherwise move
                try
                {
                    File.Replace(tempPath, finalPath, backupPath, true);
                }
                catch (PlatformNotSupportedException)
                {
                    if (File.Exists(finalPath)) File.Delete(finalPath);
                    File.Move(tempPath, finalPath);
                }
                catch (IOException)
                {
                    if (File.Exists(finalPath)) File.Delete(finalPath);
                    File.Move(tempPath, finalPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to save '{key}': {e}");
                // Cleanup temp if something went wrong
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { /* ignore */ }
            }
        }

        public void Delete(string key)
        {
            var finalPath = PathFor(key);
            var backupPath = BackupPathFor(key);
            var tempPath = TempPathFor(key);
            try { if (File.Exists(finalPath)) File.Delete(finalPath); } catch (Exception e) { Debug.LogWarning($"[SaveService] Could not delete {finalPath}: {e.Message}"); }
            try { if (File.Exists(backupPath)) File.Delete(backupPath); } catch (Exception e) { Debug.LogWarning($"[SaveService] Could not delete {backupPath}: {e.Message}"); }
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch (Exception e) { Debug.LogWarning($"[SaveService] Could not delete {tempPath}: {e.Message}"); }
        }
    }
}
