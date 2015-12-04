using System;
using UnityEngine;

public class DisconnectMgr : MonoBehaviour
{
    private AlertPopup m_dialog;
    private static DisconnectMgr s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public void DisconnectFromGameplay()
    {
        SceneMgr.Mode postDisconnectSceneMode = GameMgr.Get().GetPostDisconnectSceneMode();
        GameMgr.Get().PreparePostGameSceneMode(postDisconnectSceneMode);
        if (postDisconnectSceneMode == SceneMgr.Mode.INVALID)
        {
            Network.Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_LOST_GAME_CONNECTION", 0f);
        }
        else if (Network.WasDisconnectRequested())
        {
            SceneMgr.Get().SetNextMode(postDisconnectSceneMode);
        }
        else
        {
            this.ShowGameplayDialog(postDisconnectSceneMode);
        }
    }

    public static DisconnectMgr Get()
    {
        return s_instance;
    }

    private void OnDestroy()
    {
        if (SceneMgr.Get() != null)
        {
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        }
        s_instance = null;
    }

    private bool OnGameplayDialogProcessed(DialogBase dialog, object userData)
    {
        this.m_dialog = (AlertPopup) dialog;
        SceneMgr.Mode mode = (SceneMgr.Mode) ((int) userData);
        SceneMgr.Get().SetNextMode(mode);
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        return true;
    }

    private void OnGameplayDialogResponse(AlertPopup.Response response, object userData)
    {
        this.m_dialog = null;
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded), userData);
        this.UpdateGameplayDialog();
    }

    private void ShowGameplayDialog(SceneMgr.Mode nextMode)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_headerText = GameStrings.Get("GLOBAL_ERROR_NETWORK_TITLE"),
            m_text = GameStrings.Get("GLOBAL_ERROR_NETWORK_LOST_GAME_CONNECTION"),
            m_responseDisplay = AlertPopup.ResponseDisplay.NONE
        };
        DialogManager.Get().ShowPopup(info, new DialogManager.DialogProcessCallback(this.OnGameplayDialogProcessed), nextMode);
    }

    private void UpdateGameplayDialog()
    {
        AlertPopup.PopupInfo info = this.m_dialog.GetInfo();
        info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
        info.m_responseCallback = new AlertPopup.ResponseCallback(this.OnGameplayDialogResponse);
        this.m_dialog.UpdateInfo(info);
    }
}

