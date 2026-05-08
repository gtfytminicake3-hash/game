#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroCardReplacer
{
    [MenuItem("Tools/Rebuild Hero Card Prefab")]
    public static void RebuildPrefab()
    {
        string path = "Assets/HeroCard_SquadSelection_Prefab.prefab";
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (originalPrefab == null)
        {
            Debug.LogError($"Could not find HeroCard Prefab at {path}!");
            return;
        }
        
        // Helper để load Sprite chắc chắn hơn
        Sprite LoadSprite(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach(Object a in assets) if (a is Sprite s) return s;
            return null;
        }

        // Tạo một GameObject tạm trên scene để build lại cấu trúc
        GameObject root = new GameObject("HeroCard_Temp");
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(250, 340); // Tỷ lệ chuẩn để giữ dáng thẻ
        Button btn = root.AddComponent<Button>();

        // Avatar Image (LỚP CHÌM BÊN DƯỚI)
        GameObject avatarObj = new GameObject("AvatarImage");
        RectTransform avatarRt = avatarObj.AddComponent<RectTransform>();
        avatarRt.SetParent(rootRt, false);
        avatarRt.anchorMin = new Vector2(0.04f, 0.08f); 
        avatarRt.anchorMax = new Vector2(0.96f, 0.96f); 
        avatarRt.offsetMin = Vector2.zero; 
        avatarRt.offsetMax = Vector2.zero;
        Image avatarImg = avatarObj.AddComponent<Image>();
        avatarImg.color = Color.white; 
        avatarImg.preserveAspect = false; 

        // Khung Frame (LỚP ĐÈ LÊN TRÊN AVATAR)
        GameObject frameObj = new GameObject("FrameBackground");
        RectTransform frameRt = frameObj.AddComponent<RectTransform>();
        frameRt.SetParent(rootRt, false);
        frameRt.anchorMin = Vector2.zero;
        frameRt.anchorMax = Vector2.one;
        frameRt.offsetMin = Vector2.zero;
        frameRt.offsetMax = Vector2.zero;
        Image frameImg = frameObj.AddComponent<Image>();
        Sprite frameSprite = LoadSprite("Assets/Resources/UI/HeroCardFrame2.png");
        if (frameSprite != null)
        {
            frameImg.sprite = frameSprite;
            frameImg.type = Image.Type.Simple;
        }
        else
        {
            frameImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
        btn.targetGraphic = frameImg;

        // Bảng tên kiểu cuộn giấy (Scroll NamePlate)
        GameObject namePlateObj = new GameObject("NamePlate");
        RectTransform npRt = namePlateObj.AddComponent<RectTransform>();
        npRt.SetParent(rootRt, false);
        // Neo vào khoảng hở giữa 2 vòng tròn góc dưới
        npRt.anchorMin = new Vector2(0.25f, 0.02f); 
        npRt.anchorMax = new Vector2(0.75f, 0.18f);
        npRt.offsetMin = Vector2.zero;
        npRt.offsetMax = Vector2.zero;
        Image npImg = namePlateObj.AddComponent<Image>();
        
        Sprite scrollSprite = LoadSprite("Assets/Resources/UI/ScrollNameBg.png");
        if (scrollSprite != null)
        {
            npImg.sprite = scrollSprite;
            npImg.type = Image.Type.Sliced;
        }
        else
        {
            npImg.color = new Color(0.8f, 0.7f, 0.5f, 1f);
        }

        // Text tên bên trong NamePlate
        GameObject nameTextObj = new GameObject("NameText");
        RectTransform nTxtRt = nameTextObj.AddComponent<RectTransform>();
        nTxtRt.SetParent(npRt, false);
        nTxtRt.anchorMin = Vector2.zero; nTxtRt.anchorMax = Vector2.one;
        nTxtRt.offsetMin = new Vector2(5, 5); nTxtRt.offsetMax = new Vector2(-5, -5); 
        TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        nameText.text = ""; // Đã xoá text mockup 
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontSize = 18; 
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = 10;
        nameText.fontSizeMax = 20;
        nameText.color = new Color(0.2f, 0.1f, 0.05f, 1f); 
        nameText.fontStyle = FontStyles.Bold;
        
        // --- LEVEL VÀ CLASS ---
        
        // Text Level (Góc trái dưới - Căn lại chuẩn tuyệt đối vào hình tròn trái)
        GameObject lvlTextObj = new GameObject("LevelText");
        RectTransform lvlRt = lvlTextObj.AddComponent<RectTransform>();
        lvlRt.SetParent(rootRt, false);
        // Nhích sang trái và xuống dưới để khớp vừa khít tâm hình tròn viền kim loại
        lvlRt.anchorMin = new Vector2(0.03f, 0.02f); 
        lvlRt.anchorMax = new Vector2(0.25f, 0.19f); 
        lvlRt.offsetMin = Vector2.zero; lvlRt.offsetMax = Vector2.zero;
        
        TextMeshProUGUI levelText = lvlTextObj.AddComponent<TextMeshProUGUI>();
        levelText.text = ""; // Đã xoá text mockup
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.fontSize = 18; 
        levelText.enableAutoSizing = true;
        levelText.fontSizeMin = 8;
        levelText.fontSizeMax = 18;
        levelText.color = Color.white;
        levelText.fontStyle = FontStyles.Bold;

        // Class/Profession Badge (Góc phải dưới - Căn lại chuẩn tuyệt đối vào hình tròn phải)
        GameObject classBadgeObj = new GameObject("ClassBadge");
        RectTransform clRt = classBadgeObj.AddComponent<RectTransform>();
        clRt.SetParent(rootRt, false);
        // Đối xứng tuyệt đối với bên Level mới chỉnh
        clRt.anchorMin = new Vector2(0.75f, 0.02f);    
        clRt.anchorMax = new Vector2(0.97f, 0.19f);
        clRt.offsetMin = Vector2.zero;
        clRt.offsetMax = Vector2.zero;
        Image classImg = classBadgeObj.AddComponent<Image>();
        classImg.color = new Color(1, 1, 1, 0); // Vô hình hoàn toàn (Alpha = 0)




        // Combat Power (Nằm ở trên cùng góc trái - giữ nguyên vì nó hữu ích)
        GameObject cpBadgeObj = new GameObject("CombatPowerBadge");
        RectTransform cpRt = cpBadgeObj.AddComponent<RectTransform>();
        cpRt.SetParent(rootRt, false);
        cpRt.anchorMin = new Vector2(0.1f, 0.88f);
        cpRt.anchorMax = new Vector2(0.9f, 0.98f);
        cpRt.offsetMin = Vector2.zero; cpRt.offsetMax = Vector2.zero;
        Image cpImg = cpBadgeObj.AddComponent<Image>();
        cpImg.color = new Color(0, 0, 0, 0.5f);

        GameObject cpTextObj = new GameObject("CPText");
        RectTransform cptRt = cpTextObj.AddComponent<RectTransform>();
        cptRt.SetParent(cpRt, false);
        cptRt.anchorMin = Vector2.zero; cptRt.anchorMax = Vector2.one;
        cptRt.offsetMin = Vector2.zero; cptRt.offsetMax = Vector2.zero;
        TextMeshProUGUI cpText = cpTextObj.AddComponent<TextMeshProUGUI>();
        cpText.text = "CP: 1200";
        cpText.alignment = TextAlignmentOptions.Center;
        cpText.fontSize = 18;
        cpText.color = Color.yellow;
        cpText.fontStyle = FontStyles.Bold;

        // Nối Components vào HeroCard Script
        LegendOfBlood.HeroCard script = root.AddComponent<LegendOfBlood.HeroCard>();
        SerializedObject so = new SerializedObject(script);
        
        so.FindProperty("nameText").objectReferenceValue = nameText;
        so.FindProperty("levelText").objectReferenceValue = levelText;
        so.FindProperty("combatPowerText").objectReferenceValue = cpText;
        so.FindProperty("avatarImage").objectReferenceValue = avatarImg;
        so.FindProperty("professionIcon").objectReferenceValue = classImg; 
        
        so.ApplyModifiedProperties();

        // Ghi đè lên Prefab gốc
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        Debug.Log($"Successfully completely rebuilt HeroCard Prefab with NEW FRAME at {path}");
    }
}
#endif
