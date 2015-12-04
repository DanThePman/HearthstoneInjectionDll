using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FeignDeath : SuperSpell
{
    public GameObject m_Glow;
    public float m_Height = 1f;
    public GameObject m_RootObject;

    [DebuggerHidden]
    private IEnumerator ActionVisual()
    {
        return new <ActionVisual>c__Iterator211 { <>f__this = this };
    }

    protected override void Awake()
    {
        base.Awake();
        this.m_RootObject.SetActive(false);
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        if (!base.m_taskList.IsSourceActionOrigin())
        {
            base.OnAction(prevStateType);
        }
        else
        {
            base.m_effectsPendingFinish++;
            base.OnAction(prevStateType);
            base.m_targets.Clear();
            for (PowerTaskList list = base.m_taskList; list != null; list = list.GetNext())
            {
                foreach (PowerTask task in list.GetTaskList())
                {
                    Network.HistMetaData power = task.GetPower() as Network.HistMetaData;
                    if ((power != null) && (power.MetaType == HistoryMeta.Type.TARGET))
                    {
                        foreach (int num in power.Info)
                        {
                            Card card = GameState.Get().GetEntity(num).GetCard();
                            base.m_targets.Add(card.gameObject);
                        }
                    }
                }
            }
            base.StartCoroutine(this.ActionVisual());
        }
    }

    [CompilerGenerated]
    private sealed class <ActionVisual>c__Iterator211 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal List<GameObject>.Enumerator <$s_1214>__1;
        internal ParticleSystem[] <$s_1215>__4;
        internal int <$s_1216>__5;
        internal List<GameObject>.Enumerator <$s_1217>__7;
        internal FeignDeath <>f__this;
        internal GameObject <fx>__3;
        internal GameObject <fxObj>__8;
        internal List<GameObject> <fxObjects>__0;
        internal ParticleSystem <ps>__6;
        internal GameObject <target>__2;

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
                    this.<fxObjects>__0 = new List<GameObject>();
                    this.<$s_1214>__1 = this.<>f__this.m_targets.GetEnumerator();
                    try
                    {
                        while (this.<$s_1214>__1.MoveNext())
                        {
                            this.<target>__2 = this.<$s_1214>__1.Current;
                            this.<fx>__3 = UnityEngine.Object.Instantiate<GameObject>(this.<>f__this.m_RootObject);
                            this.<fx>__3.SetActive(true);
                            this.<fxObjects>__0.Add(this.<fx>__3);
                            this.<fx>__3.transform.position = this.<target>__2.transform.position;
                            this.<fx>__3.transform.position = new Vector3(this.<fx>__3.transform.position.x, this.<fx>__3.transform.position.y + this.<>f__this.m_Height, this.<fx>__3.transform.position.z);
                            this.<$s_1215>__4 = this.<fx>__3.GetComponentsInChildren<ParticleSystem>();
                            this.<$s_1216>__5 = 0;
                            while (this.<$s_1216>__5 < this.<$s_1215>__4.Length)
                            {
                                this.<ps>__6 = this.<$s_1215>__4[this.<$s_1216>__5];
                                this.<ps>__6.Play();
                                this.<$s_1216>__5++;
                            }
                        }
                    }
                    finally
                    {
                        this.<$s_1214>__1.Dispose();
                    }
                    this.$current = new WaitForSeconds(1f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<$s_1217>__7 = this.<fxObjects>__0.GetEnumerator();
                    try
                    {
                        while (this.<$s_1217>__7.MoveNext())
                        {
                            this.<fxObj>__8 = this.<$s_1217>__7.Current;
                            UnityEngine.Object.Destroy(this.<fxObj>__8);
                        }
                    }
                    finally
                    {
                        this.<$s_1217>__7.Dispose();
                    }
                    this.<>f__this.m_effectsPendingFinish--;
                    this.<>f__this.FinishIfPossible();
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

