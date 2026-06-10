using UnityEngine;
using TMPro;

namespace LegendOfBlood.UI.Helpers
{
    public class HeroLineageUIHelper : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI lineageText;
        [SerializeField] private TextMeshProUGUI generationText;
        [SerializeField] private TextMeshProUGUI fatherIdText;
        [SerializeField] private TextMeshProUGUI motherIdText;

        public void BindLineage(HeroData hero)
        {
            if (hero == null) return;
            
            string fId = string.IsNullOrEmpty(hero.fatherId) ? "Unknown" : hero.fatherId;
            string mId = string.IsNullOrEmpty(hero.motherId) ? "Unknown" : hero.motherId;

            if (lineageText != null)
                lineageText.text = $"Generation: {hero.generation} | FatherId: {fId} | MotherId: {mId}";
            
            if (generationText != null) generationText.text = $"Gen: {hero.generation}";
            if (fatherIdText != null) fatherIdText.text = $"FatherId: {fId}";
            if (motherIdText != null) motherIdText.text = $"MotherId: {mId}";
        }
    }
}
