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
    {//��ǲ
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
    {//���
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
        //��� cell�� �������� �����´� <<< �̰� �¾�??
        List<Cell> allCells = FindObjectsOfType<Cell>().ToList();
        
        //���� EntityContainer ó�� ��򰡿� List���·� �����صΰ� ã�Ƽ� �� �� �ִ� ����� ã�ƺ����ϳ�???


        //�̹� ���õ�~ ����Ʈ�� ��ü ������ �ѹ� �ɷ����� (���ѷ��� ������)
        
        foreach (Cell removeTarget in selectedCells)
        {
            allCells.Remove(removeTarget);
        }
        

        //allCells�� �ִ� ��� ������ �����ٰ�
        foreach(Cell cell in allCells)
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

    IEnumerator DestroyDelay(List<Cell> cells, Cell selected)
    {
        //���ο� �迭�� ����� ������� ����ִ´�



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

    //0.2�ʿ� �ѹ��� ��Ʈ����
}
