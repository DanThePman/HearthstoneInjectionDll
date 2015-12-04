using System;
using UnityEngine;

public class ChatLogFrame : MonoBehaviour
{
    public ChatLogFrameBones m_Bones;
    public ChatLog m_chatLog;
    public UberText m_NameText;
    private PlayerIcon m_playerIcon;
    public ChatLogFramePrefabs m_Prefabs;
    private BnetPlayer m_receiver;

    private void Awake()
    {
        this.InitPlayerIcon();
    }

    private void InitPlayerIcon()
    {
        this.m_playerIcon = UnityEngine.Object.Instantiate<PlayerIcon>(this.m_Prefabs.m_PlayerIcon);
        this.m_playerIcon.transform.parent = base.transform;
        TransformUtil.CopyWorld((Component) this.m_playerIcon, (Component) this.m_Bones.m_PlayerIcon);
        SceneUtils.SetLayer(this.m_playerIcon, base.gameObject.layer);
    }

    private void OnChallengeButtonReleased(UIEvent e)
    {
        FriendChallengeMgr.Get().SendChallenge(this.m_receiver);
    }

    private void OnResize()
    {
        float viewWindowMaxValue = this.m_chatLog.messageFrames.ViewWindowMaxValue;
        this.m_chatLog.messageFrames.transform.position = (Vector3) ((this.m_Bones.m_MessagesTopLeft.position + this.m_Bones.m_MessagesBottomRight.position) / 2f);
        Vector3 vector = this.m_Bones.m_MessagesBottomRight.localPosition - this.m_Bones.m_MessagesTopLeft.localPosition;
        this.m_chatLog.messageFrames.ClipSize = new Vector2(vector.x, Math.Abs(vector.y));
        this.m_chatLog.messageFrames.ViewWindowMaxValue = viewWindowMaxValue;
        this.m_chatLog.messageFrames.ScrollValue = Mathf.Clamp01(this.m_chatLog.messageFrames.ScrollValue);
        this.m_chatLog.OnResize();
    }

    private void Start()
    {
        this.UpdateLayout();
    }

    public void UpdateLayout()
    {
        this.OnResize();
    }

    private void UpdateReceiver()
    {
        this.m_playerIcon.UpdateIcon();
        this.m_NameText.Text = FriendUtils.GetUniqueNameWithColor(this.m_receiver);
    }

    public BnetPlayer Receiver
    {
        get
        {
            return this.m_receiver;
        }
        set
        {
            if (this.m_receiver != value)
            {
                this.m_receiver = value;
                if (this.m_receiver != null)
                {
                    this.m_playerIcon.SetPlayer(this.m_receiver);
                    this.UpdateReceiver();
                    this.m_chatLog.Receiver = this.m_receiver;
                }
            }
        }
    }
}

