using System;
using UnityEngine;

public class SendPMEvent : MonoBehaviour
{
    public string eventName;
    public PlayMakerFSM fsm;

    public void SendEvent()
    {
        this.fsm.SendEvent(this.eventName);
    }
}

