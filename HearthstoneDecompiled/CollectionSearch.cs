using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class CollectionSearch : MonoBehaviour
{
    private const float ANIM_TIME = 0.1f;
    private List<ActivatedListener> m_activatedListeners = new List<ActivatedListener>();
    private GameLayer m_activeLayer;
    public Color m_altSearchColor;
    public Material m_altSearchMaterial;
    public PegUIElement m_background;
    public PegUIElement m_clearButton;
    private List<ClearedListener> m_clearedListeners = new List<ClearedListener>();
    private List<DeactivatedListener> m_deactivatedListeners = new List<DeactivatedListener>();
    private bool m_isActive;
    private bool m_isTouchKeyboardDisplayMode;
    private GameLayer m_originalLayer;
    private Material m_origSearchMaterial;
    private Vector3 m_origSearchPos;
    private string m_prevText;
    public UberText m_searchText;
    private string m_text;
    public GameObject m_xMesh;
    private const int MAX_SEARCH_LENGTH = 0x1f;

    public void Activate(bool ignoreTouchMode = false)
    {
        if (!this.m_isActive)
        {
            this.MoveToActiveLayer(true);
            this.m_isActive = true;
            this.m_prevText = this.m_text;
            foreach (ActivatedListener listener in this.m_activatedListeners.ToArray())
            {
                listener();
            }
            if (!ignoreTouchMode && ((UniversalInputManager.Get().IsTouchMode() && W8Touch.s_isWindows8OrGreater) || W8Touch.Get().IsVirtualKeyboardVisible()))
            {
                this.TouchKeyboardSearchDisplay(true);
            }
            else
            {
                this.ShowInput(true);
            }
        }
    }

    public void Cancel()
    {
        if (this.m_isActive)
        {
            this.m_text = this.m_prevText;
            this.UpdateSearchText();
            this.Deactivate();
        }
    }

    public void ClearFilter(bool updateVisuals = true)
    {
        this.m_text = string.Empty;
        this.UpdateSearchText();
        this.ClearInput();
        foreach (ClearedListener listener in this.m_clearedListeners.ToArray())
        {
            listener(updateVisuals);
        }
        if ((UniversalInputManager.Get().IsTouchMode() && W8Touch.s_isWindows8OrGreater) || W8Touch.Get().IsVirtualKeyboardVisible())
        {
            this.Deactivate();
        }
    }

    private void ClearInput()
    {
        if (this.m_isActive)
        {
            SoundManager.Get().LoadAndPlay("text_box_delete_text");
            UniversalInputManager.Get().SetInputText(string.Empty);
        }
    }

    public void Deactivate()
    {
        if (this.m_isActive)
        {
            this.MoveToOriginalLayer();
            this.m_isActive = false;
            this.HideInput();
            this.ResetSearchDisplay();
            foreach (DeactivatedListener listener in this.m_deactivatedListeners.ToArray())
            {
                listener(this.m_prevText, this.m_text);
            }
            if (UniversalInputManager.UsePhoneUI != null)
            {
                Navigation.GoBack();
            }
        }
    }

    public string GetText()
    {
        return this.m_text;
    }

    private void HideInput()
    {
        UniversalInputManager.Get().CancelTextInput(base.gameObject, false);
        this.m_searchText.gameObject.SetActive(true);
    }

    public bool IsActive()
    {
        return this.m_isActive;
    }

    private void MoveToActiveLayer(bool saveOriginalLayer)
    {
        if (saveOriginalLayer)
        {
            this.m_originalLayer = (GameLayer) base.gameObject.layer;
        }
        SceneUtils.SetLayer(base.gameObject, this.m_activeLayer);
    }

    private void MoveToOriginalLayer()
    {
        SceneUtils.SetLayer(base.gameObject, this.m_originalLayer);
    }

    private void OnActivateAnimComplete()
    {
        this.ShowInput(true);
    }

    private void OnBackgroundReleased(UIEvent e)
    {
        this.Activate(false);
    }

    private void OnClearReleased(UIEvent e)
    {
        this.ClearFilter(true);
    }

    private void OnDeactivateAnimComplete()
    {
        foreach (DeactivatedListener listener in this.m_deactivatedListeners.ToArray())
        {
            listener(this.m_prevText, this.m_text);
        }
    }

    private void OnDestroy()
    {
        W8Touch.Get().VirtualKeyboardDidShow -= new System.Action(this.OnKeyboardShown);
        W8Touch.Get().VirtualKeyboardDidHide -= new System.Action(this.OnKeyboardHidden);
        if (UniversalInputManager.Get() != null)
        {
            UniversalInputManager.Get().CancelTextInput(base.gameObject, false);
        }
    }

    private void OnInputCanceled(bool userRequested, GameObject requester)
    {
        this.Cancel();
    }

    private void OnInputComplete(string input)
    {
        this.m_text = input;
        this.UpdateSearchText();
        SoundManager.Get().LoadAndPlay("text_commit");
        this.Deactivate();
    }

    private void OnInputUpdated(string input)
    {
        this.m_text = input;
        this.UpdateSearchText();
    }

    private void OnKeyboardHidden()
    {
        if (this.m_isActive && this.m_isTouchKeyboardDisplayMode)
        {
            this.ResetSearchDisplay();
        }
    }

    private void OnKeyboardShown()
    {
        if (this.m_isActive && !this.m_isTouchKeyboardDisplayMode)
        {
            this.TouchKeyboardSearchDisplay(false);
        }
    }

    public void RegisterActivatedListener(ActivatedListener listener)
    {
        if (!this.m_activatedListeners.Contains(listener))
        {
            this.m_activatedListeners.Add(listener);
        }
    }

    public void RegisterClearedListener(ClearedListener listener)
    {
        if (!this.m_clearedListeners.Contains(listener))
        {
            this.m_clearedListeners.Add(listener);
        }
    }

    public void RegisterDeactivatedListener(DeactivatedListener listener)
    {
        if (!this.m_deactivatedListeners.Contains(listener))
        {
            this.m_deactivatedListeners.Add(listener);
        }
    }

    public void RemoveActivatedListener(ActivatedListener listener)
    {
        this.m_activatedListeners.Remove(listener);
    }

    public void RemoveClearedListener(ClearedListener listener)
    {
        this.m_clearedListeners.Remove(listener);
    }

    public void RemoveDeactivatedListener(DeactivatedListener listener)
    {
        this.m_deactivatedListeners.Remove(listener);
    }

    private void ResetSearchDisplay()
    {
        if (this.m_isTouchKeyboardDisplayMode)
        {
            this.m_isTouchKeyboardDisplayMode = false;
            this.m_background.GetComponent<Renderer>().material = this.m_origSearchMaterial;
            base.transform.localPosition = this.m_origSearchPos;
            this.HideInput();
            this.ShowInput(false);
            this.m_xMesh.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        }
    }

    public void SetActiveLayer(GameLayer activeLayer)
    {
        if (activeLayer != this.m_activeLayer)
        {
            this.m_activeLayer = activeLayer;
            if (this.IsActive())
            {
                this.MoveToActiveLayer(false);
            }
        }
    }

    public void SetEnabled(bool enabled)
    {
        this.m_background.SetEnabled(enabled);
        this.m_clearButton.SetEnabled(enabled);
    }

    private void ShowInput(bool fromActivate = true)
    {
        Bounds bounds = this.m_searchText.GetBounds();
        this.m_searchText.gameObject.SetActive(false);
        Rect rect = CameraUtils.CreateGUIViewportRect(Box.Get().GetCamera(), bounds.min, bounds.max);
        Color? nullable = null;
        if (W8Touch.Get().IsVirtualKeyboardVisible())
        {
            nullable = new Color?(this.m_altSearchColor);
        }
        UniversalInputManager.TextInputParams parms = new UniversalInputManager.TextInputParams {
            m_owner = base.gameObject,
            m_rect = rect,
            m_updatedCallback = new UniversalInputManager.TextInputUpdatedCallback(this.OnInputUpdated),
            m_completedCallback = new UniversalInputManager.TextInputCompletedCallback(this.OnInputComplete),
            m_canceledCallback = new UniversalInputManager.TextInputCanceledCallback(this.OnInputCanceled),
            m_font = this.m_searchText.GetLocalizedFont(),
            m_text = this.m_text,
            m_color = nullable
        };
        parms.m_showVirtualKeyboard = fromActivate;
        UniversalInputManager.Get().UseTextInput(parms, false);
    }

    private void Start()
    {
        this.m_background.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBackgroundReleased));
        this.m_clearButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnClearReleased));
        W8Touch.Get().VirtualKeyboardDidShow += new System.Action(this.OnKeyboardShown);
        W8Touch.Get().VirtualKeyboardDidHide += new System.Action(this.OnKeyboardHidden);
        if (this.m_background.GetComponent<Renderer>() != null)
        {
            this.m_origSearchMaterial = this.m_background.GetComponent<Renderer>().material;
        }
        this.m_origSearchPos = base.transform.localPosition;
        this.UpdateSearchText();
    }

    private void TouchKeyboardSearchDisplay(bool fromActivate = false)
    {
        if (!this.m_isTouchKeyboardDisplayMode)
        {
            this.m_isTouchKeyboardDisplayMode = true;
            if (this.m_background.GetComponent<Renderer>() != null)
            {
                this.m_background.GetComponent<Renderer>().material = this.m_altSearchMaterial;
            }
            base.transform.localPosition = CollectionManagerDisplay.Get().m_activeSearchBone_Win8.transform.localPosition;
            this.HideInput();
            this.ShowInput(fromActivate || W8Touch.Get().IsVirtualKeyboardVisible());
            this.m_xMesh.GetComponent<Renderer>().material.SetColor("_Color", this.m_altSearchColor);
        }
    }

    private void UpdateSearchText()
    {
        if (string.IsNullOrEmpty(this.m_text))
        {
            this.m_searchText.Text = GameStrings.Get("GLUE_COLLECTION_SEARCH");
            this.m_clearButton.gameObject.SetActive(false);
        }
        else
        {
            this.m_searchText.Text = this.m_text;
            this.m_clearButton.gameObject.SetActive(true);
        }
    }

    public delegate void ActivatedListener();

    public delegate void ClearedListener(bool updateVisuals);

    public delegate void DeactivatedListener(string oldSearchText, string newSearchText);
}

