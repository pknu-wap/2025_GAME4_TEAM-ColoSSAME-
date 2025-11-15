using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private AICore OwnerAi;
    [SerializeField] private Scrollbar hpsSlider;
    void Awake()
    {
        hpsSlider = GetComponent<Scrollbar>();
        hpsSlider.size = OwnerAi.hp;
    }

    public void UpdateHPBar ()
    {
        hpsSlider.value = OwnerAi.hp;
    }
}
