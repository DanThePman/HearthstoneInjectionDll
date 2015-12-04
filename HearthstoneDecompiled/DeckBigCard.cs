using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class DeckBigCard : MonoBehaviour
{
    private HandActorCache m_actorCache = new HandActorCache();
    private bool m_actorCacheInit;
    public GameObject m_bottomPosition;
    private CardDef m_cardDef;
    private EntityDef m_entityDef;
    private int m_firstShowFrame;
    private CardFlair m_flair;
    private bool m_ghosted;
    private bool m_shown;
    private Actor m_shownActor;
    public GameObject m_topPosition;

    private void Awake()
    {
        this.m_firstShowFrame = 0;
    }

    public void ForceHide()
    {
        this.Hide();
    }

    private void Hide()
    {
        base.StopCoroutine("ShowWithDelayInternal");
        this.m_shown = false;
        if (this.m_shownActor != null)
        {
            this.m_shownActor.Hide();
            this.m_shownActor = null;
        }
    }

    public void Hide(EntityDef entityDef, CardFlair flair)
    {
        if ((this.m_entityDef == entityDef) && this.m_flair.Equals(flair))
        {
            this.Hide();
        }
    }

    private void OnActorLoaded(string name, Actor actor, object callbackData)
    {
        if (actor == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("DeckBigCard.OnActorLoaded() - FAILED to load {0}", name));
        }
        else
        {
            actor.TurnOffCollider();
            actor.Hide();
            actor.transform.parent = base.transform;
            TransformUtil.Identity(actor.transform);
            SceneUtils.SetLayer(actor, base.gameObject.layer);
            if (!this.m_actorCache.IsInitializing() && this.m_shown)
            {
                this.Show();
            }
        }
    }

    private void Show()
    {
        this.m_shownActor = this.m_actorCache.GetActor(this.m_entityDef, this.m_flair);
        if (this.m_shownActor != null)
        {
            this.m_shownActor.SetEntityDef(this.m_entityDef);
            this.m_shownActor.SetCardFlair(this.m_flair);
            this.m_shownActor.SetCardDef(this.m_cardDef);
            this.m_shownActor.GhostCardEffect(this.m_ghosted);
            this.m_shownActor.UpdateAllComponents();
            this.m_shownActor.Show();
        }
    }

    public void Show(EntityDef entityDef, CardFlair flair, CardDef cardDef, Vector3 sourcePosition, bool ghosted, float delay = 0)
    {
        <Show>c__AnonStorey2E0 storeye = new <Show>c__AnonStorey2E0 {
            entityDef = entityDef,
            flair = flair,
            cardDef = cardDef,
            sourcePosition = sourcePosition,
            ghosted = ghosted,
            <>f__this = this
        };
        if (false)
        {
            int frameCount = UnityEngine.Time.frameCount;
            if (this.m_firstShowFrame == 0)
            {
                this.m_firstShowFrame = frameCount;
            }
            else if ((frameCount - this.m_firstShowFrame) <= 1)
            {
                return;
            }
        }
        base.StopCoroutine("ShowWithDelayInternal");
        this.m_shown = true;
        this.m_entityDef = storeye.entityDef;
        this.m_flair = storeye.flair;
        this.m_cardDef = storeye.cardDef;
        this.m_ghosted = storeye.ghosted;
        if (delay > 0f)
        {
            KeyValuePair<float, System.Action> pair = new KeyValuePair<float, System.Action>(delay, new System.Action(storeye.<>m__BC));
            base.StartCoroutine("ShowWithDelayInternal", pair);
        }
        else
        {
            if (UniversalInputManager.UsePhoneUI == null)
            {
                float z = this.m_bottomPosition.transform.position.z;
                float max = this.m_topPosition.transform.position.z;
                float num4 = Mathf.Clamp(storeye.sourcePosition.z, z, max);
                TransformUtil.SetPosZ(base.transform, num4);
            }
            if (!this.m_actorCacheInit)
            {
                this.m_actorCacheInit = true;
                this.m_actorCache.AddActorLoadedListener(new HandActorCache.ActorLoadedCallback(this.OnActorLoaded));
                this.m_actorCache.Initialize();
            }
            if (!this.m_actorCache.IsInitializing())
            {
                this.Show();
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator ShowWithDelayInternal(KeyValuePair<float, System.Action> args)
    {
        return new <ShowWithDelayInternal>c__Iterator4C { args = args, <$>args = args };
    }

    [CompilerGenerated]
    private sealed class <Show>c__AnonStorey2E0
    {
        internal DeckBigCard <>f__this;
        internal CardDef cardDef;
        internal EntityDef entityDef;
        internal CardFlair flair;
        internal bool ghosted;
        internal Vector3 sourcePosition;

        internal void <>m__BC()
        {
            this.<>f__this.Show(this.entityDef, this.flair, this.cardDef, this.sourcePosition, this.ghosted, 0f);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowWithDelayInternal>c__Iterator4C : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal KeyValuePair<float, System.Action> <$>args;
        internal KeyValuePair<float, System.Action> args;

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
                    this.$current = new WaitForSeconds(this.args.Key);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.args.Value();
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

