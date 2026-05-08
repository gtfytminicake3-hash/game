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
        public float travelDuration;
        public float combatDuration;
    }

    public class ExpeditionManager : MonoBehaviour
    {
        private const long EXPLORATION_TIME_MS = 2000;
        private const long COMBAT_TURN_DURATION_MS = 3000; // Time per combat turn
        private const long TOWER_RECOVERY_DURATION_MS = 2 * 60 * 60 * 1000; // 2 hours

        private List<ActiveExpedition> _activeExpeditions;

        public static event Action<ExpeditionDisplayData> OnExpeditionStarted;
        public static event Action<ActiveExpedition> OnExpeditionStateChanged;
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
            bool hasChanges = false;

            foreach (var expedition in _activeExpeditions.ToList())
            {
                // Fallback for old save data without stateEndTimestamp
                if (expedition.stateEndTimestamp == 0)
                {
                    expedition.stateEndTimestamp = expedition.completionTimestamp;
                    expedition.currentState = ExpeditionState.Returning;
                }

                if (currentTime >= expedition.stateEndTimestamp)
                {
                    if (expedition.currentState == ExpeditionState.Traveling)
                    {
                        expedition.currentState = ExpeditionState.Exploring;
                        expedition.stateEndTimestamp += expedition.combatDurationMs;
                        OnExpeditionStateChanged?.Invoke(expedition);
                        hasChanges = true;
                    }
                    else if (expedition.currentState == ExpeditionState.Exploring)
                    {
                        expedition.currentState = ExpeditionState.Returning;
                        expedition.stateEndTimestamp += expedition.travelDurationMs;
                        OnExpeditionStateChanged?.Invoke(expedition);
                        hasChanges = true;
                    }
                    else if (expedition.currentState == ExpeditionState.Returning)
                    {
                        // Expedition Finished
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

                        // --- HOSPITAL LOGIC ---
                        if (GameManager.Instance != null && GameManager.Instance.HospitalSystem != null)
                        {
                            var result = expedition.preCalculatedReport.combatResult;
                            if (result != null)
                            {
                                if (result.PlayerCasualties != null)
                                {
                                    foreach (var casualty in result.PlayerCasualties)
                                    {
                                        GameManager.Instance.HospitalSystem.AdmitHero(casualty.id);
                                    }
                                }

                                if (result.PlayerSurvivors != null)
                                {
                                    foreach (var survivor in result.PlayerSurvivors)
                                    {
                                        var realHero = DataManager.Instance.GetHeroByID(survivor.id);
                                        if (realHero != null)
                                        {
                                            float maxHp = realHero.GetFinalStats().hp;
                                            if (realHero.currentHp < maxHp)
                                            {
                                                GameManager.Instance.HospitalSystem.InflictLightInjury(realHero);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // ----------------------
                        
                        Debug.Log($"Expedition {expedition.expeditionId} finished. Report moved to mailbox and injuries applied.");
                        hasChanges = true;
                    }
                }
            }

            if (hasChanges && DataManager.Instance != null)
            {
                DataManager.Instance.SavePlayerData();
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
            else if (destination.type == POIType.Boss)
            {
                StartBossExpedition(squadHeroIDs, destination);
            }
            else
            {
                StartNormalExpedition(squadHeroIDs, destination);
            }
        }

        private void StartNormalExpedition(List<string> squadHeroIDs, POIData destination)
        {
            long travelTime = CalculateTravelTime(destination.position);

            var heroSquad = squadHeroIDs.Select(id => DataManager.Instance.GetHeroByID(id)?.Clone()).Where(h => h != null).ToList();
            if (GameManager.Instance == null || GameManager.Instance.CombatSystem == null) return;

            CombatResult combatResult = GameManager.Instance.CombatSystem.Simulate(heroSquad, destination.monsterIDs, destination.difficultyLevel);

            long combatTimeMs = EXPLORATION_TIME_MS + (combatResult.TotalTurns * COMBAT_TURN_DURATION_MS);

            var report = new ExpeditionReport
            {
                poiId = destination.poiId,
                poiName = destination.poiName,
                combatResult = combatResult,
                loot = CalculateLoot(destination, combatResult.DidPlayerWin),
                experienceGained = CalculateExperience(destination, combatResult.DidPlayerWin)
            };

            CreateAndDispatchActiveExpedition(squadHeroIDs, destination, travelTime, combatTimeMs, report);
        }

        private void StartBossExpedition(List<string> squadHeroIDs, POIData destination)
        {
            long travelTime = CalculateTravelTime(destination.position);

            var heroSquad = squadHeroIDs.Select(id => DataManager.Instance.GetHeroByID(id)?.Clone()).Where(h => h != null).ToList();
            if (GameManager.Instance == null || GameManager.Instance.CombatSystem == null) return;

            string bossId = destination.monsterIDs != null && destination.monsterIDs.Count > 0 ? destination.monsterIDs[0] : "BOSS_01";
            
            CombatResult combatResult = GameManager.Instance.CombatSystem.SimulateBoss(heroSquad, bossId);

            long combatTimeMs = EXPLORATION_TIME_MS + (combatResult.TotalTurns * COMBAT_TURN_DURATION_MS);

            var report = new ExpeditionReport
            {
                poiId = destination.poiId,
                poiName = destination.poiName,
                combatResult = combatResult,
                // Bosses usually drop better loot and more exp
                loot = CalculateBossLoot(bossId, combatResult.DidPlayerWin),
                experienceGained = combatResult.DidPlayerWin ? 500 : 50
            };

            CreateAndDispatchActiveExpedition(squadHeroIDs, destination, travelTime, combatTimeMs, report);
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
            long totalCombatTimeMs = 0;
            bool isHealingChallenge = towerPoi.requiredProfession == Profession.Healer;
            bool towerConquered = false;

            for (int floor = towerPoi.currentFloor; floor <= 20; floor++)
            {
                // In healing challenge, we generate injured soldiers instead of monsters.
                var enemyMonsters = isHealingChallenge ? GenerateInjuredSoldiers(floor) : GetMonstersForTowerFloor(floor);
                
                var floorResult = GameManager.Instance.CombatSystem.Simulate(participatingHeroes, enemyMonsters, floor, isHealingChallenge);
                totalCombatTimeMs += floorResult.TotalTurns * COMBAT_TURN_DURATION_MS;

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
                TotalTurns = (int)(totalCombatTimeMs / COMBAT_TURN_DURATION_MS),
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

            long travelTimeMs = CalculateTravelTime(towerPoi.position);
            CreateAndDispatchActiveExpedition(squadHeroIDs, towerPoi, travelTimeMs, totalCombatTimeMs, report);
        }

        private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination, long travelTimeMs, long combatTimeMs, ExpeditionReport report)
        {
            long totalDurationMs = travelTimeMs * 2 + combatTimeMs;
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var newExpedition = new ActiveExpedition
            {
                expeditionId = Guid.NewGuid().ToString(),
                heroIds = heroIds,
                poiId = destination.poiId,
                currentState = ExpeditionState.Traveling,
                stateEndTimestamp = currentTime + travelTimeMs,
                travelDurationMs = travelTimeMs,
                combatDurationMs = combatTimeMs,
                completionTimestamp = currentTime + totalDurationMs, // Keep for backward compat
                preCalculatedReport = report
            };

            _activeExpeditions.Add(newExpedition);
            DataManager.Instance.SavePlayerData();

            var displayData = new ExpeditionDisplayData
            {
                expeditionId = newExpedition.expeditionId,
                destination = destination,
                travelDuration = travelTimeMs / 1000f,
                combatDuration = combatTimeMs / 1000f
            };
            OnExpeditionStarted?.Invoke(displayData);
            OnExpeditionStateChanged?.Invoke(newExpedition);
            Debug.Log($"Expedition {newExpedition.expeditionId} started. Current State: Traveling. Next state in {travelTimeMs / 1000f}s.");
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

        private List<string> GenerateInjuredSoldiers(int floor)
        {
            // For the healer challenge, we need to spawn dummy "monsters" that act as our injured soldiers.
            // We'll create temporary monster IDs that the CombatSystem will request from DataManager,
            // OR we must ensure DataManager.GetMonsterByID can handle these dynamic IDs,
            // OR we modify CombatSystem to accept HeroData objects for the enemy team directly.
            
            // To be safe and avoid touching CombatSystem's existing `List<string> enemyMonsterIDs` requirement,
            // We will add a special ID format that DataManager will intercept, 
            // OR better yet, we can intercept it in CombatSystem.
            // But actually, CombatSystem uses DataManager.Instance.GetMonsterByID(id, difficultyLevel).
            // Let's create a special Monster ID pattern: "INJURED_SOLDIER"
            
            var monsters = new List<string>();
            int soldierCount = 1 + (floor / 4); // 1 to 6 soldiers
            for (int i = 0; i < soldierCount; i++)
            {
                monsters.Add("INJURED_SOLDIER");
            }
            return monsters;
        }

        public LootData CalculateLoot(POIData poi, bool playerWon)
        {
            if (!playerWon) return new LootData();
            
            var loot = new LootData { 
                gold = 100 + (10 * poi.difficultyLevel),
                wood = UnityEngine.Random.Range(0, (5 * poi.difficultyLevel) + 1),
                stone = UnityEngine.Random.Range(0, (5 * poi.difficultyLevel) + 1)
            };

            // Process Rescue Mission Rewards
            if (poi.type == POIType.RescueMission)
            {
                int maxLvl = Mathf.Min(40, Mathf.Max(10, poi.difficultyLevel * 4)); 
                int minLvl = Mathf.Max(10, maxLvl / 2);
                int randomLvl = UnityEngine.Random.Range(minLvl, maxLvl + 1);
                
                Profession prof = (Profession)UnityEngine.Random.Range(0, 4);
                string[] possibleNames = { "Arthur", "Merlin", "Gawain", "Lancelot", "Morgan", "Guinevere", "Robin", "Tristan" };
                string randomName = possibleNames[UnityEngine.Random.Range(0, possibleNames.Length)];
                
                var rescued = new HeroData(Guid.NewGuid().ToString(), "Rescue " + randomName, (Gender)UnityEngine.Random.Range(0, 2));
                rescued.level = randomLvl;
                rescued.SetProfession(prof);
                rescued.isMature = true;
                
                // Base CP mục tiêu 400-1000 cho độ khó 1. Hệ số CP trung bình mỗi điểm potential là khoảng 62.
                // Do đó, potential 7-15 sẽ cho CP ~400-1000. Scale theo difficultyLevel.
                int minPotential = 7 * poi.difficultyLevel;
                int maxPotential = 15 * poi.difficultyLevel;
                rescued.potential = UnityEngine.Random.Range(minPotential, maxPotential + 1);
                rescued.CalculateBaseStats(); // Randomize các chỉ số hp, atk, def, spd dựa trên potential
                
                // Tích luỹ điểm cộng chỉ số (freeStatPoints) tương ứng với các level đã có sẵn
                rescued.freeStatPoints = rescued.potential * (randomLvl - 1);
                
                rescued.currentHp = rescued.GetFinalStats().hp;
                
                loot.rescuedHeroes.Add(rescued);
            }

            return loot;
        }

        public int CalculateExperience(POIData poi, bool playerWon)
        {
            return playerWon ? 50 + (5 * poi.difficultyLevel) : 0;
        }

        public LootData CalculateBossLoot(string bossId, bool playerWon)
        {
            if (!playerWon) return new LootData();
            
            var loot = new LootData { 
                gold = 2000,
                wood = 500,
                stone = 500,
                items = new Dictionary<string, int> { { "IT_EXP_BOOK_L", 2 }, { "IT_TICKET_RECRUIT", 3 } }
            };

            // Drop 1-2 pieces of high tier equipment
            int eqCount = UnityEngine.Random.Range(1, 3);
            for(int i = 0; i < eqCount; i++)
            {
                loot.equipments.Add(EquipmentSystem.GenerateRandomEquipment(30)); // Treat boss as floor 30 equivalent
            }
            
            return loot;
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

        public void ClaimReport(ExpeditionReport report, bool isX2)
        {
            if (report == null) return;
            if (DataManager.Instance == null || DataManager.Instance.Player == null) return;

            // Check if the report is actually unclaimed to ensure idempotency
            if (!DataManager.Instance.Player.UnclaimedReports.Contains(report))
            {
                Debug.LogWarning("Attempted to claim a report that is not in the unclaimed list.");
                return;
            }

            int rMulti = isX2 ? 2 : 1;

            // 1. Handle Loot
            if (report.loot != null && GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
            {
                GameManager.Instance.InventoryManager.AddGold(report.loot.gold * rMulti);
                GameManager.Instance.InventoryManager.AddResource(ResourceType.Wood, report.loot.wood * rMulti);
                GameManager.Instance.InventoryManager.AddResource(ResourceType.Stone, report.loot.stone * rMulti);
                
                foreach (var item in report.loot.items)
                {
                    GameManager.Instance.InventoryManager.AddItem(item.Key, item.Value * rMulti);
                }
                if (report.loot.equipments != null)
                {
                    foreach (var eq in report.loot.equipments)
                    {
                        GameManager.Instance.InventoryManager.AddEquipment(eq);
                    }
                }
                if (report.loot.rescuedHeroes != null && report.loot.rescuedHeroes.Count > 0)
                {
                    foreach (var hero in report.loot.rescuedHeroes)
                    {
                        DataManager.Instance.AddHero(hero);
                    }
                }
            }

            // 2. Handle Experience
            if (report.combatResult != null && report.combatResult.PlayerSurvivors != null)
            {
                foreach (var survivor in report.combatResult.PlayerSurvivors)
                {
                    var hero = DataManager.Instance.GetHeroByID(survivor.id);
                    if (hero != null)
                    { 
                        hero.AddExperience(report.experienceGained * rMulti);
                    }
                }
            }
            if (report.experienceGained > 0 && GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
            {
                GameManager.Instance.InventoryManager.AddPlayerExp(report.experienceGained * rMulti);
            }

            // 3. Remove report and save
            DataManager.Instance.Player.UnclaimedReports.Remove(report);
            DataManager.Instance.SavePlayerData();
            DataManager.TriggerReportClaimed();
        }
        #endregion
    }
}