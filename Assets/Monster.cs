using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Monster : MonoBehaviour
{
    public MonsterType type;
    public MonsterData data;

    public static List<Monster> MonsterContainer = new();

    public static List<Monster> GetMonsterContainer()
    {
        return MonsterContainer;
    }


    void Awake()
    {
        MonsterContainer.Add(this);
    }

    private void OnDestroy()
    {
        //if (destroyVfx != null)
        //    Instantiate(destroyVfx, this.transform.position, Quaternion.identity);
        MonsterContainer.Remove(this);
    }

}
