using System;
using System.Collections;
using UnityEngine;

public class CurrencyFrame : MonoBehaviour
{
    public UberText m_amount;
    public GameObject m_background;
    private bool m_backgroundFaded;
    public GameObject m_dustFX;
    public GameObject m_dustJar;
    public GameObject m_explodeFX_Common;
    public GameObject m_explodeFX_Epic;
    public GameObject m_explodeFX_Legendary;
    public GameObject m_explodeFX_Rare;
    public GameObject m_goldCoin;
    public PegUIElement m_mouseOverZone;
    private CurrencyType? m_overrideCurrencyType;
    private CurrencyType m_showingCurrency;
    private State m_state = State.SHOWN;

    private void ActivateCurrencyFrame()
    {
        this.m_state = State.SHOWN;
    }

    private void Awake()
    {
        NetCache.Get().RegisterGoldBalanceListener(new NetCache.DelGoldBalanceListener(this.OnGoldBalanceChanged));
        this.m_mouseOverZone.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnFrameMouseOver));
        this.m_mouseOverZone.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnFrameMouseOut));
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
    }

    public void DeactivateCurrencyFrame()
    {
        base.gameObject.SetActive(false);
        this.m_state = State.HIDDEN;
    }

    private void FadeBackground(bool isFaded)
    {
        Hashtable hashtable;
        if (isFaded)
        {
            object[] args = new object[] { "amount", 0.5f, "time", 0.25f, "easeType", iTween.EaseType.easeOutCubic };
            hashtable = iTween.Hash(args);
        }
        else
        {
            object[] objArray2 = new object[] { "amount", 1f, "time", 0.25f, "easeType", iTween.EaseType.easeOutCubic };
            hashtable = iTween.Hash(objArray2);
        }
        iTween.FadeTo(this.m_background, hashtable);
    }

    private CurrencyType GetCurrencyToShow()
    {
        if (this.m_overrideCurrencyType.HasValue)
        {
            return this.m_overrideCurrencyType.Value;
        }
        SceneMgr.Mode mode = SceneMgr.Get().GetMode();
        this.m_backgroundFaded = true;
        CurrencyType nONE = CurrencyType.NONE;
        switch (mode)
        {
            case SceneMgr.Mode.HUB:
                nONE = CurrencyType.GOLD;
                this.m_backgroundFaded = false;
                break;

            case SceneMgr.Mode.COLLECTIONMANAGER:
                nONE = (UniversalInputManager.UsePhoneUI == null) ? CurrencyType.ARCANE_DUST : CurrencyType.NONE;
                break;

            case SceneMgr.Mode.PACKOPENING:
            case SceneMgr.Mode.TOURNAMENT:
            case SceneMgr.Mode.FRIENDLY:
            case SceneMgr.Mode.DRAFT:
            case SceneMgr.Mode.ADVENTURE:
                nONE = (UniversalInputManager.UsePhoneUI == null) ? CurrencyType.GOLD : CurrencyType.NONE;
                break;

            case SceneMgr.Mode.TAVERN_BRAWL:
                if (UniversalInputManager.UsePhoneUI == null)
                {
                    if ((TavernBrawlDisplay.Get() != null) && TavernBrawlDisplay.Get().IsInDeckEditMode())
                    {
                        nONE = CurrencyType.ARCANE_DUST;
                    }
                    else
                    {
                        nONE = CurrencyType.GOLD;
                    }
                }
                else
                {
                    nONE = CurrencyType.NONE;
                }
                break;

            default:
                nONE = CurrencyType.NONE;
                break;
        }
        if ((UniversalInputManager.UsePhoneUI != null) && (nONE == CurrencyType.ARCANE_DUST))
        {
            nONE = CurrencyType.NONE;
        }
        return nONE;
    }

    public GameObject GetTooltipObject()
    {
        TooltipZone component = base.GetComponent<TooltipZone>();
        if (component != null)
        {
            return component.GetTooltipObject();
        }
        return null;
    }

    public void HideTemporarily()
    {
        object[] args = new object[] { "amount", 0f, "time", 0.25f, "easeType", iTween.EaseType.easeOutCubic };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(base.gameObject, hashtable);
        this.m_showingCurrency = CurrencyType.NONE;
    }

    private void OnDestroy()
    {
        if (NetCache.Get() != null)
        {
            NetCache.Get().RemoveGoldBalanceListener(new NetCache.DelGoldBalanceListener(this.OnGoldBalanceChanged));
        }
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        }
    }

    private void OnFrameMouseOut(UIEvent e)
    {
        base.GetComponent<TooltipZone>().HideTooltip();
    }

    private void OnFrameMouseOver(UIEvent e)
    {
        string key = string.Empty;
        string str2 = string.Empty;
        switch (this.m_showingCurrency)
        {
            case CurrencyType.GOLD:
                key = "GLUE_TOOLTIP_GOLD_HEADER";
                str2 = "GLUE_TOOLTIP_GOLD_DESCRIPTION";
                break;

            case CurrencyType.ARCANE_DUST:
                key = "GLUE_CRAFTING_ARCANEDUST";
                str2 = "GLUE_CRAFTING_ARCANEDUST_DESCRIPTION";
                break;
        }
        if (key != string.Empty)
        {
            KeywordHelpPanel panel = base.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get(key), GameStrings.Get(str2), 0.7f, true);
            SceneUtils.SetLayer(panel.gameObject, GameLayer.BattleNet);
            panel.transform.localEulerAngles = new Vector3(270f, 0f, 0f);
            panel.transform.localScale = new Vector3(70f, 70f, 70f);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                TransformUtil.SetPoint((Component) panel, Anchor.TOP, (Component) this.m_mouseOverZone, Anchor.BOTTOM, Vector3.zero);
            }
            else
            {
                TransformUtil.SetPoint((Component) panel, Anchor.BOTTOM, (Component) this.m_mouseOverZone, Anchor.TOP, Vector3.zero);
            }
        }
    }

    private void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
    {
        if (this.m_showingCurrency == CurrencyType.GOLD)
        {
            this.SetAmount(balance.GetTotal());
        }
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        this.m_overrideCurrencyType = null;
        this.m_amount.UpdateNow();
    }

    public void RefreshContents()
    {
        CurrencyType currencyToShow = this.GetCurrencyToShow();
        this.UpdateAmount(currencyToShow);
        this.Show(currencyToShow);
    }

    private void SetAmount(long amount)
    {
        this.m_amount.Text = amount.ToString();
    }

    public void SetCurrencyOverride(CurrencyType? type)
    {
        this.m_overrideCurrencyType = type;
        this.RefreshContents();
        BnetBar.Get().UpdateLayout();
    }

    private void Show(CurrencyType currencyType)
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.ShowImmediate(currencyType);
        }
        else
        {
            bool flag = currencyType != CurrencyType.NONE;
            if (!DemoMgr.Get().IsCurrencyEnabled())
            {
                flag = false;
            }
            if (flag)
            {
                if ((this.m_state == State.SHOWN) || (this.m_state == State.ANIMATE_IN))
                {
                    this.ShowCurrencyType(currencyType);
                }
                else
                {
                    this.m_state = State.ANIMATE_IN;
                    base.gameObject.SetActive(true);
                    object[] args = new object[] { "amount", 1f, "delay", 0f, "time", 0.25f, "easeType", iTween.EaseType.easeOutCubic, "oncomplete", "ActivateCurrencyFrame", "oncompletetarget", base.gameObject };
                    Hashtable hashtable = iTween.Hash(args);
                    iTween.Stop(base.gameObject);
                    iTween.FadeTo(base.gameObject, hashtable);
                    this.ShowCurrencyType(currencyType);
                }
            }
            else if ((this.m_state == State.HIDDEN) || (this.m_state == State.ANIMATE_OUT))
            {
                this.ShowCurrencyType(currencyType);
            }
            else
            {
                this.m_state = State.ANIMATE_OUT;
                object[] objArray2 = new object[] { "amount", 0f, "delay", 0f, "time", 0.25f, "easeType", iTween.EaseType.easeOutCubic, "oncomplete", "DeactivateCurrencyFrame", "oncompletetarget", base.gameObject };
                Hashtable hashtable2 = iTween.Hash(objArray2);
                iTween.Stop(base.gameObject);
                iTween.FadeTo(base.gameObject, hashtable2);
                this.ShowCurrencyType(currencyType);
            }
        }
    }

    private void ShowCurrencyType(CurrencyType currencyType)
    {
        this.FadeBackground(this.m_backgroundFaded);
        if (this.m_showingCurrency != currencyType)
        {
            this.m_showingCurrency = currencyType;
            iTween.FadeTo(this.m_amount.gameObject, 1f, 0.25f);
            switch (this.m_showingCurrency)
            {
                case CurrencyType.GOLD:
                    iTween.FadeTo(this.m_dustJar, 0f, 0.25f);
                    iTween.FadeTo(this.m_goldCoin, 1f, 0.25f);
                    return;

                case CurrencyType.ARCANE_DUST:
                    iTween.FadeTo(this.m_dustJar, 1f, 0.25f);
                    iTween.FadeTo(this.m_goldCoin, 0f, 0.25f);
                    return;
            }
            iTween.FadeTo(this.m_dustJar, 0f, 0.25f);
            iTween.FadeTo(this.m_goldCoin, 0f, 0.25f);
        }
    }

    private void ShowImmediate(CurrencyType currencyType)
    {
        bool flag = currencyType != CurrencyType.NONE;
        this.m_showingCurrency = currencyType;
        base.gameObject.SetActive(flag);
        iTween.Stop(base.gameObject, true);
        Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
        float a = !flag ? 0f : 1f;
        foreach (Renderer renderer in componentsInChildren)
        {
            renderer.material.color = new Color(1f, 1f, 1f, a);
        }
        float num3 = (currencyType != CurrencyType.GOLD) ? 0f : 1f;
        float num4 = (currencyType != CurrencyType.ARCANE_DUST) ? 0f : 1f;
        this.m_goldCoin.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, num3);
        this.m_dustJar.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, num4);
        this.m_state = !flag ? State.HIDDEN : State.SHOWN;
    }

    private void UpdateAmount(CurrencyType currencyType)
    {
        long amount = 0L;
        switch (currencyType)
        {
            case CurrencyType.NONE:
                return;

            case CurrencyType.GOLD:
                amount = NetCache.Get().GetNetObject<NetCache.NetCacheGoldBalance>().GetTotal();
                this.SetAmount(amount);
                return;

            case CurrencyType.ARCANE_DUST:
                if (CraftingManager.Get() == null)
                {
                    amount = NetCache.Get().GetNetObject<NetCache.NetCacheArcaneDustBalance>().Balance;
                    break;
                }
                amount = CraftingManager.Get().GetLocalArcaneDustBalance();
                break;

            default:
                return;
        }
        this.SetAmount(amount);
    }

    public enum CurrencyType
    {
        NONE,
        GOLD,
        ARCANE_DUST
    }

    public enum State
    {
        ANIMATE_IN,
        ANIMATE_OUT,
        HIDDEN,
        SHOWN
    }
}

