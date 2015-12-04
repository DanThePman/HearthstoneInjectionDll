using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class UIBScrollable : PegUICustomBehavior
{
    [Tooltip("Drag distance required to initiate deck tile dragging (inches)"), CustomEditField(Sections="Touch Settings")]
    public float m_DeckTileDragThreshold = 0.04f;
    private bool m_Enabled = true;
    private List<EnableScroll> m_EnableScrollListeners = new List<EnableScroll>();
    [CustomEditField(Sections="Preferences")]
    public bool m_ForceScrollAreaHitTest;
    private bool m_ForceShowVisibleAffectedObjects;
    public HeightMode m_HeightMode = HeightMode.UseScrollableItem;
    [CustomEditField(Sections="Thumb Settings")]
    public bool m_HideThumbWhenDisabled;
    private bool m_InputBlocked;
    [CustomEditField(Sections="Touch Settings"), Tooltip("Resistance for slowing down scrolling after the user has let go")]
    public float m_KineticScrollFriction = 6f;
    private float m_LastScrollHeightRecorded;
    private float m_LastTouchScrollValue;
    [CustomEditField(Sections="Touch Settings"), Tooltip("Stopping speed for scrolling after the user has let go")]
    public float m_MinKineticScrollSpeed = 0.01f;
    [CustomEditField(Sections="Touch Settings"), Tooltip("Distance at which the out-of-bounds scroll value will snapped to 0 or 1")]
    public float m_MinOutOfBoundsScrollValue = 0.001f;
    [CustomEditField(Sections="Preferences"), Tooltip("If scrolling is active, all PegUI calls will be suppressed")]
    public bool m_OverridePegUI;
    private bool m_Pause;
    private float m_PolledScrollHeight;
    private Vector3 m_ScrollAreaStartPos;
    [CustomEditField(Sections="Preferences")]
    public float m_ScrollBottomPadding;
    [CustomEditField(Sections="Bounds Settings")]
    public BoxCollider m_ScrollBounds;
    [CustomEditField(Sections="Touch Settings"), Tooltip("Strength of the boundary springs")]
    public float m_ScrollBoundsSpringK = 700f;
    [Tooltip("Use this to match scaling issues."), CustomEditField(Sections="Touch Settings")]
    public float m_ScrollDeltaMultiplier = 1f;
    [CustomEditField(Sections="Preferences")]
    public bool m_ScrollDirectionReverse;
    [Tooltip("Drag distance required to initiate scroll dragging (inches)"), CustomEditField(Sections="Touch Settings")]
    public float m_ScrollDragThreshold = 0.04f;
    [CustomEditField(Sections="Preferences")]
    public iTween.EaseType m_ScrollEaseType = iTween.Defaults.easeType;
    private ScrollHeightCallback m_ScrollHeightCallback;
    [CustomEditField(Sections="Bounds Settings")]
    public GameObject m_ScrollObject;
    [CustomEditField(Sections="Preferences")]
    public ScrollDirection m_ScrollPlane = ScrollDirection.Z;
    [CustomEditField(Sections="Thumb Settings")]
    public ScrollBarThumb m_ScrollThumb;
    [CustomEditField(Sections="Thumb Settings")]
    public BoxCollider m_ScrollTrack;
    [CustomEditField(Sections="Preferences")]
    public float m_ScrollTweenTime = 0.2f;
    private float m_ScrollValue;
    [CustomEditField(Sections="Preferences")]
    public float m_ScrollWheelAmount = 0.1f;
    private Vector2? m_TouchBeginScreenPos;
    private float m_TouchDragBeginScrollValue;
    private Vector3? m_TouchDragBeginWorldPos;
    [CustomEditField(Sections="Optional Bounds Settings")]
    public BoxCollider m_TouchDragFullArea;
    [CustomEditField(Sections="Touch Settings")]
    public List<BoxCollider> m_TouchScrollBlockers = new List<BoxCollider>();
    private List<OnTouchScrollEnded> m_TouchScrollEndedListeners = new List<OnTouchScrollEnded>();
    private List<OnTouchScrollStarted> m_TouchScrollStartedListeners = new List<OnTouchScrollStarted>();
    [CustomEditField(Sections="Camera Settings")]
    public bool m_UseCameraFromLayer;
    private List<VisibleAffectedObject> m_VisibleAffectedObjects = new List<VisibleAffectedObject>();
    [CustomEditField(Sections="Bounds Settings")]
    public float m_VisibleObjectThreshold;
    private static Map<string, float> s_SavedScrollValues = new Map<string, float>();

    public void AddEnableScrollListener(EnableScroll dlg)
    {
        this.m_EnableScrollListeners.Add(dlg);
    }

    private void AddScroll(float amount)
    {
        this.ScrollTo(this.m_ScrollValue + amount, null, false, true, null, null, true);
        this.ResetTouchDrag();
    }

    public void AddTouchScrollEndedListener(OnTouchScrollEnded dlg)
    {
        this.m_TouchScrollEndedListeners.Add(dlg);
    }

    public void AddTouchScrollStartedListener(OnTouchScrollStarted dlg)
    {
        this.m_TouchScrollStartedListeners.Add(dlg);
    }

    public void AddVisibleAffectedObject(GameObject obj, Vector3 extents, bool visible, VisibleAffected callback = null)
    {
        VisibleAffectedObject item = new VisibleAffectedObject {
            Obj = obj,
            Extents = extents,
            Visible = visible,
            Callback = (callback != null) ? callback : new VisibleAffected(UIBScrollable.DefaultVisibleAffectedCallback)
        };
        this.m_VisibleAffectedObjects.Add(item);
    }

    protected override void Awake()
    {
        this.ResetScrollStartPosition();
        if ((this.m_ScrollTrack != null) && (UniversalInputManager.UsePhoneUI == null))
        {
            PegUIElement component = this.m_ScrollTrack.GetComponent<PegUIElement>();
            if (component != null)
            {
                component.AddEventListener(UIEventType.PRESS, e => this.StartDragging());
            }
        }
        if (this.m_OverridePegUI)
        {
            base.Awake();
        }
    }

    [DebuggerHidden]
    private IEnumerator BlockInput(float blockTime)
    {
        return new <BlockInput>c__Iterator294 { blockTime = blockTime, <$>blockTime = blockTime, <>f__this = this };
    }

    public void CenterWorldPosition(Vector3 position)
    {
        float percentage = ((this.m_ScrollObject.transform.InverseTransformPoint(position)[(int) this.m_ScrollPlane] / -(this.m_PolledScrollHeight + this.m_ScrollBottomPadding)) * 2f) - 0.5f;
        base.StartCoroutine(this.BlockInput(this.m_ScrollTweenTime));
        this.SetScroll(percentage, false, true);
    }

    public static void DefaultVisibleAffectedCallback(GameObject obj, bool visible)
    {
        if (obj.activeSelf != visible)
        {
            obj.SetActive(visible);
        }
    }

    private void Drag()
    {
        float num;
        Vector3 min = this.m_ScrollTrack.bounds.min;
        Camera camera = CameraUtils.FindFirstByLayer(this.m_ScrollTrack.gameObject.layer);
        Plane plane = new Plane(-camera.transform.forward, min);
        Ray ray = camera.ScreenPointToRay(UniversalInputManager.Get().GetMousePosition());
        if (plane.Raycast(ray, out num))
        {
            Vector3 point = ray.GetPoint(num);
            float scrollTrackTop = this.GetScrollTrackTop();
            float scrollTrackBtm = this.GetScrollTrackBtm();
            float num4 = Mathf.Clamp01((point[(int) this.m_ScrollPlane] - scrollTrackTop) / (scrollTrackBtm - scrollTrackTop));
            if (Mathf.Abs((float) (this.m_ScrollValue - num4)) > Mathf.Epsilon)
            {
                this.m_ScrollValue = num4;
                this.UpdateThumbPosition();
                this.UpdateScrollObjectPosition(false, null, null, null, false);
            }
        }
        this.ResetTouchDrag();
    }

    public void Enable(bool enable)
    {
        if (this.m_Enabled != enable)
        {
            if (this.m_ScrollThumb != null)
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    this.m_ScrollThumb.gameObject.SetActive(false);
                }
                else
                {
                    this.m_ScrollThumb.gameObject.SetActive(!this.m_HideThumbWhenDisabled || enable);
                }
            }
            this.m_Enabled = enable;
            if (this.m_Enabled)
            {
                this.ResetTouchDrag();
            }
            this.FireEnableScrollEvent();
        }
    }

    public bool EnableIfNeeded()
    {
        bool enable = this.IsScrollNeeded();
        this.Enable(enable);
        return enable;
    }

    private void FireEnableScrollEvent()
    {
        foreach (EnableScroll scroll in this.m_EnableScrollListeners.ToArray())
        {
            scroll(this.m_Enabled);
        }
    }

    private void FireTouchEndEvent()
    {
        OnTouchScrollEnded[] endedArray = this.m_TouchScrollEndedListeners.ToArray();
        for (int i = 0; i < endedArray.Length; i++)
        {
            endedArray[i]();
        }
    }

    private void FireTouchStartEvent()
    {
        OnTouchScrollStarted[] startedArray = this.m_TouchScrollStartedListeners.ToArray();
        for (int i = 0; i < startedArray.Length; i++)
        {
            startedArray[i]();
        }
    }

    public void ForceVisibleAffectedObjectsShow(bool show)
    {
        if (this.m_ForceShowVisibleAffectedObjects != show)
        {
            this.m_ForceShowVisibleAffectedObjects = show;
            this.UpdateAndFireVisibleAffectedObjects();
        }
    }

    private BoxCollider[] GetBoxCollidersMinMax(ref Vector3 min, ref Vector3 max)
    {
        return null;
    }

    private float GetOutOfBoundsDist(float scrollValue)
    {
        if (scrollValue < 0f)
        {
            return scrollValue;
        }
        if (scrollValue > 1f)
        {
            return (scrollValue - 1f);
        }
        return 0f;
    }

    public float GetScroll()
    {
        return this.m_ScrollValue;
    }

    private float GetScrollableItemsHeight()
    {
        Vector3 zero = Vector3.zero;
        Vector3 max = Vector3.zero;
        if (this.GetScrollableItemsMinMax(ref zero, ref max) == null)
        {
            return 0f;
        }
        int scrollPlane = (int) this.m_ScrollPlane;
        return (max[scrollPlane] - zero[scrollPlane]);
    }

    private UIBScrollableItem[] GetScrollableItemsMinMax(ref Vector3 min, ref Vector3 max)
    {
        if (this.m_ScrollObject == null)
        {
            return null;
        }
        UIBScrollableItem[] componentsInChildren = this.m_ScrollObject.GetComponentsInChildren<UIBScrollableItem>(true);
        if ((componentsInChildren == null) || (componentsInChildren.Length == 0))
        {
            return null;
        }
        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        foreach (UIBScrollableItem item in componentsInChildren)
        {
            if (item.IsActive())
            {
                Vector3 vector;
                Vector3 vector2;
                item.GetWorldBounds(out vector, out vector2);
                float[] values = new float[] { min.x, vector.x, vector2.x };
                min.x = Mathf.Min(values);
                float[] singleArray2 = new float[] { min.y, vector.y, vector2.y };
                min.y = Mathf.Min(singleArray2);
                float[] singleArray3 = new float[] { min.z, vector.z, vector2.z };
                min.z = Mathf.Min(singleArray3);
                float[] singleArray4 = new float[] { max.x, vector.x, vector2.x };
                max.x = Mathf.Max(singleArray4);
                float[] singleArray5 = new float[] { max.y, vector.y, vector2.y };
                max.y = Mathf.Max(singleArray5);
                float[] singleArray6 = new float[] { max.z, vector.z, vector2.z };
                max.z = Mathf.Max(singleArray6);
            }
        }
        return componentsInChildren;
    }

    private float GetScrollBoundsHeight()
    {
        if (this.m_ScrollObject == null)
        {
            UnityEngine.Debug.LogWarning("No m_ScrollObject set for this UIBScrollable!");
            return 0f;
        }
        return this.m_ScrollBounds.bounds.size[(int) this.m_ScrollPlane];
    }

    private Camera GetScrollCamera()
    {
        return (!this.m_UseCameraFromLayer ? Box.Get().GetCamera() : CameraUtils.FindFirstByLayer(base.gameObject.layer));
    }

    private float GetScrollTrackBtm()
    {
        return ((this.m_ScrollTrack != null) ? this.m_ScrollTrack.bounds.min[(int) this.m_ScrollPlane] : 0f);
    }

    private float GetScrollTrackTop()
    {
        return ((this.m_ScrollTrack != null) ? this.m_ScrollTrack.bounds.max[(int) this.m_ScrollPlane] : 0f);
    }

    private float GetScrollValueDelta(float worldDelta)
    {
        return ((this.m_ScrollDeltaMultiplier * worldDelta) / this.GetTotalScrollHeight());
    }

    private float GetTotalScrollHeight()
    {
        return this.GetTotalScrollHeightVector().magnitude;
    }

    private Vector3 GetTotalScrollHeightVector()
    {
        if (this.m_ScrollObject == null)
        {
            return Vector3.zero;
        }
        float num = this.m_PolledScrollHeight - this.GetScrollBoundsHeight();
        if (num < 0f)
        {
            return Vector3.zero;
        }
        Vector3 zero = Vector3.zero;
        zero[(int) this.m_ScrollPlane] = num;
        zero = (Vector3) (this.m_ScrollObject.transform.parent.worldToLocalMatrix * zero);
        if (this.m_ScrollBottomPadding > 0f)
        {
            zero += (Vector3) (zero.normalized * this.m_ScrollBottomPadding);
        }
        return zero;
    }

    private Vector3? GetWorldTouchPosition()
    {
        return this.GetWorldTouchPosition(this.m_ScrollBounds);
    }

    private Vector3? GetWorldTouchPosition(BoxCollider bounds)
    {
        RaycastHit hit;
        Ray ray = this.GetScrollCamera().ScreenPointToRay(UniversalInputManager.Get().GetMousePosition());
        foreach (BoxCollider collider in this.m_TouchScrollBlockers)
        {
            if (collider.Raycast(ray, out hit, float.MaxValue))
            {
                return null;
            }
        }
        if (bounds.Raycast(ray, out hit, float.MaxValue))
        {
            return new Vector3?(ray.GetPoint(hit.distance));
        }
        return null;
    }

    private Vector3? GetWorldTouchPositionOnDragArea()
    {
        return this.GetWorldTouchPosition((this.m_TouchDragFullArea == null) ? this.m_ScrollBounds : this.m_TouchDragFullArea);
    }

    public bool IsDragging()
    {
        return (((this.m_ScrollThumb != null) && this.m_ScrollThumb.IsDragging()) || this.m_TouchBeginScreenPos.HasValue);
    }

    public bool IsEnabled()
    {
        return this.m_Enabled;
    }

    public bool IsEnabledAndScrollable()
    {
        return (this.m_Enabled && this.IsScrollNeeded());
    }

    public bool IsObjectVisibleInScrollArea(GameObject obj, Vector3 extents)
    {
        int scrollPlane = (int) this.m_ScrollPlane;
        float num2 = obj.transform.position[scrollPlane] - extents[scrollPlane];
        float num3 = obj.transform.position[scrollPlane] + extents[scrollPlane];
        Bounds bounds = this.m_ScrollBounds.bounds;
        float num4 = bounds.min[scrollPlane] - this.m_VisibleObjectThreshold;
        float num5 = bounds.max[scrollPlane] + this.m_VisibleObjectThreshold;
        return (((num2 >= num4) && (num2 <= num5)) || ((num3 >= num4) && (num3 <= num5)));
    }

    public bool IsScrollNeeded()
    {
        return (this.GetTotalScrollHeight() > 0f);
    }

    public bool IsTouchDragging()
    {
        if (!this.m_TouchBeginScreenPos.HasValue)
        {
            return false;
        }
        float num = Mathf.Abs((float) (UniversalInputManager.Get().GetMousePosition().y - this.m_TouchBeginScreenPos.Value.y));
        return ((this.m_ScrollDragThreshold * ((Screen.dpi <= 0f) ? 150f : Screen.dpi)) <= num);
    }

    public void LoadScroll(string savedName)
    {
        float num = 0f;
        if (s_SavedScrollValues.TryGetValue(savedName, out num))
        {
            this.SetScroll(num, false, true);
            this.ResetTouchDrag();
        }
    }

    protected override void OnDestroy()
    {
        if (this.m_OverridePegUI)
        {
            base.OnDestroy();
        }
    }

    public void Pause(bool pause)
    {
        this.m_Pause = pause;
    }

    public float PollScrollHeight()
    {
        HeightMode heightMode = this.m_HeightMode;
        if (heightMode != HeightMode.UseHeightCallback)
        {
            if (heightMode == HeightMode.UseScrollableItem)
            {
                return this.GetScrollableItemsHeight();
            }
            return 0f;
        }
        return ((this.m_ScrollHeightCallback == null) ? this.m_PolledScrollHeight : this.m_ScrollHeightCallback());
    }

    public void RemoveEnableScrollListener(EnableScroll dlg)
    {
        this.m_EnableScrollListeners.Remove(dlg);
    }

    public void RemoveTouchScrollEndedListener(OnTouchScrollEnded dlg)
    {
        this.m_TouchScrollEndedListeners.Remove(dlg);
    }

    public void RemoveTouchScrollStartedListener(OnTouchScrollStarted dlg)
    {
        this.m_TouchScrollStartedListeners.Remove(dlg);
    }

    public void RemoveVisibleAffectedObject(GameObject obj, VisibleAffected callback)
    {
        <RemoveVisibleAffectedObject>c__AnonStorey396 storey = new <RemoveVisibleAffectedObject>c__AnonStorey396 {
            obj = obj,
            callback = callback
        };
        this.m_VisibleAffectedObjects.RemoveAll(new Predicate<VisibleAffectedObject>(storey.<>m__2B0));
    }

    public void ResetScrollStartPosition()
    {
        if (this.m_ScrollObject != null)
        {
            this.m_ScrollAreaStartPos = this.m_ScrollObject.transform.localPosition;
        }
    }

    public void ResetScrollStartPosition(Vector3 position)
    {
        if (this.m_ScrollObject != null)
        {
            this.m_ScrollAreaStartPos = position;
        }
    }

    private void ResetTouchDrag()
    {
        bool hasValue = this.m_TouchDragBeginWorldPos.HasValue;
        this.m_TouchBeginScreenPos = null;
        this.m_TouchDragBeginWorldPos = null;
        this.m_TouchDragBeginScrollValue = this.m_ScrollValue;
        this.m_LastTouchScrollValue = this.m_ScrollValue;
        if (hasValue)
        {
            this.FireTouchEndEvent();
        }
    }

    public void SaveScroll(string savedName)
    {
        s_SavedScrollValues[savedName] = this.m_ScrollValue;
    }

    private void ScrollTo(float percentage, OnScrollComplete scrollComplete, bool blockInputWhileScrolling, bool tween, iTween.EaseType? tweenType, float? tweenTime, bool clamp)
    {
        this.m_ScrollValue = !clamp ? percentage : Mathf.Clamp01(percentage);
        this.UpdateThumbPosition();
        this.UpdateScrollObjectPosition(tween, scrollComplete, tweenType, tweenTime, blockInputWhileScrolling);
    }

    public void SetHeight(float height)
    {
        this.m_ScrollHeightCallback = null;
        this.m_PolledScrollHeight = height;
        this.UpdateHeight();
    }

    public void SetScroll(float percentage, bool blockInputWhileScrolling = false, bool clamp = true)
    {
        this.SetScroll(percentage, null, blockInputWhileScrolling, clamp);
    }

    public void SetScroll(float percentage, OnScrollComplete scrollComplete, bool blockInputWhileScrolling = false, bool clamp = true)
    {
        base.StartCoroutine(this.SetScrollWait(percentage, scrollComplete, blockInputWhileScrolling, true, null, null, clamp));
    }

    public void SetScroll(float percentage, iTween.EaseType tweenType, float tweenTime, bool blockInputWhileScrolling = false, bool clamp = true)
    {
        this.SetScroll(percentage, null, tweenType, tweenTime, blockInputWhileScrolling, clamp);
    }

    public void SetScroll(float percentage, OnScrollComplete scrollComplete, iTween.EaseType tweenType, float tweenTime, bool blockInputWhileScrolling = false, bool clamp = true)
    {
        base.StartCoroutine(this.SetScrollWait(percentage, scrollComplete, blockInputWhileScrolling, true, new iTween.EaseType?(tweenType), new float?(tweenTime), clamp));
    }

    public void SetScrollHeightCallback(ScrollHeightCallback dlg, bool refresh = false, bool resetScroll = false)
    {
        float? setResetScroll = null;
        if (resetScroll)
        {
            setResetScroll = 0f;
        }
        this.SetScrollHeightCallback(dlg, setResetScroll, refresh);
    }

    public void SetScrollHeightCallback(ScrollHeightCallback dlg, float? setResetScroll, bool refresh = false)
    {
        this.m_VisibleAffectedObjects.Clear();
        this.m_ScrollHeightCallback = dlg;
        if (setResetScroll.HasValue)
        {
            this.m_ScrollValue = setResetScroll.Value;
            this.ResetTouchDrag();
        }
        if (refresh)
        {
            this.UpdateScroll();
            this.UpdateThumbPosition();
            this.UpdateScrollObjectPosition(true, null, null, null, false);
        }
        this.m_PolledScrollHeight = this.PollScrollHeight();
        this.m_LastScrollHeightRecorded = this.m_PolledScrollHeight;
    }

    public void SetScrollSnap(float percentage, bool clamp = true)
    {
        this.SetScrollSnap(percentage, null, clamp);
    }

    public void SetScrollSnap(float percentage, OnScrollComplete scrollComplete, bool clamp = true)
    {
        this.m_PolledScrollHeight = this.PollScrollHeight();
        this.m_LastScrollHeightRecorded = this.m_PolledScrollHeight;
        this.ScrollTo(percentage, scrollComplete, false, false, null, null, clamp);
        this.ResetTouchDrag();
    }

    [DebuggerHidden]
    private IEnumerator SetScrollWait(float percentage, OnScrollComplete scrollComplete, bool blockInputWhileScrolling, bool tween, iTween.EaseType? tweenType, float? tweenTime, bool clamp)
    {
        return new <SetScrollWait>c__Iterator293 { percentage = percentage, scrollComplete = scrollComplete, blockInputWhileScrolling = blockInputWhileScrolling, tween = tween, tweenType = tweenType, tweenTime = tweenTime, clamp = clamp, <$>percentage = percentage, <$>scrollComplete = scrollComplete, <$>blockInputWhileScrolling = blockInputWhileScrolling, <$>tween = tween, <$>tweenType = tweenType, <$>tweenTime = tweenTime, <$>clamp = clamp, <>f__this = this };
    }

    private void StartDragging()
    {
        if ((!this.m_InputBlocked && !this.m_Pause) && this.m_Enabled)
        {
            this.m_ScrollThumb.StartDragging();
        }
    }

    private void Update()
    {
        this.UpdateScroll();
        if ((this.m_Enabled && !this.m_InputBlocked) && !this.m_Pause)
        {
            if (!this.m_ForceScrollAreaHitTest ? UniversalInputManager.Get().InputIsOver(this.GetScrollCamera(), this.m_ScrollBounds.gameObject) : UniversalInputManager.Get().ForcedInputIsOver(this.GetScrollCamera(), this.m_ScrollBounds.gameObject))
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    this.AddScroll(this.m_ScrollWheelAmount);
                }
                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    this.AddScroll(-this.m_ScrollWheelAmount);
                }
            }
            if ((this.m_ScrollThumb != null) && this.m_ScrollThumb.IsDragging())
            {
                this.Drag();
            }
            else if (UniversalInputManager.Get().IsTouchMode())
            {
                this.UpdateTouch();
            }
        }
    }

    private void UpdateAndFireVisibleAffectedObjects()
    {
        foreach (VisibleAffectedObject obj2 in this.m_VisibleAffectedObjects.ToArray())
        {
            bool visible = this.IsObjectVisibleInScrollArea(obj2.Obj, obj2.Extents) || this.m_ForceShowVisibleAffectedObjects;
            if (visible != obj2.Visible)
            {
                obj2.Visible = visible;
                obj2.Callback(obj2.Obj, visible);
            }
        }
    }

    private void UpdateHeight()
    {
        if (Mathf.Abs((float) (this.m_PolledScrollHeight - this.m_LastScrollHeightRecorded)) > 0.001f)
        {
            if (!this.EnableIfNeeded())
            {
                this.m_ScrollValue = 0f;
            }
            this.UpdateThumbPosition();
            this.UpdateScrollObjectPosition(false, null, null, null, false);
            this.ResetTouchDrag();
        }
        this.m_LastScrollHeightRecorded = this.m_PolledScrollHeight;
    }

    public void UpdateScroll()
    {
        this.m_PolledScrollHeight = this.PollScrollHeight();
        this.UpdateHeight();
    }

    private void UpdateScrollObjectPosition(bool tween, OnScrollComplete scrollComplete, iTween.EaseType? tweenType, float? tweenTime, bool blockInputWhileScrolling = false)
    {
        <UpdateScrollObjectPosition>c__AnonStorey397 storey = new <UpdateScrollObjectPosition>c__AnonStorey397 {
            scrollComplete = scrollComplete,
            <>f__this = this
        };
        if (this.m_ScrollObject != null)
        {
            Vector3 scrollAreaStartPos = this.m_ScrollAreaStartPos;
            Vector3 vector2 = scrollAreaStartPos;
            Vector3 totalScrollHeightVector = this.GetTotalScrollHeightVector();
            vector2 += (Vector3) (totalScrollHeightVector * (!this.m_ScrollDirectionReverse ? 1f : -1f));
            Vector3 vector4 = scrollAreaStartPos + ((Vector3) (this.m_ScrollValue * (vector2 - scrollAreaStartPos)));
            if (tween)
            {
                object[] args = new object[] { "position", vector4, "time", !tweenTime.HasValue ? this.m_ScrollTweenTime : tweenTime.Value, "isLocal", true, "easetype", !tweenType.HasValue ? this.m_ScrollEaseType : tweenType.Value, "onupdate", new Action<object>(storey.<>m__2B1), "oncomplete", new Action<object>(storey.<>m__2B2) };
                iTween.MoveTo(this.m_ScrollObject, iTween.Hash(args));
            }
            else
            {
                this.m_ScrollObject.transform.localPosition = vector4;
                this.UpdateAndFireVisibleAffectedObjects();
                if (storey.scrollComplete != null)
                {
                    storey.scrollComplete(this.m_ScrollValue);
                }
            }
        }
    }

    private void UpdateThumbPosition()
    {
        if (this.m_ScrollThumb != null)
        {
            float scrollTrackTop = this.GetScrollTrackTop();
            float scrollTrackBtm = this.GetScrollTrackBtm();
            Vector3 position = this.m_ScrollThumb.transform.position;
            position[(int) this.m_ScrollPlane] = scrollTrackTop + ((scrollTrackBtm - scrollTrackTop) * Mathf.Clamp01(this.m_ScrollValue));
            this.m_ScrollThumb.transform.position = position;
        }
    }

    private void UpdateTouch()
    {
        if (UniversalInputManager.Get().GetMouseButtonDown(0))
        {
            if (this.GetWorldTouchPosition().HasValue)
            {
                this.m_TouchBeginScreenPos = new Vector2?(UniversalInputManager.Get().GetMousePosition());
                return;
            }
        }
        else if (UniversalInputManager.Get().GetMouseButtonUp(0))
        {
            this.m_TouchBeginScreenPos = null;
            this.m_TouchDragBeginWorldPos = null;
            this.FireTouchEndEvent();
        }
        if (this.m_TouchDragBeginWorldPos.HasValue)
        {
            Vector3? worldTouchPositionOnDragArea = this.GetWorldTouchPositionOnDragArea();
            if (worldTouchPositionOnDragArea.HasValue)
            {
                int scrollPlane = (int) this.m_ScrollPlane;
                this.m_LastTouchScrollValue = this.m_ScrollValue;
                float worldDelta = worldTouchPositionOnDragArea.Value[scrollPlane] - this.m_TouchDragBeginWorldPos.Value[scrollPlane];
                float scrollValueDelta = this.GetScrollValueDelta(worldDelta);
                float scrollValue = this.m_TouchDragBeginScrollValue + scrollValueDelta;
                float outOfBoundsDist = this.GetOutOfBoundsDist(scrollValue);
                if (outOfBoundsDist != 0f)
                {
                    outOfBoundsDist = Mathf.Log10(Mathf.Abs(outOfBoundsDist) + 1f) * Mathf.Sign(outOfBoundsDist);
                    scrollValue = (outOfBoundsDist >= 0f) ? (outOfBoundsDist + 1f) : outOfBoundsDist;
                }
                this.ScrollTo(scrollValue, null, false, false, null, null, false);
            }
        }
        else if (this.m_TouchBeginScreenPos.HasValue)
        {
            float num6 = Mathf.Abs((float) (UniversalInputManager.Get().GetMousePosition().x - this.m_TouchBeginScreenPos.Value.x));
            float num7 = Mathf.Abs((float) (UniversalInputManager.Get().GetMousePosition().y - this.m_TouchBeginScreenPos.Value.y));
            bool flag = num6 > (this.m_DeckTileDragThreshold * ((Screen.dpi <= 0f) ? 150f : Screen.dpi));
            bool flag2 = num7 > (this.m_ScrollDragThreshold * ((Screen.dpi <= 0f) ? 150f : Screen.dpi));
            if (flag && ((num6 >= num7) || !flag2))
            {
                this.m_TouchBeginScreenPos = null;
            }
            else if (flag2)
            {
                this.m_TouchDragBeginWorldPos = this.GetWorldTouchPositionOnDragArea();
                this.m_TouchDragBeginScrollValue = this.m_ScrollValue;
                this.m_LastTouchScrollValue = this.m_ScrollValue;
                this.FireTouchStartEvent();
            }
        }
        else
        {
            float f = (this.m_ScrollValue - this.m_LastTouchScrollValue) / UnityEngine.Time.fixedDeltaTime;
            float num9 = this.GetOutOfBoundsDist(this.m_ScrollValue);
            if (num9 != 0f)
            {
                if (Mathf.Abs(num9) >= this.m_MinOutOfBoundsScrollValue)
                {
                    float num10 = (-this.m_ScrollBoundsSpringK * num9) - (Mathf.Sqrt(4f * this.m_ScrollBoundsSpringK) * f);
                    f += num10 * UnityEngine.Time.fixedDeltaTime;
                    this.m_LastTouchScrollValue = this.m_ScrollValue;
                    this.ScrollTo(this.m_ScrollValue + (f * UnityEngine.Time.fixedDeltaTime), null, false, false, null, null, false);
                }
                if (Mathf.Abs(this.GetOutOfBoundsDist(this.m_ScrollValue)) < this.m_MinOutOfBoundsScrollValue)
                {
                    this.ScrollTo(Mathf.Round(this.m_ScrollValue), null, false, false, null, null, false);
                    this.m_LastTouchScrollValue = this.m_ScrollValue;
                }
            }
            else if (this.m_LastTouchScrollValue != this.m_ScrollValue)
            {
                float num11 = Mathf.Sign(f);
                f -= (num11 * this.m_KineticScrollFriction) * UnityEngine.Time.fixedDeltaTime;
                this.m_LastTouchScrollValue = this.m_ScrollValue;
                if ((Mathf.Abs(f) >= this.m_MinKineticScrollSpeed) && (Mathf.Sign(f) == num11))
                {
                    this.ScrollTo(this.m_ScrollValue + (f * UnityEngine.Time.fixedDeltaTime), null, false, false, null, null, false);
                }
            }
        }
    }

    public override bool UpdateUI()
    {
        return (this.IsTouchDragging() && this.m_Enabled);
    }

    [CustomEditField(Sections="Scroll")]
    public float ScrollValue
    {
        get
        {
            return this.m_ScrollValue;
        }
        set
        {
            if (!Application.isEditor)
            {
                this.SetScroll(value, false, false);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <BlockInput>c__Iterator294 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>blockTime;
        internal UIBScrollable <>f__this;
        internal float blockTime;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_InputBlocked = true;
                    this.$current = new WaitForSeconds(this.blockTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_InputBlocked = false;
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <RemoveVisibleAffectedObject>c__AnonStorey396
    {
        internal UIBScrollable.VisibleAffected callback;
        internal GameObject obj;

        internal bool <>m__2B0(UIBScrollable.VisibleAffectedObject o)
        {
            return ((o.Obj == this.obj) && (o.Callback == this.callback));
        }
    }

    [CompilerGenerated]
    private sealed class <SetScrollWait>c__Iterator293 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>blockInputWhileScrolling;
        internal bool <$>clamp;
        internal float <$>percentage;
        internal UIBScrollable.OnScrollComplete <$>scrollComplete;
        internal bool <$>tween;
        internal float? <$>tweenTime;
        internal iTween.EaseType? <$>tweenType;
        internal UIBScrollable <>f__this;
        internal bool blockInputWhileScrolling;
        internal bool clamp;
        internal float percentage;
        internal UIBScrollable.OnScrollComplete scrollComplete;
        internal bool tween;
        internal float? tweenTime;
        internal iTween.EaseType? tweenType;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.ScrollTo(this.percentage, this.scrollComplete, this.blockInputWhileScrolling, this.tween, this.tweenType, this.tweenTime, this.clamp);
                    this.<>f__this.ResetTouchDrag();
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateScrollObjectPosition>c__AnonStorey397
    {
        internal UIBScrollable <>f__this;
        internal UIBScrollable.OnScrollComplete scrollComplete;

        internal void <>m__2B1(object newVal)
        {
            this.<>f__this.UpdateAndFireVisibleAffectedObjects();
        }

        internal void <>m__2B2(object newVal)
        {
            this.<>f__this.UpdateAndFireVisibleAffectedObjects();
            if (this.scrollComplete != null)
            {
                this.scrollComplete(this.<>f__this.m_ScrollValue);
            }
        }
    }

    public delegate void EnableScroll(bool enabled);

    public enum HeightMode
    {
        UseHeightCallback,
        UseScrollableItem,
        UseBoxCollider
    }

    public delegate void OnScrollComplete(float percentage);

    public delegate void OnTouchScrollEnded();

    public delegate void OnTouchScrollStarted();

    public enum ScrollDirection
    {
        X,
        Y,
        Z
    }

    public delegate float ScrollHeightCallback();

    public delegate void ScrollTurnedOn(bool on);

    public delegate void VisibleAffected(GameObject obj, bool visible);

    protected class VisibleAffectedObject
    {
        public UIBScrollable.VisibleAffected Callback;
        public Vector3 Extents;
        public GameObject Obj;
        public bool Visible;
    }
}

