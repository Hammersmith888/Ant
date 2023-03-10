using UnityEditor;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    [ExecuteInEditMode]
    public class NodeConnectorSetter : MonoBehaviour
    {
        public float ConnectionDistance => Mathf.Abs((transform.localScale.x + transform.localScale.y) / 2f);
        public bool Contains(Vector3 point)
        {
            return Vector3.Distance(point, transform.position) <= ConnectionDistance;
        }

    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = Color.red;
            Handles.Disc(Quaternion.identity, transform.position, Vector3.forward, ConnectionDistance, false, 0);
        }

        // public void Start()
        // {
        //     transform.localScale = new Vector3(0.15f,0.15f,1f);
        // }
        #endif 
    }
}
