using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Window/Find Missing Components")]
    private static void FindMissingComponents()
    {
        foreach (GameObject gameObject in FindObjectsOfTypeAll(typeof(GameObject)))
        {
            var components = gameObject.GetComponents<Component>();
            var componentIndex = 1;
            foreach (var component in components)
            {
                if (component == null)
                {
                    var path = gameObject.name;
                    var lastParent = gameObject.transform.parent;
                    while (true)
                    {
                        if (lastParent)
                        {
                            path = path.Insert(0, lastParent.name + "/");
                            lastParent = lastParent.parent;
                        }
                        else
                        {
                            path = path.Insert(0, "Assets/");
                            break;
                        }
                    }
                    Debug.LogError($"Пустой компонент на объекте = '{path}' , По счету {componentIndex}й");
                }

                componentIndex++;
            }
        }
    }
}
