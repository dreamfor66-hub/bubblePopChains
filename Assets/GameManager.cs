using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    /*[HideInInspector] */public bool isHold;


    //public List<(int depth, Cell cell, Cell parent)>selectedCells = new List<(int depth, Cell cell, Cell parent)>();


    public Cell selectedBefore;
    public Cell selectedCell;
    public List<Cell> selectedCells = new List<Cell>();
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
        return isHold || selectedCells.Count > 0;
    }


    // Update is called once per frame
    void Update()
    {//인풋
        if (Input.GetMouseButtonDown(0))
        {
            isHold = true;
            var mousePosition = Input.mousePosition;
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        }

        if (Input.GetMouseButtonUp(0))
        {
            lineSetCells.Clear();
            if (selectedCells.Count != 0 || isHold || selectedCells != null)
            {
                if (selectedCell != null)
                {
                    selectedCells.OrderBy(x => Vector2.Distance(x.transform.position, selectedCell.transform.position)).ToList();
                    selectedCells.Remove(selectedCell);
                    selectedCells.Insert(0, selectedCell);
                    ChainDestroy(selectedCell);
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
                //selectedCell = null;
                lineSetCells.Clear();
                selectedCells.Clear();
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
                selectedCells.Clear();
                FindCellNearby(selectedCell);
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
            }

            selectedBefore = selectedCell;
        }
        Time.timeScale = IsTimeStopped() ? 0f : 1f;
    }

    private void FixedUpdate()
    {//기능
    }

    void FindCellNearby(Cell selected)
    {
        if (selected != null)
        {

            //모든 cell의 정보값을 가져온다 <<< 이거 맞아??
            List<Cell> allCells = FindObjectsOfType<Cell>().ToList();

            //셀을 EntityContainer 처럼 어딘가에 List형태로 저장해두고 찾아서 쓸 수 있는 방법을 찾아봐야하나???


            //이미 선택된~ 리스트를 전체 셀에서 한번 걸러내기 (무한루프 방지용)

            foreach (Cell removeTarget in selectedCells)
            {
                allCells.Remove(removeTarget);
            }


            //allCells에 있는 모든 셀들을 가져다가
            foreach (Cell cell in allCells)
            {
                if (cell.color == selected.color)
                {
                    //selected와 얼마나 거리가 차이나는지를 구한다.
                    if (Vector3.Distance(selected.transform.position, cell.transform.position) < cellDistance)
                    {
                        //거리가 일정 이하면, 중복체크 후 선택 항목에 추가한다.
                        if (!selectedCells.Contains(cell))
                        {
                            selectedCells.Add(cell);
                            FindCellNearby(cell);
                        }
                    }
                }
            }
        }
    }

    void ChainDestroy(Cell selected)
    {
        if (selected != null)
        {
            //연쇄적으로, 가까운 것 부터 순서대로 격파하는 시스템이야
            destroyScheCells.Clear();

            foreach (Cell cell in selectedCells)
            {
                if (Vector2.Distance(cell.transform.position, selected.transform.position) < cellDistance)
                {
                    destroyScheCells.Add(cell);
                }
            }


            selectedCells.Remove(selected);
            destroyScheCells.Remove(selected);
            Destroy(selected.gameObject);

            foreach (Cell cell in destroyScheCells)
            {
                StartCoroutine(InvokeDestroy(cell));
            }
            //일단, 누른걸 터트려. << 그러려면, 누른게 뭔지에 대한 정보가 필요해.
            //그리고, 가까운 다음 걸 selectedCells에서 찾아. << 누른게 뭔지를 터트리기 전에, 이거 부터 찾아야 해
            //그리고 chainDestroy를 똑같이 한번 더 실행시켜. Invoke든 코루틴이든 아무거나로 할건데, 일단 와일리턴루프 잘 모르겠으니까 인보크로 한다.
        }
        else
        {
            if (selectedCells.Count > 1)
            {
                Destroy(selectedCells[0].gameObject);
                selectedCells.RemoveAt(0);
                StartCoroutine(InvokeDestroy(selectedCells[0]));
            }
        }

        
    }

    IEnumerator InvokeDestroy(Cell cell)
    {
        yield return new WaitForSecondsRealtime(destroyDelay);
        ChainDestroy(cell);
    }

    void ChainSet(Cell selected)
    {
        
        foreach (Cell cell in selectedCells)
        {
            if(!lineSetCells.Contains(cell))
            {
                if (Vector2.Distance(cell.transform.position, selected.transform.position) < cellDistance)
                {
                    var addLine = Instantiate(line, selected.transform.position, Quaternion.Euler(cell.transform.position - selected.transform.position));
                    addLine.GetComponent<CellLineRender>().Set(selected.transform.position, cell.transform.position);
                    lineSetCells.Add(cell);
                    lines.Add(addLine);
                    StartCoroutine(InvokeSet(cell));
                }
            }
        }
        
    }
    IEnumerator InvokeSet(Cell cell)
    {
        yield return new WaitForSecondsRealtime(setDelay);
        ChainSet(cell);
    }
}
