using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Team.IsAnimStopClick
{
    public class CardClickStop : MonoBehaviour
    {
        public List<Button> cardList;

        public void IsAnimStopClick()
        {
            foreach(var card in cardList)
            {
                card.interactable = false;
            }
        }

        public void IsClickCard()
        {
            foreach(var card in cardList)
            {
                card.interactable = true;
            }
        }
            
    }
}
