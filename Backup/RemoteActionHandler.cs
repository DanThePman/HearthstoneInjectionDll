using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteActionHandler : MonoBehaviour
{
    private const float DRIFT_TIME = 10f;
    private UserUI enemyActualUI = new UserUI();
    private UserUI enemyWantedUI = new UserUI();
    private UserUI friendlyActualUI = new UserUI();
    private UserUI friendlyWantedUI = new UserUI();
    private const float HIGH_FREQ_SEND_TIME = 0.25f;
    private const float LOW_FREQ_SEND_TIME = 0.35f;
    private float m_lastSendTime = UnityEngine.Time.realtimeSinceStartup;
    private UserUI myCurrentUI = new UserUI();
    private UserUI myLastUI = new UserUI();
    private static RemoteActionHandler s_instance;
    private static readonly List<EmoteType> s_validUserEmotes = new List<EmoteType> { 1, 2, 3, 4, 6, 5 };
    public const string TWEEN_NAME = "RemoteActionHandler";

    private void Awake()
    {
        s_instance = this;
        GameState.Get().RegisterTurnChangedListener(new GameState.TurnChangedCallback(this.OnTurnChanged));
    }

    private bool CanAnimateHeldCard(Card card)
    {
        if (!this.IsCardInHand(card))
        {
            return false;
        }
        string tweenName = ZoneMgr.Get().GetTweenName<ZoneHand>();
        string[] names = new string[] { "RemoteActionHandler", tweenName };
        if (iTween.HasNameNotInList(card.gameObject, names))
        {
            return false;
        }
        return true;
    }

    private bool CanSendUI()
    {
        if (GameMgr.Get() == null)
        {
            return false;
        }
        if (GameMgr.Get().IsSpectator())
        {
            return false;
        }
        if ((GameMgr.Get().IsAI() && !SpectatorManager.Get().MyGameHasSpectators()) && SpectatorManager.Get().MyGameHasSpectators())
        {
            return false;
        }
        float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
        float num2 = realtimeSinceStartup - this.m_lastSendTime;
        if (this.IsSendingTargetingArrow() && (num2 > 0.25f))
        {
            this.m_lastSendTime = realtimeSinceStartup;
            return true;
        }
        if (num2 < 0.35f)
        {
            return false;
        }
        this.m_lastSendTime = realtimeSinceStartup;
        return true;
    }

    private void DriftLeftAndRight(bool isFriendlySide)
    {
        Card card = !isFriendlySide ? this.enemyActualUI.held.card : this.friendlyActualUI.held.card;
        if (this.CanAnimateHeldCard(card))
        {
            Vector3[] vectorArray;
            if (isFriendlySide)
            {
                iTweenPath path;
                if (!iTweenPath.paths.TryGetValue(iTweenPath.FixupPathName("driftPath1_friendly"), out path))
                {
                    Transform transform = Board.Get().FindBone("OpponentCardPlayingSpot");
                    Transform transform2 = Board.Get().FindBone("FriendlyCardPlayingSpot");
                    Vector3 vector = transform2.position - transform.position;
                    iTweenPath path2 = iTweenPath.paths[iTweenPath.FixupPathName("driftPath1")];
                    path = transform2.gameObject.AddComponent<iTweenPath>();
                    path.pathVisible = true;
                    path.pathName = "driftPath1_friendly";
                    path.pathColor = path2.pathColor;
                    path.nodes = new List<Vector3>(path2.nodes);
                    for (int i = 0; i < path.nodes.Count; i++)
                    {
                        path.nodes[i] = path2.nodes[i] + vector;
                    }
                    path.enabled = false;
                    path.enabled = true;
                }
                vectorArray = path.nodes.ToArray();
            }
            else
            {
                vectorArray = iTweenPath.GetPath("driftPath1");
            }
            object[] args = new object[] { "name", "RemoteActionHandler", "path", vectorArray, "time", 10f, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.pingPong };
            Hashtable hashtable = iTween.Hash(args);
            iTween.MoveTo(card.gameObject, hashtable);
        }
    }

    public static RemoteActionHandler Get()
    {
        return s_instance;
    }

    public Card GetFriendlyHeldCard()
    {
        return this.friendlyActualUI.held.card;
    }

    public Card GetFriendlyHoverCard()
    {
        return this.friendlyActualUI.over.card;
    }

    private int GetOpponentHandHoverSlot()
    {
        Entity entity = this.enemyActualUI.over.entity;
        if (entity == null)
        {
            return -1;
        }
        if (entity.GetZone() != TAG_ZONE.HAND)
        {
            return -1;
        }
        if (entity.GetController().IsFriendlySide())
        {
            return -1;
        }
        return (entity.GetTag(GAME_TAG.ZONE_POSITION) - 1);
    }

    public Card GetOpponentHeldCard()
    {
        return this.enemyActualUI.held.card;
    }

    public void HandleAction(Network.UserUI newData)
    {
        bool flag = false;
        if (newData.playerId.HasValue)
        {
            Player friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
            flag = (friendlySidePlayer != null) && (friendlySidePlayer.GetPlayerId() == newData.playerId.Value);
        }
        if (newData.mouseInfo != null)
        {
            if (flag)
            {
                this.friendlyWantedUI.held.ID = newData.mouseInfo.HeldCardID;
                this.friendlyWantedUI.over.ID = newData.mouseInfo.OverCardID;
                this.friendlyWantedUI.origin.ID = newData.mouseInfo.ArrowOriginID;
            }
            else
            {
                this.enemyWantedUI.held.ID = newData.mouseInfo.HeldCardID;
                this.enemyWantedUI.over.ID = newData.mouseInfo.OverCardID;
                this.enemyWantedUI.origin.ID = newData.mouseInfo.ArrowOriginID;
            }
            this.UpdateCardOver();
            this.UpdateCardHeld();
            this.MaybeDestroyArrow();
            this.MaybeCreateArrow();
            this.UpdateTargetArrow();
        }
        else if (newData.emoteInfo != null)
        {
            EmoteType emote = (EmoteType) newData.emoteInfo.Emote;
            if (flag)
            {
                GameState.Get().GetFriendlySidePlayer().GetHeroCard().PlayEmote(emote);
            }
            else if (((EnemyEmoteHandler.Get() != null) && !EnemyEmoteHandler.Get().IsSquelched()) && s_validUserEmotes.Contains(emote))
            {
                GameState.Get().GetOpposingSidePlayer().GetHeroCard().PlayEmote(emote);
            }
        }
    }

    private bool IsCardInHand(Card card)
    {
        if (card == null)
        {
            return false;
        }
        if (!(card.GetZone() is ZoneHand))
        {
            return false;
        }
        if (card.GetEntity().GetZone() != TAG_ZONE.HAND)
        {
            return false;
        }
        return true;
    }

    private bool IsSendingTargetingArrow()
    {
        if (this.myCurrentUI.origin.card == null)
        {
            return false;
        }
        if (this.myCurrentUI.over.card == null)
        {
            return false;
        }
        if (this.myCurrentUI.over.card == this.myCurrentUI.origin.card)
        {
            return false;
        }
        return ((this.myCurrentUI.origin.card != this.myLastUI.origin.card) || (this.myCurrentUI.over.card != this.myLastUI.over.card));
    }

    private void MaybeCreateArrow()
    {
        if ((TargetReticleManager.Get() != null) && !TargetReticleManager.Get().IsActive())
        {
            bool flag = (GameState.Get() != null) && GameState.Get().IsFriendlySidePlayerTurn();
            UserUI rui = !flag ? this.enemyWantedUI : this.friendlyWantedUI;
            UserUI rui2 = !flag ? this.enemyActualUI : this.friendlyActualUI;
            if (((((rui.origin.card != null) && (rui2.over.card != null)) && (rui2.over.card.GetActor() != null)) && rui2.over.card.GetActor().IsShown()) && (rui2.over.card != rui.origin.card))
            {
                Player currentPlayer = GameState.Get().GetCurrentPlayer();
                if ((currentPlayer != null) && !currentPlayer.IsLocalUser())
                {
                    rui2.origin.card = rui.origin.card;
                    if (flag)
                    {
                        TargetReticleManager.Get().CreateFriendlyTargetArrow(rui2.origin.entity, rui2.origin.entity, false, true, null, false);
                    }
                    else
                    {
                        TargetReticleManager.Get().CreateEnemyTargetArrow(rui2.origin.entity);
                    }
                    if (rui2.origin.entity.HasTag(GAME_TAG.IMMUNE_WHILE_ATTACKING))
                    {
                        rui2.origin.card.ActivateActorSpell(SpellType.IMMUNE);
                    }
                    this.SetArrowTarget();
                }
            }
        }
    }

    private void MaybeDestroyArrow()
    {
        if ((TargetReticleManager.Get() != null) && TargetReticleManager.Get().IsActive())
        {
            bool flag = (GameState.Get() != null) && GameState.Get().IsFriendlySidePlayerTurn();
            UserUI rui = !flag ? this.enemyWantedUI : this.friendlyWantedUI;
            UserUI rui2 = !flag ? this.enemyActualUI : this.friendlyActualUI;
            if (rui.origin.card != rui2.origin.card)
            {
                if (rui2.origin.entity.HasTag(GAME_TAG.IMMUNE_WHILE_ATTACKING) && !rui2.origin.entity.IsImmune())
                {
                    rui2.origin.card.GetActor().DeactivateSpell(SpellType.IMMUNE);
                }
                rui2.origin.card = null;
                if (flag)
                {
                    TargetReticleManager.Get().DestroyFriendlyTargetArrow(false);
                }
                else
                {
                    TargetReticleManager.Get().DestroyEnemyTargetArrow();
                }
            }
        }
    }

    public void NotifyOpponentOfCardDropped()
    {
        this.myCurrentUI.held.card = null;
    }

    public void NotifyOpponentOfCardPickedUp(Card card)
    {
        this.myCurrentUI.held.card = card;
    }

    public void NotifyOpponentOfMouseOut()
    {
        this.myCurrentUI.over.card = null;
    }

    public void NotifyOpponentOfMouseOverEntity(Card card)
    {
        this.myCurrentUI.over.card = card;
    }

    public void NotifyOpponentOfTargetEnd()
    {
        this.myCurrentUI.origin.card = null;
    }

    public void NotifyOpponentOfTargetModeBegin(Card card)
    {
        this.myCurrentUI.origin.card = card;
    }

    private void OnDestroy()
    {
        s_instance = null;
    }

    private void OnTurnChanged(int oldTurn, int newTurn, object userData)
    {
        Player currentPlayer = GameState.Get().GetCurrentPlayer();
        if ((((currentPlayer == null) || currentPlayer.IsLocalUser()) || GameMgr.Get().IsSpectator()) && (TargetReticleManager.Get() != null))
        {
            UserUI friendlyActualUI;
            if (currentPlayer.IsFriendlySide())
            {
                friendlyActualUI = this.friendlyActualUI;
                if (TargetReticleManager.Get().IsEnemyArrowActive())
                {
                    TargetReticleManager.Get().DestroyEnemyTargetArrow();
                }
            }
            else
            {
                friendlyActualUI = this.enemyActualUI;
                if (TargetReticleManager.Get().IsLocalArrowActive())
                {
                    TargetReticleManager.Get().DestroyFriendlyTargetArrow(false);
                }
            }
            if ((((friendlyActualUI.origin != null) && (friendlyActualUI.origin.entity != null)) && ((friendlyActualUI.origin.card != null) && friendlyActualUI.origin.entity.HasTag(GAME_TAG.IMMUNE_WHILE_ATTACKING))) && !friendlyActualUI.origin.entity.IsImmune())
            {
                friendlyActualUI.origin.card.GetActor().DeactivateSpell(SpellType.IMMUNE);
            }
        }
    }

    private void SetArrowTarget()
    {
        bool flag = (GameState.Get() != null) && GameState.Get().IsFriendlySidePlayerTurn();
        UserUI rui = !flag ? this.enemyWantedUI : this.friendlyWantedUI;
        UserUI rui2 = !flag ? this.enemyActualUI : this.friendlyActualUI;
        if ((((rui2.over.card != null) && (rui2.over.card.GetActor() != null)) && rui2.over.card.GetActor().IsShown()) && (rui2.over.card != rui.origin.card))
        {
            RaycastHit hit;
            Vector3 position = Camera.main.transform.position;
            Vector3 vector2 = rui2.over.card.transform.position;
            Ray ray = new Ray(position, vector2 - position);
            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, GameLayer.DragPlane.LayerBit()))
            {
                TargetReticleManager.Get().SetRemotePlayerArrowPosition(hit.point);
            }
        }
    }

    private void StandUpright(bool isFriendlySide)
    {
        Card card = !isFriendlySide ? this.enemyActualUI.held.card : this.friendlyActualUI.held.card;
        if (this.CanAnimateHeldCard(card))
        {
            float num = 5f;
            if (!isFriendlySide && SpectatorManager.Get().IsSpectatingOpposingSide())
            {
                num = 0.3f;
            }
            object[] args = new object[] { "name", "RemoteActionHandler", "rotation", Vector3.zero, "time", num, "easetype", iTween.EaseType.easeInOutSine };
            Hashtable hashtable = iTween.Hash(args);
            iTween.RotateTo(card.gameObject, hashtable);
        }
    }

    private void StartDrift(bool isFriendlySide)
    {
        if (isFriendlySide || !SpectatorManager.Get().IsSpectatingOpposingSide())
        {
            this.StandUpright(isFriendlySide);
        }
        this.DriftLeftAndRight(isFriendlySide);
    }

    private void Update()
    {
        if (TargetReticleManager.Get() != null)
        {
            TargetReticleManager.Get().UpdateArrowPosition();
        }
        if (!this.myCurrentUI.SameAs(this.myLastUI) && this.CanSendUI())
        {
            Network.Get().SendUserUI(this.myCurrentUI.over.ID, this.myCurrentUI.held.ID, this.myCurrentUI.origin.ID, 0, 0);
            this.myLastUI.CopyFrom(this.myCurrentUI);
        }
    }

    private void UpdateCardHeld()
    {
        Card card = this.enemyActualUI.held.card;
        Card card2 = this.enemyWantedUI.held.card;
        if (card != card2)
        {
            this.enemyActualUI.held.card = card2;
            if (card != null)
            {
                card.MarkAsGrabbedByEnemyActionHandler(false);
            }
            if (this.IsCardInHand(card))
            {
                card.GetZone().UpdateLayout();
            }
            if (this.CanAnimateHeldCard(card2))
            {
                card2.MarkAsGrabbedByEnemyActionHandler(true);
                if (SpectatorManager.Get().IsSpectatingOpposingSide())
                {
                    this.StandUpright(false);
                }
                object[] args = new object[] { "name", "RemoteActionHandler", "position", Board.Get().FindBone("OpponentCardPlayingSpot").position, "time", 1f, "oncomplete", o => this.StartDrift(false), "oncompletetarget", base.gameObject };
                Hashtable hashtable = iTween.Hash(args);
                iTween.MoveTo(card2.gameObject, hashtable);
            }
        }
        if (GameMgr.Get().IsSpectator())
        {
            Card card3 = this.friendlyActualUI.held.card;
            Card card4 = this.friendlyWantedUI.held.card;
            if (card3 != card4)
            {
                this.friendlyActualUI.held.card = card4;
                if (card3 != null)
                {
                    card3.MarkAsGrabbedByEnemyActionHandler(false);
                }
                if (this.IsCardInHand(card3))
                {
                    card3.GetZone().UpdateLayout();
                }
                if (this.CanAnimateHeldCard(card4))
                {
                    Hashtable hashtable2;
                    card4.MarkAsGrabbedByEnemyActionHandler(true);
                    if (card4 == this.GetFriendlyHoverCard())
                    {
                        ZoneHand zone = card4.GetZone() as ZoneHand;
                        if (zone != null)
                        {
                            card4.NotifyMousedOut();
                            Vector3 cardScale = zone.GetCardScale(card4);
                            object[] objArray2 = new object[] { "scale", cardScale, "time", 0.15f, "easeType", iTween.EaseType.easeOutExpo, "name", "RemoteActionHandler" };
                            hashtable2 = iTween.Hash(objArray2);
                            iTween.ScaleTo(card4.gameObject, hashtable2);
                        }
                    }
                    object[] objArray3 = new object[] { "name", "RemoteActionHandler", "position", Board.Get().FindBone("FriendlyCardPlayingSpot").position, "time", 1f, "oncomplete", o => this.StartDrift(true), "oncompletetarget", base.gameObject };
                    hashtable2 = iTween.Hash(objArray3);
                    iTween.MoveTo(card4.gameObject, hashtable2);
                }
            }
        }
    }

    private void UpdateCardOver()
    {
        Card card = this.enemyActualUI.over.card;
        Card card2 = this.enemyWantedUI.over.card;
        if (card != card2)
        {
            this.enemyActualUI.over.card = card2;
            if (card != null)
            {
                card.NotifyOpponentMousedOffThisCard();
            }
            if (card2 != null)
            {
                card2.NotifyOpponentMousedOverThisCard();
            }
            ZoneMgr.Get().FindZoneOfType<ZoneHand>(Player.Side.OPPOSING).UpdateLayout(this.GetOpponentHandHoverSlot());
        }
        if (GameMgr.Get().IsSpectator())
        {
            Card card3 = this.friendlyActualUI.over.card;
            Card card4 = this.friendlyWantedUI.over.card;
            if (card3 != card4)
            {
                this.friendlyActualUI.over.card = card4;
                if (card3 != null)
                {
                    ZoneHand zone = card3.GetZone() as ZoneHand;
                    if (zone != null)
                    {
                        if (zone.CurrentStandIn == null)
                        {
                            zone.UpdateLayout(-1);
                        }
                    }
                    else
                    {
                        card3.NotifyMousedOut();
                    }
                }
                if (card4 != null)
                {
                    ZoneHand hand2 = card4.GetZone() as ZoneHand;
                    if (hand2 != null)
                    {
                        if (hand2.CurrentStandIn == null)
                        {
                            int num = hand2.FindCardPos(card4);
                            if (num >= 1)
                            {
                                hand2.UpdateLayout(num - 1);
                            }
                        }
                    }
                    else
                    {
                        card4.NotifyMousedOver();
                    }
                }
            }
        }
    }

    private void UpdateTargetArrow()
    {
        if ((TargetReticleManager.Get() != null) && TargetReticleManager.Get().IsActive())
        {
            this.SetArrowTarget();
        }
    }

    private class CardAndID
    {
        private Card m_card;
        private Entity m_entity;
        private int m_ID;

        private void Clear()
        {
            this.m_ID = 0;
            this.m_entity = null;
            this.m_card = null;
        }

        public Card card
        {
            get
            {
                return this.m_card;
            }
            set
            {
                if (value != this.m_card)
                {
                    if (value == null)
                    {
                        this.Clear();
                    }
                    else
                    {
                        this.m_card = value;
                        this.m_entity = value.GetEntity();
                        if (this.m_entity == null)
                        {
                            Debug.LogWarning("RemoteActionHandler--card has no entity");
                            this.Clear();
                        }
                        else
                        {
                            this.m_ID = this.m_entity.GetEntityId();
                            if (this.m_ID < 1)
                            {
                                Debug.LogWarning("RemoteActionHandler--invalid entity ID");
                                this.Clear();
                            }
                        }
                    }
                }
            }
        }

        public Entity entity
        {
            get
            {
                return this.m_entity;
            }
        }

        public int ID
        {
            get
            {
                return this.m_ID;
            }
            set
            {
                if (value != this.m_ID)
                {
                    if (value == 0)
                    {
                        this.Clear();
                    }
                    else
                    {
                        this.m_ID = value;
                        this.m_entity = GameState.Get().GetEntity(value);
                        if (this.m_entity == null)
                        {
                            Debug.LogWarning("RemoteActionHandler--no entity found for ID");
                            this.Clear();
                        }
                        else
                        {
                            this.m_card = this.m_entity.GetCard();
                            if (this.m_card == null)
                            {
                                Debug.LogWarning("RemoteActionHandler--entity has no card");
                                this.Clear();
                            }
                        }
                    }
                }
            }
        }
    }

    private class UserUI
    {
        public RemoteActionHandler.CardAndID held = new RemoteActionHandler.CardAndID();
        public RemoteActionHandler.CardAndID origin = new RemoteActionHandler.CardAndID();
        public RemoteActionHandler.CardAndID over = new RemoteActionHandler.CardAndID();

        public void CopyFrom(RemoteActionHandler.UserUI source)
        {
            this.held.ID = source.held.ID;
            this.over.ID = source.over.ID;
            this.origin.ID = source.origin.ID;
        }

        public bool SameAs(RemoteActionHandler.UserUI compare)
        {
            if (this.held.card != compare.held.card)
            {
                return false;
            }
            if (this.over.card != compare.over.card)
            {
                return false;
            }
            if (this.origin.card != compare.origin.card)
            {
                return false;
            }
            return true;
        }
    }
}

