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
}
