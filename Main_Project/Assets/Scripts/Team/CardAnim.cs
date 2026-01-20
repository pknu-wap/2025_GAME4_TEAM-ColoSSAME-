

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

        private int myIndex; // 카드의 고유 인덱스

        public void SetIndex(int index)
        {
            myIndex = index;
        }

        public void CardAnimEnd()
        {
            getplayer.CharacterGather[myIndex].SetActive(true);
            getplayer.CharacterGetCheck[myIndex] = 2; 
            //blockclick.IsClickCard();
             getplayer.OnAnyCardAnimEnd();
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
