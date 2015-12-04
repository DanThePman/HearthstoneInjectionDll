using System;
using UnityEngine;

public class RewardPackage : PegUIElement
{
    public int m_RewardIndex = -1;

    protected override void Awake()
    {
        base.Awake();
    }

    public void OnDisable()
    {
        base.SetEnabled(false);
    }

    public void OnEnable()
    {
        base.SetEnabled(true);
    }

    protected override void OnOver(PegUIElement.InteractionState oldState)
    {
        base.GetComponent<PlayMakerFSM>().SendEvent("Action");
    }

    protected override void OnPress()
    {
        this.OpenReward();
    }

    private void OpenReward()
    {
        base.GetComponent<PlayMakerFSM>().SendEvent("Death");
        RewardBoxesDisplay.Get().OpenReward(this.m_RewardIndex, base.transform.position);
        base.enabled = false;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && (this.m_RewardIndex == 0))
        {
            this.OpenReward();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && (this.m_RewardIndex == 1))
        {
            this.OpenReward();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && (this.m_RewardIndex == 2))
        {
            this.OpenReward();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && (this.m_RewardIndex == 3))
        {
            this.OpenReward();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && (this.m_RewardIndex == 4))
        {
            this.OpenReward();
        }
    }
}

