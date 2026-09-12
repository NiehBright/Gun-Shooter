using UnityEngine;

namespace Watermelon.SquadShooter
{
    public enum SkillType
    {
        FireExplosion,
        FrostNova,
        HealAOE,
        LightningStorm,
        BlackHole,
        OrbitalLaser,
        ShadowClone
    }

    [System.Serializable]
    public class CharacterSkillData
    {
        [SerializeField] string skillName;
        public string SkillName { get => skillName; set => skillName = value; }

        [SerializeField] SkillType skillType;
        public SkillType SkillType { get => skillType; set => skillType = value; }

        [SerializeField] Sprite buttonIcon;
        public Sprite ButtonIcon { get => buttonIcon; set => buttonIcon = value; }

        [SerializeField] float cooldown = 15f;
        public float Cooldown { get => cooldown; set => cooldown = value; }

        [SerializeField] float duration = 5f;
        public float Duration { get => duration; set => duration = value; }

        [SerializeField] float aoeRadius = 6f;
        public float AoeRadius { get => aoeRadius; set => aoeRadius = value; }

        [SerializeField] float pullSpeed = 4f;
        public float PullSpeed { get => pullSpeed; set => pullSpeed = value; }

        [SerializeField] float damageMultiplier = 1.5f; // Sát thương mỗi tick = dame súng * multiplier
        public float DamageMultiplier { get => damageMultiplier; set => damageMultiplier = value; }

        [SerializeField] float tickInterval = 0.5f; // Tần suất gây dame (giây)
        public float TickInterval { get => tickInterval; set => tickInterval = value; }

        [SerializeField] GameObject vfxPrefab;
        public GameObject VFXPrefab { get => vfxPrefab; set => vfxPrefab = value; }

        [SerializeField, TextArea(2, 5)] string description;
        public string Description { get => description; set => description = value; }

        public string SkillTag
        {
            get
            {
                switch (skillType)
                {
                    case SkillType.BlackHole: return "Khống Chế & Hút Quái";
                    case SkillType.OrbitalLaser: return "Sát Thương Quỹ Đạo";
                    case SkillType.ShadowClone: return "Lướt Lùi & Phân Thân Nổ Sét";
                    case SkillType.FireExplosion: return "Sát Thương Diện Rộng";
                    case SkillType.FrostNova: return "Đóng Băng & Làm Chậm";
                    case SkillType.HealAOE: return "Hồi Phục Sinh Lực";
                    case SkillType.LightningStorm: return "Bão Sét Diện Rộng";
                    default: return "Kỹ Năng Đặc Trưng";
                }
            }
        }

        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(description))
                return description;

            switch (skillType)
            {
                case SkillType.BlackHole:
                    return "Tạo ra hố đen trọng lực cực lớn, liên tục hút quái vật vào tâm và gây sát thương nghiền nát.";
                case SkillType.OrbitalLaser:
                    return "Gọi chùm laser vệ tinh từ quỹ đạo quét xuống mục tiêu, thiêu rụi mọi kẻ thù trong vùng quét.";
                case SkillType.ShadowClone:
                    return "Lướt lùi né đòn, để lại một phân thân thế mạng phóng điện giật quái và phát nổ cực mạnh.";
                case SkillType.FireExplosion:
                    return "Tạo ra một vụ nổ lửa cực lớn gây sát thương thiêu đốt toàn bộ kẻ thù xung quanh.";
                case SkillType.FrostNova:
                    return "Phát ra luồng băng giá đóng băng và làm chậm tốc độ di chuyển của mọi kẻ thù.";
                case SkillType.HealAOE:
                    return "Tạo vùng hồi phục năng lượng, hồi máu ngay lập tức cho nhân vật.";
                case SkillType.LightningStorm:
                    return "Triệu hồi cơn bão sấm sét giáng liên tiếp xuống những kẻ thù trong khu vực.";
                default:
                    return "Kỹ năng đặc biệt mang lại lợi thế lớn trong trận chiến.";
            }
        }
    }
}
