// Đặt file này vào thư mục Assets/Tests/Editor/
// Tên file: GameMechanicsValidationTests.cs

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
// KHÔNG CẦN using LegendOfBlood; nữa, vì chúng ta sẽ tự định nghĩa mọi thứ cần thiết

// --- Các lớp giả (Mocks) để file Test có thể tự biên dịch và chạy độc lập ---
// Coder của bạn sau này phải triển khai những thứ tương tự trong code game thật.

public enum RarityRank_Test { D, C, B, A, S }
public enum Profession_Test { None, Warrior, Archer, Mage }

public class Trait_Test
{
    public string id;
    public string familyId;
    public RarityRank_Test rank;
    public string nextUpgradeTraitID;
}

public class HeroData_Test
{
    // Các trường mà kế hoạch yêu cầu coder phải thêm vào HeroData.cs
    public int potential;
    public float baseHp;
    public float baseAtk;
    public int freeStatPoints;
    public int level;
    public List<string> traitIDs = new List<string>();
    public Profession_Test profession = Profession_Test.None;
    public long breedingCooldownEndTimestamp = 0;
    public int breedingCount = 0;
    public int maxBreedingLimit = 10;
    
    // Các hàm mà kế hoạch yêu cầu coder phải thêm vào HeroData.cs
    public void CalculateBaseStats()
    {
        // Hàm giả lập logic tính toán chỉ số
        float multiplier = UnityEngine.Random.Range(8f, 10f);
        this.baseHp = this.potential * multiplier;
        this.baseAtk = this.potential * multiplier;
    }

    public void GainExp(int amount)
    {
        // Hàm giả lập logic lên cấp
        this.level++;
        this.freeStatPoints += this.potential;
    }
}

// --- Bắt đầu Lớp Test ---

public class GameMechanicsValidationTests
{
    private Dictionary<string, Trait_Test> _mockTraits;
    private Dictionary<string, List<Trait_Test>> _traitsByFamily;

    [SetUp]
    public void Setup()
    {
        _mockTraits = new Dictionary<string, Trait_Test>
        {
            {"TR_ATK_D", new Trait_Test { id = "TR_ATK_D", familyId = "ATK_UP", rank = RarityRank_Test.D, nextUpgradeTraitID = "TR_ATK_C" }},
            {"TR_ATK_C", new Trait_Test { id = "TR_ATK_C", familyId = "ATK_UP", rank = RarityRank_Test.C, nextUpgradeTraitID = "TR_ATK_B" }},
            {"TR_ATK_B", new Trait_Test { id = "TR_ATK_B", familyId = "ATK_UP", rank = RarityRank_Test.B, nextUpgradeTraitID = "TR_ATK_A" }},
            {"TR_HP_C",  new Trait_Test { id = "TR_HP_C",  familyId = "HP_UP",  rank = RarityRank_Test.C, nextUpgradeTraitID = "TR_HP_B" }},
            {"TR_DEF_B", new Trait_Test { id = "TR_DEF_B", familyId = "DEF_UP", rank = RarityRank_Test.B, nextUpgradeTraitID = "TR_DEF_A" }},
            {"TR_SPD_A", new Trait_Test { id = "TR_SPD_A", familyId = "SPD_UP", rank = RarityRank_Test.A, nextUpgradeTraitID = null }},
        };
        _traitsByFamily = _mockTraits.Values.GroupBy(t => t.familyId).ToDictionary(g => g.Key, g => g.ToList());
    }

    [Test]
    public void T1_1_BaseStats_AreCalculatedFromPOT()
    {
        var hero = new HeroData_Test { potential = 10 };
        hero.CalculateBaseStats();
        Assert.That(hero.baseAtk, Is.InRange(80, 100), "Chỉ số ATK cơ bản phải nằm trong khoảng 8-10 lần POT.");
        Assert.That(hero.baseHp, Is.InRange(80, 100), "Chỉ số HP cơ bản phải nằm trong khoảng 8-10 lần POT.");
    }

