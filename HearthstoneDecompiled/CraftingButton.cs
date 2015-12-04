using System;
using UnityEngine;

public class CraftingButton : PegUIElement
{
    public MeshRenderer buttonRenderer;
    public Material disabledMaterial;
    public Material enabledMaterial;
    private bool isEnabled;
    public UberText labelText;
    public GameObject m_costObject;
    public Transform m_disabledCostBone;
    public Transform m_enabledCostBone;
    public Material undoMaterial;

    public virtual void DisableButton()
    {
        this.OnEnabled(false);
        this.buttonRenderer.material = this.disabledMaterial;
        this.labelText.Text = string.Empty;
    }

    public virtual void EnableButton()
    {
        this.OnEnabled(true);
        this.buttonRenderer.material = this.enabledMaterial;
    }

    public virtual void EnterUndoMode()
    {
        this.OnEnabled(true);
        this.buttonRenderer.material = this.undoMaterial;
        this.labelText.Text = GameStrings.Get("GLUE_CRAFTING_UNDO");
    }

    public bool IsButtonEnabled()
    {
        return this.isEnabled;
    }

    private void OnEnabled(bool enable)
    {
        this.isEnabled = enable;
        base.GetComponent<Collider>().enabled = enable;
        if (this.m_costObject != null)
        {
            if ((this.m_enabledCostBone != null) && (this.m_disabledCostBone != null))
            {
                this.m_costObject.transform.position = !enable ? this.m_disabledCostBone.position : this.m_enabledCostBone.position;
            }
            else
            {
                this.m_costObject.SetActive(enable);
            }
        }
    }
}

