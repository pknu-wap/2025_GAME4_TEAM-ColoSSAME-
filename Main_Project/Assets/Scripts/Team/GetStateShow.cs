using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetStateShow : MonoBehaviour
{
    public GameObject showinformation;

    public void BackButton()
    {
        showinformation.SetActive(false);
    }
}
