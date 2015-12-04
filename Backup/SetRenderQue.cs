using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SetRenderQue : MonoBehaviour
{
    public bool includeChildren;
    public int queue = 1;
    public int[] queues;

    private void Start()
    {
        if (this.includeChildren)
        {
            foreach (Renderer renderer in base.GetComponentsInChildren<Renderer>())
            {
                if (renderer != null)
                {
                    foreach (Material material in renderer.materials)
                    {
                        if (material != null)
                        {
                            material.renderQueue += this.queue;
                        }
                    }
                }
            }
        }
        else
        {
            if (base.GetComponent<Renderer>() == null)
            {
                return;
            }
            Material material1 = base.GetComponent<Renderer>().material;
            material1.renderQueue += this.queue;
        }
        if ((this.queues != null) && (base.GetComponent<Renderer>() != null))
        {
            Material[] materials = base.GetComponent<Renderer>().materials;
            if (materials != null)
            {
                int length = materials.Length;
                for (int i = 0; (i < this.queues.Length) && (i < length); i++)
                {
                    Material material2 = base.GetComponent<Renderer>().materials[i];
                    if (material2 != null)
                    {
                        if (this.queues[i] < 0)
                        {
                            Debug.LogWarning(string.Format("WARNING: Using negative renderQueue for {0}'s {1} (renderQueue = {2})", base.transform.root.name, base.gameObject.name, this.queues[i]));
                        }
                        material2.renderQueue += this.queues[i];
                    }
                }
            }
        }
    }
}