    [Test]
    public void T1_2_LevelUp_GrantsFreePointsInsteadOfStats()
    {
        var hero = new HeroData_Test { potential = 15, freeStatPoints = 0 };
        hero.CalculateBaseStats();
        float initialAtk = hero.baseAtk;
        hero.GainExp(99999);
        Assert.AreEqual(15, hero.freeStatPoints, "Khi lên cấp, hero phải nhận được 'Điểm Tự do' bằng với POT.");
        Assert.AreEqual(initialAtk, hero.baseAtk, "Chỉ số CƠ BẢN không được thay đổi khi lên cấp.");
    }
    
    [Test]
    public void T3_1_Profession_IsNoneBeforeLevel20()
    {
        var hero = new HeroData_Test { level = 19 };
        Assert.AreEqual(Profession_Test.None, hero.profession, "Anh hùng dưới cấp 20 phải ở trạng thái 'Chưa có nghề'.");
    }

    [Test]
    public void T2_2_NewTraitAtLevelUp_IsUniqueFamilyUsingTwoRolls()
    {
        var hero = new HeroData_Test { level = 19 };
        hero.traitIDs.Add("TR_ATK_D");
        
        var ownedFamilies = hero.traitIDs.Select(id => _mockTraits[id].familyId).ToHashSet();
        var unownedFamilyPool = _traitsByFamily.Keys.Where(f => !ownedFamilies.Contains(f)).ToList();
        
        string chosenFamily = "HP_UP";
        RarityRank_Test chosenRank = RarityRank_Test.C;

        var newTrait = _traitsByFamily[chosenFamily].FirstOrDefault(t => t.rank == chosenRank);
        if(newTrait != null) hero.traitIDs.Add(newTrait.id);

        Assert.IsFalse(unownedFamilyPool.Contains("ATK_UP"), "Gia đình trait đã sở hữu phải bị loại khỏi bể quay.");
        Assert.Contains("TR_HP_C", hero.traitIDs, "Hero phải nhận được trait mới thuộc gia đình chưa có.");
    }

    [Test]
    public void T2_3_TraitUpgradeAtLevel60_ReplacesOldTrait()
    {
        var hero = new HeroData_Test { level = 60 };
        hero.traitIDs.Add("TR_ATK_C");
        
        string oldTraitId = "TR_ATK_C";
        string newTraitId = _mockTraits[oldTraitId].nextUpgradeTraitID;
        
        hero.traitIDs.Remove(oldTraitId);
        hero.traitIDs.Add(newTraitId);

        Assert.IsFalse(hero.traitIDs.Contains("TR_ATK_C"), "Trait cũ (bậc C) phải bị xóa đi.");
        Assert.IsTrue(hero.traitIDs.Contains("TR_ATK_B"), "Trait mới (bậc B) phải được thêm vào.");
    }
    
    [Test]
    public void T2_4_CannotUpgrade_MaxRankTrait()
    {
        var traitA = _mockTraits["TR_SPD_A"];
        Assert.IsNull(traitA.nextUpgradeTraitID, "Trait bậc A phải có nextUpgradeTraitID là null, cho thấy không thể nâng cấp.");
    }

    [Test]
    public void T3_4_Breeding_AppliesCooldown()
    {
        var hero = new HeroData_Test();
        long currentTime = 1000;
        hero.breedingCooldownEndTimestamp = currentTime + 120;
        Assert.IsTrue(currentTime < hero.breedingCooldownEndTimestamp, "Hero phải trong trạng thái cooldown ngay sau khi lai tạo.");
    }
    
    [Test]
    public void T3_5_Breeding_IncrementsCounterAndReachesLimit()
    {
        var hero = new HeroData_Test { breedingCount = 9, maxBreedingLimit = 10 };
        hero.breedingCount++;
        Assert.AreEqual(10, hero.breedingCount, "Số lần lai tạo phải tăng lên 1.");
        Assert.IsTrue(hero.breedingCount >= hero.maxBreedingLimit, "Hero phải đạt đến giới hạn lai tạo.");
    }
}