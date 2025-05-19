using UnityEngine;

namespace Battle.Scripts.Value.Data
{
    [System.Serializable]
    public class CharacterInfo
    {
        //character 고유 번호
        public string characterKey;
        
        //character 이름
        public string name;

        //character 생김새
        public string bodyPath1, bodyPath2, bodyPath3, bodyPath4, bodyPath5, bodyPath6;
        public string eyePath1, eyePath2;
        public SerializableColor eyeColor;

        public string hairPath;
        public SerializableColor hairColor;
        public int hairMaskIndex;

        public string faceHairPath;
        public SerializableColor faceHairColor;

        public string helmetPath;
        public int helmetMaskIndex;

        public string clothPath1, clothPath2, clothPath3;
        public string pantsPath1, pantsPath2;
        public string armorPath1, armorPath2, armorPath3;
        public string backPath;

        public string weaponPath1, weaponPath2;
        
        //character 스탯
        //...
        
        //character 위치
        public Vector3 position;
    }
}