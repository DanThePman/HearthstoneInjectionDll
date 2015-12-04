using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[CustomEditClass]
public class GeneralStoreRewardsCardBack : MonoBehaviour
{
    public Animation m_cardBackAppearAnimation;
    public string m_cardBackAppearAnimationName;
    public float m_cardBackAppearDelay = 0.5f;
    [CustomEditField(T=EditType.SOUND_PREFAB)]
    public string m_cardBackAppearSound;
    public float m_cardBackAppearTime = 0.5f;
    public GameObject m_cardBackContainer;
    private int m_cardBackId = -1;
    private GameObject m_cardBackObject;
    private bool m_cardBackObjectLoading;
    public UberText m_cardBackText;
    private Vector3 m_cardBackTextOrigScale;
    public float m_driftRadius = 0.1f;
    public float m_driftTime = 10f;

    [DebuggerHidden]
    private IEnumerator AnimateCardBackIn()
    {
        return new <AnimateCardBackIn>c__Iterator1E1 { <>f__this = this };
    }

    private void Awake()
    {
        this.m_cardBackTextOrigScale = this.m_cardBackText.transform.localScale;
    }

    public void HideCardBackReward()
    {
        base.StopCoroutine("AnimateCardBackIn");
        if (this.m_cardBackContainer != null)
        {
            this.m_cardBackContainer.SetActive(false);
        }
    }

    private void LoadCardBackWithId(int cardBackId)
    {
        if (this.m_cardBackObject != null)
        {
            UnityEngine.Object.Destroy(this.m_cardBackObject);
        }
        if (cardBackId < 0)
        {
            UnityEngine.Debug.LogError("Card back ID must be a positive number");
        }
        else
        {
            this.m_cardBackId = cardBackId;
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
                if (this.m_cardBackContainer != null)
                {
                    this.m_cardBackContainer.SetActive(false);
                }
                this.m_cardBackObjectLoading = false;
            }, "Card_Hidden");
        }
    }

    public void SetCardBack(int id)
    {
        if ((id != -1) && (id != this.m_cardBackId))
        {
            this.LoadCardBackWithId(id);
        }
    }

    public void SetPreorderText(string text)
    {
        this.m_cardBackText.Text = text;
    }

    public void ShowCardBackReward()
    {
        this.HideCardBackReward();
        if ((this.m_cardBackId != -1) && (((this.m_cardBackAppearAnimation != null) && !string.IsNullOrEmpty(this.m_cardBackAppearAnimationName)) && base.gameObject.activeInHierarchy))
        {
            base.StartCoroutine("AnimateCardBackIn");
        }
    }

    [CompilerGenerated]
    private sealed class <AnimateCardBackIn>c__Iterator1E1 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GeneralStoreRewardsCardBack <>f__this;

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
                    this.<>f__this.m_cardBackText.gameObject.SetActive(false);
                    this.<>f__this.m_cardBackAppearAnimation.Stop(this.<>f__this.m_cardBackAppearAnimationName);
                    this.<>f__this.m_cardBackAppearAnimation.Rewind(this.<>f__this.m_cardBackAppearAnimationName);
                    this.$current = new WaitForSeconds(this.<>f__this.m_cardBackAppearDelay);
                    this.$PC = 1;
                    goto Label_01FD;

                case 1:
                case 2:
                    if (this.<>f__this.m_cardBackObjectLoading)
                    {
                        this.$current = null;
                        this.$PC = 2;
                    }
                    else
                    {
                        this.<>f__this.m_cardBackContainer.SetActive(true);
                        this.<>f__this.m_cardBackAppearAnimation.Play(this.<>f__this.m_cardBackAppearAnimationName);
                        if (!string.IsNullOrEmpty(this.<>f__this.m_cardBackAppearSound))
                        {
                            SoundManager.Get().LoadAndPlay(FileUtils.GameAssetPathToName(this.<>f__this.m_cardBackAppearSound));
                        }
                        this.$current = new WaitForSeconds(this.<>f__this.m_cardBackAppearTime);
                        this.$PC = 3;
                    }
                    goto Label_01FD;

                case 3:
                {
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
        Label_01FD:
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

