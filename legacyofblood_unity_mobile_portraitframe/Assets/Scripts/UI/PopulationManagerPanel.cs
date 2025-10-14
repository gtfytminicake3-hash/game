namespace LegendOfBlood.UI
{
    using LegendOfBlood;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// This panel is shown when the player acquires a new hero but the population is full.
    /// It forces the player to dismiss one hero to make room for the new one.
    /// </summary>
    public class PopulationManagerPanel : MonoBehaviour
    {
        [Tooltip("The hero that needs a slot to be freed.")]
        private HeroData _newHero;

        // TODO: UI elements for displaying the list of current heroes
        // TODO: A confirmation button to dismiss the selected hero

        private void Start()
        {
            // TODO: Add button listeners
            Debug.Log("PopulationManagerPanel Initialized");
        }

        public void ShowPanel(HeroData newHero)
        {
            _newHero = newHero;
            gameObject.SetActive(true);
            
            // TODO: Populate the UI list with the player's current heroes
            Debug.Log($"Population is full. You must dismiss a hero to make room for {_newHero.heroName}.");
        }

        private void OnDismissHeroConfirmed(string heroIdToDismiss)
        {
            // 1. Dismiss the selected hero
            DataManager.Instance.RemoveHero(heroIdToDismiss);
            
            // 2. Add the new hero
            DataManager.Instance.AddHero(_newHero);

            // 3. Close the panel
            gameObject.SetActive(false);
        }
    }
}
