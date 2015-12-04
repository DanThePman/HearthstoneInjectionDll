using System;
using UnityEngine;

public class HistoryItem : MonoBehaviour
{
    protected bool actorHasBeenInitialized;
    protected bool dead;
    protected bool isFatigue;
    protected Texture m_bigCardPortraitTexture;
    protected Material m_bigCardPotraitGoldenMaterial;
    protected Entity m_entity;
    public Actor m_mainCardActor;
    protected int m_splatAmount;

    private void DisplaySkullOnActor(Actor actor)
    {
        Spell spell = actor.GetSpell(SpellType.SKULL);
        if (spell != null)
        {
            spell.ActivateState(SpellStateType.IDLE);
        }
    }

    public void DisplaySpells()
    {
        if (!this.isFatigue && (this.m_entity.IsCharacter() || this.m_entity.IsWeapon()))
        {
            int remainingHP = this.m_entity.GetRemainingHP();
            if (this.dead || (this.m_splatAmount >= remainingHP))
            {
                this.DisplaySkullOnActor(this.m_mainCardActor);
            }
            else if (this.m_splatAmount != 0)
            {
                this.DisplaySplatOnActor(this.m_mainCardActor, this.m_splatAmount);
            }
        }
    }

    private void DisplaySplatOnActor(Actor actor, int damage)
    {
        Spell spell = actor.GetSpell(SpellType.DAMAGE);
        if (spell != null)
        {
            DamageSplatSpell spell2 = (DamageSplatSpell) spell;
            spell2.SetDamage(damage);
            spell2.ActivateState(SpellStateType.IDLE);
        }
    }

    public Texture GetBigCardPortraitTexture()
    {
        return this.m_bigCardPortraitTexture;
    }

    public Material GetBigCardPotraitGoldenMaterial()
    {
        return this.m_bigCardPotraitGoldenMaterial;
    }

    public Entity GetEntity()
    {
        return this.m_entity;
    }

    public void InitializeActor()
    {
        this.m_mainCardActor.TurnOffCollider();
        this.m_mainCardActor.SetActorState(ActorStateType.CARD_HISTORY);
        this.actorHasBeenInitialized = true;
    }

    public bool IsInitialized()
    {
        return this.actorHasBeenInitialized;
    }
}

