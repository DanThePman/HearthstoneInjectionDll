using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM08_Drakkisath : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map8;
    private bool m_cardLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator109 { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator108 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA08_1_RESPONSE_04",
            m_stringTag = "VO_BRMA08_1_RESPONSE_04"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA08_1_RESPONSE_04");
        base.PreloadSound("VO_BRMA08_1_CARD_05");
        base.PreloadSound("VO_BRMA08_1_TURN1_03");
        base.PreloadSound("VO_NEFARIAN_DRAKKISATH_RESPOND_48");
        base.PreloadSound("VO_BRMA08_1_TURN1_ALT_02");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator107 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator109 : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_DRAKKISATH_DEAD_50"), "VO_NEFARIAN_DRAKKISATH_DEAD_50", true, 0f, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator108 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM08_Drakkisath <>f__this;
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
                    switch (this.turn)
                    {
                        case 1:
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA08_1_TURN1_ALT_02", "VO_BRMA08_1_TURN1_ALT_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 1f, true, false));
                            this.$PC = 1;
                            goto Label_0198;

                        case 4:
                            if (GameMgr.Get().IsClassChallengeMission())
                            {
                                break;
                            }
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA08_1_TURN1_03", "VO_BRMA08_1_TURN1_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 1f, true, false));
                            this.$PC = 2;
                            goto Label_0198;

                        case 6:
                            if (!GameMgr.Get().IsClassChallengeMission())
                            {
                                NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_DRAKKISATH1_49"), "VO_NEFARIAN_DRAKKISATH1_49", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                                break;
                                this.$PC = -1;
                            }
                            goto Label_0196;
                    }
                    break;

                case 2:
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_DRAKKISATH_RESPOND_48"), "VO_NEFARIAN_DRAKKISATH_RESPOND_48", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                    break;
            }
        Label_0196:
            return false;
        Label_0198:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator107 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM08_Drakkisath <>f__this;
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
                        goto Label_0143;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_0141;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0143;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM08_Drakkisath.<>f__switch$map8 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(1);
                    dictionary.Add("BRMA08_3", 0);
                    BRM08_Drakkisath.<>f__switch$map8 = dictionary;
                }
                if (BRM08_Drakkisath.<>f__switch$map8.TryGetValue(cardId, out num2) && (num2 == 0))
                {
                    if (this.<>f__this.m_cardLinePlayed)
                    {
                        goto Label_0141;
                    }
                    this.<>f__this.m_cardLinePlayed = true;
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA08_1_CARD_05", "VO_BRMA08_1_CARD_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                }
            }
            this.$PC = -1;
        Label_0141:
            return false;
        Label_0143:
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

