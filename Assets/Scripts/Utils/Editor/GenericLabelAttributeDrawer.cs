using UnityEngine;
using UnityEditor;

namespace LD54.Utils.Editors
{
    [CustomPropertyDrawer(typeof(GenericLabelAttribute))]
    public class GenericLabelAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GenericLabelAttribute a = attribute as GenericLabelAttribute;
            label.text = $"{a._prefix}{fieldInfo.FieldType.Name}{a._suffix}";
            EditorGUI.PropertyField(position, property, label, property.isExpanded);
        }
    }
}
