using System;
using UnityEngine;

public class ChatBubbleFrame : MonoBehaviour
{
    public UberText m_MessageText;
    public GameObject m_MyDecoration;
    public UberText m_NameText;
    public Vector3_MobileOverride m_ScaleOverride;
    public GameObject m_TheirDecoration;
    public GameObject m_VisualRoot;
    private BnetWhisper m_whisper;

    private void Awake()
    {
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
    }

    public bool DoesMessageFit()
    {
        return !this.m_MessageText.IsEllipsized();
    }

    public BnetWhisper GetWhisper()
    {
        return this.m_whisper;
    }

    private void OnDestroy()
    {
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        BnetPlayerChange change = changelist.FindChange(this.m_whisper.GetTheirGameAccountId());
        if (change != null)
        {
            BnetPlayer oldPlayer = change.GetOldPlayer();
            BnetPlayer newPlayer = change.GetNewPlayer();
            if ((oldPlayer == null) || (oldPlayer.IsOnline() != newPlayer.IsOnline()))
            {
                this.UpdateWhisper();
            }
        }
    }

    public void SetWhisper(BnetWhisper whisper)
    {
        if (this.m_whisper != whisper)
        {
            this.m_whisper = whisper;
            this.UpdateWhisper();
        }
    }

    private void UpdateWhisper()
    {
        if (this.m_whisper != null)
        {
            if (this.m_whisper.GetSpeakerId() == BnetPresenceMgr.Get().GetMyGameAccountId())
            {
                this.m_MyDecoration.SetActive(true);
                this.m_TheirDecoration.SetActive(false);
                BnetPlayer receiver = this.m_whisper.GetReceiver();
                object[] args = new object[] { receiver.GetBestName() };
                this.m_NameText.Text = GameStrings.Format("GLOBAL_CHAT_BUBBLE_RECEIVER_NAME", args);
            }
            else
            {
                this.m_MyDecoration.SetActive(false);
                this.m_TheirDecoration.SetActive(true);
                BnetPlayer speaker = this.m_whisper.GetSpeaker();
                if (speaker.IsOnline())
                {
                    this.m_NameText.TextColor = GameColors.PLAYER_NAME_ONLINE;
                }
                else
                {
                    this.m_NameText.TextColor = GameColors.PLAYER_NAME_OFFLINE;
                }
                this.m_NameText.Text = speaker.GetBestName();
            }
            this.m_MessageText.Text = ChatUtils.GetMessage(this.m_whisper);
        }
    }
}

