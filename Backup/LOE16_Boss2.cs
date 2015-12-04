using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class LOE16_Boss2 : LOE_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map19;
    private bool m_artifactLinePlayed;
    private bool m_firstExplorerHelp;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator150 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator14F();
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOEA16_1_RESPONSE_03",
            m_stringTag = "VO_LOEA16_1_RESPONSE_03"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void OnTagChanged(TagDelta change)
    {
        base.OnTagChanged(change);
        Gameplay.Get().StartCoroutine(this.OnTagChangedHandler(change));
    }

    [DebuggerHidden]
    private IEnumerator OnTagChangedHandler(TagDelta change)
    {
        return new <OnTagChangedHandler>c__Iterator14E { change = change, <$>change = change, <>f__this = this };
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_16_TURN_2");
        base.PreloadSound("VO_ELISE_LOE16_ALT_1_FIRST_HALF_02");
        base.PreloadSound("VO_ELISE_LOE16_ALT_1_SECOND_HALF_03");
        base.PreloadSound("VO_LOE_16_TURN_2_2");
        base.PreloadSound("VO_LOE_16_TURN_2_3");
        base.PreloadSound("VO_LOE_16_TURN_3");
        base.PreloadSound("VO_LOE_16_TURN_3_2");
        base.PreloadSound("VO_LOE_16_TURN_4");
        base.PreloadSound("VO_LOE_16_TURN_5");
        base.PreloadSound("VO_LOE_16_TURN_5_2");
        base.PreloadSound("VO_LOE_16_TURN_6");
        base.PreloadSound("VO_LOE_16_FIRST_ITEM");
        base.PreloadSound("VO_LOE_092_Attack_02");
        base.PreloadSound("VO_LOE_16_GOBLET");
        base.PreloadSound("VO_LOE_16_CROWN");
        base.PreloadSound("VO_LOE_16_EYE");
        base.PreloadSound("VO_LOE_16_PIPE");
        base.PreloadSound("VO_LOE_16_TEAR");
        base.PreloadSound("VO_LOE_16_SHARD");
        base.PreloadSound("VO_LOE_16_LOCKET");
        base.PreloadSound("VO_LOE_16_SPLINTER");
        base.PreloadSound("VO_LOE_16_VIAL");
        base.PreloadSound("VO_LOE_16_GREAVE");
        base.PreloadSound("VO_LOE_16_BOOM_BOT");
        base.PreloadSound("VO_LOEA16_1_CARD_04");
        base.PreloadSound("VO_LOEA16_1_RESPONSE_03");
        base.PreloadSound("VO_LOEA16_1_TURN1_02");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator151 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    public override void StartGameplaySoundtracks()
    {
        MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_LOE_Wing4Mission4);
    }

    public override string UpdateCardText(Card card, Actor bigCardActor, string text)
    {
        Player opposingSidePlayer = GameState.Get().GetOpposingSidePlayer();
        if (opposingSidePlayer.GetHeroPowerCard() != card)
        {
            return text;
        }
        int tag = opposingSidePlayer.GetHeroPower().GetTag(GAME_TAG.ELECTRIC_CHARGE_LEVEL);
        if (GameState.Get().GetGameEntity().GetTag(GAME_TAG.TURN) < 2)
        {
            tag = 3;
        }
        string key = string.Empty;
        switch (tag)
        {
            case 0:
                key = "LOEA16_2_STAFF_TEXT_CHARGE_EXPLODED";
                break;

            case 1:
                key = "LOEA16_2_STAFF_TEXT_CHARGE_1";
                break;

            case 2:
                key = "LOEA16_2_STAFF_TEXT_CHARGE_2";
                break;

            case 3:
                key = "LOEA16_2_STAFF_TEXT_CHARGE_0";
                break;
        }
        return GameStrings.Get(key);
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator150 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal LOE16_Boss2 <>f__this;
        internal Actor <enemyActor>__0;
        internal int missionEvent;

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
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_00AE;

                case 3:
                    this.<>f__this.m_firstExplorerHelp = true;
                    goto Label_048B;

                default:
                    goto Label_048B;
            }
            if (GameState.Get().IsBusy())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_048D;
            }
        Label_00AE:
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_048D;
            }
            if ((this.missionEvent < 10) || this.<>f__this.m_firstExplorerHelp)
            {
                switch (this.missionEvent)
                {
                    case 0x6331:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_16_EYE", 3f, 1f, false));
                        this.$PC = 7;
                        goto Label_048D;

                    case 0x6332:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_16_PIPE", 3f, 1f, false));
                        this.$PC = 8;
                        goto Label_048D;

                    case 0x6333:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_16_TEAR", 3f, 1f, false));
                        this.$PC = 9;
                        goto Label_048D;

                    case 0x6334:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_16_SHARD", 3f, 1f, false));
                        this.$PC = 10;
                        goto Label_048D;

                    case 0x6335:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOE_16_SPLINTER", 3f, 1f, false));
                        this.$PC = 11;
                        goto Label_048D;

                    case 0x6336:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_16_VIAL", 3f, 1f, false));
                        this.$PC = 12;
                        goto Label_048D;

                    case 0x6337:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOE_16_GREAVE", 3f, 1f, false));
                        this.$PC = 13;
                        goto Label_048D;

                    case 0x6338:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_16_GOBLET", 3f, 1f, false));
                        this.$PC = 4;
                        goto Label_048D;

                    case 0x6339:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Finley_BigQuote", "VO_LOE_16_CROWN", 3f, 1f, false));
                        this.$PC = 5;
                        goto Label_048D;

                    case 0x633a:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Brann_BigQuote", "VO_LOE_16_LOCKET", 3f, 1f, false));
                        this.$PC = 6;
                        goto Label_048D;

                    case 2:
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_LOE_092_Attack_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 3f, 1f, true, false));
                        break;

                    case 0x8bb:
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Reno_BigQuote", "VO_LOE_16_BOOM_BOT", 3f, 1f, false));
                        this.$PC = 14;
                        goto Label_048D;
                }
            }
            else
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Elise_BigQuote", "VO_LOE_16_FIRST_ITEM", 3f, 1f, true, false));
                this.$PC = 3;
                goto Label_048D;
                this.$PC = -1;
            }
        Label_048B:
            return false;
        Label_048D:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator14F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            this.$PC = -1;
            if (this.$PC == 0)
            {
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
    private sealed class <OnTagChangedHandler>c__Iterator14E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TagDelta <$>change;
        internal LOE16_Boss2 <>f__this;
        internal int <count>__2;
        internal Actor <enemyActor>__1;
        internal int <turn>__0;
        internal TagDelta change;

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
                    if (this.change.tag == 3)
                    {
                        this.<turn>__0 = this.change.newValue;
                        this.<enemyActor>__1 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                        this.<count>__2 = this.<turn>__0;
                        if (this.<count>__2 > 14)
                        {
                            this.<count>__2 = (this.<count>__2 - 14) % 6;
                            if (this.<count>__2 == 0)
                            {
                                this.<count>__2 = 6;
                            }
                            this.<count>__2 = 8 + this.<count>__2;
                        }
                        break;
                    }
                    goto Label_04E3;

                case 1:
                    break;

                case 2:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Elise_BigQuote", "VO_ELISE_LOE16_ALT_1_FIRST_HALF_02", 3f, 1f, true, false));
                    this.$PC = 3;
                    goto Label_04E5;

                case 3:
                    if (GameState.Get().GetOpposingSidePlayer().GetTag(GAME_TAG.ELECTRIC_CHARGE_LEVEL) <= 0)
                    {
                        GameState.Get().GetOpposingSidePlayer().GetHeroPowerCard().ActivateActorSpell(SpellType.ELECTRIC_CHARGE_LEVEL_SMALL);
                    }
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Elise_BigQuote", "VO_ELISE_LOE16_ALT_1_SECOND_HALF_03", 3f, 1f, true, false));
                    this.$PC = 4;
                    goto Label_04E5;

                case 4:
                    GameState.Get().SetBusy(false);
                    goto Label_04E3;

                case 6:
                    GameState.Get().SetBusy(false);
                    goto Label_04E3;

                case 8:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Elise_BigQuote", "VO_LOE_16_TURN_3_2", 3f, 1f, true, false));
                    this.$PC = 9;
                    goto Label_04E5;

                case 11:
                    GameState.Get().SetBusy(false);
                    goto Label_04E3;

                default:
                    goto Label_04E3;
            }
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_04E5;
            }
            switch (this.<count>__2)
            {
                case 1:
                    GameState.Get().SetBusy(true);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA16_1_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 3f, 1f, true, false));
                    this.$PC = 2;
                    goto Label_04E5;

                case 2:
                case 3:
                case 4:
                case 9:
                case 10:
                    break;

                case 5:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Elise_BigQuote", "VO_LOE_16_TURN_2", 3f, 1f, true, false));
                    this.$PC = 5;
                    goto Label_04E5;

                case 6:
                    GameState.Get().SetBusy(true);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_LOE_16_TURN_2_2", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 3f, 1f, true, false));
                    this.$PC = 6;
                    goto Label_04E5;

                case 7:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Elise_BigQuote", "VO_LOE_16_TURN_2_3", 3f, 1f, true, false));
                    this.$PC = 7;
                    goto Label_04E5;

                case 8:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_16_TURN_3", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 3f, 1f, true, false));
                    this.$PC = 8;
                    goto Label_04E5;

                case 11:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Elise_BigQuote", "VO_LOE_16_TURN_4", 3f, 1f, true, false));
                    this.$PC = 10;
                    goto Label_04E5;

                case 12:
                    GameState.Get().SetBusy(true);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOE_16_TURN_5", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 3f, 1f, true, false));
                    this.$PC = 11;
                    goto Label_04E5;

                case 13:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_16_TURN_5_2", 3f, 1f, false));
                    this.$PC = 12;
                    goto Label_04E5;

                case 14:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_LOE_16_TURN_6", 3f, 1f, false));
                    this.$PC = 13;
                    goto Label_04E5;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
        Label_04E3:
            return false;
        Label_04E5:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator151 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal LOE16_Boss2 <>f__this;
        internal Actor <enemyActor>__0;
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
                        goto Label_015B;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_0159;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_015B;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (LOE16_Boss2.<>f__switch$map19 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                    dictionary.Add("LOEA16_3", 0);
                    dictionary.Add("LOEA16_4", 0);
                    dictionary.Add("LOEA16_5", 0);
                    LOE16_Boss2.<>f__switch$map19 = dictionary;
                }
                if (LOE16_Boss2.<>f__switch$map19.TryGetValue(cardId, out num2) && (num2 == 0))
                {
                    if (this.<>f__this.m_artifactLinePlayed)
                    {
                        goto Label_0159;
                    }
                    this.<>f__this.m_artifactLinePlayed = true;
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA16_1_CARD_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 3f, 1f, true, false));
                }
            }
            this.$PC = -1;
        Label_0159:
            return false;
        Label_015B:
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

