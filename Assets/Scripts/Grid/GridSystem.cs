using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Grid
{
    public class GridSystem : MonoBehaviour
    {
        [Header("Grid Value")] [SerializeField, Range(2, 10)]
        public int cols = 10;

        [SerializeField, Range(2, 10)] public int rows = 10;
        [SerializeField, Range(0.2f, 5f)] public float spacing = 1.1f;

        [Header("References")] [SerializeField]
        private GridCell gridCellPrefab;

        [SerializeField] private bool useRandomizeCell;

        private GridCell[,] GridCells { get; set; }
        private bool _isClickable = true;
        private Transform _gridParent;
        private Camera Camera => Camera.main;
        private GridLevelManager _levelManager;

        private GameManager _gameManager;
        public CellDataSo cellDataSo;
        public LevelDataSo levelDataSo;

        private GridLevelManager LevelManager => _levelManager ??= new GridLevelManager(GridCells, cols, rows, spacing,
            cellDataSo, levelDataSo, gridCellPrefab,
            OnClickedCell);

        private GridMatchFinder _matchFinder;
        private GridAnimator _gridAnimator;

        private void Awake()
        {
            GridCells = new GridCell[cols, rows];
            _gameManager = GetComponent<GameManager>();
        }

        private void Start()
        {
            if (!useRandomizeCell && levelDataSo && levelDataSo.cellData.Count > 0)
                LoadLevel();
            else
                CreateGrid();
            _gameManager.InitializeGame(levelDataSo.initialMoves, 0, levelDataSo);
        }

        private void OnClickedCell(GridCell cell)
        {
            if (!_isClickable) return;
            List<GridCell> matches = _matchFinder.FindNeighbors(cell.GridPosition.x, cell.GridPosition.y);
            if (matches.Count >= cellDataSo.minMatchableCells)
            {
                SetClickable(false);
                _gameManager.ApplyMoves();

                if (matches.Count >= cellDataSo.iconAMatchCount)
                {
                    Sequence clickedSeq = DOTween.Sequence();
                    for (var i = 0; i < matches.Count; i++)
                    {
                        GridCell match = matches[i];
                        if (cell == match) continue;
                        match.SpriteRenderer.sortingOrder = cell.SpriteRenderer.sortingOrder + i;
                        clickedSeq.Insert(i * 0.1f,
                            match.transform.DOMove(cell.transform.position, 0.5f).SetEase(Ease.InOutBack));
                    }

                    clickedSeq.OnComplete(() =>
                    {
                        _gameManager.ApplyFeedback(FeedbackType.BigMatch);
                        ExecuteMatchLogic();
                    });
                }
                else
                {
                    _gameManager.ApplyFeedback(FeedbackType.Matchable);
                    ExecuteMatchLogic();
                }

                return;

                void ExecuteMatchLogic()
                {
                    ApplyGravity(matches);
                    cell.UpdateScore(cellDataSo, _gameManager);
                }
            }

            SetClickable(false);
            _gameManager.ApplyFeedback(FeedbackType.NonMatchable);
            cell.transform.DOKill(true);
            cell.transform.DOShakeRotation(1f, new Vector3(0, 0, 10), 10, 1, false, ShakeRandomnessMode.Harmonic)
                .OnComplete(() => SetClickable(true));
        }

        private void ApplyGravity(List<GridCell> matches)
        {
            _gameManager.ApplyFeedback(FeedbackType.GravityActive);
            foreach (var match in matches)
            {
                Vector2Int position = match.GridPosition;
                GridCells[position.x, position.y] = null;
                LevelManager.ObjectPool.Release(match);
            }

            _gridAnimator.GravityAnimation(gravityComplete: () =>
            {
                _matchFinder.AllFindNeighbors();
                _gameManager.ApplyFinishGame(isGameFinished=>
                {
                    if (isGameFinished)
                    {
                        _gameManager.ApplyFeedback(FeedbackType.FinishGame);
                        SetClickable(false);
                        return;
                    }
                    if (!_matchFinder.HasAnyMatch())
                    {
                        _matchFinder.HandleShuffle(SetClickable);
                        _gameManager.ApplyFeedback(FeedbackType.Shuffle);
                        return;
                    }
                    SetClickable(true);
                });
                
            }, cellFallComplete: () => { _gameManager.ApplyFeedback(FeedbackType.GravityComplete); });
        }

        [Button]
        private void CreateGrid()
        {
            _levelManager = null;
            DOTween.KillAll();
            GridCells = LevelManager.CreateGrid();
            SetupLevel();
        }

        private void LoadLevel()
        {
            _levelManager = null;
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
#if UNITY_EDITOR
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
#endif
        [Button]
        private void CenterCameraView()
        {
            Vector2 pos = GetCenterCell();
            Camera.transform.position = new Vector3(pos.x, pos.y, -10);
        }

        private void SetClickable(bool state)
        {
            _isClickable = state;
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