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
        public bool DidPlayerWin;
        public int TotalTurns;
        public List<string> CombatLog;
        public List<CombatEvent> EventLog;
        public List<HeroData> PlayerSurvivors;
        public List<HeroData> PlayerCasualties;
        public List<HeroData> EnemySurvivors;
        public List<HeroData> EnemyCasualties;
        public List<CombatantPosition> InitialPositions;
    }

    [System.Serializable]
    public struct CombatantPosition
    {
        public string InstanceID;
        public int SlotIndex;
        public bool IsAlly;
    }

    public enum CombatEventType { TurnStart, SkillCast, Attack, Heal, TakeDamage, StatusEffect, Death, BattleEnd }

    [System.Serializable]
    public class CombatEvent
    {
        public CombatEventType EventType;
        public string SourceID;
        public string TargetID;
        public int Value;
        public bool IsCrit;
        public string Message;
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
        public float AtkMultiplier { get; set; } = 1.0f;

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

        public float GetCurrentAtk() => HeroRef.GetFinalStats().atk * AtkMultiplier;
        public float GetCurrentDef() => HeroRef.GetFinalStats().def + ActiveEffects.Where(e => e.Type == StatusEffectType.DefDown).Sum(e => e.Value);
        public float GetCurrentSpd() => HeroRef.GetFinalStats().spd + ActiveEffects.Where(e => e.Type == StatusEffectType.Slow).Sum(e => e.Value);
        public float GetCurrentCritChance() => HeroRef.GetFinalStats().critChance + ActiveEffects.Where(e => e.Type == StatusEffectType.CritUp).Sum(e => e.Value);
        public float GetCurrentCritDamage() => HeroRef.GetFinalStats().critDamage;
        public bool IsAlive() => CurrentHp > 0;
        
        // --- NEW: Track Revival Status ---
        public bool HasRevivedOnce { get; set; } = false;

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
        private List<CombatEvent> _eventLog;
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

        private bool _isHealingChallenge;

        public CombatResult Simulate(List<HeroData> playerHeroes, List<string> enemyMonsterIDs, int difficultyLevel = 1, bool isHealingChallenge = false)
        {
            _isHealingChallenge = isHealingChallenge;
            _playerTeam = playerHeroes.Select((h, i) => new Combatant(h, true, i)).ToList();
            var enemyHeroes = enemyMonsterIDs.Select(id => DataManager.Instance.GetMonsterByID(id, difficultyLevel))
                                             .Where(h => h != null)
                                             .ToList();
            _enemyTeam = enemyHeroes.Select((h, i) => new Combatant(h, false, i)).ToList();
            return RunSimulation();
        }

        public CombatResult SimulateBoss(List<HeroData> playerHeroes, string bossID)
        {
            _isHealingChallenge = false;
            _playerTeam = playerHeroes.Select((h, i) => new Combatant(h, true, i)).ToList();
            
            var bossHero = DataManager.Instance.GetMonsterByID(bossID, 1);
            _enemyTeam = new List<Combatant>();
            if (bossHero != null)
            {
                _enemyTeam.Add(new Combatant(bossHero, false, 0));
            }

            return RunSimulation();
        }

        private CombatResult RunSimulation()
        {
            _combatLog = new List<string> { LocalizationSystem.GetText("combat_log_start") };
            _eventLog = new List<CombatEvent>();

            _playerTeam.ForEach(c => c.AssignSkills(_allAvailableSkills, _rng));
            _enemyTeam.ForEach(c => c.AssignSkills(_allAvailableSkills, _rng));

            ArrangeFormation(_playerTeam);
            ArrangeFormation(_enemyTeam);

            List<CombatantPosition> initialPos = new List<CombatantPosition>();
            AssignGridSlots(_playerTeam, true, initialPos);
            AssignGridSlots(_enemyTeam, false, initialPos);

            // Xử lý Trait: Hào quang (Aura)
            bool playerHasAura = _playerTeam.Any(c => c.HeroRef.traitIDs.Any(t => t != null && t.Contains("AURA")));
            if (playerHasAura) { _playerTeam.ForEach(c => c.AtkMultiplier += 0.1f); _combatLog.Add(LocalizationSystem.GetText("combat_log_aura_player")); }
            bool enemyHasAura = _enemyTeam.Any(c => c.HeroRef.traitIDs.Any(t => t != null && t.Contains("AURA")));
            if (enemyHasAura) { _enemyTeam.ForEach(c => c.AtkMultiplier += 0.1f); _combatLog.Add(LocalizationSystem.GetText("combat_log_aura_enemy")); }

            LogFormation(_playerTeam, LocalizationSystem.GetText("combat_log_player_team"));
            LogFormation(_enemyTeam, LocalizationSystem.GetText("combat_log_enemy_team"));

            int turn = 1;
            int maxTurns = _isHealingChallenge ? 50 : 50; // The user requested 50 for the healer tower as well.
            
            while (IsTeamAlive(_playerTeam) && (_isHealingChallenge ? !IsTeamFullyHealed(_enemyTeam) : IsTeamAlive(_enemyTeam)))
            {
                _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_turn_header"), turn));
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.TurnStart, Value = turn });
                var turnOrder = _playerTeam.Concat(_enemyTeam).Where(c => c.IsAlive()).OrderByDescending(c => c.GetCurrentSpd()).ToList();

                foreach (var combatant in turnOrder)
                {
                    if (!combatant.IsAlive()) continue;
                    ProcessStartOfTurnEffects(combatant);
                    if (!combatant.IsAlive()) continue;
                    TickCooldowns(combatant);
                    var action = DecideAction(combatant);
                    ExecuteAction(action);
                    
                    if (!IsTeamAlive(_playerTeam) || (_isHealingChallenge ? IsTeamFullyHealed(_enemyTeam) : !IsTeamAlive(_enemyTeam))) break;
                }

                turn++;
                if (turn > maxTurns) { _combatLog.Add(LocalizationSystem.GetText("combat_log_draw_timeout")); break; }
            }

            // Cập nhật lại currentHp của HeroData gốc trước khi trả về
            foreach(var combatant in _playerTeam.Concat(_enemyTeam))
            {
                combatant.HeroRef.currentHp = combatant.CurrentHp;
            }

            bool playerWon = IsTeamAlive(_playerTeam) && (_isHealingChallenge ? IsTeamFullyHealed(_enemyTeam) : !IsTeamAlive(_enemyTeam));
            _combatLog.Add(playerWon ? LocalizationSystem.GetText("combat_log_victory") : LocalizationSystem.GetText("combat_log_defeat"));

            return new CombatResult
            {
                DidPlayerWin = playerWon,
                TotalTurns = turn,
                CombatLog = _combatLog,
                EventLog = _eventLog,
                PlayerSurvivors = _playerTeam.Where(c => c.IsAlive()).Select(c => c.HeroRef).ToList(),
                PlayerCasualties = _playerTeam.Where(c => !c.IsAlive()).Select(c => c.HeroRef).ToList(),
                EnemySurvivors = _enemyTeam.Where(c => c.IsAlive()).Select(c => c.HeroRef).ToList(),
                EnemyCasualties = _enemyTeam.Where(c => !c.IsAlive()).Select(c => c.HeroRef).ToList(),
                InitialPositions = initialPos
            };
        }

        private bool IsTeamFullyHealed(List<Combatant> team)
        {
            return team.All(c => c.CurrentHp >= c.MaxHp);
        }

        private void AssignGridSlots(List<Combatant> team, bool isAlly, List<CombatantPosition> positionList)
        {
            // Grid 3x3 layout (0..8)
            // Left to right, top to bottom
            // Ally: Back=0,3,6 | Middle=1,4,7 | Front=2,5,8
            // Enemy: Front=0,3,6 | Middle=1,4,7 | Back=2,5,8

            int[] frontSlots = isAlly ? new int[] { 2, 5, 8 } : new int[] { 0, 3, 6 };
            int[] midSlots = new int[] { 1, 4, 7 };
            int[] backSlots = isAlly ? new int[] { 0, 3, 6 } : new int[] { 2, 5, 8 };

            int fIdx = 0, mIdx = 0, bIdx = 0;

            foreach (var c in team)
            {
                int slot = -1;
                if (c.Position == RowPosition.Front && fIdx < 3) slot = frontSlots[fIdx++];
                else if (c.Position == RowPosition.Back && bIdx < 3) slot = backSlots[bIdx++];
                else if (mIdx < 3) slot = midSlots[mIdx++];
                else if (fIdx < 3) slot = frontSlots[fIdx++]; // Fallback nếu mid đầy
                else if (bIdx < 3) slot = backSlots[bIdx++]; // Fallback

                // Fallback cuối cùng nếu có nhiều hơn 9 tướng (như trường hợp đánh Boss 15 tướng)
                if (slot == -1) 
                {
                     slot = mIdx + 9; // Tạm xếp ra ngoài grid cơ bản
                     mIdx++;
                }

                positionList.Add(new CombatantPosition
                {
                    InstanceID = c.InstanceID,
                    SlotIndex = slot,
                    IsAlly = isAlly
                });
            }
        }

        #region Logic Cốt Lõi của Trận Đấu
        private CombatAction DecideAction(Combatant actor)
        {
            var usableSkills = actor.Skills.Where(s => actor.SkillCooldowns.ContainsKey(s.id) && actor.SkillCooldowns[s.id] == 0).ToList();
            if (usableSkills.Any())
            {
                if (actor.HeroRef.profession == Profession.Healer)
                {
                    // In healing challenge, healers want to heal the most injured enemy (soldier), otherwise allies.
                    var allies = _isHealingChallenge ? _enemyTeam : (actor.IsPlayerTeam ? _playerTeam : _enemyTeam);
                    bool needsHealing = allies.Any(a => a.IsAlive() && a.CurrentHp / a.MaxHp < 1.0f);
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
                if (_isHealingChallenge && actor.HeroRef.profession == Profession.Healer)
                {
                    var target = GetTargets(actor, TargetingType.LowestHpAlly, _enemyTeam, _playerTeam).FirstOrDefault(); // Heal the enemy team (soldiers)
                    if (target != null) 
                    {
                        // Use a dummy skill for the power ratio
                        Skill dummyHeal = new Skill { powerRatio = 1.0f, appliedEffect = StatusEffectType.None };
                        PerformHeal(actor, target, dummyHeal);
                    }
                }
                else
                {
                    var target = GetTargets(actor, TargetingType.SingleFrontEnemy, allies, enemies).FirstOrDefault();
                    if (target != null) PerformAttack(actor, target, 1.0f, null);
                }
            }
            else
            {
                var skill = action.Skill;
                
                // In a healing challenge, if it's a healing skill, target the injured soldiers (_enemyTeam)
                var skillAllies = (_isHealingChallenge && actor.HeroRef.profession == Profession.Healer) ? _enemyTeam : allies; 
                var skillEnemies = (_isHealingChallenge && actor.HeroRef.profession == Profession.Healer) ? _playerTeam : enemies;

                var targets = GetTargets(actor, skill.targeting, skillAllies, skillEnemies);
                if (!targets.Any()) return;
                
                _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_skill_cast"), actor.HeroRef.heroName, skill.skillName));
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.SkillCast, SourceID = actor.InstanceID, Message = skill.skillName });
                
                for (int i = 0; i < skill.hitCount; i++)
                {
                    var currentTargets = (skill.hitCount > 1 && skill.targeting == TargetingType.RandomEnemy) ? 
                                         GetTargets(actor, skill.targeting, skillAllies, skillEnemies) : targets;
                    
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

            // Xử lý Trait: Kẻ săn mồi (Predator)
            if (attacker.HeroRef.traitIDs.Any(t => t != null && t.Contains("PREDATOR")) && target.CurrentHp < target.MaxHp * 0.3f)
            {
                baseDamage *= 1.5f; // Sát thương thêm 50%
                _combatLog.Add(LocalizationSystem.GetText("combat_log_predator_trigger"));
            }

            float finalDamage = Mathf.Max(1, baseDamage - target.GetCurrentDef());
            
            float archerBonus = DataManager.Instance?.GameConfig?.CombatSettings?.archerBonusCritChance ?? 0.4f;
            float critChance = attacker.GetCurrentCritChance() + (skill?.id == "SK_ARCHER_01" ? archerBonus : 0f);

            bool isCrit = _rng.NextDouble() < critChance;
            if (isCrit) finalDamage *= attacker.GetCurrentCritDamage();
            int damageInt = Mathf.FloorToInt(finalDamage);
            target.CurrentHp -= damageInt;
            string log = string.Format(LocalizationSystem.GetText("combat_log_attack_normal"), attacker.HeroRef.heroName, target.HeroRef.heroName, damageInt);
            if (isCrit) log += LocalizationSystem.GetText("combat_log_attack_crit");
            
            // Xử lý Trait: Hút máu (Lifesteal)
            if (attacker.HeroRef.traitIDs.Any(t => t != null && t.Contains("LIFESTEAL")))
            {
                int healAmount = Mathf.FloorToInt(damageInt * 0.2f); // Hút lại 20% sát thương
                attacker.CurrentHp = Mathf.Min(attacker.MaxHp, attacker.CurrentHp + healAmount);
                log += $" <color=#00ff00>(+{healAmount} HP Hút máu)</color>";
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = attacker.InstanceID, TargetID = attacker.InstanceID, Value = healAmount });
            }

            _combatLog.Add(log);
            _eventLog.Add(new CombatEvent { EventType = CombatEventType.Attack, SourceID = attacker.InstanceID, TargetID = target.InstanceID, Value = damageInt, IsCrit = isCrit });

            if (skill != null && skill.appliedEffect != StatusEffectType.None)
            {
                if (_rng.NextDouble() < skill.effectChance) ApplyStatusEffect(attacker, target, skill);
            }
            if (!target.IsAlive()) 
            {
                // Xử lý Trait: Hồi sinh (Revive)
                if (target.HeroRef.traitIDs.Any(t => t != null && t.Contains("REVIVE")) && !target.HasRevivedOnce)
                {
                    target.HasRevivedOnce = true;
                    int reviveHp = Mathf.FloorToInt(target.MaxHp * 0.3f); // Hồi sinh với 30% máu
                    target.CurrentHp = reviveHp;
                    _combatLog.Add($"<color=#ffff00> {target.HeroRef.heroName} đã kích hoạt Tái Sinh và hồi {reviveHp} HP!</color>");
                    _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = target.InstanceID, TargetID = target.InstanceID, Value = reviveHp });
                }
                else
                {
                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_death"), target.HeroRef.heroName));
                    _eventLog.Add(new CombatEvent { EventType = CombatEventType.Death, TargetID = target.InstanceID });
                }
            }
        }

        private void PerformHeal(Combatant healer, Combatant target, Skill skill)
        {
            if (skill.id == "SK_HEALER_03")
            {
                var debuffs = target.ActiveEffects.Where(e => e.Type == StatusEffectType.DefDown || e.Type == StatusEffectType.Poison || e.Type == StatusEffectType.Slow).ToList();
                if (debuffs.Any())
                {
                    target.ActiveEffects.Remove(debuffs.First());
                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_cleanse"), healer.HeroRef.heroName, target.HeroRef.heroName));
                }
            }
            float healAmount = healer.GetCurrentAtk() * skill.powerRatio;
            int healInt = Mathf.FloorToInt(healAmount);
            target.CurrentHp = Mathf.Min(target.MaxHp, target.CurrentHp + healInt);
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_heal"), healer.HeroRef.heroName, healInt, target.HeroRef.heroName));
            _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = healer.InstanceID, TargetID = target.InstanceID, Value = healInt });
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
            // SỬA LỖI: Sửa 'w.Position' thành 'h.Position' cho Healer
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
            var conf = DataManager.Instance?.GameConfig?.CombatSettings;
            switch (skill.appliedEffect)
            {
                case StatusEffectType.Poison: value = caster.GetCurrentAtk() * (conf?.poisonDamageRatio ?? 0.2f); break;
                case StatusEffectType.Slow: value = conf?.slowSpeedReduction ?? -20f; break;
                case StatusEffectType.CritUp: value = conf?.critUpBonus ?? 0.15f; break;
                case StatusEffectType.DefDown: value = target.GetCurrentDef() * (conf?.defDownRatio ?? -0.15f); break;
                case StatusEffectType.HealOverTime: value = caster.GetCurrentAtk() * (conf?.healOverTimeRatio ?? 0.5f); break;
                case StatusEffectType.Shield: value = caster.GetCurrentDef() * skill.powerRatio; break;
            }
            target.ActiveEffects.RemoveAll(e => e.Type == skill.appliedEffect);
            var newEffect = new ActiveStatusEffect(skill.appliedEffect, skill.effectDuration, value, caster);
            target.ActiveEffects.Add(newEffect);
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_status_applied"), target.HeroRef.heroName, skill.appliedEffect, skill.effectDuration));
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
                        _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_poison_tick"), combatant.HeroRef.heroName, poisonDmg));
                        _eventLog.Add(new CombatEvent { EventType = CombatEventType.TakeDamage, TargetID = combatant.InstanceID, Value = poisonDmg });
                        break;
                    case StatusEffectType.HealOverTime:
                        int hotHeal = Mathf.FloorToInt(effect.Value);
                        combatant.CurrentHp = Mathf.Min(combatant.MaxHp, combatant.CurrentHp + hotHeal);
                        _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_regen_tick"), combatant.HeroRef.heroName, hotHeal));
                        _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, TargetID = combatant.InstanceID, Value = hotHeal });
                        break;
                }
                effect.Duration--;
            }
            combatant.ActiveEffects.RemoveAll(e => e.Duration <= 0);

            // Xử lý Trait: Tái sinh (Regeneration)
            if (combatant.HeroRef.traitIDs.Any(t => t != null && t.Contains("REGEN")))
            {
                int regenAmount = Mathf.FloorToInt(combatant.MaxHp * 0.05f);
                combatant.CurrentHp = Mathf.Min(combatant.MaxHp, combatant.CurrentHp + regenAmount);
                _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_trait_regen"), combatant.HeroRef.heroName, regenAmount));
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, TargetID = combatant.InstanceID, Value = regenAmount });
            }
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
            var empty = LocalizationSystem.GetText("combat_log_row_empty");
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_front"), string.IsNullOrEmpty(front) ? empty : front));
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_mid"), string.IsNullOrEmpty(middle) ? empty : middle));
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_back"), string.IsNullOrEmpty(back) ? empty : back));
        }
        #endregion
    }
    #endregion
}