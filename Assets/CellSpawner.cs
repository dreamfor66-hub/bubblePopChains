using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellSpawner : MonoBehaviour
{
    // Start is called before the first frame update

    [HideInInspector] public Rigidbody2D rb;
    public List<Cell> cells;

    BorderManager borderManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        borderManager = GetComponentInParent<BorderManager>();
        cells = borderManager.cells;
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
        var rand = Random.Range(0, 5);
        Instantiate(cells[rand], new Vector2(transform.position.x, transform.position.y -1f), Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))));
    }

}
