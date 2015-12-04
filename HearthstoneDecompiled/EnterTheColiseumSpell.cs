using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class EnterTheColiseumSpell : Spell
{
    [CompilerGenerated]
    private static Spell.StateFinishedCallback <>f__am$cache15;
    [CompilerGenerated]
    private static Spell.StateFinishedCallback <>f__am$cache16;
    public float m_CameraShakeMagnitude = 0.075f;
    public float m_DestroyMinionDelay = 0.5f;
    public Spell m_DustSpellPrefab;
    private bool m_effectsPlaying;
    public Spell m_ImpactSpellPrefab;
    public iTween.EaseType m_liftEaseType = iTween.EaseType.easeInQuart;
    public float m_LiftOffset = 0.1f;
    public float m_LiftTime = 0.5f;
    public iTween.EaseType m_lightFadeEaseType = iTween.EaseType.easeOutCubic;
    public float m_LightingFadeTime = 0.5f;
    public float m_LowerDelay = 1.5f;
    public iTween.EaseType m_lowerEaseType = iTween.EaseType.easeOutCubic;
    public float m_LowerOffset = 0.05f;
    public float m_LowerTime = 0.7f;
    private int m_numSurvivorSpellsPlaying;
    public string m_RaiseSoundName;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_SpellStartSoundPrefab;
    private List<Card> m_survivorCards;
    public float m_survivorLiftHeight = 2f;
    public bool m_survivorsMeetInMiddle = true;
    public Spell m_survivorSpellPrefab;

    private List<Card> FindSurvivors()
    {
        List<Card> list = new List<Card>();
        foreach (ZonePlay play in ZoneMgr.Get().FindZonesOfType<ZonePlay>())
        {
            List<Card> cards = play.GetCards();
            <FindSurvivors>c__AnonStorey34E storeye = new <FindSurvivors>c__AnonStorey34E();
            using (List<Card>.Enumerator enumerator2 = cards.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    storeye.playCard = enumerator2.Current;
                    if (base.m_targets.Find(new Predicate<GameObject>(storeye.<>m__1B4)) == null)
                    {
                        list.Add(storeye.playCard);
                    }
                }
            }
        }
        return list;
    }

    protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
    {
        Network.PowerHistory power = task.GetPower();
        if (power.Type != Network.PowerType.TAG_CHANGE)
        {
            return null;
        }
        Network.HistTagChange change = power as Network.HistTagChange;
        if (change.Tag != 360)
        {
            return null;
        }
        if (change.Value != 1)
        {
            return null;
        }
        Entity entity = GameState.Get().GetEntity(change.Entity);
        if (entity == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {1} but there is no entity with that id", this, change.Entity));
            return null;
        }
        return entity.GetCard();
    }

    private void LiftCard(Card card)
    {
        GameObject gameObject = card.gameObject;
        Vector3 position = gameObject.transform.position;
        Vector3 vector2 = card.GetZone().gameObject.transform.position;
        object[] args = new object[] { "time", this.m_LiftTime, "position", new Vector3(!this.m_survivorsMeetInMiddle ? position.x : vector2.x, position.y + this.m_survivorLiftHeight, position.z), "onstart", newVal => SoundManager.Get().LoadAndPlay(this.m_RaiseSoundName), "easetype", this.m_liftEaseType };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(gameObject, hashtable);
    }

    private void LowerCard(GameObject target, Vector3 finalPosition)
    {
        object[] args = new object[] { "time", this.m_LowerTime, "position", finalPosition, "easetype", this.m_lowerEaseType };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(target, hashtable);
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.OnAction(prevStateType);
        this.m_survivorCards = this.FindSurvivors();
        base.StartCoroutine(this.PerformActions());
    }

    [DebuggerHidden]
    private IEnumerator PerformActions()
    {
        return new <PerformActions>c__Iterator210 { <>f__this = this };
    }

    private void PlayDustCloudSpell()
    {
        if (this.m_DustSpellPrefab != null)
        {
            Spell spell = UnityEngine.Object.Instantiate<Spell>(this.m_DustSpellPrefab);
            if (<>f__am$cache16 == null)
            {
                <>f__am$cache16 = delegate (Spell spell, SpellStateType prevStateType, object userData) {
                    if (spell.GetActiveState() == SpellStateType.NONE)
                    {
                        UnityEngine.Object.Destroy(spell.gameObject);
                    }
                };
            }
            spell.AddStateFinishedCallback(<>f__am$cache16);
            spell.Activate();
        }
    }

    private void PlaySurvivorSpell(Card card)
    {
        if (this.m_survivorSpellPrefab != null)
        {
            this.m_numSurvivorSpellsPlaying++;
            Spell spell = UnityEngine.Object.Instantiate<Spell>(this.m_survivorSpellPrefab);
            spell.transform.parent = card.GetActor().transform;
            spell.AddFinishedCallback((spell, spellUserData) => this.m_numSurvivorSpellsPlaying--);
            if (<>f__am$cache15 == null)
            {
                <>f__am$cache15 = delegate (Spell spell, SpellStateType prevStateType, object userData) {
                    if (spell.GetActiveState() == SpellStateType.NONE)
                    {
                        UnityEngine.Object.Destroy(spell.gameObject);
                    }
                };
            }
            spell.AddStateFinishedCallback(<>f__am$cache15);
            spell.SetSource(card.gameObject);
            spell.Activate();
        }
    }

    [CompilerGenerated]
    private sealed class <FindSurvivors>c__AnonStorey34E
    {
        internal Card playCard;

        internal bool <>m__1B4(GameObject testObject)
        {
            return (this.playCard == testObject.GetComponent<Card>());
        }
    }

    [CompilerGenerated]
    private sealed class <PerformActions>c__Iterator210 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Card>.Enumerator <$s_1204>__0;
        internal List<Card>.Enumerator <$s_1205>__3;
        internal List<Card>.Enumerator <$s_1206>__5;
        internal List<Card>.Enumerator <$s_1207>__9;
        internal List<Card>.Enumerator <$s_1208>__12;
        internal List<ZonePlay>.Enumerator <$s_1209>__15;
        internal EnterTheColiseumSpell <>f__this;
        internal Card <card>__1;
        internal Card <card>__10;
        internal Card <card>__13;
        internal Card <card>__4;
        internal Card <card>__6;
        internal Zone <cardZone>__7;
        internal ZonePlay <playZone>__16;
        internal List<ZonePlay> <playZones>__14;
        internal string <showSoundName>__2;
        internal Spell <spellInstance>__11;
        internal ZonePlay <zone>__8;

        internal void <>m__1B8(Spell spell, SpellStateType prevStateType, object userData)
        {
            this.<>f__this.m_effectsPlaying = false;
            if (spell.GetActiveState() == SpellStateType.NONE)
            {
                UnityEngine.Object.Destroy(spell.gameObject);
            }
        }

        [DebuggerHidden]
        public void Dispose()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 1:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_1204>__0.Dispose();
                    }
                    break;

                case 6:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_1206>__5.Dispose();
                    }
                    break;

                case 7:
                    try
                    {
                    }
                    finally
                    {
                        this.<$s_1207>__9.Dispose();
                    }
                    break;
            }
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            bool flag = false;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_effectsPlaying = true;
                    this.<$s_1204>__0 = this.<>f__this.m_survivorCards.GetEnumerator();
                    num = 0xfffffffd;
                    break;

                case 1:
                    break;

                case 2:
                    this.<$s_1205>__3 = this.<>f__this.m_survivorCards.GetEnumerator();
                    try
                    {
                        while (this.<$s_1205>__3.MoveNext())
                        {
                            this.<card>__4 = this.<$s_1205>__3.Current;
                            this.<>f__this.PlaySurvivorSpell(this.<card>__4);
                        }
                    }
                    finally
                    {
                        this.<$s_1205>__3.Dispose();
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_DestroyMinionDelay);
                    this.$PC = 3;
                    goto Label_05F6;

                case 3:
                    this.<>f__this.OnSpellFinished();
                    CameraShakeMgr.Shake(Camera.main, new Vector3(this.<>f__this.m_CameraShakeMagnitude, this.<>f__this.m_CameraShakeMagnitude, this.<>f__this.m_CameraShakeMagnitude), 0.75f);
                    this.$current = new WaitForSeconds(this.<>f__this.m_LowerDelay);
                    this.$PC = 4;
                    goto Label_05F6;

                case 4:
                case 5:
                    if (this.<>f__this.m_numSurvivorSpellsPlaying > 0)
                    {
                        this.$current = null;
                        this.$PC = 5;
                        goto Label_05F6;
                    }
                    this.<$s_1206>__5 = this.<>f__this.m_survivorCards.GetEnumerator();
                    num = 0xfffffffd;
                    goto Label_02BC;

                case 6:
                    goto Label_02BC;

                case 7:
                    goto Label_03D5;

                case 8:
                    this.<$s_1208>__12 = this.<>f__this.m_survivorCards.GetEnumerator();
                    try
                    {
                        while (this.<$s_1208>__12.MoveNext())
                        {
                            this.<card>__13 = this.<$s_1208>__12.Current;
                            this.<card>__13.SetDoNotSort(false);
                            this.<card>__13.GetActor().SetLit();
                        }
                    }
                    finally
                    {
                        this.<$s_1208>__12.Dispose();
                    }
                    this.<playZones>__14 = ZoneMgr.Get().FindZonesOfType<ZonePlay>();
                    this.<$s_1209>__15 = this.<playZones>__14.GetEnumerator();
                    try
                    {
                        while (this.<$s_1209>__15.MoveNext())
                        {
                            this.<playZone>__16 = this.<$s_1209>__15.Current;
                            this.<playZone>__16.UpdateLayout();
                        }
                    }
                    finally
                    {
                        this.<$s_1209>__15.Dispose();
                    }
                    goto Label_05D2;

                case 9:
                    goto Label_05D2;

                default:
                    goto Label_05F4;
            }
            try
            {
                while (this.<$s_1204>__0.MoveNext())
                {
                    this.<card>__1 = this.<$s_1204>__0.Current;
                    this.<card>__1.SetDoNotSort(true);
                    this.<card>__1.GetActor().SetUnlit();
                    this.<>f__this.LiftCard(this.<card>__1);
                    this.$current = new WaitForSeconds(this.<>f__this.m_LiftOffset);
                    this.$PC = 1;
                    flag = true;
                    goto Label_05F6;
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_1204>__0.Dispose();
            }
            FullScreenFXMgr.Get().Vignette(1f, this.<>f__this.m_LightingFadeTime, this.<>f__this.m_lightFadeEaseType, null);
            if (!string.IsNullOrEmpty(this.<>f__this.m_SpellStartSoundPrefab))
            {
                this.<showSoundName>__2 = FileUtils.GameAssetPathToName(this.<>f__this.m_SpellStartSoundPrefab);
                SoundManager.Get().LoadAndPlay(this.<showSoundName>__2);
            }
            this.<>f__this.PlayDustCloudSpell();
            this.$current = new WaitForSeconds(this.<>f__this.m_LiftTime);
            this.$PC = 2;
            goto Label_05F6;
        Label_02BC:
            try
            {
                while (this.<$s_1206>__5.MoveNext())
                {
                    this.<card>__6 = this.<$s_1206>__5.Current;
                    this.<cardZone>__7 = this.<card>__6.GetZone();
                    if (this.<cardZone>__7 is ZonePlay)
                    {
                        this.<zone>__8 = (ZonePlay) this.<cardZone>__7;
                        this.<>f__this.LowerCard(this.<card>__6.gameObject, this.<zone>__8.GetCardPosition(this.<card>__6));
                        this.$current = new WaitForSeconds(this.<>f__this.m_LowerOffset);
                        this.$PC = 6;
                        flag = true;
                        goto Label_05F6;
                    }
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_1206>__5.Dispose();
            }
            FullScreenFXMgr.Get().StopVignette(this.<>f__this.m_LightingFadeTime, this.<>f__this.m_lightFadeEaseType, null);
            if (this.<>f__this.m_ImpactSpellPrefab == null)
            {
                goto Label_04C1;
            }
            this.<$s_1207>__9 = this.<>f__this.m_survivorCards.GetEnumerator();
            num = 0xfffffffd;
        Label_03D5:
            try
            {
                while (this.<$s_1207>__9.MoveNext())
                {
                    this.<card>__10 = this.<$s_1207>__9.Current;
                    this.<spellInstance>__11 = UnityEngine.Object.Instantiate<Spell>(this.<>f__this.m_ImpactSpellPrefab);
                    this.<spellInstance>__11.transform.parent = this.<card>__10.gameObject.transform;
                    this.<spellInstance>__11.transform.localPosition = new Vector3(0f, 0f, 0f);
                    this.<spellInstance>__11.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.<>m__1B8));
                    this.<spellInstance>__11.Activate();
                    this.$current = new WaitForSeconds(this.<>f__this.m_LowerOffset);
                    this.$PC = 7;
                    flag = true;
                    goto Label_05F6;
                }
            }
            finally
            {
                if (!flag)
                {
                }
                this.<$s_1207>__9.Dispose();
            }
        Label_04C1:
            this.$current = new WaitForSeconds(this.<>f__this.m_LowerTime);
            this.$PC = 8;
            goto Label_05F6;
        Label_05D2:
            if (this.<>f__this.m_effectsPlaying)
            {
                this.$current = null;
                this.$PC = 9;
                goto Label_05F6;
            }
            this.<>f__this.OnStateFinished();
            this.$PC = -1;
        Label_05F4:
            return false;
        Label_05F6:
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

