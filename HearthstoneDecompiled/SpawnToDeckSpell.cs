using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnToDeckSpell : SuperSpell
{
    public HandActorSource m_HandActorSource;
    private CardDef m_overrideCardDef;
    public string m_OverrideCardId;
    public float m_RevealFriendlySideZOffset;
    public float m_RevealOpponentSideZOffset;
    public float m_RevealStartScale = 0.1f;
    public float m_RevealYOffsetMax = 5f;
    public float m_RevealYOffsetMin = 5f;
    public SequenceData m_SequenceData = new SequenceData();
    public SpreadType m_SpreadType;
    public StackData m_StackData = new StackData();
    private const float PHONE_HAND_OFFSET = 1.5f;

    public override bool AddPowerTargets()
    {
        base.m_visualToTargetIndexMap.Clear();
        base.m_targetToMetaDataMap.Clear();
        base.m_targets.Clear();
        List<PowerTask> taskList = base.m_taskList.GetTaskList();
        for (int i = 0; i < taskList.Count; i++)
        {
            PowerTask task = taskList[i];
            Card targetCardFromPowerTask = this.GetTargetCardFromPowerTask(i, task);
            if (targetCardFromPowerTask != null)
            {
                this.AddTarget(targetCardFromPowerTask.gameObject);
            }
        }
        return (base.m_targets.Count > 0);
    }

    [DebuggerHidden]
    private IEnumerator AnimateActor(List<Actor> actors, int index, float revealSec, Vector3 revealPos, float waitSec)
    {
        return new <AnimateActor>c__Iterator227 { actors = actors, index = index, revealPos = revealPos, revealSec = revealSec, waitSec = waitSec, <$>actors = actors, <$>index = index, <$>revealPos = revealPos, <$>revealSec = revealSec, <$>waitSec = waitSec, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator AnimateSpread(List<Actor> actors)
    {
        return new <AnimateSpread>c__Iterator226 { actors = actors, <$>actors = actors, <>f__this = this };
    }

    private Vector3 ComputeRevealPosition(Vector3 offset)
    {
        Vector3 position = base.transform.position;
        float num = UnityEngine.Random.Range(this.m_RevealYOffsetMin, this.m_RevealYOffsetMax);
        position.y += num;
        switch (base.GetSourceCard().GetControllerSide())
        {
            case Player.Side.FRIENDLY:
                position.z += this.m_RevealFriendlySideZOffset;
                break;

            case Player.Side.OPPOSING:
                position.z += this.m_RevealOpponentSideZOffset;
                break;
        }
        return (position + offset);
    }

    [DebuggerHidden]
    private IEnumerator DoActionWithTiming()
    {
        return new <DoActionWithTiming>c__Iterator223 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator DoEffects(List<Actor> actors)
    {
        return new <DoEffects>c__Iterator225 { actors = actors, <$>actors = actors, <>f__this = this };
    }

    private CardDef GetCardDef(Card card)
    {
        HandActorSource handActorSource = this.m_HandActorSource;
        if (handActorSource != HandActorSource.CHOSEN_TARGET)
        {
            if (handActorSource == HandActorSource.OVERRIDE)
            {
                return this.m_overrideCardDef;
            }
            return card.GetCardDef();
        }
        return base.GetPowerTargetCard().GetCardDef();
    }

    private EntityDef GetEntityDef(Entity entity)
    {
        HandActorSource handActorSource = this.m_HandActorSource;
        if (handActorSource != HandActorSource.CHOSEN_TARGET)
        {
            if (handActorSource == HandActorSource.OVERRIDE)
            {
                return DefLoader.Get().GetEntityDef(this.m_OverrideCardId);
            }
            return entity.GetEntityDef();
        }
        return base.GetPowerTarget().GetEntityDef();
    }

    private TAG_PREMIUM GetPremium(Entity entity)
    {
        TAG_PREMIUM premiumType = base.GetSourceCard().GetEntity().GetPremiumType();
        if (premiumType == TAG_PREMIUM.GOLDEN)
        {
            return TAG_PREMIUM.GOLDEN;
        }
        HandActorSource handActorSource = this.m_HandActorSource;
        if (handActorSource != HandActorSource.CHOSEN_TARGET)
        {
            if (handActorSource == HandActorSource.OVERRIDE)
            {
                return premiumType;
            }
            return entity.GetPremiumType();
        }
        return base.GetPowerTarget().GetPremiumType();
    }

    protected override Card GetTargetCardFromPowerTask(int index, PowerTask task)
    {
        Network.PowerHistory power = task.GetPower();
        if (power.Type != Network.PowerType.FULL_ENTITY)
        {
            return null;
        }
        Network.HistFullEntity entity = power as Network.HistFullEntity;
        Network.Entity entity2 = entity.Entity;
        Entity entity3 = GameState.Get().GetEntity(entity2.ID);
        if (entity3 == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.GetTargetCardFromPowerTask() - WARNING trying to target entity with id {1} but there is no entity with that id", this, entity2.ID));
            return null;
        }
        if (entity3.GetZone() != TAG_ZONE.DECK)
        {
            return null;
        }
        return entity3.GetCard();
    }

    [DebuggerHidden]
    private IEnumerator LoadAssets(List<Actor> actors)
    {
        return new <LoadAssets>c__Iterator224 { actors = actors, <$>actors = actors, <>f__this = this };
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        base.StartCoroutine(this.DoActionWithTiming());
    }

    private void PreventDeckOverlap(List<Actor> actors, List<Vector3> revealPositions)
    {
        float screenDist = 0f;
        for (int i = 0; i < revealPositions.Count; i++)
        {
            GameObject obj2 = base.m_targets[i];
            ZoneDeck deckZone = obj2.GetComponent<Card>().GetEntity().GetController().GetDeckZone();
            float x = 0f;
            GameObject activeThickness = deckZone.GetActiveThickness();
            if (activeThickness != null)
            {
                x = activeThickness.GetComponent<Renderer>().bounds.extents.x;
            }
            Vector3 position = deckZone.transform.position;
            position.x -= x;
            Vector3 vector2 = revealPositions[i];
            vector2.x += actors[i].GetMeshRenderer().bounds.extents.x;
            Vector3 vector3 = Camera.main.WorldToScreenPoint(position);
            float num4 = Camera.main.WorldToScreenPoint(vector2).x - vector3.x;
            if (num4 > screenDist)
            {
                screenDist = num4;
            }
        }
        if (screenDist > 0f)
        {
            for (int j = 0; j < revealPositions.Count; j++)
            {
                GameObject obj4 = base.m_targets[j];
                ZoneDeck deck2 = obj4.GetComponent<Card>().GetEntity().GetController().GetDeckZone();
                float num6 = CameraUtils.ScreenToWorldDist(Camera.main, screenDist, deck2.transform.position);
                Vector3 vector5 = revealPositions[j];
                vector5 = new Vector3(vector5.x - num6, vector5.y, vector5.z);
                revealPositions[j] = vector5;
            }
        }
    }

    private void PreventHandOverlapPhone(List<Actor> actors, List<Vector3> revealPositions)
    {
        if (UniversalInputManager.UsePhoneUI != null)
        {
            Entity powerTarget = base.GetPowerTarget();
            if (powerTarget != null)
            {
                if (powerTarget.GetControllerSide() == Player.Side.OPPOSING)
                {
                    return;
                }
            }
            else
            {
                Card sourceCard = base.GetSourceCard();
                if ((sourceCard != null) && (sourceCard.GetControllerSide() == Player.Side.OPPOSING))
                {
                    return;
                }
            }
            for (int i = 0; i < revealPositions.Count; i++)
            {
                Vector3 vector = revealPositions[i];
                vector = new Vector3(vector.x, vector.y, vector.z + 1.5f);
                revealPositions[i] = vector;
            }
        }
    }

    private List<float> RandomizeRevealTimes(int count, float revealSec, float nextRevealSecMin, float nextRevealSecMax)
    {
        List<float> list = new List<float>(count);
        List<int> list2 = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(0f);
            list2.Add(i);
        }
        float num2 = revealSec;
        for (int j = 0; j < count; j++)
        {
            int index = UnityEngine.Random.Range(0, list2.Count);
            int num5 = list2[index];
            list2.RemoveAt(index);
            list[num5] = num2;
            float num6 = UnityEngine.Random.Range(nextRevealSecMin, nextRevealSecMax);
            num2 += num6;
        }
        return list;
    }

    [CompilerGenerated]
    private sealed class <AnimateActor>c__Iterator227 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Actor> <$>actors;
        internal int <$>index;
        internal Vector3 <$>revealPos;
        internal float <$>revealSec;
        internal float <$>waitSec;
        internal SpawnToDeckSpell <>f__this;
        internal Actor <actor>__0;
        internal Card <card>__2;
        internal Player <controller>__4;
        internal ZoneDeck <deck>__6;
        internal Entity <entity>__3;
        internal EntityDef <entityDef>__9;
        internal bool <hiddenActor>__10;
        internal ZonePlay <play>__5;
        internal Vector3 <revealAngles>__8;
        internal Vector3 <revealScale>__7;
        internal GameObject <targetObject>__1;
        internal List<Actor> actors;
        internal int index;
        internal Vector3 revealPos;
        internal float revealSec;
        internal float waitSec;

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
                {
                    this.<actor>__0 = this.actors[this.index];
                    this.<targetObject>__1 = this.<>f__this.m_targets[this.index];
                    this.<card>__2 = this.<targetObject>__1.GetComponent<Card>();
                    this.<entity>__3 = this.<card>__2.GetEntity();
                    this.<controller>__4 = this.<entity>__3.GetController();
                    this.<play>__5 = this.<controller>__4.GetBattlefieldZone();
                    this.<deck>__6 = this.<controller>__4.GetDeckZone();
                    this.<actor>__0.transform.localScale = new Vector3(this.<>f__this.m_RevealStartScale, this.<>f__this.m_RevealStartScale, this.<>f__this.m_RevealStartScale);
                    this.<actor>__0.transform.rotation = this.<>f__this.transform.rotation;
                    this.<actor>__0.transform.position = this.<>f__this.transform.position;
                    this.<actor>__0.Show();
                    this.<revealScale>__7 = Vector3.one;
                    this.<revealAngles>__8 = this.<play>__5.transform.rotation.eulerAngles;
                    object[] args = new object[] { "position", this.revealPos, "time", this.revealSec, "easetype", iTween.EaseType.easeOutExpo };
                    iTween.MoveTo(this.<actor>__0.gameObject, iTween.Hash(args));
                    object[] objArray2 = new object[] { "rotation", this.<revealAngles>__8, "time", this.revealSec, "easetype", iTween.EaseType.easeOutExpo };
                    iTween.RotateTo(this.<actor>__0.gameObject, iTween.Hash(objArray2));
                    object[] objArray3 = new object[] { "scale", this.<revealScale>__7, "time", this.revealSec, "easetype", iTween.EaseType.easeOutExpo };
                    iTween.ScaleTo(this.<actor>__0.gameObject, iTween.Hash(objArray3));
                    if (this.waitSec <= 0f)
                    {
                        break;
                    }
                    this.$current = new WaitForSeconds(this.waitSec);
                    this.$PC = 1;
                    goto Label_030D;
                }
                case 1:
                    break;

                case 2:
                    this.<actor>__0.Destroy();
                    this.$PC = -1;
                    goto Label_030B;

                default:
                    goto Label_030B;
            }
            this.<entityDef>__9 = this.<>f__this.GetEntityDef(this.<entity>__3);
            this.<hiddenActor>__10 = this.<entityDef>__9.GetCardType() == TAG_CARDTYPE.INVALID;
            this.$current = this.<>f__this.StartCoroutine(this.<card>__2.AnimatePlayToDeck(this.<actor>__0.gameObject, this.<deck>__6, this.<hiddenActor>__10));
            this.$PC = 2;
            goto Label_030D;
        Label_030B:
            return false;
        Label_030D:
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
    private sealed class <AnimateSpread>c__Iterator226 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Actor> <$>actors;
        internal SpawnToDeckSpell <>f__this;
        internal float <currHoldSec>__11;
        internal float <currOffset>__3;
        internal float <holdSec>__12;
        internal int <i>__14;
        internal int <i>__2;
        internal int <i>__8;
        internal float <maxRevealSec>__7;
        internal Vector3 <offset>__4;
        internal Vector3 <revealPos>__15;
        internal Vector3 <revealPos>__5;
        internal Vector3 <revealPos>__9;
        internal List<Vector3> <revealPositions>__0;
        internal float <revealSec>__10;
        internal List<float> <revealSecs>__6;
        internal float <startOffset>__1;
        internal float <waitSec>__13;
        internal List<Actor> actors;

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
                    if (this.<>f__this.m_SpreadType != SpawnToDeckSpell.SpreadType.SEQUENCE)
                    {
                        this.<i>__14 = 0;
                        while (this.<i>__14 < this.actors.Count)
                        {
                            this.<revealPos>__15 = this.<>f__this.ComputeRevealPosition(Vector3.zero);
                            this.<>f__this.StartCoroutine(this.<>f__this.AnimateActor(this.actors, this.<i>__14, this.<>f__this.m_StackData.m_RevealTime, this.<revealPos>__15, this.<>f__this.m_StackData.m_RevealTime));
                            if (this.<i>__14 < (this.actors.Count - 1))
                            {
                                this.$current = new WaitForSeconds(this.<>f__this.m_StackData.m_StaggerTime);
                                this.$PC = 1;
                                return true;
                            }
                        Label_032E:
                            this.<i>__14++;
                        }
                        break;
                    }
                    this.<revealPositions>__0 = new List<Vector3>();
                    this.<startOffset>__1 = (-0.5f * (this.actors.Count - 1)) * this.<>f__this.m_SequenceData.m_Spacing;
                    this.<i>__2 = 0;
                    while (this.<i>__2 < this.actors.Count)
                    {
                        this.<currOffset>__3 = this.<i>__2 * this.<>f__this.m_SequenceData.m_Spacing;
                        this.<offset>__4 = new Vector3(this.<startOffset>__1 + this.<currOffset>__3, 0f, 0f);
                        this.<revealPos>__5 = this.<>f__this.ComputeRevealPosition(this.<offset>__4);
                        this.<revealPositions>__0.Add(this.<revealPos>__5);
                        this.<i>__2++;
                    }
                    this.<>f__this.PreventDeckOverlap(this.actors, this.<revealPositions>__0);
                    this.<>f__this.PreventHandOverlapPhone(this.actors, this.<revealPositions>__0);
                    this.<revealSecs>__6 = this.<>f__this.RandomizeRevealTimes(this.actors.Count, this.<>f__this.m_SequenceData.m_RevealTime, this.<>f__this.m_SequenceData.m_NextCardRevealTimeMin, this.<>f__this.m_SequenceData.m_NextCardRevealTimeMax);
                    this.<maxRevealSec>__7 = Mathf.Max(this.<revealSecs>__6.ToArray());
                    this.<i>__8 = 0;
                    while (this.<i>__8 < this.actors.Count)
                    {
                        this.<revealPos>__9 = this.<revealPositions>__0[this.<i>__8];
                        this.<revealSec>__10 = this.<revealSecs>__6[this.<i>__8];
                        this.<currHoldSec>__11 = ((this.actors.Count - 1) - this.<i>__8) * this.<>f__this.m_SequenceData.m_NextCardHoldTime;
                        this.<holdSec>__12 = this.<>f__this.m_SequenceData.m_HoldTime + this.<currHoldSec>__11;
                        this.<waitSec>__13 = this.<maxRevealSec>__7 + this.<holdSec>__12;
                        this.<>f__this.StartCoroutine(this.<>f__this.AnimateActor(this.actors, this.<i>__8, this.<revealSec>__10, this.<revealPos>__9, this.<waitSec>__13));
                        this.<i>__8++;
                    }
                    break;

                case 1:
                    goto Label_032E;

                default:
                    goto Label_0359;
            }
            this.$PC = -1;
        Label_0359:
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
    private sealed class <DoActionWithTiming>c__Iterator223 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SpawnToDeckSpell <>f__this;
        internal List<Actor> <actors>__0;

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
                    this.<actors>__0 = new List<Actor>(this.<>f__this.m_targets.Count);
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.LoadAssets(this.<actors>__0));
                    this.$PC = 1;
                    goto Label_00A5;

                case 1:
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.DoEffects(this.<actors>__0));
                    this.$PC = 2;
                    goto Label_00A5;

                case 2:
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00A5:
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
    private sealed class <DoEffects>c__Iterator225 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Actor> <$>actors;
        private static Predicate<Actor> <>f__am$cache6;
        internal SpawnToDeckSpell <>f__this;
        internal Actor <livingActor>__0;
        internal List<Actor> actors;

        private static bool <>m__1D8(Actor currActor)
        {
            return (bool) currActor;
        }

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
                    this.<>f__this.StartCoroutine(this.<>f__this.AnimateSpread(this.actors));
                    this.<livingActor>__0 = null;
                    break;

                case 1:
                    goto Label_0096;

                default:
                    goto Label_00CB;
            }
        Label_0045:
            if (<>f__am$cache6 == null)
            {
                <>f__am$cache6 = new Predicate<Actor>(SpawnToDeckSpell.<DoEffects>c__Iterator225.<>m__1D8);
            }
            this.<livingActor>__0 = this.actors.Find(<>f__am$cache6);
            if (this.<livingActor>__0 != null)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
        Label_0096:
            if (this.<livingActor>__0 != null)
            {
                goto Label_0045;
            }
            this.<>f__this.m_effectsPendingFinish--;
            this.<>f__this.FinishIfPossible();
            this.$PC = -1;
        Label_00CB:
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
    private sealed class <LoadAssets>c__Iterator224 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<Actor> <$>actors;
        internal SpawnToDeckSpell <>f__this;
        internal AssetLoader.GameObjectCallback <actorCallback>__3;
        internal int <assetsLoading>__2;
        internal Card <card>__6;
        internal DefLoader.LoadDefCallback<CardDef> <cardDefCallback>__1;
        internal Entity <entity>__7;
        internal EntityDef <entityDef>__8;
        internal int <i>__4;
        internal bool <loadingOverrideCardDef>__0;
        internal string <name>__10;
        internal TAG_PREMIUM <premium>__9;
        internal GameObject <targetObject>__5;
        internal List<Actor> actors;

        internal void <>m__1D6(string cardId, CardDef def, object userData)
        {
            this.<loadingOverrideCardDef>__0 = false;
            if (def == null)
            {
                object[] messageArgs = new object[] { cardId };
                Error.AddDevFatal("SpawnToDeckSpell.LoadAssets() - FAILED to load CardDef for {0}", messageArgs);
            }
            else
            {
                this.<>f__this.m_overrideCardDef = def;
            }
        }

        internal void <>m__1D7(string name, GameObject go, object userData)
        {
            this.<assetsLoading>__2--;
            int num = (int) userData;
            if (go == null)
            {
                object[] messageArgs = new object[] { name, num };
                Error.AddDevFatal("SpawnToDeckSpell.LoadAssets() - FAILED to load actor {0} (targetIndex {1})", messageArgs);
            }
            else
            {
                Actor component = go.GetComponent<Actor>();
                Card card = this.<>f__this.m_targets[num].GetComponent<Card>();
                Entity entity = card.GetEntity();
                component.SetEntityDef(this.<>f__this.GetEntityDef(entity));
                component.SetCardDef(this.<>f__this.GetCardDef(card));
                component.SetPremium(this.<>f__this.GetPremium(entity));
                component.SetCardBackSideOverride(new Player.Side?(entity.GetControllerSide()));
                component.UpdateAllComponents();
                component.Hide();
                this.actors[num] = component;
            }
        }

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
                    this.<loadingOverrideCardDef>__0 = false;
                    if (!string.IsNullOrEmpty(this.<>f__this.m_OverrideCardId) && (this.<>f__this.m_overrideCardDef == null))
                    {
                        this.<loadingOverrideCardDef>__0 = true;
                        this.<cardDefCallback>__1 = new DefLoader.LoadDefCallback<CardDef>(this.<>m__1D6);
                        DefLoader.Get().LoadCardDef(this.<>f__this.m_OverrideCardId, this.<cardDefCallback>__1, null, null);
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_01D6;

                default:
                    goto Label_01E9;
            }
            if (this.<loadingOverrideCardDef>__0)
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_01EB;
            }
            this.<assetsLoading>__2 = this.<>f__this.m_targets.Count;
            this.<actorCallback>__3 = new AssetLoader.GameObjectCallback(this.<>m__1D7);
            this.<i>__4 = 0;
            while (this.<i>__4 < this.<>f__this.m_targets.Count)
            {
                this.<targetObject>__5 = this.<>f__this.m_targets[this.<i>__4];
                this.<card>__6 = this.<targetObject>__5.GetComponent<Card>();
                this.<entity>__7 = this.<card>__6.GetEntity();
                this.<entityDef>__8 = this.<>f__this.GetEntityDef(this.<entity>__7);
                this.<premium>__9 = this.<>f__this.GetPremium(this.<entity>__7);
                this.actors.Add(null);
                this.<name>__10 = ActorNames.GetHandActor(this.<entityDef>__8, this.<premium>__9);
                AssetLoader.Get().LoadActor(this.<name>__10, this.<actorCallback>__3, this.<i>__4, false);
                this.<i>__4++;
            }
        Label_01D6:
            while (this.<assetsLoading>__2 > 0)
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_01EB;
            }
            this.$PC = -1;
        Label_01E9:
            return false;
        Label_01EB:
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

    public enum HandActorSource
    {
        CHOSEN_TARGET,
        OVERRIDE,
        SPELL_TARGET
    }

    [Serializable]
    public class SequenceData
    {
        public float m_HoldTime = 0.3f;
        public float m_NextCardHoldTime = 0.4f;
        public float m_NextCardRevealTimeMax = 0.2f;
        public float m_NextCardRevealTimeMin = 0.1f;
        public float m_RevealTime = 0.6f;
        public float m_Spacing = 2.1f;
    }

    public enum SpreadType
    {
        STACK,
        SEQUENCE
    }

    [Serializable]
    public class StackData
    {
        public float m_RevealTime = 1f;
        public float m_StaggerTime = 1.2f;
    }
}

