using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

[CustomEditClass]
public class BigCard : MonoBehaviour
{
    [CompilerGenerated]
    private static Action<object> <>f__am$cache12;
    private readonly PlatformDependentValue<float> ENCHANTMENT_SCALING_FACTOR;
    private static readonly Vector3 INVISIBLE_SCALE = new Vector3(0.0001f, 0.0001f, 0.0001f);
    private Actor m_bigCardActor;
    private Card m_card;
    public GameObject m_EnchantmentBanner;
    public GameObject m_EnchantmentBannerBottom;
    public BigCardEnchantmentPanel m_EnchantmentPanelPrefab;
    private Pool<BigCardEnchantmentPanel> m_enchantmentPool = new Pool<BigCardEnchantmentPanel>();
    private Vector3 m_initialBannerBottomScale;
    private float m_initialBannerHeight;
    private Vector3 m_initialBannerScale;
    public LayoutData m_LayoutData;
    private List<Actor> m_phoneSecretActors;
    public int m_RenderQueueEnchantmentBanner = 1;
    public int m_RenderQueueEnchantmentPanel = 2;
    public SecretLayoutData m_SecretLayoutData;
    private readonly PlatformDependentValue<float> PLATFORM_SCALING_FACTOR;
    private static BigCard s_instance;

    public BigCard()
    {
        PlatformDependentValue<float> value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 1f,
            Tablet = 1f,
            Phone = 1.3f,
            MiniTablet = 1f
        };
        this.PLATFORM_SCALING_FACTOR = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 1f,
            Tablet = 1f,
            Phone = 1.5f,
            MiniTablet = 1f
        };
        this.ENCHANTMENT_SCALING_FACTOR = value2;
    }

    private void Awake()
    {
        s_instance = this;
        this.m_initialBannerHeight = this.m_EnchantmentBanner.GetComponent<Renderer>().bounds.size.z;
        this.m_initialBannerScale = this.m_EnchantmentBanner.transform.localScale;
        this.m_initialBannerBottomScale = this.m_EnchantmentBannerBottom.transform.localScale;
        this.m_enchantmentPool.SetCreateItemCallback(new Pool<BigCardEnchantmentPanel>.CreateItemCallback(this.CreateEnchantmentPanel));
        this.m_enchantmentPool.SetDestroyItemCallback(new Pool<BigCardEnchantmentPanel>.DestroyItemCallback(this.DestroyEnchantmentPanel));
        this.m_enchantmentPool.SetExtensionCount(1);
        this.m_enchantmentPool.SetMaxReleasedItemCount(2);
        this.ResetEnchantments();
    }

    private BigCardEnchantmentPanel CreateEnchantmentPanel(int index)
    {
        BigCardEnchantmentPanel panel = UnityEngine.Object.Instantiate<BigCardEnchantmentPanel>(this.m_EnchantmentPanelPrefab);
        panel.name = string.Format("{0}{1}", typeof(BigCardEnchantmentPanel).ToString(), index);
        SceneUtils.SetRenderQueue(panel.gameObject, this.m_RenderQueueEnchantmentPanel);
        return panel;
    }

    private void DestroyEnchantmentPanel(BigCardEnchantmentPanel panel)
    {
        UnityEngine.Object.Destroy(panel.gameObject);
    }

    private void DetermineSecretLayoutOffsets(Card mainCard, List<Actor> actors, out Vector3 initialOffset, out Vector3 spacing, out Vector3 drift)
    {
        Player controller = mainCard.GetController();
        bool flag = controller.IsFriendlySide();
        bool flag2 = controller.IsRevealed();
        int minCardThreshold = this.m_SecretLayoutData.m_MinCardThreshold;
        int maxCardThreshold = this.m_SecretLayoutData.m_MaxCardThreshold;
        SecretLayoutOffsets minCardOffsets = this.m_SecretLayoutData.m_MinCardOffsets;
        SecretLayoutOffsets maxCardOffsets = this.m_SecretLayoutData.m_MaxCardOffsets;
        float t = Mathf.InverseLerp((float) minCardThreshold, (float) maxCardThreshold, (float) actors.Count);
        if (flag2)
        {
            if (flag)
            {
                initialOffset = Vector3.Lerp(minCardOffsets.m_InitialOffset, maxCardOffsets.m_InitialOffset, t);
            }
            else
            {
                initialOffset = Vector3.Lerp(minCardOffsets.m_OpponentInitialOffset, maxCardOffsets.m_OpponentInitialOffset, t);
            }
            spacing = this.m_SecretLayoutData.m_Spacing;
        }
        else
        {
            if (flag)
            {
                initialOffset = Vector3.Lerp(minCardOffsets.m_HiddenInitialOffset, maxCardOffsets.m_HiddenInitialOffset, t);
            }
            else
            {
                initialOffset = Vector3.Lerp(minCardOffsets.m_HiddenOpponentInitialOffset, maxCardOffsets.m_HiddenOpponentInitialOffset, t);
            }
            spacing = this.m_SecretLayoutData.m_HiddenSpacing;
        }
        if (flag)
        {
            spacing.z = -spacing.z;
            drift = this.m_SecretLayoutData.m_DriftOffset;
        }
        else
        {
            drift = -this.m_SecretLayoutData.m_DriftOffset;
        }
    }

    private void DisplayBigCard()
    {
        Entity entity = this.m_card.GetEntity();
        bool flag = entity.GetController().IsFriendlySide();
        Zone zone = this.m_card.GetZone();
        Bounds bounds = this.m_bigCardActor.GetMeshRenderer().bounds;
        Vector3 position = this.m_card.GetActor().transform.position;
        Vector3 vector2 = new Vector3(0f, 0f, 0f);
        Vector3 scale = new Vector3(1.1f, 1.1f, 1.1f);
        float? overrideScale = null;
        if (zone is ZoneHero)
        {
            if (flag)
            {
                vector2 = new Vector3(0f, 4f, 0f);
            }
            else
            {
                vector2 = new Vector3(0f, 4f, -bounds.size.z * 0.7f);
            }
        }
        else if (zone is ZoneHeroPower)
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                scale = new Vector3(1.3f, 1f, 1.3f);
                if (flag)
                {
                    vector2 = new Vector3(-3.5f, 8f, 3.5f);
                }
                else
                {
                    vector2 = new Vector3(-3.5f, 8f, -3.35f);
                }
            }
            else if (flag)
            {
                vector2 = new Vector3(0f, 4f, 2.69f);
            }
            else
            {
                vector2 = new Vector3(0f, 4f, -2.6f);
            }
            overrideScale = 0.6f;
        }
        else if (zone is ZoneWeapon)
        {
            scale = new Vector3(1.65f, 1.65f, 1.65f);
            if (flag)
            {
                vector2 = new Vector3(0f, 0f, bounds.size.z * 0.9f);
            }
            else
            {
                vector2 = new Vector3(-1.57f, 0f, -1f);
            }
            scale *= this.PLATFORM_SCALING_FACTOR;
        }
        else if (zone is ZoneSecret)
        {
            scale = new Vector3(1.65f, 1.65f, 1.65f);
            vector2 = new Vector3(bounds.size.x + 0.3f, 0f, 0f);
        }
        else if (zone is ZoneHand)
        {
            vector2 = new Vector3(bounds.size.x * 0.7f, 4f, -bounds.size.z * 0.8f);
            scale = new Vector3(1.65f, 1.65f, 1.65f);
        }
        else
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                scale = new Vector3(2f, 2f, 2f);
                if (this.ShowBigCardOnRight())
                {
                    vector2 = new Vector3(bounds.size.x + 2.2f, 0f, 0f);
                }
                else
                {
                    vector2 = new Vector3(-bounds.size.x - 2.2f, 0f, 0f);
                }
            }
            else
            {
                scale = new Vector3(1.65f, 1.65f, 1.65f);
                if (this.ShowBigCardOnRight())
                {
                    vector2 = new Vector3(bounds.size.x + 0.7f, 0f, 0f);
                }
                else
                {
                    vector2 = new Vector3(-bounds.size.x - 0.7f, 0f, 0f);
                }
            }
            if (zone is ZonePlay)
            {
                vector2 += new Vector3(0f, 0.1f, 0f);
                scale *= this.PLATFORM_SCALING_FACTOR;
            }
        }
        Vector3 vector4 = new Vector3(0.02f, 0.02f, 0.02f);
        Vector3 vector5 = position + vector2;
        Vector3 vector6 = vector5 + vector4;
        Vector3 vector7 = new Vector3(1f, 1f, 1f);
        Transform parent = this.m_bigCardActor.transform.parent;
        this.m_bigCardActor.transform.localScale = scale;
        this.m_bigCardActor.transform.position = vector6;
        this.m_bigCardActor.transform.parent = null;
        if (zone is ZoneHand)
        {
            this.m_bigCardActor.SetEntity(entity);
            this.m_bigCardActor.UpdateTextComponents(entity);
        }
        else
        {
            this.UpdateEnchantments();
            if ((UniversalInputManager.UsePhoneUI != null) && this.m_EnchantmentBanner.activeInHierarchy)
            {
                float num = (this.m_enchantmentPool.GetActiveList().Count <= 1) ? 0.85f : 0.75f;
                scale = (Vector3) (scale * num);
                this.m_bigCardActor.transform.localScale = scale;
            }
        }
        this.FitInsideScreen();
        this.m_bigCardActor.transform.parent = parent;
        this.m_bigCardActor.transform.localScale = vector7;
        Vector3 vector8 = this.m_bigCardActor.transform.position;
        Transform transform = this.m_bigCardActor.transform;
        transform.position -= vector4;
        KeywordArgs args = new KeywordArgs {
            card = this.m_card,
            actor = this.m_bigCardActor,
            showOnRight = this.ShowBigCardOnRight()
        };
        if (zone is ZoneHand)
        {
            object[] objArray1 = new object[8];
            objArray1[0] = "scale";
            objArray1[1] = scale;
            objArray1[2] = "time";
            objArray1[3] = this.m_LayoutData.m_ScaleSec;
            objArray1[4] = "oncompleteparams";
            objArray1[5] = args;
            objArray1[6] = "oncomplete";
            if (<>f__am$cache12 == null)
            {
                <>f__am$cache12 = delegate (object obj) {
                    KeywordArgs args = (KeywordArgs) obj;
                    KeywordHelpPanelManager.Get().UpdateKeywordHelp(args.card, args.actor, args.showOnRight, null, null);
                };
            }
            objArray1[7] = <>f__am$cache12;
            Hashtable hashtable = iTween.Hash(objArray1);
            iTween.ScaleTo(this.m_bigCardActor.gameObject, hashtable);
        }
        else
        {
            iTween.ScaleTo(this.m_bigCardActor.gameObject, scale, this.m_LayoutData.m_ScaleSec);
            KeywordHelpPanelManager.Get().UpdateKeywordHelp(args.card, args.actor, args.showOnRight, overrideScale, null);
        }
        iTween.MoveTo(this.m_bigCardActor.gameObject, vector8, this.m_LayoutData.m_DriftSec);
        this.m_bigCardActor.transform.rotation = Quaternion.identity;
        this.m_bigCardActor.Show();
        if (UniversalInputManager.UsePhoneUI != null)
        {
            Vector3? overrideOffset = null;
            if (this.m_card.GetEntity().IsHeroPower())
            {
                overrideOffset = new Vector3(0.6f, 0f, -1.3f);
            }
            KeywordHelpPanelManager.Get().UpdateKeywordHelp(this.m_card, this.m_bigCardActor, this.ShowKeywordOnRight(), overrideScale, overrideOffset);
        }
        if (entity.IsSilenced())
        {
            this.m_bigCardActor.ActivateSpell(SpellType.SILENCE);
        }
    }

    private void DisplayPhoneSecrets(Card mainCard, List<Actor> actors, bool showDeath)
    {
        Vector3 vector;
        Vector3 vector2;
        Vector3 vector3;
        this.DetermineSecretLayoutOffsets(mainCard, actors, out vector, out vector2, out vector3);
        bool flag = GeneralUtils.IsOdd(actors.Count);
        Actor actor = mainCard.GetActor();
        Vector3 vector4 = actor.transform.position + vector;
        for (int i = 0; i < actors.Count; i++)
        {
            Vector3 vector5;
            Actor actor2 = actors[i];
            if ((i == 0) && flag)
            {
                vector5 = vector4;
            }
            else
            {
                bool flag2 = GeneralUtils.IsOdd(i);
                bool flag3 = flag == flag2;
                float num2 = !flag ? Mathf.Floor(0.5f * i) : Mathf.Ceil(0.5f * i);
                float num3 = num2 * vector2.x;
                if (!flag)
                {
                    num3 += 0.5f * vector2.x;
                }
                if (flag3)
                {
                    num3 = -num3;
                }
                float num4 = num2 * vector2.z;
                vector5 = new Vector3(vector4.x + num3, vector4.y, vector4.z + num4);
            }
            actor2.transform.position = actor.transform.position;
            actor2.transform.rotation = actor.transform.rotation;
            actor2.transform.localScale = INVISIBLE_SCALE;
            float time = !showDeath ? this.m_SecretLayoutData.m_ShowAnimTime : this.m_SecretLayoutData.m_DeathShowAnimTime;
            object[] args = new object[] { "position", vector5 - vector3, "time", time, "easeType", iTween.EaseType.easeOutExpo };
            Hashtable hashtable = iTween.Hash(args);
            iTween.MoveTo(actor2.gameObject, hashtable);
            object[] objArray2 = new object[] { "position", vector5, "delay", time, "time", this.m_SecretLayoutData.m_DriftSec, "easeType", iTween.EaseType.easeOutExpo };
            Hashtable hashtable2 = iTween.Hash(objArray2);
            iTween.MoveTo(actor2.gameObject, hashtable2);
            iTween.ScaleTo(actor2.gameObject, base.transform.localScale, time);
            if (showDeath)
            {
                this.ShowPhoneSecretDeath(actor2);
            }
        }
    }

    private void FitInsideScreen()
    {
        this.FitInsideScreenBottom();
        this.FitInsideScreenTop();
    }

    private bool FitInsideScreenBottom()
    {
        Bounds bounds = !this.m_EnchantmentBanner.activeInHierarchy ? this.m_bigCardActor.GetMeshRenderer().bounds : this.m_EnchantmentBannerBottom.GetComponent<Renderer>().bounds;
        Vector3 center = bounds.center;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            center.z -= 0.4f;
        }
        Vector3 origin = new Vector3(center.x, center.y, center.z - bounds.extents.z);
        Ray ray = new Ray(origin, origin - center);
        Plane plane = CameraUtils.CreateBottomPlane(CameraUtils.FindFirstByLayer(GameLayer.Tooltip));
        float enter = 0f;
        if (plane.Raycast(ray, out enter))
        {
            return false;
        }
        if (object.Equals(enter, 0f))
        {
            return false;
        }
        TransformUtil.SetPosZ(this.m_bigCardActor.gameObject, this.m_bigCardActor.transform.position.z - enter);
        return true;
    }

    private bool FitInsideScreenTop()
    {
        Bounds bounds = this.m_bigCardActor.GetMeshRenderer().bounds;
        Vector3 center = bounds.center;
        if (UniversalInputManager.UsePhoneUI != null)
        {
            center.z++;
        }
        Vector3 origin = new Vector3(center.x, center.y, center.z + bounds.extents.z);
        Ray ray = new Ray(origin, origin - center);
        Plane plane = CameraUtils.CreateTopPlane(CameraUtils.FindFirstByLayer(GameLayer.Tooltip));
        float enter = 0f;
        if (plane.Raycast(ray, out enter))
        {
            return false;
        }
        if (object.Equals(enter, 0f))
        {
            return false;
        }
        TransformUtil.SetPosZ(this.m_bigCardActor.gameObject, this.m_bigCardActor.transform.position.z + enter);
        return true;
    }

    public static BigCard Get()
    {
        return s_instance;
    }

    public Card GetCard()
    {
        return this.m_card;
    }

    public void Hide()
    {
        if (GameState.Get() != null)
        {
            GameState.Get().GetGameEntity().NotifyOfCardTooltipDisplayHide(this.m_card);
        }
        this.HideBigCard();
        this.HideTooltipPhoneSecrets();
        this.m_card = null;
    }

    public bool Hide(Card card)
    {
        if (this.m_card != card)
        {
            return false;
        }
        this.Hide();
        return true;
    }

    private void HideBigCard()
    {
        if (this.m_bigCardActor != null)
        {
            this.ResetEnchantments();
            iTween.Stop(this.m_bigCardActor.gameObject);
            this.m_bigCardActor.Destroy();
            this.m_bigCardActor = null;
            KeywordHelpPanelManager.Get().HideKeywordHelp();
        }
    }

    private void HidePhoneSecret(Actor actor)
    {
        <HidePhoneSecret>c__AnonStorey2F1 storeyf = new <HidePhoneSecret>c__AnonStorey2F1 {
            actor = actor
        };
        Actor actor2 = this.m_card.GetActor();
        iTween.MoveTo(storeyf.actor.gameObject, actor2.transform.position, this.m_SecretLayoutData.m_HideAnimTime);
        iTween.ScaleTo(storeyf.actor.gameObject, INVISIBLE_SCALE, this.m_SecretLayoutData.m_HideAnimTime);
        Action<object> action = new Action<object>(storeyf.<>m__ED);
        object[] args = new object[] { "time", this.m_SecretLayoutData.m_HideAnimTime, "oncomplete", action };
        Hashtable hashtable = iTween.Hash(args);
        iTween.Timer(storeyf.actor.gameObject, hashtable);
    }

    private void HideTooltipPhoneSecrets()
    {
        if (this.m_phoneSecretActors != null)
        {
            foreach (Actor actor in this.m_phoneSecretActors)
            {
                this.HidePhoneSecret(actor);
            }
            this.m_phoneSecretActors.Clear();
        }
    }

    private void LayoutEnchantments(GameObject bone)
    {
        float num2 = 0.1f;
        List<BigCardEnchantmentPanel> activeList = this.m_enchantmentPool.GetActiveList();
        BigCardEnchantmentPanel panel = null;
        for (int i = 0; i < activeList.Count; i++)
        {
            BigCardEnchantmentPanel panel2 = activeList[i];
            panel2.Show();
            Transform transform = panel2.transform;
            transform.localScale *= this.PLATFORM_SCALING_FACTOR * this.ENCHANTMENT_SCALING_FACTOR;
            if (i == 0)
            {
                TransformUtil.SetPoint(panel2.gameObject, new Vector3(0.5f, 0f, 1f), this.m_bigCardActor.GetMeshRenderer().gameObject, new Vector3(0.5f, 0f, 0f), new Vector3(0.01f, 0.01f, 0f));
            }
            else
            {
                TransformUtil.SetPoint(panel2.gameObject, new Vector3(0f, 0f, 1f), panel.gameObject, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
            }
            panel = panel2;
            panel2.transform.parent = bone.transform;
            float height = panel2.GetHeight();
            num2 += height;
        }
        this.m_EnchantmentBanner.SetActive(true);
        this.m_EnchantmentBannerBottom.SetActive(true);
        this.m_EnchantmentBannerBottom.transform.localScale = (this.m_initialBannerBottomScale * this.PLATFORM_SCALING_FACTOR) * this.ENCHANTMENT_SCALING_FACTOR;
        this.m_EnchantmentBanner.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        this.m_EnchantmentBannerBottom.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        TransformUtil.SetPoint(this.m_EnchantmentBanner, new Vector3(0.5f, 0f, 1f), this.m_bigCardActor.GetMeshRenderer().gameObject, new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0f, 0.2f));
        this.m_EnchantmentBanner.transform.localScale = (this.m_initialBannerScale * this.PLATFORM_SCALING_FACTOR) * this.ENCHANTMENT_SCALING_FACTOR;
        TransformUtil.SetLocalScaleZ(this.m_EnchantmentBanner.gameObject, (num2 / this.m_initialBannerHeight) / this.m_initialBannerScale.z);
        this.m_EnchantmentBanner.transform.parent = bone.transform;
        TransformUtil.SetPoint(this.m_EnchantmentBannerBottom, Anchor.FRONT, this.m_EnchantmentBanner, Anchor.BACK);
        this.m_EnchantmentBannerBottom.transform.parent = bone.transform;
    }

    private void LoadAndDisplayBigCard()
    {
        if (this.m_bigCardActor != null)
        {
            this.m_bigCardActor.Destroy();
        }
        string bigCardActor = ActorNames.GetBigCardActor(this.m_card.GetEntity());
        this.m_bigCardActor = AssetLoader.Get().LoadActor(bigCardActor, false, false).GetComponent<Actor>();
        this.SetupActor(this.m_card, this.m_bigCardActor);
        this.DisplayBigCard();
    }

    private void LoadAndDisplayTooltipPhoneSecrets()
    {
        if (this.m_phoneSecretActors == null)
        {
            this.m_phoneSecretActors = new List<Actor>();
        }
        else
        {
            foreach (Actor actor in this.m_phoneSecretActors)
            {
                actor.Destroy();
            }
            this.m_phoneSecretActors.Clear();
        }
        List<Card> cards = this.m_card.GetZone().GetCards();
        for (int i = 0; i < cards.Count; i++)
        {
            Actor item = this.LoadPhoneSecret(cards[i]);
            this.m_phoneSecretActors.Add(item);
        }
        this.DisplayPhoneSecrets(this.m_card, this.m_phoneSecretActors, false);
    }

    private Actor LoadPhoneSecret(Card card)
    {
        string bigCardActor = ActorNames.GetBigCardActor(card.GetEntity());
        Actor component = AssetLoader.Get().LoadActor(bigCardActor, false, false).GetComponent<Actor>();
        this.SetupActor(card, component);
        return component;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void ResetEnchantments()
    {
        this.m_EnchantmentBanner.SetActive(false);
        this.m_EnchantmentBannerBottom.SetActive(false);
        this.m_EnchantmentBanner.transform.parent = base.transform;
        this.m_EnchantmentBannerBottom.transform.parent = base.transform;
        foreach (BigCardEnchantmentPanel panel in this.m_enchantmentPool.GetActiveList())
        {
            panel.transform.parent = base.transform;
            panel.ResetScale();
            panel.Hide();
        }
    }

    private void SetupActor(Card card, Actor actor)
    {
        Entity entity = card.GetEntity();
        if (this.ShouldActorUseEntity(entity))
        {
            actor.SetEntity(entity);
        }
        else
        {
            actor.SetEntityDef(entity.GetEntityDef());
        }
        actor.SetCardFlair(entity.GetCardFlair());
        actor.SetCard(card);
        actor.SetCardDef(card.GetCardDef());
        actor.UpdateAllComponents();
        actor.GetComponentInChildren<BoxCollider>().enabled = false;
        actor.name = "BigCard_" + actor.name;
        SceneUtils.SetLayer(actor, GameLayer.Tooltip);
    }

    private bool ShouldActorUseEntity(Entity entity)
    {
        return (entity.IsHidden() || entity.IsHeroPower());
    }

    public void Show(Card card)
    {
        this.m_card = card;
        if ((GameState.Get() == null) || GameState.Get().GetGameEntity().NotifyOfCardTooltipDisplayShow(card))
        {
            Zone zone = card.GetZone();
            if ((UniversalInputManager.UsePhoneUI != null) && (zone is ZoneSecret))
            {
                this.LoadAndDisplayTooltipPhoneSecrets();
            }
            else
            {
                this.LoadAndDisplayBigCard();
            }
        }
    }

    private bool ShowBigCardOnRight()
    {
        if (UniversalInputManager.Get().IsTouchMode())
        {
            return this.ShowBigCardOnRightTouch();
        }
        return this.ShowBigCardOnRightMouse();
    }

    private bool ShowBigCardOnRightMouse()
    {
        if ((!this.m_card.GetEntity().IsHero() && !this.m_card.GetEntity().IsHeroPower()) && !this.m_card.GetEntity().IsSecret())
        {
            if (this.m_card.GetEntity().GetCardId() == "TU4c_007")
            {
                return false;
            }
            ZonePlay zone = this.m_card.GetZone() as ZonePlay;
            if (zone != null)
            {
                return (this.m_card.GetActor().GetMeshRenderer().bounds.center.x < (zone.GetComponent<BoxCollider>().bounds.center.x + 2.5f));
            }
        }
        return true;
    }

    private bool ShowBigCardOnRightTouch()
    {
        if ((!this.m_card.GetEntity().IsHero() && !this.m_card.GetEntity().IsHeroPower()) && !this.m_card.GetEntity().IsSecret())
        {
            if (this.m_card.GetEntity().GetCardId() == "TU4c_007")
            {
                return false;
            }
            ZonePlay zone = this.m_card.GetZone() as ZonePlay;
            if (zone != null)
            {
                float num = (UniversalInputManager.UsePhoneUI == null) ? -2.5f : 0f;
                return (this.m_card.GetActor().GetMeshRenderer().bounds.center.x < (zone.GetComponent<BoxCollider>().bounds.center.x + num));
            }
        }
        return false;
    }

    private bool ShowKeywordOnRight()
    {
        if (this.m_card.GetEntity().IsHeroPower())
        {
            return true;
        }
        if (this.m_card.GetEntity().IsWeapon())
        {
            return true;
        }
        if (this.m_card.GetEntity().IsHero() || this.m_card.GetEntity().IsSecret())
        {
            return false;
        }
        if (this.m_card.GetEntity().GetCardId() == "TU4c_007")
        {
            return false;
        }
        ZonePlay zone = this.m_card.GetZone() as ZonePlay;
        if (zone == null)
        {
            return false;
        }
        if (UniversalInputManager.UsePhoneUI != null)
        {
            return (this.m_card.GetActor().GetMeshRenderer().bounds.center.x > zone.GetComponent<BoxCollider>().bounds.center.x);
        }
        return (this.m_card.GetActor().GetMeshRenderer().bounds.center.x < (zone.GetComponent<BoxCollider>().bounds.center.x + 0.03f));
    }

    private void ShowPhoneSecretDeath(Actor actor)
    {
        <ShowPhoneSecretDeath>c__AnonStorey2F0 storeyf;
        storeyf = new <ShowPhoneSecretDeath>c__AnonStorey2F0 {
            actor = actor,
            deathSpellStateFinished = new Spell.StateFinishedCallback(storeyf.<>m__EB)
        };
        Action<object> action = new Action<object>(storeyf.<>m__EC);
        object[] args = new object[] { "time", this.m_SecretLayoutData.m_TimeUntilDeathSpell, "oncomplete", action };
        Hashtable hashtable = iTween.Hash(args);
        iTween.Timer(storeyf.actor.gameObject, hashtable);
    }

    public void ShowSecretDeaths(Map<Player, DeadSecretGroup> deadSecretMap)
    {
        if ((deadSecretMap != null) && (deadSecretMap.Count != 0))
        {
            int num = 0;
            foreach (DeadSecretGroup group in deadSecretMap.Values)
            {
                Card mainCard = group.GetMainCard();
                List<Card> cards = group.GetCards();
                List<Actor> actors = new List<Actor>();
                for (int i = 0; i < cards.Count; i++)
                {
                    Card card = cards[i];
                    Actor item = this.LoadPhoneSecret(card);
                    actors.Add(item);
                }
                this.DisplayPhoneSecrets(mainCard, actors, true);
                num++;
            }
        }
    }

    private void UpdateEnchantments()
    {
        this.ResetEnchantments();
        GameObject bone = this.m_bigCardActor.FindBone("EnchantmentTooltip");
        if (bone != null)
        {
            List<Entity> enchantments = this.m_card.GetEntity().GetEnchantments();
            List<BigCardEnchantmentPanel> activeList = this.m_enchantmentPool.GetActiveList();
            int count = enchantments.Count;
            int num2 = activeList.Count;
            int num3 = count - num2;
            if (num3 > 0)
            {
                this.m_enchantmentPool.AcquireBatch(num3);
            }
            else if (num3 < 0)
            {
                this.m_enchantmentPool.ReleaseBatch(count, -num3);
            }
            if (count != 0)
            {
                for (int i = 0; i < activeList.Count; i++)
                {
                    activeList[i].SetEnchantment(enchantments[i]);
                }
                this.LayoutEnchantments(bone);
                SceneUtils.SetLayer(bone, GameLayer.Tooltip);
            }
        }
    }

    [CompilerGenerated]
    private sealed class <HidePhoneSecret>c__AnonStorey2F1
    {
        internal Actor actor;

        internal void <>m__ED(object obj)
        {
            this.actor.Destroy();
        }
    }

    [CompilerGenerated]
    private sealed class <ShowPhoneSecretDeath>c__AnonStorey2F0
    {
        internal Actor actor;
        internal Spell.StateFinishedCallback deathSpellStateFinished;

        internal void <>m__EB(Spell spell, SpellStateType prevStateType, object userData)
        {
            if (spell.GetActiveState() != SpellStateType.NONE)
            {
                this.actor.Destroy();
            }
        }

        internal void <>m__EC(object obj)
        {
            Spell spell = this.actor.GetSpell(SpellType.DEATH);
            spell.AddStateFinishedCallback(this.deathSpellStateFinished);
            spell.Activate();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeywordArgs
    {
        public Card card;
        public Actor actor;
        public bool showOnRight;
    }

    [Serializable]
    public class LayoutData
    {
        public float m_DriftSec = 10f;
        public float m_ScaleSec = 0.15f;
    }

    [Serializable]
    public class SecretLayoutData
    {
        public float m_DeathShowAnimTime = 1f;
        public Vector3 m_DriftOffset = new Vector3(0f, 0f, 0.05f);
        public float m_DriftSec = 5f;
        public Vector3 m_HiddenSpacing = new Vector3(2.4f, 0f, 0.7f);
        public float m_HideAnimTime = 0.15f;
        public BigCard.SecretLayoutOffsets m_MaxCardOffsets = new BigCard.SecretLayoutOffsets();
        public int m_MaxCardThreshold = 5;
        public BigCard.SecretLayoutOffsets m_MinCardOffsets = new BigCard.SecretLayoutOffsets();
        public int m_MinCardThreshold = 1;
        public float m_ShowAnimTime = 0.15f;
        public Vector3 m_Spacing = new Vector3(2.1f, 0f, 0.7f);
        public float m_TimeUntilDeathSpell = 1.5f;
    }

    [Serializable]
    public class SecretLayoutOffsets
    {
        public Vector3 m_HiddenInitialOffset = new Vector3(0f, 4f, 4f);
        public Vector3 m_HiddenOpponentInitialOffset = new Vector3(0f, 4f, -4f);
        public Vector3 m_InitialOffset = new Vector3(0.1f, 5f, 3.3f);
        public Vector3 m_OpponentInitialOffset = new Vector3(0.1f, 5f, -3.3f);
    }
}

