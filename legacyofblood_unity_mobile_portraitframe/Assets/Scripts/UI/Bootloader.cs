using LegendOfBlood;
using System.Collections;
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
    
    private void Start()
    {
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

    /// <summary>
    /// Coroutine để tải scene một cách bất đồng bộ và cập nhật thanh loading.
    /// </summary>
    private IEnumerator LoadMainSceneAsync()
    {
        // 1. Khởi tạo ảo cho thanh loading để cho người chơi thấy bắt đầu chạy
        loadingSlider.value = 0.1f;
        loadingText.text = "Initializing Core Systems...";

        // 2. Tạo ra hệ thống lõi nếu chưa có
        if (GameManager.Instance == null && coreSystemsPrefab != null)
        {
            Instantiate(coreSystemsPrefab);
        }

        // Đợi 1 frame
        yield return null;

        loadingSlider.value = 0.4f;
        loadingText.text = "Loading Localization & Database...";

        // 3. Đợi cho đến khi hệ thống dịch thuật sẵn sàng
        yield return new WaitUntil(() => global::LocalizationSystem.IsReady);

        if (global::LocalizationSystem.IsReady) {
            loadingSlider.value = 0.7f;
            loadingText.text = string.Format(global::LocalizationSystem.GetText("loading_format"), "70");
        }

        // 4. [THỰC TẾ] Chờ cho GameManager và DataManager tải xong tệp Save của người chơi.
        // Cài đặt Timeout an toàn (5s tối đa)
        float maxWaitTime = 5.0f;
        float currentWaitTime = 0f;

        while (currentWaitTime < maxWaitTime)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.DataManager != null &&
                GameManager.Instance.DataManager.Player != null)
            {
                break; // Thoát lặp khi nạp xong The Player
            }

            // Nếu DataManager load lỗi khiến biến Player == null thì chờ
            currentWaitTime += Time.deltaTime;
            yield return null;
        }

        if (currentWaitTime >= maxWaitTime)
        {
            Debug.LogError("[Bootloader] QUÁ THỜI GIAN LOAD DATA (5s)! Hệ thống DataManager đang bị Crash ngầm. Bỏ qua và cưỡng ép mở UI.");
            // Cứu cánh: Force init rỗng nếu thất bại
            if (GameManager.Instance?.DataManager != null && GameManager.Instance.DataManager.Player == null)
            {
               GameManager.Instance.DataManager.InitializeDataManager();
            }
        }

        // Đã Load thành công (Hoặc Force load)
        loadingSlider.value = 1.0f;
        if (global::LocalizationSystem.IsReady) {
            loadingText.text = string.Format(global::LocalizationSystem.GetText("loading_format"), "100");
        }

        // Đợi một khoảng ngắn mượt UI
        yield return new WaitForSeconds(0.4f);

        // Kích hoạt Game State sang Playing nếu bị găm ở Initializing
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
             GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
        }

        // 5. Mở Panel Làng Chính (Phải kiểm tra Null kỹ, tránh crash dây chuyền)
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.ShowPanel(UIPanelType.MainScreen);
        }
        else 
        {
            Debug.LogError("[Bootloader] THIẾU UIManager! Game không thể mở màn hình UI.");
        }

        // 6. Đóng Bootloader
        if (loadingScreenGroup != null) loadingScreenGroup.SetActive(false);
        if (tapToStartGroup != null) tapToStartGroup.SetActive(false);
        
        // Cực kỳ cẩn thận: CHỈ tắt component hoặc Image background của Bootloader,
        // TUYỆT ĐỐI KHÔNG tắt gameObject nếu nó là ROOT CANVAS của game.
        // Tắt Canvas gốc = Tắt mọi thứ.
        Image bg = GetComponent<Image>();
        if (bg != null) bg.enabled = false;
        
        // gameObject.SetActive(false); // BỎ LỆNH NÀY TẠM THỜI VÌ CÓ NGUY CƠ TẮT NHẦM ROOT CANVAS
    }
}