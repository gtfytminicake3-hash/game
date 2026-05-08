#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class KingGodPassImageProcessor : EditorWindow
{
    [MenuItem("UI Tools/Auto-Process Pass Image")]
    public static void ProcessImage()
    {
        string inputPath = @"C:\Users\admin\.gemini\antigravity\brain\e342b36f-4935-42fd-a633-05534c4d5e76\media__1775790832996.png";
        string outputDir = "Assets/Textures/UI";
        
        if (!File.Exists(inputPath))
        {
            Debug.LogError("Không tìm thấy file ảnh gốc từ chat. Bạn hãy đặt file ảnh vào thư mục Assets/ và kéo tay cũng được.");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Textures")) AssetDatabase.CreateFolder("Assets", "Textures");
        if (!AssetDatabase.IsValidFolder("Assets/Textures/UI")) AssetDatabase.CreateFolder("Assets/Textures", "UI");

        byte[] fileData = File.ReadAllBytes(inputPath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);

        int w = tex.width;
        int h = tex.height;

        Color[] pixels = tex.GetPixels();
        Color[] fillPixels = new Color[pixels.Length];
        Color[] bgPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            
            // Xác định màu đỏ: Red cao hơn Green và Blue đáng kể
            bool isRed = (c.r > 0.6f && c.g < 0.4f && c.b < 0.4f) || 
                         // Lõi trắng hồng của thanh gươm
                         (c.r > 0.8f && c.g > 0.6f && c.b > 0.6f && c.r > c.b); 
                         
            // Chỉ lấy đoạn gươm (khoảng 35% chiều ngang trở đi)
            int x = i % w;
            if (isRed && x > w * 0.35f && c.a > 0)
            {
                // Thuộc về thanh đỏ (Fill)
                fillPixels[i] = c;
                // Tô nền thành màu xám tối cho phần kiếm bị mất điện
                float gray = (c.r + c.g + c.b) / 3f;
                bgPixels[i] = new Color(gray * 0.3f, gray * 0.3f, gray * 0.3f, c.a);
            }
            else
            {
                // Các phần còn lại (mũ bảo hiểm, chuôi gươm)
                fillPixels[i] = new Color(0, 0, 0, 0); // Trong suốt
                bgPixels[i] = c; // Giữ nguyên
            }
        }

        Texture2D fillTex = new Texture2D(w, h);
        fillTex.SetPixels(fillPixels);
        fillTex.Apply();

        Texture2D bgTex = new Texture2D(w, h);
        bgTex.SetPixels(bgPixels);
        bgTex.Apply();

        string fillPath = outputDir + "/KingGodPass_Fill.png";
        string bgPath = outputDir + "/KingGodPass_Bg.png";

        File.WriteAllBytes(Application.dataPath + "/../" + fillPath, fillTex.EncodeToPNG());
        File.WriteAllBytes(Application.dataPath + "/../" + bgPath, bgTex.EncodeToPNG());

        AssetDatabase.Refresh();

        // Gắn vào KingGodPass nếu có
        GameObject passGroup = GameObject.Find("KingGodPass");
        if (passGroup != null)
        {
            UnityEngine.UI.Image bgImg = passGroup.GetComponent<UnityEngine.UI.Image>();
            TextureImporter bgImporter = AssetImporter.GetAtPath(bgPath) as TextureImporter;
            if (bgImporter != null) { bgImporter.textureType = TextureImporterType.Sprite; bgImporter.SaveAndReimport(); }
            
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            if (bgImg != null && bgSprite != null) bgImg.sprite = bgSprite;

            Transform fillObj = passGroup.transform.Find("PassProgressBar/PassFill");
            if (fillObj != null)
            {
                UnityEngine.UI.Image fillImg = fillObj.GetComponent<UnityEngine.UI.Image>();
                TextureImporter fillImporter = AssetImporter.GetAtPath(fillPath) as TextureImporter;
                if (fillImporter != null) { fillImporter.textureType = TextureImporterType.Sprite; fillImporter.SaveAndReimport(); }
                
                Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fillPath);
                if (fillImg != null && fillSprite != null) fillImg.sprite = fillSprite;
            }
        }

        Debug.Log("Hình ảnh ngai vàng/thanh gươm đã được tự động tách và gắn vào giao diện!");
    }
}
#endif
