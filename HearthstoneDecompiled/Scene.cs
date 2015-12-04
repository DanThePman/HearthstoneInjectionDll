using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Scene : MonoBehaviour
{
    protected virtual void Awake()
    {
        SceneMgr.Get().SetScene(this);
    }

    public virtual bool HandleKeyboardInput()
    {
        if ((BackButton.backKey != KeyCode.None) && Input.GetKeyUp(BackButton.backKey))
        {
            if (DialogManager.Get().ShowingDialog())
            {
                DialogManager.Get().GoBack();
                return true;
            }
            if (ChatMgr.Get().IsFriendListShowing() || ChatMgr.Get().IsChatLogFrameShown())
            {
                ChatMgr.Get().GoBack();
                return true;
            }
            if ((OptionsMenu.Get() != null) && OptionsMenu.Get().IsShown())
            {
                OptionsMenu.Get().Hide(true);
                return true;
            }
            if ((GameMenu.Get() != null) && GameMenu.Get().IsShown())
            {
                GameMenu.Get().Hide();
                return true;
            }
            if (Navigation.GoBack())
            {
                return true;
            }
        }
        return false;
    }

    public virtual bool IsUnloading()
    {
        return false;
    }

    public virtual void PreUnload()
    {
    }

    public virtual void Unload()
    {
    }

    public delegate void BackButtonPressedDelegate();
}

