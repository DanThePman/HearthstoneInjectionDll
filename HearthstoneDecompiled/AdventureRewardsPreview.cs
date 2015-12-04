using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureRewardsPreview : MonoBehaviour
{
    [CompilerGenerated]
    private static UIEvent.Handler <>f__am$cache13;
    [CustomEditField(Sections="Cards Preview")]
    public PegUIElement m_BackButton;
    private List<List<Actor>> m_CardBatches = new List<List<Actor>>();
    [SerializeField]
    private float m_CardClumpAngleIncrement = 10f;
    [SerializeField]
    private Vector3 m_CardClumpSpacing = Vector3.zero;
    [CustomEditField(Sections="Cards Preview")]
    public GameObject m_CardsContainer;
    [SerializeField]
    private float m_CardSpacing = 5f;
    [CustomEditField(Sections="Cards Preview", Parent="m_PreviewCardsExpandable")]
    public AdventureRewardsDisplayArea m_CardsPreviewDisplay;
    [SerializeField]
    private float m_CardWidth = 30f;
    [CustomEditField(Sections="Cards Preview")]
    public GameObject m_ClickBlocker;
    [CustomEditField(Sections="Cards Preview")]
    public UIBScrollable m_DisableScrollbar;
    [CustomEditField(Sections="Cards Preview")]
    public UberText m_HeaderTextObject;
    private int m_HiddenCardCount;
    [CustomEditField(Sections="Cards Preview/Hidden Cards")]
    public UberText m_HiddenCardsLabel;
    [CustomEditField(Sections="Cards Preview/Hidden Cards")]
    public GameObject m_HiddenCardsLabelObject;
    private List<OnHide> m_OnHideListeners = new List<OnHide>();
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_PreviewAppearSound;
    [CustomEditField(Sections="Cards Preview")]
    public bool m_PreviewCardsExpandable;
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_PreviewShrinkSound;
    [CustomEditField(Sections="Cards Preview")]
    public float m_ShowHideAnimationTime = 0.15f;

    public void AddCardBatch(List<CardRewardData> rewards)
    {
        List<string> cardIds = new List<string>();
        foreach (CardRewardData data in rewards)
        {
            cardIds.Add(data.CardID);
        }
        this.AddCardBatch(cardIds);
    }

    public void AddCardBatch(List<string> cardIds)
    {
        List<Actor> list;
        list = new List<Actor> {
            list
        };
        this.AddCardBatch(cardIds, list);
    }

    public void AddCardBatch(int scenarioId)
    {
        List<CardRewardData> immediateCardRewardsForDefeatingScenario = AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario(scenarioId);
        this.AddCardBatch(immediateCardRewardsForDefeatingScenario);
    }

    private void AddCardBatch(List<string> cardIds, List<Actor> cardBatch)
    {
        if ((cardIds != null) && (cardIds.Count != 0))
        {
            <AddCardBatch>c__AnonStorey2A2 storeya = new <AddCardBatch>c__AnonStorey2A2 {
                <>f__this = this
            };
            using (List<string>.Enumerator enumerator = cardIds.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    storeya.cardId = enumerator.Current;
                    <AddCardBatch>c__AnonStorey2A3 storeya2 = new <AddCardBatch>c__AnonStorey2A3 {
                        <>f__ref$674 = storeya,
                        <>f__this = this
                    };
                    FullDef fullDef = DefLoader.Get().GetFullDef(storeya.cardId, null);
                    storeya2.actor = AssetLoader.Get().LoadActor(ActorNames.GetHandActor(fullDef.GetEntityDef(), TAG_PREMIUM.NORMAL), false, false).GetComponent<Actor>();
                    storeya2.actor.SetCardDef(fullDef.GetCardDef());
                    storeya2.actor.SetEntityDef(fullDef.GetEntityDef());
                    GameUtils.SetParent(storeya2.actor, this.m_CardsContainer, false);
                    SceneUtils.SetLayer(storeya2.actor, this.m_CardsContainer.gameObject.layer);
                    cardBatch.Add(storeya2.actor);
                    if (this.m_PreviewCardsExpandable && (this.m_CardsPreviewDisplay != null))
                    {
                        PegUIElement element = storeya2.actor.m_cardMesh.gameObject.AddComponent<PegUIElement>();
                        element.GetComponent<Collider>().enabled = true;
                        element.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storeya2.<>m__2F));
                    }
                }
            }
        }
    }

    public void AddHideListener(OnHide dlg)
    {
        this.m_OnHideListeners.Add(dlg);
    }

    public void AddSpecificCards(List<string> cardIds)
    {
        foreach (string str in cardIds)
        {
            List<string> list = new List<string> {
                str
            };
            this.AddCardBatch(list);
        }
    }

    private void Awake()
    {
        if (this.m_BackButton != null)
        {
            if (<>f__am$cache13 == null)
            {
                <>f__am$cache13 = e => Navigation.GoBack();
            }
            this.m_BackButton.AddEventListener(UIEventType.PRESS, <>f__am$cache13);
        }
    }

    private void FireHideEvent()
    {
        foreach (OnHide hide in this.m_OnHideListeners.ToArray())
        {
            hide();
        }
    }

    private bool OnNavigateBack()
    {
        this.Show(false);
        return true;
    }

    public void RemoveHideListener(OnHide dlg)
    {
        this.m_OnHideListeners.Remove(dlg);
    }

    public void Reset()
    {
        foreach (List<Actor> list in this.m_CardBatches)
        {
            foreach (Actor actor in list)
            {
                if (actor != null)
                {
                    UnityEngine.Object.Destroy(actor.gameObject);
                }
            }
        }
        this.m_HiddenCardCount = 0;
        this.m_CardBatches.Clear();
    }

    public void SetHeaderText(string text)
    {
        object[] args = new object[] { text };
        this.m_HeaderTextObject.Text = GameStrings.Format("GLUE_ADVENTURE_REWARDS_PREVIEW_HEADER", args);
    }

    public void SetHiddenCardCount(int hiddenCardCount)
    {
        this.m_HiddenCardCount = hiddenCardCount;
    }

    public void Show(bool show)
    {
        if (this.m_ClickBlocker != null)
        {
            this.m_ClickBlocker.SetActive(show);
        }
        if (this.m_DisableScrollbar != null)
        {
            this.m_DisableScrollbar.Enable(!show);
        }
        if (show)
        {
            this.UpdateCardPositions();
            FullScreenFXMgr.Get().StartStandardBlurVignette(this.m_ShowHideAnimationTime);
            base.gameObject.SetActive(true);
            object[] args = new object[] { "scale", (Vector3) (Vector3.one * 0.05f), "time", this.m_ShowHideAnimationTime };
            iTween.ScaleFrom(base.gameObject, iTween.Hash(args));
            if (!string.IsNullOrEmpty(this.m_PreviewAppearSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_PreviewAppearSound));
            }
            Navigation.Push(new Navigation.NavigateBackHandler(this.OnNavigateBack));
        }
        else
        {
            <Show>c__AnonStorey2A1 storeya = new <Show>c__AnonStorey2A1 {
                <>f__this = this,
                origScale = base.transform.localScale
            };
            object[] objArray2 = new object[] { "scale", (Vector3) (Vector3.one * 0.05f), "time", this.m_ShowHideAnimationTime, "oncomplete", new Action<object>(storeya.<>m__2E) };
            iTween.ScaleTo(base.gameObject, iTween.Hash(objArray2));
            if (!string.IsNullOrEmpty(this.m_PreviewShrinkSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_PreviewShrinkSound));
            }
            FullScreenFXMgr.Get().EndStandardBlurVignette(this.m_ShowHideAnimationTime, null);
        }
    }

    private void UpdateCardPositions()
    {
        int count = this.m_CardBatches.Count;
        bool flag = this.m_HiddenCardCount > 0;
        bool flag2 = this.m_HiddenCardsLabelObject != null;
        if (flag && flag2)
        {
            count++;
        }
        float num2 = ((count - 1) * this.m_CardSpacing) + (count * this.m_CardWidth);
        float num3 = (num2 * 0.5f) - (this.m_CardWidth * 0.5f);
        int num4 = 0;
        foreach (List<Actor> list in this.m_CardBatches)
        {
            if (list.Count != 0)
            {
                int num5 = 0;
                foreach (Actor actor in list)
                {
                    if (actor != null)
                    {
                        Vector3 vector = this.m_CardClumpSpacing * num5;
                        vector.x += (num4 * (this.m_CardSpacing + this.m_CardWidth)) - num3;
                        actor.transform.localScale = (Vector3) (Vector3.one * 5f);
                        actor.transform.localRotation = Quaternion.identity;
                        actor.transform.Rotate(new Vector3(0f, 1f, 0f), (float) (num5 * this.m_CardClumpAngleIncrement));
                        actor.transform.localPosition = vector;
                        actor.SetUnlit();
                        actor.ContactShadow(true);
                        actor.UpdateAllComponents();
                        actor.Show();
                        num5++;
                    }
                }
                num4++;
            }
        }
        if (flag && flag2)
        {
            Vector3 zero = Vector3.zero;
            zero.x += (num4 * (this.m_CardSpacing + this.m_CardWidth)) - num3;
            this.m_HiddenCardsLabelObject.transform.localPosition = zero;
            this.m_HiddenCardsLabel.Text = string.Format("+{0}", this.m_HiddenCardCount);
        }
        if (flag2)
        {
            this.m_HiddenCardsLabelObject.SetActive(flag);
        }
    }

    [CustomEditField(Sections="Cards Preview")]
    public float CardClumpAngleIncrement
    {
        get
        {
            return this.m_CardClumpAngleIncrement;
        }
        set
        {
            this.m_CardClumpAngleIncrement = value;
            this.UpdateCardPositions();
        }
    }

    [CustomEditField(Sections="Cards Preview")]
    public Vector3 CardClumpSpacing
    {
        get
        {
            return this.m_CardClumpSpacing;
        }
        set
        {
            this.m_CardClumpSpacing = value;
            this.UpdateCardPositions();
        }
    }

    [CustomEditField(Sections="Cards Preview")]
    public float CardSpacing
    {
        get
        {
            return this.m_CardSpacing;
        }
        set
        {
            this.m_CardSpacing = value;
            this.UpdateCardPositions();
        }
    }

    [CustomEditField(Sections="Cards Preview")]
    public float CardWidth
    {
        get
        {
            return this.m_CardWidth;
        }
        set
        {
            this.m_CardWidth = value;
            this.UpdateCardPositions();
        }
    }

    [CompilerGenerated]
    private sealed class <AddCardBatch>c__AnonStorey2A2
    {
        internal AdventureRewardsPreview <>f__this;
        internal string cardId;
    }

    [CompilerGenerated]
    private sealed class <AddCardBatch>c__AnonStorey2A3
    {
        internal AdventureRewardsPreview.<AddCardBatch>c__AnonStorey2A2 <>f__ref$674;
        internal AdventureRewardsPreview <>f__this;
        internal Actor actor;

        internal void <>m__2F(UIEvent e)
        {
            if (!this.<>f__this.m_CardsPreviewDisplay.IsShowing())
            {
                List<string> cardIds = new List<string> {
                    this.<>f__ref$674.cardId
                };
                this.<>f__this.m_CardsPreviewDisplay.ShowCards(cardIds, this.actor.transform.position, new Vector3?(this.actor.transform.position));
            }
        }
    }

    [CompilerGenerated]
    private sealed class <Show>c__AnonStorey2A1
    {
        internal AdventureRewardsPreview <>f__this;
        internal Vector3 origScale;

        internal void <>m__2E(object o)
        {
            this.<>f__this.gameObject.SetActive(false);
            this.<>f__this.transform.localScale = this.origScale;
            this.<>f__this.FireHideEvent();
        }
    }

    public delegate void OnHide();
}

