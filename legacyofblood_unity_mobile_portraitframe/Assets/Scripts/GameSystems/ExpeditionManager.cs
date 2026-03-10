using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LegendOfBlood.Combat;

namespace LegendOfBlood
{
    public class ExpeditionDisplayData
    {
        public string expeditionId;
        public POIData destination;
        public float totalDuration;
    }

    public class ExpeditionManager : MonoBehaviour
    {
        private const long EXPLORATION_TIME_MS = 2000;
        private const long TOWER_BATTLE_TIME_MS = 3000; // Time per tower floor battle
        private const long TOWER_RECOVERY_DURATION_MS = 2 * 60 * 60 * 1000; // 2 hours

        private List<ActiveExpedition> _activeExpeditions;

        public static event Action<ExpeditionDisplayData> OnExpeditionStarted;
        public static event Action<string> OnExpeditionFinished;
        public static event Action OnNewReportReceived;
        public static event Action<POIData> OnTowerConquered;
        public static event Action<POIData> OnPOICleared;


        private void Start()
        {
            if (DataManager.Instance != null && DataManager.Instance.Player != null)
            {
                _activeExpeditions = DataManager.Instance.Player.ActiveExpeditions;
            }
            else
            {
                Debug.LogError("ExpeditionManager could not load expeditions.");
                _activeExpeditions = new List<ActiveExpedition>();
            }
        }

        public void Tick()
        {
            if (_activeExpeditions == null || !_activeExpeditions.Any()) return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var expedition in _activeExpeditions.ToList())
            {
                if (currentTime >= expedition.completionTimestamp)
                {
                    DataManager.Instance.Player.UnclaimedReports.Add(expedition.preCalculatedReport);
                    _activeExpeditions.Remove(expedition);

                    OnExpeditionFinished?.Invoke(expedition.expeditionId);
                    OnNewReportReceived?.Invoke();

                    // If the report indicates a POI was cleared, invoke that event
                    if (expedition.preCalculatedReport.combatResult.DidPlayerWin && expedition.preCalculatedReport.poiId != null)
                    {
                        var poi = DataManager.Instance.GetPOIByID(expedition.preCalculatedReport.poiId);
                        if (poi != null && poi.type != POIType.TowerOfTrials)
                        {
                            OnPOICleared?.Invoke(poi);
                        }
                    }
                    
                    Debug.Log($"Expedition {expedition.expeditionId} finished. Report moved to mailbox.");
                }
            }
        }

        public int GetMaxConcurrentExpeditions()
        {
            if (DataManager.Instance == null || DataManager.Instance.AllBuildings == null) return 1;
            
            var barracks = DataManager.Instance.AllBuildings.FirstOrDefault(b => b.id == "Barracks");
            int barrackLevel = barracks != null ? barracks.level : 1;
            
            // Default 1 squad. Gain +1 squad every 5 levels.
            return 1 + (barrackLevel / 5);
        }

        public bool CanStartNewExpedition()
        {
            return _activeExpeditions.Count < GetMaxConcurrentExpeditions();
        }

        public void StartExpedition(List<string> squadHeroIDs, POIData destination)
        {
            if (!CanStartNewExpedition())
            {
                Debug.LogWarning("Max concurrent expeditions reached. Cannot start another one.");
                if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
                {
                    // If LocalizationSystem is ready, we could fetch here, but we will hardcode the fallback for now.
                    string msg = global::LocalizationSystem.GetText("msg_max_expedition_reached");
                    if (string.IsNullOrEmpty(msg)) msg = $"Đã đạt giới hạn số đội viễn chinh tối đa ({GetMaxConcurrentExpeditions()}). Hãy nâng cấp Doanh Trại để gửi thêm.";
                    
                    GameManager.Instance.UINotificationManager.ShowNotification(msg);
                }
                return;
            }
            if (destination.type == POIType.TowerOfTrials)
            {
                StartTowerChallenge(squadHeroIDs, destination);
            }
            else
            {
                StartNormalExpedition(squadHeroIDs, destination);
            }
        }

