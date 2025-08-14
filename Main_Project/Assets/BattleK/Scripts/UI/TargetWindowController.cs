using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetWindowController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText;
    public Button firstTargetSlot;
    public Button secondTargetSlot;
    public TextMeshProUGUI firstTargetText;
    public TextMeshProUGUI secondTargetText;

    [Header("직업 버튼들 (라벨은 한글: 검사/도끼병/창병/도적/궁수/마법사)")]
    public Button[] jobButtons;

    [Header("캐릭터 이미지")]
    public Image characterImage; // CharacterImg/Image 오브젝트 연결

    private enum TargetSlot { None, First, Second }
    private TargetSlot currentSelectedSlot = TargetSlot.None;

    // 현재 타겟창이 가리키는 캐릭터 키(이름)
    private string _currentCharacterKey;

    void Start()
    {
        // 슬롯 클릭 시 현재 선택 슬롯 설정
        if (firstTargetSlot)  firstTargetSlot.onClick.AddListener(() => SelectSlot(TargetSlot.First));
        if (secondTargetSlot) secondTargetSlot.onClick.AddListener(() => SelectSlot(TargetSlot.Second));

        // 직업 버튼에 클릭 이벤트 연결 (UI 라벨은 한글, 저장은 enum)
        if (jobButtons != null)
        {
            foreach (Button btn in jobButtons)
            {
                if (!btn) continue;
                var label = btn.GetComponentInChildren<TextMeshProUGUI>()?.text;
                btn.onClick.AddListener(() => SetJobToSelectedSlot(label));
            }
        }
    }

    // 창이 다시 활성화될 때(닫았다 열었을 때) 복원
    void OnEnable()
    {
        if (!string.IsNullOrEmpty(_currentCharacterKey))
            SyncUIFromSave();
    }

    /// <summary>
    /// 캐릭터 이름/이미지 설정 + 저장된 타겟 복원(항상 이 함수로 진입)
    /// </summary>
    public void SetCharacter(string name)
    {
        _currentCharacterKey = name?.Trim();

        SetCharacterName(_currentCharacterKey);
        LoadCharacterImage(_currentCharacterKey);

        // JSON에서 기존 targetClasses 복원 → UI 적용
        SyncUIFromSave();
    }

    public void SetCharacterName(string name)
    {
        if (characterNameText != null) characterNameText.text = name;
        else Debug.LogError("characterNameText가 연결되어 있지 않습니다!");
    }

    private void LoadCharacterImage(string characterName)
    {
        if (characterImage == null)
        {
            Debug.LogError("characterImage가 연결되어 있지 않습니다!");
            return;
        }

        if (string.IsNullOrEmpty(characterName))
        {
            characterImage.sprite = null;
            return;
        }

        // Resources/ScreenShots 폴더에서 이미지 로드(확장자 없이 파일명=캐릭터키)
        Sprite sprite = Resources.Load<Sprite>($"ScreenShots/{characterName}");
        if (sprite != null)
        {
            characterImage.sprite = sprite;
            characterImage.color = Color.white; // 투명 방지
            // Debug.Log($"이미지 로드 성공: {characterName}");
        }
        else
        {
            Debug.LogWarning($"이미지를 찾을 수 없습니다: {characterName} (경로: Resources/ScreenShots/{characterName}.png)");
            characterImage.sprite = null;
        }
    }

    // 저장된 값을 UI에 반영(라벨은 한글로)
    private void SyncUIFromSave()
    {
        var mgr = PlayerCharacterSaveManager.Instance;
        if (mgr == null || string.IsNullOrEmpty(_currentCharacterKey)) return;

        var rec = mgr.GetOrCreate(_currentCharacterKey);

        // 새 포맷: List<UnitClass> 기준
        if (rec.targetClasses != null && rec.targetClasses.Count > 0)
        {
            // enum → 한글 라벨 변환
            if (firstTargetText)
            {
                if (rec.targetClasses.Count > 0 && TargetClassMap.EnumToKo.TryGetValue(rec.targetClasses[0], out var ko1))
                    firstTargetText.text = ko1;
                else
                    firstTargetText.text = "타겟1";
            }

            if (secondTargetText)
            {
                if (rec.targetClasses.Count > 1 && TargetClassMap.EnumToKo.TryGetValue(rec.targetClasses[1], out var ko2))
                    secondTargetText.text = ko2;
                else
                    secondTargetText.text = "타겟2";
            }
            return;
        }

        // 구버전 호환(문자열로 저장돼 있을 수 있음)
        if (firstTargetText && !string.IsNullOrEmpty(rec.target1)) firstTargetText.text = rec.target1;
        if (secondTargetText && !string.IsNullOrEmpty(rec.target2)) secondTargetText.text = rec.target2;
    }

    private void SelectSlot(TargetSlot slot)
    {
        currentSelectedSlot = slot;
        HighlightSlot(slot);
    }

    private void HighlightSlot(TargetSlot slot)
    {
        var selectedColor = new Color(1f, 1f, 0.7f);
        var normalColor   = Color.white;

        if (firstTargetSlot)  firstTargetSlot.image.color  = (slot == TargetSlot.First)  ? selectedColor : normalColor;
        if (secondTargetSlot) secondTargetSlot.image.color = (slot == TargetSlot.Second) ? selectedColor : normalColor;
    }

    // 버튼 라벨(한글)을 눌렀을 때: UI는 한글로 표시, 저장은 Enum으로
    private void SetJobToSelectedSlot(string jobName)
    {
        if (currentSelectedSlot == TargetSlot.None)
        {
            Debug.Log("먼저 슬롯을 클릭하세요.");
            return;
        }

        // UI 표시(한글 라벨)
        if (currentSelectedSlot == TargetSlot.First && firstTargetText)      firstTargetText.text  = jobName;
        else if (currentSelectedSlot == TargetSlot.Second && secondTargetText) secondTargetText.text = jobName;

        // 저장(Enum 리스트)
        if (!string.IsNullOrEmpty(_currentCharacterKey) && PlayerCharacterSaveManager.Instance != null)
        {
            var targets = new List<UnitClass>(2);

            if (firstTargetText && TargetClassMap.TryKoToEnum(firstTargetText.text, out var t1))
                targets.Add(t1);
            if (secondTargetText && TargetClassMap.TryKoToEnum(secondTargetText.text, out var t2))
                targets.Add(t2);

            PlayerCharacterSaveManager.Instance.UpdateTargetClasses(_currentCharacterKey, targets);
        }
    }

    // 수동 초기화 버튼 등에만 사용(자동 호출 X)
    public void ResetAll()
    {
        if (characterNameText) characterNameText.text = "캐릭터 이름";
        if (firstTargetText)   firstTargetText.text   = "타겟1";
        if (secondTargetText)  secondTargetText.text  = "타겟2";
        if (characterImage)    characterImage.sprite  = null;

        currentSelectedSlot = TargetSlot.None;
        HighlightSlot(TargetSlot.None);
        // _currentCharacterKey 는 유지 (필요시 null 처리)
    }
}
