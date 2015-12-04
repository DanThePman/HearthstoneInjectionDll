using System;
using UnityEngine;

public class MobileActorGem : MonoBehaviour
{
    public GemType m_gemType;
    public UberText m_uberText;

    private void Awake()
    {
        if ((PlatformSettings.OS == OSCategory.iOS) || (PlatformSettings.OS == OSCategory.Android))
        {
            if (UniversalInputManager.UsePhoneUI != null)
            {
                if (this.m_gemType == GemType.CardPlay)
                {
                    Transform transform = base.gameObject.transform;
                    transform.localScale = (Vector3) (transform.localScale * 1.6f);
                    Transform transform2 = this.m_uberText.transform;
                    transform2.localScale = (Vector3) (transform2.localScale * 0.9f);
                    this.m_uberText.OutlineSize = 3.2f;
                }
                else if (this.m_gemType == GemType.CardHero_Attack)
                {
                    Transform transform3 = base.gameObject.transform;
                    transform3.localScale = (Vector3) (transform3.localScale * 1.6f);
                    TransformUtil.SetLocalPosX(base.gameObject, base.gameObject.transform.localPosition.x - 0.075f);
                    TransformUtil.SetLocalPosZ(base.gameObject, base.gameObject.transform.localPosition.z + 0.255f);
                    Transform transform4 = this.m_uberText.transform;
                    transform4.localScale = (Vector3) (transform4.localScale * 0.9f);
                    this.m_uberText.OutlineSize = 3.2f;
                }
                else if (this.m_gemType == GemType.CardHero_Health)
                {
                    Transform transform5 = base.gameObject.transform;
                    transform5.localScale = (Vector3) (transform5.localScale * 1.6f);
                    TransformUtil.SetLocalPosX(base.gameObject, base.gameObject.transform.localPosition.x + 0.05f);
                    TransformUtil.SetLocalPosZ(base.gameObject, base.gameObject.transform.localPosition.z + 0.255f);
                    this.m_uberText.transform.localPosition = new Vector3(0f, 0.154f, -0.0235f);
                    this.m_uberText.OutlineSize = 3.6f;
                }
                else if (this.m_gemType == GemType.CardHero_Armor)
                {
                    Transform transform6 = base.gameObject.transform;
                    transform6.localScale = (Vector3) (transform6.localScale * 1.15f);
                    TransformUtil.SetLocalPosX(base.gameObject, 0.06f);
                    TransformUtil.SetLocalPosZ(base.gameObject, base.gameObject.transform.localPosition.z - 0.3f);
                    Transform transform7 = this.m_uberText.transform;
                    transform7.localScale = (Vector3) (transform7.localScale * 1.4f);
                    this.m_uberText.FontSize = 50;
                    this.m_uberText.CharacterSize = 8f;
                    this.m_uberText.OutlineSize = 3.2f;
                }
                else if (this.m_gemType == GemType.CardHeroPower)
                {
                    TransformUtil.SetLocalScaleXZ(base.gameObject, new Vector2(1.334f * base.gameObject.transform.localScale.x, 1.334f * base.gameObject.transform.localScale.z));
                    TransformUtil.SetLocalScaleXY(this.m_uberText, new Vector2(1.5f * this.m_uberText.transform.localScale.x, 1.5f * this.m_uberText.transform.localScale.y));
                    TransformUtil.SetLocalPosZ(this.m_uberText, this.m_uberText.transform.localPosition.z + 0.04f);
                }
            }
            else if (this.m_gemType == GemType.CardPlay)
            {
                Transform transform8 = base.gameObject.transform;
                transform8.localScale = (Vector3) (transform8.localScale * 1.3f);
                Transform transform9 = this.m_uberText.transform;
                transform9.localScale = (Vector3) (transform9.localScale * 0.9f);
                this.m_uberText.OutlineSize = 3.2f;
            }
        }
    }

    public enum GemType
    {
        CardPlay,
        CardHero_Health,
        CardHero_Attack,
        CardHero_Armor,
        CardHeroPower
    }
}

