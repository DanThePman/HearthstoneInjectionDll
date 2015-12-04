using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public ActorStateMgr m_ActorStateMgr;
    public GameObject m_EndTurnButtonMesh;
    public GameObject m_GreenHighlight;
    private bool m_inputBlocked;
    private bool m_mousedOver;
    public UberText m_MyTurnText;
    private bool m_playedNmpSoundThisTurn;
    private bool m_pressed;
    public UberText m_WaitingText;
    public GameObject m_WhiteHighlight;
    private static EndTurnButton s_instance;

    private void Awake()
    {
        s_instance = this;
        this.m_MyTurnText.Text = GameStrings.Get("GAMEPLAY_END_TURN");
        this.m_WaitingText.Text = string.Empty;
        base.GetComponent<Collider>().enabled = false;
    }

    public static EndTurnButton Get()
    {
        return s_instance;
    }

    public GameObject GetButtonContainer()
    {
        return base.transform.FindChild("ButtonContainer").gameObject;
    }

    private void HandleGameStart()
    {
        this.UpdateState();
        GameState state = GameState.Get();
        if (state.IsPastBeginPhase() && state.IsFriendlySidePlayerTurn())
        {
            base.GetComponent<Collider>().enabled = true;
            GameState.Get().RegisterOptionsReceivedListener(new GameState.OptionsReceivedCallback(this.OnOptionsReceived));
        }
    }

    public void HandleMouseOut()
    {
        this.m_mousedOver = false;
        if (!this.m_inputBlocked)
        {
            if (this.m_pressed)
            {
                this.PlayButtonUpAnimation();
            }
            this.PutInMouseOffState();
        }
    }

    public void HandleMouseOver()
    {
        this.m_mousedOver = true;
        if (!this.m_inputBlocked)
        {
            this.PutInMouseOverState();
        }
    }

    public bool HasNoMorePlays()
    {
        Network.Options optionsPacket = GameState.Get().GetOptionsPacket();
        if (optionsPacket == null)
        {
            return false;
        }
        if (optionsPacket.List == null)
        {
            return false;
        }
        if (optionsPacket.List.Count > 1)
        {
            return false;
        }
        return true;
    }

    public bool IsInNMPState()
    {
        return (this.m_ActorStateMgr.GetActiveStateType() == ActorStateType.ENDTURN_NO_MORE_PLAYS);
    }

    public bool IsInputBlocked()
    {
        return this.m_inputBlocked;
    }

    public bool IsInWaitingState()
    {
        switch (this.m_ActorStateMgr.GetActiveStateType())
        {
            case ActorStateType.ENDTURN_WAITING:
                return true;

            case ActorStateType.ENDTURN_NMP_2_WAITING:
                return true;

            case ActorStateType.ENDTURN_WAITING_TIMER:
                return true;
        }
        return false;
    }

    public bool IsInYouHavePlaysState()
    {
        return (this.m_ActorStateMgr.GetActiveStateType() == ActorStateType.ENDTURN_YOUR_TURN);
    }

    private void OnCreateGame(GameState.CreateGamePhase phase, object userData)
    {
        if (phase == GameState.CreateGamePhase.CREATED)
        {
            GameState.Get().UnregisterCreateGameListener(new GameState.CreateGameCallback(this.OnCreateGame));
            this.HandleGameStart();
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void OnEndTurnRequested()
    {
        PegCursor.Get().SetMode(PegCursor.Mode.WAITING);
        this.SetStateToWaiting();
        base.GetComponent<Collider>().enabled = false;
        GameState.Get().UnregisterOptionsReceivedListener(new GameState.OptionsReceivedCallback(this.OnOptionsReceived));
    }

    public void OnMulliganEnded()
    {
        this.m_WaitingText.Text = GameStrings.Get("GAMEPLAY_ENEMY_TURN");
    }

    private void OnOptionsReceived(object userData)
    {
        this.UpdateState();
    }

    public void OnTurnStartManagerFinished()
    {
        PegCursor.Get().SetMode(PegCursor.Mode.STOPWAITING);
        this.m_playedNmpSoundThisTurn = false;
        this.SetStateToYourTurn();
        base.GetComponent<Collider>().enabled = true;
        GameState.Get().RegisterOptionsReceivedListener(new GameState.OptionsReceivedCallback(this.OnOptionsReceived));
    }

    public void OnTurnTimerEnded(bool isFriendlyPlayerTurnTimer)
    {
        if (isFriendlyPlayerTurnTimer)
        {
            this.SetButtonState(ActorStateType.ENDTURN_WAITING_TIMER);
        }
    }

    public void OnTurnTimerStart()
    {
        if (!this.m_inputBlocked && this.m_mousedOver)
        {
        }
    }

    private void OnUpdateIntensityValue(float newValue)
    {
        this.m_GreenHighlight.GetComponent<Renderer>().material.SetFloat("_Intensity", newValue);
    }

    public void PlayButtonUpAnimation()
    {
        if ((!this.m_inputBlocked && !this.IsInWaitingState()) && this.m_pressed)
        {
            this.m_pressed = false;
            this.GetButtonContainer().GetComponent<Animation>().Play("ENDTURN_PRESSED_UP");
            SoundManager.Get().LoadAndPlay("FX_EndTurn_Up");
        }
    }

    [DebuggerHidden]
    private IEnumerator PlayEndTurnSound()
    {
        return new <PlayEndTurnSound>c__Iterator8C { <>f__this = this };
    }

    public void PlayPushDownAnimation()
    {
        if ((!this.m_inputBlocked && !this.IsInWaitingState()) && !this.m_pressed)
        {
            this.m_pressed = true;
            this.GetButtonContainer().GetComponent<Animation>().Play("ENDTURN_PRESSED_DOWN");
            SoundManager.Get().LoadAndPlay("FX_EndTurn_Down");
        }
    }

    private void PutInMouseOffState()
    {
        this.m_WhiteHighlight.SetActive(false);
        if (this.IsInNMPState())
        {
            this.m_GreenHighlight.SetActive(true);
            object[] args = new object[] { "from", this.m_GreenHighlight.GetComponent<Renderer>().material.GetFloat("_Intensity"), "to", 1.1f, "time", 0.15f, "easetype", iTween.EaseType.linear, "onupdate", "OnUpdateIntensityValue", "onupdatetarget", base.gameObject, "name", "ENDTURN_INTENSITY" };
            Hashtable hashtable = iTween.Hash(args);
            iTween.StopByName(base.gameObject, "ENDTURN_INTENSITY");
            iTween.ValueTo(base.gameObject, hashtable);
        }
        else
        {
            this.m_GreenHighlight.SetActive(false);
        }
    }

    private void PutInMouseOverState()
    {
        if (this.IsInNMPState())
        {
            this.m_WhiteHighlight.SetActive(false);
            this.m_GreenHighlight.SetActive(true);
            object[] args = new object[] { "from", this.m_GreenHighlight.GetComponent<Renderer>().material.GetFloat("_Intensity"), "to", 1.4f, "time", 0.15f, "easetype", iTween.EaseType.linear, "onupdate", "OnUpdateIntensityValue", "onupdatetarget", base.gameObject, "name", "ENDTURN_INTENSITY" };
            Hashtable hashtable = iTween.Hash(args);
            iTween.StopByName(base.gameObject, "ENDTURN_INTENSITY");
            iTween.ValueTo(base.gameObject, hashtable);
        }
        else if (this.IsInYouHavePlaysState())
        {
            this.m_WhiteHighlight.SetActive(true);
            this.m_GreenHighlight.SetActive(false);
        }
        else
        {
            this.m_WhiteHighlight.SetActive(false);
            this.m_GreenHighlight.SetActive(false);
        }
    }

    private void SetButtonState(ActorStateType stateType)
    {
        if (this.m_ActorStateMgr == null)
        {
            UnityEngine.Debug.Log("End Turn Button Actor State Manager is missing!");
        }
        else if ((this.m_ActorStateMgr.GetActiveStateType() != stateType) && !this.m_inputBlocked)
        {
            this.m_ActorStateMgr.ChangeState(stateType);
            switch (stateType)
            {
                case ActorStateType.ENDTURN_YOUR_TURN:
                case ActorStateType.ENDTURN_WAITING_TIMER:
                    this.m_inputBlocked = true;
                    base.StartCoroutine(this.WaitUntilAnimationIsCompleteAndThenUnblockInput());
                    break;
            }
        }
    }

    private void SetStateToNoMorePlays()
    {
        if (this.m_ActorStateMgr != null)
        {
            if (this.IsInWaitingState())
            {
                this.SetButtonState(ActorStateType.ENDTURN_YOUR_TURN);
            }
            else
            {
                this.SetButtonState(ActorStateType.ENDTURN_NO_MORE_PLAYS);
                if (this.m_mousedOver)
                {
                    this.PutInMouseOverState();
                }
                else
                {
                    this.PutInMouseOffState();
                }
            }
            if (!this.m_playedNmpSoundThisTurn)
            {
                this.m_playedNmpSoundThisTurn = true;
                base.StartCoroutine(this.PlayEndTurnSound());
            }
        }
    }

    private void SetStateToWaiting()
    {
        if (((this.m_ActorStateMgr != null) && !this.IsInWaitingState()) && !GameState.Get().IsGameOver())
        {
            if (this.IsInNMPState())
            {
                this.SetButtonState(ActorStateType.ENDTURN_NMP_2_WAITING);
            }
            else
            {
                this.SetButtonState(ActorStateType.ENDTURN_WAITING);
            }
            this.PutInMouseOffState();
        }
    }

    private void SetStateToYourTurn()
    {
        if (this.m_ActorStateMgr != null)
        {
            if (this.HasNoMorePlays())
            {
                this.SetStateToNoMorePlays();
            }
            else
            {
                this.SetButtonState(ActorStateType.ENDTURN_YOUR_TURN);
                if (this.m_mousedOver)
                {
                    this.PutInMouseOverState();
                }
                else
                {
                    this.PutInMouseOffState();
                }
            }
        }
    }

    private void Start()
    {
        base.StartCoroutine(this.WaitAFrameAndThenChangeState());
    }

    private void UpdateState()
    {
        if (!GameState.Get().IsFriendlySidePlayerTurn())
        {
            this.SetStateToWaiting();
        }
        else if ((!GameState.Get().IsMulliganManagerActive() && !GameState.Get().IsTurnStartManagerBlockingInput()) && (GameState.Get().GetResponseMode() != GameState.ResponseMode.NONE))
        {
            this.SetStateToYourTurn();
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitAFrameAndThenChangeState()
    {
        return new <WaitAFrameAndThenChangeState>c__Iterator8A { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitUntilAnimationIsCompleteAndThenUnblockInput()
    {
        return new <WaitUntilAnimationIsCompleteAndThenUnblockInput>c__Iterator8B { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <PlayEndTurnSound>c__Iterator8C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal EndTurnButton <>f__this;

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
                    this.$current = new WaitForSeconds(1.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.<>f__this.IsInNMPState())
                    {
                        SoundManager.Get().LoadAndPlay("VO_JobsDone", this.<>f__this.gameObject);
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
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

    [CompilerGenerated]
    private sealed class <WaitAFrameAndThenChangeState>c__Iterator8A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal EndTurnButton <>f__this;

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
                    this.$current = null;
                    this.$PC = 1;
                    return true;

                case 1:
                    if (!GameState.Get().IsGameCreated())
                    {
                        this.<>f__this.m_ActorStateMgr.ChangeState(ActorStateType.ENDTURN_WAITING);
                        GameState.Get().RegisterCreateGameListener(new GameState.CreateGameCallback(this.<>f__this.OnCreateGame));
                        break;
                    }
                    this.<>f__this.HandleGameStart();
                    break;

                default:
                    goto Label_0089;
            }
            this.$PC = -1;
        Label_0089:
            return false;
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

    [CompilerGenerated]
    private sealed class <WaitUntilAnimationIsCompleteAndThenUnblockInput>c__Iterator8B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal EndTurnButton <>f__this;

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
                    this.$current = new WaitForSeconds(this.<>f__this.m_ActorStateMgr.GetMaximumAnimationTimeOfActiveStates());
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_inputBlocked = false;
                    if (this.<>f__this.HasNoMorePlays())
                    {
                        this.<>f__this.SetStateToNoMorePlays();
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
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
}

