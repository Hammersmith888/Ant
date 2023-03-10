using UnityEngine;

namespace Pathfinding.Examples
{
    /// <summary>
    /// Smooth Camera Following.
    /// \author http://wiki.unity3d.com/index.php/SmoothFollow2
    /// </summary>
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_astar_smooth_follow2.php")]
    public class AstarSmoothFollow2 : MonoBehaviour
    {
        public Transform target;
        public float distance = 3.0f;
        public float height = 3.0f;
        public float damping = 5.0f;
        public bool smoothRotation = true;
        public bool followBehind = true;
        public float rotationDamping = 10.0f;
        public bool staticOffset = false;

        void LateUpdate()
        {
            if (target)
            {
                Vector3 wantedPosition;

                if (staticOffset)
                {
                    wantedPosition = target.position + new Vector3(0, height, distance);
                }
                else
                {
                    if (followBehind)
                        wantedPosition = target.TransformPoint(-distance, height, transform.position.z);
                    else
                        wantedPosition = target.TransformPoint(distance, height, transform.position.z);
                }
                transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping);

                if (smoothRotation)
                {
                    Quaternion wantedRotation = Quaternion.LookRotation(Vector3.forward, target.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
                }
                else transform.LookAt(target, target.up);
            }
        }
    }
}
