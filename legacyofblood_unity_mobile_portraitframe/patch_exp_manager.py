import re

def patch_file():
    with open('Assets/Scripts/GameSystems/ExpeditionManager.cs', 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Update CreateAndDispatchActiveExpedition signature
    content = content.replace(
        "private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination,\n            long travelTimeMs, long combatTimeMs, ExpeditionReport report)",
        "private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination,\n            long travelTimeMs, long combatTimeMs, ExpeditionReport report, string nodeId = null)"
    )
    # also try single line just in case
    content = content.replace(
        "private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination, long travelTimeMs, long combatTimeMs, ExpeditionReport report)",
        "private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination, long travelTimeMs, long combatTimeMs, ExpeditionReport report, string nodeId = null)"
    )

    # 2. Update newExpedition object initialization
    content = content.replace(
        "poiId = destination.poiId,\n                currentState = ExpeditionState.Traveling,",
        "poiId = destination.poiId,\n                nodeId = nodeId,\n                currentState = ExpeditionState.Traveling,"
    )

    # 3. Update Tick() returning block
    # In the file, there is:
    #                     else if (expedition.currentState == ExpeditionState.Returning)
    #                     {
    #                         DataManager.Instance.Player.UnclaimedReports.Add(expedition.preCalculatedReport);
    # We want to wrap the inside with if (!string.IsNullOrEmpty(expedition.nodeId)) ... else { ... }
    
    tick_target = """                    else if (expedition.currentState == ExpeditionState.Returning)
                    {
                        DataManager.Instance.Player.UnclaimedReports.Add(expedition.preCalculatedReport);"""
    
    tick_replace = """                    else if (expedition.currentState == ExpeditionState.Returning)
                    {
                        if (!string.IsNullOrEmpty(expedition.nodeId))
                        {
                            ApplyPendingNodeBattleResult(expedition);
                            hasChanges = true;
                        }
                        else
                        {
                        DataManager.Instance.Player.UnclaimedReports.Add(expedition.preCalculatedReport);"""
    
    content = content.replace(tick_target, tick_replace)
    
    # Need to close the else block in Tick(). The end of Returning block is:
    #                             // World Map Node Thường hoặc Tower
    #                             AddReportToMailbox(expedition.preCalculatedReport);
    #                         }
    #                         hasChanges = true;
    #                     }
    tick_end_target = """                        }
                        hasChanges = true;
                    }"""
    tick_end_replace = """                        }
                        }
                        hasChanges = true;
                    }"""
    # Replace ONLY the FIRST occurrence after the tick target
    # Actually just split by the target and replace
    parts = content.split("else if (expedition.currentState == ExpeditionState.Returning)")
    if len(parts) > 1:
        # parts[1] contains the Returning block
        parts[1] = parts[1].replace("hasChanges = true;\n                    }", "}\nhasChanges = true;\n                    }", 1)
        content = parts[0] + "else if (expedition.currentState == ExpeditionState.Returning)" + parts[1]

    # 4. Append new methods before the final closing brace of the class.
    # Find the last "}" before the end of the file.
    
    new_methods = """
        public void StartNodeExpedition(List<string> squadHeroIDs, ExpeditionReport report)
        {
            long travelTime = 0; // Nodes are immediate travel
            long combatTimeMs = EXPLORATION_TIME_MS + (report.combatResult.TotalTurns * COMBAT_TURN_DURATION_MS);
            
            var destination = DataManager.Instance.GetPOIByID(report.poiId);
            if (destination == null) destination = new POIData { poiId = report.poiId, poiName = report.poiName };
            
            CreateAndDispatchActiveExpedition(squadHeroIDs, destination, travelTime, combatTimeMs, report, report.nodeId);
        }

        public void ApplyPendingNodeBattleResult(ActiveExpedition pendingBattle)
        {
            if (pendingBattle == null) return;
            
            DataManager.Instance.Player.UnclaimedReports.Add(pendingBattle.preCalculatedReport);
            _activeExpeditions.Remove(pendingBattle);

            OnExpeditionFinished?.Invoke(pendingBattle.expeditionId);
            OnNewReportReceived?.Invoke();
            
            if (!string.IsNullOrEmpty(pendingBattle.nodeId))
            {
                LegendOfBlood.UI.POI_InfoPanel.ApplyPendingNodeBattleResult(pendingBattle.preCalculatedReport);
            }
            
            if (GameManager.Instance != null && GameManager.Instance.UINotificationManager != null)
            {
                string resStr = pendingBattle.preCalculatedReport.combatResult.DidPlayerWin ? "Chiến thắng" : "Thất bại";
                GameManager.Instance.UINotificationManager.ShowNotification($"Kết quả đánh Node: {resStr}. Vui lòng kiểm tra Hòm Thư!");
            }
            
            Debug.Log($"Applying battle result by debug fast forward using same timer flow for expedition {pendingBattle.expeditionId}");
            Debug.Log($"report delivered to Mailbox");
            Debug.Log($"replay EventLog count: {(pendingBattle.preCalculatedReport.combatResult?.EventLog?.Count ?? 0)}");
        }

        public void CompleteAllPendingNodeBattlesDebug()
        {
            Debug.Log("Debug fast forward pending node battles");
            if (_activeExpeditions == null || _activeExpeditions.Count == 0)
            {
                Debug.Log("No pending node battle found");
                return;
            }

            long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            bool foundAny = false;

            foreach (var expedition in _activeExpeditions.ToList())
            {
                if (!string.IsNullOrEmpty(expedition.nodeId))
                {
                    foundAny = true;
                    Debug.Log($"battle id: {expedition.expeditionId}");
                    Debug.Log($"node id: {expedition.nodeId}");
                    Debug.Log($"old endsAt: {expedition.stateEndTimestamp}");
                    
                    expedition.stateEndTimestamp = currentTime;
                    Debug.Log($"new endsAt: {expedition.stateEndTimestamp}");
                    Debug.Log($"result victory: {expedition.preCalculatedReport.combatResult.DidPlayerWin}");
                    Debug.Log($"EventLog count: {expedition.preCalculatedReport.combatResult?.EventLog?.Count}");
                    
                    ApplyPendingNodeBattleResult(expedition);
                }
            }

            if (!foundAny)
            {
                Debug.Log("No pending node battle found");
            }
            else
            {
                DataManager.Instance.SavePlayerData();
            }
        }
    }
}
"""
    # Replace the last "} }" with the new methods
    content = content.rsplit("    }\n}", 1)
    if len(content) == 2:
        content = content[0] + new_methods
    else:
        # Try finding just "}"
        content = content.rsplit("}", 2)[0] + new_methods

    with open('Assets/Scripts/GameSystems/ExpeditionManager.cs', 'w', encoding='utf-8') as f:
        f.write(content)

    print("Patched correctly.")

if __name__ == '__main__':
    patch_file()
