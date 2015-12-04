using System;

public class AddToDeckButton : PegUIElement
{
    private CollectionDeck m_deck;
    public UberText m_deckCount;
    private EntityDef m_entityDef;
    private CardFlair m_flair;

    protected override void Awake()
    {
        this.AddEventListener(UIEventType.RELEASE, e => this.HandleRelease());
        base.Awake();
    }

    public void HandleRelease()
    {
        if (this.m_deck != null)
        {
            CollectionDeckTray.Get().AddCard(this.m_entityDef, this.m_flair, null, false, null);
            this.UpdateDeckCount();
        }
    }

    public void Hide()
    {
        this.m_deck = null;
        base.gameObject.SetActive(false);
    }

    public void Show(CollectionDeck deck, EntityDef entity, CardFlair flair)
    {
        this.m_deck = deck;
        this.m_entityDef = entity;
        this.m_flair = flair;
        this.UpdateDeckCount();
        base.gameObject.SetActive(true);
    }

    private void UpdateDeckCount()
    {
        if (this.m_deck == null)
        {
            this.m_deckCount.Text = string.Empty;
        }
        else
        {
            this.m_deckCount.Text = this.m_deck.GetTotalCardCount() + "/30";
        }
    }
}

