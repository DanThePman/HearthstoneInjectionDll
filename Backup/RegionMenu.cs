using System;
using System.Collections.Generic;
using UnityEngine;

public class RegionMenu : ButtonListMenu
{
    private List<UIBButton> m_buttons;
    public Transform m_menuBone;
    protected string m_menuDefPrefabOverride = "ButtonListMenuDef_RegionMenu";

    protected override void Awake()
    {
        Debug.Log("region menu awake!");
        base.m_menuDefPrefab = this.m_menuDefPrefabOverride;
        base.m_menuParent = this.m_menuBone;
        base.Awake();
        base.SetTransform();
        base.m_menu.m_headerText.Text = GameStrings.Get("GLUE_PICK_A_REGION");
    }

    protected override List<UIBButton> GetButtons()
    {
        return this.m_buttons;
    }

    public override void Hide()
    {
        base.Hide();
        SplashScreen.Get().UnHideWebAuth();
        UnityEngine.Object.Destroy(base.gameObject);
    }

    public void SetButtons(List<UIBButton> buttons)
    {
        this.m_buttons = buttons;
    }

    public override void Show()
    {
        base.Show();
        SplashScreen.Get().HideWebAuth();
    }
}

