using TMPro;
using UnityEngine;

namespace Battle.Scripts.UI {
    public class WinnerText : MonoBehaviour {
        GameObject[] Characters;
        public TextMeshProUGUI test;
        void Awake ()
        {
            gameObject.SetActive(false);
            test.text = "";
        }

        public void Win ()
        {
            test.text = "You win!";
        }

        public void Lose () 
        {
            test.text = "You lose!";
        }
    }
}
