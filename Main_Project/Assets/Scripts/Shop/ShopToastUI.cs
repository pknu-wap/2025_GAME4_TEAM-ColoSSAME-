using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShopToastUI : MonoBehaviour
{
    public Text infoText;
    private Coroutine c;

    public void Show(string msg, float seconds)
    {
        if (infoText == null) return;

        infoText.text = msg;
        infoText.gameObject.SetActive(true);

        if (c != null) StopCoroutine(c);
        c = StartCoroutine(HideAfter(seconds));
    }

    private IEnumerator HideAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        infoText.gameObject.SetActive(false);
        c = null;
    }
}