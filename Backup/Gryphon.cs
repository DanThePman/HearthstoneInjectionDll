using System;
using System.Collections.Generic;
using UnityEngine;

public class Gryphon : MonoBehaviour
{
    private static int cleanState = Animator.StringToHash("Base Layer.Clean");
    private static int lookState = Animator.StringToHash("Base Layer.Look");
    private Animator m_Animator;
    private AnimatorStateInfo m_CurrentBaseLayerState;
    private EndTurnButton m_EndTurnButton;
    private Transform m_EndTurnButtonTransform;
    public Transform m_HeadBone;
    public float m_HeadRotationSpeed = 15f;
    private float m_idleEndTime;
    private float m_lastScreech;
    public int m_LookAtHeroesPercent = 20;
    private Vector3 m_LookAtPosition;
    public int m_LookAtTurnButtonPercent = 0x4b;
    public float m_MaxFocusTime = 5.5f;
    public float m_MinFocusTime = 1.2f;
    public int m_PlayAnimationPercent = 20;
    private float m_RandomWeightsTotal;
    private AudioSource m_ScreechSound;
    public GameObject m_SnapCollider;
    public float m_SnapWaitTime = 1f;
    public float m_TurnButtonLookAwayTime = 0.5f;
    private UniversalInputManager m_UniversalInputManager;
    private float m_WaitStartTime;
    private static int screechState = Animator.StringToHash("Base Layer.Screech");

    private void AniamteHead()
    {
        if (((this.m_CurrentBaseLayerState.fullPathHash != lookState) && (this.m_CurrentBaseLayerState.fullPathHash != cleanState)) && (this.m_CurrentBaseLayerState.fullPathHash != screechState))
        {
            Vector3 forward = this.m_LookAtPosition - this.m_HeadBone.position;
            Quaternion to = Quaternion.LookRotation(forward);
            this.m_HeadBone.rotation = Quaternion.Slerp(this.m_HeadBone.rotation, to, UnityEngine.Time.deltaTime * this.m_HeadRotationSpeed);
        }
    }

    private void FindEndTurnButton()
    {
        this.m_EndTurnButton = EndTurnButton.Get();
        if (this.m_EndTurnButton != null)
        {
            this.m_EndTurnButtonTransform = this.m_EndTurnButton.transform;
        }
    }

    private void FindSomethingToLookAt()
    {
        List<Vector3> list = new List<Vector3>();
        ZoneMgr mgr = ZoneMgr.Get();
        if (mgr == null)
        {
            this.PlayAniamtion();
        }
        else
        {
            foreach (ZonePlay play in mgr.FindZonesOfType<ZonePlay>())
            {
                foreach (Card card in play.GetCards())
                {
                    if (card.IsMousedOver())
                    {
                        this.m_LookAtPosition = card.transform.position;
                        return;
                    }
                    list.Add(card.transform.position);
                }
            }
            if (UnityEngine.Random.Range(0, 100) < this.m_LookAtHeroesPercent)
            {
                foreach (ZoneHero hero in ZoneMgr.Get().FindZonesOfType<ZoneHero>())
                {
                    foreach (Card card2 in hero.GetCards())
                    {
                        if (card2.IsMousedOver())
                        {
                            this.m_LookAtPosition = card2.transform.position;
                            return;
                        }
                        list.Add(card2.transform.position);
                    }
                }
            }
            if (list.Count > 0)
            {
                int num = UnityEngine.Random.Range(0, list.Count);
                this.m_LookAtPosition = list[num];
            }
            else
            {
                this.PlayAniamtion();
            }
        }
    }

    private void LateUpdate()
    {
        bool mouseButtonDown = false;
        this.m_CurrentBaseLayerState = this.m_Animator.GetCurrentAnimatorStateInfo(0);
        if (this.m_UniversalInputManager != null)
        {
            if ((GameState.Get() != null) && GameState.Get().IsMulliganManagerActive())
            {
                return;
            }
            if (this.m_UniversalInputManager.InputIsOver(base.gameObject))
            {
                mouseButtonDown = UniversalInputManager.Get().GetMouseButtonDown(0);
            }
            if (mouseButtonDown)
            {
                if ((UnityEngine.Time.time - this.m_lastScreech) > 5f)
                {
                    this.m_Animator.SetBool("Screech", true);
                    SoundManager.Get().Play(this.m_ScreechSound);
                    this.m_lastScreech = UnityEngine.Time.time;
                }
            }
            else
            {
                this.m_Animator.SetBool("Screech", false);
            }
        }
        if ((this.m_CurrentBaseLayerState.fullPathHash != lookState) && ((this.m_CurrentBaseLayerState.fullPathHash != cleanState) && (this.m_CurrentBaseLayerState.fullPathHash != screechState)))
        {
            this.m_Animator.SetBool("Look", false);
            this.m_Animator.SetBool("Clean", false);
            this.PlayAniamtion();
        }
    }

    private bool LookAtTurnButton()
    {
        if (this.m_EndTurnButton == null)
        {
            this.FindEndTurnButton();
        }
        if ((this.m_EndTurnButton != null) && (this.m_EndTurnButton.IsInNMPState() && (this.m_EndTurnButtonTransform != null)))
        {
            this.m_LookAtPosition = this.m_EndTurnButtonTransform.position;
            return true;
        }
        return false;
    }

    private void PlayAniamtion()
    {
        if (UnityEngine.Time.time >= this.m_idleEndTime)
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                this.m_idleEndTime = UnityEngine.Time.time + 4f;
                this.m_Animator.SetBool("Look", false);
                this.m_Animator.SetBool("Clean", false);
            }
            else if (UnityEngine.Random.value > 0.25f)
            {
                this.m_Animator.SetBool("Look", true);
            }
            else
            {
                this.m_Animator.SetBool("Clean", true);
            }
        }
    }

    private void Start()
    {
        this.m_Animator = base.GetComponent<Animator>();
        this.m_UniversalInputManager = UniversalInputManager.Get();
        this.m_ScreechSound = base.GetComponent<AudioSource>();
        this.m_SnapWaitTime = UnityEngine.Random.Range((float) 5f, (float) 20f);
        this.m_Animator.SetLayerWeight(1, 1f);
    }
}

