using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class TGTFood : MonoBehaviour
{
    private FoodItem m_CurrentFoodItem;
    public FoodItem m_Drink;
    public List<FoodItem> m_Food;
    private bool m_isAnimating;
    private int m_lastFoodIdx;
    public float m_NewFoodDelay = 1f;
    public bool m_Phone;
    private float m_phoneNextCheckTime;
    public int m_StartingFoodIndex;
    public GameObject m_Triangle;
    public Animator m_TriangleAnimator;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_TriangleSoundPrefab;

    private void BellAnimation()
    {
        if (!this.m_Phone)
        {
            this.m_TriangleAnimator.SetTrigger("Clicked");
        }
        if (!string.IsNullOrEmpty(this.m_TriangleSoundPrefab))
        {
            string str = FileUtils.GameAssetPathToName(this.m_TriangleSoundPrefab);
            if (!string.IsNullOrEmpty(str))
            {
                SoundManager.Get().LoadAndPlay(str, this.m_Triangle);
            }
        }
    }

    private void HandleHits()
    {
        if ((UniversalInputManager.Get().GetMouseButtonUp(0) && this.IsOver(this.m_Triangle)) && !this.m_isAnimating)
        {
            base.StartCoroutine(this.RingTheBell());
        }
    }

    private bool IsOver(GameObject go)
    {
        if (go == null)
        {
            return false;
        }
        if (!InputUtil.IsPlayMakerMouseInputAllowed(go))
        {
            return false;
        }
        if (!UniversalInputManager.Get().InputIsOver(go))
        {
            return false;
        }
        return true;
    }

    [DebuggerHidden]
    private IEnumerator RingTheBell()
    {
        return new <RingTheBell>c__Iterator12 { <>f__this = this };
    }

    private void Start()
    {
        this.m_CurrentFoodItem = this.m_Food[this.m_StartingFoodIndex];
        this.m_lastFoodIdx = this.m_StartingFoodIndex;
        this.m_CurrentFoodItem.m_FSM.gameObject.SetActive(true);
        this.m_CurrentFoodItem.m_FSM.SendEvent("Birth");
        this.m_Drink.m_FSM.gameObject.SetActive(true);
        this.m_Drink.m_FSM.SendEvent("Birth");
        if (this.m_Phone)
        {
            this.m_Triangle.SetActive(false);
        }
    }

    private void Update()
    {
        this.HandleHits();
        if ((this.m_Phone && !this.m_Triangle.activeSelf) && (UnityEngine.Time.timeSinceLevelLoad >= this.m_phoneNextCheckTime))
        {
            this.m_phoneNextCheckTime = UnityEngine.Time.timeSinceLevelLoad + 0.25f;
            bool flag = this.m_CurrentFoodItem.m_FSM.FsmVariables.FindFsmBool("isEmpty").Value;
            bool flag2 = this.m_Drink.m_FSM.FsmVariables.FindFsmBool("isEmpty").Value;
            if ((flag && flag2) && !this.m_isAnimating)
            {
                this.m_Triangle.SetActive(true);
            }
            else if (this.m_Triangle.activeSelf)
            {
                this.m_Triangle.SetActive(false);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <RingTheBell>c__Iterator12 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TGTFood <>f__this;
        internal bool <drinkEmpty>__1;
        internal bool <foodEmpty>__0;
        internal int <newFoodIdx>__2;

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
                    if (this.<>f__this.m_Phone)
                    {
                        this.<>f__this.m_Triangle.SetActive(false);
                    }
                    this.<>f__this.m_isAnimating = true;
                    this.<foodEmpty>__0 = this.<>f__this.m_CurrentFoodItem.m_FSM.FsmVariables.FindFsmBool("isEmpty").Value;
                    this.<drinkEmpty>__1 = this.<>f__this.m_Drink.m_FSM.FsmVariables.FindFsmBool("isEmpty").Value;
                    this.<>f__this.BellAnimation();
                    if (this.<foodEmpty>__0)
                    {
                        this.<>f__this.m_CurrentFoodItem.m_FSM.SendEvent("Death");
                    }
                    if (this.<drinkEmpty>__1)
                    {
                        this.<>f__this.m_Drink.m_FSM.SendEvent("Death");
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_NewFoodDelay);
                    this.$PC = 1;
                    goto Label_0296;

                case 1:
                    if (this.<>f__this.m_Phone)
                    {
                        this.<>f__this.m_Triangle.SetActive(false);
                    }
                    if (this.<foodEmpty>__0)
                    {
                        this.<newFoodIdx>__2 = UnityEngine.Random.Range(0, this.<>f__this.m_Food.Count);
                        if (this.<newFoodIdx>__2 == this.<>f__this.m_lastFoodIdx)
                        {
                            this.<newFoodIdx>__2 = UnityEngine.Random.Range(0, this.<>f__this.m_Food.Count);
                            if (this.<newFoodIdx>__2 == this.<>f__this.m_lastFoodIdx)
                            {
                                this.<newFoodIdx>__2 = this.<>f__this.m_lastFoodIdx - 1;
                                if (this.<newFoodIdx>__2 < 0)
                                {
                                    this.<newFoodIdx>__2 = 0;
                                }
                            }
                        }
                        this.<>f__this.m_lastFoodIdx = this.<newFoodIdx>__2;
                        this.<>f__this.m_CurrentFoodItem = this.<>f__this.m_Food[this.<newFoodIdx>__2];
                        this.<>f__this.m_CurrentFoodItem.m_FSM.gameObject.SetActive(true);
                        this.<>f__this.m_CurrentFoodItem.m_FSM.SendEvent("Birth");
                    }
                    if (this.<drinkEmpty>__1)
                    {
                        this.<>f__this.m_Drink.m_FSM.SendEvent("Birth");
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_NewFoodDelay);
                    this.$PC = 2;
                    goto Label_0296;

                case 2:
                    this.<>f__this.m_isAnimating = false;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0296:
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

    [Serializable]
    public class FoodItem
    {
        public PlayMakerFSM m_FSM;
    }
}

