using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MobileTransform : MonoBehaviour
{
    public List<bool> m_active = new List<bool>();
    public List<Vector3> m_localPosition = new List<Vector3>();
    public List<Vector3> m_localScale = new List<Vector3>();
    public List<ScreenCategory> m_screenCategory = new List<ScreenCategory>();

    public void AddCategory()
    {
        if (!Application.isPlaying)
        {
            this.m_screenCategory.Add(PlatformSettings.Screen);
            this.m_localPosition.Add(base.transform.localPosition);
            this.m_localScale.Add(base.transform.localScale);
            this.m_active.Add(true);
        }
    }

    public void Awake()
    {
        while (this.m_active.Count < this.m_screenCategory.Count)
        {
            this.m_active.Add(true);
        }
        if (Application.isPlaying)
        {
            this.UpdateObject();
        }
        else if (this.m_screenCategory.Count == 0)
        {
            this.AddCategory();
        }
    }

    public void UpdateObject()
    {
        for (int i = 0; i < this.m_screenCategory.Count; i++)
        {
            if (PlatformSettings.Screen == ((ScreenCategory) this.m_screenCategory[i]))
            {
                base.transform.localPosition = this.m_localPosition[i];
                base.transform.localScale = this.m_localScale[i];
                if (Application.isPlaying)
                {
                    base.gameObject.SetActive(this.m_active[i]);
                }
                break;
            }
        }
    }
}

