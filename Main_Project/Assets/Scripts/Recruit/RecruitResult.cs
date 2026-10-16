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
    public bool RequiresReplacement { get; }

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

    private RecruitResult(CharacterData character, int acquiredRarity, ItemData cancelRewardItem, int cancelRewardStoneAmount, bool requiresReplacement)
        : this(character, acquiredRarity, false, cancelRewardItem, cancelRewardStoneAmount)
    {
        RequiresReplacement = requiresReplacement;
    }

    public static RecruitResult Fail(RecruitStatus status)
    {
        return new RecruitResult(status);
    }

    public static RecruitResult PendingReplacement(CharacterData character, int acquiredRarity, ItemData cancelRewardItem, int cancelRewardStoneAmount)
    {
        return new RecruitResult(character, acquiredRarity, cancelRewardItem, cancelRewardStoneAmount, true);
    }

    public RecruitResult AsAcquired()
    {
        return new RecruitResult(Character, AcquiredRarity, false, null, 0);
    }

    public RecruitResult AsDuplicateReward()
    {
        return new RecruitResult(Character, AcquiredRarity, true, RewardItem, RewardStoneAmount);
    }
}
