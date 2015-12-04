using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NAX09_Horsemen : NAX_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map22;
    private bool m_cardLinePlayed;
    private bool m_heroPowerLinePlayed;
    private bool m_introSequenceComplete;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator16A { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator16B { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX9_01_EMOTE_04",
            m_stringTag = "VO_NAX9_01_EMOTE_04"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_NAX9_01_CUSTOM_02");
        base.PreloadSound("VO_NAX9_01_EMOTE_04");
        base.PreloadSound("VO_FP1_031_EnterPlay_06");
        base.PreloadSound("VO_NAX9_02_CUSTOM_01");
        base.PreloadSound("VO_NAX9_03_CUSTOM_01");
        base.PreloadSound("VO_NAX9_04_CUSTOM_01");
        base.PreloadSound("VO_FP1_031_Attack_07");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator16C { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator16A : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateKTQuote("VO_KT_BARON2_64", "VO_KT_BARON2_64", true);
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator16B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal List<Card>.Enumerator <$s_708>__4;
        internal NAX09_Horsemen <>f__this;
        internal Actor <baronActor>__0;
        internal Actor <blaumeuxActor>__1;
        internal Card <card>__5;
        internal string <cardID>__6;
        internal Actor <thaneActor>__2;
        internal Actor <zeliekActor>__3;
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
                    if (this.turn == 1)
                    {
                        this.<baronActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                        this.<blaumeuxActor>__1 = null;
                        this.<thaneActor>__2 = null;
                        this.<zeliekActor>__3 = null;
                        this.<$s_708>__4 = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone().GetCards().GetEnumerator();
                        try
                        {
                            while (this.<$s_708>__4.MoveNext())
                            {
                                this.<card>__5 = this.<$s_708>__4.Current;
                                this.<cardID>__6 = this.<card>__5.GetEntity().GetCardId();
                                if (this.<cardID>__6 == "NAX9_02")
                                {
                                    this.<blaumeuxActor>__1 = this.<card>__5.GetActor();
                                }
                                else
                                {
                                    if (this.<cardID>__6 == "NAX9_03")
                                    {
                                        this.<thaneActor>__2 = this.<card>__5.GetActor();
                                        continue;
                                    }
                                    if (this.<cardID>__6 == "NAX9_04")
                                    {
                                        this.<zeliekActor>__3 = this.<card>__5.GetActor();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            this.<$s_708>__4.Dispose();
                        }
                        if (this.<zeliekActor>__3 == null)
                        {
                            this.<>f__this.m_introSequenceComplete = true;
                        }
                        else
                        {
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX9_02_CUSTOM_01", "VO_NAX9_02_CUSTOM_01", Notification.SpeechBubbleDirection.TopRight, this.<zeliekActor>__3, 1f, true, false));
                            this.$PC = 1;
                            goto Label_02D1;
                        }
                    }
                    goto Label_02CF;

                case 1:
                    if (this.<blaumeuxActor>__1 == null)
                    {
                        break;
                    }
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX9_03_CUSTOM_01", "VO_NAX9_03_CUSTOM_01", Notification.SpeechBubbleDirection.TopRight, this.<blaumeuxActor>__1, 1f, true, false));
                    this.$PC = 2;
                    goto Label_02D1;

                case 2:
                    break;

                case 3:
                    goto Label_0262;

                case 4:
                    goto Label_02B2;

                default:
                    goto Label_02CF;
            }
            if (this.<baronActor>__0 != null)
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX9_01_CUSTOM_02", "VO_NAX9_01_CUSTOM_02", Notification.SpeechBubbleDirection.TopRight, this.<baronActor>__0, 1f, true, false));
                this.$PC = 3;
                goto Label_02D1;
            }
        Label_0262:
            if (this.<thaneActor>__2 != null)
            {
                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX9_04_CUSTOM_01", "VO_NAX9_04_CUSTOM_01", Notification.SpeechBubbleDirection.TopRight, this.<thaneActor>__2, 1f, true, false));
                this.$PC = 4;
                goto Label_02D1;
            }
        Label_02B2:
            this.<>f__this.m_introSequenceComplete = true;
            goto Label_02CF;
            this.$PC = -1;
        Label_02CF:
            return false;
        Label_02D1:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator16C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal NAX09_Horsemen <>f__this;
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
                    if (this.<>f__this.m_introSequenceComplete)
                    {
                        break;
                    }
                    goto Label_01BD;

                case 1:
                    break;

                case 2:
                    goto Label_007A;

                default:
                    goto Label_01BD;
            }
            while (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_01BF;
            }
        Label_007A:
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01BF;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (NAX09_Horsemen.<>f__switch$map22 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(2);
                    dictionary.Add("NAX9_06", 0);
                    dictionary.Add("NAX9_07", 1);
                    NAX09_Horsemen.<>f__switch$map22 = dictionary;
                }
                if (NAX09_Horsemen.<>f__switch$map22.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_heroPowerLinePlayed)
                        {
                            goto Label_01BD;
                        }
                        this.<>f__this.m_heroPowerLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_FP1_031_EnterPlay_06", "VO_FP1_031_EnterPlay_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                    else if (num2 == 1)
                    {
                        if (this.<>f__this.m_cardLinePlayed)
                        {
                            goto Label_01BD;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_FP1_031_Attack_07", "VO_FP1_031_Attack_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                }
            }
            this.$PC = -1;
        Label_01BD:
            return false;
        Label_01BF:
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

