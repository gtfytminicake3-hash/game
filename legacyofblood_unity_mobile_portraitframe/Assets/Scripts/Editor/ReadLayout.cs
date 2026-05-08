using UnityEngine;
using UnityEditor;

public class ReadLayout : MonoBehaviour {
    [MenuItem("Tools/Read Card Layout")]
    public static void Run() {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HeroCard_SquadSelection_Prefab.prefab");
        if(go) {
            foreach(RectTransform rt in go.GetComponentsInChildren<RectTransform>(true)) {
                string p = rt.name;
                Transform curr = rt.parent;
                while(curr && curr != go.transform) { p = curr.name + "/" + p; curr = curr.parent; }
                Debug.Log($"[{p}] pos:{rt.anchoredPosition} size:{rt.sizeDelta} anchorMin:{rt.anchorMin} anchorMax:{rt.anchorMax} pivot:{rt.pivot}");
            }
        }
    }
}
