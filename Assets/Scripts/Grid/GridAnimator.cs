using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Grid
{
    public class GridAnimator
    {
        private Sequence _gravitySeq;
        private readonly GridCell[,] _gridCells;
        private readonly int _cols, _rows;
        private readonly float _spacing;
        private readonly Action<GridCell> _onClickedCell;
        private readonly ObjectPool<GridCell> _objectPool;
        private readonly CellDataSo _cellDataSo;

        public GridAnimator(GridCell[,] gridCells,int cols, int rows, float spacing,CellDataSo cellDataSo,Action<GridCell> onClickedCell, ObjectPool<GridCell> objectPool)
        {
            _gridCells = gridCells;
            _cols = cols;
            _rows = rows;
            _spacing = spacing;
            _onClickedCell = onClickedCell;
            _objectPool = objectPool;
            _cellDataSo = cellDataSo;
        }

        public void ApplyGravity(Action gravityComplete)
        {
            _gravitySeq = DOTween.Sequence();
            for (int x = 0; x < _cols; x++)
            {
                List<GridCell> validCells = new List<GridCell>();
                for (int y = 0; y < _rows; y++)
                {
                    GridCell gridCell = _gridCells[x, y];
                    if (gridCell)
                        validCells.Add(gridCell);
                }

                int currentY = 0;
                foreach (var cell in validCells)
                {
                    Vector2 targetPosition = Vector2.zero + new Vector2(x * _spacing, currentY * _spacing);
                    _gridCells[x, currentY] = cell;
                    cell.UpdateGridPosition(x, currentY);
                    cell.SpriteRenderer.sortingOrder = currentY;
                    cell.name = $"GridCell_{x}_{currentY}";
                    _gravitySeq.Join(cell.transform.DOMove(targetPosition, .5f).SetEase(Ease.OutBounce));
                    currentY++;
                }

                for (int y = currentY; y < _rows; y++)
                {
                    SpawnNewCells(x, y);
                }
            }

            _gravitySeq.OnComplete(gravityComplete.Invoke);
        }

        private void SpawnNewCells(int x, int y)
        {
            GridCellType cellType = (GridCellType)Random.Range(0, Enum.GetValues(typeof(GridCellType)).Length);
            Vector2 spawnPosition = Vector2.zero + new Vector2(x * _spacing, y * _spacing + 5f);
            Vector2 targetPosition = Vector2.zero + new Vector2(x * _spacing, y * _spacing);
            GridCell newCell = _objectPool.Get();
            _gridCells[x, y] = newCell;
            newCell.Initialize(cellType, _cellDataSo.GetType(cellType), new Vector2Int(x, y), _onClickedCell);
            newCell.transform.position = spawnPosition;
            newCell.SpriteRenderer.sortingOrder = y;
            newCell.name = $"NewGridCell_{x}_{y}";
            _gravitySeq.Join(newCell.transform.DOMove(targetPosition, 0.8f).SetEase(Ease.OutBounce));
        }
    }
}