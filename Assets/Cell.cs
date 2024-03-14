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

public class Cell : MonoBehaviour
{
    public CellColor color;

    public DestroyableObject destroyVfx;

    public static List<Cell> CellContainer = new();

    public static List<Cell> GetCellContainer()
    {
        return CellContainer;
    }


    void Awake()
    {
        CellContainer.Add(this);
    }

    private void OnDestroy()
    {
        Instantiate(destroyVfx, this.transform.position, Quaternion.identity);
        CellContainer.Remove(this);
    }

}
