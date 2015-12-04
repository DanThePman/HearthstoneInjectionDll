using System;
using UnityEngine;

public class TGTArrow : MonoBehaviour
{
    public GameObject m_ArrowMesh;
    public GameObject m_ArrowRoot;
    public ParticleSystem m_BullseyeParticles;
    public GameObject m_Trail;

    public void ArrowAnimation()
    {
        this.m_Trail.SetActive(true);
        this.m_Trail.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f, 0.15f));
        object[] args = new object[] { "color", Color.clear, "time", 0.1f, "oncomplete", "OnAnimationComplete" };
        iTween.ColorTo(this.m_Trail, iTween.Hash(args));
        Vector3 localPosition = this.m_ArrowRoot.transform.localPosition;
        object[] objArray2 = new object[] { "position", new Vector3(localPosition.x, localPosition.y, localPosition.z + 0.4f), "islocal", true, "time", 0.05f, "easetype", iTween.EaseType.easeOutQuart };
        iTween.MoveFrom(this.m_ArrowRoot, iTween.Hash(objArray2));
    }

    public void Bullseye()
    {
        this.m_BullseyeParticles.Play();
    }

    public void FireArrow(bool randomRotation)
    {
        if (randomRotation)
        {
            Vector3 localEulerAngles = this.m_ArrowMesh.transform.localEulerAngles;
            this.m_ArrowMesh.transform.localEulerAngles = new Vector3(localEulerAngles.x + UnityEngine.Random.Range((float) 0f, (float) 360f), localEulerAngles.y, localEulerAngles.z);
            this.m_ArrowRoot.transform.localEulerAngles = new Vector3(UnityEngine.Random.Range((float) 0f, (float) 20f), UnityEngine.Random.Range((float) 160f, (float) 180f), 0f);
        }
        this.ArrowAnimation();
    }

    public void OnAnimationComplete()
    {
        this.m_Trail.SetActive(false);
    }

    private void onEnable()
    {
        this.m_ArrowRoot.transform.localEulerAngles = new Vector3(0f, 170f, 0f);
    }
}

