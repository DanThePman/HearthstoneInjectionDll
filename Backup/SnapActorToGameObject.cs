using System;
using UnityEngine;

public class SnapActorToGameObject : MonoBehaviour
{
    private Transform m_actorTransform;
    private Vector3 m_OrgPosition;
    private Quaternion m_OrgRotation;
    private Vector3 m_OrgScale;
    public bool m_ResetTransformOnDisable;
    public bool m_SnapPostion = true;
    public bool m_SnapRotation = true;
    public bool m_SnapScale = true;

    private void LateUpdate()
    {
        if (this.m_actorTransform != null)
        {
            if (this.m_SnapPostion)
            {
                this.m_actorTransform.position = base.transform.position;
            }
            if (this.m_SnapRotation)
            {
                this.m_actorTransform.rotation = base.transform.rotation;
            }
            if (this.m_SnapScale)
            {
                TransformUtil.SetWorldScale(this.m_actorTransform, base.transform.lossyScale);
            }
        }
    }

    private void OnDisable()
    {
        if ((this.m_actorTransform != null) && this.m_ResetTransformOnDisable)
        {
            this.m_actorTransform.localPosition = this.m_OrgPosition;
            this.m_actorTransform.localRotation = this.m_OrgRotation;
            this.m_actorTransform.localScale = this.m_OrgScale;
        }
    }

    private void OnEnable()
    {
        if (this.m_actorTransform != null)
        {
            this.m_OrgPosition = this.m_actorTransform.localPosition;
            this.m_OrgRotation = this.m_actorTransform.localRotation;
            this.m_OrgScale = this.m_actorTransform.localScale;
        }
    }

    private void Start()
    {
        Actor actor = SceneUtils.FindComponentInThisOrParents<Actor>(base.gameObject);
        if (actor == null)
        {
            Spell message = SceneUtils.FindComponentInParents<Spell>(base.gameObject);
            if (message != null)
            {
                Debug.Log(message);
                Debug.Log(message.GetSourceCard());
                actor = message.GetSourceCard().GetActor();
            }
        }
        if (actor == null)
        {
            Debug.LogError(string.Format("SnapActorToGameObject on {0} failed to find Actor object!", base.gameObject.name));
            base.enabled = false;
        }
        else
        {
            this.m_actorTransform = actor.transform;
        }
    }
}

