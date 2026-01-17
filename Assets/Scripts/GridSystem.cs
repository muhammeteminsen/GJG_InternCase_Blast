using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")] [SerializeField, Range(2f, 10f)]
    private int rows = 10;

    [SerializeField, Range(2f, 10f)] private int cols = 10;
    [SerializeField, Min(0f)] private float rowSpacing = 1f;
    [SerializeField, Min(0f)] private float colSpacing = 1f;

    [Header("References")] [SerializeField]
    private Transform originPosition;

    [SerializeField] private GridCell gridCellPrefab;
    [SerializeField] private CellData_SO cellDataSo;

    [Header("Debug Settings")] [SerializeField]
    private bool showDebugLabels = true;
    private Camera MainCamera => Camera.main;
    private GridCell[,] _gridCells;
    private bool _isClicked;
    
    private void Start()
    {
        CreateGrid();
    }


    private void OnCellClicked(GridCell clickedCell)
    {
        if (_isClicked) return;
        List<GridCell> matchedCells = new List<GridCell>();
        HashSet<GridCell> visitedCells = new HashSet<GridCell>();
        FindNeighbors(clickedCell.GridPosition.x, clickedCell.GridPosition.y,visitedCells,matchedCells, clickedCell.CellType);
        if (matchedCells.Count > 1)
        {
            _isClicked = true;
            foreach (var cell in matchedCells)
            {
                _gridCells[cell.GridPosition.x, cell.GridPosition.y] = null;
                Destroy(cell.gameObject);
            }
            ApplyGravity();
            UpdateAllVisuals();
        }
    }

    private void UpdateAllVisuals()
    {
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GridCell cell = _gridCells[x, y];
                if (!cell) continue;
                HashSet<GridCell> visited = new HashSet<GridCell>();
                List<GridCell> matches = new List<GridCell>();
                FindNeighbors(x, y, visited, matches, cell.CellType);
                cell.UpdateVisual(matches.Count, cellDataSo.GetType(cell.CellType));
            }
        }
    }
    [Button]
    private void CreateGrid()
    {
        ClearGrid();
        _gridCells = new GridCell[cols, rows];
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GridCell newGridCell = Instantiate(gridCellPrefab, originPosition);
                GridCellType type = (GridCellType)Random.Range(0, Enum.GetValues(typeof(GridCellType)).Length);
                newGridCell.Initialize(type, cellDataSo.GetType(type), new Vector2Int(x, y), OnCellClicked);
                newGridCell.transform.position =
                    new Vector3(x * colSpacing, y * rowSpacing, 0) + originPosition.position;
                newGridCell.name = "GridCell_x_" + x + "_y_" + y;
                _gridCells[x, y] = newGridCell;
            }
        }
        UpdateAllVisuals();
        CenterCameraView();
    }

    [Button]
    private void ClearGrid()
    {
        if (!originPosition) return;
        for (int i = originPosition.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(originPosition.GetChild(i).gameObject);
            else
                DestroyImmediate(originPosition.GetChild(i).gameObject);
            
        }
    }

    [Button]
    private void CenterCameraView()
    {
        Vector3 centerPos = CenterGridCell();
        if (IsCenterable())
            MainCamera.transform.position = new Vector3(centerPos.x, centerPos.y, MainCamera.transform.position.z);
    }

    private bool IsCenterable()
    {
        Vector3 centerPos = CenterGridCell();
        return !float.IsNaN(centerPos.x) && !float.IsNaN(centerPos.y) && !float.IsNaN(centerPos.z);
    }

    private Vector3 CenterGridCell()
    {
        float width = (cols - 1) * colSpacing;
        float height = (rows - 1) * rowSpacing;
        Vector3 centerPos = originPosition.position + new Vector3(width / 2f, height / 2f, 0);
        return centerPos;
    }

    private readonly Vector2Int[] _neighborOffsets = new Vector2Int[]
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1)
    };

    private void FindNeighbors(int x, int y, HashSet<GridCell> visited,List<GridCell> matches, GridCellType targetType)
    {
        if (x < 0 || x >= cols || y < 0 || y >= rows) return;
        GridCell cell = _gridCells[x, y];
        if (visited.Contains(cell)) return;
        if (cell.CellType != targetType) return;
        visited.Add(cell);
        matches.Add(cell);
        foreach (var offset in _neighborOffsets)
            FindNeighbors(x + offset.x, y + offset.y, visited,matches, targetType);
    }

    private void ApplyGravity()
    {
        Sequence mainSequence = DOTween.Sequence();
        for (int x = 0; x < cols; x++)
        {
            List<GridCell> validCells = new List<GridCell>();
            for (int y = 0; y < rows; y++)
            {
                GridCell cell = _gridCells[x, y];
                if (cell)
                    validCells.Add(cell);
            }

            int currentY = 0;
            foreach (var cell in validCells)
            {
                Vector3 targetPos = originPosition.position + new Vector3(x * colSpacing, currentY * rowSpacing, 0);
                mainSequence.Join(cell.transform.DOMove(targetPos, 1f).SetEase(Ease.OutBounce));
                _gridCells[x, currentY] = cell;
                cell.UpdateGridPosition(x, currentY, cellDataSo.GetType(cell.CellType));
                currentY++;
            }

            for (int y = currentY; y < rows; y++)
            {
                SpawnNewCells(x, y, mainSequence);
            }
        }
    }

    private void SpawnNewCells(int x, int y, Sequence sequence)
    {
        Vector3 spawnPos = originPosition.position + new Vector3(x * colSpacing, y * rowSpacing + 3f, 0);
        Vector3 targetPos = originPosition.position + new Vector3(x * colSpacing, y * rowSpacing, 0);
        GridCell newGridCell = Instantiate(gridCellPrefab, spawnPos, Quaternion.identity, originPosition);
        GridCellType type = (GridCellType)Random.Range(0, Enum.GetValues(typeof(GridCellType)).Length);
        newGridCell.Initialize(type, cellDataSo.GetType(type), new Vector2Int(x, y), OnCellClicked);
        sequence.Join(newGridCell.transform.DOMove(targetPos, 1f).SetEase(Ease.OutBounce));
        sequence.OnComplete(() =>
        {
         _isClicked = false;
        });
        newGridCell.name = "GridCell_x_" + x + "_y_" + y;
        _gridCells[x, y] = newGridCell;
        
    }
    
    private void OnGUI()
    {
        if (!showDebugLabels || !MainCamera || !IsCenterable()) return;

        Vector3 startPos = originPosition ? originPosition.position : transform.position;
        GUIStyle style = new GUIStyle
        {
            normal =
            {
                textColor = Color.black
            },
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };

        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3 worldPos = new Vector3(i * colSpacing, j * rowSpacing, 0) + startPos;
                Vector3 screenPos = MainCamera.WorldToScreenPoint(worldPos);
                if (screenPos.z < 0) continue;
                float labelY = Screen.height - screenPos.y;
                GUI.Label(new Rect(screenPos.x, labelY, 100, 20), $"{i},{j}", style);
            }
        }
    }
}