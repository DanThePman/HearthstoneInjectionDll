using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyEmoteHandler : MonoBehaviour
{
    private bool emotesDisplayed;
    private bool m_squelched;
    public GameObject m_squelchEmote;
    public MeshRenderer m_squelchEmoteBackplate;
    public UberText m_squelchEmoteDisplayText;
    private Vector3 m_squelchEmoteStartingScale;
    private static EnemyEmoteHandler s_instance;
    private int shownAt;
    private bool squelchMousedOver;

    public bool AreEmotesActive()
    {
        return this.emotesDisplayed;
    }

    private void Awake()
    {
        s_instance = this;
        base.GetComponent<Collider>().enabled = false;
        this.m_squelchEmoteStartingScale = this.m_squelchEmote.transform.localScale;
        this.m_squelchEmoteDisplayText.Text = GameStrings.Get("GAMEPLAY_EMOTE_SQUELCH");
        this.m_squelchEmoteDisplayText.gameObject.SetActive(false);
        this.m_squelchEmoteBackplate.enabled = false;
        this.m_squelchEmote.transform.localScale = Vector3.zero;
    }

    private void DoSquelchClick()
    {
        this.m_squelched = !this.m_squelched;
        this.HideEmotes();
    }

    public static EnemyEmoteHandler Get()
    {
        return s_instance;
    }

    public void HandleInput()
    {
        RaycastHit hit;
        if (!this.HitTestEmotes(out hit))
        {
            this.HideEmotes();
        }
        else
        {
            if (hit.transform.gameObject != this.m_squelchEmote)
            {
                if (this.squelchMousedOver)
                {
                    this.MouseOutSquelch();
                    this.squelchMousedOver = false;
                }
            }
            else if (!this.squelchMousedOver)
            {
                this.squelchMousedOver = true;
                this.MouseOverSquelch();
            }
            if (UniversalInputManager.Get().GetMouseButtonUp(0))
            {
                if (this.squelchMousedOver)
                {
                    this.DoSquelchClick();
                }
                else if (UniversalInputManager.Get().IsTouchMode() && (UnityEngine.Time.frameCount != this.shownAt))
                {
                    this.HideEmotes();
                }
            }
        }
    }

    public void HideEmotes()
    {
        if (this.emotesDisplayed)
        {
            this.emotesDisplayed = false;
            base.GetComponent<Collider>().enabled = false;
            this.m_squelchEmote.GetComponent<Collider>().enabled = false;
            iTween.Stop(this.m_squelchEmote);
            object[] args = new object[] { "scale", Vector3.zero, "time", 0.1f, "easetype", iTween.EaseType.linear, "oncompletetarget", base.gameObject, "oncomplete", "FinishDisable" };
            iTween.ScaleTo(this.m_squelchEmote, iTween.Hash(args));
        }
    }

    private bool HitTestEmotes(out RaycastHit hitInfo)
    {
        if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast.LayerBit(), out hitInfo))
        {
            return false;
        }
        return (this.IsMousedOverHero(hitInfo) || (this.IsMousedOverSelf(hitInfo) || this.IsMousedOverEmote(hitInfo)));
    }

    private bool IsMousedOverEmote(RaycastHit cardHitInfo)
    {
        return (cardHitInfo.transform == this.m_squelchEmote.transform);
    }

    private bool IsMousedOverHero(RaycastHit cardHitInfo)
    {
        Actor actor = SceneUtils.FindComponentInParents<Actor>(cardHitInfo.transform);
        if (actor == null)
        {
            return false;
        }
        Card card = actor.GetCard();
        if (card == null)
        {
            return false;
        }
        return card.GetEntity().IsHero();
    }

    private bool IsMousedOverSelf(RaycastHit cardHitInfo)
    {
        return (base.GetComponent<Collider>() == cardHitInfo.collider);
    }

    public bool IsMouseOverEmoteOption()
    {
        RaycastHit hit;
        return (UniversalInputManager.Get().GetInputHitInfo(GameLayer.Default.LayerBit(), out hit) && (hit.transform.gameObject == this.m_squelchEmote));
    }

    public bool IsSquelched()
    {
        return this.m_squelched;
    }

    private void MouseOutSquelch()
    {
        object[] args = new object[] { "scale", this.m_squelchEmoteStartingScale, "time", 0.2f };
        iTween.ScaleTo(this.m_squelchEmote, iTween.Hash(args));
    }

    private void MouseOverSquelch()
    {
        object[] args = new object[] { "scale", (Vector3) (this.m_squelchEmoteStartingScale * 1.1f), "time", 0.2f };
        iTween.ScaleTo(this.m_squelchEmote, iTween.Hash(args));
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    public void ShowEmotes()
    {
        if (!this.emotesDisplayed)
        {
            this.emotesDisplayed = true;
            base.GetComponent<Collider>().enabled = true;
            this.shownAt = UnityEngine.Time.frameCount;
            if (this.m_squelched)
            {
                this.m_squelchEmoteDisplayText.Text = GameStrings.Get("GAMEPLAY_EMOTE_UNSQUELCH");
            }
            else
            {
                this.m_squelchEmoteDisplayText.Text = GameStrings.Get("GAMEPLAY_EMOTE_SQUELCH");
            }
            this.m_squelchEmoteBackplate.enabled = true;
            this.m_squelchEmoteDisplayText.gameObject.SetActive(true);
            this.m_squelchEmote.GetComponent<Collider>().enabled = true;
            iTween.Stop(this.m_squelchEmote);
            object[] args = new object[] { "scale", this.m_squelchEmoteStartingScale, "time", 0.5f, "easetype", iTween.EaseType.easeOutElastic };
            iTween.ScaleTo(this.m_squelchEmote, iTween.Hash(args));
        }
    }
}

