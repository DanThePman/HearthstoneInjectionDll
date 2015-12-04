using PegasusGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class JoustSpellController : SpellController
{
    [CompilerGenerated]
    private static Spell.StateFinishedCallback <>f__am$cache16;
    [CompilerGenerated]
    private static Spell.StateFinishedCallback <>f__am$cache17;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_DrawStingerPrefab;
    public float m_DriftCycleTime = 10f;
    public string m_FriendlyBoneName = "FriendlyJoust";
    private Jouster m_friendlyJouster;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HideSoundPrefab;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HideStingerPrefab;
    public float m_HideTime = 0.8f;
    public float m_HoldTime = 1.2f;
    private int m_joustTaskIndex;
    public Spell m_LoserSpellPrefab;
    public Spell m_NoJousterSpellPrefab;
    public string m_OpponentBoneName = "OpponentJoust";
    private Jouster m_opponentJouster;
    public float m_RandomSecMax = 0.25f;
    public float m_RandomSecMin = 0.1f;
    public iTween.EaseType m_RevealEaseType = iTween.EaseType.easeOutBack;
    public float m_RevealTime = 0.5f;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_ShowSoundPrefab;
    public float m_ShowTime = 1.2f;
    private Jouster m_sourceJouster;
    public Spell m_WinnerSpellPrefab;
    private Jouster m_winningJouster;

    protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
    {
        if (!this.HasSourceCard(taskList))
        {
            return false;
        }
        this.m_joustTaskIndex = -1;
        List<PowerTask> list = taskList.GetTaskList();
        for (int i = 0; i < list.Count; i++)
        {
            PowerTask task = list[i];
            Network.HistMetaData power = task.GetPower() as Network.HistMetaData;
            if ((power != null) && (power.MetaType == HistoryMeta.Type.JOUST))
            {
                this.m_joustTaskIndex = i;
            }
        }
        if (this.m_joustTaskIndex < 0)
        {
            return false;
        }
        Card card = taskList.GetSourceEntity().GetCard();
        base.SetSource(card);
        return true;
    }

    private Jouster CreateJouster(Player player, Network.HistMetaData metaData)
    {
        <CreateJouster>c__AnonStorey324 storey = new <CreateJouster>c__AnonStorey324 {
            entity = null
        };
        foreach (int num in metaData.Info)
        {
            Entity entity = GameState.Get().GetEntity(num);
            if ((entity != null) && (entity.GetController() == player))
            {
                storey.entity = entity;
                break;
            }
        }
        if (storey.entity == null)
        {
            return null;
        }
        storey.card = storey.entity.GetCard();
        storey.cardDef = storey.card.GetCardDef();
        storey.card.SetInputEnabled(false);
        GameObject obj2 = AssetLoader.Get().LoadActor("Card_Hidden", false, false);
        string handActor = ActorNames.GetHandActor(storey.entity);
        GameObject obj3 = AssetLoader.Get().LoadActor(handActor, false, false);
        Jouster jouster = new Jouster {
            m_player = player,
            m_card = storey.card,
            m_initialActor = obj2.GetComponent<Actor>(),
            m_revealedActor = obj3.GetComponent<Actor>()
        };
        Action<Actor> action = new Action<Actor>(storey.<>m__154);
        action(jouster.m_initialActor);
        action(jouster.m_revealedActor);
        return jouster;
    }

    private void CreateJousters()
    {
        PowerTask task = base.m_taskList.GetTaskList()[this.m_joustTaskIndex];
        Network.HistMetaData power = (Network.HistMetaData) task.GetPower();
        Player friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
        Player opposingSidePlayer = GameState.Get().GetOpposingSidePlayer();
        this.m_friendlyJouster = this.CreateJouster(friendlySidePlayer, power);
        this.m_opponentJouster = this.CreateJouster(opposingSidePlayer, power);
        this.DetermineWinner(power);
        this.DetermineSourceJouster();
    }

    private void DestroyJouster(Jouster jouster)
    {
        if (jouster != null)
        {
            jouster.m_card.SetInputEnabled(true);
            jouster.m_initialActor.Destroy();
            jouster.m_revealedActor.Destroy();
        }
    }

    private void DestroyJousters()
    {
        if (this.m_friendlyJouster != null)
        {
            this.DestroyJouster(this.m_friendlyJouster);
            this.m_friendlyJouster = null;
        }
        if (this.m_opponentJouster != null)
        {
            this.DestroyJouster(this.m_opponentJouster);
            this.m_opponentJouster = null;
        }
    }

    private void DetermineSourceJouster()
    {
        Player controller = base.GetSource().GetController();
        if ((this.m_friendlyJouster != null) && (this.m_friendlyJouster.m_card.GetController() == controller))
        {
            this.m_sourceJouster = this.m_friendlyJouster;
        }
        else if ((this.m_opponentJouster != null) && (this.m_opponentJouster.m_card.GetController() == controller))
        {
            this.m_sourceJouster = this.m_opponentJouster;
        }
    }

    private void DetermineWinner(Network.HistMetaData metaData)
    {
        Card joustWinner = GameUtils.GetJoustWinner(metaData);
        if (joustWinner != null)
        {
            if (joustWinner.GetController().IsFriendlySide())
            {
                this.m_winningJouster = this.m_friendlyJouster;
            }
            else
            {
                this.m_winningJouster = this.m_opponentJouster;
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator DoEffectWithTiming()
    {
        return new <DoEffectWithTiming>c__Iterator1C0 { <>f__this = this };
    }

    private void DriftJouster(Jouster jouster)
    {
        Card card = jouster.m_card;
        Vector3 position = card.transform.position;
        float z = jouster.m_initialActor.GetMeshRenderer().bounds.size.z;
        float num2 = 0.02f * z;
        Vector3 vector2 = (Vector3) ((GeneralUtils.RandomSign() * num2) * card.transform.up);
        Vector3 vector3 = -vector2;
        Vector3 vector4 = (Vector3) ((GeneralUtils.RandomSign() * num2) * card.transform.right);
        Vector3 vector5 = -vector4;
        List<Vector3> list = new List<Vector3> {
            (position + vector2) + vector4,
            (position + vector3) + vector4,
            position,
            (position + vector2) + vector5,
            (position + vector3) + vector5,
            position
        };
        float num3 = this.m_DriftCycleTime + this.GetRandomSec();
        object[] args = new object[] { "path", list.ToArray(), "time", num3, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.loop };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(card.gameObject, hashtable);
    }

    private float GetRandomSec()
    {
        return UnityEngine.Random.Range(this.m_RandomSecMin, this.m_RandomSecMax);
    }

    private void HideJouster(Jouster jouster, float delaySec, float hideSec)
    {
        <HideJouster>c__AnonStorey327 storey = new <HideJouster>c__AnonStorey327 {
            jouster = jouster
        };
        storey.jouster.m_effectsPendingFinish++;
        storey.card = storey.jouster.m_card;
        storey.deck = storey.jouster.m_player.GetDeckZone();
        Vector3 center = storey.deck.GetThicknessForLayout().GetComponent<Renderer>().bounds.center;
        float num = 0.5f * hideSec;
        Vector3 position = storey.card.transform.position;
        Vector3 vector3 = center + Card.ABOVE_DECK_OFFSET;
        Vector3 vector4 = center + Card.IN_DECK_OFFSET;
        Vector3 vector5 = Card.IN_DECK_ANGLES;
        Vector3 vector6 = Card.IN_DECK_SCALE;
        Vector3[] vectorArray = new Vector3[] { position, vector3, vector4 };
        object[] args = new object[] { "path", vectorArray, "delay", delaySec, "time", hideSec, "easetype", iTween.EaseType.easeInOutQuad };
        iTween.MoveTo(storey.card.gameObject, iTween.Hash(args));
        object[] objArray2 = new object[] { "rotation", vector5, "delay", delaySec, "time", num, "easetype", iTween.EaseType.easeInOutCubic };
        iTween.RotateTo(storey.card.gameObject, iTween.Hash(objArray2));
        object[] objArray3 = new object[] { "scale", vector6, "delay", delaySec + num, "time", num, "easetype", iTween.EaseType.easeInOutQuint };
        iTween.ScaleTo(storey.card.gameObject, iTween.Hash(objArray3));
        if (!string.IsNullOrEmpty(this.m_HideSoundPrefab))
        {
            string soundName = FileUtils.GameAssetPathToName(this.m_HideSoundPrefab);
            SoundManager.Get().LoadAndPlay(soundName);
        }
        Action<object> action = new Action<object>(storey.<>m__159);
        object[] objArray4 = new object[] { "delay", delaySec, "time", hideSec, "oncomplete", action };
        iTween.Timer(storey.card.gameObject, iTween.Hash(objArray4));
    }

    [DebuggerHidden]
    private IEnumerator HideJousters()
    {
        return new <HideJousters>c__Iterator1C4 { <>f__this = this };
    }

    private bool IsJousterBusy(Jouster jouster)
    {
        if (jouster == null)
        {
            return false;
        }
        return (jouster.m_effectsPendingFinish > 0);
    }

    [DebuggerHidden]
    private IEnumerator Joust()
    {
        return new <Joust>c__Iterator1C3 { <>f__this = this };
    }

    protected override void OnProcessTaskList()
    {
        base.StartCoroutine(this.DoEffectWithTiming());
    }

    private void PlayNoJousterSpell(Player player)
    {
        ZoneDeck deckZone = player.GetDeckZone();
        Spell spell = UnityEngine.Object.Instantiate<Spell>(this.m_NoJousterSpellPrefab);
        spell.SetPosition(deckZone.transform.position);
        if (<>f__am$cache16 == null)
        {
            <>f__am$cache16 = delegate (Spell spell, SpellStateType prevStateType, object userData) {
                if (spell.GetActiveState() == SpellStateType.NONE)
                {
                    UnityEngine.Object.Destroy(spell.gameObject);
                }
            };
        }
        spell.AddStateFinishedCallback(<>f__am$cache16);
        spell.Activate();
    }

    private bool PlaySpellOnActor(Jouster jouster, Actor actor, Spell spellPrefab)
    {
        <PlaySpellOnActor>c__AnonStorey328 storey = new <PlaySpellOnActor>c__AnonStorey328 {
            jouster = jouster
        };
        if (spellPrefab == null)
        {
            return false;
        }
        storey.jouster.m_effectsPendingFinish++;
        Card card = actor.GetCard();
        Spell spell = UnityEngine.Object.Instantiate<Spell>(spellPrefab);
        spell.transform.parent = actor.transform;
        spell.AddFinishedCallback(new Spell.FinishedCallback(storey.<>m__15A));
        if (<>f__am$cache17 == null)
        {
            <>f__am$cache17 = delegate (Spell spell, SpellStateType prevStateType, object userData) {
                if (spell.GetActiveState() == SpellStateType.NONE)
                {
                    UnityEngine.Object.Destroy(spell.gameObject);
                }
            };
        }
        spell.AddStateFinishedCallback(<>f__am$cache17);
        spell.SetSource(card.gameObject);
        spell.Activate();
        return true;
    }

    private void RevealJouster(Jouster jouster, float revealSec)
    {
        <RevealJouster>c__AnonStorey326 storey = new <RevealJouster>c__AnonStorey326 {
            jouster = jouster
        };
        storey.jouster.m_effectsPendingFinish++;
        Card card = storey.jouster.m_card;
        storey.hiddenActor = storey.jouster.m_initialActor;
        storey.revealedActor = storey.jouster.m_revealedActor;
        TransformUtil.SetEulerAngleZ(storey.revealedActor.gameObject, -180f);
        object[] args = new object[] { "z", 180f, "time", revealSec, "easetype", this.m_RevealEaseType };
        iTween.RotateAdd(storey.hiddenActor.gameObject, iTween.Hash(args));
        object[] objArray2 = new object[] { "z", 180f, "time", revealSec, "easetype", this.m_RevealEaseType };
        iTween.RotateAdd(storey.revealedActor.gameObject, iTween.Hash(objArray2));
        storey.startAngleZ = storey.revealedActor.transform.rotation.eulerAngles.z;
        Action<object> action = new Action<object>(storey.<>m__157);
        Action<object> action2 = new Action<object>(storey.<>m__158);
        object[] objArray3 = new object[] { "time", revealSec, "onupdate", action, "oncomplete", action2 };
        iTween.Timer(card.gameObject, iTween.Hash(objArray3));
    }

    private void ShowJouster(Jouster jouster, Vector3 localScale, Quaternion rotation, Vector3 position, float delaySec, float showSec)
    {
        <ShowJouster>c__AnonStorey325 storey = new <ShowJouster>c__AnonStorey325 {
            jouster = jouster,
            <>f__this = this
        };
        storey.jouster.m_effectsPendingFinish++;
        Card card = storey.jouster.m_card;
        ZoneDeck deckZone = storey.jouster.m_player.GetDeckZone();
        GameObject thicknessForLayout = deckZone.GetThicknessForLayout();
        storey.jouster.m_deckIndex = deckZone.RemoveCard(card);
        deckZone.SetSuppressEmotes(true);
        deckZone.UpdateLayout();
        float num = 0.5f * showSec;
        Vector3 vector = thicknessForLayout.GetComponent<Renderer>().bounds.center + Card.IN_DECK_OFFSET;
        Vector3 vector2 = vector + Card.ABOVE_DECK_OFFSET;
        Vector3 vector3 = position;
        Vector3 eulerAngles = rotation.eulerAngles;
        Vector3 vector5 = localScale;
        Vector3[] vectorArray = new Vector3[] { vector, vector2, vector3 };
        card.ShowCard();
        storey.jouster.m_initialActor.Show();
        card.transform.position = vector;
        card.transform.rotation = Card.IN_DECK_HIDDEN_ROTATION;
        card.transform.localScale = Card.IN_DECK_SCALE;
        object[] args = new object[] { "path", vectorArray, "delay", delaySec, "time", showSec, "easetype", iTween.EaseType.easeInOutQuart };
        iTween.MoveTo(card.gameObject, iTween.Hash(args));
        object[] objArray2 = new object[] { "rotation", eulerAngles, "delay", delaySec + num, "time", num, "easetype", iTween.EaseType.easeInOutCubic };
        iTween.RotateTo(card.gameObject, iTween.Hash(objArray2));
        object[] objArray3 = new object[] { "scale", vector5, "delay", delaySec + num, "time", num, "easetype", iTween.EaseType.easeInOutQuint };
        iTween.ScaleTo(card.gameObject, iTween.Hash(objArray3));
        if (!string.IsNullOrEmpty(this.m_ShowSoundPrefab))
        {
            string soundName = FileUtils.GameAssetPathToName(this.m_ShowSoundPrefab);
            SoundManager.Get().LoadAndPlay(soundName);
        }
        Action<object> action = new Action<object>(storey.<>m__155);
        object[] objArray4 = new object[] { "delay", delaySec, "time", showSec, "oncomplete", action };
        iTween.Timer(card.gameObject, iTween.Hash(objArray4));
    }

    [DebuggerHidden]
    private IEnumerator ShowJousters()
    {
        return new <ShowJousters>c__Iterator1C2 { <>f__this = this };
    }

    [DebuggerHidden]
    private IEnumerator WaitForShowEntities()
    {
        return new <WaitForShowEntities>c__Iterator1C1 { <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <CreateJouster>c__AnonStorey324
    {
        internal Card card;
        internal CardDef cardDef;
        internal Entity entity;

        internal void <>m__154(Actor actor)
        {
            actor.SetEntity(this.entity);
            actor.SetCard(this.card);
            actor.SetCardDef(this.cardDef);
            actor.UpdateAllComponents();
            actor.Hide();
        }
    }

    [CompilerGenerated]
    private sealed class <DoEffectWithTiming>c__Iterator1C0 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal JoustSpellController <>f__this;

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
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.WaitForShowEntities());
                    this.$PC = 1;
                    goto Label_00F7;

                case 1:
                    this.<>f__this.CreateJousters();
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.ShowJousters());
                    this.$PC = 2;
                    goto Label_00F7;

                case 2:
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.Joust());
                    this.$PC = 3;
                    goto Label_00F7;

                case 3:
                    this.$current = this.<>f__this.StartCoroutine(this.<>f__this.HideJousters());
                    this.$PC = 4;
                    goto Label_00F7;

                case 4:
                    this.<>f__this.DestroyJousters();
                    this.<>f__this.OnProcessTaskList();
                    this.$PC = -1;
                    break;
            }
            return false;
        Label_00F7:
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
    private sealed class <HideJouster>c__AnonStorey327
    {
        internal Card card;
        internal ZoneDeck deck;
        internal JoustSpellController.Jouster jouster;

        internal void <>m__159(object userData)
        {
            this.jouster.m_effectsPendingFinish--;
            this.jouster.m_initialActor.GetCard().HideCard();
            this.deck.InsertCard(this.jouster.m_deckIndex, this.card);
            this.deck.UpdateLayout();
            this.deck.SetSuppressEmotes(false);
        }
    }

    [CompilerGenerated]
    private sealed class <HideJousters>c__Iterator1C4 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal JoustSpellController <>f__this;
        internal float <delaySec>__1;
        internal float <delaySec>__3;
        internal float <hideSec>__2;
        internal float <hideSec>__4;
        internal string <hideSoundName>__0;

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
                    if (!string.IsNullOrEmpty(this.<>f__this.m_HideStingerPrefab))
                    {
                        this.<hideSoundName>__0 = FileUtils.GameAssetPathToName(this.<>f__this.m_HideStingerPrefab);
                        SoundManager.Get().LoadAndPlay(this.<hideSoundName>__0);
                    }
                    if (this.<>f__this.m_friendlyJouster != null)
                    {
                        this.<delaySec>__1 = this.<>f__this.GetRandomSec();
                        this.<hideSec>__2 = this.<>f__this.m_HideTime + this.<>f__this.GetRandomSec();
                        this.<>f__this.HideJouster(this.<>f__this.m_friendlyJouster, this.<delaySec>__1, this.<hideSec>__2);
                    }
                    if (this.<>f__this.m_opponentJouster != null)
                    {
                        this.<delaySec>__3 = this.<>f__this.GetRandomSec();
                        this.<hideSec>__4 = this.<>f__this.m_HideTime + this.<>f__this.GetRandomSec();
                        this.<>f__this.HideJouster(this.<>f__this.m_opponentJouster, this.<delaySec>__3, this.<hideSec>__4);
                    }
                    break;

                case 1:
                    break;

                default:
                    goto Label_0171;
            }
            while (this.<>f__this.IsJousterBusy(this.<>f__this.m_friendlyJouster) || this.<>f__this.IsJousterBusy(this.<>f__this.m_opponentJouster))
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.$PC = -1;
        Label_0171:
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
    private sealed class <Joust>c__Iterator1C3 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal JoustSpellController <>f__this;
        internal Spell <resultSpellPrefab>__2;
        internal float <revealSec>__0;
        internal float <revealSec>__1;

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
                    if (this.<>f__this.m_friendlyJouster != null)
                    {
                        this.<revealSec>__0 = this.<>f__this.m_RevealTime + this.<>f__this.GetRandomSec();
                        this.<>f__this.RevealJouster(this.<>f__this.m_friendlyJouster, this.<revealSec>__0);
                    }
                    if (this.<>f__this.m_opponentJouster != null)
                    {
                        this.<revealSec>__1 = this.<>f__this.m_RevealTime + this.<>f__this.GetRandomSec();
                        this.<>f__this.RevealJouster(this.<>f__this.m_opponentJouster, this.<revealSec>__1);
                    }
                    if (this.<>f__this.m_sourceJouster == null)
                    {
                        goto Label_017E;
                    }
                    break;

                case 1:
                    break;

                case 2:
                    goto Label_01EC;

                default:
                    goto Label_023E;
            }
            while (this.<>f__this.IsJousterBusy(this.<>f__this.m_friendlyJouster) || this.<>f__this.IsJousterBusy(this.<>f__this.m_opponentJouster))
            {
                this.$current = null;
                this.$PC = 1;
                goto Label_0240;
            }
            this.<resultSpellPrefab>__2 = (this.<>f__this.m_sourceJouster != this.<>f__this.m_winningJouster) ? this.<>f__this.m_LoserSpellPrefab : this.<>f__this.m_WinnerSpellPrefab;
            this.<>f__this.PlaySpellOnActor(this.<>f__this.m_sourceJouster, this.<>f__this.m_sourceJouster.m_revealedActor, this.<resultSpellPrefab>__2);
        Label_017E:
            if ((this.<>f__this.m_friendlyJouster != null) || (this.<>f__this.m_opponentJouster != null))
            {
                object[] args = new object[] { "time", this.<>f__this.m_HoldTime };
                iTween.Timer(this.<>f__this.gameObject, iTween.Hash(args));
            }
        Label_01EC:
            while ((this.<>f__this.IsJousterBusy(this.<>f__this.m_friendlyJouster) || this.<>f__this.IsJousterBusy(this.<>f__this.m_opponentJouster)) || iTween.HasTween(this.<>f__this.gameObject))
            {
                this.$current = null;
                this.$PC = 2;
                goto Label_0240;
            }
            this.$PC = -1;
        Label_023E:
            return false;
        Label_0240:
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
    private sealed class <PlaySpellOnActor>c__AnonStorey328
    {
        internal JoustSpellController.Jouster jouster;

        internal void <>m__15A(Spell spell, object spellUserData)
        {
            this.jouster.m_effectsPendingFinish--;
        }
    }

    [CompilerGenerated]
    private sealed class <RevealJouster>c__AnonStorey326
    {
        internal Actor hiddenActor;
        internal JoustSpellController.Jouster jouster;
        internal Actor revealedActor;
        internal float startAngleZ;

        internal void <>m__157(object tweenUserData)
        {
            float z = this.revealedActor.transform.rotation.eulerAngles.z;
            if (Mathf.DeltaAngle(this.startAngleZ, z) >= 90f)
            {
                this.revealedActor.Show();
                this.hiddenActor.Hide();
            }
        }

        internal void <>m__158(object tweenUserData)
        {
            this.revealedActor.Show();
            this.hiddenActor.Hide();
            this.jouster.m_effectsPendingFinish--;
        }
    }

    [CompilerGenerated]
    private sealed class <ShowJouster>c__AnonStorey325
    {
        internal JoustSpellController <>f__this;
        internal JoustSpellController.Jouster jouster;

        internal void <>m__155(object tweenUserData)
        {
            this.jouster.m_effectsPendingFinish--;
            this.<>f__this.DriftJouster(this.jouster);
        }
    }

    [CompilerGenerated]
    private sealed class <ShowJousters>c__Iterator1C2 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal JoustSpellController <>f__this;
        internal float <delaySec>__13;
        internal float <delaySec>__9;
        internal Transform <friendlyBone>__3;
        internal string <friendlyBoneName>__1;
        internal Vector3 <friendlyToOpponent>__5;
        internal Vector3 <localScale>__11;
        internal Vector3 <localScale>__7;
        internal Transform <opponentBone>__4;
        internal string <opponentBoneName>__2;
        internal Vector3 <position>__12;
        internal Vector3 <position>__8;
        internal Quaternion <rotation>__6;
        internal float <showSec>__10;
        internal float <showSec>__14;
        internal string <showSoundName>__0;

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
                    if (!string.IsNullOrEmpty(this.<>f__this.m_DrawStingerPrefab))
                    {
                        this.<showSoundName>__0 = FileUtils.GameAssetPathToName(this.<>f__this.m_DrawStingerPrefab);
                        SoundManager.Get().LoadAndPlay(this.<showSoundName>__0);
                    }
                    this.<friendlyBoneName>__1 = this.<>f__this.m_FriendlyBoneName;
                    this.<opponentBoneName>__2 = this.<>f__this.m_OpponentBoneName;
                    if (UniversalInputManager.UsePhoneUI != null)
                    {
                        this.<friendlyBoneName>__1 = this.<friendlyBoneName>__1 + "_phone";
                        this.<opponentBoneName>__2 = this.<opponentBoneName>__2 + "_phone";
                    }
                    this.<friendlyBone>__3 = Board.Get().FindBone(this.<friendlyBoneName>__1);
                    this.<opponentBone>__4 = Board.Get().FindBone(this.<opponentBoneName>__2);
                    this.<friendlyToOpponent>__5 = this.<opponentBone>__4.position - this.<friendlyBone>__3.position;
                    this.<rotation>__6 = Quaternion.LookRotation(this.<friendlyToOpponent>__5);
                    if (this.<>f__this.m_friendlyJouster != null)
                    {
                        this.<localScale>__7 = this.<friendlyBone>__3.localScale;
                        this.<position>__8 = this.<friendlyBone>__3.position;
                        this.<delaySec>__9 = this.<>f__this.GetRandomSec();
                        this.<showSec>__10 = this.<>f__this.m_ShowTime + this.<>f__this.GetRandomSec();
                        this.<>f__this.ShowJouster(this.<>f__this.m_friendlyJouster, this.<localScale>__7, this.<rotation>__6, this.<position>__8, this.<delaySec>__9, this.<showSec>__10);
                    }
                    else
                    {
                        this.<>f__this.PlayNoJousterSpell(GameState.Get().GetFriendlySidePlayer());
                    }
                    if (this.<>f__this.m_opponentJouster != null)
                    {
                        this.<localScale>__11 = this.<opponentBone>__4.localScale;
                        this.<position>__12 = this.<opponentBone>__4.position;
                        this.<delaySec>__13 = this.<>f__this.GetRandomSec();
                        this.<showSec>__14 = this.<>f__this.m_ShowTime + this.<>f__this.GetRandomSec();
                        this.<>f__this.ShowJouster(this.<>f__this.m_opponentJouster, this.<localScale>__11, this.<rotation>__6, this.<position>__12, this.<delaySec>__13, this.<showSec>__14);
                    }
                    else
                    {
                        this.<>f__this.PlayNoJousterSpell(GameState.Get().GetOpposingSidePlayer());
                    }
                    break;

                case 1:
                    break;

                default:
                    goto Label_02C8;
            }
            while (this.<>f__this.IsJousterBusy(this.<>f__this.m_friendlyJouster) || this.<>f__this.IsJousterBusy(this.<>f__this.m_opponentJouster))
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.$PC = -1;
        Label_02C8:
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
    private sealed class <WaitForShowEntities>c__Iterator1C1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal JoustSpellController <>f__this;
        internal bool <complete>__0;
        internal PowerTaskList.CompleteCallback <completeCallback>__1;

        internal void <>m__15C(PowerTaskList taskList, int startIndex, int count, object userData)
        {
            this.<complete>__0 = true;
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
                    this.<complete>__0 = false;
                    this.<completeCallback>__1 = new PowerTaskList.CompleteCallback(this.<>m__15C);
                    this.<>f__this.m_taskList.DoTasks(0, this.<>f__this.m_joustTaskIndex, this.<completeCallback>__1);
                    break;

                case 1:
                    break;

                default:
                    goto Label_0086;
            }
            if (!this.<complete>__0)
            {
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.$PC = -1;
        Label_0086:
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

    private class Jouster
    {
        public Card m_card;
        public int m_deckIndex;
        public int m_effectsPendingFinish;
        public Actor m_initialActor;
        public Player m_player;
        public Actor m_revealedActor;
    }
}

