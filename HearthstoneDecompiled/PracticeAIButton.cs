using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PracticeAIButton : PegUIElement
{
    private readonly string FLIP_COROUTINE = "WaitThenFlip";
    private const float FLIPPED_X_ROTATION = 180f;
    private readonly Vector3 GLOW_QUAD_FLIPPED_LOCAL_POS = new Vector3(-0.1953466f, -1.336676f, 0.00721521f);
    private readonly Vector3 GLOW_QUAD_NORMAL_LOCAL_POS = new Vector3(-0.1953466f, 1.336676f, 0.00721521f);
    public GameObject m_backsideCover;
    public UberText m_backsideName;
    private TAG_CLASS m_class;
    private bool m_covered;
    public Transform m_coveredBone;
    private long m_deckID;
    public Transform m_downBone;
    public GameObject m_frontCover;
    public HighlightState m_highlight;
    private bool m_infoSet;
    private bool m_locked;
    private int m_missionID;
    public UberText m_name;
    public int m_PortraitMaterialIdx = -1;
    public GameObject m_questBang;
    public GameObject m_rootObject;
    public GameObject m_unlockEffect;
    public Transform m_upBone;
    private bool m_usingBackside;
    private const float NORMAL_X_ROTATION = 0f;

    public void CoverUp(bool flip)
    {
        this.m_covered = true;
        if (flip)
        {
            this.GetHiddenNameMesh().Text = string.Empty;
            this.GetHiddenCover().GetComponent<Renderer>().enabled = true;
            this.Flip();
        }
        else
        {
            this.GetShowingNameMesh().Text = string.Empty;
            this.GetShowingCover().GetComponent<Renderer>().enabled = true;
        }
        object[] args = new object[] { "position", this.m_coveredBone.localPosition, "time", 0.25f, "isLocal", true, "easeType", iTween.EaseType.linear };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_rootObject, hashtable);
        base.SetEnabled(false);
    }

    private void Depress()
    {
        object[] args = new object[] { "position", this.m_downBone.localPosition, "time", 0.1f, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_rootObject, hashtable);
    }

    public void Deselect()
    {
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
        if (!this.m_covered)
        {
            this.Raise();
            if (!this.m_locked)
            {
                base.SetEnabled(true);
            }
        }
    }

    private void Flip()
    {
        base.StopCoroutine(this.FLIP_COROUTINE);
        this.m_usingBackside = !this.m_usingBackside;
        base.StartCoroutine(this.FLIP_COROUTINE, this.m_usingBackside);
    }

    public TAG_CLASS GetClass()
    {
        return this.m_class;
    }

    public long GetDeckID()
    {
        return this.m_deckID;
    }

    private GameObject GetHiddenCover()
    {
        return (!this.m_usingBackside ? this.m_backsideCover : this.m_frontCover);
    }

    private Material GetHiddenMaterial()
    {
        int index = !this.m_usingBackside ? 2 : 1;
        return this.m_rootObject.GetComponent<Renderer>().materials[index];
    }

    private UberText GetHiddenNameMesh()
    {
        return (!this.m_usingBackside ? this.m_backsideName : this.m_name);
    }

    public int GetMissionID()
    {
        return this.m_missionID;
    }

    private GameObject GetShowingCover()
    {
        return (!this.m_usingBackside ? this.m_frontCover : this.m_backsideCover);
    }

    private Material GetShowingMaterial()
    {
        int index = !this.m_usingBackside ? 1 : 2;
        return this.m_rootObject.GetComponent<Renderer>().materials[index];
    }

    private UberText GetShowingNameMesh()
    {
        return (!this.m_usingBackside ? this.m_name : this.m_backsideName);
    }

    public void Lock(bool locked)
    {
        this.m_locked = locked;
        float num = !this.m_locked ? ((float) 0) : ((float) 1);
        bool enabled = !this.m_locked;
        base.SetEnabled(enabled);
        this.GetShowingMaterial().SetFloat("_Desaturate", num);
        this.m_rootObject.GetComponent<Renderer>().materials[0].SetFloat("_Desaturate", num);
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over", base.gameObject);
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
    }

    public void PlayUnlockGlow()
    {
        this.m_unlockEffect.GetComponent<Animation>().Play("AITileGlow");
    }

    public void Raise()
    {
        this.Raise(0.1f);
    }

    private void Raise(float time)
    {
        object[] args = new object[] { "position", this.m_upBone.localPosition, "time", time, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_rootObject, hashtable);
    }

    public void Select()
    {
        SoundManager.Get().LoadAndPlay("select_AI_opponent", base.gameObject);
        this.m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
        base.SetEnabled(false);
        this.Depress();
    }

    private void SetButtonClass(TAG_CLASS buttonClass)
    {
        this.m_class = buttonClass;
    }

    private void SetDeckID(long deckID)
    {
        this.m_deckID = deckID;
    }

    private void SetHiddenMaterial(Material mat)
    {
        int materialIndex = !this.m_usingBackside ? 2 : 1;
        RenderUtils.SetMaterial(this.m_rootObject.GetComponent<Renderer>(), materialIndex, mat);
    }

    public void SetInfo(string name, TAG_CLASS buttonClass, CardDef cardDef, int missionID, bool flip)
    {
        this.SetInfo(name, buttonClass, cardDef, missionID, 0L, flip);
    }

    public void SetInfo(string name, TAG_CLASS buttonClass, CardDef cardDef, long deckID, bool flip)
    {
        this.SetInfo(name, buttonClass, cardDef, 0, deckID, flip);
    }

    private void SetInfo(string name, TAG_CLASS buttonClass, CardDef cardDef, int missionID, long deckID, bool flip)
    {
        this.SetMissionID(missionID);
        this.SetDeckID(deckID);
        this.SetButtonClass(buttonClass);
        Material practiceAIPortrait = cardDef.GetPracticeAIPortrait();
        if (flip)
        {
            this.GetHiddenNameMesh().Text = name;
            if (practiceAIPortrait != null)
            {
                this.SetHiddenMaterial(practiceAIPortrait);
            }
            this.Flip();
        }
        else
        {
            if (this.m_infoSet)
            {
                UnityEngine.Debug.LogWarning("PracticeAIButton.SetInfo() - button is being re-initialized!");
            }
            this.m_infoSet = true;
            if (practiceAIPortrait != null)
            {
                this.SetShowingMaterial(practiceAIPortrait);
            }
            this.GetShowingNameMesh().Text = name;
            base.SetOriginalLocalPosition();
        }
        this.m_covered = false;
        this.GetShowingCover().GetComponent<Renderer>().enabled = false;
    }

    private void SetMissionID(int missionID)
    {
        this.m_missionID = missionID;
    }

    private void SetShowingMaterial(Material mat)
    {
        int materialIndex = !this.m_usingBackside ? 1 : 2;
        RenderUtils.SetMaterial(this.m_rootObject.GetComponent<Renderer>(), materialIndex, mat);
    }

    public void ShowQuestBang(bool shown)
    {
        this.m_questBang.SetActive(shown);
    }

    [DebuggerHidden]
    private IEnumerator WaitThenFlip(bool flipToBackside)
    {
        return new <WaitThenFlip>c__Iterator1B0 { flipToBackside = flipToBackside, <$>flipToBackside = flipToBackside, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitThenFlip>c__Iterator1B0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>flipToBackside;
        internal PracticeAIButton <>f__this;
        internal Hashtable <args>__1;
        internal float <highlightTargetXRotation>__2;
        internal float <startXRotation>__0;
        internal bool flipToBackside;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    iTween.StopByName(this.<>f__this.gameObject, "flip");
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    return true;

                case 1:
                {
                    this.<startXRotation>__0 = !this.flipToBackside ? 180f : 0f;
                    this.<>f__this.m_rootObject.transform.localEulerAngles = new Vector3(this.<startXRotation>__0, 0f, 0f);
                    object[] args = new object[] { "amount", new Vector3(180f, 0f, 0f), "time", 0.25f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self, "name", "flip" };
                    this.<args>__1 = iTween.Hash(args);
                    iTween.RotateAdd(this.<>f__this.m_rootObject, this.<args>__1);
                    this.<highlightTargetXRotation>__2 = !this.flipToBackside ? 0f : 180f;
                    this.<>f__this.m_highlight.transform.localEulerAngles = new Vector3(this.<highlightTargetXRotation>__2, 0f, 0f);
                    this.<>f__this.m_unlockEffect.transform.localPosition = !this.flipToBackside ? this.<>f__this.GLOW_QUAD_NORMAL_LOCAL_POS : this.<>f__this.GLOW_QUAD_FLIPPED_LOCAL_POS;
                    this.$PC = -1;
                    break;
                }
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

