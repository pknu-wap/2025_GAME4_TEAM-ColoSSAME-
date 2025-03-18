using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//선수영입 -> 팀 관리

public class BackPlayer : MonoBehaviour
{
    public GameObject other1;
    public GameObject other2;
    public GameObject other3;
    public GameObject other4;
    public GameObject other5;
    public GameObject other6;
    public GameObject target;

    public void click()
    {
        other1.SetActive(true);
        other2.SetActive(true);
        other3.SetActive(true);
        other4.SetActive(true);
        other5.SetActive(true);
        other6.SetActive(true);
        target.SetActive(false);
    }
}
