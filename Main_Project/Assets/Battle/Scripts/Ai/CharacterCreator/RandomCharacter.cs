using System;
using Battle.Scripts.Value.Data.Class;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Random = System.Random;
using Battle.Scripts.Ai;
using Battle.Scripts.Value.Data;


namespace Battle.Scripts.Ai.CharacterCreator
{
    public class RandomCharacter : MonoBehaviour
    {
        private SPUM_Prefabs spum;

        //종족 외형
        public string DevilBodyFolder = "Sprites/0_Devil/Devil_1";
        public string HumanBodyFolder = "Sprites/1_Human";
        public string ElfBodyFolder = "Sprites/2_Elf";
        public string OrcBodyFolder = "Sprites/3_Orc";
        public string Skeleton = "Sprites/4_Skeleton/Skeleton_1";
        
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
        private Random rand = new();
        private string weaponSubType = "";
        public ClassType selectedClass;
        private BattleAI ai;

        private void Awake()
        {
            ai = GetComponent<BattleAI>();
            spum = GetComponent<SPUM_Prefabs>();
            selectedClass = (ClassType)rand.Next(0, Enum.GetValues(typeof(ClassType)).Length);
        }

        public void Reset()
        {
            racePath = "";
            PathReset();
            hasWeapon = false;
            ClearWeapons();
            createTest.RerenderAllParts(gameObject);
        }

        private void PathReset()
        { 
            DevilBodyFolder = "Sprites/0_Devil/Devil_1"; 
            HumanBodyFolder = "Sprites/1_Human"; 
            ElfBodyFolder = "Sprites/2_Elf"; 
            OrcBodyFolder = "Sprites/3_Orc";
        
            Skeleton = "Sprites/4_Skeleton/Skeleton_1";
            HairFolder = "Sprites/Hair";
            EyeFolder = "Sprites/Eyes/Eye";
            FaceHairFolder = "Sprites/FaceHair";
            
            ClothFolder = "Sprites/Cloth";
            PantsFolder = "Sprites/Pants";
            BackFolder = "Sprites/Back";
            HelmetFolder = "Sprites/Helmets";
            ArmorFolder = "Sprites/Armor";
            
            SwordFolder = "Sprites/0_Sword";
            SpearFolder = "Sprites/1_Spear";
            AxeFolder = "Sprites/2_Axe";
            BowFolder = "Sprites/3_Bow";
            WandFolder = "Sprites/4_Wand";
            DaggerFolder = "Sprites/5_Dagger";
            ShieldFolder = "Sprites/6_Shield";
            MaceFolder = "Sprites/7_Mace";
            WeaponPath = "";
        }

        private int RandAsset(int Start, int End)
        {
            int asset = rand.Next(Start, End + 1);
            return asset;
        }

        public void Randomize()
        {
            Reset();
            if(ai.team == TeamType.Player) ai.FlipToRight();
            else ai.FlipToLeft();

            // 1. 무작위 직업 선택
            Array values = Enum.GetValues(typeof(ClassType));
            selectedClass = (ClassType)values.GetValue(rand.Next(values.Length));

            // 2. BattleAI에 적용
            ai.Class = selectedClass;
            ai.ApplyJobSettings(); // 능력치, 사운드, 무기타입 적용

            // 3. 외형 설정
            SetBody(RandAsset(0, 4));
            SetEyes(RandAsset(0,11));
            hair = RandAsset(1,40);
            SetHair(hair);
            SetFaceHair(RandAsset(1,6));
            SetHelmet(RandAsset(1,13));
            SetCloth(RandAsset(1,12));
            SetPants(RandAsset(1,12));
            SetArmors(RandAsset(1,11));
            SetBack(RandAsset(1,3));

            // 4. 무기 타입에 맞게 설정 (BattleAI.weaponType 기준)
            SetWeaponByClass(ai.weaponType);

            // 5. 렌더링
            createTest.RerenderAllParts(gameObject);
        }
        
        private void ClearWeapons()
        {
            // 오른손 무기 (20)
            spum.ImageElement[20].ItemPath = "";
            spum.ImageElement[20].Color = Color.white;

            // 왼손 무기 (21)
            spum.ImageElement[21].ItemPath = "";
            spum.ImageElement[21].Color = Color.white;
        }
        private void Dagger(int type)
        {
            WeaponPath = DaggerFolder;
            WeaponPath += "/Dagger_" + type;
            spum.ImageElement[20].PartSubType = "Dagger";
            ApplySpritePath(20, 21, WeaponPath);
            WeaponPath = DaggerFolder;
            WeaponPath += "/Dagger_" + type;
            spum.ImageElement[21].PartSubType = "Dagger";
            ApplySpritePath(21, 22, WeaponPath);
        }
        
