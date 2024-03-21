using System.Collections;
using UnityEngine;

public class FxTrail : MonoBehaviour
{
    public TrailRenderer TrailRenderer;
    public Transform Target;

    public AnimationCurve AngleVelCurve;
    public AnimationCurve VelocityCurve;

    
    public CellColor Color { get; private set; } 
    public int Count { get; private set; }

    private float time;
    private Vector3 curDir;
    public void Init(CellColor color, int count)
    {
        Color = color;
        Count = count;
        time = 0f;
        curDir = Quaternion.AngleAxis(Random.Range(-180f, 180f), Vector3.forward) * Vector3.left;
    }

    private bool destroyed = false;
    public void Update()
    {
        if (destroyed)
            return;
        var deltaTime = Time.unscaledDeltaTime;
        time += deltaTime;
        var angleVel = AngleVelCurve.Evaluate(time);
        if (time >= AngleVelCurve.keys[^1].time)
            angleVel = float.MaxValue;
        var vel = VelocityCurve.Evaluate(time);
        var targetPos = Target.transform.position;
        targetPos.z = 0f;
        var diff = targetPos- this.transform.position;

        var toDir = diff.normalized;
        var angleDiff = Vector3.SignedAngle(curDir, toDir, Vector3.forward);

        var angleMove = angleVel * deltaTime;

        if (angleMove > Mathf.Abs(angleDiff))
            curDir = toDir;
        else
            curDir = Quaternion.AngleAxis(angleMove * Mathf.Sign(angleDiff), Vector3.forward) * curDir;

        var distMove = vel * deltaTime;
        if (distMove > diff.magnitude)
        {
            transform.position = targetPos;
            StartCoroutine(DestroyAfter(0.5f));
        }
        else
            transform.position += curDir * distMove;
    }

    IEnumerator DestroyAfter(float time)
    {
        destroyed = true;
        DoAction();
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private void DoAction()
    {
        if (Color == CellColor.white)
        {
            BattleState.Instance.Heal(Count);
            BattleState.Instance.OnHeal(Count, this.transform.position + (Vector3)Random.insideUnitCircle * .5f);
        }
        else
        {
            BattleState.Instance.basicAttackStack += Count;
        }

    }
}
