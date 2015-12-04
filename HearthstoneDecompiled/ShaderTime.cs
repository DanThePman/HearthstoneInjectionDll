using System;
using UnityEngine;

public class ShaderTime : MonoBehaviour
{
    private float m_maxTime = 999f;
    private float m_time;

    private void Awake()
    {
    }

    private void OnDestroy()
    {
        Shader.SetGlobalFloat("_ShaderTime", 0f);
    }

    private void Update()
    {
        this.UpdateShaderAnimationTime();
        this.UpdateGyro();
    }

    private void UpdateGyro()
    {
        Vector4 gravity = Input.gyro.gravity;
        Shader.SetGlobalVector("_Gyroscope", gravity);
    }

    private void UpdateShaderAnimationTime()
    {
        this.m_time += UnityEngine.Time.deltaTime / 20f;
        if (this.m_time > this.m_maxTime)
        {
            this.m_time -= this.m_maxTime;
            if (this.m_time <= 0f)
            {
                this.m_time = 0.0001f;
            }
        }
        Shader.SetGlobalFloat("_ShaderTime", this.m_time);
    }
}

