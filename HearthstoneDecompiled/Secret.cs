using System;
using UnityEngine;

public class Secret : MonoBehaviour
{
    public UberText secretLabelBottom;
    public UberText secretLabelMiddle;
    public UberText secretLabelTop;

    private void Start()
    {
        this.secretLabelTop.SetGameStringText("GAMEPLAY_SECRET_BANNER_TITLE");
        this.secretLabelMiddle.SetGameStringText("GAMEPLAY_SECRET_BANNER_TITLE");
        this.secretLabelBottom.SetGameStringText("GAMEPLAY_SECRET_BANNER_TITLE");
    }
}

