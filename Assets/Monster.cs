using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MonsterType
{
    normal = 0,
}

public class Monster : MonoBehaviour
{
    public MonsterType type;

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
