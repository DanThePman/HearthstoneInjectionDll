using System;
using UnityEngine;

public class StageTester : MonoBehaviour
{
    public GameObject entireObj;
    public GameObject flash;
    public GameObject fxEmitterA;
    public GameObject fxEmitterB;
    public GameObject highlightBase;
    public GameObject highlightEdge;
    public GameObject inplayObj;
    public GameObject rays;
    private int stage;

    private void Highlighted()
    {
        this.highlightBase.GetComponent<Animation>().Play();
        this.highlightEdge.GetComponent<Animation>().Play();
    }

    private void ManaUsed()
    {
        this.highlightBase.GetComponent<Animation>().CrossFade("AllyInHandActiveBaseMana", 0.3f);
        this.fxEmitterA.GetComponent<Animation>().CrossFade("AllyInHandFXUnHighlight", 0.3f);
    }

    private void OnMouseDown()
    {
        switch (this.stage)
        {
            case 0:
                this.Highlighted();
                break;

            case 1:
                this.Selected();
                break;

            case 2:
                this.ManaUsed();
                break;

            case 3:
                this.Released();
                break;
        }
        this.stage++;
    }

    public void PlayEmitterB()
    {
        this.fxEmitterB.GetComponent<ParticleEmitter>().emit = true;
    }

    private void Released()
    {
        this.rays.GetComponent<Animation>().Play("AllyInHandRaysUp");
        this.flash.GetComponent<Animation>().Play("AllyInHandGlowOn");
        this.entireObj.GetComponent<Animation>().Play("AllyInHandDeath");
        this.inplayObj.GetComponent<Animation>().Play("AllyInPlaySpawn");
    }

    private void Selected()
    {
        this.highlightBase.GetComponent<Animation>().CrossFade("AllyInHandActiveBaseSelected", 0.3f);
        this.fxEmitterA.GetComponent<Animation>().Play();
    }

    private void Start()
    {
    }

    private void Update()
    {
    }
}

