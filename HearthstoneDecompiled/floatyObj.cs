using System;
using UnityEngine;

public class floatyObj : MonoBehaviour
{
    public float frequencyMax = 0.001f;
    public float frequencyMin = 0.0001f;
    private float m_interval;
    public float magnitude = 0.0001f;

    private void Start()
    {
        this.m_interval = UnityEngine.Random.Range(this.frequencyMin, this.frequencyMax);
    }

    private void Update()
    {
        float x = Mathf.Sin(UnityEngine.Time.time * this.m_interval) * this.magnitude;
        Vector3 vector = new Vector3(x, x, x);
        Transform transform = base.transform;
        transform.position += vector;
        Transform transform2 = base.transform;
        transform2.eulerAngles += vector;
    }
}

