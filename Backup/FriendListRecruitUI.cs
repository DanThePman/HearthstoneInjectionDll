using System;
using UnityEngine;

public class FriendListRecruitUI : MonoBehaviour
{
    public FriendListUIElement m_CancelButton;
    private Network.RecruitInfo m_recruitInfo;
    public UberText m_recruitText;
    public GameObject m_success;

    public void Awake()
    {
        this.m_CancelButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCancelButtonPressed));
    }

    private void OnCancelButtonPressed(UIEvent e)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
        object[] args = new object[] { this.m_recruitInfo.Nickname };
        info.m_text = GameStrings.Format("GLOBAL_FRIENDLIST_CANCEL_RECRUIT_ALERT_MESSAGE", args);
        info.m_showAlertIcon = true;
        info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
        info.m_responseCallback = new AlertPopup.ResponseCallback(this.OnCancelPopupResponse);
        DialogManager.Get().ShowPopup(info, new DialogManager.DialogProcessCallback(this.OnCancelShown));
    }

    private void OnCancelPopupResponse(AlertPopup.Response response, object userData)
    {
        if (response == AlertPopup.Response.CONFIRM)
        {
            RecruitListMgr.Get().RecruitFriendCancel(this.m_recruitInfo.ID);
        }
    }

    private bool OnCancelShown(DialogBase dialog, object userData)
    {
        return true;
    }

    public void SetInfo(Network.RecruitInfo info)
    {
        this.m_recruitInfo = info;
        this.Update();
    }

    private void Update()
    {
        if (this.m_recruitInfo != null)
        {
            base.gameObject.SetActive(true);
            this.m_CancelButton.gameObject.SetActive(false);
            this.m_success.SetActive(false);
            switch (this.m_recruitInfo.Status)
            {
                case 2:
                    this.m_CancelButton.gameObject.SetActive(true);
                    break;

                case 3:
                    this.m_CancelButton.gameObject.SetActive(true);
                    break;

                case 4:
                    this.m_success.SetActive(true);
                    this.m_recruitText.Text = string.Format("Level\n{0}/50", this.m_recruitInfo.Level);
                    break;
            }
        }
        else
        {
            base.gameObject.SetActive(false);
        }
    }
}

