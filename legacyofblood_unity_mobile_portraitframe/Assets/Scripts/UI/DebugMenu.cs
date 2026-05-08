using UnityEngine;

namespace LegendOfBlood
{
    public class DebugMenu : MonoBehaviour
    {
        private bool _showDebug = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12) || (Input.touchCount == 3 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                _showDebug = !_showDebug;
            }
        }

        private void OnGUI()
        {
            if (!_showDebug) return;

            GUILayout.BeginArea(new Rect(10, 10, 350, Screen.height - 20));
            GUILayout.Box("DEBUG MENU (F12 or 3 Fingers)");

            if (GUILayout.Button("Add 100,000 Gold", GUILayout.Height(50)))
            {
                GameManager.Instance.InventoryManager.AddGold(100000);
            }

            if (GUILayout.Button("Mature All Babies", GUILayout.Height(50)))
            {
                foreach (var hero in DataManager.Instance.AllHeroes)
                {
                    if (!hero.isMature)
                    {
                        hero.isMature = true;
                        hero.SetProfession(Profession.Warrior); // Default profession for babies
                        hero.level = 1;
                        hero.CalculateBaseStats();
                        hero.currentHp = hero.GetFinalStats().hp;
                    }
                }
                DataManager.Instance.SavePlayerData();
            }

            if (GUILayout.Button("Complete All Expeditions", GUILayout.Height(50)))
            {
                var expeditions = DataManager.Instance.Player.ActiveExpeditions;
                foreach (var exp in expeditions)
                {
                    exp.stateEndTimestamp = 0; // Force complete
                }
            }

            if (GUILayout.Button("Clear Save Data (RESTART REQUIRED)", GUILayout.Height(50)))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.LogWarning("Save data cleared! Please restart the game.");
            }

            if (GUILayout.Button("Close Debug Menu", GUILayout.Height(50)))
            {
                _showDebug = false;
            }

            GUILayout.EndArea();
        }
    }
}
