using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxTrailPool : MonoBehaviour
{
    public FxTrail TrailPrefab;

    public void Spawn(Vector3 position, CellColor color, int count)
    {
        var trail = Instantiate(TrailPrefab, position, Quaternion.identity);
        trail.Init(color, count);
        trail.gameObject.SetActive(true);
    }
}
