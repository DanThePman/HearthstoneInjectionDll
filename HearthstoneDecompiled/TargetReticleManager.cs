using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class TargetReticleManager : MonoBehaviour
{
    private static readonly PlatformDependentValue<float> DAMAGE_INDICATOR_SCALE;
    private static readonly PlatformDependentValue<float> DAMAGE_INDICATOR_Z_OFFSET;
    private const float FRIENDLY_HERO_ORIGIN_Z_OFFSET = 1f;
    private const float LENGTH_BETWEEN_LINKS = 1.2f;
    private const float LINK_ANIMATION_SPEED = 0.5f;
    private const float LINK_FADE_OFFSET = -1.2f;
    private const float LINK_PARABOLA_HEIGHT = 1.5f;
    private const float LINK_Y_LENGTH = 1f;
    private GameObject m_arrow;
    private GameObject m_damageIndicator;
    private GameObject m_hunterReticle;
    private bool m_isActive;
    private bool m_isEnemyArrow;
    private float m_linkAnimationZOffset;
    private int m_numActiveLinks;
    private int m_originLocationEntityID = -1;
    private Vector3 m_remoteArrowPosition;
    private TARGET_RETICLE_TYPE m_ReticleType;
    private bool m_showArrow = true;
    private int m_sourceEntityID = -1;
    private List<GameObject> m_targetArrowLinks;
    private Vector3 m_targetArrowOrigin;
    private bool m_useHandAsOrigin;
    private const int MAX_TARGET_ARROW_LINKS = 15;
    private static TargetReticleManager s_instance;
    private static readonly PlatformDependentValue<bool> SHOW_DAMAGE_INDICATOR_ON_ENTITY;

    static TargetReticleManager()
    {
        PlatformDependentValue<bool> value2 = new PlatformDependentValue<bool>(PlatformCategory.Input) {
            Mouse = false,
            Touch = true
        };
        SHOW_DAMAGE_INDICATOR_ON_ENTITY = value2;
        PlatformDependentValue<float> value3 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 2.5f,
            Tablet = 3.75f
        };
        DAMAGE_INDICATOR_SCALE = value3;
        value3 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 0.75f,
            Tablet = -1.2f
        };
        DAMAGE_INDICATOR_Z_OFFSET = value3;
    }

    private void ActivateArrow(bool active)
    {
        this.m_isActive = active;
        SceneUtils.EnableRenderers(this.m_arrow.gameObject, false);
        this.m_hunterReticle.SetActive(false);
        if (active)
        {
            if (this.m_ReticleType == TARGET_RETICLE_TYPE.DefaultArrow)
            {
                SceneUtils.EnableRenderers(this.m_arrow.gameObject, active && this.m_showArrow);
            }
            else if (this.m_ReticleType == TARGET_RETICLE_TYPE.HunterReticle)
            {
                this.m_hunterReticle.SetActive(active && this.m_showArrow);
            }
            else
            {
                UnityEngine.Debug.LogError("Unknown Target Reticle Type!");
            }
        }
    }

    private void AddUntargetableCard(Card sourceCard, List<Card> cards, Card card)
    {
        if (sourceCard != card)
        {
            cards.Add(card);
        }
    }

    private void Awake()
    {
        s_instance = this;
    }

    public void CreateEnemyTargetArrow(Entity originEntity)
    {
        this.m_ReticleType = TARGET_RETICLE_TYPE.DefaultArrow;
        this.CreateTargetArrow(true, originEntity.GetEntityId(), originEntity.GetEntityId(), null, true, false);
    }

    public void CreateFriendlyTargetArrow(Entity originLocationEntity, Entity sourceEntity, bool showDamageIndicatorText, bool showArrow = true, string overrideText = null, bool useHandAsOrigin = false)
    {
        if (!((GameMgr.Get() != null) && GameMgr.Get().IsSpectator()))
        {
            this.DisableCollidersForUntargetableCards(sourceEntity.GetCard());
        }
        Spell playSpell = sourceEntity.GetCard().GetPlaySpell(true);
        if (playSpell != null)
        {
            this.m_ReticleType = playSpell.m_TargetReticle;
        }
        else
        {
            this.m_ReticleType = TARGET_RETICLE_TYPE.DefaultArrow;
        }
        string damageIndicatorText = null;
        if (overrideText != null)
        {
            damageIndicatorText = overrideText;
        }
        else if (showDamageIndicatorText)
        {
            damageIndicatorText = sourceEntity.GetTargetingArrowText();
        }
        this.CreateTargetArrow(false, originLocationEntity.GetEntityId(), sourceEntity.GetEntityId(), damageIndicatorText, showArrow, useHandAsOrigin);
    }

    private void CreateTargetArrow(bool isEnemyArrow, int originLocationEntityID, int sourceEntityID, string damageIndicatorText, bool showArrow, bool useHandAsOrigin = false)
    {
        if (this.IsActive())
        {
            Log.Rachelle.Print("Uh-oh... creating a targeting arrow but one is already active...", new object[0]);
            this.DestroyCurrentArrow(false);
        }
        this.m_isEnemyArrow = isEnemyArrow;
        this.m_sourceEntityID = sourceEntityID;
        this.m_originLocationEntityID = originLocationEntityID;
        this.m_showArrow = showArrow;
        this.m_useHandAsOrigin = useHandAsOrigin;
        this.UpdateArrowOriginPosition();
        bool flag = (GameMgr.Get() != null) && GameMgr.Get().IsSpectator();
        if (this.m_isEnemyArrow || flag)
        {
            this.m_remoteArrowPosition = this.m_targetArrowOrigin;
            this.m_arrow.transform.position = this.m_targetArrowOrigin;
        }
        this.ActivateArrow(true);
        this.ShowBullseye(false);
        this.ShowDamageIndicator(!this.m_isEnemyArrow);
        this.UpdateArrowPosition();
        if (!this.m_isEnemyArrow)
        {
            base.StartCoroutine(this.SetDamageText(damageIndicatorText));
            if (!flag)
            {
                PegCursor.Get().Hide();
            }
        }
    }

    private void DestroyCurrentArrow(bool isLocallyCanceled)
    {
        if (this.m_isEnemyArrow)
        {
            this.DestroyEnemyTargetArrow();
        }
        else
        {
            this.DestroyFriendlyTargetArrow(isLocallyCanceled);
        }
    }

    public void DestroyEnemyTargetArrow()
    {
        this.DestroyTargetArrow(true, false);
    }

    public void DestroyFriendlyTargetArrow(bool isLocallyCanceled)
    {
        this.EnableCollidersThatWereDisabled();
        this.DestroyTargetArrow(false, isLocallyCanceled);
    }

    private void DestroyTargetArrow(bool destroyEnemyArrow, bool isLocallyCanceled)
    {
        if (this.IsActive())
        {
            if (destroyEnemyArrow != this.m_isEnemyArrow)
            {
                Log.Rachelle.Print(string.Format("trying to destroy {0} arrow but the active arrow is {1}", !destroyEnemyArrow ? "friendly" : "enemy", !this.m_isEnemyArrow ? "friendly" : "enemy"), new object[0]);
            }
            else
            {
                if (isLocallyCanceled)
                {
                    Entity entity = GameState.Get().GetEntity(this.m_sourceEntityID);
                    if (entity != null)
                    {
                        entity.GetCard().NotifyTargetingCanceled();
                    }
                }
                this.m_originLocationEntityID = -1;
                this.m_sourceEntityID = -1;
                if (!this.m_isEnemyArrow)
                {
                    RemoteActionHandler.Get().NotifyOpponentOfTargetEnd();
                    PegCursor.Get().Show();
                }
                this.ActivateArrow(false);
                this.ShowDamageIndicator(false);
            }
        }
    }

    private void DisableCollidersForUntargetableCards(Card sourceCard)
    {
        List<Card> cards = new List<Card>();
        foreach (Player player in GameState.Get().GetPlayerMap().Values)
        {
            this.AddUntargetableCard(sourceCard, cards, player.GetHeroPowerCard());
            this.AddUntargetableCard(sourceCard, cards, player.GetWeaponCard());
            foreach (Card card in player.GetSecretZone().GetCards())
            {
                this.AddUntargetableCard(sourceCard, cards, card);
            }
        }
        foreach (Card card2 in cards)
        {
            if (card2 != null)
            {
                Actor actor = card2.GetActor();
                if (actor != null)
                {
                    actor.TurnOffCollider();
                }
            }
        }
    }

    private void EnableCollidersThatWereDisabled()
    {
        List<Card> list = new List<Card>();
        foreach (Player player in GameState.Get().GetPlayerMap().Values)
        {
            list.Add(player.GetHeroPowerCard());
            list.Add(player.GetWeaponCard());
            foreach (Card card in player.GetSecretZone().GetCards())
            {
                list.Add(card);
            }
        }
        foreach (Card card2 in list)
        {
            if ((card2 != null) && (card2.GetActor() != null))
            {
                card2.GetActor().TurnOnCollider();
            }
        }
    }

    public static TargetReticleManager Get()
    {
        return s_instance;
    }

    public bool IsActive()
    {
        if (this.m_ReticleType == TARGET_RETICLE_TYPE.DefaultArrow)
        {
            return ((this.m_arrow != null) && this.m_isActive);
        }
        if (this.m_ReticleType == TARGET_RETICLE_TYPE.HunterReticle)
        {
            return ((this.m_hunterReticle != null) && this.m_isActive);
        }
        UnityEngine.Debug.LogError("Unknown Target Reticle Type!");
        return false;
    }

    public bool IsEnemyArrow()
    {
        return this.m_isEnemyArrow;
    }

    public bool IsEnemyArrowActive()
    {
        if (!this.m_isEnemyArrow)
        {
            return false;
        }
        return this.IsActive();
    }

    public bool IsLocalArrow()
    {
        return !this.m_isEnemyArrow;
    }

    public bool IsLocalArrowActive()
    {
        if (this.m_isEnemyArrow)
        {
            return false;
        }
        return this.IsActive();
    }

    private void LoadArrowCallback(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_arrow = actorObject;
        SceneUtils.EnableRenderers(this.m_arrow.gameObject, false);
        this.ShowBullseye(false);
    }

    private void LoadDamageIndicatorCallback(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_damageIndicator = actorObject;
        this.m_damageIndicator.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        this.m_damageIndicator.transform.localScale = new Vector3((float) DAMAGE_INDICATOR_SCALE, (float) DAMAGE_INDICATOR_SCALE, (float) DAMAGE_INDICATOR_SCALE);
        this.ShowDamageIndicator(false);
    }

    private void LoadHunterReticleCallback(string actorName, GameObject actorObject, object callbackData)
    {
        this.m_hunterReticle = actorObject;
        this.m_hunterReticle.transform.parent = base.transform;
        this.m_hunterReticle.SetActive(false);
    }

    private void LoadLinkCallback(string actorName, GameObject actorObject, object callbackData)
    {
        base.StartCoroutine(this.OnLinkLoaded(actorObject));
    }

    private int NumberOfRequiredLinks(float lengthOfArrow)
    {
        int num = ((int) Mathf.Floor(lengthOfArrow / 1.2f)) + 1;
        if (num == 1)
        {
            num = 0;
        }
        return num;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    [DebuggerHidden]
    private IEnumerator OnLinkLoaded(GameObject linkActorObject)
    {
        return new <OnLinkLoaded>c__IteratorC6 { linkActorObject = linkActorObject, <$>linkActorObject = linkActorObject, <>f__this = this };
    }

    public void PreloadTargetArrows()
    {
        this.m_targetArrowLinks = new List<GameObject>();
        AssetLoader.Get().LoadActor("Target_Arrow_Bullseye", new AssetLoader.GameObjectCallback(this.LoadArrowCallback), null, false);
        AssetLoader.Get().LoadActor("TargetDamageIndicator", new AssetLoader.GameObjectCallback(this.LoadDamageIndicatorCallback), null, false);
        AssetLoader.Get().LoadActor("Target_Arrow_Link", new AssetLoader.GameObjectCallback(this.LoadLinkCallback), null, false);
        AssetLoader.Get().LoadActor("HunterReticle", new AssetLoader.GameObjectCallback(this.LoadHunterReticleCallback), null, false);
    }

    [DebuggerHidden]
    private IEnumerator SetDamageText(string damageText)
    {
        return new <SetDamageText>c__IteratorC7 { damageText = damageText, <$>damageText = damageText, <>f__this = this };
    }

    private void SetLinkAlpha(GameObject linkGameObject, float alpha)
    {
        alpha = Mathf.Clamp(alpha, 0f, 1f);
        foreach (Renderer renderer in linkGameObject.GetComponents<Renderer>())
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
        }
    }

    public void SetRemotePlayerArrowPosition(Vector3 newPosition)
    {
        this.m_remoteArrowPosition = newPosition;
    }

    public void ShowArrow(bool show)
    {
        this.m_showArrow = show;
        SceneUtils.EnableRenderers(this.m_arrow.gameObject, false);
        this.m_hunterReticle.SetActive(false);
        if (show)
        {
            if (this.m_ReticleType == TARGET_RETICLE_TYPE.DefaultArrow)
            {
                SceneUtils.EnableRenderers(this.m_arrow.gameObject, show);
            }
            else if (this.m_ReticleType == TARGET_RETICLE_TYPE.HunterReticle)
            {
                this.m_hunterReticle.SetActive(show);
            }
            else
            {
                UnityEngine.Debug.LogError("Unknown Target Reticle Type!");
            }
        }
    }

    public void ShowBullseye(bool show)
    {
        if (this.m_ReticleType == TARGET_RETICLE_TYPE.DefaultArrow)
        {
            if ((this.m_ReticleType == TARGET_RETICLE_TYPE.DefaultArrow) && (this.IsActive() && this.m_showArrow))
            {
                Transform transform = this.m_arrow.transform.FindChild("TargetArrow_TargetMesh");
                if (transform != null)
                {
                    SceneUtils.EnableRenderers(transform.gameObject, show);
                }
            }
        }
        else if ((this.m_ReticleType == TARGET_RETICLE_TYPE.HunterReticle) && (this.m_hunterReticle != null))
        {
            RenderToTexture component = this.m_hunterReticle.GetComponent<RenderToTexture>();
            if (component != null)
            {
                Material renderMaterial = component.GetRenderMaterial();
                if (renderMaterial != null)
                {
                    if (show)
                    {
                        renderMaterial.color = Color.red;
                    }
                    else
                    {
                        renderMaterial.color = Color.white;
                    }
                }
            }
        }
    }

    private void ShowDamageIndicator(bool show)
    {
        if ((this.m_damageIndicator != null) && this.m_damageIndicator.activeInHierarchy)
        {
            SceneUtils.EnableRenderers(this.m_damageIndicator.gameObject, show);
        }
    }

    private void UpdateArrowOriginPosition()
    {
        Entity entity = GameState.Get().GetEntity(this.m_originLocationEntityID);
        if (entity == null)
        {
            Log.Rachelle.Print(string.Format("entity with ID {0} does not exist... can't update arrow origin position!", this.m_originLocationEntityID), new object[0]);
            this.DestroyCurrentArrow(false);
        }
        else
        {
            this.m_targetArrowOrigin = entity.GetCard().transform.position;
            if (this.m_useHandAsOrigin)
            {
                this.m_targetArrowOrigin = InputManager.Get().GetFriendlyHand().transform.position;
            }
            if (entity.IsHero() && !this.m_isEnemyArrow)
            {
                this.m_targetArrowOrigin.z++;
            }
        }
    }

    public void UpdateArrowPosition()
    {
        if (this.IsActive())
        {
            if (!this.m_showArrow)
            {
                this.UpdateArrowOriginPosition();
                this.UpdateDamageIndicator();
            }
            else
            {
                Vector3 point;
                float y = 0f;
                bool flag = (GameMgr.Get() != null) && GameMgr.Get().IsSpectator();
                if (this.m_isEnemyArrow || flag)
                {
                    if (GameState.Get().IsBlockingServer())
                    {
                        this.DestroyCurrentArrow(false);
                        return;
                    }
                    Vector3 zero = Vector3.zero;
                    if (this.m_ReticleType == TARGET_RETICLE_TYPE.DefaultArrow)
                    {
                        zero = this.m_arrow.transform.position;
                    }
                    else if (this.m_ReticleType == TARGET_RETICLE_TYPE.HunterReticle)
                    {
                        zero = this.m_hunterReticle.transform.position;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Unknown Target Reticle Type!");
                    }
                    point.x = Mathf.Lerp(zero.x, this.m_remoteArrowPosition.x, 0.1f);
                    point.y = Mathf.Lerp(zero.y, this.m_remoteArrowPosition.y, 0.1f);
                    point.z = Mathf.Lerp(zero.z, this.m_remoteArrowPosition.z, 0.1f);
                    Card card = !this.m_isEnemyArrow ? RemoteActionHandler.Get().GetFriendlyHeldCard() : RemoteActionHandler.Get().GetOpponentHeldCard();
                    if (card != null)
                    {
                        this.m_targetArrowOrigin = card.transform.position;
                    }
                }
                else
                {
                    RaycastHit hit;
                    if (!UniversalInputManager.Get().GetInputHitInfo(Camera.main, GameLayer.DragPlane, out hit))
                    {
                        return;
                    }
                    point = hit.point;
                    this.UpdateArrowOriginPosition();
                }
                if (!object.Equals(point.z - this.m_targetArrowOrigin.z, 0f))
                {
                    float num2 = Mathf.Atan((point.x - this.m_targetArrowOrigin.x) / (point.z - this.m_targetArrowOrigin.z));
                    y = 57.29578f * num2;
                }
                if (point.z < this.m_targetArrowOrigin.z)
                {
                    y -= 180f;
                }
                if (this.m_ReticleType == TARGET_RETICLE_TYPE.DefaultArrow)
                {
                    this.m_arrow.transform.localEulerAngles = new Vector3(0f, y, 0f);
                    this.m_arrow.transform.position = point;
                    float num3 = Mathf.Pow(this.m_targetArrowOrigin.x - point.x, 2f);
                    float num4 = Mathf.Pow(this.m_targetArrowOrigin.z - point.z, 2f);
                    float lengthOfArrow = Mathf.Sqrt(num3 + num4);
                    this.UpdateTargetArrowLinks(lengthOfArrow);
                }
                else if (this.m_ReticleType == TARGET_RETICLE_TYPE.HunterReticle)
                {
                    this.m_hunterReticle.transform.position = point;
                }
                else
                {
                    UnityEngine.Debug.LogError("Unknown Target Reticle Type!");
                }
                this.UpdateDamageIndicator();
            }
        }
    }

    private void UpdateDamageIndicator()
    {
        if (this.m_damageIndicator != null)
        {
            Vector3 zero = Vector3.zero;
            if (SHOW_DAMAGE_INDICATOR_ON_ENTITY != null)
            {
                zero = this.m_targetArrowOrigin;
                zero.z += DAMAGE_INDICATOR_Z_OFFSET;
            }
            else
            {
                if (this.m_ReticleType == TARGET_RETICLE_TYPE.DefaultArrow)
                {
                    zero = this.m_arrow.transform.position;
                }
                else if (this.m_ReticleType == TARGET_RETICLE_TYPE.HunterReticle)
                {
                    zero = this.m_hunterReticle.transform.position;
                }
                else
                {
                    UnityEngine.Debug.LogError("Unknown Target Reticle Type!");
                }
                zero.z += DAMAGE_INDICATOR_Z_OFFSET;
            }
            this.m_damageIndicator.transform.position = zero;
        }
    }

    private void UpdateTargetArrowLinks(float lengthOfArrow)
    {
        this.m_numActiveLinks = this.NumberOfRequiredLinks(lengthOfArrow);
        int count = this.m_targetArrowLinks.Count;
        Transform transform = this.m_arrow.transform.FindChild("TargetArrow_ArrowMesh");
        if (this.m_numActiveLinks == 0)
        {
            transform.localEulerAngles = new Vector3(300f, 180f, 0f);
            for (int i = 0; i < count; i++)
            {
                SceneUtils.EnableRenderers(this.m_targetArrowLinks[i].gameObject, false);
            }
        }
        else
        {
            float num3 = -lengthOfArrow / 2f;
            float num4 = -1.5f / (num3 * num3);
            for (int j = 0; j < count; j++)
            {
                if (this.m_targetArrowLinks[j] != null)
                {
                    if (j >= this.m_numActiveLinks)
                    {
                        SceneUtils.EnableRenderers(this.m_targetArrowLinks[j].gameObject, false);
                    }
                    else
                    {
                        float z = -(1.2f * (j + 1)) + this.m_linkAnimationZOffset;
                        float num7 = (num4 * Mathf.Pow(z - num3, 2f)) + 1.5f;
                        float num8 = (2f * num4) * (z - num3);
                        float num9 = Mathf.Atan(num8);
                        float num10 = 180f - (num9 * 57.29578f);
                        SceneUtils.EnableRenderers(this.m_targetArrowLinks[j].gameObject, true);
                        this.m_targetArrowLinks[j].transform.localPosition = new Vector3(0f, num7, z);
                        this.m_targetArrowLinks[j].transform.eulerAngles = new Vector3(num10, this.m_arrow.transform.localEulerAngles.y, 0f);
                        float num11 = 1f;
                        if (j == 0)
                        {
                            if (z > -1.2f)
                            {
                                num11 = z / -1.2f;
                                num11 = Mathf.Pow(num11, 6f);
                            }
                        }
                        else if (j == (this.m_numActiveLinks - 1))
                        {
                            num11 = this.m_linkAnimationZOffset / 1.2f;
                            num11 *= num11;
                        }
                        this.SetLinkAlpha(this.m_targetArrowLinks[j], num11);
                    }
                }
            }
            float y = (num4 * Mathf.Pow(transform.localPosition.z - num3, 2f)) + 1.5f;
            float f = (2f * num4) * (transform.localPosition.z - num3);
            float x = Mathf.Atan(f) * 57.29578f;
            if (x < 0f)
            {
                x += 360f;
            }
            transform.localPosition = new Vector3(0f, y, transform.localPosition.z);
            transform.localEulerAngles = new Vector3(x, 180f, 0f);
            this.m_linkAnimationZOffset += UnityEngine.Time.deltaTime * 0.5f;
            if (this.m_linkAnimationZOffset > 1.2f)
            {
                this.m_linkAnimationZOffset -= 1.2f;
            }
        }
    }

    public int ArrowSourceEntityID
    {
        get
        {
            return this.m_originLocationEntityID;
        }
    }

    [CompilerGenerated]
    private sealed class <OnLinkLoaded>c__IteratorC6 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal GameObject <$>linkActorObject;
        internal TargetReticleManager <>f__this;
        internal int <i>__0;
        internal GameObject <newLink>__1;
        internal GameObject linkActorObject;

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
                case 1:
                    if (this.<>f__this.m_arrow == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<i>__0 = 0;
                    while (this.<i>__0 < 14)
                    {
                        this.<newLink>__1 = UnityEngine.Object.Instantiate<GameObject>(this.linkActorObject);
                        this.<newLink>__1.transform.parent = this.<>f__this.m_arrow.transform;
                        this.<>f__this.m_targetArrowLinks.Add(this.<newLink>__1);
                        this.<i>__0++;
                    }
                    this.linkActorObject.transform.parent = this.<>f__this.m_arrow.transform;
                    this.<>f__this.m_targetArrowLinks.Add(this.linkActorObject);
                    SceneUtils.EnableRenderers(this.<>f__this.m_arrow.gameObject, false);
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

    [CompilerGenerated]
    private sealed class <SetDamageText>c__IteratorC7 : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object $current;
        internal int $PC;
        internal string <$>damageText;
        internal TargetReticleManager <>f__this;
        internal UberText <damageTextMesh>__0;
        internal string damageText;

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
                case 1:
                    if (this.<>f__this.m_damageIndicator == null)
                    {
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    if (string.IsNullOrEmpty(this.damageText))
                    {
                        this.<>f__this.ShowDamageIndicator(false);
                    }
                    else
                    {
                        this.<damageTextMesh>__0 = this.<>f__this.m_damageIndicator.transform.GetComponentInChildren<UberText>();
                        this.<damageTextMesh>__0.Text = this.damageText;
                        this.$PC = -1;
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
}

