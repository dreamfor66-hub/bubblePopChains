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
    {//��ǲ
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
    {//���
    }

    void FindCellNearby(Cell selected)
    {
        if (selected != null)
        {

            //��� cell�� �������� �����´� <<< �̰� �¾�??
            List<Cell> allCells = FindObjectsOfType<Cell>().ToList();

            //���� EntityContainer ó�� ��򰡿� List���·� �����صΰ� ã�Ƽ� �� �� �ִ� ����� ã�ƺ����ϳ�???


            //�̹� ���õ�~ ����Ʈ�� ��ü ������ �ѹ� �ɷ����� (���ѷ��� ������)

            foreach (Cell removeTarget in selectedCells)
            {
                allCells.Remove(removeTarget);
            }


            //allCells�� �ִ� ��� ������ �����ٰ�
            foreach (Cell cell in allCells)
            {
                if (cell.color == selected.color)
                {
                    //selected�� �󸶳� �Ÿ��� ���̳������� ���Ѵ�.
                    if (Vector3.Distance(selected.transform.position, cell.transform.position) < cellDistance)
                    {
                        //�Ÿ��� ���� ���ϸ�, �ߺ�üũ �� ���� �׸� �߰��Ѵ�.
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
            //����������, ����� �� ���� ������� �����ϴ� �ý����̾�
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
            //�ϴ�, ������ ��Ʈ��. << �׷�����, ������ ������ ���� ������ �ʿ���.
            //�׸���, ����� ���� �� selectedCells���� ã��. << ������ ������ ��Ʈ���� ����, �̰� ���� ã�ƾ� ��
            //�׸��� chainDestroy�� �Ȱ��� �ѹ� �� �������. Invoke�� �ڷ�ƾ�̵� �ƹ��ų��� �Ұǵ�, �ϴ� ���ϸ��Ϸ��� �� �𸣰����ϱ� �κ�ũ�� �Ѵ�.
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
