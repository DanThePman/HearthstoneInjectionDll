using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NAX08_Gothik : NAX_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map21;
    private bool m_cardLinePlayed;
    private bool m_deadReturnLinePlayed;
    private bool m_unrelentingMinionLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator167 { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator169 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX8_01_EMOTE1_06",
            m_stringTag = "VO_NAX8_01_EMOTE1_06"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX8_01_EMOTE2_07",
            m_stringTag = "VO_NAX8_01_EMOTE2_07"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX8_01_EMOTE3_08",
            m_stringTag = "VO_NAX8_01_EMOTE3_08"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX8_01_EMOTE4_09",
            m_stringTag = "VO_NAX8_01_EMOTE4_09"
        };
        list2.Add(response);
        response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX8_01_EMOTE5_10",
            m_stringTag = "VO_NAX8_01_EMOTE5_10"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_NAX8_01_CARD_02");
        base.PreloadSound("VO_NAX8_01_CUSTOM_03");
        base.PreloadSound("VO_NAX8_01_EMOTE1_06");
        base.PreloadSound("VO_NAX8_01_EMOTE2_07");
        base.PreloadSound("VO_NAX8_01_EMOTE3_08");
        base.PreloadSound("VO_NAX8_01_EMOTE4_09");
        base.PreloadSound("VO_NAX8_01_EMOTE5_10");
        base.PreloadSound("VO_NAX8_01_CUSTOM2_04");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator168 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator167 : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateKTQuote("VO_KT_GOTHIK2_62", "VO_KT_GOTHIK2_62", true);
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator169 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal NAX08_Gothik <>f__this;
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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.HandleMissionEventWithTiming(this.missionEvent));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    if (this.missionEvent == 1)
                    {
                        if (!this.<>f__this.m_deadReturnLinePlayed)
                        {
                            this.<>f__this.m_deadReturnLinePlayed = true;
                            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX8_01_CUSTOM2_04", "VO_NAX8_01_CUSTOM2_04", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                            break;
                            this.$PC = -1;
                        }
                        break;
                    }
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator168 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal NAX08_Gothik <>f__this;
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
                        goto Label_01C2;
                    }
                    break;

                case 2:
                    break;

                default:
                    goto Label_01C0;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01C2;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (NAX08_Gothik.<>f__switch$map21 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(4);
                    dictionary.Add("NAX8_03", 0);
                    dictionary.Add("NAX8_04", 0);
                    dictionary.Add("NAX8_05", 0);
                    dictionary.Add("NAX8_02", 1);
                    NAX08_Gothik.<>f__switch$map21 = dictionary;
                }
                if (NAX08_Gothik.<>f__switch$map21.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        if (this.<>f__this.m_unrelentingMinionLinePlayed)
                        {
                            goto Label_01C0;
                        }
                        this.<>f__this.m_unrelentingMinionLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX8_01_CARD_02", "VO_NAX8_01_CARD_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                    else if (num2 == 1)
                    {
                        if (this.<>f__this.m_cardLinePlayed)
                        {
                            goto Label_01C0;
                        }
                        this.<>f__this.m_cardLinePlayed = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX8_01_CUSTOM_03", "VO_NAX8_01_CUSTOM_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    }
                }
            }
            this.$PC = -1;
        Label_01C0:
            return false;
        Label_01C2:
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

