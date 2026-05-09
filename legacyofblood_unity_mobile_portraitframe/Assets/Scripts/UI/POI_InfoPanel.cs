using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LegendOfBlood
{
    public class POI_InfoPanel : UIPanel, ILocalizable
    {
        [Header("Procedural Map References")]
        [SerializeField] private TextMeshProUGUI poiNameText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform mapContentContainer;

        [Header("Popups")]
        [SerializeField] private LegendOfBlood.DifficultySelectionPopup difficultyPopup;

        private POIData _currentPoiData;
        private Action _onExploreCallback;
        private ProceduralDifficulty _currentDifficulty;

        private void Awake()
        {
            PanelType = UIPanelType.POI_Info;
            
            Canvas mainCanvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (mainCanvas != null && (transform.parent == null || transform.parent.GetComponentInParent<Canvas>() == null))
            {
                transform.SetParent(mainCanvas.transform, false);
                transform.SetAsLastSibling();
            }

            if (mapContentContainer == null)
            {
                Transform innerMap = transform.Find("MainWindow/ScrollFrame/Viewport/InnerMap");
                if (innerMap != null) mapContentContainer = innerMap;
            }
        }

        protected override void Start()
        {
            base.Start();
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
            
            Button bgBtn = GetComponent<Button>();
            if (bgBtn != null) bgBtn.onClick.AddListener(ClosePanel);
        }

        private Action _onDifficultySelectedCallback;

        public void Show(POIData poiData, Action onExplore, Action onDifficultySelected = null)
        {
            _currentPoiData = poiData;
            _onExploreCallback = onExplore;
            _onDifficultySelectedCallback = onDifficultySelected ?? onExplore;

            // Generate a fallback ID if null to prevent Dictionary from throwing ArgumentNullException
            if (string.IsNullOrEmpty(_currentPoiData.poiId))
            {
                _currentPoiData.poiId = string.IsNullOrEmpty(_currentPoiData.poiName) ? System.Guid.NewGuid().ToString() : _currentPoiData.poiName;
            }

            UpdateLocalizedText();
            gameObject.SetActive(true);

            // Clean old map rendering immediately to prevent overlaps
            if (mapContentContainer != null)
            {
                foreach (Transform child in mapContentContainer) Destroy(child.gameObject);
                
                Button bgBtn = GetComponent<Button>();
                if (bgBtn != null) 
                {
                    bgBtn.onClick.RemoveAllListeners();
                    bgBtn.onClick.AddListener(ClosePanel);
                }
            }

            ShowDifficultySelector();

            EnsureCloseButtonIsVisible();
        }

        private void EnsureCloseButtonIsVisible()
        {
            if (closeButton == null)
            {
                GameObject btnObj = new GameObject("CloseButton_Runtime");
                btnObj.transform.SetParent(this.transform, false);
                btnObj.transform.SetAsLastSibling();

                RectTransform rt = btnObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                // Đặt vị trí an toàn xa góc để tránh tai thỏ (notch) trên điện thoại
                rt.anchoredPosition = new Vector2(50, -100); 
                rt.sizeDelta = new Vector2(250, 100);

                Image img = btnObj.AddComponent<Image>();
                img.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "Quay Lại";
                tmp.color = Color.white;
                tmp.fontSize = 45;
                tmp.alignment = TextAlignmentOptions.Center;
                
                RectTransform txtRt = tmp.rectTransform;
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.sizeDelta = Vector2.zero;
                txtRt.anchoredPosition = Vector2.zero;

                closeButton = btnObj.AddComponent<Button>();
                closeButton.onClick.AddListener(ClosePanel);
            }
            else
            {
                // Force bring to front and un-hide if hidden
                closeButton.gameObject.SetActive(true);
                closeButton.transform.SetAsLastSibling();
                
                if (closeButton.transform.parent != this.transform)
                {
                    closeButton.transform.SetParent(this.transform, true);
                }
            }
        }

        public void UpdateLocalizedText()
        {
            if (_currentPoiData == null) return;
            if (poiNameText != null) poiNameText.text = $"{_currentPoiData.poiName.ToUpper()}";
        }

        #region DIFFICULTY SELECTOR
        private void ShowDifficultySelector()
        {
            if (difficultyPopup == null) 
            {
                // Fallback: Tự động khởi tạo ngay tại Runtime nếu trong Inspector chưa gán
                CreateDifficultyPopupFallback();
            }
            
            difficultyPopup.Show(_currentPoiData.poiName, ProceedToMap);
        }

        private void CreateDifficultyPopupFallback()
        {
            // 1. Tắt các UI rác cũ có thể làm loạn màn hình
            foreach (Transform child in this.transform)
            {
                if (child.name != "RuntimeMapContainer" && !child.name.Contains("Popup") && child.name != "MainWindow")
                {
                    child.gameObject.SetActive(false);
                }
            }

            // 2. Tạo hình thức Popup
            GameObject diffObj = new GameObject("DifficultySelectionPopup_Runtime");
            diffObj.transform.SetParent(this.transform, false);
            diffObj.transform.SetAsLastSibling();
            RectTransform rt = diffObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;

            Image bg = diffObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.95f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(diffObj.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 60; titleTmp.alignment = TextAlignmentOptions.Center; titleTmp.color = new Color(0.9f, 0.8f, 0.3f);
            RectTransform titleRt = titleTmp.rectTransform; 
            titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f); 
            titleRt.sizeDelta = new Vector2(800, 150); titleRt.anchoredPosition = new Vector2(0, 500);

            // CPText
            GameObject cpObj = new GameObject("CPText");
            cpObj.transform.SetParent(diffObj.transform, false);
            TextMeshProUGUI cpTmp = cpObj.AddComponent<TextMeshProUGUI>();
            cpTmp.fontSize = 45; cpTmp.alignment = TextAlignmentOptions.Center; cpTmp.color = Color.white;
            RectTransform cpRt = cpTmp.rectTransform; 
            cpRt.anchorMin = new Vector2(0.5f, 0.5f); cpRt.anchorMax = new Vector2(0.5f, 0.5f); 
            cpRt.sizeDelta = new Vector2(800, 100); cpRt.anchoredPosition = new Vector2(0, 350);

            // Buttons (Cách đều tuyệt đối)
            Button btnNorm =   CreateRuntimeButton(diffObj.transform, "Btn_Normal", "Normal (Rec: 0 CP)", new Color(0.5f, 0.5f, 0.5f), 150);
            Button btnHard =   CreateRuntimeButton(diffObj.transform, "Btn_Hard", "Hard (Rec: 5,000 CP)", new Color(0.8f, 0.5f, 0f) * 0.7f, 0);
            Button btnHell =   CreateRuntimeButton(diffObj.transform, "Btn_Hell", "Hell (Rec: 15,000 CP)", new Color(0.9f, 0.1f, 0.1f) * 0.7f, -150);
            Button btnNight =  CreateRuntimeButton(diffObj.transform, "Btn_Nightmare", "Nightmare (Rec: 30,000 CP)", new Color(0.5f, 0f, 0.5f) * 0.7f, -300);

            difficultyPopup = diffObj.AddComponent<LegendOfBlood.DifficultySelectionPopup>();
            difficultyPopup.titleText = titleTmp;
            difficultyPopup.currentCPText = cpTmp;
            difficultyPopup.btnNormal = btnNorm;
            difficultyPopup.btnHard = btnHard;
            difficultyPopup.btnHell = btnHell;
            difficultyPopup.btnNightmare = btnNight;
        }

        private Button CreateRuntimeButton(Transform p, string n, string t, Color c, float yPos)
        {
            GameObject bObj = new GameObject(n);
            bObj.transform.SetParent(p, false);
            RectTransform rt = bObj.AddComponent<RectTransform>();
            // Căn chính giữa, thay đổi vị trí tuyệt đối (Pixels)
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 120);
            rt.anchoredPosition = new Vector2(0, yPos);
            
            Image img = bObj.AddComponent<Image>(); img.color = c;
            
            GameObject tObj = new GameObject("Text");
            tObj.transform.SetParent(bObj.transform, false);
            TextMeshProUGUI tmp = tObj.AddComponent<TextMeshProUGUI>();
            tmp.text = t; tmp.fontSize = 40; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
            tmp.rectTransform.anchorMin = Vector2.zero; tmp.rectTransform.anchorMax = Vector2.one; tmp.rectTransform.sizeDelta = Vector2.zero;

            return bObj.AddComponent<Button>();
        }

        private void ProceedToMap(ProceduralDifficulty difficulty)
        {
            _currentDifficulty = difficulty;
            _currentPoiData.difficultyLevel = (int)difficulty; // Normal=1, Hard=2, Hell=3, Nightmare=4
            
            // Ẩn bảng chọn độ khó
            if (difficultyPopup != null) difficultyPopup.gameObject.SetActive(false);
            
            // Gọi callback để WorldMapFixedController hiển thị SquadSelection (KHÔNG ClosePanel)
            _onDifficultySelectedCallback?.Invoke();
        }
        #endregion

        private void ClosePanel()
        {
            if (difficultyPopup != null) difficultyPopup.gameObject.SetActive(false);
            GameManager.Instance.UIManager.GoBack();
        }
    }
}
