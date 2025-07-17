using UnityEngine;
using UnityEngine.UI;

public class HideMyClone : MonoBehaviour
{
    public GameObject nego;
    public Button backHome;

    void Start()
    {
        backHome.onClick.AddListener(OnBackHomeClicked);
    }

    void OnBackHomeClicked()
    {
        if (nego != null)
            nego.SetActive(false);
    }
}