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
        /// <param name="gender">Giới tính của hero.</param>
        /// <param name="index">Chỉ số của avatar đã được lưu trong HeroData.</param>
        /// <returns>Sprite tương ứng, hoặc null nếu index không hợp lệ.</returns>
        public Sprite GetAvatar(Gender gender, int index)
        {
            var targetPool = (gender == Gender.Male) ? maleAvatars : femaleAvatars;

            if (targetPool == null || targetPool.Count == 0)
            {
                // Thay vì văng lỗi liên tục vào Console, chỉ trả về null để Image tự tàng hình.
                // Thêm một cảnh báo nhẹ một lần là đủ.
                Debug.LogWarning($"[AvatarManager] Avatar pool {gender} rỗng. Hãy bỏ ảnh vào mục Resources/Avatars/{gender}.");
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