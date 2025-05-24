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
                hpRange = 25,
                attackRange = 4.5f,
                defenseRange = 0,
                moveSpeedRange = 0,
                attackDelayRange = 0
            }},
            { ClassType.Thief, new ClassRandomRange {
                hpRange = 25,
                attackRange = 4.5f,
                defenseRange = 0,
                moveSpeedRange = 0.2f,
                attackDelayRange = 0
            }},
            { ClassType.Archer, new ClassRandomRange {
                hpRange = 10,
                attackRange = 4.5f,
                defenseRange = 0,
                moveSpeedRange = 0,
                attackDelayRange = 0.1f
            }},
            { ClassType.Magician, new ClassRandomRange {
                hpRange = 10,
                attackRange = 4.5f,
                defenseRange = 0,
                moveSpeedRange = 0,
                attackDelayRange = 0.2f
            }},
        };

        public static readonly Dictionary<ClassType, ClassStat> ClassStatsMap = new()
        {
            { ClassType.Warrior,
                new ClassStat
                {
                    hp = 75,
                    attack = 5.5f,
                    defense = 10,
                    moveSpeed = 2f,
                    attackRange = 1.2f,
                    attackDelay = 0.6f,
                    weaponType = GetRandomWarriorWeaponType(),
                    attackSoundPath = "Sounds/Warrior/Swing"
                }
            },
            { ClassType.Thief,
                new ClassStat
                { 
                    hp = 75,
                    attack = 5.5f,
                    defense = 5,
                    moveSpeed = 3f,
                    attackRange = 1.5f,
                    attackDelay = 0.4f,
                    weaponType = WeaponType.ShortSword,
                    attackSoundPath = "Sounds/Thief/Slash"
                }
            },
            { ClassType.Archer,
                new ClassStat
                {
                    hp = 40,
                    attack = 5.5f,
                    defense = 5,
                    moveSpeed = 2.5f,
                    attackRange = 7f,
                    attackDelay = 1f,
                    weaponType = WeaponType.Bow,
                    attackSoundPath = "Sounds/Archer/Shoot"
                }
            },
            { ClassType.Magician,
                new ClassStat
                {
                    hp = 40,
                    attack = 5.5f,
                    defense = 3,
                    moveSpeed = 1.8f,
                    attackRange = 6f,
                    attackDelay = 1.2f,
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