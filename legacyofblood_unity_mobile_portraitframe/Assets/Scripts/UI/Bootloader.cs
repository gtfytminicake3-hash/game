using LegendOfBlood;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        // Thiết lập trạng thái ban đầu
        tapToStartGroup.SetActive(true);
        loadingScreenGroup.SetActive(false);
    }

    /// <summary>
    /// Hàm này sẽ được gọi bởi nút "TapToStart_Group".
    /// </summary>
    public void StartLoading()
    {
        // Ẩn "Tap to Start", hiện màn hình loading
        tapToStartGroup.SetActive(false);
        loadingScreenGroup.SetActive(true);

        // Bắt đầu quá trình tải bất đồng bộ
        StartCoroutine(LoadMainSceneAsync());
    }

    /// <summary>
    /// Coroutine để tải scene một cách bất đồng bộ và cập nhật thanh loading.
    /// </summary>
    private IEnumerator LoadMainSceneAsync()
    {
        // 1. Tạo ra hệ thống lõi. GameManager sẽ tự xử lý việc chỉ có một instance duy nhất.
        // Nếu một GameManager đã tồn tại từ lần chạy trước (trong Editor), instance mới này sẽ tự hủy
        // và instance cũ sẽ tiếp tục hoạt động.
        if (coreSystemsPrefab != null)
        {
            Instantiate(coreSystemsPrefab);
        }

        // Đợi 1 frame để đảm bảo Core Systems được khởi tạo

        // 2. Đợi cho đến khi hệ thống dịch thuật sẵn sàng
        // Điều này tránh lỗi không tìm thấy key khi màn hình loading vừa xuất hiện
        yield return new WaitUntil(() => global::LocalizationSystem.IsReady);

        // 2. Bắt đầu tải MainScene trong nền
        AsyncOperation operation = SceneManager.LoadSceneAsync("MainScene");

        // 3. Vòng lặp cập nhật thanh loading trong khi scene đang tải
        while (!operation.isDone)
        {
            // operation.progress chỉ đi từ 0 đến 0.9.
            // Chúng ta chia cho 0.9 để chuẩn hóa nó về khoảng 0 đến 1.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Cập nhật UI
            loadingSlider.value = progress;
            loadingText.text = string.Format(global::LocalizationSystem.GetText("loading_format"), (progress * 100f).ToString("F0"));

            // Đợi frame tiếp theo
            yield return null;
        }
    }
}