using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LOE15_Boss1 : LOE_MissionEntity
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map18;
    private bool m_lowHealth;
    private bool m_magmaRagerLinePlayed;
    private List<Zone> m_zonesToHide = new List<Zone>();

    [DebuggerHidden]
    public override IEnumerator DoActionsAfterIntroBeforeMulligan()
    {
        return new <DoActionsAfterIntroBeforeMulligan>c__Iterator14A { <>f__this = this };
    }

    public override bool DoAlternateMulliganIntro()
    {
        GameState.Get().GetOpposingSidePlayer().GetDeckZone().SetVisibility(false);
        this.m_zonesToHide.Clear();
        this.m_zonesToHide.AddRange(ZoneMgr.Get().FindZonesForTag(TAG_ZONE.HAND));
        this.m_zonesToHide.AddRange(ZoneMgr.Get().FindZonesForTag(TAG_ZONE.DECK));
        foreach (Zone zone in this.m_zonesToHide)
        {
            Log.JMac.Print(string.Concat(new object[] { "Number of cards in zone ", zone, ": ", zone.GetCards().Count }), new object[0]);
            foreach (Card card in zone.GetCards())
            {
                card.HideCard();
                card.SetDoNotSort(true);
            }
        }
        return false;
    }

    [DebuggerHidden]
    protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
    {
        return new <HandleGameOverWithTiming>c__Iterator14D { gameResult = gameResult, <$>gameResult = gameResult, <>f__this = this };
    }

    [DebuggerHidden]
    protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
    {
        return new <HandleStartOfTurnWithTiming>c__Iterator14B { turn = turn, <$>turn = turn, <>f__this = this };
    }

    protected override void InitEmoteResponses()
    {
        List<MissionEntity.EmoteResponseGroup> list = new List<MissionEntity.EmoteResponseGroup>();
        MissionEntity.EmoteResponseGroup item = new MissionEntity.EmoteResponseGroup {
            m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS)
        };
        List<MissionEntity.EmoteResponse> list2 = new List<MissionEntity.EmoteResponse>();
        MissionEntity.EmoteResponse response = new MissionEntity.EmoteResponse {
            m_soundName = "VO_LOE_15_RESPONSE",
            m_stringTag = "VO_LOE_15_RESPONSE"
        };
        list2.Add(response);
        item.m_responses = list2;
        list.Add(item);
        base.m_emoteResponseGroups = list;
    }

    public override void PreloadAssets()
    {
        base.PreloadSound("VO_LOE_15_RESPONSE");
        base.PreloadSound("VO_LOEA15_1_LOW_HEALTH_10");
        base.PreloadSound("VO_LOEA15_1_TURN1_08");
        base.PreloadSound("VO_LOEA15_1_MAGMA_RAGER_09");
        base.PreloadSound("VO_LOEA15_1_LOSS_11");
        base.PreloadSound("VO_LOEA15_1_WIN_12");
        base.PreloadSound("VO_LOEA15_GOLDEN");
        base.PreloadSound("VO_LOEA15_1_START_07");
        base.PreloadSound("VO_LOE_15_SPARE");
        base.PreloadSound("VO_ELISE_WEIRD_DECK_05");
    }

    [DebuggerHidden]
    protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
    {
        return new <RespondToPlayedCardWithTiming>c__Iterator14C { entity = entity, <$>entity = entity, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <DoActionsAfterIntroBeforeMulligan>c__Iterator14A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Zone>.Enumerator <$s_705>__6;
        internal List<Card>.Enumerator <$s_706>__8;
        internal LOE15_Boss1 <>f__this;
        internal Card <card>__9;
        internal LOE_DeckTakeEvent <deckTakeEvent>__3;
        internal Actor <enemyActor>__1;
        internal Player <enemyPlayer>__0;
        internal GameObject <go>__2;
        internal ZoneDeck <opponentZoneDeck>__4;
        internal Zone <zone>__7;
        internal ZoneDeck <zoneDeck>__5;

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
                    Log.JMac.PrintWarning("Start DoPostIntroActions in LOE15_Boss1!", new object[0]);
                    this.<enemyPlayer>__0 = GameState.Get().GetOpposingSidePlayer();
                    this.<enemyActor>__1 = this.<enemyPlayer>__0.GetHeroCard().GetActor();
                    this.<go>__2 = AssetLoader.Get().LoadGameObject("LOE_DeckTakeEvent", true, false);
                    this.<deckTakeEvent>__3 = this.<go>__2.GetComponent<LOE_DeckTakeEvent>();
                    this.$current = new WaitForEndOfFrame();
                    this.$PC = 1;
                    goto Label_0313;

                case 1:
                    this.<opponentZoneDeck>__4 = GameState.Get().GetOpposingSidePlayer().GetDeckZone();
                    this.<opponentZoneDeck>__4.SetVisibility(true);
                    Gameplay.Get().SwapCardBacks();
                    this.<opponentZoneDeck>__4.SetVisibility(false);
                    this.<zoneDeck>__5 = GameState.Get().GetFriendlySidePlayer().GetDeckZone();
                    this.<zoneDeck>__5.SetVisibility(false);
                    Gameplay.Get().StartCoroutine(this.<deckTakeEvent>__3.PlayTakeDeckAnim());
                    this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeech("VO_LOEA15_1_START_07", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 3f, 1f, true, false));
                    this.$PC = 2;
                    goto Label_0313;

                case 2:
                case 3:
                    if (this.<deckTakeEvent>__3.AnimIsPlaying())
                    {
                        Log.JMac.Print("Waiting for take deck anim to finish.", new object[0]);
                        this.$current = null;
                        this.$PC = 3;
                    }
                    else
                    {
                        Gameplay.Get().StartCoroutine(this.<deckTakeEvent>__3.PlayReplacementDeckAnim());
                        this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWait("Elise_BigQuote", "VO_LOE_15_SPARE", 3f, 1f, true, false));
                        this.$PC = 4;
                    }
                    goto Label_0313;

                case 4:
                case 5:
                    while (this.<deckTakeEvent>__3.AnimIsPlaying())
                    {
                        Log.JMac.Print("Waiting for replacement deck anim to finish.", new object[0]);
                        this.$current = null;
                        this.$PC = 5;
                        goto Label_0313;
                    }
                    this.<$s_705>__6 = this.<>f__this.m_zonesToHide.GetEnumerator();
                    try
                    {
                        while (this.<$s_705>__6.MoveNext())
                        {
                            this.<zone>__7 = this.<$s_705>__6.Current;
                            this.<$s_706>__8 = this.<zone>__7.GetCards().GetEnumerator();
                            try
                            {
                                while (this.<$s_706>__8.MoveNext())
                                {
                                    this.<card>__9 = this.<$s_706>__8.Current;
                                    this.<card>__9.ShowCard();
                                    this.<card>__9.SetDoNotSort(false);
                                }
                                continue;
                            }
                            finally
                            {
                                this.<$s_706>__8.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_705>__6.Dispose();
                    }
                    this.<>f__this.m_zonesToHide.Clear();
                    Log.JMac.PrintWarning("End DoPostIntroActions in LOE15_Boss1!", new object[0]);
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0313:
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
    private sealed class <HandleGameOverWithTiming>c__Iterator14D : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TAG_PLAYSTATE <$>gameResult;
        internal LOE15_Boss1 <>f__this;
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
                    if (!GameMgr.Get().IsClassChallengeMission())
                    {
                        if (this.gameResult == TAG_PLAYSTATE.WON)
                        {
                            this.$current = new WaitForSeconds(5f);
                            this.$PC = 1;
                            goto Label_00F9;
                        }
                        if (this.gameResult == TAG_PLAYSTATE.LOST)
                        {
                            this.$current = new WaitForSeconds(5f);
                            this.$PC = 2;
                            goto Label_00F9;
                            this.$PC = -1;
                        }
                        break;
                    }
                    break;

                case 1:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Rafaam_wrap_Quote", "VO_LOEA15_1_LOSS_11", "VO_LOEA15_1_LOSS_11", 0f, false, false));
                    break;

                case 2:
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlayCharacterQuoteAndWait("Rafaam_wrap_Quote", "VO_LOEA15_1_WIN_12", "VO_LOEA15_1_WIN_12", 0f, false, false));
                    break;
            }
            return false;
        Label_00F9:
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
    private sealed class <HandleStartOfTurnWithTiming>c__Iterator14B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal int <$>turn;
        internal LOE15_Boss1 <>f__this;
        internal Actor <enemyActor>__1;
        internal Player <enemyPlayer>__0;
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
                case 1:
                    if (this.<>f__this.m_enemySpeaking)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_01D8;
                    }
                    this.<enemyPlayer>__0 = GameState.Get().GetOpposingSidePlayer();
                    this.<enemyActor>__1 = this.<enemyPlayer>__0.GetHeroCard().GetActor();
                    if ((!this.<>f__this.m_lowHealth && (this.<enemyPlayer>__0.GetHero().GetRemainingHP() < 10)) && (GameState.Get().GetFriendlySidePlayer().GetHero().GetRemainingHP() > 0x13))
                    {
                        this.<>f__this.m_lowHealth = true;
                        Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA15_1_LOW_HEALTH_10", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 3f, 1f, true, false));
                    }
                    else
                    {
                        switch (this.turn)
                        {
                            case 1:
                                if (GameState.Get().GetGameEntity().GetCost() == 1)
                                {
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA15_GOLDEN", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 3f, 1f, true, false));
                                }
                                else
                                {
                                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA15_1_TURN1_08", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__1, 3f, 1f, true, false));
                                }
                                break;

                            case 5:
                                this.$current = Gameplay.Get().StartCoroutine(this.<>f__this.PlayBigCharacterQuoteAndWaitOnce("Elise_BigQuote", "VO_ELISE_WEIRD_DECK_05", 3f, 1f, false));
                                this.$PC = 2;
                                goto Label_01D8;
                        }
                    }
                    break;
            }
            goto Label_01D6;
            this.$PC = -1;
        Label_01D6:
            return false;
        Label_01D8:
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
    private sealed class <RespondToPlayedCardWithTiming>c__Iterator14C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Entity <$>entity;
        internal LOE15_Boss1 <>f__this;
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
                if (LOE15_Boss1.<>f__switch$map18 == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(1);
                    dictionary.Add("CS2_118", 0);
                    LOE15_Boss1.<>f__switch$map18 = dictionary;
                }
                if (LOE15_Boss1.<>f__switch$map18.TryGetValue(cardId, out num2) && (num2 == 0))
                {
                    if (this.<>f__this.m_magmaRagerLinePlayed)
                    {
                        goto Label_0141;
                    }
                    this.<>f__this.m_magmaRagerLinePlayed = true;
                    Gameplay.Get().StartCoroutine(this.<>f__this.PlaySoundAndBlockSpeechOnce("VO_LOEA15_1_MAGMA_RAGER_09", Notification.SpeechBubbleDirection.TopRight, this.<enemyActor>__0, 3f, 1f, true, false));
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

