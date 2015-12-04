using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HeroXPBar : PegUIElement
{
    public UberText m_barText;
    public float m_delay;
    public GameObject m_heroFrame;
    public NetCache.HeroLevel m_heroLevel;
    public UberText m_heroLevelText;
    public bool m_isAnimated;
    public bool m_isOnDeck;
    public PlayLevelUpEffectCallback m_levelUpCallback;
    public ProgressBar m_progressBar;
    private string m_rewardText;
    public GameObject m_simpleFrame;
    public int m_soloLevelLimit;

    public void AnimateBar(NetCache.HeroLevel.LevelInfo previousLevelInfo, NetCache.HeroLevel.LevelInfo currentLevelInfo)
    {
        this.m_heroLevelText.Text = previousLevelInfo.Level.ToString();
        if (previousLevelInfo.Level < currentLevelInfo.Level)
        {
            float prevVal = ((float) previousLevelInfo.XP) / ((float) previousLevelInfo.MaxXP);
            float currVal = 1f;
            this.m_progressBar.AnimateProgress(prevVal, currVal);
            float animationTime = this.m_progressBar.GetAnimationTime();
            base.StartCoroutine(this.AnimatePostLevelUpXp(animationTime, currentLevelInfo));
        }
        else
        {
            float num4 = ((float) previousLevelInfo.XP) / ((float) previousLevelInfo.MaxXP);
            float num5 = ((float) currentLevelInfo.XP) / ((float) currentLevelInfo.MaxXP);
            if (currentLevelInfo.IsMaxLevel())
            {
                num5 = 1f;
            }
            this.m_progressBar.AnimateProgress(num4, num5);
        }
    }

    [DebuggerHidden]
    private IEnumerator AnimatePostLevelUpXp(float delayTime, NetCache.HeroLevel.LevelInfo currentLevelInfo)
    {
        return new <AnimatePostLevelUpXp>c__Iterator25E { delayTime = delayTime, currentLevelInfo = currentLevelInfo, <$>delayTime = delayTime, <$>currentLevelInfo = currentLevelInfo, <>f__this = this };
    }

    protected override void Awake()
    {
        base.Awake();
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnProgressBarOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnProgressBarOut));
    }

    [DebuggerHidden]
    private IEnumerator DelayBarAnimation(NetCache.HeroLevel.LevelInfo prevInfo, NetCache.HeroLevel.LevelInfo currInfo)
    {
        return new <DelayBarAnimation>c__Iterator25F { prevInfo = prevInfo, currInfo = currInfo, <$>prevInfo = prevInfo, <$>currInfo = currInfo, <>f__this = this };
    }

    public void EmptyLevelUpFunction()
    {
    }

    private void OnProgressBarOut(UIEvent e)
    {
        base.gameObject.GetComponent<TooltipZone>().HideTooltip();
    }

    private void OnProgressBarOver(UIEvent e)
    {
        if (((this.m_heroLevel != null) && (this.m_heroLevel.NextReward != null)) && (this.m_heroLevel.NextReward.Reward != null))
        {
            RewardData reward = this.m_heroLevel.NextReward.Reward;
            this.m_rewardText = RewardUtils.GetRewardText(reward);
            this.ShowTooltip();
        }
    }

    public void SetBarText(string barText)
    {
        if (this.m_barText != null)
        {
            this.m_barText.Text = barText;
        }
    }

    public void SetBarValue(float barValue)
    {
        this.m_progressBar.SetProgressBar(barValue);
    }

    private void ShowTooltip()
    {
        float num;
        TooltipZone component = base.gameObject.GetComponent<TooltipZone>();
        if (SceneMgr.Get().IsInGame())
        {
            num = (float) KeywordHelpPanel.MULLIGAN_SCALE;
        }
        else if (UniversalInputManager.UsePhoneUI != null)
        {
            num = (float) KeywordHelpPanel.BOX_SCALE;
        }
        else
        {
            num = (float) KeywordHelpPanel.COLLECTION_MANAGER_SCALE;
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            num *= 1.1f;
        }
        object[] args = new object[] { this.m_heroLevel.NextReward.Level };
        component.ShowTooltip(GameStrings.Format("GLOBAL_HERO_LEVEL_NEXT_REWARD_TITLE", args), this.m_rewardText, num, true);
    }

    public void UpdateDisplay()
    {
        if (this.m_isOnDeck)
        {
            this.m_simpleFrame.SetActive(true);
            this.m_heroFrame.SetActive(false);
        }
        else
        {
            this.m_simpleFrame.SetActive(false);
            this.m_heroFrame.SetActive(true);
        }
        this.SetBarText(string.Empty);
        if (this.m_isAnimated)
        {
            this.m_heroLevelText.Text = this.m_heroLevel.PrevLevel.Level.ToString();
            this.SetBarValue(((float) this.m_heroLevel.PrevLevel.XP) / ((float) this.m_heroLevel.PrevLevel.MaxXP));
            base.StartCoroutine(this.DelayBarAnimation(this.m_heroLevel.PrevLevel, this.m_heroLevel.CurrentLevel));
        }
        else
        {
            this.m_heroLevelText.Text = this.m_heroLevel.CurrentLevel.Level.ToString();
            if (this.m_heroLevel.CurrentLevel.IsMaxLevel())
            {
                this.SetBarValue(1f);
            }
            else
            {
                this.SetBarValue(((float) this.m_heroLevel.CurrentLevel.XP) / ((float) this.m_heroLevel.CurrentLevel.MaxXP));
            }
        }
    }

    [CompilerGenerated]
    private sealed class <AnimatePostLevelUpXp>c__Iterator25E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal NetCache.HeroLevel.LevelInfo <$>currentLevelInfo;
        internal float <$>delayTime;
        internal HeroXPBar <>f__this;
        internal float <targetXP>__0;
        internal NetCache.HeroLevel.LevelInfo currentLevelInfo;
        internal float delayTime;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(this.delayTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    if ((this.currentLevelInfo.Level == 3) && !Options.Get().GetBool(Option.HAS_SEEN_LEVEL_3, false))
                    {
                        NotificationManager.Get().CreateInnkeeperQuote(new Vector3(155.3f, NotificationManager.DEPTH, 34.5f), GameStrings.Get("VO_INNKEEPER_LEVEL3_TIP"), "VO_INNKEEPER_LEVEL3_TIP", 0f, null);
                        Options.Get().SetBool(Option.HAS_SEEN_LEVEL_3, true);
                    }
                    this.<>f__this.m_heroLevelText.Text = this.currentLevelInfo.Level.ToString();
                    this.<targetXP>__0 = ((float) this.currentLevelInfo.XP) / ((float) this.currentLevelInfo.MaxXP);
                    this.<>f__this.m_progressBar.AnimateProgress(0f, this.<targetXP>__0);
                    if (this.<>f__this.m_levelUpCallback != null)
                    {
                        this.<>f__this.m_levelUpCallback();
                    }
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <DelayBarAnimation>c__Iterator25F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal NetCache.HeroLevel.LevelInfo <$>currInfo;
        internal NetCache.HeroLevel.LevelInfo <$>prevInfo;
        internal HeroXPBar <>f__this;
        internal NetCache.HeroLevel.LevelInfo currInfo;
        internal NetCache.HeroLevel.LevelInfo prevInfo;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.$current = new WaitForSeconds(this.<>f__this.m_delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.AnimateBar(this.prevInfo, this.currInfo);
                    this.$PC = -1;
                    break;
            }
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }

    public delegate void PlayLevelUpEffectCallback();
}

