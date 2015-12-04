using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class HeroSkinInfoManager : MonoBehaviour
{
    [CompilerGenerated]
    private static GameUtils.EmoteSoundLoaded <>f__am$cache19;
    private bool m_animating;
    public float m_animationTime = 0.5f;
    private CardFlair m_currentCardFlair;
    private string m_currentCardId;
    private EntityDef m_currentEntityDef;
    private CollectionHeroDef m_currentHeroDef;
    private DbfRecord m_currentHeroRecord;
    public MusicPlaylistType m_defaultHeroMusic = MusicPlaylistType.UI_CMHeroSkinPreview;
    public Material m_defaultPreviewMaterial;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_enterPreviewSound;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_exitPreviewSound;
    public UberText m_newHeroDescription;
    public UIBButton m_newHeroFavoriteButton;
    public GameObject m_newHeroFrame;
    public MeshRenderer m_newHeroPreviewQuad;
    public UberText m_newHeroTitle;
    public PegUIElement m_offClicker;
    public GameObject m_previewPane;
    public UberText m_vanillaHeroDescription;
    public UIBButton m_vanillaHeroFavoriteButton;
    public GameObject m_vanillaHeroFrame;
    public Material m_vanillaHeroNonPremiumMaterial;
    public MeshRenderer m_vanillaHeroPreviewQuad;
    public UberText m_vanillaHeroTitle;
    private static HeroSkinInfoManager s_instance;

    private void AssignNewHeroPreviewMaterial(Material material, Texture portraitTexture)
    {
        if (material == null)
        {
            this.m_newHeroPreviewQuad.GetComponent<Renderer>().material = this.m_defaultPreviewMaterial;
            this.m_newHeroPreviewQuad.GetComponent<Renderer>().material.mainTexture = portraitTexture;
        }
        else
        {
            this.m_newHeroPreviewQuad.GetComponent<Renderer>().material = material;
        }
    }

    private void AssignVanillaHeroPreviewMaterial(Material material, Texture portraitTexture)
    {
        if (portraitTexture != null)
        {
            Material[] materials = this.m_vanillaHeroPreviewQuad.GetComponent<Renderer>().materials;
            materials[1] = material;
            materials[1].SetTexture("_MainTex", portraitTexture);
            this.m_vanillaHeroPreviewQuad.GetComponent<Renderer>().materials = materials;
        }
        else
        {
            RenderUtils.SetMaterial((Renderer) this.m_vanillaHeroPreviewQuad, 1, material);
        }
    }

    private void Awake()
    {
        s_instance = this;
        this.m_previewPane.SetActive(false);
        this.SetupUI();
    }

    public void CancelPreview()
    {
        <CancelPreview>c__AnonStorey2E2 storeye = new <CancelPreview>c__AnonStorey2E2 {
            <>f__this = this
        };
        if (!this.m_animating)
        {
            storeye.origScale = this.m_previewPane.transform.localScale;
            this.m_animating = true;
            object[] args = new object[] { "scale", new Vector3(0.01f, 0.01f, 0.01f), "time", this.m_animationTime, "easeType", iTween.EaseType.easeOutCirc, "oncomplete", new Action<object>(storeye.<>m__BF) };
            iTween.ScaleTo(this.m_previewPane, iTween.Hash(args));
            if (!string.IsNullOrEmpty(this.m_exitPreviewSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_exitPreviewSound));
            }
            this.StopHeroMusic();
            FullScreenFXMgr.Get().EndStandardBlurVignette(this.m_animationTime, null);
        }
    }

    public void EnterPreview(CollectionCardVisual cardVisual)
    {
        if (!this.m_animating)
        {
            this.m_currentEntityDef = cardVisual.GetActor().GetEntityDef();
            this.m_currentCardFlair = cardVisual.GetActor().GetCardFlair();
            if (this.LoadHeroDef(this.m_currentEntityDef.GetCardId()))
            {
                if (this.m_currentEntityDef.GetCardSet() == TAG_CARD_SET.CORE)
                {
                    this.m_vanillaHeroTitle.Text = this.m_currentEntityDef.GetName();
                    this.m_vanillaHeroDescription.Text = this.m_currentHeroRecord.GetLocString("DESCRIPTION");
                    Material vanillaHeroNonPremiumMaterial = null;
                    Texture portraitTexture = null;
                    if (this.m_currentCardFlair.Premium == TAG_PREMIUM.NORMAL)
                    {
                        vanillaHeroNonPremiumMaterial = this.m_vanillaHeroNonPremiumMaterial;
                        portraitTexture = cardVisual.GetActor().GetCardDef().GetPortraitTexture();
                    }
                    else
                    {
                        vanillaHeroNonPremiumMaterial = cardVisual.GetActor().GetCardDef().GetPremiumPortraitMaterial();
                    }
                    this.m_newHeroFrame.SetActive(false);
                    this.m_vanillaHeroFrame.SetActive(true);
                    this.AssignVanillaHeroPreviewMaterial(vanillaHeroNonPremiumMaterial, portraitTexture);
                }
                else
                {
                    this.m_newHeroTitle.Text = this.m_currentEntityDef.GetName();
                    this.m_newHeroDescription.Text = this.m_currentHeroRecord.GetLocString("DESCRIPTION");
                    this.m_newHeroFrame.SetActive(true);
                    this.m_vanillaHeroFrame.SetActive(false);
                    this.AssignNewHeroPreviewMaterial(this.m_currentHeroDef.m_previewTexture, cardVisual.GetActor().GetPortraitTexture());
                }
                if ((this.m_currentHeroDef != null) && (this.m_currentHeroDef.m_collectionManagerPreviewEmote != EmoteType.INVALID))
                {
                    if (<>f__am$cache19 == null)
                    {
                        <>f__am$cache19 = delegate (CardSoundSpell cardSpell) {
                            <EnterPreview>c__AnonStorey2E1 storeye = new <EnterPreview>c__AnonStorey2E1 {
                                cardSpell = cardSpell
                            };
                            if (storeye.cardSpell != null)
                            {
                                storeye.cardSpell.AddFinishedCallback(new Spell.FinishedCallback(storeye.<>m__C5));
                                storeye.cardSpell.Reactivate();
                            }
                        };
                    }
                    GameUtils.LoadCardDefEmoteSound(cardVisual.GetActor().GetCardDef(), this.m_currentHeroDef.m_collectionManagerPreviewEmote, <>f__am$cache19);
                }
            }
            else
            {
                Debug.LogError("Could not load entity def for hero skin, preview will not be shown");
                this.m_newHeroFrame.SetActive(false);
                this.m_vanillaHeroFrame.SetActive(false);
            }
            this.m_previewPane.SetActive(true);
            this.m_offClicker.gameObject.SetActive(true);
            this.m_animating = true;
            object[] args = new object[] { "scale", new Vector3(0.01f, 0.01f, 0.01f), "time", this.m_animationTime, "easeType", iTween.EaseType.easeOutCirc, "oncomplete", e => this.m_animating = false };
            iTween.ScaleFrom(this.m_previewPane, iTween.Hash(args));
            bool enabled = false;
            if (!CollectionManager.Get().IsInEditMode())
            {
                TAG_CLASS heroClass = this.m_currentEntityDef.GetClass();
                NetCache.CardDefinition favoriteHero = CollectionManager.Get().GetFavoriteHero(heroClass);
                List<NetCache.CardDefinition> bestHeroesIOwn = CollectionManager.Get().GetBestHeroesIOwn(heroClass);
                enabled = ((favoriteHero != null) && (favoriteHero.Name != this.m_currentCardId)) && (bestHeroesIOwn.Count > 1);
            }
            if (this.m_currentEntityDef.GetCardSet() == TAG_CARD_SET.CORE)
            {
                this.m_vanillaHeroFavoriteButton.SetEnabled(enabled);
                this.m_vanillaHeroFavoriteButton.Flip(enabled);
            }
            else
            {
                this.m_newHeroFavoriteButton.SetEnabled(enabled);
                this.m_newHeroFavoriteButton.Flip(enabled);
            }
            if (!string.IsNullOrEmpty(this.m_enterPreviewSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_enterPreviewSound));
            }
            this.PlayHeroMusic();
            FullScreenFXMgr.Get().StartStandardBlurVignette(this.m_animationTime);
        }
    }

    public static HeroSkinInfoManager Get()
    {
        return s_instance;
    }

    private bool LoadHeroDef(string cardId)
    {
        <LoadHeroDef>c__AnonStorey2E3 storeye = new <LoadHeroDef>c__AnonStorey2E3 {
            cardId = cardId
        };
        if ((this.m_currentCardId != storeye.cardId) || !string.IsNullOrEmpty(storeye.cardId))
        {
            DbfRecord record = GameDbf.Hero.GetRecords().Find(new Predicate<DbfRecord>(storeye.<>m__C0));
            if (record == null)
            {
                Debug.LogWarning(string.Format("Unable to find hero with ID: {0} in HERO.xml", storeye.cardId));
                return false;
            }
            string assetPath = record.GetAssetPath("HERODEF_ASSET_PATH");
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning(string.Format("Hero record ID {0} does not have HERODEF_ASSET_PATH defined", storeye.cardId));
                return false;
            }
            CollectionHeroDef component = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(assetPath), true, false).GetComponent<CollectionHeroDef>();
            if (component == null)
            {
                Debug.LogWarning(string.Format("Hero def does not exist on object: {0}", assetPath));
                return false;
            }
            if (this.m_currentHeroDef != null)
            {
                UnityEngine.Object.Destroy(this.m_currentHeroDef.gameObject);
            }
            this.m_currentCardId = storeye.cardId;
            this.m_currentHeroDef = component;
            this.m_currentHeroRecord = record;
        }
        return true;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void PlayHeroMusic()
    {
        MusicPlaylistType defaultHeroMusic;
        if ((this.m_currentHeroDef == null) || (this.m_currentHeroDef.m_heroPlaylist == MusicPlaylistType.Invalid))
        {
            defaultHeroMusic = this.m_defaultHeroMusic;
        }
        else
        {
            defaultHeroMusic = this.m_currentHeroDef.m_heroPlaylist;
        }
        if (defaultHeroMusic != MusicPlaylistType.Invalid)
        {
            MusicManager.Get().OverlayPlaylist(defaultHeroMusic, 3f);
        }
    }

    private void SetFavoriteHero()
    {
        NetCache.CardDefinition hero = new NetCache.CardDefinition {
            Name = this.m_currentEntityDef.GetCardId(),
            Premium = this.m_currentCardFlair.Premium
        };
        Network.SetFavoriteHero(this.m_currentEntityDef.GetClass(), hero);
    }

    private void SetupUI()
    {
        this.m_newHeroFavoriteButton.AddEventListener(UIEventType.RELEASE, delegate (UIEvent e) {
            this.SetFavoriteHero();
            this.CancelPreview();
        });
        if ((this.m_vanillaHeroFavoriteButton != null) && (this.m_vanillaHeroFavoriteButton != this.m_newHeroFavoriteButton))
        {
            this.m_vanillaHeroFavoriteButton.AddEventListener(UIEventType.RELEASE, delegate (UIEvent e) {
                this.SetFavoriteHero();
                this.CancelPreview();
            });
        }
        this.m_offClicker.AddEventListener(UIEventType.RELEASE, e => this.CancelPreview());
        this.m_offClicker.AddEventListener(UIEventType.RIGHTCLICK, e => this.CancelPreview());
    }

    private void StopHeroMusic()
    {
        MusicManager.Get().StopOverlayPlaylists(3f);
    }

    [CompilerGenerated]
    private sealed class <CancelPreview>c__AnonStorey2E2
    {
        internal HeroSkinInfoManager <>f__this;
        internal Vector3 origScale;

        internal void <>m__BF(object e)
        {
            this.<>f__this.m_animating = false;
            this.<>f__this.m_previewPane.transform.localScale = this.origScale;
            this.<>f__this.m_previewPane.SetActive(false);
            this.<>f__this.m_offClicker.gameObject.SetActive(false);
        }
    }

    [CompilerGenerated]
    private sealed class <EnterPreview>c__AnonStorey2E1
    {
        internal CardSoundSpell cardSpell;

        internal void <>m__C5(Spell spell, object data)
        {
            UnityEngine.Object.Destroy(this.cardSpell.gameObject);
        }
    }

    [CompilerGenerated]
    private sealed class <LoadHeroDef>c__AnonStorey2E3
    {
        internal string cardId;

        internal bool <>m__C0(DbfRecord r)
        {
            return (r.GetString("CARD_ID") == this.cardId);
        }
    }
}

