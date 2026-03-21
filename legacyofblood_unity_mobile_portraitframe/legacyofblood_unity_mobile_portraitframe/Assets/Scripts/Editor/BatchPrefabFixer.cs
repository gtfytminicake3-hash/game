using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace LegendOfBlood.EditorScripts
{
    public class BatchPrefabFixer : EditorWindow
    {
        [MenuItem("LegendOfBlood/Fix Batch 18 Prefabs")]
        public static void FixBatchPrefabs()
        {
            Dictionary<string, System.Action<GameObject>> prefabFixers = new Dictionary<string, System.Action<GameObject>>
            {
                { "ArenaShopItem_Prefab", FixArenaShopItem },
                { "InventoryEquipmentCard_Prefab", FixInventoryEquipmentCard },
                { "InventoryItemCard_Prefab", FixInventoryItemCard },
                { "PopulationHeroCard_Prefab", FixPopulationHeroCard },
                { "QuestItem_Prefab", FixQuestItem },
                { "ReportItem_Prefab", FixReportItem },
                { "SquadSlot_Prefab", FixSquadSlot },
                { "HeroInfo_Panel", FixHeroInfoPanel },
                { "EXTRACTED_Arena_Panel", FixArenaPanel },
                { "Panel_Barrack", FixBarrackPanel },
                { "Panel_Battle", FixBattlePanel },
                { "StatAllocationPanel", FixStatAllocationPanel },
                { "TraitUpgradePanel", FixTraitUpgradePanel },
                { "Panel_Inventory", FixInventoryPanel },
                { "Panel_Quest", FixQuestPanel },
                { "Panel_EquipmentDetail", FixEquipmentDetailPanel },
                { "Panel_EquipmentDetail 1", FixEquipmentDetailPanel },
                { "BuildingUpgradePanel", FixBuildingUpgradePanel },
                { "InjuredHeroCard_prefab", FixInjuredHeroCard }
            };

            foreach (var kvp in prefabFixers)
            {
                string prefabName = kvp.Key;
                string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab");
                
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    // Lọc để tránh trùng tên tương tự (ví dụ Panel_Quest và Panel_QuestItem)
                    if (System.IO.Path.GetFileNameWithoutExtension(path) == prefabName)
                    {
                        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
                        {
                            var root = editingScope.prefabContentsRoot;
                            kvp.Value.Invoke(root);
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>Successfully executed BatchPrefabFixer for 18 prefabs!</color>");
        }

        private static T AutoLink<T>(GameObject root, SerializedObject so, string propName, string searchName) where T : Component
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null) 
            {
                return null;
            }

            if (prop.objectReferenceValue != null)
            {
                T currentObj = prop.objectReferenceValue as T;
                if (currentObj != null)
                {
                    if (!currentObj.gameObject.name.ToLower().Contains(searchName.ToLower()))
                    {
                        prop.objectReferenceValue = null;
                        so.ApplyModifiedProperties();
                    }
                    else
                    {
                        return currentObj;
                    }
                }
                else 
                {
                    return null;
                }
            }

            T[] components = root.GetComponentsInChildren<T>(true);
            T bestMatch = null;
            foreach (var c in components)
            {
                if (c.gameObject.name.ToLower().Contains(searchName.ToLower()))
                {
                    bestMatch = c;
                    break;
                }
            }

            if (bestMatch == null)
            {
                GameObject go = new GameObject(searchName, typeof(RectTransform));
                go.transform.SetParent(root.transform, false);
                if (typeof(T) == typeof(TextMeshProUGUI))
                {
                    go.AddComponent<CanvasRenderer>();
                    bestMatch = go.AddComponent<TextMeshProUGUI>() as T;
                    ((TextMeshProUGUI)(object)bestMatch).text = searchName;
                    ((TextMeshProUGUI)(object)bestMatch).color = Color.black;
                    ((TextMeshProUGUI)(object)bestMatch).alignment = TextAlignmentOptions.Center;
                }
                else if (typeof(T) == typeof(Button))
                {
                    go.AddComponent<CanvasRenderer>();
                    go.AddComponent<Image>();
                    bestMatch = go.AddComponent<Button>() as T;
                }
                else if (typeof(T) == typeof(Image))
                {
                    go.AddComponent<CanvasRenderer>();
                    bestMatch = go.AddComponent<Image>() as T;
                }
                else 
                {
                    bestMatch = go.AddComponent<T>();
                }
            }

            prop.objectReferenceValue = bestMatch;
            so.ApplyModifiedProperties();
            return bestMatch;
        }

        private static void AutoLinkGameObject(GameObject root, SerializedObject so, string propName, string searchName)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null) return;
            
            if (prop.objectReferenceValue != null)
            {
                GameObject currentObj = prop.objectReferenceValue as GameObject;
                if (currentObj != null)
                {
                    if (!currentObj.name.ToLower().Contains(searchName.ToLower()))
                    {
                        prop.objectReferenceValue = null;
                        so.ApplyModifiedProperties();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            Transform bestMatch = null;
            foreach (var c in transforms)
            {
                if (c.gameObject.name.ToLower().Contains(searchName.ToLower()))
                {
                    bestMatch = c;
                    break;
                }
            }

            if (bestMatch == null)
            {
                GameObject go = new GameObject(searchName, typeof(RectTransform));
                go.transform.SetParent(root.transform, false);
                bestMatch = go.transform;
            }

            prop.objectReferenceValue = bestMatch.gameObject;
            so.ApplyModifiedProperties();
        }

        private static SerializedObject GetScriptSO(GameObject root, string typeNameSubstring)
        {
            var components = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var comp in components)
            {
                if (comp != null && comp.GetType().Name.Contains(typeNameSubstring))
                {
                    return new SerializedObject(comp);
                }
            }
            return null;
        }

        private static void FixArenaShopItem(GameObject root)
        {
            var so = GetScriptSO(root, "ArenaShopItem");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "itemNameText", "Name");
            AutoLink<TextMeshProUGUI>(root, so, "priceText", "Price");
            AutoLink<Button>(root, so, "buyButton", "Buy");
        }

        private static void FixInventoryEquipmentCard(GameObject root)
        {
            var so = GetScriptSO(root, "EquipmentCard");
            if (so == null) so = GetScriptSO(root, "InventoryEquipmentCard");
            if (so == null) return;
            AutoLink<Image>(root, so, "iconImage", "Icon");
            AutoLink<Image>(root, so, "rarityBorder", "Border");
            AutoLink<TextMeshProUGUI>(root, so, "levelText", "Level");
            AutoLink<TextMeshProUGUI>(root, so, "nameText", "Name");
            AutoLink<TextMeshProUGUI>(root, so, "mainStatText", "MainStat");
            AutoLink<TextMeshProUGUI>(root, so, "subStatsText", "SubStat");
            AutoLink<Button>(root, so, "clickButton", "ClickButton");
        }

        private static void FixInventoryItemCard(GameObject root)
        {
            var so = GetScriptSO(root, "ItemCard");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "itemNameText", "Name");
            AutoLink<TextMeshProUGUI>(root, so, "itemCountText", "Count");
            AutoLink<TextMeshProUGUI>(root, so, "itemDescriptionText", "Desc");
            AutoLink<Image>(root, so, "itemIcon", "Icon");
            AutoLink<Button>(root, so, "useButton", "Use");
        }

        private static void FixPopulationHeroCard(GameObject root)
        {
            var so = GetScriptSO(root, "HeroCard"); // or PopulationHeroCard
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "heroNameText", "Name");
            AutoLink<TextMeshProUGUI>(root, so, "levelText", "Level");
            AutoLink<TextMeshProUGUI>(root, so, "professionText", "Profession");
            AutoLink<TextMeshProUGUI>(root, so, "cpText", "CP");
            AutoLink<Image>(root, so, "avatarImage", "Avatar");
            AutoLink<Button>(root, so, "dismissButton", "Dismiss");
        }

        private static void FixQuestItem(GameObject root)
        {
            var so = GetScriptSO(root, "QuestItem");
            if (so == null) return;
            AutoLink<Image>(root, so, "rewardIcon", "RewardIcon");
            AutoLinkGameObject(root, so, "completedIndicator", "Completed");
        }

        private static void FixReportItem(GameObject root)
        {
            var so = GetScriptSO(root, "ReportItem");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "outcomeText", "Outcome");
        }

        private static void FixSquadSlot(GameObject root)
        {
            var so = GetScriptSO(root, "SquadSlot");
            if (so == null) return;
            AutoLink<Button>(root, so, "slotButton", "SlotButton");
        }

        private static void FixHeroInfoPanel(GameObject root)
        {
            var so = GetScriptSO(root, "HeroInfoPanel");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "evasionText", "Evasion");
            AutoLink<TextMeshProUGUI>(root, so, "dmgReductionText", "DmgRed");
            AutoLink<TextMeshProUGUI>(root, so, "dmgIncreaseText", "DmgInc");
            AutoLink<Button>(root, so, "statAllocationButton", "StatAllocBtn");
            AutoLink<Button>(root, so, "traitUpgradeButton", "TraitUpgBtn");
            AutoLink<Button>(root, so, "useExpItemButton", "UseExpBtn");
        }

        private static void FixArenaPanel(GameObject root)
        {
            var so = GetScriptSO(root, "ArenaPanel");
            if (so == null) return;
            AutoLink<Button>(root, so, "addTicketAdButton", "TicketAd");
            AutoLink<TextMeshProUGUI>(root, so, "rankNameText", "RankName");
            AutoLink<TextMeshProUGUI>(root, so, "rankPointsText", "RankPoints");
            AutoLink<TextMeshProUGUI>(root, so, "ticketsText", "Tickets");
            AutoLink<Image>(root, so, "rankIconImage", "RankIcon");
            AutoLink<Button>(root, so, "leaderboardButton", "Leaderboard");
            AutoLink<Button>(root, so, "shopButton", "Shop");
            AutoLink<Button>(root, so, "upgradeBuildingButton", "Upgrade");
        }

        private static void FixBarrackPanel(GameObject root)
        {
            var so = GetScriptSO(root, "BarrackPanel");
            if (so == null) so = GetScriptSO(root, "UIMainController");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "panelTitleText", "Title");
            AutoLink<Button>(root, so, "populationManagerButton", "PopManager");
        }

        private static void FixBattlePanel(GameObject root)
        {
            var so = GetScriptSO(root, "BattlePanel");
            if (so == null) so = GetScriptSO(root, "CombatVisualizer");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "skipButtonText", "SkipText");
            AutoLink<TextMeshProUGUI>(root, so, "victoryTitleText", "Victory");
            AutoLink<TextMeshProUGUI>(root, so, "defeatTitleText", "Defeat");
        }

        private static void FixStatAllocationPanel(GameObject root)
        {
            var so = GetScriptSO(root, "StatAllocation");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "panelTitleText", "Title");
            AutoLink<TextMeshProUGUI>(root, so, "confirmButtonText", "Confirm");
        }

        private static void FixTraitUpgradePanel(GameObject root)
        {
            var so = GetScriptSO(root, "TraitUpgrade");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "panelTitleText", "Title");
            AutoLink<TextMeshProUGUI>(root, so, "closeButtonText", "Close");
        }

        private static void FixInventoryPanel(GameObject root)
        {
            var so = GetScriptSO(root, "InventoryPanel");
            if (so == null) return;
            AutoLinkGameObject(root, so, "detailView", "DetailView");
            AutoLink<Image>(root, so, "detailIcon", "DetailIcon");
            AutoLink<TextMeshProUGUI>(root, so, "detailNameText", "DetailName");
            AutoLink<TextMeshProUGUI>(root, so, "detailDescText", "DetailDesc");
            AutoLink<TextMeshProUGUI>(root, so, "detailCountText", "DetailCount");
            AutoLink<Button>(root, so, "useButton", "Use");
        }

        private static void FixQuestPanel(GameObject root)
        {
            var so = GetScriptSO(root, "QuestPanel");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "panelTitleText", "Title");
        }

        private static void FixEquipmentDetailPanel(GameObject root)
        {
            var so = GetScriptSO(root, "EquipmentDetail");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "equipNameText", "EquipName");
        }

        private static void FixBuildingUpgradePanel(GameObject root)
        {
            var so = GetScriptSO(root, "BuildingUpgradePanel");
            if (so == null) return;
            AutoLink<TextMeshProUGUI>(root, so, "upgradeTimerText", "Timer");
            AutoLink<TextMeshProUGUI>(root, so, "benefitText", "Benefit");
            AutoLink<Button>(root, so, "speedUpAdButton", "SpeedUpAd");
        }

        private static void FixInjuredHeroCard(GameObject root)
        {
            // Just leaving this empty unless specific properties are needed. 
            // In the unassigned report, it was primarily parentLinkedComponent which is handled gracefully.
        }
    }
}
