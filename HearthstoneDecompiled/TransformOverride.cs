using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TransformOverride : MonoBehaviour
{
    public List<Vector3> m_localPosition = new List<Vector3>();
    public List<Quaternion> m_localRotation = new List<Quaternion>();
    public List<Vector3> m_localScale = new List<Vector3>();
    public List<ScreenCategory> m_screenCategory = new List<ScreenCategory>();
    public float testVal;

    public void AddCategory()
    {
        this.AddCategory(PlatformSettings.Screen);
    }

    public void AddCategory(ScreenCategory screen)
    {
        if (!Application.isPlaying)
        {
            this.m_screenCategory.Add(screen);
            this.m_localPosition.Add(base.transform.localPosition);
            this.m_localScale.Add(base.transform.localScale);
            this.m_localRotation.Add(base.transform.localRotation);
        }
    }

    public void Awake()
    {
        if (Application.isPlaying)
        {
            this.UpdateObject();
        }
    }

    public int GetIndex()
    {
        ScreenCategory screen = PlatformSettings.Screen;
        int num = 0;
        int num2 = 4 - screen;
        for (int i = 1; i < this.m_screenCategory.Count; i++)
        {
            int num4 = ((ScreenCategory) this.m_screenCategory[i]) - screen;
            if ((num4 >= 0) && (num4 < num2))
            {
                num = i;
                num2 = num4;
            }
        }
        return num;
    }

    public void UpdateObject()
    {
        int index = this.GetIndex();
        base.transform.localPosition = this.m_localPosition[index];
        base.transform.localScale = this.m_localScale[index];
        base.transform.localRotation = this.m_localRotation[index];
    }
}

