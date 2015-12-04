using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FriendListItemHeader : PegUIElement, ITouchListItem
{
    public float m_AnimRotateTime = 0.25f;
    public GameObject m_Arrow;
    public Transform m_FoldinBone;
    public Transform m_FoldoutBone;
    private MultiSliceElement m_multiSlice;
    private bool m_ShowContents = true;
    public UberText m_Text;
    private List<ToggleContentsListener> m_ToggleEventListeners = new List<ToggleContentsListener>();

    public void AddToggleListener(ToggleContentsFunc func, object userdata)
    {
        ToggleContentsListener item = new ToggleContentsListener();
        item.SetCallback(func);
        item.SetUserData(userdata);
        this.m_ToggleEventListeners.Add(item);
    }

    protected override void Awake()
    {
        base.Awake();
        this.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnHeaderButtonReleased));
        if (this.m_multiSlice == null)
        {
            this.m_multiSlice = base.GetComponentInChildren<MultiSliceElement>();
            if (this.m_multiSlice != null)
            {
                this.m_multiSlice.UpdateSlices();
            }
        }
    }

    public void ClearToggleListeners()
    {
        this.m_ToggleEventListeners.Clear();
    }

    private Transform GetCurrentBoneTransform()
    {
        return (!this.m_ShowContents ? this.m_FoldinBone : this.m_FoldoutBone);
    }

    GameObject ITouchListItem.get_gameObject()
    {
        return base.gameObject;
    }

    Transform ITouchListItem.get_transform()
    {
        return base.transform;
    }

    T ITouchListItem.GetComponent<T>()
    {
        return base.GetComponent<T>();
    }

    private void OnHeaderButtonReleased(UIEvent e)
    {
        this.m_ShowContents = !this.m_ShowContents;
        ToggleContentsListener[] listenerArray = this.m_ToggleEventListeners.ToArray();
        for (int i = 0; i < listenerArray.Length; i++)
        {
            listenerArray[i].Fire(this.m_ShowContents);
        }
        this.UpdateFoldoutArrow();
    }

    public void SetInitialShowContents(bool show)
    {
        this.m_ShowContents = show;
        this.m_Arrow.transform.rotation = this.GetCurrentBoneTransform().rotation;
    }

    public void SetText(string text)
    {
        this.m_Text.Text = text;
    }

    private void UpdateFoldoutArrow()
    {
        if (((this.m_Arrow != null) && (this.m_FoldinBone != null)) && (this.m_FoldoutBone != null))
        {
            iTween.RotateTo(this.m_Arrow, this.GetCurrentBoneTransform().rotation.eulerAngles, this.m_AnimRotateTime);
        }
    }

    public GameObject Background { get; set; }

    public bool IsHeader
    {
        get
        {
            return true;
        }
    }

    public bool IsShowingContents
    {
        get
        {
            return this.m_ShowContents;
        }
    }

    public Bounds LocalBounds { get; private set; }

    public Option Option { get; set; }

    public MobileFriendListItem.TypeFlags SubType { get; set; }

    public bool Visible
    {
        get
        {
            return this.IsShowingContents;
        }
        set
        {
        }
    }

    public delegate void ToggleContentsFunc(bool show, object userdata);

    private class ToggleContentsListener : EventListener<FriendListItemHeader.ToggleContentsFunc>
    {
        public void Fire(bool show)
        {
            base.m_callback(show, base.m_userData);
        }
    }
}

