using System;
using UnityEngine;

public class GhostCard : MonoBehaviour
{
    public Actor m_Actor;
    private GameObject m_AttackMesh;
    private int m_CardFrontIdx;
    private GameObject m_CardMesh;
    public Vector3 m_CardOffset = Vector3.zero;
    private GameObject m_DescriptionMesh;
    private GameObject m_DescriptionTrimMesh;
    public GameObject m_EffectRoot;
    private GameObject m_EliteMesh;
    public Material m_GhostMaterial;
    public Material m_GhostMaterialMod2x;
    public Material m_GhostMaterialTransparent;
    public GameObject m_GlowPlane;
    public GameObject m_GlowPlaneElite;
    private GameObject m_HealthMesh;
    private bool m_Init;
    private GameObject m_ManaCostMesh;
    private GameObject m_NameMesh;
    private Material m_OrgMat_Attack;
    private Material m_OrgMat_CardFront;
    private Material m_OrgMat_Description;
    private Material m_OrgMat_Description2;
    private Material m_OrgMat_DescriptionTrim;
    private Material m_OrgMat_Elite;
    private Material m_OrgMat_Health;
    private Material m_OrgMat_ManaCost;
    private Material m_OrgMat_Name;
    private Material m_OrgMat_PortraitFrame;
    private Material m_OrgMat_PremiumRibbon;
    private Material m_OrgMat_RacePlate;
    private Material m_OrgMat_RarityFrame;
    private int m_PortraitFrameIdx;
    private GameObject m_PortraitMesh;
    private int m_PremiumRibbonIdx = -1;
    private RenderToTexture m_R2T_BaseCard;
    public RenderToTexture m_R2T_EffectGhost;
    private GameObject m_RacePlateMesh;
    private GameObject m_RarityFrameMesh;

    private void ApplyGhostMaterials()
    {
        this.ApplyMaterialByIdx(this.m_CardMesh, this.m_GhostMaterial, this.m_CardFrontIdx);
        this.ApplyMaterialByIdx(this.m_CardMesh, this.m_GhostMaterial, this.m_PremiumRibbonIdx);
        this.ApplyMaterialByIdx(this.m_PortraitMesh, this.m_GhostMaterial, this.m_PortraitFrameIdx);
        this.ApplyMaterialByIdx(this.m_DescriptionMesh, this.m_GhostMaterialMod2x, 0);
        this.ApplyMaterialByIdx(this.m_DescriptionMesh, this.m_GhostMaterial, 1);
        this.ApplyMaterial(this.m_NameMesh, this.m_GhostMaterial);
        this.ApplyMaterial(this.m_ManaCostMesh, this.m_GhostMaterial);
        this.ApplyMaterial(this.m_AttackMesh, this.m_GhostMaterial);
        this.ApplyMaterial(this.m_HealthMesh, this.m_GhostMaterial);
        this.ApplyMaterial(this.m_RacePlateMesh, this.m_GhostMaterial);
        this.ApplyMaterial(this.m_RarityFrameMesh, this.m_GhostMaterial);
        if (this.m_GhostMaterialTransparent != null)
        {
            this.ApplyMaterial(this.m_DescriptionTrimMesh, this.m_GhostMaterialTransparent);
        }
        this.ApplyMaterial(this.m_EliteMesh, this.m_GhostMaterial);
    }

    private void ApplyMaterial(GameObject go, Material mat)
    {
        if (go != null)
        {
            Texture mainTexture = go.GetComponent<Renderer>().material.mainTexture;
            go.GetComponent<Renderer>().material = mat;
            go.GetComponent<Renderer>().material.mainTexture = mainTexture;
        }
    }

