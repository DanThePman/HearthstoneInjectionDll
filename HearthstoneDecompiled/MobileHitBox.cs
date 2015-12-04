using System;
using UnityEngine;

public class MobileHitBox : MonoBehaviour
{
    public BoxCollider m_boxCollider;
    private bool m_hasExecuted;
    private PlatformDependentValue<bool> m_isMobile;
    public Vector3 m_offset;
    public bool m_phoneOnly;
    public float m_scaleX = 1f;
    public float m_scaleY = 1f;
    public float m_scaleZ;

    public MobileHitBox()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.Screen) {
            Tablet = true,
            MiniTablet = true,
            Phone = true,
            PC = false
        };
        this.m_isMobile = value2;
    }

    public bool HasExecuted()
    {
        return this.m_hasExecuted;
    }

    private void Start()
    {
        if (((this.m_boxCollider != null) && (this.m_isMobile != null)) && (!this.m_phoneOnly || (UniversalInputManager.UsePhoneUI != null)))
        {
            Vector3 vector = new Vector3 {
                x = this.m_boxCollider.size.x * this.m_scaleX,
                y = this.m_boxCollider.size.y * this.m_scaleY
            };
            if (this.m_scaleZ == 0f)
            {
                vector.z = this.m_boxCollider.size.z * this.m_scaleY;
            }
            else
            {
                vector.z = this.m_boxCollider.size.z * this.m_scaleZ;
            }
            this.m_boxCollider.size = vector;
            this.m_boxCollider.center += this.m_offset;
            this.m_hasExecuted = true;
        }
    }
}

