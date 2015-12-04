using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SwipeHandler : PegUICustomBehavior
{
    private bool m_checkingPossibleSwipe;
    public Transform m_lowerRightBone;
    private float m_swipeDetectTimer;
    private PegUIElement m_swipeElement;
    private static DelSwipeListener m_swipeListener;
    private Rect m_swipeRect;
    private Vector3 m_swipeStart;
    public Transform m_upperLeftBone;
    private const float SWIPE_DETECT_DURATION = 0.1f;
    private const float SWIPE_DETECT_WIDTH = 0.09f;
    private const float SWIPE_FROM_TARGET_PENALTY = 0.035f;

    private bool CheckSwipe()
    {
        SWIPE_DIRECTION rIGHT;
        float f = this.m_swipeStart.x - UniversalInputManager.Get().GetMousePosition().x;
        float num2 = 0.09f + ((this.m_swipeElement == null) ? 0f : 0.035f);
        float num3 = Screen.width * num2;
        if (Mathf.Abs(f) <= num3)
        {
            return false;
        }
        if (f < 0f)
        {
            rIGHT = SWIPE_DIRECTION.RIGHT;
        }
        else
        {
            rIGHT = SWIPE_DIRECTION.LEFT;
        }
        if (m_swipeListener != null)
        {
            m_swipeListener(rIGHT);
        }
        return true;
    }

    private bool HandleSwipeGesture()
    {
        this.m_swipeRect = CameraUtils.CreateGUIScreenRect(Camera.main, (Component) this.m_upperLeftBone, (Component) this.m_lowerRightBone);
        if (UniversalInputManager.Get().GetMouseButtonDown(0) && this.InSwipeRect(UniversalInputManager.Get().GetMousePosition()))
        {
            this.m_checkingPossibleSwipe = true;
            this.m_swipeDetectTimer = 0f;
            this.m_swipeStart = UniversalInputManager.Get().GetMousePosition();
            this.m_swipeElement = PegUI.Get().FindHitElement();
            return true;
        }
        if (this.m_checkingPossibleSwipe)
        {
            this.m_swipeDetectTimer += UnityEngine.Time.deltaTime;
            if (UniversalInputManager.Get().GetMouseButtonUp(0))
            {
                this.m_checkingPossibleSwipe = false;
                if ((!this.CheckSwipe() && (this.m_swipeElement != null)) && (this.m_swipeElement == PegUI.Get().FindHitElement()))
                {
                    this.m_swipeElement.TriggerPress();
                    this.m_swipeElement.TriggerRelease();
                }
                return true;
            }
            if (this.m_swipeDetectTimer < 0.1f)
            {
                return true;
            }
            this.m_checkingPossibleSwipe = false;
            if (this.CheckSwipe())
            {
                return true;
            }
            if (this.m_swipeElement != null)
            {
                PegUI.Get().DoMouseDown(this.m_swipeElement, this.m_swipeStart);
            }
        }
        return false;
    }

    private bool InSwipeRect(Vector2 v)
    {
        return ((((v.x >= this.m_swipeRect.x) && (v.x <= (this.m_swipeRect.x + this.m_swipeRect.width))) && (v.y >= this.m_swipeRect.y)) && (v.y <= (this.m_swipeRect.y + this.m_swipeRect.height)));
    }

    public void RegisterSwipeListener(DelSwipeListener listener)
    {
        m_swipeListener = listener;
    }

    public override bool UpdateUI()
    {
        if (!UniversalInputManager.Get().IsTouchMode())
        {
            return false;
        }
        return this.HandleSwipeGesture();
    }

    public delegate void DelSwipeListener(SwipeHandler.SWIPE_DIRECTION direction);

    public enum SWIPE_DIRECTION
    {
        RIGHT,
        LEFT
    }
}

