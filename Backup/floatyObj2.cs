using System;
using UnityEngine;

public class floatyObj2 : MonoBehaviour
{
    public float frequencyMax = 0.001f;
    public float frequencyMaxRot = 0.001f;
    public float frequencyMin = 0.0001f;
    public float frequencyMinRot = 0.0001f;
    private float m_interval;
    private float m_rotationInterval;
    public float magnitude = 0.0001f;
    public float magnitudeRot;

    private void Start()
    {
        this.m_interval = UnityEngine.Random.Range(this.frequencyMin, this.frequencyMax);
        this.m_rotationInterval = UnityEngine.Random.Range(this.frequencyMinRot, this.frequencyMaxRot);
    }

    private void Update()
    {
        float x = Mathf.Sin(UnityEngine.Time.time * this.m_interval) * this.magnitude;
        Transform transform = base.transform;
        transform.position += new Vector3(x, x, x);
        float num2 = Mathf.Sin(UnityEngine.Time.time * this.m_rotationInterval) * this.magnitudeRot;
        Transform transform2 = base.transform;
        transform2.eulerAngles += new Vector3(num2, num2, num2);
    }
}

