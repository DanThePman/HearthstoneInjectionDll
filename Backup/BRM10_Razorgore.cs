using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM10_Razorgore : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$mapA;
    private int m_eggDeathLinePlayed;
    private bool m_heroPowerLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator110 { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator10F { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator10E { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA10_1_RESPONSE_03",
            m_stringTag = "VO_BRMA10_1_RESPONSE_03"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA10_1_RESPONSE_03");
        base.PreloadSound("VO_BRMA10_1_HERO_POWER_04");
        base.PreloadSound("VO_BRMA10_1_EGG_DEATH_1_05");
        base.PreloadSound("VO_BRMA10_1_EGG_DEATH_2_06");
        base.PreloadSound("VO_BRMA10_1_EGG_DEATH_3_07");
        base.PreloadSound("VO_BRMA10_1_TURN1_02");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator10D { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator110 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
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
                    return true;

                case 1:
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_RAZORGORE_DEAD_55"), "VO_NEFARIAN_RAZORGORE_DEAD_55", true, 0f, CanvasAnchor.BOTTOM_LEFT);
                    break;

                default:
                    break;
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator10F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal BRM10_Razorgore <>f__this;
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
                case 1:
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0199;
                    }
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    if (this.missionEvent == 1)
                    {
                        this.<>f__this.m_eggDeathLinePlayed++;
                        switch (this.<>f__this.m_eggDeathLinePlayed)
                        {
                            case 1:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA10_1_EGG_DEATH_1_05", "VO_BRMA10_1_EGG_DEATH_1_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 2;
                                goto Label_0199;

                            case 2:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA10_1_EGG_DEATH_2_06", "VO_BRMA10_1_EGG_DEATH_2_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 3;
                                goto Label_0199;

                            case 3:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA10_1_EGG_DEATH_3_07", "VO_BRMA10_1_EGG_DEATH_3_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 4;
                                goto Label_0199;
                        }
                    }
                    break;

                case 2:
                case 3:
                case 4:
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0199:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator10E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM10_Razorgore <>f__this;
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
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA10_1_TURN1_02", "VO_BRMA10_1_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator10D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM10_Razorgore <>f__this;
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
                        goto Label_014F;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_014D;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_014F;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM10_Razorgore.<>f__switch$mapA == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                    dictionary.Add("BRMA10_3", 0);
                    dictionary.Add("BRMA10_3H", 0);
                    BRM10_Razorgore.<>f__switch$mapA = dictionary;
                }
                if (BRM10_Razorgore.<>f__switch$mapA.TryGetValue(cardId, out num2) && (num2 == 0))
                {
                    if (this.<>f__this.m_heroPowerLinePlayed)
                    {
                        goto Label_014D;
                    }
                    this.<>f__this.m_heroPowerLinePlayed = true;
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA10_1_HERO_POWER_04", "VO_BRMA10_1_HERO_POWER_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                }
            }
            this.$PC = -1;
        Label_014D:
            return false;
        Label_014F:
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

