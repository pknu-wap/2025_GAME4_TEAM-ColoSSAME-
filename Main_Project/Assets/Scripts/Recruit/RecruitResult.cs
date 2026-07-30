using UnityEngine;

[System.Serializable]
public class RecruitResult
{
    public CharacterData Character { get; }
    public int AcquiredRarity { get; }
    public bool IsDuplicate { get; }
    public ItemData RewardItem { get; }
    public int RewardStoneAmount { get; }

    public RecruitResult(CharacterData character, int acquiredRarity, bool isDuplicate, ItemData rewardItem, int rewardStoneAmount)
    {
        Character = character;
        AcquiredRarity = acquiredRarity;
        IsDuplicate = isDuplicate;
        RewardItem = rewardItem;
        RewardStoneAmount = rewardStoneAmount;
    }
}