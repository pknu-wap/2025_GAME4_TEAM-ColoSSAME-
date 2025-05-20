// JobDatabase.cs

using System;
using System.Collections.Generic;
using Battle.Scripts.Ai;

namespace Battle.Scripts.Value.Data.Class
{
    public static class ClassDataBase
    {
        public static readonly Dictionary<ClassType, ClassStat> ClassStatsMap = new()
        {
            { ClassType.Warrior,
                new ClassStat
                {
                    hp = 200,
                    attack = 20,
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
                    hp = 120,
                    attack = 30,
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
                    hp = 100,
                    attack = 25,
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
                    hp = 80,
                    attack = 35,
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