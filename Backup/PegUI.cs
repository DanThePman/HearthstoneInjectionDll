using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PegUI : MonoBehaviour
{
    private const float DOUBLECLICK_COUNT_DISABLED = -1f;
    private const float DOUBLECLICK_TOLERANCE = 0.7f;
    private static readonly GameLayer[] HIT_TEST_PRIORITY = new GameLayer[] { GameLayer.IgnoreFullScreenEffects, GameLayer.BackgroundUI, GameLayer.PerspectiveUI, GameLayer.CameraMask, GameLayer.UI, GameLayer.BattleNet, GameLayer.BattleNetFriendList, GameLayer.BattleNetChat, GameLayer.BattleNetDialog };
    private PegUIElement m_currentElement;
    private List<PegUICustomBehavior> m_customBehaviors = new List<PegUICustomBehavior>();
    private Vector3 m_lastClickPosition;
    private float m_lastClickTimer;
    private PegUIElement m_mouseDownElement;
    private PegUIElement m_prevElement;
    private DelSwipeListener m_swipeListener;
    private Camera m_UICam;
    private const float MOUSEDOWN_COUNT_DISABLED = -1f;
    public Camera orthographicUICam;
    public static PegUI s_instance;

    private void Awake()
    {
        s_instance = this;
        this.m_lastClickPosition = Vector3.zero;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
    }

    public void CancelSwipeDetection(DelSwipeListener listener)
    {
        if (listener == this.m_swipeListener)
        {
            this.m_swipeListener = null;
        }
    }

    public void DoMouseDown(PegUIElement element, Vector3 mouseDownPos)
    {
        this.m_currentElement = element;
        this.m_mouseDownElement = element;
        this.m_currentElement.TriggerPress();
        this.m_lastClickPosition = mouseDownPos;
        if (Vector3.Distance(this.m_lastClickPosition, UniversalInputManager.Get().GetMousePosition()) > this.m_currentElement.GetDragTolerance())
        {
            this.m_currentElement.TriggerHold();
        }
    }

    public void EnableSwipeDetection(Rect swipeRect, DelSwipeListener listener)
    {
        this.m_swipeListener = listener;
    }

    public PegUIElement FindHitElement()
    {
        if ((!UniversalInputManager.Get().IsTouchMode() || UniversalInputManager.Get().GetMouseButton(0)) || UniversalInputManager.Get().GetMouseButtonUp(0))
        {
            RaycastHit hit;
            foreach (GameLayer layer in HIT_TEST_PRIORITY)
            {
                if (UniversalInputManager.Get().GetInputHitInfo(layer, out hit))
                {
                    return hit.transform.GetComponent<PegUIElement>();
                }
            }
            if (UniversalInputManager.Get().GetInputHitInfo(this.m_UICam, out hit))
            {
                return hit.transform.GetComponent<PegUIElement>();
            }
        }
        return null;
    }

    public static PegUI Get()
    {
        return s_instance;
    }

    public PegUIElement GetMousedOverElement()
    {
        return this.m_currentElement;
    }

    public PegUIElement GetPrevMousedOverElement()
    {
        return this.m_prevElement;
    }

    private void MouseInputUpdate()
    {
        if (this.m_UICam != null)
        {
            bool flag = false;
            foreach (PegUICustomBehavior behavior in this.m_customBehaviors)
            {
                if (behavior.UpdateUI())
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                if (this.m_mouseDownElement != null)
                {
                    this.m_mouseDownElement.TriggerOut();
                }
                this.m_mouseDownElement = null;
            }
            else
            {
                if ((UniversalInputManager.Get().GetMouseButton(0) && (this.m_mouseDownElement != null)) && ((this.m_lastClickPosition != Vector3.zero) && (Vector3.Distance(this.m_lastClickPosition, UniversalInputManager.Get().GetMousePosition()) > this.m_mouseDownElement.GetDragTolerance())))
                {
                    this.m_mouseDownElement.TriggerHold();
                }
                if (this.m_lastClickTimer != -1f)
                {
                    this.m_lastClickTimer += UnityEngine.Time.deltaTime;
                }
                PegUIElement element = this.FindHitElement();
                this.m_prevElement = this.m_currentElement;
                if ((element != null) && element.IsEnabled())
                {
                    this.m_currentElement = element;
                }
                else
                {
                    this.m_currentElement = null;
                }
                if ((this.m_prevElement != null) && (this.m_currentElement != this.m_prevElement))
                {
                    PegCursor.Get().SetMode(PegCursor.Mode.UP);
                    this.m_prevElement.TriggerOut();
                }
                if (this.m_currentElement == null)
                {
                    if (UniversalInputManager.Get().GetMouseButtonDown(0))
                    {
                        PegCursor.Get().SetMode(PegCursor.Mode.DOWN);
                    }
                    else if (!UniversalInputManager.Get().GetMouseButton(0))
                    {
                        PegCursor.Get().SetMode(PegCursor.Mode.UP);
                    }
                    if ((this.m_mouseDownElement != null) && UniversalInputManager.Get().GetMouseButtonUp(0))
                    {
                        this.m_mouseDownElement.TriggerReleaseAll(false);
                        this.m_mouseDownElement = null;
                    }
                }
                else if (!this.UpdateMouseLeftClick() && (!this.UpdateMouseLeftHold() && !this.UpdateMouseRightClick()))
                {
                    this.UpdateMouseOver();
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (UniversalInputManager.Get() != null)
        {
            UniversalInputManager.Get().UnregisterMouseOnOrOffScreenListener(new UniversalInputManager.MouseOnOrOffScreenCallback(this.OnMouseOnOrOffScreen));
        }
        s_instance = null;
    }

    private void OnMouseOnOrOffScreen(bool onScreen)
    {
        if (!onScreen)
        {
            this.m_lastClickPosition = Vector3.zero;
            if (this.m_currentElement != null)
            {
                this.m_currentElement.TriggerOut();
                this.m_currentElement = null;
            }
            PegCursor.Get().SetMode(PegCursor.Mode.UP);
            if (this.m_prevElement != null)
            {
                this.m_prevElement.TriggerOut();
                this.m_prevElement = null;
            }
            this.m_lastClickTimer = -1f;
        }
    }

    public void RegisterCustomBehavior(PegUICustomBehavior behavior)
    {
        this.m_customBehaviors.Add(behavior);
    }

    public void SetInputCamera(Camera cam)
    {
        this.m_UICam = cam;
        if (this.m_UICam == null)
        {
            Debug.Log("UICam is null!");
        }
    }

    private void Start()
    {
        UniversalInputManager.Get().RegisterMouseOnOrOffScreenListener(new UniversalInputManager.MouseOnOrOffScreenCallback(this.OnMouseOnOrOffScreen));
    }

    public void UnregisterCustomBehavior(PegUICustomBehavior behavior)
    {
        this.m_customBehaviors.Remove(behavior);
    }

    private void Update()
    {
        this.MouseInputUpdate();
    }

    private bool UpdateMouseLeftClick()
    {
        bool flag = false;
        if (UniversalInputManager.Get().GetMouseButtonDown(0))
        {
            flag = true;
            if (this.m_currentElement.GetCursorDown() != PegCursor.Mode.NONE)
            {
                PegCursor.Get().SetMode(this.m_currentElement.GetCursorDown());
            }
            else
            {
                PegCursor.Get().SetMode(PegCursor.Mode.DOWN);
            }
            if (UniversalInputManager.Get().IsTouchMode() && this.m_currentElement.GetReceiveOverWithMouseDown())
            {
                this.m_currentElement.TriggerOver();
            }
            this.m_currentElement.TriggerPress();
            this.m_lastClickPosition = UniversalInputManager.Get().GetMousePosition();
            this.m_mouseDownElement = this.m_currentElement;
        }
        if (UniversalInputManager.Get().GetMouseButtonUp(0))
        {
            flag = true;
            if (((this.m_lastClickTimer > 0f) && (this.m_lastClickTimer <= 0.7f)) && this.m_currentElement.GetDoubleClickEnabled())
            {
                this.m_currentElement.TriggerDoubleClick();
                this.m_lastClickTimer = -1f;
            }
            else
            {
                if ((this.m_mouseDownElement == this.m_currentElement) || this.m_currentElement.GetReceiveReleaseWithoutMouseDown())
                {
                    this.m_currentElement.TriggerRelease();
                }
                if (this.m_currentElement.GetReceiveOverWithMouseDown())
                {
                    this.m_currentElement.TriggerOut();
                }
                if (this.m_mouseDownElement != null)
                {
                    this.m_lastClickTimer = 0f;
                    this.m_mouseDownElement.TriggerReleaseAll(this.m_currentElement == this.m_mouseDownElement);
                    this.m_mouseDownElement = null;
                }
            }
            if (this.m_currentElement.GetCursorOver() != PegCursor.Mode.NONE)
            {
                PegCursor.Get().SetMode(this.m_currentElement.GetCursorOver());
            }
            else
            {
                PegCursor.Get().SetMode(PegCursor.Mode.OVER);
            }
            this.m_lastClickPosition = Vector3.zero;
        }
        return flag;
    }

    private bool UpdateMouseLeftHold()
    {
        if (!UniversalInputManager.Get().GetMouseButton(0))
        {
            return false;
        }
        if (this.m_currentElement.GetReceiveOverWithMouseDown() && (this.m_currentElement != this.m_prevElement))
        {
            if (this.m_currentElement.GetCursorOver() != PegCursor.Mode.NONE)
            {
                PegCursor.Get().SetMode(this.m_currentElement.GetCursorOver());
            }
            else
            {
                PegCursor.Get().SetMode(PegCursor.Mode.OVER);
            }
            this.m_currentElement.TriggerOver();
        }
        return true;
    }

    private void UpdateMouseOver()
    {
        if ((!UniversalInputManager.Get().IsTouchMode() || (UniversalInputManager.Get().GetMouseButton(0) && this.m_currentElement.GetReceiveOverWithMouseDown())) && (this.m_currentElement != this.m_prevElement))
        {
            if (this.m_currentElement.GetCursorOver() != PegCursor.Mode.NONE)
            {
                PegCursor.Get().SetMode(this.m_currentElement.GetCursorOver());
            }
            else
            {
                PegCursor.Get().SetMode(PegCursor.Mode.OVER);
            }
            this.m_currentElement.TriggerOver();
        }
    }

    private bool UpdateMouseRightClick()
    {
        bool flag = false;
        if (UniversalInputManager.Get().GetMouseButtonDown(1))
        {
            flag = true;
            this.m_currentElement.TriggerRightClick();
        }
        return flag;
    }

    public delegate void DelSwipeListener(PegUI.SWIPE_DIRECTION direction);

    public enum Layer
    {
        MANUAL,
        RELATIVE_TO_PARENT,
        BACKGROUND,
        HUD,
        DIALOG
    }

    public enum SWIPE_DIRECTION
    {
        RIGHT,
        LEFT
    }
}

