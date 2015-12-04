using System;
using UnityEngine;

public class ShownUIMgr : MonoBehaviour
{
    private UI_WINDOW m_shownUI;
    private static ShownUIMgr s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public void ClearShownUI()
    {
        this.m_shownUI = UI_WINDOW.NONE;
    }

    public static ShownUIMgr Get()
    {
        return s_instance;
    }

    public UI_WINDOW GetShownUI()
    {
        return this.m_shownUI;
    }

    public bool HasShownUI()
    {
        return (this.m_shownUI != UI_WINDOW.NONE);
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void SetShownUI(UI_WINDOW uiWindow)
    {
        this.m_shownUI = uiWindow;
    }

    public enum UI_WINDOW
    {
        NONE,
        GENERAL_STORE,
        QUEST_LOG
    }
}

