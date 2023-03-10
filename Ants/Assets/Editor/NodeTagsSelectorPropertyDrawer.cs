using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NodeTagsSelectorAttribute))]
public class NodeTagsSelectorPropertyDrawer : PropertyDrawer
{
    public int Index = 0;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        
        if (property.propertyType == SerializedPropertyType.String)
        {
            if (AstarPath.active == null)
                return;
            var tagList = AstarPath.active.GetTagNames().ToList();
            EditorGUI.BeginProperty(position, label, property);

            var propertyString = property.stringValue;
            Index = tagList.IndexOf(propertyString);

            //Draw the popup box with the current selected index
            Index = EditorGUI.Popup(position, label.text, Index, tagList.ToArray());
            
            //Adjust the actual string value of the property based on the selection
            if (Index >= 0)
            {
                property.stringValue = tagList[Index];
            }
            else
            {
                property.stringValue = "";
            }

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
