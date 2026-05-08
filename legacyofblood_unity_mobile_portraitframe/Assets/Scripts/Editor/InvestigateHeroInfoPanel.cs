using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class InvestigateHeroInfoPanel
{
    [MenuItem("Tools/LegendOfBlood/Investigate HeroInfo_Panel")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("HeroInfo_Panel t:GameObject");
        if (guids.Length == 0)
        {
            Debug.LogError("No HeroInfo_Panel found!");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        // Instantiate prefab ra để sửa rồi save lại
        GameObject prefab = PrefabUtility.InstantiatePrefab(prefabBase) as GameObject;

        var panel = prefab.GetComponent<LegendOfBlood.HeroInfoPanel>();
        if (panel == null)
        {
            Debug.LogError("HeroInfoPanel component not found on prefab.");
            return;
        }

        // Tải Sprite từ Resources mà chúng ta đã chuyển
        Sprite frameSprite = Resources.Load<Sprite>("HeroUI/PortraitFrame");
        Sprite bannerSprite = Resources.Load<Sprite>("HeroUI/NameBanner");

        if (frameSprite == null) Debug.LogError("Cannot find PortraitFrame sprite.");
        if (bannerSprite == null) Debug.LogError("Cannot find NameBanner sprite.");

        // Dùng reflection hoặc SerializedObject để đọc private fields vì các field này là private SerializeField
        SerializedObject so = new SerializedObject(panel);
        SerializedProperty avatarProp = so.FindProperty("heroAvatarImage");
        SerializedProperty nameProp = so.FindProperty("heroNameText");

        Image avatarImg = (Image)avatarProp.objectReferenceValue;
        TextMeshProUGUI nameTxt = (TextMeshProUGUI)nameProp.objectReferenceValue;

        if (avatarImg != null && frameSprite != null)
        {
            // Kiểm tra xem đã có Frame chưa
            Transform existingFrame = avatarImg.transform.Find("HeroPortraitFrame");
            if (existingFrame == null)
            {
                // Tạo frame làm con của avatarImg (hoặc đè lên)
                GameObject frameObj = new GameObject("HeroPortraitFrame");
                frameObj.transform.SetParent(avatarImg.transform, false);
                
                Image frameImageComponent = frameObj.AddComponent<Image>();
                frameImageComponent.sprite = frameSprite;
                frameImageComponent.raycastTarget = false;

                // Expand to fit Avatar with some padding if necessary
                RectTransform rt = frameObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(-15, -15); // Padding
                rt.offsetMax = new Vector2(15, 15);

                Debug.Log("Successfully added PortraitFrame!");
            }
            else
            {
                Debug.Log("PortraitFrame already exists!");
            }
            
            // Xóa cái mockup rác ở sau lưng avatar (nếu có) tên là "Frame", "Bg" v.v...
            Transform oldFrameBg = avatarImg.transform.parent.Find("Frame");
            if (oldFrameBg != null && oldFrameBg != avatarImg.transform) Object.DestroyImmediate(oldFrameBg.gameObject);
        }

        if (nameTxt != null && bannerSprite != null)
        {
            // Kiểm tra xem đã có Banner chưa
            Transform existingBanner = nameTxt.transform.parent.Find("HeroNameBanner");
            if (existingBanner == null)
            {
                // Tạo Banner làm sibling nằm TRONG parent của NameText và DƯỚI NameText
                GameObject bannerObj = new GameObject("HeroNameBanner");
                bannerObj.transform.SetParent(nameTxt.transform.parent, false);
                // Đẩy lên trước nameTxt để render ở dưới
                bannerObj.transform.SetSiblingIndex(nameTxt.transform.GetSiblingIndex());

                Image bannerImageComponent = bannerObj.AddComponent<Image>();
                bannerImageComponent.sprite = bannerSprite;
                bannerImageComponent.type = Image.Type.Sliced; // Nếu nó có viền, ta có thể set Sliced tùy chỉnh
                bannerImageComponent.raycastTarget = false;

                RectTransform bannerRt = bannerObj.GetComponent<RectTransform>();
                
                // Copy transform settings from nameTxt
                RectTransform txtRt = nameTxt.rectTransform;
                bannerRt.anchorMin = txtRt.anchorMin;
                bannerRt.anchorMax = txtRt.anchorMax;
                bannerRt.pivot = txtRt.pivot;
                bannerRt.anchoredPosition = txtRt.anchoredPosition;
                // Phóng to banner to hơn text một xíu
                bannerRt.sizeDelta = new Vector2(txtRt.sizeDelta.x + 80, txtRt.sizeDelta.y + 40);

                Debug.Log("Successfully added NameBanner!");
            }
            else
            {
                Debug.Log("NameBanner already exists!");
            }
            
            // Text color có thể cần đổi sang đậm hơn (đen / nâu) nếu cuộn giấy màu sáng
            nameTxt.color = new Color(0.2f, 0.1f, 0.05f, 1f); // Nâu đậm
        }

        // Apply changes to prefab
        PrefabUtility.SaveAsPrefabAsset(prefab, path);
        Object.DestroyImmediate(prefab);
        
        Debug.Log("Prefab modifications saved successfully.");
    }
}
