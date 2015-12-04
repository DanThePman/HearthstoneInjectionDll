using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class DialogManager : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<DialogRequest> <>f__am$cache9;
    [CompilerGenerated]
    private static Predicate<NetCache.ProfileNotice> <>f__am$cacheA;
    private DialogBase m_currentDialog;
    private Queue<DialogRequest> m_dialogRequests = new Queue<DialogRequest>();
    private List<long> m_handledMedalNoticeIDs = new List<long>();
    private bool m_isReadyForSeasonEndPopup;
    private bool m_loadingDialog;
    private bool m_suppressed;
    public List<DialogTypeMapping> m_typeMapping = new List<DialogTypeMapping>();
    private bool m_waitingToShowSeasonEndDialog;
    private static DialogManager s_instance;

    public void AddToQueue(DialogRequest request)
    {
        if (!this.IsSuppressed())
        {
            this.m_dialogRequests.Enqueue(request);
            this.UpdateQueue();
        }
    }

    private void Awake()
    {
        s_instance = this;
        if (SceneMgr.Get() != null)
        {
            this.m_suppressed = SceneMgr.Get().IsModeRequested(SceneMgr.Mode.FATAL_ERROR);
        }
        NetCache.NetCacheProfileNotices netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
        if (netObject != null)
        {
            this.MaybeShowSeasonEndDialog(netObject.Notices, false);
        }
        NetCache.Get().RegisterNewNoticesListener(new NetCache.DelNewNoticesListener(this.OnNewNotices));
    }

    private void DestroyPopupAssetsIfPossible()
    {
        if (!this.m_loadingDialog)
        {
            foreach (DialogTypeMapping mapping in this.m_typeMapping)
            {
                AssetCache.ClearGameObject(FileUtils.GameAssetPathToName(mapping.m_prefabName));
            }
        }
    }

    public static DialogManager Get()
    {
        return s_instance;
    }

    public void GoBack()
    {
        if (this.m_currentDialog != null)
        {
            this.m_currentDialog.GoBack();
        }
    }

    public bool HandleKeyboardInput()
    {
        return (Input.GetKeyUp(KeyCode.Escape) && ((this.m_currentDialog != null) && this.m_currentDialog.HandleKeyboardInput()));
    }

    public bool IsSuppressed()
    {
        return this.m_suppressed;
    }

    private void LoadPopup(DialogRequest request)
    {
        <LoadPopup>c__AnonStorey376 storey = new <LoadPopup>c__AnonStorey376 {
            request = request
        };
        DialogTypeMapping mapping = this.m_typeMapping.Find(new Predicate<DialogTypeMapping>(storey.<>m__25D));
        if ((mapping == null) || (mapping.m_prefabName == null))
        {
            object[] messageArgs = new object[] { storey.request.m_type };
            Error.AddDevFatal("DialogManager.LoadPopup() - unhandled dialog type {0}", messageArgs);
        }
        else
        {
            this.m_loadingDialog = true;
            AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(mapping.m_prefabName), new AssetLoader.GameObjectCallback(this.OnPopupLoaded), null, true);
        }
    }

    private void MaybeShowSeasonEndDialog(List<NetCache.ProfileNotice> newNotices, bool fromNewNotices)
    {
        <MaybeShowSeasonEndDialog>c__AnonStorey375 storey = new <MaybeShowSeasonEndDialog>c__AnonStorey375();
        if (!this.IsSuppressed())
        {
            if (<>f__am$cacheA == null)
            {
                <>f__am$cacheA = obj => obj.Type == NetCache.ProfileNotice.NoticeType.GAINED_MEDAL;
            }
            NetCache.ProfileNoticeMedal medal = (NetCache.ProfileNoticeMedal) newNotices.Find(<>f__am$cacheA);
            if ((medal != null) && !this.m_handledMedalNoticeIDs.Contains(medal.NoticeID))
            {
                this.m_handledMedalNoticeIDs.Add(medal.NoticeID);
                storey.seasonID = medal.OriginData;
                NetCache.ProfileNoticeRewardCardBack back = (NetCache.ProfileNoticeRewardCardBack) newNotices.Find(new Predicate<NetCache.ProfileNotice>(storey.<>m__25B));
                NetCache.ProfileNoticeBonusStars stars = (NetCache.ProfileNoticeBonusStars) newNotices.Find(new Predicate<NetCache.ProfileNotice>(storey.<>m__25C));
                if (fromNewNotices)
                {
                    NetCache.Get().RefreshNetObject<NetCache.NetCacheMedalInfo>();
                    NetCache.Get().ReloadNetObject<NetCache.NetCacheRewardProgress>();
                }
                SeasonEndDialogRequestInfo info = new SeasonEndDialogRequestInfo {
                    m_noticeMedal = medal,
                    m_noticeBonusStars = stars,
                    m_noticeCardBack = back
                };
                DialogRequest request = new DialogRequest {
                    m_type = DialogType.SEASON_END,
                    m_info = info
                };
                base.StartCoroutine(this.ShowSeasonEndDialogWhenReady(request));
            }
        }
    }

    private void OnCurrentDialogHidden(DialogBase dialog, object userData)
    {
        if (dialog == this.m_currentDialog)
        {
            UnityEngine.Object.Destroy(this.m_currentDialog.gameObject);
            this.m_currentDialog = null;
            this.UpdateQueue();
        }
    }

    private void OnDestroy()
    {
        NetCache.Get().RemoveNewNoticesListener(new NetCache.DelNewNoticesListener(this.OnNewNotices));
        s_instance = null;
    }

    private void OnNewNotices(List<NetCache.ProfileNotice> newNotices)
    {
        this.MaybeShowSeasonEndDialog(newNotices, true);
    }

    private void OnPopupLoaded(string name, GameObject go, object callbackData)
    {
        this.m_loadingDialog = false;
        if (this.IsSuppressed() || (this.m_dialogRequests.Count == 0))
        {
            UnityEngine.Object.DestroyImmediate(go);
            this.DestroyPopupAssetsIfPossible();
        }
        else
        {
            DialogRequest request = this.m_dialogRequests.Dequeue();
            DialogBase component = go.GetComponent<DialogBase>();
            if (component == null)
            {
                UnityEngine.Debug.LogError(string.Format("DialogManager.OnPopupLoaded() - game object {0} has no {1} component", go, request.m_type));
                UnityEngine.Object.DestroyImmediate(go);
                this.UpdateQueue();
            }
            else
            {
                this.ProcessRequest(request, component);
            }
        }
    }

    private void ProcessAlertRequest(DialogRequest request, AlertPopup alertPopup)
    {
        AlertPopup.PopupInfo info = (AlertPopup.PopupInfo) request.m_info;
        alertPopup.SetInfo(info);
        alertPopup.Show();
    }

    private void ProcessExistingAccountRequest(DialogRequest request, ExistingAccountPopup exAcctPopup)
    {
        exAcctPopup.SetInfo((ExistingAccountPopup.Info) request.m_info);
        exAcctPopup.Show();
    }

    private void ProcessFriendlyChallengeRequest(DialogRequest request, FriendlyChallengeDialog friendlyChallengeDialog)
    {
        friendlyChallengeDialog.SetInfo((FriendlyChallengeDialog.Info) request.m_info);
        friendlyChallengeDialog.Show();
    }

    private void ProcessMedalRequest(DialogRequest request, SeasonEndDialog seasonEndDialog)
    {
        SeasonEndDialog.SeasonEndInfo info = null;
        if (request.m_isFake)
        {
            info = request.m_info as SeasonEndDialog.SeasonEndInfo;
            if (info == null)
            {
                return;
            }
        }
        else
        {
            SeasonEndDialogRequestInfo info2 = request.m_info as SeasonEndDialogRequestInfo;
            info = new SeasonEndDialog.SeasonEndInfo {
                m_noticesToAck = { info2.m_noticeMedal.NoticeID },
                m_seasonID = (int) info2.m_noticeMedal.OriginData,
                m_rank = 0x1a - info2.m_noticeMedal.StarLevel,
                m_chestRank = 0x1a - info2.m_noticeMedal.BestStarLevel,
                m_legendIndex = info2.m_noticeMedal.LegendRank,
                m_rankedRewards = info2.m_noticeMedal.Chest.Rewards
            };
            if (info2.m_noticeBonusStars != null)
            {
                info.m_boostedRank = 0x1a - info2.m_noticeBonusStars.StarLevel;
                info.m_bonusStars = info2.m_noticeBonusStars.Stars;
                info.m_noticesToAck.Add(info2.m_noticeBonusStars.NoticeID);
            }
        }
        seasonEndDialog.Init(info);
        seasonEndDialog.Show();
    }

    private void ProcessRequest(DialogRequest request, DialogBase dialog)
    {
        if ((request.m_callback != null) && !request.m_callback(dialog, request.m_userData))
        {
            this.UpdateQueue();
        }
        else
        {
            this.m_currentDialog = dialog;
            this.m_currentDialog.AddHideListener(new DialogBase.HideCallback(this.OnCurrentDialogHidden));
            if (request.m_type == DialogType.ALERT)
            {
                this.ProcessAlertRequest(request, (AlertPopup) dialog);
            }
            else if (request.m_type == DialogType.SEASON_END)
            {
                this.ProcessMedalRequest(request, (SeasonEndDialog) dialog);
            }
            else if ((request.m_type == DialogType.FRIENDLY_CHALLENGE) || (request.m_type == DialogType.TAVERN_BRAWL_CHALLENGE))
            {
                this.ProcessFriendlyChallengeRequest(request, (FriendlyChallengeDialog) dialog);
            }
            else if (request.m_type == DialogType.EXISTING_ACCOUNT)
            {
                this.ProcessExistingAccountRequest(request, (ExistingAccountPopup) dialog);
            }
        }
    }

    public void ReadyForSeasonEndPopup(bool ready)
    {
        this.m_isReadyForSeasonEndPopup = ready;
    }

    public void RemoveUniquePopupRequestFromQueue(string id)
    {
        <RemoveUniquePopupRequestFromQueue>c__AnonStorey374 storey = new <RemoveUniquePopupRequestFromQueue>c__AnonStorey374 {
            id = id
        };
        if (!string.IsNullOrEmpty(storey.id))
        {
            foreach (DialogRequest request in this.m_dialogRequests)
            {
                if (request.m_type == DialogType.ALERT)
                {
                    AlertPopup.PopupInfo info = (AlertPopup.PopupInfo) request.m_info;
                    if (info.m_id == storey.id)
                    {
                        this.m_dialogRequests = new Queue<DialogRequest>(Enumerable.Where<DialogRequest>(this.m_dialogRequests, new Func<DialogRequest, bool>(storey.<>m__258)));
                        break;
                    }
                }
            }
        }
    }

    public void ShowExistingAccountPopup(ExistingAccountPopup.ResponseCallback responseCallback, DialogProcessCallback callback)
    {
        DialogRequest request = new DialogRequest {
            m_type = DialogType.EXISTING_ACCOUNT
        };
        ExistingAccountPopup.Info info = new ExistingAccountPopup.Info {
            m_callback = responseCallback
        };
        request.m_info = info;
        request.m_callback = callback;
        this.AddToQueue(request);
    }

    public void ShowFriendlyChallenge(BnetPlayer challenger, bool challengeIsTavernBrawl, FriendlyChallengeDialog.ResponseCallback responseCallback, DialogProcessCallback callback)
    {
        DialogRequest request = new DialogRequest {
            m_type = !challengeIsTavernBrawl ? DialogType.FRIENDLY_CHALLENGE : DialogType.TAVERN_BRAWL_CHALLENGE
        };
        FriendlyChallengeDialog.Info info = new FriendlyChallengeDialog.Info {
            m_challenger = challenger,
            m_callback = responseCallback
        };
        request.m_info = info;
        request.m_callback = callback;
        this.AddToQueue(request);
    }

    public bool ShowingDialog()
    {
        return ((this.m_currentDialog != null) || (this.m_dialogRequests.Count > 0));
    }

    public void ShowMessageOfTheDay(string message)
    {
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_text = message
        };
        this.ShowPopup(info);
    }

    public void ShowPopup(AlertPopup.PopupInfo info)
    {
        this.ShowPopup(info, null, null);
    }

    public void ShowPopup(AlertPopup.PopupInfo info, DialogProcessCallback callback)
    {
        this.ShowPopup(info, callback, null);
    }

    public void ShowPopup(AlertPopup.PopupInfo info, DialogProcessCallback callback, object userData)
    {
        if (!this.IsSuppressed())
        {
            DialogRequest request = new DialogRequest {
                m_type = DialogType.ALERT,
                m_info = info,
                m_callback = callback,
                m_userData = userData
            };
            this.AddToQueue(request);
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowSeasonEndDialogWhenReady(DialogRequest request)
    {
        return new <ShowSeasonEndDialogWhenReady>c__Iterator258 { request = request, <$>request = request, <>f__this = this };
    }

    public bool ShowUniquePopup(AlertPopup.PopupInfo info)
    {
        return this.ShowUniquePopup(info, null, null);
    }

    public bool ShowUniquePopup(AlertPopup.PopupInfo info, DialogProcessCallback callback)
    {
        return this.ShowUniquePopup(info, callback, null);
    }

    public bool ShowUniquePopup(AlertPopup.PopupInfo info, DialogProcessCallback callback, object userData)
    {
        if (!string.IsNullOrEmpty(info.m_id))
        {
            foreach (DialogRequest request in this.m_dialogRequests)
            {
                if (request.m_type == DialogType.ALERT)
                {
                    AlertPopup.PopupInfo info2 = (AlertPopup.PopupInfo) request.m_info;
                    if (info2.m_id == info.m_id)
                    {
                        return false;
                    }
                }
            }
        }
        this.ShowPopup(info, callback, userData);
        return true;
    }

    public void Suppress(bool enable)
    {
        if (this.m_suppressed != enable)
        {
            this.m_suppressed = enable;
            if (enable)
            {
                if (this.m_currentDialog != null)
                {
                    UnityEngine.Object.DestroyImmediate(this.m_currentDialog.gameObject);
                    this.m_currentDialog = null;
                }
                this.m_dialogRequests.Clear();
                this.DestroyPopupAssetsIfPossible();
            }
        }
    }

    public void UpdateQueue()
    {
        if ((!this.IsSuppressed() && (this.m_currentDialog == null)) && !this.m_loadingDialog)
        {
            if (this.m_dialogRequests.Count == 0)
            {
                this.DestroyPopupAssetsIfPossible();
            }
            else
            {
                DialogRequest request = this.m_dialogRequests.Peek();
                this.LoadPopup(request);
            }
        }
    }

    public bool WaitingToShowSeasonEndDialog()
    {
        if (this.m_waitingToShowSeasonEndDialog || ((this.m_currentDialog != null) && (this.m_currentDialog is SeasonEndDialog)))
        {
            return true;
        }
        if (<>f__am$cache9 == null)
        {
            <>f__am$cache9 = obj => obj.m_type == DialogType.SEASON_END;
        }
        return (this.m_dialogRequests.ToList<DialogRequest>().Find(<>f__am$cache9) != null);
    }

    [CompilerGenerated]
    private sealed class <LoadPopup>c__AnonStorey376
    {
        internal DialogManager.DialogRequest request;

        internal bool <>m__25D(DialogManager.DialogTypeMapping x)
        {
            return (x.m_type == this.request.m_type);
        }
    }

    [CompilerGenerated]
    private sealed class <MaybeShowSeasonEndDialog>c__AnonStorey375
    {
        internal long seasonID;

        internal bool <>m__25B(NetCache.ProfileNotice notice)
        {
            if (notice.Type != NetCache.ProfileNotice.NoticeType.REWARD_CARD_BACK)
            {
                return false;
            }
            return (notice.OriginData == this.seasonID);
        }

        internal bool <>m__25C(NetCache.ProfileNotice notice)
        {
            if (notice.Type != NetCache.ProfileNotice.NoticeType.BONUS_STARS)
            {
                return false;
            }
            return (notice.OriginData == this.seasonID);
        }
    }

    [CompilerGenerated]
    private sealed class <RemoveUniquePopupRequestFromQueue>c__AnonStorey374
    {
        internal string id;

        internal bool <>m__258(DialogManager.DialogRequest r)
        {
            return (((r.m_info != null) && (r.m_info.GetType() == typeof(AlertPopup.PopupInfo))) && (((AlertPopup.PopupInfo) r.m_info).m_id != this.id));
        }
    }

    [CompilerGenerated]
    private sealed class <ShowSeasonEndDialogWhenReady>c__Iterator258 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal DialogManager.DialogRequest <$>request;
        internal DialogManager <>f__this;
        internal DialogManager.DialogRequest request;

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
                    this.<>f__this.m_waitingToShowSeasonEndDialog = true;
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_0088;

                case 3:
                    goto Label_00DE;

                case 4:
                    goto Label_0116;

                default:
                    goto Label_0149;
            }
            if ((NetCache.Get().GetNetObject<NetCache.NetCacheRewardProgress>() == null) || !this.<>f__this.m_isReadyForSeasonEndPopup)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_014B;
            }
        Label_0088:
            while (SceneMgr.Get().IsTransitioning())
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_014B;
            }
        Label_00DE:
            while ((SceneMgr.Get().GetMode() != SceneMgr.Mode.HUB) && (SceneMgr.Get().GetMode() != SceneMgr.Mode.LOGIN))
            {
                if ((SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT) && !SceneMgr.Get().IsTransitioning())
                {
                    SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                    break;
                }
                this.$current = null;
                this.$PC = 3;
                goto Label_014B;
            }
        Label_0116:
            while (SceneMgr.Get().IsTransitioning())
            {
                this.$current = null;
                this.$PC = 4;
                goto Label_014B;
            }
            this.<>f__this.AddToQueue(this.request);
            this.<>f__this.m_waitingToShowSeasonEndDialog = false;
            this.$PC = -1;
        Label_0149:
            return false;
        Label_014B:
            return true;
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

    public delegate bool DialogProcessCallback(DialogBase dialog, object userData);

    public class DialogRequest
    {
        public DialogManager.DialogProcessCallback m_callback;
        public object m_info;
        public bool m_isFake;
        public DialogManager.DialogType m_type;
        public object m_userData;
    }

    public enum DialogType
    {
        ALERT,
        SEASON_END,
        FRIENDLY_CHALLENGE,
        TAVERN_BRAWL_CHALLENGE,
        EXISTING_ACCOUNT
    }

    [Serializable]
    public class DialogTypeMapping
    {
        [CustomEditField(T=EditType.GAME_OBJECT)]
        public string m_prefabName;
        public DialogManager.DialogType m_type;
    }

    private class SeasonEndDialogRequestInfo
    {
        public NetCache.ProfileNoticeBonusStars m_noticeBonusStars;
        public NetCache.ProfileNoticeRewardCardBack m_noticeCardBack;
        public NetCache.ProfileNoticeMedal m_noticeMedal;
    }
}

