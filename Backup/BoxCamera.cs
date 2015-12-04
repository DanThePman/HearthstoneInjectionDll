using System;
using System.Collections;
using UnityEngine;

public class BoxCamera : MonoBehaviour
{
    private bool m_applyAccelerometer;
    private Vector3 m_basePosition;
    private Vector2 m_currentAngle;
    private bool m_disableAccelerometer = true;
    public BoxCameraEventTable m_EventTable;
    private Vector2 m_gyroRotation;
    public GameObject m_IgnoreFullscreenEffectsCamera;
    private BoxCameraStateInfo m_info;
    private Vector3 m_lookAtPoint;
    private float m_offset;
    private Box m_parent;
    private State m_state;
    public GameObject m_TooltipCamera;
    private float MAX_GYRO_RANGE = 2.1f;
    private float ROTATION_SCALE = 0.085f;

    public bool ChangeState(State state)
    {
        if (this.m_state == state)
        {
            return false;
        }
        this.m_state = state;
        Vector3 cameraPosition = this.GetCameraPosition(state);
        this.m_parent.OnAnimStarted();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_applyAccelerometer = false;
            this.m_basePosition = base.transform.parent.InverseTransformPoint(cameraPosition);
            this.m_lookAtPoint = base.transform.parent.InverseTransformPoint(new Vector3(cameraPosition.x, 1.5f, cameraPosition.z));
            if (cameraPosition == base.gameObject.transform.position)
            {
                this.OnAnimFinished();
                return true;
            }
        }
        Hashtable args = null;
        if (state == State.CLOSED)
        {
            object[] objArray1 = new object[] { "position", cameraPosition, "delay", this.m_info.m_ClosedDelaySec, "time", this.m_info.m_ClosedMoveSec, "easeType", this.m_info.m_ClosedMoveEaseType, "oncomplete", "OnAnimFinished", "oncompletetarget", base.gameObject };
            args = iTween.Hash(objArray1);
        }
        else if (state == State.CLOSED_WITH_DRAWER)
        {
            object[] objArray2 = new object[] { "position", cameraPosition, "delay", this.m_info.m_ClosedWithDrawerDelaySec, "time", this.m_info.m_ClosedWithDrawerMoveSec, "easeType", this.m_info.m_ClosedWithDrawerMoveEaseType, "oncomplete", "OnAnimFinished", "oncompletetarget", base.gameObject };
            args = iTween.Hash(objArray2);
        }
        else if (state == State.OPENED)
        {
            object[] objArray3 = new object[] { "position", cameraPosition, "delay", this.m_info.m_OpenedDelaySec, "time", this.m_info.m_OpenedMoveSec, "easeType", this.m_info.m_OpenedMoveEaseType, "oncomplete", "OnAnimFinished", "oncompletetarget", base.gameObject };
            args = iTween.Hash(objArray3);
        }
        iTween.MoveTo(base.gameObject, args);
        return true;
    }

    public void EnableAccelerometer()
    {
        if (MobileCallbackManager.AreMotionEffectsEnabled())
        {
            this.m_disableAccelerometer = false;
        }
    }

    public Vector3 GetCameraPosition(State state)
    {
        Transform transform;
        Transform transform2;
        if (state == State.CLOSED)
        {
            transform = this.m_info.m_ClosedMinAspectRatioBone.transform;
            transform2 = this.m_info.m_ClosedBone.transform;
        }
        else if (state == State.CLOSED_WITH_DRAWER)
        {
            transform = this.m_info.m_ClosedWithDrawerMinAspectRatioBone.transform;
            transform2 = this.m_info.m_ClosedWithDrawerBone.transform;
        }
        else
        {
            transform = this.m_info.m_OpenedMinAspectRatioBone.transform;
            transform2 = this.m_info.m_OpenedBone.transform;
        }
        if (UniversalInputManager.UsePhoneUI == null)
        {
            return transform2.position;
        }
        return TransformUtil.GetAspectRatioDependentPosition(transform.position, transform2.position);
    }

    public BoxCameraEventTable GetEventTable()
    {
        return this.m_EventTable;
    }

    public BoxCameraStateInfo GetInfo()
    {
        return this.m_info;
    }

    public Box GetParent()
    {
        return this.m_parent;
    }

    public State GetState()
    {
        return this.m_state;
    }

    public void OnAnimFinished()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_applyAccelerometer = this.m_state != State.OPENED;
            this.m_currentAngle = new Vector2(0f, 0f);
        }
        this.m_parent.OnAnimFinished();
    }

    public void SetInfo(BoxCameraStateInfo info)
    {
        this.m_info = info;
    }

    public void SetParent(Box parent)
    {
        this.m_parent = parent;
    }

    public void Update()
    {
        if ((!this.m_disableAccelerometer && (base.transform.parent.gameObject.GetComponent<LoadingScreen>() == null)) && (UniversalInputManager.UsePhoneUI != null))
        {
            if (this.m_applyAccelerometer)
            {
                this.m_gyroRotation.x = Input.gyro.rotationRateUnbiased.x;
                this.m_gyroRotation.y = -Input.gyro.rotationRateUnbiased.y;
                this.m_currentAngle.x += this.m_gyroRotation.y * this.ROTATION_SCALE;
                this.m_currentAngle.y += this.m_gyroRotation.x * this.ROTATION_SCALE;
                this.m_currentAngle.x = Mathf.Clamp(this.m_currentAngle.x, -this.MAX_GYRO_RANGE, this.MAX_GYRO_RANGE);
                this.m_currentAngle.y = Mathf.Clamp(this.m_currentAngle.y, -this.MAX_GYRO_RANGE, this.MAX_GYRO_RANGE);
                base.gameObject.transform.localPosition = new Vector3(this.m_basePosition.x, this.m_basePosition.y, this.m_basePosition.z + this.m_currentAngle.y);
            }
            Vector3 worldUp = new Vector3(0f, 0f, 1f);
            Vector3 worldPosition = base.gameObject.transform.parent.TransformPoint(this.m_lookAtPoint);
            base.gameObject.transform.LookAt(worldPosition, worldUp);
            if (this.m_applyAccelerometer)
            {
                this.m_IgnoreFullscreenEffectsCamera.transform.position = base.gameObject.transform.parent.TransformPoint(this.m_basePosition);
                this.m_IgnoreFullscreenEffectsCamera.transform.LookAt(worldPosition, worldUp);
                this.m_TooltipCamera.transform.position = base.gameObject.transform.parent.TransformPoint(this.m_basePosition);
                this.m_TooltipCamera.transform.LookAt(worldPosition, worldUp);
            }
            else
            {
                TransformUtil.Identity(this.m_TooltipCamera);
                TransformUtil.Identity(this.m_IgnoreFullscreenEffectsCamera);
            }
        }
    }

    public void UpdateState(State state)
    {
        this.m_state = state;
        base.transform.position = this.GetCameraPosition(state);
    }

    public enum State
    {
        CLOSED,
        CLOSED_WITH_DRAWER,
        OPENED
    }
}

