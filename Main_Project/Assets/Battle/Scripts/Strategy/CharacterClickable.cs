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
            if (strategyManager != null)
            {
                strategyManager.ClickedCharacter = transform;
                Debug.Log("🖱 클릭된 오브젝트: " + gameObject.name);
                strategyManager.hasCharacter = true;
                if(strategyManager.hasPlayer) strategyManager.Move();
            }
            gameObject.GetComponent<SpriteRenderer>().material.color = Color.gray;
        }
    }
}