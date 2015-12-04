using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AnimatedLowPolyPack : MonoBehaviour
{
    public ParticleSystem m_DustParticle;
    private Vector3 m_flyInLocalAngles = Vector3.zero;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_FlyInSound = "Assets/Game/Sounds/Pack Purchasing/purchase_pack_drop_impact_1";
    private Vector3 m_flyOutLocalAngles = Vector3.zero;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_FlyOutSound = "Assets/Game/Sounds/Pack Purchasing/purchase_pack_lift_whoosh_1";
    private State m_state;
    private Vector3 m_targetLocalPos = Vector3.zero;
    private Vector3 m_targetOffScreenLocalPos = Vector3.zero;
    private static readonly Vector3 PUNCH_POSITION_AMOUNT = new Vector3(0f, 5f, 0f);
    private static readonly float PUNCH_POSITION_TIME = 0.25f;

    public bool FlyIn(float animTime, float delay)
    {
        if (this.m_state == State.FLOWN_IN)
        {
            return false;
        }
        if (this.m_state == State.FLYING_IN)
        {
            return false;
        }
        this.m_state = State.FLYING_IN;
        base.gameObject.SetActive(true);
        base.transform.localEulerAngles = this.m_flyInLocalAngles;
        object[] args = new object[] { "position", this.m_targetLocalPos, "isLocal", true, "time", animTime, "delay", delay, "easetype", iTween.EaseType.easeInCubic, "oncomplete", "OnFlownIn", "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.Stop(base.gameObject);
        iTween.MoveTo(base.gameObject, hashtable);
        return true;
    }

    public void FlyInImmediate()
    {
        iTween.Stop(base.gameObject);
        base.transform.localEulerAngles = this.m_flyInLocalAngles;
        base.transform.localPosition = this.m_targetLocalPos;
        this.m_state = State.FLOWN_IN;
        base.gameObject.SetActive(true);
    }

    public bool FlyOut(float animTime, float delay)
    {
        if (this.m_state == State.HIDDEN)
        {
            return false;
        }
        if (this.m_state == State.FLYING_OUT)
        {
            return false;
        }
        this.m_state = State.FLYING_OUT;
        base.transform.localEulerAngles = this.m_flyOutLocalAngles;
        object[] args = new object[] { "position", this.m_targetOffScreenLocalPos, "isLocal", true, "time", animTime, "delay", delay, "easetype", iTween.EaseType.linear, "oncomplete", "OnHidden", "oncompletetarget", base.gameObject };
        Hashtable hashtable = iTween.Hash(args);
        iTween.Stop(base.gameObject);
        iTween.MoveTo(base.gameObject, hashtable);
        if (!string.IsNullOrEmpty(this.m_FlyOutSound))
        {
            SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_FlyOutSound));
        }
        return true;
    }

    public void FlyOutImmediate()
    {
        iTween.Stop(base.gameObject);
        base.transform.localEulerAngles = this.m_flyOutLocalAngles;
        base.transform.localPosition = this.m_targetOffScreenLocalPos;
        this.OnHidden();
    }

    public void Hide()
    {
        this.OnHidden();
    }

    public void Init(int column, Vector3 targetLocalPos, Vector3 offScreenOffset)
    {
        this.m_targetLocalPos = targetLocalPos;
        this.m_targetOffScreenLocalPos = targetLocalPos + offScreenOffset;
        this.Column = column;
        SceneUtils.SetLayer(base.gameObject, GameLayer.IgnoreFullScreenEffects);
        this.PositionOffScreen();
    }

    private void OnFlownIn()
    {
        this.m_DustParticle.Play();
        this.m_state = State.FLOWN_IN;
        iTween.PunchPosition(base.gameObject, PUNCH_POSITION_AMOUNT, PUNCH_POSITION_TIME);
        if (!string.IsNullOrEmpty(this.m_FlyInSound))
        {
            SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.m_FlyInSound));
        }
    }

    private void OnHidden()
    {
        this.m_state = State.HIDDEN;
        base.gameObject.SetActive(false);
    }

    private void PositionOffScreen()
    {
        iTween.Stop(base.gameObject);
        base.transform.localPosition = this.m_targetOffScreenLocalPos;
        this.OnHidden();
    }

    public void SetFlyingLocalRotations(Vector3 flyInLocalAngles, Vector3 flyOutLocalAngles)
    {
        this.m_flyInLocalAngles = flyInLocalAngles;
        this.m_flyOutLocalAngles = flyOutLocalAngles;
    }

    public int Column { get; private set; }

    private enum State
    {
        UNKNOWN,
        FLOWN_IN,
        FLYING_IN,
        FLYING_OUT,
        HIDDEN
    }
}

