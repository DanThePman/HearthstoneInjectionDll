using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VarianWrynn : SuperSpell
{
    public Spell m_deckSpellPrefab;
    public string m_perMinionSound;
    public float m_spellLeadTime = 1f;
    public Spell m_varianSpellPrefab;

    [DebuggerHidden]
    private IEnumerator DoVariansCoolThing()
    {
        return new <DoVariansCoolThing>c__Iterator22A { <>f__this = this };
    }

    private bool IsMinion(Network.HistShowEntity showEntity)
    {
        for (int i = 0; i < showEntity.Entity.Tags.Count; i++)
        {
            Network.Entity.Tag tag = showEntity.Entity.Tags[i];
            if (tag.Name == 0xca)
            {
                return (tag.Value == 4);
            }
        }
        return false;
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        base.StartCoroutine(this.DoVariansCoolThing());
    }

    [CompilerGenerated]
    private sealed class <DoVariansCoolThing>c__Iterator22A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<GameObject>.Enumerator <$s_1258>__14;
        internal VarianWrynn <>f__this;
        internal bool <complete>__12;
        internal PowerTaskList.CompleteCallback <completeCallback>__13;
        internal Entity <entity>__9;
        internal bool <foundTarget>__4;
        internal GameObject <fxObj>__15;
        internal List<GameObject> <fxObjects>__1;
        internal int <i>__6;
        internal bool <lastWasMinion>__5;
        internal Network.PowerHistory <power>__7;
        internal Network.HistShowEntity <showEntity>__8;
        internal Card <sourceCard>__0;
        internal Spell <spell>__11;
        internal Spell <spell>__2;
        internal Card <targetCard>__10;
        internal List<PowerTask> <tasks>__3;

        internal void <>m__1DB(PowerTaskList taskList, int startIndex, int count, object userData)
        {
            this.<complete>__12 = true;
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
                    this.<sourceCard>__0 = this.<>f__this.m_taskList.GetSourceEntity().GetCard();
                    this.<fxObjects>__1 = new List<GameObject>();
                    if ((this.<>f__this.m_varianSpellPrefab != null) && this.<>f__this.m_taskList.IsOrigin())
                    {
                        this.<spell>__2 = UnityEngine.Object.Instantiate<Spell>(this.<>f__this.m_varianSpellPrefab);
                        this.<fxObjects>__1.Add(this.<spell>__2.gameObject);
                        this.<spell>__2.SetSource(this.<sourceCard>__0.gameObject);
                        this.<spell>__2.Activate();
                    }
                    this.<tasks>__3 = this.<>f__this.m_taskList.GetTaskList();
                    this.<foundTarget>__4 = false;
                    this.<lastWasMinion>__5 = false;
                    this.<i>__6 = 0;
                    while (this.<i>__6 < this.<tasks>__3.Count)
                    {
                        this.<power>__7 = this.<tasks>__3[this.<i>__6].GetPower();
                        if (this.<power>__7.Type != Network.PowerType.SHOW_ENTITY)
                        {
                            goto Label_02BA;
                        }
                        this.<showEntity>__8 = (Network.HistShowEntity) this.<power>__7;
                        if (this.<foundTarget>__4)
                        {
                            goto Label_021D;
                        }
                        this.<entity>__9 = GameState.Get().GetEntity(this.<showEntity>__8.Entity.ID);
                        this.<targetCard>__10 = this.<entity>__9.GetCard();
                        this.<foundTarget>__4 = true;
                        if ((this.<>f__this.m_deckSpellPrefab == null) || !this.<>f__this.m_taskList.IsOrigin())
                        {
                            goto Label_021D;
                        }
                        this.<spell>__11 = UnityEngine.Object.Instantiate<Spell>(this.<>f__this.m_deckSpellPrefab);
                        this.<fxObjects>__1.Add(this.<spell>__11.gameObject);
                        this.<spell>__11.SetSource(this.<targetCard>__10.gameObject);
                        this.<spell>__11.Activate();
                    Label_020D:
                        while (!this.<spell>__11.IsFinished())
                        {
                            this.$current = null;
                            this.$PC = 1;
                            goto Label_035D;
                        }
                    Label_021D:
                        this.<complete>__12 = false;
                        this.<completeCallback>__13 = new PowerTaskList.CompleteCallback(this.<>m__1DB);
                        this.<>f__this.m_taskList.DoTasks(0, this.<i>__6, this.<completeCallback>__13);
                        if (this.<lastWasMinion>__5)
                        {
                            this.$current = new WaitForSeconds(this.<>f__this.m_spellLeadTime);
                            this.$PC = 2;
                            goto Label_035D;
                        }
                    Label_0280:
                        this.<lastWasMinion>__5 = this.<>f__this.IsMinion(this.<showEntity>__8);
                    Label_02AF:
                        while (!this.<complete>__12)
                        {
                            this.$current = null;
                            this.$PC = 3;
                            goto Label_035D;
                        }
                    Label_02BA:
                        this.<i>__6++;
                    }
                    this.<$s_1258>__14 = this.<fxObjects>__1.GetEnumerator();
                    try
                    {
                        while (this.<$s_1258>__14.MoveNext())
                        {
                            this.<fxObj>__15 = this.<$s_1258>__14.Current;
                            UnityEngine.Object.Destroy(this.<fxObj>__15);
                        }
                    }
                    finally
                    {
                        this.<$s_1258>__14.Dispose();
                    }
                    this.<>f__this.m_effectsPendingFinish--;
                    this.<>f__this.FinishIfPossible();
                    this.$PC = -1;
                    break;

                case 1:
                    goto Label_020D;

                case 2:
                    goto Label_0280;

                case 3:
                    goto Label_02AF;
            }
            return false;
        Label_035D:
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

