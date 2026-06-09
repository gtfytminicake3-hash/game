using UnityEngine;

namespace LegendOfBlood
{
    public static class CardArtResolver
    {
        public static Sprite GetUnitSprite(LegendOfBlood.Combat.CombatReplayUnitSnapshot snap)
        {
            if (string.IsNullOrEmpty(snap.spriteId)) return null;
            Sprite result = null;
            string resourcePath = "Avatars/AnhHeroCard/";

            if (!snap.isAlly)
            {
                result = Resources.Load<Sprite>($"{resourcePath}{snap.spriteId}");
                if (result != null) return result;
                
                if (snap.spriteId.Contains(" x"))
                {
                    string baseName = snap.spriteId.Split(new string[] { " x" }, System.StringSplitOptions.None)[0];
                    result = Resources.Load<Sprite>($"{resourcePath}{baseName}");
                    if (result != null) return result;
                }
            }
            else
            {
                if (AvatarManager.Instance != null)
                {
                    result = AvatarManager.Instance.GetAvatar((Gender)snap.heroGender, snap.avatarIndex);
                    if (result != null) return result;
                }
                else
                {
                    Debug.LogWarning("[CardArtResolver] AvatarManager is null during snapshot replay.");
                }
            }

            result = Resources.Load<Sprite>($"{resourcePath}Fallback");
            if (result == null && !snap.isAlly)
            {
                result = Resources.Load<Sprite>("UI/DefaultSprite");
            }

            return result;
        }

        public static Sprite GetUnitSprite(HeroData data, bool isMonster)
        {
            if (data == null) return null;

            Sprite result = null;
            string resourcePath = "Avatars/AnhHeroCard/";

            if (isMonster)
            {
                // Try by ID first
                result = Resources.Load<Sprite>($"{resourcePath}{data.id}");
                if (result != null)
                {
                    Debug.Log($"[CardArtResolver] Loaded monster sprite by ID: {data.id} at {resourcePath}{data.id}");
                    return result;
                }

                // Try by Name
                result = Resources.Load<Sprite>($"{resourcePath}{data.heroName}");
                if (result != null)
                {
                    Debug.Log($"[CardArtResolver] Loaded monster sprite by Name: {data.heroName} at {resourcePath}{data.heroName}");
                    return result;
                }
                
                // For strings like "Goblin x2", we can try extracting the base name
                if (data.id.Contains(" x"))
                {
                    string baseName = data.id.Split(new string[] { " x" }, System.StringSplitOptions.None)[0];
                    result = Resources.Load<Sprite>($"{resourcePath}{baseName}");
                    if (result != null)
                    {
                        Debug.Log($"[CardArtResolver] Loaded monster sprite by fallback Name: {baseName} at {resourcePath}{baseName}");
                        return result;
                    }
                }

                Debug.LogWarning($"[CardArtResolver] Missing monster sprite for: {data.id} / {data.heroName}. Using fallback.");
            }
            else
            {
                // For heroes, use standard GetAvatarSprite
                result = data.GetAvatarSprite();
                if (result != null)
                {
                    Debug.Log($"[CardArtResolver] Loaded hero sprite from AvatarManager for: {data.id}");
                    return result;
                }
            }

            // Global Fallback
            // If we have a generic fallback sprite in resources, load it
            result = Resources.Load<Sprite>($"{resourcePath}Fallback");
            if (result == null && isMonster)
            {
                // Just use any available UI sprite or clear
                result = Resources.Load<Sprite>("UI/DefaultSprite"); // placeholder
            }

            return result;
        }
    }
}

