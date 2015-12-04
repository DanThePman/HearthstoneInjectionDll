using System;
using UnityEngine;

public class ActivateZoneDeck : MonoBehaviour
{
    public bool m_friendlyDeck;
    private bool onoff = true;

    public void ToggleActive()
    {
        if (((GameState.Get() == null) || (GameState.Get().GetFriendlySidePlayer() == null)) || (GameState.Get().GetOpposingSidePlayer() == null))
        {
            Debug.LogError("ActivateZoneDeck - Game State not yet initialized.");
        }
        else
        {
            ZoneDeck deckZone;
            if (this.m_friendlyDeck)
            {
                deckZone = GameState.Get().GetFriendlySidePlayer().GetDeckZone();
            }
            else
            {
                deckZone = GameState.Get().GetOpposingSidePlayer().GetDeckZone();
            }
            if (deckZone == null)
            {
                Debug.LogError("ActivateZoneDeck - zoneDeck is null!");
            }
            else
            {
                deckZone.SetVisibility(this.onoff);
            }
        }
    }
}

