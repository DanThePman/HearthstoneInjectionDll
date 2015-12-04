using System;
using UnityEngine;

public interface ITouchListItem
{
    T GetComponent<T>() where T: MonoBehaviour;

    GameObject gameObject { get; }

    bool IsHeader { get; }

    Bounds LocalBounds { get; }

    Transform transform { get; }

    bool Visible { get; set; }
}

