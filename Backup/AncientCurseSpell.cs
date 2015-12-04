using System;
using UnityEngine;

public class AncientCurseSpell : SuperSpell
{
    public void DoHeroDamage()
    {
        Log.JMac.Print("AncientCurseSpell - DoHeroDamage()!", new object[0]);
        PowerTaskList currentTaskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
        if (currentTaskList == null)
        {
            Debug.LogWarning("AncientCurseSpell.DoHeroDamage() called when there was no current PowerTaskList!");
        }
        else
        {
            GameUtils.DoDamageTasks(currentTaskList, base.GetSourceCard(), this.GetVisualTargetCard());
        }
    }
}

