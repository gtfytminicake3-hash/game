#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace LegendOfBlood
{
    public static class DebugMenu
    {
        [MenuItem("LegendOfBlood/Debug/Add 10000 Gold")]
        static void AddGold()
        {
            if (!CheckPlayMode()) return;
            GameManager.Instance.InventoryManager.AddResource(ResourceType.Gold, 10000);
            Debug.Log("[Debug] Added 10000 Gold.");
        }

        [MenuItem("LegendOfBlood/Debug/Add 5 Heroes (Male+Female)")]
        static void AddTestHeroes()
        {
            if (!CheckPlayMode()) return;
            var dm = DataManager.Instance;
            for (int i = 0; i < 3; i++)
            {
                var male = new HeroData("debug_m_" + i, "TestHero_M" + i, Gender.Male);
                male.level = 10;
                male.potential = 15;
                male.isMature = true;
                male.CalculateBaseStats();
                male.SetProfession(Profession.Warrior);
                dm.AllHeroes.Add(male);
                dm.Player.Heroes.Add(male);
            }
            for (int i = 0; i < 2; i++)
            {
                var female = new HeroData("debug_f_" + i, "TestHero_F" + i, Gender.Female);
                female.level = 10;
                female.potential = 15;
                female.isMature = true;
                female.CalculateBaseStats();
                female.SetProfession(Profession.Healer);
                dm.AllHeroes.Add(female);
                dm.Player.Heroes.Add(female);
            }
            EventManager.TriggerEvent(GameEvents.OnHeroListChanged);
            Debug.Log("[Debug] Added 5 test heroes.");
        }

        [MenuItem("LegendOfBlood/Debug/Mature All Babies")]
        static void MatureAll()
        {
            if (!CheckPlayMode()) return;
            foreach (var h in DataManager.Instance.AllHeroes)
            {
                if (!h.isMature) h.maturationEndTime = 0;
            }
            Debug.Log("[Debug] Set all baby maturationEndTime to 0. Tick will mature them next frame.");
        }

        [MenuItem("LegendOfBlood/Debug/Force Complete All Expeditions")]
        static void ForceComplete()
        {
            if (!CheckPlayMode()) return;
            foreach (var e in DataManager.Instance.Player.ActiveExpeditions)
            {
                e.completionTimestamp = 0;
            }
            Debug.Log("[Debug] Set all expedition completionTimestamp to 0. Tick will complete them next frame.");
        }

        [MenuItem("LegendOfBlood/Debug/Clear Save File")]
        static void ClearSave()
        {
            string path = Path.Combine(Application.persistentDataPath, "legendofblood_save.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[Debug] Save file deleted: {path}. Restart Play mode to get fresh save.");
            }
            else
            {
                Debug.Log($"[Debug] No save file found at: {path}");
            }
        }

        [MenuItem("LegendOfBlood/Debug/Log Active Expeditions")]
        static void LogExpeditions()
        {
            if (!CheckPlayMode()) return;
            var exps = DataManager.Instance.Player.ActiveExpeditions;
            Debug.Log($"[Debug] Active expeditions: {exps.Count}");
            foreach (var e in exps)
            {
                long remaining = e.completionTimestamp - System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                Debug.Log($"  - {e.expeditionId} | heroes: [{string.Join(", ", e.heroIds)}] | completes in {remaining / 1000f:F1}s");
            }
        }

        private static bool CheckPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Debug] This command only works in Play Mode.");
                return false;
            }
            if (GameManager.Instance == null || DataManager.Instance == null)
            {
                Debug.LogWarning("[Debug] GameManager or DataManager not ready.");
                return false;
            }
            return true;
        }
    }
}
#endif
