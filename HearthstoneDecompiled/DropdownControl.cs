using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class DropdownControl : PegUIElement
{
    [CompilerGenerated]
    private static itemChosenCallback <>f__am$cacheD;
    [CompilerGenerated]
    private static menuShownCallback <>f__am$cacheE;
    [CompilerGenerated]
    private static itemChosenCallback <>f__am$cacheF;
    [CustomEditField(Sections="Buttons")]
    public UIBButton m_button;
    [CustomEditField(Sections="Buttons")]
    public PegUIElement m_cancelCatcher;
    private itemChosenCallback m_itemChosenCallback;
    private List<DropdownMenuItem> m_items;
    private itemTextCallback m_itemTextCallback;
    [CustomEditField(Sections="Menu")]
    public MultiSliceElement m_menu;
    [CustomEditField(Sections="Menu")]
    public MultiSliceElement m_menuItemContainer;
    [CustomEditField(Sections="Menu Templates")]
    public DropdownMenuItem m_menuItemTemplate;
    [CustomEditField(Sections="Menu")]
    public GameObject m_menuMiddle;
    private menuShownCallback m_menuShownCallback;
    private Font m_overrideFont;
    private FontDef m_overrideFontNoLocalization;
    [CustomEditField(Sections="Buttons")]
    public DropdownMenuItem m_selectedItem;

    public DropdownControl()
    {
        if (<>f__am$cacheD == null)
        {
            <>f__am$cacheD = new itemChosenCallback(DropdownControl.<m_itemChosenCallback>m__2B3);
        }
        this.m_itemChosenCallback = <>f__am$cacheD;
        this.m_itemTextCallback = new itemTextCallback(DropdownControl.defaultItemTextCallback);
        if (<>f__am$cacheE == null)
        {
            <>f__am$cacheE = new menuShownCallback(DropdownControl.<m_menuShownCallback>m__2B4);
        }
        this.m_menuShownCallback = <>f__am$cacheE;
        this.m_items = new List<DropdownMenuItem>();
    }

    [CompilerGenerated]
    private static void <m_itemChosenCallback>m__2B3(object, object)
    {
    }

    [CompilerGenerated]
    private static void <m_menuShownCallback>m__2B4(bool)
    {
    }

    public void addItem(object value)
    {
        <addItem>c__AnonStorey398 storey = new <addItem>c__AnonStorey398 {
            <>f__this = this,
            item = (DropdownMenuItem) GameUtils.Instantiate(this.m_menuItemTemplate, this.m_menuItemContainer.gameObject, false)
        };
        storey.item.gameObject.transform.localRotation = this.m_menuItemTemplate.transform.localRotation;
        storey.item.gameObject.transform.localScale = this.m_menuItemTemplate.transform.localScale;
        this.m_items.Add(storey.item);
        if (this.m_overrideFontNoLocalization != null)
        {
            storey.item.m_text.SetFontWithoutLocalization(this.m_overrideFontNoLocalization);
        }
        else if (this.m_overrideFont != null)
        {
            storey.item.m_text.TrueTypeFont = this.m_overrideFont;
        }
        storey.item.SetValue(value, this.m_itemTextCallback(value));
        storey.item.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(storey.<>m__2B8));
        storey.item.gameObject.SetActive(true);
        this.layoutMenu();
    }

    public void clearItems()
    {
        foreach (DropdownMenuItem item in this.m_items)
        {
            UnityEngine.Object.Destroy(item.gameObject);
        }
        this.layoutMenu();
    }

    public static string defaultItemTextCallback(object val)
    {
        return ((val != null) ? val.ToString() : string.Empty);
    }

    private DropdownMenuItem findItem(object value)
    {
        for (int i = 0; i < this.m_items.Count; i++)
        {
            DropdownMenuItem item = this.m_items[i];
            if (item.GetValue() == value)
            {
                return item;
            }
        }
        return null;
    }

    private int findItemIndex(object value)
    {
        for (int i = 0; i < this.m_items.Count; i++)
        {
            DropdownMenuItem item = this.m_items[i];
            if (item.GetValue() == value)
            {
                return i;
            }
        }
        return -1;
    }

    public itemChosenCallback getItemChosenCallback()
    {
        return this.m_itemChosenCallback;
    }

    public itemTextCallback getItemTextCallback()
    {
        return this.m_itemTextCallback;
    }

    public menuShownCallback getMenuShownCallback()
    {
        return this.m_menuShownCallback;
    }

    public object getSelection()
    {
        return this.m_selectedItem.GetValue();
    }

    private void hideMenu()
    {
        this.m_cancelCatcher.gameObject.SetActive(false);
        this.m_menu.gameObject.SetActive(false);
        this.m_menuShownCallback(false);
    }

    public bool isMenuShown()
    {
        return this.m_menu.gameObject.activeInHierarchy;
    }

    private void layoutMenu()
    {
        if (base.gameObject.activeSelf)
        {
            this.m_menuItemTemplate.gameObject.SetActive(true);
            OrientedBounds bounds = TransformUtil.ComputeOrientedWorldBounds(this.m_menuItemTemplate.gameObject, true);
            if (bounds != null)
            {
                float num = bounds.Extents[1].magnitude * 2f;
                this.m_menuItemTemplate.gameObject.SetActive(false);
                this.m_menuItemContainer.ClearSlices();
                for (int i = 0; i < this.m_items.Count; i++)
                {
                    this.m_menuItemContainer.AddSlice(this.m_items[i].gameObject);
                }
                this.m_menuItemContainer.UpdateSlices();
                if (this.m_items.Count <= 1)
                {
                    TransformUtil.SetLocalScaleZ(this.m_menuMiddle, 0.001f);
                }
                else
                {
                    WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex(num * (this.m_items.Count - 1), 2) };
                    TransformUtil.SetLocalScaleToWorldDimension(this.m_menuMiddle, dimensions);
                }
                this.m_menu.UpdateSlices();
            }
        }
    }

    public void onUserCancelled()
    {
        if (SoundManager.Get().IsInitialized())
        {
            SoundManager.Get().LoadAndPlay("Small_Click");
        }
        this.hideMenu();
    }

    public void onUserItemClicked(DropdownMenuItem item)
    {
        this.hideMenu();
        object prevSelection = this.getSelection();
        object val = item.GetValue();
        this.setSelection(val);
        this.m_itemChosenCallback(val, prevSelection);
    }

    public void onUserPressedButton()
    {
        this.showMenu();
    }

    public void onUserPressedSelection(DropdownMenuItem item)
    {
        this.showMenu();
    }

    public bool removeItem(object value)
    {
        int index = this.findItemIndex(value);
        if (index < 0)
        {
            return false;
        }
        DropdownMenuItem item = this.m_items[index];
        this.m_items.RemoveAt(index);
        UnityEngine.Object.Destroy(item.gameObject);
        this.layoutMenu();
        return true;
    }

    public void setFont(Font font)
    {
        this.m_overrideFont = font;
        this.m_overrideFontNoLocalization = null;
        this.m_selectedItem.m_text.TrueTypeFont = font;
        this.m_menuItemTemplate.m_text.TrueTypeFont = font;
    }

    public void setFontWithoutLocalization(FontDef fontDef)
    {
        this.m_overrideFontNoLocalization = fontDef;
        this.m_overrideFont = null;
        this.m_selectedItem.m_text.SetFontWithoutLocalization(fontDef);
        this.m_menuItemTemplate.m_text.SetFontWithoutLocalization(fontDef);
    }

    public void setItemChosenCallback(itemChosenCallback callback)
    {
        if ((callback == null) && (<>f__am$cacheF == null))
        {
            <>f__am$cacheF = delegate {
            };
        }
        this.m_itemChosenCallback = <>f__am$cacheF;
    }

    public void setItemTextCallback(itemTextCallback callback)
    {
        if (callback == null)
        {
        }
        this.m_itemTextCallback = new itemTextCallback(DropdownControl.defaultItemTextCallback);
    }

    public void setMenuShownCallback(menuShownCallback callback)
    {
        this.m_menuShownCallback = callback;
    }

    public void setSelection(object val)
    {
        this.m_selectedItem.SetValue(null, string.Empty);
        for (int i = 0; i < this.m_items.Count; i++)
        {
            DropdownMenuItem item = this.m_items[i];
            object obj2 = item.GetValue();
            if (((obj2 == null) && (val == null)) || obj2.Equals(val))
            {
                item.SetSelected(true);
                this.m_selectedItem.SetValue(obj2, this.m_itemTextCallback(obj2));
            }
            else
            {
                item.SetSelected(false);
            }
        }
    }

    public void setSelectionToLastItem()
    {
        this.m_selectedItem.SetValue(null, string.Empty);
        if (this.m_items.Count != 0)
        {
            for (int i = 0; i < (this.m_items.Count - 1); i++)
            {
                this.m_items[i].SetSelected(false);
            }
            DropdownMenuItem item = this.m_items[this.m_items.Count - 1];
            item.SetSelected(true);
            this.m_selectedItem.SetValue(item.GetValue(), this.m_itemTextCallback(item.GetValue()));
        }
    }

    private void showMenu()
    {
        this.m_cancelCatcher.gameObject.SetActive(true);
        this.m_menu.gameObject.SetActive(true);
        this.layoutMenu();
        this.m_menuShownCallback(true);
    }

    public void Start()
    {
        this.m_button.AddEventListener(UIEventType.RELEASE, e => this.onUserPressedButton());
        this.m_selectedItem.AddEventListener(UIEventType.RELEASE, e => this.onUserPressedSelection(this.m_selectedItem));
        this.m_cancelCatcher.AddEventListener(UIEventType.RELEASE, e => this.onUserCancelled());
        this.hideMenu();
    }

    [CompilerGenerated]
    private sealed class <addItem>c__AnonStorey398
    {
        internal DropdownControl <>f__this;
        internal DropdownMenuItem item;

        internal void <>m__2B8(UIEvent e)
        {
            this.<>f__this.onUserItemClicked(this.item);
        }
    }

    public delegate void itemChosenCallback(object selection, object prevSelection);

    public delegate string itemTextCallback(object val);

    public delegate void menuShownCallback(bool shown);
}

