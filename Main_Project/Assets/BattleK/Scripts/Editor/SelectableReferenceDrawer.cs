using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BattleK.Scripts.Utils; // Attribute가 있는 곳

namespace BattleK.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(SelectableReferenceAttribute))]
    public class SelectableReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 1. 타입을 선택할 버튼의 영역 (상단 한 줄)
            Rect buttonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            // 현재 할당된 클래스 이름 가져오기
            string typeName = property.managedReferenceValue?.GetType().Name ?? "None (Null)";
            
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent($"Logic Type: {typeName}"), FocusType.Keyboard))
            {
                // 필드 타입(ISkillLogic) 추출
                Type fieldType = fieldInfo.FieldType.IsGenericType ? 
                    fieldInfo.FieldType.GetGenericArguments()[0] : fieldInfo.FieldType;
                
                ShowTypeMenu(property, fieldType);
            }

            // 2. 클래스 내부 필드들을 그릴 영역 (버튼 바로 아래부터)
            Rect contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, 
                                        position.width, position.height - EditorGUIUtility.singleLineHeight - 2);

            // 클래스가 할당된 경우에만 내부 필드 표시 (겹침 방지)
            if (property.managedReferenceValue != null)
            {
                EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);
            }

            EditorGUI.EndProperty();
        }

        private void ShowTypeMenu(SerializedProperty property, Type targetType)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () => {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            // 프로젝트 내의 모든 어셈블리를 뒤져서 상속받은 클래스 찾기
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => targetType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        // 높이를 동적으로 계산해야 리스트 항목끼리 겹치지 않습니다.
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = EditorGUIUtility.singleLineHeight + 4;
            if (property.managedReferenceValue == null) return baseHeight;
            
            // 클래스 내부 필드 높이 추가
            return baseHeight + EditorGUI.GetPropertyHeight(property, true);
        }
    }
}