using System.Collections.Generic;
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
    [SerializeField] private int duplicateRewardStone = 20;

    private float ThreeStarRateValue => 100f - fiveStarRate - fourStarRate;
    
    public RecruitResult Recruit()
    {
        int determinedRarity = DetermineRarity();

        List<CharacterData> recruitableCharacters = GetRecruitableCharacters();

        if (recruitableCharacters.Count == 0)
        {
            Debug.LogWarning("[RecruitManager] 뽑기 대상이 없습니다.");
            return null;
        }

        CharacterData selectedCharacter = SelectRandomCharacter(recruitableCharacters);

        bool isDuplicate = IsCharacterOwned(selectedCharacter);
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
        string currentFamilyId = GetCurrentFamilyId();

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
    private string GetCurrentFamilyId()
    {
        string selectedUnitId = UserManager.Instance.user.myUnits[0].unitId;

        if (string.IsNullOrEmpty(selectedUnitId))
        {
            Debug.LogWarning("[RecruitManager] 선택된 유닛(selectedUnitId)이 없습니다.");
            return null;
        }

        CharacterData selectedCharacterData = UnitDataManager.Instance.GetCharacterData(selectedUnitId);

        if (selectedCharacterData == null)
        {
            Debug.LogWarning($"[RecruitManager] 선택된 유닛의 데이터를 찾을 수 없습니다: {selectedUnitId}");
            return null;
        }

        return selectedCharacterData.Family_ID;
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
            if (result.RewardItem == null)
            {
                Debug.LogWarning("[RecruitManager] duplicateRewardItem(ItemData)이 연결되어 있지 않습니다.");
                return;
            }

            UserManager.Instance.AddItem(result.RewardItem.itemName, result.RewardStoneAmount);
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