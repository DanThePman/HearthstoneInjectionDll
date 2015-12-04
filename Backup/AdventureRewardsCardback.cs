using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class AdventureRewardsCardback : MonoBehaviour
{
    public Animation m_cardBackAppearAnimation;
    public string m_cardBackAppearAnimationName;
    public float m_cardBackAppearDelay = 0.5f;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_cardBackAppearSound;
    public float m_cardBackAppearTime = 0.5f;
    public GameObject m_cardBackContainer;
    public int m_cardBackId = -1;
    private GameObject m_cardBackObject;
    private bool m_cardBackObjectLoading;
    public UberText m_cardBackText;
    private Vector3 m_cardBackTextOrigScale;
    public float m_driftRadius = 0.1f;
    public float m_driftTime = 10f;
    public GeneralStoreAdventureContent m_parentContent;

    [DebuggerHidden]
    private IEnumerator AnimateCardBackIn()
    {
        return new <AnimateCardBackIn>c__IteratorB { <>f__this = this };
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            this.LoadCardBackWithId();
        }
        this.m_cardBackTextOrigScale = this.m_cardBackText.transform.localScale;
    }

    public void HideCardBackReward()
    {
        if (this.m_cardBackObject != null)
        {
            this.m_cardBackObject.SetActive(false);
        }
        if (this.m_cardBackText != null)
        {
            this.m_cardBackText.gameObject.SetActive(false);
        }
    }

    private void LoadCardBackWithId()
    {
        if (this.m_cardBackObject != null)
        {
            UnityEngine.Object.Destroy(this.m_cardBackObject);
        }
        if (this.m_cardBackId < 0)
        {
            UnityEngine.Debug.LogError("Card back ID must be a positive number");
        }
        else
        {
            this.m_cardBackObjectLoading = CardBackManager.Get().LoadCardBackByIndex(this.m_cardBackId, delegate (CardBackManager.LoadCardBackData cardBackData) {
                GameObject gameObject = cardBackData.m_GameObject;
                gameObject.transform.parent = base.transform;
                gameObject.name = "CARD_BACK_" + cardBackData.m_CardBackIndex;
                Actor component = gameObject.GetComponent<Actor>();
                if (component != null)
                {
                    GameObject cardMesh = component.m_cardMesh;
                    component.SetCardbackUpdateIgnore(true);
                    component.SetUnlit();
                    if (cardMesh != null)
                    {
                        Material material = cardMesh.GetComponent<Renderer>().material;
                        if (material.HasProperty("_SpecularIntensity"))
                        {
                            material.SetFloat("_SpecularIntensity", 0f);
                        }
                    }
                }
                this.m_cardBackObject = gameObject;
                SceneUtils.SetLayer(this.m_cardBackObject, this.m_cardBackContainer.gameObject.layer);
                GameUtils.SetParent(this.m_cardBackObject, this.m_cardBackContainer, false);
                this.m_cardBackObject.transform.localPosition = Vector3.zero;
                this.m_cardBackObject.transform.localScale = Vector3.one;
                this.m_cardBackObject.transform.localRotation = Quaternion.identity;
                AnimationUtil.FloatyPosition(this.m_cardBackContainer, this.m_driftRadius, this.m_driftTime);
                this.HideCardBackReward();
                this.m_cardBackObjectLoading = false;
            }, "Card_Hidden");
        }
    }

    public void ShowCardBackReward()
    {
        this.HideCardBackReward();
        if (((this.m_cardBackAppearAnimation != null) && !string.IsNullOrEmpty(this.m_cardBackAppearAnimationName)) && base.gameObject.activeInHierarchy)
        {
            base.StopCoroutine("AnimateCardBackIn");
            base.StartCoroutine("AnimateCardBackIn");
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateCardBackIn>c__IteratorB : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal AdventureRewardsCardback <>f__this;

        [DebuggerHidden]
        public void Dispose()
        {
            this.$PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint) this.$PC;
            this.$PC = -1;
            switch (num)
            {
                case 0:
                    this.<>f__this.m_cardBackAppearAnimation.Stop(this.<>f__this.m_cardBackAppearAnimationName);
                    this.<>f__this.m_cardBackAppearAnimation.Rewind(this.<>f__this.m_cardBackAppearAnimationName);
                    this.$current = new WaitForSeconds(this.<>f__this.m_cardBackAppearDelay);
                    this.$PC = 1;
                    goto Label_01D6;

                case 1:
                    this.<>f__this.m_cardBackAppearAnimation.Play(this.<>f__this.m_cardBackAppearAnimationName);
                    if (!string.IsNullOrEmpty(this.<>f__this.m_cardBackAppearSound))
                    {
                        SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.<>f__this.m_cardBackAppearSound));
                    }
                    this.$current = new WaitForSeconds(this.<>f__this.m_cardBackAppearTime);
                    this.$PC = 2;
                    goto Label_01D6;

                case 2:
                case 3:
                {
                    if (this.<>f__this.m_cardBackObjectLoading)
                    {
                        this.$current = null;
                        this.$PC = 3;
                        goto Label_01D6;
                    }
                    if (this.<>f__this.m_cardBackObject != null)
                    {
                        this.<>f__this.m_cardBackObject.SetActive(true);
                    }
                    this.<>f__this.m_cardBackText.gameObject.SetActive(true);
                    this.<>f__this.m_cardBackText.transform.localScale = (Vector3) (Vector3.one * 0.01f);
                    object[] args = new object[] { "scale", this.<>f__this.m_cardBackTextOrigScale, "time", this.<>f__this.m_cardBackAppearTime };
                    iTween.ScaleTo(this.<>f__this.m_cardBackText.gameObject, iTween.Hash(args));
                    this.$PC = -1;
                    break;
                }
            }
            return false;
        Label_01D6:
            return true;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.$current;
            }
        }
    }
}

