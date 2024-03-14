using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("���� ���� Ȯ�ο� ����")]
    /*[HideInInspector] */
    public bool isHold;

    public List<(int depth, Cell cell, Cell parent)> SelectedCells = new List<(int depth, Cell cell, Cell parent)>();

    //public List<int> depths = new();
    //public List<Cell> cells = new();
    //public List<Cell> parents = new();
    //public int count;
    //public List<(int depth, Cell cell, Cell parent)> DestroyScheCells = new List<(int depth, Cell cell, Cell parent)>();

    public Cell selectedBefore;
    public Cell selectedCell;
    public List<Cell> drawScheCells = new List<Cell>();

    public CellLineRender line;
    public List<CellLineRender> lines = new List<CellLineRender>();

    [HideInInspector] public TextMeshProUGUI cellNumber;
    List<TextMeshProUGUI> cellNumbers = new List<TextMeshProUGUI>();
    Canvas canvas;

    [Space(10f)]
    [Header("���� ������ ���� ����")]
    public float setDelay;
    public float destroyDelay;
    public float cellDistance;
    bool destroyOneMore;

    float fixedDeltaTime;

    [Space(10f)]
    [Header("������")]
    public Bomb bomb;

    private HashSet<Cell> visitedCells = new HashSet<Cell>();
    // Start is called before the first frame update
    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
        canvas = FindFirstObjectByType<Canvas>();
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
            drawScheCells.Clear();
            if (SelectedCells.Count != 0 || isHold)
            {
                if (selectedCell != null)
                {
                    destroyOneMore = true;
                    DestroyChain(0);
                }
            }
            foreach (CellLineRender line in lines)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }
            foreach (TextMeshProUGUI text in cellNumbers)
            {
                if (text != null)
                    Destroy(text.gameObject);
            }
            lines.Clear();
            cellNumbers.Clear();
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
                drawScheCells.Clear();
                SelectedCells.Clear();
                foreach (CellLineRender line in lines)
                {
                    if (line != null)
                        Destroy(line.gameObject);
                }
                foreach (TextMeshProUGUI text in cellNumbers)
                {
                    if (text != null)
                        Destroy(text.gameObject);
                }
                lines.Clear();
                cellNumbers.Clear();
            }

            if (selectedCell != selectedBefore)
            {
                drawScheCells.Clear();
                SelectedCells.Clear();
                foreach (CellLineRender line in lines)
                {
                    if (line != null)
                        Destroy(line.gameObject);
                }
                foreach (TextMeshProUGUI text in cellNumbers)
                {
                    if (text != null)
                        Destroy(text.gameObject);
                }
                lines.Clear();
                cellNumbers.Clear();

                drawScheCells.Add(selectedCell);
                visitedCells.Clear();

                SetSelectedCells(selectedCell);

                //drawSheCells�� ����, ���õ�(line�� �׷�����) ��� ���� �ǹ��Ѵ�.
                foreach (var i in SelectedCells)
                {
                    drawScheCells.Add(i.cell);
                }
                SetChain(0);
                //�������� �ϴ� ��� ������ ����� ã��
            }

            selectedBefore = selectedCell;
        }
        Time.timeScale = IsTimeStopped() ? 0f : 1f;
    }

    void DestroyChain(int index)
    {
        if (SelectedCells.Count <= 0 && !destroyOneMore)
            return;

        if (SelectedCells.Count <= 0)
        {
            destroyOneMore = false;
            isHold = true ;
            new WaitForSecondsRealtime(destroyDelay);
            isHold = false;
            return;
        }

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

    void SetChain(int index)
    {
        new WaitForSecondsRealtime(setDelay); //�̰� ����Լ��ε� �� ù ���࿡���� �߻��ϴ��� �ǹ�. ù��°���� �����̸� �ְڴ�!�� ��ǥ�� �־��µ�, �̻��� ������� ������.
        if (drawScheCells.Count <= 0)
            return;

        var drawTarget = SelectedCells.Where(x => x.depth == index).ToList();


        //�� ���� ���ܿ���, drawScheCells���� ������ ������ �׸� �� ���� ���� �������ָ� �������� 0�� �ɶ����� �׸��� �� �� �ִ�.

        foreach (var i in drawTarget)
        {
            if (i.parent != null)
            {
                var addLine = Instantiate(line, i.cell.transform.position, Quaternion.Euler(i.cell.transform.position - i.parent.transform.position));
                addLine.GetComponent<CellLineRender>().Set(i.cell.transform.position, i.parent.transform.position);
                drawScheCells.Remove(i.cell);
                lines.Add(addLine);

            }
            var num = Instantiate(cellNumber, Camera.main.WorldToScreenPoint(i.cell.transform.position), Quaternion.identity, canvas.transform);
            cellNumbers.Add(num);
            //i�� SelectedCells �������� ���° �������� ������
            var CellIndex = SelectedCells.FindIndex(x => x.cell == i.cell) +1;
            num.text = $"{CellIndex}";
            num.fontSize *= Mathf.Clamp(0.75f + (float)System.Int32.Parse(num.text) / 5, 0.75f, 1.5f);
            StartCoroutine(InvokeSet(index));
        }
    }
    
    //void SetChain()
    //{
    //    //�ε����� �ƴ϶�, parent�� cell�� �����ͼ� �۾��ϸ� ��.
    //    //��� ������ �� ���鼭 cell���� Instantiate line => parent��
    //    foreach (var i in SelectedCells)
    //    {
    //        //lineSetCells.Add(i.cell);

    //        var addLine = Instantiate(line, i.cell.transform.position, Quaternion.Euler(i.cell.transform.position - i.parent.transform.position));
    //        addLine.GetComponent<CellLineRender>().Set(i.cell.transform.position, i.parent.transform.position);
    //        lines.Add(addLine);
    //        StartCoroutine(InvokeSet(i.cell));
    //    }
    //}

    IEnumerator InvokeSet(int index)
    {
        yield return new WaitForSecondsRealtime(setDelay);
        SetChain(index + 1);
    }



    void SetSelectedCells(Cell root)
    {
        if (root != null)
        {
            Queue<(int depth, Cell cell, Cell parent)> queue = new Queue<(int depth, Cell cell, Cell parent)>();
            //SelectedCells.Add((0, root, null));
            var allCells = Cell.GetCellContainer().Where(x => x.color == root.color).ToList();

            visitedCells.Add(root);
            queue.Enqueue((0, root, null));
            SelectedCells.Add((0, root, null));

            //���ʿ� ���� ������ ���� �������� �������� depth�� �Ѵ� 0�ε�... << �� �κп� SelectedCells.Add�� visitedCells�� ���� �߰��ؼ� ���ۼ��� �����ϴ� ������ �ذ�
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach(Cell cell in allCells)
                {
                    if (visitedCells.Contains(cell))
                        continue;
                    if (Vector2.Distance(current.cell.transform.position, cell.transform.position) < cellDistance)
                    {
                        visitedCells.Add(cell);
                        queue.Enqueue((current.depth + 1, cell, current.cell));
                        SelectedCells.Add((current.depth + 1, cell, current.cell));
                    }
                }
            }
        }
    }

    void CreateBomb(Transform spawnPos, int destroyCount)
    {
        //5���� ��Ʈ������ �����Ѵ�. or 5���� ��Ʈ�����ٸ�, �μ��� ���� ������ ��ȯ�Ѵ�. Ȥ�� destroy �Լ��� �׻� createBomb�� �ֽ��Ѵ�.
        //�ƹ�ư!
        //root ��ġ? �ƴϸ� ������ �ε��� ��ġ? �� �ƹ�ư ��ź�� �ɴ´�. ���ο� ������Ʈ�� �� ��ġ�� �����Ѵ�.

        //BorderManager.

        //var bomb = Instantiate ()

    }
}
