using System;
using UnityEngine;

public class GvG_Promotion : MonoBehaviour
{
    private bool m_AnimExisted;
    private GameObject m_arenaObj;

    private void OnDestroy()
    {
        this.m_arenaObj.transform.localPosition = new Vector3(-0.004992113f, 1.260711f, 0.4331615f);
        this.m_arenaObj.transform.localScale = new Vector3(1f, 1f, 1f);
        this.m_arenaObj.transform.localRotation = new Quaternion(0f, -180f, 0f, 0f);
        if (!this.m_AnimExisted)
        {
            Animation component = this.m_arenaObj.GetComponent<Animation>();
            if (component != null)
            {
                UnityEngine.Object.Destroy(component);
            }
        }
    }

    private void Start()
    {
        this.m_arenaObj = Box.Get().m_ForgeButton.gameObject;
        base.transform.parent = this.m_arenaObj.transform;
        Animation component = this.m_arenaObj.GetComponent<Animation>();
        this.m_AnimExisted = true;
        if (component == null)
        {
            this.m_arenaObj.AddComponent<Animation>();
            this.m_AnimExisted = false;
        }
        base.GetComponent<Spell>().Activate();
    }
}