    private void ApplyMaterialByIdx(GameObject go, Material mat, int idx)
    {
        if (((go != null) && (mat != null)) && (idx >= 0))
        {
            Material[] materials = go.GetComponent<Renderer>().materials;
            if (idx < materials.Length)
            {
                Texture mainTexture = go.GetComponent<Renderer>().materials[idx].mainTexture;
                Texture texture = null;
                Material material = go.GetComponent<Renderer>().materials[idx];
                if (material != null)
                {
                    if (material.HasProperty("_SecondTex"))
                    {
                        texture = material.GetTexture("_SecondTex");
                    }
                    Color clear = Color.clear;
                    bool flag = material.HasProperty("_SecondTint");
                    if (flag)
                    {
                        clear = material.GetColor("_SecondTint");
                    }
                    materials[idx] = mat;
                    go.GetComponent<Renderer>().materials = materials;
                    go.GetComponent<Renderer>().materials[idx].mainTexture = mainTexture;
                    if (texture != null)
                    {
                        go.GetComponent<Renderer>().materials[idx].SetTexture("_SecondTex", texture);
                    }
                    if (flag)
                    {
                        go.GetComponent<Renderer>().materials[idx].SetColor("_SecondTint", clear);
                    }
                }
            }
        }
    }

    private void ApplySharedMaterialByIdx(GameObject go, Material mat, int idx)
    {
        if (((go != null) && (mat != null)) && (idx >= 0))
        {
            Material[] sharedMaterials = go.GetComponent<Renderer>().sharedMaterials;
            if (idx < sharedMaterials.Length)
            {
                Texture mainTexture = go.GetComponent<Renderer>().sharedMaterials[idx].mainTexture;
                sharedMaterials[idx] = mat;
                go.GetComponent<Renderer>().sharedMaterials = sharedMaterials;
                go.GetComponent<Renderer>().sharedMaterials[idx].mainTexture = mainTexture;
            }
        }
    }

