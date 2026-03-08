using UnityEngine;
using System.Collections.Generic;

namespace LegendOfBlood
{
    /// <summary>
    /// Utility class for generating random hero names.
    /// Uses two pools of 40 names: one for Male, one for Female.
    /// </summary>
    public static class NameGenerator
    {
        private static readonly string[] MaleNames = 
        {
            "Arthur", "Balian", "Cedric", "Darius", "Edgar", "Finn", "Gareth", "Hector", 
            "Igor", "Julian", "Kael", "Leon", "Marcus", "Nolan", "Orion", "Percival", 
            "Quentin", "Rowan", "Silas", "Tristan", "Ulfric", "Victor", "William", "Xander", 
            "Yorick", "Zane", "Alaric", "Bjorn", "Caspian", "Dorian", "Elias", "Felix", 
            "Gideon", "Hadrian", "Lucian", "Magnus", "Nikolai", "Osric", "Ronan", "Soren"
        };

        private static readonly string[] FemaleNames = 
        {
            "Aria", "Bella", "Cassandra", "Diana", "Elena", "Freya", "Gwen", "Helena", 
            "Isolde", "Julia", "Kira", "Lyra", "Maeve", "Nia", "Ophelia", "Penelope", 
            "Quinn", "Rose", "Serena", "Talia", "Una", "Valeria", "Willa", "Xenia", 
            "Yara", "Zara", "Amara", "Beatrix", "Cora", "Daphne", "Elara", "Fiona", 
            "Genevieve", "Hazel", "Liana", "Maya", "Nova", "Orla", "Rhea", "Stella"
        };

        /// <summary>
        /// Gets a random name based on the provided gender.
        /// </summary>
        public static string GetRandomName(Gender gender)
        {
            if (gender == Gender.Male)
            {
                int index = Random.Range(0, MaleNames.Length);
                return MaleNames[index];
            }
            else
            {
                int index = Random.Range(0, FemaleNames.Length);
                return FemaleNames[index];
            }
        }
    }
}
