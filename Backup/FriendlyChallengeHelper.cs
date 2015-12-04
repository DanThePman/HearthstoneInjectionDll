using System;

public class FriendlyChallengeHelper
{
    private AlertPopup m_friendChallengeWaitingPopup;
    private static FriendlyChallengeHelper s_instance;

    public static FriendlyChallengeHelper Get()
    {
        if (s_instance == null)
        {
            s_instance = new FriendlyChallengeHelper();
        }
        return s_instance;
    }

    public void HideFriendChallengeWaitingForOpponentDialog()
    {
        if (this.m_friendChallengeWaitingPopup != null)
        {
            this.m_friendChallengeWaitingPopup.Hide();
            this.m_friendChallengeWaitingPopup = null;
        }
    }

    private bool OnFriendChallengeWaitingForOpponentDialogProcessed(DialogBase dialog, object userData)
    {
        if (!FriendChallengeMgr.Get().HasChallenge())
        {
            return false;
        }
        if (FriendChallengeMgr.Get().DidOpponentSelectDeck())
        {
            this.WaitForFriendChallengeToStart();
            return false;
        }
        this.m_friendChallengeWaitingPopup = (AlertPopup) dialog;
        return true;
    }

    private void ShowFriendChallengeWaitingForOpponentDialog(string dialogText, AlertPopup.ResponseCallback callback)
    {
        BnetPlayer myOpponent = FriendChallengeMgr.Get().GetMyOpponent();
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
        object[] args = new object[] { FriendUtils.GetUniqueName(myOpponent) };
        info.m_text = GameStrings.Format(dialogText, args);
        info.m_showAlertIcon = false;
        info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
        info.m_responseCallback = callback;
        DialogManager.Get().ShowPopup(info, new DialogManager.DialogProcessCallback(this.OnFriendChallengeWaitingForOpponentDialogProcessed));
    }

    public void StartChallengeOrWaitForOpponent(string waitingDialogText, AlertPopup.ResponseCallback waitingCallback)
    {
        if (FriendChallengeMgr.Get().DidOpponentSelectDeck())
        {
            this.WaitForFriendChallengeToStart();
        }
        else
        {
            this.ShowFriendChallengeWaitingForOpponentDialog(waitingDialogText, waitingCallback);
        }
    }

    public void StopWaitingForFriendChallenge()
    {
        this.HideFriendChallengeWaitingForOpponentDialog();
    }

    public void WaitForFriendChallengeToStart()
    {
        GameMgr.Get().WaitForFriendChallengeToStart(FriendChallengeMgr.Get().GetScenarioId());
    }
}

