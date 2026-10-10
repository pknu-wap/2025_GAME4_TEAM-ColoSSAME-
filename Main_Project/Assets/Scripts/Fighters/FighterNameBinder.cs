using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;
using TMPro;

public class FighterNameBinder : MonoBehaviour
{
    [Header("fighter 슬롯들이 들어있는 부모(예: fighterList)")]
    public Transform fighterListParent;

    [Header("playerTrain 안의 표시 텍스트(Text Legacy)")]
    public Text curLevelText;
    public Text curExpText;
    public Slider expSlider;
    public Text selectedNameText;
    public Image selectedPortraitImage;  
    public TextMeshProUGUI expCostText;
    
    [Header("playerTrain 안의 버튼")]
    public GetExpButton getExpButton;
    
    [Header("업그레이드 매니저")]
    public BuildingUpgradeManager buildingUpgradeManager;
    
    [Header("슬롯 자식 오브젝트 이름")]
    public string nameTextObjectName = "Text (Legacy)";
    public string portraitImageObjectName = "playerImage";

    private readonly AddressableAssetLoader<Sprite> portraitLoader = new AddressableAssetLoader<Sprite>();
    private IEnumerator Start()
    {
        yield return null;
        
        // ← 씬 시작 시 선택 초상화 미리 비활성화
        if (selectedPortraitImage != null)
        {
            selectedPortraitImage.sprite  = null;
            selectedPortraitImage.enabled = false;
        }

        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("UserManager 또는 user가 준비되지 않았습니다.");
            yield break;
        }

        if (fighterListParent == null)
        {
            Debug.LogError("fighterListParent가 비어있습니다.");
            yield break;
        }

        var myUnits = UserManager.Instance.user.myUnits;
        if (myUnits == null)
        {
            Debug.LogError("myUnits가 null입니다.");
            yield break;
        }

        if (buildingUpgradeManager == null)
        {
            Debug.LogWarning("BuildingUpgradeManager 참조가 비어있습니다. 훈련 비용 할인은 적용되지 않습니다.");
        }
        
        if (getExpButton != null)
        {
            getExpButton.curLevelText = curLevelText;
            getExpButton.curExpText   = curExpText;
            getExpButton.expSlider    = expSlider;
            getExpButton.expCostText  = expCostText;
            getExpButton.buildingUpgradeManager = buildingUpgradeManager;
        }
        else
        {
            Debug.LogWarning("GetExpButton 참조가 비어있습니다.");
        }

        for (int i = 0; i < fighterListParent.childCount; i++)
        {
            Transform slot = fighterListParent.GetChild(i);

            Text nameText = FindNameText(slot);
            Image portraitImage = FindPortraitImage(slot);

            FighterSlotData data = slot.GetComponent<FighterSlotData>();
            if (data == null) data = slot.gameObject.AddComponent<FighterSlotData>();

            FighterSlotShowStats show = slot.GetComponent<FighterSlotShowStats>();
            if (show == null) show = slot.gameObject.AddComponent<FighterSlotShowStats>();

            show.slotData              = data;
            show.curLevelText          = curLevelText;
            show.curExpText            = curExpText;
            show.expSlider             = expSlider;
            show.selectedNameText      = selectedNameText;
            show.selectedPortraitImage = selectedPortraitImage;
            show.expCostText           = expCostText;
            show.buildingUpgradeManager = buildingUpgradeManager;

            if (i < myUnits.Count && myUnits[i] != null)
            {
                Unit unit = myUnits[i];

                if (nameText != null)
                    nameText.text = unit.UnitName;

                data.unitId    = unit.Id;
                data.unitClass = unit.UnitClass;

                if (portraitImage != null && !string.IsNullOrEmpty(unit.Id))
                {
                    portraitImage.sprite  = null;
                    portraitImage.enabled = false;
                    
                    Debug.Log($"[Portrait Load Try] unitId={unit.Id}");

                    yield return StartCoroutine(LoadUnitPortrait(unit.Id, portraitImage));                }
                else
                {
                    Debug.LogWarning($"[{slot.name}]에서 playerImage를 찾지 못했거나 unitId가 비어있습니다.");
                }
            }
            else
            {
                if (nameText != null)
                    nameText.text = "";

                data.unitId    = "";
                data.unitClass = UnitClass.None;

                if (portraitImage != null)
                {
                    portraitImage.sprite  = null;
                    portraitImage.enabled = false;
                }
            }
        }

        UpdateSlotActive(myUnits);

        Debug.Log(" FighterNameBinder 세팅 완료");
    }

    private Text FindNameText(Transform slot)
    {
        Transform textTr = slot.Find(nameTextObjectName);
        if (textTr == null) return null;
        return textTr.GetComponent<Text>();
    }

    private Image FindPortraitImage(Transform slot)
    {
        Transform imgTr = slot.Find(portraitImageObjectName);
        if (imgTr == null) return null;
        return imgTr.GetComponent<Image>();
    }

    private IEnumerator LoadUnitPortrait(string unitId, Image targetImage)
    {
        if (targetImage == null)
            yield break;

        targetImage.sprite = null;
        targetImage.enabled = false;

        yield return StartCoroutine(
            portraitLoader.LoadAsync(
                AddressableAssetType.Character,
                unitId,
                sprite =>
                {
                    targetImage.sprite = sprite;
                    targetImage.enabled = true;
                    targetImage.preserveAspect = true;
                },
                () =>
                {
                    targetImage.sprite = null;
                    targetImage.enabled = false;
                }
            )
        );
    }

    private void UpdateSlotActive(List<Unit> myUnits)
    {
        for (int i = 0; i < fighterListParent.childCount; i++)
        {
            fighterListParent.GetChild(i).gameObject.SetActive(i < myUnits.Count);
        }
    }
    
    public void RefreshTrainingUI()
    {
        List<Unit> myUnits = UserManager.Instance.user.myUnits;

        UpdateSlotActive(myUnits);
        StartCoroutine(RefreshPortraits(myUnits));

        getExpButton.RefreshSelectedUnitUI();
    }

    private IEnumerator RefreshPortraits(List<Unit> myUnits)
    {
        for (int i = 0; i < myUnits.Count; i++)
        {
            Transform slot = fighterListParent.GetChild(i);
            Image portraitImage = FindPortraitImage(slot);
        
            yield return StartCoroutine(LoadUnitPortrait(myUnits[i].Id, portraitImage));
        }
    }
}