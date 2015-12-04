using PegasusShared;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class BattleNetDll : IBattleNet
{
    public static DelDllAcceptFriendlyChallenge DllAcceptFriendlyChallenge;
    public static DelDllAcceptPartyInvite DllAcceptPartyInvite;
    public static DelDllAnswerChallenge DllAnswerChallenge;
    public static DelDllAssignPartyRole DllAssignPartyRole;
    public static DelBattleNetStatus DllBattleNetStatus;
    public static DelDllCancelChallenge DllCancelChallenge;
    public static DelCancelFindGame DllCancelFindGame;
    public static DelClearBnetEvents DllClearBnetEvents;
    public static DelDllClearChallenges DllClearChallenges;
    public static DelClearErrors DllClearErrors;
    public static DelDllClearFriendsUpdates DllClearFriendsUpdates;
    public static DelDllClearNotifications DllClearNotifications;
    public static DelDllClearPartyAttribute DllClearPartyAttribute;
    public static DelDllClearPartyListenerEvents DllClearPartyListenerEvents;
    public static DelDllClearPartyUpdates DllClearPartyUpdates;
    public static DelDllClearPresence DllClearPresence;
    public static DelDllClearWhispers DllClearWhispers;
    public static DelDllCreateParty DllCreateParty;
    public static DelDllDeclineFriendlyChallenge DllDeclineFriendlyChallenge;
    public static DelDllDeclinePartyInvite DllDeclinePartyInvite;
    public static DelDllDissolveParty DllDissolveParty;
    public static DelDllFilterProfanity DllFilterProfanity;
    public static DelFindGame DllFindGame;
    public static DelGetAccountCountry DllGetAccountCountry;
    public static DelGetAccountRegion DllGetAccountRegion;
    public static DelDllGetAllPartyAttributes DllGetAllPartyAttributes;
    public static DelGetBnetEvents DllGetBnetEvents;
    public static DelGetBnetEventsSize DllGetBnetEventsSize;
    public static DelDllGetChallenges DllGetChallenges;
    public static DelDllGetCountPartyAttributes DllGetCountPartyAttributes;
    public static DelDllGetCountPartyInviteRequests DllGetCountPartyInviteRequests;
    public static DelDllGetCountPartyListenerEvents DllGetCountPartyListenerEvents;
    public static DelDllGetCountPartyMembers DllGetCountPartyMembers;
    public static DelDllGetCountPartySentInvites DllGetCountPartySentInvites;
    public static DelDllGetCountReceivedPartyInvites DllGetCountReceivedPartyInvites;
    public static DelGetCurrentRegion DllGetCurrentRegion;
    public static DelGetEnvironment DllGetEnvironment;
    public static DelGetErrors DllGetErrors;
    public static DelGetErrorsCount DllGetErrorsCount;
    public static DelDllGetFriendsInfo DllGetFriendsInfo;
    public static DelDllGetFriendsUpdates DllGetFriendsUpdates;
    public static DelGetLaunchOption DllGetLaunchOption;
    public static DelDllGetMaxPartyMembers DllGetMaxPartyMembers;
    public static DelGetMyGameAccountId DllGetMyGameAccountId;
    public static DelDllGetNotificationCount DllGetNotificationCount;
    public static DelDllGetNotifications DllGetNotifications;
    public static DelDllGetPartyAttributeBlob DllGetPartyAttributeBlob;
    public static DelDllGetPartyAttributeLong DllGetPartyAttributeLong;
    public static DelDllGetPartyAttributeString DllGetPartyAttributeString;
    public static DelDllGetPartyInviteRequests DllGetPartyInviteRequests;
    public static DelDllGetPartyListenerEvents DllGetPartyListenerEvents;
    public static DelDllGetPartyMembers DllGetPartyMembers;
    public static DelDllGetPartyPrivacy DllGetPartyPrivacy;
    public static DelDllGetPartySentInvites DllGetPartySentInvites;
    public static DelDllGetPartyUpdates DllGetPartyUpdates;
    public static DelDllGetPartyUpdatesInfo DllGetPartyUpdatesInfo;
    public static DelGetPlayRestrictions DllGetPlayRestrictions;
    public static DelDllGetPresence DllGetPresence;
    public static DelGetQueueEvent DllGetQueueEvent;
    public static DelGetQueueInfo DllGetQueueInfo;
    public static DelDllGetReceivedPartyInvites DllGetReceivedPartyInvites;
    public static DelGetShutdownMinutes DllGetShutdownMinutes;
    public static DelGetVersion DllGetVersion;
    public static DelGetVersionInt DllGetVersionInt;
    public static DelGetVersionSource DllGetVersionSource;
    public static DelGetVersionString DllGetVersionString;
    public static DelDllGetWhisperInfo DllGetWhisperInfo;
    public static DelDllGetWhispers DllGetWhispers;
    public static DelDllIgnoreInviteRequest DllIgnoreInviteRequest;
    public static DelInitConnect DllInitConnect;
    public static DelDllJoinParty DllJoinParty;
    public static DelDllKickPartyMember DllKickPartyMember;
    public static DelDllLeaveParty DllLeaveParty;
    public static DelDllManageFriendInvite DllManageFriendInvite;
    public static DelNextUtilPacket DllNextUtilPacket;
    public static DelDllNumChallenges DllNumChallenges;
    public static DelDllPresenceSize DllPresenceSize;
    public static DelProcessAurora DllProcessAurora;
    public static DelQueryAurora DllQueryAurora;
    public static DelDllRemoveFriend DllRemoveFriend;
    public static DelRequestCloseAurora DllRequestCloseAurora;
    public static DelDllRequestPartyInvite DllRequestPartyInvite;
    public static DelDllRequestPresenceFields DllRequestPresenceFields;
    public static DelDllRescindFriendlyChallenge DllRescindFriendlyChallenge;
    public static DelDllRevokePartyInvite DllRevokePartyInvite;
    public static DelDllSendFriendInvite DllSendFriendInvite;
    public static DelDllSendFriendlyChallengeInvite DllSendFriendlyChallengeInvite;
    public static DelDllSendPartyChatMessage DllSendPartyChatMessage;
    public static DelDllSendPartyInvite DllSendPartyInvite;
    public static DelSendUtilPacket DllSendUtilPacket;
    public static DelDllSendWhisper DllSendWhisper;
    public static DelDllSetMyFriendlyChallengeDeck DllSetMyFriendlyChallengeDeck;
    public static DelDllSetPartyAttributeBlob DllSetPartyAttributeBlob;
    public static DelDllSetPartyAttributeLong DllSetPartyAttributeLong;
    public static DelDllSetPartyAttributeString DllSetPartyAttributeString;
    public static DelDllSetPartyPrivacy DllSetPartyPrivacy;
    public static DelDllSetPresenceBlob DllSetPresenceBlob;
    public static DelDllSetPresenceBool DllSetPresenceBool;
    public static DelDllSetPresenceInt DllSetPresenceInt;
    public static DelDllSetPresenceString DllSetPresenceString;
    public static DelDllSetRichPresence DllSetRichPresence;
    private BattleNetLogSource m_logSource = new BattleNetLogSource("Main");
    private IntPtr s_DLL = IntPtr.Zero;
    private bool s_initialized;

    public void AcceptFriendlyChallenge(ref BattleNet.DllEntityId partyId)
    {
        if (DllAcceptFriendlyChallenge == null)
        {
            BattleNet.Log.LogError("AcceptFriendlyChallenge called while Dll is null.");
        }
        else
        {
            DllAcceptFriendlyChallenge(ref partyId);
        }
    }

    public void AcceptPartyInvite(ulong invitationId)
    {
        if (!this.s_initialized || (DllAcceptPartyInvite == null))
        {
            BattleNet.Log.LogError("AcceptPartyInvite called while uninitialized or Dll is null.");
        }
        else
        {
            DllAcceptPartyInvite(invitationId);
        }
    }

    public void AnswerChallenge(ulong challengeID, string answer)
    {
        if (DllAnswerChallenge == null)
        {
            BattleNet.Log.LogError("AnswerChallenge called while Dll is null.");
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(answer);
            DllAnswerChallenge(challengeID, ptr);
            MemUtils.FreePtr(ptr);
        }
    }

    public void ApplicationWasPaused()
    {
    }

    public void ApplicationWasUnpaused()
    {
    }

    public void AppQuit()
    {
        ConnectAPI.CloseAll();
        this.Reset();
    }

    public void AssignPartyRole(BattleNet.DllEntityId partyId, BattleNet.DllEntityId memberId, uint roleId)
    {
        if (!this.s_initialized || (DllAssignPartyRole == null))
        {
            BattleNet.Log.LogError("AssignPartyRole called while uninitialized or Dll is null.");
        }
        else
        {
            DllAssignPartyRole(ref partyId, ref memberId, roleId);
        }
    }

    public int BattleNetStatus()
    {
        if (DllBattleNetStatus == null)
        {
            return 0;
        }
        return DllBattleNetStatus();
    }

    public void CancelChallenge(ulong challengeID)
    {
        if (DllCancelChallenge == null)
        {
            BattleNet.Log.LogError("CancelChallenge called while Dll is null.");
        }
        else
        {
            DllCancelChallenge(challengeID);
        }
    }

    public void CancelFindGame()
    {
        if (DllCancelFindGame != null)
        {
            DllCancelFindGame();
        }
    }

    public bool CheckWebAuth(out string url)
    {
        url = null;
        return false;
    }

    public void ClearBnetEvents()
    {
        if (DllClearBnetEvents != null)
        {
            DllClearBnetEvents();
        }
    }

    public void ClearChallenges()
    {
        if (DllClearChallenges != null)
        {
            DllClearChallenges();
        }
    }

    public void ClearErrors()
    {
        if (DllClearErrors != null)
        {
            DllClearErrors();
        }
    }

    public void ClearFriendsUpdates()
    {
        if (DllClearFriendsUpdates != null)
        {
            DllClearFriendsUpdates();
        }
    }

    public void ClearNotifications()
    {
        if (DllClearNotifications != null)
        {
            DllClearNotifications();
        }
    }

    public void ClearPartyAttribute(BattleNet.DllEntityId partyId, string attributeKey)
    {
        if (!this.s_initialized || (DllClearPartyAttribute == null))
        {
            BattleNet.Log.LogError("ClearPartyAttribute called while uninitialized or Dll is null.");
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(attributeKey);
            DllClearPartyAttribute(ref partyId, ptr);
            MemUtils.FreePtr(ptr);
        }
    }

    public void ClearPartyListenerEvents()
    {
        if (this.s_initialized)
        {
            DllClearPartyListenerEvents();
        }
    }

    public void ClearPartyUpdates()
    {
        if (DllClearPartyUpdates != null)
        {
            DllClearPartyUpdates();
        }
    }

    public void ClearPresence()
    {
        if (DllClearPresence != null)
        {
            DllClearPresence();
        }
    }

    public void ClearWhispers()
    {
        if (DllClearWhispers != null)
        {
            DllClearWhispers();
        }
    }

    public void CloseAurora()
    {
        BattleNet.Log.LogError("CloseAurora() is deprecated in BattleNetDll. Use RequestCloseAurora() instead.");
    }

    public void CreateParty(string partyType, int privacyLevel, byte[] creatorBlob)
    {
        if (this.s_initialized && (DllCreateParty != null))
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(partyType);
            if (creatorBlob == null)
            {
                creatorBlob = new byte[0];
            }
            IntPtr ptr2 = MemUtils.PtrFromBytes(creatorBlob);
            DllCreateParty(ptr, privacyLevel, ptr2, creatorBlob.Length);
            MemUtils.FreePtr(ptr);
            MemUtils.FreePtr(ptr2);
        }
    }

    public void DeclineFriendlyChallenge(ref BattleNet.DllEntityId partyId)
    {
        if (DllDeclineFriendlyChallenge == null)
        {
            BattleNet.Log.LogError("DeclineFriendlyChallenge called while Dll is null.");
        }
        else
        {
            DllDeclineFriendlyChallenge(ref partyId);
        }
    }

    public void DeclinePartyInvite(ulong invitationId)
    {
        if (!this.s_initialized || (DllDeclinePartyInvite == null))
        {
            BattleNet.Log.LogError("DeclinePartyInvite called while uninitialized or Dll is null.");
        }
        else
        {
            DllDeclinePartyInvite(invitationId);
        }
    }

    public void DissolveParty(BattleNet.DllEntityId partyId)
    {
        if (!this.s_initialized || (DllDissolveParty == null))
        {
            BattleNet.Log.LogError("DissolveParty called while uninitialized or Dll is null.");
        }
        else
        {
            DllDissolveParty(ref partyId);
        }
    }

    public string FilterProfanity(string unfiltered)
    {
        if (DllFilterProfanity == null)
        {
            return unfiltered;
        }
        IntPtr ptr = MemUtils.Utf8PtrFromString(unfiltered);
        IntPtr ptr2 = DllFilterProfanity(ptr);
        MemUtils.FreePtr(ptr);
        return MemUtils.StringFromUtf8Ptr(ptr2);
    }

    public void FindGame(byte[] requestGuid, BnetGameType gameType, int missionId, long deckId, long aiDeckId, bool setScenarioIdAttr)
    {
        if (DllFindGame == null)
        {
            BattleNet.Log.LogError("FindGame called while Dll is null.");
        }
        else
        {
            IntPtr requestGuidPtr = MemUtils.PtrFromBytes(requestGuid);
            DllFindGame(requestGuidPtr, requestGuid.Length, gameType, missionId, deckId, aiDeckId, setScenarioIdAttr);
        }
    }

    public string GetAccountCountry()
    {
        if (DllGetAccountCountry == null)
        {
            return null;
        }
        IntPtr ptr = DllGetAccountCountry();
        if (ptr == IntPtr.Zero)
        {
            return null;
        }
        return Marshal.PtrToStringAnsi(ptr);
    }

    public int GetAccountRegion()
    {
        if (DllGetAccountRegion == null)
        {
            return -1;
        }
        return DllGetAccountRegion();
    }

    public void GetAllPartyAttributes(BattleNet.DllEntityId partyId, out string[] allKeys)
    {
        if ((!this.s_initialized || (DllGetCountPartyAttributes == null)) || (DllGetAllPartyAttributes == null))
        {
            allKeys = new string[0];
        }
        else
        {
            int num = DllGetCountPartyAttributes(ref partyId);
            allKeys = new string[num];
            IntPtr[] ptrArray = new IntPtr[num];
            DllGetAllPartyAttributes(ref partyId, ptrArray);
            for (int i = 0; i < allKeys.Length; i++)
            {
                allKeys[i] = MemUtils.StringFromUtf8Ptr(ptrArray[i]);
            }
        }
    }

    public void GetBnetEvents([Out] BattleNet.BnetEvent[] bnetEvents)
    {
        if (DllGetBnetEvents != null)
        {
            BattleNet.DllBnetEvent[] events = new BattleNet.DllBnetEvent[bnetEvents.Length];
            DllGetBnetEvents(events);
            for (int i = 0; i < bnetEvents.Length; i++)
            {
                bnetEvents[i] = (BattleNet.BnetEvent) events[i].bnetEvent;
            }
        }
    }

    public int GetBnetEventsSize()
    {
        if (DllGetBnetEventsSize == null)
        {
            return 0;
        }
        return DllGetBnetEventsSize();
    }

    public void GetChallenges([Out] BattleNet.DllChallengeInfo[] challenges)
    {
        if (DllGetChallenges != null)
        {
            DllGetChallenges(challenges);
        }
    }

    public int GetCountPartyMembers(BattleNet.DllEntityId partyId)
    {
        if (this.s_initialized && (DllGetCountPartyMembers != null))
        {
            return DllGetCountPartyMembers(ref partyId);
        }
        return 0;
    }

    public int GetCurrentRegion()
    {
        if (DllGetCurrentRegion == null)
        {
            return -1;
        }
        return DllGetCurrentRegion();
    }

    public string GetEnvironment()
    {
        if (DllGetEnvironment == null)
        {
            return string.Empty;
        }
        IntPtr ptr = DllGetEnvironment();
        if (ptr == IntPtr.Zero)
        {
            return string.Empty;
        }
        return Marshal.PtrToStringAnsi(ptr);
    }

    public void GetErrors([Out] BnetErrorInfo[] errors)
    {
        if (DllGetErrors != null)
        {
            BattleNet.DllErrorInfo[] infoArray = new BattleNet.DllErrorInfo[errors.Length];
            DllGetErrors(infoArray);
            for (int i = 0; i < errors.Length; i++)
            {
                errors[i] = BnetErrorInfo.CreateFromDll(infoArray[i]);
            }
        }
    }

    public int GetErrorsCount()
    {
        if (DllGetErrorsCount == null)
        {
            return 0;
        }
        return DllGetErrorsCount();
    }

    public void GetFriendsInfo(ref BattleNet.DllFriendsInfo info)
    {
        if (DllGetFriendsInfo != null)
        {
            DllGetFriendsInfo(ref info);
        }
    }

    public void GetFriendsUpdates([Out] BattleNet.FriendsUpdate[] updates)
    {
        if (DllGetFriendsUpdates != null)
        {
            int length = updates.Length;
            BattleNet.DllFriendsUpdate[] updateArray = new BattleNet.DllFriendsUpdate[length];
            DllGetFriendsUpdates(updateArray);
            for (int i = 0; i < length; i++)
            {
                updates[i].action = updateArray[i].action;
                updates[i].entity1 = BnetEntityId.CreateFromDll(updateArray[i].entity1);
                updates[i].entity2 = BnetEntityId.CreateFromDll(updateArray[i].entity2);
                updates[i].int1 = updateArray[i].int1;
                updates[i].string1 = MemUtils.StringFromUtf8Ptr(updateArray[i].string1);
                updates[i].string2 = MemUtils.StringFromUtf8Ptr(updateArray[i].string2);
                updates[i].string3 = MemUtils.StringFromUtf8Ptr(updateArray[i].string3);
                updates[i].long1 = updateArray[i].long1;
                updates[i].long2 = updateArray[i].long2;
                updates[i].long3 = updateArray[i].long3;
                updates[i].bool1 = updateArray[i].bool1;
            }
        }
    }

    private IntPtr GetFunction(string name)
    {
        IntPtr procAddress = DLLUtils.GetProcAddress(this.s_DLL, name);
        if (procAddress == IntPtr.Zero)
        {
            BattleNet.Log.LogError("could not load Connection." + name + "()");
            this.AppQuit();
        }
        return procAddress;
    }

    public string GetLaunchOption(string key)
    {
        string str2;
        IntPtr zero = IntPtr.Zero;
        try
        {
            zero = MemUtils.Utf8PtrFromString(key);
            str2 = MemUtils.StringFromUtf8Ptr(DllGetLaunchOption(zero));
        }
        catch (Exception)
        {
            str2 = string.Empty;
        }
        finally
        {
            if (zero != IntPtr.Zero)
            {
                MemUtils.FreePtr(zero);
            }
        }
        return str2;
    }

    public BattleNetLogSource GetLogSource()
    {
        return this.m_logSource;
    }

    public int GetMaxPartyMembers(BattleNet.DllEntityId partyId)
    {
        if (this.s_initialized && (DllGetMaxPartyMembers != null))
        {
            return DllGetMaxPartyMembers(ref partyId);
        }
        return 0;
    }

    public BattleNet.DllEntityId GetMyAccountId()
    {
        BattleNet.Log.LogError("GetMyAccountId() not yet implemented");
        return new BattleNet.DllEntityId { hi = 0L, lo = 0L };
    }

    public BattleNet.DllEntityId GetMyGameAccountId()
    {
        if (DllGetMyGameAccountId == null)
        {
            return new BattleNet.DllEntityId();
        }
        return DllGetMyGameAccountId();
    }

    public int GetNotificationCount()
    {
        if (DllGetNotificationCount == null)
        {
            return 0;
        }
        return DllGetNotificationCount();
    }

    public void GetNotifications([Out] BnetNotification[] notifications)
    {
        if (DllGetNotifications != null)
        {
            int length = notifications.Length;
            BattleNet.DllNotification[] notificationArray = new BattleNet.DllNotification[length];
            DllGetNotifications(notificationArray);
            for (int i = 0; i < length; i++)
            {
                notifications[i] = BnetNotification.CreateFromDll(notificationArray[i]);
            }
        }
    }

    public void GetPartyAttributeBlob(BattleNet.DllEntityId partyId, string attributeKey, out byte[] value)
    {
        if (!this.s_initialized || (DllGetPartyAttributeBlob == null))
        {
            value = null;
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(attributeKey);
            IntPtr zero = IntPtr.Zero;
            int blobSize = 0;
            DllGetPartyAttributeBlob(ref partyId, ptr, out zero, out blobSize);
            value = MemUtils.PtrToBytes(zero, blobSize);
            MemUtils.FreePtr(ptr);
        }
    }

    public bool GetPartyAttributeLong(BattleNet.DllEntityId partyId, string attributeKey, out long value)
    {
        if (!this.s_initialized || (DllGetPartyAttributeLong == null))
        {
            value = 0L;
            return false;
        }
        value = 0L;
        IntPtr ptr = MemUtils.Utf8PtrFromString(attributeKey);
        bool flag = DllGetPartyAttributeLong(ref partyId, ptr, out value);
        MemUtils.FreePtr(ptr);
        return flag;
    }

    public void GetPartyAttributeString(BattleNet.DllEntityId partyId, string attributeKey, out string value)
    {
        if (!this.s_initialized || (DllGetPartyAttributeString == null))
        {
            value = null;
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(attributeKey);
            IntPtr zero = IntPtr.Zero;
            DllGetPartyAttributeString(ref partyId, ptr, out zero);
            value = MemUtils.StringFromUtf8Ptr(zero);
            MemUtils.FreePtr(ptr);
        }
    }

    public void GetPartyInviteRequests(BattleNet.DllEntityId partyId, out InviteRequest[] requests)
    {
        if ((!this.s_initialized || (DllGetCountPartyInviteRequests == null)) || (DllGetPartyInviteRequests == null))
        {
            requests = new InviteRequest[0];
        }
        else
        {
            int num = DllGetCountPartyInviteRequests(ref partyId);
            requests = new InviteRequest[num];
            DllPartyInviteRequest[] requestArray = new DllPartyInviteRequest[num];
            DllGetPartyInviteRequests(ref partyId, requestArray);
            for (int i = 0; i < requests.Length; i++)
            {
                requests[i] = requestArray[i].ToPartyInviteRequest();
            }
        }
    }

    public void GetPartyListenerEvents(out BattleNet.PartyListenerEvent[] events)
    {
        if ((!this.s_initialized || (DllGetCountPartyListenerEvents == null)) || (DllGetPartyListenerEvents == null))
        {
            events = new BattleNet.PartyListenerEvent[0];
        }
        else
        {
            int num = DllGetCountPartyListenerEvents();
            events = new BattleNet.PartyListenerEvent[num];
            DllPartyListenerEvent[] eventArray = new DllPartyListenerEvent[num];
            DllGetPartyListenerEvents(eventArray);
            for (int i = 0; i < events.Length; i++)
            {
                events[i] = eventArray[i].ToPartyListenerEvent();
            }
        }
    }

    public void GetPartyMembers(BattleNet.DllEntityId partyId, out BattleNet.DllPartyMember[] members)
    {
        if ((!this.s_initialized || (DllGetCountPartyMembers == null)) || (DllGetPartyMembers == null))
        {
            members = new BattleNet.DllPartyMember[0];
        }
        else
        {
            int num = DllGetCountPartyMembers(ref partyId);
            members = new BattleNet.DllPartyMember[num];
            DllGetPartyMembers(ref partyId, members);
        }
    }

    public int GetPartyPrivacy(BattleNet.DllEntityId partyId)
    {
        if (this.s_initialized && (DllGetPartyPrivacy != null))
        {
            return DllGetPartyPrivacy(ref partyId);
        }
        return 4;
    }

    public void GetPartySentInvites(BattleNet.DllEntityId partyId, out PartyInvite[] invites)
    {
        if ((!this.s_initialized || (DllGetCountPartySentInvites == null)) || (DllGetPartySentInvites == null))
        {
            invites = new PartyInvite[0];
        }
        else
        {
            int num = DllGetCountPartySentInvites(ref partyId);
            invites = new PartyInvite[num];
            DllPartyInvite[] inviteArray = new DllPartyInvite[num];
            DllGetPartySentInvites(ref partyId, inviteArray);
            for (int i = 0; i < invites.Length; i++)
            {
                invites[i] = inviteArray[i].ToPartyInvite();
            }
        }
    }

    public void GetPartyUpdates([Out] BattleNet.PartyEvent[] updates)
    {
        if (DllGetPartyUpdates != null)
        {
            BattleNet.DllPartyEvent[] eventArray = new BattleNet.DllPartyEvent[updates.Length];
            DllGetPartyUpdates(eventArray);
            for (int i = 0; i < updates.Length; i++)
            {
                updates[i].CopyFrom(eventArray[i]);
            }
        }
    }

    public void GetPartyUpdatesInfo(ref BattleNet.DllPartyInfo info)
    {
        if (DllGetPartyUpdatesInfo != null)
        {
            DllGetPartyUpdatesInfo(ref info);
        }
    }

    public void GetPlayRestrictions(ref BattleNet.DllLockouts restrictions, bool reload)
    {
        if (DllGetPlayRestrictions != null)
        {
            DllGetPlayRestrictions(ref restrictions, reload);
        }
    }

    public void GetPresence([Out] BattleNet.PresenceUpdate[] updates)
    {
        if (DllGetPresence != null)
        {
            BattleNet.DllPresenceUpdate[] updateArray = new BattleNet.DllPresenceUpdate[updates.Length];
            DllGetPresence(updateArray);
            for (int i = 0; i < updates.Length; i++)
            {
                updates[i].CopyFrom(updateArray[i]);
            }
        }
    }

    public BattleNet.QueueEvent GetQueueEvent()
    {
        if (DllGetQueueEvent != null)
        {
            BattleNet.DllQueueEvent queueEvent = new BattleNet.DllQueueEvent();
            if (DllGetQueueEvent(ref queueEvent))
            {
                return new BattleNet.QueueEvent(queueEvent);
            }
        }
        return null;
    }

    public void GetQueueInfo(ref BattleNet.DllQueueInfo queueInfo)
    {
        if (DllGetQueueInfo != null)
        {
            DllGetQueueInfo(ref queueInfo);
        }
    }

    public void GetReceivedPartyInvites(out PartyInvite[] invites)
    {
        if ((!this.s_initialized || (DllGetCountReceivedPartyInvites == null)) || (DllGetReceivedPartyInvites == null))
        {
            invites = new PartyInvite[0];
        }
        else
        {
            int num = DllGetCountReceivedPartyInvites();
            invites = new PartyInvite[num];
            DllPartyInvite[] inviteArray = new DllPartyInvite[num];
            DllGetReceivedPartyInvites(inviteArray);
            for (int i = 0; i < invites.Length; i++)
            {
                invites[i] = inviteArray[i].ToPartyInvite();
            }
        }
    }

    public int GetShutdownMinutes()
    {
        if (DllGetShutdownMinutes == null)
        {
            return 0;
        }
        return DllGetShutdownMinutes();
    }

    public string GetStoredBNetIPAddress()
    {
        return null;
    }

    public string GetVersion()
    {
        if (DllGetVersion == null)
        {
            return string.Empty;
        }
        IntPtr ptr = DllGetVersion();
        if (ptr == IntPtr.Zero)
        {
            return string.Empty;
        }
        return Marshal.PtrToStringAnsi(ptr);
    }

    public int GetVersionInt()
    {
        if (DllGetVersionInt == null)
        {
            return 0;
        }
        return DllGetVersionInt();
    }

    public string GetVersionSource()
    {
        if (DllGetVersionSource == null)
        {
            return string.Empty;
        }
        IntPtr ptr = DllGetVersionSource();
        if (ptr == IntPtr.Zero)
        {
            return string.Empty;
        }
        return Marshal.PtrToStringAnsi(ptr);
    }

    public string GetVersionString()
    {
        if (DllGetVersionString == null)
        {
            return string.Empty;
        }
        IntPtr ptr = DllGetVersionString();
        if (ptr == IntPtr.Zero)
        {
            return string.Empty;
        }
        return Marshal.PtrToStringAnsi(ptr);
    }

    public void GetWhisperInfo(ref BattleNet.DllWhisperInfo info)
    {
        if (DllGetWhisperInfo != null)
        {
            DllGetWhisperInfo(ref info);
        }
    }

    public void GetWhispers([Out] BnetWhisper[] whispers)
    {
        if (DllGetWhispers != null)
        {
            int length = whispers.Length;
            BattleNet.DllWhisper[] whisperArray = new BattleNet.DllWhisper[length];
            DllGetWhispers(whisperArray);
            for (int i = 0; i < length; i++)
            {
                whispers[i] = BnetWhisper.CreateFromDll(whisperArray[i]);
            }
        }
    }

    public void IgnoreInviteRequest(BattleNet.DllEntityId partyId, BattleNet.DllEntityId requestedTargetId)
    {
        if (!this.s_initialized || (DllIgnoreInviteRequest == null))
        {
            BattleNet.Log.LogError("IgnoreInviteRequest called while uninitialized or Dll is null.");
        }
        else
        {
            DllIgnoreInviteRequest(ref partyId, ref requestedTargetId);
        }
    }

    public bool Init(bool internalMode)
    {
        if (this.s_initialized)
        {
            return true;
        }
        if (this.s_DLL == IntPtr.Zero)
        {
            this.s_DLL = FileUtils.LoadPlugin("Connect", true);
            if (this.s_DLL == IntPtr.Zero)
            {
                BattleNet.Log.LogError("could not load Connect.dll");
                return false;
            }
            DllInitConnect = (DelInitConnect) Marshal.GetDelegateForFunctionPointer(this.GetFunction("InitConnect"), typeof(DelInitConnect));
            DllQueryAurora = (DelQueryAurora) Marshal.GetDelegateForFunctionPointer(this.GetFunction("QueryAurora"), typeof(DelQueryAurora));
            DllRequestCloseAurora = (DelRequestCloseAurora) Marshal.GetDelegateForFunctionPointer(this.GetFunction("RequestCloseAurora"), typeof(DelRequestCloseAurora));
            DllProcessAurora = (DelProcessAurora) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ProcessAurora"), typeof(DelProcessAurora));
            DllSendUtilPacket = (DelSendUtilPacket) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SendUtilPacket"), typeof(DelSendUtilPacket));
            DllBattleNetStatus = (DelBattleNetStatus) Marshal.GetDelegateForFunctionPointer(this.GetFunction("BattleNetStatus"), typeof(DelBattleNetStatus));
            DllNextUtilPacket = (DelNextUtilPacket) Marshal.GetDelegateForFunctionPointer(this.GetFunction("NextUtilPacket"), typeof(DelNextUtilPacket));
            DllGetErrorsCount = (DelGetErrorsCount) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetErrorsCount"), typeof(DelGetErrorsCount));
            DllGetErrors = (DelGetErrors) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetErrors"), typeof(DelGetErrors));
            DllClearErrors = (DelClearErrors) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearErrors"), typeof(DelClearErrors));
            DllGetMyGameAccountId = (DelGetMyGameAccountId) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetMyGameAccountId"), typeof(DelGetMyGameAccountId));
            DllGetQueueInfo = (DelGetQueueInfo) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetQueueInfo"), typeof(DelGetQueueInfo));
            DllGetVersionSource = (DelGetVersionSource) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetVersionSource"), typeof(DelGetVersionSource));
            DllGetVersion = (DelGetVersion) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetAuroraVersion"), typeof(DelGetVersion));
            DllGetVersionString = (DelGetVersionString) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetVersionString"), typeof(DelGetVersionString));
            DllGetVersionInt = (DelGetVersionInt) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetVersionInt"), typeof(DelGetVersionInt));
            DllGetEnvironment = (DelGetEnvironment) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetEnvironment"), typeof(DelGetEnvironment));
            DllGetLaunchOption = (DelGetLaunchOption) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetBnetLaunchOption"), typeof(DelGetLaunchOption));
            DllGetAccountRegion = (DelGetAccountRegion) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetAccountRegion"), typeof(DelGetAccountRegion));
            DllGetAccountCountry = (DelGetAccountCountry) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetAccountCountry"), typeof(DelGetAccountCountry));
            DllGetCurrentRegion = (DelGetCurrentRegion) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetCurrentRegion"), typeof(DelGetCurrentRegion));
            DllGetPlayRestrictions = (DelGetPlayRestrictions) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPlayRestrictions"), typeof(DelGetPlayRestrictions));
            DllGetShutdownMinutes = (DelGetShutdownMinutes) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetShutdownMinutes"), typeof(DelGetShutdownMinutes));
            DllGetBnetEventsSize = (DelGetBnetEventsSize) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetBnetEventsSize"), typeof(DelGetBnetEventsSize));
            DllClearBnetEvents = (DelClearBnetEvents) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearBnetEvents"), typeof(DelClearBnetEvents));
            DllGetBnetEvents = (DelGetBnetEvents) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetBnetEvents"), typeof(DelGetBnetEvents));
            DllFindGame = (DelFindGame) Marshal.GetDelegateForFunctionPointer(this.GetFunction("FindGame"), typeof(DelFindGame));
            DllCancelFindGame = (DelCancelFindGame) Marshal.GetDelegateForFunctionPointer(this.GetFunction("CancelFindGame"), typeof(DelCancelFindGame));
            DllGetQueueEvent = (DelGetQueueEvent) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetQueueEvent"), typeof(DelGetQueueEvent));
            DllGetFriendsInfo = (DelDllGetFriendsInfo) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetFriendInfo"), typeof(DelDllGetFriendsInfo));
            DllClearFriendsUpdates = (DelDllClearFriendsUpdates) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearFriendUpdates"), typeof(DelDllClearFriendsUpdates));
            DllGetFriendsUpdates = (DelDllGetFriendsUpdates) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetFriendUpdates"), typeof(DelDllGetFriendsUpdates));
            DllSendFriendInvite = (DelDllSendFriendInvite) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SendInvite"), typeof(DelDllSendFriendInvite));
            DllManageFriendInvite = (DelDllManageFriendInvite) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ManageInvite"), typeof(DelDllManageFriendInvite));
            DllRemoveFriend = (DelDllRemoveFriend) Marshal.GetDelegateForFunctionPointer(this.GetFunction("RemoveFriend"), typeof(DelDllRemoveFriend));
            DllPresenceSize = (DelDllPresenceSize) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPresenceUpdatesSize"), typeof(DelDllPresenceSize));
            DllClearPresence = (DelDllClearPresence) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearPresenceUpdates"), typeof(DelDllClearPresence));
            DllGetPresence = (DelDllGetPresence) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPresenceUpdates"), typeof(DelDllGetPresence));
            DllSetPresenceBool = (DelDllSetPresenceBool) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetPresenceBool"), typeof(DelDllSetPresenceBool));
            DllSetPresenceInt = (DelDllSetPresenceInt) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetPresenceInt"), typeof(DelDllSetPresenceInt));
            DllSetPresenceString = (DelDllSetPresenceString) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetPresenceString"), typeof(DelDllSetPresenceString));
            DllSetPresenceBlob = (DelDllSetPresenceBlob) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetPresenceBlob"), typeof(DelDllSetPresenceBlob));
            DllSetRichPresence = (DelDllSetRichPresence) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetRichPresence"), typeof(DelDllSetRichPresence));
            DllRequestPresenceFields = (DelDllRequestPresenceFields) Marshal.GetDelegateForFunctionPointer(this.GetFunction("RequestPresenceFields"), typeof(DelDllRequestPresenceFields));
            DllNumChallenges = (DelDllNumChallenges) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetNumClientChallenges"), typeof(DelDllNumChallenges));
            DllClearChallenges = (DelDllClearChallenges) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearClientChallenges"), typeof(DelDllClearChallenges));
            DllGetChallenges = (DelDllGetChallenges) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetChallenges"), typeof(DelDllGetChallenges));
            DllAnswerChallenge = (DelDllAnswerChallenge) Marshal.GetDelegateForFunctionPointer(this.GetFunction("AnswerChallenge"), typeof(DelDllAnswerChallenge));
            DllCancelChallenge = (DelDllCancelChallenge) Marshal.GetDelegateForFunctionPointer(this.GetFunction("CancelChallenge"), typeof(DelDllCancelChallenge));
            DllSendFriendlyChallengeInvite = (DelDllSendFriendlyChallengeInvite) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SendFriendlyChallengeInvite"), typeof(DelDllSendFriendlyChallengeInvite));
            DllSetMyFriendlyChallengeDeck = (DelDllSetMyFriendlyChallengeDeck) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetMyFriendlyChallengeDeck"), typeof(DelDllSetMyFriendlyChallengeDeck));
            DllGetPartyUpdatesInfo = (DelDllGetPartyUpdatesInfo) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyUpdatesInfo"), typeof(DelDllGetPartyUpdatesInfo));
            DllGetPartyUpdates = (DelDllGetPartyUpdates) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyUpdates"), typeof(DelDllGetPartyUpdates));
            DllClearPartyUpdates = (DelDllClearPartyUpdates) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearPartyUpdates"), typeof(DelDllClearPartyUpdates));
            DllDeclineFriendlyChallenge = (DelDllDeclineFriendlyChallenge) Marshal.GetDelegateForFunctionPointer(this.GetFunction("DeclineFriendlyChallenge"), typeof(DelDllDeclineFriendlyChallenge));
            DllAcceptFriendlyChallenge = (DelDllAcceptFriendlyChallenge) Marshal.GetDelegateForFunctionPointer(this.GetFunction("AcceptFriendlyChallenge"), typeof(DelDllAcceptFriendlyChallenge));
            DllRescindFriendlyChallenge = (DelDllRescindFriendlyChallenge) Marshal.GetDelegateForFunctionPointer(this.GetFunction("RescindFriendlyChallenge"), typeof(DelDllRescindFriendlyChallenge));
            DllCreateParty = (DelDllCreateParty) Marshal.GetDelegateForFunctionPointer(this.GetFunction("CreateParty"), typeof(DelDllCreateParty));
            DllJoinParty = (DelDllJoinParty) Marshal.GetDelegateForFunctionPointer(this.GetFunction("JoinParty"), typeof(DelDllJoinParty));
            DllLeaveParty = (DelDllLeaveParty) Marshal.GetDelegateForFunctionPointer(this.GetFunction("LeaveParty"), typeof(DelDllLeaveParty));
            DllDissolveParty = (DelDllDissolveParty) Marshal.GetDelegateForFunctionPointer(this.GetFunction("DissolveParty"), typeof(DelDllDissolveParty));
            DllSetPartyPrivacy = (DelDllSetPartyPrivacy) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetPartyPrivacy"), typeof(DelDllSetPartyPrivacy));
            DllAssignPartyRole = (DelDllAssignPartyRole) Marshal.GetDelegateForFunctionPointer(this.GetFunction("AssignPartyRole"), typeof(DelDllAssignPartyRole));
            DllSendPartyInvite = (DelDllSendPartyInvite) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SendPartyInvite"), typeof(DelDllSendPartyInvite));
            DllAcceptPartyInvite = (DelDllAcceptPartyInvite) Marshal.GetDelegateForFunctionPointer(this.GetFunction("AcceptPartyInvite"), typeof(DelDllAcceptPartyInvite));
            DllDeclinePartyInvite = (DelDllDeclinePartyInvite) Marshal.GetDelegateForFunctionPointer(this.GetFunction("DeclinePartyInvite"), typeof(DelDllDeclinePartyInvite));
            DllRevokePartyInvite = (DelDllRevokePartyInvite) Marshal.GetDelegateForFunctionPointer(this.GetFunction("RevokePartyInvite"), typeof(DelDllRevokePartyInvite));
            DllRequestPartyInvite = (DelDllRequestPartyInvite) Marshal.GetDelegateForFunctionPointer(this.GetFunction("RequestPartyInvite"), typeof(DelDllRequestPartyInvite));
            DllIgnoreInviteRequest = (DelDllIgnoreInviteRequest) Marshal.GetDelegateForFunctionPointer(this.GetFunction("IgnoreInviteRequest"), typeof(DelDllIgnoreInviteRequest));
            DllKickPartyMember = (DelDllKickPartyMember) Marshal.GetDelegateForFunctionPointer(this.GetFunction("KickPartyMember"), typeof(DelDllKickPartyMember));
            DllSendPartyChatMessage = (DelDllSendPartyChatMessage) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SendPartyChatMessage"), typeof(DelDllSendPartyChatMessage));
            DllClearPartyAttribute = (DelDllClearPartyAttribute) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearPartyAttribute"), typeof(DelDllClearPartyAttribute));
            DllSetPartyAttributeLong = (DelDllSetPartyAttributeLong) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetPartyAttributeLong"), typeof(DelDllSetPartyAttributeLong));
            DllSetPartyAttributeString = (DelDllSetPartyAttributeString) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetPartyAttributeString"), typeof(DelDllSetPartyAttributeString));
            DllSetPartyAttributeBlob = (DelDllSetPartyAttributeBlob) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SetPartyAttributeBlob"), typeof(DelDllSetPartyAttributeBlob));
            DllGetPartyPrivacy = (DelDllGetPartyPrivacy) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyPrivacy"), typeof(DelDllGetPartyPrivacy));
            DllGetCountPartyMembers = (DelDllGetCountPartyMembers) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetCountPartyMembers"), typeof(DelDllGetCountPartyMembers));
            DllGetMaxPartyMembers = (DelDllGetMaxPartyMembers) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetMaxPartyMembers"), typeof(DelDllGetMaxPartyMembers));
            DllGetPartyMembers = (DelDllGetPartyMembers) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyMembers"), typeof(DelDllGetPartyMembers));
            DllGetCountReceivedPartyInvites = (DelDllGetCountReceivedPartyInvites) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetCountReceivedPartyInvites"), typeof(DelDllGetCountReceivedPartyInvites));
            DllGetReceivedPartyInvites = (DelDllGetReceivedPartyInvites) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetReceivedPartyInvites"), typeof(DelDllGetReceivedPartyInvites));
            DllGetCountPartySentInvites = (DelDllGetCountPartySentInvites) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetCountPartySentInvites"), typeof(DelDllGetCountPartySentInvites));
            DllGetPartySentInvites = (DelDllGetPartySentInvites) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartySentInvites"), typeof(DelDllGetPartySentInvites));
            DllGetCountPartyInviteRequests = (DelDllGetCountPartyInviteRequests) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetCountPartyInviteRequests"), typeof(DelDllGetCountPartyInviteRequests));
            DllGetPartyInviteRequests = (DelDllGetPartyInviteRequests) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyInviteRequests"), typeof(DelDllGetPartyInviteRequests));
            DllGetCountPartyAttributes = (DelDllGetCountPartyAttributes) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetCountPartyAttributes"), typeof(DelDllGetCountPartyAttributes));
            DllGetAllPartyAttributes = (DelDllGetAllPartyAttributes) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetAllPartyAttributes"), typeof(DelDllGetAllPartyAttributes));
            DllGetPartyAttributeLong = (DelDllGetPartyAttributeLong) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyAttributeLong"), typeof(DelDllGetPartyAttributeLong));
            DllGetPartyAttributeString = (DelDllGetPartyAttributeString) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyAttributeString"), typeof(DelDllGetPartyAttributeString));
            DllGetPartyAttributeBlob = (DelDllGetPartyAttributeBlob) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyAttributeBlob"), typeof(DelDllGetPartyAttributeBlob));
            DllGetCountPartyListenerEvents = (DelDllGetCountPartyListenerEvents) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetCountPartyListenerEvents"), typeof(DelDllGetCountPartyListenerEvents));
            DllGetPartyListenerEvents = (DelDllGetPartyListenerEvents) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetPartyListenerEvents"), typeof(DelDllGetPartyListenerEvents));
            DllClearPartyListenerEvents = (DelDllClearPartyListenerEvents) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearPartyListenerEvents"), typeof(DelDllClearPartyListenerEvents));
            DllSendWhisper = (DelDllSendWhisper) Marshal.GetDelegateForFunctionPointer(this.GetFunction("SendWhisper"), typeof(DelDllSendWhisper));
            DllGetWhisperInfo = (DelDllGetWhisperInfo) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetWhisperInfo"), typeof(DelDllGetWhisperInfo));
            DllGetWhispers = (DelDllGetWhispers) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetWhispers"), typeof(DelDllGetWhispers));
            DllClearWhispers = (DelDllClearWhispers) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearWhispers"), typeof(DelDllClearWhispers));
            DllGetNotificationCount = (DelDllGetNotificationCount) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetNotificationCount"), typeof(DelDllGetNotificationCount));
            DllGetNotifications = (DelDllGetNotifications) Marshal.GetDelegateForFunctionPointer(this.GetFunction("GetNotifications"), typeof(DelDllGetNotifications));
            DllClearNotifications = (DelDllClearNotifications) Marshal.GetDelegateForFunctionPointer(this.GetFunction("ClearNotifications"), typeof(DelDllClearNotifications));
            DllFilterProfanity = (DelDllFilterProfanity) Marshal.GetDelegateForFunctionPointer(this.GetFunction("FilterProfanity"), typeof(DelDllFilterProfanity));
        }
        DebugConsole.Get().Init();
        DllInitConnectInfo info = new DllInitConnectInfo {
            internalMode = internalMode,
            productVersion = BattleNet.ProductVersion(),
            bnetGameTypeFriend = 1,
            missionId1v1 = 2,
            buildVersion = 0x2acc,
            defaultAuroraPort = 0x45f
        };
        string managedString = !ApplicationMgr.IsInternal() ? "us.actual.battle.net" : "bn11-01.battle.net";
        info.defaultAuroraEnv = MemUtils.Utf8PtrFromString(managedString);
        DllInitConnect(info);
        MemUtils.FreePtr(info.defaultAuroraEnv);
        this.s_initialized = ConnectAPI.ConnectInit();
        return this.s_initialized;
    }

    public bool IsInitialized()
    {
        return this.s_initialized;
    }

    public void JoinParty(BattleNet.DllEntityId partyId, string partyType)
    {
        if (!this.s_initialized || (DllJoinParty == null))
        {
            BattleNet.Log.LogError("JoinParty called while uninitialized or Dll is null.");
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(partyType);
            DllJoinParty(ref partyId, ptr);
            MemUtils.FreePtr(ptr);
        }
    }

    public void KickPartyMember(BattleNet.DllEntityId partyId, BattleNet.DllEntityId memberId)
    {
        if (!this.s_initialized || (DllKickPartyMember == null))
        {
            BattleNet.Log.LogError("KickPartyMember called while uninitialized or Dll is null.");
        }
        else
        {
            DllKickPartyMember(ref partyId, ref memberId);
        }
    }

    public void LeaveParty(BattleNet.DllEntityId partyId)
    {
        if (!this.s_initialized || (DllLeaveParty == null))
        {
            BattleNet.Log.LogError("LeaveParty called while uninitialized or Dll is null.");
        }
        else
        {
            DllLeaveParty(ref partyId);
        }
    }

    public void ManageFriendInvite(int action, ulong inviteId)
    {
        if (DllManageFriendInvite == null)
        {
            BattleNet.Log.LogError("ManageFriendInvite called while Dll is null.");
        }
        else
        {
            DllManageFriendInvite(action, inviteId);
        }
    }

    public PegasusPacket NextUtilPacket()
    {
        if (DllNextUtilPacket != null)
        {
            AuroraPacket packet = new AuroraPacket();
            if (DllNextUtilPacket(ref packet))
            {
                int size = packet.size;
                byte[] destination = new byte[size];
                Marshal.Copy(packet.payload, destination, 0, size);
                return new PegasusPacket(packet.packetId, packet.clientContext, size, destination);
            }
        }
        return null;
    }

    public int NumChallenges()
    {
        if (DllNumChallenges == null)
        {
            return 0;
        }
        return DllNumChallenges();
    }

    public int PresenceSize()
    {
        if (DllPresenceSize == null)
        {
            return 0;
        }
        return DllPresenceSize();
    }

    public void ProcessAurora()
    {
        if (DllProcessAurora != null)
        {
            DllProcessAurora();
        }
    }

    public void ProvideWebAuthToken(string token)
    {
    }

    public void QueryAurora()
    {
        if (DllQueryAurora != null)
        {
            DllQueryAurora();
        }
    }

    public void RemoveFriend(BnetAccountId account)
    {
        if (DllRemoveFriend == null)
        {
            BattleNet.Log.LogError("RemoveFriend called while Dll is null.");
        }
        else
        {
            BattleNet.DllEntityId id = BnetEntityId.CreateForDll(account);
            DllRemoveFriend(ref id);
        }
    }

    public void RequestCloseAurora()
    {
        if (DllRequestCloseAurora != null)
        {
            DllRequestCloseAurora();
        }
    }

    public void RequestPartyInvite(BattleNet.DllEntityId partyId, BattleNet.DllEntityId whomToAskForApproval, BattleNet.DllEntityId whomToInvite, string szPartyType)
    {
        if (!this.s_initialized || (DllRequestPartyInvite == null))
        {
            BattleNet.Log.LogError("RequestPartyInvite called while uninitialized or Dll is null.");
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(szPartyType);
            DllRequestPartyInvite(ref partyId, ref whomToAskForApproval, ref whomToInvite, ptr);
            MemUtils.FreePtr(ptr);
        }
    }

    public void RequestPresenceFields(bool isGameAccountEntityId, [In] BattleNet.DllEntityId entityId, [In] BattleNet.DllPresenceFieldKey[] fieldList)
    {
        if (DllRequestPresenceFields == null)
        {
            BattleNet.Log.LogError("RequestPresenceFields called while Dll is null.");
        }
        else
        {
            DllRequestPresenceFields(isGameAccountEntityId, entityId, fieldList, fieldList.Length);
        }
    }

    public void RescindFriendlyChallenge(ref BattleNet.DllEntityId partyId)
    {
        if (DllRescindFriendlyChallenge == null)
        {
            BattleNet.Log.LogError("RescindFriendlyChallenge called while Dll is null.");
        }
        else
        {
            DllRescindFriendlyChallenge(ref partyId);
        }
    }

    public void Reset()
    {
        this.s_initialized = false;
        if (this.s_DLL != IntPtr.Zero)
        {
            if (DllRequestCloseAurora != null)
            {
                this.RequestCloseAurora();
            }
            if (!DLLUtils.FreeLibrary(this.s_DLL))
            {
                BattleNet.Log.LogError("error unloading Connect.DLL");
            }
            this.s_DLL = IntPtr.Zero;
        }
    }

    public void RevokePartyInvite(BattleNet.DllEntityId partyId, ulong invitationId)
    {
        if (!this.s_initialized || (DllRevokePartyInvite == null))
        {
            BattleNet.Log.LogError("RevokePartyInvite called while uninitialized or Dll is null.");
        }
        else
        {
            DllRevokePartyInvite(ref partyId, invitationId);
        }
    }

    public void SendFriendInvite(string sender, string target, bool byEmail)
    {
        if (DllSendFriendInvite == null)
        {
            BattleNet.Log.LogError("SendFriendInvite called while Dll is null.");
        }
        else
        {
            IntPtr intiver = MemUtils.Utf8PtrFromString(sender);
            IntPtr invitee = MemUtils.Utf8PtrFromString(target);
            DllSendFriendInvite(intiver, invitee, byEmail);
            MemUtils.FreePtr(intiver);
            MemUtils.FreePtr(invitee);
        }
    }

    public void SendFriendlyChallengeInvite(ref BattleNet.DllEntityId gameAccount, int scenarioId)
    {
        if (DllSendFriendlyChallengeInvite == null)
        {
            BattleNet.Log.LogError("SendFriendlyChallengeInvite called while Dll is null.");
        }
        else
        {
            DllSendFriendlyChallengeInvite(ref gameAccount, scenarioId);
        }
    }

    public void SendPartyChatMessage(BattleNet.DllEntityId partyId, string message)
    {
        if (!this.s_initialized || (DllSendPartyChatMessage == null))
        {
            BattleNet.Log.LogError("SendPartyChatMessage called while uninitialized or Dll is null.");
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(message);
            DllSendPartyChatMessage(ref partyId, ptr);
            MemUtils.FreePtr(ptr);
        }
    }

    public void SendPartyInvite(BattleNet.DllEntityId partyId, BattleNet.DllEntityId inviteeId, bool isReservation)
    {
        if (!this.s_initialized || (DllSendPartyInvite == null))
        {
            BattleNet.Log.LogError("SendPartyInvite called while uninitialized or Dll is null.");
        }
        else
        {
            DllSendPartyInvite(ref partyId, ref inviteeId, isReservation);
        }
    }

    public void SendUtilPacket(int packetId, int systemId, byte[] bytes, int size, int subID, int context, ulong route)
    {
        if (DllSendUtilPacket == null)
        {
            BattleNet.Log.LogError("Tried to SendUtilPacket while Dll function is null.");
        }
        else
        {
            DllSendUtilPacket(packetId, systemId, bytes, size, subID, context, route);
        }
    }

    public void SendWhisper(BnetGameAccountId gameAccount, string message)
    {
        if (DllSendWhisper == null)
        {
            BattleNet.Log.LogError("SendWhisper called while Dll is null.");
        }
        else
        {
            BattleNet.DllEntityId id = BnetEntityId.CreateForDll(gameAccount);
            IntPtr ptr = MemUtils.Utf8PtrFromString(message);
            DllSendWhisper(ref id, ptr);
            MemUtils.FreePtr(ptr);
        }
    }

    public void SetMyFriendlyChallengeDeck(ref BattleNet.DllEntityId partyId, long deckID)
    {
        if (DllSetMyFriendlyChallengeDeck == null)
        {
            BattleNet.Log.LogError("SetMyFriendlyChallengeDeck called while Dll is null.");
        }
        else
        {
            DllSetMyFriendlyChallengeDeck(ref partyId, deckID);
        }
    }

    public void SetPartyAttributeBlob(BattleNet.DllEntityId partyId, string attributeKey, [In] byte[] value)
    {
        if (!this.s_initialized || (DllSetPartyAttributeBlob == null))
        {
            BattleNet.Log.LogError("SetPartyAttributeBlob called while uninitialized or Dll is null.");
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(attributeKey);
            IntPtr ptr2 = MemUtils.PtrFromBytes(value);
            DllSetPartyAttributeBlob(ref partyId, ptr, ptr2, value.Length);
            MemUtils.FreePtr(ptr);
            MemUtils.FreePtr(ptr2);
        }
    }

    public void SetPartyAttributeLong(BattleNet.DllEntityId partyId, string attributeKey, [In] long value)
    {
        if (!this.s_initialized || (DllSetPartyAttributeLong == null))
        {
            BattleNet.Log.LogError("SetPartyAttributeLong called while uninitialized or Dll is null.");
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(attributeKey);
            DllSetPartyAttributeLong(ref partyId, ptr, value);
            MemUtils.FreePtr(ptr);
        }
    }

    public void SetPartyAttributeString(BattleNet.DllEntityId partyId, string attributeKey, [In] string value)
    {
        if (!this.s_initialized || (DllSetPartyAttributeString == null))
        {
            BattleNet.Log.LogError("SetPartyAttributeString called while uninitialized or Dll is null.");
        }
        else
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(attributeKey);
            IntPtr ptr2 = MemUtils.Utf8PtrFromString(value);
            DllSetPartyAttributeString(ref partyId, ptr, ptr2);
            MemUtils.FreePtr(ptr);
            MemUtils.FreePtr(ptr2);
        }
    }

    public void SetPartyPrivacy(BattleNet.DllEntityId partyId, int privacyLevel)
    {
        if (!this.s_initialized || (DllSetPartyPrivacy == null))
        {
            BattleNet.Log.LogError("SetPartyPrivacy called while uninitialized or Dll is null.");
        }
        else
        {
            DllSetPartyPrivacy(ref partyId, privacyLevel);
        }
    }

    public void SetPresenceBlob(uint field, byte[] bytes)
    {
        if (DllSetPresenceBlob != null)
        {
            if (bytes == null)
            {
                bytes = new byte[0];
            }
            IntPtr val = MemUtils.PtrFromBytes(bytes);
            DllSetPresenceBlob(field, val, bytes.Length);
            MemUtils.FreePtr(val);
        }
    }

    public void SetPresenceBool(uint field, bool val)
    {
        if (DllSetPresenceBool != null)
        {
            DllSetPresenceBool(field, val);
        }
    }

    public void SetPresenceInt(uint field, long val)
    {
        if (DllSetPresenceInt != null)
        {
            DllSetPresenceInt(field, val);
        }
    }

    public void SetPresenceString(uint field, string val)
    {
        if (DllSetPresenceString != null)
        {
            IntPtr ptr = MemUtils.Utf8PtrFromString(val);
            DllSetPresenceString(field, ptr);
            MemUtils.FreePtr(ptr);
        }
    }

    public void SetRichPresence([In] BattleNet.DllRichPresenceUpdate[] updates)
    {
        if (DllSetRichPresence != null)
        {
            DllSetRichPresence(updates, updates.Length);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AuroraPacket
    {
        public int packetId;
        public int clientContext;
        public IntPtr payload;
        public int size;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelBattleNetStatus();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelCancelFindGame();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelClearBnetEvents();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelClearErrors();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllAcceptFriendlyChallenge(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllAcceptPartyInvite(ulong invitationId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllAnswerChallenge(ulong challengeID, IntPtr answer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllAssignPartyRole(ref BattleNet.DllEntityId partyId, ref BattleNet.DllEntityId memberId, uint roleId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllCancelChallenge(ulong challengeID);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllClearChallenges();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllClearFriendsUpdates();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllClearNotifications();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllClearPartyAttribute(ref BattleNet.DllEntityId partyId, IntPtr attributeKey);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllClearPartyListenerEvents();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllClearPartyUpdates();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllClearPresence();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllClearWhispers();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllCreateParty(IntPtr partyType, int privacyLevel, [In] IntPtr creatorBlob, int creatorBlobLength);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllDeclineFriendlyChallenge(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllDeclinePartyInvite(ulong invitationId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllDissolveParty(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr DelDllFilterProfanity(IntPtr unfiltered);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool DelDllGetAllPartyAttributes(ref BattleNet.DllEntityId partyId, [Out] IntPtr[] allKeys);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetChallenges([Out] BattleNet.DllChallengeInfo[] challenges);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetCountPartyAttributes(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetCountPartyInviteRequests(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetCountPartyListenerEvents();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetCountPartyMembers(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetCountPartySentInvites(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetCountReceivedPartyInvites();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetFriendsInfo(ref BattleNet.DllFriendsInfo info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetFriendsUpdates([Out] BattleNet.DllFriendsUpdate[] updates);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetMaxPartyMembers(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetNotificationCount();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetNotifications([Out] BattleNet.DllNotification[] notifications);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPartyAttributeBlob(ref BattleNet.DllEntityId partyId, IntPtr attributeKey, out IntPtr value, out int blobSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool DelDllGetPartyAttributeLong(ref BattleNet.DllEntityId partyId, IntPtr attributeKey, out long value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPartyAttributeString(ref BattleNet.DllEntityId partyId, IntPtr attributeKey, out IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPartyInviteRequests(ref BattleNet.DllEntityId partyId, [Out] BattleNetDll.DllPartyInviteRequest[] requests);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPartyListenerEvents([Out] BattleNetDll.DllPartyListenerEvent[] events);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPartyMembers(ref BattleNet.DllEntityId partyId, [Out] BattleNet.DllPartyMember[] members);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllGetPartyPrivacy(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPartySentInvites(ref BattleNet.DllEntityId partyId, [Out] BattleNetDll.DllPartyInvite[] invites);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPartyUpdates([Out] BattleNet.DllPartyEvent[] updates);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPartyUpdatesInfo(ref BattleNet.DllPartyInfo info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetPresence([Out] BattleNet.DllPresenceUpdate[] updates);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetReceivedPartyInvites([Out] BattleNetDll.DllPartyInvite[] invites);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetWhisperInfo(ref BattleNet.DllWhisperInfo info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllGetWhispers([Out] BattleNet.DllWhisper[] whispers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllIgnoreInviteRequest(ref BattleNet.DllEntityId partyId, ref BattleNet.DllEntityId requestedTargetId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllJoinParty(ref BattleNet.DllEntityId partyId, IntPtr partyType);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllKickPartyMember(ref BattleNet.DllEntityId partyId, ref BattleNet.DllEntityId memberId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllLeaveParty(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllManageFriendInvite(int action, ulong inviteId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllNumChallenges();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelDllPresenceSize();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllRemoveFriend(ref BattleNet.DllEntityId account);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllRequestPartyInvite(ref BattleNet.DllEntityId partyId, ref BattleNet.DllEntityId whomToAskForApproval, ref BattleNet.DllEntityId whomToInvite, [In] IntPtr szPartyType);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllRequestPresenceFields(bool isGameAccountEntityId, [In] BattleNet.DllEntityId entityId, [In] BattleNet.DllPresenceFieldKey[] fieldList, int length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllRescindFriendlyChallenge(ref BattleNet.DllEntityId partyId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllRevokePartyInvite(ref BattleNet.DllEntityId partyId, ulong invitationId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSendFriendInvite(IntPtr intiver, IntPtr invitee, bool byEmail);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSendFriendlyChallengeInvite(ref BattleNet.DllEntityId gameAccount, int scenarioId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSendPartyChatMessage(ref BattleNet.DllEntityId partyId, IntPtr message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSendPartyInvite(ref BattleNet.DllEntityId partyId, ref BattleNet.DllEntityId inviteeId, bool isReservation);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSendWhisper(ref BattleNet.DllEntityId gameAccount, IntPtr message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetMyFriendlyChallengeDeck(ref BattleNet.DllEntityId partyId, long deckID);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetPartyAttributeBlob(ref BattleNet.DllEntityId partyId, IntPtr attributeKey, [In] IntPtr value, int blobSize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetPartyAttributeLong(ref BattleNet.DllEntityId partyId, IntPtr attributeKey, [In] long value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetPartyAttributeString(ref BattleNet.DllEntityId partyId, IntPtr attributeKey, [In] IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetPartyPrivacy(ref BattleNet.DllEntityId partyId, int privacyLevel);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetPresenceBlob(uint field, IntPtr val, int length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetPresenceBool(uint field, bool val);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetPresenceInt(uint field, long val);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetPresenceString(uint field, IntPtr val);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelDllSetRichPresence([In] BattleNet.DllRichPresenceUpdate[] updates, int length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelFindGame(IntPtr requestGuidPtr, int requestGuidLength, BnetGameType gameType, int mission, long deckId, long aiDeckId, bool setScenarioIdAttr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr DelGetAccountCountry();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelGetAccountRegion();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelGetBnetEvents([Out] BattleNet.DllBnetEvent[] events);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelGetBnetEventsSize();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelGetCurrentRegion();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr DelGetEnvironment();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelGetErrors([Out] BattleNet.DllErrorInfo[] errors);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelGetErrorsCount();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr DelGetLaunchOption(IntPtr key);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate BattleNet.DllEntityId DelGetMyGameAccountId();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelGetPlayRestrictions(ref BattleNet.DllLockouts restrictions, bool reload);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool DelGetQueueEvent(ref BattleNet.DllQueueEvent queueEvent);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelGetQueueInfo(ref BattleNet.DllQueueInfo queueInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelGetShutdownMinutes();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr DelGetVersion();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DelGetVersionInt();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr DelGetVersionSource();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr DelGetVersionString();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool DelInitConnect(BattleNetDll.DllInitConnectInfo info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool DelNextUtilPacket(ref BattleNetDll.AuroraPacket packet);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelProcessAurora();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool DelQueryAurora();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelRequestCloseAurora();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DelSendUtilPacket(int packetId, int systemId, byte[] bytes, int size, int subID, int context, ulong route);

    [StructLayout(LayoutKind.Sequential)]
    public struct DllInitConnectInfo
    {
        public bool internalMode;
        public int productVersion;
        public int bnetGameTypeFriend;
        public int missionId1v1;
        public int buildVersion;
        public IntPtr defaultAuroraEnv;
        public int defaultAuroraPort;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllPartyInvite
    {
        public ulong inviteId;
        public BattleNet.DllEntityId partyId;
        public IntPtr partyType;
        public IntPtr inviterName;
        public BattleNet.DllEntityId inviterId;
        public BattleNet.DllEntityId inviteeId;
        public uint flags;
        public PartyInvite ToPartyInvite()
        {
            PartyInvite invite = new PartyInvite {
                InviteId = this.inviteId,
                PartyId = PartyId.FromEntityId(this.partyId),
                PartyType = BnetParty.GetPartyTypeFromString(MemUtils.StringFromUtf8Ptr(this.partyType)),
                InviterName = MemUtils.StringFromUtf8Ptr(this.inviterName),
                InviterId = BnetGameAccountId.CreateFromDll(this.inviterId),
                InviteeId = BnetGameAccountId.CreateFromDll(this.inviteeId)
            };
            invite.SetFlags(this.flags);
            return invite;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllPartyInviteRequest
    {
        public IntPtr targetName;
        public BattleNet.DllEntityId targetId;
        public IntPtr requesterName;
        public BattleNet.DllEntityId requesterId;
        public InviteRequest ToPartyInviteRequest()
        {
            return new InviteRequest { TargetName = MemUtils.StringFromUtf8Ptr(this.targetName), TargetId = BnetGameAccountId.CreateFromDll(this.targetId), RequesterName = MemUtils.StringFromUtf8Ptr(this.requesterName), RequesterId = BnetGameAccountId.CreateFromDll(this.requesterId) };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DllPartyListenerEvent
    {
        public int IntType;
        public BattleNet.DllEntityId partyId;
        public BattleNet.DllEntityId subjectMemberId;
        public BattleNet.DllEntityId targetMemberId;
        public uint uintData;
        public ulong ulongData;
        public IntPtr stringData;
        public IntPtr stringData2;
        public IntPtr blobData;
        public int blobDataLength;
        public BattleNet.PartyListenerEvent ToPartyListenerEvent()
        {
            return new BattleNet.PartyListenerEvent { Type = (BattleNet.PartyListenerEventType) this.IntType, PartyId = PartyId.FromEntityId(this.partyId), SubjectMemberId = BnetGameAccountId.CreateFromDll(this.subjectMemberId), TargetMemberId = BnetGameAccountId.CreateFromDll(this.targetMemberId), UintData = this.uintData, UlongData = this.ulongData, StringData = MemUtils.StringFromUtf8Ptr(this.stringData), StringData2 = MemUtils.StringFromUtf8Ptr(this.stringData2), BlobData = MemUtils.PtrToBytes(this.blobData, this.blobDataLength) };
        }
    }
}

