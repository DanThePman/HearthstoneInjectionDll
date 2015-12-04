using HutongGames.PlayMaker;
using System;
using UnityEngine;

[ActionCategory("Pegasus"), HutongGames.PlayMaker.Tooltip("Shake Minions")]
public class ShakeMinionsAction : FsmStateAction
{
    [RequiredField, HutongGames.PlayMaker.Tooltip("Custom Shake Intensity 0-1. Used when Shake Size is Custom")]
    public FsmFloat customShakeIntensity;
    [HutongGames.PlayMaker.Tooltip("Impact Object Location"), RequiredField]
    public FsmOwnerDefault gameObject;
    [HutongGames.PlayMaker.Tooltip("Minions To Shake"), RequiredField]
    public MinionsToShakeEnum MinionsToShake;
    [HutongGames.PlayMaker.Tooltip("Radius - 0 = for all objects"), RequiredField]
    public FsmFloat radius;
    [HutongGames.PlayMaker.Tooltip("Shake Intensity"), RequiredField]
    public ShakeMinionIntensity shakeSize = ShakeMinionIntensity.SmallShake;
    [HutongGames.PlayMaker.Tooltip("Shake Type"), RequiredField]
    public ShakeMinionType shakeType = ShakeMinionType.RandomDirection;

    private void DoShakeMinions()
    {
        GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
        if (ownerDefaultTarget == null)
        {
            base.Finish();
        }
        else if (this.MinionsToShake == MinionsToShakeEnum.All)
        {
            MinionShake.ShakeAllMinions(ownerDefaultTarget, this.shakeType, ownerDefaultTarget.transform.position, this.shakeSize, this.customShakeIntensity.Value, this.radius.Value, 0f);
        }
        else if (this.MinionsToShake == MinionsToShakeEnum.Target)
        {
            MinionShake.ShakeTargetMinion(ownerDefaultTarget, this.shakeType, ownerDefaultTarget.transform.position, this.shakeSize, this.customShakeIntensity.Value, 0f, 0f);
        }
        else if (this.MinionsToShake == MinionsToShakeEnum.SelectedGameObject)
        {
            MinionShake.ShakeObject(ownerDefaultTarget, this.shakeType, ownerDefaultTarget.transform.position, this.shakeSize, this.customShakeIntensity.Value, 0f, 0f);
        }
    }

    public override void OnEnter()
    {
        this.DoShakeMinions();
        base.Finish();
    }

    public override void Reset()
    {
        this.gameObject = null;
        this.MinionsToShake = MinionsToShakeEnum.All;
        this.shakeType = ShakeMinionType.RandomDirection;
        this.shakeSize = ShakeMinionIntensity.SmallShake;
        this.customShakeIntensity = 0.1f;
        this.radius = 0f;
    }

    public enum MinionsToShakeEnum
    {
        All,
        Target,
        SelectedGameObject
    }
}

