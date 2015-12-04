using System;
using UnityEngine;

public class RankedPlayDisplay : MonoBehaviour
{
    public RankedPlayToggleButton m_casualButton;
    private TournamentMedal m_medal;
    public Transform m_medalBone;
    public TournamentMedal m_medalPrefab;
    public RankedPlayToggleButton m_rankedButton;

    private void Awake()
    {
        this.m_medal = UnityEngine.Object.Instantiate<TournamentMedal>(this.m_medalPrefab);
        this.SetRankedMedalTransform(this.m_medalBone);
        this.m_medal.GetComponent<Collider>().enabled = false;
        this.m_casualButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnCasualButtonOver));
        this.m_rankedButton.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.OnRankedButtonOver));
        this.m_casualButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnCasualButtonOut));
        this.m_rankedButton.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.OnRankedButtonOut));
    }

    private void OnCasualButtonOut(UIEvent e)
    {
        this.m_casualButton.GetComponent<TooltipZone>().HideTooltip();
    }

    private void OnCasualButtonOver(UIEvent e)
    {
        this.m_casualButton.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get("GLUE_TOURNAMENT_CASUAL"), GameStrings.Get("GLUE_TOOLTIP_CASUAL_BUTTON"), 5f, true);
    }

    private void OnCasualButtonRelease(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("tournament_screen_select_hero");
        Options.Get().SetBool(Option.IN_RANKED_PLAY_MODE, false);
        this.UpdateMode();
    }

    private void OnRankedButtonOut(UIEvent e)
    {
        this.m_rankedButton.GetComponent<TooltipZone>().HideTooltip();
    }

    private void OnRankedButtonOver(UIEvent e)
    {
        this.m_rankedButton.GetComponent<TooltipZone>().ShowTooltip(GameStrings.Get("GLUE_TOURNAMENT_RANKED"), GameStrings.Get("GLUE_TOOLTIP_RANKED_BUTTON"), 5f, true);
    }

    private void OnRankedButtonRelease(UIEvent e)
    {
        SoundManager.Get().LoadAndPlay("tournament_screen_select_hero");
        Options.Get().SetBool(Option.IN_RANKED_PLAY_MODE, true);
        this.UpdateMode();
    }

    public void SetRankedMedal(NetCache.NetCacheMedalInfo medal)
    {
        this.m_medal.SetMedal(medal);
    }

    public void SetRankedMedalTransform(Transform bone)
    {
        this.m_medal.transform.parent = bone;
        this.m_medal.transform.localScale = Vector3.one;
        this.m_medal.transform.localPosition = Vector3.zero;
    }

    public void UpdateMode()
    {
        bool @bool = Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE);
        NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
        if (((netObject != null) && (netObject.Games != null)) && !netObject.Games.Casual)
        {
            this.m_casualButton.SetEnabled(false);
            if (!@bool)
            {
                @bool = true;
                Options.Get().SetBool(Option.IN_RANKED_PLAY_MODE, true);
            }
        }
        else
        {
            this.m_casualButton.SetEnabled(true);
        }
        if (@bool)
        {
            DeckPickerTrayDisplay.Get().SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY_RANKED"));
            if (UniversalInputManager.UsePhoneUI != null)
            {
                DeckPickerTrayDisplay.Get().ToggleRankedDetailsTray(true);
            }
            this.m_casualButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCasualButtonRelease));
            this.m_rankedButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnRankedButtonRelease));
            this.m_casualButton.Up();
            this.m_rankedButton.Down();
        }
        else
        {
            DeckPickerTrayDisplay.Get().SetPlayButtonText(GameStrings.Get("GLOBAL_PLAY"));
            if (UniversalInputManager.UsePhoneUI != null)
            {
                DeckPickerTrayDisplay.Get().ToggleRankedDetailsTray(false);
            }
            this.m_casualButton.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCasualButtonRelease));
            this.m_rankedButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnRankedButtonRelease));
            this.m_casualButton.Down();
            this.m_rankedButton.Up();
        }
        if (DeckPickerTrayDisplay.Get().m_playButton.IsEnabled())
        {
            DeckPickerTrayDisplay.Get().m_playButton.m_newPlayButtonText.TextAlpha = 1f;
        }
        else
        {
            DeckPickerTrayDisplay.Get().m_playButton.m_newPlayButtonText.TextAlpha = 0f;
        }
        DeckPickerTrayDisplay.Get().UpdateRankedClassWinsPlate();
    }
}

