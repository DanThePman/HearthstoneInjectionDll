using System;
using UnityEngine;

public class BackBehavior : MonoBehaviour
{
    public void Awake()
    {
        PegUIElement component = base.gameObject.GetComponent<PegUIElement>();
        if (component != null)
        {
            component.AddEventListener(UIEventType.RELEASE, e => this.OnRelease());
        }
    }

    public void OnRelease()
    {
        Navigation.GoBack();
    }
}

