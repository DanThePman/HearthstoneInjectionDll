using System;

public class RainOfFire : SuperSpell
{
    private int NumberOfCardsInOpponentsHand()
    {
        return GameState.Get().GetFirstOpponentPlayer(base.GetSourceCard().GetController()).GetHandZone().GetCardCount();
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        base.m_effectsPendingFinish--;
        base.FinishIfPossible();
    }

    protected override void UpdateVisualTargets()
    {
        int num = this.NumberOfCardsInOpponentsHand();
        base.m_TargetInfo.m_RandomTargetCountMin = num;
        base.m_TargetInfo.m_RandomTargetCountMax = num;
        ZonePlay zonePlay = SpellUtils.FindOpponentPlayZone(this);
        base.GenerateRandomPlayZoneVisualTargets(zonePlay);
        for (int i = 0; i < base.m_targets.Count; i++)
        {
            if (i < base.m_visualTargets.Count)
            {
                base.m_visualTargets[i] = base.m_targets[i];
            }
            else
            {
                this.AddVisualTarget(base.m_targets[i]);
            }
        }
    }
}

