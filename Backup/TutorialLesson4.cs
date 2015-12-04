using System;
using UnityEngine;

public class TutorialLesson4 : MonoBehaviour
{
    public UberText m_taunt;
    public UberText m_tauntDescription;
    public UberText m_tauntDescriptionTitle;

    private void Awake()
    {
        this.m_tauntDescriptionTitle.SetGameStringText("GLOBAL_TUTORIAL_TAUNT");
        this.m_tauntDescription.SetGameStringText("GLOBAL_TUTORIAL_TAUNT_DESCRIPTION");
        this.m_taunt.SetGameStringText("GLOBAL_TUTORIAL_TAUNT");
    }
}

