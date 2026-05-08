using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class DragonScrollAutoSlicer : EditorWindow
{
    [MenuItem("UI Tools/2. Auto Slice Dragon Scrollbar (MAGIC)")]
    public static void AutoSlice()
    {
        string path = "Assets/Resources/UI Inventory/scroll.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) {
            Debug.LogError("Error: Cannot find " + path);
            return;
        }

        if (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed) {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int w = original.width;
        int h = original.height;
        Color32[] pixels = original.GetPixels32();

        // 1. Find bounding left and right
        int minX = w, maxX = 0;
        int[] rowWidths = new int[h];
        int[] rowMinX = new int[h];
        int[] rowMaxX = new int[h];

        for (int y = 0; y < h; y++)
        {
            int rMin = w, rMax = 0;
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a > 10)
                {
                    if (x < rMin) rMin = x;
                    if (x > rMax) rMax = x;
                }
            }
            if (rMin <= rMax)
            {
                rowMinX[y] = rMin;
                rowMaxX[y] = rMax;
                rowWidths[y] = rMax - rMin + 1;
                if (rMin < minX) minX = rMin;
                if (rMax > maxX) maxX = rMax;
            }
            else
            {
                rowWidths[y] = 0;
            }
        }

        // 2. Find Pole Width (majority width)
        List<int> validW = new List<int>();
        for (int y = 0; y < h; y++) if (rowWidths[y] > 5) validW.Add(rowWidths[y]);
        validW.Sort();
        int poleWidth = validW[validW.Count / 2]; // median width is the pole!

        // 3. Find Handle Y bounds (from Top to Bottom: Top=h-1, Bottom=0)
        // Regions thicker than poleWidth * 1.5
        int threshold = (int)(poleWidth * 1.5f) + 4;
        
        int dragonHeadBottomY = -1;
        int handleTopY = -1;
        int handleBottomY = -1;

        // Scan from top down
        int state = 0; // 0: dragon head, 1: pole gap, 2: handle
        for (int y = h - 1; y >= 0; y--)
        {
            if (rowWidths[y] > threshold)
            {
                if (state == 1) { // Found handle top!
                    handleTopY = y;
                    state = 2;
                }
            }
            else if (rowWidths[y] <= threshold && rowWidths[y] > 0)
            {
                if (state == 0) { // Found end of dragon head
                    dragonHeadBottomY = y;
                    state = 1;
                }
                else if (state == 2) { // Found end of handle!
                    handleBottomY = y;
                    break;
                }
            }
        }

        if (handleTopY == -1 || handleBottomY == -1) {
            Debug.LogError("MAGIC SLICER FAILED: Could not detect the handle distinctively.");
            // Fallback estimation
            handleTopY = h - (int)(h * 0.15f);
            handleBottomY = h - (int)(h * 0.35f);
        }

        Debug.Log($"Detected Handle: Y {handleBottomY} to {handleTopY}");

        // 4. Extract Handle Texture
        int handleH = handleTopY - handleBottomY + 1;
        int handleW = maxX - minX + 1;
        Texture2D handleTex = new Texture2D(handleW, handleH, TextureFormat.RGBA32, false);
        Color32[] handlePixels = new Color32[handleW * handleH];
        for (int y = 0; y < handleH; y++) {
            for (int x = 0; x < handleW; x++) {
                handlePixels[y * handleW + x] = pixels[(handleBottomY + y) * w + (minX + x)];
            }
        }
        handleTex.SetPixels32(handlePixels);
        handleTex.Apply();

        // 5. Heal Background Texture
        Texture2D bgTex = new Texture2D(handleW, h, TextureFormat.RGBA32, false);
        Color32[] bgPixels = new Color32[handleW * h];

        // The safe row to clone is just slightly below the handle
        int cloneY = handleBottomY - 10;
        if (cloneY < 0) cloneY = 0;
        Color32[] safeRow = new Color32[handleW];
        for(int x = 0; x < handleW; x++) safeRow[x] = pixels[cloneY * w + (minX + x)];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < handleW; x++)
            {
                if (y >= handleBottomY && y <= handleTopY) {
                    bgPixels[y * handleW + x] = safeRow[x]; // Erase handle with pole
                } else {
                    bgPixels[y * handleW + x] = pixels[y * w + (minX + x)];
                }
            }
        }
        bgTex.SetPixels32(bgPixels);
        bgTex.Apply();

        // 6. Save PNGs
        string handlePath = "Assets/Resources/UI Inventory/scroll_handle.png";
        string bgPath = "Assets/Resources/UI Inventory/scroll_bg.png";
        File.WriteAllBytes(handlePath, handleTex.EncodeToPNG());
        File.WriteAllBytes(bgPath, bgTex.EncodeToPNG());
        
        AssetDatabase.Refresh();

        // Configure Importers
        TextureImporter bgImp = AssetImporter.GetAtPath(bgPath) as TextureImporter;
        bgImp.textureType = TextureImporterType.Sprite;
        // 9-slice bg to protect arrow and dragon head!
        int sliceBottom = handleBottomY - 20;
        int sliceTop = h - handleTopY - 20; 
        if(sliceBottom < 10) sliceBottom = 10;
        if(sliceTop < 10) sliceTop = 10;
        bgImp.spriteBorder = new Vector4(0, sliceBottom, 0, sliceTop);
        bgImp.SaveAndReimport();

        TextureImporter hImp = AssetImporter.GetAtPath(handlePath) as TextureImporter;
        hImp.textureType = TextureImporterType.Sprite;
        hImp.SaveAndReimport();

        Debug.Log("<color=cyan><b>MAGIC Slicing Successful!</b> Background and Handle exported.</color>");
        
        // 7. Inject into Panel_Inventory Automatically
        InjectToPrefab();
    }

    static void InjectToPrefab()
    {
        string prefabPath = "Assets/Prefabs/Panel/Panel_Inventory.prefab";
        GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabObj == null) return;
        
        GameObject prefab = PrefabUtility.InstantiatePrefab(prefabObj) as GameObject;
        
        // Quét tìm đúng ScrollRect chứa hộp đồ (có GridLayoutGroup)
        ScrollRect scrollRect = null;
        ScrollRect[] allScrolls = prefab.GetComponentsInChildren<ScrollRect>(true);
        foreach(var sr in allScrolls) {
            if (sr.content != null && sr.content.GetComponent<GridLayoutGroup>() != null) {
                scrollRect = sr;
                break;
            }
        }
        if (scrollRect == null && allScrolls.Length > 0) scrollRect = allScrolls[allScrolls.Length - 1]; // Rơi vào lưới cuối
        if (scrollRect == null) return;

        Transform existing = scrollRect.transform.Find("LeftScrollbar");
        if (existing == null) existing = scrollRect.transform.parent.Find("LeftScrollbar");
        if (existing != null) DestroyImmediate(existing.gameObject);

        GameObject scrollbarObj = new GameObject("LeftScrollbar");
        // Đặt BÊN TRONG ScrollRect thay vì đặt ngoài lề để tránh lệch toạ độ
        scrollbarObj.transform.SetParent(scrollRect.transform, false);

        RectTransform scrollbarRT = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRT.anchorMin = new Vector2(0, 0); scrollbarRT.anchorMax = new Vector2(0, 1);
        scrollbarRT.pivot = new Vector2(0, 0.5f); // Neo từ cạnh trái trở đi
        scrollbarRT.sizeDelta = new Vector2(50, 0); 
        scrollbarRT.anchoredPosition = new Vector2(10, 0); // Thụt vào trong 10 pixel thay vì quăng ra ngoài màn hình (-40)
        
        // Buộc thanh trượt co dãn giãn đều ở đỉnh và đáy
        scrollbarRT.offsetMin = new Vector2(10, 20); 
        scrollbarRT.offsetMax = new Vector2(60, -20);

        Image bgImage = scrollbarObj.AddComponent<Image>();
        bgImage.type = Image.Type.Sliced;
        bgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI Inventory/scroll_bg.png");

        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbarObj.transform, false);
        RectTransform saRT = slidingArea.AddComponent<RectTransform>();
        saRT.anchorMin = new Vector2(0, 0.05f); saRT.anchorMax = new Vector2(1, 0.95f);
        saRT.sizeDelta = Vector2.zero;
        saRT.offsetMin = Vector2.zero; saRT.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slidingArea.transform, false);
        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(0, 0);
        handleRT.offsetMin = Vector2.zero; handleRT.offsetMax = Vector2.zero;

        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI Inventory/scroll_handle.png");

        Scrollbar scrollbarComp = scrollbarObj.AddComponent<Scrollbar>();
        scrollbarComp.handleRect = handleRT;
        scrollbarComp.targetGraphic = handleImage;
        scrollbarComp.direction = Scrollbar.Direction.BottomToTop;

        scrollRect.verticalScrollbar = scrollbarComp;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scrollRect.verticalScrollbarSpacing = 10;

        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        DestroyImmediate(prefab);
        
        Debug.Log("<color=green><b>[SUCCESS]</b> Panel_Inventory has been upgraded with the Dynamic Dragon Scrollbar effect! Attached to Grid ScrollRect.</color>");
    }
}

