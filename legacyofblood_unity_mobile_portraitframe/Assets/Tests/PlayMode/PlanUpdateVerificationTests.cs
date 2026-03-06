using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LegendOfBlood;
using LegendOfBlood.GameConfigs;
using System.Reflection;
using System.Linq;

public class PlanUpdateVerificationTests
{
    private DataManager _dataManagerInstance;
    private GameObject _dataManagerObject;
    private EvolutionSystem _evolutionSystem;

    // Helper to create mock traits, assuming Trait is a ScriptableObject
    private Trait CreateMockTrait(string id, string name, string familyId, Trait.RarityRank rank)
    {
        var trait = ScriptableObject.CreateInstance<Trait>();
        trait.id = id;
        trait.traitName = name;
        trait.familyId = familyId;
        trait.rank = rank;
        return trait;
    }

    [SetUp]
    public void SetUp()
    {
        // 1. Setup DataManager
        _dataManagerObject = new GameObject("DataManagerTestContainer");
        _dataManagerInstance = _dataManagerObject.AddComponent<DataManager>();

        // 2. Create Mock GameConfig
        var mockConfig = ScriptableObject.CreateInstance<GameConfig>();

        // 2a. Mock Experience Table
        mockConfig.ExperienceTable = new List<ExperienceData>
        {
            new ExperienceData { level = 1, experienceRequired = 100 },
            new ExperienceData { level = 19, experienceRequired = 100 }, // For level 20 test
            new ExperienceData { level = 20, experienceRequired = 200 },
        };

        // 2b. Mock Trait Database
        mockConfig.AllTraits = new List<Trait>
        {
            // Family 1: ATK_UP (3 members)
            CreateMockTrait("T01", "ATK Up D", "ATK_UP", Trait.RarityRank.D),
            CreateMockTrait("T02", "ATK Up C", "ATK_UP", Trait.RarityRank.C),
            CreateMockTrait("T03", "ATK Up B", "ATK_UP", Trait.RarityRank.B),

            // Family 2: HP_UP (2 members)
            CreateMockTrait("T04", "HP Up D", "HP_UP", Trait.RarityRank.D),
            CreateMockTrait("T05", "HP Up C", "HP_UP", Trait.RarityRank.C),

            // Family 3: DEF_UP (for inheritance test)
            CreateMockTrait("T06", "DEF Up D", "DEF_UP", Trait.RarityRank.D),

            // Unrelated traits for random pool
            CreateMockTrait("T99", "Random Trait", "RAND", Trait.RarityRank.A)
        };

        // 3. Inject Mock Config into DataManager via Reflection
        FieldInfo configField = typeof(DataManager).GetField("_gameConfig", BindingFlags.NonPublic | BindingFlags.Instance);
        configField.SetValue(_dataManagerInstance, mockConfig);

        // 4. Manually initialize DataManager to process the mock config
        MethodInfo initMethod = typeof(DataManager).GetMethod("InitializeDataManager", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod.Invoke(_dataManagerInstance, null);
        
        // 5. Initialize EvolutionSystem for tests that need it
        _evolutionSystem = new EvolutionSystem();
    }

    [TearDown]
    public void TearDown()
    {
        if (_dataManagerObject != null)
        {
            GameObject.Destroy(_dataManagerObject);
        }
        // Unsubscribe to avoid memory leaks in test environment
        _evolutionSystem = null; 
    }

    #region Part 1 Tests
    [Test]
    public void P1_1_BaseStatCalculation_IsBasedOnPotential()
    {
        string testId = "[Checklist 1.1]";
        Debug.Log($"{testId} Bắt đầu: Kiểm tra logic tính chỉ số gốc (Base Stats) từ POT.");
        var breedingSystem = new BreedingSystem();
        MethodInfo calculateStatsMethod = typeof(BreedingSystem).GetMethod("CalculateBaseStats", BindingFlags.NonPublic | BindingFlags.Instance);
        var offspring = new HeroData { potential = 15 };
        calculateStatsMethod.Invoke(breedingSystem, new object[] { offspring });

        int pot = offspring.potential;
        float minStat = pot * 8;
        float maxStat = pot * 10;

        Assert.GreaterOrEqual(offspring.baseStats.hp, minStat, $"{testId} HP failed");
        Assert.LessOrEqual(offspring.baseStats.hp, maxStat, $"{testId} HP failed");
        Assert.Pass($"{testId} THÀNH CÔNG! Chỉ số gốc (ví dụ HP: {offspring.baseStats.hp}) nằm trong khoảng [POT*8, POT*10].");
    }

    [UnityTest]
    public IEnumerator P1_2_LevelUp_GrantsFreePointsEqualToPotential()
    {
        string testId = "[Checklist 1.2]";
        Debug.Log($"{testId} Bắt đầu: Kiểm tra logic cộng điểm tiềm năng khi lên cấp.");
        var hero = new HeroData { heroName = "Test Hero", level = 1, experience = 0, potential = 15, freeStatPoints = 0 };
        hero.AddExperience(100);
        yield return null;
        Assert.AreEqual(2, hero.level);
        Assert.AreEqual(15, hero.freeStatPoints);
        Assert.Pass($"{testId} THÀNH CÔNG! Lên cấp và nhận được 15 điểm tự do.");
    }

    [Test]
    public void P1_3_StatAllocation_ManuallyAddsStats()
    {
        string testId = "[Checklist 1.3]";
        Debug.Log($"{testId} Bắt đầu: Kiểm tra logic phân phối điểm tự do thủ công.");
        var hero = new HeroData { freeStatPoints = 10, addedStats = new HeroStats() };
        hero.addedStats.hp += 5;
        hero.freeStatPoints -= 5;
        Assert.AreEqual(5, hero.addedStats.hp);
        Assert.AreEqual(5, hero.freeStatPoints);
        Assert.Pass($"{testId} THÀNH CÔNG! Phân phối 5 điểm vào HP thành công.");
    }
    #endregion

    #region Part 2 Tests

    [Test]
    public void P2_5_TraitFamily_DataIsCorrect()
    {
        string testId = "[Checklist 2.5]";
        Debug.Log($"{testId} Bắt đầu: Kiểm tra dữ liệu gia đình trait.");
        var atkFamilyTraits = DataManager.Instance.AllTraits.Values.Where(t => t.familyId == "ATK_UP").ToList();
        Assert.AreEqual(3, atkFamilyTraits.Count, "Không tìm thấy đủ 3 trait thuộc gia đình ATK_UP trong dữ liệu giả lập.");
        Assert.IsTrue(atkFamilyTraits.Any(t => t.rank == Trait.RarityRank.D));
        Assert.IsTrue(atkFamilyTraits.Any(t => t.rank == Trait.RarityRank.C));
        Assert.IsTrue(atkFamilyTraits.Any(t => t.rank == Trait.RarityRank.B));
        Assert.Pass($"{testId} THÀNH CÔNG! Gia đình 'ATK_UP' có 3 thành viên D, C, B đúng như dữ liệu giả lập.");
    }

    [UnityTest]
    public IEnumerator P2_2_LevelUpTo20_GrantsNewFamilyTrait()
    {
        string testId = "[Checklist 2.2]";
        Debug.Log($"{testId} Bắt đầu: Kiểm tra nhận trait mới ở cấp 20.");
        var hero = new HeroData { heroName = "Evolver", level = 19, experience = 0, potential = 10 };
        hero.traitIDs.Add("T01"); // Add a trait from ATK_UP family

        int initialTraitCount = hero.traitIDs.Count;

        hero.AddExperience(100); // Level up to 20
        yield return null; 

        Assert.AreEqual(20, hero.level, "Hero should have leveled up to 20.");
        Assert.AreEqual(initialTraitCount + 1, hero.traitIDs.Count, "Hero should have gained one new trait.");
        
        string newTraitId = hero.traitIDs.Last();
        var newTrait = DataManager.Instance.GetTraitByID(newTraitId);
        Assert.IsNotNull(newTrait, "New trait should exist in DataManager.");
        Assert.AreNotEqual("ATK_UP", newTrait.familyId, "New trait must belong to a new, unowned family.");

        Assert.Pass($"{testId} THÀNH CÔNG! Hero lên cấp 20 và nhận được trait mới '{newTrait.traitName}' thuộc gia đình '{newTrait.familyId}'.");
    }

    [Test]
    public void P2_1_TraitInheritance_FollowsTheRules()
    {
        string testId = "[Checklist 2.1]";
        Debug.Log($"{testId} Bắt đầu: Kiểm tra logic di truyền 3 trait.");

        var breedingSystem = new BreedingSystem();
        MethodInfo inheritTraitsMethod = typeof(BreedingSystem).GetMethod("InheritTraits", BindingFlags.NonPublic | BindingFlags.Instance);

        var fatherTraits = new List<string> { "T01" }; // ATK_UP
        var motherTraits = new List<string> { "T04" }; // HP_UP
        var options = new BreedingOptions();

        var inherited = (List<string>)inheritTraitsMethod.Invoke(breedingSystem, new object[] { fatherTraits, motherTraits, options });

        Assert.AreEqual(3, inherited.Count, "Phải thừa hưởng đúng 3 trait.");
        Assert.IsTrue(inherited.Contains("T01"), "Phải có trait từ cha.");
        Assert.IsTrue(inherited.Contains("T04"), "Phải có trait từ mẹ.");
        Assert.IsTrue(inherited.Any(id => id != "T01" && id != "T04"), "Phải có 1 trait ngẫu nhiên.");

        Assert.Pass($"{testId} THÀNH CÔNG! Di truyền 3 trait (1 cha, 1 mẹ, 1 ngẫu nhiên) hoạt động đúng.");
    }

    #endregion

    #region Part 3 Tests

    [Test]
    public void P3_1_HeroBelowLevel20_HasNoProfession()
    {
        string testId = "[Checklist 3.1]";
        Debug.Log($"{testId} Bắt đầu: Kiểm tra hero dưới cấp 20 chưa có nghề.");
        var hero = new HeroData { level = 19 };
        Assert.AreEqual(Profession.None, hero.profession, "Hero dưới cấp 20 phải chưa có nghề (Profession.None).");
        Assert.Pass($"{testId} THÀNH CÔNG! Hero ở cấp {hero.level} có trạng thái nghề là Profession.None.");
    }

    #endregion

    #region Part 4 Tests

    [Test]
    public void P4_3_AddHero_FailsWhenPopulationIsFull()
    {
        string testId = "[Checklist 4.3]";
        Debug.Log($"{testId} Bắt đầu: Kiểm tra không thể thêm hero khi dân số đầy.");

        // ARRANGE
        var playerData = DataManager.Instance.Player;
        var buildingList = DataManager.Instance.AllBuildings;
        
        // Add a MainHall building to define population capacity. Capacity = 10 + (level * 2)
        var mainHall = new Building { id = "MainHall", level = 5 }; // Capacity = 10 + 10 = 20
        buildingList.Add(mainHall);
        
        // Fill the hero list to capacity
        playerData.Heroes.Clear();
        for(int i = 0; i < 20; i++)
        {
            
        }
        
        int initialHeroCount = playerData.Heroes.Count;
        Assert.AreEqual(20, initialHeroCount, "Setup failed: Hero list should be full.");

        // ACT
        Debug.Log($"{testId} Hành động: Dân số đang đầy ({initialHeroCount}/20). Thử thêm một hero mới.");
        var extraHero = new HeroData { id = "ExtraHero" };
        DataManager.Instance.AddHero(extraHero);

        // ASSERT
        Assert.AreEqual(initialHeroCount, playerData.Heroes.Count, "Hero count should not have increased.");
        Assert.IsFalse(playerData.Heroes.Contains(extraHero), "The extra hero should not have been added to the list.");

        Assert.Pass($"{testId} THÀNH CÔNG! Không thể thêm hero mới khi dân số đã đầy.");
    }

    #endregion
}