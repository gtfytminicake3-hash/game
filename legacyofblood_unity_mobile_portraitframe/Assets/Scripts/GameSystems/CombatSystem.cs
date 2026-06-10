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
    public class EnemyOutcome
    {
        public string enemyInstanceId;
        public string enemyTypeId;
        public float hpAfterBattle;
        public float maxHp;
        public bool isDead;
    }

    [System.Serializable]
    public struct CombatReplayUnitSnapshot
    {
        public string unitId;
        public string displayName;
        public string spriteId;
        public float maxHp;
        public float startHp;
        public int slotIndex;
        public bool isAlly;
        public int combatPower;
        public int level;
        public int heroGender;
        public int avatarIndex;
    }

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
        public List<EnemyOutcome> EnemyOutcomes;
        public List<CombatantPosition> InitialPositions;
        
        public List<CombatReplayUnitSnapshot> AllyReplayUnits;
        public List<CombatReplayUnitSnapshot> EnemyReplayUnits;
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

    #region 1. CÃ C Lá»šP Dá»® LIá»†U PHá»¤ TRá»¢
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

    #region 2. Lá»šP COMBATANT - TRÃ I TIM Cá»¦A Há»† THá» NG
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
        
        // --- NEW: Trait Caching ---
        public List<TraitEffect> CachedCombatTraits { get; private set; }
        public bool IsDefending { get; set; } = false;
        public float AtkMultiplier { get; set; } = 1.0f;
        
        public int ReviveCount { get; set; } = 0;
        
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
            CachedCombatTraits = new List<TraitEffect>();

            if (DataManager.Instance != null && HeroRef.traitIDs != null)
            {
                foreach (var tid in HeroRef.traitIDs)
                {
                    var trait = DataManager.Instance.GetTraitByID(tid);
                    if (trait != null && trait.combatEffects != null)
                    {
                        CachedCombatTraits.AddRange(trait.combatEffects);
                    }
                }
            }
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
            float buffBonus = ActiveEffects.Where(e => e.Type == StatusEffectType.DefUp).Sum(e => e.Value);
            float debuffMalus = ActiveEffects.Where(e => e.Type == StatusEffectType.DefDown).Sum(e => Math.Abs(e.Value));
            return Mathf.Max(0, def * (1f + buffBonus) - debuffMalus);
        }
        
        public float GetCurrentSpd() 
        {
            float spd = HeroRef.GetFinalStats().spd;
            float buffBonus = ActiveEffects.Where(e => e.Type == StatusEffectType.SpdUp).Sum(e => e.Value);
            float debuffMalus = ActiveEffects.Where(e => e.Type == StatusEffectType.Slow).Sum(e => Math.Abs(e.Value));
            return Mathf.Max(0, spd * (1f + buffBonus) - debuffMalus);
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

    #region 3. Há»† THá» NG CHIáº¾N Ä áº¤U CHÃ NH
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

            // Process Aura Traits (Data-Driven)
            TraitProcessor.ProcessAuraTraits(this, _playerTeam, _enemyTeam);
            TraitProcessor.ProcessAuraTraits(this, _enemyTeam, _playerTeam);

            LogFormation(_playerTeam, LocalizationSystem.GetText("combat_log_player_team") ?? "Đội Hình Người Chơi");
            LogFormation(_enemyTeam, LocalizationSystem.GetText("combat_log_enemy_team") ?? "Đội Hình Kẻ Địch");

            // --- BATTLE START HOOK ---
            foreach (var c in _playerTeam.Where(x => x.IsAlive())) TraitProcessor.ProcessBattleStart(this, c, _playerTeam, _enemyTeam);
            foreach (var c in _enemyTeam.Where(x => x.IsAlive())) TraitProcessor.ProcessBattleStart(this, c, _enemyTeam, _playerTeam);

            List<CombatReplayUnitSnapshot> allySnapshots = new List<CombatReplayUnitSnapshot>();
            foreach (var c in _playerTeam)
            {
                int sIdx = initialPos.Find(p => p.InstanceID == c.InstanceID).SlotIndex;
                allySnapshots.Add(new CombatReplayUnitSnapshot
                {
                    unitId = c.InstanceID,
                    displayName = c.HeroRef.heroName,
                    spriteId = c.HeroRef.id,
                    maxHp = c.MaxHp,
                    startHp = c.CurrentHp,
                    slotIndex = sIdx,
                    isAlly = true,
                    combatPower = c.HeroRef.GetCombatPower(),
                    level = c.HeroRef.level,
                    heroGender = (int)c.HeroRef.gender,
                    avatarIndex = c.HeroRef.avatarIndex
                });
            }

            List<CombatReplayUnitSnapshot> enemySnapshots = new List<CombatReplayUnitSnapshot>();
            foreach (var c in _enemyTeam)
            {
                int sIdx = initialPos.Find(p => p.InstanceID == c.InstanceID).SlotIndex;
                enemySnapshots.Add(new CombatReplayUnitSnapshot
                {
                    unitId = c.InstanceID,
                    displayName = c.HeroRef.heroName,
                    spriteId = c.HeroRef.id,
                    maxHp = c.MaxHp,
                    startHp = c.CurrentHp,
                    slotIndex = sIdx,
                    isAlly = false,
                    combatPower = c.HeroRef.GetCombatPower(),
                    level = c.HeroRef.level,
                    heroGender = (int)c.HeroRef.gender,
                    avatarIndex = c.HeroRef.avatarIndex
                });
            }

            UnityEngine.Debug.Log($"[CombatSystem] Replay snapshot created. Ally count: {allySnapshots.Count}, Enemy count: {enemySnapshots.Count}");

            int turn = 1;
            int maxTurns = 50; 
            
            while (IsTeamAlive(_playerTeam) && (_isHealingChallenge ? !IsTeamFullyHealed(_enemyTeam) : IsTeamAlive(_enemyTeam)))
            {
                _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_turn_header") ?? "--- Lượt {0} ---", turn));
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.TurnStart, Value = turn });
                var turnOrder = _playerTeam.Concat(_enemyTeam).Where(c => c.IsAlive()).OrderByDescending(c => c.GetCurrentSpd()).ToList();

                foreach (var combatant in turnOrder)
                {
                    if (!combatant.IsAlive()) continue;
                    
                    // --- TURN START HOOK ---
                    var allies = combatant.IsPlayerTeam ? _playerTeam : _enemyTeam;
                    var enemies = combatant.IsPlayerTeam ? _enemyTeam : _playerTeam;
                    TraitProcessor.ProcessTurnStart(this, combatant, allies, enemies);
                    
                    ProcessStartOfTurnEffects(combatant); // Status effects DMG processing & duration decrement
                    if (!combatant.IsAlive()) continue;

                    if (combatant.ActiveEffects.Any(e => e.Type == StatusEffectType.Stun))
                    {
                        string cName = combatant.HeroRef.heroName;
                        _combatLog.Add($"<color=#aaaaaa>{cName} bị khóa hành động (Mất lượt)!</color>");
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
                if (turn > maxTurns) { _combatLog.Add(LocalizationSystem.GetText("combat_log_draw_timeout") ?? "Trận đấu hòa do hết lượt."); break; }
            }

            foreach(var combatant in _playerTeam.Concat(_enemyTeam))
            {
                combatant.HeroRef.currentHp = combatant.CurrentHp;
            }

            bool playerWon = IsTeamAlive(_playerTeam) && (_isHealingChallenge ? IsTeamFullyHealed(_enemyTeam) : !IsTeamAlive(_enemyTeam));
            _combatLog.Add(playerWon ? (LocalizationSystem.GetText("combat_log_victory") ?? "Chiến thắng!") : (LocalizationSystem.GetText("combat_log_defeat") ?? "Thất bại!"));

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
                EnemyOutcomes = _enemyTeam.Select(c => new EnemyOutcome
                {
                    enemyInstanceId = c.InstanceID,
                    enemyTypeId = c.HeroRef.id,
                    hpAfterBattle = c.CurrentHp,
                    maxHp = c.MaxHp,
                    isDead = !c.IsAlive()
                }).ToList(),
                InitialPositions = initialPos,
                AllyReplayUnits = allySnapshots,
                EnemyReplayUnits = enemySnapshots
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

        #region Logic Cốt Lõi của Trận Đấu
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
                
                string sCastStr = LocalizationSystem.GetText("combat_log_skill_cast") ?? "{0} thi triển thuật {1}!";
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

        private void PerformAttack(Combatant attacker, Combatant target, float powerRatio, Skill skill, bool isTraitDamage = false)
        {
            float baseDamage = attacker.GetCurrentAtk() * powerRatio;

            if (attacker.HeroRef.traitIDs.Any(t => t != null && t.Contains("PREDATOR")) && target.CurrentHp < target.MaxHp * 0.3f)
            {
                baseDamage *= 1.5f; 
                _combatLog.Add(LocalizationSystem.GetText("combat_log_predator_trigger") ?? "Nanh vuốt kẻ săn mồi kích hoạt +50% Sát thương!");
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

            // Link Huyết Mạch
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
            
            string atkStr = LocalizationSystem.GetText("combat_log_attack_normal") ?? "{0} giáng đòn lên {1} gây {2} sát thương!";
            string log = string.Format(atkStr, attacker.HeroRef.heroName, target.HeroRef.heroName, damageInt);
            if (isCrit) log += LocalizationSystem.GetText("combat_log_attack_crit") ?? " <color=#ff0000>(Chí mạng!)</color>";
            
            if (attacker.HeroRef.traitIDs.Any(t => t != null && t.Contains("LIFESTEAL")))
            {
                int healAmount = Mathf.FloorToInt(damageInt * 0.2f);
                attacker.CurrentHp = Mathf.Min(attacker.MaxHp, attacker.CurrentHp + healAmount);
                log += $" <color=#00ff00>(+{healAmount} HP Hút máu)</color>";
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = attacker.InstanceID, TargetID = attacker.InstanceID, Value = healAmount });
            }

            _combatLog.Add(log);
            _eventLog.Add(new CombatEvent { EventType = CombatEventType.Attack, SourceID = attacker.InstanceID, TargetID = target.InstanceID, Value = damageInt, IsCrit = isCrit });

            // --- DAMAGE DEALT / RECEIVED HOOK ---
            TraitProcessor.ProcessDamageDealt(this, attacker, target, damageInt, isCrit, isTraitDamage);
            TraitProcessor.ProcessDamageReceived(this, target, attacker, damageInt, isTraitDamage);

            if (skill != null && skill.appliedEffect != StatusEffectType.None)
            {
                if (_rng.NextDouble() < skill.effectChance) AddStatusEffect(attacker, target, skill.appliedEffect, skill.effectDuration, 0f, 1);
            }
            
            CheckDeath(target, attacker);
        }

        private void PerformHeal(Combatant healer, Combatant target, Skill skill, float ratio)
        {
            if (skill.id == "SK_HEALER_03") {
                var dbs = target.ActiveEffects.Where(e => e.Type == StatusEffectType.DefDown || e.Type == StatusEffectType.Poison || e.Type == StatusEffectType.Slow).ToList();
                if (dbs.Any()) { target.ActiveEffects.Remove(dbs.First()); _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_cleanse") ?? "{0} xóa giải debuff cho {1}!", healer.HeroRef.heroName, target.HeroRef.heroName)); }
            }
            float healAmount = healer.GetCurrentAtk() * ratio;
            int healInt = Mathf.FloorToInt(healAmount);
            target.CurrentHp = Mathf.Min(target.MaxHp, target.CurrentHp + healInt);
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_heal") ?? "<color=#00ff00>{0} hồi {1} máu cho {2}!</color>", healer.HeroRef.heroName, healInt, target.HeroRef.heroName));
            _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = healer.InstanceID, TargetID = target.InstanceID, Value = healInt });
            if (skill.appliedEffect != StatusEffectType.None) AddStatusEffect(healer, target, skill.appliedEffect, skill.effectDuration, 0f, 1);
        }
        
        private void CheckDeath(Combatant target, Combatant attacker = null)
        {
            if (!target.IsAlive()) 
            {
                var victimEnemies = target.IsPlayerTeam ? _enemyTeam : _playerTeam;

                // --- DEATH HOOK (Revive check inside) ---
                bool isRevived = TraitProcessor.ProcessDeath(this, target, victimEnemies);
                if (isRevived) return;

                if (target.ActiveEffects.Any(e => e.Type == StatusEffectType.DeathImmunity))
                {
                     target.ActiveEffects.RemoveAll(e => e.Type == StatusEffectType.DeathImmunity);
                     target.CurrentHp = 1;
                     _combatLog.Add($"<color=#ffff00>[Khiên Bất Tử] Bảo vệ {target.HeroRef.heroName} thoát chết kỳ diệu với 1 HP!</color>");
                     return;
                }

                _combatLog.Add(LocalizationSystem.GetText("combat_log_die_format")?.Replace("{0}", target.HeroRef.heroName) ?? $"{target.HeroRef.heroName} đã gục ngã!");
                _eventLog.Add(new CombatEvent { EventType = CombatEventType.Death, TargetID = target.InstanceID });
                
                // --- KILL HOOK ---
                if (attacker != null) {
                    var attackerAllies = attacker.IsPlayerTeam ? _playerTeam : _enemyTeam;
                    TraitProcessor.ProcessKill(this, attacker, target, attackerAllies);
                }
            }
        }
        #endregion

        #region Helpers cho TraitProcessor (PUBLIC / INTERNAL)
        public void LogMessage(string msg) { _combatLog.Add(msg); }
        public void LogEvent(CombatEvent ev) { _eventLog.Add(ev); }
        
        public void AddStatusEffect(Combatant caster, Combatant target, StatusEffectType type, int duration, float value, int stacks = 1)
        {
            if (type == StatusEffectType.PoisonMark || type == StatusEffectType.Aegis || type == StatusEffectType.Weakness || type == StatusEffectType.LifeSeed)
            {
                var existing = target.ActiveEffects.FirstOrDefault(e => e.Type == type);
                if (existing != null) { existing.StackCount += stacks; existing.Duration = duration; return; }
            }
            target.ActiveEffects.RemoveAll(e => e.Type == type && type != StatusEffectType.PoisonMark && type != StatusEffectType.Aegis);
            target.ActiveEffects.Add(new ActiveStatusEffect(type, duration, value, caster, stacks));
        }

        public void HealTarget(Combatant healer, Combatant target, int amount)
        {
            target.CurrentHp = Mathf.Min(target.MaxHp, target.CurrentHp + amount);
            _eventLog.Add(new CombatEvent { EventType = CombatEventType.Heal, SourceID = healer.InstanceID, TargetID = target.InstanceID, Value = amount });
        }

        public void DealTraitDamage(Combatant target, int amount)
        {
            target.CurrentHp -= amount;
            _combatLog.Add($"<color=#ff8800>[Trait Damage] {target.HeroRef.heroName} gánh chịu {amount} sát thương!</color>");
            _eventLog.Add(new CombatEvent { EventType = CombatEventType.TakeDamage, TargetID = target.InstanceID, Value = amount });
            CheckDeath(target, null);
        }
        #endregion

        #region Logic Phụ Trợ
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
                        _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_poison_tick") ?? "{0} chịu {1} sát thương từ Độc!", combatant.HeroRef.heroName, pDmg));
                        _eventLog.Add(new CombatEvent { EventType = CombatEventType.TakeDamage, TargetID = combatant.InstanceID, Value = pDmg });
                        CheckDeath(combatant); break;
                    case StatusEffectType.HealOverTime:
                        int hHeal = Mathf.FloorToInt(effect.Value);
                        combatant.CurrentHp = Mathf.Min(combatant.MaxHp, combatant.CurrentHp + hHeal);
                        _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_regen_tick") ?? "{0} hồi {1} HP từ Hồi máu!", combatant.HeroRef.heroName, hHeal));
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
                _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_trait_regen") ?? "{0} tự chữa lành {1} HP!", combatant.HeroRef.heroName, regen));
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
            var empty = LocalizationSystem.GetText("combat_log_row_empty") ?? "[Trống]";
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_front") ?? "Tiền Tuyến: {0}", string.IsNullOrEmpty(front) ? empty : front));
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_mid") ?? "Trung Tuyến: {0}", string.IsNullOrEmpty(middle) ? empty : middle));
            _combatLog.Add(string.Format(LocalizationSystem.GetText("combat_log_row_back") ?? "Hậu Tuyến: {0}", string.IsNullOrEmpty(back) ? empty : back));
        }
        #endregion
    }
    #endregion

    public static class TraitProcessor
    {
        public static void ProcessBattleStart(CombatSystem sys, Combatant combatant, List<Combatant> allies, List<Combatant> enemies)
        {
            if (combatant.CachedCombatTraits == null) return;
            foreach (var effect in combatant.CachedCombatTraits)
            {
                if (UnityEngine.Random.value > effect.procChance) continue;
                
                if (effect.type == TraitEffectType.ON_BATTLE_START_BUFF_TEAM)
                {
                    foreach (var ally in allies.Where(a => a.IsAlive())) {
                        sys.AddStatusEffect(combatant, ally, effect.buffType, effect.duration > 0 ? effect.duration : 99, effect.stackValue, effect.maxStack > 0 ? effect.maxStack : 1);
                    }
                    sys.LogMessage($"<color=#00ffff>[Trait] {combatant.HeroRef.heroName} kích hoạt buff đầu trận cho toàn đội!</color>");
                }
                else if (effect.type == TraitEffectType.ON_BATTLE_START_DEBUFF_ENEMY)
                {
                    sys.LogMessage($"<color=#ff8800>[Trait] {combatant.HeroRef.heroName} giáng debuff đầu trận lên toàn đội địch!</color>");
                    foreach (var enemy in enemies.Where(e => e.IsAlive())) {
                        sys.AddStatusEffect(combatant, enemy, effect.buffType, effect.duration > 0 ? effect.duration : 99, effect.stackValue, effect.maxStack > 0 ? effect.maxStack : 1);
                    }
                }
                else if (effect.type == TraitEffectType.ON_BATTLE_START_SHIELD)
                {
                    float shieldVal = combatant.MaxHp * effect.maxHpRatio;
                    sys.AddStatusEffect(combatant, combatant, StatusEffectType.Shield, effect.duration > 0 ? effect.duration : 2, shieldVal, 1);
                    sys.LogMessage($"<color=#00ffff>[Trait] {combatant.HeroRef.heroName} tự tạo khiên bảo vệ!</color>");
                }
            }
        }

        public static void ProcessTurnStart(CombatSystem sys, Combatant combatant, List<Combatant> allies, List<Combatant> enemies)
        {
            if (combatant.CachedCombatTraits == null) return;
            foreach (var effect in combatant.CachedCombatTraits)
            {
                if (UnityEngine.Random.value > effect.procChance) continue;

                if (effect.type == TraitEffectType.ON_TURN_START_HEAL_SELF)
                {
                    int heal = Mathf.FloorToInt(combatant.MaxHp * effect.maxHpRatio);
                    sys.HealTarget(combatant, combatant, heal);
                    sys.LogMessage($"<color=#00ff00>[Trait] {combatant.HeroRef.heroName} tự hồi {heal} HP đầu lượt!</color>");
                }
                else if (effect.type == TraitEffectType.ON_TURN_START_STACK_ATK)
                {
                    sys.AddStatusEffect(combatant, combatant, StatusEffectType.AtkUp, effect.duration > 0 ? effect.duration : 99, effect.stackValue, 1);
                    sys.LogMessage($"<color=#ffff00>[Trait] {combatant.HeroRef.heroName} tăng sức mạnh đầu lượt!</color>");
                }
            }
        }

        public static void ProcessDamageDealt(CombatSystem sys, Combatant attacker, Combatant target, int damage, bool isCrit, bool isTraitDamage)
        {
            if (isTraitDamage || attacker.CachedCombatTraits == null) return;
            foreach (var effect in attacker.CachedCombatTraits)
            {
                if (UnityEngine.Random.value > effect.procChance) continue;

                if (effect.type == TraitEffectType.ON_DAMAGE_DEALT_LIFESTEAL)
                {
                    int heal = Mathf.FloorToInt(damage * (effect.maxHpRatio > 0 ? effect.maxHpRatio : effect.stackValue)); 
                    sys.HealTarget(attacker, attacker, heal);
                }
                else if (isCrit && effect.type == TraitEffectType.ON_CRIT_PROC_STUN)
                {
                    sys.AddStatusEffect(attacker, target, StatusEffectType.Stun, effect.duration > 0 ? effect.duration : 1, 0f, 1);
                    sys.LogMessage($"<color=#ff8800>[Trait] Đòn chí mạng của {attacker.HeroRef.heroName} gây choáng {target.HeroRef.heroName}!</color>");
                }
            }
        }

        public static void ProcessDamageReceived(CombatSystem sys, Combatant target, Combatant attacker, int damage, bool isTraitDamage)
        {
            if (isTraitDamage || target.CachedCombatTraits == null || !target.IsAlive() || attacker == null || !attacker.IsAlive()) return;
            foreach (var effect in target.CachedCombatTraits)
            {
                if (UnityEngine.Random.value > effect.procChance) continue;

                if (effect.type == TraitEffectType.ON_HIT_REFLECT)
                {
                    int reflectDmg = Mathf.FloorToInt(target.GetCurrentAtk() * effect.atkRatio);
                    if (reflectDmg > 0)
                    {
                        sys.LogMessage($"<color=#ff5555>[Trait] {target.HeroRef.heroName} phản đòn gây {reflectDmg} sát thương lên {attacker.HeroRef.heroName}!</color>");
                        sys.DealTraitDamage(attacker, reflectDmg);
                    }
                }
                else if (effect.type == TraitEffectType.ON_HIT_COUNTER)
                {
                    int counterDmg = Mathf.FloorToInt(target.GetCurrentAtk() * effect.atkRatio);
                    if (counterDmg > 0)
                    {
                        sys.LogMessage($"<color=#ff5555>[Trait] {target.HeroRef.heroName} đánh trả {attacker.HeroRef.heroName} gây {counterDmg} sát thương!</color>");
                        sys.DealTraitDamage(attacker, counterDmg);
                    }
                }
            }
        }

        public static void ProcessKill(CombatSystem sys, Combatant killer, Combatant victim, List<Combatant> killerAllies)
        {
            if (killer.CachedCombatTraits == null) return;
            foreach (var effect in killer.CachedCombatTraits)
            {
                if (UnityEngine.Random.value > effect.procChance) continue;

                if (effect.type == TraitEffectType.ON_KILL_HEAL_TEAM)
                {
                    foreach (var ally in killerAllies.Where(a => a.IsAlive())) {
                        int heal = Mathf.FloorToInt(ally.MaxHp * effect.maxHpRatio);
                        sys.HealTarget(killer, ally, heal);
                    }
                    sys.LogMessage($"<color=#00ff00>[Trait] {killer.HeroRef.heroName} hạ gục kẻ địch, hồi máu cho toàn đội!</color>");
                }
                else if (effect.type == TraitEffectType.ON_KILL_STACK_STAT)
                {
                    sys.AddStatusEffect(killer, killer, effect.buffType, effect.duration > 0 ? effect.duration : 99, effect.stackValue, effect.maxStack > 0 ? effect.maxStack : 1);
                    sys.LogMessage($"<color=#ffff00>[Trait] {killer.HeroRef.heroName} nhận thêm sức mạnh sau khi hạ gục kẻ địch!</color>");
                }
            }
        }

        public static bool ProcessDeath(CombatSystem sys, Combatant victim, List<Combatant> victimEnemies)
        {
            if (victim.CachedCombatTraits == null) return false;
            
            bool revived = false;
            foreach (var effect in victim.CachedCombatTraits)
            {
                if (UnityEngine.Random.value > effect.procChance) continue;

                if (effect.type == TraitEffectType.ON_DEATH_REVIVE_CHANCE)
                {
                    int maxRevives = effect.maxStack > 0 ? effect.maxStack : 1;
                    if (victim.ReviveCount < maxRevives)
                    {
                        victim.ReviveCount++;
                        int heal = Mathf.FloorToInt(victim.MaxHp * effect.reviveHpPercent);
                        if (heal <= 0) heal = 1;
                        victim.CurrentHp = heal;
                        revived = true;
                        sys.LogMessage($"<color=#ffff00>[Trait] {victim.HeroRef.heroName} sống dậy từ cái chết (Lần {victim.ReviveCount}/{maxRevives}) với {heal} HP!</color>");
                        sys.LogEvent(new CombatEvent { EventType = CombatEventType.Heal, SourceID = victim.InstanceID, TargetID = victim.InstanceID, Value = heal });
                    }
                }
                else if (effect.type == TraitEffectType.ON_DEATH_EXPLODE && !revived)
                {
                    sys.LogMessage($"<color=#ff0000>[Trait] {victim.HeroRef.heroName} tử nạn và phát nổ!</color>");
                    foreach (var enemy in victimEnemies.Where(e => e.IsAlive())) {
                        int dmg = Mathf.FloorToInt(victim.GetCurrentAtk() * effect.atkRatio);
                        if (dmg <= 0 && effect.maxHpRatio > 0) dmg = Mathf.FloorToInt(victim.MaxHp * effect.maxHpRatio);
                        if (dmg > 0) sys.DealTraitDamage(enemy, dmg);
                    }
                }
            }
            return revived;
        }

        public static void ProcessAuraTraits(CombatSystem sys, List<Combatant> team, List<Combatant> enemyTeam)
        {
            foreach (var buffer in team.Where(c => c.CachedCombatTraits != null && c.IsAlive()))
            {
                foreach (var effect in buffer.CachedCombatTraits.Where(e => e.type == TraitEffectType.AURA))
                {
                    if (UnityEngine.Random.value > effect.procChance) continue;

                    if (effect.stackValue > 0)
                    {
                        sys.LogMessage($"<color=#00ffff>[Trait] {buffer.HeroRef.heroName} kích hoạt Hào quang Aura cho đội!</color>");
                        foreach (var ally in team.Where(a => a.IsAlive()))
                        {
                            if (buffer.HeroRef.traitIDs != null)
                            {
                                if (buffer.HeroRef.traitIDs.Contains("TNK_05")) sys.AddStatusEffect(buffer, ally, StatusEffectType.DefUp, 99, effect.stackValue, 1);
                                else if (buffer.HeroRef.traitIDs.Contains("HEA_05")) sys.AddStatusEffect(buffer, ally, StatusEffectType.SpdUp, 99, effect.stackValue, 1);
                                else if (buffer.HeroRef.traitIDs.Contains("ROY_03")) 
                                {
                                    sys.AddStatusEffect(buffer, ally, StatusEffectType.AtkUp, 99, effect.stackValue, 1);
                                    sys.AddStatusEffect(buffer, ally, StatusEffectType.DefUp, 99, effect.stackValue, 1);
                                    sys.AddStatusEffect(buffer, ally, StatusEffectType.SpdUp, 99, effect.stackValue, 1);
                                }
                                else sys.AddStatusEffect(buffer, ally, StatusEffectType.AtkUp, 99, effect.stackValue, 1); // fallback
                                sys.LogEvent(new CombatEvent { EventType = CombatEventType.StatusEffect, Message = "Aura", SourceID = buffer.InstanceID.ToString(), TargetID = ally.InstanceID.ToString() });
                            }
                        }
                    }
                }
            }
        }
    }
}


