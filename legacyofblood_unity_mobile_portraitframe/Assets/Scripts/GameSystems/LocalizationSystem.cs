using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống dịch thuật tĩnh, có thể truy cập từ bất kỳ đâu trong project.
/// Nó tải các file ngôn ngữ từ thư mục Resources.
/// </summary>
public static class LocalizationSystem
{
    // Thêm dòng này: Khai báo một sự kiện tĩnh
    public static event System.Action OnLanguageChanged;

    private static Dictionary<string, string> _localizedText;
    private static bool _isReady = false;
    public static bool IsReady => _isReady;

    public static void LoadLocalizedText(Language language)
    {
        _localizedText = new Dictionary<string, string>();
        string langCode = language == Language.English ? "en" : "vi";
        TextAsset textAsset = Resources.Load<TextAsset>($"Localization/{langCode}");

        if (textAsset != null)
        {
            string[] lines = textAsset.text.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    _localizedText[parts[0]] = parts[1].Trim(); // Thêm Trim() để loại bỏ khoảng trắng thừa
                }
            }
            _isReady = true;
            Debug.Log($"LocalizationSystem: Đã tải xong ngôn ngữ {language}. Phát sự kiện OnLanguageChanged.");
            // Thêm dòng này: Phát sự kiện đi để các hệ thống khác biết
            OnLanguageChanged?.Invoke();
        }
        else
        {
            _isReady = false;
            Debug.LogError($"Could not find localization file for language: {langCode}");
        }
    }

    public static string GetText(string key)
    {
        if (_localizedText != null && _localizedText.TryGetValue(key, out string value))
        {
            return value;
        }
        
        // Chỉ cảnh báo nếu hệ thống đã sẵn sàng mà vẫn không tìm thấy key
        if (_isReady) Debug.LogWarning($"Localization key not found: {key}");
        return key; // Trả về chính key nếu không tìm thấy
    }
}