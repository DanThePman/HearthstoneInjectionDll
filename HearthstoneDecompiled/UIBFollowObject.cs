using System;
using UnityEngine;

[ExecuteInEditMode, CustomEditClass]
public class UIBFollowObject : MonoBehaviour
{
    public GameObject m_objectToFollow;
    public Vector3 m_offset;
    public GameObject m_rootObject;
    public bool m_useWorldOffset;

    public void UpdateFollowPosition()
    {
        if ((this.m_rootObject != null) && (this.m_objectToFollow != null))
        {
            Vector3 position = this.m_objectToFollow.transform.position;
            if (this.m_offset.sqrMagnitude > 0f)
            {
                position += this.m_objectToFollow.transform.localToWorldMatrix * this.m_offset;
            }
            this.m_rootObject.transform.position = position;
        }
    }
}

