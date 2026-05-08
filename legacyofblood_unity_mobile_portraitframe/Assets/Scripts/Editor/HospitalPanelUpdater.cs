#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood.Editor
{
    public static class HospitalPanelUpdater
    {
        [MenuItem("UI Tools/Update Hospital HealButton")]
        public static void UpdateHealButton()
        {
            AssetDatabase.Refresh();
            string spritePath = "Assets/Art/UI/Hospital/HealButton.png";

            // Nhập Sprite cho Button
            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }

            Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (newSprite == null)
            {
                Debug.LogError($"[Antigravity] Không tìm thấy file ảnh tại {spritePath}. Hãy chắc chắn bạn đã đổi tên và lưu đúng chỗ!");
                return;
            }

            // Cập nhật tất cả các Prefab thẻ bệnh nhân (InjuredHeroCard)
            string[] guids = AssetDatabase.FindAssets("t:Prefab InjuredHeroCard");
            if (guids.Length == 0) guids = AssetDatabase.FindAssets("t:Prefab HeroWardCard");
            
            bool found = false;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                Transform btnTransform = prefab.transform.Find("PriceButton");
                if (btnTransform == null) btnTransform = prefab.transform.Find("HealButton");

                if (btnTransform != null)
                {
                    Image btnImg = btnTransform.GetComponent<Image>();
                    if (btnImg != null)
                    {
                        btnImg.sprite = newSprite;
                        btnImg.color = Color.white; // Bỏ màu sắc tinted xanh cũ
                        btnImg.preserveAspect = true; // Thêm chế độ bảo toàn tỷ lệ
                        
                        // Chỉnh text hiển thị (Dịch sang trái vì hình dấu thập nằm bên phải)
                        Transform textObj = btnTransform.Find("Text");
                        if (textObj != null)
                        {
                            RectTransform textRect = textObj.GetComponent<RectTransform>();
                            textRect.offsetMax = new Vector2(-40, 0); // Cách lề phải 40px để tránh đè lên dấu thập
                            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
                            if (tmp != null)
                            {
                                tmp.alignment = TextAlignmentOptions.Center;
                                tmp.fontStyle = FontStyles.Bold;
                                tmp.color = new Color(0.9f, 0.9f, 0.9f);
                                tmp.text = "HEAL <color=#FFD700>50G</color>";
                            }
                        }
                        
                        EditorUtility.SetDirty(prefab);
                        found = true;
                    }
                }
            }

            // Cập nhật thẻ trên Scene (Mock data)
            var hospitalPanels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var panel in hospitalPanels)
            {
                Transform[] allChildren = panel.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == "PriceButton" || child.name == "HealButton")
                    {
                        Image btnImg = child.GetComponent<Image>();
                        if (btnImg != null)
                        {
                            btnImg.sprite = newSprite;
                            btnImg.color = Color.white; // Xóa màu xanh lá
                            btnImg.preserveAspect = true;
                            
                            Transform textObj = child.Find("Text");
                            if (textObj != null)
                            {
                                RectTransform textRect = textObj.GetComponent<RectTransform>();
                                textRect.offsetMax = new Vector2(-40, 0); 
                                TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
                                if (tmp != null)
                                {
                                    tmp.color = new Color(0.9f, 0.9f, 0.9f);
                                    tmp.text = "HEAL <color=#FFD700>50G</color>";
                                }
                            }
                            EditorUtility.SetDirty(child.gameObject);
                            found = true;
                        }
                    }
                }
            }

            // Lưu lại prefab
            if (found)
            {
                AssetDatabase.SaveAssets();
                if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty == false)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
                Debug.Log("🎉 [Antigravity] Đã thay áo mới cho nút HealButton thành công!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy Prefab thẻ bệnh nhân hoặc nút HealButton. Bạn hãy thử lưu Scene lại trước nhé.");
            }
        }

        [MenuItem("UI Tools/Update Hospital Titles")]
        public static void UpdateSevereTitle()
        {
            string severePath = "Assets/Art/UI/Hospital/SevereTitle.png";
            string lightPath = "Assets/Art/UI/Hospital/LightTitle.png";

            // Helper to import sprite
            void ImportSprite(string path)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    
                    TextureImporterSettings settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect; 
                    importer.SetTextureSettings(settings);

                    importer.SaveAndReimport();
                }
            }

            ImportSprite(severePath);
            ImportSprite(lightPath);

            Sprite severeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(severePath);
            Sprite lightSprite = AssetDatabase.LoadAssetAtPath<Sprite>(lightPath);

            if (severeSprite == null && lightSprite == null)
            {
                Debug.LogError($"[Antigravity] Không tìm thấy ảnh. Hãy chắc chắn lưu SevereTitle.png và LightTitle.png tại Assets/Art/UI/Hospital/");
                return;
            }

            bool found = false;

            // Hàm hỗ trợ áp dụng sprite
            void ApplySprite(Transform child, Sprite newSprite)
            {
                if (newSprite == null) return;
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = newSprite;
                    img.color = Color.white;
                    img.preserveAspect = true; // GIỮ NGUYÊN TỈ LỆ ẢNH CHỐNG MÉO
                    
                    // Nới lỏng khung viền RectTransform để ảnh không bị bóp nhỏ lại
                    RectTransform rt = img.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(800, 200); // Khung lớn hơn để chứa chữ
                    }
                    
                    Transform textObj = child.Find("Text");
                    if (textObj != null) textObj.gameObject.SetActive(false);
                }
            }

            // Cập nhật thẻ trên Scene (Mock data)
            var hospitalPanels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var panel in hospitalPanels)
            {
                Transform[] allChildren = panel.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == "SevereTitle") { ApplySprite(child, severeSprite); EditorUtility.SetDirty(panel.gameObject); found = true; }
                    if (child.name == "LightTitle") { ApplySprite(child, lightSprite); EditorUtility.SetDirty(panel.gameObject); found = true; }
                }
            }

            // Cập nhật Prefab EXTRACTED_Hospital_Panel
            string[] guids = AssetDatabase.FindAssets("t:Prefab Hospital");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Transform[] allChildren = prefab.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in allChildren)
                    {
                        if (child.name == "SevereTitle") { ApplySprite(child, severeSprite); EditorUtility.SetDirty(prefab); found = true; }
                        if (child.name == "LightTitle") { ApplySprite(child, lightSprite); EditorUtility.SetDirty(prefab); found = true; }
                    }
                }
            }

            if (found)
            {
                Debug.Log("<color=cyan><b>[Antigravity]</b></color> <color=green>Đã đổi thành công 2 ảnh tiêu đề (Severe và Light) vào toàn bộ Bệnh Viện!</color>");
                AssetDatabase.SaveAssets();
                if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty == false)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
                Debug.Log("🎉 [Antigravity] Đã thay nhãn SevereTitle mới thành công!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy SevereTitle trong scene hoặc Prefab.");
            }
        }
        [MenuItem("UI Tools/Update Hospital CardFrame")]
        public static void UpdateCardFrame()
        {
            AssetDatabase.Refresh();
            string spritePath = "Assets/Art/UI/Hospital/HospitalCardFrame.png";

            // Nhập Sprite cho CardFrame
            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect; 
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }

            Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (newSprite == null)
            {
                Debug.LogError($"[Antigravity] Không tìm thấy file ảnh tại {spritePath}.");
                return;
            }

            bool found = false;

            void ApplySprite(Transform child)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = newSprite;
                    img.color = Color.white; 
                    img.preserveAspect = true; // IMPORTANT for card frames to avoid severe distortion

                    // Ensure dimensions are reasonably reset if it was previously weirdly scaled
                    // CardFrame native size is 726x1024, le.preferredWidth is 157.
                    // Keep anchors and offsets as (0,1) stretched, and it will fill natively!
                }
            }

            // Cập nhật thẻ trên Scene (Mock data)
            var hospitalPanels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var panel in hospitalPanels)
            {
                Transform[] allChildren = panel.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == "CardFrame")
                    {
                        ApplySprite(child);
                        EditorUtility.SetDirty(panel.gameObject);
                        found = true;
                    }
                }
            }

            // Cập nhật Prefab EXTRACTED_Hospital_Panel và các prefabs thẻ binh lính
            string[] guids = AssetDatabase.FindAssets("t:Prefab Hospital");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Transform[] allChildren = prefab.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in allChildren)
                    {
                        if (child.name == "CardFrame")
                        {
                            ApplySprite(child);
                            EditorUtility.SetDirty(prefab);
                            found = true;
                        }
                    }
                }
            }

            // Cards specific
            string[] cardGuids = AssetDatabase.FindAssets("t:Prefab InjuredHeroCard");
            if (cardGuids.Length == 0) cardGuids = AssetDatabase.FindAssets("t:Prefab HeroWardCard");
            foreach (string guid in cardGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Transform cardFrame = prefab.transform.Find("CardFrame");
                    if (cardFrame != null)
                    {
                        ApplySprite(cardFrame);
                        EditorUtility.SetDirty(prefab);
                        found = true;
                    }
                }
            }

            if (found)
            {
                Debug.Log("<color=cyan><b>[Antigravity]</b></color> <color=green>Đã đổi thành công CardFrame mới!</color>");
                AssetDatabase.SaveAssets();
                if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty == false)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy CardFrame nào trong scene hoặc prefab.");
            }
        }
        [MenuItem("UI Tools/Fix Portrait Layer Force", false)]
        public static void FixPortraitLayerForce()
        {
            AssetDatabase.Refresh();
            int count = 0;
            var cards = Object.FindObjectsByType<LegendOfBlood.InjuredHeroCard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in cards)
            {
                var p = c.transform.Find("Portrait");
                if (p != null)
                {
                    p.SetAsFirstSibling();
                    EditorUtility.SetDirty(c.gameObject);
                    count++;
                }
            }

            var hospitalPanels = Object.FindObjectsByType<LegendOfBlood.HospitalPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var panel in hospitalPanels)
            {
                Transform[] allChildren = panel.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == "Portrait" && child.parent != null)
                    {
                        child.SetAsFirstSibling();
                        EditorUtility.SetDirty(child.parent.gameObject);
                        count++;
                    }
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab Hospital");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Transform[] allChildren = prefab.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in allChildren)
                    {
                        if (child.name == "Portrait" && child.parent != null)
                        {
                            child.SetAsFirstSibling();
                            EditorUtility.SetDirty(prefab);
                            count++;
                        }
                    }
                }
            }

            string[] cardGuids = AssetDatabase.FindAssets("t:Prefab InjuredHeroCard");
            if (cardGuids.Length == 0) cardGuids = AssetDatabase.FindAssets("t:Prefab HeroWardCard");
            foreach (string guid in cardGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Transform portrait = prefab.transform.Find("Portrait");
                    if (portrait != null)
                    {
                        portrait.SetAsFirstSibling();
                        EditorUtility.SetDirty(prefab);
                        count++;
                    }
                }
            }

            Debug.Log($"Fixed {count} portraits to be at Sibling 0.");
            AssetDatabase.SaveAssets();
            if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
    }
}
#endif
