using HutongGames.PlayMaker;
using System;
using UnityEngine;

[ActionCategory("Pegasus"), HutongGames.PlayMaker.Tooltip("Get the object being rendered to from RenderToTexture")]
public class GetRenderToTextureRenderObject : FsmStateAction
{
    [CheckForComponent(typeof(RenderToTexture)), RequiredField]
    public FsmOwnerDefault gameObject;
    [RequiredField, UIHint(UIHint.Variable)]
    public FsmGameObject renderObject;

    private void DoGetObject()
    {
        GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
        if (ownerDefaultTarget != null)
        {
            RenderToTexture component = ownerDefaultTarget.GetComponent<RenderToTexture>();
            if (component == null)
            {
                this.LogError("Missing RenderToTexture component!");
            }
            else
            {
                this.renderObject.Value = component.GetRenderToObject();
            }
        }
    }

    public override void OnEnter()
    {
        this.DoGetObject();
        base.Finish();
    }

    [HutongGames.PlayMaker.Tooltip("Get the object being rendered to from RenderToTexture. This is used to get the procedurally generated render plane object.")]
    public override void Reset()
    {
        this.gameObject = null;
        this.renderObject = null;
    }
}

