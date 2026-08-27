using UnityEngine;
using TMPro;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.Manager;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerStatsCollector collector;
    [SerializeField] private CalculateManager calculateManager;

    [SerializeField] private TextMeshProUGUI[] statText;


    public void Refresh()
    {
        //calculateManager.RefreshPlayerOnly();

        var stats = calculateManager.AllStats;

        for (int i = 0; i < statText.Length; i++)
        {
            if (i >= stats.Count)
            {
                statText[i].text = "";
                continue;
            }

            CharacterStatsRow stat = stats[i];

            statText[i].text =
                $"ATK : {stat.ATK}\n" +
                $"DEF : {stat.DEF}\n" +
                $"HP : {stat.HP}\n" +
                $"AGI : {stat.AGI}";
        }
    }
}