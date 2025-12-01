using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class TeamDetailViewer : MonoBehaviour
{
    public GameObject characterDetailsPanel;
    public List<Image> characterPortraits;
    public List<Image> cardImages;
    public List<Image> teamAttributes;
    public List<TMP_Text> characterName;
    public List<Button> characterButtons;
    public TMP_Text characterExplanationText;
    public TMP_Text familyName;

    // 이전에 선택된 카드 오브젝트를 추적하기 위한 변수
    private GameObject selectedCharacterObject = null;
    
    void Start()
    {
        characterDetailsPanel.SetActive(false);
    }
    
    public void ShowDetails(string fid)
    {
        characterDetailsPanel.SetActive(true);
        
        string jsonPath = $"CharacterData/{fid}";
        string attributePath = $"TeamAttributes/{fid}attribute";
        string cardPath = $"TeamCards/{fid}card";
        TextAsset textAsset = Resources.Load<TextAsset>(jsonPath);
        
        if (textAsset == null)
        {
            Debug.LogError($"❌ JSON 파일을 찾을 수 없습니다: {jsonPath}");
            return;
        }

        // ➡️ JSON 데이터를 클래스 객체로 파싱합니다.
        CharacterList characterList = JsonUtility.FromJson<CharacterList>(textAsset.text);
        familyName.text = characterList.Family_Name;
        OnCharacterCardClick(characterList.Characters[0],characterButtons[0].gameObject);
        for (int i = 0; i < 5; i++)
        {
            // 초상화 이미지 로드 및 할당
            string portraitFile = characterList.Characters[i].Visuals.Portrait.Replace(".png", "");
            string portraitPath = $"CharacterData/{portraitFile}";
            Sprite loadedSprite = Resources.Load<Sprite>(portraitPath);
            characterPortraits[i].sprite = loadedSprite;
            characterName[i].text = characterList.Characters[i].Unit_Name;
            
            teamAttributes[i].sprite=Resources.Load<Sprite>(attributePath);
            cardImages[i].sprite=Resources.Load<Sprite>(cardPath);
            
            int cardIndex = i;
            characterButtons[i].onClick.RemoveAllListeners();
            characterButtons[i].onClick.AddListener(() => OnCharacterCardClick(characterList.Characters[cardIndex],characterButtons[cardIndex].gameObject));
        }
    }
    
    public void OnCloseButtonClick()
    {
        characterDetailsPanel.SetActive(false);
        characterExplanationText.text = "";
    }

    private void OnCharacterCardClick(CharacterData character,GameObject clickedCardObject)
    {
        // 1. 이전에 선택된 캐릭터가 있다면 외곽선 제거
        if (selectedCharacterObject != null)
        {
            Image prevImage = selectedCharacterObject.GetComponent<Image>();
            if (prevImage != null && ShaderController.Instance != null)
            {
                prevImage.material = ShaderController.Instance.normalOutlineMaterial; 
            }
        }
    
        // 2. 새로운 선택된 카드에 머티리얼 적용
        Image newImage = clickedCardObject.GetComponent<Image>();
        if (newImage != null)
        {
            // ShaderController에서 UI 전용 외곽선 머티리얼을 가져와 적용한다.
            if (ShaderController.Instance != null)
            {
                newImage.material = ShaderController.Instance.cardOutlineMaterial;
            }
        
            // 현재 선택된 오브젝트를 저장한다.
            selectedCharacterObject = clickedCardObject;
        }
        
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