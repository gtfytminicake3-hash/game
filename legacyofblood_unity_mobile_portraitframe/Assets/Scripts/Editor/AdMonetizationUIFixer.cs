using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood;
using LegendOfBlood.UI;

namespace LegendOfBlood.EditorScripts
{
    public class AdMonetizationUIFixer : EditorWindow
    {
        [MenuItem("LegendOfBlood/Fix Ad Monetization Prefabs")]
        public static void FixPrefabs()
        {
            FixRecruitmentPanel();
            FixBuildingUpgradePanel();
            FixMainScreenPanel();
            FixBreedingPanel();
            FixArenaShopPanel();
            FixTowerPanel();
            FixMailboxPanel();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>All Missing Ad Monetization and Mailbox UI Components Generated!</color>");
        }

        private static GameObject CreateButton(GameObject parent, string name, string textStr, Vector2 anchoredPosition)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent.transform, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 50);
            rt.anchoredPosition = anchoredPosition;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = textStr;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;

            return btnObj;
        }

        private static GameObject CreateText(GameObject parent, string name, string textStr, Vector2 anchoredPosition)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(parent.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(200, 50);
            textRt.anchoredPosition = anchoredPosition;

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = textStr;
            tmp.alignment = TextAlignmentOptions.Center;
            
            return textObj;
        }

        private static GameObject CreateToggle(GameObject parent, string name, string textStr, Vector2 anchoredPosition)
        {
            GameObject toggleObj = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(parent.transform, false);
            RectTransform rt = toggleObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 20);
            rt.anchoredPosition = anchoredPosition;
            
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(toggleObj.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.5f);
            bgRt.anchorMax = new Vector2(0, 0.5f);
            bgRt.sizeDelta = new Vector2(20, 20);
            bgRt.anchoredPosition = new Vector2(10, 0);

            GameObject checkmarkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmarkObj.transform.SetParent(bgObj.transform, false);
            RectTransform checkRt = checkmarkObj.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.5f, 0.5f);
            checkRt.anchorMax = new Vector2(0.5f, 0.5f);
            checkRt.sizeDelta = new Vector2(14, 14);
            checkmarkObj.GetComponent<Image>().color = Color.black;

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.targetGraphic = bgObj.GetComponent<Image>();
            toggle.graphic = checkmarkObj.GetComponent<Image>();

            GameObject textObj = CreateText(toggleObj, "Label", textStr, new Vector2(90, 0));
            textObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            return toggleObj;
        }

        private static void FixRecruitmentPanel()
        {
            string path = "Assets/Prefabs/Panel/Panel_Recruitment.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = editingScope.prefabContentsRoot;
                var script = root.GetComponent<RecruitmentPanel>();
                if (script != null && script.recruitAdButton == null)
                {
                    GameObject btn = CreateButton(root, "Btn_RecruitAd", "Ads Recruit", new Vector2(0, -100));
                    script.recruitAdButton = btn.GetComponent<Button>();
                    Debug.Log("Added recruitAdButton to Panel_Recruitment");
                }
            }
        }

        private static void FixBuildingUpgradePanel()
        {
            string path = "Assets/Prefabs/Panel/BuildingUpgradePanel.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = editingScope.prefabContentsRoot;
                var script = root.GetComponent<BuildingUpgradePanel>();
                if (script != null)
                {
                    if (script.speedUpAdButton == null)
                    {
                        GameObject btn = CreateButton(root, "Btn_SpeedUpAd", "Speed Up", new Vector2(0, -150));
                        script.speedUpAdButton = btn.GetComponent<Button>();
                    }
                    if (script.upgradeTimerText == null)
                    {
                        GameObject txt = CreateText(root, "Text_UpgradeTimer", "Time: 00:00", new Vector2(0, 50));
                        script.upgradeTimerText = txt.GetComponent<TextMeshProUGUI>();
                    }
                    if (script.benefitText == null)
                    {
                        GameObject txt = CreateText(root, "Text_Benefit", "Benefit", new Vector2(0, 0));
                        script.benefitText = txt.GetComponent<TextMeshProUGUI>();
                    }
                    Debug.Log("Added elements to BuildingUpgradePanel");
                }
            }
        }

        private static void FixMainScreenPanel()
        {
            string path = "Assets/Prefabs/Panel/Panel_MainScreen.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = editingScope.prefabContentsRoot;
                var script = root.GetComponent<UIMainController>();
                if (script != null)
                {
                    if (script.mysticChestAdButton == null)
                    {
                        GameObject btn = CreateButton(root, "Btn_MysticChestAd", "Mystic Chest", new Vector2(250, 150));
                        script.mysticChestAdButton = btn.GetComponent<Button>();
                    }
                    // Attach ProfessionSelectionPanel Prefab as a child if missing
                    if (script.professionSelectionPanel == null)
                    {
                        GameObject profPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Panel/Panel_ProfessionSelection.prefab");
                        if (profPrefab != null)
                        {
                            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(profPrefab, root.transform);
                            inst.SetActive(false); // Default hidden
                            script.professionSelectionPanel = inst.GetComponent<ProfessionSelectionPanel>();
                        }
                    }
                    Debug.Log("Added elements to Panel_MainScreen");
                }
            }
        }

        private static void FixBreedingPanel()
        {
            string path = "Assets/Prefabs/Panel/EXTRACTED_Breeding_Panel.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = editingScope.prefabContentsRoot;
                var script = root.GetComponent<BreedingUIController>();
                if (script != null)
                {
                    if (script.mutationAdButton == null)
                    {
                        GameObject btn = CreateButton(root, "Btn_MutationAd", "Mutation Boost", new Vector2(0, -200));
                        script.mutationAdButton = btn.GetComponent<Button>();
                    }
                    if (script.useMutationPotionToggle == null)
                    {
                        GameObject toggle = CreateToggle(root, "Toggle_MutationPotion", "Use Potion", new Vector2(0, -150));
                        script.useMutationPotionToggle = toggle.GetComponent<Toggle>();
                    }
                    if (script.heroPickerPanel == null)
                    {
                        GameObject pickerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Panel/EXTRACTED_HeroPicker_Panel.prefab");
                        if (pickerPrefab != null)
                        {
                            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(pickerPrefab, root.transform);
                            inst.SetActive(false);
                            script.heroPickerPanel = inst.GetComponent<HeroPickerPanel>();
                        }
                    }
                    Debug.Log("Added elements to EXTRACTED_Breeding_Panel");
                }
            }
        }

        private static void FixArenaShopPanel()
        {
            string path = "Assets/Prefabs/Panel/Panel_ArenaShop.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = editingScope.prefabContentsRoot;
                var script = root.GetComponent<ArenaShopPanel>();
                if (script != null && script.adFreebieButton == null)
                {
                    GameObject btn = CreateButton(root, "Btn_AdFreebie", "Free Gift", new Vector2(200, 300));
                    script.adFreebieButton = btn.GetComponent<Button>();
                    Debug.Log("Added adFreebieButton to Panel_ArenaShop");
                }
            }
        }

        private static void FixTowerPanel()
        {
            string[] paths = { "Assets/Prefabs/Panel/Panel_Tower.prefab", "Assets/Prefabs/Panel/Panel_Tower 1.prefab" };
            foreach (var path in paths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var root = editingScope.prefabContentsRoot;
                    var script = root.GetComponent<TowerPanel>();
                    if (script != null && script.skipCooldownAdButton == null)
                    {
                        GameObject btn = CreateButton(root, "Btn_SkipCooldownAd", "Skip Cooldown", new Vector2(0, -250));
                        script.skipCooldownAdButton = btn.GetComponent<Button>();
                        Debug.Log("Added skipCooldownAdButton to " + path);
                    }
                }
            }
        }

        private static void FixMailboxPanel()
        {
            string path = "Assets/Prefabs/Panel/Panel_Mailbox.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = editingScope.prefabContentsRoot;
                var script = root.GetComponent<MailboxPanel>();
                if (script != null)
                {
                    if (script.reviveRetryButton == null)
                    {
                        GameObject btn = CreateButton(root, "Btn_ReviveRetry", "Revive Retry", new Vector2(0, -100));
                        script.reviveRetryButton = btn.GetComponent<Button>();
                    }
                    if (script.claimX2Button == null)
                    {
                        GameObject btn = CreateButton(root, "Btn_ClaimX2", "Claim x2 (Ad)", new Vector2(-100, -150));
                        script.claimX2Button = btn.GetComponent<Button>();
                    }
                    if (script.replayButton == null)
                    {
                        GameObject btn = CreateButton(root, "Btn_Replay", "Replay Battle", new Vector2(100, -150));
                        script.replayButton = btn.GetComponent<Button>();
                    }
                    if (script.replayButtonText == null && script.replayButton != null)
                    {
                        script.replayButtonText = script.replayButton.GetComponentInChildren<TextMeshProUGUI>();
                    }
                    Debug.Log("Added elements to Panel_Mailbox");
                }
            }
        }
    }
}
