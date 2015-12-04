using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FriendListChatIcon : MonoBehaviour
{
    private BnetPlayer m_player;

    public BnetPlayer GetPlayer()
    {
        return this.m_player;
    }

    public bool SetPlayer(BnetPlayer player)
    {
        if (this.m_player == player)
        {
            return false;
        }
        this.m_player = player;
        this.UpdateIcon();
        return true;
    }

    public void UpdateIcon()
    {
        if (this.m_player == null)
        {
            base.gameObject.SetActive(false);
        }
        else
        {
            List<BnetWhisper> whispersWithPlayer = BnetWhisperMgr.Get().GetWhispersWithPlayer(this.m_player);
            if (whispersWithPlayer == null)
            {
                base.gameObject.SetActive(false);
            }
            else if (whispersWithPlayer[whispersWithPlayer.Count - 1].IsSpeaker(BnetPresenceMgr.Get().GetMyPlayer()))
            {
                base.gameObject.SetActive(false);
            }
            else
            {
                PlayerChatInfo playerChatInfo = ChatMgr.Get().GetPlayerChatInfo(this.m_player);
                if ((playerChatInfo != null) && (Enumerable.LastOrDefault<BnetWhisper>(whispersWithPlayer, (Func<BnetWhisper, bool>) (whisper => whisper.IsSpeaker(this.m_player))) == playerChatInfo.GetLastSeenWhisper()))
                {
                    base.gameObject.SetActive(false);
                }
                else
                {
                    base.gameObject.SetActive(true);
                }
            }
        }
    }
}

