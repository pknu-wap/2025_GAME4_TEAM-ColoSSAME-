using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Changer : MonoBehaviour 
{
	public void SceneChange() 
	{
		SceneManager.LoadScene("Ingame");
	}
}
