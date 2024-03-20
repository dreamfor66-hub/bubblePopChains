using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    // Start is called before the first frame update

    [HideInInspector] public Rigidbody2D rb;
    public List<Monster> monsters;

    StageManager stageManager;

    public Transform pool;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stageManager = GetComponentInParent<StageManager>();
        pool = FindObjectOfType<MonsterContainer>().transform;
        StartCoroutine(RandomSpawn());
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    public IEnumerator RandomSpawn()
    {
        var time = 999f;
        while (time > 0)
        {

            var rand = Random.Range(0, 1);
            var isMinus = Random.Range(0, 2); //1이면 양수 0이면 음수
            float randPos = (float)Random.Range(1, 100) / 20; //Tofloat
            if (isMinus == 0)
            {
                randPos *= 1;
            }
            else if (isMinus == 1)
            {
                randPos *= -1;
            }
            Instantiate(monsters[rand], new Vector3(transform.position.x + randPos, transform.position.y, -0.1f), Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), pool);
            yield return new WaitForSeconds(1f);
        }
        
        //cellPool.CellContainer.Add(cells[rand]);
    }

}
