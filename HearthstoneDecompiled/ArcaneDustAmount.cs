using System;
using UnityEngine;

public class ArcaneDustAmount : MonoBehaviour
{
    public UberText m_dustCount;
    public GameObject m_dustFX;
    public GameObject m_dustJar;
    public GameObject m_explodeFX_Common;
    public GameObject m_explodeFX_Epic;
    public GameObject m_explodeFX_Legendary;
    public GameObject m_explodeFX_Rare;
    private static ArcaneDustAmount s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public static ArcaneDustAmount Get()
    {
        return s_instance;
    }

    private void Start()
    {
        this.UpdateCurrentDustAmount();
    }

    public void UpdateCurrentDustAmount()
    {
        this.m_dustCount.Text = CraftingManager.Get().GetLocalArcaneDustBalance().ToString();
    }
}

