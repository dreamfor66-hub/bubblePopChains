using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;


public class Monster : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public ObjectPool<Monster> Pool { get; set; }
    [Title("스펙")]
    public float MoveSpeed;

    public int MaxHpBasic;
    public int AttackDamageBasic;
    public float AttackPeriod;

    public int MaxHp => MaxHpBasic;
    public int CurHp { get; set; }
    public int ReservedDamage { get; set; }
    public bool ReservedDead => ReservedDamage >= CurHp;

    public static List<Monster> MonsterContainer = new();
    public static List<Monster> GetMonsterContainer()
    {
        return MonsterContainer;
    }


    void Awake()
    {
        
    }

    public void Init()
    {
        CurHp = MaxHp;
    }

    void OnEnable()
    {
        MonsterContainer.Add(this);
    }

    void OnDisable()
    {
        MonsterContainer.Remove(this);
    }

    public void Hit(int damage, bool reserved)
    {
        CurHp = Mathf.Clamp(CurHp - damage, 0, MaxHp);
        BattleState.Instance.OnDamage?.Invoke(damage, this.transform.position + (Vector3)Random.insideUnitCircle * 0.2f);
        if (reserved)
            ReservedDamage -= damage;

        if (CurHp <= 0)
        {
            Release();
        }
    }

    public void ReserveAttack(int value)
    {
        ReservedDamage += value;
    }

    void Update()
    {
        var deltaTime = Time.deltaTime;
        var pos = this.transform.position;
        var yBound = BattleBoard.Instance.yMin + 0.1f;
        if (pos.y < yBound)
        {
            UpdateAttack(deltaTime);
        }
        else
        {
            UpdateMove(deltaTime);
        }
    }

    private float attackTime = 0f;
    private void UpdateAttack(float deltaTime)
    {
        attackTime -= Time.deltaTime;
        if (attackTime < 0f)
        {
            attackTime += AttackPeriod;
            animator.Play("Attack", 0, 0f);
        }
    }

    private void UpdateMove(float deltaTime)
    {
        animator.Play("Walk");
        var pos = this.transform.position;
        pos.y -= MoveSpeed * deltaTime;
        pos.y = Mathf.Max(pos.y, BattleBoard.Instance.yMin);
        this.transform.position = pos;
    }

    public void DoAttack()
    {
        var damage = AttackDamageBasic;
        BattleState.Instance.Hurt(damage);
        BattleState.Instance.OnDamage(damage,
            this.transform.position + new Vector3(0f, -0.2f, 0f) + (Vector3)Random.insideUnitCircle * .2f);
    }

    void Release()
    {
        if (Pool != null)
        {
            gameObject.SetActive(false);
            Pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }


}
