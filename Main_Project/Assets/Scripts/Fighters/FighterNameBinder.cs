using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

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
    public Text expCostText;
    
    [Header("playerTrain 안의 버튼")]
    public GetExpButton getExpButton;
    
    [Header("슬롯 자식 오브젝트 이름")]
    public string nameTextObjectName = "Text (Legacy)";
    public string portraitImageObjectName = "playerImage";

    private List<AsyncOperationHandle<Sprite>> loadedHandles = new List<AsyncOperationHandle<Sprite>>();

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
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            yield break;
        }

        if (fighterListParent == null)
        {
            Debug.LogError("❌ fighterListParent가 비어있습니다.");
            yield break;
        }

        var myUnits = UserManager.Instance.user.myUnits;
        if (myUnits == null)
        {
            Debug.LogError("❌ myUnits가 null입니다.");
            yield break;
        }
        
        if (getExpButton != null)
        {
            getExpButton.curLevelText = curLevelText;
            getExpButton.curExpText   = curExpText;
            getExpButton.expSlider    = expSlider;
            getExpButton.expCostText  = expCostText;
        }
        else
        {
            Debug.LogWarning("⚠️ GetExpButton 참조가 비어있습니다.");
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

            if (i < myUnits.Count && myUnits[i] != null)
            {
                Unit unit = myUnits[i];

                if (nameText != null)
                    nameText.text = unit.unitName;

                data.unitId    = unit.unitId;
                data.unitClass = unit.unitClass;

                if (portraitImage != null && !string.IsNullOrEmpty(unit.unitId))
                {
                    portraitImage.sprite  = null;
                    portraitImage.enabled = false;
                    
                    string portraitAddress = GetPortraitAddress(unit.unitId);
                    Debug.Log($"[Portrait Load Try] unitId={unit.unitId}, address={portraitAddress}");

                    yield return StartCoroutine(LoadUnitPortrait(portraitAddress, portraitImage));
                }
                else
                {
                    Debug.LogWarning($"⚠️ [{slot.name}]에서 playerImage를 찾지 못했거나 unitId가 비어있습니다.");
                }
            }
            else
            {
                if (nameText != null)
                    nameText.text = "";

                data.unitId    = "";
                data.unitClass = "";

                if (portraitImage != null)
                {
                    portraitImage.sprite  = null;
                    portraitImage.enabled = false;
                }
            }
        }

        Debug.Log("✅ FighterNameBinder 세팅 완료");
    }

    private string GetPortraitAddress(string unitId)
    {
        return $"Portrait/{unitId}";
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

    private IEnumerator LoadUnitPortrait(string addressKey, Image targetImage)
    {
        // ← 로드 시작 전 비활성화
        if (targetImage != null)
            targetImage.enabled = false;
        
        AsyncOperationHandle<IList<IResourceLocation>> locHandle =
            Addressables.LoadResourceLocationsAsync(addressKey, typeof(Sprite));

        yield return locHandle;

        if (locHandle.Status != AsyncOperationStatus.Succeeded ||
            locHandle.Result == null ||
            locHandle.Result.Count == 0)
        {
            Debug.LogError($"❌ Addressables key를 찾지 못했습니다: {addressKey}");

            if (targetImage != null)
            {
                targetImage.sprite  = null;
                targetImage.enabled = false;
            }

            Addressables.Release(locHandle);
            yield break;
        }

        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(addressKey);
        loadedHandles.Add(handle);

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
        {
            if (targetImage != null)
            {
                targetImage.sprite         = handle.Result;
                targetImage.enabled        = true;  // ← 로드 완료 후 활성화
                targetImage.preserveAspect = true;
            }
        }
        else
        {
            Debug.LogError($"❌ Addressables Sprite 로드 실패: {addressKey}");

            if (targetImage != null)
            {
                targetImage.sprite  = null;
                targetImage.enabled = false;
            }
        }

        Addressables.Release(locHandle);
    }

    private void OnDestroy()
    {
        foreach (var handle in loadedHandles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        loadedHandles.Clear();
    }
}