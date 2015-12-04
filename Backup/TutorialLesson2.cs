using System;
using UnityEngine;

public class TutorialLesson2 : MonoBehaviour
{
    public UberText m_cost;
    public UberText m_yourMana;

    private void Awake()
    {
        this.m_cost.SetGameStringText("GLOBAL_TUTORIAL_COST");
        this.m_yourMana.SetGameStringText("GLOBAL_TUTORIAL_YOUR_MANA");
    }
}

