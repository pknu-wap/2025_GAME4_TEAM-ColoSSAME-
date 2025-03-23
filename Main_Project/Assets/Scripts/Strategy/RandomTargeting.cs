using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTargeting : MonoBehaviour
{
    private TargetingSystem targetingSystem;
    int random;
    private void Start()
    {
        targetingSystem = GetComponent<TargetingSystem>();
    }

    public void FindRandomTarget()        //Å¸°Ù ¼³Á¤
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 10f, targetingSystem.enemyLayer);
        Transform RandomEnemy = null;

        Debug.Log("Å¸°Ù Ã£´Â Áß..."); //½ÇÇà ¿©ºÎ È®ÀÎ

        random = Random.Range(0, enemies.Length);
        Debug.Log($"·£´ý ¼ýÀÚ : {random}");
        Collider2D enemy = enemies[random];
        RandomEnemy = enemy.transform;

        if (RandomEnemy != null)
        {
            targetingSystem.target = RandomEnemy;
            Debug.Log("Å¸°Ù ¼³Á¤µÊ: " + targetingSystem.target.name); //Å¸°Ù ¼³Á¤ È®ÀÎ
        }
        else
        {
            Debug.Log("Å¸°Ù ¾øÀ½!");
        }
    }
}
