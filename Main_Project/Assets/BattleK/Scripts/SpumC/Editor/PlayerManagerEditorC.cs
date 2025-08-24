using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(PlayerManagerC))]
public class PlayerManagerEditorC : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerManagerC manager = (PlayerManagerC)target;

        if (GUILayout.Button("CREATE UNIT"))
        {
            manager.GetPlayerList();
        }
        if (GUILayout.Button("Align UNIT"))
        {
            manager.SetAlignUnits();
        }
        if (GUILayout.Button("CLEAR UNIT"))
        {
            manager.ClearPlayerList();
        }
        if (GUILayout.Button("CAPTURE UNITS"))
        {
            manager.SetScreenShot();
            AssetDatabase.Refresh();
        }
    }
}