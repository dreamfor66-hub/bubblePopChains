using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("현재 상태 확인용 변수")]
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
    [Header("게임 감각에 관한 변수")]
    public float setDelay;
    public float destroyDelay;
    public float bombDestroyBombDelay;
    public float cellDistance;
    public float bombDistance;
    public float bombDistanceIncrement;
    public float bombScaleIncrement;
    bool destroyClear = true;

    float fixedDeltaTime;

    [Space(10f)]
    [Header("아이템")]
    public List<Cell> bombs = new List<Cell>();
    public Cell bomb;

    private HashSet<Cell> visitedCells = new HashSet<Cell>();
    // Start is called before the first frame update
    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
        canvas = FindFirstObjectByType<Canvas>();
    }

    public bool IsTimeStopped()
    {
        return isHold || !destroyClear || SelectedCells.Count > 0;
    }
    
    public bool CanHold()
    {
        return SelectedCells.Count == 0;
    }


    // Update is called once per frame
    void Update()
    {//인풋
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
                    StartCoroutine(DestoyChain(selectedCell));
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

            if (selectedCell != selectedBefore && selectedCell != null)
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

                if (selectedCell.type == CellType.normal)
                {
                    SetSelectedCells(selectedCell);
                    foreach (var i in SelectedCells)
                    {
                        drawScheCells.Add(i.cell);
                    }
                    SetChain(0);
                }
                else if (selectedCell.type == CellType.bomb)
                    SetBombDestroyTarget(selectedCell);

                //drawSheCells는 이제, 선택된(line을 그려야할) 모든 셀을 의미한다.
                
                //지워져야 하는 모든 순서와 연결고리 찾기
            }

            selectedBefore = selectedCell;
        }
        Time.timeScale = IsTimeStopped() ? 0f : 1f;
    }

    IEnumerator DestoyChain(Cell root)
    {
        destroyClear = false;
        var index = 0;
        var destroyCount = 0;
        var isBombDestroyBomb = false;
        var bombVector = new Vector2();
        var bombColor = new CellColor();
        while (SelectedCells.Count > 0)
        {
            var destroyTarget = SelectedCells.Where(x => x.depth == index).ToList();

            SelectedCells.RemoveAll(x => x.depth == index);

            foreach (var i in destroyTarget)
            {
                destroyCount += 1;
                bombVector = i.cell.transform.position;
                bombColor = i.cell.color;
                if (i.cell.type == CellType.normal || i.cell == root)
                { 
                    Destroy(i.cell.gameObject);
                }
                if (i.cell.type == CellType.bomb && i.cell != root)
                {
                    isBombDestroyBomb = true;
                    i.cell.destroyReady = true;
                }
            }
            index += 1;

            yield return new WaitForSecondsRealtime(destroyDelay);
        }

        if (destroyCount >= 5 && root.type != CellType.bomb)
        {
            destroyClear = false;
            CreateBomb(bombVector, destroyCount, bombColor);
            yield return new WaitForSecondsRealtime(destroyDelay);
            destroyClear = true;
        }

        if (isBombDestroyBomb)
        {
            StartCoroutine(DelayedBombDestroy());
        }

        destroyClear = true;
    }


    void SetChain(int index)
    {
        new WaitForSecondsRealtime(setDelay); //이게 재귀함수인데 왜 첫 실행에서만 발생하는지 의문. 첫번째에만 딜레이를 주겠다!는 목표는 있었는데, 이상한 방식으로 구현됨.
        if (drawScheCells.Count <= 0)
            return;

        var drawTarget = SelectedCells.Where(x => x.depth == index).ToList();


        //이 다음 스텝에서, drawScheCells에서 라인을 실제로 그릴 때 마다 값을 제거해주면 예정지가 0이 될때까지 그리게 할 수 있다.

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
            //i가 SelectedCells 기준으로 몇번째 열인지를 봐야지
            var CellIndex = SelectedCells.FindIndex(x => x.cell == i.cell) +1;
            num.text = $"{CellIndex}";
            num.fontSize *= Mathf.Clamp(0.75f + (float)System.Int32.Parse(num.text) / 5, 0.75f, 1.5f);
            StartCoroutine(InvokeSet(index));
        }
    }
    
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
            var allCells = Cell.GetCellContainer().Where(x => x.color == root.color && x.type != CellType.bomb).ToList();

            visitedCells.Add(root);
            queue.Enqueue((0, root, null));
            SelectedCells.Add((0, root, null));

            //애초에 여길 들어오기 전에 시작점과 인접점의 depth가 둘다 0인데... << 윗 부분에 SelectedCells.Add와 visitedCells를 따로 추가해서 시작셀을 구분하는 것으로 해결
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
    void SetBombDestroyTarget(Cell root)
    {
        if (root != null)
        {
            var allCells = Cell.GetCellContainer().Where(x => Vector2.Distance(x.transform.position, root.transform.position) < root.destroyRange).ToList();

            SelectedCells.Add((0, root, null));

            //애초에 여길 들어오기 전에 시작점과 인접점의 depth가 둘다 0인데... << 윗 부분에 SelectedCells.Add와 visitedCells를 따로 추가해서 시작셀을 구분하는 것으로 해결

            foreach(Cell cell in allCells)
            {
                SelectedCells.Add((0, cell, root));
                
            }
        }
    }

    void CreateBomb(Vector2 spawnPos, int destroyCount, CellColor color)
    {
        //5개를 터트리는지 감시한다. or 5개가 터트려졌다면, 인수로 터진 개수를 반환한다. 혹은 destroy 함수는 항상 createBomb를 주시한다.
        //아무튼!
        //root 위치? 아니면 마지막 인덱스 위치? 에 아무튼 폭탄을 심는다. 새로운 오브젝트를 그 위치에 생성한다.

        //BorderManager.

        // 5개 부터 0
        // 6개 부터 +1
        // 7개 부터 +2

        bomb = bombs.First(x => x.color == color);

        var addBomb = Instantiate(bomb, spawnPos, Quaternion.identity);
        addBomb.transform.localScale *= 1+((destroyCount - 5) * bombScaleIncrement);
        addBomb.destroyRange = bombDistance + ((destroyCount - 5) * bombDistanceIncrement);

    }

    IEnumerator DelayedBombDestroy()
    {
        var allDelayBomb = Cell.GetCellContainer().Where(x => x.type == CellType.bomb && x.destroyReady == true).ToList();

        var i = 0;
        while (allDelayBomb.Count > 0)
        {
            yield return new WaitForSecondsRealtime(bombDestroyBombDelay);
            SetBombDestroyTarget(allDelayBomb[i]);
            StartCoroutine(DestoyChain(allDelayBomb[i]));
            i += 1;
        }
    }
}
