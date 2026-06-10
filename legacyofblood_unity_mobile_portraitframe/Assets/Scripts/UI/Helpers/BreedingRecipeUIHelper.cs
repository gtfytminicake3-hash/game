using UnityEngine;
using TMPro;

namespace LegendOfBlood.UI.Helpers
{
    public class BreedingRecipeUIHelper : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI recipeMatchText;
        [SerializeField] private TextMeshProUGUI requiredMaterialText;

        public void UpdateRecipeUI(HeroData father, HeroData mother)
        {
            if (recipeMatchText != null) recipeMatchText.text = "Recipe: Random";
            if (requiredMaterialText != null) requiredMaterialText.text = "Material: None";

            if (father == null || mother == null) return;
            if (GameManager.Instance == null || GameManager.Instance.BreedingSystem == null) return;

            // Get the highest priority recipe
            var recipe = GameManager.Instance.BreedingSystem.GetHighestPriorityRecipe(father, mother);
            if (recipe != null)
            {
                if (recipeMatchText != null)
                {
                    string rName = string.IsNullOrEmpty(recipe.resultTraitId) ? "Random" : recipe.resultTraitId;
                    recipeMatchText.text = $"Recipe: {rName}";
                }
                
                if (requiredMaterialText != null)
                {
                    if (!string.IsNullOrEmpty(recipe.requireMaterialId) && recipe.requireMaterialAmount > 0)
                    {
                        requiredMaterialText.text = $"Material: {recipe.requireMaterialAmount}x {recipe.requireMaterialId}";
                    }
                }
            }
        }
    }
}
