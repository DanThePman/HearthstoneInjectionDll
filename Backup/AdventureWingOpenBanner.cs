using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class AdventureWingOpenBanner : MonoBehaviour
{
    private OnBannerHidden m_bannerHiddenCallback;
    public PegUIElement m_clickCatcher;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_hideSound;
    public float m_hideTime = 0.5f;
    private Vector3 m_originalScale;
    public GameObject m_root;
    public iTween.EaseType m_showEase = iTween.EaseType.easeOutElastic;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_showSound;
    public float m_showTime = 0.5f;
    public string m_VOQuoteLine;
    public Vector3 m_VOQuotePosition = new Vector3(0f, 0f, -55f);
    public string m_VOQuotePrefab;

    private void Awake()
    {
        if (this.m_clickCatcher != null)
        {
            this.m_clickCatcher.AddEventListener(UIEventType.RELEASE, e => this.HideBanner());
        }
        if (this.m_root != null)
        {
            this.m_root.SetActive(false);
        }
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, true, CanvasScaleMode.HEIGHT);
    }

    public void HideBanner()
    {
        if (this.m_root == null)
        {
            Debug.LogError("m_root not defined in banner!");
        }
        else
        {
            FullScreenFXMgr.Get().EndStandardBlurVignette(this.m_hideTime, null);
            this.m_root.transform.localScale = this.m_originalScale;
            object[] args = new object[] { "scale", new Vector3(0.01f, 0.01f, 0.01f), "oncomplete", delegate (object o) {
                UnityEngine.Object.Destroy(base.gameObject);
                if (this.m_bannerHiddenCallback != null)
                {
                    this.m_bannerHiddenCallback();
                }
            }, "time", this.m_hideTime };
            iTween.ScaleTo(this.m_root, iTween.Hash(args));
            if (!string.IsNullOrEmpty(this.m_hideSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_hideSound));
            }
        }
    }

    public void ShowBanner(OnBannerHidden onBannerHiddenCallback = null)
    {
        if (this.m_root == null)
        {
            Debug.LogError("m_root not defined in banner!");
        }
        else
        {
            this.m_bannerHiddenCallback = onBannerHiddenCallback;
            this.m_originalScale = this.m_root.transform.localScale;
            this.m_root.SetActive(true);
            object[] args = new object[] { "scale", new Vector3(0.01f, 0.01f, 0.01f), "time", this.m_showTime, "easetype", this.m_showEase };
            iTween.ScaleFrom(this.m_root, iTween.Hash(args));
            if (!string.IsNullOrEmpty(this.m_showSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_showSound));
            }
            if (!string.IsNullOrEmpty(this.m_VOQuotePrefab) && !string.IsNullOrEmpty(this.m_VOQuoteLine))
            {
                NotificationManager.Get().CreateCharacterQuote(FileUtils.GameAssetPathToName(this.m_VOQuotePrefab), this.m_VOQuotePosition, GameStrings.Get(this.m_VOQuoteLine), this.m_VOQuoteLine, true, 0f, null, CanvasAnchor.CENTER);
            }
            FullScreenFXMgr.Get().StartStandardBlurVignette(this.m_showTime);
        }
    }

    public delegate void OnBannerHidden();
}

