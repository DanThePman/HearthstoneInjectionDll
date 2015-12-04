using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayErrors
{
    private static DelGetPlayEntityError DLL_GetPlayEntityError;
    private static DelGetRequirementsMap DLL_GetRequirementsMap;
    private static DelGetTargetEntityError DLL_GetTargetEntityError;
    private static DelPlayErrorsInit DLL_PlayErrorsInit;
    public const string PLAYERRORS_DLL_FILENAME = "PlayErrors32";
    private static bool PLAYERRORS_ENABLED = true;
    private static IntPtr s_DLL;
    private static bool s_initialized = false;
    private static Map<ErrorType, string> s_playErrorsMessages;

    static PlayErrors()
    {
        Map<ErrorType, string> map = new Map<ErrorType, string>();
        map.Add(ErrorType.REQ_MINION_TARGET, "GAMEPLAY_PlayErrors_REQ_MINION_TARGET");
        map.Add(ErrorType.REQ_FRIENDLY_TARGET, "GAMEPLAY_PlayErrors_REQ_FRIENDLY_TARGET");
        map.Add(ErrorType.REQ_ENEMY_TARGET, "GAMEPLAY_PlayErrors_REQ_ENEMY_TARGET");
        map.Add(ErrorType.REQ_DAMAGED_TARGET, "GAMEPLAY_PlayErrors_REQ_DAMAGED_TARGET");
        map.Add(ErrorType.REQ_ENCHANTED_TARGET, "GAMEPLAY_PlayErrors_REQ_ENCHANTED_TARGET");
        map.Add(ErrorType.REQ_FROZEN_TARGET, "GAMEPLAY_PlayErrors_REQ_FROZEN_TARGET");
        map.Add(ErrorType.REQ_CHARGE_TARGET, "GAMEPLAY_PlayErrors_REQ_CHARGE_TARGET");
        map.Add(ErrorType.REQ_TARGET_MAX_ATTACK, "GAMEPLAY_PlayErrors_REQ_TARGET_MAX_ATTACK");
        map.Add(ErrorType.REQ_NONSELF_TARGET, "GAMEPLAY_PlayErrors_REQ_NONSELF_TARGET");
        map.Add(ErrorType.REQ_TARGET_WITH_RACE, "GAMEPLAY_PlayErrors_REQ_TARGET_WITH_RACE");
        map.Add(ErrorType.REQ_TARGET_TO_PLAY, "GAMEPLAY_PlayErrors_REQ_TARGET_TO_PLAY");
        map.Add(ErrorType.REQ_NUM_MINION_SLOTS, "GAMEPLAY_PlayErrors_REQ_NUM_MINION_SLOTS");
        map.Add(ErrorType.REQ_WEAPON_EQUIPPED, "GAMEPLAY_PlayErrors_REQ_WEAPON_EQUIPPED");
        map.Add(ErrorType.REQ_ENOUGH_MANA, "GAMEPLAY_PlayErrors_REQ_ENOUGH_MANA");
        map.Add(ErrorType.REQ_YOUR_TURN, "GAMEPLAY_PlayErrors_REQ_YOUR_TURN");
        map.Add(ErrorType.REQ_NONSTEALTH_ENEMY_TARGET, "GAMEPLAY_PlayErrors_REQ_NONSTEALTH_ENEMY_TARGET");
        map.Add(ErrorType.REQ_HERO_TARGET, "GAMEPLAY_PlayErrors_REQ_HERO_TARGET");
        map.Add(ErrorType.REQ_SECRET_CAP, "GAMEPLAY_PlayErrors_REQ_SECRET_CAP");
        map.Add(ErrorType.REQ_MINION_CAP_IF_TARGET_AVAILABLE, "GAMEPLAY_PlayErrors_REQ_MINION_CAP_IF_TARGET_AVAILABLE");
        map.Add(ErrorType.REQ_MINION_CAP, "GAMEPLAY_PlayErrors_REQ_MINION_CAP");
        map.Add(ErrorType.REQ_TARGET_ATTACKED_THIS_TURN, "GAMEPLAY_PlayErrors_REQ_TARGET_ATTACKED_THIS_TURN");
        map.Add(ErrorType.REQ_TARGET_IF_AVAILABLE, "GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE");
        map.Add(ErrorType.REQ_MINIMUM_ENEMY_MINIONS, "GAMEPLAY_PlayErrors_REQ_MINIMUM_ENEMY_MINIONS");
        map.Add(ErrorType.REQ_TARGET_FOR_COMBO, "GAMEPLAY_PlayErrors_REQ_TARGET_FOR_COMBO");
        map.Add(ErrorType.REQ_NOT_EXHAUSTED_ACTIVATE, "GAMEPLAY_PlayErrors_REQ_NOT_EXHAUSTED_ACTIVATE");
        map.Add(ErrorType.REQ_UNIQUE_SECRET, "GAMEPLAY_PlayErrors_REQ_UNIQUE_SECRET");
        map.Add(ErrorType.REQ_CAN_BE_ATTACKED, "GAMEPLAY_PlayErrors_REQ_CAN_BE_ATTACKED");
        map.Add(ErrorType.REQ_ACTION_PWR_IS_MASTER_PWR, "GAMEPLAY_PlayErrors_REQ_ACTION_PWR_IS_MASTER_PWR");
        map.Add(ErrorType.REQ_TARGET_MAGNET, "GAMEPLAY_PlayErrors_REQ_TARGET_MAGNET");
        map.Add(ErrorType.REQ_ATTACK_GREATER_THAN_0, "GAMEPLAY_PlayErrors_REQ_ATTACK_GREATER_THAN_0");
        map.Add(ErrorType.REQ_ATTACKER_NOT_FROZEN, "GAMEPLAY_PlayErrors_REQ_ATTACKER_NOT_FROZEN");
        map.Add(ErrorType.REQ_HERO_OR_MINION_TARGET, "GAMEPLAY_PlayErrors_REQ_HERO_OR_MINION_TARGET");
        map.Add(ErrorType.REQ_CAN_BE_TARGETED_BY_SPELLS, "GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_SPELLS");
        map.Add(ErrorType.REQ_SUBCARD_IS_PLAYABLE, "GAMEPLAY_PlayErrors_REQ_SUBCARD_IS_PLAYABLE");
        map.Add(ErrorType.REQ_TARGET_FOR_NO_COMBO, "GAMEPLAY_PlayErrors_REQ_TARGET_FOR_NO_COMBO");
        map.Add(ErrorType.REQ_NOT_MINION_JUST_PLAYED, "GAMEPLAY_PlayErrors_REQ_NOT_MINION_JUST_PLAYED");
        map.Add(ErrorType.REQ_NOT_EXHAUSTED_HERO_POWER, "GAMEPLAY_PlayErrors_REQ_NOT_EXHAUSTED_HERO_POWER");
        map.Add(ErrorType.REQ_CAN_BE_TARGETED_BY_OPPONENTS, "GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_OPPONENTS");
        map.Add(ErrorType.REQ_ATTACKER_CAN_ATTACK, "GAMEPLAY_PlayErrors_REQ_ATTACKER_CAN_ATTACK");
        map.Add(ErrorType.REQ_TARGET_MIN_ATTACK, "GAMEPLAY_PlayErrors_REQ_TARGET_MIN_ATTACK");
        map.Add(ErrorType.REQ_CAN_BE_TARGETED_BY_HERO_POWERS, "GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_HERO_POWERS");
        map.Add(ErrorType.REQ_ENEMY_TARGET_NOT_IMMUNE, "GAMEPLAY_PlayErrors_REQ_ENEMY_TARGET_NOT_IMMUNE");
        map.Add(ErrorType.REQ_ENTIRE_ENTOURAGE_NOT_IN_PLAY, "GAMEPLAY_PlayErrors_REQ_ENTIRE_ENTOURAGE_NOT_IN_PLAY");
        map.Add(ErrorType.REQ_MINIMUM_TOTAL_MINIONS, "GAMEPLAY_PlayErrors_REQ_MINIMUM_TOTAL_MINIONS");
        map.Add(ErrorType.REQ_MUST_TARGET_TAUNTER, "GAMEPLAY_PlayErrors_REQ_MUST_TARGET_TAUNTER");
        map.Add(ErrorType.REQ_UNDAMAGED_TARGET, "GAMEPLAY_PlayErrors_REQ_UNDAMAGED_TARGET");
        map.Add(ErrorType.REQ_CAN_BE_TARGETED_BY_BATTLECRIES, "GAMEPLAY_PlayErrors_REQ_CAN_BE_TARGETED_BY_BATTLECRIES");
        map.Add(ErrorType.REQ_STEADY_SHOT, "GAMEPLAY_PlayErrors_REQ_STEADY_SHOT");
        map.Add(ErrorType.REQ_MINION_OR_ENEMY_HERO, "GAMEPLAY_PlayErrors_REQ_MINION_OR_ENEMY_HERO");
        map.Add(ErrorType.REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND, "GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND");
        map.Add(ErrorType.REQ_LEGENDARY_TARGET, "GAMEPLAY_PlayErrors_REQ_LEGENDARY_TARGET");
        map.Add(ErrorType.REQ_FRIENDLY_MINION_DIED_THIS_TURN, "GAMEPLAY_PlayErrors_REQ_FRIENDLY_MINION_DIED_THIS_TURN");
        map.Add(ErrorType.REQ_FRIENDLY_MINION_DIED_THIS_GAME, "GAMEPLAY_PlayErrors_REQ_FRIENDLY_MINION_DIED_THIS_GAME");
        map.Add(ErrorType.REQ_ENEMY_WEAPON_EQUIPPED, "GAMEPLAY_PlayErrors_REQ_ENEMY_WEAPON_EQUIPPED");
        map.Add(ErrorType.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS, "GAMEPLAY_PlayErrors_REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS");
        map.Add(ErrorType.REQ_TARGET_WITH_BATTLECRY, "GAMEPLAY_PlayErrors_REQ_TARGET_WITH_BATTLECRY");
        map.Add(ErrorType.REQ_TARGET_WITH_DEATHRATTLE, "GAMEPLAY_PlayErrors_REQ_TARGET_WITH_DEATHRATTLE");
        map.Add(ErrorType.REQ_DRAG_TO_PLAY, "GAMEPLAY_PlayErrors_REQ_DRAG_TO_PLAY");
        s_playErrorsMessages = map;
        s_DLL = IntPtr.Zero;
    }

    public static void AppQuit()
    {
        if (PLAYERRORS_ENABLED)
        {
            UnloadDLL();
            s_initialized = false;
            Log.PlayErrors.Print("AppQuit: " + s_initialized, new object[0]);
        }
    }

    private static bool CanShowMinionTauntError()
    {
        int num;
        int num2;
        Player opposingSidePlayer = GameState.Get().GetOpposingSidePlayer();
        GameState.Get().GetTauntCounts(opposingSidePlayer, out num, out num2);
        return ((num > 0) && (num2 == 0));
    }

    public static void DisplayPlayError(ErrorType error, Entity errorSource)
    {
        Log.PlayErrors.Print(string.Concat(new object[] { "DisplayPlayError: (", s_initialized, ") ", error, " ", errorSource }), new object[0]);
        if (s_initialized && !GameState.Get().GetGameEntity().NotifyOfPlayError(error, errorSource))
        {
            switch (error)
            {
                case ErrorType.REQ_MINION_TARGET:
                case ErrorType.REQ_FRIENDLY_TARGET:
                case ErrorType.REQ_ENEMY_TARGET:
                case ErrorType.REQ_DAMAGED_TARGET:
                case ErrorType.REQ_FROZEN_TARGET:
                case ErrorType.REQ_TARGET_MAX_ATTACK:
                case ErrorType.REQ_TARGET_WITH_RACE:
                case ErrorType.REQ_HERO_TARGET:
                case ErrorType.REQ_HERO_OR_MINION_TARGET:
                case ErrorType.REQ_CAN_BE_TARGETED_BY_SPELLS:
                case ErrorType.REQ_CAN_BE_TARGETED_BY_OPPONENTS:
                case ErrorType.REQ_TARGET_MIN_ATTACK:
                case ErrorType.REQ_CAN_BE_TARGETED_BY_HERO_POWERS:
                case ErrorType.REQ_ENEMY_TARGET_NOT_IMMUNE:
                case ErrorType.REQ_CAN_BE_TARGETED_BY_BATTLECRIES:
                case ErrorType.REQ_MINION_OR_ENEMY_HERO:
                case ErrorType.REQ_LEGENDARY_TARGET:
                case ErrorType.REQ_TARGET_WITH_BATTLECRY:
                case ErrorType.REQ_TARGET_WITH_DEATHRATTLE:
                    GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_TARGET);
                    break;

                case ErrorType.REQ_TARGET_TO_PLAY:
                case ErrorType.REQ_TARGET_IF_AVAILABLE:
                case ErrorType.REQ_TARGET_FOR_COMBO:
                case ErrorType.REQ_TARGET_FOR_NO_COMBO:
                case ErrorType.REQ_STEADY_SHOT:
                case ErrorType.REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND:
                case ErrorType.REQ_FRIENDLY_MINION_DIED_THIS_TURN:
                case ErrorType.REQ_FRIENDLY_MINION_DIED_THIS_GAME:
                case ErrorType.REQ_ENEMY_WEAPON_EQUIPPED:
                case ErrorType.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS:
                    GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_PLAY);
                    break;

                case ErrorType.REQ_NUM_MINION_SLOTS:
                case ErrorType.REQ_MINION_CAP_IF_TARGET_AVAILABLE:
                case ErrorType.REQ_MINION_CAP:
                    GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_FULL_MINIONS);
                    break;

                case ErrorType.REQ_WEAPON_EQUIPPED:
                    GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_NEED_WEAPON);
                    break;

                case ErrorType.REQ_ENOUGH_MANA:
                    GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_NEED_MANA);
                    break;

                case ErrorType.REQ_YOUR_TURN:
                    return;

                case ErrorType.REQ_NONSTEALTH_ENEMY_TARGET:
                    GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_STEALTH);
                    break;

                case ErrorType.REQ_NOT_EXHAUSTED_ACTIVATE:
                    if (!errorSource.IsHero())
                    {
                        GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_MINION_ATTACKED);
                    }
                    else
                    {
                        GameState.Get().GetCurrentPlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_I_ATTACKED);
                    }
                    break;

                case ErrorType.REQ_TARGET_TAUNTER:
                    DisplayTauntErrorEffects();
                    break;

                case ErrorType.REQ_NOT_MINION_JUST_PLAYED:
                    GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_JUST_PLAYED);
                    break;

                case ErrorType.REQ_DRAG_TO_PLAY:
                    break;

                default:
                    GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_GENERIC);
                    break;
            }
            PlayRequirementInfo playRequirementInfo = GetPlayRequirementInfo(errorSource);
            string errorDescription = GetErrorDescription(error, playRequirementInfo);
            if (!string.IsNullOrEmpty(errorDescription))
            {
                GameplayErrorManager.Get().DisplayMessage(errorDescription);
            }
        }
    }

    private static void DisplayTauntErrorEffects()
    {
        if (CanShowMinionTauntError())
        {
            GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(EmoteType.ERROR_TAUNT);
        }
        GameState.Get().ShowEnemyTauntCharacters();
    }

    private static string ErrorInEditorOnly(string format, params object[] args)
    {
        return string.Empty;
    }

    private static string GetErrorDescription(ErrorType type, PlayRequirementInfo requirementInfo)
    {
        Log.PlayErrors.Print(string.Concat(new object[] { "GetErrorDescription: ", type, " ", requirementInfo }), new object[0]);
        ErrorType type2 = type;
        switch (type2)
        {
            case ErrorType.REQ_YOUR_TURN:
                return string.Empty;

            case ErrorType.REQ_SECRET_CAP:
            {
                object[] args = new object[] { GameState.Get().GetMaxSecretsPerPlayer() };
                return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_SECRET_CAP", args);
            }
            case ErrorType.REQ_TARGET_MAX_ATTACK:
            {
                object[] objArray2 = new object[] { requirementInfo.paramMaxAtk };
                return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_MAX_ATTACK", objArray2);
            }
            case ErrorType.REQ_TARGET_WITH_RACE:
            {
                object[] objArray3 = new object[] { GameStrings.GetRaceName((TAG_RACE) requirementInfo.paramRace) };
                return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_WITH_RACE", objArray3);
            }
            case ErrorType.REQ_TARGET_TAUNTER:
                if (!CanShowMinionTauntError())
                {
                    return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_TARGET_TAUNTER_CHARACTER");
                }
                return GameStrings.Get("GAMEPLAY_PlayErrors_REQ_TARGET_TAUNTER_MINION");

            case ErrorType.REQ_ACTION_PWR_IS_MASTER_PWR:
                return ErrorInEditorOnly("[Unity Editor] Action power must be master power", new object[0]);
        }
        if (type2 != ErrorType.NONE)
        {
            if (type2 == ErrorType.REQ_MINIMUM_ENEMY_MINIONS)
            {
                object[] objArray5 = new object[] { requirementInfo.paramMinNumEnemyMinions };
                return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MINIMUM_ENEMY_MINIONS", objArray5);
            }
            if (type2 == ErrorType.REQ_TARGET_MIN_ATTACK)
            {
                object[] objArray6 = new object[] { requirementInfo.paramMinAtk };
                return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_TARGET_MIN_ATTACK", objArray6);
            }
            if (type2 == ErrorType.REQ_MINIMUM_TOTAL_MINIONS)
            {
                object[] objArray7 = new object[] { requirementInfo.paramMinNumTotalMinions };
                return GameStrings.Format("GAMEPLAY_PlayErrors_REQ_MINIMUM_TOTAL_MINIONS", objArray7);
            }
            string str = null;
            if (s_playErrorsMessages.TryGetValue(type, out str))
            {
                return GameStrings.Get(str);
            }
            object[] objArray8 = new object[] { type };
            return ErrorInEditorOnly("[Unity Editor] Unknown play error ({0})", objArray8);
        }
        Debug.LogWarning("PlayErrors.GetErrorDescription() - Action is not valid, but no error string found.");
        return string.Empty;
    }

    private static IntPtr GetFunction(string name)
    {
        IntPtr procAddress = DLLUtils.GetProcAddress(s_DLL, name);
        if (procAddress == IntPtr.Zero)
        {
            Debug.LogError("Could not load PlayErrors." + name + "()");
            AppQuit();
        }
        return procAddress;
    }

    private static List<Marshaled_TargetEntityInfo> GetMarshaledEntitiesInPlay()
    {
        List<Marshaled_TargetEntityInfo> list = new List<Marshaled_TargetEntityInfo>();
        foreach (Zone zone in ZoneMgr.Get().FindZonesForTag(TAG_ZONE.PLAY))
        {
            foreach (Card card in zone.GetCards())
            {
                Entity entity = card.GetEntity();
                if (entity.GetZone() == TAG_ZONE.PLAY)
                {
                    list.Add(Marshaled_TargetEntityInfo.ConvertFromTargetEntityInfo(entity.ConvertToTargetInfo()));
                }
            }
        }
        return list;
    }

    private static List<Marshaled_SourceEntityInfo> GetMarshaledSubCards(Entity source)
    {
        List<Marshaled_SourceEntityInfo> list = new List<Marshaled_SourceEntityInfo>();
        foreach (int num in source.GetSubCardIDs())
        {
            Entity entity = GameState.Get().GetEntity(num);
            if (entity == null)
            {
                Log.PlayErrors.Print(string.Format("Subcard of {0} is null in GetMarshaledSubCards()!", source.GetName()), new object[0]);
            }
            else
            {
                SourceEntityInfo sourceInfo = entity.ConvertToSourceInfo(Power.GetDefaultMasterPower().GetPlayRequirementInfo(), source);
                list.Add(Marshaled_SourceEntityInfo.ConvertFromSourceEntityInfo(sourceInfo));
            }
        }
        return list;
    }

    private static Player GetOwningPlayer(Entity entity)
    {
        Player player = GameState.Get().GetPlayer(entity.GetControllerId());
        if (player == null)
        {
            Log.PlayErrors.Print(string.Format("Error retrieving controlling player of entity {0} in PlayErrors.GetOwningPlayer()!", entity.GetName()), new object[0]);
        }
        return player;
    }

    public static ErrorType GetPlayEntityError(Entity source)
    {
        Log.PlayErrors.Print(string.Concat(new object[] { "GetPlayEntityError (", s_initialized, ") ", source }), new object[0]);
        if (!s_initialized)
        {
            return ErrorType.NONE;
        }
        Player owningPlayer = GetOwningPlayer(source);
        if (owningPlayer == null)
        {
            return ErrorType.NONE;
        }
        SourceEntityInfo sourceInfo = source.ConvertToSourceInfo(GetPlayRequirementInfo(source), null);
        PlayerInfo playerInfo = owningPlayer.ConvertToPlayerInfo();
        GameStateInfo gameInfo = GameState.Get().ConvertToGameStateInfo();
        Marshaled_PlayErrorsParams playErrorsParams = new Marshaled_PlayErrorsParams(sourceInfo, playerInfo, gameInfo);
        List<Marshaled_TargetEntityInfo> marshaledEntitiesInPlay = GetMarshaledEntitiesInPlay();
        List<Marshaled_SourceEntityInfo> marshaledSubCards = GetMarshaledSubCards(source);
        return DLL_GetPlayEntityError(playErrorsParams, marshaledSubCards.ToArray(), marshaledSubCards.Count, marshaledEntitiesInPlay.ToArray(), marshaledEntitiesInPlay.Count);
    }

    private static PlayRequirementInfo GetPlayRequirementInfo(Entity entity)
    {
        if (entity.GetZone() == TAG_ZONE.HAND)
        {
            return entity.GetMasterPower().GetPlayRequirementInfo();
        }
        if (entity.GetZone() == TAG_ZONE.SETASIDE)
        {
            return entity.GetMasterPower().GetPlayRequirementInfo();
        }
        if (entity.IsHeroPower())
        {
            return entity.GetMasterPower().GetPlayRequirementInfo();
        }
        if (entity.ShouldUseBattlecryPower())
        {
            return entity.GetMasterPower().GetPlayRequirementInfo();
        }
        return entity.GetAttackPower().GetPlayRequirementInfo();
    }

    public static ulong GetRequirementsMap(List<ErrorType> requirements)
    {
        if (!s_initialized)
        {
            return 0L;
        }
        return DLL_GetRequirementsMap(requirements.ToArray(), requirements.Count);
    }

    public static ErrorType GetTargetEntityError(Entity source, Entity target)
    {
        if (!s_initialized)
        {
            return ErrorType.NONE;
        }
        Player owningPlayer = GetOwningPlayer(source);
        if (owningPlayer == null)
        {
            return ErrorType.NONE;
        }
        SourceEntityInfo sourceInfo = source.ConvertToSourceInfo(GetPlayRequirementInfo(source), null);
        PlayerInfo playerInfo = owningPlayer.ConvertToPlayerInfo();
        GameStateInfo gameInfo = GameState.Get().ConvertToGameStateInfo();
        Marshaled_PlayErrorsParams playErrorsParams = new Marshaled_PlayErrorsParams(sourceInfo, playerInfo, gameInfo);
        Marshaled_TargetEntityInfo info4 = Marshaled_TargetEntityInfo.ConvertFromTargetEntityInfo(target.ConvertToTargetInfo());
        List<Marshaled_TargetEntityInfo> marshaledEntitiesInPlay = GetMarshaledEntitiesInPlay();
        return DLL_GetTargetEntityError(playErrorsParams, info4, marshaledEntitiesInPlay.ToArray(), marshaledEntitiesInPlay.Count);
    }

    public static bool Init()
    {
        if (!PLAYERRORS_ENABLED)
        {
            return true;
        }
        if (s_initialized)
        {
            return true;
        }
        if (!LoadDLL())
        {
            return false;
        }
        s_initialized = DLL_PlayErrorsInit();
        Log.PlayErrors.Print("Init: " + s_initialized, new object[0]);
        if (!s_initialized)
        {
            UnloadDLL();
        }
        return s_initialized;
    }

    public static bool IsInitialized()
    {
        return s_initialized;
    }

    private static bool LoadDLL()
    {
        s_DLL = FileUtils.LoadPlugin("PlayErrors32", true);
        if (s_DLL == IntPtr.Zero)
        {
            return false;
        }
        DLL_PlayErrorsInit = (DelPlayErrorsInit) Marshal.GetDelegateForFunctionPointer(GetFunction("PlayErrorsInit"), typeof(DelPlayErrorsInit));
        DLL_GetRequirementsMap = (DelGetRequirementsMap) Marshal.GetDelegateForFunctionPointer(GetFunction("GetRequirementsMap"), typeof(DelGetRequirementsMap));
        DLL_GetPlayEntityError = (DelGetPlayEntityError) Marshal.GetDelegateForFunctionPointer(GetFunction("GetPlayEntityError"), typeof(DelGetPlayEntityError));
        DLL_GetTargetEntityError = (DelGetTargetEntityError) Marshal.GetDelegateForFunctionPointer(GetFunction("GetTargetEntityError"), typeof(DelGetTargetEntityError));
        return (s_DLL != IntPtr.Zero);
    }

    private static void UnloadDLL()
    {
        if (s_initialized)
        {
            Log.PlayErrors.Print("Unloading PlayErrors DLL..", new object[0]);
            if (!DLLUtils.FreeLibrary(s_DLL))
            {
                Debug.LogError(string.Format("error unloading {0}", "PlayErrors32"));
            }
            s_DLL = IntPtr.Zero;
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate PlayErrors.ErrorType DelGetPlayEntityError(PlayErrors.Marshaled_PlayErrorsParams playErrorsParams, PlayErrors.Marshaled_SourceEntityInfo[] subCards, int numSubCards, PlayErrors.Marshaled_TargetEntityInfo[] enititiesInPlay, int numEntitiesInPlay);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ulong DelGetRequirementsMap(PlayErrors.ErrorType[] requirements, int numRequirements);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate PlayErrors.ErrorType DelGetTargetEntityError(PlayErrors.Marshaled_PlayErrorsParams playErrorsParams, PlayErrors.Marshaled_TargetEntityInfo target, PlayErrors.Marshaled_TargetEntityInfo[] entitiesInPlay, int numEntitiesInPlay);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool DelPlayErrorsInit();

    public enum ErrorType
    {
        NONE,
        REQ_MINION_TARGET,
        REQ_FRIENDLY_TARGET,
        REQ_ENEMY_TARGET,
        REQ_DAMAGED_TARGET,
        REQ_ENCHANTED_TARGET,
        REQ_FROZEN_TARGET,
        REQ_CHARGE_TARGET,
        REQ_TARGET_MAX_ATTACK,
        REQ_NONSELF_TARGET,
        REQ_TARGET_WITH_RACE,
        REQ_TARGET_TO_PLAY,
        REQ_NUM_MINION_SLOTS,
        REQ_WEAPON_EQUIPPED,
        REQ_ENOUGH_MANA,
        REQ_YOUR_TURN,
        REQ_NONSTEALTH_ENEMY_TARGET,
        REQ_HERO_TARGET,
        REQ_SECRET_CAP,
        REQ_MINION_CAP_IF_TARGET_AVAILABLE,
        REQ_MINION_CAP,
        REQ_TARGET_ATTACKED_THIS_TURN,
        REQ_TARGET_IF_AVAILABLE,
        REQ_MINIMUM_ENEMY_MINIONS,
        REQ_TARGET_FOR_COMBO,
        REQ_NOT_EXHAUSTED_ACTIVATE,
        REQ_UNIQUE_SECRET,
        REQ_TARGET_TAUNTER,
        REQ_CAN_BE_ATTACKED,
        REQ_ACTION_PWR_IS_MASTER_PWR,
        REQ_TARGET_MAGNET,
        REQ_ATTACK_GREATER_THAN_0,
        REQ_ATTACKER_NOT_FROZEN,
        REQ_HERO_OR_MINION_TARGET,
        REQ_CAN_BE_TARGETED_BY_SPELLS,
        REQ_SUBCARD_IS_PLAYABLE,
        REQ_TARGET_FOR_NO_COMBO,
        REQ_NOT_MINION_JUST_PLAYED,
        REQ_NOT_EXHAUSTED_HERO_POWER,
        REQ_CAN_BE_TARGETED_BY_OPPONENTS,
        REQ_ATTACKER_CAN_ATTACK,
        REQ_TARGET_MIN_ATTACK,
        REQ_CAN_BE_TARGETED_BY_HERO_POWERS,
        REQ_ENEMY_TARGET_NOT_IMMUNE,
        REQ_ENTIRE_ENTOURAGE_NOT_IN_PLAY,
        REQ_MINIMUM_TOTAL_MINIONS,
        REQ_MUST_TARGET_TAUNTER,
        REQ_UNDAMAGED_TARGET,
        REQ_CAN_BE_TARGETED_BY_BATTLECRIES,
        REQ_STEADY_SHOT,
        REQ_MINION_OR_ENEMY_HERO,
        REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND,
        REQ_LEGENDARY_TARGET,
        REQ_FRIENDLY_MINION_DIED_THIS_TURN,
        REQ_FRIENDLY_MINION_DIED_THIS_GAME,
        REQ_ENEMY_WEAPON_EQUIPPED,
        REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS,
        REQ_TARGET_WITH_BATTLECRY,
        REQ_TARGET_WITH_DEATHRATTLE,
        REQ_DRAG_TO_PLAY
    }

    public class GameStateInfo
    {
        public TAG_STEP currentStep = TAG_STEP.MAIN_BEGIN;
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    private struct Marshaled_GameStateInfo
    {
        [MarshalAs(UnmanagedType.U4)]
        public TAG_STEP currentStep;
        public static PlayErrors.Marshaled_GameStateInfo ConvertFromGameStateInfo(PlayErrors.GameStateInfo gameInfo)
        {
            return new PlayErrors.Marshaled_GameStateInfo { currentStep = gameInfo.currentStep };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    private struct Marshaled_PlayerInfo
    {
        public int id;
        public int numResources;
        public int numFriendlyMinionsInPlay;
        public int numEnemyMinionsInPlay;
        public int numMinionSlotsPerPlayer;
        public int numOpenSecretSlots;
        public int numDragonsInHand;
        public int numFriendlyMinionsThatDiedThisTurn;
        public int numFriendlyMinionsThatDiedThisGame;
        [MarshalAs(UnmanagedType.U1)]
        public bool isCurrentPlayer;
        [MarshalAs(UnmanagedType.U1)]
        public bool weaponEquipped;
        [MarshalAs(UnmanagedType.U1)]
        public bool enemyWeaponEquipped;
        [MarshalAs(UnmanagedType.U1)]
        public bool comboActive;
        [MarshalAs(UnmanagedType.U1)]
        public bool steadyShotRequiresTarget;
        public static PlayErrors.Marshaled_PlayerInfo ConvertFromPlayerInfo(PlayErrors.PlayerInfo playerInfo)
        {
            return new PlayErrors.Marshaled_PlayerInfo { id = playerInfo.id, numResources = playerInfo.numResources, numFriendlyMinionsInPlay = playerInfo.numFriendlyMinionsInPlay, numEnemyMinionsInPlay = playerInfo.numEnemyMinionsInPlay, numMinionSlotsPerPlayer = playerInfo.numMinionSlotsPerPlayer, numOpenSecretSlots = playerInfo.numOpenSecretSlots, numDragonsInHand = playerInfo.numDragonsInHand, numFriendlyMinionsThatDiedThisTurn = playerInfo.numFriendlyMinionsThatDiedThisTurn, numFriendlyMinionsThatDiedThisGame = playerInfo.numFriendlyMinionsThatDiedThisGame, isCurrentPlayer = playerInfo.isCurrentPlayer, weaponEquipped = playerInfo.weaponEquipped, enemyWeaponEquipped = playerInfo.enemyWeaponEquipped, comboActive = playerInfo.comboActive, steadyShotRequiresTarget = playerInfo.steadyShotRequiresTarget };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    private struct Marshaled_PlayErrorsParams
    {
        [MarshalAs(UnmanagedType.Struct)]
        public PlayErrors.Marshaled_SourceEntityInfo source;
        [MarshalAs(UnmanagedType.Struct)]
        public PlayErrors.Marshaled_PlayerInfo player;
        [MarshalAs(UnmanagedType.U4)]
        public PlayErrors.Marshaled_GameStateInfo game;
        public Marshaled_PlayErrorsParams(PlayErrors.SourceEntityInfo sourceInfo, PlayErrors.PlayerInfo playerInfo, PlayErrors.GameStateInfo gameInfo)
        {
            this.source = PlayErrors.Marshaled_SourceEntityInfo.ConvertFromSourceEntityInfo(sourceInfo);
            this.player = PlayErrors.Marshaled_PlayerInfo.ConvertFromPlayerInfo(playerInfo);
            this.game = PlayErrors.Marshaled_GameStateInfo.ConvertFromGameStateInfo(gameInfo);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    private struct Marshaled_SourceEntityInfo
    {
        public ulong requirementsMap;
        public int id;
        public int cost;
        public int attack;
        public int minAttackRequirement;
        public int maxAttackRequirement;
        public int raceRequirement;
        public int numMinionSlotsRequirement;
        public int numMinionSlotsWithTargetRequirement;
        public int minTotalMinionsRequirement;
        public int minFriendlyMinionsRequirement;
        public int minEnemyMinionsRequirement;
        public int numTurnsInPlay;
        public int numAttacksThisTurn;
        public int numWindfury;
        [MarshalAs(UnmanagedType.U4)]
        public TAG_CARDTYPE cardType;
        [MarshalAs(UnmanagedType.U4)]
        public TAG_ZONE zone;
        [MarshalAs(UnmanagedType.U1)]
        public bool isSecret;
        [MarshalAs(UnmanagedType.U1)]
        public bool isDuplicateSecret;
        [MarshalAs(UnmanagedType.U1)]
        public bool isExhausted;
        [MarshalAs(UnmanagedType.U1)]
        public bool isMasterPower;
        [MarshalAs(UnmanagedType.U1)]
        public bool isActionPower;
        [MarshalAs(UnmanagedType.U1)]
        public bool isActivatePower;
        [MarshalAs(UnmanagedType.U1)]
        public bool isAttackPower;
        [MarshalAs(UnmanagedType.U1)]
        public bool isFrozen;
        [MarshalAs(UnmanagedType.U1)]
        public bool hasBattlecry;
        [MarshalAs(UnmanagedType.U1)]
        public bool canAttack;
        [MarshalAs(UnmanagedType.U1)]
        public bool entireEntourageInPlay;
        [MarshalAs(UnmanagedType.U1)]
        public bool hasCharge;
        [MarshalAs(UnmanagedType.U1)]
        public bool isChoiceMinion;
        [MarshalAs(UnmanagedType.U1)]
        public bool cannotAttackHeroes;
        public static PlayErrors.Marshaled_SourceEntityInfo ConvertFromSourceEntityInfo(PlayErrors.SourceEntityInfo sourceInfo)
        {
            return new PlayErrors.Marshaled_SourceEntityInfo { 
                requirementsMap = sourceInfo.requirementsMap, id = sourceInfo.id, cost = sourceInfo.cost, attack = sourceInfo.attack, minAttackRequirement = sourceInfo.minAttackRequirement, maxAttackRequirement = sourceInfo.maxAttackRequirement, raceRequirement = sourceInfo.raceRequirement, numMinionSlotsRequirement = sourceInfo.numMinionSlotsRequirement, numMinionSlotsWithTargetRequirement = sourceInfo.numMinionSlotsWithTargetRequirement, minTotalMinionsRequirement = sourceInfo.minTotalMinionsRequirement, minFriendlyMinionsRequirement = sourceInfo.minFriendlyMinionsRequirement, minEnemyMinionsRequirement = sourceInfo.minEnemyMinionsRequirement, numTurnsInPlay = sourceInfo.numTurnsInPlay, numAttacksThisTurn = sourceInfo.numAttacksThisTurn, numWindfury = sourceInfo.numWindfury, cardType = sourceInfo.cardType, 
                zone = sourceInfo.zone, isSecret = sourceInfo.isSecret, isDuplicateSecret = sourceInfo.isDuplicateSecret, isExhausted = sourceInfo.isExhausted, isMasterPower = sourceInfo.isMasterPower, isActionPower = sourceInfo.isActionPower, isActivatePower = sourceInfo.isActivatePower, isAttackPower = sourceInfo.isAttackPower, isFrozen = sourceInfo.isFrozen, hasBattlecry = sourceInfo.hasBattlecry, canAttack = sourceInfo.canAttack, entireEntourageInPlay = sourceInfo.entireEntourageInPlay, hasCharge = sourceInfo.hasCharge, isChoiceMinion = sourceInfo.isChoiceMinion, cannotAttackHeroes = sourceInfo.cannotAttackHeroes
             };
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    private struct Marshaled_TargetEntityInfo
    {
        public int id;
        public int owningPlayerID;
        public int damage;
        public int attack;
        public int race;
        public int rarity;
        [MarshalAs(UnmanagedType.U4)]
        public TAG_CARDTYPE cardType;
        [MarshalAs(UnmanagedType.U1)]
        public bool isImmune;
        [MarshalAs(UnmanagedType.U1)]
        public bool canBeAttacked;
        [MarshalAs(UnmanagedType.U1)]
        public bool canBeTargetedByOpponents;
        [MarshalAs(UnmanagedType.U1)]
        public bool canBeTargetedBySpells;
        [MarshalAs(UnmanagedType.U1)]
        public bool canBeTargetedByHeroPowers;
        [MarshalAs(UnmanagedType.U1)]
        public bool canBeTargetedByBattlecries;
        [MarshalAs(UnmanagedType.U1)]
        public bool isFrozen;
        [MarshalAs(UnmanagedType.U1)]
        public bool isEnchanted;
        [MarshalAs(UnmanagedType.U1)]
        public bool isStealthed;
        [MarshalAs(UnmanagedType.U1)]
        public bool isTaunter;
        [MarshalAs(UnmanagedType.U1)]
        public bool isMagnet;
        [MarshalAs(UnmanagedType.U1)]
        public bool hasCharge;
        [MarshalAs(UnmanagedType.U1)]
        public bool hasAttackedThisTurn;
        [MarshalAs(UnmanagedType.U1)]
        public bool hasBattlecry;
        [MarshalAs(UnmanagedType.U1)]
        public bool hasDeathrattle;
        public static PlayErrors.Marshaled_TargetEntityInfo ConvertFromTargetEntityInfo(PlayErrors.TargetEntityInfo targetInfo)
        {
            return new PlayErrors.Marshaled_TargetEntityInfo { 
                id = targetInfo.id, owningPlayerID = targetInfo.owningPlayerID, damage = targetInfo.damage, attack = targetInfo.attack, cardType = targetInfo.cardType, race = targetInfo.race, rarity = targetInfo.rarity, isImmune = targetInfo.isImmune, canBeAttacked = targetInfo.canBeAttacked, canBeTargetedByOpponents = targetInfo.canBeTargetedByOpponents, canBeTargetedBySpells = targetInfo.canBeTargetedBySpells, canBeTargetedByHeroPowers = targetInfo.canBeTargetedByHeroPowers, canBeTargetedByBattlecries = targetInfo.canBeTargetedByBattlecries, isFrozen = targetInfo.isFrozen, isEnchanted = targetInfo.isEnchanted, isStealthed = targetInfo.isStealthed, 
                isTaunter = targetInfo.isTaunter, isMagnet = targetInfo.isMagnet, hasCharge = targetInfo.hasCharge, hasAttackedThisTurn = targetInfo.hasAttackedThisTurn, hasBattlecry = targetInfo.hasBattlecry, hasDeathrattle = targetInfo.hasDeathrattle
             };
        }
    }

    public class PlayerInfo
    {
        public bool comboActive = false;
        public bool enemyWeaponEquipped = false;
        public int id = 0;
        public bool isCurrentPlayer = false;
        public int numDragonsInHand = 0;
        public int numEnemyMinionsInPlay = 0;
        public int numFriendlyMinionsInPlay = 0;
        public int numFriendlyMinionsThatDiedThisGame = 0;
        public int numFriendlyMinionsThatDiedThisTurn = 0;
        public int numMinionSlotsPerPlayer = 0;
        public int numOpenSecretSlots = 0;
        public int numResources = 0;
        public bool steadyShotRequiresTarget = false;
        public bool weaponEquipped = false;
    }

    public class PlayRequirementInfo
    {
        public int paramMaxAtk = 0;
        public int paramMinAtk = 0;
        public int paramMinNumEnemyMinions = 0;
        public int paramMinNumFriendlyMinions = 0;
        public int paramMinNumTotalMinions = 0;
        public int paramNumMinionSlots = 0;
        public int paramNumMinionSlotsWithTarget = 0;
        public int paramRace = 0;
        public ulong requirementsMap = 0L;
    }

    public class SourceEntityInfo
    {
        public int attack = 0;
        public bool canAttack = true;
        public bool cannotAttackHeroes = false;
        public TAG_CARDTYPE cardType = TAG_CARDTYPE.MINION;
        public int cost = 0;
        public bool entireEntourageInPlay = false;
        public bool hasBattlecry = false;
        public bool hasCharge = false;
        public int id = 0;
        public bool isActionPower = false;
        public bool isActivatePower = false;
        public bool isAttackPower = false;
        public bool isChoiceMinion = false;
        public bool isDuplicateSecret = false;
        public bool isExhausted = false;
        public bool isFrozen = false;
        public bool isMasterPower = false;
        public bool isSecret = false;
        public int maxAttackRequirement = 0;
        public int minAttackRequirement = 0;
        public int minEnemyMinionsRequirement = 0;
        public int minFriendlyMinionsRequirement = 0;
        public int minTotalMinionsRequirement = 0;
        public int numAttacksThisTurn = 0;
        public int numMinionSlotsRequirement = 0;
        public int numMinionSlotsWithTargetRequirement = 0;
        public int numTurnsInPlay = 0;
        public int numWindfury = 0;
        public int raceRequirement = 0;
        public ulong requirementsMap = 0L;
        public TAG_ZONE zone = TAG_ZONE.SETASIDE;
    }

    public class TargetEntityInfo
    {
        public int attack = 0;
        public bool canBeAttacked = true;
        public bool canBeTargetedByBattlecries = true;
        public bool canBeTargetedByHeroPowers = true;
        public bool canBeTargetedByOpponents = true;
        public bool canBeTargetedBySpells = true;
        public TAG_CARDTYPE cardType = TAG_CARDTYPE.MINION;
        public int damage = 0;
        public bool hasAttackedThisTurn = false;
        public bool hasBattlecry = false;
        public bool hasCharge = false;
        public bool hasDeathrattle = false;
        public int id = 0;
        public bool isEnchanted = false;
        public bool isFrozen = false;
        public bool isImmune = false;
        public bool isMagnet = false;
        public bool isStealthed = false;
        public bool isTaunter = false;
        public int owningPlayerID = 0;
        public int race = 0;
        public int rarity = 0;
    }
}

