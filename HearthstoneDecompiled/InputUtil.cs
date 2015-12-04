using System;
using UnityEngine;

public class InputUtil
{
    public static InputScheme GetInputScheme()
    {
        RuntimePlatform platform = Application.platform;
        switch (platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                return InputScheme.TOUCH;
        }
        if ((platform != RuntimePlatform.PS3) && (platform != RuntimePlatform.XBOX360))
        {
            return InputScheme.KEYBOARD_MOUSE;
        }
        return InputScheme.GAMEPAD;
    }

    public static bool IsMouseOnScreen()
    {
        return ((((UniversalInputManager.Get().GetMousePosition().x >= 0f) && (UniversalInputManager.Get().GetMousePosition().x <= Screen.width)) && (UniversalInputManager.Get().GetMousePosition().y >= 0f)) && (UniversalInputManager.Get().GetMousePosition().y <= Screen.height));
    }

    public static bool IsPlayMakerMouseInputAllowed(GameObject go)
    {
        if (UniversalInputManager.Get() == null)
        {
            return false;
        }
        if (ShouldCheckGameplayForPlayMakerMouseInput(go))
        {
            GameState state = GameState.Get();
            if ((state != null) && state.IsMulliganManagerActive())
            {
                return false;
            }
            TargetReticleManager manager = TargetReticleManager.Get();
            if ((manager != null) && manager.IsLocalArrowActive())
            {
                return false;
            }
        }
        return true;
    }

    private static bool ShouldCheckGameplayForPlayMakerMouseInput(GameObject go)
    {
        if (SceneMgr.Get() == null)
        {
            return false;
        }
        if (!SceneMgr.Get().IsInGame())
        {
            return false;
        }
        if (((LoadingScreen.Get() != null) && LoadingScreen.Get().IsPreviousSceneActive()) && (SceneUtils.FindComponentInThisOrParents<LoadingScreen>(go) != null))
        {
            return false;
        }
        if (SceneUtils.FindComponentInThisOrParents<BaseUI>(go) != null)
        {
            return false;
        }
        return true;
    }
}

