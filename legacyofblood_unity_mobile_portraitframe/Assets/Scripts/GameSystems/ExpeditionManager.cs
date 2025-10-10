namespace LegendOfBlood
{
    // BẮT BUỘC: Thêm using này và xóa các alias cũ
    using LegendOfBlood.Combat;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class ExpeditionManager : MonoBehaviour
    {
        private List<Expedition> _activeExpeditions;
        private List<Expedition> _expeditionsToRemove = new List<Expedition>();

        public static event Action<Expedition> OnExpeditionStarted;
        public static event Action<Expedition> OnExpeditionReturning;
        public static event Action<Expedition> OnExpeditionFinished;
        public static event Action<POIData> OnPOICleared;

        private void Start()
        {
            if (DataManager.Instance != null && DataManager.Instance.Player != null)
            {
                _activeExpeditions = DataManager.Instance.Player.ActiveExpeditions;
            }
            else
            {
                Debug.LogError("ExpeditionManager could not load expeditions because DataManager or PlayerData is not ready.");
                _activeExpeditions = new List<Expedition>();
            }
        }

        public void StartExpedition(List<string> squadHeroIDs, POIData destination)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long travelTime = CalculateTravelTime(destination.position);

            var newExpedition = new Expedition
            {
                id = Guid.NewGuid().ToString(),
                squadHeroIDs = squadHeroIDs,
                destination = destination,
                status = ExpeditionStatus.Traveling,
                startTime = currentTime,
                endTime = currentTime + travelTime 
            };
            
            _activeExpeditions.Add(newExpedition);
            OnExpeditionStarted?.Invoke(newExpedition);
        }

        public void Tick(float deltaTime)
        {
            if (_activeExpeditions == null || _activeExpeditions.Count == 0) return;
            
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var expedition in _activeExpeditions.ToList())
            {
                if (currentTime >= expedition.endTime)
                {
                    AdvanceExpeditionState(expedition);
                }
            }

            if (_expeditionsToRemove.Count > 0)
            {
                _activeExpeditions.RemoveAll(exp => _expeditionsToRemove.Contains(exp));
                _expeditionsToRemove.Clear();
            }
        }
        
        private void AdvanceExpeditionState(Expedition expedition)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            switch (expedition.status)
            {
                case ExpeditionStatus.Traveling:
                    expedition.status = ExpeditionStatus.Exploring;
                    
                    var heroSquadForCombat = expedition.squadHeroIDs
                                        .Select(id => DataManager.Instance.GetHeroByID(id))
                                        .Where(h => h != null && h.currentHp > 0)
                                        .ToList();

                    // SỬA LỖI: Cả hai bên của phép gán bây giờ đều là kiểu LegendOfBlood.Combat.CombatResult
                    // Biến combatResult được khai báo tường minh để đảm bảo đúng kiểu
                    CombatResult combatResult = GameManager.Instance.CombatSystem.Simulate(heroSquadForCombat, expedition.destination.monsterIDs);
                    expedition.combatResult = combatResult;
                    
                    expedition.endTime = currentTime + 2000; 
                    break;
                    
                case ExpeditionStatus.Exploring:
                    expedition.status = ExpeditionStatus.Returning;
                    long travelTime = CalculateTravelTime(expedition.destination.position);
                    expedition.endTime = currentTime + travelTime;
                    OnExpeditionReturning?.Invoke(expedition);
                    break;

                case ExpeditionStatus.Returning:
                    expedition.status = ExpeditionStatus.Finished;
                    FinalizeExpedition(expedition);
                    _expeditionsToRemove.Add(expedition);
                    break;
            }
        }
        
        private void FinalizeExpedition(Expedition expedition)
        {
            var result = expedition.combatResult;
            if (result == null)
            {
                Debug.LogError($"Expedition {expedition.id} finished but has no combat result!");
                return;
            }
            
            // Code này bây giờ sẽ hoạt động
            if (result.DidPlayerWin)
            {
                // Xử lý thắng
            }
            else
            {
                // Xử lý thua
            }
            
            OnExpeditionFinished?.Invoke(expedition);
            OnPOICleared?.Invoke(expedition.destination);
        }
        
        public bool IsHeroOnExpedition(string heroId)
        {
            if (_activeExpeditions == null) return false;
            return _activeExpeditions.Any(exp => exp.squadHeroIDs.Contains(heroId));
        }
        
        private long CalculateTravelTime(Vector2 destination)
        {
            return (long)(Vector2.Distance(Vector2.zero, destination) * 100);
        }
    }
}