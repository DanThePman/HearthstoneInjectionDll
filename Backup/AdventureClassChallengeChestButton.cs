using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AdventureClassChallengeChestButton : PegUIElement
{
    public Transform m_DownBone;
    public GameObject m_HighlightPlane;
    public bool m_IsRewardLoading;
    public GameObject m_RewardBone;
    public GameObject m_RewardCard;
    public GameObject m_RootObject;
    public Transform m_UpBone;

    private void Depress()
    {
        object[] args = new object[] { "position", this.m_DownBone.localPosition, "time", 0.1f, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_RootObject, hashtable);
    }

    private void EffectFadeOutFinished()
    {
        SceneUtils.SetLayer(base.gameObject, GameLayer.Default);
        if (this.m_RewardCard != null)
        {
            this.m_RewardCard.SetActive(false);
        }
    }

    private void HideRewardCard()
    {
        iTween.ScaleTo(this.m_RewardBone, new Vector3(0.1f, 0.1f, 0.1f), 0.2f);
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, null);
        mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, new FullScreenFXMgr.EffectListener(this.EffectFadeOutFinished));
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.ShowHighlight(false);
        this.HideRewardCard();
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over", base.gameObject);
        this.ShowHighlight(true);
        base.StartCoroutine(this.ShowRewardCard());
    }

    public void Press()
    {
        SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over", base.gameObject);
        this.Depress();
        this.ShowHighlight(true);
        base.StartCoroutine(this.ShowRewardCard());
    }

    private void Raise()
    {
        object[] args = new object[] { "position", this.m_UpBone.localPosition, "time", 0.1f, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(this.m_RootObject, hashtable);
    }

    public void Release()
    {
        this.Raise();
        this.ShowHighlight(false);
        this.HideRewardCard();
    }

    private void ShowHighlight(bool show)
    {
        this.m_HighlightPlane.GetComponent<Renderer>().enabled = show;
    }

    [DebuggerHidden]
    private IEnumerator ShowRewardCard()
    {
        return new <ShowRewardCard>c__Iterator2 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <ShowRewardCard>c__Iterator2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AdventureClassChallengeChestButton <>f__this;
        internal FullScreenFXMgr <fx>__0;

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
                case 1:
                    if (this.<>f__this.m_IsRewardLoading)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    SceneUtils.SetLayer(this.<>f__this.gameObject, GameLayer.IgnoreFullScreenEffects);
                    this.<fx>__0 = FullScreenFXMgr.Get();
                    this.<fx>__0.SetBlurBrightness(1f);
                    this.<fx>__0.SetBlurDesaturation(0f);
                    this.<fx>__0.Vignette(0.4f, 0.2f, iTween.EaseType.easeOutCirc, null);
                    this.<fx>__0.Blur(1f, 0.2f, iTween.EaseType.easeOutCirc, null);
                    this.<>f__this.m_RewardBone.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    iTween.ScaleTo(this.<>f__this.m_RewardBone, new Vector3(10f, 10f, 10f), 0.2f);
                    this.<>f__this.m_RewardCard.SetActive(true);
                    this.$PC = -1;
                    break;
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

