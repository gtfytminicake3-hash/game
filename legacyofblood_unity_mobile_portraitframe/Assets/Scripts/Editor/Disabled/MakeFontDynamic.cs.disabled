using UnityEngine;
using UnityEditor;
using TMPro;

[InitializeOnLoad]
public class MakeFontDynamic
{
    static MakeFontDynamic()
    {
        EditorApplication.delayCall += DoMakeDynamic;
    }

    static void DoMakeDynamic()
    {
        if (SessionState.GetBool("MakeFontDynamicRun", false)) return;
        SessionState.SetBool("MakeFontDynamicRun", true);

        // Nâng cấp trực tiếp Font gốc của User (SVN Arial)
        string[] guids = AssetDatabase.FindAssets("SVN-Arial 3 SDF t:TMP_FontAsset");
        if (guids.Length == 0) return;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        
        if (font != null)
        {
            // 1. Chuyển sang chế độ Dynamic (Tự động nặn chữ thiếu trong lúc chơi/edit)
            var popField = font.GetType().GetProperty("atlasPopulationMode");
            if (popField != null)
            {
                font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            }

            // 2. Dùng SerializedObject để hack xuyên qua lỗi 'read only' của Unity
            SerializedObject so = new SerializedObject(font);
            SerializedProperty srcProp = so.FindProperty("m_SourceFontFile");
            
            // Chắc chắn Font gốc đã được gắn vào để có khuôn nặn chữ
            if (srcProp != null && srcProp.objectReferenceValue == null)
            {
                string[] ttfGuids = AssetDatabase.FindAssets("SVN-Arial 3 t:Font");
                if (ttfGuids.Length > 0)
                {
                    Font ttfFont = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(ttfGuids[0]));
                    srcProp.objectReferenceValue = ttfFont;
                    so.ApplyModifiedProperties();
                }
            }

            // 3. Xoá mọi thiết lập rườm rà của Font cũ và bật cơ chế sinh chữ mới
            font.isMultiAtlasTexturesEnabled = true;

            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();

            Debug.Log("<color=green><b>[QUYẾT ĐỊNH CUỐI CÙNG] Đã bật cơ chế DYNAMIC cho Font SVN-Arial.</b> Từ nay W, F, Z sẽ TỰ ĐỘNG mọc lại ra khi gặp. Bạn KHÔNG BAO GIỜ phải nặn chữ thủ công nữa!</color>");
            
            // 4. Ép tất cả các đối tượng text dựng lại hình ảnh
            var texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
            foreach(var t in texts) 
            {
                // Nếu bị gắn đổi Font Liberation bởi tool cũ thì trả lại SVN Arial
                if (t.font != null && t.font.name.Contains("Liberation"))
                {
                    t.font = font;
                    EditorUtility.SetDirty(t);
                    if (!EditorUtility.IsPersistent(t)) EditorUtility.SetDirty(t.gameObject);
                }
                t.SetAllDirty();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
