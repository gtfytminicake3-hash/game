namespace LegendOfBlood
{
    using System.Collections.Generic;

    /// <summary>
    /// Lớp đại diện cho một đối tượng tham gia chiến đấu.
    /// Nó gói gọn HeroData và chứa các trạng thái thay đổi trong trận đấu.
    /// </summary>
    public class Combatant
    {
        public HeroData HeroRef { get; } // Tham chiếu đến dữ liệu hero gốc

        // Chỉ số chiến đấu thực tế (đã tính final stats)
        public float Atk { get; set; }
        public float Def { get; set; }
        public float Spd { get; set; }

        public int MaxHp { get; private set; }
        public int CurrentHp { get; set; }
        public int CurrentShield { get; set; }

        public bool IsPlayerTeam { get; }
        public List<string> TraitIDs { get; }

        /// <summary>
        /// Constructor để tạo một Combatant từ một HeroData.
        /// </summary>
        public Combatant(HeroData hero, bool isPlayerTeam)
        {
            HeroRef = hero;
            IsPlayerTeam = isPlayerTeam;
            TraitIDs = new List<string>(hero.traitIDs);

            // Lấy chỉ số cuối cùng đã tính toán hiệu ứng Trait
            var finalStats = hero.GetFinalStats();
            Atk = finalStats.atk;
            Def = finalStats.def;
            Spd = finalStats.spd;
            MaxHp = finalStats.hp;

            // Bắt đầu trận đấu với lượng máu hiện tại của hero
            CurrentHp = hero.currentHp;
            CurrentShield = 0;
        }

        public bool IsAlive()
        {
            return CurrentHp > 0;
        }
    }

    /// <summary>
    /// Lớp chứa kết quả trả về của một trận đấu.
    /// </summary>
    public class CombatResult
    {
        public bool DidPlayerWin;
        public List<HeroData> PlayerSurvivors; // Các hero phe người chơi còn sống
        public List<HeroData> PlayerCasualties; // Các hero phe người chơi đã gục ngã
        // TODO: Thêm phần thưởng
        // public Rewards GainedRewards;
        public List<string> CombatLog; // Tường thuật chi tiết trận đấu
    }
}