using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MobileFriendListItem : MonoBehaviour, ITouchListItem, ISelectableTouchListItem
{
    private Bounds m_localBounds;
    private ITouchListItem m_parent;
    private GameObject m_showObject;

    private void Awake()
    {
        Transform parent = base.transform.parent;
        TransformProps destination = new TransformProps();
        TransformUtil.CopyWorld(destination, base.transform);
        base.transform.parent = null;
        TransformUtil.Identity(base.transform);
        this.m_localBounds = this.ComputeWorldBounds();
        base.transform.parent = parent;
        TransformUtil.CopyWorld(base.transform, destination);
    }

    public Bounds ComputeWorldBounds()
    {
        return TransformUtil.ComputeSetPointBounds(base.gameObject);
    }

    public bool IsSelected()
    {
        FriendListUIElement component = base.GetComponent<FriendListUIElement>();
        return ((component != null) && component.IsSelected());
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

    public void Selected()
    {
        FriendListUIElement component = base.GetComponent<FriendListUIElement>();
        if (component != null)
        {
            component.SetSelected(true);
        }
    }

    public void SetParent(ITouchListItem parent)
    {
        this.m_parent = parent;
    }

    public void SetShowObject(GameObject showobj)
    {
        this.m_showObject = showobj;
    }

    public void Unselected()
    {
        FriendListUIElement component = base.GetComponent<FriendListUIElement>();
        if (component != null)
        {
            component.SetSelected(false);
        }
    }

    public bool IsHeader
    {
        get
        {
            return (this.m_parent == null);
        }
    }

    public Bounds LocalBounds
    {
        get
        {
            return this.m_localBounds;
        }
    }

    public bool Selectable
    {
        get
        {
            return ((this.Type == TypeFlags.Friend) || (this.Type == TypeFlags.NearbyPlayer));
        }
    }

    public TypeFlags Type { get; set; }

    public bool Visible
    {
        get
        {
            return ((this.m_parent != null) && this.m_parent.Visible);
        }
        set
        {
            if ((this.m_showObject != null) && (value != this.m_showObject.activeSelf))
            {
                this.m_showObject.SetActive(value);
            }
        }
    }

    [Flags]
    public enum TypeFlags
    {
        CurrentGame = 0x20,
        Friend = 0x10,
        Header = 1,
        NearbyPlayer = 0x40,
        Recruit = 8,
        Request = 0x80
    }
}

