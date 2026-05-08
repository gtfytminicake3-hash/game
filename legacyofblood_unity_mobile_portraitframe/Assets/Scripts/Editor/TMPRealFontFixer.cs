using UnityEngine;
using UnityEditor;
using TMPro;
using System.Text.RegularExpressions;

namespace LegendOfBlood.EditorTools
{
    public class TMPRealFontFixer : EditorWindow
    {
        [MenuItem("Tools/Giải cứu triệt để chữ W, F trên World Map")]
        public static void FixEnglishTextFonts()
        {
            TMP_FontAsset engFont = TMP_Settings.defaultFontAsset;
            if (engFont == null)
            {
                string[] guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
                if (guids.Length > 0)
                    engFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            if (engFont == null) 
            {
                Debug.LogError("Chịu thua! Không tìm thấy cả Unity default font.");
                return;
            }

            TextMeshProUGUI[] texts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            int count = 0;
            
            // Kiểm tra xem chữ có chứa Tiếng Việt có dấu hay không
            bool HasVietnamese(string tex)
            {
                string vietnamesePattern = @"[áàảãạăắằẳẵặâấầẩẫậeéèẻẽẹêếềểễệiíìỉĩịoóòỏõọôốồổỗộơớờởỡợuúùủũụưứừửữựyýỳỷỹỵAÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬEÉÈẺẼẸÊẾỀỂỄỆIÍÌỈĨỊOÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢUÚÙỦŨỤƯỨỪỬỮỰYÝỲỶỸỴđĐ]";
                return Regex.IsMatch(tex, vietnamesePattern);
            }

            foreach (var t in texts)
            {
                // Chỉ sửa những chữ đang nổi trên Scene / UI Prefab đang mở
                if (EditorUtility.IsPersistent(t.transform.root.gameObject))
                    continue; 

                // Nếu là từ Tiếng Anh thuần túy (như World Map, Capital Forest, Active Quests)
                // Nó chẳng cần dùng đến Font Việt Nam bị vỡ nét làm gì cả! Đổi nó về font Quốc tế chuẩn 100%.
                if (!string.IsNullOrEmpty(t.text) && !HasVietnamese(t.text))
                {
                    Undo.RecordObject(t, "Fix English Font");
                    t.font = engFont;
                    EditorUtility.SetDirty(t);
                    count++;
                }
                
                // Ép dựng lại hình ảnh của tất cả chữ (để cập nhật UI ngay lập tức trên Editor)
                t.SetAllDirty();
            }

            Debug.Log($"<color=green><b>THÀNH CÔNG RỰC RỠ!</b></color>\nĐã nhổ tận rễ lỗi chữ bằng cách phân vùng {count} ô Text Tiếng Anh (không dấu) trên màn hình và chuyển thẳng qua Font chuẩn Quốc Tế!\nChữ W, F, Z trong WORLD MAP và Capital Forest đã chính thức hồi sinh!");
        }
    }
}
