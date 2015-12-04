using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SocialToastMgr : MonoBehaviour
{
    private const float FADE_IN_TIME = 0.25f;
    private const float FADE_OUT_TIME = 0.5f;
    private const float HOLD_TIME = 2f;
    private Map<BnetGameAccountId, int> m_lastKnownMedal;
    private Map<int, LastOnlineTracker> m_lastOnlineTracker;
    public SocialToast m_socialToastPrefab;
    private SocialToast m_toast;
    private bool m_toastIsShown;
    private const float OFFLINE_TOAST_DELAY = 5f;
    private static SocialToastMgr s_instance;
    private const float SHUTDOWN_MESSAGE_TIME = 3.5f;
    private PlatformDependentValue<Vector3> TOAST_SCALE;

    public SocialToastMgr()
    {
        PlatformDependentValue<Vector3> value2 = new PlatformDependentValue<Vector3>(PlatformCategory.Screen) {
            PC = new Vector3(235f, 1f, 235f),
            Phone = new Vector3(470f, 1f, 470f)
        };
        this.TOAST_SCALE = value2;
        this.m_lastKnownMedal = new Map<BnetGameAccountId, int>();
        this.m_lastOnlineTracker = new Map<int, LastOnlineTracker>();
    }

    public void AddToast(string textArg)
    {
        this.AddToast(textArg, TOAST_TYPE.DEFAULT, 2f, true);
    }

    public void AddToast(string textArg, TOAST_TYPE toastType)
    {
        this.AddToast(textArg, toastType, 2f, true);
    }

    public void AddToast(string textArg, TOAST_TYPE toastType, bool playSound)
    {
        this.AddToast(textArg, toastType, 2f, playSound);
    }

    public void AddToast(string textArg, TOAST_TYPE toastType, float displayTime)
    {
        this.AddToast(textArg, toastType, displayTime, true);
    }

    public void AddToast(string textArg, TOAST_TYPE toastType, float displayTime, bool playSound)
    {
        string str;
        switch (toastType)
        {
            case TOAST_TYPE.DEFAULT:
                str = textArg;
                break;

            case TOAST_TYPE.FRIEND_ONLINE:
            {
                object[] objArray2 = new object[] { "5ecaf0ff", textArg };
                str = GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ONLINE", objArray2);
                break;
            }
            case TOAST_TYPE.FRIEND_OFFLINE:
            {
                object[] objArray1 = new object[] { "999999ff", textArg };
                str = GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_OFFLINE", objArray1);
                break;
            }
            case TOAST_TYPE.FRIEND_INVITE:
            {
                object[] objArray3 = new object[] { "5ecaf0ff", textArg };
                str = GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_REQUEST", objArray3);
                break;
            }
            case TOAST_TYPE.HEALTHY_GAMING:
            {
                object[] objArray4 = new object[] { textArg };
                str = GameStrings.Format("GLOBAL_HEALTHY_GAMING_TOAST", objArray4);
                break;
            }
            case TOAST_TYPE.HEALTHY_GAMING_OVER_THRESHOLD:
            {
                object[] objArray5 = new object[] { textArg };
                str = GameStrings.Format("GLOBAL_HEALTHY_GAMING_TOAST_OVER_THRESHOLD", objArray5);
                break;
            }
            case TOAST_TYPE.SPECTATOR_INVITE_SENT:
            {
                object[] objArray6 = new object[] { "5ecaf0ff", textArg };
                str = GameStrings.Format("GLOBAL_SOCIAL_TOAST_SPECTATOR_INVITE_SENT", objArray6);
                break;
            }
            case TOAST_TYPE.SPECTATOR_INVITE_RECEIVED:
            {
                object[] objArray7 = new object[] { "5ecaf0ff", textArg };
                str = GameStrings.Format("GLOBAL_SOCIAL_TOAST_SPECTATOR_INVITE_RECEIVED", objArray7);
                break;
            }
            case TOAST_TYPE.SPECTATOR_ADDED:
            {
                object[] objArray8 = new object[] { "5ecaf0ff", textArg };
                str = GameStrings.Format("GLOBAL_SOCIAL_TOAST_SPECTATOR_ADDED", objArray8);
                break;
            }
            case TOAST_TYPE.SPECTATOR_REMOVED:
            {
                object[] objArray9 = new object[] { "5ecaf0ff", textArg };
                str = GameStrings.Format("GLOBAL_SOCIAL_TOAST_SPECTATOR_REMOVED", objArray9);
                break;
            }
            default:
                str = string.Empty;
                break;
        }
        if (this.m_toastIsShown)
        {
            iTween.Stop(this.m_toast.gameObject, true);
            iTween.Stop(base.gameObject, true);
            RenderUtils.SetAlpha(this.m_toast.gameObject, 0f);
        }
        this.m_toast.gameObject.SetActive(true);
        this.m_toast.SetText(str);
        object[] args = new object[] { "amount", 1f, "time", 0.25f, "easeType", iTween.EaseType.easeInCubic, "oncomplete", "FadeOutToast", "oncompletetarget", base.gameObject, "oncompleteparams", displayTime, "name", "fade" };
        Hashtable hashtable = iTween.Hash(args);
        iTween.StopByName(base.gameObject, "fade");
        iTween.FadeTo(this.m_toast.gameObject, hashtable);
        this.m_toastIsShown = true;
        if (playSound)
        {
            SoundManager.Get().LoadAndPlay("UI_BnetToast");
        }
    }

    private void Awake()
    {
        s_instance = this;
        this.m_toast = UnityEngine.Object.Instantiate<SocialToast>(this.m_socialToastPrefab);
        RenderUtils.SetAlpha(this.m_toast.gameObject, 0f);
        this.m_toast.gameObject.SetActive(false);
        this.m_toast.transform.parent = BnetBar.Get().m_socialToastBone.transform;
        this.m_toast.transform.localRotation = Quaternion.Euler(new Vector3(90f, 180f, 0f));
        this.m_toast.transform.localScale = (Vector3) this.TOAST_SCALE;
        this.m_toast.transform.position = BnetBar.Get().m_socialToastBone.transform.position;
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        BnetPresenceMgr.Get().OnGameAccountPresenceChange += new Action<BattleNet.PresenceUpdate[]>(this.OnPresenceChanged);
        BnetFriendMgr.Get().AddChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        Network.Get().SetShutdownHandler(new Network.ShutdownHandler(this.ShutdownHandler));
        SoundManager.Get().Load("UI_BnetToast");
    }

    private void CheckArenaGameStarted(BnetPlayer player)
    {
        if (PresenceMgr.Get().GetStatus(player) == PresenceStatus.ARENA_GAME)
        {
            ArenaRecord record;
            BnetGameAccount hearthstoneGameAccount = player.GetHearthstoneGameAccount();
            if (((hearthstoneGameAccount != null) && ArenaRecord.TryParse(hearthstoneGameAccount.GetArenaRecord(), out record)) && (record.wins >= 8))
            {
                object[] args = new object[] { "5ecaf0ff", player.GetBestName(), record.wins };
                this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ARENA_START_WITH_MANY_WINS", args));
            }
        }
    }

    private void CheckArenaRecordChanged(BnetPlayer player)
    {
        ArenaRecord record;
        BnetGameAccount hearthstoneGameAccount = player.GetHearthstoneGameAccount();
        if ((hearthstoneGameAccount != null) && ArenaRecord.TryParse(hearthstoneGameAccount.GetArenaRecord(), out record))
        {
            if (record.isFinished)
            {
                if (record.wins >= 3)
                {
                    object[] args = new object[] { "5ecaf0ff", player.GetBestName(), record.wins, record.losses };
                    this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ARENA_COMPLETE", args));
                }
            }
            else if ((record.wins == 0) && (record.losses == 0))
            {
                object[] objArray2 = new object[] { "5ecaf0ff", player.GetBestName() };
                this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ARENA_START", objArray2));
            }
        }
    }

    private void CheckForCardOpened(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (newPlayerAccount.GetCardsOpened() != oldPlayerAccount.GetCardsOpened())
        {
            string cardsOpened = newPlayerAccount.GetCardsOpened();
            if (!string.IsNullOrEmpty(cardsOpened))
            {
                char[] separator = new char[] { ',' };
                string[] strArray = cardsOpened.Split(separator);
                if (strArray.Length == 2)
                {
                    EntityDef entityDef = DefLoader.Get().GetEntityDef(strArray[0]);
                    if (entityDef != null)
                    {
                        if (strArray[1] == "1")
                        {
                            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), entityDef.GetName(), "ffd200" };
                            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_GOLDEN_LEGENDARY", args));
                        }
                        else
                        {
                            object[] objArray2 = new object[] { "5ecaf0ff", newPlayer.GetBestName(), entityDef.GetName(), "ff9c00" };
                            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_LEGENDARY", objArray2));
                        }
                    }
                }
            }
        }
    }

    private void CheckForDruidLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetDruidLevel(), newPlayerAccount.GetDruidLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetDruidLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_DRUID_LEVEL", args));
        }
    }

    private void CheckForHunterLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetHunterLevel(), newPlayerAccount.GetHunterLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetHunterLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_HUNTER_LEVEL", args));
        }
    }

    private void CheckForMageLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetMageLevel(), newPlayerAccount.GetMageLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetMageLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_MAGE_LEVEL", args));
        }
    }

    private void CheckForMissionComplete(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if ((newPlayerAccount.GetTutorialBeaten() != oldPlayerAccount.GetTutorialBeaten()) && (newPlayerAccount.GetTutorialBeaten() == 1))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ILLIDAN_COMPLETE", args));
        }
    }

    private void CheckForNewRank(BnetPlayer player)
    {
        MedalInfoTranslator rankPresenceField = RankMgr.Get().GetRankPresenceField(player);
        if (rankPresenceField != null)
        {
            BnetGameAccountId hearthstoneGameAccountId = player.GetHearthstoneGameAccountId();
            int rank = rankPresenceField.GetCurrentMedal().rank;
            if (!this.m_lastKnownMedal.ContainsKey(hearthstoneGameAccountId))
            {
                this.m_lastKnownMedal[hearthstoneGameAccountId] = rank;
            }
            else if ((rank <= 10) && (this.m_lastKnownMedal[hearthstoneGameAccountId] > rank))
            {
                this.m_lastKnownMedal[hearthstoneGameAccountId] = rank;
                if (rank == 0)
                {
                    object[] args = new object[] { "5ecaf0ff", player.GetBestName() };
                    this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_RANK_LEGEND", args));
                }
                else
                {
                    object[] objArray2 = new object[] { "5ecaf0ff", player.GetBestName(), rankPresenceField.GetCurrentMedal().rank };
                    this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_GAINED_RANK", objArray2));
                }
            }
        }
    }

    private void CheckForOnlineStatusChanged(BnetPlayer oldPlayer, BnetPlayer newPlayer)
    {
        <CheckForOnlineStatusChanged>c__AnonStorey2BB storeybb = new <CheckForOnlineStatusChanged>c__AnonStorey2BB {
            newPlayer = newPlayer,
            <>f__this = this
        };
        if ((oldPlayer == null) || (oldPlayer.IsOnline() != storeybb.newPlayer.IsOnline()))
        {
            ulong bestLastOnlineMicrosec = storeybb.newPlayer.GetBestLastOnlineMicrosec();
            ulong num2 = BnetPresenceMgr.Get().GetMyPlayer().GetBestLastOnlineMicrosec();
            if (((bestLastOnlineMicrosec != 0) && (num2 != 0)) && (num2 <= bestLastOnlineMicrosec))
            {
                LastOnlineTracker tracker = null;
                float fixedTime = UnityEngine.Time.fixedTime;
                int hashCode = storeybb.newPlayer.GetAccountId().GetHashCode();
                if (!this.m_lastOnlineTracker.TryGetValue(hashCode, out tracker))
                {
                    tracker = new LastOnlineTracker();
                    this.m_lastOnlineTracker[hashCode] = tracker;
                }
                if (storeybb.newPlayer.IsOnline())
                {
                    if (tracker.m_callback != null)
                    {
                        ApplicationMgr.Get().CancelScheduledCallback(tracker.m_callback, null);
                    }
                    tracker.m_callback = null;
                    if ((fixedTime - tracker.m_localLastOnlineTime) >= 5f)
                    {
                        this.AddToast(storeybb.newPlayer.GetBestName(), TOAST_TYPE.FRIEND_ONLINE);
                    }
                }
                else
                {
                    tracker.m_localLastOnlineTime = fixedTime;
                    tracker.m_callback = new ApplicationMgr.ScheduledCallback(storeybb.<>m__71);
                    ApplicationMgr.Get().ScheduleCallback(5f, false, tracker.m_callback, null);
                }
            }
        }
    }

    private void CheckForPaladinLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetPaladinLevel(), newPlayerAccount.GetPaladinLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetPaladinLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_PALADIN_LEVEL", args));
        }
    }

    private void CheckForPriestLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetPriestLevel(), newPlayerAccount.GetPriestLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetPriestLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_PRIEST_LEVEL", args));
        }
    }

    private void CheckForRogueLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetRogueLevel(), newPlayerAccount.GetRogueLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetRogueLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_ROGUE_LEVEL", args));
        }
    }

    private void CheckForShamanLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetShamanLevel(), newPlayerAccount.GetShamanLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetShamanLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_SHAMAN_LEVEL", args));
        }
    }

    private void CheckForWarlockLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetWarlockLevel(), newPlayerAccount.GetWarlockLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetWarlockLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_WARLOCK_LEVEL", args));
        }
    }

    private void CheckForWarriorLevelChanged(BnetGameAccount oldPlayerAccount, BnetGameAccount newPlayerAccount, BnetPlayer newPlayer)
    {
        if (this.ShouldToastThisLevel(oldPlayerAccount.GetWarriorLevel(), newPlayerAccount.GetWarriorLevel()))
        {
            object[] args = new object[] { "5ecaf0ff", newPlayer.GetBestName(), newPlayerAccount.GetWarriorLevel() };
            this.AddToast(GameStrings.Format("GLOBAL_SOCIAL_TOAST_FRIEND_WARRIOR_LEVEL", args));
        }
    }

    private void DeactivateToast()
    {
        this.m_toast.gameObject.SetActive(false);
        this.m_toastIsShown = false;
    }

    private void FadeOutToast(float displayTime)
    {
        object[] args = new object[] { "amount", 0f, "delay", displayTime, "time", 0.25f, "easeType", iTween.EaseType.easeInCubic, "oncomplete", "DeactivateToast", "oncompletetarget", base.gameObject, "name", "fade" };
        Hashtable hashtable = iTween.Hash(args);
        iTween.FadeTo(this.m_toast.gameObject, hashtable);
    }

    public static SocialToastMgr Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        BnetPresenceMgr.Get().OnGameAccountPresenceChange -= new Action<BattleNet.PresenceUpdate[]>(this.OnPresenceChanged);
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        BnetFriendMgr.Get().RemoveChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        this.m_lastKnownMedal.Clear();
        s_instance = null;
    }

    private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
    {
        if (DemoMgr.Get().IsSocialEnabled())
        {
            List<BnetInvitation> addedReceivedInvites = changelist.GetAddedReceivedInvites();
            if (addedReceivedInvites != null)
            {
                BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
                if ((myPlayer == null) || !myPlayer.IsBusy())
                {
                    foreach (BnetInvitation invitation in addedReceivedInvites)
                    {
                        BnetPlayer recentOpponent = FriendMgr.Get().GetRecentOpponent();
                        if ((recentOpponent != null) && recentOpponent.HasAccount(invitation.GetInviterId()))
                        {
                            this.AddToast(GameStrings.Get("GLOBAL_SOCIAL_TOAST_RECENT_OPPONENT_FRIEND_REQUEST"));
                        }
                        else
                        {
                            this.AddToast(invitation.GetInviterName(), TOAST_TYPE.FRIEND_INVITE);
                        }
                    }
                }
            }
        }
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        if (DemoMgr.Get().IsSocialEnabled())
        {
            BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
            foreach (BnetPlayerChange change in changelist.GetChanges())
            {
                if ((((change.GetPlayer() != null) && (change.GetNewPlayer() != null)) && ((change != null) && change.GetPlayer().IsDisplayable())) && ((change.GetPlayer() != myPlayer) && BnetFriendMgr.Get().IsFriend(change.GetPlayer())))
                {
                    BnetPlayer oldPlayer = change.GetOldPlayer();
                    BnetPlayer newPlayer = change.GetNewPlayer();
                    this.CheckForOnlineStatusChanged(oldPlayer, newPlayer);
                    if (oldPlayer != null)
                    {
                        BnetGameAccount hearthstoneGameAccount = newPlayer.GetHearthstoneGameAccount();
                        BnetGameAccount oldPlayerAccount = oldPlayer.GetHearthstoneGameAccount();
                        if ((oldPlayerAccount != null) && (hearthstoneGameAccount != null))
                        {
                            this.CheckForCardOpened(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForDruidLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForHunterLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForMageLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForPaladinLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForPriestLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForRogueLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForShamanLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForWarlockLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForWarriorLevelChanged(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                            this.CheckForMissionComplete(oldPlayerAccount, hearthstoneGameAccount, newPlayer);
                        }
                    }
                }
            }
        }
    }

    private void OnPresenceChanged(BattleNet.PresenceUpdate[] updates)
    {
        BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
        foreach (BattleNet.PresenceUpdate update in updates)
        {
            if (update.programId == BnetProgramId.HEARTHSTONE)
            {
                BnetPlayer player = BnetUtils.GetPlayer(BnetGameAccountId.CreateFromDll(update.entityId));
                if ((((player != null) && (player != myPlayer)) && player.IsDisplayable()) && BnetFriendMgr.Get().IsFriend(player))
                {
                    switch (update.fieldId)
                    {
                        case 0x11:
                            this.CheckArenaGameStarted(player);
                            break;

                        case 0x12:
                            this.CheckForNewRank(player);
                            break;

                        case 3:
                            this.CheckArenaRecordChanged(player);
                            break;
                    }
                }
            }
        }
    }

    public void Reset()
    {
        iTween.Stop(this.m_toast.gameObject, true);
        iTween.Stop(base.gameObject, true);
        RenderUtils.SetAlpha(this.m_toast.gameObject, 0f);
        this.DeactivateToast();
    }

    private bool ShouldToastThisLevel(int oldLevel, int newLevel)
    {
        if (oldLevel == newLevel)
        {
            return false;
        }
        if (((newLevel != 20) && (newLevel != 30)) && (((newLevel != 40) && (newLevel != 50)) && (newLevel != 60)))
        {
            return false;
        }
        return true;
    }

    private void ShutdownHandler(int minutes)
    {
        object[] args = new object[] { "f61f1fff", minutes };
        this.AddToast(GameStrings.Format("GLOBAL_SHUTDOWN_TOAST", args), TOAST_TYPE.DEFAULT, (float) 3.5f);
    }

    [CompilerGenerated]
    private sealed class <CheckForOnlineStatusChanged>c__AnonStorey2BB
    {
        internal SocialToastMgr <>f__this;
        internal BnetPlayer newPlayer;

        internal void <>m__71(object data)
        {
            if ((this.newPlayer != null) && !this.newPlayer.IsOnline())
            {
                this.<>f__this.AddToast(this.newPlayer.GetBestName(), SocialToastMgr.TOAST_TYPE.FRIEND_OFFLINE, false);
            }
        }
    }

    private class LastOnlineTracker
    {
        public ApplicationMgr.ScheduledCallback m_callback;
        public float m_localLastOnlineTime;
    }

    public enum TOAST_TYPE
    {
        DEFAULT,
        FRIEND_ONLINE,
        FRIEND_OFFLINE,
        FRIEND_INVITE,
        HEALTHY_GAMING,
        HEALTHY_GAMING_OVER_THRESHOLD,
        FRIEND_ARENA_COMPLETE,
        SPECTATOR_INVITE_SENT,
        SPECTATOR_INVITE_RECEIVED,
        SPECTATOR_ADDED,
        SPECTATOR_REMOVED
    }
}

