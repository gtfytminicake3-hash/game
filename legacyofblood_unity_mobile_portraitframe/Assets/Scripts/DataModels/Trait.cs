// --- START OF FILE Trait.cs (FIXED) ---

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegendOfBlood
{
    /// <summary>
    /// Các loại hiệu ứng mà một Trait có thể gây ra.
    /// </summary>
    public enum TraitEffectType
    {
        // === GIỮ NGUYÊN (cũ) ===
        ADD_STAT,        // Cộng thẳng vào chỉ số (HP, ATK, DEF, SPD)
        MULTIPLY_STAT,   // Nhân chỉ số theo phần trăm
        AURA,            // Hiệu ứng hào quang ảnh hưởng đồng đội/kẻ địch đầu trận
        SPECIAL,         // Hiệu ứng đặc biệt kích hoạt theo điều kiện (khi chết, khi hạ gục,...)

        // === MỚI — Passive conditionals ===
        ADD_STAT_IF_CLASS_IN_TEAM,       // +stat nếu team có class X
        MULTIPLY_STAT_IF_CLASS_IN_TEAM,  // x stat nếu team có class X

        // === MỚI — Battle triggers ===
        ON_BATTLE_START_BUFF_TEAM,       // Đầu trận buff team
        ON_BATTLE_START_DEBUFF_ENEMY,    // Đầu trận debuff enemy
        ON_BATTLE_START_SHIELD,          // Đầu trận tạo khiên cho bản thân
        ON_TURN_START_HEAL_SELF,         // Mỗi lượt hồi máu
        ON_TURN_START_STACK_ATK,         // Stack +Atk mỗi lượt
        ON_KILL_STACK_STAT,              // Stack stat khi giết địch
        ON_KILL_HEAL_TEAM,               // Kill = hồi team
        ON_DEATH_EXPLODE,                // Chết = gây dmg AoE cho enemy
        ON_DEATH_REVIVE_CHANCE,          // % revive khi chết
        ON_HIT_REFLECT,                  // % phản sát thương
        ON_HIT_COUNTER,                  // % đánh trả (basic attack)
        ON_CRIT_AMPLIFY,                 // Crit dmg tăng thêm
        ON_CRIT_PROC_STUN,               // Crit = % stun
        ON_DAMAGE_DEALT_LIFESTEAL,       // % hút máu
    }

    /// <summary>
    /// Lớp con chứa thông tin chi tiết của một hiệu ứng.
    /// [System.Serializable] để nó có thể được hiển thị trong Inspector.
    /// </summary>
    [Serializable]
    public class TraitEffect
    {
        [Tooltip("Loại hiệu ứng mà Trait này gây ra.")]
        public TraitEffectType type;

        [Header("For ADD_STAT & MULTIPLY_STAT")]
        [Tooltip("HP: Cộng thẳng giá trị. ATK/DEF/SPD: Cộng thẳng giá trị.")]
        public int hp;
        [Tooltip("ATK: Cộng thẳng giá trị.")]
        public int atk;
        [Tooltip("DEF: Cộng thẳng giá trị.")]
        public int def;
        [Tooltip("SPD: Cộng thẳng giá trị.")]
        public int spd;

        [Header("For SPECIAL & AURA")]
        [Tooltip("Mô tả hiệu ứng để game designer dễ nhận biết.")]
        [TextArea(2, 4)]
        public string effectDescription;

        // === MỚI — Combat trigger fields ===
        [Header("Combat Trigger Config")]
        [Tooltip("% kích hoạt (0.0 đến 1.0). Mặc định 1.0 = luôn kích hoạt.")]
        [Range(0f, 1f)]
        public float procChance = 1.0f;

        [Tooltip("Loại buff/debuff (nếu có).")]
        public StatusEffectType buffType;

        [Tooltip("Hệ số sát thương dựa trên ATK (dùng cho REFLECT, COUNTER, EXPLODE).")]
        public float atkRatio;

        [Tooltip("Hệ số dựa trên Max HP (dùng cho HEAL_SELF, EXPLODE, SHIELD).")]
        public float maxHpRatio;

        [Tooltip("Thời gian buff/debuff (số lượt).")]
        public int duration;

        [Tooltip("Số stack tối đa (dùng cho ON_KILL_STACK, ON_TURN_STACK).")]
        public int maxStack;

        [Tooltip("Giá trị mỗi stack (%, flat tùy context).")]
        public float stackValue;

        [Header("Conditional — Class Synergy")]
        [Tooltip("Class yêu cầu (dùng cho ADD_STAT_IF_CLASS_IN_TEAM).")]
        public Profession requiredProfession = Profession.None;

        [Tooltip("Số đồng đội cùng class cần có (0 = mỗi 1 đồng đội = 1 stack).")]
        public int requiredCount;

        [Header("Revive")]
        [Tooltip("% HP khi hồi sinh (0.0 đến 1.0).")]
        [Range(0f, 1f)]
        public float reviveHpPercent;
    }

    /// <summary>
    /// Định nghĩa ScriptableObject cho một Trait (Đặc tính).
    /// Cho phép tạo các asset Trait trong Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrait", menuName = "LegendOfBlood/Trait", order = 1)]
    public class Trait : ScriptableObject
    {
        public enum RarityRank { D, C, B, A, S, SS, SSS }

        [Header("Basic Information")]
        [Tooltip("ID duy nhất của Trait, ví dụ: 'S_04', 'SS_07'. Rất quan trọng, không được trùng.")]
        public string id;

        [Tooltip("Tên của Trait sẽ hiển thị trong game.")]
        public string traitName;

        [Tooltip("Mô tả chi tiết về Trait, sẽ hiển thị cho người chơi.")]
        [TextArea(3, 5)]
        public string description;

        [Tooltip("Icon đại diện cho Đặc tính.")]
        public Sprite icon;

        [Header("Trait Evolution")]
        public RarityRank rank;
        public string familyId; // Ví dụ: "ATK_UP", "HP_ON_HIT"
        public string nextUpgradeTraitID; // ID của Trait kế tiếp trong cùng family
        [Tooltip("Số lượng EXP của Tướng cần tiêu hao để nâng cấp lên Trait kế tiếp.")]
        public int upgradeExpCost = 1000;

        [Header("Trait Effects — Stat (Passive)")]
        [Tooltip("Danh sách hiệu ứng stat thụ động (ADD_STAT, MULTIPLY_STAT, AURA, SPECIAL).")]
        public List<TraitEffect> effects;

        [Header("Trait Effects — Combat (Active Trigger)")]
        [Tooltip("Danh sách hiệu ứng kích hoạt trong chiến đấu (ON_BATTLE_START, ON_KILL, ON_HIT...).")]
        public List<TraitEffect> combatEffects;

        // --- CÁC HÀM TIỆN ÍCH (Tùy chọn nhưng rất hữu ích) ---

        /// <summary>
        /// Tự động điền ID dựa trên tên file của asset.
        /// Chạy khi asset được tạo hoặc khi bạn chọn "Auto-fill ID from Name" trong menu chuột phải.
        /// </summary>
        [ContextMenu("Auto-fill ID from Name")]
        private void AutofillID()
        {
            this.id = this.name;
        }

        /// <summary>
        /// Chạy mỗi khi một giá trị trong Inspector của asset này thay đổi.
        /// Dùng để đảm bảo ID không bao giờ là rỗng.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                // Nếu ID rỗng, tạm thời dùng tên của asset
                id = this.name;
                Debug.LogWarning($"Trait '{this.name}' có ID rỗng. Đã tự động điền bằng tên asset. Hãy đặt một ID duy nhất.", this);
            }
        }

        /// <summary>
        /// Sinh ra chuỗi hiển thị chi tiết cho UI Tooltip (P3 UI Helper).
        /// Null-safe và không bao giờ crash.
        /// </summary>
        public string GetTooltipText()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine($"<color=#FFD700><b>{traitName}</b></color> <size=80%>({rank})</size>");
            if (!string.IsNullOrEmpty(description))
            {
                sb.AppendLine($"<i>{description}</i>");
            }
            
            sb.AppendLine();

            if (effects != null && effects.Count > 0)
            {
                sb.AppendLine("<color=#AADDFF>Hiệu ứng:</color>");
                foreach (var effect in effects)
                {
                    if (effect == null) continue;
                    
                    if (effect.type == TraitEffectType.ADD_STAT)
                    {
                        if (effect.hp > 0) sb.AppendLine($"- Tăng {effect.hp} HP");
                        if (effect.atk > 0) sb.AppendLine($"- Tăng {effect.atk} ATK");
                        if (effect.def > 0) sb.AppendLine($"- Tăng {effect.def} DEF");
                        if (effect.spd > 0) sb.AppendLine($"- Tăng {effect.spd} SPD");
                    }
                    else if (!string.IsNullOrEmpty(effect.effectDescription))
                    {
                        sb.AppendLine($"- {effect.effectDescription}");
                    }
                }
            }
            else
            {
                sb.AppendLine("<color=#888888>Chưa rõ hiệu ứng.</color>");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
// --- END OF FILE Trait.cs (FIXED) ---