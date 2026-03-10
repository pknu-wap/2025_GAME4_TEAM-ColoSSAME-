using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가챠 테이블(SO)
/// - Item / Money 결과를 한 리스트에 섞어서 가중치 기반으로 뽑는다.
/// </summary>
[CreateAssetMenu(menuName = "Game/Gacha Table", fileName = "GachaTableSO")]
public class GachaTableSO : ScriptableObject
{
    public enum RewardType
    {
        Item,
        Money
    }

    [Serializable]
    public class GachaEntry
    {
        [Tooltip("보상 타입 (아이템/돈)")]
        public RewardType rewardType = RewardType.Item;

        [Tooltip("가중치(클수록 잘 나옴). 0이면 절대 안 나옴")]
        public int weight = 1;

        [Header("Item 보상일 때만 사용")]
        public int itemId = -1;
        public int itemCount = 1;

        [Header("Money 보상일 때만 사용")]
        public int goldAmount = 0;
    }

    [Header("가챠 풀")]
    public List<GachaEntry> entries = new List<GachaEntry>();

    [Header("안전장치")]
    [Tooltip("아이템 보상인데 DB에 없는 ID면 자동 제외할지(권장: true)")]
    public bool ignoreInvalidItemId = true;

    [Tooltip("돈 보상 최소값(실수 방지)")]
    public int minGoldReward = 100;
}