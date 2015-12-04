using System;
using UnityEngine;

public class BounceScale : MonoBehaviour
{
    public float m_Time;

    public void BounceyScale()
    {
        Vector3 localScale = base.transform.localScale;
        base.transform.localScale = Vector3.zero;
        object[] args = new object[] { "scale", localScale, "time", this.m_Time, "easetype", iTween.EaseType.easeOutElastic };
        iTween.ScaleTo(base.gameObject, iTween.Hash(args));
    }
}

