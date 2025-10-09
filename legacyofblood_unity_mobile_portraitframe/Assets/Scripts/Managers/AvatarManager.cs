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
        /// <param name="gender">Giới tính của hero.</param>
        /// <param name="index">Chỉ số của avatar đã được lưu trong HeroData.</param>
        /// <returns>Sprite tương ứng, hoặc null nếu index không hợp lệ.</returns>
        public Sprite GetAvatar(Gender gender, int index)
        {
            var targetPool = (gender == Gender.Male) ? maleAvatars : femaleAvatars;

            if (targetPool == null || targetPool.Count == 0)
            {
                Debug.LogError($"Avatar pool cho giới tính {gender} chưa được thiết lập hoặc bị rỗng!");
                return null;
            }

            if (index >= 0 && index < targetPool.Count)
            {
                return targetPool[index];
            }

            Debug.LogWarning($"Index avatar không hợp lệ ({index}) cho giới tính {gender}. Trả về avatar đầu tiên.");
            return targetPool[0]; // Trả về avatar mặc định để tránh lỗi
        }

        /// <summary>
        /// Lấy một chỉ số (index) ngẫu nhiên từ pool avatar của một giới tính.
        /// Được dùng khi tạo một hero mới.
        /// </summary>
        /// <param name="gender">Giới tính của hero sẽ được tạo.</param>
        /// <returns>Một chỉ số ngẫu nhiên.</returns>
        public int GetRandomAvatarIndex(Gender gender)
        {
            var targetPool = (gender == Gender.Male) ? maleAvatars : femaleAvatars;
            if (targetPool == null || targetPool.Count == 0) return -1; // -1 biểu thị lỗi

            return Random.Range(0, targetPool.Count);
        }
    }
}