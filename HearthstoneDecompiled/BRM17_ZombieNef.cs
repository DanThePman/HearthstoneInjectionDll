using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class BRM17_ZombieNef : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map11;
    private bool m_cardLinePlayed;
    private bool m_heroPowerLinePlayed;
    private bool m_inOnyxiaState;
    private Actor m_nefActor;

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator127 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator126 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
    {
        Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
        switch (emoteType)
        {
            case EmoteType.GREETINGS:
            case EmoteType.WELL_PLAYED:
            case EmoteType.OOPS:
            case EmoteType.THREATEN:
            case EmoteType.THANKS:
            case EmoteType.SORRY:
            {
                string cardId = GameState.Get().GetOpposingSidePlayer().GetHero().GetCardId();
                if (!(cardId == "BRMA17_2") && !(cardId == "BRMA17_2H"))
                {
                    switch (cardId)
                    {
                        case "BRMA17_3":
                        case "BRMA17_3H":
                            Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("OnyxiaBoss_EmoteResponse_1", "OnyxiaBoss_EmoteResponse_1", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                            break;
                    }
                    break;
                }
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_BRMA17_1_RESPONSE_85", "VO_BRMA17_1_RESPONSE_85", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                break;
            }
        }
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA17_1_DEATHWING_88");
        base.PreloadSound("VO_BRMA17_1_HERO_POWER_87");
        base.PreloadSound("VO_BRMA17_1_CARD_86");
        base.PreloadSound("VO_BRMA17_1_RESPONSE_85");
        base.PreloadSound("VO_BRMA17_1_TURN1_79");
        base.PreloadSound("VO_BRMA17_1_RESURRECT1_82");
        base.PreloadSound("VO_BRMA17_1_RESURRECT3_84");
        base.PreloadSound("VO_BRMA17_1_NEF_AIR1_89");
        base.PreloadSound("VO_BRMA17_1_NEF_AIR2_90");
        base.PreloadSound("VO_BRMA17_1_NEF_AIR3_91");
        base.PreloadSound("VO_BRMA17_1_NEF_AIR4_92");
        base.PreloadSound("VO_BRMA17_1_NEF_AIR5_93");
        base.PreloadSound("VO_BRMA17_1_NEF_AIR6_94");
        base.PreloadSound("VO_BRMA17_1_NEF_AIR7_95");
        base.PreloadSound("VO_BRMA17_1_NEF_AIR8_96");
        base.PreloadSound("VO_BRMA17_1_TRANSFORM1_80");
        base.PreloadSound("VO_BRMA17_1_TRANSFORM2_81");
        base.PreloadSound("OnyxiaBoss_Start_1");
        base.PreloadSound("OnyxiaBoss_Death_1");
        base.PreloadSound("OnyxiaBoss_EmoteResponse_1");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator125 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator127 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal BRM17_ZombieNef <>f__this;
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
                    switch (this.missionEvent)
                    {
                        case 1:
                            this.<>f__this.m_inOnyxiaState = true;
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_RESURRECT1_82", "VO_BRMA17_1_RESURRECT1_82", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            this.$PC = 1;
                            goto Label_0471;

                        case 3:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_DEATHWING_88", "VO_BRMA17_1_DEATHWING_88", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            break;

                        case 4:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_NEF_AIR1_89", "VO_BRMA17_1_NEF_AIR1_89", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            break;

                        case 5:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_NEF_AIR2_90", "VO_BRMA17_1_NEF_AIR2_90", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            break;

                        case 6:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_NEF_AIR3_91", "VO_BRMA17_1_NEF_AIR3_91", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            break;

                        case 7:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_NEF_AIR4_92", "VO_BRMA17_1_NEF_AIR4_92", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            break;

                        case 8:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_NEF_AIR5_93", "VO_BRMA17_1_NEF_AIR5_93", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            break;

                        case 9:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_NEF_AIR6_94", "VO_BRMA17_1_NEF_AIR6_94", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            break;

                        case 10:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_NEF_AIR7_95", "VO_BRMA17_1_NEF_AIR7_95", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            break;

                        case 11:
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_NEF_AIR8_96", "VO_BRMA17_1_NEF_AIR8_96", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                            break;

                        case 13:
                            this.<>f__this.m_inOnyxiaState = false;
                            GameState.Get().SetBusy(true);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_TRANSFORM1_80", "VO_BRMA17_1_TRANSFORM1_80", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            this.$PC = 4;
                            goto Label_0471;
                    }
                    goto Label_0468;

                case 1:
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_RESURRECT3_84", "VO_BRMA17_1_RESURRECT3_84", Notification.SpeechBubbleDirection.TopRight, this.<>f__this.m_nefActor, 1f, true, false));
                    this.$PC = 2;
                    goto Label_0471;

                case 2:
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("OnyxiaBoss_Start_1", "OnyxiaBoss_Start_1", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$PC = 3;
                    goto Label_0471;

                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_0468;

                case 4:
                    GameState.Get().SetBusy(false);
                    goto Label_0468;

                case 5:
                    break;

                case 6:
                    goto Label_0468;

                default:
                    goto Label_046F;
            }
            if (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 5;
            }
            else
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_TRANSFORM2_81", "VO_BRMA17_1_TRANSFORM2_81", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                this.$PC = 6;
            }
            goto Label_0471;
        Label_0468:
            this.$PC = -1;
        Label_046F:
            return false;
        Label_0471:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator126 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM17_ZombieNef <>f__this;
        internal Actor <enemyActor>__0;
        internal int turn;

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
                this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                if (this.turn == 1)
                {
                    this.<>f__this.m_nefActor = this.<enemyActor>__0;
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_TURN1_79", "VO_BRMA17_1_TURN1_79", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                }
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator125 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM17_ZombieNef <>f__this;
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
                        goto Label_020B;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    GameState.Get().SetBusy(false);
                    goto Label_0202;

                case 4:
                    goto Label_0202;

                default:
                    goto Label_0209;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_020B;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM17_ZombieNef.<>f__switch$map11 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                    dictionary.Add("BRMA17_4", 0);
                    dictionary.Add("BRMA17_5", 1);
                    dictionary.Add("BRMA17_5H", 1);
                    BRM17_ZombieNef.<>f__switch$map11 = dictionary;
                }
                if (BRM17_ZombieNef.<>f__switch$map11.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_cardLinePlayed || this.<>f__this.m_inOnyxiaState)
                        {
                            goto Label_0209;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        GameState.Get().SetBusy(true);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_CARD_86", "VO_BRMA17_1_CARD_86", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        this.$PC = 3;
                        goto Label_020B;
                    }
                    if (num2 == 1)
                    {
                        if (this.<>f__this.m_heroPowerLinePlayed)
                        {
                            goto Label_0209;
                        }
                        this.<>f__this.m_heroPowerLinePlayed = true;
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA17_1_HERO_POWER_87", "VO_BRMA17_1_HERO_POWER_87", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        this.$PC = 4;
                        goto Label_020B;
                    }
                }
            }
        Label_0202:
            this.$PC = -1;
        Label_0209:
            return false;
        Label_020B:
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

