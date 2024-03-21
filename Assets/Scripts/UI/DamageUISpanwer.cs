using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DamageUISpanwer : MonoBehaviour
{
    private ObjectPool<DamageUI> pool;
    [SerializeField] private DamageUI prefab;
    
    void Awake()
    {
        pool = new ObjectPool<DamageUI>(Create);
    }

    void Start()
    {
        BattleState.Instance.OnDamage += OnDamage;
        BattleState.Instance.OnHeal += OnHeal;
    }

    void OnDestroy()
    {
        if (BattleState.Instance != null)
        {
            BattleState.Instance.OnDamage -= OnDamage;
            BattleState.Instance.OnHeal -= OnHeal;
        }
    }

    public DamageUI Create()
    {
        var obj =  Instantiate(prefab, this.transform);
        obj.Pool = pool;
        return obj;
    }
    public void OnDamage(int damage, Vector3 position)
    {
        var ui = pool.Get();
        ui.Set(position, damage);
    }

    public void OnHeal(int value, Vector3 position)
    {
        var ui = pool.Get();
        ui.SetHeal(position, value);
    }
}
