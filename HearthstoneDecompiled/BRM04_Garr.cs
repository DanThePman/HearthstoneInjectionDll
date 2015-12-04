using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM04_Garr : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map5;
    private bool m_cardLinePlayed;
    private bool m_heroPowerLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__IteratorFC { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__IteratorFB { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA04_1_RESPONSE_03",
            m_stringTag = "VO_BRMA04_1_RESPONSE_03"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA04_1_RESPONSE_03");
        base.PreloadSound("VO_BRMA04_1_HERO_POWER_05");
        base.PreloadSound("VO_BRMA04_1_CARD_04");
        base.PreloadSound("VO_BRMA04_1_TURN1_02");
        base.PreloadSound("VO_NEFARIAN_GARR2_35");
        base.PreloadSound("VO_NEFARIAN_GARR3_36");
        base.PreloadSound("VO_NEFARIAN_GARR4_37");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__IteratorFA { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__IteratorFC : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_GARR_DEAD1_38"), "VO_NEFARIAN_GARR_DEAD1_38", true, 0f, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <HandleStartOfTurnWithTiming>c__IteratorFB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM04_Garr <>f__this;
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
            this.$PC = -1;
            if (this.$PC == 0)
            {
                this.<quotePos>__0 = new Vector3(95f, NotificationManager.DEPTH, 36.8f);
                this.<enemyActor>__1 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                int turn = this.turn;
                switch (turn)
                {
                    case 1:
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA04_1_TURN1_02", "VO_BRMA04_1_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 1f, true, false));
                        goto Label_0174;

                    case 4:
                        if (!GameMgr.Get().IsClassChallengeMission())
                        {
                            NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_GARR2_35"), "VO_NEFARIAN_GARR2_35", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                        }
                        goto Label_0174;
                }
                if (turn != 8)
                {
                    if ((turn == 12) && !GameMgr.Get().IsClassChallengeMission())
                    {
                        NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_GARR4_37"), "VO_NEFARIAN_GARR4_37", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                    }
                }
                else if (!GameMgr.Get().IsClassChallengeMission())
                {
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", this.<quotePos>__0, GameStrings.Get("VO_NEFARIAN_GARR3_36"), "VO_NEFARIAN_GARR3_36", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                }
            }
        Label_0174:
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
    private sealed class <RespondToPlayedCardWithTiming>c__IteratorFA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM04_Garr <>f__this;
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
                        goto Label_01B6;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_01B4;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01B6;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM04_Garr.<>f__switch$map5 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                    dictionary.Add("BRMA04_2", 0);
                    dictionary.Add("BRMA04_4", 1);
                    dictionary.Add("BRMA04_4H", 1);
                    BRM04_Garr.<>f__switch$map5 = dictionary;
                }
                if (BRM04_Garr.<>f__switch$map5.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_heroPowerLinePlayed)
                        {
                            goto Label_01B4;
                        }
                        this.<>f__this.m_heroPowerLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA04_1_HERO_POWER_05", "VO_BRMA04_1_HERO_POWER_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                    else if (num2 == 1)
                    {
                        if (this.<>f__this.m_cardLinePlayed)
                        {
                            goto Label_01B4;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA04_1_CARD_04", "VO_BRMA04_1_CARD_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                }
            }
            this.$PC = -1;
        Label_01B4:
            return false;
        Label_01B6:
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

