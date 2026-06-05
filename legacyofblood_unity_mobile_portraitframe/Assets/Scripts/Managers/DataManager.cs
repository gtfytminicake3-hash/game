// --- START OF FILE DataManager.cs (FIXED AGAIN) ---

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LegendOfBlood.GameConfigs; // Dùng GameConfig từ namespace này
using LegendOfBlood.Managers;   // Dùng PlayerQuestStatus từ namespace này
// using LegendOfBlood.DataModels; // SỬA LỖI: XÓA DÒNG NÀY
using UnityEngine;

namespace LegendOfBlood
{
    /// <summary>
    /// DataManager là một Singleton chịu trách nhiệm:
    /// 1. Tải và cung cấp dữ liệu cấu hình game (Traits, Skills, etc.) từ ScriptableObjects.
    /// 2. Quản lý dữ liệu trạng thái của người chơi (heroes, inventory, buildings).
    /// 3. Xử lý việc lưu và tải dữ liệu của người chơi ra file.
    /// </summary>
    [DisallowMultipleComponent]
    public class DataManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static DataManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) { DestroyImmediate(this.gameObject); return; }
#endif
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(this.gameObject); // Dòng này sẽ được quản lý bởi [CORE_SYSTEMS]

            // Khởi tạo toàn bộ dữ liệu khi game bắt đầu
            InitializeDataManager();
        }
        private bool _isInitialized = false;
        private float _passiveExpTimer = 0f;
        private const float PASSIVE_EXP_INTERVAL = 60f; // 60 seconds

        #endregion

        #region Fields and Properties

        [Header("Game Configuration")]
        [SerializeField]
        [Tooltip("Kéo ScriptableObject chứa toàn bộ data config của game vào đây.")]
        private GameConfigs.GameConfig _gameConfig;
        
        // Expose public property cho các System khác truy cập
        public GameConfigs.GameConfig GameConfig => _gameConfig;

        // Dữ liệu cấu hình game (được tối ưu hóa để truy cập nhanh bằng Dictionary)
        public Dictionary<string, Trait> AllTraits { get; private set; }
        public Dictionary<string, Skill> AllSkills { get; private set; }
        public Dictionary<string, ItemData> AllItems { get; private set; }
        public Dictionary<string, MonsterData> AllMonsters { get; private set; } // NEW
        public Dictionary<string, BossData> AllBosses { get; private set; }
        public Dictionary<int, int> ExpTable { get; private set; }
        // SỬA LỖI: Chỉ định rõ namespace cho EvolutionRewardData để giải quyết lỗi CS0029
        public List<GameConfigs.EvolutionRewardData> EvolutionRewards { get; private set; }
        public Dictionary<Profession, List<string>> StartingSkillsByProfession { get; private set; }
        public Dictionary<string, BuildingUpgradeData> BuildingUpgradeConfigs { get; private set; }
        public POIMonsterConfig POIMonsterConfig { get; private set; }
        public Dictionary<string, QuestData> AllQuests { get; private set; }

        // Dữ liệu trạng thái của người chơi (runtime data)
        public PlayerData Player { get; private set; }
        public List<HeroData> AllHeroes => Player.Heroes;
        public List<Building> AllBuildings { get; private set; }

        // Tên file lưu
        private const string SAVE_FILE_NAME = "legendofblood_save.json";
        private string _saveFilePath;

        // Sự kiện để thông báo cho các hệ thống khác (ví dụ: UI) khi dữ liệu thay đổi
        public static event Action OnPlayerDataLoaded;
        public static event Action OnHeroListChanged;
        public static event Action<HeroData> OnHeroStatsChanged;
        public static event Action<HeroData> OnHeroAvailabilityChanged;
        public static event Action OnReportClaimed;

        #endregion

        #region Initialization

        /// <summary>
        /// Khởi tạo DataManager, xử lý config và tải dữ liệu người chơi.
        /// </summary>
        public void InitializeDataManager()
        {
            if (Instance == null) Instance = this; // Đảm bảo Instance luôn được trỏ về mình nếu bị Unity load lộn xộn

            if (_isInitialized) return;

            _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            ProcessGameConfiguration();
            LoadPlayerData();
            
            _isInitialized = true;
        }

        /// <summary>
        /// Đọc dữ liệu từ ScriptableObject và chuyển thành Dictionary để truy cập hiệu quả.
        /// </summary>
        private void ProcessGameConfiguration()
        {
            if (_gameConfig == null)
            {
                Debug.LogError(global::LocalizationSystem.GetText("datamanager_error_no_gameconfig"));
                return;
            }

            var traitsList = _gameConfig.AllTraits ?? new List<Trait>();
            AllTraits = traitsList.ToDictionary(t => t.id, t => t);

            var skillsList = _gameConfig.AllSkills ?? new List<Skill>();
            AllSkills = skillsList.ToDictionary(s => s.id, s => s);
            
            var itemsList = _gameConfig.AllItems ?? new List<ItemData>();
            AllItems = itemsList.ToDictionary(i => i.id, i => i);

            // --- BƠM DATA ẢO TRỰC TIẾP VÀO RAM NẾU CHƯA CÓ TRONG CONFIG ---
            string[] ids = { "item_mutation_potion", "item_wish_charm", "item_speed_hourglass" };
            string[] names = { "Thuốc Biến Dị", "Bùa Ước Nguyện", "Đồng Hồ Cát" };
            string[] descs = {
                "Một loại huyết thanh kì bí sủi bọt xanh. Cung cấp exp Khổng khồ cho các Hero.",
                "Tấm bùa rách nát cổ xưa, phát ra một thứ ánh sáng ma mị. Dùng để mở khóa Breeding.",
                "Hạt cát bên trong chảy lướt qua thời không. Dùng để rút ngắn thời gian thám hiểm."
            };
            ItemType[] types = { ItemType.Consumable, ItemType.BreedingMaterial, ItemType.SpeedUp };

            for (int i = 0; i < ids.Length; i++)
            {
                if (!AllItems.ContainsKey(ids[i]))
                {
                    ItemData generatedData = ScriptableObject.CreateInstance<ItemData>();
                    generatedData.id = ids[i];
                    generatedData.itemName = names[i];
                    generatedData.description = descs[i];
                    generatedData.type = types[i];
                    generatedData.icon = UnityEngine.Resources.Load<Sprite>($"Items/{ids[i]}");
                    AllItems.Add(ids[i], generatedData);
                }
            }
            // -------------------------------------------------------------
            
            var expList = _gameConfig.ExperienceTable ?? new List<GameConfigs.ExperienceData>();
            ExpTable = expList.ToDictionary(e => e.level, e => e.experienceRequired);

            if (_gameConfig.EvolutionTable != null)
            {
                EvolutionRewards = _gameConfig.EvolutionTable.rewards;
            }
            else
            {
                EvolutionRewards = new List<GameConfigs.EvolutionRewardData>();
                Debug.LogWarning("EvolutionTable is not set in GameConfig.");
            }

            StartingSkillsByProfession = new Dictionary<Profession, List<string>>();
            var startingSkillsList = _gameConfig.StartingSkills ?? new List<GameConfigs.ProfessionStartingSkills>();
            foreach (var profSkills in startingSkillsList)
            {
                if (!StartingSkillsByProfession.ContainsKey(profSkills.profession))
                {
                    StartingSkillsByProfession.Add(profSkills.profession, profSkills.startingSkillIDs);
                }
            }

            BuildingUpgradeConfigs = new Dictionary<string, BuildingUpgradeData>();
            if (_gameConfig.BuildingUpgradeDataList != null)
            {
                foreach (var config in _gameConfig.BuildingUpgradeDataList)
                {
                    if (config != null && !BuildingUpgradeConfigs.ContainsKey(config.buildingId))
                    {
                        BuildingUpgradeConfigs.Add(config.buildingId, config);
                    }
                }
            }
            // Fallback load all building configs from Resources/GameData
            var allBuildingConfigs = Resources.LoadAll<BuildingUpgradeData>("GameData");
            foreach (var config in allBuildingConfigs)
            {
                if (config != null && !BuildingUpgradeConfigs.ContainsKey(config.buildingId))
                {
                    BuildingUpgradeConfigs.Add(config.buildingId, config);
                }
            }

            POIMonsterConfig = _gameConfig.POIMonsterConfig;
            
            AllQuests = _gameConfig.AllQuestData?.ToDictionary(q => q.questId, q => q) ?? new Dictionary<string, QuestData>();
            AllBosses = _gameConfig.AllBosses?.ToDictionary(b => b.id, b => b) ?? new Dictionary<string, BossData>();
            AllMonsters = _gameConfig.AllMonsters?.ToDictionary(m => m.id, m => m) ?? new Dictionary<string, MonsterData>();

            Debug.Log($"Đã xử lý xong Game Config: {AllTraits.Count} Traits, {AllSkills.Count} Skills, {BuildingUpgradeConfigs.Count} BuildingConfigs. {AllQuests.Count} Quests.");
        }

        #endregion

        #region Save/Load Logic

        public void LoadPlayerData()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
                    Player = loadedData.Player ?? new PlayerData();
                    AllBuildings = loadedData.AllBuildings;
                    Debug.Log($"Tải game thành công từ: {_saveFilePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Lỗi khi đọc file save! Tạo game mới. Lỗi: {e.Message}");
                    CreateNewPlayerData();
                }
            }
            else
            {
                Debug.Log("Không tìm thấy file save. Tạo game mới.");
                CreateNewPlayerData();
            }
            
            if (Player.Heroes == null) Player.Heroes = new List<HeroData>();
            if (Player.QuestStatuses == null) Player.QuestStatuses = new List<LegendOfBlood.Managers.PlayerQuestStatus>();
            if (Player.WorldPois == null) Player.WorldPois = new List<POIData>();
            if (AllBuildings == null) AllBuildings = new List<Building>();

            // Ensure default buildings exist for old saves
            if (!AllBuildings.Any(b => b.id == "Barracks")) AllBuildings.Add(new Building(BuildingType.Barracks, 1) { id = "Barracks" });
            if (!AllBuildings.Any(b => b.id == "Hospital")) AllBuildings.Add(new Building(BuildingType.Hospital, 1) { id = "Hospital" });
            if (!AllBuildings.Any(b => b.id == "BreedingPen")) AllBuildings.Add(new Building(BuildingType.BreedingPen, 1) { id = "BreedingPen" });
            if (!AllBuildings.Any(b => b.id == "TownHall")) AllBuildings.Add(new Building(BuildingType.TownHall, 1) { id = "TownHall" });

            // --- TEST INJECTION HÀNG XỊN (THẬT 100%) ---
            if (!Player.items.ContainsKey("item_mutation_potion")) Player.items["item_mutation_potion"] = 5;
            if (!Player.items.ContainsKey("item_wish_charm")) Player.items["item_wish_charm"] = 5;
            if (!Player.items.ContainsKey("item_speed_hourglass")) Player.items["item_speed_hourglass"] = 5;

            // Xóa rác "Legendary Sword" cũ do chạy sinh tự động từ trước
            Player.equipments.RemoveAll(e => e.equipmentName == "Legendary Sword" || e.id.StartsWith("wpn_"));

            // Bơm 2 món thiết bị xịn vào
            if (!Player.equipments.Any(e => e.id == "equip_ancient_weapon"))
            {
                var wpn = new EquipmentData("equip_ancient_weapon", "Kiếm Cổ Thần", EquipmentSlot.Weapon, 1, EquipmentTier.SSS, Profession.Warrior);
                wpn.atkBonus = 999;
                wpn.critChanceBonus = 0.5f; // 50% crit
                wpn.spdBonus = 20;
                Player.equipments.Add(wpn);
            }
            if (!Player.equipments.Any(e => e.id == "equip_ancient_armor"))
            {
                var arm = new EquipmentData("equip_ancient_armor", "Giáp Rồng Xương", EquipmentSlot.Armor, 1, EquipmentTier.SSS, Profession.Warrior);
                arm.hpBonus = 5000;
                arm.defBonus = 800;
                Player.equipments.Add(arm);
            }
            // -------------------------------------------------------------
            // --- BƠM TIỀN ARENA CHO TESTER ---
            if (Player.arenaCoins < 50000)
            {
                Player.arenaCoins = 100000; 
                Debug.Log("[TESTING] Đã tự động bơm 100,000 Xu Khuyển (Arena Coins) để bạn mua đồ Shop thoải mái!");
            }

            CalculateOfflineProgress();

            OnPlayerDataLoaded?.Invoke();
            OnHeroListChanged?.Invoke();
        }

        private void CreateNewPlayerData()
        {
            Player = new PlayerData();
            Player.Heroes = new List<HeroData>();
            Player.QuestStatuses = new List<LegendOfBlood.Managers.PlayerQuestStatus>();
            Player.WorldPois = new List<POIData>();
            AllBuildings = new List<Building>();
            
            // Add default buildings
            AllBuildings.Add(new Building(BuildingType.Barracks, 1) { id = "Barracks" });
            AllBuildings.Add(new Building(BuildingType.Hospital, 1) { id = "Hospital" });
            AllBuildings.Add(new Building(BuildingType.BreedingPen, 1) { id = "BreedingPen" });
            AllBuildings.Add(new Building(BuildingType.TownHall, 1) { id = "TownHall" });
            
            HeroData startingMale = CreateStartingHero(Gender.Male, "Adam");
            HeroData startingFemale = CreateStartingHero(Gender.Female, "Eva");
            Player.Heroes.Add(startingMale);
            Player.Heroes.Add(startingFemale);
        }

        public void SavePlayerData()
        {
            SaveData saveData = new SaveData
            {
                Player = this.Player,
                AllBuildings = this.AllBuildings
            };

            try
            {
                Player.lastOfflineTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(_saveFilePath, json);
                Debug.Log($"Lưu game thành công tại: {_saveFilePath}");
            }
            catch(Exception e)
            {
                Debug.LogError($"Lỗi khi lưu game! Lỗi: {e.Message}");
            }
        }
        
        private void CalculateOfflineProgress()
        {
            if (Player.lastOfflineTimestamp == 0) return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long timePassedMs = currentTime - Player.lastOfflineTimestamp;
            int minutesPassed = (int)(timePassedMs / 60000);

            if (minutesPassed > 0)
            {
                int totalExpGranted = 0;
                foreach (var hero in Player.Heroes)
                {
                    if (!hero.IsBusy())
                    {
                        // 1% of current level requirement per minute
                        if (ExpTable != null && ExpTable.TryGetValue(hero.level, out int reqExp))
                        {
                            int expPerMinute = Mathf.Max(1, reqExp / 100);
                            int gainedExp = expPerMinute * minutesPassed;
                            hero.AddExperience(gainedExp);
                            totalExpGranted += gainedExp;
                        }
                    }
                }

                if (totalExpGranted > 0)
                {
                    Debug.Log($"<color=green>[Offline Progress] Awarded {totalExpGranted} total EXP to idle heroes across {minutesPassed} minutes offline.</color>");
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (!_isInitialized || Player?.Heroes == null) return;

            _passiveExpTimer += deltaTime;
            if (_passiveExpTimer >= PASSIVE_EXP_INTERVAL)
            {
                _passiveExpTimer = 0f;
                // Update offline timestamp every minute so we don't double dip if the game crashes
                Player.lastOfflineTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                int totalExpGranted = 0;
                foreach (var hero in Player.Heroes)
                {
                    if (!hero.IsBusy())
                    {
                        if (ExpTable != null && ExpTable.TryGetValue(hero.level, out int reqExp))
                        {
                            int expPerMinute = Mathf.Max(1, reqExp / 100);
                            hero.AddExperience(expPerMinute);
                            totalExpGranted += expPerMinute;
                        }
                    }
                }
                if (totalExpGranted > 0)
                {
                    Debug.Log($"<color=green>[Online Progress] Awarded {totalExpGranted} total EXP to idle heroes for the past minute.</color>");
                }
            }
        }
        #endregion

        #region Public API (Helpers & Modifiers)

        public static void TriggerHeroStatsChanged(HeroData hero) => OnHeroStatsChanged?.Invoke(hero);
        public static void TriggerHeroAvailabilityChanged(HeroData hero) => OnHeroAvailabilityChanged?.Invoke(hero);
        public static void TriggerReportClaimed() => OnReportClaimed?.Invoke();

        public int GetPopulationCapacity()
        {
            var mainHall = AllBuildings.FirstOrDefault(b => b.id == "TownHall");
            if (mainHall != null)
            {
                // Giả định TownHall bắt đầu ở cấp 1, sức chứa sẽ là 50
                // Có thể điều chỉnh công thức tăng tiến theo ý muốn, ví dụ: +5 mỗi cấp
                return 50 + ((mainHall.level > 0 ? mainHall.level - 1 : 0) * 5);
            }
            return 50;
        }

        public bool IsPopulationFull()
        {
            return Player.Heroes.Count >= GetPopulationCapacity();
        }

        public void AddHero(HeroData newHero)
        {
            if (newHero == null) return;

            if (IsPopulationFull())
            {
                Debug.LogWarning("Population is full! Cannot add new hero.");
                if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
                {
                    GameManager.Instance.UINotificationManager.ShowNotification(LocalizationSystem.GetText("notification_population_full"));
                }
                return;
            }

            Player.Heroes.Add(newHero);
            OnHeroListChanged?.Invoke();
            Debug.Log($"Added hero {newHero.heroName}. Total heroes: {Player.Heroes.Count}/{GetPopulationCapacity()}");
        }

        public void RemoveHero(string heroId)
        {
            RemoveHeroes(new[] { heroId });
        }

        public int RemoveHeroes(IEnumerable<string> heroIds)
        {
            if (Player?.Heroes == null || heroIds == null) return 0;

            var idsToRemove = new HashSet<string>(heroIds.Where(id => !string.IsNullOrEmpty(id)));
            if (idsToRemove.Count == 0) return 0;

            int removedCount = 0;
            int refundGold = 0;

            for (int i = Player.Heroes.Count - 1; i >= 0; i--)
            {
                HeroData heroToRemove = Player.Heroes[i];
                if (heroToRemove == null || !idsToRemove.Contains(heroToRemove.id)) continue;
                if (heroToRemove.IsBusy()) continue;

                int heroRefund = heroToRemove.level * 50;
                refundGold += heroRefund;
                Debug.Log($"Dismissed hero {heroToRemove.heroName}, refund {heroRefund} Gold.");
                Player.Heroes.RemoveAt(i);
                removedCount++;
            }

            if (removedCount > 0)
            {
                if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
                {
                    GameManager.Instance.InventoryManager.AddResource(ResourceType.Gold, refundGold);
                }
                else if (Player.resources != null)
                {
                    Player.resources.gold += refundGold;
                }

                OnHeroListChanged?.Invoke();
            }

            return removedCount;
        }

        public HeroData GetHeroByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return Player.Heroes.FirstOrDefault(h => h.id == id);
        }

        public POIData GetPOIByID(string poiId)
        {
            if (string.IsNullOrEmpty(poiId) || Player?.WorldPois == null) return null;
            return Player.WorldPois.FirstOrDefault(p => p.poiId == poiId);
        }

        public HeroData GetMonsterByID(string id, int difficultyLevel = 1)
        {
            if (string.IsNullOrEmpty(id)) return null;

            // 1. Phục vụ xuất hiện Boss
            BossData bossCfg = AllBosses.TryGetValue(id, out var boss) ? boss : null;
            if (bossCfg != null) 
            {
                 // Bosses may optionally scale, but for now we keep giving them their hardcoded base stats
                 return new HeroData
                {
                    id = id,
                    heroName = bossCfg.bossName,
                    level = Mathf.Max(bossCfg.level, difficultyLevel),
                    profession = Profession.Warrior,
                    baseStats = new HeroStats { hp = bossCfg.baseHp, atk = bossCfg.baseAtk, def = bossCfg.baseDef, spd = bossCfg.baseSpd },
                    currentHp = bossCfg.baseHp,
                    isMature = true 
                };
            }

            // 2. Add special case for Healer Tower "Injured Soldiers"
            if (id == "INJURED_SOLDIER")
            {
                // Generate a dummy soldier that acts as a target for healing.
                // We want high maxHp and low currentHp.
                float soldierHp = 500f * difficultyLevel; // e.g. 500 at floor 1, 10000 at floor 20
                return new HeroData
                {
                    id = Guid.NewGuid().ToString(), // unique ID so combat doesn't overlap them
                    heroName = string.Format(LocalizationSystem.GetText("healer_tower_soldier_name"), difficultyLevel),
                    level = difficultyLevel,
                    profession = Profession.Warrior,
                    baseStats = new HeroStats { hp = soldierHp, atk = 0, def = 10 * difficultyLevel, spd = 50 }, // Slow, no atk
                    currentHp = 1, // extremely low health
                    isMature = true
                };
            }

            // 3. Lấy dữ liệu Monster cơ sở và Scale theo Difficulty
            MonsterData monsterCfg = AllMonsters != null && AllMonsters.TryGetValue(id, out var m) ? m : null;
            if (monsterCfg != null)
            {
                // Công thức tính Scale: Tăng 40% mỗi cấp độ lấy từ difficultyLevel (tối thiểu là 1)
                float multiplier = Mathf.Pow(1.4f, Mathf.Max(1, difficultyLevel) - 1);
                
                float scaledHp = monsterCfg.baseHp * multiplier;
                float scaledAtk = monsterCfg.baseAtk * multiplier;
                float scaledDef = monsterCfg.baseDef * multiplier;
                float scaledSpd = monsterCfg.baseSpd * multiplier;

                return new HeroData
                {
                    id = id,
                    heroName = monsterCfg.monsterName,
                    level = difficultyLevel,
                    profession = monsterCfg.profession,
                    baseStats = new HeroStats { hp = scaledHp, atk = scaledAtk, def = scaledDef, spd = scaledSpd, critChance = monsterCfg.critChance, critDamage = monsterCfg.critDamage },
                    currentHp = scaledHp,
                    isMature = true
                };
            }

            // 3. Quái giả (Fallback cuối cùng)
            float fbMultiplier = Mathf.Pow(1.4f, Mathf.Max(1, difficultyLevel) - 1);
            return new HeroData
            {
                id = id,
                heroName = $"Quái Nhỏ {id}",
                level = difficultyLevel,
                profession = Profession.Warrior,
                baseStats = new HeroStats { hp = 200 * fbMultiplier, atk = 20 * fbMultiplier, def = 15 * fbMultiplier, spd = 10 * fbMultiplier },
                currentHp = 200 * fbMultiplier,
                isMature = true
            };
        }

        public Trait GetTraitByID(string id)
        {
            AllTraits.TryGetValue(id, out var trait);
            return trait;
        }

        public Skill GetSkillByID(string id)
        {
            AllSkills.TryGetValue(id, out var skill);
            return skill;
        }

        public List<string> GetStartingSkills(Profession profession)
        {
            if (StartingSkillsByProfession.TryGetValue(profession, out var skillList))
            {
                return skillList;
            }
            return new List<string>();
        }

        public BuildingUpgradeData GetBuildingUpgradeData(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId) || BuildingUpgradeConfigs == null)
            {
                return null;
            }
            BuildingUpgradeConfigs.TryGetValue(buildingId, out var data);
            return data;
        }

        public QuestData GetQuestData(string questId)
        {
            if (string.IsNullOrEmpty(questId) || AllQuests == null)
            {
                return null;
            }
            AllQuests.TryGetValue(questId, out var data);
            return data;
        }

        #endregion

        #region Utility Methods
        
        private HeroData CreateStartingHero(Gender gender, string name)
        {
            Profession prof = gender == Gender.Male ? Profession.Warrior : Profession.Healer;
            List<string> startingSkills = GetStartingSkills(prof);

            var hero = new HeroData(Guid.NewGuid().ToString(), name, gender)
            {
                level = 10,
                potential = 18,
                baseStats = new HeroStats { hp = 200, atk = 25, def = 15, spd = 20 },
                isMature = true,
                traitIDs = new List<string> { "TR_ATK_D", "TR_ALL_S" }, // Tặng Tân thủ 2 gen xịn từ GameConfig 
                skillIDs = new List<string>(startingSkills) 
            };
            hero.SetProfession(prof);
            hero.freeStatPoints = 18 * 9; // 9 levels of potential for reaching level 10
            hero.currentHp = hero.GetFinalStats().hp; 
            return hero;
        }

        #endregion
    }

    [Serializable]
    public class SaveData
    {
        public PlayerData Player;
        public List<Building> AllBuildings;
    }
}
// --- END OF FILE DataManager.cs (FIXED AGAIN) ---

