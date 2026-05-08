using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace LegendOfBlood.EditorTools
{
    public class TMPFontFallbackFixer : EditorWindow
    {
        [MenuItem("Tools/Sửa vĩnh viễn lỗi Thiếu Chữ (W, F, Z...) bằng Fallback")]
        public static void FixMissingCharactersWithFallback()
        {
            // Lấy Font cơ bản chuẩn nhất của Unity làm lốp dự phòng
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            
            if (defaultFont == null)
            {
                string[] builtinGuids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
                if (builtinGuids.Length > 0)
                {
                    defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(builtinGuids[0]));
                }
            }

            if (defaultFont == null)
            {
                Debug.LogError("[Lỗi] Không tìm thấy Font dự phòng cơ bản của TextMeshPro.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            int fixedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                
                // Tránh tự ép chính nó vào chính nó
                if (font != null && font != defaultFont)
                {
                    if (font.fallbackFontAssetTable == null) 
                    {
                        font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    }

                    // Nhét font Default vào danh sách Cấp Cứu
                    if (!font.fallbackFontAssetTable.Contains(defaultFont))
                    {
                        font.fallbackFontAssetTable.Add(defaultFont);
                        EditorUtility.SetDirty(font);
                        fixedCount++;
                        Debug.Log($"[TMP Fallback] Đã ghép túi dự phòng cho Font: {font.name}");
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green><b>ĐẠI TU HOÀN TẤT!</b></color>\nĐã cài đặt Hệ thống Cứu nạn chữ cái cho {fixedCount} file Font Asset.\nTừ giờ bất kể bạn nhập chữ W, Z, F hay bất kỳ kí hiệu nào bị thiếu ở Font gốc, hệ thống sẽ tự động bù đắp bằng Font chuẩn. Không bao giờ lo mất chữ rỗ chữ nữa!");
        }
    }
}
