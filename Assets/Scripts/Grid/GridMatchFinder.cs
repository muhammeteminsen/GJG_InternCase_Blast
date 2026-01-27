using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Grid
{
    public class GridMatchFinder
    {
        private readonly GridCell[,] _gridCells;
        private readonly int _cols;
        private readonly int _rows;
        private readonly CellDataSo _cellDataSo;
        private readonly System.Action<GridCell> _onCellClickCallback;
        public GridMatchFinder(GridCell[,] gridCells, int cols, int rows, CellDataSo dataSo, System.Action<GridCell> onClickCallback)
        {
            _gridCells = gridCells; 
            _cols = cols;
            _rows = rows;
            _cellDataSo = dataSo;
            _onCellClickCallback = onClickCallback;
        }
    
        private readonly Vector2Int[] _cellOffset = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        public void AllFindNeighbors()
        {
            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    FindNeighbors(x, y);
                }
            }
        }

        public List<GridCell> FindNeighbors(int x, int y)
        {
            if (x < 0 || x >= _cols || y < 0 || y >= _rows) return null;
            List<GridCell> matches = new List<GridCell>();
            HashSet<GridCell> visited = new HashSet<GridCell>();
            Queue<Vector2Int> toCheck = new Queue<Vector2Int>();
        
            GridCell startCell = _gridCells[x, y];
            GridCellType targetType = startCell.CellType;

            toCheck.Enqueue(new Vector2Int(x, y));
            visited.Add(startCell);
            matches.Add(startCell);

            while (toCheck.Count > 0)
            {
                Vector2Int current = toCheck.Dequeue();
                foreach (var offset in _cellOffset)
                {
                    int newX = current.x + offset.x;
                    int newY = current.y + offset.y;
                    if (newX < 0 || newX >= _cols || newY < 0 || newY >= _rows) continue;
                    GridCell neighborCell = _gridCells[newX, newY];
                    if (!neighborCell) continue;
                    if (visited.Contains(neighborCell)) continue;
                    if (neighborCell.CellType != targetType) continue;
                    visited.Add(neighborCell);
                    matches.Add(neighborCell);
                    toCheck.Enqueue(new Vector2Int(newX, newY));
                }
            }

            foreach (GridCell cell in matches)
                cell.UpdateVisual(matches.Count, _cellDataSo.GetType(cell.CellType));
            return matches;
        }
        #region Shuffle
        public bool HasAnyMatch()
        {
            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    List<GridCell> matches = FindNeighbors(x, y);
                    if (matches.Count > 1)
                        return true;
                }
            }

            return false;
        }

        public void HandleShuffle(System.Action<bool> setClickableCallback)
        {
            List<GridCellType> currentTypes = new List<GridCellType>();
            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    currentTypes.Add(_gridCells[x, y].CellType);
                }
            }

            for (int i = 0; i < currentTypes.Count; i++)
            {
                GridCellType temp = currentTypes[i];
                int randomIndex = Random.Range(0, currentTypes.Count);
                currentTypes[i] = currentTypes[randomIndex];
                currentTypes[randomIndex] = temp;
            }

            Sequence seq = DOTween.Sequence();
            int index = 0;
            setClickableCallback.Invoke(false);
            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    int currentY = y;
                    int currentX = x;
                    GridCell gridCell = _gridCells[x, y];
                    seq.Join(gridCell.transform.DOPunchScale(Vector3.one * 0.3f, 0.1f));
                    seq.AppendCallback(() =>
                    {
                        gridCell.Initialize(currentTypes[index], _cellDataSo.GetType(currentTypes[index]),
                            new Vector2Int(currentX, currentY), _onCellClickCallback);
                        index++;
                    });
                }
            }

            seq.OnComplete(() =>
            {
                if (!HasAnyMatch())
                {
                    ForceMatch(setClickableCallback);
                    return;
                }
                setClickableCallback.Invoke(true);
            });
        }

        private void ForceMatch(System.Action<bool> setClickableCallback)
        {
            if (HasAnyMatch()) return;
            int randomX = Random.Range(0, _cols - 1);
            int randomY = Random.Range(0, _rows);
            Sequence seq = DOTween.Sequence();
            seq.Join(_gridCells[randomX + 1, randomY].transform.DOPunchScale(Vector3.one * 0.2f, .2f));
            seq.AppendCallback(() =>
            {
                GridCellType gridCellType = _gridCells[randomX, randomY].CellType;
                _gridCells[randomX + 1, randomY].Initialize(gridCellType, _cellDataSo.GetType(gridCellType),
                    new Vector2Int(randomX + 1, randomY), _onCellClickCallback);
            });
            seq.OnComplete(() => setClickableCallback.Invoke(true));
        }

        #endregion
    }
}