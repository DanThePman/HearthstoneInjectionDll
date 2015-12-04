using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class DeckTrayCardBackContent : DeckTrayContent
{
    private bool m_animating;
    private AnimatedCardBack m_animData;
    [CustomEditField(Sections="Positioning")]
    public GameObject m_cardBackContainer;
    private GameObject m_currentCardBack;
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
    private bool m_waitingToLoadCardback;

    public override bool AnimateContentEntranceEnd()
    {
        return !this.m_animating;
    }

    public override bool AnimateContentEntranceStart()
    {
        if (this.m_waitingToLoadCardback)
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

    public void AnimateInNewCardBack(CardBackManager.LoadCardBackData cardBackData, GameObject original)
    {
        GameObject gameObject = cardBackData.m_GameObject;
        gameObject.GetComponent<Actor>().GetSpell(SpellType.DEATHREVERSE).Reactivate();
        AnimatedCardBack back = new AnimatedCardBack {
            CardBackId = cardBackData.m_CardBackIndex,
            GameObject = gameObject,
            OriginalScale = gameObject.transform.localScale,
            OriginalPosition = original.transform.position
        };
        this.m_animData = back;
        gameObject.transform.position = new Vector3(original.transform.position.x, original.transform.position.y + 0.5f, original.transform.position.z);
        gameObject.transform.localScale = this.m_cardBackContainer.transform.lossyScale;
        object[] args = new object[] { 
            "from", 0f, "to", 1f, "time", 0.6f, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "AnimateNewCardUpdate", "onupdatetarget", base.gameObject, "oncomplete", "AnimateNewCardFinished", "oncompleteparams", back, 
            "oncompletetarget", base.gameObject
         };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(gameObject, hashtable);
    }

    private void AnimateNewCardFinished(AnimatedCardBack cardBack)
    {
        cardBack.GameObject.transform.localScale = cardBack.OriginalScale;
        this.UpdateCardBack(cardBack.CardBackId, true, cardBack.GameObject);
        this.m_animData = null;
    }

    private void AnimateNewCardUpdate(float val)
    {
        GameObject gameObject = this.m_animData.GameObject;
        Vector3 originalPosition = this.m_animData.OriginalPosition;
        Vector3 position = this.m_cardBackContainer.transform.position;
        if (val <= 0.85f)
        {
            val /= 0.85f;
            float x = Mathf.Lerp(originalPosition.x, position.x, val);
            gameObject.transform.position = new Vector3(x, (Mathf.Lerp(originalPosition.y, position.y, val) + (Mathf.Sin(val * 3.141593f) * 15f)) + (val * 4f), Mathf.Lerp(originalPosition.z, position.z, val));
        }
        else
        {
            if (this.m_currentCardBack != null)
            {
                UnityEngine.Object.Destroy(this.m_currentCardBack);
                this.m_currentCardBack = null;
            }
            val = (val - 0.85f) / 0.15f;
            gameObject.transform.position = new Vector3(position.x, position.y + Mathf.Lerp(4f, 0f, val), position.z);
        }
    }

    private void Awake()
    {
        this.m_originalLocalPosition = base.transform.localPosition;
        base.transform.localPosition = this.m_originalLocalPosition + this.m_trayHiddenOffset;
        this.m_root.SetActive(false);
    }

    public override bool PreAnimateContentEntrance()
    {
        CollectionDeck taggedDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing);
        this.UpdateCardBack(taggedDeck.CardBackID, false, null);
        return true;
    }

    private void SetCardBack(GameObject go, bool overriden, bool assigning)
    {
        GameUtils.SetParent(go, this.m_cardBackContainer, true);
        Actor component = go.GetComponent<Actor>();
        if (component == null)
        {
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            if (assigning)
            {
                Spell spell = component.GetSpell(SpellType.DEATHREVERSE);
                if (spell != null)
                {
                    spell.ActivateState(SpellStateType.BIRTH);
                }
            }
            if (this.m_currentCardBack != null)
            {
                UnityEngine.Object.Destroy(this.m_currentCardBack);
            }
            this.m_currentCardBack = go;
            GameObject cardMesh = component.m_cardMesh;
            component.SetCardbackUpdateIgnore(true);
            component.SetUnlit();
            this.UpdateMissingEffect(component, overriden);
            if (cardMesh != null)
            {
                Material material = cardMesh.GetComponent<Renderer>().material;
                if (material.HasProperty("_SpecularIntensity"))
                {
                    material.SetFloat("_SpecularIntensity", 0f);
                }
            }
        }
    }

    public bool SetNewCardBack(int cardBackId, GameObject original)
    {
        <SetNewCardBack>c__AnonStorey36A storeya = new <SetNewCardBack>c__AnonStorey36A {
            original = original,
            <>f__this = this
        };
        if (this.m_animData != null)
        {
            return false;
        }
        if (!CardBackManager.Get().LoadCardBackByIndex(cardBackId, new CardBackManager.LoadCardBackData.LoadCardBackCallback(storeya.<>m__22D), "Card_Hidden"))
        {
            Debug.LogError("Could not load CardBack " + cardBackId);
            return false;
        }
        return true;
    }

    public void UpdateCardBack(int cardBackId, bool assigning, GameObject obj = null)
    {
        <UpdateCardBack>c__AnonStorey36B storeyb = new <UpdateCardBack>c__AnonStorey36B {
            assigning = assigning,
            <>f__this = this,
            currentDeck = CollectionManager.Get().GetTaggedDeck(CollectionManager.DeckTag.Editing)
        };
        if (storeyb.assigning)
        {
            if (!string.IsNullOrEmpty(this.m_socketSound))
            {
                SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_socketSound));
            }
            storeyb.currentDeck.CardBackOverridden = true;
        }
        storeyb.currentDeck.CardBackID = cardBackId;
        if (obj != null)
        {
            this.SetCardBack(obj, storeyb.currentDeck.CardBackOverridden, storeyb.assigning);
        }
        else
        {
            this.m_waitingToLoadCardback = true;
            if (!CardBackManager.Get().LoadCardBackByIndex(cardBackId, new CardBackManager.LoadCardBackData.LoadCardBackCallback(storeyb.<>m__22E), "Card_Hidden"))
            {
                this.m_waitingToLoadCardback = false;
                Debug.LogWarning(string.Format("CardBackManager was unable to load card back ID: {0}", cardBackId));
            }
        }
    }

    private void UpdateMissingEffect(Actor cardBackActor, bool overriden)
    {
        if (cardBackActor != null)
        {
            if (overriden)
            {
                cardBackActor.DisableMissingCardEffect();
            }
            else
            {
                cardBackActor.SetMissingCardMaterial(this.m_sepiaCardMaterial);
                cardBackActor.MissingCardEffect();
            }
            cardBackActor.UpdateAllComponents();
        }
    }

    [CompilerGenerated]
    private sealed class <SetNewCardBack>c__AnonStorey36A
    {
        internal DeckTrayCardBackContent <>f__this;
        internal GameObject original;

        internal void <>m__22D(CardBackManager.LoadCardBackData cardBackData)
        {
            this.<>f__this.AnimateInNewCardBack(cardBackData, this.original);
        }
    }

    [CompilerGenerated]
    private sealed class <UpdateCardBack>c__AnonStorey36B
    {
        internal DeckTrayCardBackContent <>f__this;
        internal bool assigning;
        internal CollectionDeck currentDeck;

        internal void <>m__22E(CardBackManager.LoadCardBackData cardBackData)
        {
            this.<>f__this.m_waitingToLoadCardback = false;
            GameObject gameObject = cardBackData.m_GameObject;
            this.<>f__this.SetCardBack(gameObject, this.currentDeck.CardBackOverridden, this.assigning);
        }
    }

    private class AnimatedCardBack
    {
        public int CardBackId;
        public UnityEngine.GameObject GameObject;
        public Vector3 OriginalPosition;
        public Vector3 OriginalScale;
    }
}

