using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using SnowLib.Unity;
using UnityEngine;

public class BattleState : SingletonBehaviourOnScene<BattleState>
{
    public int playerCurHp;
    public int playerMaxHp;
    public int basicAttackStack;

    public int playerAttackBasic;

    public Action<int, Vector3> OnDamage;
    public Action<int, Vector3> OnHeal;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        playerCurHp = playerMaxHp;
    }
    
    public void Hurt(int damage)
    {
        playerCurHp = Mathf.Clamp(playerCurHp - damage, 0, playerMaxHp);
    }

    public void Heal(int value)
    {
        playerCurHp = Mathf.Clamp(playerCurHp + value, 0, playerMaxHp);
    }
}
