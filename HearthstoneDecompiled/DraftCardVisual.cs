using System;
using UnityEngine;

public class DraftCardVisual : PegUIElement
{
    private Actor m_actor;
    private int m_cardChoice = -1;
    private bool m_chosen;
    private float m_mouseOverTimer;
    private const float MOUSE_OVER_DELAY = 0.4f;

    public void ChooseThisCard()
    {
        if (!GameUtils.IsAnyTransitionActive())
        {
            Log.Arena.Print(string.Format("Client chooses: {0} ({1})", this.m_actor.GetEntityDef().GetName(), this.m_actor.GetEntityDef().GetCardId()), new object[0]);
            if (this.m_actor.GetEntityDef().IsHero())
            {
                DraftDisplay.Get().OnHeroClicked(this.m_cardChoice);
            }
            else
            {
                this.m_chosen = true;
                DraftManager.Get().MakeChoice(this.m_cardChoice);
            }
        }
    }

    public Actor GetActor()
    {
        return this.m_actor;
    }

    public int GetChoiceNum()
    {
        return this.m_cardChoice;
    }

    public bool IsChosen()
    {
        return this.m_chosen;
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_actor.SetActorState(ActorStateType.CARD_IDLE);
        KeywordHelpPanelManager.Get().HideKeywordHelp();
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        if (this.m_actor.GetEntityDef().IsHero())
        {
            SoundManager.Get().LoadAndPlay("collection_manager_hero_mouse_over");
        }
        else
        {
            SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over");
        }
        this.m_actor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
        KeywordHelpPanelManager.Get().UpdateKeywordHelpForForge(this.m_actor.GetEntityDef(), this.m_actor, this.m_cardChoice);
    }

    protected override void OnPress()
    {
        this.m_mouseOverTimer = UnityEngine.Time.realtimeSinceStartup;
    }

    protected override void OnRelease()
    {
        if (!UniversalInputManager.Get().IsTouchMode() || ((UnityEngine.Time.realtimeSinceStartup - this.m_mouseOverTimer) < 0.4f))
        {
            this.ChooseThisCard();
        }
    }

    public void SetActor(Actor actor)
    {
        this.m_actor = actor;
    }

    public void SetChoiceNum(int num)
    {
        this.m_cardChoice = num;
    }

    public void SetChosenFlag(bool bOn)
    {
        this.m_chosen = bOn;
    }
}

