using UnityEngine;
using UnityEditor;
using TMPro;

namespace LegendOfBlood.EditorTools
{
    public class TMPFontSpaceFixer : EditorWindow
    {
        [MenuItem("Tools/Sửa tiệt nọc lỗi mất dấu Space cho Font")]
        public static void FixMissingSpaceInFonts()
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            int fixedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                
                // Kí tự Space có mã ASCII là 32
                if (font != null && !font.HasCharacter(' '))
                {
                    try
                    {
                        uint spaceUnicode = 32;
                        uint dummyGlyphIndex = 99999; // Chỉ số ảo để tránh trùng lặp với glyph xịn
                        
                        // Độ rộng của nút Space (bằng 1/4 size chữ tiêu chuẩn)
                        float spaceAdvance = font.faceInfo.pointSize * 0.25f;
                        if (spaceAdvance <= 0) spaceAdvance = 12f;

                        var metrics = new UnityEngine.TextCore.GlyphMetrics(
                            0, 0, 0, 0, spaceAdvance 
                        );
                        
                        var rect = new UnityEngine.TextCore.GlyphRect(0, 0, 0, 0);
                        var glyph = new UnityEngine.TextCore.Glyph(dummyGlyphIndex, metrics, rect, 1f, 0);
                        
                        var character = new TMP_Character(spaceUnicode, font, glyph);
                        
                        font.characterTable.Add(character);
                        font.characterLookupTable[spaceUnicode] = character;
                        
                        font.glyphTable.Add(glyph);
                        font.glyphLookupTable[dummyGlyphIndex] = glyph;
                        
                        EditorUtility.SetDirty(font);
                        fixedCount++;
                        Debug.Log($"[TMPFix] Đã cấy ghép thành công Space vào font: {font.name}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[TMPFix] Cấy thất bại {font.name}: {e.Message}");
                    }
                }
            }
            
            // Lưu lại toàn bộ các Font vừa được cấy
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=green><b>HOÀN TẤT ĐẠI PHẪU!</b></color>\nĐã tiêm tự động kí tự Space vào {fixedCount} file Font Asset bị khiếm khuyết trong Project. Lúc này ấn Space thoải mái không mất chữ nữa nhé!");
        }
    }
}
