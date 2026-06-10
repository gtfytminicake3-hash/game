using UnityEngine;
using TMPro;

namespace LegendOfBlood.UI.Helpers
{
    public class TraitTooltipUIHelper : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI familyText;
        [SerializeField] private TextMeshProUGUI summaryText;

        public void BindTraitInfo(Trait trait)
        {
            if (trait == null) return;

            if (rankText != null) rankText.text = $"Rank: {trait.rank}";
            if (familyText != null) familyText.text = $"Family: {(string.IsNullOrEmpty(trait.familyId) ? "None" : trait.familyId)}";
            if (summaryText != null)
            {
                // TODO: Chuyen sang TraitTooltipHelper hien thi tooltip rieng sau nay
                summaryText.text = GenerateText(trait);
            }
        }

        private string GenerateText(Trait trait)
        {
            if (trait == null) return "";
            string s = "";
            if (trait.effects != null)
            {
                foreach(var e in trait.effects)
                {
                    s += $"- {e.type}\n";
                }
            }
            if (trait.combatEffects != null)
            {
                foreach(var e in trait.combatEffects)
                {
                    s += $"- {e.type} ({(e.procChance*100f):0}%)\n";
                }
            }
            if (string.IsNullOrEmpty(s)) s = global::LocalizationSystem.GetText(trait.description);
            return s.TrimEnd();
        }
    }
}
