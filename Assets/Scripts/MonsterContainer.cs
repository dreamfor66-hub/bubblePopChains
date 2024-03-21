using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MonsterContainer : MonoBehaviour
{
    private Dictionary<Monster, ObjectPool<Monster>> pools = new();

    // Start is called before the first frame update
    void Start()
    {
    }


    private ObjectPool<Monster> GetPool(Monster mon)
    {
        if (!pools.ContainsKey(mon))
        {
            pools[mon] = new ObjectPool<Monster>(() => CreateMonster(mon), actionOnDestroy: DestroyMonster);
        }
        return pools[mon];
    }

    public Monster Spawn(Monster mon)
    {
        var pool = GetPool(mon);
        var monObj = pool.Get();
        monObj.Init();
        monObj.gameObject.SetActive(true);
        return monObj;
    }

    private Monster CreateMonster(Monster prefab)
    {
        var monObj = Instantiate(prefab, this.transform);
        monObj.Pool = pools[prefab];
        return monObj;
    }

    private void DestroyMonster(Monster mon)
    {
        Destroy(mon.gameObject);
    }
}
