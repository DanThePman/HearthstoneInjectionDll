using System;
using UnityEngine;

public class BoardStandardGame : MonoBehaviour
{
    public Transform m_BoneParent;
    public Transform m_ColliderParent;
    public GameObject[] m_DeckGameObjects;
    private static BoardStandardGame s_instance;

    private void Awake()
    {
        s_instance = this;
        if (LoadingScreen.Get() != null)
        {
            LoadingScreen.Get().NotifyMainSceneObjectAwoke(base.gameObject);
        }
    }

    public void DeckColors()
    {
        foreach (GameObject obj2 in this.m_DeckGameObjects)
        {
            obj2.GetComponent<Renderer>().material.color = Board.Get().m_DeckColor;
        }
    }

    public Transform FindBone(string name)
    {
        return this.m_BoneParent.Find(name);
    }

    public Collider FindCollider(string name)
    {
        Transform transform = this.m_ColliderParent.Find(name);
        return ((transform != null) ? transform.GetComponent<Collider>() : null);
    }

    public static BoardStandardGame Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void Start()
    {
        this.DeckColors();
    }
}

