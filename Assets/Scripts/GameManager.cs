using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [Header("현재 상태 확인용 변수")]
    public List<(int depth, Cell cell, Cell parent)> SelectedCells = new List<(int depth, Cell cell, Cell parent)>();
    public FxTrailPool TrailPool;
    
    public Cell SelectedCell => SelectedCells.Count > 0 ? SelectedCells[0].cell : null;

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

    [Space(10f)]
    [Header("아이템")]
    public List<Cell> bombs = new List<Cell>();
    public Cell bomb;

    enum State
    {
        Idle,
        DestroyChain,
        BombDelay,
    }

    private State state;
    // Start is called before the first frame update
    void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();
        state = State.Idle;
    }

    public bool IsTimeStopped()
    {
        return (Input.GetMouseButton(0) && state != State.BombDelay) || state == State.DestroyChain;
    }

    // Update is called once per frame
    void Update()
    {
        //인풋    
        if (Input.GetMouseButton(0) && state == State.Idle)
        {
            var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.forward, 15f);
            var currentCell = hit.transform?.GetComponent<Cell>();
            SelectCell(currentCell);
        }

        if (Input.GetMouseButtonUp(0) && SelectedCells.Count > 0)
        {
            ClearDraw();
            StartCoroutine(DestroyChain());
        }
        
        Physics2D.simulationMode = IsTimeStopped() ? SimulationMode2D.Script : SimulationMode2D.FixedUpdate;
    }

    public void SelectCell(Cell newCell)
    {
        if (SelectedCell == newCell)
            return;
        
        ClearDraw();
        SelectedCells.Clear();

        if (newCell != null)
        {
            if (newCell.type == CellType.normal)
            {
                SetSelectedCells(newCell);
            }
            else if (newCell.type == CellType.bomb)
            {
                SetBombDestroyTarget(newCell);
            }
            DrawChain();
        }
    }


    IEnumerator DestroyChain()
    {
        state = State.DestroyChain;
        var root = SelectedCells.First().cell;
        var bombVector = SelectedCells.Last().cell.transform.position;
        var bombColor = SelectedCells.Last().cell.color;
        var destroyCount = SelectedCells.Count;

        var prevDepth = 0;
        var score = 1;
        foreach (var (depth, cell, _) in SelectedCells)
        {
            if (prevDepth < depth)
            {
                prevDepth = depth;
                yield return new WaitForSecondsRealtime(destroyDelay);
            }

            if (cell.type == CellType.normal || cell == root)
            {
                cell.DestroyFx();
                TrailPool.Spawn(cell.transform.position, cell.color, score);
                Destroy(cell.gameObject);
            }

            if (cell.type == CellType.bomb && cell != root)
            {
                cell.destroyReady = true;
            }

            score++;
        }

        SelectedCells.Clear();
        yield return new WaitForSecondsRealtime(destroyDelay);
        
        if (destroyCount >= 5 && root.type != CellType.bomb)
        {
            CreateBomb(bombVector, destroyCount, bombColor);
            yield return new WaitForSecondsRealtime(destroyDelay);
        }

        var delayedBomb = Cell.GetCellContainer().FirstOrDefault(x => x.type == CellType.bomb && x.destroyReady == true);
        if (delayedBomb != null)
        {
            state = State.BombDelay;
            yield return new WaitForSecondsRealtime(bombDestroyBombDelay);
            SetBombDestroyTarget(delayedBomb);
            StartCoroutine(DestroyChain());
        }
        else
        {
            state = State.Idle;
        }
    }


    private Coroutine drawChainCoroutine;
    void ClearDraw()
    {
        if (drawChainCoroutine != null)
            StopCoroutine(drawChainCoroutine);
        ClearDrawObjs();
    }

    void ClearDrawObjs()
    {
        foreach (var line in lines)
        {
            Destroy(line.gameObject);
        }
        foreach (var text in cellNumbers)
        {
            Destroy(text.gameObject);
        }
        lines.Clear();
        cellNumbers.Clear();
    }

    void DrawChain()
    {
        ClearDraw();
        drawChainCoroutine = StartCoroutine(DrawChainEnumerator());
    }

    IEnumerator DrawChainEnumerator()
    {
        var idx = 1;
        var nowDepth = 0;
        foreach (var (depth, cell, parent) in SelectedCells)
        {
            if (depth > nowDepth)
            {
                nowDepth = depth;
                yield return new WaitForSecondsRealtime(setDelay);
            }
            var cellPos = cell.transform.position;

            if (parent != null)
            {
                var parentPos = parent.transform.position;
                var addLine = Instantiate(line, cellPos, Quaternion.Euler(cellPos - parentPos));
                addLine.GetComponent<CellLineRender>().Set(cellPos, parentPos);
                lines.Add(addLine);
            }

            var num = Instantiate(cellNumber, cellPos, Quaternion.identity, canvas.transform);
            cellNumbers.Add(num);
            var curIdx = idx++;
            num.text = $"{curIdx}";
            num.fontSize *= Mathf.Clamp(0.75f + curIdx / 5f, 0.75f, 1.5f);
        }
    }
    
    void SetSelectedCells(Cell root)
    {   
        var allCells = Cell.GetCellContainer().Where(x => x.color == root.color && x.type != CellType.bomb).ToList();
        SelectedCells.Clear();
        SelectedCells.Add((0, root, null));
        var visitedCells = new HashSet<Cell> { root };
        var queueIdx = 0;

        while (queueIdx < SelectedCells.Count)
        {
            var current = SelectedCells[queueIdx++];
            foreach (Cell cell in allCells)
            {
                if (visitedCells.Contains(cell))
                    continue;
                if (Vector2.Distance(current.cell.transform.position, cell.transform.position) < cellDistance)
                {
                    visitedCells.Add(cell);
                    SelectedCells.Add((current.depth + 1, cell, current.cell));
                }
            }
        }

    }
    void SetBombDestroyTarget(Cell root)
    {
        var allCells = Cell.GetCellContainer().Where(x => Vector2.Distance(x.transform.position, root.transform.position) < root.destroyRange).ToList();
        SelectedCells.Clear();
        SelectedCells.Add((0, root, null));
        foreach(Cell cell in allCells)
        {
            if (cell == root)
                continue;
            SelectedCells.Add((0, cell, root));
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
}
