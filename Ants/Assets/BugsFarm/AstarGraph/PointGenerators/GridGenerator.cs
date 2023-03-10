using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class GridGenerator : PointGenerator
    {
        [Tooltip("Сшить точки других сеточных графов, используя края сетки как соединительне ноды")]
        [SerializeField] private bool _useEdgeJoints = false;
        
        [Tooltip("Радиус соединений")]
        [SerializeField] private float _edgeJointsRadius = 0.15f;
        
        [Tooltip("Внешние края сетки могут быть коннекторами")]
        [SerializeField] private bool _useOuterEdgeAsConnectors = false;
        
        [Tooltip("Внутриние края сетки могут быть коннекторами")]
        [SerializeField] private bool _useInnerEdgeAsConnectors = false;

        [Tooltip("Плотность сетки в квадрате")]
        [SerializeField] private float _girdDensity = 1f;
        
        [Tooltip("Размер границы сетки")]
        [SerializeField] private Vector2 _size = Vector2.one;
        
        [Tooltip("Сдвиг границы сетки")]
        [SerializeField] private Vector2 _offset = Vector2.zero;
        
        [Tooltip("Использовать якорь для смещения сетки, если не назначено, якорем будет носитель компонента.")]
        [SerializeField] private Transform _anchorGrid;
        [Tooltip("Использовать родитель контейнер для коннекторов и соединений.")]
        [SerializeField] private GameObject _nodeSetterContainer;

        [Tooltip("Тег для соединительных нод")] 
        [SerializeField] [NodeTagsSelector] private string _jointTag;
    #if UNITY_EDITOR
        [SerializeField] private Color _gridColor = Color.magenta;
    #endif
        public Bounds GridBounds => new Bounds((Vector2)(_anchorGrid ? _anchorGrid : transform).position + _offset,_size);
        public Vector2Int ScaledSize // Размер сетки умноженный на плотность сетки.
        {
            get
            {
                var modifiedSize = _size * _girdDensity;
                var countX = Mathf.FloorToInt(modifiedSize.x);
                var countY = Mathf.FloorToInt(modifiedSize.y);
                return new Vector2Int(countX, countY);
            }
        }
        
        public override IEnumerable<Node> GeneratePointsGroupe(NodeData data)
        {
            var batchNodes = new List<Node>();
            foreach (var point in GenerateGrid())
            {
                if (!point.HasValue)
                {
                    batchNodes.Add(null);
                    continue;
                }

                data.Position = point.Value;
                data.Normal = Vector3.up;
                data.Layer =  _defaultLayer;
                var node = new Node(data);
                batchNodes.Add(node);
            }

            batchNodes.AddRange(GetComponents<NodeJointSetter>().Select(x =>
            {
                data.Position = x.Position;
                data.Layer = x.GetComponent<LayerSetter>()?.Layer ?? "Default";
                data.Tag = (uint) NodeUtils.GetTagID(_jointTag);
                return new NodeJoint(x.ConnectionDistance, x.Offset, data);
            }));
            return Connection(batchNodes);
        }
        protected override IEnumerable<Node> Connection(IEnumerable<Node> nodes)
        {
            var index = 0;
            var countX = ScaledSize.x;
            var countY = ScaledSize.y;
            var points = nodes.ToList();
            var joints = points.OfType<NodeJoint>().ToArray();
            var connectors = GetComponents<NodeConnectorSetter>();
            // Это значение регулирует кол-во соединений, является размером ряда - делает больше размер квадрата.
            // [1] - квадрат 8  кол-во соединений 1:8
            // [2] - квадрат 16 кол-во соединений 1:24
            // [3] - квадрат 24 кол-во соединений 1:48
            const int rowOffset = 1;

            // Проверяет не вышел индекс за пределы границ
            // Нужно не забывать про наличие NodeJoint в конце списка.
            // Используется не кол-во массива, а исходное кол-во точек в сетке.
            bool IsNotOutOfRange(int currentIndex)
            {
                return currentIndex < (countX * countY) && currentIndex >= 0;
            }
            
            // Cоздает ряд индексов используя сдвиг от текущей позиции
            // Заменяет индексы которые вышли за пределы сетки на -1
            IEnumerable<int> GetRowIndices(int centerIndex, int posX)
            {
                // Проверка на допустимые сдвиги в ряду
                var isOutOfRangeLeft  = (posX - rowOffset) < 0;
                var isOutOfRangeRight = (posX + rowOffset) >= countX;
                
                // Сколько точек вышло за границу сетки
                var overflowLeft = isOutOfRangeLeft ? Mathf.Abs(posX - rowOffset) : 0;
                var overflowRight = isOutOfRangeRight ? Mathf.Abs(countX-1 - (posX + rowOffset)) : 0;
                var row = new List<int>();
                
                // Создает ряд с заменой индексов которые вышли за пределы сетки на -1
                void CreateRow(int overflow, bool isForward)
                {
                    var position = 0;
                    while (position < rowOffset)
                    {
                        var currIndex = -1;
                        if (overflow <= 0)
                        {
                            currIndex = centerIndex + (isForward ? (position + 1) : (-rowOffset + position));
                        }
                        row.Add(currIndex); 
                        position++;
                        overflow--;
                    }
                }

                CreateRow(overflowLeft, false);
                row.Add(centerIndex);
                CreateRow(overflowRight, true);
                return row;
            }
            
            // Создает соединение квадратное односторонее соединение
            // Добавляет коннектор или соединительную ноду в край обрезанной сетки
            void OneWayConnection(IEnumerable<int> boxRowIndices, int curentIndex, int posX,int posY)
            {
                var isConnector = false;
                var center = points[curentIndex];
                foreach (var pointIndex in boxRowIndices)
                {
                    var point = IsNotOutOfRange(pointIndex) ? points[pointIndex] : null;
                    if(point.IsNullOrDefault())
                    {
                        isConnector = true;
                        continue;
                    }
                    
                    if(point == center) continue;
                    center?.AddConnection(point, 1);
                }
                
                var modifiedToConnector = IsModifiedNode(center.Position);
                var insideConnector = isConnector && _useInnerEdgeAsConnectors && (posX < countX-1 && posX > 0) && (posY < countY-1 && posY > 0);
                var outSideConnector = isConnector && _useOuterEdgeAsConnectors && ((posX == countX-1 || posX == 0) || (posY == countY-1 || posY == 0));
                if (insideConnector || outSideConnector || modifiedToConnector)
                {
                    var data = new NodeData(center);
                    var connector = new NodeConnector(data);
                    var connections = points[curentIndex].connections;
                    connector.connections = connections;
                    points[curentIndex] = connector;
                        
                    if (_useEdgeJoints && !modifiedToConnector)
                    {
                        data.Tag = (uint)NodeUtils.GetTagID(_jointTag);
                        points.Add(new NodeJoint(Mathf.Abs(_edgeJointsRadius),Vector3.zero,data));
                    }
                }
            }
            
            bool IsModifiedNode(Vector3 node)
            {
                return joints.Any(x => x.Contains(node)) || connectors.Any(x => x.Contains(node));
            }

            // Создает для каждой точки квадрат индексов
            // Каждый квадрат выполняет односторонюю связь центральной точки к остальным в квадрате
            for (var y = 0; y < countY; y++)
            {
                for (var x = 0; x < countX; x++)
                {
                    var node = points[index];
                    if (!node.IsNullOrDefault() && !node.IsJoint())
                    {
                        var boxRowIndices = new List<int>();
                        boxRowIndices.AddRange(GetRowIndices(index - countX, x));
                        boxRowIndices.AddRange(GetRowIndices(index, x));
                        boxRowIndices.AddRange(GetRowIndices(index + countX, x));
                        OneWayConnection(boxRowIndices, index, x, y);
                    }
                    index++;
                }
            }
            
            return points.Where(x => !x.IsNullOrDefault());
        }
        
        private IEnumerable<Vector3?> GenerateGrid()
        {
            var points = new List<Vector3?>();
            var bounds = GridBounds;
            var startGrid = bounds.max.SetX(bounds.min.x);
            var countX = ScaledSize.x;
            var countY = ScaledSize.y;
            
            var stepX = (bounds.max - startGrid).magnitude / countX;
            var stepY = (bounds.max - bounds.max.SetY(bounds.min.y)).magnitude / countY;

            var halfStepX = stepX / 2;
            var halfStepY = stepY / 2;
            
            for (var y = 0; y < countY; y++)
            {
                for (var x = 0; x < countX; x++)
                {
                    var newPoint = new Vector3((stepX * x) + halfStepX, (-stepY * y) - halfStepY)  + startGrid;
                    points.Add(IsModified(newPoint) ? (Vector3?)null : newPoint);
                }
            }

            return points;
        }
        private bool IsModified(Vector3 point)
        {
            return GetComponentsInChildren<BaseModifier>().Any(modifier =>
            {
                var result = Polygon.ContainsPoint(modifier.GetVertices().ToArray(), point);
                result = modifier.Invert ? !result : result;
                return result;
            });
        }
        private new T[] GetComponents<T>()
        {
            var from = _nodeSetterContainer ?_nodeSetterContainer : gameObject;
            return from.GetComponentsInChildren<T>();
        }
        
    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(_isParent) return;
            Gizmos.color = Color.green;
            var bounds = GridBounds;
            var area = new Vector2[]
            {
                bounds.max,                    // topRight
                bounds.max.SetX(bounds.min.x), // topLeft
                bounds.min,                    // buttomleft
                bounds.min.SetX(bounds.max.x)  // buttomRight
            };
            for (var i = 0; i < area.Length; i++)
            {
                var corner = area[i];
                var cornerNext = i + 1 < area.Length? area[i + 1] : area[0];
                Gizmos.DrawLine(corner,cornerNext);
            }
            
            ConnectionGraphDebug(GenerateGrid());
        }
        private void OnValidate()
        {
            _size.x = Mathf.Max(_size.x, 0.1f);
            _size.y = Mathf.Max(_size.y, 0.1f);
        }
        private void ConnectionGraphDebug(IEnumerable<Vector3?> nodes)
        {
            var index = 0;
            var countX = ScaledSize.x;
            var countY = ScaledSize.y;
            var points = nodes.ToArray();
            var joints = new List<NodeJointSetter>(GetComponents<NodeJointSetter>());
            var connectors = GetComponents<NodeConnectorSetter>();
            // Это значение регулирует кол-во соединений.
            // [1] - соединения будут 1:8  кол-во точек 8
            // [2] - соединения будут 1:16 кол-во точек 24
            // [3] - соединения будут 1:24 кол-во точек 48
            const int rowOffset = 1;

            // проверяет не вышел индекс за пределы границ
            bool IsNotOutOfRange(int currentIndex)
            {
                return currentIndex < points.Length && currentIndex >= 0;
            }
            
            // Cоздает ряд индексов используя сдвиг от текущей позиции
            // Заменяет индексы которые вышли за пределы сетки на -1
            IEnumerable<int> GetRowIndices(int centerIndex, int posX)
            {
                // Проверка на допустимые сдвиги в ряду
                var isOutOfRangeLeft  = (posX - rowOffset) < 0;
                var isOutOfRangeRight = (posX + rowOffset) >= countX;
                
                // Сколько точек вышло за границу сетки
                var overflowLeft = isOutOfRangeLeft ? Mathf.Abs(posX - rowOffset) : 0;
                var overflowRight = isOutOfRangeRight ? Mathf.Abs(countX-1 - (posX + rowOffset)) : 0;
                var row = new List<int>();
                
                // Создает ряд с заменой индексов которые вышли за пределы сетки на -1
                void CreateRow(int overflow, bool isForward)
                {
                    var position = 0;
                    while (position < rowOffset)
                    {
                        var currIndex = -1;
                        if (overflow <= 0)
                        {
                            currIndex = centerIndex + (isForward ? (position + 1) : (-rowOffset + position));
                        }
                        row.Add(currIndex); 
                        position++;
                        overflow--;
                    }
                }

                CreateRow(overflowLeft, false);
                row.Add(centerIndex);
                CreateRow(overflowRight, true);
                return row;
            }
            
            // Создает соединение между точкой и другими соседними точками
            // Добавляет коннектор в край обрезанной сетки
            void OneWayConnection(IEnumerable<int> boxRowIndices, Vector3 center, int posX, int posY)
            {
                var isConnector = false;
                foreach (var pointIndex in boxRowIndices)
                {
                    var point = IsNotOutOfRange(pointIndex) ? points[pointIndex] : null;
                    if(!point.HasValue)
                    {
                        isConnector = true;
                        continue;
                    }
                    
                    if(point.Value == center) continue;
                    Gizmos.color = _gridColor;
                    Gizmos.DrawLine(center,point.Value);
                    
                    // center.AddConnection(point, 1);
                }

                var modifiedToConnector = IsModifiedNode(center);
                var insideConnector = isConnector && _useInnerEdgeAsConnectors && (posX < countX-1 && posX > 0) && (posY < countY-1 && posY > 0);
                var outSideConnector = isConnector && _useOuterEdgeAsConnectors && ((posX == countX-1 || posX == 0) || (posY == countY-1 || posY == 0));
                if (insideConnector || outSideConnector || modifiedToConnector)
                {
                    if (_useEdgeJoints && !modifiedToConnector)
                    {
                        Handles.color = Color.yellow;
                        Handles.Disc(Quaternion.identity, center, Vector3.forward, Mathf.Abs(_edgeJointsRadius), false, 0);
                    }

                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(center, 0.1f);
                }
                else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(center, 0.1f);
                }
            }

            bool IsModifiedNode(Vector3 node)
            {
                return joints.Any(x => x.Contains(node)) || connectors.Any(x => x.Contains(node));
            }

            // Перебирает точки сетки и создает для каждой точки квадрат индексов
            // Каждый квадрат выполняет односторонюю связь центральной точки к остальным 1:8
            for (var y = 0; y < countY; y++)
            {
                for (var x = 0; x < countX; x++)
                {
                    var center = points[index];
                    if (center.HasValue)
                    {
                        var boxRowIndices = new List<int>();
                        boxRowIndices.AddRange(GetRowIndices(index - countX, x));
                        boxRowIndices.AddRange(GetRowIndices(index, x));
                        boxRowIndices.AddRange(GetRowIndices(index + countX, x));
                        OneWayConnection(boxRowIndices.ToArray(), center.Value, x, y);
                    }
                    index++;
                }
            }
        }

        [ExposeMethodInEditor]
        private void CreatePolyModifier()
        {
            CreateModifier(typeof(PolygoneModifier));
        }
        [ExposeMethodInEditor]
        private void CreateBoxModifier()
        {
            CreateModifier(typeof(BoxModifier));
        }

        private void CreateModifier(Type component)
        {
            var boxObj = new GameObject(component.Name, component);
            boxObj.transform.SetParent(transform);
            boxObj.transform.localPosition = Vector3.zero;
        }
    #endif
    }
}