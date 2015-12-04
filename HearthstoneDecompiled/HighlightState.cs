using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HighlightState : MonoBehaviour
{
    private const string FSM_BIRTH_STATE = "Birth";
    private const string FSM_BIRTHTRANSITION_STATE = "BirthTransition";
    private const string FSM_DEATH_STATE = "Death";
    private const string FSM_DEATHTRANSITION_STATE = "DeathTransition";
    private const string FSM_IDLE_STATE = "Idle";
    private const string FSM_IDLETRANSITION_STATE = "IdleTransition";
    private readonly string HIGHLIGHT_SHADER_NAME = "Custom/Selection/Highlight";
    private string m_BirthTransition = "None";
    protected ActorStateType m_CurrentState;
    private string m_DeathTransition = "None";
    private bool m_forceRerender;
    protected PlayMakerFSM m_FSM;
    private bool m_Hide;
    public List<HighlightRenderState> m_HighlightStates;
    public HighlightStateType m_highlightType;
    public Vector3 m_HistoryTranslation = new Vector3(0f, -0.1f, 0f);
    private string m_IdleTransition = "None";
    private bool m_isDirty;
    private Material m_Material;
    protected ActorStateType m_PreviousState;
    public GameObject m_RenderPlane;
    public int m_RenderQueue;
    private string m_SecondBirthTransition = "None";
    private float m_seed;
    private string m_sendEvent;
    public Texture2D m_StaticSilouetteTexture;
    public Texture2D m_StaticSilouetteTextureUnique;
    private bool m_VisibilityState;

    private void Awake()
    {
        if (this.m_RenderPlane == null)
        {
            if (!Application.isEditor)
            {
                UnityEngine.Debug.LogError("m_RenderPlane is null!");
            }
            base.enabled = false;
        }
        else
        {
            this.m_RenderPlane.GetComponent<Renderer>().enabled = false;
            this.m_VisibilityState = false;
            this.m_FSM = this.m_RenderPlane.GetComponent<PlayMakerFSM>();
        }
        if (this.m_FSM != null)
        {
            this.m_FSM.enabled = true;
        }
        if (this.m_highlightType == HighlightStateType.NONE)
        {
            Transform parent = base.transform.parent;
            if (parent != null)
            {
                if (parent.GetComponent<ActorStateMgr>() != null)
                {
                    this.m_highlightType = HighlightStateType.CARD;
                }
                else
                {
                    this.m_highlightType = HighlightStateType.HIGHLIGHT;
                }
            }
        }
        if (this.m_highlightType == HighlightStateType.NONE)
        {
            UnityEngine.Debug.LogError("m_highlightType is not set!");
            base.enabled = false;
        }
        this.Setup();
    }

    public bool ChangeState(ActorStateType stateType)
    {
        if (stateType == this.m_CurrentState)
        {
            return true;
        }
        this.m_PreviousState = this.m_CurrentState;
        this.m_CurrentState = stateType;
        if (stateType == ActorStateType.NONE)
        {
            this.m_RenderPlane.GetComponent<Renderer>().enabled = false;
            this.m_VisibilityState = false;
            return true;
        }
        if ((stateType == ActorStateType.CARD_IDLE) || (stateType == ActorStateType.HIGHLIGHT_OFF))
        {
            if (this.m_FSM == null)
            {
                this.m_RenderPlane.GetComponent<Renderer>().enabled = false;
                this.m_VisibilityState = false;
                return true;
            }
            this.m_DeathTransition = this.m_PreviousState.ToString();
            this.SendDataToPlaymaker();
            this.SendPlaymakerDeathEvent();
            return true;
        }
        foreach (HighlightRenderState state in this.m_HighlightStates)
        {
            if (state.m_StateType == stateType)
            {
                if ((state.m_Material != null) && (this.m_Material != null))
                {
                    this.m_Material.CopyPropertiesFromMaterial(state.m_Material);
                    this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial = this.m_Material;
                    this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial.SetFloat("_Seed", this.m_seed);
                    bool flag = this.RenderSilouette();
                    if (stateType == ActorStateType.CARD_HISTORY)
                    {
                        base.transform.localPosition = this.m_HistoryTranslation;
                    }
                    else
                    {
                        base.transform.localPosition = Vector3.zero;
                    }
                    if (this.m_FSM == null)
                    {
                        if (!this.m_Hide)
                        {
                            this.m_RenderPlane.GetComponent<Renderer>().enabled = true;
                        }
                        this.m_VisibilityState = true;
                        return flag;
                    }
                    this.m_BirthTransition = stateType.ToString();
                    this.m_SecondBirthTransition = this.m_PreviousState.ToString();
                    this.m_IdleTransition = this.m_BirthTransition;
                    this.SendDataToPlaymaker();
                    this.SendPlaymakerBirthEvent();
                    return flag;
                }
                this.m_RenderPlane.GetComponent<Renderer>().enabled = false;
                this.m_VisibilityState = false;
                return true;
            }
        }
        if (this.m_highlightType == HighlightStateType.CARD)
        {
            this.m_CurrentState = ActorStateType.CARD_IDLE;
        }
        else if (this.m_highlightType == HighlightStateType.HIGHLIGHT)
        {
            this.m_CurrentState = ActorStateType.HIGHLIGHT_OFF;
        }
        this.m_DeathTransition = this.m_PreviousState.ToString();
        this.SendDataToPlaymaker();
        this.SendPlaymakerDeathEvent();
        this.m_RenderPlane.GetComponent<Renderer>().enabled = false;
        this.m_VisibilityState = false;
        return false;
    }

    [DebuggerHidden]
    private IEnumerator ContinuousSilouetteRender(float renderTime)
    {
        return new <ContinuousSilouetteRender>c__Iterator283 { renderTime = renderTime, <$>renderTime = renderTime, <>f__this = this };
    }

    public void ContinuousUpdate(float updateTime)
    {
        base.StartCoroutine(this.ContinuousSilouetteRender(updateTime));
    }

    public void ForceUpdate()
    {
        this.m_isDirty = true;
        this.m_forceRerender = true;
    }

    public void Hide()
    {
        this.m_Hide = true;
        if (this.m_RenderPlane != null)
        {
            this.m_RenderPlane.GetComponent<Renderer>().enabled = false;
        }
    }

    public bool IsReady()
    {
        return (this.m_Material != null);
    }

    private void LateUpdate()
    {
    }

    public void OnActionFinished()
    {
    }

    private void OnApplicationFocus(bool state)
    {
        this.m_isDirty = true;
        this.m_forceRerender = true;
    }

    protected void OnDestroy()
    {
        if (this.m_Material != null)
        {
            UnityEngine.Object.Destroy(this.m_Material);
        }
    }

    private bool RenderSilouette()
    {
        this.m_isDirty = false;
        if (this.m_StaticSilouetteTexture != null)
        {
            if (this.m_StaticSilouetteTextureUnique != null)
            {
                Actor actor = SceneUtils.FindComponentInParents<Actor>(base.gameObject);
                if ((actor != null) && actor.IsElite())
                {
                    this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial.mainTexture = this.m_StaticSilouetteTextureUnique;
                    this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial.renderQueue = 0xbb8 + this.m_RenderQueue;
                    this.m_forceRerender = false;
                    return true;
                }
            }
            this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial.mainTexture = this.m_StaticSilouetteTexture;
            this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial.renderQueue = 0xbb8 + this.m_RenderQueue;
            this.m_forceRerender = false;
            return true;
        }
        HighlightRender component = this.m_RenderPlane.GetComponent<HighlightRender>();
        if (component == null)
        {
            UnityEngine.Debug.LogError("Unable to find HighlightRender component on m_RenderPlane");
            return false;
        }
        if (component.enabled)
        {
            component.CreateSilhouetteTexture(this.m_forceRerender);
            this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial.mainTexture = component.SilhouetteTexture;
            this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial.renderQueue = 0xbb8 + this.m_RenderQueue;
        }
        this.m_forceRerender = false;
        return true;
    }

    private void SendDataToPlaymaker()
    {
        if (this.m_FSM != null)
        {
            FsmMaterial fsmMaterial = this.m_FSM.FsmVariables.GetFsmMaterial("HighlightMaterial");
            if (fsmMaterial != null)
            {
                fsmMaterial.Value = this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial;
            }
            FsmString fsmString = this.m_FSM.FsmVariables.GetFsmString("CurrentState");
            if (fsmString != null)
            {
                fsmString.Value = this.m_CurrentState.ToString();
            }
            FsmString str2 = this.m_FSM.FsmVariables.GetFsmString("PreviousState");
            if (str2 != null)
            {
                str2.Value = this.m_PreviousState.ToString();
            }
        }
    }

    private void SendPlaymakerBirthEvent()
    {
        if (this.m_FSM != null)
        {
            FsmString fsmString = this.m_FSM.FsmVariables.GetFsmString("BirthTransition");
            if (fsmString != null)
            {
                fsmString.Value = this.m_BirthTransition;
            }
            FsmString str2 = this.m_FSM.FsmVariables.GetFsmString("SecondBirthTransition");
            if (str2 != null)
            {
                str2.Value = this.m_SecondBirthTransition;
            }
            FsmString str3 = this.m_FSM.FsmVariables.GetFsmString("IdleTransition");
            if (str3 != null)
            {
                str3.Value = this.m_IdleTransition;
            }
            this.m_FSM.SendEvent("Birth");
        }
    }

    private void SendPlaymakerDeathEvent()
    {
        if (this.m_FSM != null)
        {
            FsmString fsmString = this.m_FSM.FsmVariables.GetFsmString("DeathTransition");
            if (fsmString != null)
            {
                fsmString.Value = this.m_DeathTransition;
            }
            this.m_FSM.SendEvent("Death");
        }
    }

    public void SetDirty()
    {
        this.m_isDirty = true;
    }

    private void Setup()
    {
        this.m_seed = UnityEngine.Random.value;
        this.m_CurrentState = ActorStateType.CARD_IDLE;
        this.m_RenderPlane.GetComponent<Renderer>().enabled = false;
        this.m_VisibilityState = false;
        if (this.m_Material == null)
        {
            Shader shader = ShaderUtils.FindShader(this.HIGHLIGHT_SHADER_NAME);
            if (shader == null)
            {
                UnityEngine.Debug.LogError("Failed to load Highlight Shader: " + this.HIGHLIGHT_SHADER_NAME);
                base.enabled = false;
            }
            this.m_Material = new Material(shader);
            this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial = this.m_Material;
        }
        this.m_RenderPlane.GetComponent<Renderer>().sharedMaterial = this.m_Material;
    }

    public void Show()
    {
        this.m_Hide = false;
        if ((this.m_RenderPlane != null) && (this.m_VisibilityState && !this.m_RenderPlane.GetComponent<Renderer>().enabled))
        {
            this.m_RenderPlane.GetComponent<Renderer>().enabled = true;
        }
    }

    private void Update()
    {
        if (this.m_Hide)
        {
            if (this.m_RenderPlane != null)
            {
                this.m_RenderPlane.GetComponent<Renderer>().enabled = false;
            }
        }
        else if ((this.m_isDirty && (this.m_RenderPlane != null)) && this.m_RenderPlane.GetComponent<Renderer>().enabled)
        {
            this.UpdateSilouette();
            this.m_isDirty = false;
        }
    }

    protected void UpdateSilouette()
    {
        this.RenderSilouette();
    }

    [CompilerGenerated]
    private sealed class <ContinuousSilouetteRender>c__Iterator283 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>renderTime;
        internal HighlightState <>f__this;
        internal float <endTime>__0;
        internal float renderTime;

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
                    if (GraphicsManager.Get().RenderQualityLevel != GraphicsQuality.Low)
                    {
                        this.<endTime>__0 = UnityEngine.Time.realtimeSinceStartup + this.renderTime;
                        break;
                    }
                    this.$current = new WaitForSeconds(this.renderTime);
                    this.$PC = 1;
                    goto Label_0124;

                case 1:
                    if (this.<>f__this.m_RenderPlane.GetComponent<Renderer>().enabled)
                    {
                        this.<>f__this.m_isDirty = true;
                        this.<>f__this.m_forceRerender = true;
                        this.<>f__this.RenderSilouette();
                    }
                    goto Label_0122;

                case 2:
                    break;

                default:
                    goto Label_0122;
            }
            while (UnityEngine.Time.realtimeSinceStartup < this.<endTime>__0)
            {
                if (!this.<>f__this.m_RenderPlane.GetComponent<Renderer>().enabled)
                {
                    break;
                }
                this.<>f__this.m_isDirty = true;
                this.<>f__this.m_forceRerender = true;
                this.<>f__this.RenderSilouette();
                this.$current = null;
                this.$PC = 2;
                goto Label_0124;
            }
            goto Label_0122;
            this.$PC = -1;
        Label_0122:
            return false;
        Label_0124:
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

