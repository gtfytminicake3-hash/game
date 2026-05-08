// Cáº§n using namespace nÆ¡i báº¡n Ä‘á»‹nh nghÄ©a Skill.cs vÃ  cÃ¡c enum
using LegendOfBlood;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegendOfBlood.Combat
{
    #region 0. Lá»šP Dá»® LIá»†U Káº¾T QUáº¢ TRáº¬N Äáº¤U
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

    #region 1. CÃC Lá»šP Dá»® LIá»†U PHá»¤ TRá»¢
    public enum RowPosition { Front, Middle, Back }

    public class ActiveStatusEffect
    {
        public StatusEffectType Type { get; }
        public int Duration { get; set; }
        public float Value { get; set; }
        public Combatant Caster { get; }
        public int StackCount { get; set; }

        public ActiveStatusEffect(StatusEffectType type, int duration, float value, Combatant caster, int stackCount = 1)
        {
            Type = type;
            Duration = duration;
            Value = value;
            Caster = caster;
            StackCount = stackCount;
        }
    }

    public class CombatAction
    {
        public Combatant Actor { get; set; }
        public Skill Skill { get; set; }
        public bool IsBasicAttack => Skill == null;
    }
    #endregion

    #region 2. Lá»šP COMBATANT - TRÃI TIM Cá»¦A Há»† THá»NG
    public class Combatant
    {
        public HeroData HeroRef { get; }
        public bool IsPlayerTeam { get; }
        public string InstanceID { get; }
        public RowPosition Position { get; set; }
        public List<Skill> Skills { get; private set; }
        public Dictionary<string, int> SkillCooldowns { get; set; }
        public float MaxHp { get; private set; }
        public float CurrentHp { get; set; }
        public List<ActiveStatusEffect> ActiveEffects { get; private set; }
        public float AtkMultiplier { get; set; } = 1.0f;
        
        public bool HasRevivedOnce { get; set; } = false;
        
        // --- NEW COMBO PROPS ---
        public Combatant LinkedAlly { get; set; }
        public bool ExtraTurn { get; set; }
        public float TempDefIgnore { get; set; }
        public float TempCritDmgBonus { get; set; }

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

        public float GetCurrentAtk() 
        {
            float atk = HeroRef.GetFinalStats().atk * AtkMultiplier;
            float buffBonus = ActiveEffects.Where(e => e.Type == StatusEffectType.AtkUp).Sum(e => e.Value);
            float debuffMalus = ActiveEffects.Where(e => e.Type == StatusEffectType.AtkDown).Sum(e => e.Value);
            return atk * (1f + buffBonus - debuffMalus);
        }
        
        public float GetCurrentDef() 
        {
            float def = HeroRef.GetFinalStats().def;
            float defDown = ActiveEffects.Where(e => e.Type == StatusEffectType.DefDown).Sum(e => Math.Abs(e.Value));
            return Mathf.Max(0, def - defDown);
        }
        
        public float GetCurrentSpd() 
        {
            return HeroRef.GetFinalStats().spd + ActiveEffects.Where(e => e.Type == StatusEffectType.Slow).Sum(e => e.Value); // Slow value is negative
        }
        
        public float GetCurrentCritChance() 
        {
            return HeroRef.GetFinalStats().critChance + ActiveEffects.Where(e => e.Type == StatusEffectType.CritUp).Sum(e => e.Value);
        }
        
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

    #region 3. Há»† THá»NG CHIáº¾N Äáº¤U CHÃNH
    public class CombatSystem
    {
        private System.Random _rng;
        private List<Combatant> _playerTeam;
        private List<Combatant> _enemyTeam;
        private List<string> _combatLog;
        private List<CombatEvent> _eventLog;
        private readonly List<Skill> _allAvailableSkills;
        private bool _isHealingChallenge;

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

        public CombatResult Simulate(List<HeroData> playerHeroes, List<string> enemyMonsterIDs, int difficultyLevel = 1, bool isHealingChallenge = false)
        {
            _isHealingChallenge = isHealingChallenge;
            _playerTeam = playerHeroes.Select((h, i) => new Combatant(h, true, i)).ToList();
            
            enemyMonsterIDs = enemyMonsterIDs ?? new List<string>();
            
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
            if (bossHero != null) _enemyTeam.Add(new Combatant(bossHero, false, 0));
            return RunSimulation();
        }

        private CombatResult RunSimulation()
        {
            _combatLog = new List<string> { LocalizationSystem.GetText("combat_log_start") ?? "Tráº­n Ä‘áº¥u báº¯t Ä‘áº§u!" };
            _eventLog = new List<CombatEvent>();

            _playerTeam.ForEach(c => c.AssignSkills(_allAvailableSkills, _rng));
            _enemyTeam.ForEach(c => c.AssignSkills(_allAvailableSkills, _rng));

            ArrangeFormation(_playerTeam);
            ArrangeFormation(_enemyTeam);

            List<CombatantPosition> initialPos = new List<CombatantPosition>();
            AssignGridSlots(_playerTeam, true, initialPos);
            AssignGridSlots(_enemyTeam, false, initialPos);

            // Aura Traits
            bool playerHasAura = _playerTeam.Any(c => c.HeroRef.traitIDs.Any(t => t != null && t.Contains("AURA")));
            if (playerHasAura) { _playerTeam.ForEach(c => c.AtkMultiplier += 0.1f); _combatLog.Add(LocalizationSystem.GetText("combat_log_aura_player") ?? "Äá»™i Player nháº­n HÃ o quang +10% ATK!"); }
            bool enemyHasAura = _enemyTeam.Any(c => c.HeroRef.traitIDs.Any(t => t != null && t.Contains("AURA")));
            if (enemyHasAura) { _enemyTeam.ForEach(c => c.AtkMultiplier += 0.1f); _combatLog.Add(LocalizationSystem.GetText("combat_log_aura_enemy") ?? "Äá»™i Äá»‹ch nháº­n HÃ o quang +10% ATK!"); }

            LogFormation(_playerTeam, LocalizationSystem.GetText("combat_log_player_team") ?? "Äá»™i HÃ¬nh NgÆ°á»i ChÆ¡i");
            LogFormation(_enemyTeam, LocalizationSystem.GetText("combat_log_enemy_team") ?? "Äá»™i HÃ¬nh Káº» Äá»‹ch");

            int turn = 1;
            int maxTurns = 50; 
            
            while (IsTeamAlive(_playerTeam) && (_isHealingChallenge ? !IsTeamFullyHealed(_enemyTeam) : IsTeamAlive(_enemyTeam)))
            {
                _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_turn_header") ?? "--- LÆ°á»£t {0} ---", turn));
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.TurnStart, Value = turn });
                var turnOrder = _playerTeam.Concat(_enemyTeam).Where(c => c.IsAlive()).OrderByDescending(c => c.GetCurrentSpd()).ToList();

                foreach (var combatant in turnOrder)
                {
                    if (!combatant.IsAlive()) continue;
                    ProcessStartOfTurnEffects(combatant); // Status effects DMG processing & duration decrement
                    if (!combatant.IsAlive()) continue;

                    if (combatant.ActiveEffects.Any(e => e.Type == StatusEffectType.Stun))
                    {
                        string cName = combatant.HeroRef.heroName;
                        _combatLog.Add($"<color=#aaaaaa>{cName} bá»‹ khÃ³a hÃ nh Ä‘á»™ng (Máº¥t lÆ°á»£t)!</color>");
                        TickCooldowns(combatant); 
                        continue;
                    }

                    do 
                    {
                        combatant.ExtraTurn = false;
                        TickCooldowns(combatant);
                        var action = DecideAction(combatant);
                        ExecuteAction(action);
                    } 
                    while (combatant.ExtraTurn && combatant.IsAlive() && IsTeamAlive(_playerTeam) && (_isHealingChallenge ? !IsTeamFullyHealed(_enemyTeam) : IsTeamAlive(_enemyTeam)));
                    
                    if (!IsTeamAlive(_playerTeam) || (_isHealingChallenge ? IsTeamFullyHealed(_enemyTeam) : !IsTeamAlive(_enemyTeam))) break;
                }

                turn++;
                if (turn > maxTurns) { _combatLog.Add(LocalizationSystem.GetText("combat_log_draw_timeout") ?? "Tráº­n Ä‘áº¥u hÃ²a do háº¿t lÆ°á»£t."); break; }
            }

            foreach(var combatant in _playerTeam.Concat(_enemyTeam))
            {
                combatant.HeroRef.currentHp = combatant.CurrentHp;
            }

            bool playerWon = IsTeamAlive(_playerTeam) && (_isHealingChallenge ? IsTeamFullyHealed(_enemyTeam) : !IsTeamAlive(_enemyTeam));
            _combatLog.Add(playerWon ? (LocalizationSystem.GetText("combat_log_victory") ?? "Chiáº¿n tháº¯ng!") : (LocalizationSystem.GetText("combat_log_defeat") ?? "Tháº¥t báº¡i!"));

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

        private bool IsTeamFullyHealed(List<Combatant> team) => team.All(c => c.CurrentHp >= c.MaxHp);

        private void AssignGridSlots(List<Combatant> team, bool isAlly, List<CombatantPosition> positionList)
        {
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
                else if (fIdx < 3) slot = frontSlots[fIdx++];
                else if (bIdx < 3) slot = backSlots[bIdx++];
                if (slot == -1) { slot = mIdx + 9; mIdx++; }

                positionList.Add(new CombatantPosition { InstanceID = c.InstanceID, SlotIndex = slot, IsAlly = isAlly });
            }
        }

        #region Logic Cá»‘t LÃµi cá»§a Tráº­n Äáº¥u
        private CombatAction DecideAction(Combatant actor)
        {
            var usableSkills = actor.Skills.Where(s => actor.SkillCooldowns.ContainsKey(s.id) && actor.SkillCooldowns[s.id] <= 0).ToList();
            if (usableSkills.Any())
            {
                if (actor.HeroRef.profession == Profession.Healer)
                {
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
                    var target = GetTargets(actor, TargetingType.LowestHpAlly, _enemyTeam, _playerTeam).FirstOrDefault();
                    if (target != null) 
                    {
                        Skill dummyHeal = ScriptableObject.CreateInstance<Skill>();
                        dummyHeal.powerRatio = 1.0f; dummyHeal.appliedEffect = StatusEffectType.None; dummyHeal.id = "DUMMY_HEAL";
                        PerformHeal(actor, target, dummyHeal, 1.0f);
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
                var skillAllies = (_isHealingChallenge && actor.HeroRef.profession == Profession.Healer) ? _enemyTeam : allies; 
                var skillEnemies = (_isHealingChallenge && actor.HeroRef.profession == Profession.Healer) ? _playerTeam : enemies;

                var targets = GetTargets(actor, skill.targeting, skillAllies, skillEnemies);
                if (!targets.Any() && skill.targeting != TargetingType.Self && skill.targeting != TargetingType.AllAllies) return;
                
                string sCastStr = LocalizationSystem.GetText("combat_log_skill_cast") ?? "{0} thi triá»ƒn thuáº­t {1}!";
                _combatLog.Add(string.Format(sCastStr, actor.HeroRef.heroName, LocalizationSystem.GetText(skill.skillName)));
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.SkillCast, SourceID = actor.InstanceID, Message = LocalizationSystem.GetText(skill.skillName) });
                
                ExecuteCustomComboSkill(actor, targets, skill, skillAllies, skillEnemies);
                actor.SkillCooldowns[skill.id] = skill.cooldown;
            }
        }

        private void ExecuteCustomComboSkill(Combatant actor, List<Combatant> targets, Skill skill, List<Combatant> allies, List<Combatant> enemies)
        {
            void DefaultExecute(float dmgRatio = -1) 
            {
                float finalRatio = dmgRatio > 0 ? dmgRatio : skill.powerRatio;
                for (int i = 0; i < skill.hitCount; i++)
                {
                    var currentTargets = (skill.hitCount > 1 && skill.targeting == TargetingType.RandomEnemy) ? 
                                         GetTargets(actor, skill.targeting, allies, enemies) : targets;
                    
                    foreach (var target in currentTargets)
                    {
                        if (actor.HeroRef.profession == Profession.Healer && skill.targeting != TargetingType.AllEnemies && skill.targeting != TargetingType.SingleFrontEnemy && skill.targeting != TargetingType.SingleBackEnemy && skill.targeting != TargetingType.RandomEnemy) 
                            PerformHeal(actor, target, skill, finalRatio); 
                        else 
                            PerformAttack(actor, target, finalRatio, skill);
                    }
                }
            }

            switch(skill.id)
            {
                // WARRIOR
                case "SK_WAR_1": 
                    DefaultExecute(); AddStatusEffect(actor, actor, StatusEffectType.Aegis, 99, 0f, 1); 
                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_war_1"), actor.HeroRef.heroName)); break;
                case "SK_WAR_2": 
                    DefaultExecute(); AddStatusEffect(actor, actor, StatusEffectType.Aegis, 99, 0f, 1);
                    foreach(var t in enemies.Where(e => e.IsAlive())) AddStatusEffect(actor, t, StatusEffectType.Taunt, 1, 0f, 1);
                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_war_2"), actor.HeroRef.heroName)); break;
                case "SK_WAR_3": 
                    int aegisStack = GetAndClearStack(actor, StatusEffectType.Aegis); float shieldRatio = 0.15f + (aegisStack * 0.10f); float shieldValue = actor.MaxHp * shieldRatio; 
                    AddStatusEffect(actor, actor, StatusEffectType.Shield, 2, shieldValue, 1); 
                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_war_3"), actor.HeroRef.heroName, shieldValue, aegisStack)); break;
                case "SK_WAR_4": 
                    DefaultExecute(); if (actor.ActiveEffects.Any(e => e.Type == StatusEffectType.Shield)) { foreach(var target in targets) AddStatusEffect(actor, target, StatusEffectType.Stun, 1, 0f, 1); _combatLog.Add(LocalizationSystem.GetText("combo_war_4")); } break;
                case "SK_WAR_5": 
                    int aegis5 = GetAndClearStack(actor, StatusEffectType.Aegis);
                    actor.TempCritDmgBonus = aegis5 * 0.30f;
                    DefaultExecute();
                    actor.TempCritDmgBonus = 0f; break;

                // MAGE
                case "SK_MAG_1": 
                    DefaultExecute();
                    foreach(var target in targets) AddStatusEffect(actor, target, StatusEffectType.PoisonMark, 99, 0f, 1); break;
                case "SK_MAG_2": 
                    DefaultExecute();
                    foreach(var target in targets) { AddStatusEffect(actor, target, StatusEffectType.Slow, 2, -20f, 1); AddStatusEffect(actor, target, StatusEffectType.PoisonMark, 99, 0f, 1); } break;
                case "SK_MAG_3": 
                    DefaultExecute(); 
                    foreach(var target in targets) {
                        int stunDuration = target.ActiveEffects.Any(e => e.Type == StatusEffectType.PoisonMark) ? 2 : 1;
                        AddStatusEffect(actor, target, StatusEffectType.Stun, stunDuration, 0f, 1);
                    } break;
                case "SK_MAG_4": 
                    Combatant magTarget = targets.FirstOrDefault(); if (magTarget != null) { float dmg = Mathf.Max(1, actor.GetCurrentAtk() * skill.powerRatio - magTarget.GetCurrentDef()); PerformAttack(actor, magTarget, skill.powerRatio, skill); int healAmt = (int)dmg; actor.CurrentHp = Mathf.Min(actor.MaxHp, actor.CurrentHp + healAmt); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_mag_4"), actor.HeroRef.heroName, healAmt)); if (magTarget.ActiveEffects.Any(e=>e.Type == StatusEffectType.PoisonMark)) { var adj = enemies.Where(e => e.IsAlive() && e != magTarget).OrderBy(e => _rng.Next()).FirstOrDefault(); if (adj != null) AddStatusEffect(actor, adj, StatusEffectType.PoisonMark, 99, 0f, 1); } } break;
                case "SK_MAG_5": 
                    DefaultExecute(); foreach(var e in enemies.Where(e => e.IsAlive())) { int marks = GetAndClearStack(e, StatusEffectType.PoisonMark); if(marks > 0) { float trueDmg = 0.05f * e.MaxHp * marks; e.CurrentHp -= (int)trueDmg; _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_mag_5"), e.HeroRef.heroName, trueDmg)); _eventLog.Add(new CombatEvent { EventType = CombatEventType.Attack, TargetID = e.InstanceID, Value = (int)trueDmg }); CheckDeath(e); } } break;

                // ARCHER
                case "SK_ARC_1":
                    DefaultExecute(); foreach(var t in targets) AddStatusEffect(actor, t, StatusEffectType.Weakness, 2, 0f, 1); break;
                case "SK_ARC_2": 
                    AddStatusEffect(actor, actor, StatusEffectType.NextAttackCrit, 1, 0f, 1); AddStatusEffect(actor, actor, StatusEffectType.AtkUp, 2, 0.3f, 1); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_arc_2"), actor.HeroRef.heroName)); break;
                case "SK_ARC_3":
                    foreach(var t in targets) t.TempDefIgnore = t.ActiveEffects.Any(e => e.Type == StatusEffectType.Weakness) ? 0.3f : 0f;
                    DefaultExecute();
                    foreach(var t in enemies) t.TempDefIgnore = 0f; break;
                case "SK_ARC_4": 
                    actor.SkillCooldowns = actor.SkillCooldowns.ToDictionary(k => k.Key, v => Mathf.Max(0, v.Value - 2)); actor.ActiveEffects.RemoveAll(e => e.Type == StatusEffectType.Poison || e.Type == StatusEffectType.Slow || e.Type == StatusEffectType.DefDown || e.Type == StatusEffectType.AtkDown || e.Type == StatusEffectType.Stun || e.Type == StatusEffectType.Weakness); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_arc_4"), actor.HeroRef.heroName)); break;
                case "SK_ARC_5": 
                    Combatant arcTarget = targets.FirstOrDefault(); if (arcTarget != null) { if (GetAndClearStack(arcTarget, StatusEffectType.Weakness) > 0) arcTarget.TempDefIgnore = 1.0f; PerformAttack(actor, arcTarget, skill.powerRatio, skill); arcTarget.TempDefIgnore = 0f; if (!arcTarget.IsAlive()) { actor.ExtraTurn = true; _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_arc_5"), actor.HeroRef.heroName)); } } break;

                // HEALER
                case "SK_HEA_1":
                    DefaultExecute(); foreach(var t in targets) AddStatusEffect(actor, t, StatusEffectType.LifeSeed, 99, 0f, 1); break;
                case "SK_HEA_2": 
                    foreach(var t in allies.Where(a => a.IsAlive())) { AddStatusEffect(actor, t, StatusEffectType.AtkUp, 2, 0.2f, 1); if (t.ActiveEffects.Any(e=>e.Type == StatusEffectType.LifeSeed)) AddStatusEffect(actor, t, StatusEffectType.CritUp, 2, 0.15f, 1); } _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_hea_2"), actor.HeroRef.heroName)); break;
                case "SK_HEA_3": 
                    Combatant linkTarget = targets.FirstOrDefault(); if (linkTarget != null && linkTarget != actor) { linkTarget.LinkedAlly = actor; actor.LinkedAlly = linkTarget; AddStatusEffect(actor, linkTarget, StatusEffectType.DamageLink, 2, 0f, 1); AddStatusEffect(actor, actor, StatusEffectType.DamageLink, 2, 0f, 1); AddStatusEffect(actor, linkTarget, StatusEffectType.LifeSeed, 99, 0f, 1); AddStatusEffect(actor, actor, StatusEffectType.LifeSeed, 99, 0f, 1); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_hea_3"), actor.HeroRef.heroName, linkTarget.HeroRef.heroName)); } break;
                case "SK_HEA_4": 
                    foreach(var t in allies.Where(a => a.IsAlive())) { t.ActiveEffects.RemoveAll(e => e.Type == StatusEffectType.Poison || e.Type == StatusEffectType.Slow || e.Type == StatusEffectType.DefDown || e.Type == StatusEffectType.AtkDown || e.Type == StatusEffectType.Stun || e.Type == StatusEffectType.Weakness); if (GetAndClearStack(t, StatusEffectType.LifeSeed) > 0) { int healAmt = (int)(t.MaxHp * 0.1f); t.CurrentHp = Mathf.Min(t.MaxHp, t.CurrentHp + healAmt); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_hea_4"), t.HeroRef.heroName, healAmt)); } } break;
                case "SK_HEA_5": 
                    DefaultExecute(); foreach(var t in allies.Where(a => a.IsAlive())) { int seeds = GetAndClearStack(t, StatusEffectType.LifeSeed); if (seeds > 0) { AddStatusEffect(actor, t, StatusEffectType.DeathImmunity, 99, 0f, 1); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_hea_5"), t.HeroRef.heroName)); } } break;

                default: DefaultExecute(); break;
            }
        }

        private void PerformAttack(Combatant attacker, Combatant target, float powerRatio, Skill skill)
        {
            float baseDamage = attacker.GetCurrentAtk() * powerRatio;

            if (attacker.HeroRef.traitIDs.Any(t => t != null && t.Contains("PREDATOR")) && target.CurrentHp < target.MaxHp * 0.3f)
            {
                baseDamage *= 1.5f; 
                _combatLog.Add(LocalizationSystem.GetText("combat_log_predator_trigger") ?? "Nanh vuá»‘t káº» sÄƒn má»“i kÃ­ch hoáº¡t +50% SÃ¡t thÆ°Æ¡ng!");
            }

            float def = target.GetCurrentDef() * (1f - target.TempDefIgnore);
            float finalDamage = Mathf.Max(1, baseDamage - def);
            
            float archerBonus = DataManager.Instance?.GameConfig?.CombatSettings?.archerBonusCritChance ?? 0.4f;
            float critChance = attacker.GetCurrentCritChance() + (attacker.HeroRef.profession == Profession.Archer ? archerBonus : 0f);

            bool isNextCrit = attacker.ActiveEffects.Any(e => e.Type == StatusEffectType.NextAttackCrit);
            bool isCrit = isNextCrit || _rng.NextDouble() < critChance;
            if (isCrit) {
                 finalDamage *= (attacker.GetCurrentCritDamage() + attacker.TempCritDmgBonus);
                 attacker.ActiveEffects.RemoveAll(e => e.Type == StatusEffectType.NextAttackCrit);
            }

            int damageInt = Mathf.FloorToInt(finalDamage);

            // Link Huyáº¿t Máº¡ch
            if (target.LinkedAlly != null && target.LinkedAlly.IsAlive() && target.ActiveEffects.Any(e=>e.Type == StatusEffectType.DamageLink))
            {
                 int shared = damageInt / 2;
                 damageInt -= shared;
                 target.LinkedAlly.CurrentHp -= shared;
                 _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_dmg_link"), target.LinkedAlly.HeroRef.heroName, shared, target.HeroRef.heroName));
                 _eventLog.Add(new CombatEvent { EventType = CombatEventType.TakeDamage, TargetID = target.LinkedAlly.InstanceID, Value = shared });
                 CheckDeath(target.LinkedAlly);
            }

            // Shield
            var shield = target.ActiveEffects.FirstOrDefault(e => e.Type == StatusEffectType.Shield);
            if (shield != null) {
                if (shield.Value >= damageInt) { shield.Value -= damageInt; damageInt = 0; }
                else { damageInt -= (int)shield.Value; target.ActiveEffects.Remove(shield); }
            }

            target.CurrentHp -= damageInt;
            
            string atkStr = LocalizationSystem.GetText("combat_log_attack_normal") ?? "{0} giÃ¡ng Ä‘Ã²n lÃªn {1} gÃ¢y {2} sÃ¡t thÆ°Æ¡ng!";
            string log = string.Format(atkStr, attacker.HeroRef.heroName, target.HeroRef.heroName, damageInt);
            if (isCrit) log += LocalizationSystem.GetText("combat_log_attack_crit") ?? " <color=#ff0000>(ChÃ­ máº¡ng!)</color>";
            
            if (attacker.HeroRef.traitIDs.Any(t => t != null && t.Contains("LIFESTEAL")))
            {
                int healAmount = Mathf.FloorToInt(damageInt * 0.2f);
                attacker.CurrentHp = Mathf.Min(attacker.MaxHp, attacker.CurrentHp + healAmount);
                log += $" <color=#00ff00>(+{healAmount} HP HÃºt mÃ¡u)</color>";
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = attacker.InstanceID, TargetID = attacker.InstanceID, Value = healAmount });
            }

            _combatLog.Add(log);
            _eventLog.Add(new CombatEvent { EventType = CombatEventType.Attack, SourceID = attacker.InstanceID, TargetID = target.InstanceID, Value = damageInt, IsCrit = isCrit });

            if (skill != null && skill.appliedEffect != StatusEffectType.None)
            {
                if (_rng.NextDouble() < skill.effectChance) AddStatusEffect(attacker, target, skill.appliedEffect, skill.effectDuration, 0f, 1);
            }
            
            CheckDeath(target);
        }

        private void PerformHeal(Combatant healer, Combatant target, Skill skill, float ratio)
        {
            if (skill.id == "SK_HEALER_03") {
                var dbs = target.ActiveEffects.Where(e => e.Type == StatusEffectType.DefDown || e.Type == StatusEffectType.Poison || e.Type == StatusEffectType.Slow).ToList();
                if (dbs.Any()) { target.ActiveEffects.Remove(dbs.First()); _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_cleanse") ?? "{0} xÃ³a giáº£i debuff cho {1}!", healer.HeroRef.heroName, target.HeroRef.heroName)); }
            }
            float healAmount = healer.GetCurrentAtk() * ratio;
            int healInt = Mathf.FloorToInt(healAmount);
            target.CurrentHp = Mathf.Min(target.MaxHp, target.CurrentHp + healInt);
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_heal") ?? "<color=#00ff00>{0} há»“i {1} mÃ¡u cho {2}!</color>", healer.HeroRef.heroName, healInt, target.HeroRef.heroName));
            _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = healer.InstanceID, TargetID = target.InstanceID, Value = healInt });
            if (skill.appliedEffect != StatusEffectType.None) AddStatusEffect(healer, target, skill.appliedEffect, skill.effectDuration, 0f, 1);
        }
        
        private void CheckDeath(Combatant target)
        {
            if (!target.IsAlive()) 
            {
                if (target.ActiveEffects.Any(e => e.Type == StatusEffectType.DeathImmunity))
                {
                     target.ActiveEffects.RemoveAll(e => e.Type == StatusEffectType.DeathImmunity);
                     target.CurrentHp = 1;
                     _combatLog.Add($"<color=#ffff00>[KhiÃªn Báº¥t Tá»­] Ä‘Ã£ vá»¡, {target.HeroRef.heroName} thoÃ¡t cháº¿t ká»³ diá»‡u vá»›i 1 HP!</color>");
                     return;
                }

                if (target.HeroRef.traitIDs.Any(t => t != null && t.Contains("REVIVE")) && !target.HasRevivedOnce)
                {
                    target.HasRevivedOnce = true;
                    int rHp = Mathf.FloorToInt(target.MaxHp * 0.3f);
                    target.CurrentHp = rHp;
                    _combatLog.Add($"<color=#ffff00>{target.HeroRef.heroName} kÃ­ch hoáº¡t TÃ¡i Sinh há»“i {rHp} HP!</color>");
                    _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = target.InstanceID, TargetID = target.InstanceID, Value = rHp });
                }
                else
                {
                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_death") ?? "<color=#ff0000>{0} Ä‘Ã£ gá»¥c ngÃ£!</color>", target.HeroRef.heroName));
                    _eventLog.Add(new CombatEvent { EventType = CombatEventType.Death, TargetID = target.InstanceID });
                }
            }
        }
        #endregion

        #region Logic Phá»¥ Trá»£
        private void ArrangeFormation(List<Combatant> team)
        {
            var warriors = team.Where(c => c.HeroRef.profession == Profession.Warrior).ToList();
            var healers = team.Where(c => c.HeroRef.profession == Profession.Healer).ToList();
            var others = team.Except(warriors).Except(healers).ToList();
            foreach (var w in warriors) w.Position = RowPosition.Front;
            foreach (var h in healers) h.Position = RowPosition.Back;
            foreach (var o in others) o.Position = RowPosition.Middle;
        }

        private List<Combatant> GetTargets(Combatant actor, TargetingType targeting, List<Combatant> allies, List<Combatant> enemies)
        {
            var livingAllies = allies.Where(c => c.IsAlive()).ToList();
            var livingEnemies = enemies.Where(c => c.IsAlive()).ToList();
            if (!livingEnemies.Any() && targeting != TargetingType.LowestHpAlly && targeting != TargetingType.Self && targeting != TargetingType.AllAllies && targeting != TargetingType.AllAlliesInRow) return new List<Combatant>();

            bool isOffensive = targeting != TargetingType.LowestHpAlly && targeting != TargetingType.Self && targeting != TargetingType.AllAllies && targeting != TargetingType.AllAlliesInRow && targeting != TargetingType.AllEnemies;
            if (isOffensive)
            {
                var taunting = livingEnemies.Where(e => e.ActiveEffects.Any(eff => eff.Type == StatusEffectType.Taunt)).ToList();
                if (taunting.Any()) return new List<Combatant> { taunting[_rng.Next(taunting.Count)] };
            }

            switch (targeting)
            {
                case TargetingType.SingleFrontEnemy:
                    var front = livingEnemies.Where(e => e.Position == RowPosition.Front).ToList();
                    if (front.Any()) return new List<Combatant> { front[_rng.Next(front.Count)] };
                    var mid = livingEnemies.Where(e => e.Position == RowPosition.Middle).ToList();
                    if (mid.Any()) return new List<Combatant> { mid[_rng.Next(mid.Count)] };
                    var back = livingEnemies.Where(e => e.Position == RowPosition.Back).ToList();
                    if (back.Any()) return new List<Combatant> { back[_rng.Next(back.Count)] };
                    return new List<Combatant>();
                case TargetingType.SingleBackEnemy:
                    var backR = livingEnemies.Where(e => e.Position == RowPosition.Back).ToList();
                    if (backR.Any()) return new List<Combatant> { backR[_rng.Next(backR.Count)] };
                    var midR = livingEnemies.Where(e => e.Position == RowPosition.Middle).ToList();
                    if (midR.Any()) return new List<Combatant> { midR[_rng.Next(midR.Count)] };
                    var frontR = livingEnemies.Where(e => e.Position == RowPosition.Front).ToList();
                    if (frontR.Any()) return new List<Combatant> { frontR[_rng.Next(frontR.Count)] };
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

        private void AddStatusEffect(Combatant caster, Combatant target, StatusEffectType type, int duration, float value, int stacks = 1)
        {
            if (type == StatusEffectType.PoisonMark || type == StatusEffectType.Aegis || type == StatusEffectType.Weakness || type == StatusEffectType.LifeSeed)
            {
                var existing = target.ActiveEffects.FirstOrDefault(e => e.Type == type);
                if (existing != null) { existing.StackCount += stacks; existing.Duration = duration; return; }
            }
            target.ActiveEffects.RemoveAll(e => e.Type == type && type != StatusEffectType.PoisonMark && type != StatusEffectType.Aegis);
            target.ActiveEffects.Add(new ActiveStatusEffect(type, duration, value, caster, stacks));
        }

        private int GetAndClearStack(Combatant target, StatusEffectType type)
        {
            var eff = target.ActiveEffects.FirstOrDefault(e => e.Type == type);
            if (eff != null)
            {
                int stacks = eff.StackCount;
                target.ActiveEffects.Remove(eff);
                return stacks;
            }
            return 0;
        }

        private void ProcessStartOfTurnEffects(Combatant combatant)
        {
            var effectsToProcess = combatant.ActiveEffects.ToList();
            foreach (var effect in effectsToProcess)
            {
                switch (effect.Type)
                {
                    case StatusEffectType.Poison:
                        int pDmg = Mathf.FloorToInt(effect.Value);
                        combatant.CurrentHp -= pDmg;
                        _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_poison_tick") ?? "{0} chá»‹u {1} sÃ¡t thÆ°Æ¡ng tá»« Äá»™c!", combatant.HeroRef.heroName, pDmg));
                        _eventLog.Add(new CombatEvent { EventType = CombatEventType.TakeDamage, TargetID = combatant.InstanceID, Value = pDmg });
                        CheckDeath(combatant); break;
                    case StatusEffectType.HealOverTime:
                        int hHeal = Mathf.FloorToInt(effect.Value);
                        combatant.CurrentHp = Mathf.Min(combatant.MaxHp, combatant.CurrentHp + hHeal);
                        _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_regen_tick") ?? "{0} há»“i {1} HP tá»« Há»“i mÃ¡u!", combatant.HeroRef.heroName, hHeal));
                        _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, TargetID = combatant.InstanceID, Value = hHeal });
                        break;
                }
                effect.Duration--;
            }
            combatant.ActiveEffects.RemoveAll(e => e.Duration <= 0);

            if (combatant.HeroRef.traitIDs.Any(t => t != null && t.Contains("REGEN")) && combatant.IsAlive())
            {
                int regen = Mathf.FloorToInt(combatant.MaxHp * 0.05f);
                combatant.CurrentHp = Mathf.Min(combatant.MaxHp, combatant.CurrentHp + regen);
                _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_trait_regen") ?? "{0} tá»± chá»¯a lÃ nh {1} HP!", combatant.HeroRef.heroName, regen));
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, TargetID = combatant.InstanceID, Value = regen });
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
            var empty = LocalizationSystem.GetText("combat_log_row_empty") ?? "[Trá»‘ng]";
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_front") ?? "Tiá»n Tuyáº¿n: {0}", string.IsNullOrEmpty(front) ? empty : front));
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_mid") ?? "Trung Tuyáº¿n: {0}", string.IsNullOrEmpty(middle) ? empty : middle));
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_back") ?? "Háº­u Tuyáº¿n: {0}", string.IsNullOrEmpty(back) ? empty : back));
        }
        #endregion
    }
    #endregion
}

