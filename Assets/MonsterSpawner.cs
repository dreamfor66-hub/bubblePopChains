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
        pool = FindObjectOfType<CellPool>().transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var hits = Physics2D.BoxCastAll(this.transform.position, Vector2.one, 0f, Vector2.up, 0.01f);
        if (!hits.Any(x => x.collider.gameObject.name.Contains("Cell")))
        {
            RandomSpawn();
        }

    }
    public void RandomSpawn()
    {
        var rand = Random.Range(0, 1);
        var isMinus = Random.Range(0, 2); //1�̸� ��� 0�̸� ����
        float randPos = (float)Random.Range(1, 100) / 100; //Tofloat
        if (isMinus == 0)
        {
            randPos *= 1;
        }
        else if (isMinus == 1)
        {
            randPos *= -1;
        }
        Instantiate(monsters[rand], new Vector2(transform.position.x + randPos, transform.position.y - .5f), Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), pool);
        //cellPool.CellContainer.Add(cells[rand]);
    }

}
