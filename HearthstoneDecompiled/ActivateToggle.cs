using System;
using UnityEngine;

public class ActivateToggle : MonoBehaviour
{
    public GameObject obj;
    private bool onoff;

    private void ToggleActive()
    {
        if (this.onoff)
        {
            this.obj.SetActive(false);
        }
        if (!this.onoff)
        {
            this.obj.SetActive(true);
        }
    }
}

