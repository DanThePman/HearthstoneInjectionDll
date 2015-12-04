using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class CardBackInfoManager : MonoBehaviour
{
    private bool m_animating;
    public float m_animationTime = 0.5f;
    public GameObject m_cardBackContainer;
    private GameObject m_currentCardBack;
    private int m_currentCardBackIdx;
    public UberText m_description;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_enterPreviewSound;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_exitPreviewSound;
    public UIBButton m_favoriteButton;
    public PegUIElement m_offClicker;
    public GameObject m_previewPane;
    public UberText m_title;
    private static CardBackInfoManager s_instance;

    private void Awake()
    {
        s_instance = this;
        this.m_previewPane.SetActive(false);
        this.SetupUI();
    }

    public void CancelPreview()
    {
        <CancelPreview>c__AnonStorey2C0 storeyc = new <CancelPreview>c__AnonStorey2C0 {
            <>f__this = this
        };
        if (!this.m_animating)
        {
            storeyc.origScale = this.m_previewPane.transform.localScale;
            this.m_animating = true;
            object[] args = new object[] { "scale", new Vector3(0.01f, 0.01f, 0.01f), "time", this.m_animationTime, "easeType", iTween.EaseType.easeOutCirc, "oncomplete", new Action<object>(storeyc.<>m__81) };
            iTween.ScaleTo(this.m_previewPane, iTween.Hash(args));
            if (!string.IsNullOrEmpty(this.m_exitPreviewSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_exitPreviewSound));
            }
            FullScreenFXMgr.Get().EndStandardBlurVignette(this.m_animationTime, null);
        }
    }

    public void EnterPreview(CollectionCardVisual cardVisual)
    {
        Actor actor = cardVisual.GetActor();
        if (actor == null)
        {
            Debug.LogError("Unable to obtain actor from card visual.");
        }
        else
        {
            CollectionCardBack component = actor.GetComponent<CollectionCardBack>();
            if (component == null)
            {
                Debug.LogError("Actor does not contain a CollectionCardBack component!");
            }
            else
            {
                this.EnterPreview(component.GetCardBackId());
            }
        }
    }

    public void EnterPreview(int cardBackIdx)
    {
        <EnterPreview>c__AnonStorey2BF storeybf = new <EnterPreview>c__AnonStorey2BF {
            cardBackIdx = cardBackIdx,
            <>f__this = this
        };
        if (!this.m_animating)
        {
            if (this.m_currentCardBack != null)
            {
                UnityEngine.Object.Destroy(this.m_currentCardBack);
                this.m_currentCardBack = null;
            }
            this.m_animating = true;
            DbfRecord record = GameDbf.CardBack.GetRecord(storeybf.cardBackIdx);
            this.m_title.Text = record.GetLocString("NAME");
            this.m_description.Text = record.GetLocString("DESCRIPTION");
            bool enabled = false;
            if (!CollectionManager.Get().IsInEditMode())
            {
                int defaultCardBack = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>().DefaultCardBack;
                enabled = CardBackManager.Get().IsCardBackOwned(storeybf.cardBackIdx) && (defaultCardBack != storeybf.cardBackIdx);
            }
            this.m_favoriteButton.SetEnabled(enabled);
            this.m_favoriteButton.Flip(enabled);
            this.m_currentCardBackIdx = storeybf.cardBackIdx;
            if (!CardBackManager.Get().LoadCardBackByIndex(storeybf.cardBackIdx, new CardBackManager.LoadCardBackData.LoadCardBackCallback(storeybf.<>m__80), "Card_Hidden"))
            {
                Debug.LogError(string.Format("Unable to load card back ID {0} for preview.", storeybf.cardBackIdx));
                this.m_animating = false;
            }
            if (!string.IsNullOrEmpty(this.m_enterPreviewSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_enterPreviewSound));
            }
            FullScreenFXMgr.Get().StartStandardBlurVignette(this.m_animationTime);
        }
    }

    public static CardBackInfoManager Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void SetFavorite()
    {
        NetCache.NetCacheCardBacks netObject = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
        if (this.m_currentCardBackIdx != netObject.DefaultCardBack)
        {
            ConnectAPI.SetDefaultCardBack(this.m_currentCardBackIdx);
        }
    }

    private void SetupUI()
    {
        this.m_favoriteButton.AddEventListener(UIEventType.RELEASE, delegate (UIEvent e) {
            this.SetFavorite();
            this.CancelPreview();
        });
        this.m_offClicker.AddEventListener(UIEventType.RELEASE, e => this.CancelPreview());
        this.m_offClicker.AddEventListener(UIEventType.RIGHTCLICK, e => this.CancelPreview());
    }

    [CompilerGenerated]
    private sealed class <CancelPreview>c__AnonStorey2C0
    {
        internal CardBackInfoManager <>f__this;
        internal Vector3 origScale;

        internal void <>m__81(object e)
        {
            this.<>f__this.m_animating = false;
            this.<>f__this.m_previewPane.transform.localScale = this.origScale;
            this.<>f__this.m_previewPane.SetActive(false);
            this.<>f__this.m_offClicker.gameObject.SetActive(false);
        }
    }

    [CompilerGenerated]
    private sealed class <EnterPreview>c__AnonStorey2BF
    {
        internal CardBackInfoManager <>f__this;
        internal int cardBackIdx;

        internal void <>m__80(CardBackManager.LoadCardBackData cardBackData)
        {
            GameObject gameObject = cardBackData.m_GameObject;
            gameObject.name = "CARD_BACK_" + this.cardBackIdx;
            SceneUtils.SetLayer(gameObject, this.<>f__this.m_cardBackContainer.gameObject.layer);
            GameUtils.SetParent(gameObject, this.<>f__this.m_cardBackContainer, false);
            this.<>f__this.m_currentCardBack = gameObject;
            this.<>f__this.m_currentCardBack.transform.localPosition = Vector3.zero;
            this.<>f__this.m_currentCardBack.transform.localScale = Vector3.one;
            this.<>f__this.m_currentCardBack.transform.localRotation = Quaternion.identity;
            this.<>f__this.m_previewPane.SetActive(true);
            this.<>f__this.m_offClicker.gameObject.SetActive(true);
            object[] args = new object[] { "scale", new Vector3(0.01f, 0.01f, 0.01f), "time", this.<>f__this.m_animationTime, "easeType", iTween.EaseType.easeOutCirc, "oncomplete", e => this.<>f__this.m_animating = false };
            iTween.ScaleFrom(this.<>f__this.m_previewPane, iTween.Hash(args));
        }

        internal void <>m__85(object e)
        {
            this.<>f__this.m_animating = false;
        }
    }
}

