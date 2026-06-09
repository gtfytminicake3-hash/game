# Region Forest Animation Targets

## 1. Data Source
**TÃ¬nh tráº¡ng**: NOT FOUND (KhÃ´ng cÃ³ object hay file data tÃªn lÃ  Region_Forest).
Region_Forest hiá»‡n táº¡i chá»‰ lÃ  má»™t string code cá»©ng trong WorldMapFixedGenerator.cs. 
**Äá» xuáº¥t**: Project Ä‘ang sá»­ dá»¥ng POIMonsterConfig.asset Ä‘á»ƒ lÆ°u chung táº¥t cáº£ monster. Ta sáº½ dÃ¹ng toÃ n bá»™ danh sÃ¡ch 12 monster hiá»‡n cÃ³ (Goblin, Slime, Orc, Troll, Dragon...) lÃ m Ä‘áº¡i diá»‡n cho "Region Forest pool".

## 2. Danh sÃ¡ch Monster (Region Forest Pool)
1. MON_Slime (Slime)
2. MON_Goblin (Goblin)
3. MON_Orc_Warrior (Orc)
4. MON_Orc_Shaman (Orc)
5. MON_Troll (Troll)
6. MON_Golem (Golem)
7. MON_Fire_Elemental (Elemental)
8. MON_Dragon_Whelp (Dragon)
9. MON_Elder_Dragon (Boss)
10. BOSS_ALPHA_WOLF (Boss)
11. BOSS_ANGRY_TURKEY (Boss)
12. BOSS_RABBIT_KING (Boss)

## 3. Danh sÃ¡ch Hero Test (8 Heroes)
HERO_MALE_ARCHER_001, HERO_MALE_HEALER_001, HERO_MALE_MAGE_001, HERO_MALE_WARRIOR_001, HERO_FEMALE_ARCHER_001, HERO_FEMALE_HEALER_001, HERO_FEMALE_MAGE_001, HERO_FEMALE_WARRIOR_001

## 4. Animation MVP
- Idle
- Attack
- Hit
- Death

## 5. Animation Optional
- Run (Movement)

## 6. Tá»•ng sá»‘ lÆ°á»£ng Sprite Sheet
- Hero MVP: 8 x 4 = 32.
- Monster MVP: 12 x 4 = 48.
- Tá»•ng MVP: 80 files.
- Tá»•ng tÃ­nh cáº£ Optional (Run): 100 files.

## 7. Thá»© tá»± Æ°u tiÃªn Generate báº±ng Flow
1. 8 Heroes (Idle, Attack, Hit, Death) -> Æ¯u tiÃªn sá»‘ 1 Ä‘á»ƒ test logic combat.
2. 3 Common Monsters (Slime, Goblin, Orc_Warrior) -> Test Ä‘á»‘i khÃ¡ng.
3. 1 Boss (BOSS_ALPHA_WOLF) -> Test boss node.
4. CÃ¡c quÃ¡i cÃ²n láº¡i.
5. Action Run (Optional).

## 8. Rá»§i ro cÃ²n láº¡i
- Thiáº¿u model tÄ©nh cho Monster: QuÃ¡i hiá»‡n táº¡i chÆ°a gen Model tÄ©nh lÃ m tham chiáº¿u, khi Flow tá»± phÃ³ng tÃ¡c sinh animation cÃ³ thá»ƒ khÃ´ng Ä‘á»“ng nháº¥t (Inconsistent). Báº¡n Cáº¦N sinh Concept hoáº·c Model Full Body tÄ©nh cho Monster trÆ°á»›c Ä‘á»ƒ lÃ m Input Reference cho Animation, giá»‘ng nhÆ° Hero Ä‘Ã£ cÃ³ Avatar.
