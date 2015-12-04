using System;
using UnityEngine;

[ExecuteInEditMode]
public class LookAtCamera : MonoBehaviour
{
    public Vector3 m_LookAtPositionOffset = Vector3.zero;
    private GameObject m_LookAtTarget;
    private Camera m_MainCamera;
    private readonly Vector3 X_VECTOR = new Vector3(1f, 0f, 0f);
    private readonly Vector3 Y_VECTOR = new Vector3(0f, 1f, 0f);
    private readonly Vector3 Z_VECTOR = new Vector3(0f, 0f, 1f);

    private void Awake()
    {
        this.CreateLookAtTarget();
    }

    private void CreateLookAtTarget()
    {
        this.m_LookAtTarget = new GameObject();
        this.m_LookAtTarget.name = "LookAtCamera Target";
    }

    private void OnDestroy()
    {
        if (this.m_LookAtTarget != null)
        {
            UnityEngine.Object.Destroy(this.m_LookAtTarget);
        }
    }

    private void Start()
    {
        this.m_MainCamera = Camera.main;
    }

    private void Update()
    {
        if (this.m_MainCamera == null)
        {
            this.m_MainCamera = Camera.main;
            if (this.m_MainCamera == null)
            {
                return;
            }
        }
        if (this.m_LookAtTarget == null)
        {
            this.CreateLookAtTarget();
            if (this.m_LookAtTarget == null)
            {
                return;
            }
        }
        this.m_LookAtTarget.transform.position = this.m_MainCamera.transform.position + this.m_LookAtPositionOffset;
        base.transform.LookAt(this.m_LookAtTarget.transform, this.Z_VECTOR);
        base.transform.Rotate(this.X_VECTOR, (float) 90f);
        base.transform.Rotate(this.Y_VECTOR, (float) 180f);
    }
}

