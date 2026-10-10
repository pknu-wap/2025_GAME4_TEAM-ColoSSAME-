using System.Collections.Generic;
using BattleK.Scripts.Data.Stat;
using UnityEngine;

public class RecruitManager : MonoBehaviour
{
    private const int FiveStarRarity = 5;
    private const int FourStarRarity = 4;
    private const int ThreeStarRarity = 3;

    [Header("등급 확률 (%)")]
    [SerializeField] private float fiveStarRate = 1f;
    [SerializeField] private float fourStarRate = 10f;

    [Header("중복 보상")]
    [SerializeField] private ItemData duplicateRewardItem;
    [SerializeField] private int duplicateRewardStone = 2;

    private float ThreeStarRateValue => 100f - fiveStarRate - fourStarRate;
    
    // 성공이 확정된 경우에만 골드를 차감한다. cost는 호출 측(RecruitUI)이 전달한다.
    public RecruitResult Recruit(int cost)
    {
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogWarning("[RecruitManager] UserManager가 준비되지 않았습니다.");
            return RecruitResult.Fail(RecruitStatus.Misconfigured);
        }

        int determinedRarity = DetermineRarity();

        List<CharacterData> recruitableCharacters = GetRecruitableCharacters();

        if (recruitableCharacters.Count == 0)
        {
            Debug.LogWarning("[RecruitManager] 뽑기 대상이 없습니다.");
            return RecruitResult.Fail(RecruitStatus.NoCandidates);
        }

        CharacterData selectedCharacter = SelectRandomCharacter(recruitableCharacters);

        bool isDuplicate = IsCharacterOwned(selectedCharacter);

        if (isDuplicate && duplicateRewardItem == null)
        {
            Debug.LogWarning("[RecruitManager] duplicateRewardItem(ItemData)이 연결되어 있지 않습니다.");
            return RecruitResult.Fail(RecruitStatus.Misconfigured);
        }

        if (!UserManager.Instance.SpendGold(cost))
        {
            return RecruitResult.Fail(RecruitStatus.NotEnoughGold);
        }

        RecruitResult result = BuildResult(selectedCharacter, determinedRarity, isDuplicate);
        ApplyResultToUser(result);

        return result;
    }
    
    private int DetermineRarity()
    {
        float roll = Random.Range(0f, 100f);

        if (roll < fiveStarRate)
        {
            return FiveStarRarity;
        }

        if (roll < fiveStarRate + fourStarRate)
        {
            return FourStarRarity;
        }

        return ThreeStarRarity;
    }
    private List<CharacterData> GetRecruitableCharacters()
    {
        string currentFamilyId = FamilyUtility.GetCurrentFamilyId();

        if (string.IsNullOrEmpty(currentFamilyId))
        {
            return new List<CharacterData>();
        }

        List<CharacterData> familyUnits = UnitDataManager.Instance.GetFamilyUnits(currentFamilyId);

        if (familyUnits == null)
        {
            return new List<CharacterData>();
        }

        return familyUnits;
    }
    private CharacterData SelectRandomCharacter(List<CharacterData> candidates)
    {
        int index = Random.Range(0, candidates.Count);
        return candidates[index];
    }

    private bool IsCharacterOwned(CharacterData character)
    {
        return UserManager.Instance.GetMyUnitById(character.Unit_ID) != null;
    }

    private RecruitResult BuildResult(CharacterData character, int rarity, bool isDuplicate)
    {
        ItemData rewardItem = isDuplicate ? duplicateRewardItem : null;
        int stoneAmount = isDuplicate ? duplicateRewardStone : 0;
        return new RecruitResult(character, rarity, isDuplicate, rewardItem, stoneAmount);
    }

    private void ApplyResultToUser(RecruitResult result)
    {
        if (result.IsDuplicate)
        {
            UserManager.Instance.AddItem(result.RewardItem.id.ToString(),result.RewardStoneAmount);
        }
        else
        {
            Unit newUnit = new Unit(
                result.Character.Unit_ID,
                result.AcquiredRarity,
                result.Character.Unit_Name,
                result.Character.Class);

            UserManager.Instance.AddUnit(newUnit);
        }
    }
}