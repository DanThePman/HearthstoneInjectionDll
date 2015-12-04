using System;
using System.Collections.Generic;
using UnityEngine;

public class DraftInputManager : MonoBehaviour
{
    private static DraftInputManager s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public static DraftInputManager Get()
    {
        return s_instance;
    }

    public bool HandleKeyboardInput()
    {
        if (DraftDisplay.Get() == null)
        {
            return false;
        }
        if (Input.GetKeyUp(KeyCode.Escape) && DraftDisplay.Get().IsInHeroSelectMode())
        {
            DraftDisplay.Get().DoHeroCancelAnimation();
            return true;
        }
        if (!ApplicationMgr.IsInternal())
        {
            return false;
        }
        List<DraftCardVisual> cardVisuals = DraftDisplay.Get().GetCardVisuals();
        if (cardVisuals == null)
        {
            return false;
        }
        if (cardVisuals.Count == 0)
        {
            return false;
        }
        int num = -1;
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            num = 0;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            num = 1;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            num = 2;
        }
        if (num == -1)
        {
            return false;
        }
        if (cardVisuals.Count < (num + 1))
        {
            return false;
        }
        DraftCardVisual visual = cardVisuals[num];
        if (visual == null)
        {
            return false;
        }
        visual.ChooseThisCard();
        return true;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void Unload()
    {
    }
}

