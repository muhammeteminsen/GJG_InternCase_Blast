using System;
using System.Collections.Generic;
using Grid;
using UnityEngine;


[CreateAssetMenu(fileName = "CellData", menuName = "ScriptableObjects/Create New CellData")]
public class CellDataSo : ScriptableObject
{
    [Serializable]
    public struct CellVisual
    {
        public GridCellType type;
        public Sprite defaultIcon;
        public Sprite iconA;
        public Sprite iconB;
        public Sprite iconC;
    }
    public List<CellVisual> cellDataList;
    [Min(0)]public int minMatchableCells = 2;
    [Min(0)]public int iconAMatchCount = 4;
    [Min(0)]public int iconBMatchCount = 7;
    [Min(0)]public int iconCMatchCount = 10;
    
    public CellVisual GetType(GridCellType type)
    {
        foreach (var cellData in cellDataList)
        {
            if (cellData.type == type)
                return cellData;
        }
        return default;
    }

    private void OnValidate()
    {
        minMatchableCells = Mathf.Min(minMatchableCells, iconAMatchCount);
    }
}
