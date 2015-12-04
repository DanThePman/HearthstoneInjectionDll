using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class MissionEntity : GameEntity
{
    protected bool m_delayCardSoundSpells;
    protected List<EmoteResponseGroup> m_emoteResponseGroups = new List<EmoteResponseGroup>();
    protected bool m_enemySpeaking;
    protected static readonly List<EmoteType> STANDARD_EMOTE_RESPONSE_TRIGGERS = new List<EmoteType> { 1, 2, 3, 6, 5, 4 };
    protected const float TIME_TO_WAIT_BEFORE_ENDING_QUOTE = 5f;

    public MissionEntity()
    {
        this.InitEmoteResponses();
    }

    protected virtual void CycleNextResponseGroupIndex(EmoteResponseGroup responseGroup)
    {
        if (responseGroup.m_responseIndex == (responseGroup.m_responses.Count - 1))
        {
            responseGroup.m_responseIndex = 0;
        }
        else
        {
            responseGroup.m_responseIndex++;
        }
    }

    public override bool DoAlternateMulliganIntro()
    {
        if (!this.ShouldDoAlternateMulliganIntro())
        {
            return false;
        }
        Gameplay.Get().StartCoroutine(this.DoAlternateMulliganIntroWithTiming());
        return true;
    }

    [DebuggerHidden]
    protected IEnumerator DoAlternateMulliganIntroWithTiming()
    {
        return new <DoAlternateMulliganIntroWithTiming>c__IteratorDF();
    }

    [DebuggerHidden]
    protected virtual IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__IteratorDE();
    }

    protected void HandleMissionEvent(int missionEvent)
    {
        Gameplay.Get().StartCoroutine(this.HandleMissionEventWithTiming(missionEvent));
    }

    [DebuggerHidden]
    protected virtual IEnumerator HandleMissionEventWithTiming(int missionEvent)
    {
        return new <HandleMissionEventWithTiming>c__IteratorDA();
    }

    protected virtual void HandleMulliganTagChange()
    {
        MulliganManager.Get().BeginMulligan();
    }

    [DebuggerHidden]
    protected IEnumerator HandlePlayerEmoteWithTiming(EmoteType emoteType, CardSoundSpell emoteSpell)
    {
        return new <HandlePlayerEmoteWithTiming>c__IteratorEE { emoteSpell = emoteSpell, emoteType = emoteType, <$>emoteSpell = emoteSpell, <$>emoteType = emoteType, <>f__this = this };
    }

    protected void HandleStartOfTurn(int turn)
    {
        Gameplay.Get().StartCoroutine(this.HandleStartOfTurnWithTiming(turn));
    }

    [DebuggerHidden]
    protected virtual IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__IteratorD9();
    }

    protected virtual void InitEmoteResponses()
    {
    }

    public override bool IsEnemySpeaking()
    {
        return this.m_enemySpeaking;
    }

    public override void NotifyOfFriendlyPlayedCard(Entity entity)
    {
        base.NotifyOfFriendlyPlayedCard(entity);
        Gameplay.Get().StartCoroutine(this.RespondToFriendlyPlayedCardWithTiming(entity));
    }

    public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
    {
        base.NotifyOfGameOver(gameResult);
        Gameplay.Get().StartCoroutine(this.HandleGameOverWithTiming(gameResult));
    }

    public override void NotifyOfOpponentPlayedCard(Entity entity)
    {
        base.NotifyOfOpponentPlayedCard(entity);
        Gameplay.Get().StartCoroutine(this.RespondToPlayedCardWithTiming(entity));
    }

    public override void NotifyOfOpponentWillPlayCard(string cardId)
    {
        base.NotifyOfOpponentWillPlayCard(cardId);
        Gameplay.Get().StartCoroutine(this.RespondToWillPlayCardWithTiming(cardId));
    }

    public override void NotifyOfStartOfTurnEventsFinished()
    {
        this.HandleStartOfTurn(base.GetTag(GAME_TAG.TURN));
    }

    public override void OnEmotePlayed(Card card, EmoteType emoteType, CardSoundSpell emoteSpell)
    {
        if (card.GetEntity().IsControlledByLocalUser())
        {
            Gameplay.Get().StartCoroutine(this.HandlePlayerEmoteWithTiming(emoteType, emoteSpell));
        }
    }

    public override void OnTagChanged(TagDelta change)
    {
        GAME_TAG tag = (GAME_TAG) change.tag;
        switch (tag)
        {
            case GAME_TAG.MISSION_EVENT:
                this.HandleMissionEvent(change.newValue);
                break;

            case GAME_TAG.STEP:
                if (change.newValue == 4)
                {
                    this.HandleMulliganTagChange();
                }
                else if (((change.oldValue == 9) && (change.newValue == 10)) && !GameState.Get().IsFriendlySidePlayerTurn())
                {
                    this.HandleStartOfTurn(base.GetTag(GAME_TAG.TURN));
                }
                break;

            default:
                if (tag == GAME_TAG.NEXT_STEP)
                {
                    if (change.newValue == 6)
                    {
                        if (GameState.Get().IsMulliganManagerActive())
                        {
                            GameState.Get().SetMulliganPowerBlocker(true);
                        }
                    }
                    else if (((change.oldValue == 9) && (change.newValue == 10)) && GameState.Get().IsFriendlySidePlayerTurn())
                    {
                        TurnStartManager.Get().BeginPlayingTurnEvents();
                    }
                }
                break;
        }
        base.OnTagChanged(change);
    }

    [DebuggerHidden]
    protected IEnumerator PlayBigCharacterQuoteAndWait(string prefabName, string audioName, float testingDuration = 3f, float waitTimeScale = 1f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false)
    {
        return new <PlayBigCharacterQuoteAndWait>c__IteratorE9 { prefabName = prefabName, audioName = audioName, waitTimeScale = waitTimeScale, testingDuration = testingDuration, allowRepeatDuringSession = allowRepeatDuringSession, delayCardSoundSpells = delayCardSoundSpells, <$>prefabName = prefabName, <$>audioName = audioName, <$>waitTimeScale = waitTimeScale, <$>testingDuration = testingDuration, <$>allowRepeatDuringSession = allowRepeatDuringSession, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlayBigCharacterQuoteAndWait(string prefabName, string audioName, string textID, float testingDuration = 3f, float waitTimeScale = 1f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false)
    {
        return new <PlayBigCharacterQuoteAndWait>c__IteratorEA { prefabName = prefabName, audioName = audioName, textID = textID, waitTimeScale = waitTimeScale, testingDuration = testingDuration, allowRepeatDuringSession = allowRepeatDuringSession, delayCardSoundSpells = delayCardSoundSpells, <$>prefabName = prefabName, <$>audioName = audioName, <$>textID = textID, <$>waitTimeScale = waitTimeScale, <$>testingDuration = testingDuration, <$>allowRepeatDuringSession = allowRepeatDuringSession, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlayBigCharacterQuoteAndWaitOnce(string prefabName, string audioName, float testingDuration = 3f, float waitTimeScale = 1f, bool delayCardSoundSpells = false)
    {
        return new <PlayBigCharacterQuoteAndWaitOnce>c__IteratorEB { prefabName = prefabName, audioName = audioName, waitTimeScale = waitTimeScale, testingDuration = testingDuration, delayCardSoundSpells = delayCardSoundSpells, <$>prefabName = prefabName, <$>audioName = audioName, <$>waitTimeScale = waitTimeScale, <$>testingDuration = testingDuration, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlayBigCharacterQuoteAndWaitOnce(string prefabName, string audioName, string textID, float testingDuration = 3f, float waitTimeScale = 1f, bool delayCardSoundSpells = false)
    {
        return new <PlayBigCharacterQuoteAndWaitOnce>c__IteratorEC { prefabName = prefabName, audioName = audioName, textID = textID, waitTimeScale = waitTimeScale, testingDuration = testingDuration, delayCardSoundSpells = delayCardSoundSpells, <$>prefabName = prefabName, <$>audioName = audioName, <$>textID = textID, <$>waitTimeScale = waitTimeScale, <$>testingDuration = testingDuration, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlayCharacterQuoteAndWait(string prefabName, string audioID, float testingDuration = 0f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false)
    {
        return new <PlayCharacterQuoteAndWait>c__IteratorE6 { prefabName = prefabName, audioID = audioID, testingDuration = testingDuration, allowRepeatDuringSession = allowRepeatDuringSession, delayCardSoundSpells = delayCardSoundSpells, <$>prefabName = prefabName, <$>audioID = audioID, <$>testingDuration = testingDuration, <$>allowRepeatDuringSession = allowRepeatDuringSession, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlayCharacterQuoteAndWait(string prefabName, string audioName, string stringName, float testingDuration = 0f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false)
    {
        return new <PlayCharacterQuoteAndWait>c__IteratorE7 { prefabName = prefabName, audioName = audioName, stringName = stringName, testingDuration = testingDuration, allowRepeatDuringSession = allowRepeatDuringSession, delayCardSoundSpells = delayCardSoundSpells, <$>prefabName = prefabName, <$>audioName = audioName, <$>stringName = stringName, <$>testingDuration = testingDuration, <$>allowRepeatDuringSession = allowRepeatDuringSession, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlayCharacterQuoteAndWait(string prefabName, string audioName, string stringName, Vector3 position, float waitTimeScale = 1f, float testingDuration = 0f, bool allowRepeatDuringSession = true, bool delayCardSoundSpells = false, bool isBig = false)
    {
        return new <PlayCharacterQuoteAndWait>c__IteratorE8 { 
            audioName = audioName, testingDuration = testingDuration, waitTimeScale = waitTimeScale, delayCardSoundSpells = delayCardSoundSpells, isBig = isBig, prefabName = prefabName, stringName = stringName, allowRepeatDuringSession = allowRepeatDuringSession, <$>audioName = audioName, <$>testingDuration = testingDuration, <$>waitTimeScale = waitTimeScale, <$>delayCardSoundSpells = delayCardSoundSpells, <$>isBig = isBig, <$>prefabName = prefabName, <$>stringName = stringName, <$>allowRepeatDuringSession = allowRepeatDuringSession, 
            <>f__this = this
         };
    }

    protected virtual void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
    {
        Actor actor = GameState.Get().GetOpposingSidePlayer().GetHeroCard().GetActor();
        foreach (EmoteResponseGroup group in this.m_emoteResponseGroups)
        {
            if ((group.m_responses.Count != 0) && group.m_triggers.Contains(emoteType))
            {
                int responseIndex = group.m_responseIndex;
                EmoteResponse response = group.m_responses[responseIndex];
                Gameplay.Get().StartCoroutine(this.PlaySoundAndBlockSpeech(response.m_soundName, response.m_stringTag, Notification.SpeechBubbleDirection.TopRight, actor, 1f, true, false));
                this.CycleNextResponseGroupIndex(group);
            }
        }
    }

    protected void PlaySound(string audioName, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
    {
        Gameplay.Get().StartCoroutine(this.PlaySoundAndWait(audioName, null, Notification.SpeechBubbleDirection.None, null, waitTimeScale, parentBubbleToActor, delayCardSoundSpells, 3f));
    }

    [DebuggerHidden]
    protected IEnumerator PlaySoundAndBlockSpeech(string audioName, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
    {
        return new <PlaySoundAndBlockSpeech>c__IteratorE0 { audioName = audioName, waitTimeScale = waitTimeScale, parentBubbleToActor = parentBubbleToActor, delayCardSoundSpells = delayCardSoundSpells, <$>audioName = audioName, <$>waitTimeScale = waitTimeScale, <$>parentBubbleToActor = parentBubbleToActor, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlaySoundAndBlockSpeech(string audioID, Notification.SpeechBubbleDirection direction, Actor actor, float testingDuration = 3f, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
    {
        return new <PlaySoundAndBlockSpeech>c__IteratorE2 { audioID = audioID, direction = direction, actor = actor, waitTimeScale = waitTimeScale, parentBubbleToActor = parentBubbleToActor, delayCardSoundSpells = delayCardSoundSpells, testingDuration = testingDuration, <$>audioID = audioID, <$>direction = direction, <$>actor = actor, <$>waitTimeScale = waitTimeScale, <$>parentBubbleToActor = parentBubbleToActor, <$>delayCardSoundSpells = delayCardSoundSpells, <$>testingDuration = testingDuration, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlaySoundAndBlockSpeech(string audioName, string stringName, Notification.SpeechBubbleDirection direction, Actor actor, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
    {
        return new <PlaySoundAndBlockSpeech>c__IteratorE1 { audioName = audioName, stringName = stringName, direction = direction, actor = actor, waitTimeScale = waitTimeScale, parentBubbleToActor = parentBubbleToActor, delayCardSoundSpells = delayCardSoundSpells, <$>audioName = audioName, <$>stringName = stringName, <$>direction = direction, <$>actor = actor, <$>waitTimeScale = waitTimeScale, <$>parentBubbleToActor = parentBubbleToActor, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlaySoundAndBlockSpeechOnce(string audioID, Notification.SpeechBubbleDirection direction, Actor actor, float testingDuration = 3f, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
    {
        return new <PlaySoundAndBlockSpeechOnce>c__IteratorE4 { audioID = audioID, direction = direction, actor = actor, waitTimeScale = waitTimeScale, parentBubbleToActor = parentBubbleToActor, delayCardSoundSpells = delayCardSoundSpells, testingDuration = testingDuration, <$>audioID = audioID, <$>direction = direction, <$>actor = actor, <$>waitTimeScale = waitTimeScale, <$>parentBubbleToActor = parentBubbleToActor, <$>delayCardSoundSpells = delayCardSoundSpells, <$>testingDuration = testingDuration, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlaySoundAndBlockSpeechOnce(string audioName, string stringName, Notification.SpeechBubbleDirection direction, Actor actor, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false)
    {
        return new <PlaySoundAndBlockSpeechOnce>c__IteratorE3 { audioName = audioName, stringName = stringName, direction = direction, actor = actor, waitTimeScale = waitTimeScale, parentBubbleToActor = parentBubbleToActor, delayCardSoundSpells = delayCardSoundSpells, <$>audioName = audioName, <$>stringName = stringName, <$>direction = direction, <$>actor = actor, <$>waitTimeScale = waitTimeScale, <$>parentBubbleToActor = parentBubbleToActor, <$>delayCardSoundSpells = delayCardSoundSpells, <>f__this = this };
    }

    [DebuggerHidden]
    protected IEnumerator PlaySoundAndWait(string audioName, string stringName, Notification.SpeechBubbleDirection direction, Actor actor, float waitTimeScale = 1f, bool parentBubbleToActor = true, bool delayCardSoundSpells = false, float testingDuration = 3f)
    {
        return new <PlaySoundAndWait>c__IteratorE5 { 
            audioName = audioName, testingDuration = testingDuration, waitTimeScale = waitTimeScale, delayCardSoundSpells = delayCardSoundSpells, direction = direction, stringName = stringName, actor = actor, parentBubbleToActor = parentBubbleToActor, <$>audioName = audioName, <$>testingDuration = testingDuration, <$>waitTimeScale = waitTimeScale, <$>delayCardSoundSpells = delayCardSoundSpells, <$>direction = direction, <$>stringName = stringName, <$>actor = actor, <$>parentBubbleToActor = parentBubbleToActor, 
            <>f__this = this
         };
    }

    [DebuggerHidden]
    protected virtual IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToFriendlyPlayedCardWithTiming>c__IteratorDD();
    }

    [DebuggerHidden]
    protected virtual IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__IteratorDC();
    }

    [DebuggerHidden]
    protected virtual IEnumerator RespondToWillPlayCardWithTiming(string cardId)
    {
        return new <RespondToWillPlayCardWithTiming>c__IteratorDB();
    }

    public override void SendCustomEvent(int eventID)
    {
        this.HandleMissionEvent(eventID);
    }

    public override bool ShouldDelayCardSoundSpells()
    {
        return this.m_delayCardSoundSpells;
    }

    public override bool ShouldUseSecretClassNames()
    {
        return true;
    }

    protected void ShowBubble(string textKey, Notification.SpeechBubbleDirection direction, Actor speakingActor, bool destroyOnNewNotification, float duration, bool parentToActor)
    {
        Notification notification;
        NotificationManager manager = NotificationManager.Get();
        if ((SoundUtils.CanDetectVolume() && SoundUtils.IsVoiceAudible()) && GameMgr.Get().IsTutorial())
        {
            notification = manager.CreateSpeechBubble(string.Empty, direction, speakingActor, destroyOnNewNotification, parentToActor);
            float num = 0.25f;
            Vector3 vector = new Vector3(notification.transform.localScale.x * num, notification.transform.localScale.y * num, notification.transform.localScale.z * num);
            notification.transform.localScale = vector;
        }
        else
        {
            notification = manager.CreateSpeechBubble(GameStrings.Get(textKey), direction, speakingActor, destroyOnNewNotification, parentToActor);
        }
        if (duration > 0f)
        {
            manager.DestroyNotification(notification, duration);
        }
    }

    [DebuggerHidden]
    protected IEnumerator WaitForCardSoundSpellDelay(float sec)
    {
        return new <WaitForCardSoundSpellDelay>c__IteratorED { sec = sec, <$>sec = sec, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <DoAlternateMulliganIntroWithTiming>c__IteratorDF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
                    Board.Get().RaiseTheLights();
                    SceneMgr.Get().NotifySceneLoaded();
                    break;

                case 1:
                    break;

                default:
                    goto Label_0086;
            }
            if (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            GameMgr.Get().UpdatePresence();
            MulliganManager.Get().SkipMulligan();
            this.$PC = -1;
        Label_0086:
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
    private sealed class <HandleGameOverWithTiming>c__IteratorDE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
    private sealed class <HandleMissionEventWithTiming>c__IteratorDA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
    private sealed class <HandlePlayerEmoteWithTiming>c__IteratorEE : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal CardSoundSpell <$>emoteSpell;
        internal EmoteType <$>emoteType;
        internal MissionEntity <>f__this;
        internal CardSoundSpell emoteSpell;
        internal EmoteType emoteType;

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
                    if (this.emoteSpell.IsActive())
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (!this.<>f__this.m_enemySpeaking)
                    {
                        this.<>f__this.PlayEmoteResponse(this.emoteType, this.emoteSpell);
                        this.$PC = -1;
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
    private sealed class <HandleStartOfTurnWithTiming>c__IteratorD9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
    private sealed class <PlayBigCharacterQuoteAndWait>c__IteratorE9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>allowRepeatDuringSession;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal string <$>prefabName;
        internal float <$>testingDuration;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal bool allowRepeatDuringSession;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal string prefabName;
        internal float testingDuration;
        internal float waitTimeScale;

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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait(this.prefabName, this.audioName, this.audioName, Vector3.zero, this.waitTimeScale, this.testingDuration, this.allowRepeatDuringSession, this.delayCardSoundSpells, true));
                    this.$PC = 1;
                    return true;

                case 1:
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
    private sealed class <PlayBigCharacterQuoteAndWait>c__IteratorEA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>allowRepeatDuringSession;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal string <$>prefabName;
        internal float <$>testingDuration;
        internal string <$>textID;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal bool allowRepeatDuringSession;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal string prefabName;
        internal float testingDuration;
        internal string textID;
        internal float waitTimeScale;

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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait(this.prefabName, this.audioName, this.textID, Vector3.zero, this.waitTimeScale, this.testingDuration, this.allowRepeatDuringSession, this.delayCardSoundSpells, true));
                    this.$PC = 1;
                    return true;

                case 1:
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
    private sealed class <PlayBigCharacterQuoteAndWaitOnce>c__IteratorEB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal string <$>prefabName;
        internal float <$>testingDuration;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal bool <allowRepeat>__0;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal string prefabName;
        internal float testingDuration;
        internal float waitTimeScale;

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
                    this.<allowRepeat>__0 = DemoMgr.Get().IsExpoDemo();
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait(this.prefabName, this.audioName, this.audioName, Vector3.zero, this.waitTimeScale, this.testingDuration, this.<allowRepeat>__0, this.delayCardSoundSpells, true));
                    this.$PC = 1;
                    return true;

                case 1:
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
    private sealed class <PlayBigCharacterQuoteAndWaitOnce>c__IteratorEC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal string <$>prefabName;
        internal float <$>testingDuration;
        internal string <$>textID;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal bool <allowRepeat>__0;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal string prefabName;
        internal float testingDuration;
        internal string textID;
        internal float waitTimeScale;

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
                    this.<allowRepeat>__0 = DemoMgr.Get().IsExpoDemo();
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait(this.prefabName, this.audioName, this.textID, Vector3.zero, this.waitTimeScale, this.testingDuration, this.<allowRepeat>__0, this.delayCardSoundSpells, true));
                    this.$PC = 1;
                    return true;

                case 1:
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
    private sealed class <PlayCharacterQuoteAndWait>c__IteratorE6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>allowRepeatDuringSession;
        internal string <$>audioID;
        internal bool <$>delayCardSoundSpells;
        internal string <$>prefabName;
        internal float <$>testingDuration;
        internal MissionEntity <>f__this;
        internal bool allowRepeatDuringSession;
        internal string audioID;
        internal bool delayCardSoundSpells;
        internal string prefabName;
        internal float testingDuration;

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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait(this.prefabName, this.audioID, this.audioID, NotificationManager.DEFAULT_CHARACTER_POS, 1f, this.testingDuration, this.allowRepeatDuringSession, this.delayCardSoundSpells, false));
                    this.$PC = 1;
                    return true;

                case 1:
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
    private sealed class <PlayCharacterQuoteAndWait>c__IteratorE7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>allowRepeatDuringSession;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal string <$>prefabName;
        internal string <$>stringName;
        internal float <$>testingDuration;
        internal MissionEntity <>f__this;
        internal bool allowRepeatDuringSession;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal string prefabName;
        internal string stringName;
        internal float testingDuration;

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
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait(this.prefabName, this.audioName, this.stringName, NotificationManager.DEFAULT_CHARACTER_POS, 1f, this.testingDuration, this.allowRepeatDuringSession, this.delayCardSoundSpells, false));
                    this.$PC = 1;
                    return true;

                case 1:
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
    private sealed class <PlayCharacterQuoteAndWait>c__IteratorE8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>allowRepeatDuringSession;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal bool <$>isBig;
        internal string <$>prefabName;
        internal string <$>stringName;
        internal float <$>testingDuration;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal float <clipLength>__2;
        internal bool <isJustTesting>__1;
        internal AudioSource <sound>__0;
        internal float <waitTime>__3;
        internal bool allowRepeatDuringSession;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal bool isBig;
        internal string prefabName;
        internal string stringName;
        internal float testingDuration;
        internal float waitTimeScale;

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
                    this.<sound>__0 = null;
                    this.<isJustTesting>__1 = false;
                    if (!string.IsNullOrEmpty(this.audioName) && this.<>f__this.CheckPreloadedSound(this.audioName))
                    {
                        this.<sound>__0 = this.<>f__this.GetPreloadedSound(this.audioName);
                        break;
                    }
                    this.<isJustTesting>__1 = true;
                    break;

                case 1:
                    goto Label_00FE;

                case 2:
                    NotificationManager.Get().DestroyActiveQuote(0f);
                    this.$PC = -1;
                    goto Label_0286;

                default:
                    goto Label_0286;
            }
            if (this.<isJustTesting>__1 || ((this.<sound>__0 != null) && (this.<sound>__0.clip != null)))
            {
                goto Label_0166;
            }
            if (!this.<>f__this.CheckPreloadedSound(this.audioName))
            {
                goto Label_0125;
            }
            this.<>f__this.RemovePreloadedSound(this.audioName);
            this.<>f__this.PreloadSound(this.audioName);
        Label_00FE:
            while (this.<>f__this.IsPreloadingAssets())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0288;
            }
            this.<sound>__0 = this.<>f__this.GetPreloadedSound(this.audioName);
        Label_0125:
            if ((this.<sound>__0 == null) || (this.<sound>__0.clip == null))
            {
                UnityEngine.Debug.Log("MissionEntity.PlaySoundAndWait() - sound error - " + this.audioName);
                goto Label_0286;
            }
        Label_0166:
            if (this.<isJustTesting>__1)
            {
                this.<clipLength>__2 = this.testingDuration;
            }
            else
            {
                this.<clipLength>__2 = this.<sound>__0.clip.length;
            }
            this.<waitTime>__3 = this.<clipLength>__2 * this.waitTimeScale;
            if (this.delayCardSoundSpells)
            {
                Gameplay.Get().StartCoroutine(this.<>f__this.WaitForCardSoundSpellDelay(this.<clipLength>__2));
            }
            if (this.isBig)
            {
                NotificationManager.Get().CreateBigCharacterQuote(this.prefabName, this.audioName, this.stringName, this.allowRepeatDuringSession, this.testingDuration, null);
            }
            else
            {
                NotificationManager.Get().CreateCharacterQuote(this.prefabName, GameStrings.Get(this.stringName), this.audioName, this.allowRepeatDuringSession, this.testingDuration * 2f, CanvasAnchor.BOTTOM_LEFT);
            }
            this.<waitTime>__3 += 0.5f;
            this.$current = new WaitForSeconds(this.<waitTime>__3);
            this.$PC = 2;
            goto Label_0288;
        Label_0286:
            return false;
        Label_0288:
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
    private sealed class <PlaySoundAndBlockSpeech>c__IteratorE0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal bool <$>parentBubbleToActor;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal bool parentBubbleToActor;
        internal float waitTimeScale;

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
                    this.<>f__this.m_enemySpeaking = true;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait(this.audioName, null, Notification.SpeechBubbleDirection.None, null, this.waitTimeScale, this.parentBubbleToActor, this.delayCardSoundSpells, 3f));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_enemySpeaking = false;
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
    private sealed class <PlaySoundAndBlockSpeech>c__IteratorE1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection <$>direction;
        internal bool <$>parentBubbleToActor;
        internal string <$>stringName;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal Actor actor;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection direction;
        internal bool parentBubbleToActor;
        internal string stringName;
        internal float waitTimeScale;

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
                    this.<>f__this.m_enemySpeaking = true;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait(this.audioName, this.stringName, this.direction, this.actor, this.waitTimeScale, this.parentBubbleToActor, this.delayCardSoundSpells, 3f));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_enemySpeaking = false;
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
    private sealed class <PlaySoundAndBlockSpeech>c__IteratorE2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal string <$>audioID;
        internal bool <$>delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection <$>direction;
        internal bool <$>parentBubbleToActor;
        internal float <$>testingDuration;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal Actor actor;
        internal string audioID;
        internal bool delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection direction;
        internal bool parentBubbleToActor;
        internal float testingDuration;
        internal float waitTimeScale;

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
                    this.<>f__this.m_enemySpeaking = true;
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait(this.audioID, this.audioID, this.direction, this.actor, this.waitTimeScale, this.parentBubbleToActor, this.delayCardSoundSpells, this.testingDuration));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_enemySpeaking = false;
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
    private sealed class <PlaySoundAndBlockSpeechOnce>c__IteratorE3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection <$>direction;
        internal bool <$>parentBubbleToActor;
        internal string <$>stringName;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal Actor actor;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection direction;
        internal bool parentBubbleToActor;
        internal string stringName;
        internal float waitTimeScale;

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
                    if (!NotificationManager.Get().HasSoundPlayedThisSession(this.audioName))
                    {
                        NotificationManager.Get().ForceAddSoundToPlayedList(this.audioName);
                        this.<>f__this.m_enemySpeaking = true;
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait(this.audioName, this.stringName, this.direction, this.actor, this.waitTimeScale, this.parentBubbleToActor, this.delayCardSoundSpells, 3f));
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    this.<>f__this.m_enemySpeaking = false;
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
    private sealed class <PlaySoundAndBlockSpeechOnce>c__IteratorE4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal string <$>audioID;
        internal bool <$>delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection <$>direction;
        internal bool <$>parentBubbleToActor;
        internal float <$>testingDuration;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal Actor actor;
        internal string audioID;
        internal bool delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection direction;
        internal bool parentBubbleToActor;
        internal float testingDuration;
        internal float waitTimeScale;

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
                    if (!NotificationManager.Get().HasSoundPlayedThisSession(this.audioID))
                    {
                        NotificationManager.Get().ForceAddSoundToPlayedList(this.audioID);
                        this.<>f__this.m_enemySpeaking = true;
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndWait(this.audioID, this.audioID, this.direction, this.actor, this.waitTimeScale, this.parentBubbleToActor, this.delayCardSoundSpells, this.testingDuration));
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    this.<>f__this.m_enemySpeaking = false;
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
    private sealed class <PlaySoundAndWait>c__IteratorE5 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Actor <$>actor;
        internal string <$>audioName;
        internal bool <$>delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection <$>direction;
        internal bool <$>parentBubbleToActor;
        internal string <$>stringName;
        internal float <$>testingDuration;
        internal float <$>waitTimeScale;
        internal MissionEntity <>f__this;
        internal float <clipLength>__2;
        internal bool <isJustTesting>__1;
        internal AudioSource <sound>__0;
        internal float <waitTime>__3;
        internal Actor actor;
        internal string audioName;
        internal bool delayCardSoundSpells;
        internal Notification.SpeechBubbleDirection direction;
        internal bool parentBubbleToActor;
        internal string stringName;
        internal float testingDuration;
        internal float waitTimeScale;

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
                    this.<sound>__0 = null;
                    this.<isJustTesting>__1 = false;
                    if (!string.IsNullOrEmpty(this.audioName) && this.<>f__this.CheckPreloadedSound(this.audioName))
                    {
                        this.<sound>__0 = this.<>f__this.GetPreloadedSound(this.audioName);
                        break;
                    }
                    this.<isJustTesting>__1 = true;
                    break;

                case 1:
                    goto Label_00FE;

                case 2:
                    this.$PC = -1;
                    goto Label_0253;

                default:
                    goto Label_0253;
            }
            if (this.<isJustTesting>__1 || ((this.<sound>__0 != null) && (this.<sound>__0.clip != null)))
            {
                goto Label_0166;
            }
            if (!this.<>f__this.CheckPreloadedSound(this.audioName))
            {
                goto Label_0125;
            }
            this.<>f__this.RemovePreloadedSound(this.audioName);
            this.<>f__this.PreloadSound(this.audioName);
        Label_00FE:
            while (this.<>f__this.IsPreloadingAssets())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0255;
            }
            this.<sound>__0 = this.<>f__this.GetPreloadedSound(this.audioName);
        Label_0125:
            if ((this.<sound>__0 == null) || (this.<sound>__0.clip == null))
            {
                UnityEngine.Debug.Log("MissionEntity.PlaySoundAndWait() - sound error - " + this.audioName);
                goto Label_0253;
            }
        Label_0166:
            this.<clipLength>__2 = this.testingDuration;
            if (!this.<isJustTesting>__1)
            {
                this.<clipLength>__2 = this.<sound>__0.clip.length;
            }
            this.<waitTime>__3 = this.<clipLength>__2 * this.waitTimeScale;
            if (!this.<isJustTesting>__1)
            {
                SoundManager.Get().PlayPreloaded(this.<sound>__0);
            }
            if (this.delayCardSoundSpells)
            {
                Gameplay.Get().StartCoroutine(this.<>f__this.WaitForCardSoundSpellDelay(this.<clipLength>__2));
            }
            if (this.direction != Notification.SpeechBubbleDirection.None)
            {
                this.<>f__this.ShowBubble(this.stringName, this.direction, this.actor, false, this.<clipLength>__2, this.parentBubbleToActor);
                this.<waitTime>__3 += 0.5f;
            }
            this.$current = new WaitForSeconds(this.<waitTime>__3);
            this.$PC = 2;
            goto Label_0255;
        Label_0253:
            return false;
        Label_0255:
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
    private sealed class <RespondToFriendlyPlayedCardWithTiming>c__IteratorDD : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
    private sealed class <RespondToPlayedCardWithTiming>c__IteratorDC : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
    private sealed class <RespondToWillPlayCardWithTiming>c__IteratorDB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;

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
    private sealed class <WaitForCardSoundSpellDelay>c__IteratorED : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>sec;
        internal MissionEntity <>f__this;
        internal float sec;

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
                    this.<>f__this.m_delayCardSoundSpells = true;
                    this.$current = new WaitForSeconds(this.sec);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_delayCardSoundSpells = false;
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

    protected class EmoteResponse
    {
        public string m_soundName;
        public string m_stringTag;
    }

    protected class EmoteResponseGroup
    {
        public int m_responseIndex;
        public List<MissionEntity.EmoteResponse> m_responses = new List<MissionEntity.EmoteResponse>();
        public List<EmoteType> m_triggers = new List<EmoteType>();
    }
}

