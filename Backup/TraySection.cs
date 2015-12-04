using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class TraySection : MonoBehaviour
{
    private static readonly Vector3 DECKBOX_LOCAL_EULER_ANGLES = new Vector3(90f, 180f, 0f);
    private const float DOOR_ANIM_SPEED = 6f;
    private readonly string DOOR_CLOSE_ANIM_NAME = "Deck_DoorClose";
    private readonly string DOOR_OPEN_ANIM_NAME = "Deck_DoorOpen";
    public CollectionDeckBoxVisual m_deckBox;
    private bool m_deckBoxShown;
    public GameObject m_door;
    private bool m_inEditPosition;
    private Vector3? m_intermediateDeckBoxLocalPos;
    private bool m_isOpen;
    private bool m_wasTouchModeEnabled;

    private void Awake()
    {
        if (this.m_deckBox != null)
        {
            this.m_deckBox.transform.localPosition = CollectionDeckBoxVisual.POPPED_DOWN_LOCAL_POS;
            this.m_deckBox.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
            this.m_deckBox.transform.localEulerAngles = new Vector3(90f, 180f, 0f);
        }
        this.m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
        UIBScrollableItem component = base.GetComponent<UIBScrollableItem>();
        if (component != null)
        {
            component.SetCustomActiveState(new UIBScrollableItem.ActiveStateCallback(this.IsDeckBoxShown));
        }
    }

    public void ClearDeckInfo()
    {
        if (this.m_deckBox != null)
        {
            this.m_deckBox.SetDeckName(string.Empty);
            this.m_deckBox.SetDeckID(-1L);
        }
    }

    public void CloseDoor()
    {
        this.CloseDoor(null);
    }

    public void CloseDoor(DelOnDoorStateChangedCallback callback)
    {
        this.CloseDoor(callback, null);
    }

    public void CloseDoor(DelOnDoorStateChangedCallback callback, object callbackData)
    {
        this.CloseDoor(false, callback, callbackData);
    }

    private void CloseDoor(bool isImmediate, DelOnDoorStateChangedCallback callback, object callbackData)
    {
        if (!this.m_isOpen)
        {
            if (callback != null)
            {
                callback(callbackData);
            }
        }
        else
        {
            this.m_isOpen = false;
            this.m_door.GetComponent<Animation>()[this.DOOR_CLOSE_ANIM_NAME].time = !isImmediate ? 0f : this.m_door.GetComponent<Animation>()[this.DOOR_CLOSE_ANIM_NAME].length;
            this.m_door.GetComponent<Animation>()[this.DOOR_CLOSE_ANIM_NAME].speed = 6f;
            this.PlayDoorAnimation(this.DOOR_CLOSE_ANIM_NAME, callback, callbackData);
        }
    }

    public void CloseDoorImmediately()
    {
        this.CloseDoorImmediately(null);
    }

    public void CloseDoorImmediately(DelOnDoorStateChangedCallback callback)
    {
        this.CloseDoorImmediately(callback, null);
    }

    public void CloseDoorImmediately(DelOnDoorStateChangedCallback callback, object callbackData)
    {
        this.CloseDoor(true, callback, callbackData);
    }

    public void FlipDeckBoxHalfOverToShow(float animTime, DelOnDoorStateChangedCallback callback = null)
    {
        <FlipDeckBoxHalfOverToShow>c__AnonStorey2EB storeyeb = new <FlipDeckBoxHalfOverToShow>c__AnonStorey2EB {
            callback = callback,
            <>f__this = this
        };
        this.m_deckBox.gameObject.SetActive(true);
        this.m_deckBox.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        SoundManager.Get().LoadAndPlay("collection_manager_new_deck_edge_flips", base.gameObject);
        iTween.StopByName(this.m_deckBox.gameObject, "rotation");
        object[] args = new object[] { "rotation", DECKBOX_LOCAL_EULER_ANGLES, "isLocal", true, "time", animTime, "easeType", iTween.EaseType.easeInCubic, "oncomplete", new Action<object>(storeyeb.<>m__D7), "name", "rotation" };
        iTween.RotateTo(this.m_deckBox.gameObject, iTween.Hash(args));
    }

    public Bounds GetDoorBounds()
    {
        return this.m_door.GetComponent<Renderer>().bounds;
    }

    public Vector3? GetIntermediateDeckBoxPos()
    {
        return this.m_intermediateDeckBoxLocalPos;
    }

    public void HideDeckBox(bool immediate = false, DelOnDoorStateChangedCallback callback = null)
    {
        <HideDeckBox>c__AnonStorey2E8 storeye = new <HideDeckBox>c__AnonStorey2E8 {
            callback = callback,
            <>f__this = this
        };
        this.m_deckBoxShown = false;
        this.CloseDoor(immediate, new DelOnDoorStateChangedCallback(storeye.<>m__D3), null);
    }

    public bool IsDeckBoxShown()
    {
        return this.m_deckBoxShown;
    }

    public bool IsOpen()
    {
        return this.m_isOpen;
    }

    public void MoveDeckBoxBackToOriginalPosition(float time, DelOnDoorStateChangedCallback callback = null)
    {
        <MoveDeckBoxBackToOriginalPosition>c__AnonStorey2EA storeyea = new <MoveDeckBoxBackToOriginalPosition>c__AnonStorey2EA {
            callback = callback,
            <>f__this = this
        };
        if (this.m_deckBox != null)
        {
            if (!this.m_intermediateDeckBoxLocalPos.HasValue)
            {
                UnityEngine.Debug.LogWarning("Unable to move deck box to original position. It has not been moved!", base.gameObject);
            }
            else
            {
                this.m_inEditPosition = false;
                if (UniversalInputManager.Get().IsTouchMode())
                {
                    this.m_deckBox.ShowEditDecal(false);
                }
                this.OpenDoor(new DelOnDoorStateChangedCallback(storeyea.<>m__D5));
                object[] args = new object[] { "position", this.m_intermediateDeckBoxLocalPos.Value, "islocal", true, "time", time, "easetype", iTween.EaseType.linear, "oncomplete", new Action<object>(storeyea.<>m__D6) };
                iTween.MoveTo(this.m_deckBox.gameObject, iTween.Hash(args));
            }
        }
    }

    public void MoveDeckBoxToEditPosition(Vector3 position, float time, DelOnDoorStateChangedCallback callback = null)
    {
        <MoveDeckBoxToEditPosition>c__AnonStorey2E9 storeye = new <MoveDeckBoxToEditPosition>c__AnonStorey2E9 {
            position = position,
            time = time,
            callback = callback,
            <>f__this = this
        };
        if (this.m_deckBox != null)
        {
            if (this.m_intermediateDeckBoxLocalPos.HasValue)
            {
                UnityEngine.Debug.LogWarning("Unable to move deck box to edit position. It has already been moved!", base.gameObject);
            }
            else
            {
                this.m_inEditPosition = true;
                this.m_deckBox.DisableButtonAnimation();
                if (UniversalInputManager.Get().IsTouchMode())
                {
                    this.m_deckBox.ShowEditDecal(true);
                }
                this.m_door.gameObject.SetActive(true);
                this.CloseDoor();
                this.m_deckBox.PlayScaleUpAnimation(new CollectionDeckBoxVisual.DelOnAnimationFinished(storeye.<>m__D4));
            }
        }
    }

    public void OpenDoor()
    {
        this.OpenDoor(null);
    }

    public void OpenDoor(DelOnDoorStateChangedCallback callback)
    {
        this.OpenDoor(callback, null);
    }

    public void OpenDoor(DelOnDoorStateChangedCallback callback, object callbackData)
    {
        this.OpenDoor(false, callback, callbackData);
    }

    private void OpenDoor(bool isImmediate, DelOnDoorStateChangedCallback callback, object callbackData)
    {
        if (this.m_isOpen)
        {
            if (callback != null)
            {
                callback(callbackData);
            }
        }
        else
        {
            this.m_isOpen = true;
            this.m_door.GetComponent<Animation>()[this.DOOR_OPEN_ANIM_NAME].time = !isImmediate ? 0f : this.m_door.GetComponent<Animation>()[this.DOOR_OPEN_ANIM_NAME].length;
            this.m_door.GetComponent<Animation>()[this.DOOR_OPEN_ANIM_NAME].speed = 6f;
            this.PlayDoorAnimation(this.DOOR_OPEN_ANIM_NAME, callback, callbackData);
        }
    }

    public void OpenDoorImmediately()
    {
        this.OpenDoorImmediately(null);
    }

    public void OpenDoorImmediately(DelOnDoorStateChangedCallback callback)
    {
        this.OpenDoorImmediately(callback, null);
    }

    public void OpenDoorImmediately(DelOnDoorStateChangedCallback callback, object callbackData)
    {
        this.OpenDoor(true, callback, callbackData);
    }

    private void PlayDoorAnimation(string animationName, DelOnDoorStateChangedCallback callback, object callbackData)
    {
        this.m_door.GetComponent<Animation>().Play(animationName);
        OnDoorStateChangedCallbackData data = new OnDoorStateChangedCallbackData {
            m_callback = callback,
            m_callbackData = callbackData,
            m_animationName = animationName
        };
        base.StopCoroutine("WaitThenCallDoorAnimationCallback");
        base.StartCoroutine("WaitThenCallDoorAnimationCallback", data);
    }

    public void ShowDeckBox(bool immediate = false, DelOnDoorStateChangedCallback callback = null)
    {
        <ShowDeckBox>c__AnonStorey2E7 storeye = new <ShowDeckBox>c__AnonStorey2E7 {
            callback = callback,
            <>f__this = this
        };
        this.m_deckBoxShown = true;
        this.m_door.gameObject.SetActive(true);
        this.OpenDoor(immediate, new DelOnDoorStateChangedCallback(storeye.<>m__D2), null);
    }

    public void ShowDoor(bool show)
    {
        this.m_door.gameObject.SetActive(show);
    }

    private void Update()
    {
        if (this.m_wasTouchModeEnabled != UniversalInputManager.Get().IsTouchMode())
        {
            this.m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
            if ((this.m_deckBox != null) && UniversalInputManager.Get().IsTouchMode())
            {
                this.m_deckBox.ShowEditDecal(this.m_inEditPosition);
            }
        }
    }

    [DebuggerHidden]
    private IEnumerator WaitThenCallDoorAnimationCallback(OnDoorStateChangedCallbackData callbackData)
    {
        return new <WaitThenCallDoorAnimationCallback>c__Iterator56 { callbackData = callbackData, <$>callbackData = callbackData, <>f__this = this };
    }

    [CompilerGenerated]
    private sealed class <FlipDeckBoxHalfOverToShow>c__AnonStorey2EB
    {
        internal TraySection <>f__this;
        internal TraySection.DelOnDoorStateChangedCallback callback;

        internal void <>m__D7(object _1)
        {
            if (this.callback != null)
            {
                this.callback(this.<>f__this);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <HideDeckBox>c__AnonStorey2E8
    {
        internal TraySection <>f__this;
        internal TraySection.DelOnDoorStateChangedCallback callback;

        internal void <>m__D3(object _1)
        {
            this.<>f__this.m_door.gameObject.SetActive(true);
            if (this.<>f__this.m_deckBox != null)
            {
                this.<>f__this.m_deckBox.PlayPopDownAnimation(delegate (object _2) {
                    this.<>f__this.m_deckBox.Hide();
                    if (this.callback != null)
                    {
                        this.callback(this.<>f__this);
                    }
                });
            }
            else if (this.callback != null)
            {
                this.callback(this.<>f__this);
            }
        }

        internal void <>m__D9(object _2)
        {
            this.<>f__this.m_deckBox.Hide();
            if (this.callback != null)
            {
                this.callback(this.<>f__this);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <MoveDeckBoxBackToOriginalPosition>c__AnonStorey2EA
    {
        internal TraySection <>f__this;
        internal TraySection.DelOnDoorStateChangedCallback callback;

        internal void <>m__D5(object _1)
        {
            this.<>f__this.m_door.gameObject.SetActive(false);
        }

        internal void <>m__D6(object _1)
        {
            this.<>f__this.m_intermediateDeckBoxLocalPos = null;
            this.<>f__this.m_deckBox.PlayScaleDownAnimation(delegate (object _2) {
                if (this.callback != null)
                {
                    this.callback(this.<>f__this);
                }
                this.<>f__this.m_deckBox.EnableButtonAnimation();
                this.<>f__this.m_door.gameObject.SetActive(false);
            });
        }

        internal void <>m__DB(object _2)
        {
            if (this.callback != null)
            {
                this.callback(this.<>f__this);
            }
            this.<>f__this.m_deckBox.EnableButtonAnimation();
            this.<>f__this.m_door.gameObject.SetActive(false);
        }
    }

    [CompilerGenerated]
    private sealed class <MoveDeckBoxToEditPosition>c__AnonStorey2E9
    {
        internal TraySection <>f__this;
        internal TraySection.DelOnDoorStateChangedCallback callback;
        internal Vector3 position;
        internal float time;

        internal void <>m__D4(object _1)
        {
            this.<>f__this.m_intermediateDeckBoxLocalPos = new Vector3?(this.<>f__this.m_deckBox.transform.localPosition);
            object[] args = new object[] { "position", this.position, "islocal", false, "time", this.time, "easetype", iTween.EaseType.linear, "oncomplete", delegate (object _2) {
                if (this.callback != null)
                {
                    this.callback(this.<>f__this);
                }
            } };
            iTween.MoveTo(this.<>f__this.m_deckBox.gameObject, iTween.Hash(args));
        }

        internal void <>m__DA(object _2)
        {
            if (this.callback != null)
            {
                this.callback(this.<>f__this);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <ShowDeckBox>c__AnonStorey2E7
    {
        internal TraySection <>f__this;
        internal TraySection.DelOnDoorStateChangedCallback callback;

        internal void <>m__D2(object _1)
        {
            if (this.<>f__this.m_deckBox != null)
            {
                this.<>f__this.m_deckBox.Show();
                this.<>f__this.m_deckBox.PlayPopUpAnimation(delegate (object _2) {
                    this.<>f__this.m_door.gameObject.SetActive(false);
                    if (this.callback != null)
                    {
                        this.callback(this.<>f__this);
                    }
                });
            }
            else
            {
                this.<>f__this.m_door.gameObject.SetActive(false);
                if (this.callback != null)
                {
                    this.callback(this.<>f__this);
                }
            }
        }

        internal void <>m__D8(object _2)
        {
            this.<>f__this.m_door.gameObject.SetActive(false);
            if (this.callback != null)
            {
                this.callback(this.<>f__this);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <WaitThenCallDoorAnimationCallback>c__Iterator56 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal TraySection.OnDoorStateChangedCallbackData <$>callbackData;
        internal TraySection <>f__this;
        internal TraySection.OnDoorStateChangedCallbackData callbackData;

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
                    if (this.callbackData.m_callback != null)
                    {
                        this.$current = new WaitForSeconds(this.<>f__this.m_door.GetComponent<Animation>()[this.callbackData.m_animationName].length / this.<>f__this.m_door.GetComponent<Animation>()[this.callbackData.m_animationName].speed);
                        this.$PC = 1;
                        return true;
                    }
                    break;

                case 1:
                    this.callbackData.m_callback(this.callbackData.m_callbackData);
                    this.$PC = -1;
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

    public delegate void DelOnDoorStateChangedCallback(object callbackData);

    private class OnDoorStateChangedCallbackData
    {
        public string m_animationName;
        public TraySection.DelOnDoorStateChangedCallback m_callback;
        public object m_callbackData;
    }
}

