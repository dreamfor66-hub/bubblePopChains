using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BulletTargeted : MonoBehaviour
{
    public Monster Target { get; set; }
    public int Damage { get; set; }

    public float Speed;
    public float Size;

    void Update()
    {
        if (Target == null)
        {
            Destroy(gameObject);
            return;
        }

        var diff = Target.transform.position - this.transform.position;

        if (diff.magnitude < Size)
        {
            Target.Hit(Damage, true);
            Destroy(gameObject);
            return;
        }
        var dir = diff.normalized;

        var mov = Speed * Time.deltaTime;
        if (mov > diff.magnitude)
            mov = diff.magnitude;

        this.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        var pos = this.transform.position;
        pos += mov * dir;
        this.transform.position = pos;
    }
}
