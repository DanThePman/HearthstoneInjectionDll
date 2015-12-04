using System;
using UnityEngine;

public class CollectionSetFilterDropdownItem : PegUIElement
{
    public UberText m_dropdownText;
    private Vector2? m_iconMaterialOffset;
    public MeshRenderer m_iconRenderer;
    public GameObject m_mouseOverBar;
    public Color m_mouseOverColor;
    private bool m_selected;
    public GameObject m_selectedBar;
    public Color m_selectedColor;
    public Color m_unselectedColor;

    public void DisableIconMaterial()
    {
        this.m_iconMaterialOffset = null;
        this.m_iconRenderer.material.SetTextureScale("_MainTex", Vector2.zero);
    }

    public Vector2? GetIconMaterialOffset()
    {
        return this.m_iconMaterialOffset;
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_mouseOverBar.SetActive(false);
        this.SetItemColors(!this.m_selected ? this.m_unselectedColor : this.m_selectedColor);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        if (!this.m_selected)
        {
            this.SetItemColors(this.m_mouseOverColor);
            this.m_mouseOverBar.SetActive(true);
        }
    }

    public void Select(bool selection)
    {
        this.m_selected = selection;
        this.SetItemColors(!this.m_selected ? this.m_unselectedColor : this.m_selectedColor);
        this.m_selectedBar.SetActive(selection);
        if (this.m_selected)
        {
            this.m_mouseOverBar.SetActive(false);
        }
    }

    public void SetIconMaterialOffset(Vector2 offset)
    {
        this.m_iconMaterialOffset = new Vector2?(offset);
        this.m_iconRenderer.material.SetTextureOffset("_MainTex", offset);
    }

    private void SetItemColors(Color color)
    {
        this.m_iconRenderer.material.SetColor("_Color", color);
        this.m_dropdownText.TextColor = color;
    }

    public void SetName(string name)
    {
        this.m_dropdownText.Text = name;
    }
}

