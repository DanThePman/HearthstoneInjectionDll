using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ButtonListMenu : MonoBehaviour
{
    protected static readonly Vector3 HIDDEN_SCALE = ((Vector3) (0.01f * Vector3.one));
    private List<UIBButton> m_allButtons = new List<UIBButton>();
    protected PegUIElement m_blocker;
    private List<GameObject> m_horizontalDividers = new List<GameObject>();
    private bool m_isShown;
    protected ButtonListMenuDef m_menu;
    protected string m_menuDefPrefab = "ButtonListMenuDef";
    protected Transform m_menuParent;
    protected Vector3 NORMAL_SCALE = Vector3.one;
    protected float PUNCH_SCALE = 1.08f;

    protected ButtonListMenu()
    {
    }

    protected virtual void Awake()
    {
        GameObject obj2 = (GameObject) GameUtils.InstantiateGameObject(this.m_menuDefPrefab, null, false);
        this.m_menu = obj2.GetComponent<ButtonListMenuDef>();
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        this.SetTransform();
        this.m_blocker = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(obj2.layer), "GameMenuInputBlocker", this, obj2.transform, 10f).AddComponent<PegUIElement>();
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        this.m_blocker.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnBlockerRelease));
    }

    public UIBButton CreateMenuButton(string name, string buttonTextString, UIEvent.Handler releaseHandler)
    {
        UIBButton item = (UIBButton) GameUtils.Instantiate(this.m_menu.m_templateButton, this.m_menu.m_buttonContainer.gameObject, false);
        item.SetText(GameStrings.Get(buttonTextString));
        if (name != null)
        {
            item.gameObject.name = name;
        }
        item.AddEventListener(UIEventType.RELEASE, releaseHandler);
        item.transform.localRotation = this.m_menu.m_templateButton.transform.localRotation;
        this.m_allButtons.Add(item);
        return item;
    }

    protected abstract List<UIBButton> GetButtons();
    public virtual void Hide()
    {
        if (base.gameObject != null)
        {
            base.gameObject.SetActive(false);
        }
        UniversalInputManager.Get().SetGameDialogActive(false);
        this.m_isShown = false;
    }

    private void HideAllButtons()
    {
        for (int i = 0; i < this.m_allButtons.Count; i++)
        {
            this.m_allButtons[i].gameObject.SetActive(false);
        }
        for (int j = 0; j < this.m_horizontalDividers.Count; j++)
        {
            this.m_horizontalDividers[j].SetActive(false);
        }
    }

    public bool IsShown()
    {
        return this.m_isShown;
    }

    protected virtual void LayoutMenu()
    {
        this.LayoutMenuButtons();
        this.m_menu.m_buttonContainer.UpdateSlices();
        this.LayoutMenuBackground();
    }

    protected void LayoutMenuBackground()
    {
        OrientedBounds bounds = TransformUtil.ComputeOrientedWorldBounds(this.m_menu.m_buttonContainer.gameObject, true);
        float width = bounds.Extents[0].magnitude * 2f;
        float height = bounds.Extents[2].magnitude * 2f;
        this.m_menu.m_background.SetSize(width, height);
        this.m_menu.m_border.SetSize(width, height);
    }

    protected void LayoutMenuButtons()
    {
        List<UIBButton> buttons = this.GetButtons();
        this.m_menu.m_buttonContainer.ClearSlices();
        int num = 0;
        int num2 = 0;
        while (num < buttons.Count)
        {
            GameObject gameObject = null;
            UIBButton button = buttons[num];
            Vector3 zero = Vector3.zero;
            bool reverse = false;
            if (button == null)
            {
                GameObject obj3;
                if (num2 >= this.m_horizontalDividers.Count)
                {
                    obj3 = (GameObject) GameUtils.Instantiate(this.m_menu.m_templateHorizontalDivider, this.m_menu.m_buttonContainer.gameObject, false);
                    obj3.transform.localRotation = this.m_menu.m_templateHorizontalDivider.transform.localRotation;
                    this.m_horizontalDividers.Add(obj3);
                }
                else
                {
                    obj3 = this.m_horizontalDividers[num2];
                }
                num2++;
                gameObject = obj3;
                zero = this.m_menu.m_horizontalDividerMinPadding;
                reverse = true;
            }
            else
            {
                gameObject = button.gameObject;
            }
            this.m_menu.m_buttonContainer.AddSlice(gameObject, zero, Vector3.zero, reverse);
            gameObject.SetActive(true);
            num++;
        }
    }

    private void OnBlockerRelease(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("Small_Click");
        this.Hide();
    }

    protected virtual void OnDestroy()
    {
        FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        this.Hide();
    }

    protected void SetTransform()
    {
        if (this.m_menuParent == null)
        {
            this.m_menuParent = base.transform;
        }
        TransformUtil.AttachAndPreserveLocalTransform(this.m_menu.transform, this.m_menuParent);
        if (this.m_blocker != null)
        {
            this.m_blocker.transform.localPosition = new Vector3(0f, -5f, 0f);
            this.m_blocker.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        }
        SceneUtils.SetLayer(this, GameLayer.UI);
        this.m_menu.gameObject.transform.localScale = this.NORMAL_SCALE;
    }

    public virtual void Show()
    {
        UniversalInputManager.Get().CancelTextInput(base.gameObject, true);
        this.SetTransform();
        SoundManager.Get().LoadAndPlay("Small_Click");
        base.gameObject.SetActive(true);
        UniversalInputManager.Get().SetGameDialogActive(true);
        this.HideAllButtons();
        this.LayoutMenu();
        this.m_isShown = true;
        Bounds textBounds = this.m_menu.m_headerText.GetTextBounds();
        WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex(textBounds.size.x, 0) };
        TransformUtil.SetLocalScaleToWorldDimension(this.m_menu.m_headerMiddle, dimensions);
        this.m_menu.m_header.UpdateSlices();
        AnimationUtil.ShowWithPunch(this.m_menu.gameObject, HIDDEN_SCALE, (Vector3) (this.PUNCH_SCALE * this.NORMAL_SCALE), this.NORMAL_SCALE, null, true, null, null, null);
    }
}

