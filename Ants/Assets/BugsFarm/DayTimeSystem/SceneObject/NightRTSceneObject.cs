using UnityEngine;

namespace BugsFarm.DayTimeSystem
{
    public class NightRTSceneObject : MonoBehaviour
    {
        [SerializeField] private MeshFilter _placeHolderMeshFilter;
        [SerializeField] private MeshRenderer _placeHolder;
        [SerializeField] private Camera _camera;

        public Mesh GetMesh()
        {
            return _placeHolderMeshFilter.mesh;
        }

        public void SetMesh(Mesh mesh)
        {
            _placeHolderMeshFilter.mesh = mesh;
            _placeHolderMeshFilter.mesh.MarkModified();
        }

        public void SetupCamera(Camera other)
        {
            var mask = _camera.cullingMask;
            _camera.CopyFrom(other);
            _camera.cullingMask = mask;
            _camera.backgroundColor = new Color(); // black with alpha zero
            _camera.depth = -1;
        }
        
        public void SetRenderTexture(RenderTexture texture)
        {
            _placeHolder.material.mainTexture = texture;
            _camera.targetTexture = texture;
        }

        public Vector3 UvToRTSpace(Vector3 uvSpacePoint)
        {
            uvSpacePoint.z = _placeHolder.transform.localPosition.z;
            uvSpacePoint = _camera.ViewportToWorldPoint(uvSpacePoint);
            return _placeHolder.transform.worldToLocalMatrix.MultiplyPoint3x4(uvSpacePoint);
        }
    }
}