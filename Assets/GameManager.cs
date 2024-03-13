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
    {//��ǲ
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

                //�������� �ϴ� ��� ������ ����� ã��
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
            // ��� ���� ��������. << ��¥? �׷��� ��??? Where�� �������� �������ְ� �׷��� ���� ������ �츮??
            var allCells = FindObjectsOfType<Cell>().Where(x => x.color == selected.color).ToList();

            // �̹� ������ ���� �ִٸ�, ��� ���� ������ ������������ �ִ� �� (���ɼ��� ��)�� ���ֱ�. ��ϵ��� ���� ���� ����.
            //foreach (var i in SelectedCells)
            //{
            //    allCells.Remove(i.cell);
            //}

            //allCells.Remove(selected);
            //allCells.Remove(parent);

            //var remainedCells = allCells;

            //allCells�� �����ִ� ��ϵ��� ���� ��
            foreach (Cell cell in allCells)
            {
                //������ �´��� üũ
                if (cell.color == selected.color)
                {
                    //selected�� �󸶳� �Ÿ��� ���̳������� üũ
                    if (Vector2.Distance(selected.transform.position, cell.transform.position) < cellDistance)
                    {
                        //�ߺ�üũ
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
