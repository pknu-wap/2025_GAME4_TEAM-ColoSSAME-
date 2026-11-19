using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(LoadingScreen))]
public class LoadingScreenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var screen = (LoadingScreen)target;
        if (DrawDefaultInspector() && Application.isPlaying)
            screen.RefreshLayout();

        var scaler = screen.GetComponentInParent<CanvasScaler>();
        if (scaler == null) return;
        float referenceWidth = scaler.referenceResolution.x;
        var size = screen.OverlayBarSize(referenceWidth);
        if (size == Vector2.zero)
        {
            EditorGUILayout.HelpBox("진행 바를 찾을 수 없습니다. Progress Bar에 바를 연결하세요.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("새 게임 진행 바 미리보기", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"기준 폭 {referenceWidth:0}에서 {size.x:0.#} × {size.y:0.#}");

        var area = GUILayoutUtility.GetRect(10f, 40f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(area, new Color(0.12f, 0.10f, 0.08f));
        GUI.BeginGroup(area);
        float ratio = area.width / referenceWidth;
        var bar = new Rect(Vector2.zero, size * ratio);
        bar.center = area.size * 0.5f;
        EditorGUI.DrawRect(bar, new Color(0.85f, 0.65f, 0.28f));
        GUI.EndGroup();
    }
}
