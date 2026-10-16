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

    private Team shownTeam;

    private void Awake() => Loc.Changed += HandleLanguageChanged;
    private void OnDestroy() => Loc.Changed -= HandleLanguageChanged;

    private void HandleLanguageChanged()
    {
        if (shownTeam != null) SetTeam(shownTeam);
    }

    public void SetTeam(Team team)
    {
        shownTeam = team;
        teamNameText.text =
            Loc.Get("rank_team", team.rank, Loc.TeamName(team));

        recordText.text =
            Loc.Get("team_record", team.win, team.draw, team.lose);

        string path =
            $"TeamImages/team_{team.id}";

        teamImage.sprite =
            Resources.Load<Sprite>(path);
    }
}