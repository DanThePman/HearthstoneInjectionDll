using System;
using UnityEngine;

public class ArcEndSphere : MonoBehaviour
{
    private void Start()
    {
    }

    private void Update()
    {
        Vector2 mainTextureOffset = base.GetComponent<Renderer>().material.mainTextureOffset;
        mainTextureOffset.x += UnityEngine.Time.deltaTime * 1f;
        base.GetComponent<Renderer>().material.mainTextureOffset = mainTextureOffset;
    }
}

