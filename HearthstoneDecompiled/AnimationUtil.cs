using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class AnimationUtil : MonoBehaviour
{
    public static void DelayedActivate(GameObject go, float time, bool activate)
    {
        go.GetComponent<MonoBehaviour>().StartCoroutine(DelayedActivation(go, time, activate));
    }

    [DebuggerHidden]
    private static IEnumerator DelayedActivation(GameObject go, float time, bool activate)
    {
        return new <DelayedActivation>c__Iterator270 { time = time, go = go, activate = activate, <$>time = time, <$>go = go, <$>activate = activate };
    }

    [DebuggerHidden]
    private static IEnumerator DriftAfterTween(GameObject go, float delayTime, Vector3 driftOffset)
    {
        return new <DriftAfterTween>c__Iterator26F { delayTime = delayTime, go = go, driftOffset = driftOffset, <$>delayTime = delayTime, <$>go = go, <$>driftOffset = driftOffset };
    }

    public static void DriftObject(GameObject go, Vector3 driftOffset)
    {
        iTween.StopByName(go, "DRIFT_MOVE_OBJECT_ITWEEN");
        object[] args = new object[] { "amount", driftOffset, "time", 10f, "name", "DRIFT_MOVE_OBJECT_ITWEEN", "easeType", iTween.EaseType.easeOutQuart };
        iTween.MoveBy(go, iTween.Hash(args));
    }

    public static void FadeTexture(MeshRenderer mesh, float fromAlpha, float toAlpha, float fadeTime, float delay, DelOnFade onCompleteCallback = null)
    {
        <FadeTexture>c__AnonStorey38C storeyc = new <FadeTexture>c__AnonStorey38C {
            onCompleteCallback = onCompleteCallback
        };
        iTween.StopByName(mesh.gameObject, "FADE_TEXTURE");
        storeyc.logoMaterial = mesh.materials[0];
        storeyc.currentColor = storeyc.logoMaterial.GetColor("_Color");
        storeyc.currentColor.a = fromAlpha;
        storeyc.logoMaterial.SetColor("_Color", storeyc.currentColor);
        object[] args = new object[] { "from", fromAlpha, "to", toAlpha, "time", fadeTime, "onupdate", new Action<object>(storeyc.<>m__286), "name", "FADE_TEXTURE" };
        Hashtable hashtable = iTween.Hash(args);
        if (delay > 0f)
        {
            hashtable.Add("delay", delay);
        }
        if (storeyc.onCompleteCallback != null)
        {
            hashtable.Add("oncomplete", new Action<object>(storeyc.<>m__287));
        }
        iTween.ValueTo(mesh.gameObject, hashtable);
    }

    public static void FloatyPosition(GameObject go, float radius, float loopTime)
    {
        FloatyPosition(go, go.transform.localPosition, radius, loopTime);
    }

    public static void FloatyPosition(GameObject go, Vector3 startPos, float localRadius, float loopTime)
    {
        Vector3[] vectorArray = new Vector3[] { startPos, startPos + new Vector3(localRadius, 0f, localRadius), startPos + new Vector3(localRadius * 2f, 0f, 0f), startPos + new Vector3(localRadius, 0f, -localRadius), startPos + Vector3.zero };
        iTween.StopByName("DriftingTween");
        object[] args = new object[] { "name", "DriftingTween", "path", vectorArray, "time", loopTime, "islocal", true, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.loop, "movetopath", false };
        iTween.MoveTo(go, iTween.Hash(args));
    }

    public static int GetLayerIndexFromName(Animator animator, string layerName)
    {
        if (layerName != null)
        {
            layerName = layerName.Trim();
            for (int i = 0; i < animator.layerCount; i++)
            {
                string str = animator.GetLayerName(i);
                if ((str != null) && str.Trim().Equals(layerName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public static void GrowThenDrift(GameObject go, Vector3 origin, Vector3 driftOffset)
    {
        object[] args = new object[] { "scale", (Vector3) (Vector3.one * 0.05f), "time", 0.15f, "easeType", iTween.EaseType.easeOutQuart };
        iTween.ScaleFrom(go, iTween.Hash(args));
        object[] objArray2 = new object[] { "position", origin, "time", 0.15f, "easeType", iTween.EaseType.easeOutQuart };
        iTween.MoveFrom(go, iTween.Hash(objArray2));
        go.GetComponent<MonoBehaviour>().StartCoroutine(DriftAfterTween(go, 0.15f, driftOffset));
    }

    public static void ScaleFade(GameObject go, Vector3 scale)
    {
        ScaleFade(go, scale, null);
    }

    public static void ScaleFade(GameObject go, Vector3 scale, string callbackName)
    {
        Hashtable hashtable;
        iTween.FadeTo(go, 0f, 0.25f);
        if (string.IsNullOrEmpty(callbackName))
        {
            object[] args = new object[] { "scale", scale, "time", 0.25f };
            hashtable = iTween.Hash(args);
        }
        else
        {
            object[] objArray2 = new object[] { "scale", scale, "time", 0.25f, "oncomplete", callbackName, "oncompletetarget", go };
            hashtable = iTween.Hash(objArray2);
        }
        iTween.ScaleTo(go, hashtable);
    }

    public static void ShowPunch(GameObject go, Vector3 scale, string callbackName = "", GameObject callbackGO = null, object callbackData = null)
    {
        if (string.IsNullOrEmpty(callbackName))
        {
            iTween.ScaleTo(go, scale, 0.15f);
        }
        else
        {
            if (callbackGO == null)
            {
                callbackGO = go;
            }
            if (callbackData == null)
            {
                callbackData = new object();
            }
            object[] args = new object[] { "scale", scale, "time", 0.15f, "oncomplete", callbackName, "oncompletetarget", callbackGO, "oncompleteparams", callbackData };
            Hashtable hashtable = iTween.Hash(args);
            iTween.ScaleTo(go, hashtable);
        }
    }

    [DebuggerHidden]
    private static IEnumerator ShowPunchRoutine(PunchData callbackData)
    {
        return new <ShowPunchRoutine>c__Iterator26E { callbackData = callbackData, <$>callbackData = callbackData };
    }

    public static void ShowWithPunch(GameObject go, Vector3 startScale, Vector3 punchScale, Vector3 afterPunchScale, string callbackName = "", bool noFade = false, GameObject callbackGO = null, object callbackData = null, DelOnShownWithPunch onShowPunchCallback = null)
    {
        if (!noFade)
        {
            iTween.FadeTo(go, 1f, 0.25f);
        }
        go.transform.localScale = startScale;
        object[] args = new object[] { "scale", punchScale, "time", 0.25f };
        iTween.ScaleTo(go, iTween.Hash(args));
        object[] objArray2 = new object[] { "position", go.transform.position + new Vector3(0.02f, 0.02f, 0.02f), "time", 1.5f };
        iTween.MoveTo(go, iTween.Hash(objArray2));
        PunchData data = new PunchData {
            m_gameObject = go,
            m_scale = afterPunchScale,
            m_callbackName = callbackName,
            m_callbackGameObject = callbackGO,
            m_callbackData = callbackData,
            m_onShowPunchCallback = onShowPunchCallback
        };
        go.GetComponent<MonoBehaviour>().StartCoroutine(ShowPunchRoutine(data));
    }

    [CompilerGenerated]
    private sealed class <DelayedActivation>c__Iterator270 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal bool <$>activate;
        internal GameObject <$>go;
        internal float <$>time;
        internal bool activate;
        internal GameObject go;
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
                    this.go.SetActive(this.activate);
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
    private sealed class <DriftAfterTween>c__Iterator26F : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>delayTime;
        internal Vector3 <$>driftOffset;
        internal GameObject <$>go;
        internal float delayTime;
        internal Vector3 driftOffset;
        internal GameObject go;

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
                    this.$current = new WaitForSeconds(this.delayTime);
                    this.$PC = 1;
                    return true;

                case 1:
                    AnimationUtil.DriftObject(this.go, this.driftOffset);
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
    private sealed class <FadeTexture>c__AnonStorey38C
    {
        internal Color currentColor;
        internal Material logoMaterial;
        internal AnimationUtil.DelOnFade onCompleteCallback;

        internal void <>m__286(object val)
        {
            this.currentColor.a = (float) val;
            this.logoMaterial.SetColor("_Color", this.currentColor);
        }

        internal void <>m__287(object o)
        {
            this.onCompleteCallback();
        }
    }

    [CompilerGenerated]
    private sealed class <ShowPunchRoutine>c__Iterator26E : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AnimationUtil.PunchData <$>callbackData;
        internal AnimationUtil.PunchData callbackData;

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
                    this.$current = new WaitForSeconds(0.25f);
                    this.$PC = 1;
                    return true;

                case 1:
                    AnimationUtil.ShowPunch(this.callbackData.m_gameObject, this.callbackData.m_scale, this.callbackData.m_callbackName, this.callbackData.m_callbackGameObject, this.callbackData.m_callbackData);
                    if (this.callbackData.m_onShowPunchCallback != null)
                    {
                        this.callbackData.m_onShowPunchCallback(this.callbackData.m_callbackData);
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

    public delegate void DelOnFade();

    public delegate void DelOnShownWithPunch(object callbackData);

    private class PunchData
    {
        public object m_callbackData;
        public GameObject m_callbackGameObject;
        public string m_callbackName;
        public GameObject m_gameObject;
        public AnimationUtil.DelOnShownWithPunch m_onShowPunchCallback;
        public Vector3 m_scale;
    }
}

