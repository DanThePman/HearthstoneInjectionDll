using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class StoreQuantityPrompt : UIBPopup
{
    public UIBButton m_cancelButton;
    private CancelListener m_currentCancelListener;
    private int m_currentMaxQuantity;
    private OkayListener m_currentOkayListener;
    public UberText m_messageText;
    public UIBButton m_okayButton;
    public UberText m_quantityText;

    private void Awake()
    {
        this.m_quantityText.RichText = false;
        this.m_okayButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnSubmitPressed));
        this.m_cancelButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelPressed));
        Debug.Log(string.Concat(new object[] { "show postition = ", base.m_showPosition, " ", base.m_showScale }));
    }

    private void Cancel()
    {
        this.Hide(true);
        this.FireCancelEvent();
    }

    private void ClearInput()
    {
        UniversalInputManager.Get().SetInputText(string.Empty);
    }

    private void FireCancelEvent()
    {
        if (this.m_currentCancelListener != null)
        {
            this.m_currentCancelListener();
        }
        this.m_currentCancelListener = null;
    }

    private void FireOkayEvent(int quantity)
    {
        if (this.m_currentOkayListener != null)
        {
            this.m_currentOkayListener(quantity);
        }
        this.m_currentOkayListener = null;
    }

    private bool GetQuantity(out int quantity)
    {
        quantity = -1;
        if (!int.TryParse(this.m_quantityText.Text, out quantity))
        {
            Debug.LogWarning(string.Format("GeneralStore.OnStoreQuantityOkayPressed: invalid quantity='{0}'", this.m_quantityText.Text));
            return false;
        }
        if (quantity <= 0)
        {
            Log.Rachelle.Print(string.Format("GeneralStore.OnStoreQuantityOkayPressed: quantity {0} must be positive", (int) quantity), new object[0]);
            return false;
        }
        if (quantity > this.m_currentMaxQuantity)
        {
            Log.Rachelle.Print(string.Format("GeneralStore.OnStoreQuantityOkayPressed: quantity {0} is larger than max allowed quantity ({1})", (int) quantity, this.m_currentMaxQuantity), new object[0]);
            return false;
        }
        return true;
    }

    protected override void Hide(bool animate)
    {
        if (base.m_shown)
        {
            base.m_shown = false;
            this.HideInput();
            base.DoHideAnimation(!animate, () => base.gameObject.SetActive(false));
        }
    }

    private void HideInput()
    {
        UniversalInputManager.Get().CancelTextInput(base.gameObject, false);
        this.m_quantityText.gameObject.SetActive(true);
    }

    private void OnCancelPressed(UIEvent e)
    {
        this.Cancel();
    }

    private void OnInputCanceled(bool userRequested, GameObject requester)
    {
        this.m_quantityText.Text = string.Empty;
        this.Cancel();
    }

    private void OnInputComplete(string input)
    {
        this.m_quantityText.Text = input;
        this.Submit();
    }

    private void OnInputUpdated(string input)
    {
        this.m_quantityText.Text = input;
    }

    private void OnSubmitPressed(UIEvent e)
    {
        this.Submit();
    }

    public bool Show(int maxQuantity, OkayListener delOkay = null, CancelListener delCancel = null)
    {
        if (base.m_shown)
        {
            return false;
        }
        this.m_currentMaxQuantity = maxQuantity;
        object[] args = new object[] { maxQuantity };
        this.m_messageText.Text = GameStrings.Format("GLUE_STORE_QUANTITY_MESSAGE", args);
        base.m_shown = true;
        this.m_currentOkayListener = delOkay;
        this.m_currentCancelListener = delCancel;
        this.m_quantityText.Text = string.Empty;
        base.gameObject.SetActive(true);
        Debug.Log(string.Concat(new object[] { "show postition2 = ", base.m_showPosition, " ", base.m_showScale }));
        base.DoShowAnimation(new UIBPopup.OnAnimationComplete(this.ShowInput));
        return true;
    }

    private void ShowInput()
    {
        this.m_quantityText.gameObject.SetActive(false);
        Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
        Bounds bounds = this.m_quantityText.GetBounds();
        Rect rect = CameraUtils.CreateGUIViewportRect(camera, bounds.min, bounds.max);
        UniversalInputManager.TextInputParams parms = new UniversalInputManager.TextInputParams {
            m_owner = base.gameObject,
            m_number = true,
            m_rect = rect,
            m_updatedCallback = new UniversalInputManager.TextInputUpdatedCallback(this.OnInputUpdated),
            m_completedCallback = new UniversalInputManager.TextInputCompletedCallback(this.OnInputComplete),
            m_canceledCallback = new UniversalInputManager.TextInputCanceledCallback(this.OnInputCanceled),
            m_font = this.m_quantityText.GetLocalizedFont(),
            m_alignment = 4,
            m_maxCharacters = 2,
            m_touchScreenKeyboardHideInput = true
        };
        UniversalInputManager.Get().UseTextInput(parms, false);
    }

    private void Submit()
    {
        this.Hide(true);
        int quantity = -1;
        if (this.GetQuantity(out quantity))
        {
            this.FireOkayEvent(quantity);
        }
        else
        {
            this.FireCancelEvent();
        }
    }

    public delegate void CancelListener();

    public delegate void OkayListener(int quantity);
}

