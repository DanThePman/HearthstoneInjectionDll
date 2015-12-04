using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FatigueSpellController : SpellController
{
    private static readonly Vector3 FATIGUE_ACTOR_FINAL_LOCAL_ROTATION = Vector3.zero;
    private static readonly Vector3 FATIGUE_ACTOR_FINAL_SCALE = Vector3.one;
    private static readonly Vector3 FATIGUE_ACTOR_INITIAL_LOCAL_ROTATION = new Vector3(270f, 270f, 0f);
    private static readonly Vector3 FATIGUE_ACTOR_START_SCALE = new Vector3(0.88f, 0.88f, 0.88f);
    private const float FATIGUE_DRAW_ANIM_TIME = 1.2f;
    private const float FATIGUE_DRAW_SCALE_TIME = 1f;
    private const float FATIGUE_HOLD_TIME = 0.8f;
    private Actor m_fatigueActor;
    private Network.HistTagChange m_fatigueTagChange;

    protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
    {
        if (!this.HasSourceCard(taskList))
        {
            return false;
        }
        this.m_fatigueTagChange = null;
        List<PowerTask> list = base.m_taskList.GetTaskList();
        for (int i = 0; i < list.Count; i++)
        {
            Network.PowerHistory power = list[i].GetPower();
            if (power.Type == Network.PowerType.TAG_CHANGE)
            {
                Network.HistTagChange change = (Network.HistTagChange) power;
                if (change.Tag == 0x16)
                {
                    this.m_fatigueTagChange = change;
                }
            }
        }
        if (this.m_fatigueTagChange == null)
        {
            return false;
        }
        Card card = taskList.GetSourceEntity().GetCard();
        base.SetSource(card);
        return true;
    }

    private void DoFinishFatigue()
    {
        Spell spell = base.GetSource().GetActor().GetSpell(SpellType.FATIGUE_DEATH);
        spell.AddFinishedCallback(new Spell.FinishedCallback(this.OnFatigueDamageFinished));
        spell.ActivateState(SpellStateType.BIRTH);
    }

    private void OnFatigueActorLoaded(string actorName, GameObject actorObject, object callbackData)
    {
        if (actorObject == null)
        {
            UnityEngine.Debug.LogWarning(string.Format("FatigueSpellController.OnFatigueActorLoaded() - FAILED to load actor \"{0}\"", actorName));
            this.DoFinishFatigue();
        }
        else
        {
            Actor component = actorObject.GetComponent<Actor>();
            if (component == null)
            {
                UnityEngine.Debug.LogWarning(string.Format("FatigueSpellController.OnFatigueActorLoaded() - ERROR actor \"{0}\" has no Actor component", actorName));
                this.DoFinishFatigue();
            }
            else
            {
                Player.Side controllerSide = base.GetSource().GetControllerSide();
                bool flag = controllerSide == Player.Side.FRIENDLY;
                this.m_fatigueActor = component;
                UberText nameText = this.m_fatigueActor.GetNameText();
                if (nameText != null)
                {
                    nameText.Text = GameStrings.Get("GAMEPLAY_FATIGUE_TITLE");
                }
                UberText powersText = this.m_fatigueActor.GetPowersText();
                if (powersText != null)
                {
                    object[] objArray1 = new object[] { this.m_fatigueTagChange.Value };
                    powersText.Text = GameStrings.Format("GAMEPLAY_FATIGUE_TEXT", objArray1);
                }
                component.SetCardBackSideOverride(new Player.Side?(controllerSide));
                component.UpdateCardBack();
                ZoneDeck deck = !flag ? GameState.Get().GetOpposingSidePlayer().GetDeckZone() : GameState.Get().GetFriendlySidePlayer().GetDeckZone();
                deck.DoFatigueGlow();
                this.m_fatigueActor.transform.localEulerAngles = FATIGUE_ACTOR_INITIAL_LOCAL_ROTATION;
                this.m_fatigueActor.transform.localScale = FATIGUE_ACTOR_START_SCALE;
                this.m_fatigueActor.transform.position = deck.transform.position;
                Vector3[] vectorArray = new Vector3[] { this.m_fatigueActor.transform.position, new Vector3(this.m_fatigueActor.transform.position.x, this.m_fatigueActor.transform.position.y + 3.6f, this.m_fatigueActor.transform.position.z), Board.Get().FindBone("FatigueCardBone").position };
                object[] args = new object[] { "path", vectorArray, "time", 1.2f, "easetype", iTween.EaseType.easeInSineOutExpo };
                iTween.MoveTo(this.m_fatigueActor.gameObject, iTween.Hash(args));
                object[] objArray3 = new object[] { "rotation", FATIGUE_ACTOR_FINAL_LOCAL_ROTATION, "time", 1.2f, "delay", 0.15f };
                iTween.RotateTo(this.m_fatigueActor.gameObject, iTween.Hash(objArray3));
                iTween.ScaleTo(this.m_fatigueActor.gameObject, FATIGUE_ACTOR_FINAL_SCALE, 1f);
                base.StartCoroutine(this.WaitThenFinishFatigue(0.8f));
            }
        }
    }

    private void OnFatigueDamageFinished(Spell spell, object userData)
    {
        spell.RemoveFinishedCallback(new Spell.FinishedCallback(this.OnFatigueDamageFinished));
        if (this.m_fatigueActor == null)
        {
            base.OnFinishedTaskList();
        }
        else
        {
            Spell spell2 = this.m_fatigueActor.GetSpell(SpellType.DEATH);
            if (spell2 == null)
            {
                base.OnFinishedTaskList();
            }
            else
            {
                Actor fatigueActor = this.m_fatigueActor;
                this.m_fatigueActor = null;
                spell2.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnFatigueDeathSpellFinished), fatigueActor);
                spell2.Activate();
                base.OnFinishedTaskList();
            }
        }
    }

    private void OnFatigueDeathSpellFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        Actor actor = (Actor) userData;
        if (actor != null)
        {
            actor.Destroy();
        }
        base.OnFinished();
    }

    protected override void OnProcessTaskList()
    {
        AssetLoader.Get().LoadActor("Card_Hand_Fatigue", new AssetLoader.GameObjectCallback(this.OnFatigueActorLoaded), null, false);
    }

    [DebuggerHidden]
    private IEnumerator WaitThenFinishFatigue(float seconds)
    {
        return new <WaitThenFinishFatigue>c__Iterator1BF { seconds = seconds, <$>seconds = seconds, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <WaitThenFinishFatigue>c__Iterator1BF : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>seconds;
        internal FatigueSpellController <>f__this;
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
                    this.<>f__this.DoFinishFatigue();
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

