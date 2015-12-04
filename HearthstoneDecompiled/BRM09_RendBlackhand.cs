using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM09_RendBlackhand : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map9;
    private bool m_cardLinePlayed;
    private bool m_heroPower1LinePlayed;
    private bool m_heroPower2LinePlayed;
    private bool m_heroPower3LinePlayed;
    private bool m_heroPower4LinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator10C { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator10B { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA09_1_RESPONSE_04",
            m_stringTag = "VO_BRMA09_1_RESPONSE_04"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA09_1_RESPONSE_04");
        base.PreloadSound("VO_BRMA09_1_HERO_POWER1_06");
        base.PreloadSound("VO_BRMA09_1_HERO_POWER2_07");
        base.PreloadSound("VO_BRMA09_1_HERO_POWER3_08");
        base.PreloadSound("VO_BRMA09_1_HERO_POWER4_09");
        base.PreloadSound("VO_BRMA09_1_CARD_05");
        base.PreloadSound("VO_BRMA09_1_TURN1_03");
        base.PreloadSound("VO_NEFARIAN_REND1_52");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator10A { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator10C : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_REND_DEAD_53"), "VO_NEFARIAN_REND_DEAD_53", true, 0f, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator10B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM09_RendBlackhand <>f__this;
        internal Actor <enemyActor>__1;
        internal Vector3 <quotePos>__0;
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
                    this.<quotePos>__0 = new Vector3(95f, NotificationManager.DEPTH, 36.8f);
                    this.<enemyActor>__1 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    if (this.turn == 1)
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA09_1_TURN1_03", "VO_BRMA09_1_TURN1_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 1f, true, false));
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    if (!GameMgr.Get().IsClassChallengeMission())
                    {
                        NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_REND1_52"), "VO_NEFARIAN_REND1_52", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                    }
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator10A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM09_RendBlackhand <>f__this;
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
                        goto Label_0308;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_0306;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0308;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM09_RendBlackhand.<>f__switch$map9 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(9);
                    dictionary.Add("BRMA09_2", 0);
                    dictionary.Add("BRMA09_2H", 0);
                    dictionary.Add("BRMA09_3", 1);
                    dictionary.Add("BRMA09_3H", 1);
                    dictionary.Add("BRMA09_4", 2);
                    dictionary.Add("BRMA09_4H", 2);
                    dictionary.Add("BRMA09_5", 3);
                    dictionary.Add("BRMA09_5H", 3);
                    dictionary.Add("BRMA09_6", 4);
                    BRM09_RendBlackhand.<>f__switch$map9 = dictionary;
                }
                if (BRM09_RendBlackhand.<>f__switch$map9.TryGetValue(cardId, out num2))
                {
                    switch (num2)
                    {
                        case 0:
                            if (!this.<>f__this.m_heroPower1LinePlayed)
                            {
                                this.<>f__this.m_heroPower1LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA09_1_HERO_POWER1_06", "VO_BRMA09_1_HERO_POWER1_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0306;

                        case 1:
                            if (!this.<>f__this.m_heroPower2LinePlayed)
                            {
                                this.<>f__this.m_heroPower2LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA09_1_HERO_POWER2_07", "VO_BRMA09_1_HERO_POWER2_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0306;

                        case 2:
                            if (!this.<>f__this.m_heroPower3LinePlayed)
                            {
                                this.<>f__this.m_heroPower3LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA09_1_HERO_POWER3_08", "VO_BRMA09_1_HERO_POWER3_08", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0306;

                        case 3:
                            if (!this.<>f__this.m_heroPower4LinePlayed)
                            {
                                this.<>f__this.m_heroPower4LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA09_1_HERO_POWER4_09", "VO_BRMA09_1_HERO_POWER4_09", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0306;

                        case 4:
                            if (!this.<>f__this.m_cardLinePlayed)
                            {
                                this.<>f__this.m_cardLinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA09_1_CARD_05", "VO_BRMA09_1_CARD_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0306;
                    }
                }
            }
            this.$PC = -1;
        Label_0306:
            return false;
        Label_0308:
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

