using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class CellsArea : MonoBehaviour
    {
        public Vector2 AreaSize { get; private set; } 
        public Vector2 Center { get; private set; }
        public Vector2 Start { get; private set; }
        public Vector2Int CellCount { get; private set; }
        public Vector2 CellSize { get; private set; }

        private Vector2Int[] _expandPattern;
        [SerializeField] private int _countX = 5;
        [SerializeField] private int _countY = 5;

        // optimize calc once
        private void Awake()
        {
            AreaSize = transform.localScale;
            Center = transform.position;
            CellCount = new Vector2Int(_countX, _countY);
            CellSize = AreaSize / CellCount;
            Start = new Vector2(Center.x - (AreaSize.x / 2f), Center.y + (AreaSize.y / 2f));
            _expandPattern = new []
            {
                // diagonal
                Vector2Int.one,
                Vector2Int.one * -1,

                // inverse diagonal
                Vector2Int.right + Vector2Int.down,
                Vector2Int.left + Vector2Int.up,

                // down - up
                Vector2Int.up,
                Vector2Int.down,

                // right - left
                Vector2Int.right,
                Vector2Int.left,

                // center from query
            };
        }
        
        public Vector2Int? GetCellIndex(Vector2 worldSpacePoint)
        {
            var cellIndex = CalcCellIndex(worldSpacePoint);
            return ValidateCellIndex(cellIndex) ? cellIndex : default;
        }

        public IEnumerable<Vector2Int> GetCellIndices(Vector2 worldSpacePoint, float atRadius)
        {
            return Expand(CalcCellIndex(worldSpacePoint));
        }

        public IEnumerable<Vector2Int> Expand(Vector2Int cellIndex, bool includeCenter = true)
        {
            if (includeCenter && ValidateCellIndex(cellIndex))
            {
                yield return cellIndex;
            }
            
            foreach (var pattern in _expandPattern)
            {
                var validate = pattern + cellIndex;
                if (ValidateCellIndex(validate))
                {
                    yield return validate;
                }
            }
        }

        public IEnumerable<Vector2Int> GetCellIndicesExpandedNearestHorizontal(Vector2 worldSpacePoint)
        {
            var cellIndex = CalcCellIndex(worldSpacePoint);
            var centerCell = GetPointAtIndex(cellIndex);
            var offset = CellSize.x / 3;
            if (ValidateCellIndex(cellIndex))
            {
                yield return cellIndex;
            }
            
            if (worldSpacePoint.x > centerCell.x + offset)
            {
                cellIndex.x += 1;
                if (ValidateCellIndex(cellIndex))
                {
                    yield return cellIndex;
                    yield break;
                }
            }
            
            if (worldSpacePoint.x < centerCell.x - offset)
            {
                cellIndex.x -= 1;
                if (ValidateCellIndex(cellIndex))
                {
                    yield return cellIndex;
                }
            }
        }

        public bool ValidateCellIndex(Vector2Int index)
        {
            return index.x >= 0 && index.x < _countX && index.y >= 0 && index.y < _countY;
        }

        private Vector2 GetPointAtIndex(Vector2Int cellIndex)
        {
            return Start + cellIndex * CellSize;
        }
        private Vector2Int CalcCellIndex(Vector2 worldSpace)
        {
            return new Vector2Int(Mathf.FloorToInt(Mathf.Abs(worldSpace.x - Start.x) / CellSize.x), 
                                  Mathf.FloorToInt(Mathf.Abs(worldSpace.y - Start.y) / CellSize.y));
        }

    #region Gizmos
    #if UNITY_EDITOR
        [SerializeField] private bool _drawGizmo = true;
        private static readonly Color _edgeColor = Color.green;
        private void OnDrawGizmos()
        {
            if (!_drawGizmo) return;
            Vector2 areaSize = transform.localScale;
            Vector2 center = transform.position;
            var cellSize = areaSize / new Vector2Int(_countX, _countY);
            var start = new Vector2(center.x - (areaSize.x / 2f), center.y + (areaSize.y / 2f));
            Gizmos.color = _edgeColor;
            for (var y = 0; y < _countY; y++)
            {
                for (var x = 0; x < _countX; x++)
                {
                    var pos = new Vector2(x, -y);
                    var offset = pos * cellSize;
                    var pointXY = start + offset;
                    Gizmos.DrawRay(pointXY, Vector2.right * cellSize.x);
                    Gizmos.DrawRay(pointXY, Vector2.down * cellSize.y);
                }
            }

            Gizmos.DrawRay(new Vector2(start.x, start.y - areaSize.y), Vector2.right * cellSize.x * _countX);
            Gizmos.DrawRay(new Vector2(start.x + areaSize.x, start.y), Vector2.down * cellSize.y * _countY);
        }
    #endif
    #endregion
    }
}