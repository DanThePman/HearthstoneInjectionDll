using System;

public class OnOffToggleButton : CheckBox
{
    public string m_offLabel = "GLOBAL_OFF";
    public string m_onLabel = "GLOBAL_ON";

    public override void SetChecked(bool isChecked)
    {
        base.SetChecked(isChecked);
        base.SetButtonText(GameStrings.Get(!isChecked ? this.m_offLabel : this.m_onLabel));
    }
}

