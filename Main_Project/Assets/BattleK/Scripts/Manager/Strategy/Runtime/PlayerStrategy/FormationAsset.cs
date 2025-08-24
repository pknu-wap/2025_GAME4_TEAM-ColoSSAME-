using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Formation/Asset", fileName = "FM_")]
public class FormationAsset : ScriptableObject
{
    [Header("UI Anchored Positions (px, PreviewSlots 기준)")]
    public Vector2[] uiAnchoredPositions;
}