using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SecretSpellController : SpellController
{
    public List<SecretBannerDef> m_BannerDefs;
    private Spell m_bannerSpell;
    public Spell m_DefaultBannerSpellPrefab;
    private Spell m_triggerSpell;

    protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
    {
        if (!this.HasSourceCard(taskList))
        {
            return false;
        }
        Entity sourceEntity = taskList.GetSourceEntity();
        Card card = sourceEntity.GetCard();
        bool flag = false;
        if (taskList.IsSourceActionOrigin() && this.InitBannerSpell(sourceEntity))
        {
            flag = true;
        }
        Spell triggerSpell = this.GetTriggerSpell(card);
        if ((triggerSpell != null) && this.InitTriggerSpell(card, triggerSpell))
        {
            flag = true;
        }
        if (!flag)
        {
            return false;
        }
        base.SetSource(card);
        return true;
    }

    [DebuggerHidden]
    private IEnumerator ContinueWithSecretEvents()
    {
        return new <ContinueWithSecretEvents>c__Iterator1C6 { <>f__this = this };
    }

    private Spell DetermineBannerSpellPrefab(Entity sourceEntity)
    {
        if (this.m_BannerDefs == null)
        {
            return null;
        }
        TAG_CLASS classTag = sourceEntity.GetClass();
        SpellClassTag tag = SpellUtils.ConvertClassTagToSpellEnum(classTag);
        if (tag == SpellClassTag.NONE)
        {
            UnityEngine.Debug.LogWarning(string.Format("{0}.DetermineBannerSpellPrefab() - entity {1} has unknown class tag {2}. Using default banner.", this, sourceEntity, classTag));
        }
        else if ((this.m_BannerDefs != null) && (this.m_BannerDefs.Count > 0))
        {
            for (int i = 0; i < this.m_BannerDefs.Count; i++)
            {
                SecretBannerDef def = this.m_BannerDefs[i];
                if (tag == def.m_HeroClass)
                {
                    return def.m_SpellPrefab;
                }
            }
            Log.Asset.Print(string.Format("{0}.DetermineBannerSpellPrefab() - class type {1} has no Banner Def. Using default banner.", this, tag), new object[0]);
        }
        return this.m_DefaultBannerSpellPrefab;
    }

    private bool FireBannerSpell()
    {
        if (this.m_bannerSpell == null)
        {
            return false;
        }
        base.StartCoroutine(this.ContinueWithSecretEvents());
        this.m_bannerSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnBannerSpellStateFinished));
        this.m_bannerSpell.Activate();
        return true;
    }

    private bool FireSecretActorSpell()
    {
        Card source = base.GetSource();
        if (!source.CanShowSecretTrigger())
        {
            return false;
        }
        source.ShowSecretTrigger();
        return true;
    }

    private bool FireTriggerSpell()
    {
        Card source = base.GetSource();
        Spell triggerSpell = this.GetTriggerSpell(source);
        if (triggerSpell == null)
        {
            return false;
        }
        if ((triggerSpell.GetPowerTaskList() != base.m_taskList) && !this.InitTriggerSpell(source, triggerSpell))
        {
            return false;
        }
        triggerSpell.AddFinishedCallback(new Spell.FinishedCallback(this.OnTriggerSpellFinished));
        triggerSpell.AddStateFinishedCallback(new Spell.StateFinishedCallback(this.OnTriggerSpellStateFinished));
        triggerSpell.SafeActivateState(SpellStateType.ACTION);
        return true;
    }

    private Spell GetTriggerSpell(Card card)
    {
        Network.HistActionStart sourceAction = base.m_taskList.GetSourceAction();
        return card.GetTriggerSpell(sourceAction.Index, true);
    }

    private bool InitBannerSpell(Entity sourceEntity)
    {
        Spell spell = this.DetermineBannerSpellPrefab(sourceEntity);
        if (spell == null)
        {
            return false;
        }
        this.m_bannerSpell = UnityEngine.Object.Instantiate<GameObject>(spell.gameObject).GetComponent<Spell>();
        return true;
    }

    private bool InitTriggerSpell(Card card, Spell triggerSpell)
    {
        if (!triggerSpell.AttachPowerTaskList(base.m_taskList))
        {
            Network.HistActionStart sourceAction = base.m_taskList.GetSourceAction();
            Log.Power.Print(string.Format("{0}.InitTriggerSpell() - FAILED to attach task list to trigger spell {1} for {2}", this, sourceAction.Index, card), new object[0]);
            return false;
        }
        return true;
    }

    private void OnBannerSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
    {
        if (this.m_bannerSpell.GetActiveState() == SpellStateType.NONE)
        {
            UnityEngine.Object.Destroy(this.m_bannerSpell.gameObject);
            this.m_bannerSpell = null;
        }
    }

    protected override void OnProcessTaskList()
    {
        base.GetSource().SetSecretTriggered(true);
        if (base.m_taskList.IsSourceActionOrigin())
        {
            this.FireSecretActorSpell();
            if (this.FireBannerSpell())
            {
                return;
            }
        }
        if (!this.FireTriggerSpell())
        {
            base.OnProcessTaskList();
        }
    }

    private void OnTriggerSpellFinished(Spell triggerSpell, object userData)
    {
        base.OnFinishedTaskList();
    }

    private void OnTriggerSpellStateFinished(Spell triggerSpell, SpellStateType prevStateType, object userData)
    {
        if (triggerSpell.GetActiveState() == SpellStateType.NONE)
        {
            base.OnFinished();
        }
    }

    [CompilerGenerated]
    private sealed class <ContinueWithSecretEvents>c__Iterator1C6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal SecretSpellController <>f__this;

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
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    goto Label_00BB;

                case 1:
                case 2:
                    if (!HistoryManager.Get().HasBigCard())
                    {
                        this.$current = null;
                        this.$PC = 2;
                    }
                    else
                    {
                        HistoryManager.Get().NotifyOfSecretSpellFinished();
                        this.$current = new WaitForSeconds(1f);
                        this.$PC = 3;
                    }
                    goto Label_00BB;

                case 3:
                    if (!this.<>f__this.FireTriggerSpell())
                    {
                        this.<>f__this.OnProcessTaskList();
                        this.$PC = -1;
                    }
                    break;
            }
            return false;
        Label_00BB:
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

