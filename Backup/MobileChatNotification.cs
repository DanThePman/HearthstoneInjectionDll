using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MobileChatNotification : MonoBehaviour
{
    private State state;

    public event NotifiedEvent Notified;

    private void FireNotification()
    {
        if ((this.Notified != null) && (this.state != State.None))
        {
            this.Notified(this.GetStateText(this.state));
        }
    }

    private string GetStateText(State state)
    {
        if (state == State.None)
        {
            return string.Empty;
        }
        DescriptionAttribute attribute = typeof(State).GetField(state.ToString()).GetCustomAttributes(false)[0] as DescriptionAttribute;
        return GameStrings.Get(attribute.Description);
    }

    private void OnDestroy()
    {
        if ((GameState.Get() != null) && !SpectatorManager.Get().IsInSpectatorMode())
        {
            GameState.Get().UnregisterTurnChangedListener(new GameState.TurnChangedCallback(this.OnTurnChanged));
            GameState.Get().UnregisterTurnTimerUpdateListener(new GameState.TurnTimerUpdateCallback(this.OnTurnTimerUpdate));
        }
    }

    private void OnEnable()
    {
        this.state = State.None;
    }

    private void OnTurnChanged(int oldTurn, int newTurn, object userData)
    {
        if (GameState.Get().IsFriendlySidePlayerTurn())
        {
            this.state = State.YourTurn;
            this.FireNotification();
        }
    }

    private void OnTurnTimerUpdate(TurnTimerUpdate update, object userData)
    {
        if ((update.GetSecondsRemaining() > 0f) && GameState.Get().IsFriendlySidePlayerTurn())
        {
            this.state = State.TurnCountdown;
            this.FireNotification();
        }
    }

    private void Update()
    {
        if ((GameState.Get() == null) || SpectatorManager.Get().IsInSpectatorMode())
        {
            this.state = State.None;
        }
        else
        {
            GameState.Get().RegisterTurnChangedListener(new GameState.TurnChangedCallback(this.OnTurnChanged));
            GameState.Get().RegisterTurnTimerUpdateListener(new GameState.TurnTimerUpdateCallback(this.OnTurnTimerUpdate));
            if (GameState.Get().IsMulliganPhase())
            {
                if (this.state == State.None)
                {
                    this.state = State.GameStarted;
                    this.FireNotification();
                }
            }
            else if (this.state == State.GameStarted)
            {
                this.state = !GameState.Get().IsFriendlySidePlayerTurn() ? State.None : State.YourTurn;
                this.FireNotification();
            }
        }
    }

    public delegate void NotifiedEvent(string text);

    private enum State
    {
        [Description("GLOBAL_MOBILECHAT_NOTIFICATION_MULLIGAIN")]
        GameStarted = 1,
        None = 0,
        [Description("GLOBAL_MOBILECHAT_NOTIFICATION_TURN_COUNTDOWN")]
        TurnCountdown = 3,
        [Description("GLOBAL_MOBILECHAT_NOTIFICATION_YOUR_TURN")]
        YourTurn = 2
    }
}

