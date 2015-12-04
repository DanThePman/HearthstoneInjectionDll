using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTransitions : MonoBehaviour
{
    public float amount;
    public List<GameObject> m_TargetList;
    private List<Renderer> rend;

    private void Start()
    {
        this.rend = new List<Renderer>();
        foreach (GameObject obj2 in this.m_TargetList)
        {
            if (obj2 != null)
            {
                this.rend.Add(obj2.GetComponent<Renderer>());
            }
        }
    }

    public void StartTransitions()
    {
        foreach (Renderer renderer in this.rend)
        {
            renderer.material.SetFloat("_Transistion", this.amount);
        }
    }

    private void Update()
    {
        this.StartTransitions();
    }
}

