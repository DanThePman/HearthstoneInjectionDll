using System;

public class StandardGameEntity : GameEntity
{
    public override void OnTagChanged(TagDelta change)
    {
        switch (((GAME_TAG) change.tag))
        {
            case GAME_TAG.STEP:
                if (change.newValue == 4)
                {
                    MulliganManager.Get().BeginMulligan();
                }
                break;

            case GAME_TAG.NEXT_STEP:
                if (change.newValue == 6)
                {
                    if (GameState.Get().IsMulliganManagerActive())
                    {
                        GameState.Get().SetMulliganPowerBlocker(true);
                    }
                }
                else if (((change.oldValue == 9) && (change.newValue == 10)) && GameState.Get().IsFriendlySidePlayerTurn())
                {
                    TurnStartManager.Get().BeginPlayingTurnEvents();
                }
                break;
        }
        base.OnTagChanged(change);
    }
}

