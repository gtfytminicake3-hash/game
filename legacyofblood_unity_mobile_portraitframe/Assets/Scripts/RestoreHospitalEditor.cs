using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using LegendOfBlood;

public class RestoreHospitalEditor
{
    [MenuItem("LegendOfBlood/Fix/Restore Hospital Button")]
    public static void RestoreHospital()
    {
        var navContainer = GameObject.Find("BottomNavIcons");
        if (navContainer == null)
        {
            Debug.LogError("BottomNavIcons not found!");
            return;
        }

        Transform existingHospital = navContainer.transform.Find("Hospital");
        if (existingHospital != null)
        {
            Debug.Log("Hospital button already exists.");
            return;
        }

        Transform campBtn = navContainer.transform.Find("Camp");
        if (campBtn == null)
        {
            Debug.LogError("Camp button not found to duplicate!");
            return;
        }

        GameObject hospitalObj = GameObject.Instantiate(campBtn.gameObject, navContainer.transform);
        hospitalObj.name = "Hospital";
        
        var navBtn = hospitalObj.GetComponent<UIPanelNavButton>();
        if (navBtn != null)
        {
            navBtn.targetPanel = UIPanelType.Hospital;
        }

        var text = hospitalObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.text = "Hospital";
        }
        
        // Cố gắng đặt lại icon nếu có sprite hospital
        var icon = hospitalObj.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null)
        {
            var hospitalSprite = Resources.Load<Sprite>("hospital/hospital");
            if (hospitalSprite != null) icon.sprite = hospitalSprite;
        }

        hospitalObj.transform.SetSiblingIndex(3); // Giữa Heroes và Breeding

        EditorUtility.SetDirty(navContainer);
        Debug.Log("Restored Hospital button successfully!");
    }
}
