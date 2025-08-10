using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Team.FighterRandomBuy;
using Scripts.Team.IsAnimStopClick;

public class CardAnim : MonoBehaviour
{   
    public GetPlayer getplayer;

    public CardClickStop blockclick;

    public void CardAnimEnd()
        {
            getplayer.fighterimage[getplayer.cardnumber].SetActive(true);
            getplayer.cardopen[getplayer.cardnumber] = 1;
            blockclick.IsClickCard();
        }
}
