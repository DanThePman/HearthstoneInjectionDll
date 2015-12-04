using System;
using UnityEngine;

public class FriendListRecruitFrame : MonoBehaviour
{
    public FriendListRecruitFrameBones m_Bones;
    public FriendListRecruitFrameOffsets m_Offsets;
    public UberText m_PlayerNameText;
    public FriendListRecruitFramePrefabs m_Prefabs;
    private Network.RecruitInfo m_recruitInfo;
    private FriendListRecruitUI m_RecruitUI;
    private Component[] m_rightComponentOrder;
    public UberText m_StatusText;

    protected virtual void Awake()
    {
        this.m_RecruitUI = UnityEngine.Object.Instantiate<FriendListRecruitUI>(this.m_Prefabs.recruitUI);
        this.m_RecruitUI.transform.parent = base.gameObject.transform;
        this.m_RecruitUI.gameObject.SetActive(false);
        this.m_rightComponentOrder = new Component[] { this.m_RecruitUI };
    }

    private float ComputeLeftComponentWidth(Transform bone, Vector3 offset, Component rightComponent)
    {
        Vector3 vector = bone.position + offset;
        Bounds bounds = TransformUtil.ComputeSetPointBounds(rightComponent);
        float num = (bounds.center.x - bounds.extents.x) + this.m_Offsets.m_RightComponent.x;
        return (num - vector.x);
    }

    public Network.RecruitInfo GetRecruitInfo()
    {
        return this.m_recruitInfo;
    }

    private void LayoutLeftText(UberText text, Transform bone, Vector3 offset, Component rightComponent)
    {
        if (text.gameObject.activeInHierarchy)
        {
            text.Width = this.ComputeLeftComponentWidth(bone, offset, rightComponent);
            TransformUtil.SetPoint((Component) text, Anchor.LEFT, (Component) bone, Anchor.RIGHT, offset);
        }
    }

    private void OnEnable()
    {
        this.UpdateRecruit();
    }

    public void SetRecruitInfo(Network.RecruitInfo info)
    {
        this.m_recruitInfo = info;
        this.UpdateRecruit();
    }

    private void UpdateLayout()
    {
        Component rightComponent = this.m_Bones.m_RightComponent;
        for (int i = this.m_rightComponentOrder.Length - 1; i >= 0; i--)
        {
            Component src = this.m_rightComponentOrder[i];
            if (src.gameObject.activeSelf)
            {
                TransformUtil.SetPoint(src, Anchor.RIGHT, rightComponent, Anchor.LEFT, this.m_Offsets.m_RightComponent);
                rightComponent = src;
            }
        }
        this.LayoutLeftText(this.m_PlayerNameText, this.m_Bones.m_PlayerNameText, this.m_Offsets.m_PlayerNameText, rightComponent);
        this.LayoutLeftText(this.m_StatusText, this.m_Bones.m_StatusText, this.m_Offsets.m_StatusText, rightComponent);
    }

    public virtual void UpdateRecruit()
    {
        if ((base.gameObject != null) && base.gameObject.activeSelf)
        {
            this.m_RecruitUI.SetInfo(this.m_recruitInfo);
            if (this.m_recruitInfo != null)
            {
                this.m_PlayerNameText.Text = this.m_recruitInfo.Nickname;
                switch (this.m_recruitInfo.Status)
                {
                    case 1:
                    {
                        string requestElapsedTimeString = FriendUtils.GetRequestElapsedTimeString(this.m_recruitInfo.CreationTimeMicrosec);
                        this.m_StatusText.Text = string.Format("Invintation sent {0}", requestElapsedTimeString);
                        break;
                    }
                    case 2:
                        this.m_StatusText.Text = "Account ineligible!";
                        break;

                    case 3:
                        this.m_StatusText.Text = "Invitation declined!";
                        break;

                    case 4:
                        this.m_StatusText.Text = "Accepted";
                        break;
                }
            }
            this.UpdateLayout();
        }
    }
}

