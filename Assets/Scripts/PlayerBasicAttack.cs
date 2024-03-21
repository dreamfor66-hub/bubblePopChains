using System.Linq;
using SnowLib.Extensions;
using UnityEngine;

public class PlayerBasicAttack : MonoBehaviour
{
    [SerializeField] private Transform spawnAnchor;
    [SerializeField] private BulletTargeted prefab;
    [SerializeField] private float attackPeriod;


    private float time;
    void Update()
    {
        time += Time.deltaTime;
        if (time > attackPeriod)
        {
            time -= attackPeriod;
            if (BattleState.Instance.basicAttackStack > 0)
            {
                BattleState.Instance.basicAttackStack--;
                Spawn();
            }
        }
    }

    private void Spawn()
    {
        var target = FindTarget();
        if (target == null)
            return;

        var damage = BattleState.Instance.playerAttackBasic;

        target.ReserveAttack(damage);
        var bulletObj = Instantiate(prefab, spawnAnchor.transform.position, Quaternion.identity);
        bulletObj.Target = target;
        bulletObj.Damage = damage;
    }


    private Monster FindTarget()
    {
        return Monster.MonsterContainer.Where(x => !x.ReservedDead).MinBy(x => x.transform.position.y);
    }
}
