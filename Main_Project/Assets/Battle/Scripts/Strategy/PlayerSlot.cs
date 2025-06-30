using UnityEngine;

namespace Battle.Scripts.Strategy
{
    public class PlayerSlot : MonoBehaviour
    {
        public bool isOccupied = false;
        public Transform occupiedCharacter = null;
        public float yOffset = 0.2f;
        public float snapRange = 0.5f;

        private SpriteRenderer highlightRenderer;

        private void Awake()
        {
            // 자식 Highlight 오브젝트의 SpriteRenderer 찾기
            Transform highlight = transform.Find("Highlight");
            if (highlight != null)
            {
                highlightRenderer = highlight.GetComponent<SpriteRenderer>();
                if (highlightRenderer != null)
                {
                    highlightRenderer.enabled = false; // 기본은 꺼둠
                }
            }
        }

        public void EnableHighlight(bool enable)
        {
            if (highlightRenderer != null)
            {
                highlightRenderer.enabled = enable;
                highlightRenderer.color = Color.yellow;
            }
        }

        public void SnapCharacter(Transform character)
        {
            if (isOccupied && occupiedCharacter != null)
            {
                var draggable = occupiedCharacter.GetComponent<DraggableCharacter>();
                if (draggable != null)
                {
                    draggable.ReturnToOriginalPosition();
                }
            }

            Vector3 targetPos = transform.position;
            targetPos.y += yOffset;
            character.position = targetPos;

            isOccupied = true;
            occupiedCharacter = character;
        }

        public void ClearSlot()
        {
            isOccupied = false;
            occupiedCharacter = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, yOffset, 0), snapRange);
        }
    }
}