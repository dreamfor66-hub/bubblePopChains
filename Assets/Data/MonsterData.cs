using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterType
{
    normal = 0,
}

[CreateAssetMenu(fileName = "MonsterData", menuName = "Data/MonsterData", order = 0)]
public class MonsterData : ScriptableObject
{
    public float MoveSpd;
    public float HpMultiplier;
}