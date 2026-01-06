using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempItem : MonoBehaviour
{
    void Start()
    {
        
    }

    public void getItem()
    {
        UserManager.Instance.AddItem("apple",1);
    }
}
