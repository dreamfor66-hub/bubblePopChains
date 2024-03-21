using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellColor
{
    white = 0,
    red = 1,
    blue = 2,
    yellow = 3,
    green = 4,
}
public enum CellType
{
    normal = 0,
    bomb = 1,
}

public class Cell : MonoBehaviour
{
    public CellColor color;
    public CellType type;

    [HideInInspector] public float destroyRange;

    [HideInInspector] public bool destroyReady = false; 

    public DestroyableObject destroyVfx;

    public static List<Cell> CellContainer = new();

    public static List<Cell> GetCellContainer()
    {
        return CellContainer;
    }


    void Awake()
    {
        CellContainer.Add(this);
        destroyReady = false;
    }

    private void OnDestroy()
    {
        CellContainer.Remove(this);
    }

    public void DestroyFx()
    {
        if (destroyVfx != null)
            Instantiate(destroyVfx, this.transform.position, Quaternion.identity);
    }

}
