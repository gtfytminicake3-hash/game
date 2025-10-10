// Cần using namespace nơi bạn định nghĩa Skill.cs và các enum
using LegendOfBlood;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegendOfBlood.Combat
{
    #region 0. LỚP DỮ LIỆU KẾT QUẢ TRẬN ĐẤU
    /// <summary>
    /// Chứa kết quả của một trận đấu mô phỏng.
    /// Đây là định nghĩa DUY NHẤT cho CombatResult.
    /// </summary>
    [System.Serializable]
    public class CombatResult
    {
        public bool DidPlayerWin { get; set; }
        public List<string> CombatLog { get; set; }
        public List<HeroData> PlayerSurvivors { get; set; }
        public List<HeroData> PlayerCasualties { get; set; }
        public List<HeroData> EnemySurvivors { get; set; }
        public List<HeroData> EnemyCasualties { get; set; }
    }
    #endregion

    #region 1. CÁC LỚP DỮ LIỆU PHỤ TRỢ

    public enum RowPosition { Front, Middle, Back }

    public class ActiveStatusEffect
    {
        public StatusEffectType Type { get; }
        public int Duration { get; set; }
        public float Value { get; }
        public Combatant Caster { get; }

        public ActiveStatusEffect(StatusEffectType type, int duration, float value, Combatant caster)
        {
            Type = type;
            Duration = duration;
            Value = value;
            Caster = caster;
        }
    }

    public class CombatAction
    {
        public Combatant Actor { get; set; }
        public Skill Skill { get; set; }
        public bool IsBasicAttack => Skill == null;
    }

    #endregion

    #region 2. LỚP COMBATANT - TRÁI TIM CỦA HỆ THỐNG

    public class Combatant
    {
        public HeroData HeroRef { get; }
        public bool IsPlayerTeam { get; }
        public string InstanceID { get; }
        public RowPosition Position { get; set; }
        public List<Skill> Skills { get; private set; }
        public Dictionary<string, int> SkillCooldowns { get; private set; }
        public float MaxHp { get; private set; }
        public float CurrentHp { get; set; }
        public List<ActiveStatusEffect> ActiveEffects { get; private set; }

        public Combatant(HeroData heroData, bool isPlayer, int instanceIndex)
        {
            HeroRef = heroData;
            IsPlayerTeam = isPlayer;
            InstanceID = $"{(isPlayer ? "P" : "E")}_{HeroRef.profession}_{instanceIndex}";
            
            var finalStats = HeroRef.GetFinalStats();
            MaxHp = finalStats.hp;
            CurrentHp = Mathf.Clamp(HeroRef.currentHp, 0, MaxHp);

            Skills = new List<Skill>();
            SkillCooldowns = new Dictionary<string, int>();
            ActiveEffects = new List<ActiveStatusEffect>();
        }

        public float GetCurrentAtk() => HeroRef.GetFinalStats().atk;
        public float GetCurrentDef() => HeroRef.GetFinalStats().def + ActiveEffects.Where(e => e.Type == StatusEffectType.DefDown).Sum(e => e.Value);
        public float GetCurrentSpd() => HeroRef.GetFinalStats().spd + ActiveEffects.Where(e => e.Type == StatusEffectType.Slow).Sum(e => e.Value);
        public float GetCurrentCritChance() => HeroRef.GetFinalStats().critChance + ActiveEffects.Where(e => e.Type == StatusEffectType.CritUp).Sum(e => e.Value);
        public float GetCurrentCritDamage() => HeroRef.GetFinalStats().critDamage;
        public bool IsAlive() => CurrentHp > 0;

        public void AssignSkills(List<Skill> availableSkills, System.Random rng)
        {
            var classSkills = availableSkills.Where(s => s.type == SkillType.Active && (HeroClass)s.requiredProfession == (HeroClass)HeroRef.profession).ToList();
            int skillCount = (HeroRef.level >= 40) ? 2 : 1;
            Skills = classSkills.OrderBy(s => rng.Next()).Take(skillCount).ToList();
            foreach (var skill in Skills)
            {
                SkillCooldowns[skill.id] = 0;
            }
        }
    }
    #endregion

    #region 3. HỆ THỐNG CHIẾN ĐẤU CHÍNH

    public class CombatSystem
    {
        private System.Random _rng;
        private List<Combatant> _playerTeam;
        private List<Combatant> _enemyTeam;
        private List<string> _combatLog;
        private readonly List<Skill> _allAvailableSkills;

        public CombatSystem(int seed, List<Skill> allSkills)
        {
            _rng = new System.Random(seed);
            _allAvailableSkills = allSkills ?? new List<Skill>();
        }

        public CombatResult Simulate(List<HeroData> playerHeroes, List<HeroData> enemyHeroes)
        {
            _playerTeam = playerHeroes.Select((h, i) => new Combatant(h, true, i)).ToList();
            _enemyTeam = enemyHeroes.Select((h, i) => new Combatant(h, false, i)).ToList();
            return RunSimulation();
        }

        public CombatResult Simulate(List<HeroData> playerHeroes, List<string> enemyMonsterIDs)
        {
            _playerTeam = playerHeroes.Select((h, i) => new Combatant(h, true, i)).ToList();
            var enemyHeroes = enemyMonsterIDs.Select(id => DataManager.Instance.GetMonsterByID(id))
                                             .Where(h => h != null)
                                             .ToList();
            _enemyTeam = enemyHeroes.Select((h, i) => new Combatant(h, false, i)).ToList();
            return RunSimulation();
        }

        private CombatResult RunSimulation()
        {
            _combatLog = new List<string> { "<b>Trận đấu bắt đầu!</b>" };

            _playerTeam.ForEach(c => c.AssignSkills(_allAvailableSkills, _rng));
            _enemyTeam.ForEach(c => c.AssignSkills(_allAvailableSkills, _rng));

            ArrangeFormation(_playerTeam);
            ArrangeFormation(_enemyTeam);

            LogFormation(_playerTeam, "Đội hình người chơi");
            LogFormation(_enemyTeam, "Đội hình địch");

            int turn = 1;
            while (IsTeamAlive(_playerTeam) && IsTeamAlive(_enemyTeam))
            {
                _combatLog.Add($"\n<color=yellow>--- Vòng {turn} ---</color>");
                var turnOrder = _playerTeam.Concat(_enemyTeam).Where(c => c.IsAlive()).OrderByDescending(c => c.GetCurrentSpd()).ToList();

                foreach (var combatant in turnOrder)
                {
                    if (!combatant.IsAlive()) continue;
                    ProcessStartOfTurnEffects(combatant);
                    if (!combatant.IsAlive()) continue;
                    TickCooldowns(combatant);
                    var action = DecideAction(combatant);
                    ExecuteAction(action);
                    if (!IsTeamAlive(_playerTeam) || !IsTeamAlive(_enemyTeam)) break;
                }

                turn++;
                if (turn > 50) { _combatLog.Add("Trận đấu quá dài, kết quả hòa!"); break; }
            }

            // Cập nhật lại currentHp của HeroData gốc trước khi trả về
            foreach(var combatant in _playerTeam.Concat(_enemyTeam))
            {
                combatant.HeroRef.currentHp = combatant.CurrentHp;
            }

            bool playerWon = IsTeamAlive(_playerTeam) && !IsTeamAlive(_enemyTeam);
            _combatLog.Add(playerWon ? "\n<color=green><b>CHIẾN THẮNG!</b></color>" : "\n<color=red><b>THẤT BẠI!</b></color>");

            return new CombatResult
            {
                DidPlayerWin = playerWon,
                CombatLog = _combatLog,
                PlayerSurvivors = _playerTeam.Where(c => c.IsAlive()).Select(c => c.HeroRef).ToList(),
                PlayerCasualties = _playerTeam.Where(c => !c.IsAlive()).Select(c => c.HeroRef).ToList(),
                EnemySurvivors = _enemyTeam.Where(c => c.IsAlive()).Select(c => c.HeroRef).ToList(),
                EnemyCasualties = _enemyTeam.Where(c => !c.IsAlive()).Select(c => c.HeroRef).ToList()
            };
        }

        #region Logic Cốt Lõi của Trận Đấu
        private CombatAction DecideAction(Combatant actor)
        {
            var usableSkills = actor.Skills.Where(s => actor.SkillCooldowns.ContainsKey(s.id) && actor.SkillCooldowns[s.id] == 0).ToList();
            if (usableSkills.Any())
            {
                if (actor.HeroRef.profession == Profession.Healer)
                {
                    var allies = actor.IsPlayerTeam ? _playerTeam : _enemyTeam;
                    bool needsHealing = allies.Any(a => a.IsAlive() && a.CurrentHp / a.MaxHp < 0.6f);
                    var healingSkill = usableSkills.FirstOrDefault(s => s.targeting == TargetingType.LowestHpAlly || s.targeting == TargetingType.AllAllies);
                    if (needsHealing && healingSkill != null) return new CombatAction { Actor = actor, Skill = healingSkill };
                }
                return new CombatAction { Actor = actor, Skill = usableSkills[_rng.Next(usableSkills.Count)] };
            }
            return new CombatAction { Actor = actor, Skill = null };
        }

        private void ExecuteAction(CombatAction action)
        {
            var actor = action.Actor;
            var allies = actor.IsPlayerTeam ? _playerTeam : _enemyTeam;
            var enemies = actor.IsPlayerTeam ? _enemyTeam : _playerTeam;
            if (action.IsBasicAttack)
            {
                var target = GetTargets(actor, TargetingType.SingleFrontEnemy, allies, enemies).FirstOrDefault();
                if (target != null) PerformAttack(actor, target, 1.0f, null);
            }
            else
            {
                var skill = action.Skill;
                var targets = GetTargets(actor, skill.targeting, allies, enemies);
                if (!targets.Any()) return;
                _combatLog.Add($"<color=lightblue>{actor.HeroRef.heroName} dùng kỹ năng [{skill.skillName}]!</color>");
                for (int i = 0; i < skill.hitCount; i++)
                {
                    var currentTargets = (skill.hitCount > 1 && skill.targeting == TargetingType.RandomEnemy) ? GetTargets(actor, skill.targeting, allies, enemies) : targets;
                    foreach (var target in currentTargets)
                    {
                        switch (actor.HeroRef.profession)
                        {
                            case Profession.Healer: PerformHeal(actor, target, skill); break;
                            default: PerformAttack(actor, target, skill.powerRatio, skill); break;
                        }
                    }
                }
                actor.SkillCooldowns[skill.id] = skill.cooldown + 1;
            }
        }

        private void PerformAttack(Combatant attacker, Combatant target, float powerRatio, Skill skill)
        {
            float baseDamage = attacker.GetCurrentAtk() * powerRatio;
            float finalDamage = Mathf.Max(1, baseDamage - target.GetCurrentDef());
            float critChance = attacker.GetCurrentCritChance() + (skill?.id == "SK_ARCHER_01" ? 0.4f : 0f);
            bool isCrit = _rng.NextDouble() < critChance;
            if (isCrit) finalDamage *= attacker.GetCurrentCritDamage();
            int damageInt = Mathf.FloorToInt(finalDamage);
            target.CurrentHp -= damageInt;
            string log = $"{attacker.HeroRef.heroName} tấn công {target.HeroRef.heroName}, gây <color=red>{damageInt}</color> sát thương.";
            if (isCrit) log += " <color=orange>(Chí mạng!)</color>";
            _combatLog.Add(log);
            if (skill != null && skill.appliedEffect != StatusEffectType.None)
            {
                if (_rng.NextDouble() < skill.effectChance) ApplyStatusEffect(attacker, target, skill);
            }
            if (!target.IsAlive()) _combatLog.Add($"<color=grey>{target.HeroRef.heroName} đã bị hạ gục!</color>");
        }

        private void PerformHeal(Combatant healer, Combatant target, Skill skill)
        {
            if (skill.id == "SK_HEALER_03")
            {
                var debuffs = target.ActiveEffects.Where(e => e.Type == StatusEffectType.DefDown || e.Type == StatusEffectType.Poison || e.Type == StatusEffectType.Slow).ToList();
                if (debuffs.Any())
                {
                    target.ActiveEffects.Remove(debuffs.First());
                    _combatLog.Add($"{healer.HeroRef.heroName} thanh tẩy hiệu ứng xấu cho {target.HeroRef.heroName}.");
                }
            }
            float healAmount = healer.GetCurrentAtk() * skill.powerRatio;
            int healInt = Mathf.FloorToInt(healAmount);
            target.CurrentHp = Mathf.Min(target.MaxHp, target.CurrentHp + healInt);
            _combatLog.Add($"{healer.HeroRef.heroName} hồi <color=green>{healInt}</color> HP cho {target.HeroRef.heroName}.");
            if (skill.appliedEffect != StatusEffectType.None) ApplyStatusEffect(healer, target, skill);
        }
        #endregion

        #region Logic Phụ Trợ
        private void ArrangeFormation(List<Combatant> team)
        {
            var warriors = team.Where(c => c.HeroRef.profession == Profession.Warrior).ToList();
            var healers = team.Where(c => c.HeroRef.profession == Profession.Healer).ToList();
            var others = team.Except(warriors).Except(healers).ToList();
            foreach (var w in warriors) w.Position = RowPosition.Front;
            // SỬA LỖI: Sửa 'w.Position' thành 'h.Position'
            foreach (var h in healers) h.Position = RowPosition.Back;
            foreach (var o in others) o.Position = RowPosition.Middle;
        }

        private List<Combatant> GetTargets(Combatant actor, TargetingType targeting, List<Combatant> allies, List<Combatant> enemies)
        {
            var livingAllies = allies.Where(c => c.IsAlive()).ToList();
            var livingEnemies = enemies.Where(c => c.IsAlive()).ToList();
            if (!livingEnemies.Any()) return new List<Combatant>();
            switch (targeting)
            {
                case TargetingType.SingleFrontEnemy:
                    var front = livingEnemies.Where(e => e.Position == RowPosition.Front).ToList();
                    if (front.Any()) return new List<Combatant> { front[_rng.Next(front.Count)] };
                    var middle = livingEnemies.Where(e => e.Position == RowPosition.Middle).ToList();
                    if (middle.Any()) return new List<Combatant> { middle[_rng.Next(middle.Count)] };
                    var back = livingEnemies.Where(e => e.Position == RowPosition.Back).ToList();
                    if (back.Any()) return new List<Combatant> { back[_rng.Next(back.Count)] };
                    return new List<Combatant>();
                case TargetingType.LowestHpAlly: return livingAllies.Any() ? livingAllies.OrderBy(a => a.CurrentHp / a.MaxHp).Take(1).ToList() : new List<Combatant>();
                case TargetingType.AllEnemies: return livingEnemies;
                case TargetingType.AllAllies: return livingAllies;
                case TargetingType.AllAlliesInRow: return livingAllies.Where(a => a.Position == actor.Position).ToList();
                case TargetingType.RandomEnemy: return new List<Combatant> { livingEnemies[_rng.Next(livingEnemies.Count)] };
                case TargetingType.Self: return new List<Combatant> { actor };
                case TargetingType.AdjacentEnemies: return GetTargets(actor, TargetingType.SingleFrontEnemy, allies, enemies);
                default: return new List<Combatant>();
            }
        }

        private void ApplyStatusEffect(Combatant caster, Combatant target, Skill skill)
        {
            float value = 0;
            switch (skill.appliedEffect)
            {
                case StatusEffectType.Poison: value = caster.GetCurrentAtk() * 0.2f; break;
                case StatusEffectType.Slow: value = -20; break;
                case StatusEffectType.CritUp: value = 0.15f; break;
                case StatusEffectType.DefDown: value = target.GetCurrentDef() * -0.15f; break;
                case StatusEffectType.HealOverTime: value = caster.GetCurrentAtk() * 0.5f; break;
                case StatusEffectType.Shield: value = caster.GetCurrentDef() * skill.powerRatio; break;
            }
            target.ActiveEffects.RemoveAll(e => e.Type == skill.appliedEffect);
            var newEffect = new ActiveStatusEffect(skill.appliedEffect, skill.effectDuration, value, caster);
            target.ActiveEffects.Add(newEffect);
            _combatLog.Add($"{target.HeroRef.heroName} bị ảnh hưởng bởi <color=magenta>{skill.appliedEffect}</color> trong {skill.effectDuration} lượt.");
        }

        private void ProcessStartOfTurnEffects(Combatant combatant)
        {
            var effectsToProcess = combatant.ActiveEffects.ToList();
            foreach (var effect in effectsToProcess)
            {
                switch (effect.Type)
                {
                    case StatusEffectType.Poison:
                        int poisonDmg = Mathf.FloorToInt(effect.Value);
                        combatant.CurrentHp -= poisonDmg;
                        _combatLog.Add($"{combatant.HeroRef.heroName} nhận <color=purple>{poisonDmg}</color> sát thương từ Độc.");
                        break;
                    case StatusEffectType.HealOverTime:
                        int hotHeal = Mathf.FloorToInt(effect.Value);
                        combatant.CurrentHp = Mathf.Min(combatant.MaxHp, combatant.CurrentHp + hotHeal);
                        _combatLog.Add($"{combatant.HeroRef.heroName} được hồi <color=green>{hotHeal}</color> HP từ Hồi Phục.");
                        break;
                }
                effect.Duration--;
            }
            combatant.ActiveEffects.RemoveAll(e => e.Duration <= 0);
        }

        private void TickCooldowns(Combatant combatant)
        {
            foreach (var skillId in combatant.SkillCooldowns.Keys.ToList())
            {
                if (combatant.SkillCooldowns[skillId] > 0) combatant.SkillCooldowns[skillId]--;
            }
        }

        private bool IsTeamAlive(List<Combatant> team) => team.Any(c => c.IsAlive());

        private void LogFormation(List<Combatant> team, string teamName)
        {
            _combatLog.Add($"<b>--- {teamName} ---</b>");
            var front = string.Join(", ", team.Where(c => c.Position == RowPosition.Front).Select(c => c.HeroRef.heroName));
            var middle = string.Join(", ", team.Where(c => c.Position == RowPosition.Middle).Select(c => c.HeroRef.heroName));
            var back = string.Join(", ", team.Where(c => c.Position == RowPosition.Back).Select(c => c.HeroRef.heroName));
            _combatLog.Add($"<b>Hàng trước:</b> {(string.IsNullOrEmpty(front) ? "Trống" : front)}");
            _combatLog.Add($"<b>Hàng giữa:</b> {(string.IsNullOrEmpty(middle) ? "Trống" : middle)}");
            _combatLog.Add($"<b>Hàng sau:</b> {(string.IsNullOrEmpty(back) ? "Trống" : back)}");
        }
        #endregion
    }
    #endregion
}