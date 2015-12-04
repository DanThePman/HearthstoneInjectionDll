using System;
using UnityEngine;

public class ShakeObject : MonoBehaviour
{
    public float amount = 1f;
    private Vector3 orgPos;

    private void Start()
    {
        this.orgPos = base.transform.position;
    }

    private void Update()
    {
        float x = UnityEngine.Random.value * this.amount;
        float y = UnityEngine.Random.value * this.amount;
        float z = UnityEngine.Random.value * this.amount;
        x *= this.amount;
        y *= this.amount;
        z *= this.amount;
        base.transform.position = this.orgPos + new Vector3(x, y, z);
    }
}

