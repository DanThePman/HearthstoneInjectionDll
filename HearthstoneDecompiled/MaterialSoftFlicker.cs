using System;
using UnityEngine;

[RequireComponent(typeof(Material))]
public class MaterialSoftFlicker : MonoBehaviour
{
    public Color m_color = new Color(1f, 1f, 1f, 1f);
    public float m_timeScale = 1f;
    public float maxIntensity = 0.5f;
    public float minIntensity = 0.25f;
    private float random;

    private void Start()
    {
        this.random = UnityEngine.Random.Range((float) 0f, (float) 65535f);
    }

    private void Update()
    {
        float t = Mathf.PerlinNoise(this.random, UnityEngine.Time.time * this.m_timeScale);
        base.gameObject.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(this.m_color.r, this.m_color.g, this.m_color.b, Mathf.Lerp(this.minIntensity, this.maxIntensity, t)));
    }
}

