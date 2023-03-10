using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SortingLayerSelectorAttribute))]
public class SortingLayerSelectorPropertyDrawer : PropertyDrawer
{
    public int Index = 0;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (AstarPath.active == null)
                return;
            var tagList = SortingLayer.layers.Select(x => x.name).ToArray();
            string propertyString = property.stringValue;
            Index = 0;
            for (int i = 0; i < tagList.Length; i++)
            {
                if (tagList[i] == propertyString)
                {
                    Index = i;
                    break;
                }
                else if(i+1 == tagList.Length) // Default если не один из слоев не назначен
                {
                    Index = i;
                    break;
                }
            }

            //Draw the popup box with the current selected index
            Index = EditorGUI.Popup(position, label.text, Index, tagList);
            
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
