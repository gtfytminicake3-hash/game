# Region Forest Animation Import Checklist

## 1. CÃ¡c file cáº§n generate
Báº¡n cáº§n generate tá»•ng cá»™ng 40 file cho Heroes (8 heroes x 5 actions) vÃ  60 file cho Monsters (12 monsters x 5 actions).
Trong Ä‘Ã³ MVP lÃ  Idle, Attack, Hit, Death. Optional lÃ  Run.

## 2. Vá»‹ trÃ­ lÆ°u file
- **Hero Sprite Sheets**: Assets/Art/Heroes/Animations/2D/SpriteSheets/TestBatch
- **Monster Sprite Sheets**: Assets/Art/Monsters/Animations/2D/SpriteSheets/Region_Forest (LÆ°u Ã½: folder nÃ y cáº§n Ä‘Æ°á»£c táº¡o tay náº¿u chÆ°a cÃ³).

## 3. CÃ¡ch kiá»ƒm tra Ä‘á»§ file
- Äáº¿m Ä‘á»§ sá»‘ lÆ°á»£ng file .png trong folder (Vd: folder Hero TestBatch pháº£i cÃ³ 32 file MVP).
- Má»Ÿ thÆ° má»¥c báº±ng Explorer, search theo format *_Idle_*, *_Attack_* Ä‘á»ƒ xem cÃ³ hero/monster/action nÃ o bá»‹ sÃ³t khÃ´ng.

## 4. CÃ¡ch kiá»ƒm tra tÃªn file Ä‘Ãºng
- TÃªn pháº£i tuÃ¢n thá»§ Ä‘Ãºng HeroId (VD: HERO_FEMALE_WARRIOR_001_Attack_24f.png)
- TÃªn quÃ¡i pháº£i Ä‘Ãºng MonsterId (VD: MON_Slime_Hit_24f.png).

## 5. CÃ¡ch kiá»ƒm tra Ä‘á»§ Idle, Attack, Hit, Death
- So sÃ¡nh danh sÃ¡ch thá»±c táº¿ import vÃ o Unity vá»›i tá»‡p AnimationPromptPack_RegionForest.json
- Má»i HeroId vÃ  MonsterId Ä‘á»u pháº£i cÃ³ Ä‘á»§ 4 file káº¿t thÃºc báº±ng _Idle_24f.png, _Attack_24f.png, _Hit_24f.png, _Death_24f.png.

## 6. Khi nÃ o chuyá»ƒn sang bÆ°á»›c táº¡o Unity Animation Clip
- Sau khi Táº¤T Cáº¢ sprite sheet Ä‘Ã£ Ä‘Æ°á»£c copy vÃ o Project.
- Texture Type Ä‘á»•i thÃ nh Sprite (2D and UI).
- Sprite Mode Ä‘á»•i thÃ nh Multiple.
- ÄÃ£ Ä‘Æ°á»£c cáº¯t (Slice) thÃ nh 24 frames Ä‘á»u nhau qua Sprite Editor (Grid by Cell Size hoáº·c Cell Count: 6 Columns, 4 Rows).
- Chá»‰ sau khi slice thÃ nh cÃ´ng má»›i kÃ©o tháº£ cÃ¡c frames vÃ o scene Ä‘á»ƒ tá»± Ä‘á»™ng sinh file .anim.
