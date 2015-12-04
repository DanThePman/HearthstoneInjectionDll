using System;
using System.Collections;
using UnityEngine;

public class CollectionDeckTileActor : Actor
{
    public UberText m_countText;
    private UberText m_countTextMesh;
    public GameObject m_frame;
    private bool m_ghosted;
    [CustomEditField(Sections="Ghosting Effect")]
    public DeckTileFrameColorSet m_ghostedColorSet = new DeckTileFrameColorSet();
    [CustomEditField(Sections="Ghosting Effect")]
    public Material m_ghostedFrameMaterial;
    public GameObject m_highlight;
    [CustomEditField(Sections="Ghosting Effect")]
    public MeshRenderer m_manaGem;
    [CustomEditField(Sections="Ghosting Effect")]
    public Material m_manaGemGhostedMaterial;
    [CustomEditField(Sections="Ghosting Effect")]
    public Material m_manaGemNormalMaterial;
    [CustomEditField(Sections="Ghosting Effect")]
    public DeckTileFrameColorSet m_normalColorSet = new DeckTileFrameColorSet();
    private Vector3 m_openSliderLocalPos;
    private Vector3 m_originalSliderLocalPos;
    public Material m_premiumFrameMaterial;
    [CustomEditField(Sections="Ghosting Effect")]
    public MeshRenderer m_slider;
    private bool m_sliderIsOpen;
    public Material m_standardFrameMaterial;
    public GameObject m_uniqueStar;
    private const float SLIDER_ANIM_TIME = 0.35f;

    private void AssignCardCount()
    {
        this.m_countTextMesh = base.m_rootObject.transform.FindChild("CardCountText").GetComponent<UberText>();
    }

    private void AssignSlider()
    {
        this.m_originalSliderLocalPos = this.m_slider.transform.localPosition;
        this.m_openSliderLocalPos = base.m_rootObject.transform.FindChild("OpenSliderPosition").transform.localPosition;
    }

    public override void Awake()
    {
        base.Awake();
        this.AssignSlider();
        this.AssignCardCount();
    }

    private void CloseSlider(bool useSliderAnimations)
    {
        if (this.m_sliderIsOpen)
        {
            this.m_sliderIsOpen = false;
            iTween.StopByName(this.m_slider.gameObject, "position");
            if (useSliderAnimations)
            {
                object[] args = new object[] { "position", this.m_originalSliderLocalPos, "isLocal", true, "time", 0.35f, "easetype", iTween.EaseType.easeOutBounce, "name", "position" };
                Hashtable hashtable = iTween.Hash(args);
                iTween.MoveTo(this.m_slider.gameObject, hashtable);
            }
            else
            {
                this.m_slider.transform.localPosition = this.m_originalSliderLocalPos;
            }
        }
    }

    protected override Material GetPortraitMaterial()
    {
        return base.m_portraitMesh.GetComponent<MeshRenderer>().material;
    }

    private void OpenSlider(bool useSliderAnimations)
    {
        if (!this.m_sliderIsOpen)
        {
            this.m_sliderIsOpen = true;
            iTween.StopByName(this.m_slider.gameObject, "position");
            if (useSliderAnimations)
            {
                object[] args = new object[] { "position", this.m_openSliderLocalPos, "isLocal", true, "time", 0.35f, "easetype", iTween.EaseType.easeOutBounce, "name", "position" };
                Hashtable hashtable = iTween.Hash(args);
                iTween.MoveTo(this.m_slider.gameObject, hashtable);
            }
            else
            {
                this.m_slider.transform.localPosition = this.m_openSliderLocalPos;
            }
        }
    }

    public override void SetCardFlair(CardFlair cardFlair)
    {
        base.SetCardFlair(cardFlair);
        this.UpdateFrameMaterial();
    }

    private void SetDesaturationAmount(Material material, DeckTileFrameColorSet colorSet)
    {
        material.SetColor("_Color", colorSet.m_desatColor);
        material.SetFloat("_Desaturate", colorSet.m_desatAmount);
        material.SetFloat("_Contrast", colorSet.m_desatContrast);
    }

    public void SetGhosted(bool enabled)
    {
        this.m_ghosted = enabled;
    }

    public void UpdateDeckCardProperties(bool cardIsUnique, int numCards, bool useSliderAnimations)
    {
        if (cardIsUnique)
        {
            this.m_uniqueStar.SetActive(base.m_shown);
            this.m_countTextMesh.gameObject.SetActive(false);
        }
        else
        {
            this.m_uniqueStar.SetActive(false);
            this.m_countTextMesh.gameObject.SetActive(base.m_shown);
            this.m_countTextMesh.Text = Convert.ToString(numCards);
        }
        if (cardIsUnique || (numCards > 1))
        {
            this.OpenSlider(useSliderAnimations);
        }
        else
        {
            this.CloseSlider(useSliderAnimations);
        }
    }

    private void UpdateFrameMaterial()
    {
        Material ghostedFrameMaterial = null;
        if (this.m_ghosted)
        {
            ghostedFrameMaterial = this.m_ghostedFrameMaterial;
        }
        else
        {
            CardFlair cardFlair = base.GetCardFlair();
            if (cardFlair != null)
            {
                if (cardFlair.Premium == TAG_PREMIUM.GOLDEN)
                {
                    ghostedFrameMaterial = this.m_premiumFrameMaterial;
                }
                else
                {
                    ghostedFrameMaterial = this.m_standardFrameMaterial;
                }
            }
        }
        if (ghostedFrameMaterial != null)
        {
            this.m_frame.GetComponent<Renderer>().material = ghostedFrameMaterial;
        }
    }

    public void UpdateGhostTileEffect()
    {
        if (this.m_manaGem != null)
        {
            this.UpdateFrameMaterial();
            DeckTileFrameColorSet colorSet = null;
            Material manaGemGhostedMaterial = null;
            if (this.m_ghosted)
            {
                colorSet = this.m_ghostedColorSet;
                manaGemGhostedMaterial = this.m_manaGemGhostedMaterial;
            }
            else
            {
                colorSet = this.m_normalColorSet;
                manaGemGhostedMaterial = this.m_manaGemNormalMaterial;
            }
            this.m_manaGem.material = manaGemGhostedMaterial;
            this.m_countText.TextColor = colorSet.m_countTextColor;
            base.m_costTextMesh.TextColor = colorSet.m_costTextColor;
            this.SetDesaturationAmount(this.GetPortraitMaterial(), colorSet);
            this.SetDesaturationAmount(this.m_uniqueStar.GetComponent<MeshRenderer>().material, colorSet);
        }
    }

    public void UpdateMaterial(Material material)
    {
        if (material != null)
        {
            base.m_portraitMesh.GetComponent<MeshRenderer>().material = material;
        }
    }

    [Serializable]
    public class DeckTileFrameColorSet
    {
        public Color m_costTextColor = Color.white;
        public Color m_countTextColor = new Color(1f, 0.9f, 0f, 1f);
        public float m_desatAmount;
        public Color m_desatColor = Color.white;
        public float m_desatContrast;
        public Color m_sliderColor = new Color(0.62f, 0.62f, 0.62f, 1f);
    }
}

