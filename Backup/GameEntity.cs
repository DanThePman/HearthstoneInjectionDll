using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameEntity : Entity
{
    private Map<string, AudioSource> m_preloadedSounds = new Map<string, AudioSource>();
    private int m_preloadsNeeded;

    public GameEntity()
    {
        this.PreloadAssets();
    }

    public virtual bool AreTooltipsDisabled()
    {
        return false;
    }

    protected virtual Spell BlowUpHero(Card card, SpellType spellType)
    {
        Spell spell = card.ActivateActorSpell(spellType);
        Gameplay.Get().StartCoroutine(this.HideOtherElements(card));
        return spell;
    }

    public bool CheckPreloadedSound(string name)
    {
        AudioSource source;
        return this.m_preloadedSounds.TryGetValue(name, out source);
    }

    public virtual string CustomChoiceBannerText()
    {
        return null;
    }

    [DebuggerHidden]
    public virtual IEnumerator DoActionsAfterIntroBeforeMulligan()
    {
        return new <DoActionsAfterIntroBeforeMulligan>c__Iterator8F();
    }

    public virtual bool DoAlternateMulliganIntro()
    {
        return false;
    }

    private void EmoteHandlerDoneLoadingCallback(string actorName, GameObject actorObject, object callbackData)
    {
        actorObject.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.FRIENDLY).transform.position;
    }

    private void EnemyEmoteHandlerDoneLoadingCallback(string actorName, GameObject actorObject, object callbackData)
    {
        actorObject.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING).transform.position;
    }

    public void FadeInActor(Actor actorToFade)
    {
        this.FadeInActor(actorToFade, 0f);
    }

    public void FadeInActor(Actor actorToFade, float lightBlendAmount)
    {
        <FadeInActor>c__AnonStorey2F8 storeyf = new <FadeInActor>c__AnonStorey2F8 {
            mat = actorToFade.m_portraitMesh.GetComponent<Renderer>().materials[actorToFade.m_portraitMatIdx],
            frameMat = actorToFade.m_portraitMesh.GetComponent<Renderer>().materials[actorToFade.m_portraitFrameMatIdx]
        };
        float @float = storeyf.mat.GetFloat("_LightingBlend");
        Action<object> action = new Action<object>(storeyf.<>m__F7);
        object[] args = new object[] { "time", 0.25f, "from", @float, "to", lightBlendAmount, "onupdate", action, "onupdatetarget", actorToFade.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(actorToFade.gameObject, hashtable);
    }

    public void FadeInHeroActor(Actor actorToFade)
    {
        this.FadeInHeroActor(actorToFade, 0f);
    }

    public void FadeInHeroActor(Actor actorToFade, float lightBlendAmount)
    {
        <FadeInHeroActor>c__AnonStorey2F7 storeyf = new <FadeInHeroActor>c__AnonStorey2F7();
        this.ToggleSpotLight(actorToFade.GetHeroSpotlight(), true);
        storeyf.heroMat = actorToFade.m_portraitMesh.GetComponent<Renderer>().materials[actorToFade.m_portraitMatIdx];
        storeyf.heroFrameMat = actorToFade.m_portraitMesh.GetComponent<Renderer>().materials[actorToFade.m_portraitFrameMatIdx];
        float @float = storeyf.heroMat.GetFloat("_LightingBlend");
        Action<object> action = new Action<object>(storeyf.<>m__F6);
        object[] args = new object[] { "time", 0.25f, "from", @float, "to", lightBlendAmount, "onupdate", action, "onupdatetarget", actorToFade.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(actorToFade.gameObject, hashtable);
    }

    public void FadeOutActor(Actor actorToFade)
    {
        <FadeOutActor>c__AnonStorey2F5 storeyf = new <FadeOutActor>c__AnonStorey2F5 {
            mat = actorToFade.m_portraitMesh.GetComponent<Renderer>().materials[actorToFade.m_portraitMatIdx],
            frameMat = actorToFade.m_portraitMesh.GetComponent<Renderer>().materials[actorToFade.m_portraitFrameMatIdx]
        };
        float @float = storeyf.mat.GetFloat("_LightingBlend");
        Action<object> action = new Action<object>(storeyf.<>m__F3);
        object[] args = new object[] { "time", 0.25f, "from", @float, "to", 1f, "onupdate", action, "onupdatetarget", actorToFade.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(actorToFade.gameObject, hashtable);
    }

    public void FadeOutHeroActor(Actor actorToFade)
    {
        <FadeOutHeroActor>c__AnonStorey2F4 storeyf = new <FadeOutHeroActor>c__AnonStorey2F4();
        this.ToggleSpotLight(actorToFade.GetHeroSpotlight(), false);
        storeyf.heroMat = actorToFade.m_portraitMesh.GetComponent<Renderer>().materials[actorToFade.m_portraitMatIdx];
        storeyf.heroFrameMat = actorToFade.m_portraitMesh.GetComponent<Renderer>().materials[actorToFade.m_portraitFrameMatIdx];
        float @float = storeyf.heroMat.GetFloat("_LightingBlend");
        Action<object> action = new Action<object>(storeyf.<>m__F2);
        object[] args = new object[] { "time", 0.25f, "from", @float, "to", 1f, "onupdate", action, "onupdatetarget", actorToFade.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(actorToFade.gameObject, hashtable);
    }

    public virtual float GetAdditionalTimeToWaitForSpells()
    {
        return 0f;
    }

    public virtual string GetAlternatePlayerName()
    {
        return string.Empty;
    }

    public virtual List<RewardData> GetCustomRewards()
    {
        return null;
    }

    public override string GetDebugName()
    {
        return "GameEntity";
    }

    public override string GetName()
    {
        return "GameEntity";
    }

    public AudioSource GetPreloadedSound(string name)
    {
        AudioSource source;
        if (this.m_preloadedSounds.TryGetValue(name, out source))
        {
            return source;
        }
        UnityEngine.Debug.LogError(string.Format("GameEntity.GetPreloadedSound() - \"{0}\" was not preloaded", name));
        return null;
    }

    public virtual string GetTurnStartReminderText()
    {
        return string.Empty;
    }

    public virtual void HandleRealTimeMissionEvent(int missionEvent)
    {
    }

    [DebuggerHidden]
    protected IEnumerator HideOtherElements(Card card)
    {
        return new <HideOtherElements>c__Iterator90 { card = card, <$>card = card };
    }

    public virtual bool IsEnemySpeaking()
    {
        return false;
    }

    public virtual bool IsKeywordHelpDelayOverridden()
    {
        return false;
    }

    public virtual bool IsMouseOverDelayOverriden()
    {
        return false;
    }

    public bool IsPreloadingAssets()
    {
        return (this.m_preloadsNeeded > 0);
    }

    public virtual bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
    {
        return true;
    }

    public virtual void NotifyOfCardDropped(Entity entity)
    {
    }

    public virtual void NotifyOfCardGrabbed(Entity entity)
    {
    }

    public virtual void NotifyOfCardMousedOff(Entity mousedOffEntity)
    {
    }

    public virtual void NotifyOfCardMousedOver(Entity mousedOverEntity)
    {
    }

    public virtual void NotifyOfCardTooltipDisplayHide(Card card)
    {
    }

    public virtual bool NotifyOfCardTooltipDisplayShow(Card card)
    {
        return true;
    }

    public virtual void NotifyOfCoinFlipResult()
    {
    }

    public virtual void NotifyOfCustomIntroFinished()
    {
    }

    public virtual void NotifyOfDebugCommand(int command)
    {
    }

    public virtual void NotifyOfDefeatCoinAnimation()
    {
    }

    public virtual bool NotifyOfEndTurnButtonPushed()
    {
        return true;
    }

    public virtual void NotifyOfEnemyManaCrystalSpawned()
    {
    }

    public virtual void NotifyOfFriendlyPlayedCard(Entity entity)
    {
    }

    public virtual void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
    {
        Spell spell;
        Spell spell2;
        PegCursor.Get().SetMode(PegCursor.Mode.STOPWAITING);
        MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_EndGameScreen);
        this.PlayHeroBlowUpSpells(gameResult, out spell, out spell2);
        this.ShowEndScreen(gameResult, spell, spell2);
    }

    public virtual void NotifyOfGamePackOpened()
    {
    }

    public virtual void NotifyOfHelpPanelDisplay(int numPanels)
    {
    }

    public virtual void NotifyOfHeroesFinishedAnimatingInMulligan()
    {
    }

    public virtual void NotifyOfHistoryTokenMousedOut()
    {
    }

    public virtual void NotifyOfHistoryTokenMousedOver(GameObject mousedOverTile)
    {
    }

    public virtual string[] NotifyOfKeywordHelpPanelDisplay(Entity entity)
    {
        return null;
    }

    public virtual void NotifyOfManaCrystalSpawned()
    {
    }

    public virtual void NotifyOfMulliganEnded()
    {
    }

    public virtual void NotifyOfMulliganInitialized()
    {
        if (!GameMgr.Get().IsTutorial())
        {
            AssetLoader.Get().LoadActor("EmoteHandler", new AssetLoader.GameObjectCallback(this.EmoteHandlerDoneLoadingCallback), null, false);
            if (!GameMgr.Get().IsAI())
            {
                AssetLoader.Get().LoadActor("EnemyEmoteHandler", new AssetLoader.GameObjectCallback(this.EnemyEmoteHandlerDoneLoadingCallback), null, false);
            }
        }
    }

    public virtual void NotifyOfOpponentPlayedCard(Entity entity)
    {
    }

    public virtual void NotifyOfOpponentWillPlayCard(string cardId)
    {
    }

    public virtual bool NotifyOfPlayError(PlayErrors.ErrorType error, Entity errorSource)
    {
        return false;
    }

    public virtual void NotifyOfStartOfTurnEventsFinished()
    {
    }

    public virtual void NotifyOfTargetModeCancelled()
    {
    }

    public virtual bool NotifyOfTooltipDisplay(TooltipZone tooltip)
    {
        return false;
    }

    public virtual void NotifyOfTooltipZoneMouseOver(TooltipZone tooltip)
    {
    }

    public virtual void OnEmotePlayed(Card card, EmoteType emoteType, CardSoundSpell emoteSpell)
    {
    }

    protected void OnEndGameScreenLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            UnityEngine.Debug.LogError(string.Format("GameEntity.OnEndGameScreenLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            EndGameScreen component = go.GetComponent<EndGameScreen>();
            ((EndGameScreenContext) callbackData).ShowScreen(component);
        }
    }

    public virtual void OnPlayThinkEmote()
    {
        if (!GameMgr.Get().IsAI())
        {
            EmoteType emoteType = EmoteType.THINK1;
            switch (UnityEngine.Random.Range(1, 4))
            {
                case 1:
                    emoteType = EmoteType.THINK1;
                    break;

                case 2:
                    emoteType = EmoteType.THINK2;
                    break;

                case 3:
                    emoteType = EmoteType.THINK3;
                    break;
            }
            GameState.Get().GetCurrentPlayer().GetHeroCard().PlayEmote(emoteType);
        }
    }

    public override void OnRealTimeTagChanged(Network.HistTagChange change)
    {
        if (change.Tag == 6)
        {
            this.HandleRealTimeMissionEvent(change.Value);
        }
    }

    private void OnSoundLoaded(string name, GameObject go, object callbackData)
    {
        this.m_preloadsNeeded--;
        if (go == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("GameEntity.OnSoundLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            AudioSource component = go.GetComponent<AudioSource>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("GameEntity.OnSoundLoaded() - ERROR \"{0}\" has no Spell component", name));
            }
            else
            {
                this.m_preloadedSounds.Add(name, component);
            }
        }
    }

    public override void OnTagsChanged(TagDeltaSet changeSet)
    {
        for (int i = 0; i < changeSet.Size(); i++)
        {
            TagDelta change = changeSet[i];
            this.OnTagChanged(change);
        }
    }

    private void PlayHeroBlowUpSpells(TAG_PLAYSTATE gameResult, out Spell enemyBlowUpSpell, out Spell friendlyBlowUpSpell)
    {
        enemyBlowUpSpell = null;
        friendlyBlowUpSpell = null;
        Card heroCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
        Card card = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
        if (gameResult == TAG_PLAYSTATE.WON)
        {
            enemyBlowUpSpell = this.BlowUpHero(heroCard, SpellType.ENDGAME_WIN);
        }
        else if (gameResult == TAG_PLAYSTATE.LOST)
        {
            friendlyBlowUpSpell = this.BlowUpHero(card, SpellType.ENDGAME_LOSE);
        }
        else if (gameResult == TAG_PLAYSTATE.TIED)
        {
            enemyBlowUpSpell = this.BlowUpHero(heroCard, SpellType.ENDGAME_DRAW);
            friendlyBlowUpSpell = this.BlowUpHero(card, SpellType.ENDGAME_LOSE);
        }
    }

    [DebuggerHidden]
    public virtual IEnumerator PlayMissionIntroLineAndWait()
    {
        return new <PlayMissionIntroLineAndWait>c__Iterator8E();
    }

    public virtual void PreloadAssets()
    {
    }

    public void PreloadSound(string soundName)
    {
        this.m_preloadsNeeded++;
        AssetLoader.Get().LoadSound(soundName, new AssetLoader.GameObjectCallback(this.OnSoundLoaded), null, false, SoundManager.Get().GetPlaceholderSound());
    }

    public void RemovePreloadedSound(string name)
    {
        this.m_preloadedSounds.Remove(name);
    }

    public virtual void SendCustomEvent(int eventID)
    {
    }

    public virtual bool ShouldAllowCardGrab(Entity entity)
    {
        return true;
    }

    public virtual bool ShouldDelayCardSoundSpells()
    {
        return false;
    }

    public virtual bool ShouldDoAlternateMulliganIntro()
    {
        return false;
    }

    public virtual bool ShouldDoOpeningTaunts()
    {
        return true;
    }

    public virtual bool ShouldHandleCoin()
    {
        return true;
    }

    public virtual bool ShouldShowBigCard()
    {
        return true;
    }

    public virtual bool ShouldShowCrazyKeywordTooltip()
    {
        return false;
    }

    public virtual bool ShouldShowHeroTooltips()
    {
        return false;
    }

    public virtual bool ShouldUseSecretClassNames()
    {
        return false;
    }

    private void ShowEndScreen(TAG_PLAYSTATE gameResult, Spell enemyBlowUpSpell, Spell friendlyBlowUpSpell)
    {
        EndGameScreenContext callbackData = new EndGameScreenContext {
            m_enemyBlowUpSpell = enemyBlowUpSpell,
            m_friendlyBlowUpSpell = friendlyBlowUpSpell
        };
        if (gameResult == TAG_PLAYSTATE.WON)
        {
            SoundManager.Get().LoadAndPlay("victory_jingle");
            if (((SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.DRAFT) && (DraftManager.Get() != null)) && (DraftManager.Get().GetWins() == 11))
            {
                DraftManager.Get().NotifyOfFinalGame(true);
            }
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_DISPLAYED_WIN_SCREEN);
            AssetLoader.Get().LoadActor("VictoryTwoScoop", new AssetLoader.GameObjectCallback(this.OnEndGameScreenLoaded), callbackData, false);
        }
        else if (gameResult == TAG_PLAYSTATE.LOST)
        {
            SoundManager.Get().LoadAndPlay("defeat_jingle");
            if (((SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.DRAFT) && (DraftManager.Get() != null)) && (DraftManager.Get().GetLosses() == 2))
            {
                DraftManager.Get().NotifyOfFinalGame(false);
            }
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_DISPLAYED_LOSS_SCREEN);
            AssetLoader.Get().LoadActor("DefeatTwoScoop", new AssetLoader.GameObjectCallback(this.OnEndGameScreenLoaded), callbackData, false);
        }
        else if (gameResult == TAG_PLAYSTATE.TIED)
        {
            SoundManager.Get().LoadAndPlay("defeat_jingle");
            if (((SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.DRAFT) && (DraftManager.Get() != null)) && (DraftManager.Get().GetLosses() == 2))
            {
                DraftManager.Get().NotifyOfFinalGame(false);
            }
            Network.TrackClient(Network.TrackLevel.LEVEL_INFO, Network.TrackWhat.TRACK_DISPLAYED_TIE_SCREEN);
            AssetLoader.Get().LoadActor("DefeatTwoScoop", new AssetLoader.GameObjectCallback(this.OnEndGameScreenLoaded), callbackData, false);
        }
    }

    public virtual void StartGameplaySoundtracks()
    {
        Board board = Board.Get();
        MusicPlaylistType type = (board != null) ? board.m_BoardMusic : MusicPlaylistType.InGame_Default;
        MusicManager.Get().StartPlaylist(type);
    }

    private void ToggleSpotLight(Light light, bool bOn)
    {
        <ToggleSpotLight>c__AnonStorey2F6 storeyf = new <ToggleSpotLight>c__AnonStorey2F6 {
            light = light
        };
        float num = 0.1f;
        float num2 = 1.3f;
        Action<object> action = new Action<object>(storeyf.<>m__F4);
        Action<object> action2 = new Action<object>(storeyf.<>m__F5);
        if (bOn)
        {
            storeyf.light.enabled = true;
            storeyf.light.intensity = 0f;
            object[] args = new object[] { "time", num, "from", 0f, "to", num2, "onupdate", action, "onupdatetarget", storeyf.light.gameObject };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ValueTo(storeyf.light.gameObject, hashtable);
        }
        else
        {
            object[] objArray2 = new object[] { "time", num, "from", storeyf.light.intensity, "to", 0f, "onupdate", action, "onupdatetarget", storeyf.light.gameObject, "oncomplete", action2 };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.ValueTo(storeyf.light.gameObject, hashtable2);
        }
    }

    public virtual string UpdateCardText(Card card, Actor bigCardActor, string text)
    {
        return text;
    }

    [CompilerGenerated]
    private sealed class <DoActionsAfterIntroBeforeMulligan>c__Iterator8F : IDisposable, IEnumerator, IEnumerator<object>
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
    private sealed class <FadeInActor>c__AnonStorey2F8
    {
        internal Material frameMat;
        internal Material mat;

        internal void <>m__F7(object amount)
        {
            this.mat.SetFloat("_LightingBlend", (float) amount);
            this.frameMat.SetFloat("_LightingBlend", (float) amount);
        }
    }

    [CompilerGenerated]
    private sealed class <FadeInHeroActor>c__AnonStorey2F7
    {
        internal Material heroFrameMat;
        internal Material heroMat;

        internal void <>m__F6(object amount)
        {
            this.heroMat.SetFloat("_LightingBlend", (float) amount);
            this.heroFrameMat.SetFloat("_LightingBlend", (float) amount);
        }
    }

    [CompilerGenerated]
    private sealed class <FadeOutActor>c__AnonStorey2F5
    {
        internal Material frameMat;
        internal Material mat;

        internal void <>m__F3(object amount)
        {
            this.mat.SetFloat("_LightingBlend", (float) amount);
            this.frameMat.SetFloat("_LightingBlend", (float) amount);
        }
    }

    [CompilerGenerated]
    private sealed class <FadeOutHeroActor>c__AnonStorey2F4
    {
        internal Material heroFrameMat;
        internal Material heroMat;

        internal void <>m__F2(object amount)
        {
            this.heroMat.SetFloat("_LightingBlend", (float) amount);
            this.heroFrameMat.SetFloat("_LightingBlend", (float) amount);
        }
    }

    [CompilerGenerated]
    private sealed class <HideOtherElements>c__Iterator90 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Card <$>card;
        internal Player <controller>__0;
        internal Card card;

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
                    this.$current = new WaitForSeconds(0.5f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<controller>__0 = this.card.GetEntity().GetController();
                    if (this.<controller>__0.GetHeroPowerCard() != null)
                    {
                        this.<controller>__0.GetHeroPowerCard().HideCard();
                        this.<controller>__0.GetHeroPowerCard().GetActor().ToggleForceIdle(true);
                        this.<controller>__0.GetHeroPowerCard().GetActor().SetActorState(ActorStateType.CARD_IDLE);
                        this.<controller>__0.GetHeroPowerCard().GetActor().DeactivateAllPreDeathSpells();
                    }
                    if (this.<controller>__0.GetWeaponCard() != null)
                    {
                        this.<controller>__0.GetWeaponCard().HideCard();
                        this.<controller>__0.GetWeaponCard().GetActor().ToggleForceIdle(true);
                        this.<controller>__0.GetWeaponCard().GetActor().SetActorState(ActorStateType.CARD_IDLE);
                        this.<controller>__0.GetWeaponCard().GetActor().DeactivateAllPreDeathSpells();
                    }
                    this.card.GetActor().HideArmorSpell();
                    this.card.GetActor().GetHealthObject().Hide();
                    this.card.GetActor().GetAttackObject().Hide();
                    this.card.GetActor().ToggleForceIdle(true);
                    this.card.GetActor().SetActorState(ActorStateType.CARD_IDLE);
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
    private sealed class <PlayMissionIntroLineAndWait>c__Iterator8E : IDisposable, IEnumerator, IEnumerator<object>
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
    private sealed class <ToggleSpotLight>c__AnonStorey2F6
    {
        internal Light light;

        internal void <>m__F4(object amount)
        {
            this.light.intensity = (float) amount;
        }

        internal void <>m__F5(object args)
        {
            this.light.enabled = false;
        }
    }

    protected class EndGameScreenContext
    {
        public Spell m_enemyBlowUpSpell;
        public Spell m_friendlyBlowUpSpell;

        private bool AreBlowUpSpellsFinished()
        {
            if ((this.m_enemyBlowUpSpell != null) && !this.m_enemyBlowUpSpell.IsFinished())
            {
                return false;
            }
            if ((this.m_friendlyBlowUpSpell != null) && !this.m_friendlyBlowUpSpell.IsFinished())
            {
                return false;
            }
            return true;
        }

        private void OnBlowUpSpellFinished(Spell spell, object userData)
        {
            EndGameScreen screen = (EndGameScreen) userData;
            if (this.AreBlowUpSpellsFinished())
            {
                screen.Show();
            }
        }

        public void ShowScreen(EndGameScreen screen)
        {
            bool flag = false;
            if ((this.m_enemyBlowUpSpell != null) && !this.m_enemyBlowUpSpell.IsFinished())
            {
                flag = true;
                this.m_enemyBlowUpSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnBlowUpSpellFinished), screen);
            }
            if ((this.m_friendlyBlowUpSpell != null) && !this.m_friendlyBlowUpSpell.IsFinished())
            {
                flag = true;
                this.m_friendlyBlowUpSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnBlowUpSpellFinished), screen);
            }
            if (!flag)
            {
                screen.Show();
            }
        }
    }
}

