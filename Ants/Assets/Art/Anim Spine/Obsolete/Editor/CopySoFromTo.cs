using System;
using System.Collections.Generic;
using BugsFarm.AnimationsSystem;
using BugsFarm.UnitSystem.Obsolete;
using UnityEditor;
using UnityEngine;

namespace Art.Anim_Spine.AnimationConfigs.Editor
{
    [Serializable]
    public class CopySoFromTo : EditorWindow
    {
        private static CopySoFromTo _window = null;

        [MenuItem("Window/CopySoFromTo")]
        public static void Open()
        {
            _window = GetWindow<CopySoFromTo>("Places Editor", true);
        }

        public List<AnimRefs_Ant> _from;
        public List<AnimationModel> _to;
        
        private void OnGUI()
        {
            ScriptableObject target = this;
            var so = new SerializedObject(target);

            var fromProperty = so.FindProperty("_from");
            var toProperty = so.FindProperty("_to");
                 
            EditorGUILayout.PropertyField(fromProperty, true);
            EditorGUILayout.PropertyField(toProperty, true);
            
            // if (GUILayout.Button("Копировать"))
            // {
            //     for (var i = 0; i < _from.Count; i++)
            //     {
            //         var fromItem = _from[i];
            //         var toItem = _to[i];
            //
            //         var allitems = fromItem.GetAll();
            //         foreach (var reference in allitems)
            //         {
            //             var newRef = new AnimationReferenceModel();
            //             
            //             newRef.Setup(reference.ReferenceAsset, reference.TimeSacale, AnimRefs_Ant.IsLoop(reference.AnimationType));
            //             toItem.SetAnimReferenceModel(reference.AnimationType,newRef);
            //         }
            //     }
            // }
            
            so.ApplyModifiedProperties();
        }
    }
}