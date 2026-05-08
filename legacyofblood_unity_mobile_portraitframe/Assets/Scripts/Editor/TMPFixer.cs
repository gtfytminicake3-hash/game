using UnityEditor;
using UnityEngine;
using TMPro;

namespace LegendOfBlood.EditorTools
{
    public class TMPFixer : EditorWindow
    {
        [MenuItem("Tools/Sửa lỗi mất chữ TMP (Space Bug)")]
        public static void FixTMPWrappingAndOverflow()
        {
            // Tìm tất cả các TextMeshProUGUI trong Scene hiện tại (bao gồm cả đang ẩn)
            TextMeshProUGUI[] allTMPs = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            int fixedCount = 0;

            foreach (var tmp in allTMPs)
            {
                // Bỏ qua các object ở trong thư mục Project (Prefab gốc chưa kéo ra scene) 
                if (EditorUtility.IsPersistent(tmp.transform.root.gameObject))
                    continue;

                Undo.RecordObject(tmp, "Fix TMP Space Bug");

                // 1. Tắt tự động xuống dòng (thủ phạm chính)
                tmp.enableWordWrapping = false;

                // 2. Chuyển Overflow sang dạng không che khuất
                tmp.overflowMode = TextOverflowModes.Overflow;

                EditorUtility.SetDirty(tmp);
                fixedCount++;
            }

            Debug.Log($"<color=green><b>Đã sửa lỗi hiển thị chữ cho {fixedCount} Object TextMeshPro trong Scene.</b></color>\nHãy kiểm tra lại chữ trên giao diện!");
        }
    }
}
