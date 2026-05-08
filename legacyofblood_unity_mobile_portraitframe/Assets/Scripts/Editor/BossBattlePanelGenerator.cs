#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LegendOfBlood.Editor
{
    public class BossBattlePanelGenerator
    {
        [MenuItem("UI Tools/Fix Battlefield Tab Link")]
        public static void FixBattlefieldTabLink()
        {
            var allNavs = Resources.FindObjectsOfTypeAll<UIPanelNavButton>();
            int count = 0;
            foreach (var nav in allNavs)
            {
                if (nav.gameObject.name.Contains("Tab_Battlefield") && nav.gameObject.scene.IsValid())
                {
                    nav.targetPanel = UIPanelType.BossBattle;
                    UnityEditor.EditorUtility.SetDirty(nav.gameObject);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(nav.gameObject.scene);
                    Debug.Log($"[FixLink] Đã sửa nút thành công tại: {nav.gameObject.transform.parent.name}/{nav.gameObject.name}");
                    count++;
                }
            }
            Debug.Log($"[FixLink] Đã sửa {count} nút Tab_Battlefield trong Scene để trỏ tới BossBattle!");
        }

        [MenuItem("UI Tools/Generate Boss Battle Panel")]
        public static void GenerateBossBattlePanel()
        {
            // 1. Setup Canvas
            GameObject canvasObj = new GameObject("Canvas_BossBattle");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 2. Setup Root
            GameObject rootObj = CreateUIElement("Panel_BossBattle", canvasObj.transform);
            SetRect(rootObj, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero); // Stretch full
            SetStretchOffsets(rootObj, 0, 0, 0, 0); // L, R, T, B

            // Gắn Runtime Controller
            BossBattlePanel controller = rootObj.AddComponent<BossBattlePanel>();
            
            // BackgroundScene Placeholder
            GameObject bgObj = CreateUIElement("BackgroundScene", rootObj.transform);
            SetRect(bgObj, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetStretchOffsets(bgObj, 0, 0, 0, 0);
            bgObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark tint

            // 5.1 TopLobbyBanner
            GameObject topBanner = CreateUIElement("TopLobbyBanner", rootObj.transform);
            SetRect(topBanner, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(1080, 84));
            SetStretchOffsets(topBanner, 0, 0, 0, 84); // Left 0, Right 0, Top 0, Height 84
            Image imgBanner = topBanner.AddComponent<Image>();
            imgBanner.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            CreateTMPText(topBanner.transform, "Illuminated Manuscript Main Lobby...", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(0, -42), new Vector2(1000, 84), 24, Color.white);

            // 5.2 BossBattleTopBar
            GameObject topBar = CreateUIElement("BossBattleTopBar", rootObj.transform);
            SetRect(topBar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-4, -83 - (109/2f)), new Vector2(846, 109)); // user's pos y was -83 meaning top pos, setting anchored exactly to match SizeDelta layout

            // 5.3 BossBattleLabel
            GameObject bossBattleLabel = CreateUIElement("BossBattleLabel", topBar.transform);
            SetRect(bossBattleLabel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -18f), new Vector2(340, 36)); // PosY -84 absolute? Local Y from topBar center
            CreateTMPText(bossBattleLabel.transform, "BOSS BATTLE", new Vector2(0,0), new Vector2(1,1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 30, Color.white);

            // 5.4 Resource blocks inside TopBar
            GameObject goldBlock = CreateUIElement("GoldBlock", topBar.transform);
            SetRect(goldBlock, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(140.5f, 0), new Vector2(281, 88)); // Adjusted mapped anchors
            goldBlock.AddComponent<Image>().color = new Color(1, 0.9f, 0, 0.5f);

            GameObject trophyBlock = CreateUIElement("TrophyBlock", topBar.transform);
            SetRect(trophyBlock, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(225, 88));
            trophyBlock.AddComponent<Image>().color = new Color(0.8f, 0.8f, 1, 0.5f);

            GameObject gemBlock = CreateUIElement("GemBlock", topBar.transform);
            SetRect(gemBlock, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-101.5f, 0), new Vector2(203, 88));
            gemBlock.AddComponent<Image>().color = new Color(1, 0, 1, 0.5f);

            // 6. Boss HP Bar
            GameObject hpBarRoot = CreateUIElement("BossHPBar", rootObj.transform);
            SetRect(hpBarRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-4, -207 - (115/2f)), new Vector2(812, 115));
            
            GameObject hpFrame = CreateUIElement("Frame", hpBarRoot.transform);
            SetRect(hpFrame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetStretchOffsets(hpFrame, 0, 0, 0, 0);
            hpFrame.AddComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 1f);

            GameObject hpFill = CreateUIElement("Fill", hpBarRoot.transform);
            SetRect(hpFill, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(0, -16 - (44/2f)), new Vector2(764, 44));
            SetStretchOffsets(hpFill, 24, 24, 16, 0); // L=24 R=24 T=16 H=44
            var imgFill = hpFill.AddComponent<Image>();
            imgFill.color = Color.red;
            controller.imgHpFill = imgFill;

            GameObject hpText = CreateUIElement("HpText", hpBarRoot.transform);
            SetRect(hpText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -43 - (31/2f)), new Vector2(316, 31));
            var tmpHp = CreateTMPText(hpText.transform, "15,000,000 / 15,000,000", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 24, Color.white);
            controller.txtHpValue = tmpHp;

            // 7. Boss Art Area
            GameObject artArea = CreateUIElement("BossArtArea", rootObj.transform);
            SetRect(artArea, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-2, 345), new Vector2(793, 592)); // Invert Y logically from top coordinate? User anchored middle center, PosY was -345 down from center. We'll set PosY = 345? User said: Y: 319 (from top 0). If screen 1920, center is 960. Y 319 -> 960-319 = 641. User said PosY = -345. Let's use user's anchoredPos.
            artArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2, 315); // Adjust for MiddleCenter visual alignment

            GameObject dragonShadow = CreateUIElement("DragonShadow", artArea.transform);
            GameObject dragonIlls = CreateUIElement("DragonIllustration", artArea.transform);
            GameObject ambientFx = CreateUIElement("AmbientFX", artArea.transform);
            
            SetRect(dragonIlls, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -5), new Vector2(665, 542));
            dragonIlls.AddComponent<Image>().color = new Color(0.4f, 0, 0.4f, 1f); // Purple dragon placeholder

            // 8. Boss Info Panel
            GameObject infoPanel = CreateUIElement("BossInfoPanel", rootObj.transform);
            SetRect(infoPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(2, 634 + (368/2f)), new Vector2(849, 368)); // Y 927 -> h 1920. 1920-927 = 993 bottom. User wrote AnchoredPos: 2, 625.

            GameObject namePlate = CreateUIElement("BossNamePlate", infoPanel.transform);
            SetRect(namePlate, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -36.5f), new Vector2(819, 73));
            namePlate.AddComponent<Image>().color = new Color(0.2f, 0.05f, 0.05f, 1f);
            var tmpName = CreateTMPText(namePlate.transform, "BOSS: ANCIENT DRACOLICH", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 36, Color.yellow);
            controller.txtBossName = tmpName;

            GameObject descPanel = CreateUIElement("BossDescriptionPanel", infoPanel.transform);
            SetRect(descPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-2, 140.5f), new Vector2(803, 281));
            descPanel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            GameObject diffObj = CreateUIElement("DifficultyText", descPanel.transform);
            SetRect(diffObj, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(48 + 454/2f, -30 - 42/2f), new Vector2(454, 42));
            var tmpDiff = CreateTMPText(diffObj.transform, "Difficulty: LUNATIC", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 28, Color.red, TMPro.TextAlignmentOptions.Left);
            controller.txtDifficulty = tmpDiff;

            GameObject descLabelObj = CreateUIElement("DescriptionLabelText", descPanel.transform);
            SetRect(descLabelObj, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(48 + 651/2f, -89 - 112/2f), new Vector2(651, 112));
            var tmpDesc = CreateTMPText(descLabelObj.transform, "This dragon breathes shadow fire. Bring holy shields.", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 24, Color.white, TMPro.TextAlignmentOptions.TopLeft);
            controller.txtDescription = tmpDesc;

            // 9. Action Buttons Area
            GameObject actionArea = CreateUIElement("ActionButtonsArea", rootObj.transform);
            SetRect(actionArea, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0f), new Vector2(0, 256 + 399/2f), new Vector2(883, 399));
            SetStretchOffsets(actionArea, 99, 98, 0, 256); // Left 99, Right 98, Bot 256, Height 399

            GameObject btnAlly = CreateUIElement("AllySupportButton", actionArea.transform);
            SetRect(btnAlly, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(94 + 197/2f, 98.5f), new Vector2(197, 197)); // user visual local offset pos
            var r1 = btnAlly.AddComponent<Image>(); r1.color = Color.blue;
            controller.btnAllySupport = btnAlly.AddComponent<Button>();

            GameObject btnRaid = CreateUIElement("BeginRaidButton", actionArea.transform);
            SetRect(btnRaid, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(2, 171.5f), new Vector2(400, 343));
            var r2 = btnRaid.AddComponent<Image>(); r2.color = Color.red;
            controller.btnBeginRaid = btnRaid.AddComponent<Button>();

            GameObject shieldFrame = CreateUIElement("ShieldFrame", btnRaid.transform);
            SetRect(shieldFrame, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 126), new Vector2(254, 252));
            shieldFrame.AddComponent<Image>().color = new Color(0.6f, 0, 0, 1f);

            GameObject raidTxtObj = CreateUIElement("BeginRaidText", btnRaid.transform);
            SetRect(raidTxtObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(196, 108));
            CreateTMPText(raidTxtObj.transform, "BEGIN\nRAID", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 36, Color.white);

            GameObject btnPrep = CreateUIElement("PrepGearButton", actionArea.transform);
            SetRect(btnPrep, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-99 - 199/2f, 103), new Vector2(199, 206));
            var r3 = btnPrep.AddComponent<Image>(); r3.color = Color.gray;
            controller.btnPrepGear = btnPrep.AddComponent<Button>();

            // 10. Bottom Nav Bar
            GameObject navBar = CreateUIElement("BottomNavBar", rootObj.transform);
            SetRect(navBar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0f), new Vector2(0, 120), new Vector2(1080, 240));
            SetStretchOffsets(navBar, 0, 0, 0, 0); // L=0, R=0, B=0, H=240
            navBar.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

            // Tab Buttons
            string[] tabs = { "Shop", "Barracks", "Lobby", "Alliance", "Battlefield" };
            float targetW = 216f;
            float totalWidth = 1080f;
            
            for (int i = 0; i < 5; i++)
            {
                GameObject tabBtn = CreateUIElement($"Tab_{tabs[i]}", navBar.transform);
                float posX = (i * targetW) + (targetW / 2f) - (totalWidth / 2f);
                float h = (i == 2) ? 245f : 196f; // Lobby higher
                float y = (i == 2) ? 122.5f : 98f;
                SetRect(tabBtn, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(posX, y), new Vector2(targetW - 10, h));
                var btnImg = tabBtn.AddComponent<Image>();
                btnImg.color = (i == 4) ? new Color(0.6f, 0.2f, 0.2f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f); // Highlight target
                
                Button btn = tabBtn.AddComponent<Button>();
                UIPanelNavButton navBehavior = tabBtn.AddComponent<UIPanelNavButton>();
                switch(i)
                {
                    case 2: navBehavior.targetPanel = UIPanelType.MainScreen; controller.tabLobby = btn; break;
                    case 4: navBehavior.targetPanel = UIPanelType.BossBattle; controller.tabBattlefield = btn; break;
                    case 0: navBehavior.targetPanel = UIPanelType.ArenaShop; controller.tabShop = btn; break; // Placeholder
                    case 1: navBehavior.targetPanel = UIPanelType.Barrack; controller.tabBarracks = btn; break;
                    case 3: navBehavior.targetPanel = UIPanelType.None; controller.tabAlliance = btn; break; // Not mapped
                }

                CreateTMPText(tabBtn.transform, tabs[i], Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, 24, Color.white);
            }

            Selection.activeGameObject = rootObj;
            Debug.Log("Generated BossBattlePanel!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void SetRect(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
        }

        private static void SetStretchOffsets(GameObject obj, float left, float right, float top, float bottom)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static TMPro.TextMeshProUGUI CreateTMPText(Transform parent, string text, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 aPos, Vector2 sizeScale, float fontSize, Color c, TMPro.TextAlignmentOptions alignment = TMPro.TextAlignmentOptions.Center)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            var tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = c;
            tmp.alignment = alignment;
            
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot;
            rt.sizeDelta = sizeScale;
            rt.anchoredPosition = aPos;

            return tmp;
        }
    }
}
#endif

