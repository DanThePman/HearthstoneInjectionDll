using System;

public class UnearthedRaptorSpell : SuperSpell
{
    public override bool AddPowerTargets()
    {
        return (base.AddPowerTargets() && (base.m_targets.Count > 0));
    }
}

