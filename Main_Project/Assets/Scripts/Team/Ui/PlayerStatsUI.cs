using UnityEngine;
using TMPro;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private CalculateManager calculateManager;
    [SerializeField] private TextMeshProUGUI[] statText;

    public void Refresh()
    {
        calculateManager.RefreshPlayerPreviewOnly();

        var stats = calculateManager.PlayerPreviewStats;

        for (int i = 0; i < statText.Length; i++)
        {
            if (i >= stats.Count)
            {
                statText[i].text = "";
                continue;
            }

            UnitBaseStat stat = stats[i];

            statText[i].text =
                $"ATK : {stat.BaseAtk}\n" +
                $"DEF : {stat.BaseDef}\n" +
                $"HP : {stat.BaseHp}\n" +
                $"AGI : {stat.BaseAgi}";
        }
    }
}