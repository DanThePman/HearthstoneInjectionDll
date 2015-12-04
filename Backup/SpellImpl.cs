using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class SpellImpl : Spell
{
    protected Actor m_actor;
    protected GameObject m_rootObject;
    protected MeshRenderer m_rootObjectRenderer;

    protected GameObject GetActorObject(string name)
    {
        if (this.m_actor == null)
        {
            return null;
        }
        return SceneUtils.FindChildBySubstring(this.m_actor.gameObject, name);
    }

    protected Material GetMaterial(GameObject go, Material material, bool getSharedMaterial = false, int materialIndex = 0)
    {
        if ((go != null) && (go.GetComponent<Renderer>() != null))
        {
            if ((materialIndex == 0) && !getSharedMaterial)
            {
                return go.GetComponent<Renderer>().material;
            }
            if ((materialIndex == 0) && getSharedMaterial)
            {
                return go.GetComponent<Renderer>().sharedMaterial;
            }
            if ((go.GetComponent<Renderer>().materials.Length > materialIndex) && !getSharedMaterial)
            {
                Material[] materials = go.GetComponent<Renderer>().materials;
                Material material2 = materials[materialIndex];
                go.GetComponent<Renderer>().materials = materials;
                return material2;
            }
            if ((go.GetComponent<Renderer>().materials.Length > materialIndex) && getSharedMaterial)
            {
                Material[] sharedMaterials = go.GetComponent<Renderer>().sharedMaterials;
                Material material3 = sharedMaterials[materialIndex];
                go.GetComponent<Renderer>().sharedMaterials = sharedMaterials;
                return material3;
            }
        }
        return null;
    }

    protected void InitActorVariables()
    {
        this.m_actor = SpellUtils.GetParentActor(this);
        this.m_rootObject = SpellUtils.GetParentRootObject(this);
        this.m_rootObjectRenderer = SpellUtils.GetParentRootObjectMesh(this);
    }

    protected void PlayAnimation(GameObject go, string animName, PlayMode playMode, float crossFade = 0f)
    {
        if (go != null)
        {
            if (crossFade <= Mathf.Epsilon)
            {
                go.GetComponent<Animation>().Play(animName, playMode);
            }
            else
            {
                go.GetComponent<Animation>().CrossFade(animName, crossFade, playMode);
            }
        }
    }

    protected void PlayParticles(GameObject go, bool includeChildren)
    {
        if (go != null)
        {
            go.GetComponent<ParticleSystem>().Play(includeChildren);
        }
    }

    protected void SetActorVisibility(bool visible, bool ignoreSpells)
    {
        if (this.m_actor != null)
        {
            if (visible)
            {
                this.m_actor.Show(ignoreSpells);
            }
            else
            {
                this.m_actor.Hide(ignoreSpells);
            }
        }
    }

    protected void SetAnimationSpeed(GameObject go, string animName, float speed)
    {
        if (go != null)
        {
            go.GetComponent<Animation>()[animName].speed = speed;
        }
    }

    protected void SetAnimationTime(GameObject go, string animName, float time)
    {
        if (go != null)
        {
            go.GetComponent<Animation>()[animName].time = time;
        }
    }

    protected void SetMaterialColor(GameObject go, Material material, string colorName, Color color, int materialIndex = 0)
    {
        if (colorName == string.Empty)
        {
            colorName = "_Color";
        }
        if (material != null)
        {
            material.SetColor(colorName, color);
        }
        else if (((go != null) && (go.GetComponent<Renderer>() != null)) && (go.GetComponent<Renderer>().material != null))
        {
            if (materialIndex == 0)
            {
                go.GetComponent<Renderer>().material.SetColor(colorName, color);
            }
            else if (go.GetComponent<Renderer>().materials.Length > materialIndex)
            {
                Material[] materials = go.GetComponent<Renderer>().materials;
                materials[materialIndex].SetColor(colorName, color);
                go.GetComponent<Renderer>().materials = materials;
            }
        }
    }

    protected void SetVisibility(GameObject go, bool visible)
    {
        go.GetComponent<Renderer>().enabled = visible;
    }

    protected void SetVisibilityRecursive(GameObject go, bool visible)
    {
        if (go != null)
        {
            Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
            if (componentsInChildren != null)
            {
                foreach (Renderer renderer in componentsInChildren)
                {
                    renderer.enabled = visible;
                }
            }
        }
    }
}

