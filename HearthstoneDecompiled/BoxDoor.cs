using System;
using System.Collections;
using UnityEngine;

public class BoxDoor : MonoBehaviour
{
    private const float BOX_SLIDE_PERCENTAGE_PHONE = 1.038f;
    private BoxDoorStateInfo m_info;
    private bool m_master;
    private Box m_parent;
    private Vector3 m_startingPosition;
    private State m_state;

    private void Awake()
    {
        this.m_startingPosition = base.gameObject.transform.localPosition;
    }

    public bool ChangeState(State state)
    {
        if (this.m_state == state)
        {
            return false;
        }
        this.m_state = state;
        if (state == State.CLOSED)
        {
            this.m_parent.OnAnimStarted();
            Vector3 vector = this.m_info.m_ClosedRotation - this.m_info.m_OpenedRotation;
            object[] args = new object[] { "amount", vector, "delay", this.m_info.m_ClosedDelaySec, "time", this.m_info.m_ClosedRotateSec, "easeType", this.m_info.m_ClosedRotateEaseType, "space", Space.Self, "oncomplete", "OnAnimFinished", "oncompletetarget", this.m_parent.gameObject };
            Hashtable hashtable = iTween.Hash(args);
            iTween.RotateAdd(base.gameObject, hashtable);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                object[] objArray2 = new object[] { "position", this.m_startingPosition, "isLocal", true, "delay", this.m_info.m_ClosedDelaySec, "time", this.m_info.m_ClosedRotateSec, "easeType", this.m_info.m_ClosedRotateEaseType };
                hashtable = iTween.Hash(objArray2);
                iTween.MoveTo(base.gameObject, hashtable);
            }
            if (this.IsMaster())
            {
                this.m_parent.GetEventSpell(BoxEventType.DOORS_CLOSE).Activate();
                this.m_parent.GetEventSpell(BoxEventType.SHADOW_FADE_IN).ActivateState(SpellStateType.BIRTH);
            }
        }
        else if (state == State.OPENED)
        {
            this.m_parent.OnAnimStarted();
            Vector3 vector2 = this.m_info.m_OpenedRotation - this.m_info.m_ClosedRotation;
            object[] objArray3 = new object[] { "amount", vector2, "delay", this.m_info.m_OpenedDelaySec, "time", this.m_info.m_OpenedRotateSec, "easeType", this.m_info.m_OpenedRotateEaseType, "space", Space.Self, "oncomplete", "OnAnimFinished", "oncompletetarget", this.m_parent.gameObject };
            Hashtable hashtable2 = iTween.Hash(objArray3);
            iTween.RotateAdd(base.gameObject, hashtable2);
            if (UniversalInputManager.UsePhoneUI != null)
            {
                Vector3 startingPosition = this.m_startingPosition;
                startingPosition.x *= 1.038f;
                object[] objArray4 = new object[] { "position", startingPosition, "isLocal", true, "delay", this.m_info.m_ClosedDelaySec, "time", this.m_info.m_ClosedRotateSec, "easeType", this.m_info.m_ClosedRotateEaseType };
                hashtable2 = iTween.Hash(objArray4);
                iTween.MoveTo(base.gameObject, hashtable2);
            }
            if (this.IsMaster())
            {
                this.m_parent.GetEventSpell(BoxEventType.DOORS_OPEN).Activate();
                this.m_parent.GetEventSpell(BoxEventType.SHADOW_FADE_OUT).ActivateState(SpellStateType.BIRTH);
            }
        }
        return true;
    }

    public void EnableMaster(bool enable)
    {
        this.m_master = enable;
    }

    public BoxDoorStateInfo GetInfo()
    {
        return this.m_info;
    }

    public Box GetParent()
    {
        return this.m_parent;
    }

    public bool IsMaster()
    {
        return this.m_master;
    }

    public void SetInfo(BoxDoorStateInfo info)
    {
        this.m_info = info;
    }

    public void SetParent(Box parent)
    {
        this.m_parent = parent;
    }

    public void UpdateState(State state)
    {
        this.m_state = state;
        if (state == State.CLOSED)
        {
            base.transform.localRotation = Quaternion.Euler(this.m_info.m_ClosedRotation);
            this.m_parent.GetEventSpell(BoxEventType.SHADOW_FADE_IN).ActivateState(SpellStateType.ACTION);
        }
        else if (state == State.OPENED)
        {
            base.transform.localRotation = Quaternion.Euler(this.m_info.m_OpenedRotation);
            this.m_parent.GetEventSpell(BoxEventType.SHADOW_FADE_OUT).ActivateState(SpellStateType.ACTION);
        }
    }

    public enum State
    {
        CLOSED,
        OPENED
    }
}

