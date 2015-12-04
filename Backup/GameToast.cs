using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameToast : MonoBehaviour
{
    public List<Material> m_intensityMaterials = new List<Material>();

    private void Start()
    {
        this.UpdateIntensity(16f);
        object[] args = new object[] { "time", 0.5f, "from", 16f, "to", 1f, "delay", 0.25f, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateIntensity" };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(base.gameObject, hashtable);
    }

    private void UpdateIntensity(float intensity)
    {
        foreach (Material material in this.m_intensityMaterials)
        {
            material.SetFloat("_Intensity", intensity);
        }
    }
}