        private void StartNormalExpedition(List<string> squadHeroIDs, POIData destination)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long travelTime = CalculateTravelTime(destination.position);
            long totalDuration = travelTime + EXPLORATION_TIME_MS + travelTime;

            var heroSquad = squadHeroIDs.Select(id => DataManager.Instance.GetHeroByID(id)?.Clone()).Where(h => h != null).ToList();
            if (GameManager.Instance == null || GameManager.Instance.CombatSystem == null) return;

            CombatResult combatResult = GameManager.Instance.CombatSystem.Simulate(heroSquad, destination.monsterIDs, destination.difficultyLevel);

            var report = new ExpeditionReport
            {
                poiId = destination.poiId,
                poiName = destination.poiName,
                combatResult = combatResult,
                loot = CalculateLoot(destination, combatResult.DidPlayerWin),
                experienceGained = CalculateExperience(destination, combatResult.DidPlayerWin)
            };

            CreateAndDispatchActiveExpedition(squadHeroIDs, destination, totalDuration, report);
        }

        private void StartTowerChallenge(List<string> squadHeroIDs, POIData towerPoi)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            // Check for cooldown
            if (currentTime < towerPoi.recoveryEndTime)
            {
                Debug.LogWarning(LocalizationSystem.GetText("msg_tower_in_recovery"));
                // In a real game, you'd show a user-facing message here
                if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
                {
                    GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("msg_tower_in_recovery"));
                }
                return;
            }
            // If cooldown has passed, reset progress
            if (towerPoi.currentFloor > 1 && currentTime >= towerPoi.recoveryEndTime)
            {
                towerPoi.currentFloor = 1;
            }

            var participatingHeroes = squadHeroIDs.Select(id => DataManager.Instance.GetHeroByID(id)?.Clone()).Where(h => h != null).ToList();
            var finalCombatLog = new List<string>();
            int floorsCleared = 0;
            bool towerConquered = false;

            for (int floor = towerPoi.currentFloor; floor <= 20; floor++)
            {
                var monsters = GetMonstersForTowerFloor(floor);
                var floorResult = GameManager.Instance.CombatSystem.Simulate(participatingHeroes, monsters, floor);

                finalCombatLog.Add($"<color=yellow>--- Tầng {floor} ---</color>");
                finalCombatLog.AddRange(floorResult.CombatLog);

                if (floorResult.DidPlayerWin)
                {
                    floorsCleared++;
                    // Update hero HP for the next battle
                    foreach(var survivor in floorResult.PlayerSurvivors)
                    {
                        var heroInSquad = participatingHeroes.FirstOrDefault(h => h.id == survivor.id);
                        if(heroInSquad != null) heroInSquad.currentHp = survivor.currentHp;
                    }
                }
                else
                {
                    // Player lost, end the challenge
                    towerPoi.currentFloor = floor;
                    towerPoi.recoveryEndTime = currentTime + TOWER_RECOVERY_DURATION_MS;
                    break;
                }

                if (floor == 20)
                {
                    towerConquered = true;
                }
            }

            CombatResult finalResult = new CombatResult
            {
                DidPlayerWin = towerConquered,
                CombatLog = finalCombatLog,
                PlayerSurvivors = participatingHeroes.Where(h => h.currentHp > 0).ToList(),
                PlayerCasualties = participatingHeroes.Where(h => h.currentHp <= 0).ToList()
            };

            ExpeditionReport report = new ExpeditionReport
            {
                poiId = towerPoi.poiId,
                poiName = $"{towerPoi.poiName} (Floors {towerPoi.currentFloor - floorsCleared}-{towerPoi.currentFloor})",
                combatResult = finalResult,
                loot = towerConquered ? CalculateTowerLoot(towerPoi.currentFloor) : new LootData(),
                experienceGained = CalculateTowerExperience(floorsCleared)
            };

            if (towerConquered)
            {
                OnTowerConquered?.Invoke(towerPoi);
            }

            long totalDuration = CalculateTravelTime(towerPoi.position) * 2 + (floorsCleared * TOWER_BATTLE_TIME_MS);
            CreateAndDispatchActiveExpedition(squadHeroIDs, towerPoi, totalDuration, report);
        }

        private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination, long duration, ExpeditionReport report)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var newExpedition = new ActiveExpedition
            {
                expeditionId = Guid.NewGuid().ToString(),
                heroIds = heroIds,
                poiId = destination.poiId,
                completionTimestamp = currentTime + duration,
                preCalculatedReport = report
            };

            _activeExpeditions.Add(newExpedition);

            var displayData = new ExpeditionDisplayData
            {
                expeditionId = newExpedition.expeditionId,
                destination = destination,
                totalDuration = duration / 1000f
            };
            OnExpeditionStarted?.Invoke(displayData);
            Debug.Log($"Expedition {newExpedition.expeditionId} started. Completion in {duration / 1000f}s.");
        }

        public bool IsHeroOnExpedition(string heroId)
        {
            return _activeExpeditions?.Any(exp => exp.heroIds.Contains(heroId)) ?? false;
        }

        private long CalculateTravelTime(Vector2 destination)
        {
            return (long)(Vector2.Distance(Vector2.zero, destination) * 100);
        }

        #region Placeholder Logic
        private List<string> GetMonstersForTowerFloor(int floor)
        {
            var towerConfigs = DataManager.Instance?.GameConfig?.TowerConfigs;
            if (towerConfigs != null && towerConfigs.Count > 0)
            {
                var floorConfig = towerConfigs.FirstOrDefault(t => t.floorIndex == floor);
                if (floorConfig != null && floorConfig.monsterPool != null && floorConfig.monsterPool.Count > 0)
                {
                    var resultMonsters = new List<string>();
                    int count = floorConfig.customMonsterCount > 0 ? floorConfig.customMonsterCount : (1 + (floor / 5));
                    for (int i = 0; i < count; i++)
                    {
                        resultMonsters.Add(floorConfig.monsterPool[UnityEngine.Random.Range(0, floorConfig.monsterPool.Count)]);
                    }
                    return resultMonsters;
                }
            }

            // Fallback just in case GameConfig is really missing, but using real names
            var fallbackPool = new List<string> { "Goblin", "Orc", "Slime" };
            var monsters = new List<string>();
            int monsterCount = 1 + (floor / 5);
            for(int i = 0; i < monsterCount; i++)
            {
                monsters.Add(fallbackPool[UnityEngine.Random.Range(0, fallbackPool.Count)]);
            }
            return monsters;
        }

        private LootData CalculateLoot(POIData poi, bool playerWon)
        {
            if (!playerWon) return new LootData();
            return new LootData { gold = 100 + (10 * poi.difficultyLevel) };
        }

        private int CalculateExperience(POIData poi, bool playerWon)
        {
            return playerWon ? 50 + (5 * poi.difficultyLevel) : 0;
        }

        private LootData CalculateTowerLoot(int maxFloorCleared)
        {
            // Special high-tier loot for conquering the tower
            var loot = new LootData();
            loot.gold = 1000 + (maxFloorCleared * 500);
            loot.items = new Dictionary<string, int> { { "IT_EXP_BOOK_S", 5 + maxFloorCleared }, { "IT_WISH_CHARM", 1 } };
            
            // Random equipment based on floor (Generates drops up to lv 40)
            EquipmentData eq = EquipmentSystem.GenerateRandomEquipment(maxFloorCleared);
            loot.equipments.Add(eq);
            
            // Nếu là những tầng chẵn hoặc cao, rớt thêm đồ cho vui
            if (maxFloorCleared >= 10 && UnityEngine.Random.value > 0.5f)
            {
                loot.equipments.Add(EquipmentSystem.GenerateRandomEquipment(maxFloorCleared));
            }
            
            return loot;
        }

        private int CalculateTowerExperience(int floorsCleared)
        {
            return floorsCleared * 100;
        }
        #endregion
    }
}