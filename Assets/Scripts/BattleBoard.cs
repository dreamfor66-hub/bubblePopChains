using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBoard : MonoBehaviour
{
    public static BattleBoard Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }



    public float width;
    public float yMin;
    public float yMax;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        var list = new Vector3[]
        {
            new Vector3(-width / 2f, yMin, 0f),
            new Vector3(-width / 2f, yMax, 0f),
            new Vector3(width / 2f, yMax, 0f),
            new Vector3(width / 2f, yMin, 0f),
        };
        Gizmos.DrawLineStrip(list, true);
    }
}
