using System;
using UnityEngine;

public class BoxSpinner : MonoBehaviour
{
    private BoxSpinnerStateInfo m_info;
    private Box m_parent;
    private Material m_spinnerMat;
    private bool m_spinning;
    private float m_spinY;

    private void Awake()
    {
        this.m_spinnerMat = base.GetComponent<Renderer>().material;
    }

    public BoxSpinnerStateInfo GetInfo()
    {
        return this.m_info;
    }

    public Box GetParent()
    {
        return this.m_parent;
    }

    public bool IsSpinning()
    {
        return this.m_spinning;
    }

    public void Reset()
    {
        this.m_spinning = false;
        this.m_spinnerMat.SetFloat("_RotAngle", 0f);
    }

    public void SetInfo(BoxSpinnerStateInfo info)
    {
        this.m_info = info;
    }

    public void SetParent(Box parent)
    {
        this.m_parent = parent;
    }

    public void Spin()
    {
        this.m_spinning = true;
    }

    public void Stop()
    {
        this.m_spinning = false;
    }

    private void Update()
    {
        if (this.IsSpinning())
        {
            this.m_spinnerMat.SetFloat("_RotAngle", this.m_spinY);
            this.m_spinY += (this.m_info.m_DegreesPerSec * UnityEngine.Time.deltaTime) * 0.01f;
        }
    }
}

