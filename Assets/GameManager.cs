using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    /*[HideInInspector] */public bool isDown;
    /*[HideInInspector] */public bool isHold;

    public Cell selectedBefore;
    public Cell selectedCell;
    public List<Cell> selectedCells = new List<Cell>();
    public float cellDistance;


    public float destroyDelay;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {//인풋
        if (Input.GetMouseButtonDown(0))
        {
            isDown = true;
            var mousePosition = Input.mousePosition;
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePosition, transform.forward, 15f);
            if (hit && hit.transform.gameObject.GetComponent<Cell>() != null)
            {
                isHold = true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Input.mousePosition, transform.forward, 15f);
            if (!hit && isHold)
            {
                //selectedCells.Clear();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (selectedCells.Count != 0 || isHold)
            {
                //foreach (var cell in selectedCells)
                //{
                //    Destroy(cell.gameObject);
                //}
                selectedCells.OrderBy(x => Vector2.Distance(x.transform.position, selectedBefore.transform.position)).ToList();
                selectedCells.Remove(selectedBefore);
                selectedCells.Insert(0, selectedBefore);
                StartCoroutine(DestroyDelay(selectedCells, selectedBefore));
            }
            
            isDown = false;
            isHold = false;
        }
    }

    private void FixedUpdate()
    {//기능
        if (isHold)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.forward, 15f);
            if (hit && hit.transform.gameObject.GetComponent<Cell>() != null)
            {
                selectedCell = hit.transform.gameObject.GetComponent<Cell>();
            }

            if (selectedBefore != selectedCell)
            {
                selectedCells.Clear();
                FindCellNearby(selectedCell);
            }

            selectedBefore = selectedCell;
        }
    }

    void FindCellNearby(Cell selected)
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
        foreach(Cell cell in allCells)
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

    IEnumerator DestroyDelay(List<Cell> cells, Cell selected)
    {
        //새로운 배열에 가까운 순서대로 집어넣는다



        foreach(Cell cell in cells)
        {
            var time = destroyDelay;

            while( time > 0)
            {
                time -= Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            Destroy(cell.gameObject);
        }
        selectedCells.Clear();
    }

    //0.2초에 한번씩 디스트로이
}
