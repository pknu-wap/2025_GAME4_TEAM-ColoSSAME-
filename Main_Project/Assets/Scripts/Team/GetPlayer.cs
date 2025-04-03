using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//팀 관리 -> 선수영입입

public class GetPlayer : MonoBehaviour
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
        other1.SetActive(false);
        other2.SetActive(false);
        other3.SetActive(false);
        other4.SetActive(false);
        other5.SetActive(false);
        other6.SetActive(false);
        target.SetActive(true);
    }
    void Update()
    {
        
    }
}
