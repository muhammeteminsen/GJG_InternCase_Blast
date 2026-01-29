using System;
using UnityEngine;

namespace Grid
{
    public enum GridCellType
    {
        Blue,
        Green,
        Pink,
        Purple,
        Red,
        Yellow
    }
    public class GridCell : MonoBehaviour
    {
        public GridCellType CellType { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public SpriteRenderer SpriteRenderer => GetComponent<SpriteRenderer>();
        private Action<GridCell> _onClicked;
        private int _matchValue;
        private void OnMouseDown()
        {
            _onClicked?.Invoke(this);
        }

        public void Initialize(GridCellType cellType, CellDataSo.CellVisual cellVisual,Vector2Int position,Action<GridCell> onClicked)
        {
            CellType = cellType;
            SpriteRenderer.sprite = cellVisual.defaultIcon;
            GridPosition = position;
            _onClicked = onClicked;
        }
        public void UpdateGridPosition(int x, int y)
        {
            GridPosition = new Vector2Int(x, y);
        }
    
        public void UpdateVisual(int value, CellDataSo cellDataSo)
        {
            CellDataSo.CellVisual cellVisual = cellDataSo.GetType(CellType);
            if (value >= cellDataSo.iconCMatchCount)
                SpriteRenderer.sprite = cellVisual.iconC;
            else if (value >= cellDataSo.iconBMatchCount)
                SpriteRenderer.sprite = cellVisual.iconB;
            else if (value >= cellDataSo.iconAMatchCount)
                SpriteRenderer.sprite = cellVisual.iconA;
            else
                SpriteRenderer.sprite = cellVisual.defaultIcon;
            _matchValue = value;
        }

        public void UpdateScore(CellDataSo cellDataSo,GameManager gameManager)
        {
            if (_matchValue>= cellDataSo.iconCMatchCount)
                gameManager.ApplyUpdateScore(cellDataSo.iconCScoreMultiplier * _matchValue);
            else if (_matchValue >= cellDataSo.iconBMatchCount)
                gameManager.ApplyUpdateScore(cellDataSo.iconBScoreMultiplier * _matchValue);
            else if (_matchValue >= cellDataSo.iconAMatchCount)
                gameManager.ApplyUpdateScore(cellDataSo.iconAScoreMultiplier* _matchValue);
            else
                gameManager.ApplyUpdateScore(cellDataSo.baseScore);
        }
    }
}