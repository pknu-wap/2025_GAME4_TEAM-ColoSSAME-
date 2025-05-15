using UnityEngine;

namespace Battle.Scripts.UI {
    public class NextScene : MonoBehaviour
    {
        void Awake ()
        {
            gameObject.SetActive(false);
        }

        public void ButtonOn ()
        {
            gameObject.SetActive (true);
        }
    }
}
