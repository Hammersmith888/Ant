using System;
using System.Collections.Generic;
using System.Linq;
using DesperateDevs.Utils;
using UnityEngine;
using UnityEditor;

[Serializable]
// TODO : Dont need, can be remeved
public class ReplaceColliderPrefab : EditorWindow
{
    public List<PolygonCollider2D> _from;
    public List<Collider2D> _to;

    public ReplaceColliderPrefab()
    {
        _from = new List<PolygonCollider2D>();
        _to = new List<Collider2D>();
    }
    [MenuItem("Window/ReplaceColliderPrefab")]
    public static void Open()
    {
        GetWindow<ReplaceColliderPrefab>("ReplaceColliderPrefab", true);
    }

    private void OnGUI()
    {
        ScriptableObject target = this;
        var so = new SerializedObject(target);
        var colliderFrom = so.FindProperty("_from");
        var colliderTo = so.FindProperty("_to");
        
        EditorGUILayout.PropertyField(colliderFrom, true);
        EditorGUILayout.PropertyField(colliderTo, true);

        so.ApplyModifiedProperties();
        var canDisplayButton = _from.Any() && _to.Any() && _from.Count == _to.Count;
        if (canDisplayButton && GUILayout.Button("Копировать"))
        {
            for (var i = 0; i < _from.Count; i++)
            {
                var from = _from[i];
                var to = _to[i];
                Copy(from, to);
            }
            if (_from.Any() || _to.Any())
            {
                return;
            }
            Debug.LogError($"{this} : From - To  пустые");
        }
    }

    private void Copy(PolygonCollider2D from, Component to)
    {
        var target = to.gameObject.AddComponent<PolygonCollider2D>();
        from.CopyPublicMemberValues(target);
        DestroyImmediate(to, true);
        if (target) {
 
            target.gameObject.SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);
        }
    }
}
