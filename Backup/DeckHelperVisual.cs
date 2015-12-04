using System;

public class DeckHelperVisual : PegUIElement
{
    private Actor m_actor;
    private bool m_chosen;
    private Actor m_movingDeckTile;

    public void ChooseThisCard()
    {
        KeywordHelpPanelManager.Get().HideKeywordHelp();
        this.m_chosen = true;
        this.m_actor.GetSpell(SpellType.DEATHREVERSE).ActivateState(SpellStateType.BIRTH);
        CollectionDeckTray.Get().AddCard(this.m_actor.GetEntityDef(), this.m_actor.GetCardFlair(), null, false, this.m_actor);
        DeckHelper.Get().UpdateChoices();
    }

    public Actor GetActor()
    {
        return this.m_actor;
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
        SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over");
        this.m_actor.SetActorState(ActorStateType.CARD_MOUSE_OVER);
        KeywordHelpPanelManager.Get().UpdateKeywordHelpForDeckHelper(this.m_actor.GetEntityDef(), this.m_actor);
    }

    protected override void OnRelease()
    {
        this.ChooseThisCard();
    }

    public void SetActor(Actor actor)
    {
        this.m_actor = actor;
    }
}

