using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Team.FighterRandomBuy;

public class CardAnim : MonoBehaviour
{   
    public GetPlayer getplayer;

    public void CardAnimEnd()
        {
            getplayer.fighterimage[getplayer.cardnumber].SetActive(true);
            getplayer.cardopen[getplayer.cardnumber] = 1;
        }
}
