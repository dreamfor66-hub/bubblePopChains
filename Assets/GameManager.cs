using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    /*[HideInInspector] */public bool isHold;

    public List<(int depth, Cell cell, Cell parent)> SelectedCells = new List<(int depth, Cell cell, Cell parent)>();

    public int count;
    //public List<(int depth, Cell cell, Cell parent)> DestroyScheCells = new List<(int depth, Cell cell, Cell parent)>();


    public Cell selectedBefore;
    public Cell selectedCell;
    public List<Cell> destroyScheCells = new List<Cell>(); // now
    public List<Cell> lineSetCells = new List<Cell>();
    public float cellDistance;

    public CellLineRender line;
    public List<CellLineRender> lines = new List<CellLineRender>();

    public float setDelay;
    public float destroyDelay;

    float fixedDeltaTime;

    // Start is called before the first frame update
    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    public bool IsTimeStopped()
    {
        return isHold || SelectedCells.Count > 0;
    }


    // Update is called once per frame
    void Update()
    {//인풋
        count = SelectedCells.Count;
        if (Input.GetMouseButtonDown(0))
        {
            isHold = true;
            var mousePosition = Input.mousePosition;
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            lineSetCells.Clear();
            if (SelectedCells.Count != 0 || isHold)
            {
                if (selectedCell != null)
                {
                    DestroyChain(0);
                    //ChainDestroy(0,selectedCell,null);
                }
            }
            foreach (CellLineRender line in lines)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
            lines.Clear();
            isHold = false;
        }


        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;


        if (isHold)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.forward, 15f);
            if (hit && hit.transform.gameObject.GetComponent<Cell>() != null)
            {
                selectedCell = hit.transform.gameObject.GetComponent<Cell>();
            }
            else if (!hit)
            {
                lineSetCells.Clear();
                SelectedCells.Clear();
                foreach (CellLineRender line in lines)
                {
                    if (line != null)
                    {
                        Destroy(line.gameObject);
                    }
                }
                lines.Clear();
                ChainSet(selectedCell);
            }

            if (selectedBefore != selectedCell)
            {
                lineSetCells.Clear();
                SelectedCells.Clear();
                foreach (CellLineRender line in lines)
                {
                    if (line != null)
                    {
                        Destroy(line.gameObject);
                    }
                }
                lines.Clear();
                lineSetCells.Add(selectedCell);
                ChainSet(selectedCell);

                //지워져야 하는 모든 순서와 연결고리 찾기
                SelectedCells.Add((0, selectedCell, null));
                SetSelectedCells(0, selectedCell, null);
            }

            selectedBefore = selectedCell;
        }
        Time.timeScale = IsTimeStopped() ? 0f : 1f;
    }

    void DestroyChain(int index)
    {
        if (SelectedCells.Count <= 0)
            return;

        var destroyTarget = SelectedCells.Where(x => x.depth == index).ToList();

        SelectedCells.RemoveAll(x => x.depth == index);

        foreach ((int depth, Cell cell, Cell parent) in destroyTarget)
        {
            Destroy(cell.gameObject);
        }
        

        StartCoroutine(InvokeDestroyChain(index));
    }

    IEnumerator InvokeDestroyChain(int index)
    {
        yield return new WaitForSecondsRealtime(destroyDelay);
        DestroyChain(index + 1);
    }

    void ChainSet(Cell selected)
    {
        
        foreach (var i in SelectedCells)
        {
            if(!lineSetCells.Contains(i.cell))
            {
                if (Vector2.Distance(i.cell.transform.position, selected.transform.position) < cellDistance)
                {
                    var addLine = Instantiate(line, selected.transform.position, Quaternion.Euler(i.cell.transform.position - selected.transform.position));
                    addLine.GetComponent<CellLineRender>().Set(selected.transform.position, i.cell.transform.position);
                    lineSetCells.Add(i.cell);
                    lines.Add(addLine);
                    StartCoroutine(InvokeSet(i.cell));
                }
            }
        }
        
    }
    IEnumerator InvokeSet(Cell cell)
    {
        yield return new WaitForSecondsRealtime(setDelay);
        ChainSet(cell);
    }




    void SetSelectedCells(int index, Cell selected, Cell parent)
    {
        if (selected != null)
        {
            // 모든 셀을 가져오기. << 진짜? 그래도 됨??? Where로 같은색만 필터해주고 그래야 하지 않을까 우리??
            var allCells = FindObjectsOfType<Cell>().Where(x => x.color == selected.color).ToList();

            // 이미 선택한 셀이 있다면, 모든 셀을 가져온 지역변수에서 있는 셀 (가능성의 셀)을 빼주기. 등록되지 않은 셀만 남음.
            //foreach (var i in SelectedCells)
            //{
            //    allCells.Remove(i.cell);
            //}

            //allCells.Remove(selected);
            //allCells.Remove(parent);

            //var remainedCells = allCells;

            //allCells에 남아있는 등록되지 않은 셀
            foreach (Cell cell in allCells)
            {
                //색상이 맞는지 체크
                if (cell.color == selected.color)
                {
                    //selected와 얼마나 거리가 차이나는지를 체크
                    if (Vector2.Distance(selected.transform.position, cell.transform.position) < cellDistance)
                    {
                        //중복체크
                        if (SelectedCells.Any(x=>x.cell != cell))
                        {
                            SelectedCells.Add((index+1, cell, selected));
                            SetSelectedCells(index+1, cell, selected);
                        }
                    }
                }
            }
        }
    }
}
