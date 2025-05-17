using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Random = System.Random;

namespace Battle.Scripts.Ai.CharacterCreator
{
    public class RandomCharacter : MonoBehaviour
    {
        
        public GameObject Tester;
        private SPUM_Prefabs spum;
        //종족 외형
        public string DevilBodyFolder = "Sprites/0_Devil/Devil_1";
        public string HumanBodyFolder = "Sprites/1_Human";
        public string ElfBodyFolder = "Sprites/2_Elf";
        public string OrcBodyFolder = "Sprites/3_Orc";
        public string Skeleton = "Sprites/4_Skeleton/Skelton_1";
        
        //얼굴 외형
        public string HairFolder = "Sprites/Hair";
        public string EyeFolder = "Sprites/Eyes/Eye";
        public string FaceHairFolder = "Sprites/FaceHair";
        //의복 외형
        public string ClothFolder = "Sprites/Cloth";
        public string PantsFolder = "Sprites/Pants";
        public string BackFolder = "Sprites/Back";
        //갑옷 외형
        public string HelmetFolder = "Sprites/Helmets";
        public string ArmorFolder = "Sprites/Armor";
        
        //무기
        public string SwordFolder = "Sprites/0_Sword";
        public string SpearFolder = "Sprites/1_Spear";
        public string AxeFolder = "Sprites/2_Axe";
        public string BowFolder = "Sprites/3_Bow";
        public string WandFolder = "Sprites/4_Wand";
        public string DaggerFolder = "Sprites/5_Dagger";
        public string ShieldFolder = "Sprites/6_Shield";
        public string MaceFolder = "Sprites/7_Mace";
        
        public CreateTest createTest;
        
        public Sprite[] BodySprite;
        
        private int tribe;
        private string racePath;
        private string WeaponPath;
        private int hair;
        private bool hasWeapon;
        private bool hasDagger;
        private Random rand = new();

        private void Start()
        {
            spum = GetComponent<SPUM_Prefabs>();
            Randomize();
        }

        public void Randomize()
        {
            hasWeapon = false;
            SetBody();
            SetEyes();
            SetHair();
            SetFaceHair();
            SetHelmet();
            SetCloth();
            SetPants();
            SetArmors();
            SetBack();
            ClearWeapons();
            SetWeapon(20);
            SetWeapon(21);
            createTest.RerenderAllParts(Tester);
        }
        
        private void ClearWeapons()
        {
            // 오른손 무기 (20)
            spum.ImageElement[20].ItemPath = "";
            spum.ImageElement[20].Structure = "Weapons";
            spum.ImageElement[20].PartSubType = "";
            spum.ImageElement[20].Color = Color.white;

            // 왼손 무기 (21)
            spum.ImageElement[21].ItemPath = "";
            spum.ImageElement[21].Structure = "Weapons";
            spum.ImageElement[21].PartSubType = "";
            spum.ImageElement[21].Color = Color.white;
        }
        private void SetWeapon(int index)
        {
            if(hasWeapon && !hasDagger) return;
            int type;
            string weaponType = "";
            if (!hasDagger)
            {
                int Weapon = rand.Next(0, 8);
                hasWeapon = true;
                switch (Weapon)
                {
                    case 0:
                        WeaponPath = SwordFolder;
                        type = rand.Next(1, 5);
                        WeaponPath += "/Sword_" + type;
                        weaponType = "Sword";
                        break;
                    case 1:
                        WeaponPath = SpearFolder;
                        type = rand.Next(1, 5);
                        WeaponPath += "/Spear_" + type;
                        weaponType = "Spear";
                        break;
                    case 2:
                        WeaponPath = AxeFolder;
                        type = rand.Next(1, 4);
                        WeaponPath += "/Axe_" + type;
                        weaponType = "Axe";
                        break;
                    case 3:
                        WeaponPath = BowFolder;
                        type = rand.Next(1, 4);
                        WeaponPath += "/Bow_" + type;
                        weaponType = "Bow";
                        break;
                    case 4:
                        WeaponPath = WandFolder;
                        type = rand.Next(1, 3);
                        WeaponPath += "/Wand_" + type;
                        weaponType = "Wand";
                        break;
                    case 5:
                        WeaponPath = DaggerFolder;
                        type = rand.Next(1, 4);
                        WeaponPath += "/Dagger_" + type;
                        weaponType = "Dagger";
                        hasDagger = true;
                        break;
                    case 6:
                        WeaponPath = ShieldFolder;
                        type = rand.Next(1, 5);
                        WeaponPath += "/Shield_" + type;
                        weaponType = "Shield";
                        hasWeapon = false;
                        break;
                    case 7:
                        WeaponPath = MaceFolder;
                        type = 1;
                        WeaponPath += "/Mace_" + type;
                        weaponType = "Mace";
                        break;
                }
            }
            else
            {
                WeaponPath = DaggerFolder;
                type = rand.Next(1, 4);
                WeaponPath += "/Dagger_" + type;
                weaponType = "Dagger";
            }
            Tester.GetComponent<SPUM_Prefabs>().ImageElement[index].PartSubType = weaponType;
            ApplySpritePath(index, index + 1, WeaponPath);
        }

