using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class Wave
{
    [LabelText("monsters, amount")]
    public List<MonsterInWave> monsters = new List<MonsterInWave>();
    public int nextWaveTime;
}

[System.Serializable]
public class MonsterInWave
{
    [HorizontalGroup]
    [LabelText("")]
    public Monster monster;
    [HorizontalGroup]
    [LabelText("")]
    public int monsterAmount;
}

public class StageManager : MonoBehaviour
{
    [TableList]
    public List<Wave> Waves = new List<Wave>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
