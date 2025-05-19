using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.Scripts.Strategy
{
    public class CharacterClickable : MonoBehaviour, IPointerClickHandler
    {
        public StrategyManager strategyManager;

        public void OnPointerClick(PointerEventData eventData)
        {
            gameObject.GetComponent<SpriteRenderer>().material.color = Color.gray;
            
            if (strategyManager != null)
            {
                if (!strategyManager.hasCharacter)
                {
                    strategyManager.ClickedCharacter = transform;
                    Debug.Log("🖱 클릭된 오브젝트: " + gameObject.name);
                    strategyManager.hasCharacter = true;
                    if (strategyManager.hasPlayer) strategyManager.Move();
                } else
                {
                    strategyManager.ResetCharacter();
                    gameObject.GetComponent<SpriteRenderer>().material.color = Color.white;
                }
            }
        }
    }
}