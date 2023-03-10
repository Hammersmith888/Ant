using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class SteeringTarget
    {
        public Transform LinkedTransfom { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Position { get; private set; }

        public void Initialize(Transform linkedTransfom)
        {
            LinkedTransfom = linkedTransfom;
            TeleportToLinked();
        }

        /// <summary>
        /// Установить позицию таргета за которым следует игрок
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(Vector3 position)
        {
            position.z = LinkedTransfom.position.z;
            Position = position;
        }

        /// <summary>
        /// Установить вращение таргета за которым следует игрок
        /// </summary>
        public void SetRotation(Quaternion rotation)
        {
            Rotation = rotation;
        }

        /// <summary>
        /// Переместить таргет к связанному объекту
        /// </summary>
        public void TeleportToLinked()
        {
            SetPosition(LinkedTransfom.position);
            SetRotation(LinkedTransfom.rotation);
        }
    }
}