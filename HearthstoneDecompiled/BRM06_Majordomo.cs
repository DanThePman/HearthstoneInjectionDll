using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BRM06_Majordomo : BRM_MissionEntity
{
    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator102 { gameResult = gameResult, <$>gameResult = gameResult };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__Iterator100 { missionEvent = missionEvent, <$>missionEvent = missionEvent, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator101 { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
    {
        Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
        switch (emoteType)
        {
            case EmoteType.GREETINGS:
            case EmoteType.WELL_PLAYED:
            case EmoteType.OOPS:
            case EmoteType.THREATEN:
            case EmoteType.THANKS:
            case EmoteType.SORRY:
            {
                Entity hero = GameState.Get().GetOpposingSidePlayer().GetHero();
                if (!(hero.GetCardId() == "BRMA06_1") && !(hero.GetCardId() == "BRMA06_1H"))
                {
                    if ((hero.GetCardId() == "BRMA06_3") || (hero.GetCardId() == "BRMA06_3H"))
                    {
                        Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_BRMA06_3_RESPONSE_03", "VO_BRMA06_3_RESPONSE_03", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                    }
                    break;
                }
                Gameplay.Get().StartCoroutine(base.PlaySoundAndBlockSpeech("VO_BRMA06_1_RESPONSE_03", "VO_BRMA06_1_RESPONSE_03", Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                break;
            }
        }
    }

    [DebuggerHidden]
    public override IEnumerator PlayMissionIntroLineAndWait()
    {
        return new <PlayMissionIntroLineAndWait>c__Iterator103 { <>f__this = this };
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_BRMA06_1_RESPONSE_03");
        base.PreloadSound("VO_BRMA06_3_RESPONSE_03");
        base.PreloadSound("VO_BRMA06_1_DEATH_04");
        base.PreloadSound("VO_BRMA06_1_TURN1_02_ALT");
        base.PreloadSound("VO_BRMA06_1_SUMMON_RAG_05");
        base.PreloadSound("VO_BRMA06_3_INTRO_01");
        base.PreloadSound("VO_BRMA06_3_TURN1_02");
        base.PreloadSound("VO_NEFARIAN_MAJORDOMO_41");
    }

    [CompilerGenerated]
    private sealed class <HandleGameOverWithTiming>c__Iterator102 : IDisposable, IEnumerator, IEnumerator<object>
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
                    NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", GameStrings.Get("VO_NEFARIAN_MAJORDOMO_DEAD_42"), "VO_NEFARIAN_MAJORDOMO_DEAD_42", true, 0f, CanvasAnchor.BOTTOM_LEFT);
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
    private sealed class <HandleMissionEventWithTiming>c__Iterator100 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>missionEvent;
        internal BRM06_Majordomo <>f__this;
        internal Actor <enemyActor>__0;
        internal EntityDef <ragDef>__1;
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
                    if (this.missionEvent == 1)
                    {
                        this.<ragDef>__1 = DefLoader.Get().GetEntityDef("BRMA06_3");
                        Gameplay.Get().UpdateEnemySideNameBannerName(this.<ragDef>__1.GetName());
                        NotificationManager.Get().CreateCharacterQuote("Majordomo_Quote", new Vector3(157.6f, NotificationManager.DEPTH, 84.5f), GameStrings.Get("VO_BRMA06_1_SUMMON_RAG_05"), string.Empty, true, 30f, null, CanvasAnchor.BOTTOM_LEFT);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA06_1_SUMMON_RAG_05", 1f, true, false));
                        this.$PC = 1;
                        goto Label_0160;
                    }
                    break;

                case 1:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    this.<enemyActor>__0 = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA06_3_TURN1_02", "VO_BRMA06_3_TURN1_02", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
                    this.$PC = 2;
                    goto Label_0160;

                case 2:
                    break;

                default:
                    goto Label_015E;
            }
            this.$PC = -1;
        Label_015E:
            return false;
        Label_0160:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator101 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal BRM06_Majordomo <>f__this;
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
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_BRMA06_1_TURN1_02_ALT", "VO_BRMA06_1_TURN1_02_ALT", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 1f, true, false));
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
    private sealed class <PlayMissionIntroLineAndWait>c__Iterator103 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal BRM06_Majordomo <>f__this;

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
                    if (!NotificationManager.Get().HasSoundPlayedThisSession("VO_NEFARIAN_MAJORDOMO_41"))
                    {
                        NotificationManager.Get().ForceAddSoundToPlayedList("VO_NEFARIAN_MAJORDOMO_41");
                        NotificationManager.Get().CreateCharacterQuote("NormalNefarian_Quote", new Vector3(97.7f, NotificationManager.DEPTH, 27.8f), GameStrings.Get("VO_NEFARIAN_MAJORDOMO_41"), string.Empty, false, 30f, null, CanvasAnchor.BOTTOM_LEFT);
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait("VO_NEFARIAN_MAJORDOMO_41", string.Empty, Notification.SpeechBubbleDirection.None, null, 1f, true, false, 3f));
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    NotificationManager.Get().DestroyActiveQuote(0f);
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
}

