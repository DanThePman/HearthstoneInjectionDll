using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NAX07_Razuvious : NAX_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map20;
    private bool m_heroPowerLinePlayed;

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator164 { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator166 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_NAX7_01_EMOTE_05",
            m_stringTag = "VO_NAX7_01_EMOTE_05"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_NAX7_01_HP_02");
        base.PreloadSound("VO_NAX7_01_START_01");
        base.PreloadSound("VO_NAX7_01_EMOTE_05");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator165 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator164 : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateKTQuote("VO_KT_RAZUVIOUS2_59", "VO_KT_RAZUVIOUS2_59", true);
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator166 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal List<PowerTask>.Enumerator <$s_707>__3;
        internal NAX07_Razuvious <>f__this;
        internal Actor <enemyActor>__9;
        internal int <j>__7;
        internal Network.HistMetaData <metaData>__6;
        internal Network.PowerHistory <power>__5;
        internal Entity <sourceEntity>__2;
        internal Entity <targetEntity>__8;
        internal PowerTask <task>__4;
        internal PowerTaskList <taskList>__1;
        internal bool <understudiesAreInPlay>__0;
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
                    if (this.missionEvent == 1)
                    {
                        this.<understudiesAreInPlay>__0 = false;
                        this.<taskList>__1 = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
                        this.<sourceEntity>__2 = (this.<taskList>__1 != null) ? this.<taskList>__1.GetSourceEntity() : null;
                        if ((this.<sourceEntity>__2 != null) && (this.<sourceEntity>__2.GetCardId() == "NAX7_05"))
                        {
                            this.<$s_707>__3 = this.<taskList>__1.GetTaskList().GetEnumerator();
                            try
                            {
                                while (this.<$s_707>__3.MoveNext())
                                {
                                    this.<task>__4 = this.<$s_707>__3.Current;
                                    this.<power>__5 = this.<task>__4.GetPower();
                                    if (this.<power>__5.Type == Network.PowerType.META_DATA)
                                    {
                                        this.<metaData>__6 = this.<power>__5 as Network.HistMetaData;
                                        if (((this.<metaData>__6.MetaType == HistoryMeta.Type.TARGET) && (this.<metaData>__6.Info != null)) && (this.<metaData>__6.Info.Count != 0))
                                        {
                                            this.<j>__7 = 0;
                                            while (this.<j>__7 < this.<metaData>__6.Info.Count)
                                            {
                                                this.<targetEntity>__8 = GameState.Get().GetEntity(this.<metaData>__6.Info[this.<j>__7]);
                                                if ((this.<targetEntity>__8 != null) && (this.<targetEntity>__8.GetCardId() == "NAX7_02"))
                                                {
                                                    this.<understudiesAreInPlay>__0 = true;
                                                    break;
                                                }
                                                this.<j>__7++;
                                            }
                                            if (this.<understudiesAreInPlay>__0)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                this.<$s_707>__3.Dispose();
                            }
                        }
                        break;
                    }
                    goto Label_0292;

                default:
                    goto Label_0292;
            }
            if (this.<understudiesAreInPlay>__0)
            {
                this.<enemyActor>__9 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX7_01_START_01", "VO_NAX7_01_START_01", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__9, 1f, true, false));
                goto Label_0292;
                this.$PC = -1;
            }
        Label_0292:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator165 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal NAX07_Razuvious <>f__this;
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
                if (NAX07_Razuvious.<>f__switch$map20 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(1);
                    dictionary.Add("NAX7_03", 0);
                    NAX07_Razuvious.<>f__switch$map20 = dictionary;
                }
                if (NAX07_Razuvious.<>f__switch$map20.TryGetValue(cardId, out num2) && (num2 == 0))
                {
                    if (this.<>f__this.m_heroPowerLinePlayed)
                    {
                        goto Label_0141;
                    }
                    this.<>f__this.m_heroPowerLinePlayed = true;
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NAX7_01_HP_02", "VO_NAX7_01_HP_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
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

