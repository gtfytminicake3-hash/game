using LegendOfBlood;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement; // Remove this if unused completely, but keeping it is fine.
using UnityEngine.UI;
using TMPro;

public class Bootloader : MonoBehaviour
{
    [Header("Core Systems")]
    [SerializeField] private GameObject coreSystemsPrefab;

    [Header("UI References")]
    [SerializeField] private GameObject tapToStartGroup;
    [SerializeField] private GameObject loadingScreenGroup;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tapToStartText;
    
    private void Start()
    {
        if (tapToStartText != null && global::LocalizationSystem.IsReady) tapToStartText.text = global::LocalizationSystem.GetText("bootloader_tap_to_start");
        // Tự động gọi hàm chạy Loading, do Game đã đổi sang 1 Scene
        StartLoading();
    }

    /// <summary>
    /// Hàm này sẽ được gọi bởi nút "TapToStart_Group".
    /// </summary>
    public void StartLoading()
    {
        // Ẩn "Tap to Start", hiện màn hình loading
        if (tapToStartGroup != null) tapToStartGroup.SetActive(false);
        if (loadingScreenGroup != null) loadingScreenGroup.SetActive(true);

        // Bắt đầu quá trình tải bất đồng bộ
        StartCoroutine(LoadMainSceneAsync());
    }

    // --- UI Dynamic Variables ---
    private GameObject _dynamicContainer;
    private Image _dynamicBarFill;
    private RectTransform _dynamicPercentRect;
    private TextMeshProUGUI _dynamicPercentText;
    private TextMeshProUGUI _dynamicLoreText;

