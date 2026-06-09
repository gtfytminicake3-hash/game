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
        private const long COMBAT_TURN_DURATION_MS = 3000;
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

                        if (expedition.preCalculatedReport != null && !expedition.preCalculatedReport.combatResult.DidPlayerWin)
                        {
                            GameManager.Instance?.UINotificationManager?.ShowNotification(
                                $"Äá»™i thÃ¡m hiá»ƒm táº¡i {expedition.preCalculatedReport.poiName} Ä‘Ã£ THáº¤T Báº I vÃ  Ä‘ang rÃºt lui vá» cÄƒn cá»©!");
                        }
                    }
                    else if (expedition.currentState == ExpeditionState.Returning)
                    {
                        if (!string.IsNullOrEmpty(expedition.nodeId))
                        {
                            ApplyPendingNodeBattleResult(expedition);
                            hasChanges = true;
                        }
                        else
                        {
                            DataManager.Instance.Player.UnclaimedReports.Add(expedition.preCalculatedReport);
                            _activeExpeditions.Remove(expedition);

                            OnExpeditionFinished?.Invoke(expedition.expeditionId);
                            OnNewReportReceived?.Invoke();

                            GameManager.Instance?.UINotificationManager?.ShowNotification(
                                $"Äá»™i thÃ¡m hiá»ƒm Ä‘Ã£ trá»Ÿ vá» tá»« {expedition.preCalculatedReport.poiName}. Vui lÃ²ng kiá»ƒm tra HÃ²m ThÆ°!");

                            if (expedition.preCalculatedReport.combatResult.DidPlayerWin && expedition.preCalculatedReport.poiId != null)
                            {
                                var poi = DataManager.Instance.GetPOIByID(expedition.preCalculatedReport.poiId);
                                if (poi != null && poi.type != POIType.TowerOfTrials)
                                    OnPOICleared?.Invoke(poi);
                            }

                            // Chá»‰ xá»­ lÃ½ hospital náº¿u lÃ  Boss Victory (NodeMap Boss hoáº·c WorldMap Boss)
                            if (expedition.preCalculatedReport.isBossVictory)
                            {
                                // 1. ThÃªm report vÃ o mailbox
                                AddReportToMailbox(expedition.preCalculatedReport);
                                // 2. Hospital logic
                                ProcessHospitalAfterBossVictory(expedition.preCalculatedReport);
                                // 3. Clear Node Map lock
                                CompleteExpeditionAfterBossVictory(expedition.preCalculatedReport.poiId);
                            }
                            else
                            {
                                // World Map Node ThÆ°á»ng hoáº·c Tower
                                DataManager.Instance.Player.UnclaimedReports.Add(expedition.preCalculatedReport);
                                OnNewReportReceived?.Invoke();
                            }

                            Debug.Log($"[ExpeditionManager] Expedition {expedition.expeditionId} finished. " +
                                      $"Hospital processed={expedition.preCalculatedReport.isBossVictory}.");
                            hasChanges = true;
                        }
                    }
                }
            }

            if (hasChanges && DataManager.Instance != null)
                DataManager.Instance.SavePlayerData();
        }

        // =====================================================================
        // PUBLIC API - EXPEDITION LIFECYCLE (ORDERED)
        // =====================================================================

        /// <summary>
        /// BÆ¯á»šC 1: Chá»‰ thÃªm report vÃ o HÃ²m ThÆ°. KHÃ”NG gá»i hospital.
        /// Hospital chá»‰ Ä‘Æ°á»£c xá»­ lÃ½ sau khi Boss Ä‘Ã£ bá»‹ Ä‘Ã¡nh báº¡i.
        /// </summary>
        public void AddReportToMailbox(ExpeditionReport report)
        {
            if (report == null || DataManager.Instance?.Player == null) return;

            DataManager.Instance.Player.UnclaimedReports.Add(report);
            OnNewReportReceived?.Invoke();
            DataManager.Instance.SavePlayerData();

            Debug.Log($"[ExpeditionManager] AddReportToMailbox: POI={report.poiName}, " +
                      $"isBossVictory={report.isBossVictory}. Hospital NOT triggered here.");
        }

        /// <summary>
        /// BÆ¯á»šC 2: Xá»­ lÃ½ hospital SAU KHI boss bá»‹ Ä‘Ã¡nh báº¡i. Chá»‰ gá»i vá»›i BossVictoryReport.
        /// Guard check: náº¿u report.isBossVictory = false â†’ bá» qua, khÃ´ng lÃ m gÃ¬.
        /// Thá»© tá»± báº¯t buá»™c: AddReportToMailbox â†’ ProcessHospitalAfterBossVictory â†’ CompleteExpeditionAfterBossVictory.
        /// </summary>
        public void ProcessHospitalAfterBossVictory(ExpeditionReport report)
        {
            if (report == null || !report.isBossVictory)
            {
                Debug.LogWarning($"[ExpeditionManager] ProcessHospitalAfterBossVictory: Ignored. " +
                                 $"report={report?.poiName}, isBossVictory={report?.isBossVictory}");
                return;
            }

            Debug.Log($"[Hospital] Hospital processing started. Report={report.poiName}, BossNodeId={report.bossNodeId}");

            int injuredCount = 0;
            int deadCount = 0;

            if (report.heroOutcomeSnapshots != null)
            {
                foreach (var snapshot in report.heroOutcomeSnapshots)
                {
                    if (snapshot.isDead || snapshot.injurySeverity == 2)
                    {
                        deadCount++;
                        Debug.Log($"[Hospital] Admitting casualty: id={snapshot.heroId}, maxHp={snapshot.maxHp}");
                        GameManager.Instance?.HospitalSystem?.AdmitHero(snapshot.heroId);
                    }
                    else if (snapshot.hpAfterBattle < snapshot.maxHp)
                    {
                        var realHero = DataManager.Instance.GetHeroByID(snapshot.heroId);
                        if (realHero != null)
                        {
                            // Äáº£m báº£o HP tháº­t khá»›p vá»›i Snapshot trÆ°á»›c khi nháº­p viá»‡n
                            realHero.currentHp = snapshot.hpAfterBattle; 
                            injuredCount++;
                            Debug.Log($"[Hospital] Light injury: {realHero.heroName}, HP={snapshot.hpAfterBattle}/{snapshot.maxHp}");
                            GameManager.Instance?.HospitalSystem?.InflictLightInjury(realHero);
                        }
                        else
                        {
                            Debug.LogWarning($"[Hospital] Hero {snapshot.heroId} not found, but has HP loss in snapshot.");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"[Hospital] heroOutcomeSnapshots is null! Cannot process injuries for Boss Victory.");
            }

            Debug.Log($"[Hospital] Processing complete. Dead={deadCount}, Injured={injuredCount}. Report={report.poiName}");
            DataManager.Instance.SavePlayerData();
        }

        /// <summary>
        /// BÆ¯á»šC 3: Káº¿t thÃºc expedition session SAU KHI hospital Ä‘Ã£ Ä‘Æ°á»£c xá»­ lÃ½.
        /// Gá»i cuá»‘i cÃ¹ng trong chuá»—i: AddReportToMailbox â†’ ProcessHospitalAfterBossVictory â†’ CompleteExpeditionAfterBossVictory.
        /// </summary>
        public void CompleteExpeditionAfterBossVictory(string poiId)
        {
            Debug.Log($"[ExpeditionManager] Expedition completed after boss victory. POI={poiId}");
            
            // XoÃ¡ lock squad vÃ  map trong POI_InfoPanel
            POI_InfoPanel.UnlockSquadAndMap(poiId);
            
            Debug.Log($"[ExpeditionManager] Expedition session cleared. Squad editing is now unlocked.");
        }

        // =====================================================================
        // RETURN TRIP CREATION (NODE MAP)
        // =====================================================================

        /// <summary>
        /// Táº¡o chuyáº¿n Ä‘i Returning 5 giÃ¢y sau khi tháº¯ng Boss Node Map.
        /// Háº¿t 5 giÃ¢y, ExpeditionManager.Tick() sáº½ gá»i Hospital logic.
        /// </summary>
        public void StartReturnTripFromBossVictory(ExpeditionReport report, List<string> squadIds, POIData destination)
        {
            if (report == null || destination == null) return;

            long returnTimeMs = 5000; // Táº¡m thá»i 5 giÃ¢y theo yÃªu cáº§u
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var newExpedition = new ActiveExpedition
            {
                expeditionId = Guid.NewGuid().ToString(),
                heroIds = squadIds,
                poiId = destination.poiId,
                currentState = ExpeditionState.Returning, // Báº¯t Ä‘áº§u á»Ÿ Returning
                stateEndTimestamp = currentTime + returnTimeMs,
                travelDurationMs = returnTimeMs, // DÃ¹ng travel duration Ä‘á»ƒ chá»©a thá»i gian
                combatDurationMs = 0,
                completionTimestamp = currentTime + returnTimeMs,
                preCalculatedReport = report
            };

            _activeExpeditions.Add(newExpedition);
            DataManager.Instance.SavePlayerData();

            var displayData = new ExpeditionDisplayData
            {
                expeditionId = newExpedition.expeditionId,
                destination = destination,
                travelDuration = returnTimeMs / 1000f,
                combatDuration = 0f
            };

            OnExpeditionStarted?.Invoke(displayData);
            OnExpeditionStateChanged?.Invoke(newExpedition);
            Debug.Log($"[ExpeditionManager] Return Trip created for Node Boss {destination.poiName}. Squad locked for {returnTimeMs / 1000f}s.");
        }

        // =====================================================================
        // HERO & EXPEDITION STATE
        // =====================================================================

        public bool IsHeroOnExpedition(string heroId)
        {
            return _activeExpeditions?.Any(exp => exp.heroIds.Contains(heroId)) ?? false;
        }

        public int GetMaxConcurrentExpeditions()
        {
            if (DataManager.Instance == null || DataManager.Instance.AllBuildings == null) return 1;
            var barracks = DataManager.Instance.AllBuildings.FirstOrDefault(b => b.id == "Barracks");
            int barrackLevel = barracks != null ? barracks.level : 1;
            return 1 + (barrackLevel / 5);
        }

        public bool CanStartNewExpedition()
        {
            return _activeExpeditions.Count < GetMaxConcurrentExpeditions();
        }

        // =====================================================================
        // START EXPEDITION (WorldMap path - khÃ´ng pháº£i Node Map)
        // =====================================================================

        public void StartExpedition(List<string> squadHeroIDs, POIData destination)
        {
            if (!CanStartNewExpedition())
            {
                Debug.LogWarning("Max concurrent expeditions reached.");
                string msg = global::LocalizationSystem.GetText("msg_max_expedition_reached");
                if (string.IsNullOrEmpty(msg))
                    msg = $"ÄÃ£ Ä‘áº¡t giá»›i háº¡n sá»‘ Ä‘á»™i viá»…n chinh tá»‘i Ä‘a ({GetMaxConcurrentExpeditions()}). HÃ£y nÃ¢ng cáº¥p Doanh Tráº¡i Ä‘á»ƒ gá»­i thÃªm.";
                GameManager.Instance?.UINotificationManager?.ShowNotification(msg);
                return;
            }

            if (destination.type == POIType.TowerOfTrials)
                StartTowerChallenge(squadHeroIDs, destination);
            else if (destination.type == POIType.Boss)
                StartBossExpedition(squadHeroIDs, destination);
            else
                StartNormalExpedition(squadHeroIDs, destination);
        }

        private void StartNormalExpedition(List<string> squadHeroIDs, POIData destination)
        {
            long travelTime = CalculateTravelTime(destination.position);
            var heroSquad = squadHeroIDs.Select(id => DataManager.Instance.GetHeroByID(id)?.Clone()).Where(h => h != null).ToList();
            if (GameManager.Instance?.CombatSystem == null) return;

            CombatResult combatResult = GameManager.Instance.CombatSystem.Simulate(heroSquad, destination.monsterIDs, destination.difficultyLevel);
            long combatTimeMs = EXPLORATION_TIME_MS + (combatResult.TotalTurns * COMBAT_TURN_DURATION_MS);

            var report = new ExpeditionReport
            {
                poiId = destination.poiId,
                poiName = destination.poiName,
                combatResult = combatResult,
                loot = CalculateLoot(destination, combatResult.DidPlayerWin),
                experienceGained = CalculateExperience(destination, combatResult.DidPlayerWin),
                isBossVictory = false // Node thÆ°á»ng khÃ´ng kÃ­ch hoáº¡t hospital
            };

            CreateAndDispatchActiveExpedition(squadHeroIDs, destination, travelTime, combatTimeMs, report);
        }

        private void StartBossExpedition(List<string> squadHeroIDs, POIData destination)
        {
            long travelTime = CalculateTravelTime(destination.position);
            var heroSquad = squadHeroIDs.Select(id => DataManager.Instance.GetHeroByID(id)?.Clone()).Where(h => h != null).ToList();
            if (GameManager.Instance?.CombatSystem == null) return;

            string bossId = destination.monsterIDs != null && destination.monsterIDs.Count > 0
                ? destination.monsterIDs[0] : "BOSS_01";

            CombatResult combatResult = GameManager.Instance.CombatSystem.SimulateBoss(heroSquad, bossId);
            long combatTimeMs = EXPLORATION_TIME_MS + (combatResult.TotalTurns * COMBAT_TURN_DURATION_MS);

            var report = new ExpeditionReport
            {
                poiId = destination.poiId,
                poiName = destination.poiName,
                combatResult = combatResult,
                loot = CalculateBossLoot(bossId, combatResult.DidPlayerWin),
                experienceGained = combatResult.DidPlayerWin ? 500 : 50,
                // WorldMap Boss: isBossVictory chá»‰ true khi tháº¯ng
                isBossVictory = combatResult.DidPlayerWin,
                bossNodeId = bossId,
                squadIds = new List<string>(squadHeroIDs)
            };

            CreateAndDispatchActiveExpedition(squadHeroIDs, destination, travelTime, combatTimeMs, report);
        }

        private void StartTowerChallenge(List<string> squadHeroIDs, POIData towerPoi)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (currentTime < towerPoi.recoveryEndTime)
            {
                Debug.LogWarning(LocalizationSystem.GetText("msg_tower_in_recovery"));
                GameManager.Instance?.UINotificationManager?.ShowNotification(
                    LocalizationSystem.GetText("msg_tower_in_recovery"));
                return;
            }
            if (towerPoi.currentFloor > 1 && currentTime >= towerPoi.recoveryEndTime)
                towerPoi.currentFloor = 1;

            var participatingHeroes = squadHeroIDs.Select(id => DataManager.Instance.GetHeroByID(id)?.Clone()).Where(h => h != null).ToList();
            var finalCombatLog = new List<string>();
            int floorsCleared = 0;
            long totalCombatTimeMs = 0;
            bool isHealingChallenge = towerPoi.requiredProfession == Profession.Healer;
            bool towerConquered = false;

            for (int floor = towerPoi.currentFloor; floor <= 20; floor++)
            {
                var enemyMonsters = isHealingChallenge ? GenerateInjuredSoldiers(floor) : GetMonstersForTowerFloor(floor);
                var floorResult = GameManager.Instance.CombatSystem.Simulate(participatingHeroes, enemyMonsters, floor, isHealingChallenge);
                totalCombatTimeMs += floorResult.TotalTurns * COMBAT_TURN_DURATION_MS;

                finalCombatLog.Add($"<color=yellow>--- Táº§ng {floor} ---</color>");
                finalCombatLog.AddRange(floorResult.CombatLog);

                if (floorResult.DidPlayerWin)
                {
                    floorsCleared++;
                    foreach (var survivor in floorResult.PlayerSurvivors)
                    {
                        var heroInSquad = participatingHeroes.FirstOrDefault(h => h.id == survivor.id);
                        if (heroInSquad != null) heroInSquad.currentHp = survivor.currentHp;
                    }
                }
                else
                {
                    towerPoi.currentFloor = floor;
                    towerPoi.recoveryEndTime = currentTime + TOWER_RECOVERY_DURATION_MS;
                    break;
                }

                if (floor == 20) towerConquered = true;
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
                experienceGained = CalculateTowerExperience(floorsCleared),
                isBossVictory = towerConquered // Tower: hospital chá»‰ khi chinh phá»¥c xong thÃ¡p
            };

            if (towerConquered) OnTowerConquered?.Invoke(towerPoi);

            long travelTimeMs = CalculateTravelTime(towerPoi.position);
            CreateAndDispatchActiveExpedition(squadHeroIDs, towerPoi, travelTimeMs, totalCombatTimeMs, report);
        }

        private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination,
            long travelTimeMs, long combatTimeMs, ExpeditionReport report, string nodeId = null)
        {
            long totalDurationMs = travelTimeMs * 2 + combatTimeMs;
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var newExpedition = new ActiveExpedition
            {
                expeditionId = Guid.NewGuid().ToString(),
                heroIds = heroIds,
                poiId = destination.poiId,
                nodeId = nodeId,
                currentState = ExpeditionState.Traveling,
                stateEndTimestamp = currentTime + travelTimeMs,
                travelDurationMs = travelTimeMs,
                combatDurationMs = combatTimeMs,
                completionTimestamp = currentTime + totalDurationMs,
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
            Debug.Log($"Expedition {newExpedition.expeditionId} started. " +
                      $"State: Traveling. Next state in {travelTimeMs / 1000f}s.");
        }

        // =====================================================================
        // CLAIM REPORT
        // =====================================================================

        public void ClaimReport(ExpeditionReport report, bool isX2)
        {
            if (report == null) return;
            if (DataManager.Instance?.Player == null) return;

            if (!DataManager.Instance.Player.UnclaimedReports.Contains(report))
            {
                Debug.LogWarning("Attempted to claim a report that is not in the unclaimed list.");
                return;
            }

            int rMulti = isX2 ? 2 : 1;

            if (report.loot != null && GameManager.Instance?.InventoryManager != null)
            {
                GameManager.Instance.InventoryManager.AddGold(report.loot.gold * rMulti);
                GameManager.Instance.InventoryManager.AddResource(ResourceType.Wood, report.loot.wood * rMulti);
                GameManager.Instance.InventoryManager.AddResource(ResourceType.Stone, report.loot.stone * rMulti);

                foreach (var item in report.loot.items)
                    GameManager.Instance.InventoryManager.AddItem(item.Key, item.Value * rMulti);

                if (report.loot.equipments != null)
                    foreach (var eq in report.loot.equipments)
                        GameManager.Instance.InventoryManager.AddEquipment(eq);

                if (report.loot.rescuedHeroes != null)
                    foreach (var hero in report.loot.rescuedHeroes)
                        DataManager.Instance.AddHero(hero);
            }

            if (report.combatResult?.PlayerSurvivors != null)
            {
                foreach (var survivor in report.combatResult.PlayerSurvivors)
                {
                    var hero = DataManager.Instance.GetHeroByID(survivor.id);
                    if (hero != null) hero.AddExperience(report.experienceGained * rMulti);
                }
            }

            if (report.experienceGained > 0 && GameManager.Instance?.InventoryManager != null)
                GameManager.Instance.InventoryManager.AddPlayerExp(report.experienceGained * rMulti);

            DataManager.Instance.Player.UnclaimedReports.Remove(report);
            DataManager.Instance.SavePlayerData();
            DataManager.TriggerReportClaimed();
        }

        // =====================================================================
        // LOOT & EXPERIENCE CALCULATORS
        // =====================================================================

        public LootData CalculateLoot(POIData poi, bool playerWon)
        {
            if (!playerWon) return new LootData();

            var loot = new LootData
            {
                gold = 100 + (10 * poi.difficultyLevel),
                wood = UnityEngine.Random.Range(0, (5 * poi.difficultyLevel) + 1),
                stone = UnityEngine.Random.Range(0, (5 * poi.difficultyLevel) + 1)
            };

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

                int minPotential = 7 * poi.difficultyLevel;
                int maxPotential = 15 * poi.difficultyLevel;
                rescued.potential = UnityEngine.Random.Range(minPotential, maxPotential + 1);
                rescued.CalculateBaseStats();
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

            var loot = new LootData
            {
                gold = 2000,
                wood = 500,
                stone = 500,
                items = new Dictionary<string, int> { { "IT_EXP_BOOK_L", 2 }, { "IT_TICKET_RECRUIT", 3 } }
            };

            int eqCount = UnityEngine.Random.Range(1, 3);
            for (int i = 0; i < eqCount; i++)
                loot.equipments.Add(EquipmentSystem.GenerateRandomEquipment(30));

            return loot;
        }

        private LootData CalculateTowerLoot(int maxFloorCleared)
        {
            var loot = new LootData();
            loot.gold = 1000 + (maxFloorCleared * 500);
            loot.items = new Dictionary<string, int> { { "IT_EXP_BOOK_S", 5 + maxFloorCleared }, { "IT_WISH_CHARM", 1 } };

            EquipmentData eq = EquipmentSystem.GenerateRandomEquipment(maxFloorCleared);
            loot.equipments.Add(eq);

            if (maxFloorCleared >= 10 && UnityEngine.Random.value > 0.5f)
                loot.equipments.Add(EquipmentSystem.GenerateRandomEquipment(maxFloorCleared));

            return loot;
        }

        private int CalculateTowerExperience(int floorsCleared)
        {
            return floorsCleared * 100;
        }

        // =====================================================================
        // HELPER: MONSTER GENERATION
        // =====================================================================

        #region Placeholder Logic

        private List<string> GetMonstersForTowerFloor(int floor)
        {
            var towerConfigs = DataManager.Instance?.GameConfig?.TowerConfigs;
            if (towerConfigs != null && towerConfigs.Count > 0)
            {
                var floorConfig = towerConfigs.FirstOrDefault(t => t.floorIndex == floor);
                if (floorConfig != null && floorConfig.monsterPool?.Count > 0)
                {
                    var resultMonsters = new List<string>();
                    int count = floorConfig.customMonsterCount > 0 ? floorConfig.customMonsterCount : (1 + (floor / 5));
                    for (int i = 0; i < count; i++)
                        resultMonsters.Add(floorConfig.monsterPool[UnityEngine.Random.Range(0, floorConfig.monsterPool.Count)]);
                    return resultMonsters;
                }
            }

            var fallbackPool = new List<string> { "Goblin", "Orc", "Slime" };
            var monsters = new List<string>();
            int monsterCount = 1 + (floor / 5);
            for (int i = 0; i < monsterCount; i++)
                monsters.Add(fallbackPool[UnityEngine.Random.Range(0, fallbackPool.Count)]);
            return monsters;
        }

        private List<string> GenerateInjuredSoldiers(int floor)
        {
            var monsters = new List<string>();
            int soldierCount = 1 + (floor / 4);
            for (int i = 0; i < soldierCount; i++)
                monsters.Add("INJURED_SOLDIER");
            return monsters;
        }

        private long CalculateTravelTime(Vector2 destination)
        {
            return (long)(Vector2.Distance(Vector2.zero, destination) * 100);
        }

        #endregion

        public void StartNodeExpedition(List<string> squadHeroIDs, ExpeditionReport report)
        {
            long travelTime = 0; // Nodes are immediate travel
            long combatTimeMs = EXPLORATION_TIME_MS + (report.combatResult.TotalTurns * COMBAT_TURN_DURATION_MS);
            
            var destination = DataManager.Instance.GetPOIByID(report.poiId);
            if (destination == null) destination = new POIData { poiId = report.poiId, poiName = report.poiName };
            
            CreateAndDispatchActiveExpedition(squadHeroIDs, destination, travelTime, combatTimeMs, report, report.nodeId);
        }

        public void ApplyPendingNodeBattleResult(ActiveExpedition pendingBattle)
        {
            if (pendingBattle == null) return;
            
            DataManager.Instance.Player.UnclaimedReports.Add(pendingBattle.preCalculatedReport);
            _activeExpeditions.Remove(pendingBattle);

            OnExpeditionFinished?.Invoke(pendingBattle.expeditionId);
            OnNewReportReceived?.Invoke();
            
            if (!string.IsNullOrEmpty(pendingBattle.nodeId))
            {
                LegendOfBlood.POI_InfoPanel.ApplyPendingNodeBattleResult(pendingBattle.preCalculatedReport);
            }
            
            if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
            {
                string resStr = pendingBattle.preCalculatedReport.combatResult.DidPlayerWin ? "Chiáº¿n tháº¯ng" : "Tháº¥t báº¡i";
                GameManager.Instance.UINotificationManager.ShowNotification($"Káº¿t quáº£ Ä‘Ã¡nh Node: {resStr}. Vui lÃ²ng kiá»ƒm tra HÃ²m ThÆ°!");
            }
            
            Debug.Log($"Applying battle result by debug fast forward using same timer flow for expedition {pendingBattle.expeditionId}");
            Debug.Log($"report delivered to Mailbox");
            Debug.Log($"replay EventLog count: {(pendingBattle.preCalculatedReport.combatResult?.EventLog?.Count ?? 0)}");
        }

        public void CompleteAllPendingNodeBattlesDebug()
        {
            Debug.Log("Debug fast forward pending node battles");
            if (_activeExpeditions == null || _activeExpeditions.Count == 0)
            {
                Debug.Log("No pending node battle found");
                return;
            }

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            bool foundAny = false;

            foreach (var expedition in _activeExpeditions.ToList())
            {
                if (!string.IsNullOrEmpty(expedition.nodeId))
                {
                    foundAny = true;
                    Debug.Log($"battle id: {expedition.expeditionId}");
                    Debug.Log($"node id: {expedition.nodeId}");
                    Debug.Log($"old endsAt: {expedition.stateEndTimestamp}");
                    
                    expedition.stateEndTimestamp = currentTime;
                    Debug.Log($"new endsAt: {expedition.stateEndTimestamp}");
                    Debug.Log($"result victory: {expedition.preCalculatedReport.combatResult.DidPlayerWin}");
                    Debug.Log($"EventLog count: {expedition.preCalculatedReport.combatResult?.EventLog?.Count}");
                    
                    ApplyPendingNodeBattleResult(expedition);
                }
            }

            if (!foundAny)
            {
                Debug.Log("No pending node battle found");
            }
            else
            {
                DataManager.Instance.SavePlayerData();
            }
        }

    }
}

