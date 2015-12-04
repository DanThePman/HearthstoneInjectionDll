using System;
using UnityEngine;

public class PegCursor : MonoBehaviour
{
    private Mode m_currentMode;
    public Texture2D m_cursorDown;
    public Texture2D m_cursorDown64;
    public Vector2 m_cursorDownHotspot = Vector2.zero;
    public Vector2 m_cursorDownHotspot64 = Vector2.zero;
    public Texture2D m_cursorDrag;
    public Texture2D m_cursorDrag64;
    public Vector2 m_cursorDragHotspot = Vector2.zero;
    public Vector2 m_cursorDragHotspot64 = Vector2.zero;
    public Texture2D m_cursorOver;
    public Texture2D m_cursorOver64;
    public Vector2 m_cursorOverHotspot = Vector2.zero;
    public Vector2 m_cursorOverHotspot64 = Vector2.zero;
    private Texture2D m_cursorTexture;
    public Texture2D m_cursorUp;
    public Texture2D m_cursorUp64;
    public Vector2 m_cursorUpHotspot = Vector2.zero;
    public Vector2 m_cursorUpHotspot64 = Vector2.zero;
    public Texture2D m_cursorWaiting;
    public Texture2D m_cursorWaiting64;
    public Texture2D m_cursorWaitingDown;
    public Texture2D m_cursorWaitingDown64;
    public Vector2 m_cursorWaitingDownHotspot = Vector2.zero;
    public Vector2 m_cursorWaitingDownHotspot64 = Vector2.zero;
    public Vector2 m_cursorWaitingHotspot = Vector2.zero;
    public Vector2 m_cursorWaitingHotspot64 = Vector2.zero;
    public Texture2D m_cursorWaitingUp;
    public Texture2D m_cursorWaitingUp64;
    public Vector2 m_cursorWaitingUpHotspot = Vector2.zero;
    public Vector2 m_cursorWaitingUpHotspot64 = Vector2.zero;
    public GameObject m_explosionPrefab;
    public Texture2D m_leftArrow;
    public Texture2D m_leftArrow64;
    public Vector2 m_leftArrowHotspot = Vector2.zero;
    public Vector2 m_leftArrowHotspot64 = Vector2.zero;
    public Texture2D m_rightArrow;
    public Texture2D m_rightArrow64;
    public Vector2 m_rightArrowHotspot = Vector2.zero;
    public Vector2 m_rightArrowHotspot64 = Vector2.zero;
    private static PegCursor s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public static PegCursor Get()
    {
        return s_instance;
    }

    public GameObject GetExplosionPrefab()
    {
        return this.m_explosionPrefab;
    }

    public Mode GetMode()
    {
        return this.m_currentMode;
    }

    public void Hide()
    {
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void SetMode(Mode mode)
    {
        bool flag = false;
        if ((this.m_currentMode == Mode.WAITING) && (mode != Mode.STOPWAITING))
        {
            if (mode == Mode.DOWN)
            {
                if (flag)
                {
                    Cursor.SetCursor(this.m_cursorWaitingDown64, this.m_cursorWaitingDownHotspot64, CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(this.m_cursorWaitingDown, this.m_cursorWaitingDownHotspot, CursorMode.Auto);
                }
            }
            else if (mode == Mode.UP)
            {
                if (flag)
                {
                    Cursor.SetCursor(this.m_cursorWaiting64, this.m_cursorWaitingHotspot64, CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(this.m_cursorWaiting, this.m_cursorWaitingHotspot, CursorMode.Auto);
                }
            }
        }
        else if ((this.m_currentMode != Mode.DRAG) || (mode == Mode.STOPDRAG))
        {
            this.m_currentMode = mode;
            if (flag)
            {
                switch (mode)
                {
                    case Mode.UP:
                        Cursor.SetCursor(this.m_cursorUp64, this.m_cursorUpHotspot64, CursorMode.Auto);
                        break;

                    case Mode.DOWN:
                        Cursor.SetCursor(this.m_cursorDown64, this.m_cursorDownHotspot64, CursorMode.Auto);
                        break;

                    case Mode.OVER:
                        Cursor.SetCursor(this.m_cursorUp64, this.m_cursorUpHotspot64, CursorMode.Auto);
                        break;

                    case Mode.DRAG:
                        Cursor.SetCursor(this.m_cursorDrag64, this.m_cursorDragHotspot64, CursorMode.Auto);
                        break;

                    case Mode.STOPDRAG:
                    case Mode.STOPWAITING:
                        Cursor.SetCursor(this.m_cursorUp64, this.m_cursorUpHotspot64, CursorMode.Auto);
                        break;

                    case Mode.WAITING:
                        Cursor.SetCursor(this.m_cursorWaiting64, this.m_cursorWaitingHotspot64, CursorMode.Auto);
                        break;

                    case Mode.LEFTARROW:
                        Cursor.SetCursor(this.m_leftArrow64, this.m_leftArrowHotspot64, CursorMode.Auto);
                        break;

                    case Mode.RIGHTARROW:
                        Cursor.SetCursor(this.m_rightArrow64, this.m_rightArrowHotspot64, CursorMode.Auto);
                        break;
                }
            }
            else
            {
                switch (mode)
                {
                    case Mode.UP:
                        Cursor.SetCursor(this.m_cursorUp, this.m_cursorUpHotspot, CursorMode.Auto);
                        break;

                    case Mode.DOWN:
                        Cursor.SetCursor(this.m_cursorDown, this.m_cursorDownHotspot, CursorMode.Auto);
                        break;

                    case Mode.OVER:
                        Cursor.SetCursor(this.m_cursorUp, this.m_cursorUpHotspot, CursorMode.Auto);
                        break;

                    case Mode.DRAG:
                        Cursor.SetCursor(this.m_cursorDrag, this.m_cursorDragHotspot, CursorMode.Auto);
                        break;

                    case Mode.STOPDRAG:
                    case Mode.STOPWAITING:
                        Cursor.SetCursor(this.m_cursorUp, this.m_cursorUpHotspot, CursorMode.Auto);
                        break;

                    case Mode.WAITING:
                        Cursor.SetCursor(this.m_cursorWaiting, this.m_cursorWaitingHotspot, CursorMode.Auto);
                        break;

                    case Mode.LEFTARROW:
                        Cursor.SetCursor(this.m_leftArrow, this.m_leftArrowHotspot, CursorMode.Auto);
                        break;

                    case Mode.RIGHTARROW:
                        Cursor.SetCursor(this.m_rightArrow, this.m_rightArrowHotspot, CursorMode.Auto);
                        break;
                }
            }
        }
    }

    public void Show()
    {
        Cursor.visible = true;
    }

    public enum Mode
    {
        UP,
        DOWN,
        OVER,
        DRAG,
        STOPDRAG,
        WAITING,
        STOPWAITING,
        LEFTARROW,
        RIGHTARROW,
        NONE
    }
}

