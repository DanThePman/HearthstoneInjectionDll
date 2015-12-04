using System;
using UnityEngine;

public class OverrideCustomSpawnSpell : SuperSpell
{
    public Spell m_CustomSpawnSpell;
    public bool m_SuppressPlaySounds;

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        foreach (GameObject obj2 in this.GetVisualTargets())
        {
            if (obj2 != null)
            {
                Card component = obj2.GetComponent<Card>();
                component.OverrideCustomSpawnSpell(UnityEngine.Object.Instantiate<Spell>(this.m_CustomSpawnSpell));
                component.SuppressPlaySounds(this.m_SuppressPlaySounds);
            }
        }
        base.m_effectsPendingFinish--;
        base.FinishIfPossible();
    }
}