    private void Disable()
    {
        this.RestoreOrgMaterials();
        if (this.m_R2T_BaseCard != null)
        {
            this.m_R2T_BaseCard.enabled = false;
        }
        if (this.m_R2T_EffectGhost != null)
        {
            this.m_R2T_EffectGhost.enabled = false;
        }
        if (this.m_GlowPlane != null)
        {
            this.m_GlowPlane.GetComponent<Renderer>().enabled = false;
        }
        if (this.m_GlowPlaneElite != null)
        {
            this.m_GlowPlaneElite.GetComponent<Renderer>().enabled = false;
        }
        if (this.m_EffectRoot != null)
        {
            ParticleSystem componentInChildren = this.m_EffectRoot.GetComponentInChildren<ParticleSystem>();
            if (componentInChildren != null)
            {
                componentInChildren.Stop();
                componentInChildren.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void DisableGhost()
    {
        this.Disable();
        base.enabled = false;
    }

    private void Init(bool forceRender)
    {
        if (!this.m_Init || forceRender)
        {
            if (this.m_Actor == null)
            {
                this.m_Actor = SceneUtils.FindComponentInThisOrParents<Actor>(base.gameObject);
                if (this.m_Actor == null)
                {
                    Debug.LogError(string.Format("{0} Ghost card effect failed to find Actor!", base.transform.root.name));
                    base.enabled = false;
                    return;
                }
            }
            this.m_CardMesh = this.m_Actor.m_cardMesh;
            this.m_CardFrontIdx = this.m_Actor.m_cardFrontMatIdx;
            this.m_PremiumRibbonIdx = this.m_Actor.m_premiumRibbon;
            this.m_PortraitMesh = this.m_Actor.m_portraitMesh;
            this.m_PortraitFrameIdx = this.m_Actor.m_portraitFrameMatIdx;
            this.m_NameMesh = this.m_Actor.m_nameBannerMesh;
            this.m_DescriptionMesh = this.m_Actor.m_descriptionMesh;
            this.m_DescriptionTrimMesh = this.m_Actor.m_descriptionTrimMesh;
            this.m_RarityFrameMesh = this.m_Actor.m_rarityFrameMesh;
            if (this.m_Actor.m_attackObject != null)
            {
                Renderer component = this.m_Actor.m_attackObject.GetComponent<Renderer>();
                if (component != null)
                {
                    this.m_AttackMesh = component.gameObject;
                }
                if (this.m_AttackMesh == null)
                {
                    foreach (Renderer renderer2 in this.m_Actor.m_attackObject.GetComponentsInChildren<Renderer>())
                    {
                        if (renderer2.GetComponent<UberText>() == null)
                        {
                            this.m_AttackMesh = renderer2.gameObject;
                        }
                    }
                }
            }
            if (this.m_Actor.m_healthObject != null)
            {
                Renderer renderer3 = this.m_Actor.m_healthObject.GetComponent<Renderer>();
                if (renderer3 != null)
                {
                    this.m_HealthMesh = renderer3.gameObject;
                }
                if (this.m_HealthMesh == null)
                {
                    foreach (Renderer renderer4 in this.m_Actor.m_healthObject.GetComponentsInChildren<Renderer>())
                    {
                        if (renderer4.GetComponent<UberText>() == null)
                        {
                            this.m_HealthMesh = renderer4.gameObject;
                        }
                    }
                }
            }
            this.m_ManaCostMesh = this.m_Actor.m_manaObject;
            this.m_RacePlateMesh = this.m_Actor.m_racePlateObject;
            this.m_EliteMesh = this.m_Actor.m_eliteObject;
            this.StoreOrgMaterials();
            this.m_R2T_BaseCard = base.GetComponent<RenderToTexture>();
            this.m_R2T_BaseCard.m_ObjectToRender = this.m_Actor.GetRootObject();
            if ((this.m_R2T_BaseCard.m_Material != null) && this.m_R2T_BaseCard.m_Material.HasProperty("_Seed"))
            {
                this.m_R2T_BaseCard.m_Material.SetFloat("_Seed", UnityEngine.Random.Range((float) 0f, (float) 1f));
            }
            this.m_Init = true;
        }
    }

    private void OnDestroy()
    {
        if (this.m_EffectRoot != null)
        {
            ParticleSystem componentInChildren = this.m_EffectRoot.GetComponentInChildren<ParticleSystem>();
            if (componentInChildren != null)
            {
                componentInChildren.Stop();
            }
        }
    }

    private void OnDisable()
    {
        this.Disable();
    }

    private void RenderGhost()
    {
        this.RenderGhost(false);
    }

    private void RenderGhost(bool forceRender)
    {
        this.Init(forceRender);
        this.m_R2T_BaseCard.enabled = true;
        if (this.m_R2T_EffectGhost != null)
        {
            this.m_R2T_EffectGhost.enabled = true;
        }
        this.m_R2T_BaseCard.m_ObjectToRender = this.m_Actor.GetRootObject();
        this.m_Actor.GetRootObject().transform.localPosition = this.m_CardOffset;
        this.m_Actor.ShowAllText();
        this.ApplyGhostMaterials();
        this.m_R2T_BaseCard.Render();
        Material renderMaterial = this.m_R2T_BaseCard.GetRenderMaterial();
        if (this.m_GlowPlane != null)
        {
            if (!this.m_Actor.IsElite())
            {
                this.m_GlowPlane.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                this.m_GlowPlane.GetComponent<Renderer>().enabled = false;
            }
        }
        if (this.m_GlowPlaneElite != null)
        {
            if (this.m_Actor.IsElite())
            {
                this.m_GlowPlaneElite.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                this.m_GlowPlaneElite.GetComponent<Renderer>().enabled = false;
            }
        }
        if (this.m_EffectRoot != null)
        {
            this.m_EffectRoot.transform.parent = null;
            this.m_EffectRoot.transform.position = new Vector3(-500f, -500f, -500f);
            this.m_EffectRoot.transform.localScale = Vector3.one;
            this.m_R2T_EffectGhost.enabled = true;
            RenderTexture texture = this.m_R2T_EffectGhost.RenderNow();
            renderMaterial.SetTexture("_FxTex", texture);
            if (this.m_GlowPlane != null)
            {
                this.m_GlowPlane.GetComponent<Renderer>().material.SetTexture("_FxTex", texture);
            }
            if (this.m_GlowPlaneElite != null)
            {
                this.m_GlowPlaneElite.GetComponent<Renderer>().material.SetTexture("_FxTex", texture);
            }
            ParticleSystem componentInChildren = this.m_EffectRoot.GetComponentInChildren<ParticleSystem>();
            if (componentInChildren != null)
            {
                componentInChildren.Play();
            }
        }
    }

    public void RenderGhostCard()
    {
        this.RenderGhostCard(false);
    }

    public void RenderGhostCard(bool forceRender)
    {
        this.RenderGhost(forceRender);
    }

    public void Reset()
    {
        this.m_Init = false;
    }

    private void RestoreOrgMaterials()
    {
        this.ApplyMaterialByIdx(this.m_CardMesh, this.m_OrgMat_CardFront, this.m_CardFrontIdx);
        this.ApplyMaterialByIdx(this.m_CardMesh, this.m_OrgMat_PremiumRibbon, this.m_PremiumRibbonIdx);
        this.ApplyMaterialByIdx(this.m_PortraitMesh, this.m_OrgMat_PortraitFrame, this.m_PortraitFrameIdx);
        this.ApplyMaterialByIdx(this.m_DescriptionMesh, this.m_OrgMat_Description, 0);
        this.ApplyMaterialByIdx(this.m_DescriptionMesh, this.m_OrgMat_Description2, 1);
        this.ApplyMaterial(this.m_NameMesh, this.m_OrgMat_Name);
        this.ApplyMaterial(this.m_ManaCostMesh, this.m_OrgMat_ManaCost);
        this.ApplyMaterial(this.m_AttackMesh, this.m_OrgMat_Attack);
        this.ApplyMaterial(this.m_HealthMesh, this.m_OrgMat_Health);
        this.ApplyMaterial(this.m_RacePlateMesh, this.m_OrgMat_RacePlate);
        this.ApplyMaterial(this.m_RarityFrameMesh, this.m_OrgMat_RarityFrame);
        this.ApplyMaterial(this.m_DescriptionTrimMesh, this.m_OrgMat_DescriptionTrim);
        this.ApplyMaterial(this.m_EliteMesh, this.m_OrgMat_Elite);
    }

    private void StoreOrgMaterials()
    {
        if (this.m_CardMesh != null)
        {
            if (this.m_CardFrontIdx > -1)
            {
                this.m_OrgMat_CardFront = this.m_CardMesh.GetComponent<Renderer>().materials[this.m_CardFrontIdx];
            }
            if (this.m_PremiumRibbonIdx > -1)
            {
                this.m_OrgMat_PremiumRibbon = this.m_CardMesh.GetComponent<Renderer>().materials[this.m_PremiumRibbonIdx];
            }
        }
        if ((this.m_PortraitMesh != null) && (this.m_PortraitFrameIdx > -1))
        {
            this.m_OrgMat_PortraitFrame = this.m_PortraitMesh.GetComponent<Renderer>().sharedMaterials[this.m_PortraitFrameIdx];
        }
        if (this.m_NameMesh != null)
        {
            this.m_OrgMat_Name = this.m_NameMesh.GetComponent<Renderer>().material;
        }
        if (this.m_ManaCostMesh != null)
        {
            this.m_OrgMat_ManaCost = this.m_ManaCostMesh.GetComponent<Renderer>().material;
        }
        if (this.m_AttackMesh != null)
        {
            this.m_OrgMat_Attack = this.m_AttackMesh.GetComponent<Renderer>().material;
        }
        if (this.m_HealthMesh != null)
        {
            this.m_OrgMat_Health = this.m_HealthMesh.GetComponent<Renderer>().material;
        }
        if (this.m_RacePlateMesh != null)
        {
            this.m_OrgMat_RacePlate = this.m_RacePlateMesh.GetComponent<Renderer>().material;
        }
        if (this.m_RarityFrameMesh != null)
        {
            this.m_OrgMat_RarityFrame = this.m_RarityFrameMesh.GetComponent<Renderer>().material;
        }
        if (this.m_DescriptionMesh != null)
        {
            Material[] materials = this.m_DescriptionMesh.GetComponent<Renderer>().materials;
            if (materials.Length > 1)
            {
                this.m_OrgMat_Description = materials[0];
                this.m_OrgMat_Description2 = materials[1];
            }
            else
            {
                this.m_OrgMat_Description = this.m_DescriptionMesh.GetComponent<Renderer>().material;
            }
        }
        if (this.m_DescriptionTrimMesh != null)
        {
            this.m_OrgMat_DescriptionTrim = this.m_DescriptionTrimMesh.GetComponent<Renderer>().material;
        }
        if (this.m_EliteMesh != null)
        {
            this.m_OrgMat_Elite = this.m_EliteMesh.GetComponent<Renderer>().material;
        }
    }
}

