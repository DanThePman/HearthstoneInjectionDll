using System;
using System.Collections.Generic;

public class AmbushSpell : OverrideCustomSpawnSpell
{
    public override bool AddPowerTargets()
    {
        if (!base.m_taskList.IsSourceActionOrigin())
        {
            return false;
        }
        List<PowerTask> taskList = base.m_taskList.GetTaskList();
        base.AddMultiplePowerTargets_FromMetaData(taskList);
        return true;
    }
}

