using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM15_Maloriak : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$mapF;
    private bool m_cardLinePlayed;
    private bool m_heroPowerLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator120 { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator11F { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA15_1_RESPONSE_03",
            m_stringTag = "VO_BRMA15_1_RESPONSE_03"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA15_1_RESPONSE_03");
        base.PreloadSound("VO_BRMA15_1_HERO_POWER_06");
        base.PreloadSound("VO_BRMA15_1_CARD_05");
        base.PreloadSound("VO_BRMA15_1_TURN1_02");
        base.PreloadSound("VO_NEFARIAN_MALORIAK_TURN2_71");
        base.PreloadSound("VO_NEFARIAN_MALORIAK_DEATH_PT1_72");
        base.PreloadSound("VO_NEFARIAN_MALORIAK_DEATH_PT2_73");
        base.PreloadSound("VO_NEFARIAN_MALORIAK_DEATH_PT3_74");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator11E { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator120 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal BRM15_Maloriak <>f__this;
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
                    goto Label_0156;

                case 1:
                    NotificationManager.Get().CreateCharacterQuote("NefarianDragon_Quote", GameStrings.Get("VO_NEFARIAN_MALORIAK_DEATH_PT2_73"), string.Empty, true, 5f, CanvasAnchor.BOTTOM_LEFT);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_NEFARIAN_MALORIAK_DEATH_PT2_73", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                    this.$PC = 2;
                    goto Label_0156;

                case 2:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    NotificationManager.Get().CreateCharacterQuote("NefarianDragon_Quote", GameStrings.Get("VO_NEFARIAN_MALORIAK_DEATH_PT3_74"), string.Empty, true, 7f, CanvasAnchor.BOTTOM_LEFT);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_NEFARIAN_MALORIAK_DEATH_PT3_74", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                    this.$PC = 3;
                    goto Label_0156;

                case 3:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0156:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator11F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM15_Maloriak <>f__this;
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
                switch (this.turn)
                {
                    case 1:
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA15_1_TURN1_02", "VO_BRMA15_1_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        break;

                    case 2:
                        NotificationManager.Get().CreateCharacterQuote("NefarianDragon_Quote", new Vector3(95f, NotificationManager.DEPTH, 36.8f), GameStrings.Get("VO_NEFARIAN_MALORIAK_TURN2_71"), "VO_NEFARIAN_MALORIAK_TURN2_71", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                        break;
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator11E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM15_Maloriak <>f__this;
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
                if (BRM15_Maloriak.<>f__switch$mapF == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(3);
                    dictionary.Add("BRMA15_2", 0);
                    dictionary.Add("BRMA15_2H", 0);
                    dictionary.Add("BRMA15_3", 1);
                    BRM15_Maloriak.<>f__switch$mapF = dictionary;
                }
                if (BRM15_Maloriak.<>f__switch$mapF.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_heroPowerLinePlayed)
                        {
                            goto Label_01B4;
                        }
                        this.<>f__this.m_heroPowerLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA15_1_HERO_POWER_06", "VO_BRMA15_1_HERO_POWER_06", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                    else if (num2 == 1)
                    {
                        if (this.<>f__this.m_cardLinePlayed)
                        {
                            goto Label_01B4;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA15_1_CARD_05", "VO_BRMA15_1_CARD_05", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
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

