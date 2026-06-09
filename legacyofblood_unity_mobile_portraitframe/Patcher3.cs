using System;
using System.IO;
using System.Text.RegularExpressions;

public class Program
{
    public static void Main()
    {
        string path = @"Assets\Scripts\UI\CombatVisualizerPanel.cs";
        string content = File.ReadAllText(path);

        string replacement = @"        private BattleUnitUI InstantiateUnitInSlot(HeroData hData, string unitId, bool isAlly, int slotIndex)
        {
            Transform container = isAlly ? allyContainer : enemyContainer;
            Transform parentSlot = (container.childCount > slotIndex) ? container.GetChild(slotIndex) : container;

            GameObject go = Instantiate(battleUnitPrefab, parentSlot);
            go.SetActive(true);
            
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                go.transform.SetAsLastSibling();
            }
            BattleUnitUI unit = go.GetComponent<BattleUnitUI>();
            
            int maxHp = (int)hData.GetFinalStats().hp;
            int startHp = UnityEngine.Mathf.Max(0, (int)hData.currentHp);
            
            UnityEngine.Sprite sprite = LegendOfBlood.CardArtResolver.GetUnitSprite(hData, !isAlly);
            unit.Setup(sprite, maxHp, startHp);

            UnityEngine.Debug.Log($""[Spawn] Spawned hero card: {hData.heroName}, ID: {unitId}, Parent slot: {parentSlot.name}, Active: {go.activeSelf}"");

            return unit;
        }

        private void ClearBoard()
        {
            int allySlotCount = allyContainer.childCount;
            int enemySlotCount = enemyContainer.childCount;
            int destroyedAllyCards = 0;
            int destroyedEnemyCards = 0;

            _unitMap.Clear();

            foreach (Transform childOrSlot in allyContainer)
            {
                if (childOrSlot.GetComponent<BattleUnitUI>() != null)
                {
                    UnityEngine.Object.Destroy(childOrSlot.gameObject);
                    destroyedAllyCards++;
                }
                else
                {
                    foreach (Transform child in childOrSlot)
                    {
                        if (child.GetComponent<BattleUnitUI>() != null)
                        {
                            UnityEngine.Object.Destroy(child.gameObject);
                            destroyedAllyCards++;
                        }
                    }
                }
            }

            foreach (Transform childOrSlot in enemyContainer)
            {
                if (childOrSlot.GetComponent<BattleUnitUI>() != null)
                {
                    UnityEngine.Object.Destroy(childOrSlot.gameObject);
                    destroyedEnemyCards++;
                }
                else
                {
                    foreach (Transform child in childOrSlot)
                    {
                        if (child.GetComponent<BattleUnitUI>() != null)
                        {
                            UnityEngine.Object.Destroy(child.gameObject);
                            destroyedEnemyCards++;
                        }
                    }
                }
            }
        }";

        content = Regex.Replace(content, @"(?s)private BattleUnitUI InstantiateUnitInSlot\(HeroData hData, string unitId, bool isAlly, int slotIndex\).*?Debug\.Log\(\$""\[ClearBoard\].*?""\);\s*\}", replacement);
        
        // Remove _visualCurrentHp / _visualMaxHp / _simpleUnitPrefab
        content = Regex.Replace(content, @"\s*private Dictionary<string, int> _visualCurrentHp = new Dictionary<string, int>\(\);", "");
        content = Regex.Replace(content, @"\s*private Dictionary<string, int> _visualMaxHp = new Dictionary<string, int>\(\);", "");
        content = Regex.Replace(content, @"\s*private GameObject _simpleUnitPrefab;", "");

        File.WriteAllText(path, content);
        Console.WriteLine("Patch completed.");
    }
}
