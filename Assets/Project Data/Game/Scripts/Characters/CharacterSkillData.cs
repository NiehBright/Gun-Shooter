using UnityEngine;

namespace Watermelon.SquadShooter
{
    public enum SkillType
    {
        FireExplosion,
        FrostNova,
        HealAOE,
        LightningStorm,
        BlackHole
    }

    [System.Serializable]
    public class CharacterSkillData
    {
        [SerializeField] string skillName;
        public string SkillName => skillName;

        [SerializeField] SkillType skillType;
        public SkillType SkillType => skillType;

        [SerializeField] Sprite buttonIcon;
        public Sprite ButtonIcon => buttonIcon;

        [SerializeField] float cooldown = 15f;
        public float Cooldown => cooldown;

        [SerializeField] float duration = 5f;
        public float Duration => duration;

        [SerializeField] float aoeRadius = 6f;
        public float AoeRadius => aoeRadius;

        [SerializeField] float pullSpeed = 4f;
        public float PullSpeed => pullSpeed;

        [SerializeField] float damageMultiplier = 1.5f; // Sát thương mỗi tick = dame súng * multiplier
        public float DamageMultiplier => damageMultiplier;

        [SerializeField] float tickInterval = 0.5f; // Tần suất gây dame (giây)
        public float TickInterval => tickInterval;

        [SerializeField] GameObject vfxPrefab;
        public GameObject VFXPrefab => vfxPrefab;
    }
}
