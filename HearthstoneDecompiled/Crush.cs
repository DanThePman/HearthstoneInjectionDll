using System;
using UnityEngine;

public class Crush : Spell
{
    public UberText m_attack;
    public UberText m_health;
    public MinionPieces m_minionPieces;
    public Material m_premiumEliteMaterial;
    public Material m_premiumTauntMaterial;

    protected override void OnAction(SpellStateType prevStateType)
    {
        base.OnAction(prevStateType);
        Entity entity = base.GetSourceCard().GetEntity();
        Actor actor = SceneUtils.FindComponentInParents<Actor>(this);
        GameObject main = this.m_minionPieces.m_main;
        bool flag = entity.HasTag(GAME_TAG.PREMIUM);
        if (flag)
        {
            main = this.m_minionPieces.m_premium;
            SceneUtils.EnableRenderers(this.m_minionPieces.m_main, false);
        }
        GameObject portraitMesh = actor.GetPortraitMesh();
        main.GetComponent<Renderer>().material = portraitMesh.GetComponent<Renderer>().sharedMaterial;
        main.SetActive(true);
        SceneUtils.EnableRenderers(main, true);
        if (entity.HasTaunt())
        {
            if (flag)
            {
                this.m_minionPieces.m_taunt.GetComponent<Renderer>().material = this.m_premiumTauntMaterial;
            }
            this.m_minionPieces.m_taunt.SetActive(true);
            SceneUtils.EnableRenderers(this.m_minionPieces.m_taunt, true);
        }
        if (entity.IsElite())
        {
            if (flag)
            {
                this.m_minionPieces.m_legendary.GetComponent<Renderer>().material = this.m_premiumEliteMaterial;
            }
            this.m_minionPieces.m_legendary.SetActive(true);
            SceneUtils.EnableRenderers(this.m_minionPieces.m_legendary, true);
        }
        this.m_attack.SetGameStringText(entity.GetATK().ToString());
        this.m_health.SetGameStringText(entity.GetHealth().ToString());
    }
}

