using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TeamDetailViewer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject characterDetailsPanel;
    [SerializeField] private List<Image> characterPortraits;
    [SerializeField] private List<TMP_Text> characterName;
    [SerializeField] private List<Button> characterButtons;
    [SerializeField] private TMP_Text characterExplanationText;
    [SerializeField] private TMP_Text familyNameText;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TMP_Text characterTitleText;   // 구분선 아래 큰 이름
    [SerializeField] private Sprite selectedCardSprite;     // 선택한 카드 틀 (portrait_selected)


    private GameObject _selectedCharacterObject;
    private readonly Dictionary<Image, Sprite> _normalCardSprites = new();
    private readonly List<AsyncOperationHandle<Sprite>> _portraitHandles = new();

    private List<CharacterData> shownCharacters;
    private CharacterData shownCharacter;
    private string shownFamilyName;

    private void Start()
    {
        this.characterDetailsPanel.SetActive(false);
        Loc.Changed += RefreshTexts;
    }

    private void OnDestroy()
    {
        Loc.Changed -= RefreshTexts;
        shownCharacters = null;
        ReleasePortraits();
    }

    private void RefreshTexts()
    {
        if (shownCharacters == null || shownCharacters.Count == 0) return;
        this.familyNameText.text = PanelTitle(shownCharacters[0]);
        for (int i = 0; i < shownCharacters.Count && i < this.characterName.Count; i++)
            this.characterName[i].text = CardName(shownCharacters[i]);
        if (shownCharacter != null)
            ShowCharacterTexts(shownCharacter);
    }

    public void ShowDetails(string familyName)
    {
        if (!UnitDataManager.Instance.IsLoaded)
        {
            Debug.LogWarning("❗ 유닛 데이터가 아직 로드되지 않았습니다.");
            return;
        }

        List<CharacterData> characters =
            UnitDataManager.Instance.GetFamilyUnits(familyName)?.FindAll(unit => !unit.Unit_ID.Contains("_Recruit_"));

        if (characters != null && characters.Count > characterButtons.Count)
            characters = characters.GetRange(0, characterButtons.Count);

        if (characters == null || characters.Count == 0)
        {
            Debug.LogWarning($" 가문 데이터 없음: {familyName}");
            return;
        }

        this.panelCanvasGroup.DOKill();
        this.ReleasePortraits();
        this.characterDetailsPanel.SetActive(true);
        this.panelCanvasGroup.alpha = 0f;
        this.panelCanvasGroup.DOFade(1f, 0.25f)
            .OnComplete(() =>
            {
                // 기본 선택 캐릭터
                this.OnCharacterCardClick(
                    characters[0],
                    this.characterButtons[0].gameObject);
            });

        for (int i = 0; i < characterButtons.Count; i++)
            characterButtons[i].gameObject.SetActive(i < characters.Count);

        shownCharacters = characters;
        shownFamilyName = familyName;
        this.familyNameText.text = PanelTitle(characters[0]);

        // =========================
        // 🔹 캐릭터 개별 Portrait (Addressables)
        // =========================
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];

            string portraitFile = character.Visuals.Portrait;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(portraitFile);
            string key = $"Portrait/{fileName}";

            int index = i; // 클로저 방지

            AsyncOperationHandle<Sprite> handle =
                Addressables.LoadAssetAsync<Sprite>(key);

            handle.Completed += op =>
            {
                if (shownCharacters != characters) return;
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    this.characterPortraits[index].sprite = op.Result;
                }
                else
                {
                    Debug.LogError($" Portrait 로드 실패: {key}");
                }
            };

            this._portraitHandles.Add(handle);

            this.characterName[i].text = CardName(character);

            int buttonIndex = i;
            this.characterButtons[i].onClick.RemoveAllListeners();
            this.characterButtons[i].onClick.AddListener(() =>
                this.OnCharacterCardClick(
                    characters[buttonIndex],
                    this.characterButtons[buttonIndex].gameObject));
        }
    }

    private void ClearSelection()
    {
        if (this._selectedCharacterObject == null)
            return;

        SetCardSelected(this._selectedCharacterObject, false);
        this._selectedCharacterObject = null;
    }

    public void OnCloseButtonClick()
    {
        this.ClearSelection();
        this.panelCanvasGroup.DOKill();
        shownCharacters = null;
        shownCharacter = null;

        this.panelCanvasGroup.DOFade(0f, 0.2f)
            .OnComplete(() =>
            {
                this.characterDetailsPanel.SetActive(false);
            });

        this.characterExplanationText.text = string.Empty;
        if (this.characterTitleText != null) this.characterTitleText.text = string.Empty;
        this.ReleasePortraits();
    }

    private void ReleasePortraits()
    {
        foreach (AsyncOperationHandle<Sprite> handle in this._portraitHandles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        this._portraitHandles.Clear();
    }

    private void OnCharacterCardClick(CharacterData character, GameObject clickedCardObject)
    {
        if (this._selectedCharacterObject != null)
            SetCardSelected(this._selectedCharacterObject, false);

        SetCardSelected(clickedCardObject, true);
        this._selectedCharacterObject = clickedCardObject;

        shownCharacter = character;
        ShowCharacterTexts(character);
    }

    private void ShowCharacterTexts(CharacterData character)
    {
        if (this.characterTitleText != null)
            this.characterTitleText.text = Loc.UnitName(character.Unit_ID, character.Unit_Name);
        this.characterExplanationText.text = Loc.UnitDescription(character.Unit_ID, character.Description);
    }

    // 선택한 카드는 selected 틀로 고정, 나머지는 원래 틀 + Sprite Swap(hover·pressed)
    private void SetCardSelected(GameObject card, bool selected)
    {
        var image = card.GetComponent<Image>();
        var button = card.GetComponent<Button>();
        if (image == null) return;
        if (!this._normalCardSprites.ContainsKey(image))
            this._normalCardSprites[image] = image.sprite;

        image.sprite = selected && this.selectedCardSprite != null ? this.selectedCardSprite : this._normalCardSprites[image];
        // Sprite Swap은 overrideSprite로 그림을 덮으므로, 남아 있는 상태 그림(selected/pressed)을 지워야 위 sprite가 보인다
        image.overrideSprite = null;
        if (button != null)
            button.transition = selected ? Selectable.Transition.None : Selectable.Transition.SpriteSwap;
    }

    private static string PanelTitle(CharacterData character) =>
        Loc.Get("house_core_title", Loc.FamilyName(character.Family_ID, character.Family_ID));

    // "제피로스 카이루스" → "제피로스\n카이루스"
    private static string CardName(CharacterData character)
    {
        string name = Loc.UnitName(character.Unit_ID, character.Unit_Name) ?? string.Empty;
        int space = name.LastIndexOf(' ');
        return space > 0 ? name.Substring(0, space) + "\n" + name.Substring(space + 1) : name;
    }
}
