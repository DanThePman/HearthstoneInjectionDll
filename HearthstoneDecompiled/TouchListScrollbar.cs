using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TouchListScrollbar : PegUIElement
{
    public GameObject cover;
    private bool isActive;
    public TouchList list;
    public PegUIElement thumb;
    public Transform thumbMax;
    public Transform thumbMin;
    public PegUIElement track;

    protected override void Awake()
    {
        if (this.list.orientation == TouchList.Orientation.Horizontal)
        {
            UnityEngine.Debug.LogError("Horizontal TouchListScrollbar not implemented");
            UnityEngine.Object.Destroy(this);
        }
        else
        {
            base.Awake();
            this.ShowThumb(this.isActive);
            this.list.ClipSizeChanged += new System.Action(this.UpdateLayout);
            this.list.ScrollingEnabledChanged += new TouchList.ScrollingEnabledChangedEvent(this.UpdateActive);
            this.list.Scrolled += new System.Action(this.UpdateThumb);
            this.thumb.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.ThumbPressed));
            this.track.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.TrackPressed));
            this.UpdateLayout();
        }
    }

    private Vector3 GetTouchPoint(Plane dragPlane, Camera camera)
    {
        float num;
        Ray ray = camera.ScreenPointToRay(UniversalInputManager.Get().GetMousePosition());
        dragPlane.Raycast(ray, out num);
        return ray.GetPoint(num);
    }

    private void ShowThumb(bool show)
    {
        Transform transform = this.thumb.transform.FindChild("Mesh");
        if (transform != null)
        {
            transform.gameObject.SetActive(show);
        }
        if (this.cover != null)
        {
            this.cover.SetActive(!show);
        }
    }

    private void ThumbPressed(UIEvent e)
    {
        base.StartCoroutine(this.UpdateThumbDrag());
    }

    private void TrackPressed(UIEvent e)
    {
        Camera camera = CameraUtils.FindFirstByLayer(base.gameObject.layer);
        Plane dragPlane = new Plane(-camera.transform.forward, this.track.transform.position);
        float num = Mathf.Clamp(this.GetTouchPoint(dragPlane, camera).y, this.thumbMax.position.y, this.thumbMin.position.y);
        this.list.ScrollValue = (num - this.thumbMin.position.y) / (this.thumbMax.position.y - this.thumbMin.position.y);
    }

    private void UpdateActive(bool canScroll)
    {
        if (this.isActive != canScroll)
        {
            this.isActive = canScroll;
            this.thumb.GetComponent<Collider>().enabled = this.isActive;
            if (this.isActive)
            {
                this.UpdateThumb();
            }
            this.ShowThumb(this.isActive);
        }
    }

    private void UpdateLayout()
    {
        TransformUtil.SetPosX(this.thumb, this.thumbMin.position.x);
        this.UpdateThumb();
    }

    private void UpdateThumb()
    {
        if (this.isActive)
        {
            TransformUtil.SetPosZ(this.thumb, base.GetComponent<Collider>().bounds.min.z);
            float scrollValue = this.list.ScrollValue;
            float y = this.thumbMin.position.y + ((this.thumbMax.position.y - this.thumbMin.position.y) * Mathf.Clamp01(scrollValue));
            TransformUtil.SetPosY(this.thumb, y);
            this.thumb.transform.localScale = Vector3.one;
            if ((scrollValue < 0f) || (scrollValue > 1f))
            {
                float num3 = 1f / ((scrollValue >= 0f) ? scrollValue : (-scrollValue + 1f));
                Vector3 vector5 = (scrollValue >= 0f) ? this.thumb.GetComponent<Collider>().bounds.min : this.thumb.GetComponent<Collider>().bounds.max;
                float num4 = vector5.y;
                float num5 = (this.thumb.transform.position.y - num4) * (num3 - 1f);
                TransformUtil.SetPosY(this.thumb, this.thumb.transform.position.y + num5);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateThumbDrag()
    {
        return new <UpdateThumbDrag>c__Iterator295 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <UpdateThumbDrag>c__Iterator295 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TouchListScrollbar <>f__this;
        internal Camera <camera>__0;
        internal float <dragOffset>__2;
        internal Plane <dragPlane>__1;
        internal float <thumbPosY>__3;

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
                {
                    this.<camera>__0 = CameraUtils.FindFirstByLayer(this.<>f__this.gameObject.layer);
                    this.<dragPlane>__1 = new Plane(-this.<camera>__0.transform.forward, this.<>f__this.thumb.transform.position);
                    Vector3 vector = this.<>f__this.thumb.transform.position - this.<>f__this.GetTouchPoint(this.<dragPlane>__1, this.<camera>__0);
                    this.<dragOffset>__2 = vector.y;
                    break;
                }
                case 1:
                    break;

                default:
                    goto Label_01B1;
            }
            if (!UniversalInputManager.Get().GetMouseButtonUp(0))
            {
                this.<thumbPosY>__3 = this.<>f__this.GetTouchPoint(this.<dragPlane>__1, this.<camera>__0).y + this.<dragOffset>__2;
                this.<thumbPosY>__3 = Mathf.Clamp(this.<thumbPosY>__3, this.<>f__this.thumbMax.position.y, this.<>f__this.thumbMin.position.y);
                this.<>f__this.list.ScrollValue = (this.<thumbPosY>__3 - this.<>f__this.thumbMin.position.y) / (this.<>f__this.thumbMax.position.y - this.<>f__this.thumbMin.position.y);
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.$PC = -1;
        Label_01B1:
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
}

