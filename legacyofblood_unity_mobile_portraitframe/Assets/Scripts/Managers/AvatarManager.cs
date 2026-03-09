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

        /// <summary>
        /// Lấy một Sprite avatar theo Giới tính và Nghề nghiệp (Class).
        /// Cấu trúc thư mục Resources yêu cầu: Resources/Avatars/Male/Warrior, etc.
        /// </summary>
        public Sprite GetAvatar(Gender gender, string professionName)
        {
            // Fallback cho None hoặc trẻ sơ sinh
            if (string.IsNullOrEmpty(professionName) || professionName == "None")
            {
                professionName = "None"; // Cần 1 ảnh mặc định tên None trong folder
            }

            string path = $"Avatars/{gender}/{professionName}";
            Sprite avatar = Resources.Load<Sprite>(path);

            if (avatar != null) 
            {
                return avatar;
            }

            // Nếu không tìm thấy file có tên nghề, load ảnh mặc định (để tránh null)
            Debug.LogWarning($"[AvatarManager] Không tìm thấy ảnh avatar tại đường dẫn: Resources/{path}. Trả về ảnh mặc định.");
            return Resources.Load<Sprite>($"Avatars/{gender}/Default");
        }
    }
}