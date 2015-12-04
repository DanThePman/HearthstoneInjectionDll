using System;
using UnityEngine;

public class TransitionPulse : MonoBehaviour
{
    public float frequencyMax = 1f;
    public float frequencyMin = 0.0001f;
    private float m_interval;
    public float magnitude = 0.0001f;

    private void Start()
    {
        this.m_interval = UnityEngine.Random.Range(this.frequencyMin, this.frequencyMax);
    }

    private void Update()
    {
        float num = Mathf.Sin(UnityEngine.Time.time * this.m_interval) * this.magnitude;
        base.gameObject.GetComponent<Renderer>().material.SetFloat("_Transistion", num);
    }
}

