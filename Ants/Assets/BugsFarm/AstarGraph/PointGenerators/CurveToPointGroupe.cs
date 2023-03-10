using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class CurveToPointGroupe : PointGenerator
    {
        [Header("Настройка конвертирования группы кривых в группу точек")]
        [Tooltip("Дистанция между точками, влияет на количество созданных точек, по умолчанию 0.4, Погрешность 10% .")]
        [SerializeField] private float _aproximateDistance = 0.3f;
        [Tooltip("Детализация кривой, чем больше значение, тем более точные расчеты")]
        [SerializeField] private int _resolution = 10;
        public int Resolution => _resolution;
        public float AproximateDistance => _aproximateDistance;
        public override IEnumerable<Node> GeneratePointsGroupe(NodeData data)
        {
            var batchNodes = new List<Node>();
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out BezierCurve curve))
                {
                    curve.SetDirty();
                    curve.TryGetComponent(out LayerSetter layerSetter);
                    TryGetComponent(out DependencySetter dependency);
                    var aproxSegments = curve.length / _aproximateDistance;
                    var nSegments = Mathf.FloorToInt(aproxSegments < 3 ? 3 : aproxSegments + 1);
                    var step = 1f / nSegments;
                    var progress = 0f;
                    var isLast = false;


                    while (true)
                    {
                        data.Position = curve.GetPointAt(progress);
                        data.Normal = curve.GetNormalAt(progress);
                        
                        if (layerSetter)
                        {
                            data.Layer = layerSetter.Layer;
                        }

                        if (dependency)
                        {
                            data.DependencyWalkable = dependency.DependencyWalkable;
                            data.DependencyID = dependency.PathID;
                        }

                        Node node;
                        switch (progress)
                        {
                            case 0: case 1:  node = new NodeConnector(data); break;
                            default: node = dependency ? new NodeDependency(data) : new Node(data);
                                break;
                        }
                        batchNodes.Add(node);
                        
                        if (!isLast)
                        {
                            progress = Mathf.Min(progress + step, 1);
                            isLast = progress >= 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
                if(child.TryGetComponent(out NodeJointSetter joint))
                {
                    data.Position = joint.Position;
                    data.Layer = child.GetComponent<LayerSetter>()?.Layer ?? "Default";
                    batchNodes.Add(joint.NodeExclusion
                                       ? new NodeJointExclusion(joint.ConnectionDistance, joint.Offset,
                                                                joint.ExclusionDistance, joint.ExclusionOffset, data)
                                       : new NodeJoint(joint.ConnectionDistance, joint.Offset, data));
                }
            }
            return Connection(batchNodes);
        }
        public void Override(CurveToPointGroupe from)
        {
            _resolution = from.Resolution;
            _aproximateDistance = from.AproximateDistance;
        }
        protected override IEnumerable<Node> Connection(IEnumerable<Node> nodes)
        {
            var addedNodes = nodes.ToArray();

            for (var i = 1; i < addedNodes.Length; i++)
            {
                var node1 = addedNodes[i-1];
                var node2 = addedNodes[i];
                if(node1.IsJoint() || node2.IsJoint()) continue;
                
                if(!node1.IsConnector() || !node2.IsConnector())
                {
                    node1.AddConnection(node2, 1);
                    node2.AddConnection(node1, 1);
                }
            }
            return addedNodes;
        }
        protected void OnValidate()
        {
            if(_isParent)
            {
                var childs = GetComponentsInChildren<CurveToPointGroupe>(true);
                var curves = GetComponentsInChildren<BezierCurve>(true);
                foreach (var item in childs)
                {
                    if(!item.Equals(this))
                        item.Override(this);
                }
                foreach (var item in curves)
                {
                    item.resolution = _resolution;
                }
            }
        }
        
        [ExposeMethodInEditor]
        protected void SetLayerSetterVisible()
        {
            //if(!_isParent) return;
            var beziers = GetComponentsInChildren<BezierCurve>();
            foreach (var bezier in beziers)
            {
                if (!bezier.TryGetComponent(out LayerSetter layerSetter))
                {
                    layerSetter = bezier.gameObject.AddComponent<LayerSetter>();
                }
                layerSetter.SetLayer(_defaultLayer);
            }
        }
    }
}
