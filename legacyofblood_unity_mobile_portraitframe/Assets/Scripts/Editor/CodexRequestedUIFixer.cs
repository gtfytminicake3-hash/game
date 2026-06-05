namespace LegendOfBlood.EditorTools
{
    using LegendOfBlood;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    public static class CodexRequestedUIFixer
    {
        private const string BarrackPanelPath = "Assets/Prefabs/Panel/Panel_Barrack.prefab";
        private const string BootloaderPanelPath = "Assets/Prefabs/Panel/Panel_Bootloader.prefab";
        private const string HeroCardPath = "Assets/Prefabs/HeroCard_Prefab.prefab";
        private const string InjuredHeroCardPath = "Assets/Prefabs/Panel/InjuredHeroCard_Final.prefab";

        [MenuItem("Tools/Codex/Apply Requested UI Fixes")]
        public static void ApplyRequestedUIFixes()
        {
            FixBarrackPrefab();
            FixBootloaderPrefab();
            FixHeroCardPrefab();
            FixInjuredHeroCardPrefab();
            FixOpenSceneBarrackPanels();
            FixOpenSceneBootloaderPanels();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CodexRequestedUIFixer] Applied requested UI fixes.");
        }

        private static void FixBarrackPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(BarrackPanelPath);
            try
            {
                BarrackPanel panel = root.GetComponentInChildren<BarrackPanel>(true);
                if (panel == null) return;

                SerializedObject so = new SerializedObject(panel);
                so.FindProperty("upgradeBuildingButton").objectReferenceValue = FindButton(root.transform, "UpgradeButton_Auto", "UpgradeButton", "Upgrade");
                so.FindProperty("associatedBuildingId").stringValue = "Barracks";

                Object heroCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroCardPath);
                if (heroCardPrefab != null) so.FindProperty("heroCardPrefab").objectReferenceValue = heroCardPrefab;

                AssignIfNull(so, "closeButton", FindButton(root.transform, "CloseButton"));
                AssignIfNull(so, "populationManagerButton", FindButton(root.transform, "PopulationButton"));
                Button sortButton = FindOrCreateButton(root.transform, "SortButton", "Sort");
                Button filterButton = FindOrCreateButton(root.transform, "FilterButton", "Filter");
                so.FindProperty("sortButton").objectReferenceValue = sortButton;
                so.FindProperty("filterButton").objectReferenceValue = filterButton;
                so.FindProperty("sortButtonText").objectReferenceValue = FindOrCreateButtonText(sortButton, "Sort");
                so.FindProperty("filterButtonText").objectReferenceValue = FindOrCreateButtonText(filterButton, "Filter");
                so.FindProperty("populationCountText").objectReferenceValue = FindTMP(root.transform, "PopulationText", "Population");

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(panel);
                PrefabUtility.SaveAsPrefabAsset(root, BarrackPanelPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void FixHeroCardPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HeroCardPath);
            try
            {
                FixHeroCard(root.GetComponent<HeroCard>(), root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, HeroCardPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void FixBootloaderPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(BootloaderPanelPath);
            try
            {
                StretchRect(root.GetComponent<RectTransform>());
                Transform bg = FindTransform(root.transform, "BootloaderBackgroundImage");
                if (bg != null) StretchRect(bg.GetComponent<RectTransform>());

                CanvasScaler scaler = root.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = root.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                PrefabUtility.SaveAsPrefabAsset(root, BootloaderPanelPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void FixInjuredHeroCardPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(InjuredHeroCardPath);
            try
            {
                RectTransform rootRt = root.GetComponent<RectTransform>();
                if (rootRt != null)
                {
                    rootRt.sizeDelta = new Vector2(640f, 300f);
                }

                LayoutElement layout = root.GetComponent<LayoutElement>();
                if (layout == null) layout = root.AddComponent<LayoutElement>();
                layout.minWidth = 640f;
                layout.preferredWidth = 640f;
                layout.minHeight = 300f;
                layout.preferredHeight = 300f;
                layout.flexibleWidth = 0f;
                layout.flexibleHeight = 0f;

                SetRect(root.transform, "PortraitArea", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 12f), new Vector2(156f, 208f));
                SetRect(root.transform, "AvatarImage", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(32f, 12f), new Vector2(140f, 188f));
                SetRect(root.transform, "CardFrame", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 12f), new Vector2(156f, 208f));
                SetRect(root.transform, "NamePlate", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(70f, -18f), new Vector2(-220f, 44f));
                SetRect(root.transform, "NameText", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(70f, -18f), new Vector2(-220f, 44f));
                SetRect(root.transform, "LevelText", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190f, -78f), new Vector2(120f, 34f));
                SetRect(root.transform, "CombatPowerText_Runtime", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(190f, -118f), new Vector2(-240f, 36f));
                SetRect(root.transform, "TimerText", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(190f, 86f), new Vector2(-240f, 42f));
                SetRect(root.transform, "HealButton", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(190f, 64f));

                Image avatar = FindImage(root.transform, "AvatarImage");
                if (avatar != null)
                {
                    avatar.color = Color.white;
                    avatar.preserveAspect = true;
                    avatar.raycastTarget = false;
                }

                FixHeroCard(root.GetComponent<HeroCard>(), root.transform);
                EditorUtility.SetDirty(root);
                PrefabUtility.SaveAsPrefabAsset(root, InjuredHeroCardPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void FixOpenSceneBarrackPanels()
        {
            BarrackPanel[] panels = Object.FindObjectsByType<BarrackPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (BarrackPanel panel in panels)
            {
                SerializedObject so = new SerializedObject(panel);
                so.FindProperty("upgradeBuildingButton").objectReferenceValue = FindButton(panel.transform, "UpgradeButton_Auto", "UpgradeButton", "Upgrade");
                so.FindProperty("associatedBuildingId").stringValue = "Barracks";
                Object heroCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroCardPath);
                if (heroCardPrefab != null) so.FindProperty("heroCardPrefab").objectReferenceValue = heroCardPrefab;
                Button sortButton = FindOrCreateButton(panel.transform, "SortButton", "Sort");
                Button filterButton = FindOrCreateButton(panel.transform, "FilterButton", "Filter");
                so.FindProperty("sortButton").objectReferenceValue = sortButton;
                so.FindProperty("filterButton").objectReferenceValue = filterButton;
                so.FindProperty("sortButtonText").objectReferenceValue = FindOrCreateButtonText(sortButton, "Sort");
                so.FindProperty("filterButtonText").objectReferenceValue = FindOrCreateButtonText(filterButton, "Filter");
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(panel);
            }

            if (panels.Length > 0) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void FixOpenSceneBootloaderPanels()
        {
            Bootloader[] bootloaders = Object.FindObjectsByType<Bootloader>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Bootloader bootloader in bootloaders)
            {
                StretchRect(bootloader.GetComponent<RectTransform>());
                Transform bg = bootloader.transform.Find("BootloaderBackgroundImage");
                if (bg != null) StretchRect(bg.GetComponent<RectTransform>());

                Canvas canvas = bootloader.GetComponentInParent<Canvas>();
                CanvasScaler scaler = canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080f, 1920f);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                    EditorUtility.SetDirty(scaler);
                }

                EditorUtility.SetDirty(bootloader);
            }

            if (bootloaders.Length > 0) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void FixHeroCard(HeroCard card, Transform root)
        {
            if (card == null) return;

            SerializedObject so = new SerializedObject(card);
            so.FindProperty("nameText").objectReferenceValue = FindTMP(root, "NameText", "NamePlate", "name");
            so.FindProperty("levelText").objectReferenceValue = FindTMP(root, "LevelText", "level");
            so.FindProperty("combatPowerText").objectReferenceValue = FindTMP(root, "CPText", "CombatPowerText", "combat", "cp");
            so.FindProperty("professionIcon").objectReferenceValue = FindImage(root, "ProfessionIcon", "ClassBadge", "class");
            so.FindProperty("avatarImage").objectReferenceValue = FindImage(root, "AvatarImage", "Avatar", "Portrait");
            so.FindProperty("cardFrame").objectReferenceValue = FindImage(root, "FrameImage", "CardFrame", "frame");
            so.FindProperty("cardButton").objectReferenceValue = root.GetComponent<Button>();
            so.FindProperty("warriorIcon").objectReferenceValue = LoadProfessionIcon("warrior");
            so.FindProperty("archerIcon").objectReferenceValue = LoadProfessionIcon("archer");
            so.FindProperty("mageIcon").objectReferenceValue = LoadProfessionIcon("mage");
            so.FindProperty("healerIcon").objectReferenceValue = LoadProfessionIcon("healer");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(card);
        }

        private static Sprite LoadProfessionIcon(string professionName)
        {
            Sprite sprite = Resources.Load<Sprite>($"UI/barrack/icon_{professionName}");
            if (sprite == null) sprite = Resources.Load<Sprite>($"Icons/Gen/icon_skill_{professionName}");
            return sprite;
        }

        private static void AssignIfNull(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty property = so.FindProperty(propertyName);
            if (property != null && property.objectReferenceValue == null && value != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static Button FindButton(Transform root, params string[] names)
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            foreach (string name in names)
            {
                foreach (Button button in buttons)
                {
                    if (button != null && button.gameObject.name.ToLowerInvariant().Contains(name.ToLowerInvariant()))
                    {
                        return button;
                    }
                }
            }
            return null;
        }

        private static Button FindOrCreateButton(Transform root, params string[] names)
        {
            Button button = FindButton(root, names);
            if (button != null) return button;

            Transform target = FindTransform(root, names);
            if (target == null) return null;

            Image image = target.GetComponent<Image>();
            if (image == null) image = target.gameObject.AddComponent<Image>();
            image.raycastTarget = true;

            button = target.GetComponent<Button>();
            if (button == null) button = target.gameObject.AddComponent<Button>();
            EditorUtility.SetDirty(target.gameObject);
            return button;
        }

        private static TextMeshProUGUI FindTMP(Transform root, params string[] names)
        {
            TextMeshProUGUI[] texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (string name in names)
            {
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text != null && text.gameObject.name.ToLowerInvariant().Contains(name.ToLowerInvariant()))
                    {
                        return text;
                    }
                }
            }
            return null;
        }

        private static TextMeshProUGUI FindOrCreateButtonText(Button button, string fallbackText)
        {
            if (button == null) return null;

            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text == null)
            {
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(button.transform, false);
                text = textObj.AddComponent<TextMeshProUGUI>();
                RectTransform rt = text.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            text.text = fallbackText;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 28f;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.raycastTarget = false;
            EditorUtility.SetDirty(text);
            return text;
        }

        private static Image FindImage(Transform root, params string[] names)
        {
            Image[] images = root.GetComponentsInChildren<Image>(true);
            foreach (string name in names)
            {
                foreach (Image image in images)
                {
                    if (image != null && image.gameObject.name.ToLowerInvariant().Contains(name.ToLowerInvariant()))
                    {
                        return image;
                    }
                }
            }
            return null;
        }

        private static void SetRect(Transform root, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            Transform child = FindTransform(root, name);
            RectTransform rt = child != null ? child.GetComponent<RectTransform>() : null;
            if (rt == null) return;

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
            EditorUtility.SetDirty(rt);
        }

        private static void StretchRect(RectTransform rect)
        {
            if (rect == null) return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            EditorUtility.SetDirty(rect);
        }

        private static Transform FindTransform(Transform root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child != null && child.gameObject.name == name) return child;
            }
            return null;
        }

        private static Transform FindTransform(Transform root, params string[] names)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (string name in names)
            {
                string targetName = name.ToLowerInvariant();
                foreach (Transform child in children)
                {
                    if (child != null && child.gameObject.name.ToLowerInvariant().Contains(targetName))
                    {
                        return child;
                    }
                }
            }
            return null;
        }
    }
}
