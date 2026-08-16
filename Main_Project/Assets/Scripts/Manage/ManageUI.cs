using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BattleK.Scripts.Manager;

public class ManageUI : MonoBehaviour
{
    [Header("왼쪽 리스트")]
    [SerializeField] private Transform characterListParent;

    [Header("오른쪽 캐릭터 상세 정보 (charInfo)")]
    [SerializeField] private Image charInfoImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text agilityText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text healthText;

    [Header("하위 기능 버튼")]
    [SerializeField] private Button skillButton;
    [SerializeField] private Button itemButton;

    [Header("하위 페이지")]
    [SerializeField] private GameObject skillPage;
    [SerializeField] private GameObject itemPage;

    private string selectedUnitId;

    private readonly List<GameObject> characterSlots = new List<GameObject>();
    
    private readonly AddressableAssetLoader<Sprite> portraitLoader = new AddressableAssetLoader<Sprite>();

    private void Awake()
    {
        CollectCharacterSlots();
        BindCharacterSlotClicks();

        if (skillButton != null)
        {
            skillButton.onClick.AddListener(OnSkillButtonClicked);
        }

        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OnItemButtonClicked);
        }

        SetDetailButtonsInteractable(false);
    }

    private void OnEnable()
    {
        PopulateCharacterSlots();
    }

    private void OnDestroy()
    {
        if (skillButton != null)
        {
            skillButton.onClick.RemoveListener(OnSkillButtonClicked);
        }

        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(OnItemButtonClicked);
        }

        portraitLoader.ReleaseAll();
    }
    
    private void CollectCharacterSlots()
    {
        characterSlots.Clear();

        if (characterListParent == null)
        {
            Debug.LogWarning("[ManageUI] characterListParent가 연결되어 있지 않습니다.");
            return;
        }

        foreach (Transform child in characterListParent)
        {
            characterSlots.Add(child.gameObject);
        }
    }

    private void BindCharacterSlotClicks()
    {
        for (int i = 0; i < characterSlots.Count; i++)
        {
            int slotIndex = i;
            Button slotButton = characterSlots[slotIndex].GetComponent<Button>();

            if (slotButton == null)
            {
                Debug.LogWarning($"[ManageUI] 슬롯 {slotIndex}에 Button 컴포넌트가 없습니다.");
                continue;
            }

            slotButton.onClick.AddListener(() => OnCharacterSlotClicked(slotIndex));
        }
    }
    
    private void PopulateCharacterSlots()
    {
        var myUnits = UserManager.Instance.user.myUnits;

        for (int i = 0; i < characterSlots.Count; i++)
        {
            Image slotImage = characterSlots[i].GetComponent<Image>();

            if (slotImage == null)
            {
                Debug.LogWarning($"[ManageUI] 슬롯 {i}에 Image 컴포넌트가 없습니다.");
                continue;
            }

            if (i >= myUnits.Count)
            {
                slotImage.sprite = null;
                continue;
            }

            string unitId = myUnits[i].unitId;
            StartCoroutine(LoadSlotPortraitRoutine(slotImage, unitId));
        }
    }

    private IEnumerator LoadSlotPortraitRoutine(Image slotImage, string unitId)
    {
        yield return CharacterInfoProvider.LoadPortraitByUnitIdAsync(
            portraitLoader,
            unitId,
            sprite =>
            {
                slotImage.sprite = sprite;
                slotImage.preserveAspect = true;
            },
            () => Debug.LogWarning($"[ManageUI] 슬롯 초상화 로드 실패: {unitId}"));
    }

    private void OnCharacterSlotClicked(int slotIndex)
    {
        var myUnits = UserManager.Instance.user.myUnits;

        if (slotIndex < 0 || slotIndex >= myUnits.Count)
        {
            return;
        }

        Unit selectedUnit = myUnits[slotIndex];
        selectedUnitId = selectedUnit.unitId;

        DisplayCharacterDetail(selectedUnit);
        SetDetailButtonsInteractable(true);
    }
    
    private void DisplayCharacterDetail(Unit unit)
    {
        CharacterData characterData = CharacterInfoProvider.GetCharacterData(unit.unitId);

        if (characterData == null)
        {
            Debug.LogWarning($"[ManageUI] 캐릭터 데이터를 찾을 수 없습니다: {unit.unitId}");
            return;
        }

        if (charInfoImage != null)
        {
            StartCoroutine(LoadCharacterDetailPortrait(characterData));
        }

        if (nameText != null)
        {
            nameText.text = characterData.Unit_Name;
        }

        if (levelText != null)
        {
            levelText.text = $"Lv.{unit.level}";
        }

        DisplayStats(characterData);
    }
    
    private void DisplayStats(CharacterData characterData)
    {
        Stat_Distribution stats = characterData.Stat_Distribution;

        if (stats == null)
        {
            Debug.LogWarning($"[ManageUI] Stat_Distribution이 없습니다: {characterData.Unit_ID}");
            return;
        }

        if (attackText != null)
        {
            attackText.text = stats.ATK.ToString();
        }

        if (agilityText != null)
        {
            agilityText.text = stats.AGI.ToString();
        }

        if (defenseText != null)
        {
            defenseText.text = stats.DEF.ToString();
        }

        if (healthText != null)
        {
            healthText.text = stats.HP.ToString();
        }
    }
    
    private IEnumerator LoadCharacterDetailPortrait(CharacterData characterData)
    {
        yield return CharacterInfoProvider.LoadPortraitAsync(
            portraitLoader,
            characterData,
            sprite =>
            {
                charInfoImage.sprite = sprite;
                charInfoImage.preserveAspect = true;
            },
            () => Debug.LogWarning($"[ManageUI] 캐릭터 초상화 로드 실패: {characterData.Unit_ID}"));
    }

    private void SetDetailButtonsInteractable(bool interactable)
    {
        if (skillButton != null)
        {
            skillButton.interactable = interactable;
        }

        if (itemButton != null)
        {
            itemButton.interactable = interactable;
        }
    }
    
    private void OnSkillButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedUnitId))
        {
            return;
        }

        if (skillPage != null)
        {
            skillPage.SetActive(true);
        }

        // TODO: 선택된 캐릭터(selectedUnitId) 기준 스킬 설정 로직 연결 예정
    }
    
    private void OnItemButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedUnitId))
        {
            return;
        }

        if (itemPage != null)
        {
            itemPage.SetActive(true);
        }

        // TODO: 선택된 캐릭터(selectedUnitId) 기준 아이템 장착 로직 연결 예정
    }
}