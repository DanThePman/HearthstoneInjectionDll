using System;
using UnityEngine;

public class AspectRatioDependentPosition : MonoBehaviour
{
    public Vector3 m_minLocalPosition;

    private void Awake()
    {
        base.transform.localPosition = TransformUtil.GetAspectRatioDependentPosition(this.m_minLocalPosition, base.transform.localPosition);
    }
}

