using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TeamDetailViewer : MonoBehaviour
{
    public GameObject characterDetailsPanel;
    public List<Image> characterPortraits;
    public List<TMP_Text> characterName;
    public List<Button> characterButtons;
    public TMP_Text characterExplanationText;
    public TMP_Text familyName;

    void Start()
    {
        characterDetailsPanel.SetActive(false);
    }
    
    public void ShowDetails(string fid)
    {
        characterDetailsPanel.SetActive(true);
        
        string jsonPath = $"CharacterData/{fid}";
        TextAsset textAsset = Resources.Load<TextAsset>(jsonPath);
        
        if (textAsset == null)
        {
            Debug.LogError($"❌ JSON 파일을 찾을 수 없습니다: {jsonPath}");
            return;
        }

        // ➡️ JSON 데이터를 클래스 객체로 파싱합니다.
        CharacterList characterList = JsonUtility.FromJson<CharacterList>(textAsset.text);
        familyName.text = characterList.Family_Name;
        if (characterList.Characters.Count != characterPortraits.Count)
        {
            Debug.LogError("캐릭터 수와 카드 슬롯 수가 일치하지 않습니다!");
            return;
        }

        for (int i = 0; i < characterList.Characters.Count; i++)
        {
            // 초상화 이미지 로드 및 할당
            string portraitFile = characterList.Characters[i].Visuals.Portrait.Replace(".png", "");
            string portraitPath = $"CharacterData/{portraitFile}";
            Sprite loadedSprite = Resources.Load<Sprite>(portraitPath);
            characterPortraits[i].sprite = loadedSprite;
            characterName[i].text = characterList.Characters[i].Unit_Name;
            
            int cardIndex = i;
            characterButtons[i].onClick.RemoveAllListeners();
            characterButtons[i].onClick.AddListener(() => OnCharacterCardClick(characterList.Characters[cardIndex]));
        }
        
        if (characterList.Characters.Count > 0)
        {
            OnCharacterCardClick(characterList.Characters[0]);
        }
    }
    
    public void OnCloseButtonClick()
    {
        characterDetailsPanel.SetActive(false);
        characterExplanationText.text = "";
    }

    private void OnCharacterCardClick(CharacterData character)
    {
        characterExplanationText.text = character.Description;
    }
    
    // ➡️ JSON 데이터를 담을 클래스 구조입니다.
    // 이 클래스들은 모두 [System.Serializable] 속성이 있어야 유니티가 JSON 파싱을 할 수 있습니다.

    [System.Serializable]
    public class CharacterList
    {
        public string Family_Name;
        public string Family_Description;
        public List<CharacterData> Characters;
    }
    
    [System.Serializable]
    public class CharacterData
    {
        public string Unit_ID;
        public string Unit_Name;
        public int Rarity;
        public int Level;
        public string Class;
        public string Description;
        public string Story;
        public StatDistribution Stat_Distribution;
        public Visuals Visuals;
    }

    [System.Serializable]
    public class StatDistribution
    {
        public int ATK;
        public int DEF;
        public int HP;
        public int AGI;
    }

    [System.Serializable]
    public class Visuals
    {
        public string Portrait;
        public string Prefab;
    }
}