import re
import os

path = r"d:\game\legendofblood\legacyofblood_unity_mobile_portraitframe\Assets\Scripts\GameSystems\CombatSystem.cs"

with open(path, 'r', encoding='utf-8', errors='replace') as f:
    text = f.read()

reps = [
    (r'case "SK_WAR_1":.*?break;', r'case "SK_WAR_1": \n                    DefaultExecute(); AddStatusEffect(actor, actor, StatusEffectType.Aegis, 99, 0f, 1); \n                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_war_1"), actor.HeroRef.heroName)); break;'),
    (r'case "SK_WAR_2":.*?break;', r'case "SK_WAR_2": \n                    DefaultExecute(); AddStatusEffect(actor, actor, StatusEffectType.Aegis, 99, 0f, 1);\n                    foreach(var t in enemies.Where(e => e.IsAlive())) AddStatusEffect(actor, t, StatusEffectType.Taunt, 1, 0f, 1);\n                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_war_2"), actor.HeroRef.heroName)); break;'),
    (r'case "SK_WAR_3":.*?(break;)', r'case "SK_WAR_3": \n                    int aegisStack = GetAndClearStack(actor, StatusEffectType.Aegis); float shieldRatio = 0.15f + (aegisStack * 0.10f); float shieldValue = actor.MaxHp * shieldRatio; \n                    AddStatusEffect(actor, actor, StatusEffectType.Shield, 2, shieldValue, 1); \n                    _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_war_3"), actor.HeroRef.heroName, shieldValue, aegisStack)); \1'),
    (r'case "SK_WAR_4":.*?(break;)', r'case "SK_WAR_4": \n                    DefaultExecute(); if (actor.ActiveEffects.Any(e => e.Type == StatusEffectType.Shield)) { foreach(var target in targets) AddStatusEffect(actor, target, StatusEffectType.Stun, 1, 0f, 1); _combatLog.Add(LocalizationSystem.GetText("combo_war_4")); } \1'),
    
    (r'case "SK_MAG_4":.*?(break;)', r'case "SK_MAG_4": \n                    Combatant magTarget = targets.FirstOrDefault(); if (magTarget != null) { float dmg = Mathf.Max(1, actor.GetCurrentAtk() * skill.powerRatio - magTarget.GetCurrentDef()); PerformAttack(actor, magTarget, skill.powerRatio, skill); int healAmt = (int)dmg; actor.CurrentHp = minH(actor.MaxHp, actor.CurrentHp + healAmt); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_mag_4"), actor.HeroRef.heroName, healAmt)); if (magTarget.ActiveEffects.Any(e=>e.Type == StatusEffectType.PoisonMark)) { var adj = enemies.Where(e => e.IsAlive() && e != magTarget).OrderBy(e => _rng.Next()).FirstOrDefault(); if (adj != null) AddStatusEffect(actor, adj, StatusEffectType.PoisonMark, 99, 0f, 1); } } \1'),
    (r'case "SK_MAG_5":.*?(break;)', r'case "SK_MAG_5": \n                    DefaultExecute(); foreach(var e in enemies.Where(e => e.IsAlive())) { int marks = GetAndClearStack(e, StatusEffectType.PoisonMark); if(marks > 0) { float trueDmg = 0.05f * e.MaxHp * marks; e.CurrentHp -= (int)trueDmg; _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_mag_5"), e.HeroRef.heroName, trueDmg)); _eventLog.Add(new CombatEvent { EventType = CombatEventType.Attack, TargetID = e.InstanceID, Value = (int)trueDmg }); CheckDeath(e); } } \1'),
    
    (r'case "SK_ARC_2":.*?(break;)', r'case "SK_ARC_2": \n                    AddStatusEffect(actor, actor, StatusEffectType.NextAttackCrit, 1, 0f, 1); AddStatusEffect(actor, actor, StatusEffectType.AtkUp, 2, 0.3f, 1); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_arc_2"), actor.HeroRef.heroName)); \1'),
    (r'case "SK_ARC_4":.*?(break;)', r'case "SK_ARC_4": \n                    actor.SkillCooldowns = actor.SkillCooldowns.ToDictionary(k => k.Key, v => maxV(0, v.Value - 2)); actor.ActiveEffects.RemoveAll(e => e.Type == StatusEffectType.Poison || e.Type == StatusEffectType.Slow || e.Type == StatusEffectType.DefDown || e.Type == StatusEffectType.AtkDown || e.Type == StatusEffectType.Stun || e.Type == StatusEffectType.Weakness); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_arc_4"), actor.HeroRef.heroName)); \1'),
    (r'case "SK_ARC_5":.*?(break;)', r'case "SK_ARC_5": \n                    Combatant arcTarget = targets.FirstOrDefault(); if (arcTarget != null) { if (GetAndClearStack(arcTarget, StatusEffectType.Weakness) > 0) arcTarget.TempDefIgnore = 1.0f; PerformAttack(actor, arcTarget, skill.powerRatio, skill); arcTarget.TempDefIgnore = 0f; if (!arcTarget.IsAlive()) { actor.ExtraTurn = true; _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_arc_5"), actor.HeroRef.heroName)); } } \1'),
    
    (r'case "SK_HEA_2":.*?(break;)', r'case "SK_HEA_2": \n                    foreach(var t in allies.Where(a => a.IsAlive())) { AddStatusEffect(actor, t, StatusEffectType.AtkUp, 2, 0.2f, 1); if (t.ActiveEffects.Any(e=>e.Type == StatusEffectType.LifeSeed)) AddStatusEffect(actor, t, StatusEffectType.CritUp, 2, 0.15f, 1); } _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_hea_2"), actor.HeroRef.heroName)); \1'),
    (r'case "SK_HEA_3":.*?(break;)', r'case "SK_HEA_3": \n                    Combatant linkTarget = targets.FirstOrDefault(); if (linkTarget != null && linkTarget != actor) { linkTarget.LinkedAlly = actor; actor.LinkedAlly = linkTarget; AddStatusEffect(actor, linkTarget, StatusEffectType.DamageLink, 2, 0f, 1); AddStatusEffect(actor, actor, StatusEffectType.DamageLink, 2, 0f, 1); AddStatusEffect(actor, linkTarget, StatusEffectType.LifeSeed, 99, 0f, 1); AddStatusEffect(actor, actor, StatusEffectType.LifeSeed, 99, 0f, 1); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_hea_3"), actor.HeroRef.heroName, linkTarget.HeroRef.heroName)); } \1'),
    (r'case "SK_HEA_4":.*?(break;)', r'case "SK_HEA_4": \n                    foreach(var t in allies.Where(a => a.IsAlive())) { t.ActiveEffects.RemoveAll(e => e.Type == StatusEffectType.Poison || e.Type == StatusEffectType.Slow || e.Type == StatusEffectType.DefDown || e.Type == StatusEffectType.AtkDown || e.Type == StatusEffectType.Stun || e.Type == StatusEffectType.Weakness); if (GetAndClearStack(t, StatusEffectType.LifeSeed) > 0) { int healAmt = (int)(t.MaxHp * 0.1f); t.CurrentHp = minH(t.MaxHp, t.CurrentHp + healAmt); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_hea_4"), t.HeroRef.heroName, healAmt)); } } \1'),
    (r'case "SK_HEA_5":.*?(break;)', r'case "SK_HEA_5": \n                    DefaultExecute(); foreach(var t in allies.Where(a => a.IsAlive())) { int seeds = GetAndClearStack(t, StatusEffectType.LifeSeed); if (seeds > 0) { AddStatusEffect(actor, t, StatusEffectType.DeathImmunity, 99, 0f, 1); _combatLog.Add(string.Format(LocalizationSystem.GetText("combo_hea_5"), t.HeroRef.heroName)); } } \1'),

    # Link damage
    (r'_combatLog\.Add\(\$"\<color=#ff00ff\>.*?(shared).*?HeroRef\.heroName\}.*?\</color>"\);', r'_combatLog.Add(string.Format(LocalizationSystem.GetText("combo_dmg_link"), target.LinkedAlly.HeroRef.heroName, shared, target.HeroRef.heroName));'),
    
    # Immortal Shield 
    (r'_combatLog\.Add\(\$"\<color=#ffff00\>\[Khiên.*?1 HP!\</color>"\);', r'_combatLog.Add(string.Format(LocalizationSystem.GetText("combo_immortal_pop"), target.HeroRef.heroName));')
]

for pat, repl in reps:
    text = re.sub(pat, repl, text, flags=re.DOTALL)

# Fix minH and maxV mappings because I shortened Mathf.Min and Mathf.Max to not break grouping blocks
text = text.replace("minH(", "Mathf.Min(")
text = text.replace("maxV(", "Mathf.Max(")

# Write the fixed ASCII code back, safe to use native encoding
with open(path, 'w', encoding='utf-8') as f:
    f.write(text)

print("Replaced all hardcoded text with LocalizationSystem.GetText keys.")
