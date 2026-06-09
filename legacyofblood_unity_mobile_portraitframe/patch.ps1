$content = Get-Content -Raw -Path "Assets\Scripts\GameSystems\ExpeditionManager.cs" -Encoding UTF8

# 1. Update CreateAndDispatchActiveExpedition signature
$target1 = "private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination,
            long travelTimeMs, long combatTimeMs, ExpeditionReport report)"
$replacement1 = "private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination,
            long travelTimeMs, long combatTimeMs, ExpeditionReport report, string nodeId = null)"
$content = $content.Replace($target1, $replacement1)

# also try single line just in case
$target1b = "private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination, long travelTimeMs, long combatTimeMs, ExpeditionReport report)"
$replacement1b = "private void CreateAndDispatchActiveExpedition(List<string> heroIds, POIData destination, long travelTimeMs, long combatTimeMs, ExpeditionReport report, string nodeId = null)"
$content = $content.Replace($target1b, $replacement1b)

# 2. Update newExpedition object initialization
$target2 = "poiId = destination.poiId,
                currentState = ExpeditionState.Traveling,"
$replacement2 = "poiId = destination.poiId,
                nodeId = nodeId,
                currentState = ExpeditionState.Traveling,"
$content = $content.Replace($target2, $replacement2)

# 3. Update Tick() returning block
$target3 = "                    else if (expedition.currentState == ExpeditionState.Returning)
                    {
                        DataManager.Instance.Player.UnclaimedReports.Add(expedition.preCalculatedReport);"
$replacement3 = "                    else if (expedition.currentState == ExpeditionState.Returning)
                    {
                        if (!string.IsNullOrEmpty(expedition.nodeId))
                        {
                            ApplyPendingNodeBattleResult(expedition);
                            hasChanges = true;
                        }
                        else
                        {
                        DataManager.Instance.Player.UnclaimedReports.Add(expedition.preCalculatedReport);"
$content = $content.Replace($target3, $replacement3)

# 4. Find the end of the Tick method to close the else block
$target4 = "                        }
                        hasChanges = true;
                    }"
$replacement4 = "                        }
                        }
                        hasChanges = true;
                    }"

# We only want to replace the FIRST occurrence of target4 that appears after "else if (expedition.currentState == ExpeditionState.Returning)"
$parts = $content -split "else if \(expedition.currentState == ExpeditionState.Returning\)"
if ($parts.Length -gt 1) {
    # Escape the regex special characters, though replace uses literal strings in .NET
    $parts[1] = $parts[1].Substring(0, $parts[1].IndexOf($target4)) + $replacement4 + $parts[1].Substring($parts[1].IndexOf($target4) + $target4.Length)
    $content = $parts[0] + "else if (expedition.currentState == ExpeditionState.Returning)" + $parts[1]
}

# 5. Append new methods before the final closing brace of the class.
$new_methods = @"
        public void StartNodeExpedition(System.Collections.Generic.List<string> squadHeroIDs, ExpeditionReport report)
        {
            long travelTime = 0; // Nodes are immediate travel
            long combatTimeMs = 2000 + (report.combatResult.TotalTurns * 3000);
            
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
            
            UnityEngine.Debug.Log($"Applying battle result by debug fast forward using same timer flow for expedition {pendingBattle.expeditionId}");
            UnityEngine.Debug.Log($"report delivered to Mailbox");
            UnityEngine.Debug.Log($"replay EventLog count: {(pendingBattle.preCalculatedReport.combatResult?.EventLog?.Count ?? 0)}");
        }

        public void CompleteAllPendingNodeBattlesDebug()
        {
            UnityEngine.Debug.Log("Debug fast forward pending node battles");
            if (_activeExpeditions == null || _activeExpeditions.Count == 0)
            {
                UnityEngine.Debug.Log("No pending node battle found");
                return;
            }

            long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            bool foundAny = false;

            foreach (var expedition in _activeExpeditions.ToArray())
            {
                if (!string.IsNullOrEmpty(expedition.nodeId))
                {
                    foundAny = true;
                    UnityEngine.Debug.Log($"battle id: {expedition.expeditionId}");
                    UnityEngine.Debug.Log($"node id: {expedition.nodeId}");
                    UnityEngine.Debug.Log($"old endsAt: {expedition.stateEndTimestamp}");
                    
                    expedition.stateEndTimestamp = currentTime;
                    UnityEngine.Debug.Log($"new endsAt: {expedition.stateEndTimestamp}");
                    UnityEngine.Debug.Log($"result victory: {expedition.preCalculatedReport.combatResult.DidPlayerWin}");
                    UnityEngine.Debug.Log($"EventLog count: {expedition.preCalculatedReport.combatResult?.EventLog?.Count}");
                    
                    ApplyPendingNodeBattleResult(expedition);
                }
            }

            if (!foundAny)
            {
                UnityEngine.Debug.Log("No pending node battle found");
            }
            else
            {
                DataManager.Instance.SavePlayerData();
            }
        }
"@

$lastBraceIndex = $content.LastIndexOf("}")
if ($lastBraceIndex -ge 0) {
    $secondLastBraceIndex = $content.LastIndexOf("}", $lastBraceIndex - 1)
    if ($secondLastBraceIndex -ge 0) {
        $content = $content.Substring(0, $secondLastBraceIndex) + $new_methods + "`r`n    }`r`n}"
    }
}

Set-Content -Path "Assets\Scripts\GameSystems\ExpeditionManager.cs" -Value $content -Encoding UTF8
Write-Host "Patched ExpeditionManager.cs"
