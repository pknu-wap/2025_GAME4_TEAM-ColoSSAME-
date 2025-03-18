using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//신 변경

public class Changer : MonoBehaviour 
{
	public void SceneChange() 
	{
		SceneManager.LoadScene("Ingame");
	}
}
