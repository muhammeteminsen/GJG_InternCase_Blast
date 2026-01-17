using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CellData", menuName = "ScriptableObjects/CellData")]
public class CellData_SO : ScriptableObject
{
    [Serializable]
    public struct CellVisual
    {
        public GridCellType type;
        public Sprite defaultIcon;
        [Tooltip("Between 5 and 7")]
        public Sprite iconA;
        [Tooltip("Between 8 and 10")]
        public Sprite iconB;
        [Tooltip("10+")]
        public Sprite iconC;
    }
    public List<CellVisual> cellDataList;
    
    public CellVisual GetType(GridCellType type)
    {
        foreach (var cellData in cellDataList)
        {
            if (cellData.type == type)
                return cellData;
        }
        return default;
    }
}
