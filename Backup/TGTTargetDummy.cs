using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class TGTTargetDummy : MonoBehaviour
{
    public GameObject m_Body;
    public float m_BodyHitIntensity = 25f;
    public GameObject m_BodyMesh;
    public GameObject m_BodyRotX;
    public GameObject m_BodyRotY;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HitBodySoundPrefab;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HitShieldSoundPrefab;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HitSpinSoundPrefab;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_HitSwordSoundPrefab;
    private Quaternion m_lastFrameSqueakAngle;
    private float m_lastSqueakSoundVol;
    public GameObject m_Shield;
    public float m_ShieldHitIntensity = 25f;
    private AudioSource m_squeakSound;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_SqueakSoundPrefab;
    private float m_squeakSoundVelocity;
    public GameObject m_Sword;
    public float m_SwordHitIntensity = -25f;
    private static TGTTargetDummy s_instance;
    private const int SPIN_PERCENT = 5;

    public void ArrowHit()
    {
        this.m_BodyRotX.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range((float) (this.m_BodyHitIntensity * 0.25f), (float) (this.m_BodyHitIntensity * 0.5f)), 0f, 0f);
    }

    private void Awake()
    {
        s_instance = this;
    }

    public void BodyHit()
    {
        this.PlaySqueakSound();
        if (!string.IsNullOrEmpty(this.m_HitBodySoundPrefab))
        {
            string str = FileUtils.GameAssetPathToName(this.m_HitBodySoundPrefab);
            if (!string.IsNullOrEmpty(str))
            {
                SoundManager.Get().LoadAndPlay(str, this.m_Body);
            }
        }
        this.m_BodyRotX.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range(this.m_BodyHitIntensity * 0.75f, this.m_BodyHitIntensity), 0f, 0f);
        this.m_BodyRotY.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, UnityEngine.Random.Range((float) -5f, (float) 5f), 0f);
    }

    public static TGTTargetDummy Get()
    {
        return s_instance;
    }

    private void HandleHits()
    {
        if (UniversalInputManager.Get().GetMouseButtonDown(0) && this.IsOver(this.m_Body))
        {
            this.BodyHit();
        }
        if (UniversalInputManager.Get().GetMouseButtonDown(0) && this.IsOver(this.m_Shield))
        {
            this.ShieldHit();
        }
        if (UniversalInputManager.Get().GetMouseButtonDown(0) && this.IsOver(this.m_Sword))
        {
            this.SwordHit();
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

    private void PlaySqueakSound()
    {
        base.StopCoroutine("SqueakSound");
        this.m_lastSqueakSoundVol = 0f;
        base.StartCoroutine("SqueakSound");
    }

    [DebuggerHidden]
    private IEnumerator RegisterBoardEventLargeShake()
    {
        return new <RegisterBoardEventLargeShake>c__Iterator1A { <>f__this = this };
    }

    public void ShieldHit()
    {
        this.PlaySqueakSound();
        if (UnityEngine.Random.Range(0, 100) < 5)
        {
            this.Spin(false);
        }
        else
        {
            if (!string.IsNullOrEmpty(this.m_HitShieldSoundPrefab))
            {
                string str = FileUtils.GameAssetPathToName(this.m_HitShieldSoundPrefab);
                if (!string.IsNullOrEmpty(str))
                {
                    SoundManager.Get().LoadAndPlay(str, this.m_Body);
                }
            }
            this.m_BodyRotY.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, UnityEngine.Random.Range(this.m_ShieldHitIntensity * 0.7f, this.m_ShieldHitIntensity), 0f);
            this.m_BodyRotX.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range((float) -5f, (float) -10f), 0f, 0f);
        }
    }

    private void Spin(bool reverse)
    {
        float num = 1080f;
        if (reverse)
        {
            num = -1080f;
        }
        if (!string.IsNullOrEmpty(this.m_HitSpinSoundPrefab))
        {
            string str = FileUtils.GameAssetPathToName(this.m_HitSpinSoundPrefab);
            if (!string.IsNullOrEmpty(str))
            {
                SoundManager.Get().LoadAndPlay(str, this.m_Body);
            }
        }
        this.m_BodyMesh.transform.localEulerAngles = Vector3.zero;
        Vector3 vector = new Vector3(0f, this.m_BodyMesh.transform.localEulerAngles.y + num, 0f);
        object[] args = new object[] { "rotation", vector, "isLocal", true, "time", 3f, "easetype", iTween.EaseType.easeOutElastic };
        Hashtable hashtable = iTween.Hash(args);
        iTween.RotateTo(this.m_BodyMesh, hashtable);
    }

    [DebuggerHidden]
    private IEnumerator SqueakSound()
    {
        return new <SqueakSound>c__Iterator1B { <>f__this = this };
    }

    private void Start()
    {
        base.StartCoroutine(this.RegisterBoardEventLargeShake());
        this.m_BodyRotX.GetComponent<Rigidbody>().maxAngularVelocity = this.m_BodyHitIntensity;
        this.m_BodyRotY.GetComponent<Rigidbody>().maxAngularVelocity = Mathf.Max(this.m_SwordHitIntensity, this.m_ShieldHitIntensity);
    }

    public void SwordHit()
    {
        this.PlaySqueakSound();
        if (UnityEngine.Random.Range(0, 100) < 5)
        {
            this.Spin(true);
        }
        else
        {
            if (!string.IsNullOrEmpty(this.m_HitSwordSoundPrefab))
            {
                string str = FileUtils.GameAssetPathToName(this.m_HitSwordSoundPrefab);
                if (!string.IsNullOrEmpty(str))
                {
                    SoundManager.Get().LoadAndPlay(str, this.m_Body);
                }
            }
            this.m_BodyRotY.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, UnityEngine.Random.Range(this.m_SwordHitIntensity * 0.7f, this.m_SwordHitIntensity), 0f);
            this.m_BodyRotX.GetComponent<Rigidbody>().angularVelocity = new Vector3(UnityEngine.Random.Range((float) -5f, (float) -10f), 0f, 0f);
        }
    }

    private void Update()
    {
        this.HandleHits();
    }

    [CompilerGenerated]
    private sealed class <RegisterBoardEventLargeShake>c__Iterator1A : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TGTTargetDummy <>f__this;

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
                    }
                    else
                    {
                        this.$current = new WaitForSeconds(2f);
                        this.$PC = 2;
                    }
                    return true;

                case 2:
                    BoardEvents.Get().RegisterLargeShakeEvent(new BoardEvents.LargeShakeEventDelegate(this.<>f__this.BodyHit));
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
    private sealed class <SqueakSound>c__Iterator1B : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TGTTargetDummy <>f__this;
        internal float <difAngle>__1;
        internal GameObject <squeakSoundGO>__0;

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
                    if (!string.IsNullOrEmpty(this.<>f__this.m_SqueakSoundPrefab))
                    {
                        if ((this.<>f__this.m_squeakSound != null) && this.<>f__this.m_squeakSound.isPlaying)
                        {
                            SoundManager.Get().Stop(this.<>f__this.m_squeakSound);
                        }
                        if (this.<>f__this.m_squeakSound == null)
                        {
                            this.<squeakSoundGO>__0 = AssetLoader.Get().LoadSound(FileUtils.GameAssetPathToName(this.<>f__this.m_SqueakSoundPrefab), true, false);
                            this.<squeakSoundGO>__0.transform.position = this.<>f__this.m_Body.transform.position;
                            this.<>f__this.m_squeakSound = this.<squeakSoundGO>__0.GetComponent<AudioSource>();
                        }
                        if (this.<>f__this.m_squeakSound != null)
                        {
                            SoundManager.Get().PlayPreloaded(this.<>f__this.m_squeakSound, (float) 0f);
                            break;
                        }
                    }
                    goto Label_0221;

                case 1:
                    break;

                default:
                    goto Label_0221;
            }
            while ((this.<>f__this.m_squeakSound != null) && this.<>f__this.m_squeakSound.isPlaying)
            {
                this.<difAngle>__1 = Mathf.Clamp01(Quaternion.Angle(this.<>f__this.m_Body.transform.rotation, this.<>f__this.m_lastFrameSqueakAngle) * 0.1f);
                this.<>f__this.m_lastFrameSqueakAngle = this.<>f__this.m_Body.transform.rotation;
                this.<difAngle>__1 = Mathf.SmoothDamp(this.<difAngle>__1, this.<>f__this.m_lastSqueakSoundVol, ref this.<>f__this.m_squeakSoundVelocity, 0.5f);
                this.<>f__this.m_lastSqueakSoundVol = this.<difAngle>__1;
                SoundManager.Get().SetVolume(this.<>f__this.m_squeakSound, Mathf.Clamp01(this.<difAngle>__1));
                this.$current = null;
                this.$PC = 1;
                return true;
            }
            this.$PC = -1;
        Label_0221:
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

