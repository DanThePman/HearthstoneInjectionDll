using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class MimironsHead : SuperSpell
{
    private float m_absorbTime = 1f;
    public GameObject m_background;
    private Map<GameObject, List<GameObject>> m_cleanup = new Map<GameObject, List<GameObject>>();
    private Color m_clear = new Color(1f, 1f, 1f, 0f);
    private float m_flashDelay = 0.15f;
    private float m_glowTime = 0.5f;
    public GameObject m_highPosBone;
    private bool m_isNegFlash;
    private List<Card> m_mechMinions = new List<Card>();
    private Card m_mimiron;
    public GameObject m_mimironElectricity;
    public GameObject m_mimironFlare;
    public GameObject m_mimironGlow;
    private float m_mimironHighTime = 1.5f;
    public GameObject m_mimironNegative;
    public GameObject m_minionElectricity;
    public GameObject m_minionGlow;
    private float m_minionHighTime = 2f;
    public GameObject m_minionPosBone;
    public string m_perMinionSound;
    public GameObject m_root;
    private float m_sparkDelay = 0.3f;
    public string[] m_startSounds;
    private Card m_volt;
    private Transform m_voltParent;
    public Spell m_voltSpawnOverrideSpell;
    private PowerTaskList m_waitForTaskList;

    private void AbsorbMinions()
    {
        Vector3 vector = new Vector3(0f, -1f, 0f);
        for (int i = 0; i < this.m_mechMinions.Count; i++)
        {
            <AbsorbMinions>c__AnonStorey350 storey = new <AbsorbMinions>c__AnonStorey350 {
                <>f__this = this
            };
            float num2 = (this.m_absorbTime / ((float) this.m_mechMinions.Count)) * i;
            storey.minion = this.m_mechMinions[i].GetActor().gameObject;
            if (i < (this.m_mechMinions.Count - 1))
            {
                object[] args = new object[] { "position", this.m_highPosBone.transform.localPosition + vector, "easetype", iTween.EaseType.easeInOutSine, "delay", (this.m_glowTime + num2) + this.m_sparkDelay, "time", 0.5f, "oncomplete", new Action<object>(storey.<>m__1C2) };
                iTween.MoveTo(storey.minion, iTween.Hash(args));
            }
            else
            {
                object[] objArray2 = new object[] { "position", this.m_highPosBone.transform.localPosition + vector, "easetype", iTween.EaseType.easeInOutSine, "delay", (this.m_glowTime + num2) + this.m_sparkDelay, "time", 0.5f, "oncomplete", new Action<object>(storey.<>m__1C3) };
                iTween.MoveTo(storey.minion, iTween.Hash(objArray2));
            }
        }
        this.m_isNegFlash = true;
        base.StartCoroutine(this.MimironNegativeFX());
    }

    public override bool AddPowerTargets()
    {
        if (!base.CanAddPowerTargets())
        {
            return false;
        }
        Card card = base.m_taskList.GetSourceEntity().GetCard();
        if (base.m_taskList.IsOrigin())
        {
            List<PowerTaskList> list = new List<PowerTaskList>();
            for (PowerTaskList list2 = base.m_taskList; list2 != null; list2 = list2.GetNext())
            {
                list.Add(list2);
            }
            foreach (PowerTaskList list3 in list)
            {
                foreach (PowerTask task in list3.GetTaskList())
                {
                    Network.PowerHistory power = task.GetPower();
                    if (power.Type == Network.PowerType.TAG_CHANGE)
                    {
                        Network.HistTagChange change = power as Network.HistTagChange;
                        if ((change.Tag == 360) && (change.Value == 1))
                        {
                            Entity entity = GameState.Get().GetEntity(change.Entity);
                            if (entity == null)
                            {
                                UnityEngine.Debug.LogWarning(string.Format("{0}.AddPowerTargets() - WARNING trying to target entity with id {1} but there is no entity with that id", this, change.Entity));
                                continue;
                            }
                            Card item = entity.GetCard();
                            if (item != card)
                            {
                                this.m_mechMinions.Add(item);
                            }
                            else
                            {
                                this.m_mimiron = item;
                            }
                            this.m_waitForTaskList = list3;
                        }
                    }
                    if (power.Type == Network.PowerType.FULL_ENTITY)
                    {
                        Network.HistFullEntity entity3 = power as Network.HistFullEntity;
                        Network.Entity entity4 = entity3.Entity;
                        Entity entity5 = GameState.Get().GetEntity(entity4.ID);
                        if (entity5 == null)
                        {
                            UnityEngine.Debug.LogWarning(string.Format("{0}.AddPowerTargets() - WARNING trying to target entity with id {1} but there is no entity with that id", this, entity4.ID));
                        }
                        else
                        {
                            Card card3 = entity5.GetCard();
                            object[] args = new object[] { entity5.GetName() };
                            Log.Becca.Print("Found V-07-TR-0N: {0}", args);
                            this.m_volt = card3;
                            this.m_waitForTaskList = list3;
                        }
                    }
                }
            }
            if (((this.m_volt != null) && (this.m_mimiron != null)) && (this.m_mechMinions.Count > 0))
            {
                this.m_mimiron.IgnoreDeath(true);
                foreach (Card card4 in this.m_mechMinions)
                {
                    card4.IgnoreDeath(true);
                }
                foreach (Card card5 in card.GetController().GetBattlefieldZone().GetCards())
                {
                    card5.SetDoNotSort(true);
                }
            }
            else
            {
                this.m_volt = null;
                this.m_mimiron = null;
                this.m_mechMinions.Clear();
            }
        }
        if (((this.m_volt == null) || (this.m_mimiron == null)) || ((this.m_mechMinions.Count == 0) || (base.m_taskList != this.m_waitForTaskList)))
        {
            return false;
        }
        foreach (Card card6 in card.GetController().GetBattlefieldZone().GetCards())
        {
            card6.SetDoNotSort(true);
        }
        return true;
    }

    private void DestroyMinions()
    {
        foreach (Card card in this.m_mechMinions)
        {
            card.IgnoreDeath(false);
            card.SetDoNotSort(false);
            card.GetActor().Destroy();
        }
        this.m_mimiron.IgnoreDeath(false);
        this.m_mimiron.SetDoNotSort(false);
        this.m_mimiron.GetActor().Destroy();
    }

    private void DropV07tron()
    {
        object[] args = new object[] { "position", Vector3.zero, "time", 0.3f, "islocal", true };
        iTween.MoveTo(this.m_volt.GetActor().gameObject, iTween.Hash(args));
        this.Finish();
    }

    private void FadeInBackground()
    {
        this.m_background.SetActive(true);
        this.m_background.GetComponent<Renderer>().material.SetColor("_Color", this.m_clear);
        HighlightState componentInChildren = this.m_volt.GetActor().gameObject.GetComponentInChildren<HighlightState>();
        if (componentInChildren != null)
        {
            componentInChildren.Hide();
        }
        object[] args = new object[] { "r", 1f, "g", 1f, "b", 1f, "a", 1f, "time", 0.5f, "oncomplete", newVal => this.MimironPowerUp() };
        iTween.ColorTo(this.m_background, iTween.Hash(args));
    }

    private void FadeOutBackground()
    {
        this.m_mimironGlow.SetActive(false);
        this.m_mimironFlare.SetActive(false);
        object[] args = new object[] { "r", 1f, "g", 1f, "b", 1f, "a", 0f, "time", 0.5f, "oncomplete", newVal => this.RaiseVolt() };
        iTween.ColorTo(this.m_background, iTween.Hash(args));
    }

    private void Finish()
    {
        this.m_volt = null;
        this.m_mimiron = null;
        this.m_mechMinions.Clear();
        base.m_effectsPendingFinish--;
        base.FinishIfPossible();
    }

    private void FlareMimiron()
    {
        this.m_mimironGlow.GetComponent<Renderer>().material.SetColor("_TintColor", this.m_clear);
        this.m_mimironFlare.GetComponent<Renderer>().material.SetColor("_TintColor", this.m_clear);
        this.m_mimironGlow.SetActive(true);
        this.m_mimironFlare.SetActive(true);
        object[] args = new object[] { "from", 0f, "to", 0.7f, "time", 0.3, "onupdate", newVal => this.SetGlow(this.m_mimironGlow.GetComponent<Renderer>().material, (float) newVal, "_TintColor") };
        iTween.ValueTo(this.m_mimironGlow, iTween.Hash(args));
        object[] objArray2 = new object[] { "from", 0f, "to", 2.5f, "time", 0.3f, "onupdate", newVal => this.SetGlow(this.m_mimironFlare.GetComponent<Renderer>().material, (float) newVal, "_Intensity"), "oncomplete", newVal => this.UnflareMimiron() };
        iTween.ValueTo(this.m_mimironFlare, iTween.Hash(objArray2));
    }

    [DebuggerHidden]
    private IEnumerator MimironNegativeFX()
    {
        return new <MimironNegativeFX>c__Iterator218 { <>f__this = this };
    }

    private void MimironPowerUp()
    {
        this.m_mimironElectricity.GetComponent<ParticleSystem>().Play();
        for (int i = 0; i < this.m_mechMinions.Count; i++)
        {
            <MimironPowerUp>c__AnonStorey34F storeyf = new <MimironPowerUp>c__AnonStorey34F {
                <>f__this = this
            };
            GameObject gameObject = this.m_mechMinions[i].GetActor().gameObject;
            storeyf.minionGlow = UnityEngine.Object.Instantiate<GameObject>(this.m_minionGlow);
            if (!this.m_cleanup.ContainsKey(gameObject))
            {
                this.m_cleanup.Add(gameObject, new List<GameObject>());
            }
            this.m_cleanup[gameObject].Add(storeyf.minionGlow);
            storeyf.minionGlow.transform.parent = gameObject.transform;
            storeyf.minionGlow.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            float num2 = (this.m_absorbTime / ((float) this.m_mechMinions.Count)) * i;
            storeyf.minionGlow.GetComponent<Renderer>().material.SetColor("_TintColor", this.m_clear);
            SceneUtils.EnableRenderers(storeyf.minionGlow, true);
            if (i < (this.m_mechMinions.Count - 1))
            {
                object[] args = new object[] { "from", 0f, "to", 1f, "time", this.m_glowTime, "delay", (0.1f + num2) + this.m_sparkDelay, "onstart", new Action<object>(storeyf.<>m__1BB), "onupdate", new Action<object>(storeyf.<>m__1BC) };
                iTween.ValueTo(storeyf.minionGlow, iTween.Hash(args));
                object[] objArray2 = new object[] { "from", 1f, "to", 0f, "time", this.m_glowTime, "delay", ((0.1f + num2) + this.m_sparkDelay) + this.m_glowTime, "onupdate", new Action<object>(storeyf.<>m__1BD) };
                iTween.ValueTo(storeyf.minionGlow, iTween.Hash(objArray2));
            }
            else
            {
                object[] objArray3 = new object[] { "from", 0f, "to", 1f, "time", this.m_glowTime, "delay", (0.1f + num2) + this.m_sparkDelay, "onstart", new Action<object>(storeyf.<>m__1BE), "onupdate", new Action<object>(storeyf.<>m__1BF), "oncomplete", new Action<object>(storeyf.<>m__1C0) };
                iTween.ValueTo(storeyf.minionGlow, iTween.Hash(objArray3));
                object[] objArray4 = new object[] { "from", 1f, "to", 0f, "time", this.m_glowTime, "delay", ((0.1f + num2) + this.m_sparkDelay) + this.m_glowTime, "onupdate", new Action<object>(storeyf.<>m__1C1) };
                iTween.ValueTo(storeyf.minionGlow, iTween.Hash(objArray4));
            }
        }
    }

    private void MinionCleanup(GameObject minion)
    {
        if (this.m_cleanup.ContainsKey(minion))
        {
            foreach (GameObject obj2 in this.m_cleanup[minion])
            {
                if (obj2 != null)
                {
                    UnityEngine.Object.Destroy(obj2);
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator MinionPlayFX(GameObject minion, GameObject FX, float delay)
    {
        return new <MinionPlayFX>c__Iterator217 { FX = FX, minion = minion, delay = delay, <$>FX = FX, <$>minion = minion, <$>delay = delay, <>f__this = this };
    }

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.m_effectsPendingFinish++;
        base.OnAction(prevStateType);
        if (this.m_voltSpawnOverrideSpell != null)
        {
            this.m_volt.OverrideCustomSpawnSpell(UnityEngine.Object.Instantiate<Spell>(this.m_voltSpawnOverrideSpell));
        }
        object[] args = new object[] { this.m_volt.GetEntity().GetName() };
        Log.Becca.Print("V-07-TR-0N: {0}", args);
        base.StartCoroutine(this.TransformEffect());
    }

    private void RaiseVolt()
    {
        this.m_mimironElectricity.GetComponent<ParticleSystem>().Stop();
        this.m_background.GetComponent<Renderer>().material.SetColor("_Color", this.m_clear);
        this.m_background.SetActive(false);
        GameObject gameObject = this.m_volt.GetActor().gameObject;
        gameObject.transform.parent = this.m_voltParent;
        object[] args = new object[] { "position", gameObject.transform.localPosition + new Vector3(0f, 3f, 0f), "time", 0.2f, "islocal", true, "oncomplete", newVal => this.DropV07tron() };
        iTween.MoveTo(gameObject, iTween.Hash(args));
    }

    private void SetGlow(Material glowMat, float newVal, string colorVal = "_TintColor")
    {
        glowMat.SetColor(colorVal, Color.Lerp(this.m_clear, Color.white, newVal));
    }

    [DebuggerHidden]
    private IEnumerator TransformEffect()
    {
        return new <TransformEffect>c__Iterator216 { <>f__this = this };
    }

    private void TransformMinions()
    {
        float num = 1f;
        Vector3 vector = new Vector3(0f, 0f, 2.3f);
        List<int> list = new List<int>();
        for (int i = 0; i < this.m_mechMinions.Count; i++)
        {
            list.Add(i);
        }
        List<int> list2 = new List<int>();
        for (int j = 0; j < this.m_mechMinions.Count; j++)
        {
            int index = UnityEngine.Random.Range(0, list.Count);
            list2.Add(list[index]);
            list.RemoveAt(index);
        }
        for (int k = 0; k < this.m_mechMinions.Count; k++)
        {
            Vector3 vector2 = (Vector3) (Quaternion.Euler(0f, (float) (((360 / this.m_mechMinions.Count) * list2[k]) + 15), 0f) * vector);
            this.m_minionPosBone.transform.localPosition = this.m_highPosBone.transform.localPosition + vector2;
            GameObject gameObject = this.m_mechMinions[k].GetActor().gameObject;
            float num6 = (num / ((float) this.m_mechMinions.Count)) * k;
            base.StartCoroutine(this.MinionPlayFX(gameObject, this.m_minionElectricity, num6 / 2f));
            List<Vector3> list3 = new List<Vector3>();
            Vector3 vector3 = new Vector3(UnityEngine.Random.Range((float) -2f, (float) 2f), 0f, UnityEngine.Random.Range((float) -2f, (float) 2f));
            list3.Add((gameObject.transform.position + ((Vector3) ((this.m_minionPosBone.transform.localPosition - gameObject.transform.position) / 4f))) + vector3);
            list3.Add(this.m_minionPosBone.transform.localPosition);
            if (k < (this.m_mechMinions.Count - 1))
            {
                object[] args = new object[] { "path", list3.ToArray(), "easetype", iTween.EaseType.easeInOutSine, "delay", num6, "time", this.m_minionHighTime / ((float) this.m_mechMinions.Count) };
                iTween.MoveTo(gameObject, iTween.Hash(args));
            }
            else
            {
                object[] objArray2 = new object[] { "path", list3.ToArray(), "easetype", iTween.EaseType.easeInOutSine, "delay", num6, "time", this.m_minionHighTime / ((float) this.m_mechMinions.Count), "oncomplete", newVal => this.FadeInBackground() };
                iTween.MoveTo(gameObject, iTween.Hash(objArray2));
            }
        }
    }

    private void UnflareMimiron()
    {
        this.m_volt.SetDoNotSort(false);
        ZonePlay battlefieldZone = this.m_volt.GetController().GetBattlefieldZone();
        foreach (Card card in battlefieldZone.GetCards())
        {
            card.SetDoNotSort(false);
        }
        battlefieldZone.UpdateLayout();
        this.DestroyMinions();
        this.m_volt.GetActor().Show();
        this.m_mimironGlow.GetComponent<Renderer>().material.SetColor("_TintColor", this.m_clear);
        this.m_mimironFlare.GetComponent<Renderer>().material.SetColor("_TintColor", this.m_clear);
        this.m_mimironGlow.SetActive(true);
        this.m_mimironFlare.SetActive(true);
        object[] args = new object[] { "from", 0.7f, "to", 0f, "time", 0.3, "onupdate", newVal => this.SetGlow(this.m_mimironGlow.GetComponent<Renderer>().material, (float) newVal, "_TintColor") };
        iTween.ValueTo(this.m_mimironGlow, iTween.Hash(args));
        object[] objArray2 = new object[] { "from", 2.5f, "to", 0f, "time", 0.3f, "onupdate", newVal => this.SetGlow(this.m_mimironFlare.GetComponent<Renderer>().material, (float) newVal, "_Intensity"), "oncomplete", newVal => this.FadeOutBackground() };
        iTween.ValueTo(this.m_mimironFlare, iTween.Hash(objArray2));
        this.m_isNegFlash = false;
        this.OnSpellFinished();
    }

    [CompilerGenerated]
    private sealed class <AbsorbMinions>c__AnonStorey350
    {
        internal MimironsHead <>f__this;
        internal GameObject minion;

        internal void <>m__1C2(object newVal)
        {
            this.<>f__this.MinionCleanup(this.minion);
        }

        internal void <>m__1C3(object newVal)
        {
            this.<>f__this.MinionCleanup(this.minion);
            this.<>f__this.FlareMimiron();
        }
    }

    [CompilerGenerated]
    private sealed class <MimironNegativeFX>c__Iterator218 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal MimironsHead <>f__this;

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
                    break;

                case 1:
                    this.<>f__this.m_mimironNegative.SetActive(!this.<>f__this.m_mimironNegative.activeSelf);
                    if (this.<>f__this.m_flashDelay > 0.05f)
                    {
                        this.<>f__this.m_flashDelay -= 0.01f;
                    }
                    break;

                default:
                    goto Label_00BF;
            }
            if (this.<>f__this.m_isNegFlash)
            {
                this.$current = new WaitForSeconds(this.<>f__this.m_flashDelay);
                this.$PC = 1;
                return true;
            }
            this.<>f__this.m_mimironNegative.SetActive(false);
            this.$PC = -1;
        Label_00BF:
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
    private sealed class <MimironPowerUp>c__AnonStorey34F
    {
        internal MimironsHead <>f__this;
        internal GameObject minionGlow;

        internal void <>m__1BB(object newVal)
        {
            SoundManager.Get().LoadAndPlay(this.<>f__this.m_perMinionSound);
        }

        internal void <>m__1BC(object newVal)
        {
            this.<>f__this.SetGlow(this.minionGlow.GetComponent<Renderer>().material, (float) newVal, "_TintColor");
        }

        internal void <>m__1BD(object newVal)
        {
            this.<>f__this.SetGlow(this.minionGlow.GetComponent<Renderer>().material, (float) newVal, "_TintColor");
        }

        internal void <>m__1BE(object newVal)
        {
            SoundManager.Get().LoadAndPlay(this.<>f__this.m_perMinionSound);
        }

        internal void <>m__1BF(object newVal)
        {
            this.<>f__this.SetGlow(this.minionGlow.GetComponent<Renderer>().material, (float) newVal, "_TintColor");
        }

        internal void <>m__1C0(object newVal)
        {
            this.<>f__this.AbsorbMinions();
        }

        internal void <>m__1C1(object newVal)
        {
            this.<>f__this.SetGlow(this.minionGlow.GetComponent<Renderer>().material, (float) newVal, "_TintColor");
        }
    }

    [CompilerGenerated]
    private sealed class <MinionPlayFX>c__Iterator217 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delay;
        internal GameObject <$>FX;
        internal GameObject <$>minion;
        internal MimironsHead <>f__this;
        internal GameObject <minionFX>__0;
        internal float delay;
        internal GameObject FX;
        internal GameObject minion;

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
                    this.<minionFX>__0 = UnityEngine.Object.Instantiate<GameObject>(this.FX);
                    this.<minionFX>__0.transform.parent = this.minion.transform;
                    this.<minionFX>__0.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                    if (!this.<>f__this.m_cleanup.ContainsKey(this.minion))
                    {
                        this.<>f__this.m_cleanup.Add(this.minion, new List<GameObject>());
                    }
                    this.<>f__this.m_cleanup[this.minion].Add(this.<minionFX>__0);
                    this.$current = new WaitForSeconds(this.delay);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<minionFX>__0.GetComponent<ParticleSystem>().Play();
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
    private sealed class <TransformEffect>c__Iterator216 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string[] <$s_1225>__0;
        internal int <$s_1226>__1;
        internal MimironsHead <>f__this;
        internal GameObject <mimiron>__4;
        internal string <sound>__2;
        internal GameObject <volt>__3;

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
                    this.<$s_1225>__0 = this.<>f__this.m_startSounds;
                    this.<$s_1226>__1 = 0;
                    while (this.<$s_1226>__1 < this.<$s_1225>__0.Length)
                    {
                        this.<sound>__2 = this.<$s_1225>__0[this.<$s_1226>__1];
                        SoundManager.Get().LoadAndPlay(this.<sound>__2);
                        this.<$s_1226>__1++;
                    }
                    this.<>f__this.m_volt.SetDoNotSort(true);
                    this.<>f__this.m_taskList.DoAllTasks();
                    break;

                case 1:
                    break;

                case 2:
                    this.<>f__this.TransformMinions();
                    this.$PC = -1;
                    goto Label_0265;

                default:
                    goto Label_0265;
            }
            while (!this.<>f__this.m_taskList.IsComplete())
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0267;
            }
            this.<>f__this.m_volt.GetActor().Hide();
            this.<volt>__3 = this.<>f__this.m_volt.GetActor().gameObject;
            this.<>f__this.m_voltParent = this.<volt>__3.transform.parent;
            this.<volt>__3.transform.parent = this.<>f__this.m_highPosBone.transform;
            this.<volt>__3.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            this.<>f__this.m_root.transform.parent = null;
            this.<>f__this.m_root.transform.localPosition = Vector3.zero;
            this.<mimiron>__4 = this.<>f__this.m_mimiron.gameObject;
            object[] args = new object[] { "position", this.<>f__this.m_highPosBone.transform.localPosition, "easetype", iTween.EaseType.easeOutQuart, "time", this.<>f__this.m_mimironHighTime, "delay", 0.5f };
            iTween.MoveTo(this.<mimiron>__4, iTween.Hash(args));
            this.$current = new WaitForSeconds(0.5f + (this.<>f__this.m_mimironHighTime / 5f));
            this.$PC = 2;
            goto Label_0267;
        Label_0265:
            return false;
        Label_0267:
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

