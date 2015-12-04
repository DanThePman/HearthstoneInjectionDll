using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStoreHeroesContentDisplay : MonoBehaviour
{
    private GameObject m_cardBack;
    public GameObject m_cardBackContainer;
    public MeshRenderer m_classIcon;
    public UberText m_className;
    private CollectionHeroDef m_currentHeroAsset;
    public MeshRenderer m_fauxPlateTexture;
    private Actor m_heroActor;
    public GameObject m_heroContainer;
    public UberText m_heroName;
    private Actor m_heroPowerActor;
    public GameObject m_heroPowerContainer;
    private MeshRenderer m_keyArt;
    public Animator m_keyArtAnimation;
    private bool m_keyArtShowing = true;
    private GeneralStoreHeroesContent m_parent;
    public GameObject m_previewButtonFX;
    private CardSoundSpell m_previewEmote;
    public UIBButton m_previewToggle;
    public GameObject m_purchasedCheckMark;
    private CardSoundSpell m_purchaseEmote;
    public GameObject m_renderArtQuad;

    private void Awake()
    {
        this.m_previewToggle.AddEventListener(UIEventType.RELEASE, e => this.TogglePreview());
    }

    private void ClearEmotes()
    {
        if (this.m_previewEmote != null)
        {
            UnityEngine.Object.Destroy(this.m_previewEmote.gameObject);
        }
        if (this.m_purchaseEmote != null)
        {
            UnityEngine.Object.Destroy(this.m_purchaseEmote.gameObject);
        }
    }

    public void Init()
    {
        if (this.m_heroActor == null)
        {
            this.m_heroActor = AssetLoader.Get().LoadActor("Card_Play_Hero", false, false).GetComponent<Actor>();
            this.m_heroActor.SetUnlit();
            this.m_heroActor.Show();
            this.m_heroActor.GetHealthObject().Hide();
            this.m_heroActor.GetAttackObject().Hide();
            GameUtils.SetParent(this.m_heroActor, this.m_heroContainer, true);
            SceneUtils.SetLayer(this.m_heroActor, this.m_heroContainer.layer);
        }
        if (this.m_heroPowerActor == null)
        {
            this.m_heroPowerActor = AssetLoader.Get().LoadActor("Card_Play_HeroPower", false, false).GetComponent<Actor>();
            this.m_heroPowerActor.SetUnlit();
            this.m_heroPowerActor.Show();
            GameUtils.SetParent(this.m_heroPowerActor, this.m_heroPowerContainer, true);
            SceneUtils.SetLayer(this.m_heroPowerActor, this.m_heroPowerContainer.layer);
        }
    }

    private void PlayKeyArtAnimation(bool showPreview)
    {
        string keyArtFadeAnim;
        string keyArtFadeSound;
        if (showPreview)
        {
            keyArtFadeAnim = this.m_parent.m_keyArtFadeAnim;
            keyArtFadeSound = this.m_parent.m_keyArtFadeSound;
        }
        else
        {
            keyArtFadeAnim = this.m_parent.m_keyArtAppearAnim;
            keyArtFadeSound = this.m_parent.m_keyArtAppearSound;
        }
        this.m_previewButtonFX.SetActive(showPreview);
        if (!string.IsNullOrEmpty(keyArtFadeSound))
        {
            SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(keyArtFadeSound));
        }
        this.m_keyArtAnimation.enabled = true;
        this.m_keyArtAnimation.StopPlayback();
        this.m_keyArtAnimation.Play(keyArtFadeAnim, -1, 0f);
    }

    public void PlayPreviewEmote()
    {
        if (this.m_previewEmote != null)
        {
            AudioSource activeAudioSource = this.m_previewEmote.GetActiveAudioSource();
            if (activeAudioSource == null)
            {
                activeAudioSource = this.m_previewEmote.DetermineBestAudioSource();
            }
            activeAudioSource.transform.position = Box.Get().GetCamera().transform.position;
            this.m_previewEmote.Reactivate();
        }
    }

    public void PlayPurchaseEmote()
    {
        if (this.m_purchaseEmote != null)
        {
            AudioSource activeAudioSource = this.m_purchaseEmote.GetActiveAudioSource();
            if (activeAudioSource == null)
            {
                activeAudioSource = this.m_purchaseEmote.DetermineBestAudioSource();
            }
            activeAudioSource.transform.position = Box.Get().GetCamera().transform.position;
            this.m_purchaseEmote.Reactivate();
        }
    }

    public void ResetPreview()
    {
        this.m_keyArtShowing = true;
        this.m_keyArtAnimation.enabled = true;
        this.m_keyArtAnimation.StopPlayback();
        this.m_keyArtAnimation.Play(this.m_parent.m_keyArtAppearAnim, -1, 1f);
        this.m_previewButtonFX.SetActive(false);
    }

    public void SetKeyArtRenderer(MeshRenderer keyArtRenderer)
    {
        this.m_keyArt = keyArtRenderer;
    }

    public void SetParent(GeneralStoreHeroesContent parent)
    {
        this.m_parent = parent;
    }

    public void ShowPurchasedCheckmark(bool show)
    {
        if (this.m_purchasedCheckMark != null)
        {
            this.m_purchasedCheckMark.SetActive(show);
        }
    }

    public void TogglePreview()
    {
        if (!string.IsNullOrEmpty(this.m_parent.m_previewButtonClick))
        {
            SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_parent.m_previewButtonClick));
        }
        this.PlayKeyArtAnimation(this.m_keyArtShowing);
        this.m_keyArtShowing = !this.m_keyArtShowing;
        if (!this.m_keyArtShowing)
        {
            this.m_heroActor.Show();
            this.m_heroPowerActor.Show();
            this.PlayPreviewEmote();
        }
        else
        {
            this.m_heroActor.Hide();
            this.m_heroPowerActor.Hide();
        }
    }

    public void UpdateFrame(string heroID, int cardBackIdx, CollectionHeroDef heroDef)
    {
        <UpdateFrame>c__AnonStorey33D storeyd = new <UpdateFrame>c__AnonStorey33D {
            heroDef = heroDef,
            cardBackIdx = cardBackIdx,
            <>f__this = this
        };
        this.Init();
        this.m_keyArt.material = storeyd.heroDef.m_previewTexture;
        if (storeyd.heroDef.m_fauxPlateTexture != null)
        {
            this.m_fauxPlateTexture.material.SetTexture("_MainTex", storeyd.heroDef.m_fauxPlateTexture);
        }
        DefLoader.Get().LoadFullDef(heroID, new DefLoader.LoadDefCallback<FullDef>(storeyd.<>m__17C));
        if (this.m_cardBack != null)
        {
            UnityEngine.Object.Destroy(this.m_cardBack);
            this.m_cardBack = null;
        }
        CardBackManager.Get().LoadCardBackByIndex(storeyd.cardBackIdx, new CardBackManager.LoadCardBackData.LoadCardBackCallback(storeyd.<>m__17D), "Card_Hidden");
    }

    [CompilerGenerated]
    private sealed class <UpdateFrame>c__AnonStorey33D
    {
        internal GeneralStoreHeroesContentDisplay <>f__this;
        internal int cardBackIdx;
        internal CollectionHeroDef heroDef;

        internal void <>m__17C(string heroCardId, FullDef heroFullDef, object data1)
        {
            Vector2 vector;
            CardDef cardDef = heroFullDef.GetCardDef();
            this.<>f__this.m_heroActor.SetCardFlair(new CardFlair(TAG_PREMIUM.NORMAL));
            this.<>f__this.m_heroActor.SetCardDef(cardDef);
            this.<>f__this.m_heroActor.SetEntityDef(heroFullDef.GetEntityDef());
            this.<>f__this.m_heroActor.UpdateAllComponents();
            this.<>f__this.m_heroActor.Hide();
            this.<>f__this.m_heroName.Text = heroFullDef.GetEntityDef().GetName();
            this.<>f__this.m_className.Text = GameStrings.GetClassName(heroFullDef.GetEntityDef().GetClass());
            string heroPowerCardIdFromHero = GameUtils.GetHeroPowerCardIdFromHero(heroCardId);
            DefLoader.Get().LoadFullDef(heroPowerCardIdFromHero, delegate (string powerCardId, FullDef powerDef, object data2) {
                this.<>f__this.m_heroPowerActor.SetCardFlair(new CardFlair(TAG_PREMIUM.GOLDEN));
                this.<>f__this.m_heroPowerActor.SetCardDef(powerDef.GetCardDef());
                this.<>f__this.m_heroPowerActor.SetEntityDef(powerDef.GetEntityDef());
                this.<>f__this.m_heroPowerActor.UpdateAllComponents();
                this.<>f__this.m_heroPowerActor.Hide();
            });
            if (CollectionPageManager.s_classTextureOffsets.TryGetValue(heroFullDef.GetEntityDef().GetClass(), out vector))
            {
                this.<>f__this.m_classIcon.material.SetTextureOffset("_MainTex", vector);
            }
            this.<>f__this.ClearEmotes();
            if (this.heroDef.m_storePreviewEmote != EmoteType.INVALID)
            {
                GameUtils.LoadCardDefEmoteSound(cardDef, this.heroDef.m_storePreviewEmote, delegate (CardSoundSpell spell) {
                    if (spell != null)
                    {
                        this.<>f__this.m_previewEmote = spell;
                        GameUtils.SetParent((Component) this.<>f__this.m_previewEmote, (Component) this.<>f__this, false);
                    }
                });
            }
            if (this.heroDef.m_storePurchaseEmote != EmoteType.INVALID)
            {
                GameUtils.LoadCardDefEmoteSound(cardDef, this.heroDef.m_storePurchaseEmote, delegate (CardSoundSpell spell) {
                    if (spell != null)
                    {
                        this.<>f__this.m_purchaseEmote = spell;
                        GameUtils.SetParent((Component) this.<>f__this.m_purchaseEmote, (Component) this.<>f__this, false);
                    }
                });
            }
        }

        internal void <>m__17D(CardBackManager.LoadCardBackData cardBackData)
        {
            GameObject gameObject = cardBackData.m_GameObject;
            gameObject.name = "CARD_BACK_" + this.cardBackIdx;
            this.<>f__this.m_cardBack = gameObject;
            SceneUtils.SetLayer(gameObject, this.<>f__this.m_cardBackContainer.gameObject.layer);
            GameUtils.SetParent(gameObject, this.<>f__this.m_cardBackContainer, false);
            this.<>f__this.m_cardBack.transform.localPosition = Vector3.zero;
            this.<>f__this.m_cardBack.transform.localScale = Vector3.one;
            this.<>f__this.m_cardBack.transform.localRotation = Quaternion.identity;
            AnimationUtil.FloatyPosition(this.<>f__this.m_cardBack, 0.05f, 10f);
        }

        internal void <>m__17E(string powerCardId, FullDef powerDef, object data2)
        {
            this.<>f__this.m_heroPowerActor.SetCardFlair(new CardFlair(TAG_PREMIUM.GOLDEN));
            this.<>f__this.m_heroPowerActor.SetCardDef(powerDef.GetCardDef());
            this.<>f__this.m_heroPowerActor.SetEntityDef(powerDef.GetEntityDef());
            this.<>f__this.m_heroPowerActor.UpdateAllComponents();
            this.<>f__this.m_heroPowerActor.Hide();
        }

        internal void <>m__17F(CardSoundSpell spell)
        {
            if (spell != null)
            {
                this.<>f__this.m_previewEmote = spell;
                GameUtils.SetParent((Component) this.<>f__this.m_previewEmote, (Component) this.<>f__this, false);
            }
        }

        internal void <>m__180(CardSoundSpell spell)
        {
            if (spell != null)
            {
                this.<>f__this.m_purchaseEmote = spell;
                GameUtils.SetParent((Component) this.<>f__this.m_purchaseEmote, (Component) this.<>f__this, false);
            }
        }
    }
}

