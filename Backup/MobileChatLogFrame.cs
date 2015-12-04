using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MobileChatLogFrame : MonoBehaviour
{
    [CompilerGenerated]
    private static Func<Bounds, Renderer, Bounds> <>f__am$cache11;
    public ChatLog chatLog;
    public UIBButton closeButton;
    public Followers followers;
    public InputInfo inputInfo;
    public TextField inputTextField;
    public TournamentMedal medal;
    public GameObject medalPatch;
    public TouchList messageFrames;
    public MessageInfo messageInfo;
    public UberText nameText;
    public MobileChatNotification notifications;
    private PlayerIcon playerIcon;
    public Spawner playerIconRef;
    private BnetPlayer receiver;
    public NineSliceElement window;

    public event System.Action CloseButtonReleased;

    public event System.Action InputCanceled;

    private void Awake()
    {
        this.playerIcon = this.playerIconRef.Spawn<PlayerIcon>();
        this.UpdateBackgroundCollider();
        this.inputTextField.maxCharacters = 0x200;
        this.inputTextField.Submitted += new Action<string>(this.OnInputComplete);
        this.inputTextField.Canceled += new System.Action(this.OnInputCanceled);
        this.closeButton.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.OnCloseButtonReleased));
        BnetPresenceMgr.Get().AddPlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
    }

    public void Focus(bool focus)
    {
        if (focus && !this.inputTextField.Active)
        {
            this.inputTextField.Activate();
        }
        else if (!focus && this.inputTextField.Active)
        {
            this.inputTextField.Deactivate();
        }
    }

    private bool IsFullScreenKeyboard()
    {
        return (ChatMgr.Get().KeyboardRect.height == Screen.height);
    }

    private void OnCloseButtonReleased(UIEvent e)
    {
        if (this.CloseButtonReleased != null)
        {
            this.CloseButtonReleased();
        }
    }

    private void OnDestroy()
    {
        BnetPresenceMgr.Get().RemovePlayersChangedListener(new BnetPresenceMgr.PlayersChangedCallback(this.OnPlayersChanged));
    }

    private void OnInputCanceled()
    {
        if (this.InputCanceled != null)
        {
            this.InputCanceled();
        }
    }

    private void OnInputComplete(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            if (!BnetWhisperMgr.Get().SendWhisper(this.receiver, input))
            {
                this.chatLog.OnWhisperFailed();
            }
            ChatMgr.Get().AddRecentWhisperPlayerToTop(this.receiver);
        }
    }

    private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
    {
        if (changelist.FindChange(this.receiver) != null)
        {
            this.UpdateReceiver();
        }
    }

    public void SetWorldRect(float x, float y, float width, float height)
    {
        bool activeSelf = base.gameObject.activeSelf;
        base.gameObject.SetActive(true);
        float viewWindowMaxValue = this.messageFrames.ViewWindowMaxValue;
        this.window.SetEntireSize(width, height);
        Vector3 vector = TransformUtil.ComputeWorldPoint(TransformUtil.ComputeSetPointBounds(this.window), new Vector3(0f, 1f, 0f));
        Vector3 translation = new Vector3(x, y, vector.z) - vector;
        base.transform.Translate(translation);
        this.messageFrames.transform.position = (Vector3) ((this.messageInfo.messagesTopLeft.position + this.messageInfo.messagesBottomRight.position) / 2f);
        Vector3 vector3 = this.messageInfo.messagesBottomRight.position - this.messageInfo.messagesTopLeft.position;
        this.messageFrames.ClipSize = new Vector2(vector3.x, Math.Abs(vector3.y));
        this.messageFrames.ViewWindowMaxValue = viewWindowMaxValue;
        this.messageFrames.ScrollValue = Mathf.Clamp01(this.messageFrames.ScrollValue);
        this.chatLog.OnResize();
        this.UpdateBackgroundCollider();
        this.UpdateFollowers();
        base.gameObject.SetActive(activeSelf);
    }

    private void Start()
    {
        if (this.receiver == null)
        {
            base.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
    }

    private void UpdateBackgroundCollider()
    {
        if (<>f__am$cache11 == null)
        {
            <>f__am$cache11 = delegate (Bounds aggregate, Renderer renderer) {
                aggregate.Encapsulate(renderer.bounds);
                return aggregate;
            };
        }
        Bounds bounds = Enumerable.Aggregate<Renderer, Bounds>(this.window.GetComponentsInChildren<Renderer>(), new Bounds(base.transform.position, Vector3.zero), <>f__am$cache11);
        Vector3 vector = base.transform.InverseTransformPoint(bounds.min);
        Vector3 vector2 = base.transform.InverseTransformPoint(bounds.max);
        BoxCollider component = base.GetComponent<BoxCollider>();
        if (component == null)
        {
            component = base.gameObject.AddComponent<BoxCollider>();
        }
        component.center = ((Vector3) ((vector + vector2) / 2f)) + Vector3.forward;
        component.size = vector2 - vector;
    }

    private void UpdateFollowers()
    {
        this.followers.UpdateFollowPosition();
    }

    private void UpdateReceiver()
    {
        this.playerIcon.UpdateIcon();
        string str = !this.receiver.IsOnline() ? "999999ff" : "5ecaf0ff";
        string bestName = this.receiver.GetBestName();
        this.nameText.Text = string.Format("<color=#{0}>{1}</color>", str, bestName);
        if (((this.receiver != null) && this.receiver.IsDisplayable()) && this.receiver.IsOnline())
        {
            MedalInfoTranslator rankPresenceField = RankMgr.Get().GetRankPresenceField(this.receiver.GetBestGameAccount());
            if ((rankPresenceField == null) || (rankPresenceField.GetCurrentMedal().rank == 0x19))
            {
                this.medalPatch.SetActive(false);
                this.playerIcon.Show();
            }
            else
            {
                this.playerIcon.Hide();
                this.medal.SetEnabled(false);
                this.medal.SetMedal(rankPresenceField, false);
                this.medalPatch.SetActive(true);
            }
        }
        else if (!this.receiver.IsOnline())
        {
            this.medalPatch.SetActive(false);
            this.playerIcon.Show();
        }
    }

    public bool HasFocus
    {
        get
        {
            return this.inputTextField.Active;
        }
    }

    public BnetPlayer Receiver
    {
        get
        {
            return this.receiver;
        }
        set
        {
            if (this.receiver != value)
            {
                this.receiver = value;
                this.Focus(this.receiver != null);
                if (this.receiver != null)
                {
                    this.playerIcon.SetPlayer(this.receiver);
                    this.UpdateReceiver();
                    this.chatLog.Receiver = this.receiver;
                }
            }
        }
    }

    [Serializable]
    public class Followers
    {
        public UIBFollowObject bubbleFollower;
        public UIBFollowObject closeButtonFollower;
        public UIBFollowObject playerInfoFollower;

        public void UpdateFollowPosition()
        {
            this.playerInfoFollower.UpdateFollowPosition();
            this.closeButtonFollower.UpdateFollowPosition();
            this.bubbleFollower.UpdateFollowPosition();
        }
    }

    [Serializable]
    public class InputInfo
    {
        public Transform inputBottomRight;
        public Transform inputTopLeft;
    }

    [Serializable]
    public class MessageInfo
    {
        public Transform messagesBottomRight;
        public Transform messagesTopLeft;
    }
}

