using System;
using UnityEngine;
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
    private void OnMouseDown()
    {
        _onClicked?.Invoke(this);
    }

    public void Initialize(GridCellType cellType, CellData_SO.CellVisual cellVisual,Vector2Int position,Action<GridCell> onClicked)
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
    
    public void UpdateVisual(int value, CellData_SO.CellVisual cellVisual)
    {
        if (value >= 10)
            SpriteRenderer.sprite = cellVisual.iconC;
        else if (value >= 8)
            SpriteRenderer.sprite = cellVisual.iconB;
        else if (value >= 5)
            SpriteRenderer.sprite = cellVisual.iconA;
        else
            SpriteRenderer.sprite = cellVisual.defaultIcon;
    }
}
