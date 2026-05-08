using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using LegendOfBlood;
using TMPro;

public class GenerateEquipmentUpgradePanel : MonoBehaviour
{
    [MenuItem("LegendOfBlood/Fix/Generate Equipment Upgrade Panel")]
    public static void GeneratePanel()
    {
        // Tìm Canvas chính trong Scene
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas nào trong Scene để đặt Panel!");
            return;
        }

        // Tạo Panel gốc
        GameObject panelObj = new GameObject("Panel_EquipmentUpgrade", typeof(RectTransform), typeof(Image), typeof(EquipmentUpgradePanel));
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // Background (màu xám đen mờ)
        Image bgImg = panelObj.GetComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.8f);

        // Tạo Container cho nội dung
        GameObject containerObj = new GameObject("Container", typeof(RectTransform), typeof(Image));
        containerObj.transform.SetParent(panelObj.transform, false);
        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(800, 1000);
        containerRect.anchoredPosition = Vector2.zero;
        containerObj.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Tiêu đề
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(containerObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(700, 100);
        titleRect.anchoredPosition = new Vector2(0, -70);
        TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        titleTxt.text = "CHỌN RÁC ĐỂ NÂNG CẤP";
        titleTxt.fontSize = 60;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = Color.white;

        // Tạo nhóm Toggle
        GameObject toggleGroupObj = new GameObject("ToggleGroup", typeof(RectTransform), typeof(VerticalLayoutGroup));
        toggleGroupObj.transform.SetParent(containerObj.transform, false);
        RectTransform tgRect = toggleGroupObj.GetComponent<RectTransform>();
        tgRect.anchorMin = new Vector2(0.5f, 0.5f);
        tgRect.anchorMax = new Vector2(0.5f, 0.5f);
        tgRect.sizeDelta = new Vector2(600, 500);
        tgRect.anchoredPosition = new Vector2(0, 50);
        VerticalLayoutGroup vlg = toggleGroupObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;

        EquipmentUpgradePanel panelScript = panelObj.GetComponent<EquipmentUpgradePanel>();

        // Tạo 5 Toggles
        panelScript.toggleTierS = CreateTierToggle("S", toggleGroupObj.transform);
        panelScript.toggleTierA = CreateTierToggle("A", toggleGroupObj.transform);
        panelScript.toggleTierB = CreateTierToggle("B", toggleGroupObj.transform);
        panelScript.toggleTierC = CreateTierToggle("C", toggleGroupObj.transform);
        panelScript.toggleTierD = CreateTierToggle("D", toggleGroupObj.transform);

        // Nút Upgrade
        GameObject upgradeBtnObj = new GameObject("ConfirmUpgradeButton", typeof(RectTransform), typeof(Image), typeof(Button));
        upgradeBtnObj.transform.SetParent(containerObj.transform, false);
        RectTransform upgBtnRect = upgradeBtnObj.GetComponent<RectTransform>();
        upgBtnRect.anchorMin = new Vector2(0.5f, 0f);
        upgBtnRect.anchorMax = new Vector2(0.5f, 0f);
        upgBtnRect.sizeDelta = new Vector2(400, 120);
        upgBtnRect.anchoredPosition = new Vector2(0, 250);
        upgradeBtnObj.GetComponent<Image>().color = new Color(1f, 0.6f, 0f);
        
        GameObject upgTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        upgTextObj.transform.SetParent(upgradeBtnObj.transform, false);
        RectTransform utRect = upgTextObj.GetComponent<RectTransform>();
        utRect.anchorMin = Vector2.zero; utRect.anchorMax = Vector2.one; utRect.sizeDelta = Vector2.zero; utRect.anchoredPosition = Vector2.zero;
        TextMeshProUGUI utTxt = upgTextObj.GetComponent<TextMeshProUGUI>();
        utTxt.text = "NÂNG CẤP";
        utTxt.fontSize = 50; utTxt.alignment = TextAlignmentOptions.Center; utTxt.color = Color.white;
        
        panelScript.confirmUpgradeButton = upgradeBtnObj.GetComponent<Button>();

        // Nút Close
        GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(containerObj.transform, false);
        RectTransform clsBtnRect = closeBtnObj.GetComponent<RectTransform>();
        clsBtnRect.anchorMin = new Vector2(0.5f, 0f);
        clsBtnRect.anchorMax = new Vector2(0.5f, 0f);
        clsBtnRect.sizeDelta = new Vector2(400, 120);
        clsBtnRect.anchoredPosition = new Vector2(0, 100);
        closeBtnObj.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);

        GameObject clsTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        clsTextObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform ctRect = clsTextObj.GetComponent<RectTransform>();
        ctRect.anchorMin = Vector2.zero; ctRect.anchorMax = Vector2.one; ctRect.sizeDelta = Vector2.zero; ctRect.anchoredPosition = Vector2.zero;
        TextMeshProUGUI ctTxt = clsTextObj.GetComponent<TextMeshProUGUI>();
        ctTxt.text = "ĐÓNG";
        ctTxt.fontSize = 50; ctTxt.alignment = TextAlignmentOptions.Center; ctTxt.color = Color.white;

        panelScript.closeButton = closeBtnObj.GetComponent<Button>();

        // Set mặc định ẩn đi
        panelObj.SetActive(false);

        // Lưu thành Prefab
        string prefabPath = "Assets/Resources/UI/Panel_EquipmentUpgrade.prefab";
        System.IO.Directory.CreateDirectory("Assets/Resources/UI");
        PrefabUtility.SaveAsPrefabAssetAndConnect(panelObj, prefabPath, InteractionMode.UserAction);

        Debug.Log("Đã tạo thành công Panel_EquipmentUpgrade trong Scene!");
    }

    private static Toggle CreateTierToggle(string tierStr, Transform parent)
    {
        GameObject toggleObj = new GameObject($"Toggle_{tierStr}", typeof(RectTransform), typeof(Toggle));
        toggleObj.transform.SetParent(parent, false);
        
        RectTransform rect = toggleObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 80);

        // Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        bgRect.sizeDelta = new Vector2(50, 50);
        bgRect.anchoredPosition = new Vector2(50, 0);

        // Checkmark
        GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkObj.transform.SetParent(bgObj.transform, false);
        RectTransform chkRect = checkObj.GetComponent<RectTransform>();
        chkRect.anchorMin = Vector2.zero; chkRect.anchorMax = Vector2.one; chkRect.sizeDelta = Vector2.zero; chkRect.anchoredPosition = Vector2.zero;
        checkObj.GetComponent<Image>().color = Color.green;

        // Text
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(toggleObj.transform, false);
        RectTransform lblRect = labelObj.GetComponent<RectTransform>();
        lblRect.anchorMin = new Vector2(0, 0.5f);
        lblRect.anchorMax = new Vector2(1, 0.5f);
        lblRect.sizeDelta = new Vector2(0, 80);
        lblRect.anchoredPosition = new Vector2(120, 0);
        
        TextMeshProUGUI txt = labelObj.GetComponent<TextMeshProUGUI>();
        txt.text = $"Sử dụng trang bị cấp {tierStr} làm phôi";
        txt.fontSize = 40;
        txt.alignment = TextAlignmentOptions.Left;
        txt.color = Color.white;

        Toggle toggle = toggleObj.GetComponent<Toggle>();
        toggle.targetGraphic = bgObj.GetComponent<Image>();
        toggle.graphic = checkObj.GetComponent<Image>();

        return toggle;
    }
}
