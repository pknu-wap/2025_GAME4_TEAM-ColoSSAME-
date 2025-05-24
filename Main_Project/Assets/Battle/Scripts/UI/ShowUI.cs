using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowUI : MonoBehaviour
{
   public GameObject targetUI;

   public void ToShowUI()
   {
      targetUI.SetActive(true);
   }
}
