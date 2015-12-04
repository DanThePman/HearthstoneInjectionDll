using System;

public class RankedWinsPlate : PegUIElement
{
    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        base.GetComponent<TooltipZone>().HideTooltip();
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        base.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get("GLUE_TOOLTIP_GOLDEN_WINS_HEADER"), GameStrings.Get("GLUE_TOOLTIP_GOLDEN_WINS_DESC"), 5f, true);
    }
}

