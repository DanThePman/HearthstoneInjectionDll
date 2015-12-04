using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GoldenHeroEvent : MonoBehaviour
{
    private List<AnimationDoneListener> m_animationDoneListeners = new List<AnimationDoneListener>();
    public GameObject m_burningHero;
    public Transform m_heroBone;
    public PlayMakerFSM m_playmaker;
    private CardDef m_VanillaHeroCardDef;
    private VictoryTwoScoop m_victoryTwoScoop;

    public void AnimationDone()
    {
        this.FireAnimationDoneEvent();
    }

    private void Awake()
    {
        this.LoadVanillaHeroCardDef();
    }

    private void FireAnimationDoneEvent()
    {
        foreach (AnimationDoneListener listener in this.m_animationDoneListeners.ToArray())
        {
            listener();
        }
    }

    public void Hide()
    {
        this.m_playmaker.SendEvent("Done");
        SoundManager.Get().LoadAndPlay("rank_window_shrink");
    }

    public void HideTwoScoop()
    {
        this.m_victoryTwoScoop.Hide();
    }

    private void LoadVanillaHeroCardDef()
    {
        Player player = null;
        foreach (Player player2 in GameState.Get().GetPlayerMap().Values)
        {
            if (player2.GetSide() == Player.Side.FRIENDLY)
            {
                player = player2;
                break;
            }
        }
        if (player == null)
        {
            Debug.LogWarning("GoldenHeroEvent.LoadVanillaHeroCardDef() - currentPlayer == null");
        }
        else
        {
            EntityDef entityDef = player.GetEntityDef();
            if (entityDef.GetCardSet() == TAG_CARD_SET.HERO_SKINS)
            {
                string vanillaHeroCardID = CollectionManager.Get().GetVanillaHeroCardID(entityDef);
                CardPortraitQuality quality = new CardPortraitQuality(3, TAG_PREMIUM.NORMAL);
                DefLoader.Get().LoadCardDef(vanillaHeroCardID, new DefLoader.LoadDefCallback<CardDef>(this.OnVanillaHeroCardDefLoaded), new object(), quality);
            }
        }
    }

    private void OnVanillaHeroCardDefLoaded(string cardId, CardDef def, object userData)
    {
        if (def == null)
        {
            Debug.LogError("GoldenHeroEvent.LoadDefaultHeroTexture() faild to load CardDef!");
        }
        else
        {
            this.m_VanillaHeroCardDef = def;
        }
    }

    public void RegisterAnimationDoneListener(AnimationDoneListener listener)
    {
        if (!this.m_animationDoneListeners.Contains(listener))
        {
            this.m_animationDoneListeners.Add(listener);
        }
    }

    public void RemoveAnimationDoneListener(AnimationDoneListener listener)
    {
        this.m_animationDoneListeners.Remove(listener);
    }

    public void SetHeroBurnAwayTexture(Texture heroTexture)
    {
        this.m_burningHero.GetComponent<Renderer>().material.mainTexture = heroTexture;
    }

    public void SetVictoryTwoScoop(VictoryTwoScoop twoScoop)
    {
        this.m_victoryTwoScoop = twoScoop;
    }

    public void Show()
    {
        base.gameObject.SetActive(true);
        this.m_playmaker.SendEvent("Action");
        this.m_victoryTwoScoop.HideXpBar();
        this.m_victoryTwoScoop.m_bannerLabel.Text = string.Empty;
    }

    public void SwapHeroToVanilla()
    {
        if (this.m_VanillaHeroCardDef != null)
        {
            this.m_victoryTwoScoop.m_heroActor.SetCardDef(this.m_VanillaHeroCardDef);
            this.m_victoryTwoScoop.m_heroActor.UpdateAllComponents();
        }
    }

    public void SwapMaterialToPremium()
    {
        this.m_victoryTwoScoop.m_heroActor.SetCardFlair(new CardFlair(TAG_PREMIUM.GOLDEN));
        this.m_victoryTwoScoop.m_heroActor.UpdateAllComponents();
    }

    public delegate void AnimationDoneListener();
}

