using System;

public class PearlOfTidesSpell : SuperSpell
{
    protected override void OnAction(SpellStateType prevStateType)
    {
        foreach (PowerTask task in base.m_taskList.GetTaskList())
        {
            Network.HistFullEntity power = task.GetPower() as Network.HistFullEntity;
            if (power != null)
            {
                GameState.Get().GetEntity(power.Entity.ID).GetCard().SuppressPlaySounds(true);
            }
        }
        base.OnAction(prevStateType);
    }
}

