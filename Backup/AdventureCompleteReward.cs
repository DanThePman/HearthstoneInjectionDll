using System;
using UnityEngine;

[CustomEditClass]
public class AdventureCompleteReward : Reward
{
    [CustomEditField(Sections="Banner")]
    public GameObject m_BannerObject;
    [CustomEditField(Sections="Banner")]
    public Vector3_MobileOverride m_BannerScaleOverride;
    [CustomEditField(Sections="Banner")]
    public UberText m_BannerTextObject;
    [CustomEditField(Sections="State Event Table")]
    public StateEventTable m_StateTable;
    private const string s_EventHide = "Hide";
    private const string s_EventShowBadlyHurt = "ShowBadlyHurt";
    private const string s_EventShowHurt = "ShowHurt";

    private void DestroyThis()
    {
        UnityEngine.Object.DestroyImmediate(base.gameObject);
    }

    private void FadeFullscreenEffectsIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr == null)
        {
            Debug.LogWarning("AdventureCompleteReward: FullScreenFXMgr.Get() returned null!");
        }
        else
        {
            mgr.SetBlurBrightness(0.85f);
            mgr.SetBlurDesaturation(0f);
            mgr.Vignette(0.4f, 0.5f, iTween.EaseType.easeOutCirc, null);
            mgr.Blur(1f, 0.5f, iTween.EaseType.easeOutCirc, null);
        }
    }

    private void FadeFullscreenEffectsOut()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr == null)
        {
            Debug.LogWarning("AdventureCompleteReward: FullScreenFXMgr.Get() returned null!");
        }
        else
        {
            mgr.StopVignette(1f, iTween.EaseType.easeOutCirc, new FullScreenFXMgr.EffectListener(this.DestroyThis));
            mgr.StopBlur(1f, iTween.EaseType.easeOutCirc, null);
        }
    }

    protected override void HideReward()
    {
        if (base.IsShown)
        {
            base.HideReward();
            if (this.m_StateTable != null)
            {
                this.m_StateTable.TriggerState("Hide", true, null);
            }
            this.FadeFullscreenEffectsOut();
        }
    }

    protected override void InitData()
    {
        base.SetData(new AdventureCompleteRewardData(), false);
    }

    protected override void OnDataSet(bool updateVisuals)
    {
        if (updateVisuals)
        {
            if (!(base.Data is AdventureCompleteRewardData))
            {
                Debug.LogWarning(string.Format("AdventureCompleteReward.OnDataSet() - Data {0} is not AdventureCompleteRewardData", base.Data));
            }
            else
            {
                base.EnableClickCatcher(true);
                base.RegisterClickListener((reward, userData) => this.HideReward());
                base.SetReady(true);
            }
        }
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        if (!base.IsShown)
        {
            AdventureCompleteRewardData data = base.Data as AdventureCompleteRewardData;
            if (this.m_StateTable != null)
            {
                string eventName = (!data.IsBadlyHurt || !this.m_StateTable.HasState("ShowBadlyHurt")) ? "ShowHurt" : "ShowBadlyHurt";
                this.m_StateTable.TriggerState(eventName, true, null);
            }
            if (this.m_BannerTextObject != null)
            {
                this.m_BannerTextObject.Text = data.BannerText;
            }
            if ((this.m_BannerObject != null) && (this.m_BannerScaleOverride != null))
            {
                Vector3 bannerScaleOverride = (Vector3) this.m_BannerScaleOverride;
                if (bannerScaleOverride != Vector3.zero)
                {
                    this.m_BannerObject.transform.localScale = bannerScaleOverride;
                }
            }
            this.FadeFullscreenEffectsIn();
        }
    }
}

