using bnet;
using bnet.protocol.attribute;
using PegasusGame;
using PegasusShared;
using SpectatorProto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class SpectatorManager
{
    [CompilerGenerated]
    private static Func<PartyInvite, bool> <>f__am$cache1A;
    [CompilerGenerated]
    private static AlertPopup.ResponseCallback <>f__am$cache1B;
    [CompilerGenerated]
    private static Func<KeyValuePair<BnetGameAccountId, ReceivedInvite>, float> <>f__am$cache1C;
    [CompilerGenerated]
    private static AlertPopup.ResponseCallback <>f__am$cache1D;
    [CompilerGenerated]
    private static Func<PartyInfo, string> <>f__am$cache1E;
    [CompilerGenerated]
    private static Func<KeyValuePair<BnetGameAccountId, ReceivedInvite>, float> <>f__am$cache1F;
    [CompilerGenerated]
    private static Func<PartyInfo, string> <>f__am$cache20;
    [CompilerGenerated]
    private static Func<PartyInfo, bool> <>f__am$cache21;
    private const string ALERTPOPUPID_WAITINGFORNEXTGAME = "SPECTATOR_WAITING_FOR_NEXT_GAME";
    private static readonly PlatformDependentValue<bool> DISABLE_MENU_BUTTON_WHILE_WAITING;
    private const float ENDGAMESCREEN_AUTO_CLOSE_SECONDS = 5f;
    private const float KICKED_FROM_SPECTATING_BLACKOUT_DURATION_SECONDS = 10f;
    private int? m_expectedDisconnectReason;
    private HashSet<BnetGameAccountId> m_gameServerKnownSpectators = new HashSet<BnetGameAccountId>();
    private bool m_initialized;
    private bool m_isExpectingArriveInGameplayAsSpectator;
    private bool m_isShowingRemovedAsSpectatorPopup;
    private Map<BnetGameAccountId, float> m_kickedFromSpectatingList;
    private HashSet<BnetGameAccountId> m_kickedPlayers;
    private Map<PartyId, BnetGameAccountId> m_knownPartyCreatorIds = new Map<PartyId, BnetGameAccountId>();
    private HashSet<PartyId> m_leavePartyIdsRequested;
    private PendingSpectatePlayer m_pendingSpectatePlayerAfterLeave;
    private Map<BnetGameAccountId, ReceivedInvite> m_receivedSpectateMeInvites = new Map<BnetGameAccountId, ReceivedInvite>();
    private IntendedSpectateeParty m_requestedInvite;
    private Map<BnetGameAccountId, float> m_sentSpectateMeInvites = new Map<BnetGameAccountId, float>();
    private BnetGameAccountId m_spectateeFriendlySide;
    private BnetGameAccountId m_spectateeOpposingSide;
    private PartyId m_spectatorPartyIdMain;
    private PartyId m_spectatorPartyIdOpposingSide;
    private HashSet<BnetGameAccountId> m_userInitiatedOutgoingInvites;
    private AlertPopup m_waitingForNextGameDialog;
    public const int MAX_SPECTATORS_PER_SIDE = 10;
    private const float RECEIVED_INVITE_TIMEOUT_SECONDS = 300f;
    private const float REQUEST_INVITE_TIMEOUT_SECONDS = 5f;
    private static SpectatorManager s_instance;
    private const float SENT_INVITE_TIMEOUT_SECONDS = 30f;
    private static readonly PlatformDependentValue<float> WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS;

    public event InviteReceivedHandler OnInviteReceived;

    public event InviteSentHandler OnInviteSent;

    public event SpectatorModeChangedHandler OnSpectatorModeChanged;

    public event SpectatorToMyGameHandler OnSpectatorToMyGame;

    static SpectatorManager()
    {
        PlatformDependentValue<float> value2 = new PlatformDependentValue<float>(PlatformCategory.OS) {
            iOS = 300f,
            Android = 300f,
            PC = -1f,
            Mac = -1f
        };
        WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS = value2;
        PlatformDependentValue<bool> value3 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            iOS = true,
            Android = true,
            PC = false,
            Mac = false
        };
        DISABLE_MENU_BUTTON_WHILE_WAITING = value3;
        s_instance = null;
    }

    private SpectatorManager()
    {
    }

    private void AddKnownSpectator(BnetGameAccountId gameAccountId)
    {
        if (gameAccountId != null)
        {
            bool flag = this.m_gameServerKnownSpectators.Add(gameAccountId);
            this.CreatePartyIfNecessary();
            this.RemoveSentInvitation(gameAccountId);
            this.RemoveReceivedInvitation(gameAccountId);
            if (flag)
            {
                if (SceneMgr.Get().IsInGame() && Network.IsConnectedToGameServer())
                {
                    bool flag2 = BnetParty.IsMember(this.m_spectatorPartyIdMain, gameAccountId);
                    BnetPlayer spectator = BnetUtils.GetPlayer(gameAccountId);
                    if (!flag2)
                    {
                        ApplicationMgr.Get().StartCoroutine(this.WaitForPresenceThenToast(gameAccountId, SocialToastMgr.TOAST_TYPE.SPECTATOR_ADDED));
                    }
                    if (this.OnSpectatorToMyGame != null)
                    {
                        this.OnSpectatorToMyGame(OnlineEventType.ADDED, spectator);
                    }
                }
                this.UpdateSpectatorPresence();
            }
        }
    }

    private void AddReceivedInvitation(BnetGameAccountId inviterId, JoinInfo joinInfo)
    {
        bool flag = !this.m_receivedSpectateMeInvites.ContainsKey(inviterId);
        ReceivedInvite invite = new ReceivedInvite(joinInfo);
        this.m_receivedSpectateMeInvites[inviterId] = invite;
        if (flag)
        {
            BnetPlayer inviter = BnetUtils.GetPlayer(inviterId);
            if (SocialToastMgr.Get() != null)
            {
                SocialToastMgr.Get().AddToast(BnetUtils.GetPlayerBestName(inviterId), SocialToastMgr.TOAST_TYPE.SPECTATOR_INVITE_RECEIVED);
            }
            if (this.OnInviteReceived != null)
            {
                this.OnInviteReceived(OnlineEventType.ADDED, inviter);
            }
        }
        if (<>f__am$cache1F == null)
        {
            <>f__am$cache1F = kv => kv.Value.m_timestamp;
        }
        float num = Enumerable.Min<KeyValuePair<BnetGameAccountId, ReceivedInvite>>(this.m_receivedSpectateMeInvites, <>f__am$cache1F);
        float secondsToWait = Mathf.Max((float) 0f, (float) ((num + 300f) - UnityEngine.Time.realtimeSinceStartup));
        ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.ReceivedInvitation_ExpireTimeout), null);
        ApplicationMgr.Get().ScheduleCallback(secondsToWait, true, new ApplicationMgr.ScheduledCallback(this.ReceivedInvitation_ExpireTimeout), null);
    }

    private void AddSentInvitation(BnetGameAccountId inviteeId)
    {
        if (inviteeId != null)
        {
            bool flag = !this.m_sentSpectateMeInvites.ContainsKey(inviteeId);
            this.m_sentSpectateMeInvites[inviteeId] = UnityEngine.Time.realtimeSinceStartup;
            if (flag)
            {
                BnetPlayer invitee = BnetUtils.GetPlayer(inviteeId);
                if (this.OnInviteSent != null)
                {
                    this.OnInviteSent(OnlineEventType.ADDED, invitee);
                }
            }
        }
    }

    private void AutoSpectateOpposingSide()
    {
        if (GameState.Get() != null)
        {
            if (GameState.Get().GetCreateGamePhase() < GameState.CreateGamePhase.CREATED)
            {
                GameState.Get().RegisterCreateGameListener(new GameState.CreateGameCallback(this.GameState_CreateGameEvent), null);
            }
            else if (SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
            {
                if ((GameMgr.Get().GetTransitionPopup() != null) && GameMgr.Get().GetTransitionPopup().IsShown())
                {
                    GameMgr.Get().GetTransitionPopup().OnHidden += new Action<TransitionPopup>(this.EnterSpectatorMode_OnTransitionPopupHide);
                }
                else if (((this.m_spectatorPartyIdOpposingSide != null) && (this.m_spectateeOpposingSide != null)) && this.IsStillInParty(this.m_spectatorPartyIdOpposingSide))
                {
                    if (this.IsPlayerInGame(this.m_spectateeOpposingSide))
                    {
                        PartyServerInfo partyServerInfo = GetPartyServerInfo(this.m_spectatorPartyIdOpposingSide);
                        JoinInfo joinInfo = (partyServerInfo != null) ? CreateJoinInfo(partyServerInfo) : null;
                        if (joinInfo != null)
                        {
                            this.SpectateSecondPlayer_Network(joinInfo);
                        }
                    }
                    else
                    {
                        this.LogInfoPower("================== End Spectating 2nd player ==================", new object[0]);
                        this.LeaveParty(this.m_spectatorPartyIdOpposingSide, false);
                    }
                }
            }
        }
    }

    private void BnetFriendMgr_OnFriendsChanged(BnetFriendChangelist changelist, object userData)
    {
        if ((changelist != null) && this.IsBeingSpectated())
        {
            List<BnetPlayer> removedFriends = changelist.GetRemovedFriends();
            if (removedFriends != null)
            {
                foreach (BnetPlayer player in removedFriends)
                {
                    BnetGameAccountId hearthstoneGameAccountId = player.GetHearthstoneGameAccountId();
                    if (this.IsSpectatingMe(hearthstoneGameAccountId))
                    {
                        this.KickSpectator_Internal(player, true, false);
                    }
                }
            }
        }
    }

    private void BnetParty_OnChatMessage(PartyInfo party, BnetGameAccountId speakerId, string chatMessage)
    {
    }

    private void BnetParty_OnError(PartyError error)
    {
        if (error.IsOperationCallback)
        {
            switch (error.FeatureEvent)
            {
                case BnetFeatureEvent.Party_Create_Callback:
                    if (error.ErrorCode != 0)
                    {
                        this.m_userInitiatedOutgoingInvites = null;
                        string header = GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER");
                        string body = GameStrings.Format("GLOBAL_SPECTATOR_ERROR_CREATE_PARTY_TEXT", new object[0]);
                        DisplayErrorDialog(header, body);
                    }
                    break;

                case BnetFeatureEvent.Party_Leave_Callback:
                case BnetFeatureEvent.Party_Dissolve_Callback:
                    if (this.m_leavePartyIdsRequested != null)
                    {
                        this.m_leavePartyIdsRequested.Remove(error.PartyId);
                    }
                    if ((this.m_pendingSpectatePlayerAfterLeave != null) && (error.ErrorCode != 0))
                    {
                        string playerBestName = BnetUtils.GetPlayerBestName(this.m_pendingSpectatePlayerAfterLeave.SpectateeId);
                        string str2 = GameStrings.Get("GLOBAL_ERROR_GENERIC_HEADER");
                        object[] args = new object[] { playerBestName };
                        string str3 = GameStrings.Format("GLOBAL_SPECTATOR_ERROR_LEAVE_FOR_SPECTATE_PLAYER_TEXT", args);
                        DisplayErrorDialog(str2, str3);
                        this.m_pendingSpectatePlayerAfterLeave = null;
                    }
                    break;
            }
        }
    }

    private void BnetParty_OnJoined(OnlineEventType evt, PartyInfo party, LeaveReason? reason)
    {
        if (this.m_initialized && (party.Type == PartyType.SPECTATOR_PARTY))
        {
            if (evt == OnlineEventType.REMOVED)
            {
                bool flag = false;
                if (this.m_leavePartyIdsRequested != null)
                {
                    flag = this.m_leavePartyIdsRequested.Remove(party.Id);
                }
                object[] args = new object[] { party, this.m_spectatorPartyIdMain, !reason.HasValue ? "null" : reason.Value.ToString(), flag };
                this.LogInfoParty("SpectatorParty_OnLeft: left party={0} current={1} reason={2} wasRequested={3}", args);
                bool flag2 = false;
                if (party.Id == this.m_spectatorPartyIdOpposingSide)
                {
                    this.m_spectatorPartyIdOpposingSide = null;
                    flag2 = true;
                }
                else if (this.m_spectateeFriendlySide != null)
                {
                    if (party.Id == this.m_spectatorPartyIdMain)
                    {
                        this.m_spectatorPartyIdMain = null;
                        flag2 = true;
                    }
                }
                else if ((this.m_spectateeFriendlySide == null) && (this.m_spectateeOpposingSide == null))
                {
                    if (party.Id != this.m_spectatorPartyIdMain)
                    {
                        this.CreatePartyIfNecessary();
                        return;
                    }
                    this.m_userInitiatedOutgoingInvites = null;
                    this.m_spectatorPartyIdMain = null;
                    this.UpdateSpectatorPresence();
                    if ((reason.HasValue && (((LeaveReason) reason.Value) != LeaveReason.MEMBER_LEFT)) && (((LeaveReason) reason.Value) != LeaveReason.DISSOLVED_BY_MEMBER))
                    {
                        ApplicationMgr.Get().ScheduleCallback(1f, true, userData => this.CreatePartyIfNecessary(), null);
                    }
                }
                if (((this.m_pendingSpectatePlayerAfterLeave != null) && (this.m_spectatorPartyIdMain == null)) && (this.m_spectatorPartyIdOpposingSide == null))
                {
                    this.SpectatePlayer_Internal(this.m_pendingSpectatePlayerAfterLeave.SpectateeId, this.m_pendingSpectatePlayerAfterLeave.JoinInfo);
                }
                else if (flag2 && (this.m_spectatorPartyIdMain == null))
                {
                    if (flag)
                    {
                        this.EndSpectatorMode(true);
                    }
                    else
                    {
                        bool flag3 = reason.HasValue && (((LeaveReason) reason.Value) == LeaveReason.MEMBER_KICKED);
                        bool flag4 = this.m_expectedDisconnectReason.HasValue && (this.m_expectedDisconnectReason.Value == 0);
                        this.EndSpectatorMode(true);
                        if (flag3 && !flag4)
                        {
                            if (flag3)
                            {
                                BnetGameAccountId partyCreator = this.GetPartyCreator(party.Id);
                                if (partyCreator == null)
                                {
                                    PartyMember leader = BnetParty.GetLeader(party.Id);
                                    partyCreator = (leader != null) ? leader.GameAccountId : null;
                                }
                                if (partyCreator != null)
                                {
                                    if (this.m_kickedFromSpectatingList == null)
                                    {
                                        this.m_kickedFromSpectatingList = new Map<BnetGameAccountId, float>();
                                    }
                                    this.m_kickedFromSpectatingList[partyCreator] = UnityEngine.Time.realtimeSinceStartup;
                                }
                            }
                            if (!this.m_isShowingRemovedAsSpectatorPopup)
                            {
                                bool flag5 = GameMgr.Get().IsTransitionPopupShown();
                                AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                                    m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_HEADER"),
                                    m_text = GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_TEXT"),
                                    m_responseDisplay = AlertPopup.ResponseDisplay.OK
                                };
                                if (flag5)
                                {
                                    info.m_responseCallback = new AlertPopup.ResponseCallback(this.Network_OnSpectatorNotifyEvent_Removed_GoToNextMode);
                                }
                                else
                                {
                                    if (<>f__am$cache1D == null)
                                    {
                                        <>f__am$cache1D = (r, data) => Get().m_isShowingRemovedAsSpectatorPopup = false;
                                    }
                                    info.m_responseCallback = <>f__am$cache1D;
                                }
                                this.m_isShowingRemovedAsSpectatorPopup = true;
                                DialogManager.Get().ShowPopup(info);
                            }
                        }
                    }
                }
                if (ApplicationMgr.Get() != null)
                {
                    ApplicationMgr.Get().ScheduleCallback(0.5f, false, new ApplicationMgr.ScheduledCallback(this.BnetParty_OnLostPartyReference_RemoveKnownCreator), party.Id);
                }
            }
            if (evt == OnlineEventType.ADDED)
            {
                BnetGameAccountId id = this.GetPartyCreator(party.Id);
                if (id == null)
                {
                    object[] objArray2 = new object[] { party.Id };
                    this.LogInfoParty("SpectatorParty_OnJoined: joined party={0} without creator.", objArray2);
                    this.LeaveParty(party.Id, BnetParty.IsLeader(party.Id));
                }
                else
                {
                    if ((this.m_requestedInvite != null) && (this.m_requestedInvite.PartyId == party.Id))
                    {
                        this.m_requestedInvite = null;
                        ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.SpectatePlayer_RequestInvite_FriendlySide_Timeout), null);
                        ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.SpectatePlayer_RequestInvite_OpposingSide_Timeout), null);
                    }
                    bool flag6 = this.ShouldBePartyLeader(party.Id);
                    bool flag7 = this.m_spectatorPartyIdMain == null;
                    bool flag8 = flag7;
                    if (((this.m_spectatorPartyIdMain != null) && (this.m_spectatorPartyIdMain != party.Id)) && (flag6 || (id != this.m_spectateeOpposingSide)))
                    {
                        flag8 = true;
                        object[] objArray3 = new object[3];
                        objArray3[0] = party.Id;
                        objArray3[1] = this.m_spectatorPartyIdMain;
                        if (<>f__am$cache1E == null)
                        {
                            <>f__am$cache1E = i => i.ToString();
                        }
                        objArray3[2] = string.Join(", ", Enumerable.Select<PartyInfo, string>(BnetParty.GetJoinedParties(), <>f__am$cache1E).ToArray<string>());
                        this.LogInfoParty("SpectatorParty_OnJoined: joined party={0} when different current={1} (will be clobbered) joinedParties={2}", objArray3);
                    }
                    if (flag6)
                    {
                        this.m_spectatorPartyIdMain = party.Id;
                        if (flag8)
                        {
                            this.UpdateSpectatorPresence();
                        }
                        this.UpdateSpectatorPartyServerInfo();
                        this.ReinviteKnownSpectatorsNotInParty();
                        if (this.m_userInitiatedOutgoingInvites != null)
                        {
                            foreach (BnetGameAccountId id3 in this.m_userInitiatedOutgoingInvites)
                            {
                                BnetParty.SendInvite(this.m_spectatorPartyIdMain, id3);
                            }
                        }
                        if (flag7 && (this.OnSpectatorToMyGame != null))
                        {
                            foreach (PartyMember member2 in BnetParty.GetMembers(this.m_spectatorPartyIdMain))
                            {
                                if (member2.GameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId())
                                {
                                    ApplicationMgr.Get().StartCoroutine(this.WaitForPresenceThenToast(member2.GameAccountId, SocialToastMgr.TOAST_TYPE.SPECTATOR_ADDED));
                                    BnetPlayer spectator = BnetUtils.GetPlayer(member2.GameAccountId);
                                    this.OnSpectatorToMyGame(OnlineEventType.ADDED, spectator);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool flag9 = true;
                        if (this.m_spectateeFriendlySide == null)
                        {
                            this.m_spectateeFriendlySide = id;
                            this.m_spectatorPartyIdMain = party.Id;
                            flag9 = false;
                        }
                        else if (id == this.m_spectateeFriendlySide)
                        {
                            this.m_spectatorPartyIdMain = party.Id;
                        }
                        else if (id == this.m_spectateeOpposingSide)
                        {
                            this.m_spectatorPartyIdOpposingSide = party.Id;
                        }
                        if (BnetParty.GetPartyAttributeBlob(party.Id, "WTCG.Party.ServerInfo") != null)
                        {
                            object[] objArray4 = new object[] { party.Id };
                            this.LogInfoParty("SpectatorParty_OnJoined: joined party={0} as spectator, begin spectating game.", objArray4);
                            if (!flag9)
                            {
                                if (id == this.m_spectateeOpposingSide)
                                {
                                    this.LogInfoPower("================== Begin Spectating 2nd player ==================", new object[0]);
                                }
                                else
                                {
                                    this.LogInfoPower("================== Begin Spectating 1st player ==================", new object[0]);
                                }
                            }
                            this.JoinPartyGame(party.Id);
                        }
                        else
                        {
                            if (!SceneMgr.Get().IsInGame())
                            {
                                this.ShowWaitingForNextGameDialog();
                            }
                            BnetPlayer player = BnetUtils.GetPlayer(id);
                            this.FireSpectatorModeChanged(OnlineEventType.ADDED, player);
                        }
                    }
                }
            }
        }
    }

    private void BnetParty_OnLostPartyReference_RemoveKnownCreator(object userData)
    {
        <BnetParty_OnLostPartyReference_RemoveKnownCreator>c__AnonStorey381 storey = new <BnetParty_OnLostPartyReference_RemoveKnownCreator>c__AnonStorey381 {
            partyId = userData as PartyId
        };
        if (((storey.partyId != null) && !BnetParty.IsInParty(storey.partyId)) && !Enumerable.Any<PartyInvite>(BnetParty.GetReceivedInvites(), new Func<PartyInvite, bool>(storey.<>m__271)))
        {
            Get().m_knownPartyCreatorIds.Remove(storey.partyId);
        }
    }

    private void BnetParty_OnMemberEvent(OnlineEventType evt, PartyInfo party, BnetGameAccountId memberId, bool isRolesUpdate, LeaveReason? reason)
    {
        if ((party.Id != null) && ((party.Id == this.m_spectatorPartyIdMain) || (party.Id == this.m_spectatorPartyIdOpposingSide)))
        {
            if ((evt == OnlineEventType.ADDED) && BnetParty.IsLeader(party.Id))
            {
                BnetGameAccountId partyCreator = this.GetPartyCreator(party.Id);
                if ((partyCreator != null) && (partyCreator == memberId))
                {
                    BnetParty.SetLeader(party.Id, memberId);
                }
            }
            if (((this.m_initialized && (evt != OnlineEventType.UPDATED)) && ((memberId != BnetPresenceMgr.Get().GetMyGameAccountId()) && this.ShouldBePartyLeader(party.Id))) && ((!SceneMgr.Get().IsInGame() || !Network.IsConnectedToGameServer()) || !this.m_gameServerKnownSpectators.Contains(memberId)))
            {
                SocialToastMgr.TOAST_TYPE toastType = (evt != OnlineEventType.ADDED) ? SocialToastMgr.TOAST_TYPE.SPECTATOR_REMOVED : SocialToastMgr.TOAST_TYPE.SPECTATOR_ADDED;
                ApplicationMgr.Get().StartCoroutine(this.WaitForPresenceThenToast(memberId, toastType));
                if (this.OnSpectatorToMyGame != null)
                {
                    BnetPlayer spectator = BnetUtils.GetPlayer(memberId);
                    this.OnSpectatorToMyGame(evt, spectator);
                }
            }
        }
    }

    private void BnetParty_OnPartyAttributeChanged_ServerInfo(PartyInfo party, string attributeKey, bnet.protocol.attribute.Variant value)
    {
        if ((party.Type == PartyType.SPECTATOR_PARTY) && (value != null))
        {
            byte[] bytes = !value.HasBlobValue ? null : value.BlobValue;
            if (bytes != null)
            {
                PartyServerInfo a = ProtobufUtil.ParseFrom<PartyServerInfo>(bytes, 0, -1);
                if (a != null)
                {
                    if (!a.HasSecretKey || string.IsNullOrEmpty(a.SecretKey))
                    {
                        this.LogInfoParty("BnetParty_OnPartyAttributeChanged_ServerInfo: no secret key in serverInfo.", new object[0]);
                    }
                    else
                    {
                        BattleNet.GameServerInfo lastGameServerJoined = Network.Get().GetLastGameServerJoined();
                        bool flag = Network.IsConnectedToGameServer() && IsSameGameAndServer(a, lastGameServerJoined);
                        if (!flag && SceneMgr.Get().IsInGame())
                        {
                            object[] args = new object[] { a.GameHandle, lastGameServerJoined.GameHandle };
                            this.LogInfoParty("BnetParty_OnPartyAttributeChanged_ServerInfo: cannot join game while in gameplay new={0} curr={1}.", args);
                        }
                        else
                        {
                            JoinInfo joinInfo = CreateJoinInfo(a);
                            if (party.Id == this.m_spectatorPartyIdOpposingSide)
                            {
                                if ((GameMgr.Get().GetTransitionPopup() == null) && GameMgr.Get().IsSpectator())
                                {
                                    this.SpectateSecondPlayer_Network(joinInfo);
                                }
                            }
                            else if (!flag && (party.Id == this.m_spectatorPartyIdMain))
                            {
                                this.LogInfoPower("================== Start Spectator Game ==================", new object[0]);
                                this.m_isExpectingArriveInGameplayAsSpectator = true;
                                GameMgr.Get().SpectateGame(joinInfo);
                                this.CloseWaitingForNextGameDialog();
                            }
                        }
                    }
                }
            }
        }
    }

    private void BnetParty_OnReceivedInvite(OnlineEventType evt, PartyInfo party, ulong inviteId, InviteRemoveReason? reason)
    {
        if (this.m_initialized && (party.Type == PartyType.SPECTATOR_PARTY))
        {
            PartyInvite receivedInvite = BnetParty.GetReceivedInvite(inviteId);
            BnetPlayer inviter = (receivedInvite != null) ? BnetUtils.GetPlayer(receivedInvite.InviterId) : null;
            bool flag = true;
            if (evt == OnlineEventType.ADDED)
            {
                if (receivedInvite == null)
                {
                    return;
                }
                if (receivedInvite.IsRejoin || this.ShouldBePartyLeader(receivedInvite.PartyId))
                {
                    BnetGameAccountId partyCreator = this.GetPartyCreator(receivedInvite.PartyId);
                    object[] args = new object[] { receivedInvite.IsRejoin, receivedInvite.PartyId, partyCreator };
                    this.LogInfoParty("Spectator_OnReceivedInvite rejoin={0} partyId={1} creatorId={2}", args);
                    bool flag2 = false;
                    if (this.ShouldBePartyLeader(receivedInvite.PartyId))
                    {
                        BnetParty.AcceptReceivedInvite(inviteId);
                        flag = false;
                    }
                    else if (this.m_spectatorPartyIdMain != null)
                    {
                        if (this.m_spectatorPartyIdMain == receivedInvite.PartyId)
                        {
                            BnetParty.AcceptReceivedInvite(inviteId);
                            flag = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    else
                    {
                        flag2 = true;
                        if ((partyCreator != null) && (this.m_spectateeFriendlySide == null))
                        {
                            flag2 = false;
                            this.m_spectateeFriendlySide = partyCreator;
                            BnetParty.AcceptReceivedInvite(inviteId);
                            flag = false;
                        }
                    }
                    if (flag2)
                    {
                        BnetParty.DeclineReceivedInvite(inviteId);
                    }
                }
                else if (((receivedInvite.InviterId == this.m_spectateeFriendlySide) || (receivedInvite.InviterId == this.m_spectateeOpposingSide)) || ((this.m_requestedInvite != null) && (this.m_requestedInvite.PartyId == receivedInvite.PartyId)))
                {
                    BnetParty.AcceptReceivedInvite(inviteId);
                    flag = false;
                    if (this.m_requestedInvite != null)
                    {
                        this.m_requestedInvite = null;
                        ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.SpectatePlayer_RequestInvite_FriendlySide_Timeout), null);
                        ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.SpectatePlayer_RequestInvite_OpposingSide_Timeout), null);
                    }
                }
                else
                {
                    if (this.m_kickedFromSpectatingList != null)
                    {
                        this.m_kickedFromSpectatingList.Remove(receivedInvite.InviterId);
                    }
                    if (SocialToastMgr.Get() != null)
                    {
                        string inviterBestName = BnetUtils.GetInviterBestName(receivedInvite);
                        SocialToastMgr.Get().AddToast(inviterBestName, SocialToastMgr.TOAST_TYPE.SPECTATOR_INVITE_RECEIVED);
                    }
                }
            }
            else if (((evt == OnlineEventType.REMOVED) && (!reason.HasValue || (((InviteRemoveReason) reason.Value) == InviteRemoveReason.ACCEPTED))) && (ApplicationMgr.Get() != null))
            {
                ApplicationMgr.Get().ScheduleCallback(0.5f, false, new ApplicationMgr.ScheduledCallback(this.BnetParty_OnLostPartyReference_RemoveKnownCreator), party.Id);
            }
            if (flag && (this.OnInviteReceived != null))
            {
                this.OnInviteReceived(evt, inviter);
            }
        }
    }

    private void BnetParty_OnReceivedInviteRequest(OnlineEventType evt, PartyInfo party, InviteRequest request, InviteRequestRemovedReason? reason)
    {
        if ((party.Type == PartyType.SPECTATOR_PARTY) && (evt == OnlineEventType.ADDED))
        {
            bool flag = false;
            if (party.Id != this.m_spectatorPartyIdMain)
            {
                flag = true;
            }
            if (((request.RequesterId != null) && (request.RequesterId == request.TargetId)) && !Options.Get().GetBool(Option.SPECTATOR_OPEN_JOIN))
            {
                flag = true;
            }
            if (!BnetFriendMgr.Get().IsFriend(request.RequesterId))
            {
                flag = true;
            }
            if (!BnetFriendMgr.Get().IsFriend(request.TargetId))
            {
                flag = true;
            }
            if ((this.m_kickedPlayers != null) && (this.m_kickedPlayers.Contains(request.RequesterId) || this.m_kickedPlayers.Contains(request.TargetId)))
            {
                flag = true;
            }
            if (flag)
            {
                BnetParty.IgnoreInviteRequest(party.Id, request.TargetId);
            }
            else
            {
                BnetParty.AcceptInviteRequest(party.Id, request.TargetId);
            }
        }
    }

    private void BnetParty_OnSentInvite(OnlineEventType evt, PartyInfo party, ulong inviteId, bool senderIsMyself, InviteRemoveReason? reason)
    {
        if ((party.Type == PartyType.SPECTATOR_PARTY) && senderIsMyself)
        {
            PartyInvite sentInvite = BnetParty.GetSentInvite(party.Id, inviteId);
            BnetPlayer invitee = (sentInvite != null) ? BnetUtils.GetPlayer(sentInvite.InviteeId) : null;
            if (evt == OnlineEventType.ADDED)
            {
                bool flag = false;
                if ((this.m_userInitiatedOutgoingInvites != null) && (sentInvite != null))
                {
                    flag = this.m_userInitiatedOutgoingInvites.Remove(sentInvite.InviteeId);
                }
                if (((flag && (sentInvite != null)) && (this.ShouldBePartyLeader(party.Id) && !this.m_gameServerKnownSpectators.Contains(sentInvite.InviteeId))) && (SocialToastMgr.Get() != null))
                {
                    string playerBestName = BnetUtils.GetPlayerBestName(sentInvite.InviteeId);
                    SocialToastMgr.Get().AddToast(playerBestName, SocialToastMgr.TOAST_TYPE.SPECTATOR_INVITE_SENT);
                }
            }
            if (((sentInvite != null) && !this.m_gameServerKnownSpectators.Contains(sentInvite.InviteeId)) && (this.OnInviteSent != null))
            {
                this.OnInviteSent(evt, invitee);
            }
        }
    }

    public bool CanAddSpectators()
    {
        if ((GameMgr.Get() != null) && GameMgr.Get().IsSpectator())
        {
            return false;
        }
        if ((this.m_spectateeFriendlySide != null) || (this.m_spectateeOpposingSide != null))
        {
            return false;
        }
        int countSpectatingMe = this.GetCountSpectatingMe();
        if (!this.IsInSpectatableGame())
        {
            if (this.m_spectatorPartyIdMain == null)
            {
                return false;
            }
            if (countSpectatingMe <= 0)
            {
                return false;
            }
        }
        if (countSpectatingMe >= 10)
        {
            return false;
        }
        return true;
    }

    public bool CanInviteToSpectateMyGame(BnetGameAccountId gameAccountId)
    {
        if (!this.CanAddSpectators())
        {
            return false;
        }
        BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
        if (gameAccountId == myGameAccountId)
        {
            return false;
        }
        if (this.IsSpectatingMe(gameAccountId))
        {
            return false;
        }
        if (this.IsInvitedToSpectateMyGame(gameAccountId))
        {
            return false;
        }
        BnetGameAccount gameAccount = BnetPresenceMgr.Get().GetGameAccount(gameAccountId);
        if ((gameAccount == null) || !gameAccount.IsOnline())
        {
            return false;
        }
        if (!gameAccount.CanBeInvitedToGame() && !this.IsPlayerSpectatingMyGamesOpposingSide(gameAccountId))
        {
            return false;
        }
        if (ApplicationMgr.IsPublic())
        {
            BnetGameAccount hearthstoneGameAccount = BnetPresenceMgr.Get().GetMyPlayer().GetHearthstoneGameAccount();
            if (string.Compare(gameAccount.GetClientVersion(), hearthstoneGameAccount.GetClientVersion()) != 0)
            {
                return false;
            }
            if (string.Compare(gameAccount.GetClientEnv(), hearthstoneGameAccount.GetClientEnv()) != 0)
            {
                return false;
            }
        }
        if (!SceneMgr.Get().IsInGame())
        {
            return false;
        }
        return true;
    }

    public bool CanKickSpectator(BnetGameAccountId gameAccountId)
    {
        if (!this.IsSpectatingMe(gameAccountId))
        {
            return false;
        }
        return true;
    }

    public bool CanSpectate(BnetPlayer player)
    {
        BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
        if (player == myPlayer)
        {
            return false;
        }
        BnetGameAccountId hearthstoneGameAccountId = player.GetHearthstoneGameAccountId();
        if (this.IsSpectatingPlayer(hearthstoneGameAccountId))
        {
            return false;
        }
        if (this.m_spectateeOpposingSide != null)
        {
            return false;
        }
        if (this.HasPreviouslyKickedMeFromGame(hearthstoneGameAccountId) && !this.HasInvitedMeToSpectate(hearthstoneGameAccountId))
        {
            return false;
        }
        if (GameMgr.Get().IsFindingGame())
        {
            return false;
        }
        if (GameMgr.Get().IsNextSpectator())
        {
            return false;
        }
        if (!player.GetBestGameAccount().IsSpectatorSlotAvailable() && !this.HasInvitedMeToSpectate(hearthstoneGameAccountId))
        {
            return false;
        }
        if (GameMgr.Get().IsSpectator())
        {
            if (!this.IsPlayerInGame(hearthstoneGameAccountId))
            {
                return false;
            }
        }
        else if (SceneMgr.Get().IsInGame())
        {
            return false;
        }
        if (!GameUtils.AreAllTutorialsComplete())
        {
            return false;
        }
        if (ApplicationMgr.IsPublic())
        {
            BnetGameAccount hearthstoneGameAccount = player.GetHearthstoneGameAccount();
            BnetGameAccount account2 = myPlayer.GetHearthstoneGameAccount();
            if (string.Compare(hearthstoneGameAccount.GetClientVersion(), account2.GetClientVersion()) != 0)
            {
                return false;
            }
            if (string.Compare(hearthstoneGameAccount.GetClientEnv(), account2.GetClientEnv()) != 0)
            {
                return false;
            }
        }
        if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN)
        {
            return false;
        }
        return true;
    }

    private void ClearAllGameServerKnownSpectators()
    {
        BnetGameAccountId[] idArray = this.m_gameServerKnownSpectators.ToArray<BnetGameAccountId>();
        this.m_gameServerKnownSpectators.Clear();
        if (((this.OnSpectatorToMyGame != null) && SceneMgr.Get().IsInGame()) && Network.IsConnectedToGameServer())
        {
            foreach (BnetGameAccountId id in idArray)
            {
                BnetPlayer spectator = BnetUtils.GetPlayer(id);
                this.OnSpectatorToMyGame(OnlineEventType.REMOVED, spectator);
            }
        }
        if (idArray.Length > 0)
        {
            this.UpdateSpectatorPresence();
        }
    }

    private void ClearAllReceivedInvitations()
    {
        BnetGameAccountId[] idArray = this.m_receivedSpectateMeInvites.Keys.ToArray<BnetGameAccountId>();
        this.m_receivedSpectateMeInvites.Clear();
        if (this.OnInviteReceived != null)
        {
            foreach (BnetGameAccountId id in idArray)
            {
                BnetPlayer inviter = BnetUtils.GetPlayer(id);
                this.OnInviteReceived(OnlineEventType.REMOVED, inviter);
            }
        }
    }

    private void ClearAllSentInvitations()
    {
        BnetGameAccountId[] idArray = this.m_sentSpectateMeInvites.Keys.ToArray<BnetGameAccountId>();
        this.m_sentSpectateMeInvites.Clear();
        if (this.OnInviteSent != null)
        {
            foreach (BnetGameAccountId id in idArray)
            {
                BnetPlayer invitee = BnetUtils.GetPlayer(id);
                this.OnInviteSent(OnlineEventType.REMOVED, invitee);
            }
        }
    }

    private void CloseWaitingForNextGameDialog()
    {
        if (DISABLE_MENU_BUTTON_WHILE_WAITING != null)
        {
            BnetBar.Get().m_menuButton.SetEnabled(true);
        }
        if (DialogManager.Get() != null)
        {
            DialogManager.Get().RemoveUniquePopupRequestFromQueue("SPECTATOR_WAITING_FOR_NEXT_GAME");
        }
        if (this.m_waitingForNextGameDialog != null)
        {
            this.m_waitingForNextGameDialog.Hide();
            this.m_waitingForNextGameDialog = null;
        }
        ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(SpectatorManager.WaitingForNextGame_AutoLeaveSpectatorMode), null);
    }

    private static SpectatorManager CreateInstance()
    {
        s_instance = new SpectatorManager();
        ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(s_instance.OnFindGameEvent));
        SceneMgr.Get().RegisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(s_instance.OnSceneUnloaded));
        GameState.RegisterGameStateInitializedListener(new GameState.GameStateInitializedCallback(s_instance.GameState_InitializedEvent), null);
        Network.Get().SetSpectatorInviteReceivedHandler(new Network.SpectatorInviteReceivedHandler(s_instance.Network_OnSpectatorInviteReceived));
        Options.Get().RegisterChangedListener(Option.SPECTATOR_OPEN_JOIN, new Options.ChangedCallback(s_instance.OnSpectatorOpenJoinOptionChanged));
        BnetPresenceMgr.Get().OnGameAccountPresenceChange += new Action<BattleNet.PresenceUpdate[]>(s_instance.Presence_OnGameAccountPresenceChange);
        BnetFriendMgr.Get().AddChangeListener(new BnetFriendMgr.ChangeCallback(s_instance.BnetFriendMgr_OnFriendsChanged));
        EndGameScreen.OnTwoScoopsShown = (EndGameScreen.OnTwoScoopsShownHandler) Delegate.Combine(EndGameScreen.OnTwoScoopsShown, new EndGameScreen.OnTwoScoopsShownHandler(s_instance.EndGameScreen_OnTwoScoopsShown));
        Network.Get().RegisterNetHandler(SpectatorNotify.PacketID.ID, new Network.NetHandler(s_instance.Network_OnSpectatorNotifyEvent), null);
        BnetParty.OnError += new BnetParty.PartyErrorHandler(s_instance.BnetParty_OnError);
        BnetParty.OnJoined += new BnetParty.JoinedHandler(s_instance.BnetParty_OnJoined);
        BnetParty.OnReceivedInvite += new BnetParty.ReceivedInviteHandler(s_instance.BnetParty_OnReceivedInvite);
        BnetParty.OnSentInvite += new BnetParty.SentInviteHandler(s_instance.BnetParty_OnSentInvite);
        BnetParty.OnReceivedInviteRequest += new BnetParty.ReceivedInviteRequestHandler(s_instance.BnetParty_OnReceivedInviteRequest);
        BnetParty.OnMemberEvent += new BnetParty.MemberEventHandler(s_instance.BnetParty_OnMemberEvent);
        BnetParty.OnChatMessage += new BnetParty.ChatMessageHandler(s_instance.BnetParty_OnChatMessage);
        BnetParty.RegisterAttributeChangedHandler("WTCG.Party.ServerInfo", new BnetParty.PartyAttributeChangedHandler(s_instance.BnetParty_OnPartyAttributeChanged_ServerInfo));
        return s_instance;
    }

    private static JoinInfo CreateJoinInfo(PartyServerInfo serverInfo)
    {
        JoinInfo info = new JoinInfo {
            ServerIpAddress = serverInfo.ServerIpAddress,
            ServerPort = serverInfo.ServerPort,
            GameHandle = serverInfo.GameHandle,
            SecretKey = serverInfo.SecretKey
        };
        if (serverInfo.HasGameType)
        {
            info.GameType = serverInfo.GameType;
        }
        if (serverInfo.HasMissionId)
        {
            info.MissionId = serverInfo.MissionId;
        }
        return info;
    }

    private bool CreatePartyIfNecessary()
    {
        if (this.m_spectatorPartyIdMain != null)
        {
            if ((this.GetPartyCreator(this.m_spectatorPartyIdMain) != null) && !this.ShouldBePartyLeader(this.m_spectatorPartyIdMain))
            {
                return false;
            }
            PartyInfo[] joinedParties = BnetParty.GetJoinedParties();
            if (Enumerable.FirstOrDefault<PartyInfo>(joinedParties, (Func<PartyInfo, bool>) (i => ((i.Id == this.m_spectatorPartyIdMain) && (i.Type == PartyType.SPECTATOR_PARTY)))) == null)
            {
                object[] args = new object[2];
                args[0] = this.m_spectatorPartyIdMain;
                if (<>f__am$cache20 == null)
                {
                    <>f__am$cache20 = i => i.ToString();
                }
                args[1] = string.Join(", ", Enumerable.Select<PartyInfo, string>(joinedParties, <>f__am$cache20).ToArray<string>());
                this.LogInfoParty("CreatePartyIfNecessary stored PartyId={0} is not in joined party list: {1}", args);
                this.m_spectatorPartyIdMain = null;
                this.UpdateSpectatorPresence();
            }
            if (<>f__am$cache21 == null)
            {
                <>f__am$cache21 = i => i.Type == PartyType.SPECTATOR_PARTY;
            }
            PartyInfo info = Enumerable.FirstOrDefault<PartyInfo>(joinedParties, <>f__am$cache21);
            if ((info != null) && (this.m_spectatorPartyIdMain != info.Id))
            {
                object[] objArray2 = new object[] { this.m_spectatorPartyIdMain, info.Id };
                this.LogInfoParty("CreatePartyIfNecessary repairing mismatching PartyIds current={0} new={1}", objArray2);
                this.m_spectatorPartyIdMain = info.Id;
                this.UpdateSpectatorPresence();
            }
            if (this.m_spectatorPartyIdMain != null)
            {
                return false;
            }
        }
        if (this.GetCountSpectatingMe() <= 0)
        {
            return false;
        }
        BnetParty.CreateParty(PartyType.SPECTATOR_PARTY, PrivacyLevel.OPEN_INVITATION, null);
        return true;
    }

    private void DeclineAllReceivedInvitations()
    {
        foreach (PartyInvite invite in BnetParty.GetReceivedInvites())
        {
            if (invite.PartyType == PartyType.SPECTATOR_PARTY)
            {
                BnetParty.DeclineReceivedInvite(invite.InviteId);
            }
        }
    }

    private static void DisplayErrorDialog(string header, string body)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = header,
            m_text = body,
            m_responseDisplay = AlertPopup.ResponseDisplay.OK
        };
        DialogManager.Get().ShowPopup(info);
    }

    private void EndCurrentSpectatedGame(bool isLeavingGameplay)
    {
        if (isLeavingGameplay && this.IsInSpectatorMode())
        {
            SoundManager.Get().LoadAndPlay("SpectatorMode_Exit");
        }
        this.m_expectedDisconnectReason = null;
        this.m_isExpectingArriveInGameplayAsSpectator = false;
        this.ClearAllGameServerKnownSpectators();
        if ((ApplicationMgr.Get() != null) && !ApplicationMgr.Get().IsResetting())
        {
            this.UpdateSpectatorPresence();
        }
        if ((GameMgr.Get() != null) && (GameMgr.Get().GetTransitionPopup() != null))
        {
            GameMgr.Get().GetTransitionPopup().OnHidden -= new Action<TransitionPopup>(this.EnterSpectatorMode_OnTransitionPopupHide);
        }
    }

    private void EndGameScreen_OnTwoScoopsShown(bool shown, EndGameTwoScoop twoScoops)
    {
        if (this.IsInSpectatorMode())
        {
            if (shown)
            {
                ApplicationMgr.Get().ScheduleCallback(5f, false, new ApplicationMgr.ScheduledCallback(this.EndGameScreen_OnTwoScoopsShown_AutoClose), null);
            }
            else
            {
                ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.EndGameScreen_OnTwoScoopsShown_AutoClose), null);
            }
        }
    }

    private void EndGameScreen_OnTwoScoopsShown_AutoClose(object userData)
    {
        if (EndGameScreen.Get() != null)
        {
            if (WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS >= 0f)
            {
                while (EndGameScreen.Get().ContinueEvents())
                {
                }
            }
            else
            {
                EndGameScreen.Get().ContinueEvents();
            }
        }
    }

    private void EndSpectatorMode(bool wasKnownSpectating = false)
    {
        bool isExpectingArriveInGameplayAsSpectator = this.m_isExpectingArriveInGameplayAsSpectator;
        bool flag2 = (wasKnownSpectating || (this.m_spectateeFriendlySide != null)) || (this.m_spectateeOpposingSide != null);
        this.LeaveSpectatorMode();
        this.EndCurrentSpectatedGame(false);
        this.m_spectateeFriendlySide = null;
        this.m_spectateeOpposingSide = null;
        this.m_requestedInvite = null;
        this.CloseWaitingForNextGameDialog();
        this.m_pendingSpectatePlayerAfterLeave = null;
        this.m_isExpectingArriveInGameplayAsSpectator = false;
        if (flag2)
        {
            this.LogInfoPower("================== End Spectator Mode ==================", new object[0]);
            BnetPlayer spectatee = BnetUtils.GetPlayer(this.m_spectateeFriendlySide);
            this.FireSpectatorModeChanged(OnlineEventType.REMOVED, spectatee);
        }
        if (isExpectingArriveInGameplayAsSpectator)
        {
            SceneMgr.Mode hUB = SceneMgr.Mode.HUB;
            if (!GameUtils.AreAllTutorialsComplete())
            {
                Network.Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_LOST_GAME_CONNECTION", 0f);
            }
            else if (!SceneMgr.Get().IsModeRequested(hUB))
            {
                SceneMgr.Get().SetNextMode(hUB);
            }
            else if (SceneMgr.Get().GetMode() == hUB)
            {
                SceneMgr.Get().ReloadMode();
            }
        }
    }

    private void EnterSpectatorMode_OnTransitionPopupHide(TransitionPopup popup)
    {
        popup.OnHidden -= new Action<TransitionPopup>(this.EnterSpectatorMode_OnTransitionPopupHide);
        if (SoundManager.Get() != null)
        {
            SoundManager.Get().LoadAndPlay("SpectatorMode_Enter");
        }
        if (this.m_spectateeOpposingSide != null)
        {
            this.AutoSpectateOpposingSide();
        }
    }

    private void FireSpectatorModeChanged(OnlineEventType evt, BnetPlayer spectatee)
    {
        if (FriendChallengeMgr.Get() != null)
        {
            FriendChallengeMgr.Get().UpdateMyAvailability();
        }
        if (this.OnSpectatorModeChanged != null)
        {
            this.OnSpectatorModeChanged(evt, spectatee);
        }
        if (evt == OnlineEventType.ADDED)
        {
            Screen.sleepTimeout = -1;
        }
        else if ((evt == OnlineEventType.REMOVED) && (SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY))
        {
            Screen.sleepTimeout = -2;
        }
    }

    private void GameState_CreateGameEvent(GameState.CreateGamePhase createGamePhase, object userData)
    {
        if (createGamePhase >= GameState.CreateGamePhase.CREATED)
        {
            GameState.Get().UnregisterCreateGameListener(new GameState.CreateGameCallback(this.GameState_CreateGameEvent));
            if (this.m_spectatorPartyIdOpposingSide != null)
            {
                this.AutoSpectateOpposingSide();
            }
        }
    }

    private void GameState_InitializedEvent(GameState instance, object userData)
    {
        if (this.m_spectatorPartyIdOpposingSide != null)
        {
            GameState.Get().RegisterCreateGameListener(new GameState.CreateGameCallback(this.GameState_CreateGameEvent), null);
        }
    }

    public static SpectatorManager Get()
    {
        if (s_instance == null)
        {
            CreateInstance();
        }
        return s_instance;
    }

    public int GetCountSpectatingMe()
    {
        if ((this.m_spectatorPartyIdMain != null) && !this.ShouldBePartyLeader(this.m_spectatorPartyIdMain))
        {
            return 0;
        }
        int count = this.m_gameServerKnownSpectators.Count;
        return Mathf.Max(BnetParty.CountMembers(this.m_spectatorPartyIdMain) - 1, count);
    }

    private JoinInfo GetMyGameJoinInfo()
    {
        JoinInfo info = null;
        JoinInfo info2 = new JoinInfo();
        if (this.IsInSpectatorMode())
        {
            if (this.m_spectateeFriendlySide != null)
            {
                BnetId item = BnetEntityId.CreateForNet(this.m_spectateeFriendlySide);
                info2.SpectatedPlayers.Add(item);
            }
            if (this.m_spectateeOpposingSide != null)
            {
                BnetId id2 = BnetEntityId.CreateForNet(this.m_spectateeOpposingSide);
                info2.SpectatedPlayers.Add(id2);
            }
            if (info2.SpectatedPlayers.Count > 0)
            {
                info = info2;
            }
            return info;
        }
        int countSpectatingMe = this.GetCountSpectatingMe();
        if (this.CanAddSpectators())
        {
            BattleNet.GameServerInfo lastGameServerJoined = Network.Get().GetLastGameServerJoined();
            if (((this.m_spectatorPartyIdMain == null) && (lastGameServerJoined != null)) && (SceneMgr.Get().IsInGame() && !IsGameOver))
            {
                info2.ServerIpAddress = lastGameServerJoined.Address;
                info2.ServerPort = (uint) lastGameServerJoined.Port;
                info2.GameHandle = lastGameServerJoined.GameHandle;
                if (lastGameServerJoined.SpectatorPassword == null)
                {
                }
                info2.SecretKey = string.Empty;
            }
            if (this.m_spectatorPartyIdMain != null)
            {
                BnetId id3 = this.m_spectatorPartyIdMain.ToPegasusShared();
                info2.PartyId = id3;
            }
        }
        info2.CurrentNumSpectators = countSpectatingMe;
        info2.MaxNumSpectators = 10;
        info2.IsJoinable = info2.CurrentNumSpectators < info2.MaxNumSpectators;
        info2.GameType = GameMgr.Get().GetGameType();
        info2.MissionId = GameMgr.Get().GetMissionId();
        return info2;
    }

    private BnetGameAccountId GetPartyCreator(PartyId partyId)
    {
        if (partyId == null)
        {
            return null;
        }
        BnetGameAccountId id = null;
        if (!this.m_knownPartyCreatorIds.TryGetValue(partyId, out id) || (id == null))
        {
            byte[] partyAttributeBlob = BnetParty.GetPartyAttributeBlob(partyId, "WTCG.Party.Creator");
            if (partyAttributeBlob == null)
            {
                return null;
            }
            id = BnetGameAccountId.CreateFromNet(ProtobufUtil.ParseFrom<BnetId>(partyAttributeBlob, 0, -1));
            if (id.IsValid())
            {
                this.m_knownPartyCreatorIds[partyId] = id;
            }
        }
        return id;
    }

    private static PartyServerInfo GetPartyServerInfo(PartyId partyId)
    {
        byte[] partyAttributeBlob = BnetParty.GetPartyAttributeBlob(partyId, "WTCG.Party.ServerInfo");
        return ((partyAttributeBlob != null) ? ProtobufUtil.ParseFrom<PartyServerInfo>(partyAttributeBlob, 0, -1) : null);
    }

    public BnetGameAccountId GetSpectateeFriendlySide()
    {
        return this.m_spectateeFriendlySide;
    }

    public BnetGameAccountId GetSpectateeOpposingSide()
    {
        return this.m_spectateeOpposingSide;
    }

    public BnetGameAccountId[] GetSpectatorPartyMembers(bool friendlySide = true, bool includeSelf = false)
    {
        List<BnetGameAccountId> list = new List<BnetGameAccountId>(this.m_gameServerKnownSpectators);
        PartyMember[] members = BnetParty.GetMembers(!friendlySide ? this.m_spectatorPartyIdOpposingSide : this.m_spectatorPartyIdMain);
        BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
        foreach (PartyMember member in members)
        {
            if ((includeSelf || (member.GameAccountId != myGameAccountId)) && !list.Contains(member.GameAccountId))
            {
                list.Add(member.GameAccountId);
            }
        }
        return list.ToArray();
    }

    private string GetWaitingForNextGameDialogText()
    {
        BnetPlayer player = BnetUtils.GetPlayer(this.m_spectateeFriendlySide);
        string playerBestName = BnetUtils.GetPlayerBestName(this.m_spectateeFriendlySide);
        string str2 = ((player == null) || !player.IsOnline()) ? GameStrings.Get("GLOBAL_OFFLINE") : PresenceMgr.Get().GetStatusText(player);
        if (str2 != null)
        {
            str2 = str2.Trim();
        }
        string key = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_OFFLINE";
        if (((player != null) && player.IsOnline()) && !string.IsNullOrEmpty(str2))
        {
            key = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT";
            Enum[] statusEnums = PresenceMgr.Get().GetStatusEnums(player);
            if ((statusEnums.Length > 0) && (((PresenceStatus) statusEnums[0]) == PresenceStatus.ADVENTURE_SCENARIO_SELECT))
            {
                key = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_ENTERING";
            }
            else if ((statusEnums.Length > 0) && (((PresenceStatus) statusEnums[0]) == PresenceStatus.ADVENTURE_SCENARIO_PLAYING_GAME))
            {
                if ((statusEnums.Length > 1) && GameUtils.IsHeroicAdventureMission((int) ((ScenarioDbId) statusEnums[1])))
                {
                    key = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_BATTLING";
                }
                else if ((statusEnums.Length > 1) && GameUtils.IsClassChallengeMission((int) ((ScenarioDbId) statusEnums[1])))
                {
                    key = "GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TEXT_PLAYING";
                }
            }
        }
        object[] args = new object[] { playerBestName, str2 };
        return GameStrings.Format(key, args);
    }

    public bool HandleDisconnectFromGameplay()
    {
        bool hasValue = this.m_expectedDisconnectReason.HasValue;
        this.EndCurrentSpectatedGame(false);
        if (hasValue)
        {
            if (GameMgr.Get().IsTransitionPopupShown())
            {
                GameMgr.Get().GetTransitionPopup().Cancel();
                return hasValue;
            }
            this.LeaveGameScene();
        }
        return hasValue;
    }

    public bool HasAnyReceivedInvites()
    {
        if (<>f__am$cache1A == null)
        {
            <>f__am$cache1A = i => i.PartyType == PartyType.SPECTATOR_PARTY;
        }
        return (Enumerable.Where<PartyInvite>(BnetParty.GetReceivedInvites(), <>f__am$cache1A).ToArray<PartyInvite>().Length > 0);
    }

    public bool HasInvitedMeToSpectate(BnetGameAccountId gameAccountId)
    {
        return (BnetParty.GetReceivedInviteFrom(gameAccountId, PartyType.SPECTATOR_PARTY) != null);
    }

    public bool HasPreviouslyKickedMeFromGame(BnetGameAccountId playerId)
    {
        float num;
        if ((this.m_kickedFromSpectatingList != null) && this.m_kickedFromSpectatingList.TryGetValue(playerId, out num))
        {
            if ((UnityEngine.Time.realtimeSinceStartup >= num) && ((UnityEngine.Time.realtimeSinceStartup - num) < 10f))
            {
                return true;
            }
            this.m_kickedFromSpectatingList.Remove(playerId);
            if (this.m_kickedFromSpectatingList.Count == 0)
            {
                this.m_kickedFromSpectatingList = null;
            }
        }
        return false;
    }

    public void Initialize()
    {
        if (!this.m_initialized)
        {
            this.m_initialized = true;
            foreach (PartyInfo info in BnetParty.GetJoinedParties())
            {
                LeaveReason? reason = null;
                this.BnetParty_OnJoined(OnlineEventType.ADDED, info, reason);
            }
            foreach (PartyInvite invite in BnetParty.GetReceivedInvites())
            {
                InviteRemoveReason? nullable2 = null;
                this.BnetParty_OnReceivedInvite(OnlineEventType.ADDED, new PartyInfo(invite.PartyId, invite.PartyType), invite.InviteId, nullable2);
            }
        }
    }

    public void InviteToSpectateMe(BnetPlayer player)
    {
        if (player != null)
        {
            BnetGameAccountId hearthstoneGameAccountId = player.GetHearthstoneGameAccountId();
            if ((this.m_kickedPlayers != null) && this.m_kickedPlayers.Contains(hearthstoneGameAccountId))
            {
                this.m_kickedPlayers.Remove(hearthstoneGameAccountId);
            }
            if (this.CanInviteToSpectateMyGame(hearthstoneGameAccountId))
            {
                if (this.m_userInitiatedOutgoingInvites == null)
                {
                    this.m_userInitiatedOutgoingInvites = new HashSet<BnetGameAccountId>();
                }
                this.m_userInitiatedOutgoingInvites.Add(hearthstoneGameAccountId);
                if (this.m_spectatorPartyIdMain == null)
                {
                    BnetParty.CreateParty(PartyType.SPECTATOR_PARTY, PrivacyLevel.OPEN_INVITATION, null);
                }
                else
                {
                    BnetParty.SendInvite(this.m_spectatorPartyIdMain, hearthstoneGameAccountId);
                }
            }
        }
    }

    public bool IsBeingSpectated()
    {
        return (this.GetCountSpectatingMe() > 0);
    }

    public bool IsInSpectatableGame()
    {
        if (!SceneMgr.Get().IsInGame())
        {
            return false;
        }
        if (GameMgr.Get().IsSpectator())
        {
            return false;
        }
        if (IsGameOver)
        {
            return false;
        }
        return true;
    }

    public bool IsInSpectatorMode()
    {
        if ((GameMgr.Get() == null) || !GameMgr.Get().IsSpectator())
        {
            if (this.m_spectatorPartyIdMain == null)
            {
                return false;
            }
            if (!BnetParty.IsInParty(this.m_spectatorPartyIdMain))
            {
                return false;
            }
            if (!this.m_initialized)
            {
                return false;
            }
            if (this.GetPartyCreator(this.m_spectatorPartyIdMain) == null)
            {
                return false;
            }
            if (this.ShouldBePartyLeader(this.m_spectatorPartyIdMain))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsInvitedToSpectateMyGame(BnetGameAccountId gameAccountId)
    {
        <IsInvitedToSpectateMyGame>c__AnonStorey37F storeyf = new <IsInvitedToSpectateMyGame>c__AnonStorey37F {
            gameAccountId = gameAccountId
        };
        return (Enumerable.FirstOrDefault<PartyInvite>(BnetParty.GetSentInvites(this.m_spectatorPartyIdMain), new Func<PartyInvite, bool>(storeyf.<>m__269)) != null);
    }

    private bool IsPlayerInGame(BnetGameAccountId gameAccountId)
    {
        GameState state = GameState.Get();
        if (state != null)
        {
            foreach (KeyValuePair<int, Player> pair in state.GetPlayerMap())
            {
                BnetPlayer bnetPlayer = pair.Value.GetBnetPlayer();
                if ((bnetPlayer != null) && (bnetPlayer.GetHearthstoneGameAccountId() == gameAccountId))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsPlayerSpectatingMyGamesOpposingSide(BnetGameAccountId gameAccountId)
    {
        BnetGameAccount gameAccount = BnetPresenceMgr.Get().GetGameAccount(gameAccountId);
        if (gameAccount == null)
        {
            return false;
        }
        BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
        bool flag = false;
        if ((BnetFriendMgr.Get() != null) && BnetFriendMgr.Get().IsFriend(gameAccountId))
        {
            JoinInfo spectatorJoinInfo = gameAccount.GetSpectatorJoinInfo();
            Map<int, Player>.ValueCollection values = (GameState.Get() != null) ? GameState.Get().GetPlayerMap().Values : null;
            if (((spectatorJoinInfo == null) || (spectatorJoinInfo.SpectatedPlayers.Count <= 0)) || ((values == null) || (values.Count <= 0)))
            {
                return flag;
            }
            for (int i = 0; i < spectatorJoinInfo.SpectatedPlayers.Count; i++)
            {
                <IsPlayerSpectatingMyGamesOpposingSide>c__AnonStorey37E storeye = new <IsPlayerSpectatingMyGamesOpposingSide>c__AnonStorey37E {
                    spectatedPlayerId = BnetGameAccountId.CreateFromNet(spectatorJoinInfo.SpectatedPlayers[i])
                };
                if ((storeye.spectatedPlayerId != myGameAccountId) && Enumerable.Any<Player>(values, new Func<Player, bool>(storeye.<>m__268)))
                {
                    return true;
                }
            }
        }
        return flag;
    }

    private static bool IsSameGameAndServer(PartyServerInfo a, BattleNet.GameServerInfo b)
    {
        if (a == null)
        {
            return (b == null);
        }
        if (b == null)
        {
            return false;
        }
        return ((a.ServerIpAddress == b.Address) && (a.GameHandle == b.GameHandle));
    }

    private static bool IsSameGameAndServer(PartyServerInfo a, PartyServerInfo b)
    {
        if (a == null)
        {
            return (b == null);
        }
        if (b == null)
        {
            return false;
        }
        return ((a.ServerIpAddress == b.ServerIpAddress) && (a.GameHandle == b.GameHandle));
    }

    public bool IsSpectatingMe(BnetGameAccountId gameAccountId)
    {
        if (this.IsInSpectatorMode())
        {
            return false;
        }
        return (this.m_gameServerKnownSpectators.Contains(gameAccountId) || ((gameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId()) && BnetParty.IsMember(this.m_spectatorPartyIdMain, gameAccountId)));
    }

    public bool IsSpectatingOpposingSide()
    {
        return (this.m_spectateeOpposingSide != null);
    }

    public bool IsSpectatingPlayer(BnetGameAccountId gameAccountId)
    {
        return (((this.m_spectateeFriendlySide != null) && (this.m_spectateeFriendlySide == gameAccountId)) || ((this.m_spectateeOpposingSide != null) && (this.m_spectateeOpposingSide == gameAccountId)));
    }

    private bool IsStillInParty(PartyId partyId)
    {
        if (!BnetParty.IsInParty(partyId))
        {
            return false;
        }
        if ((this.m_leavePartyIdsRequested != null) && this.m_leavePartyIdsRequested.Contains(partyId))
        {
            return false;
        }
        return true;
    }

    private void JoinPartyGame(PartyId partyId)
    {
        if (partyId != null)
        {
            PartyInfo joinedParty = BnetParty.GetJoinedParty(partyId);
            if (joinedParty != null)
            {
                this.BnetParty_OnPartyAttributeChanged_ServerInfo(joinedParty, "WTCG.Party.ServerInfo", BnetParty.GetPartyAttributeVariant(partyId, "WTCG.Party.ServerInfo"));
            }
        }
    }

    public void KickSpectator(BnetPlayer player, bool regenerateSpectatorPassword)
    {
        this.KickSpectator_Internal(player, regenerateSpectatorPassword, true);
    }

    private void KickSpectator_Internal(BnetPlayer player, bool regenerateSpectatorPassword, bool addToKickList)
    {
        if (player != null)
        {
            BnetGameAccountId hearthstoneGameAccountId = player.GetHearthstoneGameAccountId();
            if (this.CanKickSpectator(hearthstoneGameAccountId))
            {
                if (addToKickList)
                {
                    if (this.m_kickedPlayers == null)
                    {
                        this.m_kickedPlayers = new HashSet<BnetGameAccountId>();
                    }
                    this.m_kickedPlayers.Add(hearthstoneGameAccountId);
                }
                if (Network.IsConnectedToGameServer())
                {
                    BnetGameAccountId[] bnetGameAccountIds = new BnetGameAccountId[] { hearthstoneGameAccountId };
                    Network.Get().SendRemoveSpectators(regenerateSpectatorPassword, bnetGameAccountIds);
                }
                if (((this.m_spectatorPartyIdMain != null) && this.ShouldBePartyLeader(this.m_spectatorPartyIdMain)) && BnetParty.IsMember(this.m_spectatorPartyIdMain, hearthstoneGameAccountId))
                {
                    BnetParty.KickMember(this.m_spectatorPartyIdMain, hearthstoneGameAccountId);
                }
            }
        }
    }

    private void LeaveGameScene()
    {
        if (EndGameScreen.Get() != null)
        {
            EndGameScreen.Get().m_hitbox.TriggerPress();
            EndGameScreen.Get().m_hitbox.TriggerRelease();
        }
        else
        {
            SceneMgr.Mode postGameSceneMode = GameMgr.Get().GetPostGameSceneMode();
            SceneMgr.Get().SetNextMode(postGameSceneMode);
        }
    }

    private void LeaveParty(PartyId partyId, bool dissolve)
    {
        if (partyId != null)
        {
            if (this.m_leavePartyIdsRequested == null)
            {
                this.m_leavePartyIdsRequested = new HashSet<PartyId>();
            }
            this.m_leavePartyIdsRequested.Add(partyId);
            if (dissolve)
            {
                BnetParty.DissolveParty(partyId);
            }
            else
            {
                BnetParty.Leave(partyId);
            }
        }
    }

    public void LeaveSpectatorMode()
    {
        if (GameMgr.Get().IsSpectator())
        {
            if (Network.IsConnectedToGameServer())
            {
                Network.DisconnectFromGameServer();
            }
            else
            {
                this.LeaveGameScene();
            }
        }
        if (this.m_spectatorPartyIdOpposingSide != null)
        {
            this.LeaveParty(this.m_spectatorPartyIdOpposingSide, false);
        }
        if (this.m_spectatorPartyIdMain != null)
        {
            this.LeaveParty(this.m_spectatorPartyIdMain, false);
        }
    }

    private void LogInfoParty(string format, params object[] args)
    {
        Log.Party.Print(format, args);
    }

    private void LogInfoPower(string format, params object[] args)
    {
        Log.Party.Print(format, args);
        Log.Power.Print(format, args);
    }

    public bool MyGameHasSpectators()
    {
        if (!SceneMgr.Get().IsInGame())
        {
            return false;
        }
        return (this.m_gameServerKnownSpectators.Count > 0);
    }

    private void Network_OnSpectatorInviteReceived(Invite protoInvite)
    {
        BnetGameAccountId inviterId = BnetGameAccountId.CreateFromNet(protoInvite.InviterGameAccountId);
        this.AddReceivedInvitation(inviterId, protoInvite.JoinInfo);
    }

    private void Network_OnSpectatorInviteReceived_ResponseCallback(AlertPopup.Response response, object userData)
    {
        BnetGameAccountId inviterId = (BnetGameAccountId) userData;
        if (response == AlertPopup.Response.CANCEL)
        {
            this.RemoveReceivedInvitation(inviterId);
        }
        else
        {
            BnetPlayer player = BnetUtils.GetPlayer(inviterId);
            if (player != null)
            {
                this.SpectatePlayer(player);
            }
        }
    }

    private void Network_OnSpectatorNotifyEvent()
    {
        SpectatorNotify spectatorNotify = Network.GetSpectatorNotify();
        if (spectatorNotify.HasSpectatorPasswordUpdate && !string.IsNullOrEmpty(spectatorNotify.SpectatorPasswordUpdate))
        {
            BattleNet.GameServerInfo lastGameServerJoined = Network.Get().GetLastGameServerJoined();
            if (!spectatorNotify.SpectatorPasswordUpdate.Equals(lastGameServerJoined.SpectatorPassword))
            {
                lastGameServerJoined.SpectatorPassword = spectatorNotify.SpectatorPasswordUpdate;
                this.UpdateMySpectatorInfo();
                this.RevokeAllSentInvitations();
            }
        }
        if (spectatorNotify.HasSpectatorRemoved)
        {
            this.m_expectedDisconnectReason = new int?(spectatorNotify.SpectatorRemoved.ReasonCode);
            bool flag = GameMgr.Get().IsTransitionPopupShown();
            if (spectatorNotify.SpectatorRemoved.ReasonCode == 0)
            {
                if (spectatorNotify.SpectatorRemoved.HasRemovedBy)
                {
                    if (this.m_kickedFromSpectatingList == null)
                    {
                        this.m_kickedFromSpectatingList = new Map<BnetGameAccountId, float>();
                    }
                    BnetGameAccountId id = BnetGameAccountId.CreateFromNet(spectatorNotify.SpectatorRemoved.RemovedBy);
                    this.m_kickedFromSpectatingList[id] = UnityEngine.Time.realtimeSinceStartup;
                }
                if (!this.m_isShowingRemovedAsSpectatorPopup)
                {
                    AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                        m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_HEADER"),
                        m_text = GameStrings.Get("GLOBAL_SPECTATOR_REMOVED_PROMPT_TEXT"),
                        m_responseDisplay = AlertPopup.ResponseDisplay.OK
                    };
                    if (flag)
                    {
                        info.m_responseCallback = new AlertPopup.ResponseCallback(this.Network_OnSpectatorNotifyEvent_Removed_GoToNextMode);
                    }
                    else
                    {
                        if (<>f__am$cache1B == null)
                        {
                            <>f__am$cache1B = (r, data) => Get().m_isShowingRemovedAsSpectatorPopup = false;
                        }
                        info.m_responseCallback = <>f__am$cache1B;
                    }
                    this.m_isShowingRemovedAsSpectatorPopup = true;
                    DialogManager.Get().ShowPopup(info);
                }
            }
            else if (flag)
            {
                this.Network_OnSpectatorNotifyEvent_Removed_GoToNextMode(AlertPopup.Response.OK, null);
            }
            SoundManager.Get().LoadAndPlay("SpectatorMode_Exit");
            this.EndSpectatorMode(true);
            this.m_expectedDisconnectReason = new int?(spectatorNotify.SpectatorRemoved.ReasonCode);
        }
        if (((spectatorNotify != null) && (spectatorNotify.SpectatorChange.Count != 0)) && ((GameMgr.Get() == null) || !GameMgr.Get().IsSpectator()))
        {
            foreach (SpectatorChange change in spectatorNotify.SpectatorChange)
            {
                BnetGameAccountId gameAccountId = BnetGameAccountId.CreateFromNet(change.GameAccountId);
                if (change.IsRemoved)
                {
                    this.RemoveKnownSpectator(gameAccountId);
                }
                else
                {
                    this.AddKnownSpectator(gameAccountId);
                    this.ReinviteKnownSpectatorsNotInParty();
                }
            }
        }
    }

    private void Network_OnSpectatorNotifyEvent_Removed_GoToNextMode(AlertPopup.Response response, object userData)
    {
        this.m_isShowingRemovedAsSpectatorPopup = false;
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        switch (eventData.m_state)
        {
            case FindGameState.CLIENT_CANCELED:
            case FindGameState.CLIENT_ERROR:
            case FindGameState.BNET_QUEUE_CANCELED:
            case FindGameState.BNET_ERROR:
                if (this.IsInSpectatorMode())
                {
                    this.EndSpectatorMode(true);
                }
                break;

            case FindGameState.SERVER_GAME_CANCELED:
                if (this.IsInSpectatorMode())
                {
                    string header = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_HEADER");
                    string body = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_TEXT");
                    DisplayErrorDialog(header, body);
                    this.EndSpectatorMode(true);
                }
                break;
        }
        return false;
    }

    public void OnRealTimeGameOver()
    {
        this.UpdateMySpectatorInfo();
    }

    private void OnSceneUnloaded(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        SceneMgr.Mode mode = SceneMgr.Get().GetMode();
        if ((mode == SceneMgr.Mode.GAMEPLAY) && (prevMode != SceneMgr.Mode.GAMEPLAY))
        {
            if (this.m_spectateeFriendlySide != null)
            {
                BnetBar.Get().HideFriendList();
            }
            if (GameMgr.Get().IsSpectator())
            {
                if (GameMgr.Get().GetTransitionPopup() != null)
                {
                    GameMgr.Get().GetTransitionPopup().OnHidden += new Action<TransitionPopup>(this.EnterSpectatorMode_OnTransitionPopupHide);
                }
                if (this.m_spectateeOpposingSide == null)
                {
                }
                BnetPlayer spectatee = BnetUtils.GetPlayer(this.m_spectateeFriendlySide);
                this.FireSpectatorModeChanged(OnlineEventType.ADDED, spectatee);
            }
            else
            {
                this.m_kickedPlayers = null;
            }
            this.CloseWaitingForNextGameDialog();
            this.DeclineAllReceivedInvitations();
            this.UpdateMySpectatorInfo();
        }
        else if ((prevMode == SceneMgr.Mode.GAMEPLAY) && (mode != SceneMgr.Mode.GAMEPLAY))
        {
            if (this.IsInSpectatorMode())
            {
                this.LogInfoPower("================== End Spectator Game ==================", new object[0]);
                if (SceneDebugger.Get() != null)
                {
                    UnityEngine.Time.timeScale = SceneDebugger.GetDevTimescale();
                }
                else
                {
                    UnityEngine.Time.timeScale = 1f;
                }
            }
            this.EndCurrentSpectatedGame(true);
            this.UpdateMySpectatorInfo();
            if (((this.m_spectatorPartyIdMain != null) && this.IsStillInParty(this.m_spectatorPartyIdMain)) && !this.ShouldBePartyLeader(this.m_spectatorPartyIdMain))
            {
                PartyServerInfo partyServerInfo = GetPartyServerInfo(this.m_spectatorPartyIdMain);
                if (partyServerInfo == null)
                {
                    this.ShowWaitingForNextGameDialog();
                }
                else
                {
                    BattleNet.GameServerInfo lastGameServerJoined = Network.Get().GetLastGameServerJoined();
                    if (!IsSameGameAndServer(partyServerInfo, lastGameServerJoined))
                    {
                        this.LogInfoPower("================== OnSceneUnloaded: auto-spectating game after leaving game ==================", new object[0]);
                        this.BnetParty_OnPartyAttributeChanged_ServerInfo(new PartyInfo(this.m_spectatorPartyIdMain, PartyType.SPECTATOR_PARTY), "WTCG.Party.ServerInfo", BnetParty.GetPartyAttributeVariant(this.m_spectatorPartyIdMain, "WTCG.Party.ServerInfo"));
                    }
                    else
                    {
                        this.ShowWaitingForNextGameDialog();
                    }
                }
            }
        }
    }

    private bool OnSceneUnloaded_AwaitingNextGame_DialogProcessCallback(DialogBase dialog, object userData)
    {
        if (SceneMgr.Get().IsInGame() || ((GameMgr.Get() != null) && GameMgr.Get().IsFindingGame()))
        {
            return false;
        }
        this.m_waitingForNextGameDialog = (AlertPopup) dialog;
        this.UpdateWaitingForNextGameDialog();
        if (DISABLE_MENU_BUTTON_WHILE_WAITING != null)
        {
            BnetBar.Get().m_menuButton.SetEnabled(false);
        }
        return true;
    }

    private void OnSceneUnloaded_AwaitingNextGame_LeaveSpectatorMode(AlertPopup.Response response, object userData)
    {
        this.LeaveSpectatorMode();
    }

    private void OnSpectatorOpenJoinOptionChanged(Option option, object prevValue, bool existed, object userData)
    {
        bool @bool = Options.Get().GetBool(Option.SPECTATOR_OPEN_JOIN);
        if ((((!existed || (((bool) prevValue) != @bool)) && (SceneMgr.Get() != null)) && SceneMgr.Get().IsInGame()) && ((GameMgr.Get() == null) || !GameMgr.Get().IsSpectator()))
        {
            JoinInfo myGameJoinInfo;
            if (@bool)
            {
                myGameJoinInfo = this.GetMyGameJoinInfo();
            }
            else
            {
                myGameJoinInfo = null;
            }
            if (Network.ShouldBeConnectedToAurora())
            {
                BnetPresenceMgr.Get().SetGameFieldBlob(0x15, myGameJoinInfo);
            }
        }
    }

    private void Presence_OnGameAccountPresenceChange(BattleNet.PresenceUpdate[] updates)
    {
        foreach (BattleNet.PresenceUpdate update in updates)
        {
            <Presence_OnGameAccountPresenceChange>c__AnonStorey380 storey = new <Presence_OnGameAccountPresenceChange>c__AnonStorey380 {
                entityId = BnetGameAccountId.CreateFromDll(update.entityId)
            };
            bool flag = (update.fieldId == 1) && (update.programId == BnetProgramId.BNET);
            bool flag2 = (update.programId == BnetProgramId.HEARTHSTONE) && (update.fieldId == 0x11);
            if ((((this.m_waitingForNextGameDialog != null) && (this.m_spectateeFriendlySide != null)) && (flag || flag2)) && (storey.entityId == this.m_spectateeFriendlySide))
            {
                this.UpdateWaitingForNextGameDialog();
            }
            if (flag && update.boolVal)
            {
                foreach (PartyId id in BnetParty.GetJoinedPartyIds())
                {
                    if (BnetParty.IsLeader(id) && !BnetParty.IsMember(id, storey.entityId))
                    {
                        BnetGameAccountId partyCreator = this.GetPartyCreator(id);
                        if (((partyCreator != null) && (partyCreator == storey.entityId)) && !Enumerable.Any<PartyInvite>(BnetParty.GetSentInvites(id), new Func<PartyInvite, bool>(storey.<>m__26D)))
                        {
                            BnetParty.SendInvite(id, storey.entityId);
                        }
                    }
                }
            }
        }
    }

    private void PruneOldInvites()
    {
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        List<BnetGameAccountId> list = new List<BnetGameAccountId>();
        foreach (KeyValuePair<BnetGameAccountId, ReceivedInvite> pair in this.m_receivedSpectateMeInvites)
        {
            float timestamp = pair.Value.m_timestamp;
            float num3 = realtimeSinceStartup - timestamp;
            if (num3 > 300f)
            {
                list.Add(pair.Key);
            }
        }
        foreach (BnetGameAccountId id in list)
        {
            this.RemoveReceivedInvitation(id);
        }
        list.Clear();
        foreach (KeyValuePair<BnetGameAccountId, float> pair2 in this.m_sentSpectateMeInvites)
        {
            float num4 = pair2.Value;
            float num5 = realtimeSinceStartup - num4;
            if (num5 > 30f)
            {
                list.Add(pair2.Key);
            }
        }
        foreach (BnetGameAccountId id2 in list)
        {
            this.RemoveSentInvitation(id2);
        }
    }

    private void ReceivedInvitation_ExpireTimeout(object userData)
    {
        this.PruneOldInvites();
        if (this.m_receivedSpectateMeInvites.Count > 0)
        {
            if (<>f__am$cache1C == null)
            {
                <>f__am$cache1C = kv => kv.Value.m_timestamp;
            }
            float num = Enumerable.Min<KeyValuePair<BnetGameAccountId, ReceivedInvite>>(this.m_receivedSpectateMeInvites, <>f__am$cache1C);
            float secondsToWait = Mathf.Max((float) 0f, (float) ((num + 300f) - UnityEngine.Time.realtimeSinceStartup));
            ApplicationMgr.Get().ScheduleCallback(secondsToWait, true, new ApplicationMgr.ScheduledCallback(this.ReceivedInvitation_ExpireTimeout), null);
        }
    }

    private void ReinviteKnownSpectatorsNotInParty()
    {
        if ((this.m_spectatorPartyIdMain != null) && this.ShouldBePartyLeader(this.m_spectatorPartyIdMain))
        {
            PartyMember[] members = BnetParty.GetMembers(this.m_spectatorPartyIdMain);
            <ReinviteKnownSpectatorsNotInParty>c__AnonStorey382 storey = new <ReinviteKnownSpectatorsNotInParty>c__AnonStorey382();
            using (HashSet<BnetGameAccountId>.Enumerator enumerator = this.m_gameServerKnownSpectators.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    storey.knownSpectator = enumerator.Current;
                    if (Enumerable.FirstOrDefault<PartyMember>(members, new Func<PartyMember, bool>(storey.<>m__276)) == null)
                    {
                        BnetParty.SendInvite(this.m_spectatorPartyIdMain, storey.knownSpectator);
                    }
                }
            }
        }
    }

    private void RemoveKnownSpectator(BnetGameAccountId gameAccountId)
    {
        if ((gameAccountId != null) && this.m_gameServerKnownSpectators.Remove(gameAccountId))
        {
            if (SceneMgr.Get().IsInGame() && Network.IsConnectedToGameServer())
            {
                bool flag2 = BnetParty.IsMember(this.m_spectatorPartyIdMain, gameAccountId);
                BnetPlayer spectator = BnetUtils.GetPlayer(gameAccountId);
                if (!flag2)
                {
                    ApplicationMgr.Get().StartCoroutine(this.WaitForPresenceThenToast(gameAccountId, SocialToastMgr.TOAST_TYPE.SPECTATOR_REMOVED));
                }
                if (this.OnSpectatorToMyGame != null)
                {
                    this.OnSpectatorToMyGame(OnlineEventType.REMOVED, spectator);
                }
            }
            this.UpdateSpectatorPresence();
        }
    }

    private void RemoveReceivedInvitation(BnetGameAccountId inviterId)
    {
        if ((inviterId != null) && this.m_receivedSpectateMeInvites.Remove(inviterId))
        {
            BnetPlayer inviter = BnetUtils.GetPlayer(inviterId);
            if (this.OnInviteReceived != null)
            {
                this.OnInviteReceived(OnlineEventType.REMOVED, inviter);
            }
        }
    }

    private void RemoveSentInvitation(BnetGameAccountId inviteeId)
    {
        if ((inviteeId != null) && this.m_sentSpectateMeInvites.Remove(inviteeId))
        {
            BnetPlayer invitee = BnetUtils.GetPlayer(inviteeId);
            if (this.OnInviteSent != null)
            {
                this.OnInviteSent(OnlineEventType.REMOVED, invitee);
            }
        }
    }

    private void ResetAllCache()
    {
        this.EndSpectatorMode(false);
        this.m_userInitiatedOutgoingInvites = null;
        this.m_kickedPlayers = null;
        this.m_isShowingRemovedAsSpectatorPopup = false;
    }

    private void RevokeAllSentInvitations()
    {
        this.ClearAllSentInvitations();
        BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
        PartyId[] idArray1 = new PartyId[] { this.m_spectatorPartyIdMain, this.m_spectatorPartyIdOpposingSide };
        foreach (PartyId id2 in idArray1)
        {
            if (id2 != null)
            {
                foreach (PartyInvite invite in BnetParty.GetSentInvites(id2))
                {
                    if (invite.InviterId == myGameAccountId)
                    {
                        BnetParty.RevokeSentInvite(id2, invite.InviteId);
                    }
                }
            }
        }
    }

    private bool ShouldBePartyLeader(PartyId partyId)
    {
        if (GameMgr.Get().IsSpectator())
        {
            return false;
        }
        if ((this.m_spectateeFriendlySide != null) || (this.m_spectateeOpposingSide != null))
        {
            return false;
        }
        BnetGameAccountId partyCreator = this.GetPartyCreator(partyId);
        if (partyCreator == null)
        {
            return false;
        }
        if (partyCreator != BnetPresenceMgr.Get().GetMyGameAccountId())
        {
            return false;
        }
        return true;
    }

    public bool ShouldBeSpectatingInGame()
    {
        if (this.m_spectatorPartyIdMain == null)
        {
            return false;
        }
        if (BnetParty.GetPartyAttributeBlob(this.m_spectatorPartyIdMain, "WTCG.Party.ServerInfo") == null)
        {
            return false;
        }
        return true;
    }

    public void ShowWaitingForNextGameDialog()
    {
        if (Network.IsLoggedIn())
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_id = "SPECTATOR_WAITING_FOR_NEXT_GAME",
                m_layerToUse = 13,
                m_headerText = GameStrings.Get("GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_HEADER"),
                m_text = this.GetWaitingForNextGameDialogText(),
                m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL,
                m_cancelText = GameStrings.Get("GLOBAL_LEAVE_SPECTATOR_MODE"),
                m_responseCallback = new AlertPopup.ResponseCallback(this.OnSceneUnloaded_AwaitingNextGame_LeaveSpectatorMode),
                m_keyboardEscIsCancel = false
            };
            DialogManager.Get().ShowUniquePopup(info, new DialogManager.DialogProcessCallback(this.OnSceneUnloaded_AwaitingNextGame_DialogProcessCallback));
            ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(SpectatorManager.WaitingForNextGame_AutoLeaveSpectatorMode), null);
            if (WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS >= 0f)
            {
                ApplicationMgr.Get().ScheduleCallback((float) WAITING_FOR_NEXT_GAME_AUTO_LEAVE_SECONDS, true, new ApplicationMgr.ScheduledCallback(SpectatorManager.WaitingForNextGame_AutoLeaveSpectatorMode), null);
            }
        }
    }

    public void SpectatePlayer(BnetPlayer player)
    {
        if ((this.m_pendingSpectatePlayerAfterLeave == null) && this.CanSpectate(player))
        {
            BnetGameAccountId hearthstoneGameAccountId = player.GetHearthstoneGameAccountId();
            JoinInfo joinInfo = null;
            PartyInvite receivedInviteFrom = BnetParty.GetReceivedInviteFrom(hearthstoneGameAccountId, PartyType.SPECTATOR_PARTY);
            if (receivedInviteFrom != null)
            {
                if ((this.m_spectateeFriendlySide == null) || ((this.m_spectateeOpposingSide == null) && this.IsPlayerInGame(hearthstoneGameAccountId)))
                {
                    this.CloseWaitingForNextGameDialog();
                    if ((this.m_spectateeFriendlySide != null) && (hearthstoneGameAccountId != this.m_spectateeFriendlySide))
                    {
                        this.m_spectateeOpposingSide = hearthstoneGameAccountId;
                    }
                    BnetParty.AcceptReceivedInvite(receivedInviteFrom.InviteId);
                }
                else
                {
                    object[] args = new object[5];
                    object[] objArray2 = new object[] { hearthstoneGameAccountId, " (", BnetUtils.GetPlayerBestName(hearthstoneGameAccountId), ")" };
                    args[0] = string.Concat(objArray2);
                    args[1] = this.m_spectateeFriendlySide;
                    args[2] = this.m_spectateeOpposingSide;
                    args[3] = this.IsPlayerInGame(hearthstoneGameAccountId);
                    args[4] = receivedInviteFrom.InviteId;
                    this.LogInfoParty("SpectatePlayer: trying to accept an invite even though there is no room for another spectatee: player={0} spectatee1={1} spectatee2={2} isPlayerInGame={3} inviteId={4}", args);
                    BnetParty.DeclineReceivedInvite(receivedInviteFrom.InviteId);
                }
            }
            else
            {
                joinInfo = player.GetHearthstoneGameAccount().GetSpectatorJoinInfo();
                if (joinInfo == null)
                {
                    Error.AddWarningLoc("Bad Spectator Key", "Spectator key is blank!", new object[0]);
                }
                else if (!joinInfo.HasPartyId && string.IsNullOrEmpty(joinInfo.SecretKey))
                {
                    Error.AddWarningLoc("No Party/Bad Spectator Key", "No party information and Spectator key is blank!", new object[0]);
                }
                else if (joinInfo.HasPartyId && (this.m_requestedInvite != null))
                {
                    object[] objArray3 = new object[] { this.m_requestedInvite.SpectateeId, this.m_requestedInvite.PartyId, hearthstoneGameAccountId, PartyId.FromProtocol(joinInfo.PartyId) };
                    this.LogInfoParty("SpectatePlayer: already requesting invite from {0}:party={1}, cannot request another from {2}:party={3}", objArray3);
                }
                else
                {
                    ShownUIMgr mgr = ShownUIMgr.Get();
                    if (mgr != null)
                    {
                        switch (mgr.GetShownUI())
                        {
                            case ShownUIMgr.UI_WINDOW.GENERAL_STORE:
                                if (GeneralStore.Get() != null)
                                {
                                    GeneralStore.Get().Close(false);
                                }
                                break;

                            case ShownUIMgr.UI_WINDOW.QUEST_LOG:
                                if (QuestLog.Get() != null)
                                {
                                    QuestLog.Get().Hide();
                                }
                                break;
                        }
                    }
                    if ((((this.m_spectateeFriendlySide == null) || (this.m_spectateeOpposingSide != null)) || (GameMgr.Get() == null)) || !GameMgr.Get().IsSpectator())
                    {
                        if (this.m_spectatorPartyIdMain != null)
                        {
                            if (this.IsInSpectatorMode())
                            {
                                this.EndSpectatorMode(true);
                            }
                            else
                            {
                                this.LeaveParty(this.m_spectatorPartyIdMain, this.ShouldBePartyLeader(this.m_spectatorPartyIdMain));
                            }
                            this.m_pendingSpectatePlayerAfterLeave = new PendingSpectatePlayer(hearthstoneGameAccountId, joinInfo);
                            return;
                        }
                        if (this.m_spectatorPartyIdOpposingSide != null)
                        {
                            this.m_pendingSpectatePlayerAfterLeave = new PendingSpectatePlayer(hearthstoneGameAccountId, joinInfo);
                            this.LeaveParty(this.m_spectatorPartyIdOpposingSide, false);
                            return;
                        }
                    }
                    this.SpectatePlayer_Internal(hearthstoneGameAccountId, joinInfo);
                }
            }
        }
    }

    private void SpectatePlayer_Internal(BnetGameAccountId gameAccountId, JoinInfo joinInfo)
    {
        if (!this.m_initialized)
        {
            object[] args = new object[] { gameAccountId };
            this.LogInfoParty("ERROR: SpectatePlayer_Internal called before initialized; spectatee={0}", args);
        }
        this.m_pendingSpectatePlayerAfterLeave = null;
        if (WelcomeQuests.Get() != null)
        {
            WelcomeQuests.Hide();
        }
        PartyInvite receivedInviteFrom = BnetParty.GetReceivedInviteFrom(gameAccountId, PartyType.SPECTATOR_PARTY);
        if (this.m_spectateeFriendlySide == null)
        {
            this.LogInfoPower("================== Begin Spectating 1st player ==================", new object[0]);
            this.m_spectateeFriendlySide = gameAccountId;
            if (receivedInviteFrom != null)
            {
                this.CloseWaitingForNextGameDialog();
                BnetParty.AcceptReceivedInvite(receivedInviteFrom.InviteId);
            }
            else if (joinInfo.HasPartyId)
            {
                PartyId partyId = PartyId.FromProtocol(joinInfo.PartyId);
                this.m_requestedInvite = new IntendedSpectateeParty(gameAccountId, partyId);
                BnetGameAccountId myGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
                BnetParty.RequestInvite(partyId, gameAccountId, myGameAccountId, PartyType.SPECTATOR_PARTY);
                ApplicationMgr.Get().ScheduleCallback(5f, true, new ApplicationMgr.ScheduledCallback(this.SpectatePlayer_RequestInvite_FriendlySide_Timeout), null);
            }
            else
            {
                this.CloseWaitingForNextGameDialog();
                this.m_isExpectingArriveInGameplayAsSpectator = true;
                GameMgr.Get().SpectateGame(joinInfo);
            }
        }
        else if (this.m_spectateeOpposingSide == null)
        {
            if (!this.IsPlayerInGame(gameAccountId))
            {
                Error.AddWarning("Error", "Cannot spectate two different games at same time.", new object[0]);
            }
            else if (this.m_spectateeFriendlySide == gameAccountId)
            {
                object[] objArray2 = new object[] { gameAccountId };
                this.LogInfoParty("SpectatePlayer: already spectating player {0}", objArray2);
                if (receivedInviteFrom != null)
                {
                    BnetParty.AcceptReceivedInvite(receivedInviteFrom.InviteId);
                }
            }
            else
            {
                this.LogInfoPower("================== Begin Spectating 2nd player ==================", new object[0]);
                this.m_spectateeOpposingSide = gameAccountId;
                if (receivedInviteFrom != null)
                {
                    BnetParty.AcceptReceivedInvite(receivedInviteFrom.InviteId);
                }
                else if (joinInfo.HasPartyId)
                {
                    PartyId id3 = PartyId.FromProtocol(joinInfo.PartyId);
                    this.m_requestedInvite = new IntendedSpectateeParty(gameAccountId, id3);
                    BnetGameAccountId whomToInvite = BnetPresenceMgr.Get().GetMyGameAccountId();
                    BnetParty.RequestInvite(id3, gameAccountId, whomToInvite, PartyType.SPECTATOR_PARTY);
                    ApplicationMgr.Get().ScheduleCallback(5f, true, new ApplicationMgr.ScheduledCallback(this.SpectatePlayer_RequestInvite_OpposingSide_Timeout), null);
                }
                else
                {
                    this.SpectateSecondPlayer_Network(joinInfo);
                }
            }
        }
        else if ((this.m_spectateeFriendlySide == gameAccountId) || (this.m_spectateeOpposingSide == gameAccountId))
        {
            object[] objArray3 = new object[] { gameAccountId };
            this.LogInfoParty("SpectatePlayer: already spectating player {0}", objArray3);
        }
        else
        {
            Error.AddDevFatal("Cannot spectate more than 2 players.", new object[0]);
        }
    }

    private void SpectatePlayer_RequestInvite_FriendlySide_Timeout(object userData)
    {
        if (this.m_requestedInvite != null)
        {
            this.m_spectateeFriendlySide = null;
            this.EndSpectatorMode(true);
            string header = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_HEADER");
            string body = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_TEXT");
            DisplayErrorDialog(header, body);
        }
    }

    private void SpectatePlayer_RequestInvite_OpposingSide_Timeout(object userData)
    {
        if (this.m_requestedInvite != null)
        {
            this.m_requestedInvite = null;
            this.m_spectateeOpposingSide = null;
            string header = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_HEADER");
            string body = GameStrings.Get("GLOBAL_SPECTATOR_SERVER_REJECTED_TEXT");
            DisplayErrorDialog(header, body);
        }
    }

    private void SpectateSecondPlayer_Network(JoinInfo joinInfo)
    {
        BattleNet.GameServerInfo info = new BattleNet.GameServerInfo {
            Address = joinInfo.ServerIpAddress,
            Port = (int) joinInfo.ServerPort,
            GameHandle = joinInfo.GameHandle,
            SpectatorPassword = joinInfo.SecretKey,
            SpectatorMode = true
        };
        Network.Get().SpectateSecondPlayer(info);
    }

    private void SpectatorManager_UpdatePresenceNextFrame(object userData)
    {
        JoinInfo protoMessage = null;
        if (Options.Get().GetBool(Option.SPECTATOR_OPEN_JOIN) || this.IsInSpectatorMode())
        {
            protoMessage = this.GetMyGameJoinInfo();
        }
        if (Network.ShouldBeConnectedToAurora())
        {
            BnetPresenceMgr.Get().SetGameFieldBlob(0x15, protoMessage);
        }
    }

    public void UpdateMySpectatorInfo()
    {
        this.UpdateSpectatorPresence();
        this.UpdateSpectatorPartyServerInfo();
    }

    private void UpdateSpectatorPartyServerInfo()
    {
        if (this.m_spectatorPartyIdMain != null)
        {
            if (!this.ShouldBePartyLeader(this.m_spectatorPartyIdMain))
            {
                if (BnetParty.IsLeader(this.m_spectatorPartyIdMain))
                {
                    BnetParty.ClearPartyAttribute(this.m_spectatorPartyIdMain, "WTCG.Party.ServerInfo");
                }
            }
            else
            {
                byte[] partyAttributeBlob = BnetParty.GetPartyAttributeBlob(this.m_spectatorPartyIdMain, "WTCG.Party.ServerInfo");
                BattleNet.GameServerInfo lastGameServerJoined = Network.Get().GetLastGameServerJoined();
                if ((IsGameOver || !SceneMgr.Get().IsInGame()) || ((!Network.IsConnectedToGameServer() || (lastGameServerJoined == null)) || string.IsNullOrEmpty(lastGameServerJoined.Address)))
                {
                    if (partyAttributeBlob != null)
                    {
                        BnetParty.ClearPartyAttribute(this.m_spectatorPartyIdMain, "WTCG.Party.ServerInfo");
                    }
                }
                else
                {
                    PartyServerInfo protobuf = new PartyServerInfo {
                        ServerIpAddress = lastGameServerJoined.Address,
                        ServerPort = (uint) lastGameServerJoined.Port,
                        GameHandle = lastGameServerJoined.GameHandle
                    };
                    if (lastGameServerJoined.SpectatorPassword == null)
                    {
                    }
                    protobuf.SecretKey = string.Empty;
                    protobuf.GameType = GameMgr.Get().GetGameType();
                    protobuf.MissionId = GameMgr.Get().GetMissionId();
                    byte[] buffer2 = ProtobufUtil.ToByteArray(protobuf);
                    if (!GeneralUtils.AreArraysEqual<byte>(buffer2, partyAttributeBlob))
                    {
                        BnetParty.SetPartyAttributeBlob(this.m_spectatorPartyIdMain, "WTCG.Party.ServerInfo", buffer2);
                    }
                }
            }
        }
    }

    private void UpdateSpectatorPresence()
    {
        if (ApplicationMgr.Get() != null)
        {
            ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.SpectatorManager_UpdatePresenceNextFrame), null);
            ApplicationMgr.Get().ScheduleCallback(0f, true, new ApplicationMgr.ScheduledCallback(this.SpectatorManager_UpdatePresenceNextFrame), null);
        }
        else
        {
            this.SpectatorManager_UpdatePresenceNextFrame(null);
        }
    }

    private void UpdateWaitingForNextGameDialog()
    {
        if (this.m_waitingForNextGameDialog != null)
        {
            string waitingForNextGameDialogText = this.GetWaitingForNextGameDialogText();
            this.m_waitingForNextGameDialog.BodyText = waitingForNextGameDialogText;
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitForPresenceThenToast(BnetGameAccountId gameAccountId, SocialToastMgr.TOAST_TYPE toastType)
    {
        return new <WaitForPresenceThenToast>c__Iterator263 { gameAccountId = gameAccountId, toastType = toastType, <$>gameAccountId = gameAccountId, <$>toastType = toastType };
    }

    private static void WaitingForNextGame_AutoLeaveSpectatorMode(object userData)
    {
        if (Get().IsInSpectatorMode() && !SceneMgr.Get().IsInGame())
        {
            Get().LeaveSpectatorMode();
            string header = GameStrings.Get("GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_HEADER");
            string body = GameStrings.Format("GLOBAL_SPECTATOR_WAITING_FOR_NEXT_GAME_TIMEOUT", new object[0]);
            DisplayErrorDialog(header, body);
        }
    }

    private void WillReset()
    {
        this.ResetAllCache();
        if (ApplicationMgr.Get() != null)
        {
            ApplicationMgr.Get().CancelScheduledCallback(new ApplicationMgr.ScheduledCallback(this.SpectatorManager_UpdatePresenceNextFrame), null);
        }
    }

    private static bool IsGameOver
    {
        get
        {
            if (GameState.Get() == null)
            {
                return false;
            }
            return GameState.Get().IsGameOverNowOrPending();
        }
    }

    [CompilerGenerated]
    private sealed class <BnetParty_OnLostPartyReference_RemoveKnownCreator>c__AnonStorey381
    {
        internal PartyId partyId;

        internal bool <>m__271(PartyInvite i)
        {
            return (i.PartyId == this.partyId);
        }
    }

    [CompilerGenerated]
    private sealed class <IsInvitedToSpectateMyGame>c__AnonStorey37F
    {
        internal BnetGameAccountId gameAccountId;

        internal bool <>m__269(PartyInvite i)
        {
            return (i.InviteeId == this.gameAccountId);
        }
    }

    [CompilerGenerated]
    private sealed class <IsPlayerSpectatingMyGamesOpposingSide>c__AnonStorey37E
    {
        internal BnetGameAccountId spectatedPlayerId;

        internal bool <>m__268(Player p)
        {
            return (p.GetGameAccountId() == this.spectatedPlayerId);
        }
    }

    [CompilerGenerated]
    private sealed class <Presence_OnGameAccountPresenceChange>c__AnonStorey380
    {
        internal BnetGameAccountId entityId;

        internal bool <>m__26D(PartyInvite i)
        {
            return (i.InviteeId == this.entityId);
        }
    }

    [CompilerGenerated]
    private sealed class <ReinviteKnownSpectatorsNotInParty>c__AnonStorey382
    {
        internal BnetGameAccountId knownSpectator;

        internal bool <>m__276(PartyMember m)
        {
            return (m.GameAccountId == this.knownSpectator);
        }
    }

    [CompilerGenerated]
    private sealed class <WaitForPresenceThenToast>c__Iterator263 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal BnetGameAccountId <$>gameAccountId;
        internal SocialToastMgr.TOAST_TYPE <$>toastType;
        internal string <playerName>__2;
        internal float <timeElapsed>__1;
        internal float <timeStarted>__0;
        internal BnetGameAccountId gameAccountId;
        internal SocialToastMgr.TOAST_TYPE toastType;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<timeStarted>__0 = UnityEngine.Time.time;
                    this.<timeElapsed>__1 = UnityEngine.Time.time - this.<timeStarted>__0;
                    break;

                case 1:
                    this.<timeElapsed>__1 = UnityEngine.Time.time - this.<timeStarted>__0;
                    break;

                default:
                    goto Label_00C6;
            }
            if ((this.<timeElapsed>__1 < 30f) && !BnetUtils.HasPlayerBestNamePresence(this.gameAccountId))
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            if (SocialToastMgr.Get() != null)
            {
                this.<playerName>__2 = BnetUtils.GetPlayerBestName(this.gameAccountId);
                SocialToastMgr.Get().AddToast(this.<playerName>__2, this.toastType);
            }
            this.$PC = -1;
        Label_00C6:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    private class IntendedSpectateeParty
    {
        public PartyId PartyId;
        public BnetGameAccountId SpectateeId;

        public IntendedSpectateeParty(BnetGameAccountId spectateeId, PartyId partyId)
        {
            this.SpectateeId = spectateeId;
            this.PartyId = partyId;
        }
    }

    public delegate void InviteReceivedHandler(OnlineEventType evt, BnetPlayer inviter);

    public delegate void InviteSentHandler(OnlineEventType evt, BnetPlayer invitee);

    private class PendingSpectatePlayer
    {
        public SpectatorProto.JoinInfo JoinInfo;
        public BnetGameAccountId SpectateeId;

        public PendingSpectatePlayer(BnetGameAccountId spectateeId, SpectatorProto.JoinInfo joinInfo)
        {
            this.SpectateeId = spectateeId;
            this.JoinInfo = joinInfo;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ReceivedInvite
    {
        public float m_timestamp;
        public JoinInfo m_joinInfo;
        public ReceivedInvite(JoinInfo joinInfo)
        {
            this.m_timestamp = UnityEngine.Time.realtimeSinceStartup;
            this.m_joinInfo = joinInfo;
        }
    }

    public delegate void SpectatorModeChangedHandler(OnlineEventType evt, BnetPlayer spectatee);

    public delegate void SpectatorToMyGameHandler(OnlineEventType evt, BnetPlayer spectator);
}

