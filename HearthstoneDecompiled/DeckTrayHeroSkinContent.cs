using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class DeckTrayHeroSkinContent : DeckTrayContent
{
    private bool m_animating;
    private AnimatedHeroSkin m_animData;
    private string m_currentHeroCardId;
    public UberText m_currentHeroSkinName;
    private List<HeroAssigned> m_heroAssignedListeners = new List<HeroAssigned>();
    [CustomEditField(Sections="Positioning")]
    public GameObject m_heroSkinContainer;
    private Actor m_heroSkinObject;
    private Vector3 m_originalLocalPosition;
    [CustomEditField(Sections="Positioning")]
    public GameObject m_root;
    [CustomEditField(Sections="Card Effects")]
    public Material m_sepiaCardMaterial;
    [CustomEditField(Sections="Animation & Sounds", T=EditType.SOUND_PREFAB)]
    public string m_socketSound;
    [CustomEditField(Sections="Positioning")]
    public Vector3 m_trayHiddenOffset;
    [CustomEditField(Sections="Animation & Sounds")]
    public float m_traySlideAnimationTime = 0.25f;
    [CustomEditField(Sections="Animation & Sounds")]
    public iTween.EaseType m_traySlideSlideInAnimation = iTween.EaseType.easeOutBounce;
    [CustomEditField(Sections="Animation & Sounds")]
    public iTween.EaseType m_traySlideSlideOutAnimation;
    private bool m_waitingToLoadHeroDef;

    public override bool AnimateContentEntranceEnd()
    {
        return !this.m_animating;
    }

    public override bool AnimateContentEntranceStart()
    {
        if (this.m_waitingToLoadHeroDef)
        {
            return false;
        }
        this.m_root.SetActive(true);
        base.transform.localPosition = this.m_originalLocalPosition;
        this.m_animating = true;
        object[] args = new object[] { "position", this.m_originalLocalPosition + this.m_trayHiddenOffset, "islocal", true, "time", this.m_traySlideAnimationTime, "easetype", this.m_traySlideSlideInAnimation, "oncomplete", delegate (object o) {
            this.m_animating = false;
        } };
        iTween.MoveFrom(base.gameObject, iTween.Hash(args));
        return true;
    }

    public override bool AnimateContentExitEnd()
    {
        return !this.m_animating;
    }

    public override bool AnimateContentExitStart()
    {
        base.transform.localPosition = this.m_originalLocalPosition;
        this.m_animating = true;
        object[] args = new object[] { "position", this.m_originalLocalPosition + this.m_trayHiddenOffset, "islocal", true, "time", this.m_traySlideAnimationTime, "easetype", this.m_traySlideSlideOutAnimation, "oncomplete", delegate (object o) {
            this.m_animating = false;
            this.m_root.SetActive(false);
        } };
        iTween.MoveTo(base.gameObject, iTween.Hash(args));
        return true;
    }

    public void AnimateInNewHeroSkin(Actor actor)
    {
        GameObject gameObject = actor.gameObject;
        AnimatedHeroSkin skin = new AnimatedHeroSkin {
            Actor = actor,
            GameObject = gameObject,
            OriginalScale = gameObject.transform.localScale,
            OriginalPosition = gameObject.transform.position
        };
        this.m_animData = skin;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 0.5f, gameObject.transform.position.z);
        gameObject.transform.localScale = this.m_heroSkinContainer.transform.lossyScale;
        object[] args = new object[] { 
            "from", 0f, "to", 1f, "time", 0.6f, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "AnimateNewHeroSkinUpdate", "onupdatetarget", base.gameObject, "oncomplete", "AnimateNewHeroSkinFinished", "oncompleteparams", skin, 
            "oncompletetarget", base.gameObject
         };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(gameObject, hashtable);
        CollectionHeroSkin component = actor.gameObject.GetComponent<CollectionHeroSkin>();
        if (component != null)
        {
            component.ShowSocketFX();
        }
    }

    private void AnimateNewHeroSkinFinished()
    {
        this.m_heroSkinObject.gameObject.SetActive(true);
        Actor baseActor = this.m_animData.Actor;
        this.UpdateHeroSkin(baseActor.GetEntityDef().GetCardId(), baseActor.GetCardFlair(), true, baseActor);
        UnityEngine.Object.Destroy(this.m_animData.GameObject);
        this.m_animData = null;
    }

    private void AnimateNewHeroSkinUpdate(float val)
    {
        GameObject gameObject = this.m_animData.GameObject;
        Vector3 originalPosition = this.m_animData.OriginalPosition;
        Vector3 position = this.m_heroSkinContainer.transform.position;
        if (val <= 0.85f)
        {
            val /= 0.85f;
            float x = Mathf.Lerp(originalPosition.x, position.x, val);
            gameObject.transform.position = new Vector3(x, (Mathf.Lerp(originalPosition.y, position.y, val) + (Mathf.Sin(val * 3.141593f) * 15f)) + (val * 4f), Mathf.Lerp(originalPosition.z, position.z, val));
        }
        else
        {
            this.m_heroSkinObject.gameObject.SetActive(false);
            val = (val - 0.85f) / 0.15f;
            gameObject.transform.position = new Vector3(position.x, position.y + Mathf.Lerp(4f, 0f, val), position.z);
        }
    }

    private void Awake()
    {
        this.m_originalLocalPosition = base.transform.localPosition;
        base.transform.localPosition = this.m_originalLocalPosition + this.m_trayHiddenOffset;
        this.m_root.SetActive(false);
        this.LoadHeroSkinActor();
    }

    private void LoadHeroSkinActor()
    {
        string heroSkinOrHandActor = ActorNames.GetHeroSkinOrHandActor(TAG_CARDTYPE.HERO, TAG_PREMIUM.NORMAL);
        AssetLoader.Get().LoadActor(heroSkinOrHandActor, delegate (string name, GameObject go, object callbackData) {
            if (go == null)
            {
                Debug.LogWarning(string.Format("DeckTrayHeroSkinContent.LoadHeroSkinActor - FAILED to load \"{0}\"", name));
            }
            else
            {
                Actor child = go.GetComponent<Actor>();
                if (child == null)
                {
                    Debug.LogWarning(string.Format("HandActorCache.OnActorLoaded() - ERROR \"{0}\" has no Actor component", name));
                }
                else
                {
                    GameUtils.SetParent(child, this.m_heroSkinContainer, false);
                    this.m_heroSkinObject = child;
                }
            }
        }, null, false);
    }

    private void OnPickEmoteFinished(Spell spell, object userData)
    {
        UnityEngine.Object.Destroy(spell.gameObject);
    }

    private void OnPickEmoteLoaded(CardSoundSpell spell)
    {
        if (spell != null)
        {
            spell.AddFinishedCallback(new Spell.FinishedCallback(this.OnPickEmoteFinished));
            spell.Reactivate();
        }
    }

    public override bool PreAnimateContentEntrance()
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        this.UpdateHeroSkin(taggedDeck.HeroCardID, taggedDeck.HeroCardFlair, false, null);
        return true;
    }

    public void RegisterHeroAssignedListener(HeroAssigned dlg)
    {
        this.m_heroAssignedListeners.Add(dlg);
    }

    public void SetNewHeroSkin(Actor actor)
    {
        if (this.m_animData == null)
        {
            Actor actor2 = actor.Clone();
            actor2.SetCardDef(actor.GetCardDef());
            this.AnimateInNewHeroSkin(actor2);
        }
    }

    private void ShowSocketFX()
    {
        CollectionHeroSkin component = this.m_heroSkinObject.GetComponent<CollectionHeroSkin>();
        if (component != null)
        {
            component.ShowSocketFX();
        }
    }

    public void UnregisterHeroAssignedListener(HeroAssigned dlg)
    {
        this.m_heroAssignedListeners.Remove(dlg);
    }

    public void UpdateHeroSkin(EntityDef entityDef, CardFlair cardFlair, bool assigning)
    {
        this.UpdateHeroSkin(entityDef.GetCardId(), cardFlair, assigning, null);
    }

    public void UpdateHeroSkin(string cardId, CardFlair cardFlair, bool assigning, Actor baseActor = null)
    {
        <UpdateHeroSkin>c__AnonStorey373 storey = new <UpdateHeroSkin>c__AnonStorey373 {
            cardFlair = cardFlair,
            assigning = assigning,
            <>f__this = this
        };
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        if (storey.assigning)
        {
            if (!string.IsNullOrEmpty(this.m_socketSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_socketSound));
            }
            taggedDeck.HeroOverridden = true;
        }
        this.UpdateMissingEffect(taggedDeck.HeroOverridden);
        if (this.m_currentHeroCardId == cardId)
        {
            this.ShowSocketFX();
        }
        else
        {
            this.m_currentHeroCardId = cardId;
            taggedDeck.HeroCardID = cardId;
            taggedDeck.HeroCardFlair = storey.cardFlair;
            if (baseActor != null)
            {
                this.UpdateHeroSkinVisual(baseActor.GetEntityDef(), baseActor.GetCardDef(), baseActor.GetCardFlair(), storey.assigning);
            }
            else
            {
                this.m_waitingToLoadHeroDef = true;
                DefLoader.Get().LoadFullDef(cardId, new DefLoader.LoadDefCallback<FullDef>(storey.<>m__253));
            }
        }
    }

    private void UpdateHeroSkinVisual(EntityDef entityDef, CardDef cardDef, CardFlair cardFlair, bool assigning)
    {
        if (this.m_heroSkinObject == null)
        {
            Debug.LogError("Hero skin object not loaded yet! Cannot set portrait!");
        }
        else
        {
            this.m_heroSkinObject.SetEntityDef(entityDef);
            this.m_heroSkinObject.SetCardDef(cardDef);
            this.m_heroSkinObject.SetCardFlair(cardFlair);
            this.m_heroSkinObject.UpdateAllComponents();
            CollectionHeroSkin component = this.m_heroSkinObject.GetComponent<CollectionHeroSkin>();
            if (component != null)
            {
                component.SetClass(entityDef.GetClass());
            }
            foreach (HeroAssigned assigned in this.m_heroAssignedListeners.ToArray())
            {
                assigned(entityDef.GetCardId());
            }
            if (assigning)
            {
                GameUtils.LoadCardDefEmoteSound(cardDef, EmoteType.PICKED, new GameUtils.EmoteSoundLoaded(this.OnPickEmoteLoaded));
            }
            if (this.m_currentHeroSkinName != null)
            {
                this.m_currentHeroSkinName.Text = entityDef.GetName();
            }
            this.ShowSocketFX();
        }
    }

    private void UpdateMissingEffect(bool overriden)
    {
        if (overriden)
        {
            this.m_heroSkinObject.DisableMissingCardEffect();
        }
        else
        {
            this.m_heroSkinObject.SetMissingCardMaterial(this.m_sepiaCardMaterial);
            this.m_heroSkinObject.MissingCardEffect();
        }
        this.m_heroSkinObject.UpdateAllComponents();
    }

    [CompilerGenerated]
    private sealed class <UpdateHeroSkin>c__AnonStorey373
    {
        internal DeckTrayHeroSkinContent <>f__this;
        internal bool assigning;
        internal CardFlair cardFlair;

        internal void <>m__253(string cardID, FullDef fullDef, object callbackData)
        {
            this.<>f__this.m_waitingToLoadHeroDef = false;
            this.<>f__this.UpdateHeroSkinVisual(fullDef.GetEntityDef(), fullDef.GetCardDef(), this.cardFlair, this.assigning);
        }
    }

    private class AnimatedHeroSkin
    {
        public Actor Actor;
        public UnityEngine.GameObject GameObject;
        public Vector3 OriginalPosition;
        public Vector3 OriginalScale;
    }

    public delegate void HeroAssigned(string cardId);
}

