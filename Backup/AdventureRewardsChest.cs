using System;
using UnityEngine;

[CustomEditClass]
public class AdventureRewardsChest : MonoBehaviour
{
    [CustomEditField(Sections="UI")]
    public GameObject m_CheckmarkContainer;
    [CustomEditField(Sections="UI")]
    public PegUIElement m_ChestClickArea;
    [CustomEditField(Sections="UI")]
    public GameObject m_ChestContainer;
    [CustomEditField(Sections="Event Table")]
    public StateEventTable m_EventTable;
    private const string s_EventBlinkChest = "BlinkChest";
    private const string s_EventBurstCheckmark = "BurstCheckmark";
    private const string s_EventOpenChest = "OpenChest";
    private const string s_EventSlamInCheckmark = "SlamInCheckmark";

    public void AddChestEventListener(UIEventType type, UIEvent.Handler handler)
    {
        this.m_ChestClickArea.AddEventListener(type, handler);
    }

    public void BlinkChest()
    {
        this.ShowCheckmark();
        this.m_EventTable.TriggerState("BlinkChest", true, null);
    }

    public void BurstCheckmark()
    {
        this.ShowCheckmark();
        this.m_EventTable.TriggerState("BurstCheckmark", true, null);
    }

    public void Enable(bool enable)
    {
        if (this.m_ChestClickArea != null)
        {
            this.m_ChestClickArea.gameObject.SetActive(enable);
        }
    }

    public void HideAll()
    {
        this.m_CheckmarkContainer.SetActive(false);
        this.m_ChestContainer.SetActive(false);
    }

    public void RemoveChestEventListener(UIEventType type, UIEvent.Handler handler)
    {
        this.m_ChestClickArea.RemoveEventListener(type, handler);
    }

    public void ShowCheckmark()
    {
        this.m_CheckmarkContainer.SetActive(true);
        this.m_ChestContainer.SetActive(false);
    }

    public void ShowChest()
    {
        this.m_CheckmarkContainer.SetActive(false);
        this.m_ChestContainer.SetActive(true);
    }

    public void SlamInCheckmark()
    {
        this.ShowCheckmark();
        this.m_EventTable.TriggerState("SlamInCheckmark", true, null);
    }
}

