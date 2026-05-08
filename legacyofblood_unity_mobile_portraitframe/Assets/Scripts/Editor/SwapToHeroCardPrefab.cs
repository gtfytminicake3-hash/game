using UnityEngine;
using UnityEditor;

public class SwapToHeroCardPrefab {
    [MenuItem("Tools/Swap To HeroCard_Prefab")]
    public static void Run() {
        var bp = Object.FindAnyObjectByType<LegendOfBlood.BarrackPanel>(FindObjectsInactive.Include);
        if(bp) {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HeroCard_Prefab.prefab");
            if (go) {
                var so = new SerializedObject(bp);
                so.FindProperty("heroCardPrefab").objectReferenceValue = go;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(bp);
                Debug.Log("Swapped to Assets/Prefabs/HeroCard_Prefab.prefab");
                
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
