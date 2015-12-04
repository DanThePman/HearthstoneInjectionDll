using bnet;
using PegasusShared;
using PegasusUtil;
using SpectatorProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class Cheats
{
    [CompilerGenerated]
    private static BnetParty.MemberEventHandler <>f__am$cache10;
    [CompilerGenerated]
    private static BnetParty.ReceivedInviteHandler <>f__am$cache11;
    [CompilerGenerated]
    private static BnetParty.SentInviteHandler <>f__am$cache12;
    [CompilerGenerated]
    private static BnetParty.ReceivedInviteRequestHandler <>f__am$cache13;
    [CompilerGenerated]
    private static BnetParty.ChatMessageHandler <>f__am$cache14;
    [CompilerGenerated]
    private static BnetParty.PartyAttributeChangedHandler <>f__am$cache15;
    [CompilerGenerated]
    private static Func<PartyInfo, PartyId> <>f__am$cache16;
    [CompilerGenerated]
    private static Func<string, bool> <>f__am$cache18;
    [CompilerGenerated]
    private static Func<PartyType, string> <>f__am$cache19;
    [CompilerGenerated]
    private static BnetParty.CreateSuccessCallback <>f__am$cache1A;
    [CompilerGenerated]
    private static Func<BnetPlayer, bool> <>f__am$cache1B;
    [CompilerGenerated]
    private static Func<BnetPlayer, bool> <>f__am$cache1C;
    [CompilerGenerated]
    private static Func<BnetPlayer, bool> <>f__am$cache1D;
    [CompilerGenerated]
    private static Func<PartyMember, bool> <>f__am$cache1E;
    [CompilerGenerated]
    private static Func<PartyMember, bool> <>f__am$cache1F;
    [CompilerGenerated]
    private static Func<PrivacyLevel, string> <>f__am$cache20;
    [CompilerGenerated]
    private static Func<PartyId, bool> <>f__am$cache21;
    [CompilerGenerated]
    private static Func<uint, string> <>f__am$cache22;
    [CompilerGenerated]
    private static FixedRewardsMgr.DelPositionNonToastReward <>f__am$cache23;
    [CompilerGenerated]
    private static Func<string, bool> <>f__am$cache26;
    [CompilerGenerated]
    private static AssetLoader.GameObjectCallback <>f__am$cache2A;
    [CompilerGenerated]
    private static Func<BoosterDbId, bool> <>f__am$cache2B;
    [CompilerGenerated]
    private static Func<string, bool> <>f__am$cacheC;
    [CompilerGenerated]
    private static BnetParty.PartyErrorHandler <>f__am$cacheD;
    [CompilerGenerated]
    private static BnetParty.JoinedHandler <>f__am$cacheE;
    [CompilerGenerated]
    private static BnetParty.PrivacyLevelChangedHandler <>f__am$cacheF;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map32;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map33;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map34;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map35;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map36;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map37;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map38;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map39;
    private AlertPopup m_alert;
    private string m_board;
    private string[] m_lastUtilServerCmd;
    private bool m_loadingStoreChallengePrompt;
    private QuickLaunchState m_quickLaunchState = new QuickLaunchState();
    private StoreChallengePrompt m_storeChallengePrompt;
    private static bool s_hasSubscribedToPartyEvents;
    private static Cheats s_instance;
    private static readonly Map<KeyCode, string> s_opponentHeroKeyMap;
    private static readonly Map<KeyCode, ScenarioDbId> s_quickPlayKeyMap;

    static Cheats()
    {
        Map<KeyCode, ScenarioDbId> map = new Map<KeyCode, ScenarioDbId>();
        map.Add(KeyCode.F1, ScenarioDbId.PRACTICE_EXPERT_MAGE);
        map.Add(KeyCode.F2, ScenarioDbId.PRACTICE_EXPERT_HUNTER);
        map.Add(KeyCode.F3, ScenarioDbId.PRACTICE_EXPERT_WARRIOR);
        map.Add(KeyCode.F4, ScenarioDbId.PRACTICE_EXPERT_SHAMAN);
        map.Add(KeyCode.F5, ScenarioDbId.PRACTICE_EXPERT_DRUID);
        map.Add(KeyCode.F6, ScenarioDbId.PRACTICE_EXPERT_PRIEST);
        map.Add(KeyCode.F7, ScenarioDbId.PRACTICE_EXPERT_ROGUE);
        map.Add(KeyCode.F8, ScenarioDbId.PRACTICE_EXPERT_PALADIN);
        map.Add(KeyCode.F9, ScenarioDbId.PRACTICE_EXPERT_WARLOCK);
        s_quickPlayKeyMap = map;
        Map<KeyCode, string> map2 = new Map<KeyCode, string>();
        map2.Add(KeyCode.F1, "HERO_08");
        map2.Add(KeyCode.F2, "HERO_05");
        map2.Add(KeyCode.F3, "HERO_01");
        map2.Add(KeyCode.F4, "HERO_02");
        map2.Add(KeyCode.F5, "HERO_06");
        map2.Add(KeyCode.F6, "HERO_09");
        map2.Add(KeyCode.F7, "HERO_03");
        map2.Add(KeyCode.F8, "HERO_04");
        map2.Add(KeyCode.F9, "HERO_07");
        s_opponentHeroKeyMap = map2;
        s_hasSubscribedToPartyEvents = false;
    }

    private static string Cheat_ShowRewardBoxes(string parsableRewardBags)
    {
        if (SceneMgr.Get().IsInGame())
        {
            return "Cannot display reward boxes in gameplay.";
        }
        char[] separator = new char[] { ' ' };
        string[] source = parsableRewardBags.Trim().Split(separator, StringSplitOptions.RemoveEmptyEntries);
        if (source.Length < 2)
        {
            return ("Error parsing reply, should start with 'Success:' then player_id: " + parsableRewardBags);
        }
        if (source.Length < 3)
        {
            return ("No rewards returned by server: reply=" + parsableRewardBags);
        }
        List<NetCache.ProfileNotice> notices = new List<NetCache.ProfileNotice>();
        source = source.Skip<string>(2).ToArray<string>();
        for (int i = 0; i < source.Length; i++)
        {
            int result = 0;
            int index = i * 2;
            if (index >= source.Length)
            {
                break;
            }
            if (!int.TryParse(source[index], out result))
            {
                object[] objArray1 = new object[] { "Reward at index ", index, " (", source[index], ") is not an int: reply=", parsableRewardBags };
                return string.Concat(objArray1);
            }
            if (result == 0)
            {
                continue;
            }
            index++;
            if (index >= source.Length)
            {
                object[] objArray2 = new object[] { "No reward bag data at index ", index, ": reply=", parsableRewardBags };
                return string.Concat(objArray2);
            }
            long num4 = 0L;
            if (!long.TryParse(source[index], out num4))
            {
                object[] objArray3 = new object[] { "Reward Data at index ", index, " (", source[index], ") is not a long int: reply=", parsableRewardBags };
                return string.Concat(objArray3);
            }
            NetCache.ProfileNotice item = null;
            TAG_PREMIUM nORMAL = TAG_PREMIUM.NORMAL;
            switch (result)
            {
                case 1:
                case 12:
                {
                    NetCache.ProfileNoticeRewardBooster booster = new NetCache.ProfileNoticeRewardBooster {
                        Id = (int) num4,
                        Count = 1
                    };
                    item = booster;
                    goto Label_02A3;
                }
                case 2:
                {
                    NetCache.ProfileNoticeRewardGold gold = new NetCache.ProfileNoticeRewardGold {
                        Amount = (int) num4
                    };
                    item = gold;
                    goto Label_02A3;
                }
                case 3:
                {
                    NetCache.ProfileNoticeRewardDust dust = new NetCache.ProfileNoticeRewardDust {
                        Amount = (int) num4
                    };
                    item = dust;
                    goto Label_02A3;
                }
                case 4:
                case 5:
                case 6:
                case 7:
                    break;

                case 8:
                case 9:
                case 10:
                case 11:
                    nORMAL = TAG_PREMIUM.GOLDEN;
                    break;

                case 13:
                {
                    NetCache.ProfileNoticeRewardCardBack back = new NetCache.ProfileNoticeRewardCardBack {
                        CardBackID = (int) num4
                    };
                    item = back;
                    goto Label_02A3;
                }
                default:
                    Debug.LogError(string.Concat(new object[] { "Unknown Reward Bag Type: ", result, " (data=", num4, ") at index ", index, ": reply=", parsableRewardBags }));
                    goto Label_02A3;
            }
            NetCache.ProfileNoticeRewardCard card = new NetCache.ProfileNoticeRewardCard {
                CardID = GameUtils.TranslateDbIdToCardId((int) num4),
                Premium = nORMAL
            };
            item = card;
        Label_02A3:
            if (item != null)
            {
                notices.Add(item);
            }
        }
        List<RewardData> rewards = RewardUtils.GetRewards(notices);
        if (<>f__am$cache2A == null)
        {
            <>f__am$cache2A = delegate (string name, GameObject go, object callbackData) {
                RewardBoxesDisplay component = go.GetComponent<RewardBoxesDisplay>();
                component.SetRewards(callbackData as List<RewardData>);
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    component.m_Root.transform.position = new Vector3(0f, 14.7f, 3f);
                }
                else
                {
                    component.m_Root.transform.position = new Vector3(0f, 131.2f, -3.2f);
                }
                if (((Box.Get() != null) && (Box.Get().GetBoxCamera() != null)) && (Box.Get().GetBoxCamera().GetState() == BoxCamera.State.OPENED))
                {
                    Transform transform = component.m_Root.transform;
                    transform.position += new Vector3(-3f, 0f, 4.6f);
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        Transform transform2 = component.m_Root.transform;
                        transform2.position += new Vector3(0f, 0f, -7f);
                    }
                    else
                    {
                        component.transform.localScale = (Vector3) (Vector3.one * 0.6f);
                    }
                }
                component.AnimateRewards();
            };
        }
        AssetLoader.GameObjectCallback callback = <>f__am$cache2A;
        AssetLoader.Get().LoadGameObject("RewardBoxes", callback, rewards, false);
        return null;
    }

    private AlertPopup.PopupInfo GenerateAlertInfo(string rawArgs)
    {
        Map<string, string> map = this.ParseAlertArgs(rawArgs);
        AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
            m_showAlertIcon = false,
            m_headerText = "Header",
            m_text = "Message",
            m_responseDisplay = AlertPopup.ResponseDisplay.OK,
            m_okText = "OK",
            m_confirmText = "Confirm",
            m_cancelText = "Cancel"
        };
        foreach (KeyValuePair<string, string> pair in map)
        {
            string key = pair.Key;
            string strVal = pair.Value;
            if (key.Equals("header"))
            {
                info.m_headerText = strVal;
            }
            else if (key.Equals("text"))
            {
                info.m_text = strVal;
            }
            else if (key.Equals("response"))
            {
                strVal = strVal.ToLowerInvariant();
                if (strVal.Equals("ok"))
                {
                    info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
                }
                else if (strVal.Equals("confirm"))
                {
                    info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM;
                }
                else if (strVal.Equals("cancel"))
                {
                    info.m_responseDisplay = AlertPopup.ResponseDisplay.CANCEL;
                }
                else if (strVal.Equals("confirm_cancel") || strVal.Equals("cancel_confirm"))
                {
                    info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
                }
            }
            else if (key.Equals("icon"))
            {
                info.m_showAlertIcon = GeneralUtils.ForceBool(strVal);
            }
            else if (key.Equals("oktext"))
            {
                info.m_okText = strVal;
            }
            else if (key.Equals("confirmtext"))
            {
                info.m_confirmText = strVal;
            }
            else if (key.Equals("canceltext"))
            {
                info.m_cancelText = strVal;
            }
            else if (key.Equals("offset"))
            {
                string[] strArray = strVal.Split(new char[0]);
                Vector3 vector = new Vector3();
                if ((strArray.Length % 2) == 0)
                {
                    for (int i = 0; i < strArray.Length; i += 2)
                    {
                        string str3 = strArray[i].ToLowerInvariant();
                        string str = strArray[i + 1];
                        if (str3.Equals("x"))
                        {
                            vector.x = GeneralUtils.ForceFloat(str);
                        }
                        else if (str3.Equals("y"))
                        {
                            vector.y = GeneralUtils.ForceFloat(str);
                        }
                        else if (str3.Equals("z"))
                        {
                            vector.z = GeneralUtils.ForceFloat(str);
                        }
                    }
                }
                info.m_offset = vector;
            }
            else if (key.Equals("padding"))
            {
                info.m_padding = GeneralUtils.ForceFloat(strVal);
            }
        }
        return info;
    }

    public static Cheats Get()
    {
        return s_instance;
    }

    public string GetBoard()
    {
        return this.m_board;
    }

    private bool GetBonusStarsAndLevel(int lastSeasonRank, out int bonusStars, out int newSeasonRank)
    {
        int num = 0x1a - lastSeasonRank;
        bonusStars = num - 1;
        int num2 = 1;
        newSeasonRank = 0x1a - num2;
        switch (num)
        {
            case 1:
                num2 = 1;
                break;

            case 2:
                num2 = 1;
                break;

            case 3:
                num2 = 1;
                break;

            case 4:
                num2 = 2;
                break;

            case 5:
                num2 = 2;
                break;

            case 6:
                num2 = 3;
                break;

            case 7:
                num2 = 3;
                break;

            case 8:
                num2 = 4;
                break;

            case 9:
                num2 = 4;
                break;

            case 10:
                num2 = 5;
                break;

            case 11:
                num2 = 5;
                break;

            case 12:
                num2 = 6;
                break;

            case 13:
                num2 = 6;
                break;

            case 14:
                num2 = 6;
                break;

            case 15:
                num2 = 7;
                break;

            case 0x10:
                num2 = 7;
                break;

            case 0x11:
                num2 = 7;
                break;

            case 0x12:
                num2 = 8;
                break;

            case 0x13:
                num2 = 8;
                break;

            case 20:
                num2 = 8;
                break;

            case 0x15:
                num2 = 9;
                break;

            case 0x16:
                num2 = 9;
                break;

            case 0x17:
                num2 = 9;
                break;

            case 0x18:
                num2 = 10;
                break;

            case 0x19:
                num2 = 10;
                break;

            case 0x1a:
                num2 = 10;
                break;

            default:
                return false;
        }
        newSeasonRank = 0x1a - num2;
        return true;
    }

    private static string GetPartyInviteSummary(PartyInvite invite, int index)
    {
        object[] args = new object[] { (index < 0) ? string.Empty : string.Format("[{0}] ", index), invite.InviteId, invite.InviterId + " " + invite.InviterName, invite.InviteeId, new PartyInfo(invite.PartyId, invite.PartyType) };
        return string.Format("{0}: inviteId={1} sender={2} recipient={3} party={4}", args);
    }

    private static string GetPartySummary(PartyInfo info, int index)
    {
        PartyMember leader = BnetParty.GetLeader(info.Id);
        object[] args = new object[] { (index < 0) ? string.Empty : string.Format("[{0}] ", index), info, BnetParty.CountMembers(info.Id) + (!BnetParty.IsPartyFull(info.Id, true) ? string.Empty : "(full)"), BnetParty.GetSentInvites(info.Id).Length, BnetParty.GetPrivacyLevel(info.Id), (leader != null) ? leader.GameAccountId.ToString() : "null" };
        return string.Format("{0}{1}: members={2} invites={3} privacy={4} leader={5}", args);
    }

    private QuickLaunchAvailability GetQuickLaunchAvailability()
    {
        if (this.m_quickLaunchState.m_launching)
        {
            return QuickLaunchAvailability.ACTIVE_GAME;
        }
        if (SceneMgr.Get().IsInGame())
        {
            return QuickLaunchAvailability.ACTIVE_GAME;
        }
        if (GameMgr.Get().IsFindingGame())
        {
            return QuickLaunchAvailability.FINDING_GAME;
        }
        if (SceneMgr.Get().GetNextMode() != SceneMgr.Mode.INVALID)
        {
            return QuickLaunchAvailability.SCENE_TRANSITION;
        }
        if (!SceneMgr.Get().IsSceneLoaded())
        {
            return QuickLaunchAvailability.SCENE_TRANSITION;
        }
        if (LoadingScreen.Get().IsTransitioning())
        {
            return QuickLaunchAvailability.ACTIVE_GAME;
        }
        if ((CollectionManager.Get() != null) && CollectionManager.Get().IsFullyLoaded())
        {
            return QuickLaunchAvailability.OK;
        }
        return QuickLaunchAvailability.COLLECTION_NOT_READY;
    }

    private string GetQuickPlayMissionName(int missionId)
    {
        return this.GetQuickPlayMissionNameImpl(missionId, "NAME");
    }

    private string GetQuickPlayMissionName(KeyCode keyCode)
    {
        return this.GetQuickPlayMissionName(s_quickPlayKeyMap[keyCode]);
    }

    private string GetQuickPlayMissionNameImpl(int missionId, string columnName)
    {
        DbfRecord record = GameDbf.Scenario.GetRecord(missionId);
        if (record != null)
        {
            return record.GetLocString(columnName);
        }
        string str = missionId.ToString();
        try
        {
            str = ((ScenarioDbId) missionId).ToString();
        }
        catch (Exception)
        {
        }
        return str;
    }

    private string GetQuickPlayMissionShortName(int missionId)
    {
        return this.GetQuickPlayMissionNameImpl(missionId, "SHORT_NAME");
    }

    private string GetQuickPlayMissionShortName(KeyCode keyCode)
    {
        return this.GetQuickPlayMissionShortName(s_quickPlayKeyMap[keyCode]);
    }

    public bool HandleKeyboardInput()
    {
        return (ApplicationMgr.IsInternal() && this.HandleQuickPlayInput());
    }

    private bool HandleQuickPlayInput()
    {
        if (SceneMgr.Get() == null)
        {
            return false;
        }
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            return false;
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            this.PrintQuickPlayLegend();
            return false;
        }
        if (this.GetQuickLaunchAvailability() != QuickLaunchAvailability.OK)
        {
            return false;
        }
        ScenarioDbId iNVALID = ScenarioDbId.INVALID;
        string str = null;
        foreach (KeyValuePair<KeyCode, ScenarioDbId> pair in s_quickPlayKeyMap)
        {
            KeyCode key = pair.Key;
            ScenarioDbId id2 = pair.Value;
            if (Input.GetKeyDown(key))
            {
                iNVALID = id2;
                str = s_opponentHeroKeyMap[key];
                break;
            }
        }
        if (iNVALID == ScenarioDbId.INVALID)
        {
            return false;
        }
        this.m_quickLaunchState.m_mirrorHeroes = false;
        this.m_quickLaunchState.m_flipHeroes = false;
        this.m_quickLaunchState.m_skipMulligan = true;
        this.m_quickLaunchState.m_opponentHeroCardId = str;
        if ((Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt)) && (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)))
        {
            this.m_quickLaunchState.m_mirrorHeroes = true;
            this.m_quickLaunchState.m_skipMulligan = false;
            this.m_quickLaunchState.m_flipHeroes = false;
        }
        else if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
        {
            this.m_quickLaunchState.m_flipHeroes = false;
            this.m_quickLaunchState.m_skipMulligan = false;
            this.m_quickLaunchState.m_mirrorHeroes = false;
        }
        else if (Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt))
        {
            this.m_quickLaunchState.m_flipHeroes = true;
            this.m_quickLaunchState.m_skipMulligan = false;
            this.m_quickLaunchState.m_mirrorHeroes = false;
        }
        this.LaunchQuickGame((int) iNVALID);
        return true;
    }

    public static void Initialize()
    {
        s_instance = new Cheats();
        s_instance.InitializeImpl();
    }

    private void InitializeImpl()
    {
        CheatMgr mgr = CheatMgr.Get();
        if (ApplicationMgr.IsInternal())
        {
            mgr.RegisterCheatHandler("collectionfirstxp", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_collectionfirstxp), "Set the number of page and cover flips to zero", string.Empty, string.Empty);
            mgr.RegisterCheatHandler("board", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_board), "Set which board will be loaded on the next game", "<BRM|STW|GVG>", "BRM");
            mgr.RegisterCheatHandler("brode", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_brode), "Brode's personal cheat", string.Empty, string.Empty);
            mgr.RegisterCheatHandler("resettips", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_resettips), "Resets Innkeeper tips for collection manager", string.Empty, string.Empty);
            mgr.RegisterCheatHandler("questcomplete", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_questcomplete), "Shows the quest complete achievement screen", "<quest_id>", "58");
            mgr.RegisterCheatHandler("questprogress", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_questprogress), "Pop up a quest progress toast", "<title> <description> <progress> <maxprogress>", "Hello World 3 10");
            mgr.RegisterCheatHandler("questwelcome", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_questwelcome), "Open list of daily quests", "<fromLogin>", "true");
            mgr.RegisterCheatHandler("newquest", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_newquest), "Shows a new quest, only usable while a quest popup is active", null, null);
            mgr.RegisterCheatHandler("storepassword", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_storepassword), "Show store challenge popup", string.Empty, string.Empty);
            mgr.RegisterCheatHandler("retire", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_retire), "Retires your draft deck", string.Empty, string.Empty);
            mgr.RegisterCheatHandler("defaultcardback", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_defaultcardback), "Set your cardback as if through the options menu", "<cardback id>", null);
            mgr.RegisterCheatHandler("disconnect", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_disconnect), "Disconnects you from a game in progress.", null, null);
            mgr.RegisterCheatHandler("seasonroll", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_seasonroll), "Open the season end dialog", "<season number> <ending rank>", "20 7");
            mgr.RegisterCheatHandler("playnullsound", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_playnullsound), "Tell SoundManager to play a null sound.", null, null);
            mgr.RegisterCheatHandler("spectate", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_spectate), "Connects to a game server to spectate", "<ip_address> <port> <game_handle> <spectator_password> [gameType] [missionId]", null);
            mgr.RegisterCheatHandler("party", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_party), "Run a variety of party related commands", "[sub command] [subcommand args]", "list");
            mgr.RegisterCheatHandler("cheat", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_cheat), "Send a cheat command to the server", "<command> <arguments>", null);
            string[] textArray1 = new string[] { "c" };
            mgr.RegisterCheatAlias("cheat", textArray1);
            mgr.RegisterCheatHandler("autohand", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_autohand), "Set whether PhoneUI automatically hides your hand after playing a card", "<true/false>", "true");
            mgr.RegisterCheatHandler("fixedrewardcomplete", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_fixedrewardcomplete), "Shows the visual for a fixed reward", "<fixed_reward_map_id>", null);
            mgr.RegisterCheatHandler("iks", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_iks), "Open InnKeepersSpecial with a custom url", "<url>", null);
            mgr.RegisterCheatHandler("adventureChallengeUnlock", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_adventureChallengeUnlock), "Show adventure challenge unlock", "<wing number>", null);
            mgr.RegisterCheatHandler("quote", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_quote), string.Empty, "<character> <line> [sound]", "Innkeeper VO_INNKEEPER_FORGE_COMPLETE_22 VO_INNKEEPER_ARENA_COMPLETE");
            mgr.RegisterCheatHandler("demotext", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_demotext), string.Empty, "<line>", "HelloWorld!");
            mgr.RegisterCheatHandler("popuptext", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_popuptext), string.Empty, "<line>", "HelloWorld!");
            mgr.RegisterCheatHandler("favoritehero", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_favoritehero), "Change your favorite hero for a class (only works from CollectionManager)", "<class_id> <hero_card_id> <hero_premium>", null);
            mgr.RegisterCheatHandler("rewardboxes", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_rewardboxes), "Open the reward box screen with example rewards", "<card|cardback|gold|dust|random> <num_boxes>", string.Empty);
            mgr.RegisterCheatHandler("rankchange", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_rankchange), "Open the rankchange twoscoop", "<start_rank> <end_rank> [start_stars] [end_stars] [chest|winstreak]", "6 5 chest");
            mgr.RegisterCheatHandler("easyrank", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_easyrank), "Easier cheat command to set your rank on the util server", "<rank>", "16");
            mgr.RegisterCheatHandler("timescale", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_timescale), "Cheat to change the timescale", "<timescale>", "0.5");
            mgr.RegisterCheatHandler("onlygold", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_onlygold), "In collection manager, do you want to see gold, nogold, or both?", "<command name>", string.Empty);
            mgr.RegisterCheatHandler("help", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_help), "Get help for a specific command or list of commands", "<command name>", string.Empty);
            mgr.RegisterCheatHandler("example", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_example), "Run an example of this command if one exists", "<command name>", null);
            mgr.RegisterCheatHandler("tb", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_tavernbrawl), "Run a variety of Tavern Brawl related commands", "[subcommand] [subcommand args]", "view");
            mgr.RegisterCheatHandler("util", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_utilservercmd), "Run a cheat on the UTIL server you're connected to.", "[subcommand] [subcommand args]", "help");
            mgr.RegisterCheatHandler("game", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_gameservercmd), "Run a cheat on the GAME server you're connected to.", "[subcommand] [subcommand args]", "help");
            Network.Get().RegisterNetHandler(DebugCommandResponse.PacketID.ID, new Network.NetHandler(this.OnProcessCheat_utilservercmd_OnResponse), null);
            mgr.RegisterCheatHandler("scenario", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_scenario), "Launch a scenario.", "<scenarioId>", null);
            string[] textArray2 = new string[] { "mission" };
            mgr.RegisterCheatAlias("scenario", textArray2);
        }
        mgr.RegisterCheatHandler("has", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_HasOption), "Query whether a Game Option exists.", null, null);
        mgr.RegisterCheatHandler("get", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_GetOption), "Get the value of a Game Option.", null, null);
        mgr.RegisterCheatHandler("set", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_SetOption), "Set the value of a Game Option.", null, null);
        mgr.RegisterCheatHandler("getvar", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_GetVar), "Get the value of a client.config var.", null, null);
        mgr.RegisterCheatHandler("setvar", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_SetVar), "Set the value of a client.config var.", null, null);
        mgr.RegisterCheatHandler("nav", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_navigation), "Debug Navigation.GoBack", null, null);
        mgr.RegisterCheatHandler("delete", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_DeleteOption), "Delete a Game Option; the absence of option may trigger default behavior", null, null);
        string[] aliases = new string[] { "del" };
        mgr.RegisterCheatAlias("delete", aliases);
        mgr.RegisterCheatHandler("warning", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_warning), "Show a warning message", "<message>", "Test You're a cheater and you've been warned!");
        mgr.RegisterCheatHandler("fatal", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_fatal), "Brings up the Fatal Error screen", "<error to display>", "Hearthstone cheated and failed!");
        mgr.RegisterCheatHandler("exit", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_exit), "Exit the application", string.Empty, string.Empty);
        string[] textArray4 = new string[] { "quit" };
        mgr.RegisterCheatAlias("exit", textArray4);
        mgr.RegisterCheatHandler("log", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_log), null, null, null);
        mgr.RegisterCheatHandler("autodraft", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_autodraft), "Sets Arena autodraft on/off.", "<on | off>", "on");
        mgr.RegisterCheatHandler("alert", new CheatMgr.ProcessCheatCallback(this.OnProcessCheat_alert), "Show a popup alert", "header=<string> text=<string> icon=<bool> response=<ok|confirm|cancel|confirm_cancel> oktext=<string> confirmtext=<string>", "header=header text=body text icon=true response=confirm");
        string[] textArray5 = new string[] { "popup", "dialog" };
        mgr.RegisterCheatAlias("alert", textArray5);
    }

    public bool IsLaunchingQuickGame()
    {
        return this.m_quickLaunchState.m_launching;
    }

    private void LaunchQuickGame(int missionId)
    {
        string name;
        this.m_quickLaunchState.m_launching = true;
        long @long = Options.Get().GetLong(Option.LAST_CUSTOM_DECK_CHOSEN);
        CollectionDeck deck = CollectionManager.Get().GetDeck(@long);
        if (deck == null)
        {
            TAG_CLASS mAGE = TAG_CLASS.MAGE;
            @long = CollectionManager.Get().GetPreconDeck(mAGE).ID;
            name = string.Format("Precon {0}", GameStrings.GetClassName(mAGE));
        }
        else
        {
            name = deck.Name;
        }
        string quickPlayMissionName = this.GetQuickPlayMissionName(missionId);
        string message = string.Format("Launching {0}\nDeck: {1}", quickPlayMissionName, name);
        UIStatus.Get().AddInfo(message);
        UnityEngine.Time.timeScale = SceneDebugger.Get().m_MaxTimeScale;
        SceneMgr.Get().RegisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
        GameMgr.Get().SetPendingAutoConcede(true);
        GameMgr.Get().RegisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        GameMgr.Get().FindGame(GameType.GT_VS_AI, missionId, @long, 0L);
    }

    private bool OnAlertProcessed(DialogBase dialog, object userData)
    {
        this.m_alert = (AlertPopup) dialog;
        return true;
    }

    private void OnAlertResponse(AlertPopup.Response response, object userData)
    {
        this.m_alert = null;
    }

    private bool OnFindGameEvent(FindGameEventData eventData, object userData)
    {
        FindGameState state = eventData.m_state;
        switch (state)
        {
            case FindGameState.BNET_ERROR:
            case FindGameState.SERVER_GAME_CANCELED:
                break;

            default:
                if ((state != FindGameState.CLIENT_CANCELED) && (state != FindGameState.CLIENT_ERROR))
                {
                    goto Label_0059;
                }
                break;
        }
        GameMgr.Get().UnregisterFindGameEvent(new GameMgr.FindGameCallback(this.OnFindGameEvent));
        this.m_quickLaunchState = new QuickLaunchState();
    Label_0059:
        return false;
    }

    private bool OnProcessCheat_adventureChallengeUnlock(string func, string[] args, string rawArgs)
    {
        int num;
        if (args.Length < 1)
        {
            return false;
        }
        if (!int.TryParse(args[0].ToLowerInvariant(), out num))
        {
            return false;
        }
        List<int> classChallengeUnlocks = new List<int> {
            num
        };
        AdventureMissionDisplay.Get().ShowClassChallengeUnlock(classChallengeUnlocks);
        return true;
    }

    private bool OnProcessCheat_alert(string func, string[] args, string rawArgs)
    {
        AlertPopup.PopupInfo info = this.GenerateAlertInfo(rawArgs);
        if (this.m_alert == null)
        {
            DialogManager.Get().ShowPopup(info, new DialogManager.DialogProcessCallback(this.OnAlertProcessed));
        }
        else
        {
            this.m_alert.UpdateInfo(info);
        }
        return true;
    }

    private bool OnProcessCheat_autodraft(string func, string[] args, string rawArgs)
    {
        string str = args[0];
        bool flag = string.IsNullOrEmpty(str) || GeneralUtils.ForceBool(str);
        VarsInternal.Get().Set("Arena.AutoDraft", !flag ? "false" : "true");
        if (flag && (DraftDisplay.Get() != null))
        {
            DraftDisplay.Get().StartCoroutine(DraftDisplay.Get().RunAutoDraftCheat());
        }
        string message = string.Format("Arena autodraft turned {0}.", !flag ? "off" : "on");
        Debug.Log(message);
        UIStatus.Get().AddInfo(message);
        return true;
    }

    private bool OnProcessCheat_autohand(string func, string[] args, string rawArgs)
    {
        bool flag;
        string str2;
        if (args.Length == 0)
        {
            return false;
        }
        string strVal = args[0];
        if (!GeneralUtils.TryParseBool(strVal, out flag))
        {
            return false;
        }
        if (InputManager.Get() == null)
        {
            return false;
        }
        if (flag)
        {
            str2 = "auto hand hiding is on";
        }
        else
        {
            str2 = "auto hand hiding is off";
        }
        Debug.Log(str2);
        UIStatus.Get().AddInfo(str2);
        InputManager.Get().SetHideHandAfterPlayingCard(flag);
        return true;
    }

    private bool OnProcessCheat_board(string func, string[] args, string rawArgs)
    {
        this.m_board = args[0].ToUpperInvariant();
        return true;
    }

    private bool OnProcessCheat_brode(string func, string[] args, string rawArgs)
    {
        NotificationManager.Get().CreateInnkeeperQuote(new Vector3(133.1f, NotificationManager.DEPTH, 54.2f), GameStrings.Get("VO_INNKEEPER_FORGE_1WIN"), "VO_INNKEEPER_ARENA_1WIN", 0f, null);
        return true;
    }

    private bool OnProcessCheat_cheat(string func, string[] args, string rawArgs)
    {
        string command = rawArgs;
        Network.SendConsoleCmdToServer(command);
        return true;
    }

    private bool OnProcessCheat_collectionfirstxp(string func, string[] args, string rawArgs)
    {
        Options.Get().SetInt(Option.COVER_MOUSE_OVERS, 0);
        Options.Get().SetInt(Option.PAGE_MOUSE_OVERS, 0);
        return true;
    }

    private bool OnProcessCheat_defaultcardback(string func, string[] args, string rawArgs)
    {
        int num;
        if (args.Length == 0)
        {
            return false;
        }
        if (!int.TryParse(args[0].ToLowerInvariant(), out num))
        {
            return false;
        }
        ConnectAPI.SetDefaultCardBack(num);
        return true;
    }

    private bool OnProcessCheat_DeleteOption(string func, string[] args, string rawArgs)
    {
        Option option;
        string str = args[0];
        try
        {
            option = EnumUtils.GetEnum<Option>(str, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
        Options.Get().DeleteOption(option);
        string message = string.Format("DeleteOption: {0}. HasOption = {1}.", EnumUtils.GetString<Option>(option), Options.Get().HasOption(option));
        Debug.Log(message);
        UIStatus.Get().AddInfo(message);
        return true;
    }

    private bool OnProcessCheat_demotext(string func, string[] args, string rawArgs)
    {
        if (args.Length < 1)
        {
            return false;
        }
        string demoText = args[0];
        DemoMgr.Get().CreateDemoText(demoText);
        return true;
    }

    private bool OnProcessCheat_disconnect(string func, string[] args, string rawArgs)
    {
        if (!Network.IsConnectedToGameServer())
        {
            return false;
        }
        bool flag = ((args == null) || (args.Length == 0)) || (args[0] != "force");
        object[] objArray1 = new object[] { ReconnectMgr.Get().IsReconnectEnabled() };
        Log.LoadingScreen.Print("Cheats.OnProcessCheat_disconnect() - reconnect={0}", objArray1);
        if (flag)
        {
            if (ReconnectMgr.Get().IsReconnectEnabled())
            {
                Network.DisconnectFromGameServer();
            }
            else
            {
                Network.Concede();
            }
        }
        else
        {
            ConnectAPI.SimulateUncleanDisconnectFromGameServer();
        }
        return true;
    }

    private bool OnProcessCheat_easyrank(string func, string[] args, string rawArgs)
    {
        string str = args[0].ToLower();
        if (string.IsNullOrEmpty(str))
        {
            str = "20";
        }
        int result = 0x19;
        if (!int.TryParse(str, out result))
        {
            return false;
        }
        int a = 0x1a - result;
        int num3 = 0;
        num3 += Mathf.Min(a, 5) * 2;
        if (a > 5)
        {
            num3 += Mathf.Min(a - 5, 5) * 3;
        }
        if (a > 10)
        {
            num3 += Mathf.Min(a - 10, 5) * 4;
        }
        if (a > 15)
        {
            num3 += Mathf.Min(a - 15, 10) * 5;
        }
        CheatMgr.Get().ProcessCheat(string.Format("util ranked set starlevel={0}", a));
        CheatMgr.Get().ProcessCheat(string.Format("util ranked set beststarlevel={0}", a));
        CheatMgr.Get().ProcessCheat(string.Format("util ranked set stars={0}", num3));
        return true;
    }

    private bool OnProcessCheat_example(string func, string[] args, string rawArgs)
    {
        if ((args.Length < 1) || string.IsNullOrEmpty(args[0]))
        {
            return false;
        }
        string key = args[0];
        string str2 = string.Empty;
        if (!CheatMgr.Get().cheatExamples.TryGetValue(key, out str2))
        {
            return false;
        }
        CheatMgr.Get().ProcessCheat(key + " " + str2);
        return true;
    }

    private bool OnProcessCheat_exit(string func, string[] args, string rawArgs)
    {
        GeneralUtils.ExitApplication();
        return true;
    }

    private bool OnProcessCheat_fatal(string func, string[] args, string rawArgs)
    {
        Error.AddFatal(rawArgs);
        return true;
    }

    private bool OnProcessCheat_favoritehero(string func, string[] args, string rawArgs)
    {
        int num;
        TAG_CLASS tag_class;
        int num2;
        TAG_PREMIUM tag_premium;
        if (!(SceneMgr.Get().GetScene() is CollectionManagerScene))
        {
            Debug.LogWarning("OnProcessCheat_favoritehero must be used from the CollectionManagaer!");
            return false;
        }
        if (args.Length != 3)
        {
            return false;
        }
        if (!int.TryParse(args[0].ToLowerInvariant(), out num))
        {
            return false;
        }
        if (!EnumUtils.TryCast<TAG_CLASS>(num, out tag_class))
        {
            return false;
        }
        string str = args[1];
        if (!int.TryParse(args[2].ToLowerInvariant(), out num2))
        {
            return false;
        }
        if (!EnumUtils.TryCast<TAG_PREMIUM>(num2, out tag_premium))
        {
            return false;
        }
        NetCache.CardDefinition hero = new NetCache.CardDefinition {
            Name = str,
            Premium = tag_premium
        };
        object[] objArray1 = new object[] { hero, tag_class };
        Log.Rachelle.Print("OnProcessCheat_favoritehero setting favorite hero to {0} for class {1}", objArray1);
        Network.SetFavoriteHero(tag_class, hero);
        return true;
    }

    private bool OnProcessCheat_fixedrewardcomplete(string func, string[] args, string rawArgs)
    {
        int num;
        Scene scene = SceneMgr.Get().GetScene();
        if ((args.Length < 1) || string.IsNullOrEmpty(args[0]))
        {
            return false;
        }
        if (!GeneralUtils.TryParseInt(args[0], out num))
        {
            return false;
        }
        if ((scene is Login) || (scene is Hub))
        {
            return FixedRewardsMgr.Get().Cheat_ShowFixedReward(num, new FixedRewardsMgr.DelPositionNonToastReward(this.PositionLoginFixedReward), (Vector3) Login.REWARD_PUNCH_SCALE, (Vector3) Login.REWARD_SCALE);
        }
        if (!(scene is AdventureScene))
        {
            return false;
        }
        if (<>f__am$cache23 == null)
        {
            <>f__am$cache23 = delegate (Reward reward) {
                reward.transform.localPosition = AdventureScene.REWARD_LOCAL_POS;
            };
        }
        return FixedRewardsMgr.Get().Cheat_ShowFixedReward(num, <>f__am$cache23, (Vector3) AdventureScene.REWARD_PUNCH_SCALE, (Vector3) AdventureScene.REWARD_SCALE);
    }

    private bool OnProcessCheat_gameservercmd(string func, string[] args, string rawArgs)
    {
        return true;
    }

    private bool OnProcessCheat_GetOption(string func, string[] args, string rawArgs)
    {
        Option option;
        string str = args[0];
        try
        {
            option = EnumUtils.GetEnum<Option>(str, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
        string message = string.Format("GetOption: {0} = {1}", EnumUtils.GetString<Option>(option), Options.Get().GetOption(option));
        Debug.Log(message);
        UIStatus.Get().AddInfo(message);
        return true;
    }

    private bool OnProcessCheat_GetVar(string func, string[] args, string rawArgs)
    {
        string key = args[0];
        string str = Vars.Key(key).GetStr(null);
        if (str == null)
        {
        }
        string message = string.Format("Var: {0} = {1}", key, "(null)");
        Debug.Log(message);
        UIStatus.Get().AddInfo(message);
        return true;
    }

    private bool OnProcessCheat_HasOption(string func, string[] args, string rawArgs)
    {
        Option option;
        string str = args[0];
        try
        {
            option = EnumUtils.GetEnum<Option>(str, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
        string message = string.Format("HasOption: {0} = {1}", EnumUtils.GetString<Option>(option), Options.Get().HasOption(option));
        Debug.Log(message);
        UIStatus.Get().AddInfo(message);
        return true;
    }

    private bool OnProcessCheat_help(string func, string[] args, string rawArgs)
    {
        StringBuilder builder = new StringBuilder();
        string str = null;
        if ((args.Length > 0) && !string.IsNullOrEmpty(args[0]))
        {
            str = args[0];
        }
        List<string> list = new List<string>();
        if (str != null)
        {
            foreach (string str2 in CheatMgr.Get().GetCheatCommands())
            {
                if (str2.Contains(str))
                {
                    list.Add(str2);
                }
            }
        }
        else
        {
            foreach (string str3 in CheatMgr.Get().GetCheatCommands())
            {
                list.Add(str3);
            }
        }
        Debug.Log(string.Concat(new object[] { "found commands ", list, " ", list.Count }));
        if (list.Count == 1)
        {
            str = list[0];
        }
        if ((str == null) || (list.Count != 1))
        {
            if (str == null)
            {
                builder.Append("All available cheat commands:\n");
            }
            else
            {
                builder.Append("Cheat commands containing: \"" + str + "\"\n");
            }
            int num = 0;
            string str4 = string.Empty;
            foreach (string str5 in list)
            {
                str4 = str4 + str5 + ", ";
                num++;
                if (num > 4)
                {
                    num = 0;
                    builder.Append(str4);
                    str4 = string.Empty;
                }
            }
            if (!string.IsNullOrEmpty(str4))
            {
                builder.Append(str4);
            }
            UIStatus.Get().AddInfo(builder.ToString(), (float) 10f);
        }
        else
        {
            string str6 = string.Empty;
            CheatMgr.Get().cheatDesc.TryGetValue(str, out str6);
            string str7 = string.Empty;
            CheatMgr.Get().cheatArgs.TryGetValue(str, out str7);
            builder.Append("Usage: ");
            builder.Append(str);
            if (!string.IsNullOrEmpty(str7))
            {
                builder.Append(" " + str7);
            }
            if (!string.IsNullOrEmpty(str6))
            {
                builder.Append("\n(" + str6 + ")");
            }
            UIStatus.Get().AddInfo(builder.ToString(), (float) 10f);
        }
        return true;
    }

    private bool OnProcessCheat_iks(string func, string[] args, string rawArgs)
    {
        if (args.Length < 1)
        {
            return false;
        }
        InnKeepersSpecial.Get().adUrlOverride = args[0];
        WelcomeQuests.Show(true, null, false);
        return true;
    }

    private bool OnProcessCheat_log(string func, string[] args, string rawArgs)
    {
        string str = args[0].ToLowerInvariant();
        if (!(str == "load") && !(str == "reload"))
        {
            return false;
        }
        Log.Get().Load();
        return true;
    }

    private bool OnProcessCheat_navigation(string func, string[] args, string rawArgs)
    {
        string str = args[0].ToLowerInvariant();
        string key = str;
        if (key != null)
        {
            int num;
            if (<>f__switch$map33 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                dictionary.Add("debug", 0);
                dictionary.Add("dump", 1);
                <>f__switch$map33 = dictionary;
            }
            if (<>f__switch$map33.TryGetValue(key, out num))
            {
                if (num == 0)
                {
                    Navigation.NAVIGATION_DEBUG = (args.Length < 2) || GeneralUtils.ForceBool(args[1]);
                    if (Navigation.NAVIGATION_DEBUG)
                    {
                        Navigation.DumpStack();
                        UIStatus.Get().AddInfo("Navigation debugging turned on - see Console or output log for nav dump.");
                    }
                    else
                    {
                        UIStatus.Get().AddInfo("Navigation debugging turned off.");
                    }
                    goto Label_0101;
                }
                if (num == 1)
                {
                    Navigation.DumpStack();
                    UIStatus.Get().AddInfo("Navigation dumped, see Console or output log.");
                    goto Label_0101;
                }
            }
        }
        UIStatus.Get().AddError("Unknown cmd: " + (!string.IsNullOrEmpty(str) ? str : "(blank)") + "\nValid cmds: debug, dump");
    Label_0101:
        return true;
    }

    private bool OnProcessCheat_newquest(string func, string[] args, string rawArgs)
    {
        if (WelcomeQuests.Get() == null)
        {
            return false;
        }
        WelcomeQuests.Get().GetFirstQuestTile().SetupTile(AchieveManager.Get().GetAchievement(int.Parse(rawArgs)));
        return true;
    }

    private bool OnProcessCheat_onlygold(string func, string[] args, string rawArgs)
    {
        string val = args[0].ToLowerInvariant();
        string key = val;
        if (key != null)
        {
            int num;
            if (<>f__switch$map32 == null)
            {
                Dictionary<string, int> dictionary = new Dictionary<string, int>(4);
                dictionary.Add("gold", 0);
                dictionary.Add("normal", 0);
                dictionary.Add("standard", 0);
                dictionary.Add("both", 1);
                <>f__switch$map32 = dictionary;
            }
            if (<>f__switch$map32.TryGetValue(key, out num))
            {
                if (num == 0)
                {
                    Options.Get().SetString(Option.COLLECTION_PREMIUM_TYPE, val);
                    goto Label_00D0;
                }
                if (num == 1)
                {
                    Options.Get().DeleteOption(Option.COLLECTION_PREMIUM_TYPE);
                    goto Label_00D0;
                }
            }
        }
        UIStatus.Get().AddError("Unknown cmd: " + (!string.IsNullOrEmpty(val) ? val : "(blank)") + "\nValid cmds: gold, standard, both");
        return false;
    Label_00D0:
        return true;
    }

    private bool OnProcessCheat_party(string func, string[] args, string rawArgs)
    {
        bool flag;
        string str2;
        <OnProcessCheat_party>c__AnonStorey367 storey = new <OnProcessCheat_party>c__AnonStorey367();
        if (args.Length >= 1)
        {
            if (<>f__am$cache18 == null)
            {
                <>f__am$cache18 = a => string.IsNullOrEmpty(a);
            }
            if (!Enumerable.Any<string>(args, <>f__am$cache18))
            {
                storey.cmd = args[0];
                if (storey.cmd == "unsubscribe")
                {
                    BnetParty.RemoveFromAllEventHandlers(this);
                    s_hasSubscribedToPartyEvents = false;
                    object[] objArray1 = new object[] { storey.cmd };
                    PartyLogger.Print("party {0}: unsubscribed.", objArray1);
                    return true;
                }
                flag = true;
                string[] source = args.Skip<string>(1).ToArray<string>();
                str2 = null;
                SubscribePartyEvents();
                string cmd = storey.cmd;
                if (cmd != null)
                {
                    int num40;
                    if (<>f__switch$map34 == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(20);
                        dictionary.Add("create", 0);
                        dictionary.Add("leave", 1);
                        dictionary.Add("dissolve", 1);
                        dictionary.Add("join", 2);
                        dictionary.Add("chat", 3);
                        dictionary.Add("invite", 4);
                        dictionary.Add("accept", 5);
                        dictionary.Add("decline", 5);
                        dictionary.Add("revoke", 6);
                        dictionary.Add("requestinvite", 7);
                        dictionary.Add("ignorerequest", 8);
                        dictionary.Add("setleader", 9);
                        dictionary.Add("kick", 10);
                        dictionary.Add("setprivacy", 11);
                        dictionary.Add("setlong", 12);
                        dictionary.Add("setstring", 12);
                        dictionary.Add("setblob", 12);
                        dictionary.Add("clearattr", 13);
                        dictionary.Add("subscribe", 14);
                        dictionary.Add("list", 14);
                        <>f__switch$map34 = dictionary;
                    }
                    if (<>f__switch$map34.TryGetValue(cmd, out num40))
                    {
                        switch (num40)
                        {
                            case 0:
                                if (source.Length < 1)
                                {
                                    if (<>f__am$cache19 == null)
                                    {
                                        <>f__am$cache19 = v => string.Concat(new object[] { v, " (", (int) v, ")" });
                                    }
                                    str2 = "party create: requires a PartyType: " + string.Join(" | ", Enumerable.Select<PartyType, string>(Enum.GetValues(typeof(PartyType)).Cast<PartyType>(), <>f__am$cache19).ToArray<string>());
                                }
                                else
                                {
                                    PartyType type;
                                    int num;
                                    if (int.TryParse(source[0], out num))
                                    {
                                        type = (PartyType) num;
                                    }
                                    else if (!EnumUtils.TryGetEnum<PartyType>(source[0], out type))
                                    {
                                        str2 = "party create: unknown PartyType specified: " + source[0];
                                    }
                                    if (str2 == null)
                                    {
                                        if (<>f__am$cache1A == null)
                                        {
                                            <>f__am$cache1A = delegate (PartyType t, PartyId partyId) {
                                                object[] args = new object[] { t, partyId };
                                                PartyLogger.Print("BnetParty.CreateSuccessCallback type={0} partyId={1}", args);
                                            };
                                        }
                                        BnetParty.CreateParty(type, PrivacyLevel.OPEN_INVITATION_AND_FRIEND, <>f__am$cache1A);
                                    }
                                }
                                goto Label_2028;

                            case 1:
                            {
                                bool flag2 = storey.cmd == "dissolve";
                                if (source.Length == 0)
                                {
                                    object[] objArray2 = new object[] { storey.cmd };
                                    PartyLogger.Print("NOTE: party {0} without any arguments will {0} all joined parties.", objArray2);
                                    PartyInfo[] joinedParties = BnetParty.GetJoinedParties();
                                    if (joinedParties.Length == 0)
                                    {
                                        PartyLogger.Print("No joined parties.", new object[0]);
                                    }
                                    foreach (PartyInfo info in joinedParties)
                                    {
                                        object[] objArray3 = new object[] { storey.cmd, !flag2 ? "leaving" : "dissolving", info };
                                        PartyLogger.Print("party {0}: {1} party {2}", objArray3);
                                        if (flag2)
                                        {
                                            BnetParty.DissolveParty(info.Id);
                                        }
                                        else
                                        {
                                            BnetParty.Leave(info.Id);
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < source.Length; i++)
                                    {
                                        string arg = source[i];
                                        string errorMsg = null;
                                        PartyId id = ParsePartyId(storey.cmd, arg, i, ref errorMsg);
                                        if (errorMsg != null)
                                        {
                                            PartyLogger.Print(errorMsg, new object[0]);
                                        }
                                        if (id != null)
                                        {
                                            object[] objArray4 = new object[] { storey.cmd, !flag2 ? "leaving" : "dissolving", BnetParty.GetJoinedParty(id) };
                                            PartyLogger.Print("party {0}: {1} party {2}", objArray4);
                                            if (flag2)
                                            {
                                                BnetParty.DissolveParty(id);
                                            }
                                            else
                                            {
                                                BnetParty.Leave(id);
                                            }
                                        }
                                    }
                                }
                                goto Label_2028;
                            }
                            case 2:
                                if (source.Length >= 1)
                                {
                                    PartyType dEFAULT = PartyType.DEFAULT;
                                    foreach (string str5 in source)
                                    {
                                        int index = str5.IndexOf('-');
                                        int result = -1;
                                        PartyId id2 = null;
                                        if (index >= 0)
                                        {
                                            ulong num7;
                                            ulong num8;
                                            string s = str5.Substring(0, index);
                                            string str7 = (str5.Length <= index) ? string.Empty : str5.Substring(index + 1);
                                            if (ulong.TryParse(s, out num7) && ulong.TryParse(str7, out num8))
                                            {
                                                id2 = new PartyId(num7, num8);
                                            }
                                            else
                                            {
                                                str2 = "party " + storey.cmd + ": unable to parse partyId (in format Hi-Lo).";
                                            }
                                        }
                                        else if (int.TryParse(str5, out result))
                                        {
                                            if (<>f__am$cache1B == null)
                                            {
                                                <>f__am$cache1B = p => p.IsOnline() && (p.GetHearthstoneGameAccount() != null);
                                            }
                                            BnetPlayer[] playerArray = Enumerable.Where<BnetPlayer>(BnetFriendMgr.Get().GetFriends(), <>f__am$cache1B).ToArray<BnetPlayer>();
                                            if ((result < 0) || (result >= playerArray.Length))
                                            {
                                                object[] objArray5 = new object[] { "party ", storey.cmd, ": no online friend at index ", result };
                                                str2 = string.Concat(objArray5);
                                            }
                                            else
                                            {
                                                str2 = "party " + storey.cmd + ": Not-Yet-Implemented: find partyId from online friend's presence.";
                                            }
                                        }
                                        else
                                        {
                                            str2 = "party " + storey.cmd + ": unable to parse online friend index.";
                                        }
                                        if (id2 != null)
                                        {
                                            BnetParty.JoinParty(id2, dEFAULT);
                                        }
                                    }
                                }
                                else
                                {
                                    str2 = "party " + storey.cmd + ": must specify an online friend index or a partyId (Hi-Lo format)";
                                }
                                goto Label_2028;

                            case 3:
                            {
                                PartyId[] joinedPartyIds = BnetParty.GetJoinedPartyIds();
                                if (source.Length >= 1)
                                {
                                    int count = 1;
                                    PartyId id3 = ParsePartyId(storey.cmd, source[0], -1, ref str2);
                                    if ((id3 == null) && (joinedPartyIds.Length > 0))
                                    {
                                        str2 = null;
                                        id3 = joinedPartyIds[0];
                                        count = 0;
                                    }
                                    if (id3 != null)
                                    {
                                        BnetParty.SendChatMessage(id3, string.Join(" ", source.Skip<string>(count).ToArray<string>()));
                                    }
                                }
                                else
                                {
                                    str2 = "party chat: must specify 1-2 arguments: party (index or LowBits or type) or a message to send.";
                                }
                                goto Label_2028;
                            }
                            case 4:
                            {
                                PartyId id4 = null;
                                int num10 = 1;
                                if (source.Length != 0)
                                {
                                    id4 = ParsePartyId(storey.cmd, source[0], -1, ref str2);
                                }
                                else
                                {
                                    PartyId[] idArray2 = BnetParty.GetJoinedPartyIds();
                                    if (idArray2.Length <= 0)
                                    {
                                        str2 = "party invite: no joined parties to invite to.";
                                    }
                                    else
                                    {
                                        id4 = idArray2[0];
                                        num10 = 0;
                                    }
                                }
                                if (id4 != null)
                                {
                                    string[] strArray3 = source.Skip<string>(num10).ToArray<string>();
                                    HashSet<BnetPlayer> set = new HashSet<BnetPlayer>();
                                    if (<>f__am$cache1C == null)
                                    {
                                        <>f__am$cache1C = p => p.IsOnline() && (p.GetHearthstoneGameAccount() != null);
                                    }
                                    IEnumerable<BnetPlayer> enumerable = Enumerable.Where<BnetPlayer>(BnetFriendMgr.Get().GetFriends(), <>f__am$cache1C);
                                    if (strArray3.Length == 0)
                                    {
                                        PartyLogger.Print("NOTE: party invite without any arguments will pick the first online friend.", new object[0]);
                                        BnetPlayer item = null;
                                        item = enumerable.FirstOrDefault<BnetPlayer>();
                                        if (item == null)
                                        {
                                            str2 = "party invite: no online Hearthstone friend found.";
                                        }
                                        else
                                        {
                                            set.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        for (int j = 0; j < strArray3.Length; j++)
                                        {
                                            int num12;
                                            <OnProcessCheat_party>c__AnonStorey363 storey2 = new <OnProcessCheat_party>c__AnonStorey363 {
                                                arg = strArray3[j]
                                            };
                                            if (int.TryParse(storey2.arg, out num12))
                                            {
                                                BnetPlayer player2 = enumerable.ElementAtOrDefault<BnetPlayer>(num12);
                                                if (player2 == null)
                                                {
                                                    str2 = "party invite: no online Hearthstone friend index " + num12;
                                                }
                                                else
                                                {
                                                    set.Add(player2);
                                                }
                                            }
                                            else
                                            {
                                                IEnumerable<BnetPlayer> enumerable2 = Enumerable.Where<BnetPlayer>(enumerable, new Func<BnetPlayer, bool>(storey2.<>m__21C));
                                                if (!enumerable2.Any<BnetPlayer>())
                                                {
                                                    object[] objArray6 = new object[] { "party invite: no online Hearthstone friend matching name ", storey2.arg, " (arg index ", j, ")" };
                                                    str2 = string.Concat(objArray6);
                                                }
                                                else
                                                {
                                                    IEnumerator<BnetPlayer> enumerator = enumerable2.GetEnumerator();
                                                    try
                                                    {
                                                        while (enumerator.MoveNext())
                                                        {
                                                            BnetPlayer current = enumerator.Current;
                                                            if (!set.Contains(current))
                                                            {
                                                                set.Add(current);
                                                                goto Label_08E5;
                                                            }
                                                        }
                                                    }
                                                    finally
                                                    {
                                                        if (enumerator == null)
                                                        {
                                                        }
                                                        enumerator.Dispose();
                                                    }
                                                Label_08E5:;
                                                }
                                            }
                                        }
                                    }
                                    foreach (BnetPlayer player4 in set)
                                    {
                                        BnetGameAccountId hearthstoneGameAccountId = player4.GetHearthstoneGameAccountId();
                                        if (BnetParty.IsMember(id4, hearthstoneGameAccountId))
                                        {
                                            object[] objArray7 = new object[] { player4, BnetParty.GetJoinedParty(id4) };
                                            PartyLogger.Print("party invite: already a party member of {0}: {1}", objArray7);
                                        }
                                        else
                                        {
                                            object[] objArray8 = new object[] { hearthstoneGameAccountId, player4, BnetParty.GetJoinedParty(id4) };
                                            PartyLogger.Print("party invite: inviting {0} {1} to party {2}", objArray8);
                                            BnetParty.SendInvite(id4, hearthstoneGameAccountId);
                                        }
                                    }
                                }
                                goto Label_2028;
                            }
                            case 5:
                            {
                                bool flag3 = storey.cmd == "accept";
                                PartyInvite[] receivedInvites = BnetParty.GetReceivedInvites();
                                if (receivedInvites.Length != 0)
                                {
                                    if (source.Length == 0)
                                    {
                                        object[] objArray9 = new object[] { storey.cmd };
                                        PartyLogger.Print("NOTE: party {0} without any arguments will {0} all received invites.", objArray9);
                                        foreach (PartyInvite invite in receivedInvites)
                                        {
                                            object[] objArray10 = new object[] { storey.cmd, !flag3 ? "declining" : "accepting", invite.InviteId, invite.InviterName, new PartyInfo(invite.PartyId, invite.PartyType) };
                                            PartyLogger.Print("party {0}: {1} inviteId={2} from {3} for party {4}.", objArray10);
                                            if (flag3)
                                            {
                                                BnetParty.AcceptReceivedInvite(invite.InviteId);
                                            }
                                            else
                                            {
                                                BnetParty.DeclineReceivedInvite(invite.InviteId);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int k = 0; k < source.Length; k++)
                                        {
                                            <OnProcessCheat_party>c__AnonStorey364 storey3 = new <OnProcessCheat_party>c__AnonStorey364();
                                            if (ulong.TryParse(source[k], out storey3.indexOrId))
                                            {
                                                PartyInvite invite2 = null;
                                                if (storey3.indexOrId < receivedInvites.LongLength)
                                                {
                                                    invite2 = receivedInvites[(int) ((IntPtr) storey3.indexOrId)];
                                                }
                                                else
                                                {
                                                    invite2 = Enumerable.FirstOrDefault<PartyInvite>(receivedInvites, new Func<PartyInvite, bool>(storey3.<>m__21D));
                                                    if (invite2 == null)
                                                    {
                                                        object[] objArray11 = new object[] { storey.cmd, source[k] };
                                                        PartyLogger.Print("party {0}: unable to find received invite (id or index): {1}", objArray11);
                                                    }
                                                }
                                                if (invite2 != null)
                                                {
                                                    object[] objArray12 = new object[] { storey.cmd, !flag3 ? "declining" : "accepting", invite2.InviteId, invite2.InviterName, new PartyInfo(invite2.PartyId, invite2.PartyType) };
                                                    PartyLogger.Print("party {0}: {1} inviteId={2} from {3} for party {4}.", objArray12);
                                                    if (flag3)
                                                    {
                                                        BnetParty.AcceptReceivedInvite(invite2.InviteId);
                                                    }
                                                    else
                                                    {
                                                        BnetParty.DeclineReceivedInvite(invite2.InviteId);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                object[] objArray13 = new object[] { storey.cmd, source[k] };
                                                PartyLogger.Print("party {0}: unable to parse invite (id or index): {1}", objArray13);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    str2 = "party " + storey.cmd + ": no received party invites.";
                                }
                                goto Label_2028;
                            }
                            case 6:
                            {
                                PartyId id6 = null;
                                if (source.Length == 0)
                                {
                                    object[] objArray14 = new object[] { storey.cmd };
                                    PartyLogger.Print("NOTE: party {0} without any arguments will {0} all sent invites for all parties.", objArray14);
                                    PartyId[] idArray3 = BnetParty.GetJoinedPartyIds();
                                    if (idArray3.Length == 0)
                                    {
                                        object[] objArray15 = new object[] { storey.cmd };
                                        PartyLogger.Print("party {0}: no joined parties.", objArray15);
                                    }
                                    foreach (PartyId id7 in idArray3)
                                    {
                                        foreach (PartyInvite invite3 in BnetParty.GetSentInvites(id7))
                                        {
                                            object[] objArray16 = new object[] { storey.cmd, invite3.InviteId, invite3.InviterName, BnetParty.GetJoinedParty(id7) };
                                            PartyLogger.Print("party {0}: revoking inviteId={1} from {2} for party {3}.", objArray16);
                                            BnetParty.RevokeSentInvite(id7, invite3.InviteId);
                                        }
                                    }
                                }
                                else
                                {
                                    id6 = ParsePartyId(storey.cmd, source[0], -1, ref str2);
                                }
                                if (id6 != null)
                                {
                                    PartyInfo joinedParty = BnetParty.GetJoinedParty(id6);
                                    PartyInvite[] sentInvites = BnetParty.GetSentInvites(id6);
                                    if (sentInvites.Length == 0)
                                    {
                                        object[] objArray17 = new object[] { "party ", storey.cmd, ": no sent invites for party ", joinedParty };
                                        str2 = string.Concat(objArray17);
                                    }
                                    else
                                    {
                                        string[] strArray4 = source.Skip<string>(1).ToArray<string>();
                                        if (strArray4.Length == 0)
                                        {
                                            object[] objArray18 = new object[] { storey.cmd };
                                            PartyLogger.Print("NOTE: party {0} without specifying InviteId (or index) will {0} all sent invites.", objArray18);
                                            foreach (PartyInvite invite4 in sentInvites)
                                            {
                                                object[] objArray19 = new object[] { storey.cmd, invite4.InviteId, invite4.InviterName, joinedParty };
                                                PartyLogger.Print("party {0}: revoking inviteId={1} from {2} for party {3}.", objArray19);
                                                BnetParty.RevokeSentInvite(id6, invite4.InviteId);
                                            }
                                        }
                                        else
                                        {
                                            for (int n = 0; n < strArray4.Length; n++)
                                            {
                                                <OnProcessCheat_party>c__AnonStorey365 storey4 = new <OnProcessCheat_party>c__AnonStorey365();
                                                if (ulong.TryParse(strArray4[n], out storey4.indexOrId))
                                                {
                                                    PartyInvite invite5 = null;
                                                    if (storey4.indexOrId < sentInvites.LongLength)
                                                    {
                                                        invite5 = sentInvites[(int) ((IntPtr) storey4.indexOrId)];
                                                    }
                                                    else
                                                    {
                                                        invite5 = Enumerable.FirstOrDefault<PartyInvite>(sentInvites, new Func<PartyInvite, bool>(storey4.<>m__21E));
                                                        if (invite5 == null)
                                                        {
                                                            object[] objArray20 = new object[] { storey.cmd, strArray4[n], joinedParty };
                                                            PartyLogger.Print("party {0}: unable to find sent invite (id or index): {1} for party {2}", objArray20);
                                                        }
                                                    }
                                                    if (invite5 != null)
                                                    {
                                                        object[] objArray21 = new object[] { storey.cmd, invite5.InviteId, invite5.InviterName, joinedParty };
                                                        PartyLogger.Print("party {0}: revoking inviteId={1} from {2} for party {3}.", objArray21);
                                                        BnetParty.RevokeSentInvite(id6, invite5.InviteId);
                                                    }
                                                }
                                                else
                                                {
                                                    object[] objArray22 = new object[] { storey.cmd, strArray4[n] };
                                                    PartyLogger.Print("party {0}: unable to parse invite (id or index): {1}", objArray22);
                                                }
                                            }
                                        }
                                    }
                                }
                                goto Label_2028;
                            }
                            case 7:
                                if (source.Length >= 2)
                                {
                                    PartyType partyType = PartyType.DEFAULT;
                                    foreach (string str8 in source)
                                    {
                                        int length = str8.IndexOf('-');
                                        int num21 = -1;
                                        PartyId id8 = null;
                                        BnetGameAccountId whomToAskForApproval = null;
                                        if (length >= 0)
                                        {
                                            ulong num22;
                                            ulong num23;
                                            string str9 = str8.Substring(0, length);
                                            string str10 = (str8.Length <= length) ? string.Empty : str8.Substring(length + 1);
                                            if (ulong.TryParse(str9, out num22) && ulong.TryParse(str10, out num23))
                                            {
                                                id8 = new PartyId(num22, num23);
                                            }
                                            else
                                            {
                                                str2 = "party " + storey.cmd + ": unable to parse partyId (in format Hi-Lo).";
                                            }
                                        }
                                        else if (int.TryParse(str8, out num21))
                                        {
                                            if (<>f__am$cache1D == null)
                                            {
                                                <>f__am$cache1D = p => p.IsOnline() && (p.GetHearthstoneGameAccount() != null);
                                            }
                                            BnetPlayer[] playerArray2 = Enumerable.Where<BnetPlayer>(BnetFriendMgr.Get().GetFriends(), <>f__am$cache1D).ToArray<BnetPlayer>();
                                            if ((num21 < 0) || (num21 >= playerArray2.Length))
                                            {
                                                object[] objArray23 = new object[] { "party ", storey.cmd, ": no online friend at index ", num21 };
                                                str2 = string.Concat(objArray23);
                                            }
                                            else
                                            {
                                                whomToAskForApproval = playerArray2[num21].GetHearthstoneGameAccountId();
                                            }
                                        }
                                        else
                                        {
                                            str2 = "party " + storey.cmd + ": unable to parse online friend index.";
                                        }
                                        if ((id8 != null) && (whomToAskForApproval != null))
                                        {
                                            BnetParty.RequestInvite(id8, whomToAskForApproval, BnetPresenceMgr.Get().GetMyGameAccountId(), partyType);
                                        }
                                    }
                                }
                                else
                                {
                                    str2 = "party " + storey.cmd + ": must specify a partyId (Hi-Lo format) and an online friend index";
                                }
                                goto Label_2028;

                            case 8:
                            {
                                PartyId[] idArray5 = BnetParty.GetJoinedPartyIds();
                                if (idArray5.Length != 0)
                                {
                                    foreach (PartyId id10 in idArray5)
                                    {
                                        foreach (InviteRequest request in BnetParty.GetInviteRequests(id10))
                                        {
                                            object[] objArray25 = new object[] { request.TargetName, request.TargetId, request.RequesterName, request.RequesterId };
                                            PartyLogger.Print("party {0}: ignoring request to invite {0} {1} from {2} {3}.", objArray25);
                                            BnetParty.IgnoreInviteRequest(id10, request.TargetId);
                                        }
                                    }
                                }
                                else
                                {
                                    object[] objArray24 = new object[] { storey.cmd };
                                    PartyLogger.Print("party {0}: no joined parties.", objArray24);
                                }
                                goto Label_2028;
                            }
                            case 9:
                            {
                                IEnumerable<PartyId> enumerable3 = null;
                                int num26 = -1;
                                if ((source.Length >= 2) && (!int.TryParse(source[1], out num26) || (num26 < 0)))
                                {
                                    str2 = string.Format("party {0}: invalid memberIndex={1}", storey.cmd, source[1]);
                                }
                                if (source.Length == 0)
                                {
                                    object[] objArray26 = new object[] { storey.cmd };
                                    PartyLogger.Print("NOTE: party {0} without any arguments will {0} to first member in all parties.", objArray26);
                                    PartyId[] idArray7 = BnetParty.GetJoinedPartyIds();
                                    if (idArray7.Length == 0)
                                    {
                                        object[] objArray27 = new object[] { storey.cmd };
                                        PartyLogger.Print("party {0}: no joined parties.", objArray27);
                                    }
                                    else
                                    {
                                        enumerable3 = idArray7;
                                    }
                                }
                                else
                                {
                                    PartyId id11 = ParsePartyId(storey.cmd, source[0], -1, ref str2);
                                    if (id11 != null)
                                    {
                                        PartyId[] idArray1 = new PartyId[] { id11 };
                                        enumerable3 = idArray1;
                                    }
                                }
                                if (enumerable3 != null)
                                {
                                    IEnumerator<PartyId> enumerator3 = enumerable3.GetEnumerator();
                                    try
                                    {
                                        while (enumerator3.MoveNext())
                                        {
                                            PartyId id12 = enumerator3.Current;
                                            PartyMember[] members = BnetParty.GetMembers(id12);
                                            if (num26 >= 0)
                                            {
                                                if (num26 >= members.Length)
                                                {
                                                    object[] objArray28 = new object[] { storey.cmd, BnetParty.GetJoinedParty(id12), num26 };
                                                    PartyLogger.Print("party {0}: party={1} has no member at index={2}", objArray28);
                                                }
                                                else
                                                {
                                                    PartyMember member = members[num26];
                                                    BnetParty.SetLeader(id12, member.GameAccountId);
                                                }
                                            }
                                            else
                                            {
                                                if (<>f__am$cache1E == null)
                                                {
                                                    <>f__am$cache1E = m => m.GameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId();
                                                }
                                                if (Enumerable.Any<PartyMember>(members, <>f__am$cache1E))
                                                {
                                                    if (<>f__am$cache1F == null)
                                                    {
                                                        <>f__am$cache1F = m => m.GameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId();
                                                    }
                                                    BnetParty.SetLeader(id12, Enumerable.First<PartyMember>(members, <>f__am$cache1F).GameAccountId);
                                                    continue;
                                                }
                                                object[] objArray29 = new object[] { storey.cmd, BnetParty.GetJoinedParty(id12) };
                                                PartyLogger.Print("party {0}: party={1} has no member not myself to set as leader.", objArray29);
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (enumerator3 == null)
                                        {
                                        }
                                        enumerator3.Dispose();
                                    }
                                }
                                goto Label_2028;
                            }
                            case 10:
                            {
                                PartyId id13 = null;
                                if (source.Length == 0)
                                {
                                    object[] objArray30 = new object[] { storey.cmd };
                                    PartyLogger.Print("NOTE: party {0} without any arguments will {0} all members for all parties (other than self).", objArray30);
                                    PartyId[] idArray8 = BnetParty.GetJoinedPartyIds();
                                    if (idArray8.Length == 0)
                                    {
                                        object[] objArray31 = new object[] { storey.cmd };
                                        PartyLogger.Print("party {0}: no joined parties.", objArray31);
                                    }
                                    foreach (PartyId id14 in idArray8)
                                    {
                                        foreach (PartyMember member2 in BnetParty.GetMembers(id14))
                                        {
                                            if (member2.GameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId())
                                            {
                                                object[] objArray32 = new object[] { storey.cmd, member2.GameAccountId, BnetParty.GetJoinedParty(id14) };
                                                PartyLogger.Print("party {0}: kicking memberId={1} from party {2}.", objArray32);
                                                BnetParty.KickMember(id14, member2.GameAccountId);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    id13 = ParsePartyId(storey.cmd, source[0], -1, ref str2);
                                }
                                if (id13 != null)
                                {
                                    PartyInfo info3 = BnetParty.GetJoinedParty(id13);
                                    PartyMember[] memberArray3 = BnetParty.GetMembers(id13);
                                    if (memberArray3.Length == 1)
                                    {
                                        object[] objArray33 = new object[] { "party ", storey.cmd, ": no members (other than self) for party ", info3 };
                                        str2 = string.Concat(objArray33);
                                    }
                                    else
                                    {
                                        string[] strArray6 = source.Skip<string>(1).ToArray<string>();
                                        if (strArray6.Length == 0)
                                        {
                                            object[] objArray34 = new object[] { storey.cmd };
                                            PartyLogger.Print("NOTE: party {0} without specifying member index will {0} all members (other than self).", objArray34);
                                            foreach (PartyMember member3 in memberArray3)
                                            {
                                                if (member3.GameAccountId != BnetPresenceMgr.Get().GetMyGameAccountId())
                                                {
                                                    object[] objArray35 = new object[] { storey.cmd, member3.GameAccountId, info3 };
                                                    PartyLogger.Print("party {0}: kicking memberId={1} from party {2}.", objArray35);
                                                    BnetParty.KickMember(id13, member3.GameAccountId);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            for (int num30 = 0; num30 < strArray6.Length; num30++)
                                            {
                                                <OnProcessCheat_party>c__AnonStorey366 storey5 = new <OnProcessCheat_party>c__AnonStorey366();
                                                if (ulong.TryParse(strArray6[num30], out storey5.indexOrId))
                                                {
                                                    PartyMember member4 = null;
                                                    if (storey5.indexOrId < memberArray3.LongLength)
                                                    {
                                                        member4 = memberArray3[(int) ((IntPtr) storey5.indexOrId)];
                                                    }
                                                    else
                                                    {
                                                        member4 = Enumerable.FirstOrDefault<PartyMember>(memberArray3, new Func<PartyMember, bool>(storey5.<>m__222));
                                                        if (member4 == null)
                                                        {
                                                            object[] objArray36 = new object[] { storey.cmd, strArray6[num30], info3 };
                                                            PartyLogger.Print("party {0}: unable to find member (id or index): {1} for party {2}", objArray36);
                                                        }
                                                    }
                                                    if (member4 != null)
                                                    {
                                                        if (member4.GameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId())
                                                        {
                                                            object[] objArray37 = new object[] { storey.cmd, num30, info3 };
                                                            PartyLogger.Print("party {0}: cannot kick yourself (argIndex={1}); party={2}", objArray37);
                                                        }
                                                        else
                                                        {
                                                            object[] objArray38 = new object[] { storey.cmd, member4.GameAccountId, info3 };
                                                            PartyLogger.Print("party {0}: kicking memberId={1} from party {2}.", objArray38);
                                                            BnetParty.KickMember(id13, member4.GameAccountId);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    object[] objArray39 = new object[] { storey.cmd, strArray6[num30] };
                                                    PartyLogger.Print("party {0}: unable to parse member (id or index): {1}", objArray39);
                                                }
                                            }
                                        }
                                    }
                                }
                                goto Label_2028;
                            }
                            case 11:
                            {
                                PartyId id15 = null;
                                if (source.Length < 2)
                                {
                                    if (<>f__am$cache20 == null)
                                    {
                                        <>f__am$cache20 = v => string.Concat(new object[] { v, " (", (int) v, ")" });
                                    }
                                    str2 = "party setprivacy: must specify a party (index or LowBits or type) and a PrivacyLevel: " + string.Join(" | ", Enumerable.Select<PrivacyLevel, string>(Enum.GetValues(typeof(PrivacyLevel)).Cast<PrivacyLevel>(), <>f__am$cache20).ToArray<string>());
                                }
                                else
                                {
                                    id15 = ParsePartyId(storey.cmd, source[0], -1, ref str2);
                                }
                                if (id15 != null)
                                {
                                    int num31;
                                    PrivacyLevel? nullable = null;
                                    if (int.TryParse(source[1], out num31))
                                    {
                                        nullable = new PrivacyLevel?((PrivacyLevel) num31);
                                    }
                                    else
                                    {
                                        PrivacyLevel level;
                                        if (!EnumUtils.TryGetEnum<PrivacyLevel>(source[1], out level))
                                        {
                                            str2 = "party setprivacy: unknown PrivacyLevel specified: " + source[1];
                                        }
                                        else
                                        {
                                            nullable = new PrivacyLevel?(level);
                                        }
                                    }
                                    if (nullable.HasValue)
                                    {
                                        object[] objArray40 = new object[] { nullable.Value, BnetParty.GetJoinedParty(id15) };
                                        PartyLogger.Print("party setprivacy: setting PrivacyLevel={0} for party {1}.", objArray40);
                                        BnetParty.SetPrivacy(id15, nullable.Value);
                                    }
                                }
                                goto Label_2028;
                            }
                            case 12:
                            {
                                bool flag4 = storey.cmd == "setlong";
                                bool flag5 = storey.cmd == "setstring";
                                bool flag6 = storey.cmd == "setblob";
                                int num32 = 1;
                                PartyId id16 = null;
                                if (source.Length >= 2)
                                {
                                    id16 = ParsePartyId(storey.cmd, source[0], -1, ref str2);
                                    if (id16 == null)
                                    {
                                        PartyId[] idArray10 = BnetParty.GetJoinedPartyIds();
                                        if (idArray10.Length > 0)
                                        {
                                            object[] objArray41 = new object[] { storey.cmd };
                                            PartyLogger.Print("party {0}: treating first argument as attributeKey (and not PartyId) - will use PartyId at index 0", objArray41);
                                            str2 = null;
                                            id16 = idArray10[0];
                                        }
                                    }
                                    else
                                    {
                                        object[] objArray42 = new object[] { storey.cmd };
                                        PartyLogger.Print("party {0}: treating first argument as PartyId (second argument will be attributeKey)", objArray42);
                                    }
                                }
                                else
                                {
                                    str2 = "party " + storey.cmd + ": must specify attributeKey and a value.";
                                }
                                if (id16 != null)
                                {
                                    bool flag7 = false;
                                    string attributeKey = source[num32];
                                    string str12 = string.Join(" ", source.Skip<string>((num32 + 1)).ToArray<string>());
                                    if (flag4)
                                    {
                                        long num33;
                                        if (long.TryParse(str12, out num33))
                                        {
                                            BnetParty.SetPartyAttributeLong(id16, attributeKey, num33);
                                            flag7 = true;
                                        }
                                    }
                                    else if (flag5)
                                    {
                                        BnetParty.SetPartyAttributeString(id16, attributeKey, str12);
                                        flag7 = true;
                                    }
                                    else if (flag6)
                                    {
                                        byte[] bytes = Encoding.UTF8.GetBytes(str12);
                                        BnetParty.SetPartyAttributeBlob(id16, attributeKey, bytes);
                                        flag7 = true;
                                    }
                                    else
                                    {
                                        str2 = "party " + storey.cmd + ": unhandled attribute type!";
                                    }
                                    if (flag7)
                                    {
                                        object[] objArray43 = new object[] { storey.cmd, attributeKey, str12, BnetParty.GetJoinedParty(id16) };
                                        PartyLogger.Print("party {0}: complete key={1} val={2} party={3}", objArray43);
                                    }
                                }
                                goto Label_2028;
                            }
                            case 13:
                            {
                                PartyId id17 = null;
                                if (source.Length >= 2)
                                {
                                    id17 = ParsePartyId(storey.cmd, source[0], -1, ref str2);
                                    if (id17 == null)
                                    {
                                        PartyId[] idArray11 = BnetParty.GetJoinedPartyIds();
                                        if (idArray11.Length > 0)
                                        {
                                            object[] objArray44 = new object[] { storey.cmd };
                                            PartyLogger.Print("party {0}: treating first argument as attributeKey (and not PartyId) - will use PartyId at index 0", objArray44);
                                            str2 = null;
                                            id17 = idArray11[0];
                                        }
                                    }
                                    else
                                    {
                                        object[] objArray45 = new object[] { storey.cmd };
                                        PartyLogger.Print("party {0}: treating first argument as PartyId (second argument will be attributeKey)", objArray45);
                                    }
                                }
                                else
                                {
                                    str2 = "party " + storey.cmd + ": must specify attributeKey.";
                                }
                                if (id17 != null)
                                {
                                    string str13 = source[1];
                                    BnetParty.ClearPartyAttribute(id17, str13);
                                    object[] objArray46 = new object[] { storey.cmd, str13, BnetParty.GetJoinedParty(id17) };
                                    PartyLogger.Print("party {0}: cleared key={1} party={2}", objArray46);
                                }
                                goto Label_2028;
                            }
                            case 14:
                            {
                                IEnumerable<PartyId> enumerable4 = null;
                                if (source.Length != 0)
                                {
                                    if (<>f__am$cache21 == null)
                                    {
                                        <>f__am$cache21 = p => p != null;
                                    }
                                    enumerable4 = Enumerable.Where<PartyId>(Enumerable.Select<string, PartyId>(source, new Func<string, int, PartyId>(storey.<>m__224)), <>f__am$cache21);
                                }
                                else
                                {
                                    PartyInfo[] infoArray3 = BnetParty.GetJoinedParties();
                                    if (infoArray3.Length != 0)
                                    {
                                        PartyLogger.Print("party list: listing all joined parties and the details of the party at index 0.", new object[0]);
                                        PartyId[] idArray12 = new PartyId[] { infoArray3[0].Id };
                                        enumerable4 = idArray12;
                                    }
                                    else
                                    {
                                        PartyLogger.Print("party list: no joined parties.", new object[0]);
                                    }
                                    for (int num34 = 0; num34 < infoArray3.Length; num34++)
                                    {
                                        object[] objArray47 = new object[] { GetPartySummary(infoArray3[num34], num34) };
                                        PartyLogger.Print("   {0}", objArray47);
                                    }
                                }
                                if (enumerable4 != null)
                                {
                                    int num35 = -1;
                                    IEnumerator<PartyId> enumerator4 = enumerable4.GetEnumerator();
                                    try
                                    {
                                        while (enumerator4.MoveNext())
                                        {
                                            PartyId id18 = enumerator4.Current;
                                            num35++;
                                            PartyInfo info4 = BnetParty.GetJoinedParty(id18);
                                            object[] objArray48 = new object[] { storey.cmd, GetPartySummary(BnetParty.GetJoinedParty(id18), num35) };
                                            PartyLogger.Print("party {0}: {1}", objArray48);
                                            PartyMember[] memberArray5 = BnetParty.GetMembers(id18);
                                            if (memberArray5.Length == 0)
                                            {
                                                PartyLogger.Print("   no members.", new object[0]);
                                            }
                                            else
                                            {
                                                PartyLogger.Print("   members:", new object[0]);
                                            }
                                            for (int num36 = 0; num36 < memberArray5.Length; num36++)
                                            {
                                                bool flag8 = memberArray5[num36].GameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId();
                                                object[] objArray49 = new object[5];
                                                objArray49[0] = num36;
                                                objArray49[1] = memberArray5[num36].GameAccountId;
                                                objArray49[2] = flag8;
                                                objArray49[3] = memberArray5[num36].IsLeader(info4.Type);
                                                if (<>f__am$cache22 == null)
                                                {
                                                    <>f__am$cache22 = r => r.ToString();
                                                }
                                                objArray49[4] = string.Join(",", Enumerable.Select<uint, string>(memberArray5[num36].RoleIds, <>f__am$cache22).ToArray<string>());
                                                PartyLogger.Print("      [{0}] {1} isMyself={2} isLeader={3} roleIds={4}", objArray49);
                                            }
                                            PartyInvite[] inviteArray6 = BnetParty.GetSentInvites(id18);
                                            if (inviteArray6.Length == 0)
                                            {
                                                PartyLogger.Print("   no sent invites.", new object[0]);
                                            }
                                            else
                                            {
                                                PartyLogger.Print("   sent invites:", new object[0]);
                                            }
                                            for (int num37 = 0; num37 < inviteArray6.Length; num37++)
                                            {
                                                PartyInvite invite6 = inviteArray6[num37];
                                                object[] objArray50 = new object[] { GetPartyInviteSummary(invite6, num37) };
                                                PartyLogger.Print("      {0}", objArray50);
                                            }
                                            KeyValuePair<string, object>[] allPartyAttributes = BnetParty.GetAllPartyAttributes(id18);
                                            if (allPartyAttributes.Length == 0)
                                            {
                                                PartyLogger.Print("   no party attributes.", new object[0]);
                                            }
                                            else
                                            {
                                                PartyLogger.Print("   party attributes:", new object[0]);
                                            }
                                            for (int num38 = 0; num38 < allPartyAttributes.Length; num38++)
                                            {
                                                KeyValuePair<string, object> pair = allPartyAttributes[num38];
                                                string str14 = (pair.Value != null) ? string.Format("[{0}]{1}", pair.Value.GetType().Name, pair.Value.ToString()) : "<null>";
                                                if (pair.Value is byte[])
                                                {
                                                    byte[] buffer2 = (byte[]) pair.Value;
                                                    str14 = "blobLength=" + buffer2.Length;
                                                    try
                                                    {
                                                        string str15 = Encoding.UTF8.GetString(buffer2);
                                                        if (str15 != null)
                                                        {
                                                            str14 = str14 + " decodedUtf8=" + str15;
                                                        }
                                                    }
                                                    catch (ArgumentException)
                                                    {
                                                    }
                                                }
                                                object[] objArray51 = new object[2];
                                                if (pair.Key == null)
                                                {
                                                }
                                                objArray51[0] = "<null>";
                                                objArray51[1] = str14;
                                                PartyLogger.Print("      {0}={1}", objArray51);
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (enumerator4 == null)
                                        {
                                        }
                                        enumerator4.Dispose();
                                    }
                                }
                                PartyInvite[] inviteArray7 = BnetParty.GetReceivedInvites();
                                if (inviteArray7.Length == 0)
                                {
                                    PartyLogger.Print("party list: no received party invites.", new object[0]);
                                }
                                else
                                {
                                    PartyLogger.Print("party list: received party invites:", new object[0]);
                                }
                                for (int num39 = 0; num39 < inviteArray7.Length; num39++)
                                {
                                    PartyInvite invite7 = inviteArray7[num39];
                                    object[] objArray52 = new object[] { GetPartyInviteSummary(invite7, num39) };
                                    PartyLogger.Print("   {0}", objArray52);
                                }
                                goto Label_2028;
                            }
                        }
                    }
                }
                str2 = "party: unknown party cmd: " + storey.cmd;
                goto Label_2028;
            }
        }
        string message = "USAGE: party [cmd] [args]\nCommands: create | join | leave | dissolve | list | invite | accept | decline | revoke | requestinvite | ignorerequest | setleader | kick | chat | setprivacy | setlong | setstring | setblob | clearattr | subscribe | unsubscribe";
        Error.AddWarning("Party Cheat Error", message, new object[0]);
        return false;
    Label_2028:
        if (str2 != null)
        {
            PartyLogger.Print(str2, new object[0]);
            Error.AddWarning("Party Cheat Error", str2, new object[0]);
            flag = false;
        }
        return flag;
    }

    private bool OnProcessCheat_playnullsound(string func, string[] args, string rawArgs)
    {
        SoundManager.Get().Play(null);
        return true;
    }

    private bool OnProcessCheat_popuptext(string func, string[] args, string rawArgs)
    {
        if (args.Length < 1)
        {
            return false;
        }
        string text = args[0];
        NotificationManager.Get().CreatePopupText(Box.Get().m_LeftDoor.transform.position, TutorialEntity.HELP_POPUP_SCALE, text, true);
        return true;
    }

    private bool OnProcessCheat_questcomplete(string func, string[] args, string rawArgs)
    {
        QuestToast.ShowQuestToast(null, false, AchieveManager.Get().GetAchievement(int.Parse(rawArgs)));
        return true;
    }

    private bool OnProcessCheat_questprogress(string func, string[] args, string rawArgs)
    {
        if (args.Length == 4)
        {
            string questName = args[0];
            string questDescription = args[1];
            int progress = int.Parse(args[2]);
            int maxProgress = int.Parse(args[3]);
            if (GameToastMgr.Get() != null)
            {
                GameToastMgr.Get().AddQuestProgressToast(questName, questDescription, progress, maxProgress);
                return true;
            }
        }
        return false;
    }

    private bool OnProcessCheat_questwelcome(string func, string[] args, string rawArgs)
    {
        bool boolVal = false;
        if ((args.Length > 0) && !string.IsNullOrEmpty(args[0]))
        {
            GeneralUtils.TryParseBool(args[0], out boolVal);
        }
        WelcomeQuests.Show(boolVal, null, false);
        return true;
    }

    private bool OnProcessCheat_quote(string func, string[] args, string rawArgs)
    {
        if (args.Length < 2)
        {
            return false;
        }
        string prefabName = args[0];
        string key = args[1];
        string soundName = key;
        if (args.Length > 2)
        {
            soundName = args[2];
        }
        if (prefabName.ToLowerInvariant().Contains("innkeeper"))
        {
            NotificationManager.Get().CreateInnkeeperQuote(NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get(key), soundName, 0f, null);
        }
        else
        {
            if (!prefabName.EndsWith("_Quote"))
            {
                prefabName = prefabName + "_Quote";
            }
            NotificationManager.Get().CreateCharacterQuote(prefabName, NotificationManager.DEFAULT_CHARACTER_POS, GameStrings.Get(key), soundName, true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
        }
        return true;
    }

    private bool OnProcessCheat_rankchange(string func, string[] args, string rawArgs)
    {
        <OnProcessCheat_rankchange>c__AnonStorey369 storey = new <OnProcessCheat_rankchange>c__AnonStorey369 {
            args = args
        };
        if (string.IsNullOrEmpty(storey.args[0].ToLower()))
        {
        }
        AssetLoader.GameObjectCallback callback = new AssetLoader.GameObjectCallback(storey.<>m__22C);
        AssetLoader.Get().LoadGameObject("RankChangeTwoScoop", callback, null, false);
        return true;
    }

    private bool OnProcessCheat_resettips(string func, string[] args, string rawArgs)
    {
        Options.Get().SetBool(Option.HAS_SEEN_COLLECTIONMANAGER, false);
        return true;
    }

    private bool OnProcessCheat_retire(string func, string[] args, string rawArgs)
    {
        if (DemoMgr.Get().GetMode() != DemoMode.BLIZZCON_2013)
        {
            return false;
        }
        DraftManager manager = DraftManager.Get();
        if (manager == null)
        {
            return false;
        }
        Network.RetireDraftDeck(manager.GetDraftDeck().ID, manager.GetSlot());
        return true;
    }

    private bool OnProcessCheat_rewardboxes(string func, string[] args, string rawArgs)
    {
        if (string.IsNullOrEmpty(args[0].ToLower()))
        {
        }
        int val = 5;
        if (args.Length > 1)
        {
            GeneralUtils.TryParseInt(args[1], out val);
        }
        BoosterDbId id = BoosterDbId.THE_GRAND_TOURNAMENT;
        if (<>f__am$cache2B == null)
        {
            <>f__am$cache2B = i => i != BoosterDbId.INVALID;
        }
        BoosterDbId[] idArray = Enumerable.Where<BoosterDbId>(Enum.GetValues(typeof(BoosterDbId)).Cast<BoosterDbId>(), <>f__am$cache2B).ToArray<BoosterDbId>();
        id = idArray[UnityEngine.Random.Range(0, idArray.Length)];
        string message = Cheat_ShowRewardBoxes(((((((((("Success: 123456" + " " + 13) + " " + UnityEngine.Random.Range(1, 0x22)) + " " + 1) + " " + ((int) id)) + " " + 3) + " " + (UnityEngine.Random.Range(1, 0x1f) * 5)) + " " + 2) + " " + (UnityEngine.Random.Range(1, 0x1f) * 5)) + " " + ((UnityEngine.Random.Range(0, 2) != 0) ? 10 : 6)) + " " + GameUtils.TranslateCardIdToDbId("EX1_279"));
        if (message != null)
        {
            UIStatus.Get().AddError(message);
        }
        return true;
    }

    private bool OnProcessCheat_scenario(string func, string[] args, string rawArgs)
    {
        int num;
        string str = args[0];
        if (!GeneralUtils.TryParseInt(str, out num))
        {
            object[] messageArgs = new object[] { str };
            Error.AddWarning("scenario Cheat Error", "Error reading a scenario id from \"{0}\"", messageArgs);
            return false;
        }
        QuickLaunchAvailability quickLaunchAvailability = this.GetQuickLaunchAvailability();
        switch (quickLaunchAvailability)
        {
            case QuickLaunchAvailability.FINDING_GAME:
                Error.AddDevWarning("scenario Cheat Error", "You are already finding a game.", new object[0]);
                break;

            case QuickLaunchAvailability.ACTIVE_GAME:
                Error.AddDevWarning("scenario Cheat Error", "You are already in a game.", new object[0]);
                break;

            case QuickLaunchAvailability.SCENE_TRANSITION:
                Error.AddDevWarning("scenario Cheat Error", "Can't start a game because a scene transition is active.", new object[0]);
                break;

            case QuickLaunchAvailability.COLLECTION_NOT_READY:
                Error.AddDevWarning("scenario Cheat Error", "Can't start a game because your collection is not fully loaded.", new object[0]);
                break;

            case QuickLaunchAvailability.OK:
                this.LaunchQuickGame(num);
                return true;

            default:
            {
                object[] objArray2 = new object[] { quickLaunchAvailability };
                Error.AddDevWarning("scenario Cheat Error", "Can't start a game: {0}", objArray2);
                break;
            }
        }
        return false;
    }

    private bool OnProcessCheat_seasonroll(string func, string[] args, string rawArgs)
    {
        int num;
        int num2;
        int num3;
        <OnProcessCheat_seasonroll>c__AnonStorey360 storey = new <OnProcessCheat_seasonroll>c__AnonStorey360();
        if (args.Length != 2)
        {
            return false;
        }
        if (!int.TryParse(args[0].ToLowerInvariant(), out storey.seasonID))
        {
            return false;
        }
        if (!int.TryParse(args[1].ToLowerInvariant(), out num))
        {
            return false;
        }
        if (!this.GetBonusStarsAndLevel(num, out num3, out num2))
        {
            return false;
        }
        SeasonEndDialog.SeasonEndInfo info = new SeasonEndDialog.SeasonEndInfo {
            m_seasonID = storey.seasonID,
            m_rank = num,
            m_chestRank = num,
            m_legendIndex = 0,
            m_bonusStars = num3,
            m_boostedRank = num2,
            m_rankedRewards = new List<RewardData>()
        };
        DbfRecord record = null;
        if (num <= 20)
        {
            record = GameDbf.CardBack.GetRecords().Find(new Predicate<DbfRecord>(storey.<>m__209));
            info.m_rankedRewards.Add(new CardBackRewardData(record.GetId()));
            info.m_rankedRewards.Add(new CardRewardData("EX1_279", TAG_PREMIUM.GOLDEN, 1));
            if (num <= 15)
            {
                info.m_rankedRewards.Add(new CardRewardData("EX1_279", TAG_PREMIUM.GOLDEN, 1));
            }
            if (num <= 10)
            {
                info.m_rankedRewards.Add(new CardRewardData("EX1_279", TAG_PREMIUM.GOLDEN, 1));
            }
            if (num <= 5)
            {
                info.m_rankedRewards.Add(new CardRewardData("EX1_279", TAG_PREMIUM.GOLDEN, 1));
            }
        }
        info.m_isFake = true;
        DialogManager.DialogRequest request = new DialogManager.DialogRequest {
            m_type = DialogManager.DialogType.SEASON_END,
            m_info = info,
            m_isFake = true
        };
        DialogManager.Get().AddToQueue(request);
        return true;
    }

    private bool OnProcessCheat_SetOption(string func, string[] args, string rawArgs)
    {
        Option option;
        string str = args[0];
        try
        {
            option = EnumUtils.GetEnum<Option>(str, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
        if (args.Length < 2)
        {
            return false;
        }
        string strVal = args[1];
        System.Type optionType = Options.Get().GetOptionType(option);
        if (optionType == typeof(bool))
        {
            bool flag;
            if (!GeneralUtils.TryParseBool(strVal, out flag))
            {
                return false;
            }
            Options.Get().SetBool(option, flag);
        }
        else if (optionType == typeof(int))
        {
            int num;
            if (!GeneralUtils.TryParseInt(strVal, out num))
            {
                return false;
            }
            Options.Get().SetInt(option, num);
        }
        else if (optionType == typeof(long))
        {
            long num2;
            if (!GeneralUtils.TryParseLong(strVal, out num2))
            {
                return false;
            }
            Options.Get().SetLong(option, num2);
        }
        else if (optionType == typeof(float))
        {
            float num3;
            if (!GeneralUtils.TryParseFloat(strVal, out num3))
            {
                return false;
            }
            Options.Get().SetFloat(option, num3);
        }
        else if (optionType == typeof(string))
        {
            strVal = rawArgs.Remove(0, str.Length + 1);
            Options.Get().SetString(option, strVal);
        }
        switch (option)
        {
            case Option.CURSOR:
                Cursor.visible = Options.Get().GetBool(Option.CURSOR);
                break;

            case Option.FAKE_PACK_OPENING:
                NetCache.Get().ReloadNetObject<NetCache.NetCacheBoosters>();
                break;

            default:
                if ((option == Option.FAKE_PACK_COUNT) && GameUtils.IsFakePackOpeningEnabled())
                {
                    NetCache.Get().ReloadNetObject<NetCache.NetCacheBoosters>();
                }
                break;
        }
        string message = string.Format("SetOption: {0} to {1}. GetOption = {2}", EnumUtils.GetString<Option>(option), strVal, Options.Get().GetOption(option));
        Debug.Log(message);
        UIStatus.Get().AddInfo(message);
        return true;
    }

    private bool OnProcessCheat_SetVar(string func, string[] args, string rawArgs)
    {
        string key = args[0];
        string str2 = (args.Length >= 2) ? args[1] : null;
        VarsInternal.Get().Set(key, str2);
        if (str2 == null)
        {
        }
        string message = string.Format("Var: {0} = {1}", key, "(null)");
        Debug.Log(message);
        UIStatus.Get().AddInfo(message);
        if (key.Equals("Arena.AutoDraft", StringComparison.InvariantCultureIgnoreCase) && (DraftDisplay.Get() != null))
        {
            DraftDisplay.Get().StartCoroutine(DraftDisplay.Get().RunAutoDraftCheat());
        }
        return true;
    }

    private bool OnProcessCheat_spectate(string func, string[] args, string rawArgs)
    {
        if ((args.Length >= 1) && (args[0] == "waiting"))
        {
            SpectatorManager.Get().ShowWaitingForNextGameDialog();
            return true;
        }
        if (args.Length >= 4)
        {
            if (<>f__am$cacheC == null)
            {
                <>f__am$cacheC = a => string.IsNullOrEmpty(a);
            }
            if (!Enumerable.Any<string>(args, <>f__am$cacheC))
            {
                uint num;
                int num2;
                JoinInfo joinInfo = new JoinInfo {
                    ServerIpAddress = args[0],
                    SecretKey = args[3]
                };
                if (!uint.TryParse(args[1], out num))
                {
                    Error.AddWarning("Spectate Cheat Error", "error parsing the port # (uint) argument: " + args[1], new object[0]);
                    return false;
                }
                joinInfo.ServerPort = num;
                if (!int.TryParse(args[2], out num2))
                {
                    Error.AddWarning("Spectate Cheat Error", "error parsing the game_handle (int) argument: " + args[2], new object[0]);
                    return false;
                }
                joinInfo.GameHandle = num2;
                joinInfo.GameType = GameType.GT_UNKNOWN;
                joinInfo.MissionId = 2;
                if ((args.Length >= 5) && int.TryParse(args[4], out num2))
                {
                    joinInfo.GameType = (GameType) num2;
                }
                if ((args.Length >= 6) && int.TryParse(args[5], out num2))
                {
                    joinInfo.MissionId = num2;
                }
                GameMgr.Get().SpectateGame(joinInfo);
                return true;
            }
        }
        Error.AddWarning("Spectate Cheat Error", "spectate cheat must have the following args:\n\nspectate ipaddress port game_handle spectator_password [gameType] [missionId]", new object[0]);
        return false;
    }

    private bool OnProcessCheat_storepassword(string func, string[] args, string rawArgs)
    {
        if (!this.m_loadingStoreChallengePrompt)
        {
            if (this.m_storeChallengePrompt == null)
            {
                this.m_loadingStoreChallengePrompt = true;
                AssetLoader.GameObjectCallback callback = delegate (string name, GameObject go, object callbackData) {
                    this.m_loadingStoreChallengePrompt = false;
                    this.m_storeChallengePrompt = go.GetComponent<StoreChallengePrompt>();
                    this.m_storeChallengePrompt.Hide();
                    this.m_storeChallengePrompt.Show(ChallengeType.PASSWORD, 0L, false);
                };
                AssetLoader.Get().LoadGameObject("StoreChallengePrompt", callback, null, false);
            }
            else if (this.m_storeChallengePrompt.IsShown())
            {
                this.m_storeChallengePrompt.Hide();
            }
            else
            {
                this.m_storeChallengePrompt.Show(ChallengeType.PASSWORD, 0L, false);
            }
        }
        return true;
    }

    private bool OnProcessCheat_suicide(string func, string[] args, string rawArgs)
    {
        string s = args[0].ToLowerInvariant();
        int result = 0;
        int.TryParse(s, out result);
        Application.CommitSuicide(result);
        return true;
    }

    private bool OnProcessCheat_tavernbrawl(string func, string[] args, string rawArgs)
    {
        string message = "USAGE: tb [cmd] [args]\nCommands: view, get, set, refresh, scenario, reset";
        if (args.Length >= 1)
        {
            if (<>f__am$cache26 == null)
            {
                <>f__am$cache26 = a => string.IsNullOrEmpty(a);
            }
            if (!Enumerable.Any<string>(args, <>f__am$cache26))
            {
                string str2 = args[0];
                string[] source = args.Skip<string>(1).ToArray<string>();
                string str3 = null;
                string key = str2;
                if (key != null)
                {
                    Dictionary<string, int> dictionary;
                    int num3;
                    if (<>f__switch$map36 == null)
                    {
                        dictionary = new Dictionary<string, int>(8);
                        dictionary.Add("help", 0);
                        dictionary.Add("reset", 1);
                        dictionary.Add("refresh", 2);
                        dictionary.Add("get", 3);
                        dictionary.Add("set", 3);
                        dictionary.Add("view", 4);
                        dictionary.Add("scen", 5);
                        dictionary.Add("scenario", 5);
                        <>f__switch$map36 = dictionary;
                    }
                    if (<>f__switch$map36.TryGetValue(key, out num3))
                    {
                        switch (num3)
                        {
                            case 0:
                                str3 = "usage";
                                break;

                            case 1:
                                if (source.Length != 0)
                                {
                                    if ("toserver".Equals(source[0], StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (TavernBrawlManager.Get().IsCheated)
                                        {
                                            TavernBrawlManager.Get().Cheat_ResetToServerData();
                                            TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
                                            if (mission == null)
                                            {
                                                str3 = "TB settings reset to server-specified Scenario ID <null>";
                                            }
                                            else
                                            {
                                                str3 = "TB settings reset to server-specified Scenario ID " + mission.missionId;
                                            }
                                        }
                                        else
                                        {
                                            str3 = "TB not locally cheated. Already using server-specified data.";
                                        }
                                    }
                                    else if ("seen".Equals(source[0], StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        int result = 0;
                                        if ((source.Length > 1) && !int.TryParse(source[1], out result))
                                        {
                                            str3 = "Error parsing new seen value: " + source[1];
                                        }
                                        if (str3 == null)
                                        {
                                            TavernBrawlManager.Get().Cheat_ResetSeenStuff(result);
                                            str3 = "all \"seentb*\" client-options reset to " + result;
                                        }
                                    }
                                    else
                                    {
                                        str3 = "Unknown reset parameter: " + source[0];
                                    }
                                    break;
                                }
                                str3 = "Please specify what to reset: seen, toserver";
                                break;

                            case 2:
                                TavernBrawlManager.Get().RefreshServerData();
                                str3 = "TB refreshing";
                                break;

                            case 3:
                            {
                                bool flag = str2 == "set";
                                string str4 = source.FirstOrDefault<string>();
                                if (!string.IsNullOrEmpty(str4))
                                {
                                    string str5 = null;
                                    string str9 = str4.ToLower();
                                    if (str9 != null)
                                    {
                                        int num4;
                                        if (<>f__switch$map35 == null)
                                        {
                                            dictionary = new Dictionary<string, int>(1);
                                            dictionary.Add("refreshtime", 0);
                                            <>f__switch$map35 = dictionary;
                                        }
                                        if (<>f__switch$map35.TryGetValue(str9, out num4) && (num4 == 0))
                                        {
                                            if (flag)
                                            {
                                                str3 = "cannot set RefreshTime";
                                            }
                                            else if (TavernBrawlManager.Get().IsRefreshingTavernBrawlInfo)
                                            {
                                                str3 = "refreshing right now";
                                            }
                                            else
                                            {
                                                str5 = TavernBrawlManager.Get().ScheduledSecondsToRefresh + " secs";
                                            }
                                        }
                                    }
                                    if (flag)
                                    {
                                        str3 = string.Format("tb set {0} {1} successful.", str4, (source.Length < 2) ? "null" : source[1]);
                                    }
                                    else if (string.IsNullOrEmpty(str3))
                                    {
                                        if (str5 == null)
                                        {
                                        }
                                        str3 = string.Format("tb variable {0}: {1}", str4, "null");
                                    }
                                    break;
                                }
                                str3 = string.Format("Please specify a TB variable to {0}. Variables:RefreshTime", str2);
                                break;
                            }
                            case 4:
                            {
                                TavernBrawlMission mission2 = TavernBrawlManager.Get().CurrentMission();
                                if (mission2 != null)
                                {
                                    string locString = string.Empty;
                                    string str7 = string.Empty;
                                    DbfRecord record = GameDbf.Scenario.GetRecord(mission2.missionId);
                                    if (record != null)
                                    {
                                        locString = record.GetLocString("NAME");
                                        str7 = record.GetLocString("DESCRIPTION");
                                    }
                                    str3 = string.Format("Active TB: [{0}] {1}\n{2}", mission2.missionId, locString, str7);
                                    break;
                                }
                                str3 = "No active Tavern Brawl at this time.";
                                break;
                            }
                            case 5:
                                if (source.Length >= 1)
                                {
                                    int num2;
                                    if (!int.TryParse(source[0], out num2))
                                    {
                                        str3 = "tb scenario: invalid non-integer Scenario ID " + source[0];
                                    }
                                    if (str3 == null)
                                    {
                                        TavernBrawlManager.Get().Cheat_SetScenario(num2);
                                    }
                                    break;
                                }
                                str3 = "tb scenario: requires an ID parameter";
                                break;
                        }
                    }
                }
                if (str3 != null)
                {
                    UIStatus.Get().AddInfo(str3, (float) 5f);
                }
                return true;
            }
        }
        UIStatus.Get().AddInfo(message, (float) 10f);
        return true;
    }

    private bool OnProcessCheat_timescale(string func, string[] args, string rawArgs)
    {
        string str = args[0].ToLower();
        if (string.IsNullOrEmpty(str))
        {
            UIStatus.Get().AddInfo(string.Format("Current timeScale is: {0}", SceneDebugger.GetDevTimescale()), (float) (3f * SceneDebugger.GetDevTimescale()));
            return true;
        }
        float result = 1f;
        if (!float.TryParse(str, out result))
        {
            return false;
        }
        SceneDebugger.SetDevTimescale(result);
        UIStatus.Get().AddInfo(string.Format("Setting timescale to: {0}", result), (float) (3f * result));
        return true;
    }

    private bool OnProcessCheat_utilservercmd(string func, string[] args, string rawArgs)
    {
        <OnProcessCheat_utilservercmd>c__AnonStorey368 storey = new <OnProcessCheat_utilservercmd>c__AnonStorey368();
        if (args.Length < 1)
        {
            UIStatus.Get().AddError("Must specify a sub-command.");
            return true;
        }
        storey.cmd = args[0].ToLower();
        storey.cmdArgs = args.Skip<string>(1).ToArray<string>();
        string str = (storey.cmdArgs.Length != 0) ? storey.cmdArgs[0].ToLower() : null;
        AlertPopup.ResponseCallback callback = new AlertPopup.ResponseCallback(storey.<>m__229);
        bool flag = true;
        string cmd = storey.cmd;
        if (cmd != null)
        {
            Dictionary<string, int> dictionary;
            int num;
            if (<>f__switch$map39 == null)
            {
                dictionary = new Dictionary<string, int>(4);
                dictionary.Add("help", 0);
                dictionary.Add("tb", 1);
                dictionary.Add("arena", 2);
                dictionary.Add("ranked", 3);
                <>f__switch$map39 = dictionary;
            }
            if (<>f__switch$map39.TryGetValue(cmd, out num))
            {
                string str4;
                int num2;
                switch (num)
                {
                    case 0:
                        flag = false;
                        break;

                    case 1:
                        str4 = str;
                        if (str4 != null)
                        {
                            if (<>f__switch$map37 == null)
                            {
                                dictionary = new Dictionary<string, int>(4);
                                dictionary.Add("help", 0);
                                dictionary.Add("view", 0);
                                dictionary.Add("list", 0);
                                dictionary.Add("reset", 1);
                                <>f__switch$map37 = dictionary;
                            }
                            if (<>f__switch$map37.TryGetValue(str4, out num2))
                            {
                                if (num2 == 0)
                                {
                                    flag = false;
                                }
                                else if (num2 == 1)
                                {
                                    string str2 = (storey.cmdArgs.Length >= 2) ? storey.cmdArgs[1].ToLower() : null;
                                    flag = str2 != "help";
                                }
                            }
                        }
                        break;

                    case 2:
                        flag = false;
                        break;

                    case 3:
                        flag = false;
                        str4 = str;
                        if (str4 != null)
                        {
                            if (<>f__switch$map38 == null)
                            {
                                dictionary = new Dictionary<string, int>(2);
                                dictionary.Add("seasonroll", 0);
                                dictionary.Add("seasonreset", 0);
                                <>f__switch$map38 = dictionary;
                            }
                            if (<>f__switch$map38.TryGetValue(str4, out num2) && (num2 == 0))
                            {
                                flag = true;
                            }
                        }
                        break;
                }
            }
        }
        this.m_lastUtilServerCmd = args;
        if (flag)
        {
            AlertPopup.PopupInfo info = new AlertPopup.PopupInfo {
                m_headerText = "Run UTIL server command?",
                m_text = "You are about to run a UTIL Server command - this may affect other players on this environment and possibly change configuration on this environment.\n\nPlease confirm you want to do this.",
                m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
                m_responseCallback = callback
            };
            DialogManager.Get().ShowPopup(info);
        }
        else
        {
            callback(AlertPopup.Response.OK, null);
        }
        return true;
    }

    private void OnProcessCheat_utilservercmd_OnResponse()
    {
        DebugCommandResponse debugCommandResponse = ConnectAPI.GetDebugCommandResponse();
        bool success = false;
        string format = "null response";
        string str2 = ((this.m_lastUtilServerCmd != null) && (this.m_lastUtilServerCmd.Length != 0)) ? this.m_lastUtilServerCmd[0] : string.Empty;
        string[] strArray = (this.m_lastUtilServerCmd != null) ? this.m_lastUtilServerCmd.Skip<string>(1).ToArray<string>() : new string[0];
        string str3 = (strArray.Length != 0) ? strArray[0] : null;
        string str4 = (strArray.Length >= 2) ? strArray[1].ToLower() : null;
        this.m_lastUtilServerCmd = null;
        if (debugCommandResponse != null)
        {
            success = debugCommandResponse.Success;
            format = string.Format("{0} {1}", !debugCommandResponse.Success ? "FAILED:" : "Success:", !debugCommandResponse.HasResponse ? "reply=<blank>" : debugCommandResponse.Response);
        }
        LogLevel level = !success ? LogLevel.Error : LogLevel.Info;
        Log.Net.Print(level, format, new object[0]);
        bool flag2 = true;
        float delay = 5f;
        if (success)
        {
            switch (str2)
            {
                case "tb":
                    if ((((str3 == "scenario") || (str3 == "scen")) || ((str3 == "season") || (str3 == "end_offset"))) || ((str3 == "start_offset") || ((str3 == "reset") && (str4 != "help"))))
                    {
                        TavernBrawlManager.Get().RefreshServerData();
                    }
                    goto Label_02D7;

                case "ranked":
                    switch (str3)
                    {
                        case "medal":
                            UnityEngine.Time.timeScale = SceneDebugger.Get().m_MaxTimeScale;
                            break;

                        case "medal":
                        case "seasonroll":
                            success = success && (!debugCommandResponse.HasResponse || !debugCommandResponse.Response.StartsWith("Error"));
                            if (success)
                            {
                                NetCache.Get().ReloadNetObject<NetCache.NetCacheProfileNotices>();
                                format = "Success";
                                delay = 0.5f;
                                break;
                            }
                            if (debugCommandResponse.HasResponse)
                            {
                                format = debugCommandResponse.Response;
                            }
                            break;
                    }
                    goto Label_02D7;
            }
            if ((str2 == "arena") && (str3 == "reward"))
            {
                success = success && (!debugCommandResponse.HasResponse || !debugCommandResponse.Response.StartsWith("Error"));
                if (success)
                {
                    format = Cheat_ShowRewardBoxes(format);
                    if (format == null)
                    {
                        delay = 0.5f;
                        format = "Success";
                    }
                    else
                    {
                        success = false;
                    }
                }
            }
        }
    Label_02D7:
        if (flag2)
        {
            if (success)
            {
                UIStatus.Get().AddInfo(format, delay);
            }
            else
            {
                UIStatus.Get().AddError(format);
            }
        }
    }

    private bool OnProcessCheat_warning(string func, string[] args, string rawArgs)
    {
        string str;
        string str2;
        this.ParseErrorText(args, rawArgs, out str, out str2);
        Error.AddWarning(str, str2, new object[0]);
        return true;
    }

    private void OnSceneLoaded(SceneMgr.Mode mode, Scene scene, object userData)
    {
        if ((SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.GAMEPLAY) && (mode != SceneMgr.Mode.GAMEPLAY))
        {
            SceneMgr.Get().UnregisterSceneLoadedEvent(new SceneMgr.SceneLoadedCallback(this.OnSceneLoaded));
            this.m_quickLaunchState = new QuickLaunchState();
        }
    }

    private Map<string, string> ParseAlertArgs(string rawArgs)
    {
        Map<string, string> map = new Map<string, string>();
        int startIndex = -1;
        int num2 = -1;
        string str = null;
        for (int i = 0; i < rawArgs.Length; i++)
        {
            char ch = rawArgs[i];
            if (ch == '=')
            {
                int num4 = -1;
                for (int j = i - 1; j >= 0; j--)
                {
                    char c = rawArgs[j];
                    char ch3 = rawArgs[j + 1];
                    if (!char.IsWhiteSpace(c))
                    {
                        num4 = j;
                    }
                    if (char.IsWhiteSpace(c) && !char.IsWhiteSpace(ch3))
                    {
                        break;
                    }
                }
                if (num4 >= 0)
                {
                    num2 = num4 - 2;
                    if (str != null)
                    {
                        map[str] = rawArgs.Substring(startIndex, (num2 - startIndex) + 1);
                    }
                    startIndex = i + 1;
                    str = rawArgs.Substring(num4, i - num4).Trim().ToLowerInvariant();
                }
            }
        }
        num2 = rawArgs.Length - 1;
        if (str != null)
        {
            map[str] = rawArgs.Substring(startIndex, (num2 - startIndex) + 1);
        }
        return map;
    }

    private void ParseErrorText(string[] args, string rawArgs, out string header, out string message)
    {
        header = (args.Length != 0) ? args[0] : "[PH] Header";
        if (args.Length <= 1)
        {
            message = "[PH] Message";
        }
        else
        {
            int startIndex = 0;
            bool flag = false;
            for (int i = 0; i < rawArgs.Length; i++)
            {
                char c = rawArgs[i];
                if (char.IsWhiteSpace(c))
                {
                    if (!flag)
                    {
                        continue;
                    }
                    startIndex = i;
                    break;
                }
                flag = true;
            }
            message = rawArgs.Substring(startIndex).Trim();
        }
    }

    private static PartyId ParsePartyId(string cmd, string arg, int argIndex, ref string errorMsg)
    {
        <ParsePartyId>c__AnonStorey361 storey = new <ParsePartyId>c__AnonStorey361();
        PartyId id = null;
        if (ulong.TryParse(arg, out storey.low))
        {
            PartyId[] joinedPartyIds = BnetParty.GetJoinedPartyIds();
            if (((storey.low >= 0L) && (joinedPartyIds.Length > 0)) && (storey.low < joinedPartyIds.LongLength))
            {
                id = joinedPartyIds[(int) ((IntPtr) storey.low)];
            }
            else
            {
                id = Enumerable.FirstOrDefault<PartyId>(joinedPartyIds, new Func<PartyId, bool>(storey.<>m__214));
            }
            if (id == null)
            {
                object[] objArray1 = new object[] { "party ", cmd, ": couldn't find party at index, or with PartyId low bits: ", storey.low };
                errorMsg = string.Concat(objArray1);
            }
            return id;
        }
        <ParsePartyId>c__AnonStorey362 storey2 = new <ParsePartyId>c__AnonStorey362();
        if (!EnumUtils.TryGetEnum<PartyType>(arg, out storey2.type))
        {
            string[] textArray1 = new string[] { "party ", cmd, ": unable to parse party (index or LowBits or type)", (argIndex < 0) ? string.Empty : (" at arg index=" + argIndex), " (", arg, "), please specify the Low bits of a PartyId or a PartyType." };
            errorMsg = string.Concat(textArray1);
            return id;
        }
        if (<>f__am$cache16 == null)
        {
            <>f__am$cache16 = info => info.Id;
        }
        id = Enumerable.Select<PartyInfo, PartyId>(Enumerable.Where<PartyInfo>(BnetParty.GetJoinedParties(), new Func<PartyInfo, bool>(storey2.<>m__215)), <>f__am$cache16).FirstOrDefault<PartyId>();
        if (id == null)
        {
            errorMsg = "party " + cmd + ": no joined party with PartyType: " + arg;
        }
        return id;
    }

    private void PositionLoginFixedReward(Reward reward)
    {
        Scene scene = SceneMgr.Get().GetScene();
        reward.transform.parent = scene.transform;
        reward.transform.localRotation = Quaternion.identity;
        reward.transform.localPosition = Login.REWARD_LOCAL_POS;
    }

    private void PrintQuickPlayLegend()
    {
        object[] args = new object[] { this.GetQuickPlayMissionName(KeyCode.F1), this.GetQuickPlayMissionName(KeyCode.F2), this.GetQuickPlayMissionName(KeyCode.F3), this.GetQuickPlayMissionName(KeyCode.F4), this.GetQuickPlayMissionName(KeyCode.F5), this.GetQuickPlayMissionName(KeyCode.F6), this.GetQuickPlayMissionName(KeyCode.F7), this.GetQuickPlayMissionName(KeyCode.F8), this.GetQuickPlayMissionName(KeyCode.F9) };
        string message = string.Format("F1: {0}\nF2: {1}\nF3: {2}\nF4: {3}\nF5: {4}\nF6: {5}\nF7: {6}\nF8: {7}\nF9: {8}\n(CTRL and ALT will Show mulligan)\nSHIFT + CTRL = Hero on players side\nSHIFT + ALT = Hero on opponent side\nSHIFT + ALT + CTRL = Hero on both sides", args);
        if (UIStatus.Get() != null)
        {
            UIStatus.Get().AddInfo(message);
        }
        object[] objArray2 = new object[] { this.GetQuickPlayMissionShortName(KeyCode.F1), this.GetQuickPlayMissionShortName(KeyCode.F2), this.GetQuickPlayMissionShortName(KeyCode.F3), this.GetQuickPlayMissionShortName(KeyCode.F4), this.GetQuickPlayMissionShortName(KeyCode.F5), this.GetQuickPlayMissionShortName(KeyCode.F6), this.GetQuickPlayMissionShortName(KeyCode.F7), this.GetQuickPlayMissionShortName(KeyCode.F8), this.GetQuickPlayMissionShortName(KeyCode.F9) };
        Debug.Log(string.Format("F1: {0}  F2: {1}  F3: {2}  F4: {3}  F5: {4}  F6: {5}  F7: {6}  F8: {7}  F9: {8}\n(CTRL and ALT will Show mulligan) -- SHIFT + CTRL = Hero on players side -- SHIFT + ALT = Hero on opponent side -- SHIFT + ALT + CTRL = Hero on both sides", objArray2));
    }

    public bool QuickGameFlipHeroes()
    {
        return this.m_quickLaunchState.m_flipHeroes;
    }

    public bool QuickGameMirrorHeroes()
    {
        return this.m_quickLaunchState.m_mirrorHeroes;
    }

    public string QuickGameOpponentHeroCardId()
    {
        return this.m_quickLaunchState.m_opponentHeroCardId;
    }

    public bool QuickGameSkipMulligan()
    {
        return this.m_quickLaunchState.m_skipMulligan;
    }

    private static void SubscribePartyEvents()
    {
        if (!s_hasSubscribedToPartyEvents)
        {
            if (<>f__am$cacheD == null)
            {
                <>f__am$cacheD = delegate (PartyError error) {
                    object[] args = new object[] { error.DebugContext, error.ErrorCode, error.FeatureEvent.ToString(), new PartyInfo(error.PartyId, error.PartyType), error.StringData };
                    PartyLogger.Print("{0} code={1} feature={2} party={3} str={4}", args);
                };
            }
            BnetParty.OnError += <>f__am$cacheD;
            if (<>f__am$cacheE == null)
            {
                <>f__am$cacheE = delegate (OnlineEventType e, PartyInfo party, LeaveReason? reason) {
                    object[] args = new object[] { e, party, !reason.HasValue ? "null" : reason.Value.ToString() };
                    PartyLogger.Print("Party.OnJoined {0} party={1} reason={2}", args);
                };
            }
            BnetParty.OnJoined += <>f__am$cacheE;
            if (<>f__am$cacheF == null)
            {
                <>f__am$cacheF = delegate (PartyInfo party, PrivacyLevel privacy) {
                    object[] args = new object[] { party, privacy };
                    PartyLogger.Print("Party.OnPrivacyLevelChanged party={0} privacy={1}", args);
                };
            }
            BnetParty.OnPrivacyLevelChanged += <>f__am$cacheF;
            if (<>f__am$cache10 == null)
            {
                <>f__am$cache10 = delegate (OnlineEventType e, PartyInfo party, BnetGameAccountId memberId, bool isRolesUpdate, LeaveReason? reason) {
                    object[] args = new object[] { e, party, memberId, isRolesUpdate, !reason.HasValue ? "null" : reason.Value.ToString() };
                    PartyLogger.Print("Party.OnMemberEvent {0} party={1} memberId={2} isRolesUpdate={3} reason={4}", args);
                };
            }
            BnetParty.OnMemberEvent += <>f__am$cache10;
            if (<>f__am$cache11 == null)
            {
                <>f__am$cache11 = delegate (OnlineEventType e, PartyInfo party, ulong inviteId, InviteRemoveReason? reason) {
                    object[] args = new object[] { e, party, inviteId, !reason.HasValue ? "null" : reason.Value.ToString() };
                    PartyLogger.Print("Party.OnReceivedInvite {0} party={1} inviteId={2} reason={3}", args);
                };
            }
            BnetParty.OnReceivedInvite += <>f__am$cache11;
            if (<>f__am$cache12 == null)
            {
                <>f__am$cache12 = delegate (OnlineEventType e, PartyInfo party, ulong inviteId, bool senderIsMyself, InviteRemoveReason? reason) {
                    PartyInvite sentInvite = BnetParty.GetSentInvite(party.Id, inviteId);
                    object[] args = new object[] { e, party, inviteId, senderIsMyself, (sentInvite != null) ? sentInvite.IsRejoin.ToString() : "null", !reason.HasValue ? "null" : reason.Value.ToString() };
                    PartyLogger.Print("Party.OnSentInvite {0} party={1} inviteId={2} senderIsMyself={3} isRejoin={4} reason={5}", args);
                };
            }
            BnetParty.OnSentInvite += <>f__am$cache12;
            if (<>f__am$cache13 == null)
            {
                <>f__am$cache13 = delegate (OnlineEventType e, PartyInfo party, InviteRequest request, InviteRequestRemovedReason? reason) {
                    object[] args = new object[] { e, party, request.TargetName, request.TargetId, request.RequesterName, request.RequesterId, !reason.HasValue ? "null" : reason.Value.ToString() };
                    PartyLogger.Print("Party.OnReceivedInviteRequest {0} party={1} target={2} {3} requester={4} {5} reason={6}", args);
                };
            }
            BnetParty.OnReceivedInviteRequest += <>f__am$cache13;
            if (<>f__am$cache14 == null)
            {
                <>f__am$cache14 = delegate (PartyInfo party, BnetGameAccountId speakerId, string msg) {
                    object[] args = new object[] { party, speakerId, msg };
                    PartyLogger.Print("Party.OnChatMessage party={0} speakerId={1} msg={2}", args);
                };
            }
            BnetParty.OnChatMessage += <>f__am$cache14;
            if (<>f__am$cache15 == null)
            {
                <>f__am$cache15 = delegate (PartyInfo party, string key, bnet.protocol.attribute.Variant attrVal) {
                    string str = "null";
                    if (attrVal.HasIntValue)
                    {
                        str = "[long]" + attrVal.IntValue.ToString();
                    }
                    else if (attrVal.HasStringValue)
                    {
                        str = "[string]" + attrVal.StringValue;
                    }
                    else if (attrVal.HasBlobValue)
                    {
                        byte[] bytes = attrVal.BlobValue;
                        if (bytes != null)
                        {
                            str = "blobLength=" + bytes.Length;
                            try
                            {
                                string str2 = Encoding.UTF8.GetString(bytes);
                                if (str2 != null)
                                {
                                    str = str + " decodedUtf8=" + str2;
                                }
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                    }
                    object[] args = new object[] { party, key, str };
                    PartyLogger.Print("BnetParty.OnPartyAttributeChanged party={0} key={1} value={2}", args);
                };
            }
            BnetParty.OnPartyAttributeChanged += <>f__am$cache15;
            s_hasSubscribedToPartyEvents = true;
        }
    }

    private static Logger PartyLogger
    {
        get
        {
            return Log.Henry;
        }
    }

    [CompilerGenerated]
    private sealed class <OnProcessCheat_party>c__AnonStorey363
    {
        internal string arg;

        internal bool <>m__21C(BnetPlayer p)
        {
            return (p.GetBattleTag().ToString().Contains(this.arg, StringComparison.OrdinalIgnoreCase) || ((p.GetFullName() != null) && p.GetFullName().Contains(this.arg, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [CompilerGenerated]
    private sealed class <OnProcessCheat_party>c__AnonStorey364
    {
        internal ulong indexOrId;

        internal bool <>m__21D(PartyInvite inv)
        {
            return (inv.InviteId == this.indexOrId);
        }
    }

    [CompilerGenerated]
    private sealed class <OnProcessCheat_party>c__AnonStorey365
    {
        internal ulong indexOrId;

        internal bool <>m__21E(PartyInvite inv)
        {
            return (inv.InviteId == this.indexOrId);
        }
    }

    [CompilerGenerated]
    private sealed class <OnProcessCheat_party>c__AnonStorey366
    {
        internal ulong indexOrId;

        internal bool <>m__222(PartyMember m)
        {
            return (m.GameAccountId.GetLo() == this.indexOrId);
        }
    }

    [CompilerGenerated]
    private sealed class <OnProcessCheat_party>c__AnonStorey367
    {
        internal string cmd;

        internal PartyId <>m__224(string a, int i)
        {
            string errorMsg = null;
            PartyId id = Cheats.ParsePartyId(this.cmd, a, i, ref errorMsg);
            if (errorMsg != null)
            {
                Cheats.PartyLogger.Print(errorMsg, new object[0]);
            }
            return id;
        }
    }

    [CompilerGenerated]
    private sealed class <OnProcessCheat_rankchange>c__AnonStorey369
    {
        internal string[] args;

        internal void <>m__22C(string name, GameObject go, object callbackData)
        {
            RankChangeTwoScoop component = go.GetComponent<RankChangeTwoScoop>();
            if (UniversalInputManager.UsePhoneUI != null)
            {
                component.transform.localPosition = new Vector3(0f, 156.5f, 1.4f);
            }
            else
            {
                component.transform.localPosition = new Vector3(0f, 292f, -9f);
            }
            component.CheatRankUp(this.args);
            component.Initialize(null, null);
        }
    }

    [CompilerGenerated]
    private sealed class <OnProcessCheat_seasonroll>c__AnonStorey360
    {
        internal int seasonID;

        internal bool <>m__209(DbfRecord obj)
        {
            if (!obj.GetString("SOURCE").Equals("season"))
            {
                return false;
            }
            return (obj.GetLong("DATA1") == this.seasonID);
        }
    }

    [CompilerGenerated]
    private sealed class <OnProcessCheat_utilservercmd>c__AnonStorey368
    {
        internal string cmd;
        internal string[] cmdArgs;

        internal void <>m__229(AlertPopup.Response response, object userData)
        {
            if ((response == AlertPopup.Response.CONFIRM) || (response == AlertPopup.Response.OK))
            {
                DebugCommandRequest packet = new DebugCommandRequest {
                    Command = this.cmd
                };
                packet.Args.AddRange(this.cmdArgs);
                ConnectAPI.SendDebugCommandRequest(packet);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ParsePartyId>c__AnonStorey361
    {
        internal ulong low;

        internal bool <>m__214(PartyId p)
        {
            return (p.Lo == this.low);
        }
    }

    [CompilerGenerated]
    private sealed class <ParsePartyId>c__AnonStorey362
    {
        internal PartyType type;

        internal bool <>m__215(PartyInfo info)
        {
            return (info.Type == this.type);
        }
    }

    private enum QuickLaunchAvailability
    {
        OK,
        FINDING_GAME,
        ACTIVE_GAME,
        SCENE_TRANSITION,
        COLLECTION_NOT_READY
    }

    private class QuickLaunchState
    {
        public bool m_flipHeroes;
        public bool m_launching;
        public bool m_mirrorHeroes;
        public string m_opponentHeroCardId;
        public bool m_skipMulligan;
    }
}

