using System;
using UnityEngine;

public class UberFloaty : MonoBehaviour
{
    public float frequencyMax = 3f;
    public float frequencyMaxRot = 3f;
    public float frequencyMin = 1f;
    public float frequencyMinRot = 1f;
    public bool localSpace = true;
    private Vector3 m_interval;
    private Vector3 m_offset;
    private Vector3 m_rotationInterval;
    public Vector3 magnitude = new Vector3(0.001f, 0.001f, 0.001f);
    public Vector3 magnitudeRot = new Vector3(0f, 0f, 0f);

    private void Start()
    {
        this.m_interval.x = UnityEngine.Random.Range(this.frequencyMin, this.frequencyMax);
        this.m_interval.y = UnityEngine.Random.Range(this.frequencyMin, this.frequencyMax);
        this.m_interval.z = UnityEngine.Random.Range(this.frequencyMin, this.frequencyMax);
        this.m_offset.x = 0.5f * UnityEngine.Random.Range(-this.m_interval.x, this.m_interval.x);
        this.m_offset.y = 0.5f * UnityEngine.Random.Range(-this.m_interval.y, this.m_interval.y);
        this.m_offset.z = 0.5f * UnityEngine.Random.Range(-this.m_interval.z, this.m_interval.z);
        this.m_rotationInterval.x = UnityEngine.Random.Range(this.frequencyMinRot, this.frequencyMaxRot);
        this.m_rotationInterval.y = UnityEngine.Random.Range(this.frequencyMinRot, this.frequencyMaxRot);
        this.m_rotationInterval.z = UnityEngine.Random.Range(this.frequencyMinRot, this.frequencyMaxRot);
    }

    private void Update()
    {
        Vector3 vector;
        Vector3 vector2;
        vector.x = (Mathf.Sin((UnityEngine.Time.time * this.m_interval.x) + this.m_offset.x) * this.magnitude.x) * this.m_interval.x;
        vector.y = (Mathf.Sin((UnityEngine.Time.time * this.m_interval.y) + this.m_offset.y) * this.magnitude.y) * this.m_interval.y;
        vector.z = (Mathf.Sin((UnityEngine.Time.time * this.m_interval.z) + this.m_offset.z) * this.magnitude.z) * this.m_interval.z;
        if (this.localSpace)
        {
            Transform transform1 = base.transform;
            transform1.localPosition += vector;
        }
        else
        {
            Transform transform2 = base.transform;
            transform2.position += vector;
        }
        vector2.x = (Mathf.Sin((UnityEngine.Time.time * this.m_rotationInterval.x) + this.m_offset.x) * this.magnitudeRot.x) * this.m_rotationInterval.x;
        vector2.y = (Mathf.Sin((UnityEngine.Time.time * this.m_rotationInterval.y) + this.m_offset.y) * this.magnitudeRot.y) * this.m_rotationInterval.y;
        vector2.z = (Mathf.Sin((UnityEngine.Time.time * this.m_rotationInterval.z) + this.m_offset.z) * this.magnitudeRot.z) * this.m_rotationInterval.z;
        Transform transform = base.transform;
        transform.eulerAngles += vector2;
    }
}

