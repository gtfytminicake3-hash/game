using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class MockupHPFixer : MonoBehaviour
{
    [MenuItem("UI Tools/Fix Severe and Light Cards HP Bar")]
    public static void FixMockups()
    {
        string framePath = "Assets/Art/UI/Hospital/HPFrame.png";
        string fillPath = "Assets/Art/UI/Hospital/HPFill.png";
        
        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
        Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fillPath);

        int count = 0;
        var cards = Object.FindObjectsByType<LegendOfBlood.InjuredHeroCard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var card in cards) {
            ReplaceBar(card.transform, frameSprite, fillSprite);
            EditorUtility.SetDirty(card.gameObject);
            count++;
        }

        // Also check prefabs just in case
        string pGuid = "Assets/Prefabs/InjuredHeroCard_prefab.prefab";
        if (System.IO.File.Exists(pGuid)) {
            GameObject pObj = PrefabUtility.LoadPrefabContents(pGuid);
            ReplaceBar(pObj.transform, frameSprite, fillSprite);
            PrefabUtility.SaveAsPrefabAsset(pObj, pGuid);
            PrefabUtility.UnloadPrefabContents(pObj);
            count++;
        }

        Debug.Log("Successfully applied heart HP Bar to " + count + " Severe/Light cards in Scene!");
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    private static void ReplaceBar(Transform card, Sprite frameSprite, Sprite fillSprite) 
    {
        Transform oldHpBar = card.Find("HPBar");
        Transform existingArea = card.Find("HPBarArea");
        
        Vector2 pos = new Vector2(0, 43);
        Vector2 anchorMin = new Vector2(0.5f, 0);
        Vector2 anchorMax = new Vector2(0.5f, 0);
        Vector2 pivot = new Vector2(0.5f, 0);
        Vector2 size = new Vector2(130, 24);

        if (oldHpBar != null) {
            RectTransform oldRt = oldHpBar.GetComponent<RectTransform>();
            if (oldRt != null) {
                pos = oldRt.anchoredPosition;
                anchorMin = oldRt.anchorMin;
                anchorMax = oldRt.anchorMax;
                pivot = oldRt.pivot;
                size = oldRt.sizeDelta;
                // Override size to ensure heart fits nicely, but respect height
                size = new Vector2(Mathf.Max(size.x, 100), Mathf.Max(size.y, 24));
            }
            Undo.DestroyObjectImmediate(oldHpBar.gameObject);
        }
        if (existingArea != null) {
            RectTransform extRt = existingArea.GetComponent<RectTransform>();
            if (extRt != null) {
                pos = extRt.anchoredPosition;
                anchorMin = extRt.anchorMin;
                anchorMax = extRt.anchorMax;
                pivot = extRt.pivot;
                size = extRt.sizeDelta;
            }
            Undo.DestroyObjectImmediate(existingArea.gameObject);
        }

        GameObject hpArea = new GameObject("HPBarArea");
        hpArea.AddComponent<RectTransform>();
        hpArea.transform.SetParent(card, false);
        
        RectTransform areaRt = hpArea.GetComponent<RectTransform>();
        areaRt.anchorMin = anchorMin; 
        areaRt.anchorMax = anchorMax;
        areaRt.pivot = pivot;
        areaRt.anchoredPosition = pos;
        areaRt.sizeDelta = size;

        GameObject hpBarBg = new GameObject("HPBarBG");
        hpBarBg.AddComponent<RectTransform>();
        hpBarBg.transform.SetParent(hpArea.transform, false);
        RectTransform bgRt = hpBarBg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        if (frameSprite != null) {
            Image bgImg = hpBarBg.AddComponent<Image>();
            bgImg.sprite = frameSprite;
            bgImg.preserveAspect = true;
        }

        GameObject hpBarFill = new GameObject("HPBarFill");
        hpBarFill.AddComponent<RectTransform>();
        hpBarFill.transform.SetParent(hpArea.transform, false);
        RectTransform fillRt = hpBarFill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
        if (fillSprite != null) {
            Image fillImg = hpBarFill.AddComponent<Image>();
            fillImg.sprite = fillSprite;
            fillImg.preserveAspect = true;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0; 
            fillImg.fillAmount = 0.5f;
            
            if (card.name.Contains("Severe")) fillImg.fillAmount = 0.2f;
            else if (card.name.Contains("Light")) fillImg.fillAmount = 0.8f;
        }

        // Put above PriceButton / HealButton
        Transform pb = card.Find("PriceButton");
        if (pb == null) pb = card.Find("HealButton");
        if (pb != null) hpArea.transform.SetSiblingIndex(pb.GetSiblingIndex());
    }
}
