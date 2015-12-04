using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HeroPickerButton : PegUIElement
{
    private static readonly Color BASIC_SET_COLOR_IN_PROGRESS = new Color(0.97f, 0.82f, 0.22f);
    public List<Material> CLASS_MATERIALS = new List<Material>();
    public HeroPickerButtonBones m_bones;
    public GameObject m_buttonFrame;
    private CardFlair m_cardFlair;
    public UberText m_classLabel;
    private FullDef m_fullDef;
    public TAG_CLASS m_heroClass;
    public GameObject m_heroClassIcon;
    private HighlightState m_highlightState;
    private bool m_isSelected;
    public GameObject m_labelGradient;
    private bool m_locked;
    private long m_preconDeckID;
    private float? m_seed;
    public UberText m_setProgressLabel;
    private UnlockedCallback m_unlockedCallback;

    public void Activate(bool enable)
    {
        base.SetEnabled(enable);
    }

    public CardFlair GetCardFlair()
    {
        return this.m_cardFlair;
    }

    private Material GetClassIconMaterial(TAG_CLASS classTag)
    {
        int num = 0;
        switch (classTag)
        {
            case TAG_CLASS.INVALID:
                num = 9;
                break;

            case TAG_CLASS.DRUID:
                num = 5;
                break;

            case TAG_CLASS.HUNTER:
                num = 4;
                break;

            case TAG_CLASS.MAGE:
                num = 7;
                break;

            case TAG_CLASS.PALADIN:
                num = 3;
                break;

            case TAG_CLASS.PRIEST:
                num = 8;
                break;

            case TAG_CLASS.ROGUE:
                num = 2;
                break;

            case TAG_CLASS.SHAMAN:
                num = 1;
                break;

            case TAG_CLASS.WARLOCK:
                num = 6;
                break;

            case TAG_CLASS.WARRIOR:
                num = 0;
                break;
        }
        return this.CLASS_MATERIALS[num];
    }

    public FullDef GetFullDef()
    {
        return this.m_fullDef;
    }

    public long GetPreconDeckID()
    {
        return this.m_preconDeckID;
    }

    public bool IsLocked()
    {
        return this.m_locked;
    }

    public bool IsSelected()
    {
        return this.m_isSelected;
    }

    public void Lock()
    {
        base.transform.parent.transform.localEulerAngles = new Vector3(0f, 180f, 180f);
        this.m_locked = true;
    }

    public void Lower()
    {
        float num;
        if (UniversalInputManager.UsePhoneUI == null)
        {
            this.Activate(false);
        }
        if (this.m_locked)
        {
            num = 0.7f;
        }
        else
        {
            num = -0.7f;
        }
        object[] args = new object[] { "position", new Vector3(base.GetOriginalLocalPosition().x, base.GetOriginalLocalPosition().y + num, base.GetOriginalLocalPosition().z), "time", 0.1f, "easeType", iTween.EaseType.linear, "isLocal", true };
        Hashtable hashtable = iTween.Hash(args);
        iTween.MoveTo(base.gameObject, hashtable);
    }

    protected override void OnRelease()
    {
        this.Lower();
    }

    public void Raise()
    {
        if (!this.m_isSelected)
        {
            this.Activate(true);
            object[] args = new object[] { "position", new Vector3(base.GetOriginalLocalPosition().x, base.GetOriginalLocalPosition().y, base.GetOriginalLocalPosition().z), "time", 0.1f, "easeType", iTween.EaseType.linear, "isLocal", true };
            Hashtable hashtable = iTween.Hash(args);
            iTween.MoveTo(base.gameObject, hashtable);
        }
    }

    public void SetBasicSetProgress(TAG_CLASS classTag)
    {
        int basicCardsIOwn = CollectionManager.Get().GetBasicCardsIOwn(classTag);
        int num2 = 20;
        if (basicCardsIOwn == num2)
        {
            this.m_classLabel.transform.position = this.m_bones.m_classLabelOneLine.position;
            this.m_labelGradient.transform.parent = this.m_bones.m_gradientOneLine;
            this.m_labelGradient.transform.localPosition = Vector3.zero;
            this.m_labelGradient.transform.localScale = Vector3.one;
            this.m_setProgressLabel.gameObject.SetActive(false);
        }
        else
        {
            this.m_classLabel.transform.position = this.m_bones.m_classLabelTwoLine.position;
            this.m_labelGradient.transform.parent = this.m_bones.m_gradientTwoLine;
            object[] args = new object[] { basicCardsIOwn, num2 };
            this.m_setProgressLabel.Text = GameStrings.Format((UniversalInputManager.UsePhoneUI == null) ? "GLUE_BASIC_SET_PROGRESS" : "GLUE_BASIC_SET_PROGRESS_PHONE", args);
            this.m_labelGradient.transform.localPosition = Vector3.zero;
            this.m_labelGradient.transform.localScale = Vector3.one;
            this.m_setProgressLabel.gameObject.SetActive(true);
            this.m_setProgressLabel.TextColor = BASIC_SET_COLOR_IN_PROGRESS;
        }
    }

    public void SetCardFlair(CardFlair cardFlair)
    {
        this.m_cardFlair = cardFlair;
        this.UpdatePortrait();
    }

    public void SetClassIcon(Material mat)
    {
        this.m_heroClassIcon.GetComponent<Renderer>().material = mat;
        this.m_heroClassIcon.GetComponent<Renderer>().material.renderQueue = 0xbbf;
    }

    public void SetClassname(string s)
    {
        this.m_classLabel.Text = s;
    }

    public void SetFullDef(FullDef def)
    {
        this.m_fullDef = def;
        this.UpdatePortrait();
    }

    public void SetHighlightState(ActorStateType stateType)
    {
        if (this.m_highlightState == null)
        {
            this.m_highlightState = base.gameObject.transform.parent.GetComponentInChildren<HighlightState>();
        }
        if (this.m_highlightState != null)
        {
            this.m_highlightState.ChangeState(stateType);
        }
    }

    public void SetPreconDeckID(long preconDeckID)
    {
        this.m_preconDeckID = preconDeckID;
    }

    public void SetProgress(int acknowledgedProgress, int currProgress, int maxProgress)
    {
        this.SetProgress(acknowledgedProgress, currProgress, maxProgress, true);
    }

    public void SetProgress(int acknowledgedProgress, int currProgress, int maxProgress, bool shouldAnimate)
    {
        bool flag = acknowledgedProgress == currProgress;
        if (currProgress == maxProgress)
        {
            this.Unlock(!flag && shouldAnimate);
        }
    }

    public void SetSelected(bool isSelected)
    {
        this.m_isSelected = isSelected;
        if (isSelected)
        {
            this.Lower();
        }
        else
        {
            this.Raise();
        }
    }

    public void SetUnlockedCallback(UnlockedCallback unlockedCallback)
    {
        this.m_unlockedCallback = unlockedCallback;
    }

    private void Unlock(bool animate)
    {
        base.transform.parent.localEulerAngles = new Vector3(0f, 180f, 0f);
        this.UnlockAfterAnimate();
    }

    private void UnlockAfterAnimate()
    {
        this.m_locked = false;
        if (this.m_unlockedCallback != null)
        {
            this.m_unlockedCallback(this);
        }
    }

    public void UpdateDisplay(FullDef def, CardFlair cardFlair)
    {
        this.m_heroClass = def.GetEntityDef().GetClass();
        this.SetFullDef(def);
        this.SetClassname(GameStrings.GetClassName(this.m_heroClass));
        this.SetClassIcon(this.GetClassIconMaterial(this.m_heroClass));
        this.SetBasicSetProgress(this.m_heroClass);
        this.SetCardFlair(cardFlair);
    }

    private void UpdatePortrait()
    {
        if ((this.m_cardFlair != null) && (this.m_fullDef != null))
        {
            CardDef cardDef = this.m_fullDef.GetCardDef();
            if (cardDef != null)
            {
                Material deckPickerPortrait = cardDef.GetDeckPickerPortrait();
                if (deckPickerPortrait != null)
                {
                    DeckPickerHero component = base.GetComponent<DeckPickerHero>();
                    Material premiumPortraitMaterial = cardDef.GetPremiumPortraitMaterial();
                    if ((this.m_cardFlair.Premium == TAG_PREMIUM.GOLDEN) && (premiumPortraitMaterial != null))
                    {
                        component.m_PortraitMesh.GetComponent<Renderer>().material = premiumPortraitMaterial;
                        component.m_PortraitMesh.GetComponent<Renderer>().material.mainTextureOffset = deckPickerPortrait.mainTextureOffset;
                        component.m_PortraitMesh.GetComponent<Renderer>().material.mainTextureScale = deckPickerPortrait.mainTextureScale;
                        component.m_PortraitMesh.GetComponent<Renderer>().material.SetTexture("_ShadowTex", null);
                        if (!this.m_seed.HasValue)
                        {
                            this.m_seed = new float?(UnityEngine.Random.value);
                        }
                        if (component.m_PortraitMesh.GetComponent<Renderer>().material.HasProperty("_Seed"))
                        {
                            component.m_PortraitMesh.GetComponent<Renderer>().material.SetFloat("_Seed", this.m_seed.Value);
                        }
                    }
                    else
                    {
                        component.m_PortraitMesh.GetComponent<Renderer>().sharedMaterial = deckPickerPortrait;
                    }
                }
            }
        }
    }

    public delegate void UnlockedCallback(HeroPickerButton button);
}

