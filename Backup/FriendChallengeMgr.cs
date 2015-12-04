using bnet.protocol.attribute;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class FriendChallengeMgr
{
    public const string ATTRIBUTE_CB = "cb";
    public const string ATTRIBUTE_DECK_PLAYER1 = "d1";
    public const string ATTRIBUTE_DECK_PLAYER2 = "d2";
    public const string ATTRIBUTE_DLL = "dll";
    public const string ATTRIBUTE_ERROR = "error";
    public const string ATTRIBUTE_LEFT = "left";
    public const string ATTRIBUTE_PARM = "parm";
    public const string ATTRIBUTE_STATE_PLAYER1 = "s1";
    public const string ATTRIBUTE_STATE_PLAYER2 = "s2";
    public const string ATTRIBUTE_VALUE_STATE_DECK = "deck";
    public const string ATTRIBUTE_VALUE_STATE_GAME = "game";
    public const string ATTRIBUTE_VALUE_STATE_GOTO = "goto";
    public const string ATTRIBUTE_VALUE_STATE_WAIT = "wait";
    private const int DEFAULT_SCENARIO_ID = 2;
    private DialogBase m_challengeDialog;
    private BnetPlayer m_challengee;
    private bool m_challengeeAccepted;
    private bool m_challengeeDeckSelected;
    private BnetEntityId m_challengeePartyId;
    private BnetPlayer m_challenger;
    private bool m_challengerDeckSelected;
    private BnetGameAccountId m_challengerId;
    private bool m_challengerPending;
    private List<ChangedListener> m_changedListeners = new List<ChangedListener>();
    private bool m_hasSeenDeclinedReason;
    private bool m_isChallengeTavernBrawl;
    private bool m_myPlayerReady;
    private bool m_netCacheReady;
    private BnetEntityId m_partyId;
    private int m_scenarioId = 2;
    private static FriendChallengeMgr s_instance;
    private const float SPECTATE_GAME_SERVER_SETUP_TIMEOUT_SECONDS = 15f;

    public void AcceptChallenge()
    {
        if (this.DidReceiveChallenge())
        {
            this.m_challengeeAccepted = true;
            Network.AcceptFriendChallenge(this.m_partyId);
            this.FireChangedEvent(FriendChallengeEvent.I_ACCEPTED_CHALLENGE, this.m_challenger);
        }
    }

    public bool AddChangedListener(ChangedCallback callback)
    {
        return this.AddChangedListener(callback, null);
    }

    public bool AddChangedListener(ChangedCallback callback, object userData)
    {
        ChangedListener item = new ChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_changedListeners.Contains(item))
        {
            return false;
        }
        this.m_changedListeners.Add(item);
        return true;
    }

    public bool AmIAvailable()
    {
        if (!this.m_netCacheReady)
        {
            return false;
        }
        if (!this.m_myPlayerReady)
        {
            return false;
        }
        if (SpectatorManager.Get().IsInSpectatorMode())
        {
            return false;
        }
        BnetGameAccount hearthstoneGameAccount = BnetPresenceMgr.Get().GetMyPlayer().GetHearthstoneGameAccount();
        if (hearthstoneGameAccount == null)
        {
            return false;
        }
        return hearthstoneGameAccount.CanBeInvitedToGame();
    }

    private void BnetParty_OnJoined(OnlineEventType evt, PartyInfo party, LeaveReason? reason)
    {
        if ((party.Type == PartyType.FRIENDLY_CHALLENGE) && (evt == OnlineEventType.ADDED))
        {
            long? partyAttributeLong = BnetParty.GetPartyAttributeLong(party.Id, "WTCG.Game.ScenarioId");
            if (partyAttributeLong.HasValue)
            {
                this.m_scenarioId = (int) partyAttributeLong.Value;
                TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
                this.m_isChallengeTavernBrawl = (mission != null) && (this.m_scenarioId == mission.missionId);
            }
        }
    }

    private void BnetParty_OnPartyAttributeChanged_DeclineReason(PartyInfo party, string attributeKey, bnet.protocol.attribute.Variant value)
    {
        if (((party.Type == PartyType.FRIENDLY_CHALLENGE) && this.DidSendChallenge()) && value.HasIntValue)
        {
            DeclineReason intValue = (DeclineReason) ((int) value.IntValue);
            string key = null;
            switch (intValue)
            {
                case DeclineReason.NoValidDeck:
                    key = "GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_RECIPIENT_NO_VALID_DECK_SENDER";
                    break;

                case DeclineReason.NotUnlocked:
                    key = "GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_RECIPIENT_NOT_UNLOCKED_SENDER";
                    break;
            }
            if (key != null)
            {
                this.m_hasSeenDeclinedReason = true;
                AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                    m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_HEADER"),
                    m_text = GameStrings.Get(key),
                    m_responseDisplay = AlertPopup.ResponseDisplay.OK
                };
                DialogManager.Get().ShowPopup(info);
            }
        }
    }

    private void BnetParty_OnPartyAttributeChanged_Error(PartyInfo party, string attributeKey, bnet.protocol.attribute.Variant value)
    {
        if (party.Type == PartyType.FRIENDLY_CHALLENGE)
        {
            if (this.DidReceiveChallenge() && value.HasIntValue)
            {
                object[] args = new object[] { value.IntValue };
                Log.Party.Print(LogLevel.Error, "BnetParty_OnPartyAttributeChanged_Error - code={0}", args);
                BnetErrorInfo info = new BnetErrorInfo(BnetFeature.Games, BnetFeatureEvent.Games_OnCreated, (BattleNetErrors) ((uint) value.IntValue));
                GameMgr.Get().OnBnetError(info, null);
            }
            if (BnetParty.IsLeader(party.Id) && !value.IsNone())
            {
                BnetParty.ClearPartyAttribute(party.Id, attributeKey);
            }
        }
    }

    public void CancelChallenge()
    {
        if (this.HasChallenge())
        {
            if (this.DidSendChallenge())
            {
                this.RescindChallenge();
            }
            else if (this.DidReceiveChallenge())
            {
                this.DeclineChallenge();
            }
        }
    }

    public bool CanChallenge(BnetPlayer player)
    {
        if (player == null)
        {
            return false;
        }
        BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
        if (player == myPlayer)
        {
            return false;
        }
        if (!this.AmIAvailable())
        {
            return false;
        }
        if ((TavernBrawlManager.Get().ShouldNewFriendlyChallengeBeTavernBrawl() && TavernBrawlManager.Get().CurrentMission().canCreateDeck) && !TavernBrawlManager.Get().HasValidDeck())
        {
            return false;
        }
        BnetGameAccount hearthstoneGameAccount = player.GetHearthstoneGameAccount();
        if (hearthstoneGameAccount == null)
        {
            return false;
        }
        if (!hearthstoneGameAccount.IsOnline())
        {
            return false;
        }
        if (!hearthstoneGameAccount.CanBeInvitedToGame())
        {
            return false;
        }
        if (ApplicationMgr.IsPublic())
        {
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
        return true;
    }

    private bool CanPromptReceivedChallenge()
    {
        if (!this.IsChallengeTavernBrawl())
        {
            return true;
        }
        if (!TavernBrawlManager.Get().HasUnlockedTavernBrawl)
        {
            DeclineReason notUnlocked = DeclineReason.NotUnlocked;
            BnetParty.SetPartyAttributeLong(this.m_partyId, "WTCG.Friendly.DeclineReason", (long) notUnlocked);
            this.DeclineChallenge();
            return false;
        }
        TavernBrawlManager.Get().EnsureScenarioDataReady(new TavernBrawlManager.CallbackEnsureServerDataReady(this.TavernBrawl_ReceivedChallenge_OnEnsureServerDataReady));
        return false;
    }

    private void CleanUpChallengeData(bool updateAvailability = true)
    {
        this.m_partyId = null;
        this.m_challengeePartyId = null;
        this.m_challengerId = null;
        this.m_challengerPending = false;
        this.m_challenger = null;
        this.m_challengerDeckSelected = false;
        this.m_challengee = null;
        this.m_challengeeAccepted = false;
        this.m_challengeeDeckSelected = false;
        this.m_scenarioId = 2;
        this.m_isChallengeTavernBrawl = false;
        if (updateAvailability)
        {
            this.UpdateMyAvailability();
        }
    }

    public void DeclineChallenge()
    {
        if (this.DidReceiveChallenge())
        {
            this.RevertTavernBrawlStatus();
            Network.DeclineFriendChallenge(this.m_partyId);
            BnetPlayer challenger = this.m_challenger;
            this.CleanUpChallengeData(true);
            this.FireChangedEvent(FriendChallengeEvent.I_DECLINED_CHALLENGE, challenger);
        }
    }

    public void DeselectDeck()
    {
        if (this.DidSendChallenge() && this.m_challengerDeckSelected)
        {
            this.m_challengerDeckSelected = false;
        }
        else if (this.DidReceiveChallenge() && this.m_challengeeDeckSelected)
        {
            this.m_challengeeDeckSelected = false;
        }
        else
        {
            return;
        }
        Network.ChooseFriendChallengeDeck(this.m_partyId, 0L);
        this.FireChangedEvent(FriendChallengeEvent.DESELECTED_DECK, BnetPresenceMgr.Get().GetMyPlayer());
    }

    public bool DidISelectDeck()
    {
        if (this.DidSendChallenge())
        {
            return this.m_challengerDeckSelected;
        }
        if (this.DidReceiveChallenge())
        {
            return this.m_challengeeDeckSelected;
        }
        return true;
    }

    public bool DidOpponentSelectDeck()
    {
        if (this.DidSendChallenge())
        {
            return this.m_challengeeDeckSelected;
        }
        if (this.DidReceiveChallenge())
        {
            return this.m_challengerDeckSelected;
        }
        return true;
    }

    public bool DidReceiveChallenge()
    {
        return (this.m_challengerPending || ((this.m_challenger != null) && (this.m_challengee == BnetPresenceMgr.Get().GetMyPlayer())));
    }

    public bool DidSendChallenge()
    {
        return ((this.m_challengee != null) && (this.m_challenger == BnetPresenceMgr.Get().GetMyPlayer()));
    }

    private void FireChangedEvent(FriendChallengeEvent challengeEvent, BnetPlayer player)
    {
        foreach (ChangedListener listener in this.m_changedListeners.ToArray())
        {
            listener.Fire(challengeEvent, player);
        }
    }

    public static FriendChallengeMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new FriendChallengeMgr();
            ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
        }
        return s_instance;
    }

    private FriendListAvailabilityBlockerReasons GetAvailabilityBlockerReason()
    {
        if (!this.m_netCacheReady)
        {
            return FriendListAvailabilityBlockerReasons.NETCACHE_NOT_READY;
        }
        if (!this.m_myPlayerReady)
        {
            return FriendListAvailabilityBlockerReasons.MY_PLAYER_NOT_READY;
        }
        if (this.HasChallenge())
        {
            return FriendListAvailabilityBlockerReasons.HAS_EXISTING_CHALLENGE;
        }
        if (SpectatorManager.Get().IsInSpectatorMode())
        {
            return FriendListAvailabilityBlockerReasons.SPECTATING_GAME;
        }
        if (GameMgr.Get().IsFindingGame())
        {
            return FriendListAvailabilityBlockerReasons.FINDING_GAME;
        }
        if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FATAL_ERROR))
        {
            return FriendListAvailabilityBlockerReasons.HAS_FATAL_ERROR;
        }
        if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.LOGIN))
        {
            return FriendListAvailabilityBlockerReasons.LOGGING_IN;
        }
        if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.STARTUP))
        {
            return FriendListAvailabilityBlockerReasons.STARTING_UP;
        }
        if (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.GAMEPLAY))
        {
            if (GameMgr.Get().IsSpectator() || GameMgr.Get().IsNextSpectator())
            {
                return FriendListAvailabilityBlockerReasons.SPECTATING_GAME;
            }
            if (!GameMgr.Get().IsAI() && !GameMgr.Get().IsNextAI())
            {
                return FriendListAvailabilityBlockerReasons.PLAYING_NON_AI_GAME;
            }
            return FriendListAvailabilityBlockerReasons.PLAYING_AI_GAME;
        }
        if (!GameUtils.AreAllTutorialsComplete())
        {
            return FriendListAvailabilityBlockerReasons.TUTORIALS_INCOMPLETE;
        }
        if (ShownUIMgr.Get().GetShownUI() == ShownUIMgr.UI_WINDOW.GENERAL_STORE)
        {
            return FriendListAvailabilityBlockerReasons.STORE_IS_SHOWN;
        }
        if ((TavernBrawlDisplay.Get() != null) && TavernBrawlDisplay.Get().IsInDeckEditMode())
        {
            return FriendListAvailabilityBlockerReasons.EDITING_TAVERN_BRAWL;
        }
        return FriendListAvailabilityBlockerReasons.NONE;
    }

    public BnetPlayer GetChallengee()
    {
        return this.m_challengee;
    }

    public BnetPlayer GetChallenger()
    {
        return this.m_challenger;
    }

    public BnetPlayer GetMyOpponent()
    {
        BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
        return this.GetOpponent(myPlayer);
    }

    public BnetPlayer GetOpponent(BnetPlayer player)
    {
        if (player == this.m_challenger)
        {
            return this.m_challengee;
        }
        if (player == this.m_challengee)
        {
            return this.m_challenger;
        }
        return null;
    }

    public int GetScenarioId()
    {
        return this.m_scenarioId;
    }

    private void HandleJoinedParty(BnetEntityId partyId, BnetGameAccountId otherMemberId)
    {
        this.m_partyId = partyId;
        this.m_challengeePartyId = partyId;
        this.m_challengerId = otherMemberId;
        this.m_challenger = BnetUtils.GetPlayer(this.m_challengerId);
        this.m_challengee = BnetPresenceMgr.Get().GetMyPlayer();
        this.m_hasSeenDeclinedReason = false;
        if ((this.m_challenger == null) || !this.m_challenger.IsDisplayable())
        {
            this.m_challengerPending = true;
            this.UpdateMyAvailability();
        }
        else
        {
            this.UpdateMyAvailability();
            this.FireChangedEvent(FriendChallengeEvent.I_RECEIVED_CHALLENGE, this.m_challenger);
        }
    }

    private bool HasAvailabilityBlocker()
    {
        return (this.GetAvailabilityBlockerReason() != FriendListAvailabilityBlockerReasons.NONE);
    }

    public bool HasChallenge()
    {
        return (this.DidSendChallenge() || this.DidReceiveChallenge());
    }

    public bool IsChallengeTavernBrawl()
    {
        if (!this.HasChallenge())
        {
            return false;
        }
        return this.m_isChallengeTavernBrawl;
    }

    private void OnBnetEventOccurred(BattleNet.BnetEvent bnetEvent, object userData)
    {
        if (bnetEvent == BattleNet.BnetEvent.Disconnected)
        {
            if (this.m_challengeDialog != null)
            {
                this.m_challengeDialog.Hide();
                this.m_challengeDialog = null;
            }
            this.CleanUpChallengeData(true);
        }
    }

    private void OnChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, object userData)
    {
        switch (challengeEvent)
        {
            case FriendChallengeEvent.I_SENT_CHALLENGE:
                this.ShowISentChallengeDialog(player);
                break;

            case FriendChallengeEvent.OPPONENT_ACCEPTED_CHALLENGE:
                this.StartChallengeProcess();
                break;

            case FriendChallengeEvent.OPPONENT_DECLINED_CHALLENGE:
                this.ShowOpponentDeclinedChallengeDialog(player);
                break;

            case FriendChallengeEvent.I_RECEIVED_CHALLENGE:
                if (this.CanPromptReceivedChallenge())
                {
                    if (this.IsChallengeTavernBrawl())
                    {
                        Enum[] args = new Enum[] { PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING };
                        PresenceMgr.Get().SetStatus(args);
                    }
                    this.ShowIReceivedChallengeDialog(player);
                    break;
                }
                break;

            case FriendChallengeEvent.I_ACCEPTED_CHALLENGE:
                this.StartChallengeProcess();
                break;

            case FriendChallengeEvent.OPPONENT_RESCINDED_CHALLENGE:
                this.ShowOpponentCanceledChallengeDialog(player);
                break;

            case FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE:
                this.ShowOpponentCanceledChallengeDialog(player);
                break;

            case FriendChallengeEvent.OPPONENT_REMOVED_FROM_FRIENDS:
                this.ShowOpponentRemovedFromFriendsDialog(player);
                break;
        }
    }

    private bool OnChallengeReceivedDialogProcessed(DialogBase dialog, object userData)
    {
        if (!this.DidReceiveChallenge())
        {
            return false;
        }
        this.m_challengeDialog = dialog;
        return true;
    }

    private void OnChallengeReceivedDialogResponse(bool accept)
    {
        this.m_challengeDialog = null;
        if (accept)
        {
            this.AcceptChallenge();
        }
        else
        {
            this.DeclineChallenge();
        }
    }

    private bool OnChallengeSentDialogProcessed(DialogBase dialog, object userData)
    {
        if (!this.DidSendChallenge())
        {
            return false;
        }
        this.m_challengeDialog = dialog;
        this.UpdateChallengeSentDialog();
        return true;
    }

    private void OnChallengeSentDialogResponse(AlertPopup.Response response, object userData)
    {
        this.m_challengeDialog = null;
        this.RescindChallenge();
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        this.UpdateMyAvailability();
        if (eventData.m_state == FindGameState.BNET_ERROR)
        {
            if (this.DidSendChallenge())
            {
                BnetParty.SetPartyAttributeLong(this.m_partyId, "error", (long) GameMgr.Get().GetLastEnterGameError());
                BnetParty.SetPartyAttributeString(this.m_partyId, "s1", "deck");
            }
            else if (this.DidReceiveChallenge())
            {
                BnetParty.SetPartyAttributeString(this.m_partyId, "s2", "deck");
            }
        }
        return false;
    }

    private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
    {
        if (this.HasChallenge())
        {
            List<BnetPlayer> removedFriends = changelist.GetRemovedFriends();
            if (removedFriends != null)
            {
                BnetPlayer opponent = this.GetOpponent(BnetPresenceMgr.Get().GetMyPlayer());
                if (opponent != null)
                {
                    foreach (BnetPlayer player2 in removedFriends)
                    {
                        if (player2 == opponent)
                        {
                            this.RevertTavernBrawlStatus();
                            this.CleanUpChallengeData(true);
                            this.FireChangedEvent(FriendChallengeEvent.OPPONENT_REMOVED_FROM_FRIENDS, opponent);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void OnLoggedIn()
    {
        Network.Get().SetPartyHandler(new Network.PartyHandler(this.OnPartyUpdate));
        NetCache.Get().RegisterFriendChallenge(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        SceneMgr.Get().RegisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(this.OnSceneUnloaded));
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
        BnetFriendMgr.Get().AddChangeListener(new BnetFriendMgr.ChangeCallback(this.OnFriendsChanged));
        BnetNearbyPlayerMgr.Get().AddChangeListener(new BnetNearbyPlayerMgr.ChangeCallback(this.OnNearbyPlayersChanged));
        BnetEventMgr.Get().AddChangeListener(new BnetEventMgr.ChangeCallback(this.OnBnetEventOccurred));
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        BnetParty.OnJoined += new BnetParty.JoinedHandler(this.BnetParty_OnJoined);
        BnetParty.RegisterAttributeChangedHandler("WTCG.Friendly.DeclineReason", new BnetParty.PartyAttributeChangedHandler(this.BnetParty_OnPartyAttributeChanged_DeclineReason));
        BnetParty.RegisterAttributeChangedHandler("error", new BnetParty.PartyAttributeChangedHandler(this.BnetParty_OnPartyAttributeChanged_Error));
        this.AddChangedListener(new ChangedCallback(this.OnChallengeChanged));
        BnetPresenceMgr.Get().SetGameField(0x13, BattleNet.GetVersion());
        BnetPresenceMgr.Get().SetGameField(20, BattleNet.GetEnvironment());
    }

    private void OnNearbyPlayersChanged(BnetNearbyPlayerChangelist changelist, object userData)
    {
        if (this.HasChallenge())
        {
            List<BnetPlayer> removedPlayers = changelist.GetRemovedPlayers();
            if (removedPlayers != null)
            {
                BnetPlayer opponent = this.GetOpponent(BnetPresenceMgr.Get().GetMyPlayer());
                if (opponent != null)
                {
                    foreach (BnetPlayer player2 in removedPlayers)
                    {
                        if (player2 == opponent)
                        {
                            this.CleanUpChallengeData(true);
                            this.FireChangedEvent(FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE, opponent);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void OnNetCacheReady()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        this.m_netCacheReady = true;
        if (SceneMgr.Get().GetMode() != SceneMgr.Mode.FATAL_ERROR)
        {
            this.UpdateMyAvailability();
        }
    }

    private void OnPartyUpdate(BattleNet.PartyEvent[] updates)
    {
        for (int i = 0; i < updates.Length; i++)
        {
            BattleNet.PartyEvent event2 = updates[i];
            BnetEntityId partyId = BnetEntityId.CreateFromDll(event2.partyId);
            BnetGameAccountId otherMemberId = BnetGameAccountId.CreateFromDll(event2.otherMemberId);
            if (event2.eventName == "s1")
            {
                if (event2.eventData == "wait")
                {
                    this.OnPartyUpdate_CreatedParty(partyId, otherMemberId);
                }
                else if (event2.eventData == "deck")
                {
                    if (this.DidReceiveChallenge() && this.m_challengerDeckSelected)
                    {
                        this.m_challengerDeckSelected = false;
                        this.FireChangedEvent(FriendChallengeEvent.DESELECTED_DECK, this.m_challenger);
                    }
                }
                else if (event2.eventData == "game")
                {
                    if (this.DidReceiveChallenge())
                    {
                        this.m_challengerDeckSelected = true;
                        this.FireChangedEvent(FriendChallengeEvent.SELECTED_DECK, this.m_challenger);
                    }
                }
                else if (event2.eventData == "goto")
                {
                    this.m_challengerDeckSelected = false;
                }
            }
            else if (event2.eventName == "s2")
            {
                if (event2.eventData == "wait")
                {
                    this.OnPartyUpdate_JoinedParty(partyId, otherMemberId);
                }
                else if (event2.eventData == "deck")
                {
                    if (this.DidSendChallenge())
                    {
                        if (this.m_challengeeAccepted)
                        {
                            this.m_challengeeDeckSelected = false;
                            this.FireChangedEvent(FriendChallengeEvent.DESELECTED_DECK, this.m_challengee);
                        }
                        else
                        {
                            this.m_challengeeAccepted = true;
                            this.FireChangedEvent(FriendChallengeEvent.OPPONENT_ACCEPTED_CHALLENGE, this.m_challengee);
                        }
                    }
                }
                else if (event2.eventData == "game")
                {
                    if (this.DidSendChallenge())
                    {
                        this.m_challengeeDeckSelected = true;
                        this.FireChangedEvent(FriendChallengeEvent.SELECTED_DECK, this.m_challengee);
                    }
                }
                else if (event2.eventData == "goto")
                {
                    this.m_challengeeDeckSelected = false;
                }
            }
            else if (event2.eventName == "left")
            {
                if (this.DidSendChallenge())
                {
                    BnetPlayer challengee = this.m_challengee;
                    bool challengeeAccepted = this.m_challengeeAccepted;
                    this.RevertTavernBrawlStatus();
                    this.CleanUpChallengeData(true);
                    if (challengeeAccepted)
                    {
                        this.FireChangedEvent(FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE, challengee);
                    }
                    else
                    {
                        this.FireChangedEvent(FriendChallengeEvent.OPPONENT_DECLINED_CHALLENGE, challengee);
                    }
                }
                else if (this.DidReceiveChallenge())
                {
                    BnetPlayer challenger = this.m_challenger;
                    bool flag2 = this.m_challengeeAccepted;
                    this.RevertTavernBrawlStatus();
                    this.CleanUpChallengeData(true);
                    if (challenger != null)
                    {
                        if (flag2)
                        {
                            this.FireChangedEvent(FriendChallengeEvent.OPPONENT_CANCELED_CHALLENGE, challenger);
                        }
                        else
                        {
                            this.FireChangedEvent(FriendChallengeEvent.OPPONENT_RESCINDED_CHALLENGE, challenger);
                        }
                    }
                }
            }
        }
    }

    private void OnPartyUpdate_CreatedParty(BnetEntityId partyId, BnetGameAccountId otherMemberId)
    {
        this.m_partyId = partyId;
        if ((this.m_challengeePartyId != null) && (this.m_challengeePartyId != this.m_partyId))
        {
            this.ResolveChallengeConflict();
        }
        else
        {
            this.m_challengeePartyId = this.m_partyId;
            this.UpdateChallengeSentDialog();
        }
    }

    private void OnPartyUpdate_JoinedParty(BnetEntityId partyId, BnetGameAccountId otherMemberId)
    {
        if (this.DidSendChallenge() && (this.m_challengee.GetHearthstoneGameAccount().GetId() == otherMemberId))
        {
            if (partyId != this.m_partyId)
            {
                this.m_challengeePartyId = partyId;
                if (this.m_partyId != null)
                {
                    this.ResolveChallengeConflict();
                }
            }
        }
        else if (!BnetUtils.CanReceiveChallengeFrom(otherMemberId))
        {
            Network.DeclineFriendChallenge(partyId);
        }
        else if (!this.AmIAvailable())
        {
            Network.DeclineFriendChallenge(partyId);
        }
        else
        {
            this.HandleJoinedParty(partyId, otherMemberId);
        }
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        BnetPlayer myPlayer = BnetPresenceMgr.Get().GetMyPlayer();
        if (changelist.FindChange(myPlayer) != null)
        {
            BnetGameAccount hearthstoneGameAccount = myPlayer.GetHearthstoneGameAccount();
            if (((hearthstoneGameAccount != null) && !this.m_myPlayerReady) && (hearthstoneGameAccount.HasGameField(20) && hearthstoneGameAccount.HasGameField(0x13)))
            {
                this.m_myPlayerReady = true;
                this.UpdateMyAvailability();
            }
            if (!this.AmIAvailable() && this.m_challengerPending)
            {
                Network.DeclineFriendChallenge(this.m_partyId);
                this.CleanUpChallengeData(true);
            }
        }
        if (this.m_challengerPending)
        {
            BnetPlayerChange change = changelist.FindChange(this.m_challengerId);
            if (change != null)
            {
                BnetPlayer player = change.GetPlayer();
                if (player.IsDisplayable())
                {
                    this.m_challenger = player;
                    this.m_challengerPending = false;
                    this.FireChangedEvent(FriendChallengeEvent.I_RECEIVED_CHALLENGE, this.m_challenger);
                }
            }
        }
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if (((SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY) && (mode != SceneMgr.Mode.GAMEPLAY)) && (mode != SceneMgr.Mode.FATAL_ERROR))
        {
            this.m_netCacheReady = false;
            if ((mode == SceneMgr.Mode.FRIENDLY) || TavernBrawlManager.IsInTavernBrawlFriendlyChallenge())
            {
                this.UpdateMyAvailability();
            }
            else
            {
                this.CleanUpChallengeData(true);
            }
            NetCache.Get().RegisterFriendChallenge(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        }
    }

    private void OnSceneUnloaded(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        if (Network.IsLoggedIn() && (prevMode != SceneMgr.Mode.GAMEPLAY))
        {
            this.UpdateMyAvailability();
        }
    }

    public void OnStoreClosed()
    {
        this.UpdateMyAvailability();
    }

    public void OnStoreOpened()
    {
        this.UpdateMyAvailability();
    }

    public bool RemoveChangedListener(ChangedCallback callback)
    {
        return this.RemoveChangedListener(callback, null);
    }

    public bool RemoveChangedListener(ChangedCallback callback, object userData)
    {
        ChangedListener item = new ChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_changedListeners.Remove(item);
    }

    public void RescindChallenge()
    {
        if (this.DidSendChallenge())
        {
            this.RevertTavernBrawlStatus();
            Network.RescindFriendChallenge(this.m_partyId);
            BnetPlayer challengee = this.m_challengee;
            this.CleanUpChallengeData(true);
            this.FireChangedEvent(FriendChallengeEvent.I_RESCINDED_CHALLENGE, challengee);
        }
    }

    private void ResolveChallengeConflict()
    {
        if (this.m_partyId.GetLo() < this.m_challengeePartyId.GetLo())
        {
            Network.DeclineFriendChallenge(this.m_challengeePartyId);
            this.m_challengeePartyId = this.m_partyId;
        }
        else if (this.m_challengee != null)
        {
            this.HandleJoinedParty(this.m_challengeePartyId, this.m_challengee.GetHearthstoneGameAccount().GetId());
        }
    }

    private bool RevertTavernBrawlStatus()
    {
        Enum[] status = PresenceMgr.Get().GetStatus();
        if ((this.IsChallengeTavernBrawl() && (status != null)) && ((status.Length > 0) && (((PresenceStatus) status[0]) == PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING)))
        {
            PresenceMgr.Get().SetPrevStatus();
            return true;
        }
        return false;
    }

    public void SelectDeck(long deckId)
    {
        if (this.DidSendChallenge())
        {
            this.m_challengerDeckSelected = true;
        }
        else if (this.DidReceiveChallenge())
        {
            this.m_challengeeDeckSelected = true;
        }
        else
        {
            return;
        }
        Network.ChooseFriendChallengeDeck(this.m_partyId, deckId);
        this.FireChangedEvent(FriendChallengeEvent.SELECTED_DECK, BnetPresenceMgr.Get().GetMyPlayer());
    }

    public void SendChallenge(BnetPlayer player)
    {
        if (this.CanChallenge(player))
        {
            this.m_challenger = BnetPresenceMgr.Get().GetMyPlayer();
            this.m_challengerId = this.m_challenger.GetHearthstoneGameAccount().GetId();
            this.m_challengee = player;
            this.m_hasSeenDeclinedReason = false;
            this.m_scenarioId = 2;
            this.m_isChallengeTavernBrawl = false;
            if (TavernBrawlManager.Get().ShouldNewFriendlyChallengeBeTavernBrawl())
            {
                this.m_scenarioId = TavernBrawlManager.Get().CurrentMission().missionId;
                Enum[] args = new Enum[] { PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING };
                PresenceMgr.Get().SetStatus(args);
                this.m_isChallengeTavernBrawl = true;
            }
            Network.SendFriendChallenge(player.GetHearthstoneGameAccount().GetId(), this.m_scenarioId);
            this.UpdateMyAvailability();
            this.FireChangedEvent(FriendChallengeEvent.I_SENT_CHALLENGE, player);
        }
    }

    private void ShowIReceivedChallengeDialog(BnetPlayer challenger)
    {
        if (this.m_challengeDialog != null)
        {
            this.m_challengeDialog.Hide();
            this.m_challengeDialog = null;
        }
        DialogManager.Get().ShowFriendlyChallenge(challenger, this.IsChallengeTavernBrawl(), new FriendlyChallengeDialog.ResponseCallback(this.OnChallengeReceivedDialogResponse), new DialogManager.DialogProcessCallback(this.OnChallengeReceivedDialogProcessed));
    }

    private void ShowISentChallengeDialog(BnetPlayer challengee)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER")
        };
        object[] args = new object[] { FriendUtils.GetUniqueName(challengee) };
        info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_WAITING_RESPONSE", args);
        info.m_showAlertIcon = false;
        info.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
        info.m_responseCallback = new AlertPopup.ResponseCallback(this.OnChallengeSentDialogResponse);
        DialogManager.Get().ShowPopup(info, new DialogManager.DialogProcessCallback(this.OnChallengeSentDialogProcessed));
    }

    private void ShowOpponentCanceledChallengeDialog(BnetPlayer otherPlayer)
    {
        if (this.m_challengeDialog != null)
        {
            this.m_challengeDialog.Hide();
            this.m_challengeDialog = null;
        }
        if ((((SceneMgr.Get() == null) || !SceneMgr.Get().IsInGame()) || ((GameState.Get() == null) || GameState.Get().IsGameOverNowOrPending())) || (((GameState.Get().GetOpposingSidePlayer() == null) || (GameState.Get().GetOpposingSidePlayer().GetBnetPlayer() == null)) || ((otherPlayer == null) || (otherPlayer.GetHearthstoneGameAccountId() != GameState.Get().GetOpposingSidePlayer().GetBnetPlayer().GetHearthstoneGameAccountId()))))
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER")
            };
            object[] args = new object[] { FriendUtils.GetUniqueName(otherPlayer) };
            info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_CANCELED", args);
            info.m_showAlertIcon = false;
            info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
            DialogManager.Get().ShowPopup(info);
        }
    }

    private void ShowOpponentDeclinedChallengeDialog(BnetPlayer challengee)
    {
        if (this.m_challengeDialog != null)
        {
            this.m_challengeDialog.Hide();
            this.m_challengeDialog = null;
        }
        if (!this.m_hasSeenDeclinedReason)
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER")
            };
            object[] args = new object[] { FriendUtils.GetUniqueName(challengee) };
            info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_DECLINED", args);
            info.m_showAlertIcon = false;
            info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
            DialogManager.Get().ShowPopup(info);
        }
    }

    private void ShowOpponentRemovedFromFriendsDialog(BnetPlayer otherPlayer)
    {
        if (this.m_challengeDialog != null)
        {
            this.m_challengeDialog.Hide();
            this.m_challengeDialog = null;
        }
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_HEADER")
        };
        object[] args = new object[] { FriendUtils.GetUniqueName(otherPlayer) };
        info.m_text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_OPPONENT_FRIEND_REMOVED", args);
        info.m_showAlertIcon = false;
        info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
        DialogManager.Get().ShowPopup(info);
    }

    public void SkipDeckSelection()
    {
        this.SelectDeck(1L);
    }

    private void StartChallengeProcess()
    {
        if (this.m_challengeDialog != null)
        {
            this.m_challengeDialog.Hide();
            this.m_challengeDialog = null;
        }
        GameMgr.Get().SetPendingAutoConcede(true);
        if (this.IsChallengeTavernBrawl() && !TavernBrawlManager.Get().SelectHeroBeforeMission())
        {
            if (TavernBrawlManager.Get().CurrentMission().canCreateDeck)
            {
                if (!TavernBrawlManager.Get().HasValidDeck())
                {
                    Debug.LogError("Attempting to start a Tavern Brawl challenge without a valid deck!  How did this happen?");
                    return;
                }
                this.SelectDeck(TavernBrawlManager.Get().CurrentDeck().ID);
            }
            else
            {
                this.SkipDeckSelection();
            }
            FriendlyChallengeHelper.Get().WaitForFriendChallengeToStart();
        }
        else
        {
            Navigation.Clear();
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.FRIENDLY);
        }
    }

    private void TavernBrawl_ReceivedChallenge_OnEnsureServerDataReady()
    {
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        if ((mission != null) && mission.canCreateDeck)
        {
            CollectionDeck deck = TavernBrawlManager.Get().CurrentDeck();
            if ((deck != null) && !deck.NetworkContentsLoaded())
            {
                CollectionManager.Get().RegisterDeckContentsListener(new CollectionManager.DelOnDeckContents(this.TavernBrawl_ReceivedChallenge_OnEnsureServerDataReady_OnDeckContentsReady));
                CollectionManager.Get().RequestDeckContents(deck.ID);
                return;
            }
        }
        this.TavernBrawl_ReceivedChallenge_OnEnsureServerDataReady_OnDeckContentsReady(0L);
    }

    private void TavernBrawl_ReceivedChallenge_OnEnsureServerDataReady_OnDeckContentsReady(long deckId)
    {
        CollectionManager.Get().RemoveDeckContentsListener(new CollectionManager.DelOnDeckContents(this.TavernBrawl_ReceivedChallenge_OnEnsureServerDataReady_OnDeckContentsReady));
        TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
        string key = null;
        DeclineReason? nullable = null;
        if (mission == null)
        {
            nullable = 0;
        }
        if (((mission != null) && mission.canCreateDeck) && !TavernBrawlManager.Get().HasValidDeck())
        {
            nullable = 2;
            key = "GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_RECIPIENT_NO_VALID_DECK_RECIPIENT";
        }
        if (key != null)
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLOBAL_FRIEND_CHALLENGE_TAVERN_BRAWL_HEADER")
            };
            object[] args = new object[] { FriendUtils.GetUniqueName(this.m_challenger) };
            info.m_text = GameStrings.Format(key, args);
            info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
            DialogManager.Get().ShowPopup(info);
        }
        if (nullable.HasValue)
        {
            BnetParty.SetPartyAttributeLong(this.m_partyId, "WTCG.Friendly.DeclineReason", (long) nullable.Value);
            this.DeclineChallenge();
        }
        else
        {
            if (this.IsChallengeTavernBrawl())
            {
                Enum[] enumArray1 = new Enum[] { PresenceStatus.TAVERN_BRAWL_FRIENDLY_WAITING };
                PresenceMgr.Get().SetStatus(enumArray1);
            }
            this.ShowIReceivedChallengeDialog(this.m_challenger);
        }
    }

    private void UpdateChallengeSentDialog()
    {
        if ((this.m_partyId != null) && (this.m_challengeDialog != null))
        {
            AlertPopup challengeDialog = (AlertPopup) this.m_challengeDialog;
            AlertPopup.PopupInfo info = challengeDialog.GetInfo();
            if (info != null)
            {
                info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
                challengeDialog.UpdateInfo(info);
            }
        }
    }

    public void UpdateMyAvailability()
    {
        bool val = !this.HasAvailabilityBlocker();
        BnetPresenceMgr.Get().SetGameField(1, val);
        BnetNearbyPlayerMgr.Get().SetAvailability(val);
    }

    private void WillReset()
    {
        this.CleanUpChallengeData(false);
        if (this.m_challengeDialog != null)
        {
            this.m_challengeDialog.Hide();
            this.m_challengeDialog = null;
        }
    }

    public delegate void ChangedCallback(FriendChallengeEvent challengeEvent, BnetPlayer player, object userData);

    private class ChangedListener : EventListener<FriendChallengeMgr.ChangedCallback>
    {
        public void Fire(FriendChallengeEvent challengeEvent, BnetPlayer player)
        {
            base.m_callback(challengeEvent, player, base.m_userData);
        }
    }

    public enum DeclineReason
    {
        None,
        UserDeclined,
        NoValidDeck,
        NotUnlocked
    }

    private enum FriendListAvailabilityBlockerReasons
    {
        NONE,
        NETCACHE_NOT_READY,
        MY_PLAYER_NOT_READY,
        HAS_EXISTING_CHALLENGE,
        FINDING_GAME,
        HAS_FATAL_ERROR,
        LOGGING_IN,
        STARTING_UP,
        PLAYING_NON_AI_GAME,
        TUTORIALS_INCOMPLETE,
        STORE_IS_SHOWN,
        PLAYING_AI_GAME,
        SPECTATING_GAME,
        EDITING_TAVERN_BRAWL
    }
}

