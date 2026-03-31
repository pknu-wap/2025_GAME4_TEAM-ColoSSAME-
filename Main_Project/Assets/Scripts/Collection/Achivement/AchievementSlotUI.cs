using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 도감 슬롯(한 줄) 담당.
/// - 이 슬롯은 하나의 groupId(도감 라인)의 "현재 단계"를 보여준다.
/// - 수령 버튼을 누르면 progress가 올라가고, 자동으로 다음 단계가 표시된다.
/// </summary>
public class AchievementSlotUI : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("AchievementDatabase에 등록된 groupId를 정확히 입력")]
    public string groupId;

    [Header("UI (Unity Text)")]
    public Text groupNameText;        // Text(GroupName) - 필요 없으면 비워도 됨
    public Text stageTitleText;       // Text(StageTitle)
    public Text stageDescText;        // Text(StageDescription)
    public Text rewardText;           // Text(RewardText)
    public Text statusText;           // Text(StatusText)
    public Button claimButton;        // Button(ClaimButton)

    private AchievementPageUI page;

    /// <summary>
    /// 페이지(UI)에서 슬롯을 등록할 때 호출. 버튼 이벤트 연결.
    /// </summary>
    public void Bind(AchievementPageUI ownerPage)
    {
        page = ownerPage;

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClickClaim);
        }
    }

    private void OnClickClaim()
    {
        if (page == null) return;
        if (string.IsNullOrEmpty(groupId)) return;

        page.TryClaim(groupId);
    }

    /// <summary>
    /// 현재 도감 상태를 읽어서 슬롯 내용을 갱신한다.
    /// </summary>
    public void Refresh(AchievementManager manager)
    {
        if (manager == null)
        {
            SetStatusOnly("⚠️ Manager 없음", false);
            return;
        }

        if (string.IsNullOrEmpty(groupId))
        {
            SetAll("설정 필요", "groupId를 입력하세요", "", "", "⚠️ 설정 필요", false);
            return;
        }

        // 그룹/현재 단계
        AchievementGroup group = manager.GetGroup(groupId);
        AchievementStage stage = manager.GetCurrentStage(groupId);

        // 그룹명 (원하면 표시, 아니면 groupNameText는 안 써도 됨)
        if (groupNameText != null)
        {
            if (group != null && !string.IsNullOrEmpty(group.displayName))
                groupNameText.text = group.displayName;
            else
                groupNameText.text = groupId;
        }

        // ✅ 전체 완료(마지막 단계까지 수령 완료)
        if (stage == null)
        {
            SetAll(
                (groupNameText != null ? groupNameText.text : groupId),
                "완료",
                "모든 보상을 수령했습니다.",
                "",
                "✅ 완료",
                false
            );
            return;
        }

        // 현재 단계 표시 (이게 곧 "도감 한 줄")
        if (stageTitleText != null) stageTitleText.text = stage.title;
        if (stageDescText != null) stageDescText.text = stage.description;
        if (rewardText != null) rewardText.text = $"보상: {stage.rewardGold}G";

        bool canClaim = manager.IsCurrentStageCompleted(groupId);
        if (statusText != null) statusText.text = canClaim ? "✅ 수령 가능" : "❌ 조건 미달";
        if (claimButton != null) claimButton.interactable = canClaim;
    }

    private void SetStatusOnly(string status, bool interactable)
    {
        if (statusText != null) statusText.text = status;
        if (claimButton != null) claimButton.interactable = interactable;
    }

    private void SetAll(string groupName, string title, string desc, string reward, string status, bool interactable)
    {
        if (groupNameText != null) groupNameText.text = groupName;
        if (stageTitleText != null) stageTitleText.text = title;
        if (stageDescText != null) stageDescText.text = desc;
        if (rewardText != null) rewardText.text = reward;
        if (statusText != null) statusText.text = status;
        if (claimButton != null) claimButton.interactable = interactable;
    }
}