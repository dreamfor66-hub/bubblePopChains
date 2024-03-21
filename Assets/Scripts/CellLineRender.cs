using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellLineRender : MonoBehaviour
{
    LineRenderer line;
    public Vector2 startPos;
    public Vector2 endPos;
    // Start is called before the first frame update
    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Set(Vector2 startPos, Vector2 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;

        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
