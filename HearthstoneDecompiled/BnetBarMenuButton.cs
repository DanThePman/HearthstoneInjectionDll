using System;
using System.Collections;
using UnityEngine;

public class BnetBarMenuButton : PegUIElement
{
    public GameObject m_highlight;
    public GameObject m_phoneBar;
    public Transform m_phoneBarOneElementBone;
    private int m_phoneBarStatus = -1;
    public Transform m_phoneBarTwoElementBone;
    private bool m_selected;

    protected override void Awake()
    {
        base.Awake();
        this.UpdateHighlight();
    }

    public bool IsSelected()
    {
        return this.m_selected;
    }

    public void LockHighlight(bool isLocked)
    {
        this.m_highlight.SetActive(isLocked);
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        base.GetComponent<TooltipZone>().HideTooltip();
        this.UpdateHighlight();
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        KeywordHelpPanel src = base.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get("GLOBAL_TOOLTIP_MENU_HEADER"), GameStrings.Get("GLOBAL_TOOLTIP_MENU_DESC"), 0.7f, true);
        SceneUtils.SetLayer(src.gameObject, GameLayer.BattleNet);
        src.transform.localEulerAngles = new Vector3(270f, 0f, 0f);
        src.transform.localScale = new Vector3(82.35294f, 70f, 90.32258f);
        TransformUtil.SetPoint(src, Anchor.BOTTOM, base.gameObject, Anchor.TOP, new Vector3(-98.22766f, 0f, 0f));
        SoundManager.Get().LoadAndPlay("Small_Mouseover");
        this.UpdateHighlight();
    }

    public void OnStatusBarUpdate()
    {
        BnetBar.Get().UpdateLayout();
    }

    public void SetPhoneStatusBarState(int nElements)
    {
        if (nElements != this.m_phoneBarStatus)
        {
            Hashtable hashtable;
            this.m_phoneBarStatus = nElements;
            switch (nElements)
            {
                case 0:
                    this.m_phoneBar.SetActive(false);
                    break;

                case 1:
                {
                    this.m_phoneBar.SetActive(true);
                    iTween.Stop(this.m_phoneBar);
                    object[] args = new object[] { "position", this.m_phoneBarOneElementBone.position, "time", 1f, "isLocal", false, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "OnStatusBarUpdate", "onupdatetarget", base.gameObject };
                    hashtable = iTween.Hash(args);
                    iTween.MoveTo(this.m_phoneBar, hashtable);
                    break;
                }
                case 2:
                {
                    this.m_phoneBar.SetActive(true);
                    iTween.Stop(this.m_phoneBar);
                    object[] objArray2 = new object[] { "position", this.m_phoneBarTwoElementBone.position, "time", 1f, "isLocal", false, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "OnStatusBarUpdate", "onupdatetarget", base.gameObject };
                    hashtable = iTween.Hash(objArray2);
                    iTween.MoveTo(this.m_phoneBar, hashtable);
                    break;
                }
                default:
                    Debug.LogError("Invalid phone status bar state " + nElements);
                    break;
            }
        }
    }

    public void SetSelected(bool enable)
    {
        if (enable != this.m_selected)
        {
            this.m_selected = enable;
            this.UpdateHighlight();
        }
    }

    private bool ShouldBeHighlighted()
    {
        return (this.m_selected || (base.GetInteractionState() == PegUIElement.InteractionState.Over));
    }

    protected virtual void UpdateHighlight()
    {
        bool flag = this.ShouldBeHighlighted();
        if ((!flag && (GameMenu.Get() != null)) && GameMenu.Get().IsShown())
        {
            flag = true;
        }
        if (this.m_highlight.activeSelf != flag)
        {
            this.m_highlight.SetActive(flag);
        }
    }
}

