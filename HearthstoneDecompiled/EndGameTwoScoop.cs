using System;
using System.Collections;
using UnityEngine;

public class EndGameTwoScoop : MonoBehaviour
{
    private static readonly float AFTER_PUNCH_SCALE_VAL = 2.3f;
    protected static readonly float BAR_ANIMATION_DELAY = 1f;
    protected static readonly float END_SCALE_VAL = 2.5f;
    public UberText m_bannerLabel;
    protected string m_bannerText;
    public Actor m_heroActor;
    protected bool m_heroActorLoaded;
    public GameObject m_heroBone;
    private bool m_isShown;
    public GameObject m_levelUpTier1;
    public GameObject m_levelUpTier2;
    public GameObject m_levelUpTier3;
    protected HeroXPBar m_xpBar;
    public HeroXPBar m_xpBarPrefab;
    protected static readonly Vector3 START_POSITION = new Vector3(-7.8f, 8.2f, -5f);
    protected static readonly float START_SCALE_VAL = 0.01f;

    private void Awake()
    {
        base.gameObject.SetActive(false);
        AssetLoader.Get().LoadActor("Card_Play_Hero", new AssetLoader.GameObjectCallback(this.OnHeroActorLoaded), null, false);
    }

    protected void EnableBannerLabel(bool enable)
    {
        this.m_bannerLabel.gameObject.SetActive(enable);
    }

    public void Hide()
    {
        this.HideAll();
    }

    private void HideAll()
    {
        object[] args = new object[] { "scale", new Vector3(START_SCALE_VAL, START_SCALE_VAL, START_SCALE_VAL), "time", 0.25f, "oncomplete", "OnAllHidden", "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(base.gameObject, 0f, 0.25f);
        iTween.ScaleTo(base.gameObject, hashtable);
        this.m_isShown = false;
    }

    public void HideXpBar()
    {
        if (this.m_xpBar != null)
        {
            this.m_xpBar.gameObject.SetActive(false);
        }
    }

    public bool IsLoaded()
    {
        return this.m_heroActorLoaded;
    }

    public bool IsShown()
    {
        return this.m_isShown;
    }

    private void OnAllHidden()
    {
        iTween.FadeTo(base.gameObject, 0f, 0f);
        base.gameObject.SetActive(false);
        this.ResetPositions();
    }

    private void OnHeroActorLoaded(string name, GameObject go, object callbackData)
    {
        go.transform.parent = base.transform;
        go.transform.localPosition = this.m_heroBone.transform.localPosition;
        go.transform.localScale = this.m_heroBone.transform.localScale;
        this.m_heroActor = go.GetComponent<Actor>();
        this.m_heroActor.TurnOffCollider();
        this.m_heroActor.m_healthObject.SetActive(false);
        this.m_heroActorLoaded = true;
        this.m_heroActor.SetCardFlair(GameState.Get().GetFriendlySidePlayer().GetHeroCard().GetCardFlair());
        this.m_heroActor.UpdateAllComponents();
    }

    protected void PlayLevelUpEffect()
    {
        GameObject obj2 = UnityEngine.Object.Instantiate<GameObject>(this.m_levelUpTier1);
        if (obj2 != null)
        {
            obj2.transform.parent = base.transform;
            obj2.GetComponent<PlayMakerFSM>().SendEvent("Birth");
        }
    }

    protected void PunchEndGameTwoScoop()
    {
        EndGameScreen.Get().NotifyOfAnimComplete();
        iTween.ScaleTo(base.gameObject, new Vector3(AFTER_PUNCH_SCALE_VAL, AFTER_PUNCH_SCALE_VAL, AFTER_PUNCH_SCALE_VAL), 0.15f);
    }

    protected virtual void ResetPositions()
    {
    }

    protected void SaveBannerText(string bannerText)
    {
        this.m_bannerText = bannerText;
    }

    protected void SetBannerLabel(string label)
    {
        this.m_bannerLabel.Text = label;
    }

    public void Show()
    {
        this.m_isShown = true;
        base.gameObject.SetActive(true);
        this.ShowImpl();
        if (!GameMgr.Get().IsTutorial() && !GameMgr.Get().IsSpectator())
        {
            NetCache.HeroLevel heroLevel = GameUtils.GetHeroLevel(GameState.Get().GetFriendlySidePlayer().GetStartingHero().GetClass());
            if (heroLevel == null)
            {
                this.HideXpBar();
            }
            else
            {
                this.m_xpBar = UnityEngine.Object.Instantiate<HeroXPBar>(this.m_xpBarPrefab);
                this.m_xpBar.transform.parent = this.m_heroActor.transform;
                this.m_xpBar.transform.localScale = new Vector3(0.88f, 0.88f, 0.88f);
                this.m_xpBar.transform.localPosition = new Vector3(-0.1886583f, 0.2122119f, -0.7446293f);
                this.m_xpBar.m_soloLevelLimit = NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>().XPSoloLimit;
                this.m_xpBar.m_isAnimated = true;
                this.m_xpBar.m_delay = BAR_ANIMATION_DELAY;
                this.m_xpBar.m_heroLevel = heroLevel;
                this.m_xpBar.m_levelUpCallback = new HeroXPBar.PlayLevelUpEffectCallback(this.PlayLevelUpEffect);
                this.m_xpBar.UpdateDisplay();
            }
        }
    }

    protected virtual void ShowImpl()
    {
    }

    private void Start()
    {
        SceneUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
        this.ResetPositions();
    }

    public virtual void StopAnimating()
    {
    }
}

