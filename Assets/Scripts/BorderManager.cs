using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderManager : MonoBehaviour
{
    //뭔가 리소스 관리자 같은애가 갖게 하고싶은데... Instance로 가져오게 하는 스크립터블 오브젝트 그런거?
    public  List<Cell> cells ;
    public  List<Cell> GetCells()
    {
        return cells;
    }
    // Start is called before the first frame update

}
