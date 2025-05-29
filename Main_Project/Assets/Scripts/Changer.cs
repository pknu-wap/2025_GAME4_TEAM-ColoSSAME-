using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Battle.Scripts.Ai.CharacterCreator;
using Battle.Scripts.Ai;
//신 변경

public class Changer : MonoBehaviour 
{	

	public void SceneChange() 
	{
		StartCoroutine(createDelay());
	}
	IEnumerator createDelay()
	{
		 yield return new WaitForSeconds(5f);
		 SceneManager.LoadScene("playerSave");
	}
	
}
