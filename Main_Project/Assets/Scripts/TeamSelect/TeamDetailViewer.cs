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
    [SerializeField] private List<Image> cardImages;       // 가문 공통 배경
    [SerializeField] private List<Image> teamAttributes;   // 가문 공통 속성
    [SerializeField] private List<TMP_Text> characterName;
    [SerializeField] private List<Button> characterButtons;
    [SerializeField] private TMP_Text characterExplanationText;
    [SerializeField] private TMP_Text familyNameText;
    [SerializeField] private CanvasGroup panelCanvasGroup;


    private GameObject _selectedCharacterObject;
    private readonly List<AsyncOperationHandle<Sprite>> _portraitHandles = new();

    private void Start()
    {
        this.characterDetailsPanel.SetActive(false);
    }

    public void ShowDetails(string familyName)
    {
        if (!UnitDataManager.Instance.IsLoaded)
        {
            Debug.LogWarning("❗ 유닛 데이터가 아직 로드되지 않았습니다.");
            return;
        }

        List<CharacterData> characters =
            UnitDataManager.Instance.GetFamilyUnits(familyName);

        if (characters == null || characters.Count == 0)
        {
            Debug.LogWarning($"❌ 가문 데이터 없음: {familyName}");
            return;
        }

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

        this.familyNameText.text = familyName;
        
        // 가문 공통 이미지 (Resources)
        string attributePath = $"TeamAttributes/{familyName}attribute";
        string cardPath = $"TeamCards/{familyName}card";

        Sprite attributeSprite = Resources.Load<Sprite>(attributePath);
        Sprite cardSprite = Resources.Load<Sprite>(cardPath);

        foreach (Image img in this.teamAttributes)
            img.sprite = attributeSprite;

        foreach (Image img in this.cardImages)
            img.sprite = cardSprite;
        

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
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    this.characterPortraits[index].sprite = op.Result;
                }
                else
                {
                    Debug.LogError($"❌ Portrait 로드 실패: {key}");
                }
            };

            this._portraitHandles.Add(handle);

            this.characterName[i].text = character.Unit_Name;

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

        Image img = this._selectedCharacterObject.GetComponent<Image>();
        if (img != null && ShaderController.Instance != null)
        {
            img.material = ShaderController.Instance.normalOutlineMaterial;
        }

        this._selectedCharacterObject = null;
    }

    public void OnCloseButtonClick()
    {
        this.ClearSelection();
        
        this.panelCanvasGroup.DOFade(0f, 0.2f)
            .OnComplete(() =>
            {
                this.characterDetailsPanel.SetActive(false);
            });

        this.characterExplanationText.text = string.Empty;
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
        // 이전 선택 해제
        if (this._selectedCharacterObject != null)
        {
            Image prevImage = this._selectedCharacterObject.GetComponent<Image>();
            if (prevImage != null && ShaderController.Instance != null)
            {
                prevImage.material = ShaderController.Instance.normalOutlineMaterial;
            }
        }

        // 새로운 선택
        Image newImage = clickedCardObject.GetComponent<Image>();
        if (newImage != null && ShaderController.Instance != null)
        {
            newImage.material = ShaderController.Instance.cardOutlineMaterial;
            this._selectedCharacterObject = clickedCardObject;
        }

        this.characterExplanationText.text = character.Description;
    }
}
