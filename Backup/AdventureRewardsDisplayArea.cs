using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class AdventureRewardsDisplayArea : MonoBehaviour
{
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_CardPreviewAppearSound;
    private List<Actor> m_CurrentCardRewards = new List<Actor>();
    [CustomEditField(Sections="UI")]
    public bool m_EnableFullscreenMode;
    [CustomEditField(Sections="UI", Parent="m_EnableFullscreenMode")]
    public UIBScrollable m_FullscreenDisableScrollBar;
    private bool m_FullscreenEnabled;
    [CustomEditField(Sections="UI", Parent="m_EnableFullscreenMode")]
    public PegUIElement m_FullscreenModeOffClicker;
    [CustomEditField(Sections="UI")]
    public GameObject m_RewardsCardArea;
    [CustomEditField(Sections="UI")]
    public Vector3 m_RewardsCardDriftAmount;
    [CustomEditField(Sections="UI")]
    public float m_RewardsCardMouseOffset;
    [CustomEditField(Sections="UI")]
    public Vector3 m_RewardsCardOffset;
    [CustomEditField(Sections="UI")]
    public Vector3 m_RewardsCardScale;
    [CustomEditField(Sections="UI")]
    public float m_RewardsCardSpacing = 10f;
    private bool m_Showing;

    private void Awake()
    {
        if (this.m_FullscreenModeOffClicker != null)
        {
            this.m_FullscreenModeOffClicker.AddEventListener(UIEventType.PRESS, e => this.HideCardRewards());
        }
        if (this.m_FullscreenDisableScrollBar != null)
        {
            this.m_FullscreenDisableScrollBar.AddTouchScrollStartedListener(new UIBScrollable.OnTouchScrollStarted(this.HideCardRewards));
        }
    }

    private void DisableFullscreen()
    {
        if (this.m_FullscreenEnabled)
        {
            if (FullScreenFXMgr.Get() != null)
            {
                FullScreenFXMgr.Get().EndStandardBlurVignette(0.25f, null);
            }
            if (this.m_FullscreenModeOffClicker != null)
            {
                this.m_FullscreenModeOffClicker.gameObject.SetActive(false);
            }
            this.m_FullscreenEnabled = false;
        }
    }

    private void DoShowCardRewards(List<string> cardIds, Vector3? finalPosition, Vector3? origin, bool disableFullscreen)
    {
        int index = 0;
        int count = cardIds.Count;
        foreach (string str in cardIds)
        {
            FullDef fullDef = DefLoader.Get().GetFullDef(str, null);
            GameObject obj2 = AssetLoader.Get().LoadActor(ActorNames.GetHandActor(fullDef.GetEntityDef(), TAG_PREMIUM.NORMAL), false, false);
            Actor component = obj2.GetComponent<Actor>();
            component.SetCardDef(fullDef.GetCardDef());
            component.SetEntityDef(fullDef.GetEntityDef());
            if (component.m_cardMesh != null)
            {
                BoxCollider collider = component.m_cardMesh.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
            this.m_CurrentCardRewards.Add(component);
            GameUtils.SetParent(component, this.m_RewardsCardArea, false);
            this.ShowCardRewardsObject(obj2, finalPosition, origin, index, count);
            index++;
        }
        this.EnableFullscreen(disableFullscreen);
    }

    private void EnableFullscreen(bool disableFullscreen)
    {
        if (this.m_EnableFullscreenMode && !disableFullscreen)
        {
            FullScreenFXMgr.Get().StartStandardBlurVignette(0.25f);
            if (this.m_FullscreenModeOffClicker != null)
            {
                this.m_FullscreenModeOffClicker.gameObject.SetActive(true);
            }
            this.m_FullscreenEnabled = true;
        }
    }

    public void HideCardRewards()
    {
        this.m_Showing = false;
        foreach (Actor actor in this.m_CurrentCardRewards)
        {
            if (actor != null)
            {
                UnityEngine.Object.Destroy(actor.gameObject);
            }
        }
        this.m_CurrentCardRewards.Clear();
        this.DisableFullscreen();
    }

    public bool IsShowing()
    {
        return this.m_Showing;
    }

    private void OnDestroy()
    {
        this.DisableFullscreen();
    }

    private void ShowCardRewardsObject(GameObject obj, Vector3? finalPosition, Vector3? origin, int index, int totalCount)
    {
        Vector3 vector;
        <ShowCardRewardsObject>c__AnonStorey2A0 storeya = new <ShowCardRewardsObject>c__AnonStorey2A0 {
            obj = obj,
            <>f__this = this
        };
        if (finalPosition.HasValue)
        {
            Vector3 min = this.m_RewardsCardArea.GetComponent<Collider>().bounds.min;
            Vector3 max = this.m_RewardsCardArea.GetComponent<Collider>().bounds.max;
            vector = finalPosition.Value + this.m_RewardsCardOffset;
            float num = index * this.m_RewardsCardSpacing;
            vector.z = Mathf.Clamp(vector.z, min.z, max.z);
            if ((vector.x + this.m_RewardsCardMouseOffset) > max.x)
            {
                vector.x -= this.m_RewardsCardMouseOffset + num;
            }
            else
            {
                vector.x += this.m_RewardsCardMouseOffset + num;
            }
        }
        else
        {
            vector = this.m_RewardsCardArea.transform.position + this.m_RewardsCardOffset;
            float num2 = index * this.m_RewardsCardSpacing;
            vector.x += num2;
            vector.x -= ((totalCount - 1) * this.m_RewardsCardSpacing) * 0.5f;
        }
        storeya.obj.transform.localScale = this.m_RewardsCardScale;
        storeya.obj.transform.position = vector;
        storeya.obj.SetActive(true);
        if (this.m_EnableFullscreenMode)
        {
            SceneUtils.SetLayer(storeya.obj, GameLayer.IgnoreFullScreenEffects);
            if (this.m_FullscreenModeOffClicker != null)
            {
                SceneUtils.SetLayer(this.m_FullscreenModeOffClicker, GameLayer.IgnoreFullScreenEffects);
            }
        }
        iTween.StopByName(storeya.obj, "REWARD_SCALE_UP");
        object[] args = new object[] { "scale", (Vector3) (Vector3.one * 0.05f), "time", 0.15f, "easeType", iTween.EaseType.easeOutQuart, "name", "REWARD_SCALE_UP" };
        iTween.ScaleFrom(storeya.obj, iTween.Hash(args));
        if (origin.HasValue)
        {
            iTween.StopByName(storeya.obj, "REWARD_MOVE_FROM_ORIGIN");
            object[] objArray2 = new object[] { "position", origin.Value, "time", 0.15f, "easeType", iTween.EaseType.easeOutQuart, "name", "REWARD_MOVE_FROM_ORIGIN", "oncomplete", new Action<object>(storeya.<>m__2C) };
            iTween.MoveFrom(storeya.obj, iTween.Hash(objArray2));
        }
        else if (this.m_RewardsCardDriftAmount != Vector3.zero)
        {
            AnimationUtil.DriftObject(storeya.obj, this.m_RewardsCardDriftAmount);
        }
        if (!string.IsNullOrEmpty(this.m_CardPreviewAppearSound))
        {
            string soundName = FileUtils.GameAssetPathToName(this.m_CardPreviewAppearSound);
            SoundManager.Get().LoadAndPlay(soundName);
        }
    }

    public void ShowCards(List<CardRewardData> rewards, Vector3 finalPosition, Vector3? origin = new Vector3?())
    {
        List<string> cardIds = new List<string>();
        foreach (CardRewardData data in rewards)
        {
            cardIds.Add(data.CardID);
        }
        this.ShowCards(cardIds, finalPosition, origin);
    }

    public void ShowCards(List<string> cardIds, Vector3 finalPosition, Vector3? origin = new Vector3?())
    {
        if (!this.m_Showing)
        {
            this.m_Showing = true;
            if (this.m_EnableFullscreenMode)
            {
                this.DoShowCardRewards(cardIds, null, origin, false);
            }
            else
            {
                this.DoShowCardRewards(cardIds, new Vector3?(finalPosition), origin, false);
            }
        }
    }

    public void ShowCardsNoFullscreen(List<CardRewardData> rewards, Vector3 finalPosition, Vector3? origin = new Vector3?())
    {
        List<string> cardIds = new List<string>();
        foreach (CardRewardData data in rewards)
        {
            cardIds.Add(data.CardID);
        }
        this.ShowCardsNoFullscreen(cardIds, finalPosition, origin);
    }

    public void ShowCardsNoFullscreen(List<string> cardIds, Vector3 finalPosition, Vector3? origin = new Vector3?())
    {
        this.DoShowCardRewards(cardIds, new Vector3?(finalPosition), origin, true);
    }

    [CompilerGenerated]
    private sealed class <ShowCardRewardsObject>c__AnonStorey2A0
    {
        internal AdventureRewardsDisplayArea <>f__this;
        internal GameObject obj;

        internal void <>m__2C(object o)
        {
            if (this.<>f__this.m_RewardsCardDriftAmount != Vector3.zero)
            {
                AnimationUtil.DriftObject(this.obj, this.<>f__this.m_RewardsCardDriftAmount);
            }
        }
    }
}

