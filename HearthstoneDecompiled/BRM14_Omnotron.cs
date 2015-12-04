using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM14_Omnotron : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$mapE;
    private bool m_cardLinePlayed;
    private bool m_heroPower1LinePlayed;
    private bool m_heroPower2LinePlayed;
    private bool m_heroPower3LinePlayed;
    private bool m_heroPower4LinePlayed;
    private bool m_heroPower5LinePlayed;

    protected override void CycleNextResponseGroupIndex(MissionEntity.EmoteResponseGroup responseGroup)
    {
        if (responseGroup.m_responseIndex != (responseGroup.m_responses.Count - 1))
        {
            responseGroup.m_responseIndex++;
        }
    }

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator11D { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator11C { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA14_1_RESPONSE1_10",
            m_stringTag = "VO_BRMA14_1_RESPONSE1_10"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA14_1_RESPONSE2_11",
            m_stringTag = "VO_BRMA14_1_RESPONSE2_11"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA14_1_RESPONSE3_12",
            m_stringTag = "VO_BRMA14_1_RESPONSE3_12"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA14_1_RESPONSE4_13",
            m_stringTag = "VO_BRMA14_1_RESPONSE4_13"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA14_1_RESPONSE5_14",
            m_stringTag = "VO_BRMA14_1_RESPONSE5_14"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA14_1_RESPONSE1_10");
        base.PreloadSound("VO_BRMA14_1_RESPONSE2_11");
        base.PreloadSound("VO_BRMA14_1_RESPONSE3_12");
        base.PreloadSound("VO_BRMA14_1_RESPONSE4_13");
        base.PreloadSound("VO_BRMA14_1_RESPONSE5_14");
        base.PreloadSound("VO_BRMA14_1_HP1_03");
        base.PreloadSound("VO_BRMA14_1_HP2_04");
        base.PreloadSound("VO_BRMA14_1_HP3_05");
        base.PreloadSound("VO_BRMA14_1_HP4_06");
        base.PreloadSound("VO_BRMA14_1_HP5_07");
        base.PreloadSound("VO_BRMA14_1_CARD_09");
        base.PreloadSound("VO_BRMA14_1_TURN1_02");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator11B { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator11D : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateCharacterQuote("NefarianDragon_Quote", GameStrings.Get("VO_NEFARIAN_OMNOTRON_DEAD_69"), "VO_NEFARIAN_OMNOTRON_DEAD_69", true, 0f, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator11C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM14_Omnotron <>f__this;
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
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA14_1_TURN1_02", "VO_BRMA14_1_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator11B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM14_Omnotron <>f__this;
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
                        goto Label_0378;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_0376;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0378;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM14_Omnotron.<>f__switch$mapE == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(11);
                    dictionary.Add("BRMA14_6", 0);
                    dictionary.Add("BRMA14_6H", 0);
                    dictionary.Add("BRMA14_4", 1);
                    dictionary.Add("BRMA14_4H", 1);
                    dictionary.Add("BRMA14_2", 2);
                    dictionary.Add("BRMA14_2H", 2);
                    dictionary.Add("BRMA14_8", 3);
                    dictionary.Add("BRMA14_8H", 3);
                    dictionary.Add("BRMA14_10", 4);
                    dictionary.Add("BRMA14_10H", 4);
                    dictionary.Add("BRMA14_11", 5);
                    BRM14_Omnotron.<>f__switch$mapE = dictionary;
                }
                if (BRM14_Omnotron.<>f__switch$mapE.TryGetValue(cardId, out num2))
                {
                    switch (num2)
                    {
                        case 0:
                            if (!this.<>f__this.m_heroPower1LinePlayed)
                            {
                                this.<>f__this.m_heroPower1LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA14_1_HP1_03", "VO_BRMA14_1_HP1_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0376;

                        case 1:
                            if (!this.<>f__this.m_heroPower2LinePlayed)
                            {
                                this.<>f__this.m_heroPower2LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA14_1_HP2_04", "VO_BRMA14_1_HP2_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0376;

                        case 2:
                            if (!this.<>f__this.m_heroPower3LinePlayed)
                            {
                                this.<>f__this.m_heroPower3LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA14_1_HP3_05", "VO_BRMA14_1_HP3_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0376;

                        case 3:
                            if (!this.<>f__this.m_heroPower4LinePlayed)
                            {
                                this.<>f__this.m_heroPower4LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA14_1_HP4_06", "VO_BRMA14_1_HP4_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0376;

                        case 4:
                            if (!this.<>f__this.m_heroPower5LinePlayed)
                            {
                                this.<>f__this.m_heroPower5LinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA14_1_HP5_07", "VO_BRMA14_1_HP5_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0376;

                        case 5:
                            if (!this.<>f__this.m_cardLinePlayed)
                            {
                                this.<>f__this.m_cardLinePlayed = true;
                                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA14_1_CARD_09", "VO_BRMA14_1_CARD_09", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                break;
                            }
                            goto Label_0376;
                    }
                }
            }
            this.$PC = -1;
        Label_0376:
            return false;
        Label_0378:
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

