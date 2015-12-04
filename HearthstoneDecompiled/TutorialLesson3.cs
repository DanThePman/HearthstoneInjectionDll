using System;
using UnityEngine;

public class TutorialLesson3 : MonoBehaviour
{
    public UberText m_attacker;
    public UberText m_defender;

    private void Awake()
    {
        this.m_attacker.SetGameStringText("GLOBAL_TUTORIAL_ATTACKER");
        this.m_defender.SetGameStringText("GLOBAL_TUTORIAL_DEFENDER");
    }
}

