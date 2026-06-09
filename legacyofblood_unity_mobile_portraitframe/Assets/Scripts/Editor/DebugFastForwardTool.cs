#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using LegendOfBlood.Managers;
using System.Linq;

namespace LegendOfBlood.EditorTools
{
    public class DebugFastForwardTool
    {
        [MenuItem("Tools/Debug/Complete Pending Node Battle Now")]
        public static void CompletePendingNodeBattleNow()
        {
            Debug.Log("Debug fast forward pending node battles");

            if (!Application.isPlaying)
            {
                Debug.Log("Must be in Play Mode to use this tool.");
                return;
            }

            var expMgr = GameManager.Instance?.ExpeditionManager;
            if (expMgr == null)
            {
                Debug.Log("ExpeditionManager not found.");
                return;
            }

            expMgr.CompleteAllPendingNodeBattlesDebug();
        }
    }
}
#endif
