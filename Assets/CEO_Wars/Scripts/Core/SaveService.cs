using System;
using System.IO;
using UnityEngine;

namespace CEOWars.Core
{
    public static class SaveService
    {
        private const string FileName = "ceowars_save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static GameState Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return new GameState();
                var json = File.ReadAllText(SavePath);
                var state = JsonUtility.FromJson<GameState>(json);
                return state ?? new GameState();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"CEO Wars save could not be loaded: {ex.Message}");
                return new GameState();
            }
        }

        public static void Save(GameState state)
        {
            try
            {
                state.lastSavedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(SavePath, JsonUtility.ToJson(state, true));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"CEO Wars save could not be written: {ex.Message}");
            }
        }

        public static void Delete()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }
    }
}