    private void Awake()
    {
        ApplyResponsiveLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        StretchRect(GetComponent<RectTransform>());
        StretchRect(tapToStartGroup != null ? tapToStartGroup.GetComponent<RectTransform>() : null);
        StretchRect(loadingScreenGroup != null ? loadingScreenGroup.GetComponent<RectTransform>() : null);

        Transform bg = transform.Find("BootloaderBackgroundImage");
        if (bg != null) StretchRect(bg.GetComponent<RectTransform>());

        Canvas canvas = GetComponentInParent<Canvas>();
        CanvasScaler scaler = canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private void StretchRect(RectTransform rect)
    {
        if (rect == null) return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    /// <summary>
    /// Coroutine để tải scene một cách bất đồng bộ và cập nhật thanh loading.
    /// </summary>
    private IEnumerator LoadMainSceneAsync()
    {
        // 1. Dựng UI Thanh Loading tự động mang âm hưởng Dark Fantasy
        SetupDynamicProgressBar();

        float targetProgress = 0f;
        float displayedProgress = 0f;
        float fillSpeed = 0.5f; // Tốc độ chạy giả lập

        // 2. Bắt đầu Fake Loading (Phase 1: Init - 20%)
        targetProgress = 0.2f;
        UpdateLoadingText("bootloader_init", "Awakening ancient bloodlines...");
        
        while (displayedProgress < targetProgress)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, fillSpeed * Time.deltaTime);
            UpdateBarUI(displayedProgress);
            yield return null;
        }

        // Tạo ra hệ thống lõi nếu chưa có
        if (GameManager.Instance == null && coreSystemsPrefab != null)
        {
            Instantiate(coreSystemsPrefab);
        }

        // 3. Phase 2: Load Localization & Database (50%)
        targetProgress = 0.5f;
        UpdateLoadingText("bootloader_load_data", "Forging the battleground...");

        while (displayedProgress < targetProgress)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, fillSpeed * Time.deltaTime);
            UpdateBarUI(displayedProgress);
            yield return null;
        }

        // Chờ hệ thống ngôn ngữ sẵn sàng
        yield return new WaitUntil(() => global::LocalizationSystem.IsReady);

        // 4. Phase 3: Nạp Save Player (80%)
        targetProgress = 0.8f;
        UpdateLoadingText("bootloader_load_player", "Summoning legends...");

        // Cài đặt Timeout an toàn (5s tối đa)
        float maxWaitTime = 5.0f;
        float currentWaitTime = 0f;

        while (currentWaitTime < maxWaitTime)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, (fillSpeed * 0.5f) * Time.deltaTime);
            UpdateBarUI(displayedProgress);

            if (GameManager.Instance != null && GameManager.Instance.DataManager != null && GameManager.Instance.DataManager.Player != null)
            {
                break; // Thoát lặp khi nạp xong The Player
            }

            currentWaitTime += Time.deltaTime;
            yield return null;
        }

        if (currentWaitTime >= maxWaitTime)
        {
            Debug.LogError("[Bootloader] QUÁ THỜI GIAN LOAD DATA! Cưỡng ép khởi tạo dữ liệu mớI.");
            if (GameManager.Instance?.DataManager != null && GameManager.Instance.DataManager.Player == null)
            {
               GameManager.Instance.DataManager.InitializeDataManager();
            }
        }

        // 5. Phase 4: Sẵn sàng (100%)
        targetProgress = 1.0f;
        fillSpeed = 1.5f; // Tua nhanh khúc cuối
        UpdateLoadingText("bootloader_ready", "Enter the abyss...");

        while (displayedProgress < targetProgress)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, fillSpeed * Time.deltaTime);
            UpdateBarUI(displayedProgress);
            yield return null;
        }

        // Đợi một khoảng ngắn mượt UI để người xem thấy số 100%
        yield return new WaitForSeconds(0.4f);

        // Kích hoạt Game State sang Playing nếu bị găm ở Initializing
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
             GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
        }

        // 5. Mở Panel Làng Chính
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.MainScreen);
        }

        // 6. Đóng Bootloader & Dọn dẹp Bar
        if (_dynamicContainer != null) Destroy(_dynamicContainer);
        if (loadingScreenGroup != null) loadingScreenGroup.SetActive(false);
        if (tapToStartGroup != null) tapToStartGroup.SetActive(false);
        
        Image bg = GetComponent<Image>();
        if (bg != null) bg.enabled = false;
    }

    private void UpdateBarUI(float progress)
    {
        if (loadingSlider != null) loadingSlider.value = progress;
        if (_dynamicBarFill != null) _dynamicBarFill.fillAmount = progress;

        int percent = Mathf.RoundToInt(progress * 100f);
        if (_dynamicPercentText != null)
        {
            _dynamicPercentText.text = $"{percent}%";
        }

        // 3. Di chuyển khối giọt máu dọc theo rãnh ngang
        if (_dynamicPercentRect != null && _dynamicBarFill != null)
        {
            // Neo Rect tự trượt góc phải (Trượt theo width của Fill)
            float currentProgress = _dynamicBarFill.fillAmount;
            _dynamicPercentRect.anchorMin = new Vector2(currentProgress, 0f);
            _dynamicPercentRect.anchorMax = new Vector2(currentProgress, 1f);
        }
    }

    private void UpdateLoadingText(string localizationKey, string fallbackText)
    {
        string textToShow = global::LocalizationSystem.IsReady ? global::LocalizationSystem.GetText(localizationKey) : fallbackText;
        if (loadingText != null) loadingText.text = textToShow;
    }

    private void SetupDynamicProgressBar()
    {
        ApplyResponsiveLayout();
        if (loadingSlider != null) loadingSlider.gameObject.SetActive(false);
        if (loadingText != null) loadingText.gameObject.SetActive(false);

        if (Camera.main != null) 
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.05f, 0.02f, 0.02f, 1f); 
        }

        // --- BACKGROUND HÌNH ẢNH ---
        Transform bgT = transform.Find("BootloaderBackgroundImage");
        GameObject bgImageObj = bgT != null ? bgT.gameObject : new GameObject("BootloaderBackgroundImage");
        if (bgT == null) bgImageObj.transform.SetParent(transform, false);
        
        var imgComp = bgImageObj.GetComponent<Image>() ?? bgImageObj.AddComponent<Image>();
        imgComp.color = Color.white;
        if (imgComp.sprite == null)
        {
            Sprite[] bgSprites = System.Linq.Enumerable.OrderBy(Resources.LoadAll<Sprite>("UI/BootloaderBackdrop_Frames"), s => s.name).ToArray();

            // Nếu không có frames (do chưa set Texture Type, fallback lại ảnh cũ)
            if (bgSprites == null || bgSprites.Length == 0)
                bgSprites = Resources.LoadAll<Sprite>("UI/BootloaderBackdrop");

            if (bgSprites != null && bgSprites.Length > 0)
            {
                imgComp.sprite = bgSprites[0];
                imgComp.preserveAspect = false; 
                var aspectRatioFitter = bgImageObj.GetComponent<AspectRatioFitter>() ?? bgImageObj.AddComponent<AspectRatioFitter>();
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                aspectRatioFitter.aspectRatio = (float)bgSprites[0].texture.width / bgSprites[0].texture.height;

                // Simple GIF Animation Support (if it has multiple frames)
                if (bgSprites.Length > 1)
                {
                    StartCoroutine(AnimateBackground(imgComp, bgSprites));
                }
            }
        }
        bgImageObj.transform.SetAsFirstSibling();
        
        // Cố định RectTransform cho Background nếu là tạo mới
        StretchRect(bgImageObj.GetComponent<RectTransform>());

        // --- MASTER CONTAINER ---
        Transform containerParent = loadingScreenGroup != null ? loadingScreenGroup.transform : transform;
        Transform containerT = containerParent.Find("LegendOfBlood_UI_Container");
        _dynamicContainer = containerT != null ? containerT.gameObject : new GameObject("LegendOfBlood_UI_Container");
        if (containerT == null)
        {
            _dynamicContainer.transform.SetParent(containerParent, false);
            var rect = _dynamicContainer.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.08f, 0.035f); 
            rect.anchorMax = new Vector2(0.92f, 0.18f); 
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // --- KHUNG LOADING (CHASSIS) ---
        Transform chassisT = _dynamicContainer.transform.Find("LoadingBar_Chassis");
        GameObject barChassisObj = chassisT != null ? chassisT.gameObject : new GameObject("LoadingBar_Chassis");
        if (chassisT == null)
        {
            barChassisObj.transform.SetParent(_dynamicContainer.transform, false);
            var rect = barChassisObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(0, 50);
            rect.anchoredPosition = Vector2.zero;
        }

        var chassisImg = barChassisObj.GetComponent<Image>() ?? barChassisObj.AddComponent<Image>();
        chassisImg.color = Color.white;
        if (chassisImg.sprite == null)
        {
            Sprite chassisSprite = Resources.Load<Sprite>("UI/LoadingBarChassis");
            if (chassisSprite == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("UI/LoadingBarChassis");
                if (tex != null) chassisSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            if (chassisSprite != null)
            {
                chassisImg.sprite = chassisSprite;
                chassisImg.type = Image.Type.Simple;
                chassisImg.preserveAspect = true;
            }
        }

        // --- LÕI MÁU (FILL) ---
        Transform fillT = barChassisObj.transform.Find("Fill_Blood");
        GameObject fillObj = fillT != null ? fillT.gameObject : new GameObject("Fill_Blood");
        if (fillT == null)
        {
            fillObj.transform.SetParent(barChassisObj.transform, false);
            var rect = fillObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20, 15);
            rect.offsetMax = new Vector2(-20, -15);
            fillObj.transform.SetAsFirstSibling();
        }

        _dynamicBarFill = fillObj.GetComponent<Image>() ?? fillObj.AddComponent<Image>();
        _dynamicBarFill.color = Color.white;
        _dynamicBarFill.type = Image.Type.Filled;
        _dynamicBarFill.fillMethod = Image.FillMethod.Horizontal;
        _dynamicBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        
        if (_dynamicBarFill.sprite == null)
        {
            Sprite liquidSprite = Resources.Load<Sprite>("UI/liquid_loadingbar_rmbg");
            if (liquidSprite == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("UI/liquid_loadingbar_rmbg");
                if (tex != null) liquidSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            if (liquidSprite != null) _dynamicBarFill.sprite = liquidSprite;
        }

        // --- CỤM PHẦN TRĂM (TRACKER) ---
        Transform trackerT = barChassisObj.transform.Find("TrackerPercent");
        GameObject trackerObj = trackerT != null ? trackerT.gameObject : new GameObject("TrackerPercent");
        if (trackerT == null)
        {
            trackerObj.transform.SetParent(barChassisObj.transform, false);
            _dynamicPercentRect = trackerObj.AddComponent<RectTransform>();
            _dynamicPercentRect.anchorMin = new Vector2(0f, 0f);
            _dynamicPercentRect.anchorMax = new Vector2(0f, 1f);
            _dynamicPercentRect.sizeDelta = Vector2.zero;
            _dynamicPercentRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            _dynamicPercentRect = trackerObj.GetComponent<RectTransform>();
        }

        // --- BACKGROUND PHẦN TRĂM ---
        Transform percentBgT = trackerObj.transform.Find("Percent_BG");
        GameObject bgObj = percentBgT != null ? percentBgT.gameObject : new GameObject("Percent_BG");
        if (percentBgT == null)
        {
            bgObj.transform.SetParent(trackerObj.transform, false);
            var rect = bgObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(120, 120);
            rect.anchoredPosition = new Vector2(0, 40);
        }

        var pBgImg = bgObj.GetComponent<Image>() ?? bgObj.AddComponent<Image>();
        pBgImg.color = Color.white;
        if (pBgImg.sprite == null)
        {
            Sprite sp = Resources.Load<Sprite>("UI/blood_percent_rmbg");
            if (sp == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("UI/blood_percent_rmbg");
                if (tex != null) sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            if (sp != null) { pBgImg.sprite = sp; pBgImg.preserveAspect = true; }
        }

        // --- TEXT PHẦN TRĂM ---
        Transform textT = trackerObj.transform.Find("Text_Percent");
        GameObject textObj = textT != null ? textT.gameObject : new GameObject("Text_Percent");
        if (textT == null)
        {
            textObj.transform.SetParent(trackerObj.transform, false);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(100, 40);
            rect.anchoredPosition = new Vector2(0, 40);
        }

        _dynamicPercentText = textObj.GetComponent<TextMeshProUGUI>() ?? textObj.AddComponent<TextMeshProUGUI>();
        if (textT == null)
        {
            if (loadingText != null) _dynamicPercentText.font = loadingText.font;
            _dynamicPercentText.fontSize = 24;
            _dynamicPercentText.color = Color.white;
            _dynamicPercentText.alignment = TextAlignmentOptions.Center;
            _dynamicPercentText.fontStyle = FontStyles.Bold;
            if (textObj.GetComponent<UnityEngine.UI.Outline>() == null)
                textObj.AddComponent<UnityEngine.UI.Outline>().effectColor = Color.black;
        }
        _dynamicPercentText.text = "0%";
    }

    private IEnumerator AnimateBackground(Image imgComp, Sprite[] frames)
    {
        int index = 0;
        float frameRate = 0.1f; // 10 FPS as default
        while (imgComp != null && imgComp.gameObject.activeInHierarchy)
        {
            imgComp.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSecondsRealtime(frameRate);
        }
    }
}
