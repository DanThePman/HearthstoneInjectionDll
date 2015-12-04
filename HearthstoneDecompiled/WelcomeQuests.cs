using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class WelcomeQuests : MonoBehaviour
{
    public UberText m_allCompletedCaption;
    public Animation m_bannerFX;
    public PegUIElement m_clickCatcher;
    private List<QuestTile> m_currentQuests;
    public Banner m_headlineBanner;
    private Vector3 m_originalScale;
    public Collider m_placementCollider;
    public UberText m_questCaption;
    public QuestTile m_questTilePrefab;
    public GameObject m_Root;
    private ShowRequestData m_showRequestData;
    private static WelcomeQuests s_instance;

    private void Awake()
    {
        this.m_originalScale = base.transform.localScale;
        this.m_headlineBanner.gameObject.SetActive(false);
        this.m_clickCatcher.gameObject.SetActive(false);
        this.m_allCompletedCaption.gameObject.SetActive(false);
        SoundManager.Get().Load("new_quest_pop_up");
        SoundManager.Get().Load("existing_quest_pop_up");
        SoundManager.Get().Load("new_quest_click_and_shrink");
    }

    private void Close()
    {
        Navigation.PopUnique(new Navigation.NavigateBackHandler(WelcomeQuests.OnNavigateBack));
        s_instance = null;
        this.m_clickCatcher.RemoveEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.CloseWelcomeQuests));
        this.FadeEffectsOut();
        object[] args = new object[] { "scale", Vector3.zero, "time", 0.5f, "oncompletetarget", base.gameObject, "oncomplete", "DestroyWelcomeQuests" };
        iTween.ScaleTo(base.gameObject, iTween.Hash(args));
        SoundManager.Get().LoadAndPlay("new_quest_click_and_shrink");
        this.m_bannerFX.Play("BannerClose");
        GameToastMgr.Get().UpdateQuestProgressToasts();
        GameToastMgr.Get().AddSeasonTimeRemainingToast();
        if (this.m_showRequestData != null)
        {
            if (!this.m_showRequestData.m_keepRichPresence)
            {
                PresenceMgr.Get().SetPrevStatus();
            }
            if (this.m_showRequestData.m_onCloseCallback != null)
            {
                this.m_showRequestData.m_onCloseCallback();
            }
        }
        InnKeepersSpecial.Get().Show(false);
    }

    private void CloseWelcomeQuests(UIEvent e)
    {
        Navigation.GoBack();
    }

    private void DestroyWelcomeQuests()
    {
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void DoInnkeeperLine(Achievement quest)
    {
        if (quest.ID == 11)
        {
        }
    }

    private void FadeEffectsIn()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr != null)
        {
            mgr.SetBlurBrightness(1f);
            mgr.SetBlurDesaturation(0f);
            mgr.Vignette(0.4f, 0.4f, iTween.EaseType.easeOutCirc, null);
            mgr.Blur(1f, 0.4f, iTween.EaseType.easeOutCirc, null);
        }
    }

    private void FadeEffectsOut()
    {
        FullScreenFXMgr mgr = FullScreenFXMgr.Get();
        if (mgr != null)
        {
            mgr.StopVignette(0.2f, iTween.EaseType.easeOutCirc, null);
            mgr.StopBlur(0.2f, iTween.EaseType.easeOutCirc, null);
        }
    }

    private void FlipQuest(QuestTile quest)
    {
        object[] args = new object[] { "amount", new Vector3(0f, 0f, 540f), "delay", 1, "time", 2f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self };
        Hashtable hashtable = iTween.Hash(args);
        iTween.RotateAdd(quest.gameObject, hashtable);
    }

    public static WelcomeQuests Get()
    {
        return s_instance;
    }

    public QuestTile GetFirstQuestTile()
    {
        return this.m_currentQuests[0];
    }

    public static void Hide()
    {
        if (s_instance != null)
        {
            s_instance.Close();
        }
    }

    private void InitAndShow(ShowRequestData showRequestData)
    {
        OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, false, CanvasScaleMode.HEIGHT);
        this.m_showRequestData = showRequestData;
        if ((AchieveManager.Get().GetActiveQuests(false).Count < 1) && (!InnKeepersSpecial.Get().LoadedSuccessfully() || InnKeepersSpecial.Get().HasAlreadySeenResponse()))
        {
            this.Close();
        }
        else
        {
            this.m_clickCatcher.gameObject.SetActive(true);
            this.m_clickCatcher.AddEventListener(UIEventType.RELEASE, new UIEvent.Handler(this.CloseWelcomeQuests));
            this.ShowActiveQuests();
            this.FadeEffectsIn();
            base.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            iTween.ScaleTo(base.gameObject, this.m_originalScale, 0.5f);
            int val = Options.Get().GetInt(Option.IKS_VIEWS, 0) + 1;
            Options.Get().SetInt(Option.IKS_VIEWS, val);
            if ((showRequestData.m_fromLogin && InnKeepersSpecial.Get().LoadedSuccessfully()) && (val > 3))
            {
                if (UniversalInputManager.UsePhoneUI != null)
                {
                    Vector3 localPosition = base.transform.localPosition;
                    localPosition.y += 2f;
                    base.transform.localPosition = localPosition;
                }
                InnKeepersSpecial.Get().Show(true);
            }
            Navigation.PushUnique(new Navigation.NavigateBackHandler(WelcomeQuests.OnNavigateBack));
        }
    }

    private void OnDestroy()
    {
        if (s_instance != null)
        {
            Navigation.PopUnique(new Navigation.NavigateBackHandler(WelcomeQuests.OnNavigateBack));
            s_instance = null;
            this.FadeEffectsOut();
            InnKeepersSpecial.Get().Show(false);
        }
    }

    public static bool OnNavigateBack()
    {
        if (s_instance != null)
        {
            s_instance.Close();
        }
        return true;
    }

    private static void OnWelcomeQuestsLoaded(string name, GameObject go, object callbackData)
    {
        if ((SceneMgr.Get() != null) && SceneMgr.Get().IsInGame())
        {
            if (s_instance != null)
            {
                s_instance.Close();
            }
        }
        else if (go == null)
        {
            Debug.LogError(string.Format("WelcomeQuests.OnWelcomeQuestsLoaded() - FAILED to load \"{0}\"", name));
        }
        else
        {
            s_instance = go.GetComponent<WelcomeQuests>();
            if (s_instance == null)
            {
                Debug.LogError(string.Format("WelcomeQuests.OnWelcomeQuestsLoaded() - ERROR object \"{0}\" has no WelcomeQuests component", name));
            }
            else
            {
                ShowRequestData showRequestData = callbackData as ShowRequestData;
                s_instance.InitAndShow(showRequestData);
            }
        }
    }

    public static void Show(bool fromLogin, DelOnWelcomeQuestsClosed onCloseCallback = null, bool keepRichPresence = false)
    {
        Enum[] args = new Enum[] { PresenceStatus.WELCOMEQUESTS };
        PresenceMgr.Get().SetStatus(args);
        ShowRequestData showRequestData = new ShowRequestData {
            m_fromLogin = fromLogin,
            m_onCloseCallback = onCloseCallback,
            m_keepRichPresence = keepRichPresence
        };
        if (s_instance != null)
        {
            Debug.LogWarning("WelcomeQuests.Show(): requested to show welcome quests while it was already active!");
            s_instance.InitAndShow(showRequestData);
        }
        else
        {
            AssetLoader.Get().LoadGameObject("WelcomeQuests", new AssetLoader.GameObjectCallback(WelcomeQuests.OnWelcomeQuestsLoaded), showRequestData, false);
        }
    }

    private void ShowActiveQuests()
    {
        List<Achievement> activeQuests = AchieveManager.Get().GetActiveQuests(false);
        if (activeQuests.Count < 1)
        {
            this.m_allCompletedCaption.gameObject.SetActive(true);
        }
        else
        {
            this.m_headlineBanner.gameObject.SetActive(true);
            if (this.m_showRequestData.m_fromLogin)
            {
                this.m_headlineBanner.SetText(GameStrings.Get("GLUE_QUEST_NOTIFICATION_HEADER"));
            }
            else
            {
                this.m_headlineBanner.SetText(GameStrings.Get("GLUE_QUEST_NOTIFICATION_HEADER_NEW_ONLY"));
            }
            if (AchieveManager.Get().HasUnlockedFeature(Achievement.UnlockableFeature.DAILY_QUESTS))
            {
                this.m_questCaption.Text = GameStrings.Get("GLUE_QUEST_NOTIFICATION_CAPTION");
            }
            else
            {
                this.m_questCaption.Text = string.Empty;
            }
            this.m_currentQuests = new List<QuestTile>();
            float x = 0.4808684f;
            float num2 = this.m_placementCollider.transform.position.x - this.m_placementCollider.GetComponent<Collider>().bounds.extents.x;
            float num3 = this.m_placementCollider.bounds.size.x / ((float) activeQuests.Count);
            float num4 = num3 / 2f;
            bool flag = false;
            for (int i = 0; i < activeQuests.Count; i++)
            {
                Achievement quest = activeQuests[i];
                bool flag2 = quest.IsNewlyActive();
                float y = 180f;
                if (flag2)
                {
                    y = 0f;
                    this.DoInnkeeperLine(quest);
                }
                GameObject go = UnityEngine.Object.Instantiate<GameObject>(this.m_questTilePrefab.gameObject);
                SceneUtils.SetLayer(go, GameLayer.UI);
                go.transform.position = new Vector3(num2 + num4, this.m_placementCollider.transform.position.y, this.m_placementCollider.transform.position.z);
                go.transform.parent = base.transform;
                go.transform.localEulerAngles = new Vector3(90f, y, 0f);
                go.transform.localScale = new Vector3(x, x, x);
                QuestTile component = go.GetComponent<QuestTile>();
                component.SetupTile(quest);
                this.m_currentQuests.Add(component);
                num4 += num3;
                if (flag2)
                {
                    flag = true;
                    this.FlipQuest(component);
                }
            }
            if (flag)
            {
                SoundManager.Get().LoadAndPlay("new_quest_pop_up");
            }
            else
            {
                SoundManager.Get().LoadAndPlay("existing_quest_pop_up");
            }
        }
    }

    public delegate void DelOnWelcomeQuestsClosed();

    private class ShowRequestData
    {
        public bool m_fromLogin;
        public bool m_keepRichPresence;
        public WelcomeQuests.DelOnWelcomeQuestsClosed m_onCloseCallback;
    }
}

