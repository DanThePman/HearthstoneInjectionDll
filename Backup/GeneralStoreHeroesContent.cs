using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStoreHeroesContent : GeneralStoreContent
{
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_backgroundFlipSound;
    private int m_currentCardBackPreview = -1;
    private DbfRecord m_currentDbfRecord;
    private int m_currentDisplay = -1;
    private string m_currentHero;
    private CollectionHeroDef m_currentHeroDef;
    public GeneralStoreHeroesContentDisplay m_heroDisplay;
    private GeneralStoreHeroesContentDisplay m_heroDisplay1;
    private GeneralStoreHeroesContentDisplay m_heroDisplay2;
    public GameObject m_heroEmptyDisplay;
    public string m_keyArtAppearAnim = "HeroSkinArtGlowIn";
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_keyArtAppearSound;
    public string m_keyArtFadeAnim = "HeroSkinArt_WipeAway";
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_keyArtFadeSound;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_previewButtonClick;
    public MeshRenderer m_renderQuad1;
    public MeshRenderer m_renderQuad2;
    public GameObject m_renderToTexture1;
    public GameObject m_renderToTexture2;

    private void AnimateAndUpdateDisplays(string heroID, int cardBackIdx, CollectionHeroDef heroDef, bool purchased)
    {
        <AnimateAndUpdateDisplays>c__AnonStorey33C storeyc = new <AnimateAndUpdateDisplays>c__AnonStorey33C {
            currDisplay = null
        };
        GameObject target = null;
        if (this.m_currentDisplay == -1)
        {
            this.m_currentDisplay = 1;
            storeyc.currDisplay = this.m_heroEmptyDisplay;
        }
        else
        {
            storeyc.currDisplay = this.GetCurrentDisplayContainer();
        }
        target = this.GetNextDisplayContainer();
        GeneralStoreHeroesContentDisplay currentDisplay = this.GetCurrentDisplay();
        this.m_currentDisplay = (this.m_currentDisplay + 1) % 2;
        storeyc.currDisplay.transform.localRotation = Quaternion.identity;
        target.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
        target.SetActive(true);
        iTween.StopByName(storeyc.currDisplay, "ROTATION_TWEEN");
        iTween.StopByName(target, "ROTATION_TWEEN");
        object[] args = new object[] { "amount", new Vector3(0.5f, 0f, 0f), "time", 0.5f, "name", "ROTATION_TWEEN", "oncomplete", new Action<object>(storeyc.<>m__17A) };
        iTween.RotateBy(storeyc.currDisplay, iTween.Hash(args));
        object[] objArray2 = new object[] { "amount", new Vector3(0.5f, 0f, 0f), "time", 0.5f, "name", "ROTATION_TWEEN" };
        iTween.RotateBy(target, iTween.Hash(objArray2));
        if (!string.IsNullOrEmpty(this.m_backgroundFlipSound))
        {
            SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_backgroundFlipSound));
        }
        GeneralStoreHeroesContentDisplay display2 = this.GetCurrentDisplay();
        display2.UpdateFrame(heroID, cardBackIdx, heroDef);
        display2.ShowPurchasedCheckmark(purchased);
        display2.ResetPreview();
        currentDisplay.ResetPreview();
    }

    public override bool AnimateEntranceEnd()
    {
        base.m_parentStore.HideAccentTexture();
        return true;
    }

    private void Awake()
    {
        this.m_heroDisplay1 = this.m_heroDisplay;
        this.m_heroDisplay2 = UnityEngine.Object.Instantiate<GeneralStoreHeroesContentDisplay>(this.m_heroDisplay);
        this.m_heroDisplay2.transform.parent = this.m_heroDisplay1.transform.parent;
        this.m_heroDisplay2.transform.localPosition = this.m_heroDisplay1.transform.localPosition;
        this.m_heroDisplay2.transform.localScale = this.m_heroDisplay1.transform.localScale;
        this.m_heroDisplay2.transform.localRotation = this.m_heroDisplay1.transform.localRotation;
        this.m_heroDisplay2.gameObject.SetActive(false);
        this.m_heroDisplay1.SetParent(this);
        this.m_heroDisplay2.SetParent(this);
        this.m_heroDisplay1.SetKeyArtRenderer(this.m_renderQuad1);
        this.m_heroDisplay2.SetKeyArtRenderer(this.m_renderQuad2);
        this.m_renderToTexture1.GetComponent<RenderToTexture>().m_RenderToObject = this.m_heroDisplay1.m_renderArtQuad;
        this.m_renderToTexture2.GetComponent<RenderToTexture>().m_RenderToObject = this.m_heroDisplay2.m_renderArtQuad;
    }

    private GeneralStoreHeroesContentDisplay GetCurrentDisplay()
    {
        return ((this.m_currentDisplay != 0) ? this.m_heroDisplay2 : this.m_heroDisplay1);
    }

    private GameObject GetCurrentDisplayContainer()
    {
        return this.GetCurrentDisplay().gameObject;
    }

    private string GetHeroDescriptionString()
    {
        if (UniversalInputManager.UsePhoneUI == null)
        {
            return this.m_currentDbfRecord.GetLocString("STORE_DESCRIPTION");
        }
        EntityDef entityDef = DefLoader.Get().GetEntityDef(this.m_currentDbfRecord.GetString("CARD_ID"));
        object[] args = new object[] { GameStrings.GetClassName(entityDef.GetClass()) };
        return GameStrings.Format("GLUE_STORE_HEROES_DESCRIPTION_PHONE", args);
    }

    public override string GetMoneyDisplayOwnedText()
    {
        return GameStrings.Get("GLUE_STORE_HERO_BUTTON_COST_OWNED_TEXT");
    }

    private GameObject GetNextDisplayContainer()
    {
        return ((((this.m_currentDisplay + 1) % 2) != 0) ? this.m_heroDisplay2.gameObject : this.m_heroDisplay1.gameObject);
    }

    public override bool IsPurchaseDisabled()
    {
        return (this.m_currentDisplay == -1);
    }

    protected override void OnRefresh()
    {
        Network.Bundle heroBundle = null;
        if (!string.IsNullOrEmpty(this.m_currentHero))
        {
            StoreManager.Get().GetHeroBundle(this.m_currentHero, out heroBundle);
        }
        this.GetCurrentDisplay().ShowPurchasedCheckmark(heroBundle == null);
        base.SetCurrentMoneyBundle(heroBundle, false);
        this.UpdateHeroDescription(heroBundle == null);
    }

    public void PlayCurrentHeroPurchaseEmote()
    {
        GeneralStoreHeroesContentDisplay currentDisplay = this.GetCurrentDisplay();
        if (currentDisplay != null)
        {
            currentDisplay.PlayPurchaseEmote();
        }
    }

    private void PlayHeroMusic()
    {
        if (((this.m_currentHeroDef == null) || (this.m_currentHeroDef.m_heroPlaylist == MusicPlaylistType.Invalid)) || !MusicManager.Get().StartPlaylist(this.m_currentHeroDef.m_heroPlaylist))
        {
            base.m_parentStore.ResumePreviousMusicPlaylist();
        }
    }

    public override void PostStoreFlipIn(bool animatedFlipIn)
    {
        this.PlayHeroMusic();
    }

    public override void PreStoreFlipIn()
    {
        this.ResetHeroPreview();
    }

    private void ResetHeroPreview()
    {
        this.GetCurrentDisplay().ResetPreview();
    }

    public void SelectHero(string heroID, bool animate = true)
    {
        if (this.m_currentHero != heroID)
        {
            DbfRecord record = GameDbf.Hero.GetRecord("CARD_ID", heroID);
            if (record == null)
            {
                Debug.LogError(string.Format("Unable to find hero in DBF: {0}", heroID));
            }
            else
            {
                this.m_currentHero = heroID;
                this.m_currentDbfRecord = record;
                Network.Bundle heroBundle = null;
                StoreManager.Get().GetHeroBundle(heroID, out heroBundle);
                base.SetCurrentMoneyBundle(heroBundle, false);
                if (this.m_currentHeroDef != null)
                {
                    UnityEngine.Object.Destroy(this.m_currentHeroDef.gameObject);
                    this.m_currentHeroDef = null;
                }
                this.m_currentCardBackPreview = record.GetInt("CARD_BACK_ID");
                string assetPath = record.GetAssetPath("HERODEF_ASSET_PATH");
                this.m_currentHeroDef = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(assetPath), true, false).GetComponent<CollectionHeroDef>();
                this.AnimateAndUpdateDisplays(heroID, this.m_currentCardBackPreview, this.m_currentHeroDef, heroBundle == null);
                this.PlayHeroMusic();
                this.UpdateHeroDescription(heroBundle == null);
            }
        }
    }

    public override void StoreShown(bool isCurrent)
    {
        if ((this.m_currentDisplay != -1) && isCurrent)
        {
            this.PlayHeroMusic();
            this.ResetHeroPreview();
        }
    }

    private void UpdateHeroDescription(bool purchased)
    {
        if ((this.m_currentDisplay == -1) || (this.m_currentDbfRecord == null))
        {
            base.m_parentStore.SetChooseDescription(GameStrings.Get("GLUE_STORE_CHOOSE_HERO"));
        }
        else
        {
            string warning = !StoreManager.Get().IsKoreanCustomer() ? string.Empty : GameStrings.Get("GLUE_STORE_SUMMARY_KOREAN_AGREEMENT_HERO");
            base.m_parentStore.SetDescription(string.Empty, this.GetHeroDescriptionString(), warning);
        }
        base.m_parentStore.HideAccentTexture();
    }

    [CompilerGenerated]
    private sealed class <AnimateAndUpdateDisplays>c__AnonStorey33C
    {
        internal GameObject currDisplay;

        internal void <>m__17A(object o)
        {
            this.currDisplay.SetActive(false);
        }
    }
}

