using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class GameManager : MonoBehaviour
{

    /*[HideInInspector] */public bool isHold;

    public List<(int depth, Cell cell, Cell parent)> SelectedCells = new List<(int depth, Cell cell, Cell parent)>();

    public List<int> depths = new();
    public List<Cell> cells = new();
    public List<Cell> parents = new();


    public int count;
    //public List<(int depth, Cell cell, Cell parent)> DestroyScheCells = new List<(int depth, Cell cell, Cell parent)>();


    public Cell selectedBefore;
    public Cell selectedCell;
    //public List<Cell> destroyScheCells = new List<Cell>(); // now
    public List<Cell> lineSetCells = new List<Cell>();
    public float cellDistance;

    public CellLineRender line;
    public List<CellLineRender> lines = new List<CellLineRender>();

    public float setDelay;
    public float destroyDelay;

    float fixedDeltaTime;

    private HashSet<Cell> visitedCells = new HashSet<Cell>();
    // Start is called before the first frame update
    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    public bool IsTimeStopped()
    {
        return isHold || SelectedCells.Count > 0;
    }
    
    public bool CanHold()
    {
        return SelectedCells.Count == 0;
    }


    // Update is called once per frame
    void Update()
    {//��ǲ
        depths = new();
        cells = new();
        parents = new();
        count = SelectedCells.Count;
        foreach ((int x, Cell y, Cell z) in SelectedCells)
        {
            depths.Add(x);
            cells.Add(y);
            parents.Add(z);
        }

            /////
        if (Input.GetMouseButtonDown(0) && CanHold())
        {
            isHold = true;
        }

        if (Input.GetMouseButton(0))
        {
            if (CanHold() && !isHold)
            {
                isHold = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isHold)
        {
            lineSetCells.Clear();
            if (SelectedCells.Count != 0 || isHold)
            {
                if (selectedCell != null)
                {
                    DestroyChain(0);
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
                selectedCell = null;
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
                //ChainSet(selectedCell);
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

                //�������� �ϴ� ��� ������ ����� ã��
                visitedCells.Clear();
                SetSelectedCells(selectedCell);
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

        foreach (var i in destroyTarget)
        {
            Destroy(i.cell.gameObject);
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
    
    void SetChain(int index)
    {
        SelectedCells.OrderBy(x => x.depth).ToList();
        foreach (var i in SelectedCells)
        {
            if(!lineSetCells.Contains(i.cell))
            {
                lineSetCells.Add(i.cell);

                var addLine = Instantiate(line, i.cell.transform.position, Quaternion.identity);
                addLine.GetComponent<CellLineRender>().Set(i.cell.transform.position, SelectedCells.Where(x => x.depth == index + 1).FirstOrDefault().cell.transform.position);
                lines.Add(addLine);
                StartCoroutine(InvokeSet(i.cell));
            }
        }
        
    }
    IEnumerator InvokeSet(Cell cell)
    {
        yield return new WaitForSecondsRealtime(setDelay);
        ChainSet(cell);
    }



    void SetSelectedCells(Cell root)
    {
        if (root != null)
        {
            Queue<(int depth, Cell cell, Cell parent)> queue = new Queue<(int depth, Cell cell, Cell parent)>();
            //SelectedCells.Add((0, root, null));

            queue.Enqueue((0, root, null));


            //���ʿ� ���� ������ ���� �������� �������� depth�� �Ѵ� 0�ε�...
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var allCells = Cell.GetCellContainer().Where(x => x.color == current.cell.color).ToList();
                var remainedCells = new List<Cell>(allCells);

                remainedCells.RemoveAll(x => visitedCells.Contains(x));

                //if (current.depth == 0)
                //    SelectedCells.Add((current.depth, current.cell, current.cell));

                foreach (Cell cell in remainedCells)
                {
                    if (Vector2.Distance(current.cell.transform.position, cell.transform.position) < cellDistance)
                    {
                        visitedCells.Add(cell);
                        queue.Enqueue((current.depth + 1, cell, current.cell));
                        if (cell == root)
                            SelectedCells.Add((current.depth + 0, cell, current.cell));
                        else
                            SelectedCells.Add((current.depth + 1, cell, current.cell));
                    }
                }
            }
        }
    }

    //void SetSelectedCells(int index, Cell selected, Cell parent)
    //{
    //    if (selected != null && visitedCells.Add(selected))
    //    {
    //        // ��� ���� ��������. << ��¥? �׷��� ��??? Where�� �������� �������ְ� �׷��� ���� ������ �츮??
    //        //var allCells = FindObjectsOfType<Cell>().Where(x => x.color == selected.color).ToList();
    //        var allCells = Cell.GetCellContainer().Where(x => x.color == selected.color).ToList();

    //        var remainedCells = new List<Cell>(allCells);

    //        // �̹� ������ ���� �ִٸ�, ��� ���� ������ ������������ �ִ� �� (���ɼ��� ��)�� ���ֱ�. ��ϵ��� ���� ���� ����.
    //        //foreach (var i in SelectedCells)
    //        //{
    //        //    remainedCells.Remove(i.cell);
    //        //}

    //        remainedCells.RemoveAll(x => SelectedCells.Any(y => y.cell == x));

    //        //remainedCells.Remove(selected);
    //        //remainedCells.Remove(parent);


    //        //allCells�� �����ִ� ��ϵ��� ���� ��
    //        foreach (Cell cell in remainedCells)
    //        {
    //            //selected�� �󸶳� �Ÿ��� ���̳������� üũ
    //            if (Vector2.Distance(selected.transform.position, cell.transform.position) < cellDistance)
    //            {
    //                if (index >= SelectedCells.Last().depth)
    //                {
    //                    SelectedCells.Add((index+1, cell, selected));
    //                    SetSelectedCells(index+1, cell, selected);
    //                }
    //            }
    //        }
    //    }
    //}
}
