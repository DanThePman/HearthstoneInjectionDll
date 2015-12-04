using PegasusGame;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellUtils
{
    public static bool CanAddPowerTargets(PowerTaskList taskList)
    {
        if (IsNonMetaTaskListInMetaBlock(taskList))
        {
            return false;
        }
        if (!taskList.HasTasks() && !taskList.IsEndOfBlock())
        {
            return false;
        }
        return true;
    }

    public static SpellClassTag ConvertClassTagToSpellEnum(TAG_CLASS classTag)
    {
        switch (classTag)
        {
            case TAG_CLASS.DEATHKNIGHT:
                return SpellClassTag.DEATHKNIGHT;

            case TAG_CLASS.DRUID:
                return SpellClassTag.DRUID;

            case TAG_CLASS.HUNTER:
                return SpellClassTag.HUNTER;

            case TAG_CLASS.MAGE:
                return SpellClassTag.MAGE;

            case TAG_CLASS.PALADIN:
                return SpellClassTag.PALADIN;

            case TAG_CLASS.PRIEST:
                return SpellClassTag.PRIEST;

            case TAG_CLASS.ROGUE:
                return SpellClassTag.ROGUE;

            case TAG_CLASS.SHAMAN:
                return SpellClassTag.SHAMAN;

            case TAG_CLASS.WARLOCK:
                return SpellClassTag.WARLOCK;

            case TAG_CLASS.WARRIOR:
                return SpellClassTag.WARRIOR;
        }
        return SpellClassTag.NONE;
    }

    public static Player.Side ConvertSpellSideToPlayerSide(Spell spell, SpellPlayerSide spellSide)
    {
        Entity entity = spell.GetSourceCard().GetEntity();
        switch (spellSide)
        {
            case SpellPlayerSide.FRIENDLY:
                return Player.Side.FRIENDLY;

            case SpellPlayerSide.OPPONENT:
                return Player.Side.OPPOSING;

            case SpellPlayerSide.SOURCE:
                if (!entity.IsControlledByFriendlySidePlayer())
                {
                    return Player.Side.OPPOSING;
                }
                return Player.Side.FRIENDLY;

            case SpellPlayerSide.TARGET:
                if (!entity.IsControlledByFriendlySidePlayer())
                {
                    return Player.Side.FRIENDLY;
                }
                return Player.Side.OPPOSING;
        }
        return Player.Side.NEUTRAL;
    }

    private static void FaceOppositeOf(Component source, Component target, SpellFacingOptions options)
    {
        FaceOppositeOf(source.transform, target.transform, options);
    }

    private static void FaceOppositeOf(Component source, GameObject target, SpellFacingOptions options)
    {
        FaceOppositeOf(source.transform, target.transform, options);
    }

    private static void FaceOppositeOf(GameObject source, Component target, SpellFacingOptions options)
    {
        FaceOppositeOf(source.transform, target.transform, options);
    }

    private static void FaceOppositeOf(GameObject source, GameObject target, SpellFacingOptions options)
    {
        FaceOppositeOf(source.transform, target.transform, options);
    }

    private static void FaceOppositeOf(Transform source, Transform target, SpellFacingOptions options)
    {
        SetOrientation(source, target.position, target.position - target.forward, options);
    }

    private static void FaceSameAs(Component source, Component target, SpellFacingOptions options)
    {
        FaceSameAs(source.transform, target.transform, options);
    }

    private static void FaceSameAs(Component source, GameObject target, SpellFacingOptions options)
    {
        FaceSameAs(source.transform, target.transform, options);
    }

    private static void FaceSameAs(GameObject source, Component target, SpellFacingOptions options)
    {
        FaceSameAs(source.transform, target.transform, options);
    }

    private static void FaceSameAs(GameObject source, GameObject target, SpellFacingOptions options)
    {
        FaceSameAs(source.transform, target.transform, options);
    }

    private static void FaceSameAs(Transform source, Transform target, SpellFacingOptions options)
    {
        SetOrientation(source, target.position, target.position + target.forward, options);
    }

    private static void FaceTowards(Component source, Component target, SpellFacingOptions options)
    {
        FaceTowards(source.transform, target.transform, options);
    }

    private static void FaceTowards(Component source, GameObject target, SpellFacingOptions options)
    {
        FaceTowards(source.transform, target.transform, options);
    }

    private static void FaceTowards(GameObject source, Component target, SpellFacingOptions options)
    {
        FaceTowards(source.transform, target.transform, options);
    }

    private static void FaceTowards(GameObject source, GameObject target, SpellFacingOptions options)
    {
        FaceTowards(source.transform, target.transform, options);
    }

    private static void FaceTowards(Transform source, Transform target, SpellFacingOptions options)
    {
        SetOrientation(source, source.position, target.position, options);
    }

    private static GameObject FindAutoObjectForSpell(GameObject cardObject)
    {
        if (cardObject == null)
        {
            return null;
        }
        Card component = cardObject.GetComponent<Card>();
        if (component == null)
        {
            return cardObject;
        }
        Entity entity = component.GetEntity();
        TAG_CARDTYPE cardType = entity.GetCardType();
        if (cardType == TAG_CARDTYPE.SPELL)
        {
            Card heroCard = entity.GetController().GetHeroCard();
            if (heroCard == null)
            {
                return cardObject;
            }
            return heroCard.gameObject;
        }
        if (cardType != TAG_CARDTYPE.HERO_POWER)
        {
            return cardObject;
        }
        Card heroPowerCard = entity.GetController().GetHeroPowerCard();
        if (heroPowerCard == null)
        {
            return cardObject;
        }
        return heroPowerCard.gameObject;
    }

    private static Card FindBestTargetCard(Spell spell)
    {
        Card sourceCard = spell.GetSourceCard();
        if (sourceCard != null)
        {
            Player controller = sourceCard.GetEntity().GetController();
            if (controller == null)
            {
                return spell.GetVisualTargetCard();
            }
            Player.Side side = controller.GetSide();
            List<GameObject> visualTargets = spell.GetVisualTargets();
            for (int i = 0; i < visualTargets.Count; i++)
            {
                Card component = visualTargets[i].GetComponent<Card>();
                if ((component != null) && (component.GetEntity().GetController().GetSide() != side))
                {
                    return component;
                }
            }
        }
        return spell.GetVisualTargetCard();
    }

    public static Player FindFriendlyPlayer(Spell spell)
    {
        if (spell == null)
        {
            return null;
        }
        Card sourceCard = spell.GetSourceCard();
        if (sourceCard == null)
        {
            return null;
        }
        return sourceCard.GetEntity().GetController();
    }

    public static ZonePlay FindFriendlyPlayZone(Spell spell)
    {
        Player player = FindFriendlyPlayer(spell);
        if (player == null)
        {
            return null;
        }
        return ZoneMgr.Get().FindZoneOfType<ZonePlay>(player.GetSide());
    }

    private static Card FindHeroCard(Card card)
    {
        if (card == null)
        {
            return null;
        }
        Player controller = card.GetEntity().GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetHeroCard();
    }

    private static Card FindHeroPowerCard(Card card)
    {
        if (card == null)
        {
            return null;
        }
        Player controller = card.GetEntity().GetController();
        if (controller == null)
        {
            return null;
        }
        return controller.GetHeroPowerCard();
    }

    public static Player FindOpponentPlayer(Spell spell)
    {
        Player player = FindFriendlyPlayer(spell);
        if (player == null)
        {
            return null;
        }
        return GameState.Get().GetFirstOpponentPlayer(player);
    }

    public static ZonePlay FindOpponentPlayZone(Spell spell)
    {
        Player player = FindOpponentPlayer(spell);
        if (player == null)
        {
            return null;
        }
        return ZoneMgr.Get().FindZoneOfType<ZonePlay>(player.GetSide());
    }

    public static Zone FindTargetZone(Spell spell)
    {
        Card targetCard = spell.GetTargetCard();
        if (targetCard == null)
        {
            return null;
        }
        Entity entity = targetCard.GetEntity();
        return ZoneMgr.Get().FindZoneForEntity(entity);
    }

    public static List<Zone> FindZonesFromTag(SpellZoneTag zoneTag)
    {
        ZoneMgr mgr = ZoneMgr.Get();
        if (mgr != null)
        {
            switch (zoneTag)
            {
                case SpellZoneTag.PLAY:
                    return mgr.FindZonesOfType<Zone, ZonePlay>();

                case SpellZoneTag.HERO:
                    return mgr.FindZonesOfType<Zone, ZoneHero>();

                case SpellZoneTag.HERO_POWER:
                    return mgr.FindZonesOfType<Zone, ZoneHeroPower>();

                case SpellZoneTag.WEAPON:
                    return mgr.FindZonesOfType<Zone, ZoneWeapon>();

                case SpellZoneTag.DECK:
                    return mgr.FindZonesOfType<Zone, ZoneDeck>();

                case SpellZoneTag.HAND:
                    return mgr.FindZonesOfType<Zone, ZoneHand>();

                case SpellZoneTag.GRAVEYARD:
                    return mgr.FindZonesOfType<Zone, ZoneGraveyard>();

                case SpellZoneTag.SECRET:
                    return mgr.FindZonesOfType<Zone, ZoneSecret>();
            }
            Debug.LogWarning(string.Format("SpellUtils.FindZonesFromTag() - unhandled zoneTag {0}", zoneTag));
        }
        return null;
    }

    public static List<Zone> FindZonesFromTag(Spell spell, SpellZoneTag zoneTag, SpellPlayerSide spellSide)
    {
        if (ZoneMgr.Get() != null)
        {
            if (spellSide == SpellPlayerSide.NEUTRAL)
            {
                return null;
            }
            if (spellSide == SpellPlayerSide.BOTH)
            {
                return FindZonesFromTag(zoneTag);
            }
            Player.Side side = ConvertSpellSideToPlayerSide(spell, spellSide);
            switch (zoneTag)
            {
                case SpellZoneTag.PLAY:
                    return ZoneMgr.Get().FindZonesOfType<Zone, ZonePlay>(side);

                case SpellZoneTag.HERO:
                    return ZoneMgr.Get().FindZonesOfType<Zone, ZoneHero>(side);

                case SpellZoneTag.HERO_POWER:
                    return ZoneMgr.Get().FindZonesOfType<Zone, ZoneHeroPower>(side);

                case SpellZoneTag.WEAPON:
                    return ZoneMgr.Get().FindZonesOfType<Zone, ZoneWeapon>(side);

                case SpellZoneTag.DECK:
                    return ZoneMgr.Get().FindZonesOfType<Zone, ZoneDeck>(side);

                case SpellZoneTag.HAND:
                    return ZoneMgr.Get().FindZonesOfType<Zone, ZoneHand>(side);

                case SpellZoneTag.GRAVEYARD:
                    return ZoneMgr.Get().FindZonesOfType<Zone, ZoneGraveyard>(side);

                case SpellZoneTag.SECRET:
                    return ZoneMgr.Get().FindZonesOfType<Zone, ZoneSecret>(side);
            }
            Debug.LogWarning(string.Format("SpellUtils.FindZonesFromTag() - Unhandled zoneTag {0}. spellSide={1} playerSide={2}", zoneTag, spellSide, side));
        }
        return null;
    }

    public static GameObject GetLocationObject(Spell spell)
    {
        SpellLocation location = spell.GetLocation();
        if (location == SpellLocation.NONE)
        {
            return null;
        }
        GameObject parentObject = null;
        switch (location)
        {
            case SpellLocation.SOURCE:
                parentObject = spell.GetSource();
                break;

            case SpellLocation.SOURCE_AUTO:
                parentObject = FindAutoObjectForSpell(spell.GetSource());
                break;

            case SpellLocation.SOURCE_HERO:
            {
                Card card2 = FindHeroCard(spell.GetSourceCard());
                if (card2 == null)
                {
                    return null;
                }
                parentObject = card2.gameObject;
                break;
            }
            case SpellLocation.SOURCE_HERO_POWER:
            {
                Card card4 = FindHeroPowerCard(spell.GetSourceCard());
                if (card4 == null)
                {
                    return null;
                }
                parentObject = card4.gameObject;
                break;
            }
            case SpellLocation.SOURCE_PLAY_ZONE:
            {
                Card sourceCard = spell.GetSourceCard();
                if (sourceCard == null)
                {
                    return null;
                }
                Player controller = sourceCard.GetEntity().GetController();
                ZonePlay play = ZoneMgr.Get().FindZoneOfType<ZonePlay>(controller.GetSide());
                if (play == null)
                {
                    return null;
                }
                parentObject = play.gameObject;
                break;
            }
            case SpellLocation.TARGET:
                parentObject = spell.GetVisualTarget();
                break;

            case SpellLocation.TARGET_AUTO:
                parentObject = FindAutoObjectForSpell(spell.GetVisualTarget());
                break;

            case SpellLocation.TARGET_HERO:
            {
                Card card7 = FindHeroCard(spell.GetVisualTargetCard());
                if (card7 == null)
                {
                    return null;
                }
                parentObject = card7.gameObject;
                break;
            }
            case SpellLocation.TARGET_HERO_POWER:
            {
                Card card9 = FindHeroPowerCard(spell.GetVisualTargetCard());
                if (card9 == null)
                {
                    return null;
                }
                parentObject = card9.gameObject;
                break;
            }
            case SpellLocation.TARGET_PLAY_ZONE:
            {
                Card visualTargetCard = spell.GetVisualTargetCard();
                if (visualTargetCard == null)
                {
                    return null;
                }
                Player player2 = visualTargetCard.GetEntity().GetController();
                ZonePlay play2 = ZoneMgr.Get().FindZoneOfType<ZonePlay>(player2.GetSide());
                if (play2 == null)
                {
                    return null;
                }
                parentObject = play2.gameObject;
                break;
            }
            case SpellLocation.BOARD:
                if (Board.Get() == null)
                {
                    return null;
                }
                parentObject = Board.Get().gameObject;
                break;

            case SpellLocation.FRIENDLY_HERO:
            {
                Player player3 = FindFriendlyPlayer(spell);
                if (player3 == null)
                {
                    return null;
                }
                Card heroCard = player3.GetHeroCard();
                if (heroCard == null)
                {
                    return null;
                }
                parentObject = heroCard.gameObject;
                break;
            }
            case SpellLocation.FRIENDLY_HERO_POWER:
            {
                Player player4 = FindFriendlyPlayer(spell);
                if (player4 == null)
                {
                    return null;
                }
                Card heroPowerCard = player4.GetHeroPowerCard();
                if (heroPowerCard == null)
                {
                    return null;
                }
                parentObject = heroPowerCard.gameObject;
                break;
            }
            case SpellLocation.FRIENDLY_PLAY_ZONE:
            {
                ZonePlay play3 = FindFriendlyPlayZone(spell);
                if (play3 == null)
                {
                    return null;
                }
                parentObject = play3.gameObject;
                break;
            }
            case SpellLocation.OPPONENT_HERO:
            {
                Player player5 = FindOpponentPlayer(spell);
                if (player5 == null)
                {
                    return null;
                }
                Card card13 = player5.GetHeroCard();
                if (card13 == null)
                {
                    return null;
                }
                parentObject = card13.gameObject;
                break;
            }
            case SpellLocation.OPPONENT_HERO_POWER:
            {
                Player player6 = FindOpponentPlayer(spell);
                if (player6 == null)
                {
                    return null;
                }
                Card card14 = player6.GetHeroPowerCard();
                if (card14 == null)
                {
                    return null;
                }
                parentObject = card14.gameObject;
                break;
            }
            case SpellLocation.OPPONENT_PLAY_ZONE:
            {
                ZonePlay play4 = FindOpponentPlayZone(spell);
                if (play4 == null)
                {
                    return null;
                }
                parentObject = play4.gameObject;
                break;
            }
            case SpellLocation.CHOSEN_TARGET:
            {
                Card powerTargetCard = spell.GetPowerTargetCard();
                if (powerTargetCard == null)
                {
                    return null;
                }
                parentObject = powerTargetCard.gameObject;
                break;
            }
        }
        if (parentObject == null)
        {
            return null;
        }
        string locationTransformName = spell.GetLocationTransformName();
        if (!string.IsNullOrEmpty(locationTransformName))
        {
            GameObject obj3 = SceneUtils.FindChildBySubstring(parentObject, locationTransformName);
            if (obj3 != null)
            {
                return obj3;
            }
        }
        return parentObject;
    }

    public static Transform GetLocationTransform(Spell spell)
    {
        GameObject locationObject = GetLocationObject(spell);
        return ((locationObject != null) ? locationObject.transform : null);
    }

    public static Actor GetParentActor(Spell spell)
    {
        return SceneUtils.FindComponentInThisOrParents<Actor>(spell.gameObject);
    }

    public static GameObject GetParentRootObject(Spell spell)
    {
        Actor parentActor = GetParentActor(spell);
        if (parentActor == null)
        {
            return null;
        }
        return parentActor.GetRootObject();
    }

    public static MeshRenderer GetParentRootObjectMesh(Spell spell)
    {
        Actor parentActor = GetParentActor(spell);
        if (parentActor == null)
        {
            return null;
        }
        return parentActor.GetMeshRenderer();
    }

    public static bool IsNonMetaTaskListInMetaBlock(PowerTaskList taskList)
    {
        if (!taskList.DoesBlockHaveMetaDataTasks(HistoryMeta.Type.TARGET))
        {
            return false;
        }
        if (taskList.HasMetaDataTasks(HistoryMeta.Type.TARGET))
        {
            return false;
        }
        return true;
    }

    public static void SetCustomSpellParent(Spell spell, Component c)
    {
        spell.transform.parent = c.transform;
        spell.transform.localPosition = Vector3.zero;
    }

    private static void SetOrientation(Transform source, Vector3 sourcePosition, Vector3 targetPosition, SpellFacingOptions options)
    {
        if (!options.m_RotateX || !options.m_RotateY)
        {
            if (!options.m_RotateX)
            {
                if (!options.m_RotateY)
                {
                    return;
                }
                targetPosition.y = sourcePosition.y;
            }
            else
            {
                targetPosition.x = sourcePosition.x;
            }
        }
        Vector3 forward = targetPosition - sourcePosition;
        if (forward.sqrMagnitude > Mathf.Epsilon)
        {
            source.rotation = Quaternion.LookRotation(forward);
        }
    }

    public static bool SetOrientationFromFacing(Spell spell)
    {
        SpellFacing facing = spell.GetFacing();
        if (facing == SpellFacing.NONE)
        {
            return false;
        }
        SpellFacingOptions facingOptions = spell.GetFacingOptions();
        if (facingOptions == null)
        {
            facingOptions = new SpellFacingOptions();
        }
        switch (facing)
        {
            case SpellFacing.SAME_AS_SOURCE:
            {
                GameObject source = spell.GetSource();
                if (source == null)
                {
                    return false;
                }
                FaceSameAs(spell, source, facingOptions);
                break;
            }
            case SpellFacing.SAME_AS_SOURCE_AUTO:
            {
                GameObject target = FindAutoObjectForSpell(spell.GetSource());
                if (target == null)
                {
                    return false;
                }
                FaceSameAs(spell, target, facingOptions);
                break;
            }
            case SpellFacing.SAME_AS_SOURCE_HERO:
            {
                Card card2 = FindHeroCard(spell.GetSourceCard());
                if (card2 == null)
                {
                    return false;
                }
                FaceSameAs((Component) spell, (Component) card2, facingOptions);
                break;
            }
            case SpellFacing.TOWARDS_SOURCE:
            {
                GameObject obj4 = spell.GetSource();
                if (obj4 == null)
                {
                    return false;
                }
                FaceTowards(spell, obj4, facingOptions);
                break;
            }
            case SpellFacing.TOWARDS_SOURCE_AUTO:
            {
                GameObject obj5 = FindAutoObjectForSpell(spell.GetSource());
                if (obj5 == null)
                {
                    return false;
                }
                FaceTowards(spell, obj5, facingOptions);
                break;
            }
            case SpellFacing.TOWARDS_SOURCE_HERO:
            {
                Card card4 = FindHeroCard(spell.GetSourceCard());
                if (card4 == null)
                {
                    return false;
                }
                FaceTowards((Component) spell, (Component) card4, facingOptions);
                break;
            }
            case SpellFacing.TOWARDS_TARGET:
            {
                GameObject visualTarget = spell.GetVisualTarget();
                if (visualTarget == null)
                {
                    return false;
                }
                FaceTowards(spell, visualTarget, facingOptions);
                break;
            }
            case SpellFacing.TOWARDS_TARGET_HERO:
            {
                Card card6 = FindHeroCard(FindBestTargetCard(spell));
                if (card6 == null)
                {
                    return false;
                }
                FaceTowards((Component) spell, (Component) card6, facingOptions);
                break;
            }
            case SpellFacing.TOWARDS_CHOSEN_TARGET:
            {
                Card powerTargetCard = spell.GetPowerTargetCard();
                if (powerTargetCard == null)
                {
                    return false;
                }
                FaceTowards((Component) spell, (Component) powerTargetCard, facingOptions);
                break;
            }
            case SpellFacing.OPPOSITE_OF_SOURCE:
            {
                GameObject obj7 = spell.GetSource();
                if (obj7 == null)
                {
                    return false;
                }
                FaceOppositeOf(spell, obj7, facingOptions);
                break;
            }
            case SpellFacing.OPPOSITE_OF_SOURCE_AUTO:
            {
                GameObject obj8 = FindAutoObjectForSpell(spell.GetSource());
                if (obj8 == null)
                {
                    return false;
                }
                FaceOppositeOf(spell, obj8, facingOptions);
                break;
            }
            default:
            {
                if (facing != SpellFacing.OPPOSITE_OF_SOURCE_HERO)
                {
                    return false;
                }
                Card card9 = FindHeroCard(spell.GetSourceCard());
                if (card9 == null)
                {
                    return false;
                }
                FaceOppositeOf((Component) spell, (Component) card9, facingOptions);
                break;
            }
        }
        return true;
    }

    public static bool SetPositionFromLocation(Spell spell)
    {
        return SetPositionFromLocation(spell, false);
    }

    public static bool SetPositionFromLocation(Spell spell, bool setParent)
    {
        Transform locationTransform = GetLocationTransform(spell);
        if (locationTransform == null)
        {
            return false;
        }
        if (setParent)
        {
            spell.transform.parent = locationTransform;
        }
        spell.transform.position = locationTransform.position;
        return true;
    }
}

