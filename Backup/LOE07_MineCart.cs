using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE07_MineCart : LOE_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map13;
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map14;
    private MineCartRushArt m_mineCartArt;
    private HashSet<string> m_playedLines = new HashSet<string>();
    private Notification m_turnCounter;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator137 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator134 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    private void InitMineCartArt()
    {
        this.m_mineCartArt = AssetLoader.Get().LoadGameObject("MineCartRushArt", true, false).GetComponent<MineCartRushArt>();
    }

    private void InitTurnCounter(int cost)
    {
        this.m_turnCounter = AssetLoader.Get().LoadGameObject("LOE_Turn_Timer", true, false).GetComponent<Notification>();
        PlayMakerFSM component = this.m_turnCounter.GetComponent<PlayMakerFSM>();
        component.FsmVariables.GetFsmBool("RunningMan").Value = false;
        component.FsmVariables.GetFsmBool("MineCart").Value = true;
        component.SendEvent("Birth");
        Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
        this.m_turnCounter.transform.parent = actor.gameObject.transform;
        this.m_turnCounter.transform.localPosition = new Vector3(-1.4f, 0.187f, -0.11f);
        this.m_turnCounter.transform.localScale = (Vector3) (Vector3.one * 0.52f);
        this.UpdateTurnCounterText(cost);
    }

    private void InitVisuals()
    {
        int cost = base.GetCost();
        this.InitMineCartArt();
        this.InitTurnCounter(cost);
    }

    public override void NotifyOfMulliganEnded()
    {
        base.NotifyOfMulliganEnded();
        GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor().GetHealthObject().Hide();
    }

    public override void NotifyOfMulliganInitialized()
    {
        base.NotifyOfMulliganInitialized();
        this.InitVisuals();
    }

    public override void OnTagChanged(TagDelta change)
    {
        base.OnTagChanged(change);
        if ((change.tag == 0x30) && (change.newValue != change.oldValue))
        {
            this.UpdateVisuals(change.newValue);
        }
    }

    protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
    {
        if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
        {
            Actor actor = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            SoundManager.Get().LoadAndPlay("Mine_response", actor.gameObject);
        }
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_07_START");
        base.PreloadSound("VO_LOE_07_WIN");
        base.PreloadSound("VO_LOEA07_MINE_ARCHAEDAS");
        base.PreloadSound("VO_LOEA07_MINE_DETONATE");
        base.PreloadSound("VO_LOEA07_MINE_RAMMING");
        base.PreloadSound("VO_LOEA07_MINE_PARROT");
        base.PreloadSound("VO_LOEA07_MINE_BOOMZOOKA");
        base.PreloadSound("VO_LOEA07_MINE_DYNAMITE");
        base.PreloadSound("VO_LOEA07_MINE_BOOM");
        base.PreloadSound("VO_LOEA07_MINE_BARREL_FORWARD");
        base.PreloadSound("VO_LOEA07_MINE_HUNKER_DOWN");
        base.PreloadSound("VO_LOEA07_MINE_SPIKED_DECOY");
        base.PreloadSound("VO_LOEA07_MINE_REPAIRS");
        base.PreloadSound("VO_BRANN_WIN_07_ALT_07");
        base.PreloadSound("VO_LOE_07_RESPONSE");
        base.PreloadSound("Mine_response");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToFriendlyPlayedCardWithTiming>c__Iterator136 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator135 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    public override bool ShouldDoAlternateMulliganIntro()
    {
        return true;
    }

    public override bool ShouldHandleCoin()
    {
        return false;
    }

    public override void StartGameplaySoundtracks()
    {
        MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_LOE_Minecart);
    }

    private void UpdateMineCartArt()
    {
        Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
        this.m_mineCartArt.DoPortraitSwap(actor);
    }

    private void UpdateTurnCounter(int cost)
    {
        this.m_turnCounter.GetComponent<PlayMakerFSM>().SendEvent("Action");
        this.UpdateTurnCounterText(cost);
    }

    private void UpdateTurnCounterText(int cost)
    {
        GameStrings.PluralNumber[] numberArray1 = new GameStrings.PluralNumber[1];
        GameStrings.PluralNumber number = new GameStrings.PluralNumber {
            m_number = cost
        };
        numberArray1[0] = number;
        GameStrings.PluralNumber[] pluralNumbers = numberArray1;
        string headlineString = GameStrings.FormatPlurals("MISSION_DEFAULTCOUNTERNAME", pluralNumbers, new object[0]);
        this.m_turnCounter.ChangeDialogText(headlineString, cost.ToString(), string.Empty, string.Empty);
    }

    private void UpdateVisuals(int cost)
    {
        this.UpdateMineCartArt();
        this.UpdateTurnCounter(cost);
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator137 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE07_MineCart <>f__this;
        internal TAG_PLAYSTATE gameResult;

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
                    if ((this.gameResult != TAG_PLAYSTATE.WON) || GameMgr.Get().IsClassChallengeMission())
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(5f);
                    this.$PC = 1;
                    goto Label_009D;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Brann_Quote", "VO_BRANN_WIN_07_ALT_07", 0f, false, false));
                    this.$PC = 2;
                    goto Label_009D;

                case 2:
                    break;

                default:
                    goto Label_009B;
            }
            this.$PC = -1;
        Label_009B:
            return false;
        Label_009D:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator134 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE07_MineCart <>f__this;
        internal int turn;

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
                    switch (this.turn)
                    {
                        case 1:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOE_07_START", 3f, 1f, false));
                            this.$PC = 1;
                            goto Label_00CB;

                        case 11:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOE_07_WIN", 3f, 1f, false));
                            this.$PC = 2;
                            goto Label_00CB;
                    }
                    break;

                case 1:
                case 2:
                    break;

                default:
                    goto Label_00C9;
            }
            this.$PC = -1;
        Label_00C9:
            return false;
        Label_00CB:
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
    private sealed class <RespondToFriendlyPlayedCardWithTiming>c__Iterator136 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal LOE07_MineCart <>f__this;
        internal string <cardID>__0;
        internal Entity entity;

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
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0533;
                    }
                    break;

                case 2:
                    break;

                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                    goto Label_052A;

                default:
                    goto Label_0531;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0533;
            }
            if (this.<>f__this.m_playedLines.Contains(this.entity.GetCardId()))
            {
                goto Label_0531;
            }
            this.<cardID>__0 = this.entity.GetCardId();
            string key = this.<cardID>__0;
            if (key != null)
            {
                int num2;
                if (LOE07_MineCart.<>f__switch$map14 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(10);
                    dictionary.Add("LOEA07_23", 0);
                    dictionary.Add("LOEA07_19", 1);
                    dictionary.Add("LOEA07_25", 2);
                    dictionary.Add("LOEA07_32", 3);
                    dictionary.Add("LOEA07_18", 4);
                    dictionary.Add("LOEA07_20", 5);
                    dictionary.Add("LOEA07_21", 6);
                    dictionary.Add("LOEA07_22", 7);
                    dictionary.Add("LOEA07_24", 8);
                    dictionary.Add("LOEA07_28", 9);
                    LOE07_MineCart.<>f__switch$map14 = dictionary;
                }
                if (LOE07_MineCart.<>f__switch$map14.TryGetValue(key, out num2))
                {
                    switch (num2)
                    {
                        case 0:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_DETONATE", 3f, 1f, false));
                            this.$PC = 3;
                            goto Label_0533;

                        case 1:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_RAMMING", 3f, 1f, false));
                            this.$PC = 4;
                            goto Label_0533;

                        case 2:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_PARROT", 3f, 1f, false));
                            this.$PC = 5;
                            goto Label_0533;

                        case 3:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_BOOMZOOKA", 3f, 1f, false));
                            this.$PC = 6;
                            goto Label_0533;

                        case 4:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_DYNAMITE", 3f, 1f, false));
                            this.$PC = 7;
                            goto Label_0533;

                        case 5:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_BOOM", 3f, 1f, false));
                            this.$PC = 8;
                            goto Label_0533;

                        case 6:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_BARREL_FORWARD", 3f, 1f, false));
                            this.$PC = 9;
                            goto Label_0533;

                        case 7:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_HUNKER_DOWN", 3f, 1f, false));
                            this.$PC = 10;
                            goto Label_0533;

                        case 8:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_SPIKED_DECOY", 3f, 1f, false));
                            this.$PC = 11;
                            goto Label_0533;

                        case 9:
                            this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_REPAIRS", 3f, 1f, false));
                            this.$PC = 12;
                            goto Label_0533;
                    }
                }
            }
        Label_052A:
            this.$PC = -1;
        Label_0531:
            return false;
        Label_0533:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator135 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal LOE07_MineCart <>f__this;
        internal string <cardID>__0;
        internal Entity entity;

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
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_015D;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    goto Label_0154;

                default:
                    goto Label_015B;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_015D;
            }
            if (this.<>f__this.m_playedLines.Contains(this.entity.GetCardId()))
            {
                goto Label_015B;
            }
            this.<cardID>__0 = this.entity.GetCardId();
            string key = this.<cardID>__0;
            if (key != null)
            {
                int num2;
                if (LOE07_MineCart.<>f__switch$map13 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(1);
                    dictionary.Add("LOEA07_16", 0);
                    LOE07_MineCart.<>f__switch$map13 = dictionary;
                }
                if (LOE07_MineCart.<>f__switch$map13.TryGetValue(key, out num2) && (num2 == 0))
                {
                    this.<>f__this.m_playedLines.Add(this.<cardID>__0);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOEA07_MINE_ARCHAEDAS", 3f, 1f, false));
                    this.$PC = 3;
                    goto Label_015D;
                }
            }
        Label_0154:
            this.$PC = -1;
        Label_015B:
            return false;
        Label_015D:
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

