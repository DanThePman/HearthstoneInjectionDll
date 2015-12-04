using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class CameraShakeMgr : MonoBehaviour
{
    private Vector3 m_amount;
    private float m_durationSec;
    private float? m_holdAtSec;
    private Vector3 m_initialPos;
    private AnimationCurve m_intensityCurve;
    private float m_progressSec;
    private bool m_started;

    private float ComputeIntensity()
    {
        return this.m_intensityCurve.Evaluate(this.m_progressSec);
    }

    private void DestroyShake()
    {
        base.transform.position = this.m_initialPos;
        UnityEngine.Object.Destroy(this);
    }

    private static bool DoesCurveHaveZeroTime(AnimationCurve intensityCurve)
    {
        if (intensityCurve == null)
        {
            return true;
        }
        if (intensityCurve.length == 0)
        {
            return true;
        }
        Keyframe keyframe = intensityCurve.keys[intensityCurve.length - 1];
        return (keyframe.time <= 0f);
    }

    private bool IsHolding()
    {
        if (!this.m_holdAtSec.HasValue)
        {
            return false;
        }
        float? holdAtSec = this.m_holdAtSec;
        return (holdAtSec.HasValue && (this.m_progressSec >= holdAtSec.Value));
    }

    public static bool IsShaking(Camera camera)
    {
        if (camera == null)
        {
            return false;
        }
        if (camera.GetComponent<CameraShakeMgr>() == null)
        {
            return false;
        }
        return true;
    }

    public static void Shake(Camera camera, Vector3 amount, float time)
    {
        AnimationCurve intensityCurve = AnimationCurve.Linear(0f, 1f, time, 0f);
        Shake(camera, amount, intensityCurve, null);
    }

    public static void Shake(Camera camera, Vector3 amount, AnimationCurve intensityCurve, float? holdAtTime = new float?())
    {
        if (camera != null)
        {
            CameraShakeMgr component = camera.GetComponent<CameraShakeMgr>();
            if (component != null)
            {
                if (DoesCurveHaveZeroTime(intensityCurve))
                {
                    component.DestroyShake();
                    return;
                }
            }
            else
            {
                if (DoesCurveHaveZeroTime(intensityCurve))
                {
                    return;
                }
                component = camera.gameObject.AddComponent<CameraShakeMgr>();
            }
            component.StartShake(amount, intensityCurve, holdAtTime);
        }
    }

    private void StartShake(Vector3 amount, AnimationCurve intensityCurve, float? holdAtSec = new float?())
    {
        this.m_amount = amount;
        this.m_intensityCurve = intensityCurve;
        this.m_holdAtSec = holdAtSec;
        if (!this.m_started)
        {
            this.m_started = true;
            this.m_initialPos = base.transform.position;
        }
        this.m_progressSec = 0f;
        Keyframe keyframe = intensityCurve.keys[intensityCurve.length - 1];
        this.m_durationSec = keyframe.time;
    }

    public static void Stop(Camera camera, float time = 0f)
    {
        if (camera != null)
        {
            CameraShakeMgr component = camera.GetComponent<CameraShakeMgr>();
            if (component != null)
            {
                if (time <= 0f)
                {
                    component.DestroyShake();
                }
                else
                {
                    float valueStart = component.ComputeIntensity();
                    AnimationCurve intensityCurve = AnimationCurve.Linear(0f, valueStart, time, 0f);
                    component.StartShake(component.m_amount, intensityCurve, null);
                }
            }
        }
    }

    private void Update()
    {
        if ((this.m_progressSec >= this.m_durationSec) && !this.IsHolding())
        {
            this.DestroyShake();
        }
        else
        {
            this.UpdateShake();
        }
    }

    private void UpdateShake()
    {
        float num = this.ComputeIntensity();
        Vector3 vector = new Vector3 {
            x = UnityEngine.Random.Range((float) (-this.m_amount.x * num), (float) (this.m_amount.x * num)),
            y = UnityEngine.Random.Range((float) (-this.m_amount.y * num), (float) (this.m_amount.y * num)),
            z = UnityEngine.Random.Range((float) (-this.m_amount.z * num), (float) (this.m_amount.z * num))
        };
        base.transform.position = this.m_initialPos + vector;
        if (!this.IsHolding())
        {
            this.m_progressSec = Mathf.Min(this.m_progressSec + UnityEngine.Time.deltaTime, this.m_durationSec);
        }
    }
}

