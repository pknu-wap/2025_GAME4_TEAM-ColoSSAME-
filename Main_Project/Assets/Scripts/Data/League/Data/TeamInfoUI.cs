using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamInfoUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text teamNameText;

    [SerializeField]
    private TMP_Text recordText;

    [SerializeField]
    private Image teamImage;

    [SerializeField] 
    private Hide hideManager;
    
    [SerializeField] 
    private GameObject teamInfoPanel;

    public void SetTeam(Team team)
    {
        teamNameText.text =
            $"{team.rank}등 {team.name}";

        recordText.text =
            $"{team.win}승 {team.draw}무 {team.lose}패";

        string path =
            $"TeamImages/team_{team.id}";

        teamImage.sprite =
            Resources.Load<Sprite>(path);
    }
}