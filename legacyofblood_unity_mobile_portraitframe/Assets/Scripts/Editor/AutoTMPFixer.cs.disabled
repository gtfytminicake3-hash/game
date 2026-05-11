using UnityEngine;
using UnityEditor;
using TMPro;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class AutoTMPFixer
{
    static AutoTMPFixer()
    {
        EditorApplication.delayCall += FixNow;
    }

    public static void FixNow()
    {
        // Chạy 1 lần duy nhất trong session Editor
        if (SessionState.GetBool("AutoTMPFixerRun", false)) return;
        SessionState.SetBool("AutoTMPFixerRun", true);

        TMP_FontAsset engFont = TMP_Settings.defaultFontAsset;
        if (engFont == null)
        {
            string[] guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
            if (guids.Length > 0)
                engFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        if (engFont == null) return;

        // Quét TẤT CẢ các loại 3D Text, UI TextUGUI bao gồm cả những thứ giấu trong Prefab EXTRACTED_WorldMap
        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        int count = 0;
        
        bool HasVietnamese(string tex)
        {
            string vietnamesePattern = @"[áàảãạăắằẳẵặâấầẩẫậeéèẻẽẹêếềểễệiíìỉĩịoóòỏõọôốồổỗộơớờởỡợuúùủũụưứừửữựyýỳỷỹỵAÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬEÉÈẺẼẸÊẾỀỂỄỆIÍÌỈĨỊOÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢUÚÙỦŨỤƯỨỪỬỮỰYÝỲỶỸỴđĐ]";
            return Regex.IsMatch(tex, vietnamesePattern);
        }

        foreach (var t in texts)
        {
            // Nếu font hiện tại là SVN-Arial và bị mất nét W, F...
            if (t.font != null && t.font.name.Contains("SVN") && !string.IsNullOrEmpty(t.text) && !HasVietnamese(t.text) && t.font != engFont)
            {
                Undo.RecordObject(t, "Auto Fix WorldMap Font");
                t.font = engFont;
                t.SetAllDirty();
                EditorUtility.SetDirty(t);
                
                // Nếu đây là Prefab, đánh dấu lưu
                EditorUtility.SetDirty(t.gameObject); 
                count++;
            }
        }
        
        if (count > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=cyan><b>[PHÉP MÀU AUTO-FIX]</b></color> Đã tự động dò tìm dọn dẹp và sửa lỗi mất chữ W, F cho {count} đối tượng Text trong Prefab WorldMap! Bạn hãy mở lại WorldMap ra xem kết quả nhé.");
        }
    }
}
