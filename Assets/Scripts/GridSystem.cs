using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class GridSystem : MonoBehaviour
{
    [SerializeField, Range(2, 10)] private int cols = 10;
    [SerializeField, Range(2, 10)] private int rows = 10;
    [SerializeField, Range(0.2f, 5f)] private float rowSpacing = 1.1f;
    [SerializeField, Range(0.2f, 5f)] private float colSpacing = 1.1f;


    [Header("References")] [SerializeField]
    private GridCell gridCellPrefab;

    [SerializeField] private CellData_SO cellDataSo;
    private ObjectPool<GridCell> _objectPool;
    private GridCell[,] _gridCells;
    private bool _isClickable = true;
    private void Awake()
    {
        _objectPool = new ObjectPool<GridCell>(createFunc: CreatePooledCell, actionOnGet: OnGetPool,
            actionOnRelease: OnReturnPool, actionOnDestroy: OnDestroyOnPool, collectionCheck: true,
            defaultCapacity: 10, maxSize: 100);
    }

    private void Start()
    {
        CreateGrid();
    }

    private void OnClickedCell(GridCell cell)
    {
        if (!_isClickable) return;
        List<GridCell> matches = FindNeighbors(cell.GridPosition.x, cell.GridPosition.y);
        if (matches.Count > 1)
        {
            _isClickable = false;
            foreach (var match in matches)
            {
                Vector2Int position = match.GridPosition;
                _gridCells[position.x, position.y] = null;
                _objectPool.Release(match);  
            }
            ApplyGravity();
            AllFindNeighbors();
            return;
        }
        cell.transform.DOKill(true);
        cell.transform.DOShakeRotation(0.2f, new Vector3(0, 0, 10), 10, .1f, false);
    }
   
    private Sequence _gravitySeq;
    private void ApplyGravity()
    {
        _gravitySeq = DOTween.Sequence();
        for (int x = 0; x < cols; x++)
        {
            List<GridCell> validCells = new List<GridCell>();
            for (int y = 0; y < rows; y++)
            {
               GridCell gridCell = _gridCells[x, y];
               if (gridCell)
                   validCells.Add(gridCell);
            }
            int currentY = 0;
            foreach (var cell in validCells)
            {
                Vector2 targetPosition = Vector2.zero + new Vector2(x*colSpacing ,currentY*rowSpacing);
                _gridCells[x, currentY] = cell;
                cell.UpdateGridPosition(x,currentY);
                cell.SpriteRenderer.sortingOrder = currentY;
                cell.name = $"GridCell_{x}_{currentY}"; 
                _gravitySeq.Join(cell.transform.DOMove(targetPosition, .5f).SetEase(Ease.OutBounce));
                currentY++;
            }
            for (int y = currentY; y < rows; y++)
            {
                SpawnNewCells(x,y);
            }
        }

        _gravitySeq.OnComplete(()=> _isClickable = true);
    }

    private void SpawnNewCells(int x, int y)
    {
        GridCellType cellType = (GridCellType)Random.Range(0, Enum.GetValues(typeof(GridCellType)).Length);
        Vector2 spawnPosition = Vector2.zero + new Vector2(x*colSpacing, y * rowSpacing +3f);
        Vector2 targetPosition = Vector2.zero + new Vector2(x*colSpacing, y*rowSpacing);
        GridCell newCell = _objectPool.Get();
        _gridCells[x, y] = newCell;
        newCell.Initialize(cellType,cellDataSo.GetType(cellType),new Vector2Int(x,y),OnClickedCell);
        newCell.transform.position = spawnPosition;
        newCell.SpriteRenderer.sortingOrder = y;
        newCell.name = $"NewGridCell_{x}_{y}";
        _gravitySeq.Join(newCell.transform.DOMove(targetPosition,0.8f).SetEase(Ease.OutBounce));
    }
    #region GridSystem

    [Button]
    private void CreateGrid()
    {
        ClearGrid();
        _gridCells = new GridCell[cols, rows];
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GridCell cell = _objectPool.Get();
                GridCellType type = (GridCellType)Random.Range(0, Enum.GetValues(typeof(GridCellType)).Length);
                CellInitialize(x, y, cell, type);
            }
        }
        AllFindNeighbors();
        CenterCameraView();
    }

    private void CellInitialize(int x, int y, GridCell cell, GridCellType type)
    {
        cell.name = $"GridCell_{x}_{y}";
        cell.SpriteRenderer.sortingOrder = y;
        cell.transform.position = Vector2.zero + new Vector2(x * colSpacing, y * rowSpacing);
        _gridCells[x, y] = cell;
        cell.Initialize(type, cellDataSo.GetType(type), new Vector2Int(x, y), OnClickedCell);
    }

    [Button]
    private void ClearGrid()
    {
        if (_gridCells == null) return;
        foreach (var cell in _gridCells)
            if (cell)
                _objectPool.Release(cell);
        _gridCells = null;
    }

    private void CenterCameraView()
    {
        Camera mainCamera = Camera.main;
        Vector2 center = GetCenterCell();
        if (mainCamera) mainCamera.transform.position = new Vector3(center.x, center.y, -10);
    }

    private Vector2 GetCenterCell()
    {
        float height = (rows - 1) * rowSpacing;
        float width = (cols - 1) * colSpacing;
        Vector2 centerPos = Vector2.zero + new Vector2(width / 2, height / 2);
        return centerPos;
    }

    #endregion
    #region FindNeighborsSystem
    private readonly Vector2Int[] _cellOffset = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private void AllFindNeighbors()
    {
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                FindNeighbors(x,y);
            }
        }
    }
    private List<GridCell> FindNeighbors(int x, int y)
    {
        if (x < 0 || x >= cols || y < 0 || y >= rows) return null;
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
                if (newX < 0 || newX >= cols || newY < 0 || newY >= rows) continue;
                GridCell neighborCell = _gridCells[newX, newY];
                if (neighborCell == null) continue;
                if (visited.Contains(neighborCell)) continue;
                if (neighborCell.CellType != targetType) continue;
                visited.Add(neighborCell);
                matches.Add(neighborCell);
                toCheck.Enqueue(new Vector2Int(newX, newY));
            }
        }   
        foreach (GridCell cell in matches)
            cell.UpdateVisual(matches.Count, cellDataSo.GetType(cell.CellType));
        return matches;
    }
    #endregion
    #region PooledObjects

    private GridCell CreatePooledCell()
    {
        GridCell cell = Instantiate(gridCellPrefab, transform);
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
        Destroy(gridCell.gameObject);
    }

    #endregion
}