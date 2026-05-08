#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood.UI;

namespace LegendOfBlood.Editor
{
    public class KingGodPassUIBuilder : EditorWindow
    {
        [MenuItem("UI Tools/Generate KingGodPass Panel")]
        public static void GenerateUI()
        {
            // Tìm Canvas cha
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Không tìm thấy Canvas nào trong Scene. Hãy tạo Canvas trước!");
                return;
            }

            // Xóa panel cũ
            KingGodPassPanel oldPanel = Object.FindAnyObjectByType<KingGodPassPanel>(FindObjectsInactive.Include);
            if (oldPanel != null)
            {
                Undo.DestroyObjectImmediate(oldPanel.gameObject);
            }

            // Tạo Parent Panel
            GameObject panelObj = new GameObject("KingGodPassPanel");
            panelObj.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            panelObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            KingGodPassPanel panelScript = panelObj.AddComponent<KingGodPassPanel>();
            panelScript.PanelType = UIPanelType.KingGodPass;

            // Header Title
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(panelObj.transform, false);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0, 200);

            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(headerObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "KING GOD PASS";
            titleText.fontSize = 80;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.8f, 0.2f); // Gold

            // Nút Close
            GameObject closeBtnObj = new GameObject("CloseBtn");
            closeBtnObj.transform.SetParent(headerObj.transform, false);
            RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.pivot = new Vector2(1, 0.5f);
            closeRect.anchoredPosition = new Vector2(-50, 0);
            closeRect.sizeDelta = new Vector2(100, 100);
            closeBtnObj.AddComponent<Image>().color = Color.red;
            closeBtnObj.AddComponent<Button>();
            
            UIPanelNavButton closeNav = closeBtnObj.AddComponent<UIPanelNavButton>();
            closeNav.isBackButton = true;

            GameObject closeTxtObj = new GameObject("Text");
            closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform ctRect = closeTxtObj.AddComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
            closeTxt.text = "X";
            closeTxt.fontSize = 60;
            closeTxt.alignment = TextAlignmentOptions.Center;
            closeTxt.color = Color.white;

