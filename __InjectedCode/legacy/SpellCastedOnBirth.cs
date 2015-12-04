#region " Imports "
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using UnityEngine;
#endregion 

#region " Referenced assemblies "
// - mscorlib v2.0.5.0
// - Assembly-CSharp-firstpass v0.0.0.0
// - System.Core v2.0.5.0
// - UnityEngine v0.0.0.0
// - PlayMaker v1.6.0.0
// - System v2.0.5.0
// - ICSharpCode.SharpZipLib v0.86.0.518
// - IFacebook v0.0.0.0
// - System.Xml v2.0.5.0
// - UnityEngine.UI v1.0.0.0
// - Assembly-CSharp v0.0.0.0
#endregion 

class Spell : MonoBehaviour
{
    // Limited support!
    // You can only reference methods or fields defined in the class (not in ancestors classes)
    // Fields and methods stubs are needed for compilation purposes only.
    // Reflexil will automaticaly map current type, fields or methods to original references.
    void OnBirth(SpellStateType prevStateType)
    {
        if (this.name == "Emote_Start(Clone)")//GAMESTART
        {
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("HearthstoneInjection");
            Microsoft.Win32.Registry.CurrentUser.CreateSubKey("HearthstoneInjection");
        }

        if (this.name == "GangUp_FX(Clone)") //Gang up
        {
            var dirKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("HearthstoneInjection");
            var boolKey = dirKey.CreateSubKey("BoolValuesKey");
            boolKey.SetValue(this.name, "true");
            dirKey.Close();
            boolKey.Close();
        }

        if (this.name == "Sneaky_Missile_Standard2(Clone)" ||
            this.name == "Barrel_Missile_Beer") //Anub'ar Ambusher with target & young brewmaster sap with target
        {
            var dirKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("HearthstoneInjection");
            var boolKey = dirKey.CreateSubKey("BoolValuesKey");
            boolKey.SetValue(this.name, this.GetTarget().name);
            dirKey.Close();
            boolKey.Close();
        }
        if (this.name == "Sneaky_Untargetted_Impact_Standard(Clone)") //Sap
        {
            var dirKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("HearthstoneInjection");
            var boolKey = dirKey.CreateSubKey("BoolValuesKey");
            boolKey.SetValue(this.name, "true");
            dirKey.Close();
            boolKey.Close();
        }

        this.UpdateTransform();
        this.FireStateStartedCallbacks(prevStateType);
    }

    #region " Methods stubs "
    // Do not add or update any method. If compilation fails because of a method declaration, comment it
    Spell()
    {
    }

    void Awake()
    {
    }

    void OnDestroy()
    {
    }

    void Start()
    {
    }

    void Update()
    {
    }

    SpellType GetSpellType()
    {
        return default(SpellType);
    }

    void SetSpellType(SpellType spellType)
    {
    }

    bool DoesBlockServerEvents()
    {
        return default(bool);
    }

    SuperSpell GetSuperSpellParent()
    {
        return default(SuperSpell);
    }

    PowerTaskList GetPowerTaskList()
    {
        return default(PowerTaskList);
    }

    Entity GetPowerSource()
    {
        return default(Entity);
    }

    Card GetPowerSourceCard()
    {
        return default(Card);
    }

    Entity GetPowerTarget()
    {
        return default(Entity);
    }

    Card GetPowerTargetCard()
    {
        return default(Card);
    }

    SpellLocation GetLocation()
    {
        return default(SpellLocation);
    }

    string GetLocationTransformName()
    {
        return default(string);
    }

    SpellFacing GetFacing()
    {
        return default(SpellFacing);
    }

    SpellFacingOptions GetFacingOptions()
    {
        return default(SpellFacingOptions);
    }

    void SetPosition(UnityEngine.Vector3 position)
    {
    }

    void SetLocalPosition(UnityEngine.Vector3 position)
    {
    }

    void SetOrientation(UnityEngine.Quaternion orientation)
    {
    }

    void SetLocalOrientation(UnityEngine.Quaternion orientation)
    {
    }

    void UpdateTransform()
    {
    }

    void UpdatePosition()
    {
    }

    void UpdateOrientation()
    {
    }