        private void SetWeaponByClass(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Sword:
                    SetWeapon(20, 0, RandAsset(1, 4), 0, RandAsset(0, 1));
                    break;
                case WeaponType.LongSpear:
                    SetWeapon(20, 1, RandAsset(1, 3), 0, 0);
                    break;
                case WeaponType.TwoHanded:
                    SetWeapon(20, 2, RandAsset(1, 3), 0, 0);
                    break;
                case WeaponType.ShortSword:
                    Dagger(RandAsset(1, 3));
                    break;
                case WeaponType.Bow:
                    SetWeapon(20, 3, RandAsset(1, 3), 0, 0);
                    break;
                case WeaponType.Magic:
                    SetWeapon(20, 4, RandAsset(1, 2), 0, 0);
                    break;
                default:
                    SetWeapon(20, 0, RandAsset(1, 4), 0, 0);
                    break;
            }
        }
        private void SetWeapon(int index, int asset, int type, int shieldType, int HasShield)
        {
            spum.ImageElement[index].Structure = "Weapons";
            ApplySpritePath(index, index + 1, WeaponPath);
            weaponSubType = "";
            spum.ImageElement[index].PartSubType = weaponSubType;
            if (!hasWeapon)
            {
                switch (asset)
                    {
                        case 0:
                            WeaponPath = SwordFolder;
                            if (type == 0) type = RandAsset(1, 4);
                            WeaponPath += "/Sword_" + type;
                            weaponSubType = "Sword";
                            break;
                        case 1:
                            WeaponPath = SpearFolder;
                            if (type == 0) type = RandAsset(1, 4);
                            WeaponPath += "/Spear_" + type;
                            weaponSubType = "Spear";
                            break;
                        case 2:
                            WeaponPath = AxeFolder;
                            if (type == 0) type = RandAsset(1, 3);
                            WeaponPath += "/Axe_" + type;
                            weaponSubType = "Axe";
                            break;
                        case 3:
                            WeaponPath = BowFolder;
                            if (type == 0) type = RandAsset(1, 3);
                            WeaponPath += "/Bow_" + type;
                            weaponSubType = "Bow";
                            break;
                        case 4:
                            WeaponPath = WandFolder;
                            if (type == 0) type = RandAsset(1, 2);
                            WeaponPath += "/Wand_" + type;
                            weaponSubType = "Wand";
                            break;
                        case 5:
                            if (type == 0) type = RandAsset(1, 3);
                            Dagger(type);
                            return;
                        case 6:
                            WeaponPath = MaceFolder;
                            type = 1;
                            WeaponPath += "/Mace_" + type;
                            weaponSubType = "Mace";
                            break;
                    }
                hasWeapon = true;
            }
            spum.ImageElement[index].PartSubType = weaponSubType;
            ApplySpritePath(index, index + 1, WeaponPath);
            
            if (HasShield == 0) return;
            WeaponPath = ShieldFolder;
            if(shieldType == 0) shieldType = RandAsset(1, 4);
            WeaponPath += "/Shield_" + shieldType;
            weaponSubType = "Shield";
            spum.ImageElement[21].PartSubType = weaponSubType;
            ApplySpritePath(21, 22, WeaponPath);
        }

        private void SetBack(int asset)
        {
            BackFolder += "/Back_" + asset;
            ApplySpritePath(19, 20, BackFolder);    //19번: back
        }

        private void SetArmors(int asset)
        {
            ArmorFolder += "/New_Armor_" + asset;
            ApplySpritePath(16, 19, ArmorFolder);   //16번 ~ 18번: armor
        }

        private void SetPants(int asset)
        {
            PantsFolder += "/New_Pant_" + asset;
            ApplySpritePath(14, 16, PantsFolder);   //14 ~ 15번 : Pants
        }
        
        private void SetCloth(int asset)
        {
            ClothFolder += "/New_Cloth_" + asset;
            ApplySpritePath(11, 14, ClothFolder);  //11번 ~ 13번: cloth
        }

        private void SetHelmet(int asset)
        {
            spum.ImageElement[10].MaskIndex = 0;
            if (hair <= 25)
            {
                spum.ImageElement[10].MaskIndex = 1;
                HelmetFolder = "";
                ApplySpritePath(10,11, HelmetFolder);
                return;
            }
            
            spum.ImageElement[8].MaskIndex = 1;
            spum.ImageElement[10].MaskIndex = 0;
            
            HelmetFolder += "/New_Helmet_" + asset;
            ApplySpritePath(10,11, HelmetFolder);  //10번 : helmet
        }

        private void SetFaceHair(int asset)
        {
            FaceHairFolder += "/FaceHair_" + asset;
            if(asset == 6) FaceHairFolder = "";
            ApplySpritePath(9, 10, FaceHairFolder);
            RandomColor(9,10);  //9번 : faceHair
            
        }
        private void SetHair(int asset)
        {
            HairFolder += "/New_Hair_" + asset;
            if(hair >= 21) HairFolder = "";
            ApplySpritePath(8, 9, HairFolder);   //8번: 머리카락 관련 에셋
            RandomColor(8,9);
            spum.ImageElement[8].MaskIndex = 0;
        }
        
        private void SetEyes(int asset)
        {
            EyeFolder += asset;
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
                spum.ImageElement[i].ItemPath = Path;
            }
        }

        private void RandomColor(int start, int end)
        {
            var spum = this.spum;
            Color randomColor = new Color(rand.Next(0, 100) / 100f, rand.Next(0, 100) / 100f, rand.Next(0, 100) / 100f);
            for (int i = start; i < end; i++)
            {
                spum.ImageElement[i].Color = randomColor;
            }
        }
        
        public void SetBody(int asset)
        {
            switch (asset)
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
