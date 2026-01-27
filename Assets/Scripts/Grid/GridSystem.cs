using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Grid
{
    public class GridSystem : MonoBehaviour
    {
        [Header("Grid Value")] 
        [SerializeField,Range(2, 10)] public int cols = 10;
        [SerializeField,Range(2, 10)] public int rows = 10;
        [SerializeField,Range(0.2f, 5f)] public float spacing = 1.1f;

        [Header("References")] 
        [SerializeField] private GridCell gridCellPrefab;
        [SerializeField] private CellDataSo cellDataSo;
        [SerializeField] private LevelDataSo levelDataSo;
        [SerializeField] private bool useRandomizeCell;

        private GridCell[,] GridCells { get; set; }
        private bool _isClickable = true;
        private Transform _gridParent;
        private Camera Camera => Camera.main;
   
        private GridLevelManager _levelManager;
        private GridLevelManager LevelManager
        {
            get
            {
                if (_levelManager == null)
                {
                    InitializeReferences();
                }
                return _levelManager;
            }
        }
        private GridMatchFinder _matchFinder;
        private GridAnimator _gridAnimator;

        private void Awake()
        {
            GridCells = new GridCell[cols, rows];
        }
        private void InitializeReferences()
        {
            if (_levelManager != null) return;
            _levelManager = new GridLevelManager(GridCells, cols, rows, spacing, cellDataSo, levelDataSo, gridCellPrefab,
                OnClickedCell);
       
        }

        private void Start()
        {
            if (!useRandomizeCell && levelDataSo && levelDataSo.cellData.Count > 0)
                LoadLevel();
            else
                CreateGrid();
        
        }

        private void OnClickedCell(GridCell cell)
        {
            if (!_isClickable) return;
            List<GridCell> matches = _matchFinder.FindNeighbors(cell.GridPosition.x, cell.GridPosition.y);
            if (matches.Count > 1)
            {
                SetClickable(false);
                foreach (var match in matches)
                {
                    Vector2Int position = match.GridPosition;
                    GridCells[position.x, position.y] = null;
                    _levelManager.ObjectPool.Release(match);
                }

                _gridAnimator.ApplyGravity(gravityComplete:() =>
                {
                    _matchFinder.AllFindNeighbors();
                    if (!_matchFinder.HasAnyMatch())
                    {
                        _matchFinder.HandleShuffle(SetClickable);
                        return;
                    }

                    SetClickable(true);
                });
                return;
            }

            cell.transform.DOKill(true);
            cell.transform.DOShakeRotation(0.2f, new Vector3(0, 0, 10), 10, .1f, false);
        }

        [Button]
        private void CreateGrid()
        {
            DOTween.KillAll();
            GridCells = LevelManager.CreateGrid();
            SetupLevel();
        }

        private void LoadLevel()
        {
            cols = levelDataSo.cols;
            rows = levelDataSo.rows;
            DOTween.KillAll();
            GridCells = LevelManager.LoadLevel();
            SetupLevel();
        }

        private void SetupLevel()
        {
            CenterCameraView();
            _matchFinder = new GridMatchFinder(GridCells, cols, rows, cellDataSo, OnClickedCell);
            _gridAnimator = new GridAnimator(GridCells, cols, rows, spacing, cellDataSo, OnClickedCell,
                LevelManager.ObjectPool);
            _matchFinder.AllFindNeighbors();
            if (!_matchFinder.HasAnyMatch())
                _matchFinder.HandleShuffle(SetClickable);
        }
        [Button]
        public void ClearGrid()
        {
            LevelManager.ClearGrid();
        }

        [Button]
        public void SaveLevel()
        {
            LevelManager.SaveLevel();
        }

        private void SetClickable(bool state)
        {
            _isClickable = state;
        }

        [Button]
        private void CenterCameraView()
        {
            Vector2 pos = GetCenterCell();
            Camera.transform.position = new Vector3(pos.x,pos.y, -10);
        }
    
        private Vector2 GetCenterCell()
        {
            float height = (rows - 1) * spacing;
            float width = (cols - 1) * spacing;
            Vector2 centerPos = Vector2.zero + new Vector2(width / 2, height / 2);
            return centerPos;
        }
    }
}