    UnityEngine.GameObject GetSource()
    {
        return default(UnityEngine.GameObject);
    }

    void SetSource(UnityEngine.GameObject go)
    {
    }

    void RemoveSource()
    {
    }

    bool IsSource(UnityEngine.GameObject go)
    {
        return default(bool);
    }

    Card GetSourceCard()
    {
        return default(Card);
    }

    System.Collections.Generic.List<UnityEngine.GameObject> GetTargets()
    {
        return default(System.Collections.Generic.List<UnityEngine.GameObject>);
    }

    UnityEngine.GameObject GetTarget()
    {
        return default(UnityEngine.GameObject);
    }

    void AddTarget(UnityEngine.GameObject go)
    {
    }

    void AddTargets(System.Collections.Generic.List<UnityEngine.GameObject> targets)
    {
    }

    bool RemoveTarget(UnityEngine.GameObject go)
    {
        return default(bool);
    }

    void RemoveAllTargets()
    {
    }

    bool IsTarget(UnityEngine.GameObject go)
    {
        return default(bool);
    }

    Card GetTargetCard()
    {
        return default(Card);
    }

    System.Collections.Generic.List<UnityEngine.GameObject> GetVisualTargets()
    {
        return default(System.Collections.Generic.List<UnityEngine.GameObject>);
    }

    UnityEngine.GameObject GetVisualTarget()
    {
        return default(UnityEngine.GameObject);
    }

    void AddVisualTarget(UnityEngine.GameObject go)
    {
    }

    void AddVisualTargets(System.Collections.Generic.List<UnityEngine.GameObject> targets)
    {
    }

    bool RemoveVisualTarget(UnityEngine.GameObject go)
    {
        return default(bool);
    }

    void RemoveAllVisualTargets()
    {
    }

    bool IsVisualTarget(UnityEngine.GameObject go)
    {
        return default(bool);
    }

    Card GetVisualTargetCard()
    {
        return default(Card);
    }

    bool IsShown()
    {
        return default(bool);
    }

    void Show()
    {
    }

    void Hide()
    {
    }

    void ActivateObjectContainer(bool enable)
    {
    }

    bool IsActive()
    {
        return default(bool);
    }

    void Activate()
    {
    }

    void Reactivate()
    {
    }

    void Deactivate()
    {
    }

    void ForceDeactivate()
    {
    }

    void ActivateState(SpellStateType stateType)
    {
    }

    void SafeActivateState(SpellStateType stateType)
    {
    }

    bool HasUsableState(SpellStateType stateType)
    {
        return default(bool);
    }

    SpellStateType GetActiveState()
    {
        return default(SpellStateType);
    }

    SpellState GetFirstSpellState(SpellStateType stateType)
    {
        return default(SpellState);
    }

    System.Collections.Generic.List<SpellState> GetActiveStateList()
    {
        return default(System.Collections.Generic.List<SpellState>);
    }

    bool IsFinished()
    {
        return default(bool);
    }

    void ChangeState(SpellStateType stateType)
    {
    }

    SpellStateType GuessNextStateType()
    {
        return default(SpellStateType);
    }

    SpellStateType GuessNextStateType(SpellStateType stateType)
    {
        return default(SpellStateType);
    }

    bool AttachPowerTaskList(PowerTaskList taskList)
    {
        return default(bool);
    }

    bool AddPowerTargets()
    {
        return default(bool);
    }

    PowerTaskList GetLastHandledTaskList(PowerTaskList taskList)
    {
        return default(PowerTaskList);
    }

    bool IsHandlingLastTaskList()
    {
        return default(bool);
    }

    void OnStateFinished()
    {
    }

    void OnSpellFinished()
    {
    }

    void OnFsmStateStarted(HutongGames.PlayMaker.FsmState state, SpellStateType stateType)
    {
    }

    void OnAttachPowerTaskList()
    {
    }

    void OnIdle(SpellStateType prevStateType)
    {
    }

    void OnAction(SpellStateType prevStateType)
    {
    }

    void OnCancel(SpellStateType prevStateType)
    {
    }

    void OnDeath(SpellStateType prevStateType)
    {
    }

    void OnNone(SpellStateType prevStateType)
    {
    }

    void BuildSpellStateMap()
    {
    }

