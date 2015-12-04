using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CanvasAnchors
{
    public Transform m_BottomLeft;
    public Transform m_BottomRight;
    public Transform m_Center;
    public Transform m_TopLeft;
    public Transform m_TopRight;

    public Transform GetAnchor(CanvasAnchor type)
    {
        if (type != CanvasAnchor.CENTER)
        {
            if (type == CanvasAnchor.BOTTOM_LEFT)
            {
                return this.m_BottomLeft;
            }
            if (type == CanvasAnchor.BOTTOM_RIGHT)
            {
                return this.m_BottomRight;
            }
            if (type == CanvasAnchor.TOP_LEFT)
            {
                return this.m_TopLeft;
            }
            if (type == CanvasAnchor.TOP_RIGHT)
            {
                return this.m_TopRight;
            }
        }
        return this.m_Center;
    }

    public void WillReset()
    {
        IEnumerator enumerator = this.m_Center.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                UnityEngine.Object.Destroy(current.gameObject);
            }
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable == null)
            {
            }
            disposable.Dispose();
        }
        IEnumerator enumerator2 = this.m_BottomLeft.GetEnumerator();
        try
        {
            while (enumerator2.MoveNext())
            {
                Transform transform2 = (Transform) enumerator2.Current;
                UnityEngine.Object.Destroy(transform2.gameObject);
            }
        }
        finally
        {
            IDisposable disposable2 = enumerator2 as IDisposable;
            if (disposable2 == null)
            {
            }
            disposable2.Dispose();
        }
        IEnumerator enumerator3 = this.m_BottomRight.GetEnumerator();
        try
        {
            while (enumerator3.MoveNext())
            {
                Transform transform3 = (Transform) enumerator3.Current;
                UnityEngine.Object.Destroy(transform3.gameObject);
            }
        }
        finally
        {
            IDisposable disposable3 = enumerator3 as IDisposable;
            if (disposable3 == null)
            {
            }
            disposable3.Dispose();
        }
        IEnumerator enumerator4 = this.m_TopLeft.GetEnumerator();
        try
        {
            while (enumerator4.MoveNext())
            {
                Transform transform4 = (Transform) enumerator4.Current;
                UnityEngine.Object.Destroy(transform4.gameObject);
            }
        }
        finally
        {
            IDisposable disposable4 = enumerator4 as IDisposable;
            if (disposable4 == null)
            {
            }
            disposable4.Dispose();
        }
        IEnumerator enumerator5 = this.m_TopRight.GetEnumerator();
        try
        {
            while (enumerator5.MoveNext())
            {
                Transform transform5 = (Transform) enumerator5.Current;
                UnityEngine.Object.Destroy(transform5.gameObject);
            }
        }
        finally
        {
            IDisposable disposable5 = enumerator5 as IDisposable;
            if (disposable5 == null)
            {
            }
            disposable5.Dispose();
        }
    }
}

