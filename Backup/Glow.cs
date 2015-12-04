using System;
using UnityEngine;

public class Glow : MonoBehaviour
{
    public void UpdateAlpha(float alpha)
    {
        base.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 1f, 1f, alpha));
    }
}

