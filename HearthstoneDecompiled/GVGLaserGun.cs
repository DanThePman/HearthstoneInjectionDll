using System;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class GVGLaserGun : MonoBehaviour
{
    private float m_angle;
    [CustomEditField(Sections="Rotation", ListTable=true)]
    public List<AngleDef> m_AngleDefs = new List<AngleDef>();
    private int m_angleIndex = -1;
    [CustomEditField(Sections="Debug")]
    public bool m_DebugShowGunAngle;
    [CustomEditField(Sections="Lever")]
    public GameObject m_GunLever;
    [Tooltip("The thing that will be rotated."), CustomEditField(Sections="Rotation")]
    public GameObject m_GunRotator;
    private bool m_leverEffectsActive;
    private int m_maxAngleIndex = -1;
    private int m_minAngleIndex = -1;
    [CustomEditField(Sections="Lever")]
    public Spell m_PullLeverSpell;
    private int m_requestedRotationDir;
    [CustomEditField(Sections="Rotation")]
    public GameObject m_RotateLeftButton;
    [CustomEditField(Sections="Rotation")]
    public GameObject m_RotateRightButton;
    private int m_rotationDir;
    [Tooltip("How fast the gun rotates in degrees per second."), CustomEditField(Sections="Rotation")]
    public float m_RotationSpeed;
    private List<int> m_sortedAngleDefIndexes = new List<int>();
    [CustomEditField(Sections="Rotation")]
    public Spell m_StartRotationSpell;
    [CustomEditField(Sections="Rotation")]
    public Spell m_StopRotationSpell;

    private static int AngleDefSortComparison(AngleDef def1, AngleDef def2)
    {
        if (def1.m_Angle < def2.m_Angle)
        {
            return -1;
        }
        if (def1.m_Angle > def2.m_Angle)
        {
            return 1;
        }
        return 0;
    }

    private void Awake()
    {
        if (this.m_AngleDefs.Count != 0)
        {
            for (int i = 0; i < this.m_AngleDefs.Count; i++)
            {
                this.m_sortedAngleDefIndexes.Add(i);
            }
            this.m_sortedAngleDefIndexes.Sort(delegate (int index1, int index2) {
                AngleDef def = this.m_AngleDefs[index1];
                AngleDef def2 = this.m_AngleDefs[index2];
                return AngleDefSortComparison(def, def2);
            });
            this.m_angleIndex = 0;
            this.m_minAngleIndex = 0;
            this.m_maxAngleIndex = 0;
            float angle = this.m_AngleDefs[0].m_Angle;
            float num3 = angle;
            for (int j = 0; j < this.m_sortedAngleDefIndexes.Count; j++)
            {
                AngleDef def = this.m_AngleDefs[this.m_sortedAngleDefIndexes[j]];
                if (def.m_Angle < angle)
                {
                    angle = def.m_Angle;
                    this.m_minAngleIndex = j;
                }
                if (def.m_Angle > num3)
                {
                    num3 = def.m_Angle;
                    this.m_maxAngleIndex = j;
                }
                if (def.m_Default)
                {
                    this.m_angleIndex = j;
                    this.SetAngle(def.m_Angle);
                }
            }
        }
    }

    private AngleDef GetAngleDef()
    {
        if (this.m_angleIndex < 0)
        {
            return null;
        }
        if (this.m_angleIndex >= this.m_sortedAngleDefIndexes.Count)
        {
            return null;
        }
        int num = this.m_sortedAngleDefIndexes[this.m_angleIndex];
        return this.m_AngleDefs[num];
    }

    private void HandleLever()
    {
        if (((this.m_rotationDir == 0) && !this.m_leverEffectsActive) && (UniversalInputManager.Get().GetMouseButtonUp(0) && this.IsOver(this.m_GunLever)))
        {
            this.PullLever();
        }
    }

    private void HandleRotation()
    {
        if (!this.m_leverEffectsActive)
        {
            this.m_requestedRotationDir = 0;
            if (UniversalInputManager.Get().GetMouseButton(0))
            {
                if (this.IsOver(this.m_RotateLeftButton))
                {
                    this.m_requestedRotationDir = -1;
                }
                else if (this.IsOver(this.m_RotateRightButton))
                {
                    this.m_requestedRotationDir = 1;
                }
            }
            if (this.ShouldStartRotating())
            {
                this.StartRotating(this.m_requestedRotationDir);
            }
            if (this.m_rotationDir < 0)
            {
                this.RotateLeft();
            }
            else if (this.m_rotationDir > 0)
            {
                this.RotateRight();
            }
        }
    }

    private bool IsOver(GameObject go)
    {
        if (go == null)
        {
            return false;
        }
        if (!InputUtil.IsPlayMakerMouseInputAllowed(go))
        {
            return false;
        }
        if (!UniversalInputManager.Get().InputIsOver(go))
        {
            return false;
        }
        return true;
    }

    private void OnImpactSpellFinished(Spell spell, object userData)
    {
        this.m_leverEffectsActive = false;
    }

    private void OnPullLeverSpellFinished(Spell spell, object userData)
    {
        Spell impactSpell = this.GetAngleDef().m_ImpactSpell;
        if (impactSpell == null)
        {
            this.m_leverEffectsActive = false;
        }
        else
        {
            impactSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnImpactSpellFinished));
            impactSpell.Activate();
        }
    }

    private void PullLever()
    {
        if (this.m_PullLeverSpell != null)
        {
            this.m_leverEffectsActive = true;
            this.m_PullLeverSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnPullLeverSpellFinished));
            this.m_PullLeverSpell.Activate();
        }
    }

    private void RotateLeft()
    {
        AngleDef angleDef = this.GetAngleDef();
        float angle = Mathf.MoveTowards(this.m_angle, angleDef.m_Angle, this.m_RotationSpeed * UnityEngine.Time.deltaTime);
        if (angle <= angleDef.m_Angle)
        {
            if ((this.m_requestedRotationDir == 0) || (this.m_angleIndex == this.m_minAngleIndex))
            {
                this.SetAngle(angle);
                this.StopRotating();
            }
            else
            {
                this.m_angleIndex--;
            }
        }
        else
        {
            this.SetAngle(angle);
        }
    }

    private void RotateRight()
    {
        AngleDef angleDef = this.GetAngleDef();
        float angle = Mathf.MoveTowards(this.m_angle, angleDef.m_Angle, this.m_RotationSpeed * UnityEngine.Time.deltaTime);
        if (angle >= angleDef.m_Angle)
        {
            if ((this.m_requestedRotationDir == 0) || (this.m_angleIndex == this.m_maxAngleIndex))
            {
                this.SetAngle(angle);
                this.StopRotating();
            }
            else
            {
                this.m_angleIndex++;
            }
        }
        else
        {
            this.SetAngle(angle);
        }
    }

    private void SetAngle(float angle)
    {
        this.m_angle = angle;
        TransformUtil.SetLocalEulerAngleY(this.m_GunRotator, this.m_angle);
    }

    private bool ShouldStartRotating()
    {
        if (this.m_requestedRotationDir == 0)
        {
            return false;
        }
        if (this.m_requestedRotationDir == this.m_rotationDir)
        {
            return false;
        }
        if ((this.m_requestedRotationDir < 0) && (this.m_angleIndex == this.m_minAngleIndex))
        {
            return false;
        }
        if ((this.m_requestedRotationDir > 0) && (this.m_angleIndex == this.m_maxAngleIndex))
        {
            return false;
        }
        return true;
    }

    private void StartRotating(int dir)
    {
        this.m_rotationDir = dir;
        if (dir < 0)
        {
            this.m_angleIndex--;
        }
        else
        {
            this.m_angleIndex++;
        }
        if (this.m_StartRotationSpell != null)
        {
            this.m_StartRotationSpell.Activate();
        }
    }

    private void StopRotating()
    {
        this.m_rotationDir = 0;
        if (this.m_StopRotationSpell != null)
        {
            this.m_StopRotationSpell.Activate();
        }
    }

    private void Update()
    {
        this.HandleRotation();
        this.HandleLever();
    }

    [Serializable]
    public class AngleDef
    {
        [CustomEditField(ListSortable=true)]
        public float m_Angle;
        public bool m_Default;
        public Spell m_ImpactSpell;

        public int CustomBehaviorListCompare(GVGLaserGun.AngleDef def1, GVGLaserGun.AngleDef def2)
        {
            return GVGLaserGun.AngleDefSortComparison(def1, def2);
        }
    }
}

