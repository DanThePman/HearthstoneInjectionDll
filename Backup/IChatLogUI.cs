using System;
using UnityEngine;

public interface IChatLogUI
{
    void GoBack();
    void Hide();
    void ShowForPlayer(BnetPlayer player);

    UnityEngine.GameObject GameObject { get; }

    bool IsShowing { get; }

    BnetPlayer Receiver { get; }
}

