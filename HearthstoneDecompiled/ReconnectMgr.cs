using PegasusShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ReconnectMgr : MonoBehaviour
{
    private AlertPopup m_dialog;
    private Map<GameType, string> m_gameTypeNameKeys;
    private NetCache.ProfileNoticeDisconnectedGame m_pendingReconnectNotice;
    private float m_reconnectStartTimestamp;
    private ReconnectType m_reconnectType;
    private float m_retryStartTimestamp;
    private SavedStartGameParameters m_savedStartGameParams;
    private List<TimeoutListener> m_timeoutListeners;
    private static ReconnectMgr s_instance;

    public ReconnectMgr()
    {
        Map<GameType, string> map = new Map<GameType, string>();
        map.Add(GameType.GT_VS_FRIEND, "GLUE_RECONNECT_GAME_TYPE_FRIENDLY");
        map.Add(GameType.GT_ARENA, "GLUE_RECONNECT_GAME_TYPE_ARENA");
        map.Add(GameType.GT_RANKED, "GLUE_RECONNECT_GAME_TYPE_RANKED");
        map.Add(GameType.GT_UNRANKED, "GLUE_RECONNECT_GAME_TYPE_UNRANKED");
        map.Add(GameType.GT_TAVERNBRAWL, "GLUE_RECONNECT_GAME_TYPE_TAVERN_BRAWL");
        this.m_gameTypeNameKeys = map;
        this.m_savedStartGameParams = new SavedStartGameParameters();
        this.m_timeoutListeners = new List<TimeoutListener>();
    }

    private void AckNotice(NetCache.ProfileNoticeDisconnectedGame notice)
    {
        Network.AckNotice(notice.NoticeID);
    }

    public bool AddTimeoutListener(TimeoutCallback callback)
    {
        return this.AddTimeoutListener(callback, null);
    }

    public bool AddTimeoutListener(TimeoutCallback callback, object userData)
    {
        TimeoutListener item = new TimeoutListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_timeoutListeners.Contains(item))
        {
            return false;
        }
        this.m_timeoutListeners.Add(item);
        return true;
    }

    private void Awake()
    {
        s_instance = this;
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
    }

    private void ChangeDialogToReconnected()
    {
        if (this.m_dialog != null)
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTED_HEADER")
            };
            if (this.m_reconnectType == ReconnectType.LOGIN)
            {
                info.m_text = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTED_LOGIN");
            }
            else
            {
                info.m_text = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTED");
            }
            info.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
            info.m_showAlertIcon = true;
            this.m_dialog.UpdateInfo(info);
            LoadingScreen.Get().RegisterPreviousSceneDestroyedListener(new LoadingScreen.PreviousSceneDestroyedCallback(this.OnPreviousSceneDestroyed));
        }
    }

    private void ChangeDialogToTimeout()
    {
        if (this.m_dialog != null)
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = GameStrings.Get("GLOBAL_RECONNECT_TIMEOUT_HEADER"),
                m_text = GameStrings.Get("GLOBAL_RECONNECT_TIMEOUT"),
                m_responseDisplay = AlertPopup.ResponseDisplay.OK,
                m_showAlertIcon = true,
                m_responseCallback = new AlertPopup.ResponseCallback(this.OnTimeoutDialogResponse)
            };
            this.m_dialog.UpdateInfo(info);
        }
    }

    private void CheckReconnectTimeout()
    {
        if (this.IsReconnecting())
        {
            float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            float num2 = realtimeSinceStartup - this.m_reconnectStartTimestamp;
            float timeout = this.GetTimeout();
            if (num2 >= timeout)
            {
                this.OnReconnectTimeout();
            }
            else
            {
                ClientConnection<PegasusPacket> gameServerConnection = ConnectAPI.GetGameServerConnection();
                if (!gameServerConnection.Active && !gameServerConnection.HasEvents())
                {
                    float num4 = realtimeSinceStartup - this.m_retryStartTimestamp;
                    float retryTime = this.GetRetryTime();
                    if (num4 >= retryTime)
                    {
                        this.m_retryStartTimestamp = realtimeSinceStartup;
                        this.StartGame();
                    }
                }
            }
        }
    }

    private void ClearReconnectData()
    {
        this.m_reconnectType = ReconnectType.INVALID;
        this.m_reconnectStartTimestamp = 0f;
        this.m_retryStartTimestamp = 0f;
    }

    private void FireTimeoutEvent()
    {
        TimeoutListener[] listenerArray = this.m_timeoutListeners.ToArray();
        this.m_timeoutListeners.Clear();
        bool flag = false;
        for (int i = 0; i < listenerArray.Length; i++)
        {
            flag = listenerArray[i].Fire() || flag;
        }
        if (!flag && Network.IsLoggedIn())
        {
            SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
        }
    }

    public static ReconnectMgr Get()
    {
        return s_instance;
    }

    private NetCache.ProfileNoticeDisconnectedGame GetDCGameNotice()
    {
        NetCache.NetCacheProfileNotices netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
        if (((netObject == null) || (netObject.Notices == null)) || (netObject.Notices.Count == 0))
        {
            return null;
        }
        NetCache.ProfileNoticeDisconnectedGame game = null;
        foreach (NetCache.ProfileNotice notice in netObject.Notices)
        {
            NetCache.ProfileNoticeDisconnectedGame game2 = notice as NetCache.ProfileNoticeDisconnectedGame;
            if (game2 != null)
            {
                if (game == null)
                {
                    game = game2;
                }
                else if (game2.NoticeID > game.NoticeID)
                {
                    game = game2;
                }
            }
        }
        List<NetCache.ProfileNoticeDisconnectedGame> list = new List<NetCache.ProfileNoticeDisconnectedGame>();
        foreach (NetCache.ProfileNotice notice2 in netObject.Notices)
        {
            NetCache.ProfileNoticeDisconnectedGame item = notice2 as NetCache.ProfileNoticeDisconnectedGame;
            if ((item != null) && (notice2.NoticeID != game.NoticeID))
            {
                list.Add(item);
            }
        }
        foreach (NetCache.ProfileNoticeDisconnectedGame game4 in list)
        {
            this.AckNotice(game4);
        }
        return game;
    }

    private string GetGameTypeName(GameType gameType, int missionId)
    {
        DbfRecord adventureRecord = GameUtils.GetAdventureRecord(missionId);
        if (adventureRecord == null)
        {
            string str;
            if (this.m_gameTypeNameKeys.TryGetValue(gameType, out str))
            {
                return GameStrings.Get(str);
            }
            object[] messageArgs = new object[] { missionId, gameType };
            Error.AddDevFatal("ReconnectMgr.GetGameTypeName() - no name for mission {0} gameType {1}", messageArgs);
            return string.Empty;
        }
        switch (adventureRecord.GetInt("ID"))
        {
            case 1:
                return GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_TUTORIAL");

            case 2:
                return GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_PRACTICE");

            case 3:
                return GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_NAXXRAMAS");

            case 4:
                return GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_BRM");

            case 7:
                return GameStrings.Get("GLUE_RECONNECT_GAME_TYPE_TAVERN_BRAWL");
        }
        return adventureRecord.GetLocString("NAME");
    }

    public float GetRetryTime()
    {
        if (ApplicationMgr.IsInternal())
        {
            return Options.Get().GetFloat(Option.RECONNECT_RETRY_TIME);
        }
        return (float) OptionDataTables.s_defaultsMap[Option.RECONNECT_RETRY_TIME];
    }

    public float GetTimeout()
    {
        if (ApplicationMgr.IsInternal())
        {
            return Options.Get().GetFloat(Option.RECONNECT_TIMEOUT);
        }
        return (float) OptionDataTables.s_defaultsMap[Option.RECONNECT_TIMEOUT];
    }

    private void HideDialog()
    {
        if (this.m_dialog != null)
        {
            this.m_dialog.Hide();
            this.m_dialog = null;
        }
    }

    public bool IsReconnectEnabled()
    {
        return (ApplicationMgr.IsPublic() || Options.Get().GetBool(Option.RECONNECT));
    }

    public bool IsReconnecting()
    {
        return (this.m_reconnectType != ReconnectType.INVALID);
    }

    public bool IsStartingReconnectGame()
    {
        if (GameMgr.Get().IsReconnect())
        {
            if (SceneMgr.Get().GetNextMode() == SceneMgr.Mode.GAMEPLAY)
            {
                return true;
            }
            if ((SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY) && !SceneMgr.Get().IsSceneLoaded())
            {
                return true;
            }
        }
        return false;
    }

    private void OnDestroy()
    {
        GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        s_instance = null;
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        this.ClearReconnectData();
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        switch (eventData.m_state)
        {
            case FindGameState.SERVER_GAME_STARTED:
                if (this.IsReconnecting())
                {
                    this.m_timeoutListeners.Clear();
                    this.ChangeDialogToReconnected();
                    this.ClearReconnectData();
                    this.m_pendingReconnectNotice = null;
                }
                break;

            case FindGameState.SERVER_GAME_CANCELED:
                if (this.IsReconnecting())
                {
                    this.OnReconnectTimeout();
                    return true;
                }
                break;
        }
        return false;
    }

    private void OnGameResult(NetCache.ProfileNoticeDisconnectedGame dcGameNotice)
    {
        this.ShowDisconnectedGameResult(dcGameNotice);
        this.AckNotice(dcGameNotice);
    }

    private void OnNetCacheReady()
    {
        GameType gameType = this.m_pendingReconnectNotice.GameType;
        if (gameType != GameType.GT_UNKNOWN)
        {
            NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.OnNetCacheReady));
            ReconnectType lOGIN = ReconnectType.LOGIN;
            NetCache.NetCacheDisconnectedGame netObject = NetCache.Get().GetNetObject<NetCache.NetCacheDisconnectedGame>();
            if ((netObject == null) || (netObject.ServerInfo == null))
            {
                this.OnReconnectTimeout();
            }
            else
            {
                this.StartGame(gameType, lOGIN, netObject.ServerInfo);
            }
        }
    }

    private void OnPreviousSceneDestroyed(object userData)
    {
        LoadingScreen.Get().UnregisterPreviousSceneDestroyedListener(new LoadingScreen.PreviousSceneDestroyedCallback(this.OnPreviousSceneDestroyed));
        this.HideDialog();
    }

    private bool OnReconnectingDialogProcessed(DialogBase dialog, object userData)
    {
        if (!this.IsReconnecting())
        {
            return false;
        }
        this.m_dialog = (AlertPopup) dialog;
        if (this.IsStartingReconnectGame())
        {
            this.ChangeDialogToReconnected();
        }
        return true;
    }

    private void OnReconnectingDialogResponse(AlertPopup.Response response, object userData)
    {
        this.m_dialog = null;
        ApplicationMgr.Get().Exit();
    }

    private void OnReconnectTimeout()
    {
        this.ClearReconnectData();
        this.ChangeDialogToTimeout();
        if (this.m_pendingReconnectNotice != null)
        {
            this.AckNotice(this.m_pendingReconnectNotice);
            this.m_pendingReconnectNotice = null;
        }
        this.FireTimeoutEvent();
    }

    private void OnTimeoutDialogResponse(AlertPopup.Response response, object userData)
    {
        this.m_dialog = null;
        if (!Network.IsLoggedIn())
        {
            if (ApplicationMgr.AllowResetFromFatalError != null)
            {
                ApplicationMgr.Get().Reset();
            }
            else
            {
                ApplicationMgr.Get().Exit();
            }
        }
    }

    public bool ReconnectFromGameplay()
    {
        if (!this.IsReconnectEnabled())
        {
            return false;
        }
        BattleNet.GameServerInfo lastGameServerJoined = Network.Get().GetLastGameServerJoined();
        if (lastGameServerJoined == null)
        {
            Debug.LogError("serverInfo in ReconnectMgr.ReconnectFromGameplay is null and should not be!");
            return false;
        }
        if (!lastGameServerJoined.Resumable)
        {
            return false;
        }
        this.HideDialog();
        GameType gameType = GameMgr.Get().GetGameType();
        ReconnectType gAMEPLAY = ReconnectType.GAMEPLAY;
        this.StartReconnecting(gAMEPLAY);
        this.StartGame(gameType, gAMEPLAY, lastGameServerJoined);
        return true;
    }

    public bool ReconnectFromLogin()
    {
        NetCache.ProfileNoticeDisconnectedGame dCGameNotice = this.GetDCGameNotice();
        if (dCGameNotice == null)
        {
            return false;
        }
        if (!this.IsReconnectEnabled())
        {
            return false;
        }
        if (dCGameNotice.GameResult != ProfileNoticeDisconnectedGameResult.GameResult.GR_PLAYING)
        {
            this.OnGameResult(dCGameNotice);
            return false;
        }
        if (dCGameNotice.GameType == GameType.GT_UNKNOWN)
        {
            return false;
        }
        this.m_pendingReconnectNotice = dCGameNotice;
        ReconnectType lOGIN = ReconnectType.LOGIN;
        this.StartReconnecting(lOGIN);
        NetCache.Get().RegisterReconnectMgr(new NetCache.NetCacheCallback(this.OnNetCacheReady));
        return true;
    }

    public bool RemoveTimeoutListener(TimeoutCallback callback)
    {
        return this.RemoveTimeoutListener(callback, null);
    }

    public bool RemoveTimeoutListener(TimeoutCallback callback, object userData)
    {
        TimeoutListener item = new TimeoutListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_timeoutListeners.Remove(item);
    }

    public bool ShowDisconnectedGameResult(NetCache.ProfileNoticeDisconnectedGame dcGame)
    {
        object[] objArray1;
        AlertPopup.PopupInfo info;
        string str;
        if (GameUtils.IsMatchmadeGameType(dcGame.GameType))
        {
            switch (dcGame.GameResult)
            {
                case ProfileNoticeDisconnectedGameResult.GameResult.GR_WINNER:
                case ProfileNoticeDisconnectedGameResult.GameResult.GR_TIE:
                    if (dcGame.GameType != GameType.GT_UNKNOWN)
                    {
                        info = new AlertPopup.PopupInfo {
                            m_headerText = GameStrings.Get("GLUE_RECONNECT_RESULT_HEADER")
                        };
                        str = null;
                        if (dcGame.GameResult == ProfileNoticeDisconnectedGameResult.GameResult.GR_TIE)
                        {
                            str = "GLUE_RECONNECT_RESULT_TIE";
                            goto Label_00EA;
                        }
                        switch (dcGame.YourResult)
                        {
                            case ProfileNoticeDisconnectedGameResult.PlayerResult.PR_WON:
                                str = "GLUE_RECONNECT_RESULT_WIN";
                                goto Label_00EA;

                            case ProfileNoticeDisconnectedGameResult.PlayerResult.PR_LOST:
                            case ProfileNoticeDisconnectedGameResult.PlayerResult.PR_QUIT:
                                str = "GLUE_RECONNECT_RESULT_LOSE";
                                goto Label_00EA;

                            case ProfileNoticeDisconnectedGameResult.PlayerResult.PR_DISCONNECTED:
                                str = "GLUE_RECONNECT_RESULT_DISCONNECT";
                                goto Label_00EA;
                        }
                        Debug.LogError(string.Format("ReconnectMgr.ShowDisconnectedGameResult() - unhandled player result {0}", dcGame.YourResult));
                    }
                    return false;
            }
            Debug.LogError(string.Format("ReconnectMgr.ShowDisconnectedGameResult() - unhandled game result {0}", dcGame.GameResult));
        }
        return false;
    Label_00EA:
        objArray1 = new object[] { this.GetGameTypeName(dcGame.GameType, dcGame.MissionId) };
        info.m_text = GameStrings.Format(str, objArray1);
        info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
        info.m_showAlertIcon = true;
        DialogManager.Get().ShowPopup(info);
        return true;
    }

    private void ShowReconnectingDialog()
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTING_HEADER")
        };
        if (this.m_reconnectType == ReconnectType.LOGIN)
        {
            info.m_text = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTING_LOGIN");
        }
        else
        {
            info.m_text = GameStrings.Get("GLOBAL_RECONNECT_RECONNECTING");
        }
        if (ApplicationMgr.CanQuitGame != null)
        {
            info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
            info.m_cancelText = GameStrings.Get("GLOBAL_RECONNECT_EXIT_BUTTON");
        }
        else
        {
            info.m_responseDisplay = AlertPopup.ResponseDisplay.NONE;
        }
        info.m_showAlertIcon = true;
        info.m_responseCallback = new AlertPopup.ResponseCallback(this.OnReconnectingDialogResponse);
        DialogManager.Get().ShowPopup(info, new DialogManager.DialogProcessCallback(this.OnReconnectingDialogProcessed));
    }

    private void Start()
    {
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
    }

    private void StartGame()
    {
        GameMgr.Get().ReconnectGame(this.m_savedStartGameParams.GameType, this.m_savedStartGameParams.ReconnectType, this.m_savedStartGameParams.ServerInfo);
    }

    private void StartGame(GameType gameType, ReconnectType reconnectType, BattleNet.GameServerInfo serverInfo)
    {
        this.m_savedStartGameParams.GameType = gameType;
        this.m_savedStartGameParams.ReconnectType = reconnectType;
        this.m_savedStartGameParams.ServerInfo = serverInfo;
        this.StartGame();
    }

    private void StartReconnecting(ReconnectType reconnectType)
    {
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        this.m_reconnectType = reconnectType;
        this.m_reconnectStartTimestamp = realtimeSinceStartup;
        this.m_retryStartTimestamp = realtimeSinceStartup;
        this.ShowReconnectingDialog();
    }

    private void Update()
    {
        this.CheckReconnectTimeout();
    }

    private void WillReset()
    {
        this.m_dialog = null;
        this.ClearReconnectData();
        this.m_timeoutListeners.Clear();
        this.m_pendingReconnectNotice = null;
    }

    private class SavedStartGameParameters
    {
        public PegasusShared.GameType GameType;
        public ReconnectType ReconnectType;
        public BattleNet.GameServerInfo ServerInfo;
    }

    public delegate bool TimeoutCallback(object userData);

    private class TimeoutListener : EventListener<ReconnectMgr.TimeoutCallback>
    {
        public bool Fire()
        {
            return base.m_callback(base.m_userData);
        }
    }
}

