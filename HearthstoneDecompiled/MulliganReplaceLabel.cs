using System;
using UnityEngine;

public class MulliganReplaceLabel : MonoBehaviour
{
    public UberText m_labelText;

    private void Awake()
    {
        this.m_labelText.Text = GameStrings.Get("GAMEPLAY_MULLIGAN_REPLACE");
    }
}

