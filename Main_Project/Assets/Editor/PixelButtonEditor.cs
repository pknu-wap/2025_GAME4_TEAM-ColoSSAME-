using UnityEditor;
using UnityEditor.UI;

// ButtonEditor는 하위 클래스의 필드를 그리지 않으므로 PixelButton의 글자 설정을 따로 그린다.
[CustomEditor(typeof(PixelButton), true)]
[CanEditMultipleObjects]
public class PixelButtonEditor : ButtonEditor
{
    private static readonly string[] LabelFields =
    {
        "label", "labelGraphic", "pressedOffset", "labelColor", "labelDisabledColor"
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Label", EditorStyles.boldLabel);
        foreach (var field in LabelFields)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
        }
        serializedObject.ApplyModifiedProperties();
    }
}
