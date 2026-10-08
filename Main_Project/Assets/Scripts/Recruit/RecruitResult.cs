using UnityEngine;

public enum RecruitStatus
{
    Success,
    NotEnoughGold,
    NoCandidates,
    Misconfigured
}

[System.Serializable]
public class RecruitResult
{
    public RecruitStatus Status { get; }
    public CharacterData Character { get; }
    public int AcquiredRarity { get; }
    public bool IsDuplicate { get; }
    public ItemData RewardItem { get; }
    public int RewardStoneAmount { get; }

    public RecruitResult(CharacterData character, int acquiredRarity, bool isDuplicate, ItemData rewardItem, int rewardStoneAmount)
    {
        Status = RecruitStatus.Success;
        Character = character;
        AcquiredRarity = acquiredRarity;
        IsDuplicate = isDuplicate;
        RewardItem = rewardItem;
        RewardStoneAmount = rewardStoneAmount;
    }

    private RecruitResult(RecruitStatus status)
    {
        Status = status;
    }

    public static RecruitResult Fail(RecruitStatus status)
    {
        return new RecruitResult(status);
    }
}
