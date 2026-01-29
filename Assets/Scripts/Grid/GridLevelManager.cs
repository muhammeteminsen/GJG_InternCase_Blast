using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Grid
{
    public class GridLevelManager
    {
        public ObjectPool<GridCell> ObjectPool;
        private readonly LevelDataSo _levelDataSo;
        private readonly CellDataSo _cellDataSo;
        private Transform _gridParent;
        private GridCell[,] _gridCells;
        private readonly GridCell _gridCellPrefab;
        private readonly int _cols, _rows;
        private readonly float _spacing;
        private readonly Action<GridCell> _onClickCallback;

        public GridLevelManager(GridCell[,] gridCells, int cols, int rows, float spacing, CellDataSo cellDataSo,
            LevelDataSo levelDataSo, GridCell gridCellPrefab, Action<GridCell> onClickCallback)
        {
            _gridCells = gridCells;
            _cols = cols;
            _rows = rows;
            _spacing = spacing;
            _levelDataSo = levelDataSo;
            _cellDataSo = cellDataSo;
            _onClickCallback = onClickCallback;
            _gridCellPrefab = gridCellPrefab;
            PoolInitialize();
        }

        #region LevelSytem

#if UNITY_EDITOR
        public void SaveLevel()
        {
            if (!_levelDataSo || _gridCells == null) return;
            _levelDataSo.cellData.Clear();
            _levelDataSo.cols = _cols;
            _levelDataSo.rows = _rows;
            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    if (_gridCells[x, y])
                    {
                        _levelDataSo.cellData.Add(new CellData
                        {
                            x = x,
                            y = y,
                            type = _gridCells[x, y].CellType
                        });
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(_levelDataSo);
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log($"Level saved: {_levelDataSo.cellData.Count} cells");
        }
#endif
        public GridCell[,] LoadLevel()
        {
            ClearGrid();
            PoolInitialize();
            _gridCells = new GridCell[_levelDataSo.cols, _levelDataSo.rows];
            foreach (var data in _levelDataSo.cellData)
            {
                GridCell cell = ObjectPool.Get();
                if (cell)
                    CellInitialize(data.x, data.y, cell, data.type);
            }

            return _gridCells;
        }

        #endregion

        #region GridSystem
        public GridCell[,] CreateGrid()
        {
            ClearGrid();
            PoolInitialize();
            _gridCells = new GridCell[_cols, _rows];
            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    GridCell cell = ObjectPool?.Get();
                    GridCellType type = (GridCellType)Random.Range(0, _cellDataSo.cellDataList.Count);
                    CellInitialize(x, y, cell, type);
                }
            }

            return _gridCells;
        }


        private void CellInitialize(int x, int y, GridCell cell, GridCellType type)
        {
            cell.name = $"GridCell_{x}_{y}";
            cell.SpriteRenderer.sortingOrder = y;
            cell.transform.position = Vector2.zero + new Vector2(x * _spacing, y * _spacing);
            _gridCells[x, y] = cell;
            cell.Initialize(type, _cellDataSo.GetType(type), new Vector2Int(x, y), _onClickCallback);
        }

        public void ClearGrid()
        {
            if (!_gridParent)
            {
                GameObject existingParent = GameObject.Find("GridParent");
                _gridParent = existingParent ? existingParent.transform : new GameObject("GridParent").transform;
            }

            if (_gridCells != null && ObjectPool != null)
            {
                foreach (var cell in _gridCells)
                {
                    if (cell && cell.gameObject.activeSelf)
                    {
                        try
                        {
                            ObjectPool.Release(cell);
                        }
                        catch
                        {
                            Object.DestroyImmediate(cell.gameObject);
                        }
                    }
                }

                _gridCells = null;
            }

            if (_gridParent)
            {
                for (int i = _gridParent.childCount - 1; i >= 0; i--)
                {
                    GameObject child = _gridParent.GetChild(i).gameObject;
#if UNITY_EDITOR
                    Object.DestroyImmediate(child);
#else
                Object.Destroy(child);
#endif
                }
            }

            ObjectPool?.Clear();
        }

        #endregion

        #region PooledObjects

        private GridCell CreatePooledCell()
        {
            GridCell cell = Object.Instantiate(_gridCellPrefab, _gridParent ? _gridParent.transform : null);
            cell.gameObject.SetActive(false);
            return cell;
        }

        private void OnGetPool(GridCell gridCell)
        {
            gridCell.gameObject.SetActive(true);
        }

        private void OnReturnPool(GridCell gridCell)
        {
            gridCell.gameObject.SetActive(false);
        }

        private void OnDestroyOnPool(GridCell gridCell)
        {
            if (!gridCell) return;
            Object.Destroy(gridCell.gameObject);
        }

        private void PoolInitialize()
        {
            ObjectPool ??= new ObjectPool<GridCell>(
                createFunc: CreatePooledCell,
                actionOnGet: OnGetPool,
                actionOnRelease: OnReturnPool,
                actionOnDestroy: OnDestroyOnPool,
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 100
            );
        }

        #endregion
    }
}