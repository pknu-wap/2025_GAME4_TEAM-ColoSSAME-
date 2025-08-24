using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Team.FighterRandomBuy;
using Scripts.Team.IsAnimStopClick;

namespace Scripts.Team.CardAnimcontrol
{
    public class CardAnim : MonoBehaviour
    {   
        public GetPlayer getplayer;

        public CardClickStop blockclick;

        public void CardAnimEnd()
        {
            getplayer.CharacterGather[getplayer.count].SetActive(true);
            blockclick.IsClickCard();
        }
        public void CardResetAnim()
        {
            for (int i = 0; i < 10; i++)
            {
                getplayer.anim[i].SetTrigger("IsUnitBuy");
            }
        }
    }
}
