using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegendOfBlood.Map
{
    public static class BranchRouteProgressionService
    {
        public static string GetEdgeKey(int parentId, int childId)
        {
            return $"{parentId}_{childId}";
        }

        public static void InitializeMap(ShapeDrivenMapData mapData)
        {
            // Set all nodes to FutureLocked by default
            foreach (var node in mapData.Nodes)
            {
                node.Status = NodeStatus.FutureLocked;
            }

            // Set all edges to Future by default
            mapData.EdgeStates.Clear();
            foreach (var node in mapData.Nodes)
            {
                foreach (var childId in node.OutgoingEdges)
                {
                    mapData.EdgeStates[GetEdgeKey(node.Id, childId)] = EdgeState.Future;
                }
            }

            mapData.CurrentFrontierNodeIds.Clear();

            var startNode = mapData.Nodes.FirstOrDefault(n => n.Type == SubStageNodeType.Start);
            if (startNode != null)
            {
                mapData.CurrentNodeId = startNode.Id;
                startNode.Status = NodeStatus.Cleared;
                mapData.CurrentFrontierNodeIds.Add(startNode.Id);

                // Activate edges from start node
                foreach (var nextId in startNode.OutgoingEdges)
                {
                    mapData.EdgeStates[GetEdgeKey(startNode.Id, nextId)] = EdgeState.Active;
                }
            }

            RefreshNodeAvailability(mapData);
            
            Debug.Log($"[ProgressionService] Initialized map. Frontier: {string.Join(",", mapData.CurrentFrontierNodeIds)}");
        }

        public static void CommitChoice(ShapeDrivenMapData mapData, int selectedNodeId)
        {
            var selectedNode = mapData.Nodes.Find(x => x.Id == selectedNodeId);
            if (selectedNode == null) return;

            // Find all active parents of the selected node
            List<int> activeParentIds = new List<int>();
            foreach (var pid in selectedNode.IncomingEdges)
            {
                string edgeKey = GetEdgeKey(pid, selectedNodeId);
                if (mapData.EdgeStates.TryGetValue(edgeKey, out EdgeState state) && state == EdgeState.Active)
                {
                    activeParentIds.Add(pid);
                }
            }

            foreach (var choiceGroupId in activeParentIds)
            {
                var parentNode = mapData.Nodes.Find(x => x.Id == choiceGroupId);
                if (parentNode == null) continue;

                List<int> blockedSiblings = new List<int>();
                List<string> blockedEdgesLog = new List<string>();
                List<int> skippedMergeNodesLog = new List<int>();

                foreach (var optionId in parentNode.OutgoingEdges)
                {
                    string edgeKey = GetEdgeKey(parentNode.Id, optionId);
                    if (optionId == selectedNodeId)
                    {
                        mapData.EdgeStates[edgeKey] = EdgeState.Active;
                    }
                    else
                    {
                        mapData.EdgeStates[edgeKey] = EdgeState.BlockedByChoice;
                        blockedEdgesLog.Add(edgeKey);
                        blockedSiblings.Add(optionId);
                        
                        var optionNode = mapData.Nodes.Find(x => x.Id == optionId);
                        if (optionNode != null)
                        {
                            BlockUniqueBranchPath(mapData, optionNode, skippedMergeNodesLog);
                        }
                    }
                }

                Debug.Log($"[ProgressionService] CommitChoice ChoiceGroup={choiceGroupId}, Selected={selectedNodeId}, BlockedSiblings={string.Join(",", blockedSiblings)}, BlockedEdges={string.Join(",", blockedEdgesLog)}, SkippedMergeNodes={string.Join(",", skippedMergeNodesLog)}");
            }

            RefreshNodeAvailability(mapData);
        }

        private static bool IsMergeNode(SubStageNode node)
        {
            return node != null && node.IncomingEdges.Count > 1;
        }

        private static bool HasActiveIncomingParent(ShapeDrivenMapData mapData, SubStageNode node)
        {
            foreach (var pid in node.IncomingEdges)
            {
                if (mapData.EdgeStates.TryGetValue(GetEdgeKey(pid, node.Id), out EdgeState state))
                {
                    if (state == EdgeState.Active)
                        return true;
                }
            }
            return false;
        }

        private static void BlockUniqueBranchPath(ShapeDrivenMapData mapData, SubStageNode startNode, List<int> skippedMergeNodesLog)
        {
            Queue<SubStageNode> queue = new Queue<SubStageNode>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();

                // Stop if merge node (and don't block it)
                if (IsMergeNode(curr))
                {
                    skippedMergeNodesLog.Add(curr.Id);
                    continue;
                }

                // Stop if has active incoming parent from selected route
                if (HasActiveIncomingParent(mapData, curr))
                {
                    continue;
                }

                curr.Status = NodeStatus.BlockedByChoice;

                foreach (var childId in curr.OutgoingEdges)
                {
                    string edgeKey = GetEdgeKey(curr.Id, childId);
                    mapData.EdgeStates[edgeKey] = EdgeState.BlockedByChoice;
                    
                    var childNode = mapData.Nodes.Find(x => x.Id == childId);
                    if (childNode != null && childNode.Status != NodeStatus.BlockedByChoice)
                    {
                        queue.Enqueue(childNode);
                    }
                }
            }
        }

        public static void SetNodeCleared(ShapeDrivenMapData mapData, int clearedNodeId)
        {
            var node = mapData.Nodes.Find(x => x.Id == clearedNodeId);
            if (node == null) return;

            node.Status = NodeStatus.Cleared;

            // Update frontier
            if (!mapData.CurrentFrontierNodeIds.Contains(clearedNodeId))
            {
                mapData.CurrentFrontierNodeIds.Add(clearedNodeId);
            }

            // Remove parents from frontier
            foreach (var pid in node.IncomingEdges)
            {
                mapData.CurrentFrontierNodeIds.Remove(pid);
            }

            // Make outgoing edges active for direct children
            List<int> openedChildren = new List<int>();
            List<int> lockedChildren = new List<int>();

            foreach (var childId in node.OutgoingEdges)
            {
                string edgeKey = GetEdgeKey(node.Id, childId);
                // Only activate edge if not blocked by choice
                if (mapData.EdgeStates.ContainsKey(edgeKey) && mapData.EdgeStates[edgeKey] != EdgeState.BlockedByChoice)
                {
                    mapData.EdgeStates[edgeKey] = EdgeState.Active;
                }
            }

            RefreshNodeAvailability(mapData);

            // Log children states after refresh
            foreach (var childId in node.OutgoingEdges)
            {
                var cNode = mapData.Nodes.Find(x => x.Id == childId);
                if (cNode != null && cNode.Status == NodeStatus.Available)
                    openedChildren.Add(childId);
                else
                    lockedChildren.Add(childId);
            }

            Debug.Log($"[ProgressionService] SetNodeCleared Node={clearedNodeId}, OpenedChildren={string.Join(",", openedChildren)}, LockedChildren={string.Join(",", lockedChildren)}");
        }

        private static void RefreshNodeAvailability(ShapeDrivenMapData mapData)
        {
            int availableCount = 0;
            int blockedCount = 0;
            int futureCount = 0;

            foreach (var node in mapData.Nodes)
            {
                if (node.Status == NodeStatus.Cleared) continue;
                if (node.Status == NodeStatus.BlockedByChoice)
                {
                    blockedCount++;
                    continue;
                }

                NodeStatus oldStatus = node.Status;
                NodeStatus newStatus = NodeStatus.FutureLocked;
                string reason = "";

                if (IsReachableFromFrontier(mapData, node))
                {
                    List<int> activeParents = new List<int>();
                    int clearedActiveParents = 0;

                    foreach (var pid in node.IncomingEdges)
                    {
                        string edgeKey = GetEdgeKey(pid, node.Id);
                        if (mapData.EdgeStates.TryGetValue(edgeKey, out EdgeState eState) && eState == EdgeState.Active)
                        {
                            activeParents.Add(pid);
                            var pNode = mapData.Nodes.Find(x => x.Id == pid);
                            if (pNode != null && pNode.Status == NodeStatus.Cleared)
                            {
                                clearedActiveParents++;
                            }
                        }
                    }

                    if (activeParents.Count > 0)
                    {
                        if (clearedActiveParents > 0)
                        {
                            newStatus = NodeStatus.Available;
                            reason = "Any active parent is Cleared";
                        }
                        else
                        {
                            newStatus = NodeStatus.FutureLocked;
                            reason = "No active parent is Cleared yet";
                        }
                    }
                    else
                    {
                        newStatus = NodeStatus.FutureLocked;
                        reason = "No active edges connected";
                    }

                    if (oldStatus != newStatus)
                    {
                        Debug.Log($"[ProgressionService] Refresh Node={node.Id}, Old={oldStatus}, New={newStatus}, ActiveParents={string.Join(",", activeParents)}, ClearedActive={clearedActiveParents}, Reason: {reason}");
                    }
                }
                else
                {
                    newStatus = NodeStatus.FutureLocked;
                }

                node.Status = newStatus;

                if (node.Status == NodeStatus.Available) availableCount++;
                else if (node.Status == NodeStatus.FutureLocked) futureCount++;
            }

            Debug.Log($"[ProgressionService] RenderUI Count: Available={availableCount}, BlockedByChoice={blockedCount}, FutureLocked={futureCount}");
        }

        private static bool IsReachableFromFrontier(ShapeDrivenMapData mapData, SubStageNode targetNode)
        {
            // Check if there is any active path from any frontier node to targetNode
            // Only using edges that are EdgeState.Active
            Queue<int> queue = new Queue<int>(mapData.CurrentFrontierNodeIds);
            HashSet<int> visited = new HashSet<int>(mapData.CurrentFrontierNodeIds);

            while (queue.Count > 0)
            {
                int currId = queue.Dequeue();
                if (currId == targetNode.Id) return true;

                var currNode = mapData.Nodes.Find(x => x.Id == currId);
                if (currNode == null) continue;

                foreach (var childId in currNode.OutgoingEdges)
                {
                    string edgeKey = GetEdgeKey(currNode.Id, childId);
                    if (mapData.EdgeStates.TryGetValue(edgeKey, out EdgeState eState) && eState == EdgeState.Active)
                    {
                        if (!visited.Contains(childId))
                        {
                            visited.Add(childId);
                            queue.Enqueue(childId);
                        }
                    }
                }
            }

            return false;
        }
    }
}
