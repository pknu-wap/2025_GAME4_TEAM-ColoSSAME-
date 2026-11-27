using UnityEngine;
using UnityEngine.UI;

public static class StrategyRoomKey
{
    public static void Display(Image image, string familyId)
    {
        if (image == null) return;
        image.sprite = string.IsNullOrEmpty(familyId)
            ? null
            : Resources.Load<Sprite>("StrategyKeys/" + familyId);
        image.enabled = image.sprite != null;
    }
}
