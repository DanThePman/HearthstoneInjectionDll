using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class QuestToast : MonoBehaviour
{
    private static QuestToast m_activeQuest;
    public PegUIElement m_clickCatcher;
    public GameObject m_nameLine;
    private DelOnCloseQuestToast m_onCloseCallback;
    private object m_onCloseCallbackData;
    private static bool m_questActive;
    public UberText m_questName;
    public UberText m_requirement;
    public Transform m_rewardBone;
    private static bool m_showFullscreenEffects = true;
    private string m_toastDescription = string.Empty;
    private string m_toastName = string.Empty;
    private List<RewardData> m_toastRewards;
    private static readonly Vector3 REWARD_SCALE = new Vector3(1.42f, 1.42f, 1.42f);

    public void Awake()
    {
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
    }

    public void CloseQuestToast()
    {
        if (base.gameObject != null)
        {
            m_questActive = false;
            this.m_clickCatcher.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.CloseQuestToast));
            this.FadeEffectsOut();
            object[] args = new object[] { "scale", Vector3.zero, "time", 0.5f, "oncompletetarget", base.gameObject, "oncomplete", "DestroyQuestToast" };
            iTween.ScaleTo(base.gameObject, iTween.Hash(args));
            if (this.m_onCloseCallback != null)
            {
                this.m_onCloseCallback(this.m_onCloseCallbackData);
            }
        }
    }

    private void CloseQuestToast(UIEvent e)
    {
        this.CloseQuestToast();
    }

    private void DestroyQuestToast()
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void FadeEffectsIn()
    {
        if (m_showFullscreenEffects)
        {
            FullScreenFXMgr mgr = FullScreenFXMgr.Get();
            mgr.SetBlurBrightness(1f);
            mgr.SetBlurDesaturation(0f);
            mgr.Vignette(0.4f, 0.4f, iTween.EaseType.easeOutCirc, null);
            mgr.Blur(1f, 0.4f, iTween.EaseType.easeOutCirc, null);
        }
    }

    private void FadeEffectsOut()
    {
        if (m_showFullscreenEffects)
        {
            FullScreenFXMgr mgr = FullScreenFXMgr.Get();
            mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, null);
            mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
        }
    }

    public static QuestToast GetCurrentToast()
    {
        return m_activeQuest;
    }

    public static bool IsQuestActive()
    {
        return (m_questActive && (m_activeQuest != null));
    }

    public void OnDestroy()
    {
        if (this == m_activeQuest)
        {
            if (m_questActive)
            {
                this.FadeEffectsOut();
                m_questActive = false;
            }
            m_activeQuest = null;
        }
    }

    private static void PositionActor(string actorName, GameObject actorObject, object c)
    {
        actorObject.transform.localPosition = new Vector3(6f, 5f, 3f);
        Vector3 localScale = actorObject.transform.localScale;
        actorObject.transform.localScale = (Vector3) (0.01f * Vector3.one);
        actorObject.SetActive(true);
        iTween.ScaleTo(actorObject, localScale, 0.5f);
        QuestToast component = actorObject.GetComponent<QuestToast>();
        if (component == null)
        {
            Debug.LogWarning("QuestToast.PositionActor(): actor has no QuestToast component");
            m_questActive = false;
        }
        else
        {
            m_activeQuest = component;
            ToastCallbackData data = c as ToastCallbackData;
            component.m_onCloseCallback = data.m_onCloseCallback;
            component.m_toastRewards = data.m_toastRewards;
            component.m_toastName = data.m_toastName;
            component.m_toastDescription = data.m_toastDescription;
            component.SetUpToast(data.m_updateCacheValues);
        }
    }

    private void RewardObjectLoaded(Reward reward, object callbackData)
    {
        bool updateCacheValues = (bool) callbackData;
        reward.Hide(false);
        reward.transform.parent = this.m_rewardBone;
        reward.transform.localEulerAngles = Vector3.zero;
        reward.transform.localScale = REWARD_SCALE;
        reward.transform.localPosition = Vector3.zero;
        BoosterPackReward componentInChildren = reward.gameObject.GetComponentInChildren<BoosterPackReward>();
        if (componentInChildren != null)
        {
            componentInChildren.m_Layer = (GameLayer) base.gameObject.layer;
        }
        SceneUtils.SetLayer(reward.gameObject, base.gameObject.layer);
        reward.Show(updateCacheValues);
    }

    public void SetUpToast(bool updateCacheValues)
    {
        this.m_clickCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.CloseQuestToast));
        this.m_questName.Text = this.m_toastName;
        this.m_requirement.Text = this.m_toastDescription;
        RewardData data = null;
        if (this.m_toastRewards != null)
        {
            foreach (RewardData data2 in this.m_toastRewards)
            {
                data = data2;
                break;
            }
        }
        if (data != null)
        {
            data.LoadRewardObject(new Reward.DelOnRewardLoaded(this.RewardObjectLoaded), updateCacheValues);
        }
        this.FadeEffectsIn();
    }

    public static void ShowFixedRewardQuestToast(DelOnCloseQuestToast onClosedCallback, RewardData rewardData, string name, string description)
    {
        ShowFixedRewardQuestToast(onClosedCallback, null, rewardData, name, description, true);
    }

    public static void ShowFixedRewardQuestToast(DelOnCloseQuestToast onClosedCallback, RewardData rewardData, string name, string description, bool fullscreenEffects)
    {
        ShowFixedRewardQuestToast(onClosedCallback, null, rewardData, name, description, fullscreenEffects);
    }

    public static void ShowFixedRewardQuestToast(DelOnCloseQuestToast onClosedCallback, object callbackUserData, RewardData rewardData, string name, string description)
    {
        ShowFixedRewardQuestToast(onClosedCallback, null, rewardData, name, description, true);
    }

    public static void ShowFixedRewardQuestToast(DelOnCloseQuestToast onClosedCallback, object callbackUserData, RewardData rewardData, string name, string description, bool fullscreenEffects)
    {
        m_showFullscreenEffects = fullscreenEffects;
        m_questActive = true;
        List<RewardData> list = new List<RewardData> {
            rewardData
        };
        ToastCallbackData callbackData = new ToastCallbackData {
            m_toastRewards = list,
            m_toastName = name,
            m_toastDescription = description,
            m_onCloseCallback = onClosedCallback,
            m_onCloseCallbackData = callbackUserData,
            m_updateCacheValues = true
        };
        AssetLoader.Get().LoadActor("QuestToast", true, new AssetLoader.GameObjectCallback(QuestToast.PositionActor), callbackData, false);
        SoundManager.Get().LoadAndPlay("Quest_Complete_Jingle");
        SoundManager.Get().LoadAndPlay("quest_complete_pop_up");
        SoundManager.Get().LoadAndPlay("tavern_crowd_play_reaction_positive_random");
    }

    public static void ShowQuestToast(DelOnCloseQuestToast onClosedCallback, bool updateCacheValues, Achievement quest)
    {
        ShowQuestToast(onClosedCallback, updateCacheValues, quest, true);
    }

    public static void ShowQuestToast(DelOnCloseQuestToast onClosedCallback, bool updateCacheValues, Achievement quest, bool fullScreenEffects)
    {
        ShowQuestToast(onClosedCallback, null, updateCacheValues, quest, fullScreenEffects);
    }

    public static void ShowQuestToast(DelOnCloseQuestToast onClosedCallback, object callbackUserData, bool updateCacheValues, Achievement quest)
    {
        ShowQuestToast(onClosedCallback, callbackUserData, updateCacheValues, quest, true);
    }

    public static void ShowQuestToast(DelOnCloseQuestToast onClosedCallback, object callbackUserData, bool updateCacheValues, Achievement quest, bool fullscreenEffects)
    {
        quest.AckCurrentProgressAndRewardNotices();
        if (quest.ID != 0x38)
        {
            m_showFullscreenEffects = fullscreenEffects;
            m_questActive = true;
            ToastCallbackData callbackData = new ToastCallbackData {
                m_toastRewards = quest.Rewards,
                m_toastName = quest.Name,
                m_toastDescription = quest.Description,
                m_onCloseCallback = onClosedCallback,
                m_onCloseCallbackData = callbackUserData,
                m_updateCacheValues = updateCacheValues
            };
            AssetLoader.Get().LoadActor("QuestToast", true, new AssetLoader.GameObjectCallback(QuestToast.PositionActor), callbackData, false);
            SoundManager.Get().LoadAndPlay("Quest_Complete_Jingle");
            SoundManager.Get().LoadAndPlay("quest_complete_pop_up");
            SoundManager.Get().LoadAndPlay("tavern_crowd_play_reaction_positive_random");
        }
    }

    public delegate void DelOnCloseQuestToast(object userData);

    private class ToastCallbackData
    {
        public QuestToast.DelOnCloseQuestToast m_onCloseCallback;
        public object m_onCloseCallbackData;
        public string m_toastDescription = string.Empty;
        public string m_toastName = string.Empty;
        public List<RewardData> m_toastRewards;
        public bool m_updateCacheValues;
    }
}

