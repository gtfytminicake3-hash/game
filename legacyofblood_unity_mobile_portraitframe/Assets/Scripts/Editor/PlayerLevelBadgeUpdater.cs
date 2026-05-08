using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace LegendOfBlood.Editor
{
    public static class PlayerLevelBadgeUpdater
    {
        [MenuItem("UI Tools/Update PlayerLevel Badge")]
        public static void UpdatePlayerLevelBadge()
        {
            string framePath = "Assets/Art/UI/PlayerLevel/PlayerLevelFrame.png";
            string fillPath = "Assets/Art/UI/PlayerLevel/PlayerLevelFill.png";

            // Nhập Sprite cho Frame
            TextureImporter frameImporter = AssetImporter.GetAtPath(framePath) as TextureImporter;
            if (frameImporter != null && frameImporter.textureType != TextureImporterType.Sprite)
            {
                frameImporter.textureType = TextureImporterType.Sprite;
                frameImporter.spriteImportMode = SpriteImportMode.Single;
                frameImporter.alphaIsTransparency = true;
                frameImporter.SaveAndReimport();
            }

            // Nhập Sprite cho Fill
            TextureImporter fillImporter = AssetImporter.GetAtPath(fillPath) as TextureImporter;
            if (fillImporter != null && fillImporter.textureType != TextureImporterType.Sprite)
            {
                fillImporter.textureType = TextureImporterType.Sprite;
                fillImporter.spriteImportMode = SpriteImportMode.Single;
                fillImporter.alphaIsTransparency = true;
                fillImporter.SaveAndReimport();
            }

            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fillPath);

            GameObject oldBadge = null;
            var allTransforms = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTransforms)
            {
                if (t.name == "PlayerLevelBadge")
                {
                    oldBadge = t.gameObject;
                    break;
                }
            }

            if (oldBadge == null)
            {
                Debug.LogError("[Antigravity] Không tìm thấy 'PlayerLevelBadge' trong scene (kể cả khi đã ẩn). Hãy chắc chắn bạn đã Generate giao diện trước đó!");
                return;
            }

            // Xóa các thành phần cũ
            Transform badgeBg = oldBadge.transform.Find("BadgeBg");
            if (badgeBg != null) Object.DestroyImmediate(badgeBg.gameObject);

            // Tìm Text Cũ
            Transform textLvl = oldBadge.transform.Find("Text_LVL");
            Transform textLevel = oldBadge.transform.Find("Text_Level");

            RectTransform badgeRect = oldBadge.GetComponent<RectTransform>();
            badgeRect.sizeDelta = new Vector2(300, 224); // Tỷ lệ 1024x764 đã được thu nhỏ
            badgeRect.anchoredPosition = new Vector2(20, -5);

            // Tạo Fill (Nằm dưới Frame)
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(oldBadge.transform, false);
            fillObj.transform.SetSiblingIndex(0); // Dưới cùng
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(85, 93); // Left, Bottom
            fillRect.offsetMax = new Vector2(-18, -93); // Right, Top
            fillRect.sizeDelta = new Vector2(fillRect.sizeDelta.x, fillRect.sizeDelta.y); // Tính lại sau offset
            fillRect.anchoredPosition = new Vector2(fillRect.anchoredPosition.x, fillRect.anchoredPosition.y);

            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = fillSprite;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0.5f; // Mẫu 50%

            // Tạo Frame (Nằm đè lên Fill)
            GameObject frameObj = new GameObject("Frame");
            frameObj.transform.SetParent(oldBadge.transform, false);
            frameObj.transform.SetSiblingIndex(1); // Trên Fill
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.sizeDelta = Vector2.zero;
            frameRect.anchoredPosition = Vector2.zero;

            Image frameImage = frameObj.AddComponent<Image>();
            frameImage.sprite = frameSprite;
            frameImage.raycastTarget = false;

            // Chỉnh lại vị trí Text cho khớp với vòng tròn gai bên trái
            // Vòng tròn nằm dịch về bên trái so với khung
            if (textLvl != null)
            {
                textLvl.SetAsLastSibling();
                RectTransform rt = textLvl.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-15, 20); // Dịch vào tâm vòng tròn gai
                var tmp = textLvl.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = new Color(0.9f, 0.8f, 0.6f, 1f); // Màu Vàng Đồng
                }
            }

            if (textLevel != null)
            {
                textLevel.SetAsLastSibling();
                RectTransform rt = textLevel.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-15, -10); // Dịch vào tâm vòng tròn gai
                var tmp = textLevel.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize = 24;
                    tmp.color = new Color(1f, 1f, 1f, 1f);
                }
            }

            // Tìm Text Exp cũ nếu có
            Transform existingExpText = oldBadge.transform.Find("Text_EXP");
            if (existingExpText != null) Object.DestroyImmediate(existingExpText.gameObject);

            // Tạo Exp Text mới
            GameObject expTextObj = new GameObject("Text_EXP");
            expTextObj.transform.SetParent(oldBadge.transform, false);
            expTextObj.transform.SetAsLastSibling();
            RectTransform expRect = expTextObj.AddComponent<RectTransform>();
            expRect.anchorMin = new Vector2(0.5f, 0.5f);
            expRect.anchorMax = new Vector2(0.5f, 0.5f);
            expRect.sizeDelta = new Vector2(150, 40);
            expRect.anchoredPosition = new Vector2(35, -5); // Dịch một chút về phải vì hình tròn to ở bên trái

            TextMeshProUGUI expTmp = expTextObj.AddComponent<TextMeshProUGUI>();
            expTmp.text = "0 / 1000";
            expTmp.alignment = TextAlignmentOptions.CenterGeoAligned;
            expTmp.fontSize = 14;
            expTmp.color = Color.white;
            expTmp.fontStyle = FontStyles.Bold;

            // Gắn Script Quản Lý (PlayerLevelBadgeUI)
            var badgeUI = oldBadge.GetComponent<LegendOfBlood.UI.PlayerLevelBadgeUI>();
            if (badgeUI == null)
            {
                badgeUI = oldBadge.AddComponent<LegendOfBlood.UI.PlayerLevelBadgeUI>();
            }
            if (textLevel != null) badgeUI.levelText = textLevel.GetComponent<TextMeshProUGUI>();
            badgeUI.expFillImage = fillImage;
            badgeUI.expText = expTmp;

            EditorUtility.SetDirty(oldBadge);
            if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

            Debug.Log("🎉 [Antigravity] Đã thay lớp da mới cho PlayerLevelBadge thành công!");
        }

        [MenuItem("UI Tools/DEBUG: Add 200 Player EXP")]
        public static void DebugAddPlayerExp()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Vui lòng nhấn nút Play (Chạy Game) trước khi dùng tool này để test thêm EXP!");
                return;
            }
            if (GameManager.Instance != null && GameManager.Instance.InventoryManager != null)
            {
                GameManager.Instance.InventoryManager.AddPlayerExp(200);
                Debug.Log("🎉 Đã buff thành công 200 EXP cho tài khoản!");
            }
        }
    }
}