            // Tạo Content ScrollView
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(panelObj.transform, false);
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(50, 250); // Padding Bottom & Left
            scrollRect.offsetMax = new Vector2(-50, -200); // Padding Top & Right
            scrollObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);
            ScrollRect scrollRectComp = scrollObj.AddComponent<ScrollRect>();
            scrollRectComp.horizontal = false;
            scrollRectComp.vertical = true;

            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportObj.AddComponent<Image>().color = new Color(1f, 1f, 1f, 1f); // Transparent mask (handled by showMaskGraphic=false)
            viewportObj.AddComponent<Mask>().showMaskGraphic = false;

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 20;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRectComp.viewport = viewportRect;
            scrollRectComp.content = contentRect;

            // Generate PassRow Prefab/Template (just build a disabled template under panel)
            GameObject templateRow = GenerateRowTemplate(panelObj.transform);
            panelScript.rowPrefab = templateRow;

            // Footer (Nút Mua Premium)
            GameObject footerObj = new GameObject("Footer");
            footerObj.transform.SetParent(panelObj.transform, false);
            RectTransform footerRect = footerObj.AddComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0, 0);
            footerRect.anchorMax = new Vector2(1, 0);
            footerRect.pivot = new Vector2(0.5f, 0);
            footerRect.anchoredPosition = Vector2.zero;
            footerRect.sizeDelta = new Vector2(0, 200);

            GameObject upgradeBtnObj = new GameObject("UpgradePremiumBtn");
            upgradeBtnObj.transform.SetParent(footerObj.transform, false);
            RectTransform upgRect = upgradeBtnObj.AddComponent<RectTransform>();
            upgRect.anchorMin = new Vector2(0.5f, 0.5f);
            upgRect.anchorMax = new Vector2(0.5f, 0.5f);
            upgRect.pivot = new Vector2(0.5f, 0.5f);
            upgRect.sizeDelta = new Vector2(500, 120);
            upgRect.anchoredPosition = Vector2.zero;
            upgradeBtnObj.AddComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f); // Red
            Button upgBtn = upgradeBtnObj.AddComponent<Button>();

            GameObject upgTxtObj = new GameObject("Text");
            upgTxtObj.transform.SetParent(upgradeBtnObj.transform, false);
            RectTransform upgTxtRect = upgTxtObj.AddComponent<RectTransform>();
            upgTxtRect.anchorMin = Vector2.zero;
            upgTxtRect.anchorMax = Vector2.one;
            upgTxtRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI upgTxt = upgTxtObj.AddComponent<TextMeshProUGUI>();
            upgTxt.text = "UNLOCK PREMIUM";
            upgTxt.fontSize = 50;
            upgTxt.alignment = TextAlignmentOptions.Center;
            upgTxt.color = Color.white;

            panelScript.contentTransform = contentRect;
            panelScript.upgradePremiumBtn = upgBtn;

            panelObj.SetActive(false); // Default hidden
            Selection.activeGameObject = panelObj;
            Debug.Log("🎉 Đã tạo KingGodPassPanel thành công! Script nằm trong UI Tools/Generate KingGodPass Panel");
        }

        private static GameObject GenerateRowTemplate(Transform parent)
        {
            GameObject rowObj = new GameObject("PassRowTemplate");
            rowObj.transform.SetParent(parent, false);
            RectTransform rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 250);
            rowObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            PassRewardRowUI rowScript = rowObj.AddComponent<PassRewardRowUI>();

            // Left (Free)
            GameObject freeGroup = new GameObject("FreeReward");
            freeGroup.transform.SetParent(rowObj.transform, false);
            RectTransform frRect = freeGroup.AddComponent<RectTransform>();
            frRect.anchorMin = new Vector2(0, 0);
            frRect.anchorMax = new Vector2(0.35f, 1);
            frRect.offsetMin = Vector2.zero;
            frRect.offsetMax = Vector2.zero;

            GameObject frIcon = new GameObject("Icon");
            frIcon.transform.SetParent(freeGroup.transform, false);
            RectTransform frIconRect = frIcon.AddComponent<RectTransform>();
            frIconRect.anchorMin = new Vector2(0.5f, 0.6f);
            frIconRect.anchorMax = new Vector2(0.5f, 0.6f);
            frIconRect.sizeDelta = new Vector2(100, 100);
            frIconRect.anchoredPosition = Vector2.zero;
            Image freeImg = frIcon.AddComponent<Image>();
            freeImg.color = Color.yellow; // Dummy

            GameObject frText = new GameObject("Amount");
            frText.transform.SetParent(freeGroup.transform, false);
            RectTransform frTxtRect = frText.AddComponent<RectTransform>();
            frTxtRect.anchorMin = new Vector2(0, 0.2f);
            frTxtRect.anchorMax = new Vector2(1, 0.4f);
            frTxtRect.offsetMin = Vector2.zero;
            frTxtRect.offsetMax = Vector2.zero;
            TextMeshProUGUI freeTxt = frText.AddComponent<TextMeshProUGUI>();
            freeTxt.text = "100";
            freeTxt.alignment = TextAlignmentOptions.Center;

            GameObject frBtnObj = new GameObject("ClaimBtn");
            frBtnObj.transform.SetParent(freeGroup.transform, false);
            RectTransform frBtnRect = frBtnObj.AddComponent<RectTransform>();
            frBtnRect.anchorMin = new Vector2(0.1f, 0.05f);
            frBtnRect.anchorMax = new Vector2(0.9f, 0.2f);
            frBtnRect.offsetMin = Vector2.zero;
            frBtnRect.offsetMax = Vector2.zero;
            frBtnObj.AddComponent<Image>().color = Color.green;
            Button freeBtn = frBtnObj.AddComponent<Button>();

            // Middle (Level & Progress)
            GameObject midGroup = new GameObject("LevelInfo");
            midGroup.transform.SetParent(rowObj.transform, false);
            RectTransform midRect = midGroup.AddComponent<RectTransform>();
            midRect.anchorMin = new Vector2(0.35f, 0);
            midRect.anchorMax = new Vector2(0.65f, 1);
            midRect.offsetMin = Vector2.zero;
            midRect.offsetMax = Vector2.zero;

            GameObject midProgObj = new GameObject("ProgressBg");
            midProgObj.transform.SetParent(midGroup.transform, false);
            RectTransform midPbgRect = midProgObj.AddComponent<RectTransform>();
            midPbgRect.anchorMin = new Vector2(0.45f, 0);
            midPbgRect.anchorMax = new Vector2(0.55f, 1);
            midPbgRect.offsetMin = Vector2.zero;
            midPbgRect.offsetMax = Vector2.zero;
            midProgObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(midProgObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = Color.yellow;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Vertical;

            GameObject lvlTxtObj = new GameObject("LVL");
            lvlTxtObj.transform.SetParent(midGroup.transform, false);
            RectTransform lvlRect = lvlTxtObj.AddComponent<RectTransform>();
            lvlRect.anchorMin = new Vector2(0, 0.4f);
            lvlRect.anchorMax = new Vector2(1, 0.6f);
            lvlRect.offsetMin = Vector2.zero;
            lvlRect.offsetMax = Vector2.zero;
            TextMeshProUGUI lvlTxt = lvlTxtObj.AddComponent<TextMeshProUGUI>();
            lvlTxt.text = "10";
            lvlTxt.fontSize = 50;
            lvlTxt.fontStyle = FontStyles.Bold;
            lvlTxt.alignment = TextAlignmentOptions.Center;

            // Right (Premium)
            GameObject premGroup = new GameObject("PremiumReward");
            premGroup.transform.SetParent(rowObj.transform, false);
            RectTransform prRect = premGroup.AddComponent<RectTransform>();
            prRect.anchorMin = new Vector2(0.65f, 0);
            prRect.anchorMax = new Vector2(1, 1);
            prRect.offsetMin = Vector2.zero;
            prRect.offsetMax = Vector2.zero;

            GameObject prIcon = new GameObject("Icon");
            prIcon.transform.SetParent(premGroup.transform, false);
            RectTransform prIconRect = prIcon.AddComponent<RectTransform>();
            prIconRect.anchorMin = new Vector2(0.5f, 0.6f);
            prIconRect.anchorMax = new Vector2(0.5f, 0.6f);
            prIconRect.sizeDelta = new Vector2(100, 100);
            prIconRect.anchoredPosition = Vector2.zero;
            Image premImg = prIcon.AddComponent<Image>();
            premImg.color = Color.cyan;

            GameObject prText = new GameObject("Amount");
            prText.transform.SetParent(premGroup.transform, false);
            RectTransform prTxtRect = prText.AddComponent<RectTransform>();
            prTxtRect.anchorMin = new Vector2(0, 0.2f);
            prTxtRect.anchorMax = new Vector2(1, 0.4f);
            prTxtRect.offsetMin = Vector2.zero;
            prTxtRect.offsetMax = Vector2.zero;
            TextMeshProUGUI premTxt = prText.AddComponent<TextMeshProUGUI>();
            premTxt.text = "VIP";
            premTxt.alignment = TextAlignmentOptions.Center;

            GameObject prBtnObj = new GameObject("ClaimBtn");
            prBtnObj.transform.SetParent(premGroup.transform, false);
            RectTransform prBtnRect = prBtnObj.AddComponent<RectTransform>();
            prBtnRect.anchorMin = new Vector2(0.1f, 0.05f);
            prBtnRect.anchorMax = new Vector2(0.9f, 0.2f);
            prBtnRect.offsetMin = Vector2.zero;
            prBtnRect.offsetMax = Vector2.zero;
            prBtnObj.AddComponent<Image>().color = new Color(1f, 0.5f, 0f);
            Button premBtn = prBtnObj.AddComponent<Button>();

            // Gắn Refernces
            rowScript.levelText = lvlTxt;
            rowScript.progressFill = fillImg;
            rowScript.freeIcon = freeImg;
            rowScript.freeAmountText = freeTxt;
            rowScript.freeClaimBtn = freeBtn;
            rowScript.premiumIcon = premImg;
            rowScript.premiumAmountText = premTxt;
            rowScript.premiumClaimBtn = premBtn;

            rowObj.SetActive(false); // Template must be inactive
            return rowObj;
        }
    }
}
#endif
