using System.Collections.Generic;
using UnityEngine;

namespace LegendOfBlood
{
    /// <summary>
    /// Singleton quản lý các pool avatar cho hero.
    /// Cho phép lấy avatar ngẫu nhiên hoặc theo index cụ thể.
    /// </summary>
    public class AvatarManager : MonoBehaviour
    {
        // --- Singleton Pattern ---
        public static AvatarManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Dự phòng: Tự động tải từ Resources nếu Inspector bị trống
            if (maleAvatars == null || maleAvatars.Count == 0)
            {
                maleAvatars = new List<Sprite>(Resources.LoadAll<Sprite>("Avatars/Male"));
            }

            if (femaleAvatars == null || femaleAvatars.Count == 0)
            {
                femaleAvatars = new List<Sprite>(Resources.LoadAll<Sprite>("Avatars/Female"));
            }
        }
        // -------------------------

        [Header("Avatar Pools")]
        [Tooltip("Kéo tất cả các Sprite avatar của Nam vào đây.")]
        [SerializeField] private List<Sprite> maleAvatars;

        [Tooltip("Kéo tất cả các Sprite avatar của Nữ vào đây.")]
        [SerializeField] private List<Sprite> femaleAvatars;

        /// <summary>
        /// Lấy một Sprite avatar cụ thể dựa trên giới tính và chỉ số (index).
        /// </summary>
        public Sprite GetAvatar(Gender gender, int index)
        {
            var targetPool = (gender == Gender.Male) ? maleAvatars : femaleAvatars;

            if (targetPool == null || targetPool.Count == 0 || index < 0 || index >= targetPool.Count)
            {
                // Fallback an toàn nếu lỗi index
                return Resources.Load<Sprite>($"Avatars/{gender}/Default");
            }
            return targetPool[index];
        }

        /// <summary>
        /// Tìm tất cả avatar phù hợp với Class và pick random 1 cái, trả về index của nó trong Pool chính.
        /// </summary>
        public int GetRandomAvatarIndexByClass(Gender gender, string professionName)
        {
            var targetPool = (gender == Gender.Male) ? maleAvatars : femaleAvatars;
            if (targetPool == null || targetPool.Count == 0) return 0;

            if (string.IsNullOrEmpty(professionName)) professionName = "None";

            List<int> validIndices = new List<int>();
            for (int i = 0; i < targetPool.Count; i++)
            {
                if (targetPool[i].name.Contains(professionName))
                {
                    validIndices.Add(i);
                }
            }

            if (validIndices.Count > 0)
            {
                int randomPick = Random.Range(0, validIndices.Count);
                return validIndices[randomPick];
            }

            // Fallback: Nếu không có hệ class này, trả về random toàn bộ
            return Random.Range(0, targetPool.Count);
        }
    }
}