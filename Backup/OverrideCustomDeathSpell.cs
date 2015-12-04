using System;
using UnityEngine;

public class OverrideCustomDeathSpell : SuperSpell
{
    public Spell m_CustomDeathSpell;
    public bool m_SuppressKeywordDeaths = true;

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        foreach (GameObject obj2 in this.GetVisualTargets())
        {
            if (obj2 != null)
            {
                Card component = obj2.GetComponent<Card>();
                component.SuppressKeywordDeaths(this.m_SuppressKeywordDeaths);
                component.OverrideCustomDeathSpell(UnityEngine.Object.Instantiate<Spell>(this.m_CustomDeathSpell));
            }
        }
        base.m_effectsPendingFinish--;
        base.FinishIfPossible();
    }
}

