using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class ManaCrystalMgr : MonoBehaviour
{
    [CompilerGenerated]
    private static Predicate<ManaCrystal> <>f__am$cache17;
    [CompilerGenerated]
    private static Predicate<ManaCrystal> <>f__am$cache18;
    private const float ANIMATE_TIME = 0.6f;
    public GameObject friendlyManaCounter;
    private Texture friendlyManaGemTexture;
    private readonly string GEM_FLIP_ANIM_NAME = "Resource_Large_phone_Flip";
    private const float GEM_FLIP_TEXT_FADE_TIME = 0.1f;
    private int m_additionalOverloadedCrystalsOwedNextTurn;
    private int m_additionalOverloadedCrystalsOwedThisTurn;
    public ManaCrystalEventSpells m_eventSpells;
    private GameObject m_friendlyManaGem;
    private UberText m_friendlyManaText;
    private float m_manaCrystalWidth;
    private int m_numCrystalsLoading;
    private int m_numQueuedToReady;
    private int m_numQueuedToSpawn;
    private int m_numQueuedToSpend;
    private bool m_overloadLocksAreShowing;
    private List<ManaCrystal> m_permanentCrystals;
    private int m_proposedManaSourceEntID = -1;
    private List<ManaCrystal> m_temporaryCrystals;
    public Transform manaGemBone;
    [CustomEditField(T=EditType.GAME_OBJECT)]
    public String_MobileOverride manaLockPrefab;
    public SlidingTray manaTrayPhone;
    private static ManaCrystalMgr s_instance;
    private const float SECS_BETW_MANA_READIES = 0.2f;
    private const float SECS_BETW_MANA_SPAWNS = 0.2f;
    private const float SECS_BETW_MANA_SPENDS = 0.2f;
    public Material tempManaCrystalMaterial;
    public Material tempManaCrystalProposedQuadMaterial;

    public void AddManaCrystals(int numCrystals, bool isTurnStart)
    {
        for (int i = 0; i < numCrystals; i++)
        {
            GameState.Get().GetGameEntity().NotifyOfManaCrystalSpawned();
            base.StartCoroutine(this.WaitThenAddManaCrystal(false, isTurnStart));
        }
    }

    public void AddTempManaCrystals(int numCrystals)
    {
        for (int i = 0; i < numCrystals; i++)
        {
            base.StartCoroutine(this.WaitThenAddManaCrystal(true, false));
        }
    }

    private void Awake()
    {
        s_instance = this;
        if (base.gameObject.GetComponent<AudioSource>() == null)
        {
            base.gameObject.AddComponent<AudioSource>();
        }
    }

    public void CancelAllProposedMana(Entity entity)
    {
        if ((entity != null) && (this.m_proposedManaSourceEntID == entity.GetEntityId()))
        {
            this.m_proposedManaSourceEntID = -1;
            this.m_eventSpells.m_proposeUsageSpell.ActivateState(SpellStateType.DEATH);
            for (int i = 0; i < this.m_temporaryCrystals.Count; i++)
            {
                if (this.m_temporaryCrystals[i].state == ManaCrystal.State.PROPOSED)
                {
                    this.m_temporaryCrystals[i].state = ManaCrystal.State.READY;
                }
            }
            for (int j = this.m_permanentCrystals.Count - 1; j >= 0; j--)
            {
                if (this.m_permanentCrystals[j].state == ManaCrystal.State.PROPOSED)
                {
                    this.m_permanentCrystals[j].state = ManaCrystal.State.READY;
                }
            }
        }
    }

    private void DestroyManaCrystal()
    {
        if (this.m_permanentCrystals.Count > 0)
        {
            int index = 0;
            ManaCrystal crystal = this.m_permanentCrystals[index];
            this.m_permanentCrystals.RemoveAt(index);
            crystal.GetComponent<ManaCrystal>().Destroy();
            this.UpdateLayout();
            base.StartCoroutine(this.UpdatePermanentCrystalStates());
        }
    }

    public void DestroyManaCrystals(int numCrystals)
    {
        base.StartCoroutine(this.WaitThenDestroyManaCrystals(false, numCrystals));
    }

    private void DestroyTempManaCrystal()
    {
        if (this.m_temporaryCrystals.Count > 0)
        {
            int index = this.m_temporaryCrystals.Count - 1;
            ManaCrystal crystal = this.m_temporaryCrystals[index];
            this.m_temporaryCrystals.RemoveAt(index);
            crystal.GetComponent<ManaCrystal>().Destroy();
            this.UpdateLayout();
        }
    }

    public void DestroyTempManaCrystals(int numCrystals)
    {
        base.StartCoroutine(this.WaitThenDestroyManaCrystals(true, numCrystals));
    }

    public static ManaCrystalMgr Get()
    {
        return s_instance;
    }

    public Vector3 GetManaCrystalSpawnPosition()
    {
        return base.transform.position;
    }

    public int GetSpendableManaCrystals()
    {
        int num = 0;
        for (int i = 0; i < this.m_temporaryCrystals.Count; i++)
        {
            ManaCrystal crystal = this.m_temporaryCrystals[i];
            if (crystal.state == ManaCrystal.State.READY)
            {
                num++;
            }
        }
        for (int j = 0; j < this.m_permanentCrystals.Count; j++)
        {
            ManaCrystal crystal2 = this.m_permanentCrystals[j];
            if ((crystal2.state == ManaCrystal.State.READY) && !crystal2.IsOverloaded())
            {
                num++;
            }
        }
        return num;
    }

    public float GetWidth()
    {
        if (this.m_permanentCrystals.Count == 0)
        {
            return 0f;
        }
        return ((this.m_permanentCrystals[0].transform.FindChild("Gem_Mana").GetComponent<Renderer>().bounds.size.x * this.m_permanentCrystals.Count) * this.m_temporaryCrystals.Count);
    }

    public void HandleSameTurnOverloadChanged(int crystalsChanged)
    {
        if (crystalsChanged > 0)
        {
            this.MarkCrystalsOwedForOverload(crystalsChanged);
        }
        else if (crystalsChanged < 0)
        {
            this.ReclaimCrystalsOwedForOverload(-crystalsChanged);
        }
    }

    public void HidePhoneManaTray()
    {
        this.m_friendlyManaGem.GetComponent<Animation>()[this.GEM_FLIP_ANIM_NAME].speed = -1f;
        if (this.m_friendlyManaGem.GetComponent<Animation>()[this.GEM_FLIP_ANIM_NAME].time == 0f)
        {
            this.m_friendlyManaGem.GetComponent<Animation>()[this.GEM_FLIP_ANIM_NAME].time = this.m_friendlyManaGem.GetComponent<Animation>()[this.GEM_FLIP_ANIM_NAME].length;
        }
        this.m_friendlyManaGem.GetComponent<Animation>().Play(this.GEM_FLIP_ANIM_NAME);
        object[] args = new object[] { "from", this.m_friendlyManaText.TextAlpha, "to", 1f, "time", 0.1f, "onupdate", newVal => this.m_friendlyManaText.TextAlpha = (float) newVal };
        iTween.ValueTo(base.gameObject, iTween.Hash(args));
        this.manaTrayPhone.ToggleTraySlider(false, null, true);
    }

    private void LoadCrystalCallback(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_numCrystalsLoading--;
        if (this.m_manaCrystalWidth <= 0f)
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                this.m_manaCrystalWidth = 0.33f;
            }
            else
            {
                this.m_manaCrystalWidth = actorObject.transform.FindChild("Gem_Mana").GetComponent<Renderer>().bounds.size.x;
            }
        }
        LoadCrystalCallbackData data = callbackData as LoadCrystalCallbackData;
        ManaCrystal component = actorObject.GetComponent<ManaCrystal>();
        if (data.IsTempCrystal)
        {
            component.MarkAsTemp();
            this.m_temporaryCrystals.Add(component);
        }
        else
        {
            this.m_permanentCrystals.Add(component);
            if (data.IsTurnStart)
            {
                if (this.m_additionalOverloadedCrystalsOwedThisTurn > 0)
                {
                    component.PayOverload();
                    this.m_additionalOverloadedCrystalsOwedThisTurn--;
                }
            }
            else if (this.m_additionalOverloadedCrystalsOwedNextTurn > 0)
            {
                component.state = ManaCrystal.State.USED;
                component.MarkAsOwedForOverload();
                this.m_additionalOverloadedCrystalsOwedNextTurn--;
            }
            base.StartCoroutine(this.UpdatePermanentCrystalStates());
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            component.transform.parent = this.manaGemBone.transform.parent;
            component.transform.localRotation = this.manaGemBone.transform.localRotation;
            component.transform.localScale = this.manaGemBone.transform.localScale;
        }
        else
        {
            component.transform.parent = base.transform;
        }
        component.transform.localPosition = Vector3.zero;
        component.PlayCreateAnimation();
        SoundManager.Get().LoadAndPlay("mana_crystal_add", base.gameObject);
        this.UpdateLayout();
    }

    public void MarkCrystalsOwedForOverload(int numCrystals)
    {
        if (numCrystals > 0)
        {
            this.m_overloadLocksAreShowing = true;
        }
        int num = 0;
        for (int i = 0; numCrystals != num; i++)
        {
            if (i == this.m_permanentCrystals.Count)
            {
                this.m_additionalOverloadedCrystalsOwedNextTurn += numCrystals - num;
                break;
            }
            ManaCrystal crystal = this.m_permanentCrystals[i];
            if (!crystal.IsOwedForOverload())
            {
                crystal.MarkAsOwedForOverload();
                num++;
            }
        }
    }

    public void OnCurrentPlayerChanged()
    {
        this.m_additionalOverloadedCrystalsOwedThisTurn = this.m_additionalOverloadedCrystalsOwedNextTurn;
        this.m_additionalOverloadedCrystalsOwedNextTurn = 0;
        if (this.m_additionalOverloadedCrystalsOwedThisTurn > 0)
        {
            this.m_overloadLocksAreShowing = true;
        }
        else
        {
            this.m_overloadLocksAreShowing = false;
        }
        for (int i = 0; i < this.m_permanentCrystals.Count; i++)
        {
            ManaCrystal crystal = this.m_permanentCrystals[i];
            if (crystal.IsOverloaded())
            {
                crystal.UnlockOverload();
            }
            if (crystal.IsOwedForOverload())
            {
                this.m_overloadLocksAreShowing = true;
                crystal.PayOverload();
            }
            else if (this.m_additionalOverloadedCrystalsOwedThisTurn > 0)
            {
                crystal.PayOverload();
                this.m_additionalOverloadedCrystalsOwedThisTurn--;
            }
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void ProposeManaCrystalUsage(Entity entity)
    {
        if (entity != null)
        {
            this.m_proposedManaSourceEntID = entity.GetEntityId();
            int cost = entity.GetCost();
            this.m_eventSpells.m_proposeUsageSpell.ActivateState(SpellStateType.BIRTH);
            int num2 = 0;
            for (int i = this.m_temporaryCrystals.Count - 1; i >= 0; i--)
            {
                if (this.m_temporaryCrystals[i].state == ManaCrystal.State.USED)
                {
                    Log.Rachelle.Print("Found a SPENT temporary mana crystal... this shouldn't happen!", new object[0]);
                }
                else if (num2 < cost)
                {
                    this.m_temporaryCrystals[i].state = ManaCrystal.State.PROPOSED;
                    num2++;
                }
                else
                {
                    this.m_temporaryCrystals[i].state = ManaCrystal.State.READY;
                }
            }
            for (int j = 0; j < this.m_permanentCrystals.Count; j++)
            {
                if ((this.m_permanentCrystals[j].state != ManaCrystal.State.USED) && !this.m_permanentCrystals[j].IsOverloaded())
                {
                    if (num2 < cost)
                    {
                        this.m_permanentCrystals[j].state = ManaCrystal.State.PROPOSED;
                        num2++;
                    }
                    else
                    {
                        this.m_permanentCrystals[j].state = ManaCrystal.State.READY;
                    }
                }
            }
        }
    }

    private void ReadyManaCrystal()
    {
        base.StartCoroutine(this.WaitThenReadyManaCrystal());
    }

    public void ReadyManaCrystals(int numCrystals)
    {
        for (int i = 0; i < numCrystals; i++)
        {
            this.ReadyManaCrystal();
        }
    }

    public void ReclaimCrystalsOwedForOverload(int numCrystals)
    {
        int num = 0;
        if (<>f__am$cache17 == null)
        {
            <>f__am$cache17 = crystal => crystal.IsOwedForOverload();
        }
        int num2 = this.m_permanentCrystals.FindLastIndex(<>f__am$cache17);
        while ((num < numCrystals) && (num2 >= 0))
        {
            this.m_permanentCrystals[num2].ReclaimOverload();
            num2--;
            num++;
        }
        this.m_additionalOverloadedCrystalsOwedNextTurn -= numCrystals - num;
        this.m_overloadLocksAreShowing = (num2 >= 0) || (this.m_additionalOverloadedCrystalsOwedNextTurn > 0);
    }

    public void SetFriendlyManaGemTexture(Texture texture)
    {
        if (this.m_friendlyManaGem == null)
        {
            this.friendlyManaGemTexture = texture;
        }
        else
        {
            this.m_friendlyManaGem.GetComponentInChildren<MeshRenderer>().material.mainTexture = texture;
        }
    }

    public void SetFriendlyManaGemTint(Color tint)
    {
        if (this.m_friendlyManaGem != null)
        {
            this.m_friendlyManaGem.GetComponentInChildren<MeshRenderer>().material.SetColor("_TintColor", tint);
        }
    }

    public bool ShouldShowOverloadTooltip()
    {
        return this.m_overloadLocksAreShowing;
    }

    public void ShowPhoneManaTray()
    {
        this.m_friendlyManaGem.GetComponent<Animation>()[this.GEM_FLIP_ANIM_NAME].speed = 1f;
        this.m_friendlyManaGem.GetComponent<Animation>().Play(this.GEM_FLIP_ANIM_NAME);
        object[] args = new object[] { "from", this.m_friendlyManaText.TextAlpha, "to", 0f, "time", 0.1f, "onupdate", newVal => this.m_friendlyManaText.TextAlpha = (float) newVal };
        iTween.ValueTo(base.gameObject, iTween.Hash(args));
        this.manaTrayPhone.ToggleTraySlider(true, null, true);
    }

    private void SpendManaCrystal()
    {
        base.StartCoroutine(this.WaitThenSpendManaCrystal());
    }

    public void SpendManaCrystals(int numCrystals)
    {
        SoundManager.Get().LoadAndPlay("mana_crystal_expend", base.gameObject);
        for (int i = 0; i < numCrystals; i++)
        {
            this.SpendManaCrystal();
        }
    }

    private void Start()
    {
        this.m_permanentCrystals = new List<ManaCrystal>();
        this.m_temporaryCrystals = new List<ManaCrystal>();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            this.m_friendlyManaText = this.friendlyManaCounter.GetComponent<UberText>();
            this.m_friendlyManaGem = this.friendlyManaCounter.GetComponent<ManaCounter>().GetPhoneGem();
            if (this.friendlyManaGemTexture != null)
            {
                this.SetFriendlyManaGemTexture(this.friendlyManaGemTexture);
                this.friendlyManaGemTexture = null;
            }
        }
    }

    public void UnlockCrystals(int numCrystals)
    {
        int num = 0;
        if (<>f__am$cache18 == null)
        {
            <>f__am$cache18 = crystal => crystal.IsOverloaded();
        }
        int num2 = this.m_permanentCrystals.FindLastIndex(<>f__am$cache18);
        while ((num < numCrystals) && (num2 >= 0))
        {
            this.m_permanentCrystals[num2].UnlockOverload();
            num2--;
            num++;
        }
        this.m_additionalOverloadedCrystalsOwedThisTurn -= numCrystals - num;
        this.m_overloadLocksAreShowing = (num2 >= 0) || (this.m_additionalOverloadedCrystalsOwedThisTurn > 0);
    }

    private void UpdateLayout()
    {
        if (this.m_permanentCrystals.Count > 0)
        {
            Vector3 position = base.transform.position;
            if (UniversalInputManager.UsePhoneUI != null)
            {
                position = this.manaGemBone.transform.position;
            }
            for (int i = this.m_permanentCrystals.Count - 1; i >= 0; i--)
            {
                this.m_permanentCrystals[i].transform.position = position;
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    position.z += this.m_manaCrystalWidth;
                }
                else
                {
                    position.x += this.m_manaCrystalWidth;
                }
            }
            for (int j = 0; j < this.m_temporaryCrystals.Count; j++)
            {
                this.m_temporaryCrystals[j].transform.position = position;
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    position.z += this.m_manaCrystalWidth;
                }
                else
                {
                    position.x += this.m_manaCrystalWidth;
                }
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator UpdatePermanentCrystalStates()
    {
        return new <UpdatePermanentCrystalStates>c__IteratorA7 { <>f__this = this };
    }

    public void UpdateSpentMana(int shownChangeAmount)
    {
        if (shownChangeAmount > 0)
        {
            this.SpendManaCrystals(shownChangeAmount);
        }
        else if (GameState.Get().IsTurnStartManagerActive())
        {
            TurnStartManager.Get().NotifyOfManaCrystalFilled(-shownChangeAmount);
        }
        else
        {
            this.ReadyManaCrystals(-shownChangeAmount);
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenAddManaCrystal(bool isTemp, bool isTurnStart)
    {
        return new <WaitThenAddManaCrystal>c__IteratorA8 { isTemp = isTemp, isTurnStart = isTurnStart, <$>isTemp = isTemp, <$>isTurnStart = isTurnStart, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenDestroyManaCrystals(bool isTemp, int numCrystals)
    {
        return new <WaitThenDestroyManaCrystals>c__IteratorA9 { numCrystals = numCrystals, isTemp = isTemp, <$>numCrystals = numCrystals, <$>isTemp = isTemp, <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenReadyManaCrystal()
    {
        return new <WaitThenReadyManaCrystal>c__IteratorAA { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitThenSpendManaCrystal()
    {
        return new <WaitThenSpendManaCrystal>c__IteratorAB { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <UpdatePermanentCrystalStates>c__IteratorA7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ManaCrystalMgr <>f__this;
        internal int <i>__1;
        internal int <j>__2;
        internal int <numUsedCrystals>__0;

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
                case 1:
                    if (((this.<>f__this.m_numQueuedToReady > 0) || (this.<>f__this.m_numCrystalsLoading > 0)) || (this.<>f__this.m_numQueuedToSpend > 0))
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<numUsedCrystals>__0 = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.RESOURCES_USED);
                    this.<i>__1 = 0;
                    while (this.<i>__1 < this.<numUsedCrystals>__0)
                    {
                        if (this.<i>__1 == this.<>f__this.m_permanentCrystals.Count)
                        {
                            break;
                        }
                        if (this.<>f__this.m_permanentCrystals[this.<i>__1].state != ManaCrystal.State.USED)
                        {
                            this.<>f__this.m_permanentCrystals[this.<i>__1].state = ManaCrystal.State.USED;
                        }
                        this.<i>__1++;
                    }
                    break;

                default:
                    goto Label_0192;
            }
            this.<j>__2 = this.<i>__1;
            while (this.<j>__2 < this.<>f__this.m_permanentCrystals.Count)
            {
                if (this.<>f__this.m_permanentCrystals[this.<j>__2].state != ManaCrystal.State.READY)
                {
                    this.<>f__this.m_permanentCrystals[this.<j>__2].state = ManaCrystal.State.READY;
                }
                this.<j>__2++;
            }
            this.$PC = -1;
        Label_0192:
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
    private sealed class <WaitThenAddManaCrystal>c__IteratorA8 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>isTemp;
        internal bool <$>isTurnStart;
        internal ManaCrystalMgr <>f__this;
        internal ManaCrystalMgr.LoadCrystalCallbackData <callbackData>__0;
        internal bool isTemp;
        internal bool isTurnStart;

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
                    this.<>f__this.m_numCrystalsLoading++;
                    this.<>f__this.m_numQueuedToSpawn++;
                    this.$current = new WaitForSeconds(this.<>f__this.m_numQueuedToSpawn * 0.2f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<callbackData>__0 = new ManaCrystalMgr.LoadCrystalCallbackData(this.isTemp, this.isTurnStart);
                    AssetLoader.Get().LoadActor("Resource", new AssetLoader.GameObjectCallback(this.<>f__this.LoadCrystalCallback), this.<callbackData>__0, false);
                    this.<>f__this.m_numQueuedToSpawn--;
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
    private sealed class <WaitThenDestroyManaCrystals>c__IteratorA9 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>isTemp;
        internal int <$>numCrystals;
        internal ManaCrystalMgr <>f__this;
        internal int <i>__0;
        internal bool isTemp;
        internal int numCrystals;

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
                case 1:
                    if (this.<>f__this.m_numCrystalsLoading > 0)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<i>__0 = 0;
                    while (this.<i>__0 < this.numCrystals)
                    {
                        if (this.isTemp)
                        {
                            this.<>f__this.DestroyTempManaCrystal();
                        }
                        else
                        {
                            this.<>f__this.DestroyManaCrystal();
                        }
                        this.<i>__0++;
                    }
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
    private sealed class <WaitThenReadyManaCrystal>c__IteratorAA : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ManaCrystalMgr <>f__this;
        internal int <i>__0;

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
                    this.<>f__this.m_numQueuedToReady++;
                    this.$current = new WaitForSeconds(this.<>f__this.m_numQueuedToReady * 0.2f);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (this.<>f__this.m_permanentCrystals.Count > 0)
                    {
                        this.<i>__0 = this.<>f__this.m_permanentCrystals.Count - 1;
                        while (this.<i>__0 >= 0)
                        {
                            if (this.<>f__this.m_permanentCrystals[this.<i>__0].state == ManaCrystal.State.USED)
                            {
                                SoundManager.Get().LoadAndPlay("mana_crystal_refresh", this.<>f__this.gameObject);
                                this.<>f__this.m_permanentCrystals[this.<i>__0].state = ManaCrystal.State.READY;
                                break;
                            }
                            this.<i>__0--;
                        }
                    }
                    break;

                default:
                    goto Label_0125;
            }
            this.<>f__this.m_numQueuedToReady--;
            this.$PC = -1;
        Label_0125:
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
    private sealed class <WaitThenSpendManaCrystal>c__IteratorAB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal ManaCrystalMgr <>f__this;
        internal int <i>__0;

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
                    this.<>f__this.m_numQueuedToSpend++;
                    this.$current = new WaitForSeconds((this.<>f__this.m_numQueuedToSpend - 1) * 0.2f);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<i>__0 = 0;
                    while (this.<i>__0 < this.<>f__this.m_permanentCrystals.Count)
                    {
                        if ((this.<>f__this.m_permanentCrystals[this.<i>__0].state != ManaCrystal.State.USED) && !this.<>f__this.m_permanentCrystals[this.<i>__0].IsOverloaded())
                        {
                            this.<>f__this.m_permanentCrystals[this.<i>__0].state = ManaCrystal.State.USED;
                            break;
                        }
                        this.<i>__0++;
                    }
                    break;

                default:
                    goto Label_013A;
            }
            this.<>f__this.m_numQueuedToSpend--;
            if (this.<>f__this.m_numQueuedToSpend <= 0)
            {
                InputManager.Get().OnManaCrystalMgrManaSpent();
                this.$PC = -1;
            }
        Label_013A:
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

    private class LoadCrystalCallbackData
    {
        private bool m_isTempCrystal;
        private bool m_isTurnStart;

        public LoadCrystalCallbackData(bool isTempCrystal, bool isTurnStart)
        {
            this.m_isTempCrystal = isTempCrystal;
            this.m_isTurnStart = isTurnStart;
        }

        public bool IsTempCrystal
        {
            get
            {
                return this.m_isTempCrystal;
            }
        }

        public bool IsTurnStart
        {
            get
            {
                return this.m_isTurnStart;
            }
        }
    }
}

