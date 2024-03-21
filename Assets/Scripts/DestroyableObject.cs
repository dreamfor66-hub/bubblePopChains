using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    public bool unDestroy;
    public float lifeTime;
    [SerializeField] float curLife;
    // Start is called before the first frame update
    void Start()
    {
        curLife = lifeTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!unDestroy)
        {
            if (curLife < 0)
                Destroy(this.gameObject);

            curLife -= Time.deltaTime;
        }

    }
}