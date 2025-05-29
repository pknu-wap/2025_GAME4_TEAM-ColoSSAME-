// JobDatabase.cs

using System;
using System.Collections.Generic;
using Battle.Scripts.Ai;

namespace Battle.Scripts.Value.Data.Class
{
    public struct ClassRandomRange
    {
        public float hpRange;
        public float attackRange;
        public float defenseRange;
        public float moveSpeedRange;
        public float attackDelayRange;
    }
    public static class ClassDataBase
    {
        public static readonly Dictionary<ClassType, ClassRandomRange> ClassRandomRanges = new()
        {
            { ClassType.Warrior, new ClassRandomRange {
                hpRange = 12.5f,
                attackRange = 2.5f,
                defenseRange = 1.5f,
                moveSpeedRange = 0,
                attackDelayRange = 0
            }},
            { ClassType.Thief, new ClassRandomRange {
                hpRange = 7,
                attackRange = 1f,
                defenseRange = 1.5f,
                moveSpeedRange = 0.2f,
                attackDelayRange = 0
            }},
            { ClassType.Archer, new ClassRandomRange {
                hpRange = 10,
                attackRange = 2.5f,
                defenseRange = 1,
                moveSpeedRange = 0,
                attackDelayRange = 0.1f
            }},
            { ClassType.Magician, new ClassRandomRange {
                hpRange = 10,
                attackRange = 1.5f,
                defenseRange = 1,
                moveSpeedRange = 0,
                attackDelayRange = 0.2f
            }},
        };

        public static readonly Dictionary<ClassType, ClassStat> ClassStatsMap = new()
        {
            { ClassType.Warrior,
                new ClassStat
                {
                    hp = 87,
                    attack = 5.5f,
                    defense = 5.5f,
                    moveSpeed = 2f,
                    attackRange = 1.5f,
                    attackDelay = 0.6f,
                    retreatDistance = 0.1f,
                    weaponType = GetRandomWarriorWeaponType(),
                    attackSoundPath = "Sounds/Warrior/Swing"
                }
            },
            { ClassType.Thief,
                new ClassStat
                { 
                    hp = 67,
                    attack = 3f,
                    defense = 3.5f,
                    moveSpeed = 3.2f,
                    attackRange = 1.2f,
                    attackDelay = 0.25f,
                    retreatDistance = 1f,
                    weaponType = WeaponType.ShortSword,
                    attackSoundPath = "Sounds/Thief/Slash"
                }
            },
            { ClassType.Archer,
                new ClassStat
                {
                    hp = 40,
                    attack = 7.5f,
                    defense = 2.5f,
                    moveSpeed = 2.5f,
                    attackRange = 5.5f,
                    attackDelay = 1.5f,
                    retreatDistance = 2f,
                    weaponType = WeaponType.Bow,
                    attackSoundPath = "Sounds/Archer/Shoot"
                }
            },
            { ClassType.Magician,
                new ClassStat
                {
                    hp = 50,
                    attack = 13.5f,
                    defense = 3,
                    moveSpeed = 1.8f,
                    attackRange = 4f,
                    attackDelay = 2f,
                    retreatDistance = 2f,
                    weaponType = WeaponType.Magic,
                    attackSoundPath = "Sounds/Magician/MagicBlast"
                }
            }
        };
        private static WeaponType GetRandomWarriorWeaponType()
        {
            WeaponType[] warriorWeapons = new WeaponType[]
            {
                WeaponType.Sword,
                WeaponType.LongSpear,
                WeaponType.TwoHanded,
            };

            Random rand = new Random();
            int index = rand.Next(warriorWeapons.Length);
            return warriorWeapons[index];
        }
    }
}