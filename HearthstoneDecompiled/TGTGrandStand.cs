using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class TGTGrandStand : MonoBehaviour
{
    private readonly string[] ANIMATION_CHEER = new string[] { "Cheer01", "Cheer02", "Cheer03" };
    private const string ANIMATION_IDLE = "Idle";
    private readonly string[] ANIMATION_OHNO = new string[] { "OhNo01", "OhNo02" };
    private const string ANIMATION_SCORE_CARD = "ScoreCard";
    private const float CHEER_ANIMATION_PLAY_TIME = 4f;
    private const float FRIENDLY_HERO_DAMAGE_WEIGHT_TRGGER = 7f;
    private const float FRIENDLY_LEGENDARY_DEATH_MIN_COST_TRGGER = 6f;
    private const float FRIENDLY_LEGENDARY_SPAWN_MIN_COST_TRGGER = 6f;
    private const float FRIENDLY_MINION_DAMAGE_WEIGHT = 15f;
    private const float FRIENDLY_MINION_DEATH_WEIGHT = 15f;
    private const float FRIENDLY_MINION_SPAWN_WEIGHT = 10f;
    private BoardEvents m_boardEvents;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Human Sounds")]
    public List<string> m_CheerHumanSounds;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Knight Sounds")]
    public List<string> m_CheerKnightSounds;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Orc Sounds")]
    public List<string> m_CheerOrcSounds;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Human Sounds")]
    public string m_ClickHumanSound;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Knight Sounds")]
    public string m_ClickKnightSound;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Orc Sounds")]
    public string m_ClickOrcSound;
    public Animator m_HumanAnimator;
    public GameObject m_HumanRoot;
    public GameObject m_HumanScoreCard;
    public UberText m_HumanScoreUberText;
    private bool m_isAnimating;
    public Animator m_KnightAnimator;
    public GameObject m_KnightRoot;
    public GameObject m_KnightScoreCard;
    public UberText m_KnightScoreUberText;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Human Sounds")]
    public List<string> m_OhNoHumanSounds;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Knight Sounds")]
    public List<string> m_OhNoKnightSounds;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Orc Sounds")]
    public List<string> m_OhNoOrcSounds;
    public Animator m_OrcAnimator;
    public GameObject m_OrcRoot;
    public GameObject m_OrcScoreCard;
    public UberText m_OrcScoreUberText;
    [CustomEditField(T=EditType.SOUND_PREFAB, Sections="Sounds")]
    public string m_ScoreCardSound;
    private const float MAX_RANDOM_TIME_FACTOR = 0.2f;
    private const float MIN_RANDOM_TIME_FACTOR = 0.05f;
    private const float OHNO_ANIMATION_PLAY_TIME = 3.5f;
    private const float OPPONENT_HERO_DAMAGE_SCORE_CARD_10S_TRIGGER = 20f;
    private const float OPPONENT_HERO_DAMAGE_SCORE_CARD_TRIGGER = 15f;
    private const float OPPONENT_HERO_DAMAGE_WEIGHT_TRGGER = 10f;
    private const float OPPONENT_LEGENDARY_DEATH_MIN_COST_TRGGER = 9f;
    private const float OPPONENT_LEGENDARY_SPAWN_MIN_COST_TRGGER = 9f;
    private const float OPPONENT_MINION_DAMAGE_WEIGHT = 15f;
    private const float OPPONENT_MINION_DEATH_WEIGHT = 15f;
    private const float OPPONENT_MINION_SPAWN_WEIGHT = 10f;
    private static TGTGrandStand s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    private void FriendlyHeroDamage(float weight)
    {
        this.PlayOhNoAnimation();
    }

    private void FriendlyLegendaryDeath(float weight)
    {
        this.PlayOhNoAnimation();
    }

    private void FriendlyLegendarySpawn(float weight)
    {
        this.PlayCheerAnimation();
    }

    private void FriendlyMinionDamage(float weight)
    {
        this.PlayOhNoAnimation();
    }

    private void FriendlyMinionDeath(float weight)
    {
        this.PlayOhNoAnimation();
    }

    private void FriendlyMinionSpawn(float weight)
    {
        this.PlayCheerAnimation();
    }

    public static TGTGrandStand Get()
    {
        return s_instance;
    }

    private void HandleClicks()
    {
        if (UniversalInputManager.Get().GetMouseButtonDown(0) && this.IsOver(this.m_HumanRoot))
        {
            this.HumanClick();
        }
        if (UniversalInputManager.Get().GetMouseButtonDown(0) && this.IsOver(this.m_OrcRoot))
        {
            this.OrcClick();
        }
        if (UniversalInputManager.Get().GetMouseButtonDown(0) && this.IsOver(this.m_KnightRoot))
        {
            this.KnightClick();
        }
    }

    private void HumanClick()
    {
        this.m_HumanRoot.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, -30f);
        if (!string.IsNullOrEmpty(this.m_ClickHumanSound))
        {
            string str = FileUtils.GameAssetPathToName(this.m_ClickHumanSound);
            if (!string.IsNullOrEmpty(str))
            {
                SoundManager.Get().LoadAndPlay(str, this.m_HumanRoot);
            }
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

    private void KnightClick()
    {
        this.m_KnightRoot.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, -30f);
        if (!string.IsNullOrEmpty(this.m_ClickKnightSound))
        {
            string str = FileUtils.GameAssetPathToName(this.m_ClickKnightSound);
            if (!string.IsNullOrEmpty(str))
            {
                SoundManager.Get().LoadAndPlay(str, this.m_KnightRoot);
            }
        }
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OpponentHeroDamage(float weight)
    {
        if (weight > 15f)
        {
            if (weight > 20f)
            {
                this.PlayScoreCard("10", "10", "10");
            }
            else
            {
                this.PlayScoreCard("10", UnityEngine.Random.Range(7, 9).ToString(), UnityEngine.Random.Range(8, 10).ToString());
            }
        }
        else
        {
            this.PlayCheerAnimation();
        }
    }

    private void OpponentLegendaryDeath(float weight)
    {
        this.PlayCheerAnimation();
    }

    private void OpponentLegendarySpawn(float weight)
    {
        this.PlayOhNoAnimation();
    }

    private void OpponentMinionDamage(float weight)
    {
        this.PlayCheerAnimation();
    }

    private void OpponentMinionDeath(float weight)
    {
        this.PlayCheerAnimation();
    }

    private void OpponentMinionSpawn(float weight)
    {
        this.PlayOhNoAnimation();
    }

    private void OrcClick()
    {
        this.m_OrcRoot.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, -30f);
        if (!string.IsNullOrEmpty(this.m_ClickOrcSound))
        {
            string str = FileUtils.GameAssetPathToName(this.m_ClickOrcSound);
            if (!string.IsNullOrEmpty(str))
            {
                SoundManager.Get().LoadAndPlay(str, this.m_OrcRoot);
            }
        }
    }

    private void PlayAnimation(Animator animator, string animName, float time)
    {
        this.m_isAnimating = true;
        this.m_HumanScoreCard.SetActive(false);
        this.m_OrcScoreCard.SetActive(false);
        this.m_KnightScoreCard.SetActive(false);
        base.StartCoroutine(this.PlayAnimationRandomStart(animator, animName, time));
    }

    [DebuggerHidden]
    private IEnumerator PlayAnimationRandomStart(Animator animator, string animName, float time)
    {
        return new <PlayAnimationRandomStart>c__Iterator14 { animator = animator, animName = animName, time = time, <$>animator = animator, <$>animName = animName, <$>time = time, <>f__this = this };
    }

    public void PlayCheerAnimation()
    {
        int index = UnityEngine.Random.Range(0, this.ANIMATION_CHEER.Length);
        string animName = this.ANIMATION_CHEER[index];
        this.PlayAnimation(this.m_HumanAnimator, animName, 4f);
        this.PlaySoundFromList(this.m_CheerHumanSounds, index);
        index = UnityEngine.Random.Range(0, this.ANIMATION_CHEER.Length);
        animName = this.ANIMATION_CHEER[index];
        this.PlayAnimation(this.m_OrcAnimator, animName, 4f);
        this.PlaySoundFromList(this.m_CheerOrcSounds, index);
        index = UnityEngine.Random.Range(0, this.ANIMATION_CHEER.Length);
        animName = this.ANIMATION_CHEER[index];
        this.PlayAnimation(this.m_KnightAnimator, animName, 4f);
        this.PlaySoundFromList(this.m_CheerKnightSounds, index);
    }

    public void PlayOhNoAnimation()
    {
        int index = UnityEngine.Random.Range(0, this.ANIMATION_OHNO.Length);
        string animName = this.ANIMATION_OHNO[index];
        this.PlayAnimation(this.m_HumanAnimator, animName, 3.5f);
        this.PlaySoundFromList(this.m_OhNoHumanSounds, index);
        index = UnityEngine.Random.Range(0, this.ANIMATION_OHNO.Length);
        animName = this.ANIMATION_OHNO[index];
        this.PlayAnimation(this.m_OrcAnimator, animName, 3.5f);
        this.PlaySoundFromList(this.m_OhNoOrcSounds, index);
        index = UnityEngine.Random.Range(0, this.ANIMATION_OHNO.Length);
        animName = this.ANIMATION_OHNO[index];
        this.PlayAnimation(this.m_KnightAnimator, animName, 3.5f);
        this.PlaySoundFromList(this.m_OhNoKnightSounds, index);
    }

    public void PlayScoreCard(string humanScore, string orcScore, string knightScore)
    {
        this.m_HumanScoreUberText.Text = humanScore;
        this.m_OrcScoreUberText.Text = orcScore;
        this.m_KnightScoreUberText.Text = knightScore;
        this.m_HumanAnimator.SetTrigger("ScoreCard");
        this.m_OrcAnimator.SetTrigger("ScoreCard");
        this.m_KnightAnimator.SetTrigger("ScoreCard");
        this.PlaySound(this.m_ScoreCardSound);
    }

    private void PlaySound(string soundString)
    {
        if (!string.IsNullOrEmpty(soundString))
        {
            string str = FileUtils.GameAssetPathToName(soundString);
            if (!string.IsNullOrEmpty(str))
            {
                SoundManager.Get().LoadAndPlay(str, this.m_OrcRoot);
            }
        }
    }

    private void PlaySoundFromList(List<string> soundList, int index)
    {
        if ((soundList != null) && (soundList.Count != 0))
        {
            if (index > soundList.Count)
            {
                index = 0;
            }
            this.PlaySound(soundList[index]);
        }
    }

    [DebuggerHidden]
    private IEnumerator RegisterBoardEvents()
    {
        return new <RegisterBoardEvents>c__Iterator19 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ReturnToIdleAnimation(Animator animator, float time)
    {
        return new <ReturnToIdleAnimation>c__Iterator15 { time = time, animator = animator, <$>time = time, <$>animator = animator, <>f__this = this };
    }

    private void Shake()
    {
        if (!this.m_isAnimating)
        {
            base.StartCoroutine(this.ShakeHuman());
            base.StartCoroutine(this.ShakeOrc());
            base.StartCoroutine(this.ShakeKnight());
        }
    }

    [DebuggerHidden]
    private IEnumerator ShakeHuman()
    {
        return new <ShakeHuman>c__Iterator16 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ShakeKnight()
    {
        return new <ShakeKnight>c__Iterator18 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator ShakeOrc()
    {
        return new <ShakeOrc>c__Iterator17 { <>f__this = this };
    }

    private void Start()
    {
        base.StartCoroutine(this.RegisterBoardEvents());
    }

    [DebuggerHidden]
    private IEnumerator TestAnimations()
    {
        return new <TestAnimations>c__Iterator13 { <>f__this = this };
    }

    private void Update()
    {
        this.HandleClicks();
    }

    [CompilerGenerated]
    private sealed class <PlayAnimationRandomStart>c__Iterator14 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Animator <$>animator;
        internal string <$>animName;
        internal float <$>time;
        internal TGTGrandStand <>f__this;
        internal Animator animator;
        internal string animName;
        internal float time;

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
                    this.$current = new WaitForSeconds(UnityEngine.Random.Range((float) 0.05f, (float) 0.2f));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.animator.SetTrigger(this.animName);
                    this.<>f__this.StartCoroutine(this.<>f__this.ReturnToIdleAnimation(this.animator, this.time));
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
    private sealed class <RegisterBoardEvents>c__Iterator19 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TGTGrandStand <>f__this;

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
                    if (BoardEvents.Get() == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<>f__this.m_boardEvents = BoardEvents.Get();
                    this.<>f__this.m_boardEvents.RegisterFriendlyHeroDamageEvent(new BoardEvents.EventDelegate(this.<>f__this.FriendlyHeroDamage), 7f);
                    this.<>f__this.m_boardEvents.RegisterOpponentHeroDamageEvent(new BoardEvents.EventDelegate(this.<>f__this.OpponentHeroDamage), 10f);
                    this.<>f__this.m_boardEvents.RegisterFriendlyLegendaryMinionSpawnEvent(new BoardEvents.EventDelegate(this.<>f__this.FriendlyLegendarySpawn), 6f);
                    this.<>f__this.m_boardEvents.RegisterOppenentLegendaryMinionSpawnEvent(new BoardEvents.EventDelegate(this.<>f__this.OpponentLegendarySpawn), 9f);
                    this.<>f__this.m_boardEvents.RegisterFriendlyLegendaryMinionDeathEvent(new BoardEvents.EventDelegate(this.<>f__this.FriendlyLegendaryDeath), 6f);
                    this.<>f__this.m_boardEvents.RegisterOppenentLegendaryMinionDeathEvent(new BoardEvents.EventDelegate(this.<>f__this.OpponentLegendaryDeath), 9f);
                    this.<>f__this.m_boardEvents.RegisterFriendlyMinionDamageEvent(new BoardEvents.EventDelegate(this.<>f__this.FriendlyMinionDamage), 15f);
                    this.<>f__this.m_boardEvents.RegisterOpponentMinionDamageEvent(new BoardEvents.EventDelegate(this.<>f__this.OpponentMinionDamage), 15f);
                    this.<>f__this.m_boardEvents.RegisterFriendlyMinionDeathEvent(new BoardEvents.EventDelegate(this.<>f__this.FriendlyMinionDeath), 15f);
                    this.<>f__this.m_boardEvents.RegisterOppenentMinionDeathEvent(new BoardEvents.EventDelegate(this.<>f__this.OpponentMinionDeath), 15f);
                    this.<>f__this.m_boardEvents.RegisterFriendlyMinionSpawnEvent(new BoardEvents.EventDelegate(this.<>f__this.FriendlyMinionSpawn), 10f);
                    this.<>f__this.m_boardEvents.RegisterOppenentMinionSpawnEvent(new BoardEvents.EventDelegate(this.<>f__this.OpponentMinionSpawn), 10f);
                    this.<>f__this.m_boardEvents.RegisterLargeShakeEvent(new BoardEvents.LargeShakeEventDelegate(this.<>f__this.Shake));
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
    private sealed class <ReturnToIdleAnimation>c__Iterator15 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal Animator <$>animator;
        internal float <$>time;
        internal TGTGrandStand <>f__this;
        internal Animator animator;
        internal float time;

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
                    this.$current = new WaitForSeconds(this.time);
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_isAnimating = false;
                    this.animator.SetTrigger("Idle");
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
    private sealed class <ShakeHuman>c__Iterator16 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TGTGrandStand <>f__this;

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
                    this.$current = new WaitForSeconds(UnityEngine.Random.Range((float) 0.05f, (float) 0.2f));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_HumanRoot.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, -25f);
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
    private sealed class <ShakeKnight>c__Iterator18 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TGTGrandStand <>f__this;

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
                    this.$current = new WaitForSeconds(UnityEngine.Random.Range((float) 0.05f, (float) 0.2f));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_KnightRoot.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, -25f);
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
    private sealed class <ShakeOrc>c__Iterator17 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TGTGrandStand <>f__this;

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
                    this.$current = new WaitForSeconds(UnityEngine.Random.Range((float) 0.05f, (float) 0.2f));
                    this.$PC = 1;
                    return true;

                case 1:
                    this.<>f__this.m_OrcRoot.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, -25f);
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
    private sealed class <TestAnimations>c__Iterator13 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TGTGrandStand <>f__this;

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
                    this.$current = new WaitForSeconds(4f);
                    this.$PC = 1;
                    goto Label_0102;

                case 1:
                    this.<>f__this.PlayCheerAnimation();
                    this.$current = new WaitForSeconds(8f);
                    this.$PC = 2;
                    goto Label_0102;

                case 2:
                    this.<>f__this.PlayCheerAnimation();
                    this.$current = new WaitForSeconds(9f);
                    this.$PC = 3;
                    goto Label_0102;

                case 3:
                    this.<>f__this.PlayCheerAnimation();
                    this.$current = new WaitForSeconds(8f);
                    this.$PC = 4;
                    goto Label_0102;

                case 4:
                    this.<>f__this.PlayOhNoAnimation();
                    this.$current = new WaitForSeconds(8f);
                    this.$PC = 5;
                    goto Label_0102;

                case 5:
                    this.<>f__this.PlayOhNoAnimation();
                    break;

                default:
                    break;
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_0102:
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

