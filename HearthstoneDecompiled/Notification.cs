using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public MeshRenderer artOverlay;
    public GameObject bottomLeftBubble;
    public GameObject bottomLeftPopupArrow;
    public GameObject bottomPopupArrow;
    public GameObject bottomRightBubble;
    public GameObject bottomRightPopupArrow;
    private const float BOUNCE_SPEED = 0.75f;
    public GameObject bounceObject;
    private SpeechBubbleDirection bubbleDirection;
    public NormalButton ButtonNO;
    public NormalButton ButtonOK;
    public StandardPegButtonNew ButtonStart;
    public NormalButton ButtonYES;
    public PegUIElement clickOff;
    public const float DEATH_ANIMATION_DURATION = 0.5f;
    public Spell destroyEvent;
    private const float FADE_PAUSE = 0.85f;
    private const float FADE_SPEED = 0.5f;
    public GameObject fadeArrowObject;
    public UberText headlineUberText;
    private bool isDying;
    public GameObject leftPopupArrow;
    private AudioSource m_accompaniedAudio;
    private const int MAX_CHARACTERS = 20;
    private const int MAX_CHARACTERS_IN_DIALOG = 0x1c;
    public System.Action OnFinishDeathState;
    public GameObject rightPopupArrow;
    public Spell showEvent;
    public UberText speechUberText;
    public Material swapMaterial;
    public GameObject topPopupArrow;
    public GameObject upperLeftBubble;
    public GameObject upperRightBubble;

    public void AssignAudio(AudioSource source)
    {
        this.m_accompaniedAudio = source;
    }

    private void BounceDown()
    {
        object[] args = new object[] { "islocal", true, "z", this.bounceObject.transform.localPosition.z + 0.5f, "time", 0.75f, "easetype", iTween.EaseType.easeOutCubic, "oncomplete", "BounceUp", "oncompletetarget", base.gameObject };
        iTween.MoveTo(this.bounceObject, iTween.Hash(args));
    }

    private void BounceUp()
    {
        object[] args = new object[] { "islocal", true, "z", this.bounceObject.transform.localPosition.z - 0.5f, "time", 0.75f, "easetype", iTween.EaseType.easeInCubic, "oncomplete", "BounceDown", "oncompletetarget", base.gameObject };
        iTween.MoveTo(this.bounceObject, iTween.Hash(args));
    }

    public void ChangeDialogText(string headlineString, string bodyString, string yesOrOKstring, string noString)
    {
        this.speechUberText.Text = bodyString;
        this.headlineUberText.Text = headlineString;
        if (noString != string.Empty)
        {
            this.ButtonNO.gameObject.SetActive(true);
            this.ButtonNO.SetText(noString);
            this.ButtonOK.gameObject.SetActive(false);
            if (yesOrOKstring != string.Empty)
            {
                this.ButtonYES.gameObject.SetActive(true);
                this.ButtonYES.SetText(yesOrOKstring);
            }
        }
        else if (yesOrOKstring != string.Empty)
        {
            this.ButtonOK.gameObject.SetActive(true);
            this.ButtonOK.SetText(yesOrOKstring);
            if (this.ButtonNO != null)
            {
                this.ButtonNO.gameObject.SetActive(false);
            }
            if (this.ButtonYES != null)
            {
                this.ButtonYES.gameObject.SetActive(false);
            }
        }
    }

    public void ChangeText(string newText)
    {
        this.speechUberText.Text = newText;
    }

    public void FaceDirection(SpeechBubbleDirection direction)
    {
        this.bubbleDirection = direction;
        switch (direction)
        {
            case SpeechBubbleDirection.TopLeft:
                this.upperLeftBubble.GetComponent<Renderer>().enabled = true;
                break;

            case SpeechBubbleDirection.TopRight:
                this.upperRightBubble.GetComponent<Renderer>().enabled = true;
                break;

            case SpeechBubbleDirection.BottomLeft:
                this.bottomLeftBubble.GetComponent<Renderer>().enabled = true;
                break;

            case SpeechBubbleDirection.BottomRight:
                this.bottomRightBubble.GetComponent<Renderer>().enabled = true;
                break;
        }
    }

    private void FadeComplete()
    {
        object[] args = new object[] { "islocal", true, "z", this.fadeArrowObject.transform.localPosition.z + 0.5f, "time", 0f, "delay", 0.5f, "easetype", iTween.EaseType.linear, "oncomplete", "FadeOut", "oncompletetarget", base.gameObject };
        iTween.MoveTo(this.fadeArrowObject, iTween.Hash(args));
        AnimationUtil.FadeTexture(this.fadeArrowObject.GetComponentInChildren<MeshRenderer>(), 0f, 1f, 0f, 0.85f, null);
    }

    private void FadeOut()
    {
        object[] args = new object[] { "islocal", true, "z", this.fadeArrowObject.transform.localPosition.z - 0.5f, "time", 0.5f, "easetype", iTween.EaseType.linear, "oncomplete", "FadeComplete", "oncompletetarget", base.gameObject };
        iTween.MoveTo(this.fadeArrowObject, iTween.Hash(args));
        AnimationUtil.FadeTexture(this.fadeArrowObject.GetComponentInChildren<MeshRenderer>(), 1f, 0f, 0.5f, 0.15f, null);
    }

    private void FinishDeath()
    {
        UnityEngine.Object.Destroy(base.gameObject);
        if (this.OnFinishDeathState != null)
        {
            this.OnFinishDeathState();
        }
    }

    public AudioSource GetAudio()
    {
        return this.m_accompaniedAudio;
    }

    public SpeechBubbleDirection GetSpeechBubbleDirection()
    {
        return this.bubbleDirection;
    }

    public bool IsDying()
    {
        return this.isDying;
    }

    private void OnDestroy()
    {
        if ((this.m_accompaniedAudio != null) && (SoundManager.Get() != null))
        {
            SoundManager.Get().Destroy(this.m_accompaniedAudio);
        }
    }

    public void PlayBirth()
    {
        if (this.showEvent != null)
        {
            this.showEvent.Activate();
        }
        if ((this.bounceObject == null) && (this.fadeArrowObject == null))
        {
            Vector3 localScale = base.transform.localScale;
            base.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            object[] args = new object[] { "scale", localScale, "easetype", iTween.EaseType.easeOutElastic, "time", 1f };
            iTween.ScaleTo(base.gameObject, iTween.Hash(args));
        }
        else if (this.bounceObject != null)
        {
            this.BounceDown();
        }
        else if (this.fadeArrowObject != null)
        {
            this.FadeOut();
        }
    }

    public void PlayBirthWithForcedScale(Vector3 targetScale)
    {
        object[] args = new object[] { "scale", targetScale, "easetype", iTween.EaseType.easeOutElastic, "time", 1f };
        iTween.ScaleTo(base.gameObject, iTween.Hash(args));
    }

    public void PlayDeath()
    {
        if (this.destroyEvent != null)
        {
            this.destroyEvent.Activate();
        }
        if ((this.bounceObject != null) || (this.fadeArrowObject != null))
        {
            this.FinishDeath();
        }
        else if (this.ButtonOK != null)
        {
            this.isDying = true;
            object[] args = new object[] { "scale", Vector3.zero, "easetype", iTween.EaseType.easeOutExpo, "time", 0.5f, "oncomplete", "FinishDeath", "oncompletetarget", base.gameObject };
            iTween.ScaleTo(base.gameObject, iTween.Hash(args));
        }
        else
        {
            this.isDying = true;
            object[] objArray2 = new object[] { "scale", Vector3.zero, "easetype", iTween.EaseType.easeInExpo, "time", 0.5f, "oncomplete", "FinishDeath", "oncompletetarget", base.gameObject };
            iTween.ScaleTo(base.gameObject, iTween.Hash(objArray2));
        }
    }

    public void PlaySmallBirthForFakeBubble()
    {
        if (this.showEvent != null)
        {
            this.showEvent.Activate();
        }
        if ((this.bounceObject == null) && (this.fadeArrowObject == null))
        {
            float num = 0.25f;
            Vector3 vector = new Vector3(num * base.transform.localScale.x, num * base.transform.localScale.y, num * base.transform.localScale.z);
            base.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            object[] args = new object[] { "scale", vector, "easetype", iTween.EaseType.easeOutElastic, "time", 1f };
            iTween.ScaleTo(base.gameObject, iTween.Hash(args));
        }
        else
        {
            this.BounceDown();
        }
    }

    [DebuggerHidden]
    private IEnumerator PulseReminder(float seconds)
    {
        return new <PulseReminder>c__Iterator261 { seconds = seconds, <$>seconds = seconds, <>f__this = this };
    }

    public void PulseReminderEveryXSeconds(float seconds)
    {
        base.StartCoroutine(this.PulseReminder(seconds));
    }

    public void SetPosition(Actor actor, SpeechBubbleDirection direction)
    {
        if (actor.GetBones() == null)
        {
            UnityEngine.Debug.LogError("Notification Error - Tried to set the position of a Speech Bubble, but the target actor has no bones!");
        }
        else
        {
            GameObject parentObject = SceneUtils.FindChildBySubstring(actor.GetBones(), "SpeechBubbleBones");
            if (parentObject == null)
            {
                UnityEngine.Debug.LogError("Notification Error - Tried to set the position of a Speech Bubble, but the target actor has no SpeechBubbleBones!");
            }
            else
            {
                Vector3 zero = Vector3.zero;
                switch (direction)
                {
                    case SpeechBubbleDirection.TopLeft:
                        zero = SceneUtils.FindChildBySubstring(parentObject, "BottomRight").transform.position;
                        break;

                    case SpeechBubbleDirection.TopRight:
                        zero = SceneUtils.FindChildBySubstring(parentObject, "BottomLeft").transform.position;
                        break;

                    case SpeechBubbleDirection.BottomLeft:
                        zero = SceneUtils.FindChildBySubstring(parentObject, "TopRight").transform.position;
                        break;

                    case SpeechBubbleDirection.BottomRight:
                        zero = SceneUtils.FindChildBySubstring(parentObject, "TopLeft").transform.position;
                        break;
                }
                base.transform.position = zero;
            }
        }
    }

    public void SetPositionForSmallBubble(Actor actor)
    {
        if (actor.GetBones() == null)
        {
            UnityEngine.Debug.LogError("Notification Error - Tried to set the position of a Speech Bubble, but the target actor has no bones!");
        }
        else
        {
            GameObject parentObject = SceneUtils.FindChildBySubstring(actor.GetBones(), "SpeechBubbleBones");
            if (parentObject == null)
            {
                UnityEngine.Debug.LogError("Notification Error - Tried to set the position of a Speech Bubble, but the target actor has no SpeechBubbleBones!");
            }
            else
            {
                base.transform.position = SceneUtils.FindChildBySubstring(parentObject, "SmallBubble").transform.position;
            }
        }
    }

    public void ShowPopUpArrow(PopUpArrowDirection direction)
    {
        switch (direction)
        {
            case PopUpArrowDirection.Left:
                this.leftPopupArrow.GetComponent<Renderer>().enabled = true;
                break;

            case PopUpArrowDirection.Right:
                this.rightPopupArrow.GetComponent<Renderer>().enabled = true;
                break;

            case PopUpArrowDirection.Down:
                this.bottomPopupArrow.GetComponent<Renderer>().enabled = true;
                break;

            case PopUpArrowDirection.Up:
                this.topPopupArrow.GetComponent<Renderer>().enabled = true;
                break;

            case PopUpArrowDirection.LeftDown:
                this.bottomLeftPopupArrow.GetComponent<Renderer>().enabled = true;
                break;

            case PopUpArrowDirection.RightDown:
                this.bottomRightPopupArrow.GetComponent<Renderer>().enabled = true;
                break;
        }
    }

    [CompilerGenerated]
    private sealed class <PulseReminder>c__Iterator261 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal float <$>seconds;
        internal Notification <>f__this;
        internal float seconds;

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
                    this.$current = new WaitForSeconds(this.seconds);
                    this.$PC = 1;
                    return true;

                case 1:
                    if (!this.<>f__this.isDying)
                    {
                        object[] args = new object[] { "amount", Vector3.one, "time", 1f };
                        iTween.PunchScale(this.<>f__this.gameObject, iTween.Hash(args));
                        this.<>f__this.StartCoroutine(this.<>f__this.PulseReminder(this.seconds));
                        this.$PC = -1;
                        break;
                    }
                    break;
            }
            return false;
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

    public enum PopUpArrowDirection
    {
        Left,
        Right,
        Down,
        Up,
        LeftDown,
        RightDown
    }

    public enum SpeechBubbleDirection
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}

