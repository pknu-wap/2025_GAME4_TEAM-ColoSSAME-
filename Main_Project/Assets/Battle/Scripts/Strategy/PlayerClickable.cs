using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.Scripts.Strategy
{
    public class PlayerClickable : MonoBehaviour, IPointerClickHandler
    {
        public StrategyManager strategyManager;

        public void OnPointerClick(PointerEventData eventData)
        {
            gameObject.GetComponent<SpriteRenderer>().material.color = Color.gray;
            if (strategyManager != null)
            {
                strategyManager.ClickedPlayer = transform;
                strategyManager.hasPlayer = true;
                if (strategyManager.hasCharacter)
                {
                    strategyManager.Move();
                }
            }
        }
    }
}