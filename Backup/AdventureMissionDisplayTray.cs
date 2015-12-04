using System;
using System.Collections.Generic;
using UnityEngine;

public class AdventureMissionDisplayTray : MonoBehaviour
{
    public PegUIElement m_rewardsChest;
    public AdventureRewardsDisplayArea m_rewardsDisplay;
    public Transform m_rewardsDisplayBone;
    public SlidingTray m_slidingTray;

    private void Awake()
    {
        AdventureConfig config = AdventureConfig.Get();
        config.AddAdventureMissionSetListener(new AdventureConfig.AdventureMissionSet(this.OnMissionSelected));
        config.AddSubSceneChangeListener(new AdventureConfig.SubSceneChange(this.OnSubsceneChanged));
        if (this.m_rewardsChest != null)
        {
            this.m_rewardsChest.AddEventListener(UIEventType.ROLLOVER, e => this.ShowCardRewards());
            this.m_rewardsChest.AddEventListener(UIEventType.ROLLOUT, e => this.HideCardRewards());
        }
    }

    private void HideCardRewards()
    {
        this.m_rewardsDisplay.HideCardRewards();
    }

    private void OnDestroy()
    {
        AdventureConfig config = AdventureConfig.Get();
        if (config != null)
        {
            config.RemoveAdventureMissionSetListener(new AdventureConfig.AdventureMissionSet(this.OnMissionSelected));
            config.RemoveSubSceneChangeListener(new AdventureConfig.SubSceneChange(this.OnSubsceneChanged));
        }
    }

    private void OnMissionSelected(ScenarioDbId mission, bool showDetails)
    {
        if (mission != ScenarioDbId.INVALID)
        {
            if (showDetails)
            {
                this.m_slidingTray.ToggleTraySlider(true, null, true);
            }
            List<CardRewardData> immediateCardRewardsForDefeatingScenario = AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario((int) mission);
            bool flag = AdventureProgressMgr.Get().HasDefeatedScenario((int) mission);
            this.m_rewardsChest.gameObject.SetActive(((immediateCardRewardsForDefeatingScenario != null) && (immediateCardRewardsForDefeatingScenario.Count != 0)) && !flag);
        }
    }

    private void OnSubsceneChanged(AdventureSubScenes newscene, bool forward)
    {
        this.m_slidingTray.ToggleTraySlider(false, null, true);
    }

    private void ShowCardRewards()
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            NotificationManager.Get().DestroyActiveQuote(0.2f);
        }
        List<CardRewardData> immediateCardRewardsForDefeatingScenario = AdventureProgressMgr.Get().GetImmediateCardRewardsForDefeatingScenario((int) AdventureConfig.Get().GetMission());
        this.m_rewardsDisplay.ShowCardsNoFullscreen(immediateCardRewardsForDefeatingScenario, this.m_rewardsDisplayBone.position, new Vector3?(this.m_rewardsChest.transform.position));
    }
}

