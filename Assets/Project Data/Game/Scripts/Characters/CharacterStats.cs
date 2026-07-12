using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterStats
    {
        [SerializeField] int health;
        public int BaseHealth => health;
        public int Health
        {
            get
            {
                int bonusHP = 0;
                if (Application.isPlaying)
                {
                    bonusHP = Mathf.RoundToInt(EquipmentController.GetTotalBonusStats().bonusHP);
                }
                return health + bonusHP;
            }
        }

        [Space]
        [SerializeField] float bulletDamageMultiplier = 1.0f;
        public float BaseBulletDamageMultiplier => bulletDamageMultiplier;
        public float BulletDamageMultiplier
        {
            get
            {
                float bonusDmgPercent = 0f;
                if (Application.isPlaying)
                {
                    bonusDmgPercent = EquipmentController.GetTotalBonusStats().bonusDamagePercent;
                }
                return bulletDamageMultiplier * (1f + bonusDmgPercent / 100f);
            }
        }

        [SerializeField] int power;
        public int Power => power;

        // key upgrade - "ideal" way to play the game, based on this upgrades sequence is built economy
        [SerializeField] int keyUpgradeNumber;
        public int KeyUpgradeNumber => keyUpgradeNumber;
    }
}