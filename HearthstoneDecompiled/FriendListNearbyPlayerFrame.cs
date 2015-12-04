using System;
using UnityEngine;

public class FriendListNearbyPlayerFrame : MonoBehaviour
{
    public FriendListNearbyPlayerFrameBones m_Bones;
    public FriendListChallengeButton m_ChallengeButton;
    public FriendListNearbyPlayerFrameOffsets m_Offsets;
    protected BnetPlayer m_player;
    public UberText m_PlayerNameText;
    private Component[] m_rightComponentOrder;

    protected virtual void Awake()
    {
        this.m_ChallengeButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnChallengeButtonReleased));
        this.m_rightComponentOrder = new Component[] { this.m_ChallengeButton };
    }

    private float ComputeLeftComponentWidth(Transform bone, Vector3 offset, Component rightComponent)
    {
        Vector3 vector = bone.position + offset;
        Bounds bounds = TransformUtil.ComputeSetPointBounds(rightComponent);
        float num = (bounds.center.x - bounds.extents.x) + this.m_Offsets.m_RightComponent.x;
        return (num - vector.x);
    }

    public BnetPlayer GetNearbyPlayer()
    {
        return this.m_player;
    }

    private void LayoutLeftText(UberText text, Transform bone, Vector3 offset, Component rightComponent)
    {
        if (text.gameObject.activeInHierarchy)
        {
            text.Width = this.ComputeLeftComponentWidth(bone, offset, rightComponent);
            TransformUtil.SetPoint((Component) text, Anchor.LEFT, (Component) bone, Anchor.RIGHT, offset);
        }
    }

    private void OnChallengeButtonReleased(UIEvent e)
    {
        FriendListFriendFrame.OnPlayerChallengeButtonPressed(this.m_ChallengeButton, this.m_player);
    }

    private void OnEnable()
    {
        this.UpdateNearbyPlayer();
    }

    public virtual bool SetNearbyPlayer(BnetPlayer player)
    {
        if (this.m_player == player)
        {
            return false;
        }
        this.m_player = player;
        this.m_ChallengeButton.SetPlayer(player);
        this.UpdateNearbyPlayer();
        return true;
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
    }

    public virtual void UpdateNearbyPlayer()
    {
        if (base.gameObject.activeSelf)
        {
            if (this.m_player == null)
            {
                this.m_PlayerNameText.Text = string.Empty;
            }
            else
            {
                BnetPlayer friend = BnetFriendMgr.Get().FindFriend(this.m_player.GetAccountId());
                if (friend != null)
                {
                    this.m_PlayerNameText.Text = FriendUtils.GetFriendListName(friend, true);
                }
                else
                {
                    this.m_PlayerNameText.Text = FriendUtils.GetFriendListName(this.m_player, true);
                }
            }
            this.m_ChallengeButton.UpdateButton();
            this.UpdateLayout();
        }
    }
}

