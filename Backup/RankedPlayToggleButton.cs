using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class RankedPlayToggleButton : PegUIElement
{
    public GameObject m_button;
    public Material m_buttonDisabledMaterial;
    public Material m_buttonDownMaterial;
    public Material m_buttonUpMaterial;
    public Transform m_downBone;
    public MeshRenderer m_glowQuad;
    public HighlightState m_highlight;
    public EnabledStateMaterialToggler[] m_materialTogglers;
    public Transform m_upBone;

    public void Down()
    {
        this.m_glowQuad.enabled = true;
        object[] args = new object[] { "position", this.m_downBone.localPosition, "isLocal", true, "time", 0.1, "easeType", iTween.EaseType.linear };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_button, hashtable);
        this.m_button.GetComponent<Renderer>().material = ((this.m_buttonDisabledMaterial == null) || base.IsEnabled()) ? this.m_buttonDownMaterial : this.m_buttonDisabledMaterial;
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        this.UpdateMaterialTogglers();
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        if (!this.m_glowQuad.enabled)
        {
            this.m_highlight.ChangeState(ActorStateType.NONE);
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over");
        if (!this.m_glowQuad.enabled)
        {
            this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
        }
    }

    public void Up()
    {
        this.m_glowQuad.enabled = false;
        object[] args = new object[] { "position", this.m_upBone.localPosition, "isLocal", true, "time", 0.1, "easeType", iTween.EaseType.linear };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_button, hashtable);
        this.m_button.GetComponent<Renderer>().material = ((this.m_buttonDisabledMaterial == null) || base.IsEnabled()) ? this.m_buttonUpMaterial : this.m_buttonDisabledMaterial;
        this.m_highlight.ChangeState(ActorStateType.NONE);
        this.UpdateMaterialTogglers();
    }

    private void UpdateMaterialTogglers()
    {
        foreach (EnabledStateMaterialToggler toggler in this.m_materialTogglers)
        {
            Renderer introduced3;
            introduced3.material = !base.IsEnabled() ? toggler.m_disabledMaterial : toggler.m_enabledMaterial;
        }
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct EnabledStateMaterialToggler
    {
        public MeshRenderer m_targetMesh;
        public Material m_enabledMaterial;
        public Material m_disabledMaterial;
    }
}

