using PegasusShared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TournamentDisplay : MonoBehaviour
{
    private bool m_allInitialized;
    private NetCache.NetCacheMedalInfo m_currentMedalInfo;
    public Vector3_MobileOverride m_deckPickerPosition;
    private DeckPickerTrayDisplay m_deckPickerTray;
    private bool m_deckPickerTrayLoaded;
    private List<DelMedalChanged> m_medalChangedListeners = new List<DelMedalChanged>();
    public TextMesh m_modeName;
    private bool m_netCacheReturned;
    private static TournamentDisplay s_instance;

    private void Awake()
    {
        AssetLoader.Get().LoadActor((UniversalInputManager.UsePhoneUI == null) ? "DeckPickerTray" : "DeckPickerTray_phone", new AssetLoader.GameObjectCallback(this.DeckPickerTrayLoaded), null, false);
        s_instance = this;
    }

    private void DeckPickerTrayLoaded(string name, GameObject go, object callbackData)
    {
        this.m_deckPickerTray = go.GetComponent<DeckPickerTrayDisplay>();
        this.m_deckPickerTray.SetHeaderText(GameStrings.Get("GLUE_TOURNAMENT"));
        this.m_deckPickerTray.transform.parent = base.transform;
        this.m_deckPickerTray.transform.localPosition = (Vector3) this.m_deckPickerPosition;
        this.m_deckPickerTrayLoaded = true;
    }

    public static TournamentDisplay Get()
    {
        return s_instance;
    }

    public NetCache.NetCacheMedalInfo GetCurrentMedalInfo()
    {
        return this.m_currentMedalInfo;
    }

    public int GetRankedWinsForClass(TAG_CLASS heroClass)
    {
        int num = 0;
        foreach (NetCache.PlayerRecord record in NetCache.Get().GetNetObject<NetCache.NetCachePlayerRecords>().Records)
        {
            if (record.Data != 0)
            {
                EntityDef entityDef = DefLoader.Get().GetEntityDef(record.Data);
                if (((entityDef != null) && (entityDef.GetClass() == heroClass)) && (record.RecordType == GameType.GT_RANKED))
                {
                    num += record.Wins;
                }
            }
        }
        return num;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void RegisterMedalChangedListener(DelMedalChanged listener)
    {
        if (!this.m_medalChangedListeners.Contains(listener))
        {
            this.m_medalChangedListeners.Add(listener);
        }
    }

    public void RemoveMedalChangedListener(DelMedalChanged listener)
    {
        this.m_medalChangedListeners.Remove(listener);
    }

    private void Start()
    {
        MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_Tournament);
        NetCache.Get().RegisterScreenTourneys(new NetCache.NetCacheCallback(this.UpdateTourneyPage), new NetCache.ErrorCallback(NetCache.DefaultErrorHandler));
    }

    public void Unload()
    {
        NetCache.Get().UnregisterNetCacheHandler(new NetCache.NetCacheCallback(this.UpdateTourneyPage));
    }

    private void Update()
    {
        if (!this.m_allInitialized && (this.m_netCacheReturned && this.m_deckPickerTrayLoaded))
        {
            base.StartCoroutine(this.UpdateTourneyPageWhenReady());
            this.m_deckPickerTray.Init();
            this.m_allInitialized = true;
        }
    }

    private void UpdateTourneyPage()
    {
        if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.Tournament)
        {
            if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
            {
                SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                Error.AddWarningLoc("GLOBAL_FEATURE_DISABLED_TITLE", "GLOBAL_FEATURE_DISABLED_MESSAGE_PLAY", new object[0]);
            }
        }
        else
        {
            NetCache.NetCacheMedalInfo netObject = NetCache.Get().GetNetObject<NetCache.NetCacheMedalInfo>();
            bool flag = (this.m_currentMedalInfo != null) && ((netObject.StarLevel != this.m_currentMedalInfo.StarLevel) || (netObject.Stars != this.m_currentMedalInfo.Stars));
            this.m_currentMedalInfo = netObject;
            if (flag)
            {
                foreach (DelMedalChanged changed in this.m_medalChangedListeners.ToArray())
                {
                    changed(this.m_currentMedalInfo);
                }
            }
            this.m_netCacheReturned = true;
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdateTourneyPageWhenReady()
    {
        return new <UpdateTourneyPageWhenReady>c__Iterator1E9 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <UpdateTourneyPageWhenReady>c__Iterator1E9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TournamentDisplay <>f__this;

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
                case 1:
                    if (!AchieveManager.Get().IsReady() || !this.<>f__this.m_deckPickerTray.IsLoaded())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (AchieveManager.Get().HasActiveQuests(true))
                    {
                        WelcomeQuests.Show(false, null, false);
                    }
                    else
                    {
                        GameToastMgr.Get().UpdateQuestProgressToasts();
                        GameToastMgr.Get().AddSeasonTimeRemainingToast();
                    }
                    this.<>f__this.m_deckPickerTray.UpdateRankedPlayDisplay();
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

    public delegate void DelMedalChanged(NetCache.NetCacheMedalInfo medalInfo);
}

