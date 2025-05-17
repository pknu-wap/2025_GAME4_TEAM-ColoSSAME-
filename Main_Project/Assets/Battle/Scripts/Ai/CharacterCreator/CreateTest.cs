using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Battle.Scripts.Ai.CharacterCreator {
    public class CreateTest : MonoBehaviour
    {
        public void RerenderAllParts(GameObject character)
        {
            var spum = character.GetComponent<SPUM_Prefabs>();
            if (spum != null && spum.ImageElement != null)
            {
                ApplyParts(character, spum.ImageElement);
            }
        }
    
        public void ApplyParts(GameObject character, List<PreviewMatchingElement> partList)
        {
            // ✅ 실제 렌더링 가능한 MatchingElement 목록을 불러온다
            var allMatchingElements = character.GetComponentsInChildren<SPUM_MatchingList>(true)
                .SelectMany(mt => mt.matchingTables)
                .ToList();

            foreach (var matchingElement in allMatchingElements)
            {
                var match = partList.Find(ie =>
                        ie.UnitType == matchingElement.UnitType &&
                        ie.PartType == matchingElement.PartType &&
                        ie.Dir == matchingElement.Dir &&
                        ie.Structure == matchingElement.Structure &&
                        ie.PartSubType == matchingElement.PartSubType
                    );

                if (match != null)
                {
                    Sprite loadedSprite = LoadSpriteFromMultiple(match.ItemPath, match.Structure);
                    matchingElement.renderer.sprite = loadedSprite;
                    matchingElement.renderer.color = match.Color;
                    matchingElement.renderer.maskInteraction = (SpriteMaskInteraction)match.MaskIndex;
                }
            }
        }
    
        public Sprite LoadSpriteFromMultiple(string path, string spriteName)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"No sprites found at path: {path}");
                return null;
            }

            Sprite found = System.Array.Find(sprites, sprite => sprite.name == spriteName);
            return found != null ? found : sprites[0]; // 못 찾으면 첫 번째 반환
        }

    }
}
