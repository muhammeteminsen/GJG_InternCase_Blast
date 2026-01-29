using System.Collections.Generic;
using Grid;
using UnityEngine;

[System.Serializable]
public struct CellData
{
    public int x;
    public int y;
    public GridCellType type;
}
[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/Create New Level Data")]
public class LevelDataSo : ScriptableObject
{
    public int initialMoves;
    public int cols;
    public int rows;
    public List<CellData> cellData = new List<CellData>();
}
