using System;
using UnityEngine;

public class FriendListBaseFriendFrame : MonoBehaviour
{
    public FriendListChatIcon m_ChatIcon;
    protected MedalInfoTranslator m_medal;
    protected BnetPlayer m_player;
    public PlayerIcon m_PlayerIcon;
    public UberText m_PlayerNameText;
    public FriendListFrameBasePrefabs m_Prefabs;
    public TournamentMedal m_rankMedal;
    public TournamentMedal m_rankMedalPrefab;
    public Spawner m_rankMedalSpawner;
    protected Network.RecruitInfo m_recruitInfo;
    protected FriendListRecruitUI m_RecruitUI;
    public UberText m_StatusText;
    private static readonly Color TEXT_COLOR_AWAY = Color.yellow;
    private static readonly Color TEXT_COLOR_BUSY = Color.red;
    private static readonly Color TEXT_COLOR_NORMAL = Color.white;
    private static readonly Color TEXT_COLOR_OFFLINE = Color.grey;

    protected virtual void Awake()
    {
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        BnetWhisperMgr.Get().AddWhisperListener(new BnetWhisperMgr.WhisperCallback(this.OnWhisper));
        ChatMgr.Get().AddPlayerChatInfoChangedListener(new ChatMgr.PlayerChatInfoChangedCallback(this.OnPlayerChatInfoChanged));
        RecruitListMgr.Get().AddRecruitsChangedListener(new RecruitListMgr.RecruitsChangedCallback(this.OnRecruitsChanged));
        this.m_RecruitUI = UnityEngine.Object.Instantiate<FriendListRecruitUI>(this.m_Prefabs.recruitUI);
        this.m_RecruitUI.transform.parent = base.gameObject.transform;
        this.m_RecruitUI.gameObject.SetActive(false);
        if (this.m_rankMedalSpawner == null)
        {
            this.m_rankMedal = UnityEngine.Object.Instantiate<TournamentMedal>(this.m_rankMedalPrefab);
            this.m_rankMedal.transform.parent = base.transform;
            this.m_rankMedal.transform.localScale = new Vector3(20f, 1f, 20f);
            this.m_rankMedal.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
        }
        else
        {
            this.m_rankMedal = this.m_rankMedalSpawner.Spawn<TournamentMedal>();
        }
        this.m_rankMedal.RemoveEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.m_rankMedal.MedalOver));
        this.m_rankMedal.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.RankMedalOver));
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_rankMedal.GetComponent<Collider>().enabled = false;
        }
        this.m_rankMedal.gameObject.SetActive(false);
        SceneUtils.SetLayer(this.m_rankMedal, GameLayer.BattleNetFriendList);
    }

    public BnetPlayer GetFriend()
    {
        return this.m_player;
    }

    public Network.RecruitInfo GetRecruitInfo()
    {
        return this.m_recruitInfo;
    }

    protected virtual void OnDestroy()
    {
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        BnetWhisperMgr.Get().RemoveWhisperListener(new BnetWhisperMgr.WhisperCallback(this.OnWhisper));
        RecruitListMgr.Get().RemoveRecruitsChangedListener(new RecruitListMgr.RecruitsChangedCallback(this.OnRecruitsChanged));
        if (ChatMgr.Get() != null)
        {
            ChatMgr.Get().RemovePlayerChatInfoChangedListener(new ChatMgr.PlayerChatInfoChangedCallback(this.OnPlayerChatInfoChanged));
        }
    }

    private void OnPlayerChatInfoChanged(PlayerChatInfo chatInfo, object userData)
    {
        if (this.m_player == chatInfo.GetPlayer())
        {
            this.UpdateFriend();
        }
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        if (changelist.HasChange(this.m_player))
        {
            this.UpdateFriend();
        }
    }

    private void OnRecruitsChanged()
    {
        if ((this.m_player != null) && (RecruitListMgr.Get().GetRecruitInfoFromAccountId(this.m_player.GetAccountId()) != null))
        {
            this.UpdateFriend();
        }
    }

    private void OnWhisper(BnetWhisper whisper, object userData)
    {
        if ((this.m_player != null) && whisper.IsSpeakerOrReceiver(this.m_player))
        {
            this.UpdateFriend();
        }
    }

    private void RankMedalOver(UIEvent e)
    {
        string name = this.m_medal.GetCurrentMedal().name;
        KeywordHelpPanel src = this.m_rankMedal.GetComponent<TooltipZone>().ShowTooltip(name, string.Empty, 0.7f, true);
        SceneUtils.SetLayer(src.gameObject, GameLayer.BattleNetFriendList);
        src.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        src.transform.localScale = new Vector3(3f, 3f, 3f);
        TransformUtil.SetPoint(src, Anchor.LEFT, this.m_rankMedal.gameObject, Anchor.RIGHT, new Vector3(1f, 0f, 0f));
    }

    public virtual bool SetFriend(BnetPlayer player)
    {
        if (this.m_player == player)
        {
            return false;
        }
        this.m_player = player;
        this.m_PlayerIcon.SetPlayer(player);
        this.m_ChatIcon.SetPlayer(player);
        this.UpdateFriend();
        return true;
    }

    public virtual void UpdateFriend()
    {
        this.m_ChatIcon.UpdateIcon();
        if (this.m_player != null)
        {
            Color color;
            if (this.m_player.IsOnline())
            {
                if (this.m_player.IsAway())
                {
                    color = TEXT_COLOR_AWAY;
                }
                else if (this.m_player.IsBusy())
                {
                    color = TEXT_COLOR_BUSY;
                }
                else
                {
                    color = TEXT_COLOR_NORMAL;
                }
            }
            else
            {
                color = TEXT_COLOR_OFFLINE;
            }
            this.m_StatusText.TextColor = color;
            BnetGameAccount hearthstoneGameAccount = this.m_player.GetHearthstoneGameAccount();
            this.m_medal = (hearthstoneGameAccount != null) ? RankMgr.Get().GetRankPresenceField(hearthstoneGameAccount) : null;
            if ((this.m_medal == null) || (this.m_medal.GetCurrentMedal().rank == 0x19))
            {
                this.m_rankMedal.gameObject.SetActive(false);
                this.m_PlayerIcon.Show();
            }
            else
            {
                this.m_PlayerIcon.Hide();
                this.m_rankMedal.gameObject.SetActive(true);
                this.m_rankMedal.SetMedal(this.m_medal, false);
            }
        }
    }

    protected void UpdateOfflineStatus()
    {
        ulong bestLastOnlineMicrosec = this.m_player.GetBestLastOnlineMicrosec();
        this.m_StatusText.Text = FriendUtils.GetLastOnlineElapsedTimeString(bestLastOnlineMicrosec);
    }

    protected void UpdateOnlineStatus()
    {
        if (this.m_player.IsAway())
        {
            this.m_StatusText.Text = FriendUtils.GetAwayTimeString(this.m_player.GetBestAwayTimeMicrosec());
        }
        else if (this.m_player.IsBusy())
        {
            this.m_StatusText.Text = GameStrings.Get("GLOBAL_FRIENDLIST_BUSYSTATUS");
        }
        else
        {
            string statusText = PresenceMgr.Get().GetStatusText(this.m_player);
            if (statusText != null)
            {
                this.m_StatusText.Text = statusText;
            }
            else
            {
                BnetProgramId bestProgramId = this.m_player.GetBestProgramId();
                if (bestProgramId != null)
                {
                    this.m_StatusText.Text = BnetProgramId.GetName(bestProgramId);
                }
                else
                {
                    this.m_StatusText.Text = string.Empty;
                }
            }
        }
    }
}

