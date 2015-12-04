using System;
using UnityEngine;

public class CardBackDragEffect : MonoBehaviour
{
    private bool m_Active;
    public Actor m_Actor;
    private CardBackManager m_CardBackManager;
    public GameObject m_EffectsRoot;
    private Vector3 m_LastPosition;
    private float m_Max = 30f;
    private float m_Min = 2f;
    private float m_Speed;
    private const float MAX_VELOCITY = 30f;
    private const float MIN_VELOCITY = 2f;

    private void Awake()
    {
    }

    private void FixedUpdate()
    {
    }

    private void OnDestroy()
    {
    }

    private void OnDisable()
    {
        if (this.m_EffectsRoot != null)
        {
            this.ShowParticles(false);
        }
    }

    private void OnEnable()
    {
    }

    public void SetEffect()
    {
        if (this.m_CardBackManager == null)
        {
            this.m_CardBackManager = CardBackManager.Get();
            if (this.m_CardBackManager == null)
            {
                Debug.LogError("Failed to get CardBackManager!");
                base.enabled = false;
                return;
            }
        }
        bool friendlySide = true;
        Entity entity = this.m_Actor.GetEntity();
        if (entity != null)
        {
            Player controller = entity.GetController();
            if ((controller != null) && (controller.GetSide() == Player.Side.OPPOSING))
            {
                friendlySide = false;
            }
        }
        this.m_CardBackManager.UpdateDragEffect(base.gameObject, friendlySide);
        CardBack cardBack = this.m_CardBackManager.GetCardBack(this.m_Actor);
        if (cardBack != null)
        {
            this.m_Min = cardBack.m_EffectMinVelocity;
            this.m_Max = cardBack.m_EffectMaxVelocity;
        }
    }

    private void ShowParticles(bool show)
    {
        if (show)
        {
            foreach (ParticleSystem system in base.GetComponentsInChildren<ParticleSystem>())
            {
                if (system != null)
                {
                    system.Play();
                }
            }
        }
        else
        {
            foreach (ParticleSystem system2 in base.GetComponentsInChildren<ParticleSystem>())
            {
                if (system2 == null)
                {
                    return;
                }
                system2.Stop();
            }
        }
    }

    private void Start()
    {
        if ((SceneMgr.Get() == null) || (SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY))
        {
            base.enabled = false;
        }
        else
        {
            this.m_LastPosition = base.transform.position;
            if (this.m_CardBackManager == null)
            {
                this.m_CardBackManager = CardBackManager.Get();
                if (this.m_CardBackManager == null)
                {
                    Debug.LogError("Failed to get CardBackManager!");
                    base.enabled = false;
                }
            }
            this.SetEffect();
        }
    }

    private void Update()
    {
        if (this.m_EffectsRoot != null)
        {
            if (!base.GetComponent<Renderer>().enabled)
            {
                this.ShowParticles(false);
                if (this.m_EffectsRoot.activeSelf)
                {
                    this.m_EffectsRoot.SetActive(false);
                }
            }
            else
            {
                Vector3 vector = base.transform.position - this.m_LastPosition;
                this.m_Speed = (vector.magnitude / UnityEngine.Time.deltaTime) * 3.6f;
                this.UpdateDragEffect();
                this.m_LastPosition = base.transform.position;
            }
        }
    }

    private void UpdateDragEffect()
    {
        if ((this.m_Speed > this.m_Min) && (this.m_Speed < this.m_Max))
        {
            if (!this.m_Active)
            {
                this.m_Active = true;
                this.ShowParticles(true);
            }
        }
        else if (this.m_Active)
        {
            this.m_Active = false;
            this.ShowParticles(false);
        }
    }
}