        private void SetBack()
        {
            int back = rand.Next(1, 4);
            BackFolder += "/Back_" + back;
            ApplySpritePath(19, 20, BackFolder);    //19번: back
        }

        private void SetArmors()
        {
            int armor = rand.Next(1, 12);
            ArmorFolder += "/New_Armor_" + armor;
            ApplySpritePath(16, 19, ArmorFolder);   //16번 ~ 18번: armor
        }

        private void SetPants()
        {
            int pants = rand.Next(1, 13);
            PantsFolder += "/New_Pant_" + pants;
            ApplySpritePath(14, 16, PantsFolder);   //14 ~ 15번 : Pants
        }
        
        private void SetCloth()
        {
            int cloth = rand.Next(1, 13);
            ClothFolder += "/New_Cloth_" + cloth;
            ApplySpritePath(11, 14, ClothFolder);  //11번 ~ 13번: cloth
        }

        private void SetHelmet()
        {
            Debug.Log(hair);
            if (hair != 21)
            {
                Tester.GetComponent<SPUM_Prefabs>().ImageElement[10].MaskIndex = 1;
                return;
            }
            
            Tester.GetComponent<SPUM_Prefabs>().ImageElement[8].MaskIndex = 1;
            Tester.GetComponent<SPUM_Prefabs>().ImageElement[10].MaskIndex = 0;
            
            int helmet = rand.Next(1, 12);
            HelmetFolder += "/New_Helmet_" + helmet;
            ApplySpritePath(10,11, HelmetFolder);  //10번 : helmet
        }

        private void SetFaceHair()
        {
            int faceHair = rand.Next(1, 7);
            FaceHairFolder += "/FaceHair_" + faceHair;
            ApplySpritePath(9, 10, FaceHairFolder);
            RandomColor(9,10);  //9번 : faceHair
            
        }
        private void SetHair()
        {
            hair = rand.Next(1, 22);
            HairFolder += "/New_Hair_" + hair;
            ApplySpritePath(8, 9, HairFolder);   //8번: 머리카락 관련 에셋
            RandomColor(8,9);
            Tester.GetComponent<SPUM_Prefabs>().ImageElement[8].MaskIndex = 0;
        }
        
        private void SetEyes()
        {
            int eye = rand.Next(0, 12);
            EyeFolder += eye;
            ApplySpritePath(6, 8, EyeFolder);   //6번 ~ 7번: 눈 관련 에셋
            RandomColor(6,8);
        }

        private void SetRace()
        {
            int race = rand.Next(0,(BodySprite.Length/6));
            racePath += (race + 1);
        }

        private void ApplySpritePath(int start, int end, string Path)
        {
            for (int i = start; i < end; i++)
            {
                Tester.GetComponent<SPUM_Prefabs>().ImageElement[i].ItemPath = Path;
            }
        }

        private void RandomColor(int start, int end)
        {
            var spum = Tester.GetComponent<SPUM_Prefabs>();
            Color randomColor = new Color(rand.Next(0, 100) / 100f, rand.Next(0, 100) / 100f, rand.Next(0, 100) / 100f);
            for (int i = start; i < end; i++)
            {
                spum.ImageElement[i].Color = randomColor;
            }
        }
        
        public void SetBody()
        {
            tribe = rand.Next(0, 5);
            switch (tribe)
            {
                case 0:
                    BodySprite = Resources.LoadAll<Sprite>(DevilBodyFolder);
                    racePath = DevilBodyFolder;
                    break;
                case 1:
                    BodySprite = Resources.LoadAll<Sprite>(HumanBodyFolder);
                    racePath = HumanBodyFolder + "/Human_";
                    SetRace();
                    break;
                case 2:
                    BodySprite = Resources.LoadAll<Sprite>(ElfBodyFolder);
                    racePath = ElfBodyFolder + "/Elf_";
                    SetRace();
                    break;
                case 3:
                    BodySprite = Resources.LoadAll<Sprite>(OrcBodyFolder);
                    racePath = OrcBodyFolder + "/Orc_";
                    SetRace();
                    break;
                case 4:
                    BodySprite = Resources.LoadAll<Sprite>(Skeleton);
                    racePath = Skeleton;
                    break;
            }
            ApplySpritePath(0,6,racePath);     //0번 ~ 5번: 종족 관련 에셋
        }
    }
}
