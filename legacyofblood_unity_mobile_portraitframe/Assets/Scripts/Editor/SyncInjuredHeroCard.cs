using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using LegendOfBlood;

public class SyncInjuredHeroCard : MonoBehaviour
{
    [MenuItem("UI Tools/1. Sync Hospital Mockup to Prefab")]
    public static void SyncPrefab()
    {
        var panel = FindFirstObjectByType<HospitalPanel>();
        if (panel == null)
        {
            Debug.LogError("HospitalPanel not found in scene!");
            return;
        }

        SerializedObject so = new SerializedObject(panel);
        Transform severeContainer = so.FindProperty("severeInjuryListContainer").objectReferenceValue as Transform;
        
        if (severeContainer == null || severeContainer.childCount == 0)
        {
            Debug.LogError("No mockup cards found in Severe Injury List Container to extract!");
            return;
        }

        GameObject mockupUI = severeContainer.GetChild(0).gameObject;

        // Ensure scripts
        InjuredHeroCard injuredScript = mockupUI.GetComponent<InjuredHeroCard>();
        if (injuredScript == null) injuredScript = mockupUI.AddComponent<InjuredHeroCard>();
        
        HeroCard heroScript = mockupUI.GetComponent<HeroCard>();
        if (heroScript == null) heroScript = mockupUI.AddComponent<HeroCard>();

        // Auto Wire HeroCard
        SerializedObject soHero = new SerializedObject(heroScript);
        AutoWireHeroCard(soHero, mockupUI.transform);
        soHero.ApplyModifiedProperties();

        // Auto Wire InjuredHeroCard
        SerializedObject soInjured = new SerializedObject(injuredScript);
        AutoWireInjuredCard(soInjured, mockupUI.transform);
        soInjured.ApplyModifiedProperties();

        // Save Prefab
        string prefabPath = "Assets/Prefabs/Panel/InjuredHeroCard_Final.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(mockupUI, prefabPath, InteractionMode.UserAction);
        
        if (savedPrefab != null)
        {
            so.FindProperty("injuredHeroCardPrefab").objectReferenceValue = savedPrefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(panel);

            // Clean up other mockups so they don't get duplicated on playing
            for (int i = severeContainer.childCount - 1; i >= 0; i--) {
                Undo.DestroyObjectImmediate(severeContainer.GetChild(i).gameObject);
            }
            Transform lightContainer = so.FindProperty("lightInjuryListContainer").objectReferenceValue as Transform;
            if (lightContainer != null) {
                for (int i = lightContainer.childCount - 1; i >= 0; i--) {
                    Undo.DestroyObjectImmediate(lightContainer.GetChild(i).gameObject);
                }
            }

            Debug.Log("<color=green>SUCCESS:</color> Extracted mockup to Prefab, Auto-Wired all texts/images, assigned to Panel, and cleaned up mockups. Game is ready to run!");
        }
    }

    private static void AutoWireHeroCard(SerializedObject so, Transform root)
    {
        // Name
        var nameT = FindDeep(root, "Name") ?? FindDeep(root, "NameText");
        if (nameT) so.FindProperty("nameText").objectReferenceValue = nameT.GetComponent<TextMeshProUGUI>();

        // Level
        var lvlT = FindDeep(root, "Level") ?? FindDeep(root, "LevelText");
        if (lvlT) so.FindProperty("levelText").objectReferenceValue = lvlT.GetComponent<TextMeshProUGUI>();

        // CP
        var cpT = FindDeep(root, "CP") ?? FindDeep(root, "CombatPowerText");
        if (cpT) so.FindProperty("combatPowerText").objectReferenceValue = cpT.GetComponent<TextMeshProUGUI>();

        // Avatar
        var ava = FindDeep(root, "Avatar") ?? FindDeep(root, "AvatarImage");
        if (ava) so.FindProperty("avatarImage").objectReferenceValue = ava.GetComponent<Image>();

        // Class Icon
        var cls = FindDeep(root, "ClassIcon") ?? FindDeep(root, "ProfessionIcon");
        if (cls) so.FindProperty("professionIcon").objectReferenceValue = cls.GetComponent<Image>();
    }

    private static void AutoWireInjuredCard(SerializedObject so, Transform root)
    {
        // Timer Text
        var timer = FindDeep(root, "Timer") ?? FindDeep(root, "TimerText") ?? FindDeep(root, "Time");
        if (timer) so.FindProperty("timerText").objectReferenceValue = timer.GetComponent<TextMeshProUGUI>();

        // Heal Button
        var btn = FindDeep(root, "HealButton") ?? FindDeep(root, "Button");
        if (btn) {
            Button bComp = btn.GetComponent<Button>();
            if (bComp == null) bComp = btn.gameObject.AddComponent<Button>();
            so.FindProperty("healButton").objectReferenceValue = bComp;

            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt) so.FindProperty("costText").objectReferenceValue = txt;
        }
    }

    private static Transform FindDeep(Transform parent, string nameContains)
    {
        if (parent.name.ToLower().Contains(nameContains.ToLower())) return parent;
        foreach (Transform child in parent)
        {
            var result = FindDeep(child, nameContains);
            if (result != null) return result;
        }
        return null;
    }
}