    void BuildFsmStateMap()
    {
    }

    System.Collections.Generic.List<HutongGames.PlayMaker.FsmState> GenerateSpellFsmStateList()
    {
        return default(System.Collections.Generic.List<HutongGames.PlayMaker.FsmState>);
    }

    void ChangeStateImpl(SpellStateType stateType)
    {
    }

    void ChangeFsmState(SpellStateType stateType)
    {
    }

    System.Collections.IEnumerator WaitThenChangeFsmState(SpellStateType stateType)
    {
        return default(System.Collections.IEnumerator);
    }

    void ChangeFsmStateNow(SpellStateType stateType)
    {
    }

    void FinishIfNecessary()
    {
    }

    void CallStateFunction(SpellStateType prevStateType, SpellStateType stateType)
    {
    }

    void FireFinishedCallbacks()
    {
    }

    void FireStateFinishedCallbacks(SpellStateType prevStateType)
    {
    }

    void FireStateStartedCallbacks(SpellStateType prevStateType)
    {
    }

    bool HasStateContent(SpellStateType stateType)
    {
        return default(bool);
    }

    bool HasOverriddenStateMethod(SpellStateType stateType)
    {
        return default(bool);
    }

    string GetStateMethodName(SpellStateType stateType)
    {
        return default(string);
    }

    bool CanAddPowerTargets()
    {
        return default(bool);
    }

    bool AddSinglePowerTarget()
    {
        return default(bool);
    }

    bool AddSinglePowerTarget_FromSourceAction(Network.HistActionStart sourceAction)
    {
        return default(bool);
    }

    bool AddSinglePowerTarget_FromMetaData(System.Collections.Generic.List<PowerTask> tasks)
    {
        return default(bool);
    }

    bool AddSinglePowerTarget_FromAnyPower(Card sourceCard, System.Collections.Generic.List<PowerTask> tasks)
    {
        return default(bool);
    }

    bool AddMultiplePowerTargets()
    {
        return default(bool);
    }

    bool AddMultiplePowerTargets_FromMetaData(System.Collections.Generic.List<PowerTask> tasks)
    {
        return default(bool);
    }

    void AddMultiplePowerTargets_FromAnyPower(Card sourceCard, System.Collections.Generic.List<PowerTask> tasks)
    {
    }

    Card GetTargetCardFromPowerTask(int index, PowerTask task)
    {
        return default(Card);
    }

    void AddTargetFromMetaData(int metaDataIndex, Card targetCard)
    {
    }

    bool CompleteMetaDataTasks(int metaDataIndex)
    {
        return default(bool);
    }

    void ShowImpl()
    {
    }

    void HideImpl()
    {
    }

    void OnExitedNoneState()
    {
    }

    void OnEnteredNoneState()
    {
    }

    void BlockZones(bool block)
    {
    }

    void OnLoad()
    {
    }

    #endregion

    #region " Fields stubs "
    // Do not add or update any field. If compilation fails because of a field declaration, comment it
    bool m_BlockServerEvents;
    UnityEngine.GameObject m_ObjectContainer;
    SpellLocation m_Location;
    string m_LocationTransformName;
    bool m_SetParentToLocation;
    SpellFacing m_Facing;
    SpellFacingOptions m_FacingOptions;
    TARGET_RETICLE_TYPE m_TargetReticle;
    System.Collections.Generic.List<SpellZoneTag> m_ZonesToDisable;
    float m_ZoneLayoutDelayForDeaths;
    SpellType m_spellType;
    Map<SpellStateType, System.Collections.Generic.List<SpellState>> m_spellStateMap;
    SpellStateType m_activeStateType;
    SpellStateType m_activeStateChange;
    UnityEngine.GameObject m_source;
    System.Collections.Generic.List<UnityEngine.GameObject> m_targets;
    PowerTaskList m_taskList;
    bool m_shown;
    PlayMakerFSM m_fsm;
    Map<SpellStateType, HutongGames.PlayMaker.FsmState> m_fsmStateMap;
    bool m_fsmSkippedFirstFrame;
    bool m_fsmReady;
    bool m_positionDirty;
    bool m_orientationDirty;
    bool m_finished;
    #endregion

}
