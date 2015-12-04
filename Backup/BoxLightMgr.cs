using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BoxLightMgr : MonoBehaviour
{
    private BoxLightStateType m_activeStateType = BoxLightStateType.DEFAULT;
    public List<BoxLightState> m_States;

    private void ChangeAmbient(BoxLightState state)
    {
        <ChangeAmbient>c__AnonStorey2BC storeybc = new <ChangeAmbient>c__AnonStorey2BC {
            state = state,
            prevAmbientColor = RenderSettings.ambientLight
        };
        Action<object> action = new Action<object>(storeybc.<>m__73);
        object[] args = new object[] { "from", 0f, "to", 1f, "delay", storeybc.state.m_DelaySec, "time", storeybc.state.m_TransitionSec, "easetype", storeybc.state.m_TransitionEaseType, "onupdate", action };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ValueTo(base.gameObject, hashtable);
    }

    private void ChangeLight(BoxLightState state, BoxLightInfo lightInfo)
    {
        <ChangeLight>c__AnonStorey2BD storeybd = new <ChangeLight>c__AnonStorey2BD {
            lightInfo = lightInfo
        };
        iTween.Stop(storeybd.lightInfo.m_Light.gameObject);
        object[] args = new object[] { "color", storeybd.lightInfo.m_Color, "delay", state.m_DelaySec, "time", state.m_TransitionSec, "easetype", state.m_TransitionEaseType };
        Hashtable hashtable = iTween.Hash(args);
        iTween.ColorTo(storeybd.lightInfo.m_Light.gameObject, hashtable);
        float intensity = storeybd.lightInfo.m_Light.intensity;
        Action<object> action = new Action<object>(storeybd.<>m__74);
        object[] objArray2 = new object[] { "from", intensity, "to", storeybd.lightInfo.m_Intensity, "delay", state.m_DelaySec, "time", state.m_TransitionSec, "easetype", state.m_TransitionEaseType, "onupdate", action };
        Hashtable hashtable2 = iTween.Hash(objArray2);
        iTween.ValueTo(storeybd.lightInfo.m_Light.gameObject, hashtable2);
        LightType type = storeybd.lightInfo.m_Light.type;
        switch (type)
        {
            case LightType.Point:
            case LightType.Spot:
            {
                float range = storeybd.lightInfo.m_Light.range;
                Action<object> action2 = new Action<object>(storeybd.<>m__75);
                object[] objArray3 = new object[] { "from", range, "to", storeybd.lightInfo.m_Range, "delay", state.m_DelaySec, "time", state.m_TransitionSec, "easetype", state.m_TransitionEaseType, "onupdate", action2 };
                Hashtable hashtable3 = iTween.Hash(objArray3);
                iTween.ValueTo(storeybd.lightInfo.m_Light.gameObject, hashtable3);
                if (type == LightType.Spot)
                {
                    float spotAngle = storeybd.lightInfo.m_Light.spotAngle;
                    Action<object> action3 = new Action<object>(storeybd.<>m__76);
                    object[] objArray4 = new object[] { "from", spotAngle, "to", storeybd.lightInfo.m_SpotAngle, "delay", state.m_DelaySec, "time", state.m_TransitionSec, "easetype", state.m_TransitionEaseType, "onupdate", action3 };
                    Hashtable hashtable4 = iTween.Hash(objArray4);
                    iTween.ValueTo(storeybd.lightInfo.m_Light.gameObject, hashtable4);
                }
                break;
            }
        }
    }

    public void ChangeState(BoxLightStateType stateType)
    {
        if ((stateType != BoxLightStateType.INVALID) && (this.m_activeStateType != stateType))
        {
            this.ChangeStateImpl(stateType);
        }
    }

    private void ChangeStateImpl(BoxLightStateType stateType)
    {
        this.m_activeStateType = stateType;
        BoxLightState state = this.FindState(stateType);
        if (state != null)
        {
            iTween.Stop(base.gameObject);
            state.m_Spell.ActivateState(SpellStateType.BIRTH);
            this.ChangeAmbient(state);
            if (state.m_LightInfos != null)
            {
                foreach (BoxLightInfo info in state.m_LightInfos)
                {
                    this.ChangeLight(state, info);
                }
            }
        }
    }

    private BoxLightState FindState(BoxLightStateType stateType)
    {
        foreach (BoxLightState state in this.m_States)
        {
            if (state.m_Type == stateType)
            {
                return state;
            }
        }
        return null;
    }

    public BoxLightStateType GetActiveState()
    {
        return this.m_activeStateType;
    }

    public void SetState(BoxLightStateType stateType)
    {
        if (this.m_activeStateType != stateType)
        {
            this.m_activeStateType = stateType;
            this.UpdateState();
        }
    }

    private void Start()
    {
        this.UpdateState();
    }

    public void UpdateState()
    {
        BoxLightState state = this.FindState(this.m_activeStateType);
        if (state != null)
        {
            state.m_Spell.ActivateState(SpellStateType.ACTION);
            iTween.Stop(base.gameObject);
            RenderSettings.ambientLight = state.m_AmbientColor;
            if (state.m_LightInfos != null)
            {
                foreach (BoxLightInfo info in state.m_LightInfos)
                {
                    iTween.Stop(info.m_Light.gameObject);
                    info.m_Light.color = info.m_Color;
                    info.m_Light.intensity = info.m_Intensity;
                    LightType type = info.m_Light.type;
                    switch (type)
                    {
                        case LightType.Point:
                        case LightType.Spot:
                            info.m_Light.range = info.m_Range;
                            if (type == LightType.Spot)
                            {
                                info.m_Light.spotAngle = info.m_SpotAngle;
                            }
                            break;
                    }
                }
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ChangeAmbient>c__AnonStorey2BC
    {
        internal Color prevAmbientColor;
        internal BoxLightState state;

        internal void <>m__73(object amount)
        {
            RenderSettings.ambientLight = Color.Lerp(this.prevAmbientColor, this.state.m_AmbientColor, (float) amount);
        }
    }

    [CompilerGenerated]
    private sealed class <ChangeLight>c__AnonStorey2BD
    {
        internal BoxLightInfo lightInfo;

        internal void <>m__74(object amount)
        {
            this.lightInfo.m_Light.intensity = (float) amount;
        }

        internal void <>m__75(object amount)
        {
            this.lightInfo.m_Light.range = (float) amount;
        }

        internal void <>m__76(object amount)
        {
            this.lightInfo.m_Light.spotAngle = (float) amount;
        }
    }
}

