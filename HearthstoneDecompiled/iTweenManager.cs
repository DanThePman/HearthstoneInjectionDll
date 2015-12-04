using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class iTweenManager : MonoBehaviour
{
    private iTweenCollection m_tweenCollection = new iTweenCollection();
    private static iTweenManager s_instance;
    private static bool s_quitting;

    public static void Add(iTween tween)
    {
        iTweenManager manager = Get();
        if (manager != null)
        {
            manager.AddImpl(tween);
        }
    }

    private void AddImpl(iTween tween)
    {
        this.m_tweenCollection.Add(tween);
        tween.Awake();
    }

    public void FixedUpdate()
    {
        iTween tween;
        iTweenIterator iterator = this.m_tweenCollection.GetIterator();
        while ((tween = iterator.GetNext()) != null)
        {
            tween.Upkeep();
            tween.FixedUpdate();
        }
        this.m_tweenCollection.CleanUp();
    }

    public static void ForEach(TweenOperation op, GameObject go = null, string name = null, string type = null, bool includeChildren = false)
    {
        iTweenManager manager = Get();
        if (manager != null)
        {
            manager.ForEachImpl(op, go, name, type, includeChildren);
        }
    }

    public static void ForEachByGameObject(TweenOperation op, GameObject go)
    {
        ForEach(op, go, null, null, false);
    }

    public static void ForEachByName(TweenOperation op, string name)
    {
        ForEach(op, null, name, null, false);
    }

    public static void ForEachByType(TweenOperation op, string type)
    {
        ForEach(op, null, null, type, false);
    }

    private void ForEachImpl(TweenOperation op, GameObject go = null, string name = null, string type = null, bool includeChildren = false)
    {
        iTween tween;
        iTweenIterator iterator = this.m_tweenCollection.GetIterator();
        while ((tween = iterator.GetNext()) != null)
        {
            if ((((go == null) || (tween.gameObject == go)) && ((name == null) || name.Equals(tween._name))) && ((type == null) || (tween.type + tween.method).Substring(0, type.Length).ToLower().Equals(type.ToLower())))
            {
                op(tween);
                if ((go != null) && includeChildren)
                {
                    IEnumerator enumerator = go.transform.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            Transform current = (Transform) enumerator.Current;
                            ForEach(op, current.gameObject, name, type, true);
                        }
                        continue;
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable == null)
                        {
                        }
                        disposable.Dispose();
                    }
                }
            }
        }
    }

    public static void ForEachInverted(TweenOperation op, GameObject go, string name, string type, bool includeChildren = false)
    {
        iTweenManager manager = Get();
        if (manager != null)
        {
            manager.ForEachInvertedImpl(op, go, name, type, includeChildren);
        }
    }

    private void ForEachInvertedImpl(TweenOperation op, GameObject go, string name, string type, bool includeChildren = false)
    {
        iTween tween;
        iTweenIterator iterator = this.m_tweenCollection.GetIterator();
        while ((tween = iterator.GetNext()) != null)
        {
            if ((((go == null) || (tween.gameObject == go)) && ((name == null) || !name.Equals(tween._name))) && ((type == null) || !(tween.type + tween.method).Substring(0, type.Length).ToLower().Equals(type.ToLower())))
            {
                op(tween);
                if ((go != null) && includeChildren)
                {
                    IEnumerator enumerator = go.transform.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            Transform current = (Transform) enumerator.Current;
                            ForEachInverted(op, current.gameObject, name, type, true);
                        }
                        continue;
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable == null)
                        {
                        }
                        disposable.Dispose();
                    }
                }
            }
        }
    }

    public static iTweenManager Get()
    {
        if (s_quitting)
        {
            return null;
        }
        if (s_instance == null)
        {
            s_instance = new GameObject { name = "iTweenManager" }.AddComponent<iTweenManager>();
        }
        return s_instance;
    }

    public static iTweenIterator GetIterator()
    {
        iTweenManager manager = Get();
        if (manager == null)
        {
            return new iTweenIterator(null);
        }
        return manager.GetIteratorImpl();
    }

    private iTweenIterator GetIteratorImpl()
    {
        return this.m_tweenCollection.GetIterator();
    }

    public static int GetTweenCount()
    {
        iTweenManager manager = Get();
        if (manager == null)
        {
            return 0;
        }
        return manager.GetTweenCountImpl();
    }

    private int GetTweenCountImpl()
    {
        return this.m_tweenCollection.Count;
    }

    public iTween GetTweenForObject(GameObject obj)
    {
        iTween tween;
        iTweenIterator iterator = this.m_tweenCollection.GetIterator();
        while ((tween = iterator.GetNext()) != null)
        {
            if (tween.gameObject == obj)
            {
                return tween;
            }
        }
        return null;
    }

    public static iTween[] GetTweensForObject(GameObject obj)
    {
        iTweenManager manager = Get();
        if (manager != null)
        {
            return manager.GetTweensForObjectImpl(obj);
        }
        return new iTween[0];
    }

    private iTween[] GetTweensForObjectImpl(GameObject obj)
    {
        iTween tween;
        List<iTween> list = new List<iTween>();
        iTweenIterator iterator = this.m_tweenCollection.GetIterator();
        while ((tween = iterator.GetNext()) != null)
        {
            if (tween.gameObject == obj)
            {
                list.Add(tween);
            }
        }
        return list.ToArray();
    }

    public void LateUpdate()
    {
        iTween tween;
        iTweenIterator iterator = this.m_tweenCollection.GetIterator();
        while ((tween = iterator.GetNext()) != null)
        {
            tween.Upkeep();
            tween.LateUpdate();
        }
        this.m_tweenCollection.CleanUp();
    }

    public void OnApplicationQuit()
    {
        s_instance = null;
        s_quitting = true;
        UnityEngine.Object.Destroy(base.gameObject);
    }

    public void OnDestroy()
    {
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    public static void Remove(iTween tween)
    {
        iTweenManager manager = Get();
        if (manager != null)
        {
            manager.RemoveImpl(tween);
        }
    }

    private void RemoveImpl(iTween tween)
    {
        this.m_tweenCollection.Remove(tween);
        tween.destroyed = true;
    }

    public void Update()
    {
        iTween tween;
        iTweenIterator iterator = this.m_tweenCollection.GetIterator();
        while ((tween = iterator.GetNext()) != null)
        {
            tween.Upkeep();
            tween.Update();
        }
        this.m_tweenCollection.CleanUp();
    }

    private class iTweenEntry
    {
        private Hashtable args;
        private GameObject gameObject;
        private iTween iTween;
    }

    public delegate void TweenOperation(iTween tween);
}

