using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExternalSpumApplier : MonoBehaviour
{
    public GameObject spumCharacterPrefab;
    public List<PreviewMatchingElement> partsToApply;

    void Start()
    {
        GameObject instance = Instantiate(spumCharacterPrefab);
        ApplyParts(instance, partsToApply);
    }

    public void ApplyParts(GameObject character, List<PreviewMatchingElement> partList)
    {
        // 1. matching list 수집
        var matchingTables = character.GetComponentsInChildren<SPUM_MatchingList>(true);
        var allMatchingElements = matchingTables.SelectMany(mt => mt.matchingTables).ToList();

        foreach (var matchingElement in allMatchingElements)
        {
            var matchingTypeElement = partList.FirstOrDefault(ie =>
                ie.UnitType == matchingElement.UnitType &&
                ie.PartType == matchingElement.PartType &&
                ie.Dir == matchingElement.Dir &&
                ie.Structure == matchingElement.Structure &&
                ie.PartSubType == matchingElement.PartSubType
            );

            if (matchingTypeElement != null)
            {
                Sprite loadSprite = LoadSpriteFromMultiple(matchingTypeElement.ItemPath, matchingTypeElement.Structure);
                matchingElement.renderer.sprite = loadSprite;
                matchingElement.renderer.color = matchingTypeElement.Color;
                matchingElement.renderer.maskInteraction = (SpriteMaskInteraction)matchingTypeElement.MaskIndex;
            }
        }
    }

    // 외부에서도 쓸 수 있도록 LoadSprite 함수 복사
    public Sprite LoadSpriteFromMultiple(string path, string spriteName)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found at path: {path}");
            return null;
        }
        Sprite foundSprite = System.Array.Find(sprites, sprite => sprite.name == spriteName);
        return foundSprite != null ? foundSprite : sprites[0];
    }
}
