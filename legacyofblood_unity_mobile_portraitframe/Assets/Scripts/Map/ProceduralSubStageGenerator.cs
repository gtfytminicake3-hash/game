using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegendOfBlood
{
    public enum SubStageNodeType
    {
        Start, Combat, Elite, Event, Shop, Reward, Boss
    }

    public enum ProceduralDifficulty
    {
        Normal = 0,
        Hard = 1,
        Hell = 2,
        Nightmare = 3
    }

    public enum NodeStatus
    {
        Locked,             // Chưa mở, trạng thái chưa xử lý
        Available,          // Đang sáng, có thể click đánh
        Cleared,            // Đã đánh thắng
        BlockedByChoice,    // Bị khóa vì đã chọn nhánh khác
        FutureLocked        // Nằm phía sau route hiện tại, chưa sáng
    }

    public enum EdgeState
    {
        Active,             // Đang dùng cho route hiện tại
        BlockedByChoice,    // Bị khóa vì chọn nhánh khác
        Future              // Đường đi trong tương lai, chưa đụng tới
    }

    // 1. BIỂU DIỄN PATH (SHAPE DRIVEN)
    public class PathSegment
    {
        public Vector2Int From { get; set; }
        public Vector2Int To { get; set; }
        public bool IsActive { get; set; } = true;

        public PathSegment(Vector2Int from, Vector2Int to)
        {
            From = from;
            To = to;
        }
    }

    public class MapShapeTemplate
    {
        public string Name { get; set; }
        public ProceduralDifficulty DifficultyLevel { get; set; }
        public List<PathSegment> Segments { get; set; }
    }

    public class ShapeDrivenMapData
    {
        public List<SubStageNode> Nodes { get; set; }
        public List<PathSegment> DrawnSegments { get; set; }
        public ProceduralDifficulty Difficulty { get; set; }
        public int CurrentNodeId { get; set; } = -1;

        // --- Added for Route Progression ---
        public List<int> CurrentFrontierNodeIds { get; set; } = new List<int>();
        public Dictionary<string, EdgeState> EdgeStates { get; set; } = new Dictionary<string, EdgeState>();
    }

    public class SubStageNode
    {
        private const int MAX_FLOORS = 15;
        // X can expand based on template, originally 0->4 (5 slots), now can be up to 7 slots for nightmare
        public int Id { get; set; }
        public int Floor { get; set; }  
        public int Slot { get; set; }   
        public SubStageNodeType Type { get; set; }
        public NodeStatus Status { get; set; }
        public List<int> OutgoingEdges { get; set; }
        public List<int> IncomingEdges { get; set; }
        
        // --- NEW FEATURES ---
        public List<string> ExpectedMonsters { get; set; }
        public List<string> ExpectedRewards { get; set; }
        
        // Lưu trạng thái quái vật còn sống sau khi đội quân thua.
        public List<HeroData> SurvivingEnemies { get; set; } = null;
        // --------------------

        public SubStageNode(int id, int floor, int slot)
        {
            Id = id;
            Floor = floor;
            Slot = slot;
            Type = SubStageNodeType.Combat;
            Status = NodeStatus.Locked;
            OutgoingEdges = new List<int>();
            IncomingEdges = new List<int>();
            ExpectedMonsters = new List<string>();
            ExpectedRewards = new List<string>();
            SurvivingEnemies = null;
        }
    }

    public class ProceduralSubStageGenerator
    {
        public ShapeDrivenMapData GenerateMap(ProceduralDifficulty mode = ProceduralDifficulty.Normal)
        {
            var allTemplates = GetTemplates();
            // Filter templates based on difficulty
            var validTemplates = allTemplates.Where(t => t.DifficultyLevel == mode).ToList();
            if (validTemplates.Count == 0) validTemplates = allTemplates; // Fallback
            
            var template = validTemplates[Random.Range(0, validTemplates.Count)];

            // 2. SINH NODE TỪ PATH (Mọi điểm giao / đầu cuối)
            HashSet<Vector2Int> uniquePoints = new HashSet<Vector2Int>();
            foreach(var seg in template.Segments)
            {
                if (!seg.IsActive) continue;
                uniquePoints.Add(seg.From);
                uniquePoints.Add(seg.To);
            }

            List<SubStageNode> nodes = new List<SubStageNode>();
            int idCounter = 0;
            Dictionary<Vector2Int, SubStageNode> posToNode = new Dictionary<Vector2Int, SubStageNode>();

            foreach(var pt in uniquePoints)
            {
                SubStageNode node = new SubStageNode(idCounter++, pt.y, pt.x);
                nodes.Add(node);
                posToNode[pt] = node;
            }

            // 3. GÁN LOẠI NODE VÀ DỮ LIỆU ĐI KÈM
            int minFloor = uniquePoints.Min(p => p.y);
            int maxFloor = uniquePoints.Max(p => p.y);
            int centerX = 2; 

            foreach(var node in nodes)
            {
                if (node.Floor == minFloor) node.Type = SubStageNodeType.Start;
                else if (node.Floor == maxFloor) node.Type = SubStageNodeType.Boss;
                else if (node.Floor == maxFloor - 1) node.Type = SubStageNodeType.Reward;
                else
                {
                    if (node.Slot == centerX)
                    {
                        node.Type = Random.value < 0.5f ? SubStageNodeType.Elite : SubStageNodeType.Combat;
                    }
                    else
                    {
                        float r = Random.value;
                        if (r < 0.4f) node.Type = SubStageNodeType.Event;
                        else if (r < 0.7f) node.Type = SubStageNodeType.Shop;
                        else node.Type = SubStageNodeType.Combat;
                    }
                }

                // Gán dữ liệu quái / đồ dựa trên Type
                AssignNodeData(node, mode);
            }

            // 4. KIẾN TẠO MẠNG LƯỚI EDGE THEO HƯỚNG CHẢY LÊN
            foreach(var seg in template.Segments)
            {
                if (!seg.IsActive) continue;
                var nFrom = posToNode[seg.From];
                var nTo = posToNode[seg.To];
                
                // Dọc từ dưới lên trên
                if (nTo.Floor > nFrom.Floor)
                {
                    nFrom.OutgoingEdges.Add(nTo.Id);
                    nTo.IncomingEdges.Add(nFrom.Id);
                }
                else if (nFrom.Floor > nTo.Floor)
                {
                    nTo.OutgoingEdges.Add(nFrom.Id);
                    nFrom.IncomingEdges.Add(nTo.Id);
                }
                else // Ngang: Theo hướng từ Giữa tỏa ra 2 biên
                {
                    if (Mathf.Abs(nFrom.Slot - centerX) < Mathf.Abs(nTo.Slot - centerX))
                    {
                        nFrom.OutgoingEdges.Add(nTo.Id);
                        nTo.IncomingEdges.Add(nFrom.Id);
                    }
                    else
                    {
                        nTo.OutgoingEdges.Add(nFrom.Id);
                        nFrom.IncomingEdges.Add(nTo.Id);
                    }
                }
            }

            var mapData = new ShapeDrivenMapData {
                Nodes = nodes.OrderBy(n => n.Floor).ThenBy(n => n.Slot).ToList(),
                DrawnSegments = template.Segments.Where(s => s.IsActive).ToList(),
                Difficulty = mode
            };

            LegendOfBlood.Map.BranchRouteProgressionService.InitializeMap(mapData);

            return mapData;
        }

        private void AssignNodeData(SubStageNode node, ProceduralDifficulty mode)
        {
            int diffFactor = (int)mode + 1;
            
            // Random Monsters
            if (node.Type == SubStageNodeType.Combat || node.Type == SubStageNodeType.Elite || node.Type == SubStageNodeType.Boss)
            {
                int count = node.Type == SubStageNodeType.Boss ? 1 : Random.Range(2, 4 + diffFactor);
                string[] possibleM = { "Goblin", "Orc", "Skeleton", "Zombie", "Slime", "DarkMage" };
                for (int i = 0; i < count; i++)
                {
                    node.ExpectedMonsters.Add($"{possibleM[Random.Range(0, possibleM.Length)]} x{Random.Range(1, 3 * diffFactor)}");
                }
            }

            // Random Rewards
            if (node.Type == SubStageNodeType.Combat || node.Type == SubStageNodeType.Elite || 
                node.Type == SubStageNodeType.Boss || node.Type == SubStageNodeType.Reward)
            {
                node.ExpectedRewards.Add($"Gold x{Random.Range(10, 50) * diffFactor}");
                if (Random.value < 0.3f + (0.1f * diffFactor)) {
                    node.ExpectedRewards.Add("Exp Potion x1");
                }
                if (node.Type == SubStageNodeType.Elite || node.Type == SubStageNodeType.Boss) {
                    node.ExpectedRewards.Add($"Relic Tier {diffFactor}");
                }
            }
        }

        private List<MapShapeTemplate> GetTemplates()
        {
            var shapeBox = new MapShapeTemplate() { Name = "Box Architecture (15F)", DifficultyLevel = ProceduralDifficulty.Normal, Segments = new List<PathSegment>() };
            shapeBox.Segments = new List<PathSegment>
            {
                new PathSegment(new Vector2Int(2,0), new Vector2Int(2,1)),
                
                // Hộp 1: Tầng 1 -> Tầng 3
                new PathSegment(new Vector2Int(2,1), new Vector2Int(1,1)),
                new PathSegment(new Vector2Int(2,1), new Vector2Int(3,1)),
                new PathSegment(new Vector2Int(1,1), new Vector2Int(1,3)), 
                new PathSegment(new Vector2Int(3,1), new Vector2Int(3,3)), 
                new PathSegment(new Vector2Int(2,1), new Vector2Int(2,3)), 
                new PathSegment(new Vector2Int(1,3), new Vector2Int(2,3)), 
                new PathSegment(new Vector2Int(3,3), new Vector2Int(2,3)), 

                new PathSegment(new Vector2Int(2,3), new Vector2Int(2,4)),

                // Hộp 2: Tầng 4 -> Tầng 6
                new PathSegment(new Vector2Int(2,4), new Vector2Int(0,4)),
                new PathSegment(new Vector2Int(2,4), new Vector2Int(4,4)),
                new PathSegment(new Vector2Int(0,4), new Vector2Int(0,6)), 
                new PathSegment(new Vector2Int(4,4), new Vector2Int(4,6)), 
                new PathSegment(new Vector2Int(2,4), new Vector2Int(2,6)), 
                new PathSegment(new Vector2Int(0,6), new Vector2Int(2,6)), 
                new PathSegment(new Vector2Int(4,6), new Vector2Int(2,6)), 

                new PathSegment(new Vector2Int(2,6), new Vector2Int(2,7)),

                // Hộp 3: Tầng 7 -> Tầng 9
                new PathSegment(new Vector2Int(2,7), new Vector2Int(1,7)),
                new PathSegment(new Vector2Int(2,7), new Vector2Int(3,7)),
                new PathSegment(new Vector2Int(1,7), new Vector2Int(1,9)), 
                new PathSegment(new Vector2Int(3,7), new Vector2Int(3,9)), 
                new PathSegment(new Vector2Int(2,7), new Vector2Int(2,9)), 
                new PathSegment(new Vector2Int(1,9), new Vector2Int(2,9)), 
                new PathSegment(new Vector2Int(3,9), new Vector2Int(2,9)), 

                new PathSegment(new Vector2Int(2,9), new Vector2Int(2,10)),

                // Hộp 4: Tầng 10 -> Tầng 12
                new PathSegment(new Vector2Int(2,10), new Vector2Int(0,10)),
                new PathSegment(new Vector2Int(2,10), new Vector2Int(4,10)),
                new PathSegment(new Vector2Int(0,10), new Vector2Int(0,12)), 
                new PathSegment(new Vector2Int(4,10), new Vector2Int(4,12)), 
                new PathSegment(new Vector2Int(2,10), new Vector2Int(2,12)), 
                new PathSegment(new Vector2Int(0,12), new Vector2Int(2,12)), 
                new PathSegment(new Vector2Int(4,12), new Vector2Int(2,12)), 

                // Đỉnh chóp
                new PathSegment(new Vector2Int(2,12), new Vector2Int(2,13)),
                new PathSegment(new Vector2Int(2,13), new Vector2Int(2,14))
            };

            var shapeX = new MapShapeTemplate() { Name = "X Factor (15F)", DifficultyLevel = ProceduralDifficulty.Normal, Segments = new List<PathSegment>() };
            shapeX.Segments = new List<PathSegment>
            {
                new PathSegment(new Vector2Int(2,0), new Vector2Int(2,1)),
                
                // Mở hẹp
                new PathSegment(new Vector2Int(2,1), new Vector2Int(1,1)),
                new PathSegment(new Vector2Int(2,1), new Vector2Int(3,1)),
                new PathSegment(new Vector2Int(1,1), new Vector2Int(1,3)),
                new PathSegment(new Vector2Int(3,1), new Vector2Int(3,3)),
                
                // Trục chéo tạo chữ X 1
                new PathSegment(new Vector2Int(1,3), new Vector2Int(2,4)),
                new PathSegment(new Vector2Int(3,3), new Vector2Int(2,4)),
                
                // Mở hẹp 2
                new PathSegment(new Vector2Int(2,4), new Vector2Int(1,5)),
                new PathSegment(new Vector2Int(2,4), new Vector2Int(3,5)),
                
                new PathSegment(new Vector2Int(1,5), new Vector2Int(1,7)),
                new PathSegment(new Vector2Int(3,5), new Vector2Int(3,7)),
                
                // Trục chéo tạo chữ X 2
                new PathSegment(new Vector2Int(1,7), new Vector2Int(2,8)),
                new PathSegment(new Vector2Int(3,7), new Vector2Int(2,8)),

                // Mở rộng ra cực đại ngã tư giữa
                new PathSegment(new Vector2Int(2,8), new Vector2Int(0,9)),
                new PathSegment(new Vector2Int(2,8), new Vector2Int(4,9)),
                new PathSegment(new Vector2Int(0,9), new Vector2Int(0,12)),
                new PathSegment(new Vector2Int(4,9), new Vector2Int(4,12)),
                new PathSegment(new Vector2Int(0,12), new Vector2Int(2,13)),
                new PathSegment(new Vector2Int(4,12), new Vector2Int(2,13)),
                
                // Lên top
                new PathSegment(new Vector2Int(2,13), new Vector2Int(2,14))
            };

            // Hard, Hell, Nightmare mode just duplicates them to satisfy filter but adds chaos branching
            var shapeBoxHard = new MapShapeTemplate() { Name = "Box Architecture Hard", DifficultyLevel = ProceduralDifficulty.Hard, Segments = new List<PathSegment>(shapeBox.Segments) };
            var shapeXHard = new MapShapeTemplate() { Name = "X Factor Hard", DifficultyLevel = ProceduralDifficulty.Hard, Segments = new List<PathSegment>(shapeX.Segments) };
            var shapeBoxHell = new MapShapeTemplate() { Name = "Box Architecture Hell", DifficultyLevel = ProceduralDifficulty.Hell, Segments = new List<PathSegment>(shapeBox.Segments) };
            var shapeXHell = new MapShapeTemplate() { Name = "X Factor Hell", DifficultyLevel = ProceduralDifficulty.Hell, Segments = new List<PathSegment>(shapeX.Segments) };
            var shapeNightmare = new MapShapeTemplate() { Name = "Nightmare", DifficultyLevel = ProceduralDifficulty.Nightmare, Segments = new List<PathSegment>(shapeBox.Segments) };
            
            // Add some extra random lateral branches for chaos in higher difficulties
            for (int f = 1; f < 13; f++)
            {
                shapeBoxHard.Segments.Add(new PathSegment(new Vector2Int(2, f), new Vector2Int(Random.Range(1,4), f+1)));
                shapeBoxHell.Segments.Add(new PathSegment(new Vector2Int(2, f), new Vector2Int(Random.Range(0,5), f+1)));
                shapeNightmare.Segments.Add(new PathSegment(new Vector2Int(2, f), new Vector2Int(Random.Range(0,5), f+1)));
                shapeNightmare.Segments.Add(new PathSegment(new Vector2Int(Random.Range(1,4), f), new Vector2Int(Random.Range(0,5), f+1)));
            }

            return new List<MapShapeTemplate> { shapeBox, shapeX, shapeBoxHard, shapeXHard, shapeBoxHell, shapeXHell, shapeNightmare };
        }
    }
}
