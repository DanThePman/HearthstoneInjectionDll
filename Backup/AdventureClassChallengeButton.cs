using System;
using System.Collections;
using UnityEngine;

public class AdventureClassChallengeButton : PegUIElement
{
    public GameObject m_Checkmark;
    public GameObject m_Chest;
    public Transform m_DownBone;
    public HighlightState m_Highlight;
    public GameObject m_RootObject;
    public int m_ScenarioID;
    public UberText m_Text;
    public Transform m_UpBone;

    private void Depress()
    {
        object[] args = new object[] { "position", this.m_DownBone.localPosition, "time", 0.1f, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_RootObject, hashtable);
    }

    public void Deselect()
    {
        this.m_Highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
        this.Raise(0.1f);
        base.SetEnabled(true);
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_Highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over", base.gameObject);
        this.m_Highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
    }

    private void Raise(float time)
    {
        object[] args = new object[] { "position", this.m_UpBone.localPosition, "time", time, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_RootObject, hashtable);
    }

    public void Select(bool playSound)
    {
        if (playSound)
        {
            SoundManager.Get().LoadAndPlay("select_AI_opponent", base.gameObject);
        }
        this.m_Highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        base.SetEnabled(false);
        this.Depress();
    }

    public void SetPortraitMaterial(Material portraitMat)
    {
        Renderer component = this.m_RootObject.GetComponent<Renderer>();
        Material[] materials = component.materials;
        materials[1] = portraitMat;
        component.materials = materials;
    }
}

