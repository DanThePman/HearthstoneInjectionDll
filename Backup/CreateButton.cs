using System;

public class CreateButton : CraftingButton
{
    public override void EnableButton()
    {
        if (CraftingManager.Get().GetNumTransactions() < 0)
        {
            base.EnterUndoMode();
        }
        else
        {
            base.labelText.Text = GameStrings.Get("GLUE_CRAFTING_CREATE");
            base.EnableButton();
        }
    }

    protected override void OnRelease()
    {
        if (CraftingManager.Get().GetPendingTransaction() == null)
        {
            CraftingManager.Get().CreateButtonPressed();
            if (UniversalInputManager.UsePhoneUI != null)
            {
                base.GetComponent<Animation>().Play("CardExchange_ButtonPress2_phone");
            }
            else
            {
                base.GetComponent<Animation>().Play("CardExchange_ButtonPress2");
            }
        }
    }
}

