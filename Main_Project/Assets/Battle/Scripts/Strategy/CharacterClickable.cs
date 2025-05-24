using System.Collections;
using Battle.Scripts.ImageManager;
using Battle.Scripts.UI;
using Battle.Scripts.Value.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.Scripts.Strategy
{
    public class CharacterClickable : MonoBehaviour, IPointerClickHandler
    {
        public StrategyManager strategyManager;
        public GameObject EnemyStatusIcon;
        public GameObject EnemyStatusText;
        public TransparentScreenshot iconMaker;

        public void OnPointerClick(PointerEventData eventData)
        {
            gameObject.GetComponent<SpriteRenderer>().material.color = Color.gray;
            if (CompareTag("PlayerCharacter"))
            {
                if (strategyManager == null) return;
                if (strategyManager.hasEnemy)
                {
                    strategyManager.ResetCharacter();
                    strategyManager.ClickedEnemy = null;
                }
                if (!strategyManager.hasCharacter)
                {
                    strategyManager.ClickedCharacter = transform;
                    Debug.Log("🖱 클릭된 오브젝트: " + gameObject.name);
                    strategyManager.hasCharacter = true;
                    if (strategyManager.hasPlayer) strategyManager.Move();
                }
                else
                {
                    strategyManager.ResetCharacter();
                    gameObject.GetComponent<SpriteRenderer>().material.color = Color.white;
                }
            }
            else
            {
                if (strategyManager.hasCharacter)
                {
                    strategyManager.ResetCharacter();
                    strategyManager.ClickedCharacter = null;
                }
                if (!strategyManager.hasEnemy)
                {
                    strategyManager.ClickedEnemy = gameObject;
                    strategyManager.hasEnemy = true;
                    EnemyStatusIcon.GetComponentInChildren<CharacterID>().characterKey =
                        gameObject.GetComponent<CharacterID>().characterKey;
                    EnemyStatusText.GetComponentInChildren<CharacterID>().characterKey =
                        gameObject.GetComponent<CharacterID>().characterKey;
                }
                else
                {
                    strategyManager.ResetCharacter();
                    gameObject.GetComponent<SpriteRenderer>().material.color = Color.gray;
                    strategyManager.ClickedEnemy = gameObject;
                    strategyManager.hasEnemy = true;
                    EnemyStatusIcon.GetComponentInChildren<CharacterID>().characterKey =
                        gameObject.GetComponent<CharacterID>().characterKey;
                    EnemyStatusText.GetComponent<CharacterID>().characterKey = gameObject.GetComponent<CharacterID>().characterKey;
                }
                iconMaker.LoadSpriteForSingle(EnemyStatusIcon, "EnemyCharacter");
                EnemyStatusText.GetComponent<StrategyUIText>().LoadJsonData();
            }
        }
    }
}