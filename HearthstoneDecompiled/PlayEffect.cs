using System;
using UnityEngine;

public class PlayEffect : MonoBehaviour
{
    public GameObject fxEmitter1;

    public void PlayEmitter1()
    {
        this.fxEmitter1.GetComponent<ParticleEmitter>().emit = true;
    }

    private void Start()
    {
    }

    public void StopEmitter1()
    {
        this.fxEmitter1.GetComponent<ParticleEmitter>().emit = false;
    }

    private void Update()
    {
    }
}

