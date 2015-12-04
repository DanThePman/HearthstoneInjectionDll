using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM13_Nefarian : BRM_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$mapD;
    private bool m_heroPowerLinePlayed;
    private int m_ragLine;
    private Vector3 ragLinePosition = new Vector3(95f, NotificationManager.DEPTH, 36.8f);

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator119 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_BRMA13_1_RESPONSE_05",
            m_stringTag = "VO_BRMA13_1_RESPONSE_05"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA13_1_RESPONSE_05");
        base.PreloadSound("VO_BRMA13_1_TURN1_PT1_02");
        base.PreloadSound("VO_BRMA13_1_TURN1_PT2_03");
        base.PreloadSound("VO_RAGNAROS_NEF1_71");
        base.PreloadSound("VO_BRMA13_1_HP_PALADIN_07");
        base.PreloadSound("VO_BRMA13_1_HP_PRIEST_08");
        base.PreloadSound("VO_BRMA13_1_HP_WARLOCK_10");
        base.PreloadSound("VO_BRMA13_1_HP_WARRIOR_09");
        base.PreloadSound("VO_BRMA13_1_HP_MAGE_11");
        base.PreloadSound("VO_BRMA13_1_HP_DRUID_14");
        base.PreloadSound("VO_BRMA13_1_HP_SHAMAN_13");
        base.PreloadSound("VO_BRMA13_1_HP_HUNTER_12");
        base.PreloadSound("VO_BRMA13_1_HP_ROGUE_15");
        base.PreloadSound("VO_BRMA13_1_HP_GENERIC_18");
        base.PreloadSound("VO_BRMA13_1_DEATHWING_19");
        base.PreloadSound("VO_NEFARIAN_NEF2_65");
        base.PreloadSound("VO_NEFARIAN_NEF_MISSION_66");
        base.PreloadSound("VO_RAGNAROS_NEF3_72");
        base.PreloadSound("VO_NEFARIAN_HEROIC_BLOCK_77");
        base.PreloadSound("VO_RAGNAROS_NEF4_73");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator118 { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator UnBusyInSeconds(float seconds)
    {
        return new <UnBusyInSeconds>c__Iterator11A { seconds = seconds, <$>seconds = seconds };
    }

    [CompilerGenerated]
    private sealed class <HandleMissionEventWithTiming>c__Iterator119 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal BRM13_Nefarian <>f__this;
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
                        case 5:
                            GameState.Get().SetBusy(true);
                            this.$current = new WaitForSeconds(4f);
                            this.$PC = 2;
                            goto Label_0502;

                        case 6:
                            NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", this.<>f__this.ragLinePosition, GameStrings.Get("VO_RAGNAROS_NEF4_73"), string.Empty, true, 30f, null, CanvasAnchor.BOTTOM_LEFT);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_RAGNAROS_NEF4_73", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                            this.$PC = 8;
                            goto Label_0502;
                    }
                    goto Label_04F9;

                case 1:
                    break;

                case 2:
                case 3:
                    while (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 3;
                        goto Label_0502;
                    }
                    this.<>f__this.m_ragLine++;
                    Gameplay.Get().StartCoroutine(this.<>f__this.UnBusyInSeconds(1f));
                    switch (this.<>f__this.m_ragLine)
                    {
                        case 1:
                            NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", this.<>f__this.ragLinePosition, GameStrings.Get("VO_RAGNAROS_NEF1_71"), string.Empty, true, 30f, null, CanvasAnchor.BOTTOM_LEFT);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_RAGNAROS_NEF1_71", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                            this.$PC = 4;
                            goto Label_0502;

                        case 2:
                            NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", this.<>f__this.ragLinePosition, GameStrings.Get("VO_RAGNAROS_NEF3_72"), string.Empty, true, 30f, null, CanvasAnchor.BOTTOM_LEFT);
                            this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_RAGNAROS_NEF3_72", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                            this.$PC = 6;
                            goto Label_0502;

                        case 3:
                            NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", this.<>f__this.ragLinePosition, GameStrings.Get("VO_RAGNAROS_NEF4_73"), "VO_RAGNAROS_NEF4_73", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                            goto Label_04F9;

                        case 4:
                            NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", this.<>f__this.ragLinePosition, GameStrings.Get("VO_RAGNAROS_NEF5_74"), "VO_RAGNAROS_NEF5_74", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                            goto Label_04F9;

                        case 5:
                            NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", this.<>f__this.ragLinePosition, GameStrings.Get("VO_RAGNAROS_NEF6_75"), "VO_RAGNAROS_NEF6_75", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                            goto Label_04F9;

                        case 6:
                            NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", this.<>f__this.ragLinePosition, GameStrings.Get("VO_RAGNAROS_NEF7_76"), "VO_RAGNAROS_NEF7_76", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                            goto Label_04F9;
                    }
                    NotificationManager.Get().CreateCharacterQuote("Ragnaros_Quote", this.<>f__this.ragLinePosition, GameStrings.Get("VO_RAGNAROS_NEF8_77"), "VO_RAGNAROS_NEF8_77", true, 0f, null, CanvasAnchor.BOTTOM_LEFT);
                    this.<>f__this.m_ragLine = 2;
                    goto Label_04F9;

                case 4:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NEFARIAN_NEF2_65", "VO_NEFARIAN_NEF2_65", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$PC = 5;
                    goto Label_0502;

                case 5:
                case 7:
                case 9:
                    goto Label_04F9;

                case 6:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NEFARIAN_NEF_MISSION_66", "VO_NEFARIAN_NEF_MISSION_66", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$PC = 7;
                    goto Label_0502;

                case 8:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_NEFARIAN_HEROIC_BLOCK_77", "VO_NEFARIAN_HEROIC_BLOCK_77", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$PC = 9;
                    goto Label_0502;

                default:
                    goto Label_0500;
            }
            if (this.<>f__this.m_enemySpeaking)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0502;
            }
            Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_DEATHWING_19", "VO_BRMA13_1_DEATHWING_19", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
        Label_04F9:
            this.$PC = -1;
        Label_0500:
            return false;
        Label_0502:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator118 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal BRM13_Nefarian <>f__this;
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
                        goto Label_051E;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_TURN1_PT2_03", "VO_BRMA13_1_TURN1_PT2_03", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$PC = 4;
                    goto Label_051E;

                case 4:
                    goto Label_0515;

                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    GameState.Get().SetBusy(false);
                    goto Label_0515;

                default:
                    goto Label_051C;
            }
            while (this.entity.GetCardType() == TAG_CARDTYPE.INVALID)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_051E;
            }
            this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHero().GetCard().GetActor();
            string cardId = this.entity.GetCardId();
            if (cardId != null)
            {
                int num2;
                if (BRM13_Nefarian.<>f__switch$mapD == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(4);
                    dictionary.Add("BRMA13_2", 0);
                    dictionary.Add("BRMA13_2H", 0);
                    dictionary.Add("BRMA13_4", 1);
                    dictionary.Add("BRMA13_4H", 1);
                    BRM13_Nefarian.<>f__switch$mapD = dictionary;
                }
                if (BRM13_Nefarian.<>f__switch$mapD.TryGetValue(cardId, out num2))
                {
                    if (num2 == 0)
                    {
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_TURN1_PT1_02", "VO_BRMA13_1_TURN1_PT1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, false, false));
                        this.$PC = 3;
                    }
                    else
                    {
                        if (num2 != 1)
                        {
                            goto Label_0515;
                        }
                        if (this.<>f__this.m_heroPowerLinePlayed)
                        {
                            goto Label_051C;
                        }
                        this.<>f__this.m_heroPowerLinePlayed = true;
                        GameState.Get().SetBusy(true);
                        switch (GameState.Get().GetFriendlySidePlayer().GetHero().GetClass())
                        {
                            case TAG_CLASS.DRUID:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_DRUID_14", "VO_BRMA13_1_HP_DRUID_14", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 10;
                                goto Label_051E;

                            case TAG_CLASS.HUNTER:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_HUNTER_12", "VO_BRMA13_1_HP_HUNTER_12", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 12;
                                goto Label_051E;

                            case TAG_CLASS.MAGE:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_MAGE_11", "VO_BRMA13_1_HP_MAGE_11", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 9;
                                goto Label_051E;

                            case TAG_CLASS.PALADIN:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_PALADIN_07", "VO_BRMA13_1_HP_PALADIN_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 5;
                                goto Label_051E;

                            case TAG_CLASS.PRIEST:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_PRIEST_08", "VO_BRMA13_1_HP_PRIEST_08", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 6;
                                goto Label_051E;

                            case TAG_CLASS.ROGUE:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_ROGUE_15", "VO_BRMA13_1_HP_ROGUE_15", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 13;
                                goto Label_051E;

                            case TAG_CLASS.SHAMAN:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_SHAMAN_13", "VO_BRMA13_1_HP_SHAMAN_13", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 11;
                                goto Label_051E;

                            case TAG_CLASS.WARLOCK:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_WARLOCK_10", "VO_BRMA13_1_HP_WARLOCK_10", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 7;
                                goto Label_051E;

                            case TAG_CLASS.WARRIOR:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_WARRIOR_09", "VO_BRMA13_1_HP_WARRIOR_09", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                                this.$PC = 8;
                                goto Label_051E;
                        }
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA13_1_HP_GENERIC_18", "VO_BRMA13_1_HP_GENERIC_18", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                        this.$PC = 14;
                    }
                    goto Label_051E;
                }
            }
        Label_0515:
            this.$PC = -1;
        Label_051C:
            return false;
        Label_051E:
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
    private sealed class <UnBusyInSeconds>c__Iterator11A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>seconds;
        internal float seconds;

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
                    this.$current = new WaitForSeconds(this.seconds);
                    this.$PC = 1;
                    return true;

                case 1:
                    GameState.Get().SetBusy(false);
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
}

