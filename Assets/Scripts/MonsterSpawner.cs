using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private BoxCollider2D range;
    [SerializeField] private MonsterContainer pool;
    [SerializeField] private Monster prefab;
    [SerializeField] private float term = 1f;

    private float time;

    void Update()
    {
        time += Time.deltaTime;
        if (time > term)
        {
            time -= term;
            RandomSpawn();
        }
    }

    public void RandomSpawn()
    {
        var bounds = range.bounds;
        var pos = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0f);
        var mon = pool.Spawn(prefab);
        mon.transform.position = pos;
    }

}
