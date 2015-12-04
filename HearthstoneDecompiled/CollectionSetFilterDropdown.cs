using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class CollectionSetFilterDropdown : MonoBehaviour
{
    public MultiSliceElement m_background;
    public GameObject m_backgroundMiddleSection;
    public float m_backgroundPadding;
    public PegUIElement m_hideDropDownButton;
    public MultiSliceElement m_itemContainer;
    private List<CollectionSetFilterDropdownItem> m_items = new List<CollectionSetFilterDropdownItem>();
    public CollectionSetFilterDropdownItem m_itemTemplate;
    public CollectionSetFilterDropdownToggle m_showDropDownButton;
    private bool m_showing;
    public UIBHighlight m_toggleButtonHighlight;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_toggleSound;

    public void AddItem(string itemName, Vector2? materialOffset, ItemSelectedCallback callback, List<object> data)
    {
        <AddItem>c__AnonStorey2DF storeydf = new <AddItem>c__AnonStorey2DF {
            callback = callback,
            data = data,
            <>f__this = this
        };
        CollectionSetFilterDropdownItem item = (CollectionSetFilterDropdownItem) GameUtils.Instantiate(this.m_itemTemplate, this.m_itemContainer.gameObject, true);
        item.SetName(itemName);
        if (materialOffset.HasValue)
        {
            item.SetIconMaterialOffset(materialOffset.Value);
        }
        else
        {
            item.DisableIconMaterial();
        }
        storeydf.itemIndex = this.m_items.Count;
        item.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(storeydf.<>m__BB));
        item.gameObject.SetActive(true);
        this.m_itemContainer.AddSlice(item.gameObject);
        this.m_items.Add(item);
    }

    private void Awake()
    {
        base.gameObject.SetActive(false);
        this.m_showDropDownButton.AddEventListener(UIEventType.PRESS, e => this.Show(true));
        this.m_hideDropDownButton.AddEventListener(UIEventType.PRESS, e => this.Show(false));
        this.m_hideDropDownButton.gameObject.SetActive(false);
    }

    public void ClearFilter()
    {
        if (this.m_items.Count > 0)
        {
            this.Select(0);
        }
    }

    public bool IsShowing()
    {
        return this.m_showing;
    }

    public void Select(int index)
    {
        foreach (CollectionSetFilterDropdownItem item in this.m_items)
        {
            item.Select(false);
        }
        if (index >= this.m_items.Count)
        {
            Debug.LogError(string.Format("Out of range dropdown index: {0}", index));
        }
        else
        {
            CollectionSetFilterDropdownItem item2 = this.m_items[index];
            item2.Select(true);
            this.m_showDropDownButton.SetToggleIconOffset(item2.GetIconMaterialOffset());
        }
    }

    public void Show(bool show)
    {
        if (this.m_showing != show)
        {
            this.m_showing = show;
            base.gameObject.SetActive(this.m_showing);
            this.m_hideDropDownButton.gameObject.SetActive(show);
            this.m_toggleButtonHighlight.AlwaysOver = show;
            if (show)
            {
                this.m_itemContainer.UpdateSlices();
                OrientedBounds bounds = TransformUtil.ComputeOrientedWorldBounds(this.m_itemContainer.gameObject, true);
                WorldDimensionIndex[] dimensions = new WorldDimensionIndex[] { new WorldDimensionIndex((bounds.Extents[2].magnitude * 2f) + this.m_backgroundPadding, 2) };
                TransformUtil.SetLocalScaleToWorldDimension(this.m_backgroundMiddleSection, dimensions);
                this.m_background.UpdateSlices();
            }
            if (!string.IsNullOrEmpty(this.m_toggleSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_toggleSound));
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AddItem>c__AnonStorey2DF
    {
        internal CollectionSetFilterDropdown <>f__this;
        internal CollectionSetFilterDropdown.ItemSelectedCallback callback;
        internal List<object> data;
        internal int itemIndex;

        internal void <>m__BB(UIEvent e)
        {
            this.<>f__this.Select(this.itemIndex);
            if (this.callback != null)
            {
                this.callback(this.data);
            }
        }
    }

    public delegate void ItemSelectedCallback(object data);
}

