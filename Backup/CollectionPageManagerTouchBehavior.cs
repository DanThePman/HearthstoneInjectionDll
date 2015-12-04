using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CollectionPageManagerTouchBehavior : PegUICustomBehavior
{
    private Vector2 m_swipeStartPosition;
    private PegUIElement pageDragRegion;
    private PegUIElement pageLeftRegion;
    private PegUIElement pageRightRegion;
    private SwipeState swipeState;
    private float TurnDist = 0.07f;

    protected override void Awake()
    {
        base.Awake();
        CollectionPageManager component = base.GetComponent<CollectionPageManager>();
        this.pageLeftRegion = component.m_pageLeftClickableRegion;
        this.pageRightRegion = component.m_pageRightClickableRegion;
        this.pageDragRegion = component.m_pageDraggableRegion;
        this.pageDragRegion.gameObject.SetActive(true);
        this.pageDragRegion.AddEventListener(UIEventType.PRESS, new UIEvent.Handler(this.OnPageDraggableRegionDown));
    }

    private Vector2 GetTouchPosition()
    {
        Vector3 touchPosition = W8Touch.Get().GetTouchPosition();
        return new Vector2(touchPosition.x, touchPosition.y);
    }

    [DebuggerHidden]
    private IEnumerator HandlePageTurnGesture()
    {
        return new <HandlePageTurnGesture>c__Iterator41 { <>f__this = this };
    }

    private PegUIElement HitTestPageTurnRegions()
    {
        PegUIElement component = null;
        RaycastHit hit;
        this.pageDragRegion.GetComponent<Collider>().enabled = false;
        if (UniversalInputManager.Get().GetInputHitInfo(out hit))
        {
            component = hit.collider.GetComponent<PegUIElement>();
            if ((component != this.pageLeftRegion) && (component != this.pageRightRegion))
            {
                component = null;
            }
        }
        this.pageDragRegion.GetComponent<Collider>().enabled = true;
        return component;
    }

    protected override void OnDestroy()
    {
        this.pageDragRegion.gameObject.SetActive(false);
        this.pageDragRegion.RemoveEventListener(UIEventType.PRESS, new UIEvent.Handler(this.OnPageDraggableRegionDown));
        base.OnDestroy();
    }

    private void OnPageDraggableRegionDown(UIEvent e)
    {
        if (base.gameObject != null)
        {
            this.TryStartPageTurnGesture();
        }
    }

    private void TryStartPageTurnGesture()
    {
        if (this.swipeState != SwipeState.Update)
        {
            base.StartCoroutine(this.HandlePageTurnGesture());
        }
    }

    public override bool UpdateUI()
    {
        if (CollectionInputMgr.Get().HasHeldCard() || ((CraftingManager.Get() != null) && CraftingManager.Get().IsCardShowing()))
        {
            return false;
        }
        bool flag = false;
        if (UniversalInputManager.Get().GetMouseButtonUp(0))
        {
            flag = this.swipeState == SwipeState.Success;
            this.swipeState = SwipeState.None;
        }
        return ((this.swipeState != SwipeState.None) || flag);
    }

    [CompilerGenerated]
    private sealed class <HandlePageTurnGesture>c__Iterator41 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CollectionPageManagerTouchBehavior <>f__this;
        internal float <pixelDist>__1;
        internal float <pixelTurnDist>__0;
        internal Vector2 <swipeCurrentPosition>__3;
        internal PegUIElement <touchDownPageTurnRegion>__2;

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
                    if (UniversalInputManager.Get().IsTouchMode())
                    {
                        break;
                    }
                    this.$current = null;
                    this.$PC = 1;
                    goto Label_01EC;

                case 1:
                    break;

                case 2:
                    goto Label_0190;

                default:
                    goto Label_01EA;
            }
            this.<>f__this.m_swipeStartPosition = this.<>f__this.GetTouchPosition();
            this.<>f__this.swipeState = CollectionPageManagerTouchBehavior.SwipeState.Update;
            this.<pixelTurnDist>__0 = Mathf.Clamp((float) (this.<>f__this.TurnDist * Screen.currentResolution.width), (float) 2f, (float) 300f);
            this.<pixelDist>__1 = 0f;
            this.<touchDownPageTurnRegion>__2 = this.<>f__this.HitTestPageTurnRegions();
        Label_0190:
            while (!UniversalInputManager.Get().GetMouseButtonUp(0))
            {
                this.<swipeCurrentPosition>__3 = this.<>f__this.GetTouchPosition();
                Vector2 vector = this.<swipeCurrentPosition>__3 - this.<>f__this.m_swipeStartPosition;
                this.<pixelDist>__1 = vector.x;
                if ((this.<pixelDist>__1 <= -this.<pixelTurnDist>__0) && this.<>f__this.pageRightRegion.enabled)
                {
                    this.<>f__this.pageRightRegion.TriggerRelease();
                    this.<>f__this.swipeState = CollectionPageManagerTouchBehavior.SwipeState.Success;
                    goto Label_01EA;
                }
                if ((this.<pixelDist>__1 >= this.<pixelTurnDist>__0) && this.<>f__this.pageLeftRegion.enabled)
                {
                    this.<>f__this.pageLeftRegion.TriggerRelease();
                    this.<>f__this.swipeState = CollectionPageManagerTouchBehavior.SwipeState.Success;
                    goto Label_01EA;
                }
                this.$current = null;
                this.$PC = 2;
                goto Label_01EC;
            }
            if ((this.<touchDownPageTurnRegion>__2 != null) && (this.<touchDownPageTurnRegion>__2 == this.<>f__this.HitTestPageTurnRegions()))
            {
                this.<touchDownPageTurnRegion>__2.TriggerRelease();
            }
            this.<>f__this.swipeState = CollectionPageManagerTouchBehavior.SwipeState.None;
            this.$PC = -1;
        Label_01EA:
            return false;
        Label_01EC:
            return true;
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

    private enum SwipeState
    {
        None,
        Update,
        Success
    }
}

