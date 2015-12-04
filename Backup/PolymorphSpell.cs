using System;

public class PolymorphSpell : SuperSpell
{
    public bool m_SuppressNewCardPlaySound;

    private Card FindNewCard()
    {
        foreach (PowerTask task in base.m_taskList.GetTaskList())
        {
            Network.HistFullEntity power = task.GetPower() as Network.HistFullEntity;
            if (power != null)
            {
                return GameState.Get().GetEntity(power.Entity.ID).GetCard();
            }
        }
        return null;
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        if (this.m_SuppressNewCardPlaySound)
        {
            Card card = this.FindNewCard();
            if (card != null)
            {
                card.SuppressPlaySounds(true);
            }
        }
        base.OnAction(prevStateType);
    }
}

