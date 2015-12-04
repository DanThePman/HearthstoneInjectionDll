using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TouchList : PegUIElement, IEnumerable, IList<ITouchListItem>, ICollection<ITouchListItem>, IEnumerable<ITouchListItem>
{
    [CompilerGenerated]
    private static Func<float, float> <>f__am$cache29;
    public Alignment alignment = Alignment.Mid;
    private bool allowModification = true;
    public TiledBackground background;
    public int breadth = 1;
    private const float CLIPSIZE_EPSILON = 0.0001f;
    private GameObject content;
    private float contentSize;
    private Vector3 dragBeginContentPosition = Vector3.zero;
    private Vector3? dragBeginOffsetFromContent;
    private Vector3? dragItemBegin;
    public float elementSpacing;
    private float excessContentSize;
    public float itemDragFinishDistance;
    private const float ItemDragThreshold = 0.05f;
    private Map<ITouchListItem, ItemInfo> itemInfos = new Map<ITouchListItem, ItemInfo>();
    private List<ITouchListItem> items = new List<ITouchListItem>();
    private const float KineticScrollFriction = 10000f;
    private float lastContentPosition;
    private Vector3 lastTouchPosition = Vector3.zero;
    private int layoutDimension1;
    private int layoutDimension2;
    private int layoutDimension3;
    public LayoutPlane layoutPlane;
    private bool layoutSuspended;
    private ILongListBehavior longListBehavior;
    private float m_fullListContentSize;
    private PegUIElement m_hoveredOverItem;
    private bool m_isHoveredOverTouchList;
    public Float_MobileOverride maxKineticScrollSpeed = new Float_MobileOverride();
    private const float MinKineticScrollSpeed = 0.01f;
    private const float MinOutOfBoundsDistance = 0.05f;
    public Orientation orientation;
    private static readonly Func<float, float> OutOfBoundsDistReducer;
    public Vector2 padding = Vector2.zero;
    private static readonly float ScrollBoundsSpringB = Mathf.Sqrt(1600f);
    private const float ScrollBoundsSpringK = 400f;
    private const float ScrollDragThreshold = 0.05f;
    public float scrollWheelIncrement = 30f;
    private int? selection;
    private ITouchListItem touchBeginItem;
    private Vector2? touchBeginScreenPosition;

    public event System.Action ClipSizeChanged;

    public event ItemDragEvent ItemDragFinished;

    public event ItemDragEvent ItemDragged;

    public event ItemDragEvent ItemDragStarted;

    public event System.Action Scrolled;

    public event ScrollingEnabledChangedEvent ScrollingEnabledChanged;

    public event SelectedIndexChangingEvent SelectedIndexChanging;

    static TouchList()
    {
        if (<>f__am$cache29 == null)
        {
            <>f__am$cache29 = new Func<float, float>(TouchList.<OutOfBoundsDistReducer>m__68);
        }
        OutOfBoundsDistReducer = <>f__am$cache29;
    }

    [CompilerGenerated]
    private static float <OutOfBoundsDistReducer>m__68(float dist)
    {
        return (30f * (Mathf.Log(dist + 30f) - Mathf.Log(30f)));
    }

    public void Add(ITouchListItem item)
    {
        this.Add(item, true);
    }

    public void Add(ITouchListItem item, bool repositionItems)
    {
        this.EnforceInitialized();
        if (this.allowModification)
        {
            this.items.Add(item);
            Vector3 negatedScale = this.GetNegatedScale(item.transform.localScale);
            item.transform.parent = this.content.transform;
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            if (this.orientation == Orientation.Vertical)
            {
                item.transform.localScale = negatedScale;
            }
            this.itemInfos[item] = new ItemInfo(item, this.layoutPlane);
            item.gameObject.SetActive(false);
            if (((this.selection == -1) && (item is ISelectableTouchListItem)) && ((ISelectableTouchListItem) item).IsSelected())
            {
                this.selection = new int?(this.items.Count - 1);
            }
            if (repositionItems)
            {
                this.RepositionItems(this.items.Count - 1, null);
                this.RecalculateLongListContentSize(true);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        this.content = new GameObject("Content");
        this.content.transform.parent = base.transform;
        TransformUtil.Identity(this.content.transform);
        this.layoutDimension1 = 0;
        this.layoutDimension2 = (this.layoutPlane != LayoutPlane.XY) ? 2 : 1;
        this.layoutDimension3 = 3 - this.layoutDimension2;
        if (this.orientation == Orientation.Vertical)
        {
            GeneralUtils.Swap<int>(ref this.layoutDimension1, ref this.layoutDimension2);
            Vector3 one = Vector3.one;
            one[this.layoutDimension1] = -1f;
            base.transform.localScale = one;
        }
        if (this.background != null)
        {
            if (this.orientation == Orientation.Vertical)
            {
                this.background.transform.localScale = this.GetNegatedScale(this.background.transform.localScale);
            }
            this.UpdateBackgroundBounds();
        }
    }

    private Bounds CalculateLocalClipBounds()
    {
        Vector3 vector = this.content.transform.InverseTransformPoint(base.GetComponent<Collider>().bounds.min);
        Vector3 vector2 = this.content.transform.InverseTransformPoint(base.GetComponent<Collider>().bounds.max);
        Vector3 center = (Vector3) ((vector2 + vector) / 2f);
        return new Bounds(center, VectorUtils.Abs(vector2 - vector));
    }

    public void Clear()
    {
        this.EnforceInitialized();
        if (this.allowModification)
        {
            foreach (ITouchListItem item in this.items)
            {
                Vector3 negatedScale = this.GetNegatedScale(item.transform.localScale);
                item.transform.parent = null;
                if (this.orientation == Orientation.Vertical)
                {
                    item.transform.localScale = negatedScale;
                }
            }
            this.content.transform.localPosition = Vector3.zero;
            this.lastContentPosition = 0f;
            this.items.Clear();
            this.RecalculateSize();
            this.UpdateBackgroundScroll();
            this.RecalculateLongListContentSize(true);
            if (this.SelectionEnabled)
            {
                this.SelectedIndex = -1;
            }
            if (this.m_hoveredOverItem != null)
            {
                this.m_hoveredOverItem.TriggerOut();
                this.m_hoveredOverItem = null;
            }
        }
    }

    public bool Contains(ITouchListItem item)
    {
        this.EnforceInitialized();
        return this.items.Contains(item);
    }

    public void CopyTo(ITouchListItem[] array, int arrayIndex)
    {
        this.EnforceInitialized();
        this.items.CopyTo(array, arrayIndex);
    }

    private void EnforceInitialized()
    {
        if (!this.IsInitialized)
        {
            throw new InvalidOperationException("TouchList must be initialized before using it. Please wait for Awake to finish.");
        }
    }

    public int FindIndex(Predicate<ITouchListItem> match)
    {
        this.EnforceInitialized();
        return this.items.FindIndex(match);
    }

    private void FixUpScrolling()
    {
        if ((this.longListBehavior != null) && (this.items.Count > 0))
        {
            Bounds bounds = this.CalculateLocalClipBounds();
            ITouchListItem item = this.items[0];
            ItemInfo info = this.itemInfos[item];
            if ((info.LongListIndex == 0) && !this.CanScrollBehind)
            {
                float num = bounds.min[this.layoutDimension1];
                Vector3 min = info.Min;
                if (min[this.layoutDimension1] != num)
                {
                    Vector3 zero = Vector3.zero;
                    zero[this.layoutDimension1] = num - min[this.layoutDimension1];
                    for (int i = 0; i < this.items.Count; i++)
                    {
                        item = this.items[i];
                        item.gameObject.transform.Translate(-zero);
                    }
                }
            }
            else if ((this.items.Count > 1) && !this.CanScrollAhead)
            {
                float num3 = bounds.max[this.layoutDimension1];
                item = this.items[this.items.Count - 1];
                info = this.itemInfos[item];
                if (info.LongListIndex >= (this.longListBehavior.AllItemsCount - 1))
                {
                    Vector3 max = info.Max;
                    if (max[this.layoutDimension1] != num3)
                    {
                        Vector3 vector4 = Vector3.zero;
                        vector4[this.layoutDimension1] = num3 - max[this.layoutDimension1];
                        for (int j = 0; j < this.items.Count; j++)
                        {
                            item = this.items[j];
                            item.gameObject.transform.Translate(-vector4);
                        }
                    }
                }
            }
        }
    }

    private float GetBreadthPosition(int itemIndex)
    {
        float num = this.padding[this.GetVector2Dimension(this.layoutDimension2)];
        float num2 = 0f;
        int num3 = itemIndex - (itemIndex % this.breadth);
        int num4 = Math.Min(num3 + this.breadth, this.items.Count);
        for (int i = num3; i < num4; i++)
        {
            if (i == itemIndex)
            {
                num2 = num;
            }
            num += this.itemInfos[this.items[i]].Size[this.layoutDimension2];
        }
        num += this.padding[this.GetVector2Dimension(this.layoutDimension2)];
        float num6 = 0f;
        float num7 = (base.GetComponent<Collider>() as BoxCollider).size[this.layoutDimension2];
        Alignment alignment = this.alignment;
        if ((this.orientation == Orientation.Horizontal) && (this.alignment != Alignment.Mid))
        {
            alignment = this.alignment ^ Alignment.Max;
        }
        switch (alignment)
        {
            case Alignment.Min:
                num6 = -num7 / 2f;
                break;

            case Alignment.Mid:
                num6 = -num / 2f;
                break;

            case Alignment.Max:
                num6 = (num7 / 2f) - num;
                break;
        }
        return (num6 + num2);
    }

    public IEnumerator<ITouchListItem> GetEnumerator()
    {
        this.EnforceInitialized();
        return this.items.GetEnumerator();
    }

    private float GetItemDragDelta(Vector3 touchPosition)
    {
        if (this.dragItemBegin.HasValue)
        {
            return (touchPosition[this.layoutDimension2] - this.dragItemBegin.Value[this.layoutDimension2]);
        }
        return 0f;
    }

    private List<ITouchListItem> GetItemsInView()
    {
        this.EnforceInitialized();
        List<ITouchListItem> list = new List<ITouchListItem>();
        float num = this.CalculateLocalClipBounds().max[this.layoutDimension1];
        for (int i = this.GetNumItemsBehindView(); i < this.items.Count; i++)
        {
            ItemInfo info = this.itemInfos[this.items[i]];
            Vector3 vector = info.Min - this.content.transform.localPosition;
            if (vector[this.layoutDimension1] >= num)
            {
                return list;
            }
            list.Add(this.items[i]);
        }
        return list;
    }

    private unsafe Vector3 GetNegatedScale(Vector3 scale)
    {
        ref Vector3 vectorRef;
        int num;
        float num2 = vectorRef[num];
        (vectorRef = (Vector3) &scale)[num = (this.layoutPlane != LayoutPlane.XY) ? 2 : 1] = num2 * -1f;
        return scale;
    }

    private int GetNumItemsAheadOfView()
    {
        float num = this.CalculateLocalClipBounds().max[this.layoutDimension1];
        for (int i = this.items.Count - 1; i >= 0; i--)
        {
            ITouchListItem item = this.items[i];
            ItemInfo info = this.itemInfos[item];
            if (info.Min[this.layoutDimension1] < num)
            {
                return ((this.items.Count - 1) - i);
            }
        }
        return this.items.Count;
    }

    private int GetNumItemsBehindView()
    {
        float num = this.CalculateLocalClipBounds().min[this.layoutDimension1];
        for (int i = 0; i < this.items.Count; i++)
        {
            ITouchListItem item = this.items[i];
            ItemInfo info = this.itemInfos[item];
            if (info.Max[this.layoutDimension1] > num)
            {
                return i;
            }
        }
        return this.items.Count;
    }

    private float GetOutOfBoundsDist(float contentPosition)
    {
        float num = 0f;
        if (contentPosition < -this.excessContentSize)
        {
            return (contentPosition + this.excessContentSize);
        }
        if (contentPosition > 0f)
        {
            num = contentPosition;
        }
        return num;
    }

    private Vector3 GetTouchPosition()
    {
        float num3;
        Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
        if (camera == null)
        {
            return Vector3.zero;
        }
        float num = Vector3.Distance(camera.transform.position, base.GetComponent<Collider>().bounds.min);
        float num2 = Vector3.Distance(camera.transform.position, base.GetComponent<Collider>().bounds.max);
        Vector3 inPoint = (num >= num2) ? base.GetComponent<Collider>().bounds.max : base.GetComponent<Collider>().bounds.min;
        Plane plane = new Plane(-camera.transform.forward, inPoint);
        Ray ray = camera.ScreenPointToRay(UniversalInputManager.Get().GetMousePosition());
        plane.Raycast(ray, out num3);
        return base.transform.InverseTransformPoint(ray.GetPoint(num3));
    }

    private int GetVector2Dimension(int vec3Dimension)
    {
        return ((vec3Dimension != 0) ? 1 : vec3Dimension);
    }

    private int GetVector3Dimension(int vec2Dimension)
    {
        if ((vec2Dimension != 0) && (this.layoutPlane != LayoutPlane.XY))
        {
            return 2;
        }
        return vec2Dimension;
    }

    public int IndexOf(ITouchListItem item)
    {
        this.EnforceInitialized();
        return this.items.IndexOf(item);
    }

    public void Insert(int index, ITouchListItem item)
    {
        this.Insert(index, item, true);
    }

    public void Insert(int index, ITouchListItem item, bool repositionItems)
    {
        this.EnforceInitialized();
        if (this.allowModification)
        {
            this.items.Insert(index, item);
            Vector3 negatedScale = this.GetNegatedScale(item.transform.localScale);
            item.transform.parent = this.content.transform;
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            if (this.orientation == Orientation.Vertical)
            {
                item.transform.localScale = negatedScale;
            }
            this.itemInfos[item] = new ItemInfo(item, this.layoutPlane);
            if (((this.selection == -1) && (item is ISelectableTouchListItem)) && ((ISelectableTouchListItem) item).IsSelected())
            {
                this.selection = new int?(index);
            }
            if (repositionItems)
            {
                this.RepositionItems(index, null);
                this.RecalculateLongListContentSize(true);
            }
        }
    }

    private void InsertAndPositionBehind(ITouchListItem item, int longListIndex)
    {
        if (this.items.Count == 0)
        {
            this.Add(item, true);
        }
        else
        {
            ITouchListItem item2 = this.items.FirstOrDefault<ITouchListItem>();
            if (item2 == null)
            {
                this.Insert(0, item, true);
            }
            else
            {
                if (this.orientation == Orientation.Vertical)
                {
                    base.transform.localScale = Vector3.one;
                }
                ItemInfo info = this.itemInfos[item2];
                Vector3 vector = item2.transform.localPosition - info.Offset;
                this.Insert(0, item, false);
                this.itemInfos[item].LongListIndex = longListIndex;
                ItemInfo info2 = this.itemInfos[item];
                Vector3 vector2 = vector;
                float num = info2.Size[this.layoutDimension1] + this.elementSpacing;
                vector2[this.layoutDimension1] -= num;
                vector2 += info2.Offset;
                item.transform.localPosition = vector2;
                if (((this.selection == -1) && (item is ISelectableTouchListItem)) && ((ISelectableTouchListItem) item).IsSelected())
                {
                    this.selection = 0;
                }
                this.RecalculateSize();
                this.UpdateBackgroundScroll();
                if (this.orientation == Orientation.Vertical)
                {
                    base.transform.localScale = this.GetNegatedScale(Vector3.one);
                }
                bool flag = this.IsItemVisible(0);
                item.gameObject.SetActive(flag);
            }
        }
    }

    private bool IsItemVisible(int visualizedListIndex)
    {
        Bounds localClipBounds = this.CalculateLocalClipBounds();
        return this.IsItemVisible_Internal(visualizedListIndex, ref localClipBounds);
    }

    private bool IsItemVisible_Internal(int visualizedListIndex, ref Bounds localClipBounds)
    {
        ITouchListItem item = this.items[visualizedListIndex];
        ItemInfo info = this.itemInfos[item];
        Vector3 min = info.Min;
        Vector3 max = info.Max;
        if (!this.IsWithinClipBounds(min, max, ref localClipBounds))
        {
            return false;
        }
        return true;
    }

    private bool IsWithinClipBounds(Vector3 localBoundsMin, Vector3 localBoundsMax, ref Bounds localClipBounds)
    {
        float num = localClipBounds.min[this.layoutDimension1];
        float num2 = localClipBounds.max[this.layoutDimension1];
        if (localBoundsMax[this.layoutDimension1] < num)
        {
            return false;
        }
        if (localBoundsMin[this.layoutDimension1] > num2)
        {
            return false;
        }
        return true;
    }

    private void LoadAhead()
    {
        bool allowModification = this.allowModification;
        bool layoutSuspended = this.layoutSuspended;
        this.allowModification = true;
        int startingIndex = -1;
        int num2 = 0;
        int numItemsBehindView = this.GetNumItemsBehindView();
        for (int i = 0; i < (numItemsBehindView - this.longListBehavior.MinBuffer); i++)
        {
            ITouchListItem item = this.items[0];
            this.RemoveAt(0, false);
            this.longListBehavior.ReleaseItem(item);
        }
        float num5 = this.CalculateLocalClipBounds().max[this.layoutDimension1];
        int num6 = 0;
        for (int j = (this.items.Count != 0) ? (this.itemInfos[this.items.Last<ITouchListItem>()].LongListIndex + 1) : 0; ((j < this.longListBehavior.AllItemsCount) && (this.items.Count < this.longListBehavior.MaxAcquiredItems)) && (num6 < this.longListBehavior.MinBuffer); j++)
        {
            if (this.longListBehavior.IsItemShowable(j))
            {
                if (startingIndex < 0)
                {
                    startingIndex = this.items.Count;
                }
                ITouchListItem item2 = this.longListBehavior.AcquireItem(j);
                this.Add(item2, false);
                ItemInfo info = this.itemInfos[item2];
                info.LongListIndex = j;
                num2++;
                if (info.Min[this.layoutDimension1] > num5)
                {
                    num6++;
                }
            }
        }
        if (startingIndex >= 0)
        {
            this.layoutSuspended = false;
            this.RepositionItems(startingIndex, null);
        }
        this.allowModification = allowModification;
        if (layoutSuspended != this.layoutSuspended)
        {
            this.layoutSuspended = layoutSuspended;
        }
    }

    private void LoadBehind()
    {
        bool allowModification = this.allowModification;
        this.allowModification = true;
        int num = 0;
        int numItemsAheadOfView = this.GetNumItemsAheadOfView();
        for (int i = 0; i < (numItemsAheadOfView - this.longListBehavior.MinBuffer); i++)
        {
            ITouchListItem item = this.items[this.items.Count - 1];
            this.RemoveAt(this.items.Count - 1, false);
            this.longListBehavior.ReleaseItem(item);
        }
        float num4 = this.CalculateLocalClipBounds().min[this.layoutDimension1];
        int num5 = 0;
        for (int j = (this.items.Count != 0) ? (this.itemInfos[this.items.First<ITouchListItem>()].LongListIndex - 1) : (this.longListBehavior.AllItemsCount - 1); ((j >= 0) && (this.items.Count < this.longListBehavior.MaxAcquiredItems)) && (num5 < this.longListBehavior.MinBuffer); j--)
        {
            if (this.longListBehavior.IsItemShowable(j))
            {
                ITouchListItem item2 = this.longListBehavior.AcquireItem(j);
                this.InsertAndPositionBehind(item2, j);
                ItemInfo info = this.itemInfos[item2];
                info.LongListIndex = j;
                num++;
                if (info.Max[this.layoutDimension1] < num4)
                {
                    num5++;
                }
            }
        }
        this.allowModification = allowModification;
    }

    private void OnHover(bool isKnownOver)
    {
        if (!UniversalInputManager.Get().IsTouchMode())
        {
            if ((this.touchBeginItem != null) || this.dragItemBegin.HasValue)
            {
                if (this.m_hoveredOverItem != null)
                {
                    this.m_hoveredOverItem.TriggerOut();
                    this.m_hoveredOverItem = null;
                }
            }
            else
            {
                Camera requestedCamera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
                if (requestedCamera == null)
                {
                    if (this.m_hoveredOverItem != null)
                    {
                        this.m_hoveredOverItem.TriggerOut();
                        this.m_hoveredOverItem = null;
                    }
                }
                else
                {
                    RaycastHit hit;
                    if ((!isKnownOver && (!UniversalInputManager.Get().GetInputHitInfo(requestedCamera, out hit) || (hit.transform != base.transform))) && (this.m_hoveredOverItem != null))
                    {
                        this.m_hoveredOverItem.TriggerOut();
                        this.m_hoveredOverItem = null;
                    }
                    base.GetComponent<Collider>().enabled = false;
                    PegUIElement component = null;
                    if (UniversalInputManager.Get().GetInputHitInfo(requestedCamera, out hit))
                    {
                        component = hit.transform.GetComponent<PegUIElement>();
                    }
                    base.GetComponent<Collider>().enabled = true;
                    if ((component != null) && (this.m_hoveredOverItem != component))
                    {
                        if (this.m_hoveredOverItem != null)
                        {
                            this.m_hoveredOverItem.TriggerOut();
                        }
                        component.TriggerOver();
                        this.m_hoveredOverItem = component;
                    }
                }
            }
        }
    }

    protected override void OnOut(PegUIElement.InteractionState oldState)
    {
        this.m_isHoveredOverTouchList = false;
        if (this.m_hoveredOverItem != null)
        {
            this.m_hoveredOverItem.TriggerOut();
            this.m_hoveredOverItem = null;
        }
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        this.m_isHoveredOverTouchList = true;
        this.OnHover(true);
    }

    protected override void OnPress()
    {
        if (CameraUtils.FindFirstByLayer(base.gameObject.layer) != null)
        {
            this.touchBeginScreenPosition = new Vector2?(UniversalInputManager.Get().GetMousePosition());
            if (this.lastContentPosition == this.content.transform.localPosition[this.layoutDimension1])
            {
                Vector3 point = this.GetTouchPosition() - this.content.transform.localPosition;
                for (int i = 0; i < this.items.Count; i++)
                {
                    ITouchListItem item = this.items[i];
                    if ((item.IsHeader || item.Visible) && this.itemInfos[item].Contains(point, this.layoutPlane))
                    {
                        this.touchBeginItem = item;
                        break;
                    }
                }
            }
        }
    }

    protected override void OnRelease()
    {
        Camera requestedCamera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
        if ((requestedCamera != null) && ((this.touchBeginItem != null) && !this.dragItemBegin.HasValue))
        {
            RaycastHit hit;
            this.touchBeginScreenPosition = null;
            base.GetComponent<Collider>().enabled = false;
            PegUIElement component = null;
            if (UniversalInputManager.Get().GetInputHitInfo(requestedCamera, out hit))
            {
                component = hit.transform.GetComponent<PegUIElement>();
            }
            base.GetComponent<Collider>().enabled = true;
            if (component != null)
            {
                component.TriggerRelease();
                this.touchBeginItem = null;
            }
        }
    }

    private void OnScrolled()
    {
        this.UpdateBackgroundScroll();
        this.SetVisibilityOfAllItems();
        if (this.Scrolled != null)
        {
            this.Scrolled();
        }
    }

    private void OnScrollingEnabledChanged()
    {
        if (this.ScrollingEnabledChanged != null)
        {
            if (this.longListBehavior == null)
            {
                this.ScrollingEnabledChanged(this.excessContentSize > 0f);
            }
            else
            {
                this.ScrollingEnabledChanged(this.m_fullListContentSize > this.ClipSize[this.GetVector2Dimension(this.layoutDimension1)]);
            }
        }
    }

    private void PreBufferLongListItems(bool scrolledAhead)
    {
        if (this.LongListBehavior != null)
        {
            this.allowModification = true;
            if (scrolledAhead && (this.GetNumItemsAheadOfView() < this.longListBehavior.MinBuffer))
            {
                bool canScrollAhead = this.CanScrollAhead;
                if (this.items.Count > 0)
                {
                    Bounds bounds = this.CalculateLocalClipBounds();
                    ITouchListItem item = this.items[this.items.Count - 1];
                    ItemInfo info = this.itemInfos[item];
                    Vector3 max = info.Max;
                    float num = bounds.min[this.layoutDimension1];
                    if (max[this.layoutDimension1] < num)
                    {
                        this.RefreshList(0, true);
                        canScrollAhead = false;
                    }
                }
                if (canScrollAhead)
                {
                    this.LoadAhead();
                }
            }
            else if (!scrolledAhead && (this.GetNumItemsBehindView() < this.longListBehavior.MinBuffer))
            {
                bool canScrollBehind = this.CanScrollBehind;
                if (this.items.Count > 0)
                {
                    Bounds bounds2 = this.CalculateLocalClipBounds();
                    ITouchListItem item2 = this.items[0];
                    ItemInfo info2 = this.itemInfos[item2];
                    Vector3 min = info2.Min;
                    float num2 = bounds2.max[this.layoutDimension1];
                    if (min[this.layoutDimension1] > num2)
                    {
                        this.RefreshList(0, true);
                        canScrollBehind = false;
                    }
                }
                if (canScrollBehind)
                {
                    this.LoadBehind();
                }
            }
            this.allowModification = false;
        }
    }

    public bool RecalculateLongListContentSize(bool fireOnScroll = true)
    {
        if (this.longListBehavior == null)
        {
            return false;
        }
        float fullListContentSize = this.m_fullListContentSize;
        this.m_fullListContentSize = 0f;
        bool flag = true;
        for (int i = 0; i < this.longListBehavior.AllItemsCount; i++)
        {
            if (this.longListBehavior.IsItemShowable(i))
            {
                Vector3 itemSize = this.longListBehavior.GetItemSize(i);
                this.m_fullListContentSize += itemSize[this.layoutDimension1];
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    this.m_fullListContentSize += this.elementSpacing;
                }
            }
        }
        if (this.m_fullListContentSize > 0f)
        {
            this.m_fullListContentSize += 2f * this.padding[this.GetVector2Dimension(this.layoutDimension1)];
        }
        bool flag2 = !(fullListContentSize == this.m_fullListContentSize);
        if (flag2 && fireOnScroll)
        {
            this.OnScrolled();
            this.OnScrollingEnabledChanged();
        }
        return flag2;
    }

    private void RecalculateSize()
    {
        float num = Math.Abs((base.GetComponent<Collider>() as BoxCollider).size[this.layoutDimension1]);
        float num2 = -num / 2f;
        float num3 = num2;
        if (this.items.Any<ITouchListItem>())
        {
            this.ValidateBreadth();
            int num4 = this.items.Count - 1;
            num4 -= num4 % this.breadth;
            int num5 = Math.Min(num4 + this.breadth, this.items.Count);
            for (int i = num4; i < num5; i++)
            {
                ITouchListItem item = this.items[i];
                ItemInfo info = this.itemInfos[item];
                num3 = Math.Max(info.Max[this.layoutDimension1], num3);
            }
            this.contentSize = (num3 - num2) + this.padding[this.GetVector2Dimension(this.layoutDimension1)];
            this.excessContentSize = Math.Max((float) (this.contentSize - num), (float) 0f);
        }
        else
        {
            this.contentSize = 0f;
            this.excessContentSize = 0f;
        }
        this.OnScrollingEnabledChanged();
    }

    public unsafe void RefreshList(int startingLongListIndex, bool preserveScrolling)
    {
        if (this.longListBehavior != null)
        {
            int num15;
            float num16;
            bool allowModification = this.allowModification;
            this.allowModification = true;
            int num = (this.SelectedItem != null) ? this.itemInfos[this.SelectedItem].LongListIndex : -1;
            int num2 = -2;
            int startingIndex = -1;
            if (startingLongListIndex > 0)
            {
                for (int j = 0; j < this.items.Count; j++)
                {
                    ITouchListItem item = this.items[j];
                    if (this.itemInfos[item].LongListIndex < startingLongListIndex)
                    {
                        num2 = j;
                    }
                    else
                    {
                        startingIndex = j;
                        break;
                    }
                }
            }
            else
            {
                startingIndex = 0;
            }
            int num6 = (startingIndex != -1) ? startingIndex : (num2 + 1);
            Bounds bounds = base.GetComponent<Collider>().bounds;
            Vector3? initialItemPosition = null;
            Vector3 zero = Vector3.zero;
            int num7 = (this.orientation != Orientation.Vertical) ? 1 : -1;
            if (preserveScrolling)
            {
                ref Vector3 vectorRef;
                ref Vector3 vectorRef2;
                zero = this.content.transform.position;
                num16 = vectorRef[num15];
                (vectorRef = (Vector3) &zero)[num15 = this.layoutDimension1] = num16 - (num7 * bounds.extents[this.layoutDimension1]);
                num16 = vectorRef2[num15];
                (vectorRef2 = (Vector3) &zero)[num15 = this.layoutDimension1] = num16 + (num7 * this.padding[this.GetVector2Dimension(this.layoutDimension1)]);
                zero[this.layoutDimension2] = bounds.center[this.layoutDimension2];
                zero[this.layoutDimension3] = bounds.center[this.layoutDimension3];
                Vector3 localPosition = this.content.transform.localPosition;
                this.content.transform.localPosition = Vector3.zero;
                Bounds bounds2 = this.CalculateLocalClipBounds();
                Vector3 min = bounds2.min;
                min[this.layoutDimension1] = -localPosition[this.layoutDimension1] + bounds2.min[this.layoutDimension1];
                this.content.transform.localPosition = localPosition;
                initialItemPosition = new Vector3?(min);
                if (num2 >= 0)
                {
                    ref Vector3 vectorRef3;
                    ITouchListItem item2 = this.items[num2];
                    ItemInfo info = this.itemInfos[item2];
                    zero = item2.transform.position - info.Offset;
                    num16 = vectorRef3[num15];
                    (vectorRef3 = (Vector3) &zero)[num15 = this.layoutDimension1] = num16 + (num7 * this.elementSpacing);
                    ITouchListItem item3 = this.items[0];
                    ItemInfo info2 = this.itemInfos[item3];
                    initialItemPosition = new Vector3?(item3.transform.localPosition - info2.Offset);
                }
            }
            int num8 = 0;
            if (num6 >= 0)
            {
                for (int k = this.items.Count - 1; k >= num6; k--)
                {
                    num8++;
                    ITouchListItem item4 = this.items[k];
                    this.RemoveAt(k, false);
                    this.longListBehavior.ReleaseItem(item4);
                }
            }
            if (startingIndex < 0)
            {
                startingIndex = num2 + 1;
                if (startingIndex < 0)
                {
                    startingIndex = 0;
                }
            }
            int num10 = 0;
            for (int i = startingLongListIndex; (i < this.longListBehavior.AllItemsCount) && (this.items.Count < this.longListBehavior.MaxAcquiredItems); i++)
            {
                if (this.longListBehavior.IsItemShowable(i))
                {
                    bool flag3 = true;
                    if (preserveScrolling)
                    {
                        ref Vector3 vectorRef4;
                        ref Vector3 vectorRef5;
                        flag3 = false;
                        Vector3 itemSize = this.longListBehavior.GetItemSize(i);
                        Vector3 point = zero;
                        num16 = vectorRef4[num15];
                        (vectorRef4 = (Vector3) &point)[num15 = this.layoutDimension1] = num16 + (num7 * itemSize[this.layoutDimension1]);
                        if (bounds.Contains(zero) || bounds.Contains(point))
                        {
                            flag3 = true;
                        }
                        zero = point;
                        num16 = vectorRef5[num15];
                        (vectorRef5 = (Vector3) &zero)[num15 = this.layoutDimension1] = num16 + (num7 * this.elementSpacing);
                    }
                    if (flag3)
                    {
                        num10++;
                        ITouchListItem item5 = this.longListBehavior.AcquireItem(i);
                        this.Add(item5, false);
                        this.itemInfos[item5].LongListIndex = i;
                    }
                }
            }
            this.RepositionItems(startingIndex, initialItemPosition);
            if (startingIndex == 0)
            {
                this.LoadBehind();
            }
            if (num6 >= 0)
            {
                this.LoadAhead();
            }
            bool flag4 = false;
            float outOfBoundsDist = this.GetOutOfBoundsDist(this.content.transform.localPosition[this.layoutDimension1]);
            if ((outOfBoundsDist != 0f) && (this.excessContentSize > 0f))
            {
                ref Vector3 vectorRef6;
                Vector3 vector6 = this.content.transform.localPosition;
                num16 = vectorRef6[num15];
                (vectorRef6 = (Vector3) &vector6)[num15 = this.layoutDimension1] = num16 - outOfBoundsDist;
                float num13 = vector6[this.layoutDimension1] - this.content.transform.localPosition[this.layoutDimension1];
                this.content.transform.localPosition = vector6;
                this.lastContentPosition = this.content.transform.localPosition[this.layoutDimension1];
                if (num13 < 0f)
                {
                    this.LoadAhead();
                }
                else
                {
                    this.LoadBehind();
                }
                flag4 = true;
            }
            if (((num >= 0) && (this.items.Count > 0)) && ((num >= this.itemInfos[this.items.First<ITouchListItem>()].LongListIndex) && (num <= this.itemInfos[this.items.Last<ITouchListItem>()].LongListIndex)))
            {
                for (int m = 0; m < this.items.Count; m++)
                {
                    ISelectableTouchListItem item6 = this.items[m] as ISelectableTouchListItem;
                    if (item6 != null)
                    {
                        ItemInfo info3 = this.itemInfos[item6];
                        if (info3.LongListIndex == num)
                        {
                            this.selection = new int?(m);
                            item6.Selected();
                            break;
                        }
                    }
                }
            }
            flag4 = this.RecalculateLongListContentSize(false) || flag4;
            this.allowModification = allowModification;
            if (flag4)
            {
                this.OnScrolled();
                this.OnScrollingEnabledChanged();
            }
        }
    }

    public bool Remove(ITouchListItem item)
    {
        this.EnforceInitialized();
        if (this.allowModification)
        {
            int index = this.items.IndexOf(item);
            if (index != -1)
            {
                this.RemoveAt(index, true);
                return true;
            }
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        this.RemoveAt(index, true);
    }

    public void RemoveAt(int index, bool repositionItems)
    {
        this.EnforceInitialized();
        if (this.allowModification)
        {
            Vector3 negatedScale = this.GetNegatedScale(this.items[index].transform.localScale);
            ITouchListItem item = this.items[index];
            item.transform.parent = base.transform;
            if (this.orientation == Orientation.Vertical)
            {
                this.items[index].transform.localScale = negatedScale;
            }
            this.itemInfos.Remove(this.items[index]);
            this.items.RemoveAt(index);
            if (index == this.selection)
            {
                this.selection = -1;
            }
            else
            {
                int? selection = this.selection;
                if (selection.HasValue && (index < selection.Value))
                {
                    int? nullable3 = this.selection;
                    if (nullable3.HasValue)
                    {
                        this.selection = new int?(nullable3.Value - 1);
                    }
                }
            }
            if ((this.m_hoveredOverItem != null) && (item.GetComponent<PegUIElement>() == this.m_hoveredOverItem))
            {
                this.m_hoveredOverItem.TriggerOut();
                this.m_hoveredOverItem = null;
            }
            if (repositionItems)
            {
                this.RepositionItems(index, null);
                this.RecalculateLongListContentSize(true);
            }
        }
    }

    private unsafe void RepositionItems(int startingIndex, Vector3? initialItemPosition = new Vector3?())
    {
        if (!this.layoutSuspended)
        {
            ref Vector3 vectorRef;
            int num5;
            if (this.orientation == Orientation.Vertical)
            {
                base.transform.localScale = Vector3.one;
            }
            Vector3 localPosition = this.content.transform.localPosition;
            this.content.transform.localPosition = Vector3.zero;
            Vector3 min = this.CalculateLocalClipBounds().min;
            if (initialItemPosition.HasValue)
            {
                min = initialItemPosition.Value;
            }
            float num6 = vectorRef[num5];
            (vectorRef = (Vector3) &min)[num5 = this.layoutDimension1] = num6 + this.padding[this.GetVector2Dimension(this.layoutDimension1)];
            min[this.layoutDimension3] = 0f;
            this.content.transform.localPosition = localPosition;
            this.ValidateBreadth();
            startingIndex -= startingIndex % this.breadth;
            if (startingIndex > 0)
            {
                int num = startingIndex - this.breadth;
                float minValue = float.MinValue;
                for (int j = num; (j < startingIndex) && (j < this.items.Count); j++)
                {
                    ITouchListItem item = this.items[j];
                    ItemInfo info = this.itemInfos[item];
                    minValue = Mathf.Max(info.Max[this.layoutDimension1], minValue);
                }
                min[this.layoutDimension1] = minValue + this.elementSpacing;
            }
            Vector3 zero = Vector3.zero;
            zero[this.layoutDimension1] = 1f;
            for (int i = startingIndex; i < this.items.Count; i++)
            {
                ITouchListItem item2 = this.items[i];
                if (!(item2.IsHeader || item2.Visible))
                {
                    this.items[i].Visible = false;
                    this.items[i].gameObject.SetActive(false);
                }
                else
                {
                    ItemInfo info2 = this.itemInfos[this.items[i]];
                    Vector3 vector4 = min + info2.Offset;
                    vector4[this.layoutDimension2] = this.GetBreadthPosition(i) + info2.Offset[this.layoutDimension2];
                    this.items[i].transform.localPosition = vector4;
                    if (((i + 1) % this.breadth) == 0)
                    {
                        min = (Vector3) ((info2.Max[this.layoutDimension1] + this.elementSpacing) * zero);
                    }
                }
            }
            this.RecalculateSize();
            this.UpdateBackgroundScroll();
            if (this.orientation == Orientation.Vertical)
            {
                base.transform.localScale = this.GetNegatedScale(Vector3.one);
            }
            this.SetVisibilityOfAllItems();
        }
    }

    public void ResetState()
    {
        this.touchBeginScreenPosition = null;
        this.dragBeginOffsetFromContent = null;
        this.dragBeginContentPosition = Vector3.zero;
        this.lastTouchPosition = Vector3.zero;
        this.lastContentPosition = 0f;
        this.dragItemBegin = null;
        if (this.content != null)
        {
            this.content.transform.localPosition = Vector3.zero;
        }
    }

    public void ResumeLayout(bool repositionItems = true)
    {
        this.EnforceInitialized();
        this.layoutSuspended = false;
        if (repositionItems)
        {
            this.RepositionItems(0, null);
        }
    }

    private void ScrollToItem(ITouchListItem item)
    {
        Bounds bounds = this.CalculateLocalClipBounds();
        ItemInfo info = this.itemInfos[item];
        float num = info.Max[this.layoutDimension1] - bounds.max[this.layoutDimension1];
        if (num > 0f)
        {
            Vector3 zero = Vector3.zero;
            zero[this.layoutDimension1] = num;
            this.content.transform.Translate(zero);
            this.lastContentPosition = this.content.transform.localPosition[this.layoutDimension1];
            this.PreBufferLongListItems(true);
            this.OnScrolled();
        }
        float num2 = bounds.min[this.layoutDimension1] - info.Min[this.layoutDimension1];
        if (num2 > 0f)
        {
            Vector3 translation = Vector3.zero;
            translation[this.layoutDimension1] = -num2;
            this.content.transform.Translate(translation);
            this.lastContentPosition = this.content.transform.localPosition[this.layoutDimension1];
            this.PreBufferLongListItems(false);
            this.OnScrolled();
        }
    }

    public void SetVisibilityOfAllItems()
    {
        if (!this.layoutSuspended)
        {
            this.EnforceInitialized();
            Bounds localClipBounds = this.CalculateLocalClipBounds();
            for (int i = 0; i < this.items.Count; i++)
            {
                ITouchListItem item = this.items[i];
                bool flag = this.IsItemVisible_Internal(i, ref localClipBounds);
                if (flag != item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(flag);
                }
            }
        }
    }

    public void Sort(Comparison<ITouchListItem> comparison)
    {
        this.EnforceInitialized();
        ITouchListItem selectedItem = this.SelectedItem;
        this.items.Sort(comparison);
        this.RepositionItems(0, null);
        this.selection = new int?(this.items.IndexOf(selectedItem));
    }

    public void SuspendLayout()
    {
        this.EnforceInitialized();
        this.layoutSuspended = true;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        this.EnforceInitialized();
        return this.items.GetEnumerator();
    }

    private void Update()
    {
        if (UniversalInputManager.Get().IsTouchMode())
        {
            this.UpdateTouchInput();
        }
        else
        {
            this.UpdateMouseInput();
        }
        if (this.m_isHoveredOverTouchList)
        {
            this.OnHover(false);
        }
    }

    private void UpdateBackgroundBounds()
    {
        if (this.background != null)
        {
            Vector3 size = (base.GetComponent<Collider>() as BoxCollider).size;
            size[this.layoutDimension1] = Math.Abs(size[this.layoutDimension1]);
            size[this.layoutDimension3] = 0f;
            Camera camera = CameraUtils.FindFirstByLayer((GameLayer) base.gameObject.layer);
            if (camera != null)
            {
                float num = Vector3.Distance(camera.transform.position, base.GetComponent<Collider>().bounds.min);
                float num2 = Vector3.Distance(camera.transform.position, base.GetComponent<Collider>().bounds.max);
                Vector3 position = (num <= num2) ? base.GetComponent<Collider>().bounds.max : base.GetComponent<Collider>().bounds.min;
                Vector3 zero = Vector3.zero;
                zero[this.layoutDimension3] = this.content.transform.InverseTransformPoint(position)[this.layoutDimension3];
                this.background.SetBounds(new Bounds(zero, size));
                this.UpdateBackgroundScroll();
            }
        }
    }

    private void UpdateBackgroundScroll()
    {
        if (this.background != null)
        {
            float num = Math.Abs((base.GetComponent<Collider>() as BoxCollider).size[this.layoutDimension1]);
            float num2 = this.content.transform.localPosition[this.layoutDimension1];
            if (this.orientation == Orientation.Vertical)
            {
                num2 *= -1f;
            }
            Vector2 offset = this.background.Offset;
            offset[this.GetVector2Dimension(this.layoutDimension1)] = num2 / num;
            this.background.Offset = offset;
        }
    }

    private void UpdateMouseInput()
    {
        Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
        if (camera != null)
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(UniversalInputManager.Get().GetMousePosition());
            if (base.GetComponent<Collider>().Raycast(ray, out hit, camera.farClipPlane))
            {
                float f = 0f;
                if ((Input.GetAxis("Mouse ScrollWheel") < 0f) && this.CanScrollAhead)
                {
                    f -= this.scrollWheelIncrement;
                }
                if ((Input.GetAxis("Mouse ScrollWheel") > 0f) && this.CanScrollBehind)
                {
                    f += this.scrollWheelIncrement;
                }
                if (Mathf.Abs(f) > Mathf.Epsilon)
                {
                    float num2 = this.content.transform.localPosition[this.layoutDimension1] + f;
                    if (num2 <= -this.excessContentSize)
                    {
                        num2 = -this.excessContentSize;
                    }
                    else if (num2 >= 0f)
                    {
                        num2 = 0f;
                    }
                    Vector3 localPosition = this.content.transform.localPosition;
                    this.lastContentPosition = localPosition[this.layoutDimension1];
                    localPosition[this.layoutDimension1] = num2;
                    this.content.transform.localPosition = localPosition;
                    float num3 = this.content.transform.localPosition[this.layoutDimension1] - this.lastContentPosition;
                    this.lastContentPosition = this.content.transform.localPosition[this.layoutDimension1];
                    if (num3 != 0f)
                    {
                        this.PreBufferLongListItems(num3 < 0f);
                    }
                    this.FixUpScrolling();
                    this.OnScrolled();
                }
            }
        }
    }

    private unsafe void UpdateTouchInput()
    {
        float num;
        Vector3 touchPosition = this.GetTouchPosition();
        if (UniversalInputManager.Get().GetMouseButtonUp(0))
        {
            if (this.dragItemBegin.HasValue && (this.ItemDragFinished != null))
            {
                this.ItemDragFinished(this.touchBeginItem, this.GetItemDragDelta(touchPosition));
                this.dragItemBegin = null;
            }
            this.touchBeginItem = null;
            this.touchBeginScreenPosition = null;
            this.dragBeginOffsetFromContent = null;
        }
        if (this.touchBeginScreenPosition.HasValue)
        {
            Func<int, float, bool> func = delegate (int dimension, float inchThreshold) {
                int num = this.GetVector2Dimension(dimension);
                float f = this.touchBeginScreenPosition.Value[num] - UniversalInputManager.Get().GetMousePosition()[num];
                float num3 = inchThreshold * ((Screen.dpi <= 0f) ? 150f : Screen.dpi);
                return Mathf.Abs(f) > num3;
            };
            if (((this.ItemDragStarted != null) && func(this.layoutDimension2, 0.05f)) && this.ItemDragStarted(this.touchBeginItem, this.GetItemDragDelta(touchPosition)))
            {
                this.dragItemBegin = new Vector3?(this.GetTouchPosition());
                this.touchBeginScreenPosition = null;
            }
            else if (func(this.layoutDimension1, 0.05f))
            {
                this.dragBeginContentPosition = this.content.transform.localPosition;
                this.dragBeginOffsetFromContent = new Vector3?(this.dragBeginContentPosition - this.lastTouchPosition);
                this.touchBeginItem = null;
                this.touchBeginScreenPosition = null;
            }
        }
        if (this.dragItemBegin.HasValue)
        {
            if (!this.ItemDragged(this.touchBeginItem, this.GetItemDragDelta(touchPosition)))
            {
                this.dragItemBegin = null;
                this.touchBeginItem = null;
            }
        }
        else if (this.dragBeginOffsetFromContent.HasValue)
        {
            float contentPosition = touchPosition[this.layoutDimension1] + this.dragBeginOffsetFromContent.Value[this.layoutDimension1];
            float outOfBoundsDist = this.GetOutOfBoundsDist(contentPosition);
            if (outOfBoundsDist != 0f)
            {
                outOfBoundsDist = OutOfBoundsDistReducer(Mathf.Abs(outOfBoundsDist)) * Mathf.Sign(outOfBoundsDist);
                contentPosition = (outOfBoundsDist >= 0f) ? outOfBoundsDist : (-this.excessContentSize + outOfBoundsDist);
            }
            Vector3 localPosition = this.content.transform.localPosition;
            this.lastContentPosition = localPosition[this.layoutDimension1];
            localPosition[this.layoutDimension1] = contentPosition;
            this.content.transform.localPosition = localPosition;
            if (this.lastContentPosition != localPosition[this.layoutDimension1])
            {
                this.OnScrolled();
            }
        }
        else
        {
            int num12;
            float num13;
            float num4 = this.content.transform.localPosition[this.layoutDimension1];
            float num5 = this.GetOutOfBoundsDist(num4);
            num = this.content.transform.localPosition[this.layoutDimension1] - this.lastContentPosition;
            float a = num / UnityEngine.Time.fixedDeltaTime;
            if (this.maxKineticScrollSpeed > Mathf.Epsilon)
            {
                if (a > 0f)
                {
                    a = Mathf.Min(a, (float) this.maxKineticScrollSpeed);
                }
                else
                {
                    a = Mathf.Max(a, (float) -this.maxKineticScrollSpeed);
                }
            }
            if (num5 != 0f)
            {
                ref Vector3 vectorRef;
                Vector3 vector3 = this.content.transform.localPosition;
                this.lastContentPosition = num4;
                float num7 = (-400f * num5) - (ScrollBoundsSpringB * a);
                float num8 = a + (num7 * UnityEngine.Time.fixedDeltaTime);
                num13 = vectorRef[num12];
                (vectorRef = (Vector3) &vector3)[num12 = this.layoutDimension1] = num13 + (num8 * UnityEngine.Time.fixedDeltaTime);
                if (Mathf.Abs(this.GetOutOfBoundsDist(vector3[this.layoutDimension1])) < 0.05f)
                {
                    float introduced35 = Mathf.Abs((float) (vector3[this.layoutDimension1] + this.excessContentSize));
                    float num9 = (introduced35 >= Mathf.Abs(vector3[this.layoutDimension1])) ? 0f : -this.excessContentSize;
                    vector3[this.layoutDimension1] = num9;
                    this.lastContentPosition = num9;
                }
                this.content.transform.localPosition = vector3;
                this.OnScrolled();
            }
            else if (a != 0f)
            {
                this.lastContentPosition = this.content.transform.localPosition[this.layoutDimension1];
                float num10 = -Mathf.Sign(a) * 10000f;
                float num11 = a + (num10 * UnityEngine.Time.fixedDeltaTime);
                if ((Mathf.Abs(num11) >= 0.01f) && (Mathf.Sign(num11) == Mathf.Sign(a)))
                {
                    ref Vector3 vectorRef2;
                    Vector3 vector4 = this.content.transform.localPosition;
                    num13 = vectorRef2[num12];
                    (vectorRef2 = (Vector3) &vector4)[num12 = this.layoutDimension1] = num13 + (num11 * UnityEngine.Time.fixedDeltaTime);
                    this.content.transform.localPosition = vector4;
                    this.OnScrolled();
                }
            }
        }
        num = this.content.transform.localPosition[this.layoutDimension1] - this.lastContentPosition;
        if (num != 0f)
        {
            this.PreBufferLongListItems(num < 0f);
        }
        this.lastTouchPosition = touchPosition;
    }

    private void ValidateBreadth()
    {
        if (this.longListBehavior != null)
        {
            this.breadth = 1;
        }
        else
        {
            this.breadth = Math.Max(this.breadth, 1);
        }
    }

    public bool CanScrollAhead
    {
        get
        {
            if (this.ScrollValue < 1f)
            {
                return true;
            }
            if ((this.longListBehavior != null) && (this.items.Count > 0))
            {
                ITouchListItem item = this.items.Last<ITouchListItem>();
                ItemInfo info = this.itemInfos[item];
                for (int i = info.LongListIndex + 1; i < this.longListBehavior.AllItemsCount; i++)
                {
                    if (this.longListBehavior.IsItemShowable(i))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public bool CanScrollBehind
    {
        get
        {
            if (this.ScrollValue > 0f)
            {
                return true;
            }
            if ((this.longListBehavior != null) && (this.items.Count > 0))
            {
                ITouchListItem item = this.items.First<ITouchListItem>();
                ItemInfo info = this.itemInfos[item];
                if (this.longListBehavior.AllItemsCount > 0)
                {
                    for (int i = info.LongListIndex - 1; i >= 0; i--)
                    {
                        if (this.longListBehavior.IsItemShowable(i))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    public Vector2 ClipSize
    {
        get
        {
            this.EnforceInitialized();
            BoxCollider component = base.GetComponent<Collider>() as BoxCollider;
            return new Vector2(component.size.x, (this.layoutPlane != LayoutPlane.XY) ? component.size.z : component.size.y);
        }
        set
        {
            this.EnforceInitialized();
            BoxCollider component = base.GetComponent<Collider>() as BoxCollider;
            Vector3 vector = new Vector3(value.x, 0f, 0f);
            vector[1] = (this.layoutPlane != LayoutPlane.XY) ? component.size.y : value.y;
            vector[2] = (this.layoutPlane != LayoutPlane.XZ) ? component.size.z : value.y;
            Vector3 vector2 = VectorUtils.Abs(component.size - vector);
            if (((vector2.x > 0.0001f) || (vector2.y > 0.0001f)) || (vector2.z > 0.0001f))
            {
                component.size = vector;
                this.UpdateBackgroundBounds();
                if (this.longListBehavior == null)
                {
                    this.RepositionItems(0, null);
                }
                else
                {
                    this.RefreshList(0, true);
                }
                if (this.ClipSizeChanged != null)
                {
                    this.ClipSizeChanged();
                }
            }
        }
    }

    public int Count
    {
        get
        {
            this.EnforceInitialized();
            return this.items.Count;
        }
    }

    public bool IsInitialized
    {
        get
        {
            return (this.content != null);
        }
    }

    public bool IsLayoutSuspended
    {
        get
        {
            return this.layoutSuspended;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            this.EnforceInitialized();
            return false;
        }
    }

    public ITouchListItem this[int index]
    {
        get
        {
            return this.items[index];
        }
        set
        {
            if (this.allowModification)
            {
                this.items[index] = value;
            }
        }
    }

    public ILongListBehavior LongListBehavior
    {
        get
        {
            this.EnforceInitialized();
            return this.longListBehavior;
        }
        set
        {
            this.EnforceInitialized();
            if (value != this.longListBehavior)
            {
                this.allowModification = true;
                this.Clear();
                if (this.longListBehavior != null)
                {
                    this.longListBehavior.ReleaseAllItems();
                }
                this.longListBehavior = value;
                if (this.longListBehavior != null)
                {
                    this.RefreshList(0, false);
                    this.allowModification = false;
                }
            }
        }
    }

    public float ScrollableAmount
    {
        get
        {
            if (this.longListBehavior == null)
            {
                return this.excessContentSize;
            }
            return Mathf.Max((float) 0f, (float) (this.m_fullListContentSize - this.ClipSize[this.GetVector2Dimension(this.layoutDimension1)]));
        }
    }

    public float ScrollValue
    {
        get
        {
            this.EnforceInitialized();
            float scrollableAmount = this.ScrollableAmount;
            float num2 = (scrollableAmount <= 0f) ? 0f : Mathf.Clamp01(-this.content.transform.localPosition[this.layoutDimension1] / scrollableAmount);
            if ((num2 != 0f) && (num2 != 1f))
            {
                return num2;
            }
            return ((-this.GetOutOfBoundsDist(this.content.transform.localPosition[this.layoutDimension1]) / Mathf.Max(this.contentSize, this.ClipSize[this.GetVector2Dimension(this.layoutDimension1)])) + num2);
        }
        set
        {
            this.EnforceInitialized();
            if (!this.dragBeginOffsetFromContent.HasValue && !Mathf.Approximately(this.ScrollValue, value))
            {
                float scrollableAmount = this.ScrollableAmount;
                Vector3 localPosition = this.content.transform.localPosition;
                localPosition[this.layoutDimension1] = -Mathf.Clamp01(value) * scrollableAmount;
                this.content.transform.localPosition = localPosition;
                float num2 = localPosition[this.layoutDimension1] - this.lastContentPosition;
                if (num2 != 0f)
                {
                    this.PreBufferLongListItems(num2 < 0f);
                }
                this.lastContentPosition = localPosition[this.layoutDimension1];
                this.FixUpScrolling();
                this.OnScrolled();
            }
        }
    }

    public int SelectedIndex
    {
        get
        {
            this.EnforceInitialized();
            return (!this.selection.HasValue ? -1 : this.selection.Value);
        }
        set
        {
            this.EnforceInitialized();
            if ((this.SelectionEnabled && (value != this.selection)) && ((this.SelectedIndexChanging == null) || this.SelectedIndexChanging(value)))
            {
                ISelectableTouchListItem selectedItem = this.SelectedItem as ISelectableTouchListItem;
                ISelectableTouchListItem item = ((value == -1) ? null : ((ISelectableTouchListItem) this[value])) as ISelectableTouchListItem;
                if ((value == -1) || ((item != null) && item.Selectable))
                {
                    this.selection = new int?(value);
                }
                if ((selectedItem != null) && (this.selection == value))
                {
                    selectedItem.Unselected();
                }
                if ((this.selection == value) && (item != null))
                {
                    item.Selected();
                    this.ScrollToItem(item);
                }
            }
        }
    }

    public ITouchListItem SelectedItem
    {
        get
        {
            this.EnforceInitialized();
            return ((!this.selection.HasValue || (this.selection.Value == -1)) ? null : this[this.selection.Value]);
        }
        set
        {
            this.EnforceInitialized();
            int index = this.items.IndexOf(value);
            if (index != -1)
            {
                this.SelectedIndex = index;
            }
        }
    }

    public bool SelectionEnabled
    {
        get
        {
            this.EnforceInitialized();
            return this.selection.HasValue;
        }
        set
        {
            this.EnforceInitialized();
            if (value != this.SelectionEnabled)
            {
                if (value)
                {
                    this.selection = -1;
                }
                else
                {
                    this.selection = null;
                }
            }
        }
    }

    public float ViewWindowMaxValue
    {
        get
        {
            float num = this.content.transform.localPosition[this.layoutDimension1];
            float num2 = this.ClipSize[this.GetVector2Dimension(this.layoutDimension1)];
            return ((-num + num2) / this.contentSize);
        }
        set
        {
            Vector3 localPosition = this.content.transform.localPosition;
            localPosition[this.layoutDimension1] = (-Mathf.Clamp01(value) * this.contentSize) + this.ClipSize[this.GetVector2Dimension(this.layoutDimension1)];
            this.content.transform.localPosition = localPosition;
            float num = this.content.transform.localPosition[this.layoutDimension1] - this.lastContentPosition;
            if (num != 0f)
            {
                this.PreBufferLongListItems(num < 0f);
            }
            this.lastContentPosition = localPosition[this.layoutDimension1];
            this.OnScrolled();
        }
    }

    public float ViewWindowMinValue
    {
        get
        {
            float num = this.content.transform.localPosition[this.layoutDimension1];
            return (-num / this.contentSize);
        }
        set
        {
            Vector3 localPosition = this.content.transform.localPosition;
            localPosition[this.layoutDimension1] = -Mathf.Clamp01(value) * this.contentSize;
            this.content.transform.localPosition = localPosition;
            float num = this.content.transform.localPosition[this.layoutDimension1] - this.lastContentPosition;
            if (num != 0f)
            {
                this.PreBufferLongListItems(num < 0f);
            }
            this.lastContentPosition = localPosition[this.layoutDimension1];
            this.OnScrolled();
        }
    }

    public enum Alignment
    {
        Min,
        Mid,
        Max
    }

    public interface ILongListBehavior
    {
        ITouchListItem AcquireItem(int index);
        Vector3 GetItemSize(int allItemsIndex);
        bool IsItemShowable(int allItemsIndex);
        void ReleaseAllItems();
        void ReleaseItem(ITouchListItem item);

        int AllItemsCount { get; }

        int MaxAcquiredItems { get; }

        int MaxVisibleItems { get; }

        int MinBuffer { get; }
    }

    public delegate bool ItemDragEvent(ITouchListItem item, float dragAmount);

    private class ItemInfo
    {
        private readonly ITouchListItem item;

        public ItemInfo(ITouchListItem item, TouchList.LayoutPlane layoutPlane)
        {
            this.item = item;
            Vector3 vector = Vector3.Scale(item.LocalBounds.min, VectorUtils.Abs(item.transform.localScale)) - item.transform.localPosition;
            Vector3 vector2 = Vector3.Scale(item.LocalBounds.max, VectorUtils.Abs(item.transform.localScale)) - item.transform.localPosition;
            this.Size = vector2 - vector;
            Vector3 vector3 = vector;
            if (layoutPlane == TouchList.LayoutPlane.XZ)
            {
                vector3.y = vector2.y;
            }
            this.Offset = item.transform.localPosition - vector3;
        }

        public bool Contains(Vector2 point, TouchList.LayoutPlane layoutPlane)
        {
            Vector3 min = this.Min;
            Vector3 max = this.Max;
            int num = (layoutPlane != TouchList.LayoutPlane.XY) ? 2 : 1;
            return ((((point.x > min.x) && (point.y > min[num])) && (point.x < max.x)) && (point.y < max[num]));
        }

        public int LongListIndex { get; set; }

        public Vector3 Max
        {
            get
            {
                return (this.item.transform.localPosition + Vector3.Scale(this.item.LocalBounds.max, VectorUtils.Abs(this.item.transform.localScale)));
            }
        }

        public Vector3 Min
        {
            get
            {
                return (this.item.transform.localPosition + Vector3.Scale(this.item.LocalBounds.min, VectorUtils.Abs(this.item.transform.localScale)));
            }
        }

        public Vector3 Offset { get; private set; }

        public Vector3 Size { get; private set; }
    }

    public enum LayoutPlane
    {
        XY,
        XZ
    }

    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public delegate void ScrollingEnabledChangedEvent(bool canScroll);

    public delegate bool SelectedIndexChangingEvent(int index);
}

