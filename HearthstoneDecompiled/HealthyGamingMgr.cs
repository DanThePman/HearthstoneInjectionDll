using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HealthyGamingMgr : MonoBehaviour
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map3C;
    private const float CAIS_ACTIVE_MESSAGE_DISPLAY_TIME = 60f;
    private const float CAIS_MESSAGE_DISPLAY_TIME = 60f;
    private const float CHECK_INTERVAL = 300f;
    private const float KOREA_MESSAGE_DISPLAY_TIME = 5f;
    private string m_AccountCountry = string.Empty;
    private bool m_BattleNetReady;
    private bool m_DebugMode;
    private bool m_HealthyGamingArenaEnabled = true;
    private bool m_NetworkDataReady;
    private float m_NextCheckTime;
    private float m_NextMessageDisplayTime;
    private BattleNet.DllLockouts m_Restrictions;
    private ulong m_SessionStartTime;
    private int m_TimePlayed;
    private int m_TimeRested;
    private static HealthyGamingMgr s_Instance;

    private void Awake()
    {
        s_Instance = this;
        if (Options.Get().GetBool(Option.HEALTHY_GAMING_DEBUG, false))
        {
            this.m_DebugMode = true;
        }
        this.m_NextCheckTime = UnityEngine.Time.realtimeSinceStartup + 45f;
        ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
        ApplicationMgr.Get().Resetting += new System.Action(this.OnReset);
        FatalErrorMgr.Get().AddErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        base.StartCoroutine(this.InitNetworkData());
    }

    private void ChinaRestrictions()
    {
        BattleNet.GetPlayRestrictions(ref this.m_Restrictions, true);
        base.StartCoroutine(this.ChinaRestrictionsUpdate());
    }

    private void ChinaRestrictions_3to5Hours(int minutesPlayed)
    {
        if (this.m_NextMessageDisplayTime < 0f)
        {
            this.m_NextMessageDisplayTime = this.m_TimePlayed + (30 - (minutesPlayed % 30));
        }
        else
        {
            this.m_NextMessageDisplayTime = this.m_TimePlayed + 30;
        }
        string textArg = GameStrings.Get("GLOBAL_HEALTHY_GAMING_CHINA_THREE_TO_FIVE_HOURS");
        SocialToastMgr.Get().AddToast(textArg, SocialToastMgr.TOAST_TYPE.DEFAULT, (float) 60f);
        if (this.m_DebugMode)
        {
            Log.HealthyGaming.Print("GLOBAL_HEALTHY_GAMING_CHINA_THREE_TO_FIVE_HOURS: " + minutesPlayed.ToString(), new object[0]);
            Log.HealthyGaming.Print(GameStrings.Get("GLOBAL_HEALTHY_GAMING_CHINA_THREE_TO_FIVE_HOURS"), new object[0]);
            Log.HealthyGaming.Print("NextMessageDisplayTime: " + this.m_NextMessageDisplayTime.ToString(), new object[0]);
        }
        else
        {
            Log.HealthyGaming.Print(string.Format("Time: {0},  Played: {1},  {2}", UnityEngine.Time.realtimeSinceStartup, minutesPlayed, textArg), new object[0]);
        }
    }

    private void ChinaRestrictions_LessThan3Hours(int minutesPlayed, int hours)
    {
        if (this.m_NextMessageDisplayTime < 0f)
        {
            this.m_NextMessageDisplayTime = this.m_TimePlayed + (60 - (minutesPlayed % 60));
        }
        else
        {
            this.m_NextMessageDisplayTime = this.m_TimePlayed + 60;
        }
        object[] args = new object[] { hours };
        string textArg = GameStrings.Format("GLOBAL_HEALTHY_GAMING_CHINA_LESS_THAN_THREE_HOURS", args);
        SocialToastMgr.Get().AddToast(textArg, SocialToastMgr.TOAST_TYPE.DEFAULT, (float) 60f);
        if (this.m_DebugMode)
        {
            Log.HealthyGaming.Print("GLOBAL_HEALTHY_GAMING_CHINA_LESS_THAN_THREE_HOURS: " + minutesPlayed.ToString(), new object[0]);
            object[] objArray2 = new object[] { hours };
            Log.HealthyGaming.Print(GameStrings.Format("GLOBAL_HEALTHY_GAMING_CHINA_LESS_THAN_THREE_HOURS", objArray2), new object[0]);
            Log.HealthyGaming.Print("NextMessageDisplayTime: " + this.m_NextMessageDisplayTime.ToString(), new object[0]);
        }
        else
        {
            Log.HealthyGaming.Print(string.Format("Time: {0},  Played: {1},  {2}", UnityEngine.Time.realtimeSinceStartup, minutesPlayed, textArg), new object[0]);
        }
    }

    private void ChinaRestrictions_LockoutFeatures(int minutesPlayed)
    {
        this.m_HealthyGamingArenaEnabled = false;
        Box box = Box.Get();
        if (box != null)
        {
            box.UpdateUI(false);
        }
    }

    private void ChinaRestrictions_MoreThan5Hours(int minutesPlayed)
    {
        if (this.m_NextMessageDisplayTime < 0f)
        {
            this.m_NextMessageDisplayTime = this.m_TimePlayed + (15 - (minutesPlayed % 15));
        }
        else
        {
            this.m_NextMessageDisplayTime = this.m_TimePlayed + 15;
        }
        string textArg = GameStrings.Get("GLOBAL_HEALTHY_GAMING_CHINA_MORE_THAN_FIVE_HOURS");
        SocialToastMgr.Get().AddToast(textArg, SocialToastMgr.TOAST_TYPE.DEFAULT, (float) 60f);
        if (this.m_DebugMode)
        {
            Log.HealthyGaming.Print("GLOBAL_HEALTHY_GAMING_CHINA_MORE_THAN_FIVE_HOURS: " + minutesPlayed.ToString(), new object[0]);
            Log.HealthyGaming.Print(GameStrings.Get("GLOBAL_HEALTHY_GAMING_CHINA_MORE_THAN_FIVE_HOURS"), new object[0]);
            Log.HealthyGaming.Print("NextMessageDisplayTime: " + this.m_NextMessageDisplayTime.ToString(), new object[0]);
        }
        else
        {
            Log.HealthyGaming.Print(string.Format("Time: {0},  Played: {1},  {2}", UnityEngine.Time.realtimeSinceStartup, minutesPlayed, textArg), new object[0]);
        }
    }

    [DebuggerHidden]
    private IEnumerator ChinaRestrictionsUpdate()
    {
        return new <ChinaRestrictionsUpdate>c__Iterator25B { <>f__this = this };
    }

    public static HealthyGamingMgr Get()
    {
        return s_Instance;
    }

    public ulong GetSessionStartTime()
    {
        return this.m_SessionStartTime;
    }

    [DebuggerHidden]
    private IEnumerator InitNetworkData()
    {
        return new <InitNetworkData>c__Iterator25A { <>f__this = this };
    }

    public bool isArenaEnabled()
    {
        return this.m_HealthyGamingArenaEnabled;
    }

    private void KoreaRestrictions()
    {
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        if (this.m_DebugMode)
        {
            Log.HealthyGaming.Print("Minutes Played: " + (realtimeSinceStartup / 60f), new object[0]);
        }
        if (realtimeSinceStartup >= this.m_NextMessageDisplayTime)
        {
            this.m_NextMessageDisplayTime += 3600f;
            int num2 = ((int) (realtimeSinceStartup / 60f)) / 60;
            object[] args = new object[] { num2 };
            SocialToastMgr.Get().AddToast(GameStrings.Format("GLOBAL_HEALTHY_GAMING_TOAST", args), SocialToastMgr.TOAST_TYPE.DEFAULT, (float) 5f);
        }
    }

    private void OnDestroy()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        ApplicationMgr.Get().Resetting -= new System.Action(this.OnReset);
        FatalErrorMgr.Get().RemoveErrorListener(new FatalErrorMgr.ErrorCallback(this.OnFatalError));
        s_Instance = null;
    }

    private void OnFatalError(FatalErrorMessage message, object userData)
    {
        this.StopCoroutinesAndResetState();
    }

    public void OnLoggedIn()
    {
        this.m_BattleNetReady = true;
    }

    private void OnReset()
    {
        base.StartCoroutine(this.InitNetworkData());
    }

    private void StopCoroutinesAndResetState()
    {
        this.m_BattleNetReady = false;
        this.m_NetworkDataReady = false;
        base.StopAllCoroutines();
    }

    private void Update()
    {
        if (this.m_NetworkDataReady && (UnityEngine.Time.realtimeSinceStartup >= this.m_NextCheckTime))
        {
            this.m_NextCheckTime = UnityEngine.Time.realtimeSinceStartup + 300f;
            string accountCountry = this.m_AccountCountry;
            if (accountCountry != null)
            {
                int num;
                if (<>f__switch$map3C == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                    dictionary.Add("CHN", 0);
                    dictionary.Add("KOR", 1);
                    <>f__switch$map3C = dictionary;
                }
                if (<>f__switch$map3C.TryGetValue(accountCountry, out num))
                {
                    if (num == 0)
                    {
                        this.ChinaRestrictions();
                        return;
                    }
                    if (num == 1)
                    {
                        this.KoreaRestrictions();
                        return;
                    }
                }
            }
            base.enabled = false;
        }
    }

    private void WillReset()
    {
        this.StopCoroutinesAndResetState();
    }

    [CompilerGenerated]
    private sealed class <ChinaRestrictionsUpdate>c__Iterator25B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HealthyGamingMgr <>f__this;
        internal int <hours>__1;
        internal int <minutesPlayed>__0;

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
                    Log.Kyle.Print("ChinaRestrictionsUpdate()", new object[0]);
                    break;

                case 1:
                    break;

                case 2:
                    this.<>f__this.m_NextMessageDisplayTime = -1f;
                    goto Label_016F;

                default:
                    goto Label_025D;
            }
            if (!this.<>f__this.m_Restrictions.loaded)
            {
                BattleNet.GetPlayRestrictions(ref this.<>f__this.m_Restrictions, false);
                this.$current = null;
                this.$PC = 1;
                goto Label_025F;
            }
            this.<>f__this.m_TimePlayed = this.<>f__this.m_Restrictions.CAISplayed;
            this.<>f__this.m_TimeRested = this.<>f__this.m_Restrictions.CAISrested;
            this.<minutesPlayed>__0 = this.<>f__this.m_TimePlayed;
            if (this.<>f__this.m_DebugMode)
            {
                Log.HealthyGaming.Print(string.Format("CAIS Time Played: {0}    Rested: {1}", this.<>f__this.m_TimePlayed.ToString(), this.<>f__this.m_TimeRested.ToString()), new object[0]);
                Log.HealthyGaming.Print(string.Format("CAIS Minutes Played: {0}", this.<minutesPlayed>__0), new object[0]);
            }
            if (this.<>f__this.m_NextMessageDisplayTime == -2f)
            {
                this.$current = new WaitForSeconds(60f);
                this.$PC = 2;
                goto Label_025F;
            }
        Label_016F:
            if ((this.<minutesPlayed>__0 >= this.<>f__this.m_NextMessageDisplayTime) || (this.<>f__this.m_NextMessageDisplayTime <= 0f))
            {
                this.<hours>__1 = this.<minutesPlayed>__0 / 60;
                if (this.<minutesPlayed>__0 >= 180)
                {
                    this.<>f__this.ChinaRestrictions_LockoutFeatures(this.<minutesPlayed>__0);
                }
                if ((this.<minutesPlayed>__0 < 180) && (this.<minutesPlayed>__0 >= 60))
                {
                    this.<>f__this.ChinaRestrictions_LessThan3Hours(this.<minutesPlayed>__0, this.<hours>__1);
                }
                if ((this.<minutesPlayed>__0 >= 180) && (this.<minutesPlayed>__0 <= 300))
                {
                    this.<>f__this.ChinaRestrictions_3to5Hours(this.<minutesPlayed>__0);
                }
                if (this.<minutesPlayed>__0 > 300)
                {
                    this.<>f__this.ChinaRestrictions_MoreThan5Hours(this.<minutesPlayed>__0);
                }
                this.$PC = -1;
            }
        Label_025D:
            return false;
        Label_025F:
            return true;
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
    private sealed class <InitNetworkData>c__Iterator25A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal HealthyGamingMgr <>f__this;
        internal string <message>__0;

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
                    if (Network.ShouldBeConnectedToAurora())
                    {
                        break;
                    }
                    goto Label_03E6;

                case 1:
                    break;

                case 2:
                    goto Label_00FA;

                default:
                    goto Label_03E6;
            }
            while (!this.<>f__this.m_BattleNetReady)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_03E8;
            }
            this.<>f__this.m_AccountCountry = BattleNet.GetAccountCountry();
            if ((this.<>f__this.m_AccountCountry != "CHN") && (this.<>f__this.m_AccountCountry != "KOR"))
            {
                this.<>f__this.enabled = false;
            }
            this.<>f__this.m_Restrictions = new BattleNet.DllLockouts();
            BattleNet.GetPlayRestrictions(ref this.<>f__this.m_Restrictions, true);
        Label_00FA:
            while (!this.<>f__this.m_Restrictions.loaded)
            {
                BattleNet.GetPlayRestrictions(ref this.<>f__this.m_Restrictions, false);
                this.$current = null;
                this.$PC = 2;
                goto Label_03E8;
            }
            this.<>f__this.m_SessionStartTime = this.<>f__this.m_Restrictions.sessionStartTime;
            this.<>f__this.m_TimePlayed = this.<>f__this.m_Restrictions.CAISplayed;
            this.<>f__this.m_TimeRested = this.<>f__this.m_Restrictions.CAISrested;
            if (this.<>f__this.m_DebugMode)
            {
                Log.HealthyGaming.Print("Healthy Gaming Debug ON", new object[0]);
                Log.HealthyGaming.Print("CAIS Active = " + this.<>f__this.m_Restrictions.CAISactive.ToString(), new object[0]);
                Log.HealthyGaming.Print("Accout Country: " + BattleNet.GetAccountCountry(), new object[0]);
                Log.HealthyGaming.Print("Accout Region: " + BattleNet.GetAccountRegion().ToString(), new object[0]);
                Log.HealthyGaming.Print("Current Region: " + BattleNet.GetCurrentRegion().ToString(), new object[0]);
                Log.HealthyGaming.Print("Session StartTime " + this.<>f__this.m_SessionStartTime, new object[0]);
            }
            else
            {
                object[] args = new object[] { this.<>f__this.m_Restrictions.CAISactive.ToString(), BattleNet.GetAccountCountry(), UnityEngine.Time.realtimeSinceStartup, this.<>f__this.m_Restrictions.CAISplayed, this.<>f__this.m_Restrictions.CAISrested, this.<>f__this.m_Restrictions.sessionStartTime };
                Log.HealthyGaming.Print(string.Format("CAIS Active: {0},  Accout Country: {1},  Time: {2},  Played Time: {3},  Rested Time: {4}, Session Start Time: {5}", args), new object[0]);
            }
            if (!this.<>f__this.m_Restrictions.CAISactive && (this.<>f__this.m_AccountCountry == "CHN"))
            {
                this.<>f__this.enabled = false;
            }
            else
            {
                if (this.<>f__this.m_DebugMode)
                {
                    Log.HealthyGaming.Print("Healthy Gaming Active!", new object[0]);
                }
                if (this.<>f__this.m_AccountCountry == "KOR")
                {
                    this.<>f__this.m_NextMessageDisplayTime = UnityEngine.Time.realtimeSinceStartup + 3600f;
                }
                if (this.<>f__this.m_AccountCountry == "CHN")
                {
                    this.<message>__0 = GameStrings.Get("GLOBAL_HEALTHY_GAMING_CHINA_CAIS_ACTIVE");
                    SocialToastMgr.Get().AddToast(this.<message>__0, SocialToastMgr.TOAST_TYPE.DEFAULT, (float) 60f);
                    this.<>f__this.m_NextMessageDisplayTime = -2f;
                }
                this.<>f__this.m_NetworkDataReady = true;
                goto Label_03E6;
                this.$PC = -1;
            }
        Label_03E6:
            return false;
        Label_03E8:
            return true;
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
}

