using System;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class AdventureBossCoin : PegUIElement
{
    public GameObject m_Coin;
    public StateEventTable m_CoinStateTable;
    public GameObject m_Connector;
    public PegUIElement m_DisabledCollider;
    private bool m_Enabled;
    public int m_PortraitMaterialIndex = 1;
    public MeshRenderer m_PortraitRenderer;
    private const string s_EventCoinFlip = "Flip";

    public void Enable(bool flag, bool animate = true)
    {
        base.GetComponent<Collider>().enabled = flag;
        if (this.m_DisabledCollider != null)
        {
            this.m_DisabledCollider.gameObject.SetActive(!flag);
        }
        if (this.m_Enabled != flag)
        {
            this.m_Enabled = flag;
            if (animate && flag)
            {
                this.ShowCoin(false);
                this.m_CoinStateTable.TriggerState("Flip", true, null);
            }
            else
            {
                this.ShowCoin(flag);
            }
        }
    }

    private void EnableFancyHighlight(bool enable)
    {
        UIBHighlightStateControl component = base.GetComponent<UIBHighlightStateControl>();
        if (component != null)
        {
            component.Select(enable, false);
        }
    }

    public void HighlightOnce()
    {
        UIBHighlight component = base.GetComponent<UIBHighlight>();
        if (component != null)
        {
            component.HighlightOnce();
        }
    }

    public void Select(bool selected)
    {
        UIBHighlight component = base.GetComponent<UIBHighlight>();
        if (component != null)
        {
            component.AlwaysOver = selected;
            if (selected)
            {
                this.EnableFancyHighlight(false);
            }
        }
    }

    public void SetPortraitMaterial(Material mat)
    {
        if ((this.m_PortraitRenderer != null) && (this.m_PortraitMaterialIndex < this.m_PortraitRenderer.materials.Length))
        {
            Material[] materials = this.m_PortraitRenderer.materials;
            materials[this.m_PortraitMaterialIndex] = mat;
            this.m_PortraitRenderer.materials = materials;
        }
    }

    private void ShowCoin(bool show)
    {
        if (this.m_Coin != null)
        {
            TransformUtil.SetEulerAngleZ(this.m_Coin, !show ? -180f : 0f);
        }
    }

    public void ShowConnector(bool show)
    {
        if (this.m_Connector != null)
        {
            this.m_Connector.SetActive(show);
        }
    }

    public void ShowNewLookGlow()
    {
        this.EnableFancyHighlight(true);
    }
}

