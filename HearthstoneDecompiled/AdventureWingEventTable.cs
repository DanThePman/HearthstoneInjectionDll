using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[CustomEditClass]
public class AdventureWingEventTable : StateEventTable
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map0;
    private const string s_EventBigChestCover = "BigChestCover";
    private const string s_EventBigChestOpen = "BigChestOpen";
    private const string s_EventBigChestShow = "BigChestShow";
    private const string s_EventBigChestStayOpen = "BigChestStayOpen";
    private const string s_EventPlateActivate = "PlateActivate";
    private const string s_EventPlateBuy = "PlateBuy";
    private const string s_EventPlateCoverPreviewChest = "PlateCoverPreviewChest";
    private const string s_EventPlateDeactivate = "PlateDeactivate";
    private const string s_EventPlateInitialBuy = "PlateInitialBuy";
    private const string s_EventPlateInitialKey = "PlateInitialKey";
    private const string s_EventPlateInitialText = "PlateInitialText";
    private const string s_EventPlateKey = "PlateKey";
    private const string s_EventPlateOpen = "PlateOpen";

    public void AddOpenChestEndEventListener(StateEventTable.StateEventTrigger dlg, bool once = false)
    {
        base.AddStateEventEndListener("BigChestOpen", dlg, once);
    }

    public void AddOpenChestStartEventListener(StateEventTable.StateEventTrigger dlg, bool once = false)
    {
        base.AddStateEventStartListener("BigChestOpen", dlg, once);
    }

    public void AddOpenPlateEndEventListener(StateEventTable.StateEventTrigger dlg, bool once = false)
    {
        base.AddStateEventEndListener("PlateOpen", dlg, once);
    }

    public void AddOpenPlateStartEventListener(StateEventTable.StateEventTrigger dlg, bool once = false)
    {
        base.AddStateEventStartListener("PlateOpen", dlg, once);
    }

    public void BigChestCover()
    {
        base.TriggerState("BigChestCover", true, null);
    }

    public void BigChestOpen()
    {
        base.TriggerState("BigChestOpen", true, null);
    }

    public void BigChestShow()
    {
        base.TriggerState("BigChestShow", true, null);
    }

    public void BigChestStayOpen()
    {
        base.TriggerState("BigChestStayOpen", true, null);
    }

    public bool IsPlateBuy()
    {
        string lastState = base.GetLastState();
        return ((lastState == "PlateBuy") || (lastState == "PlateInitialBuy"));
    }

    public bool IsPlateInitialText()
    {
        return (base.GetLastState() == "PlateInitialText");
    }

    public bool IsPlateInOrGoingToAnActiveState()
    {
        string lastState = base.GetLastState();
        if (lastState != null)
        {
            int num;
            if (<>f__switch$map0 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(6);
                dictionary.Add("PlateActivate", 0);
                dictionary.Add("PlateInitialText", 0);
                dictionary.Add("PlateBuy", 0);
                dictionary.Add("PlateInitialBuy", 0);
                dictionary.Add("PlateKey", 0);
                dictionary.Add("PlateInitialKey", 0);
                <>f__switch$map0 = dictionary;
            }
            if (<>f__switch$map0.TryGetValue(lastState, out num) && (num == 0))
            {
                return true;
            }
        }
        return false;
    }

    public void PlateActivate()
    {
        base.TriggerState("PlateActivate", true, null);
    }

    public void PlateBuy(bool initial = false)
    {
        if (!this.IsPlateBuy())
        {
            base.TriggerState(!initial ? "PlateBuy" : "PlateInitialBuy", true, null);
        }
    }

    public void PlateCoverPreviewChest()
    {
        base.TriggerState("PlateCoverPreviewChest", false, null);
    }

    public void PlateDeactivate()
    {
        base.TriggerState("PlateDeactivate", true, null);
    }

    public void PlateInitialText()
    {
        base.TriggerState("PlateInitialText", true, null);
    }

    public void PlateKey(bool initial = false)
    {
        if (!initial && !this.IsPlateBuy())
        {
            base.TriggerState("PlateBuy", true, null);
        }
        base.TriggerState(!initial ? "PlateKey" : "PlateInitialKey", true, null);
    }

    public void PlateOpen(float delay = 0f)
    {
        base.SetFloatVar("PlateOpen", "PostAnimationDelay", delay);
        base.TriggerState("PlateOpen", true, null);
    }

    public void RemoveOpenChestEndEventListener(StateEventTable.StateEventTrigger dlg)
    {
        base.RemoveStateEventEndListener("BigChestOpen", dlg);
    }

    public void RemoveOpenChestStartEventListener(StateEventTable.StateEventTrigger dlg)
    {
        base.RemoveStateEventStartListener("BigChestOpen", dlg);
    }

    public void RemoveOpenPlateEndEventListener(StateEventTable.StateEventTrigger dlg)
    {
        base.RemoveStateEventEndListener("PlateOpen", dlg);
    }

    public void RemoveOpenPlateStartEventListener(StateEventTable.StateEventTrigger dlg)
    {
        base.RemoveStateEventStartListener("PlateOpen", dlg);
    }
}

