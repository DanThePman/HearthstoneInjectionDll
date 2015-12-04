using System;
using UnityEngine;

public class KeywordHelpPanel : MonoBehaviour
{
    public static PlatformDependentValue<float> BOX_SCALE;
    public static PlatformDependentValue<float> COLLECTION_MANAGER_SCALE;
    public const float DECK_HELPER_SCALE = 3.75f;
    public static PlatformDependentValue<float> FORGE_SCALE;
    public const float GAMEPLAY_HERO_POWER_SCALE = 0.6f;
    public static readonly PlatformDependentValue<float> GAMEPLAY_SCALE;
    public static readonly PlatformDependentValue<float> GAMEPLAY_SCALE_LARGE;
    public static readonly PlatformDependentValue<float> HAND_SCALE;
    public static PlatformDependentValue<float> HISTORY_SCALE;
    public NewThreeSliceElement m_background;
    public UberText m_body;
    private float m_initialBackgroundHeight;
    private Vector3 m_initialBackgroundScale = Vector3.zero;
    public UberText m_name;
    public static PlatformDependentValue<float> MULLIGAN_SCALE;
    public const float PACK_OPENING_SCALE = 2.75f;
    private float scaleToUse = ((float) GAMEPLAY_SCALE);
    public const float UNOPENED_PACK_SCALE = 5f;

    static KeywordHelpPanel()
    {
        PlatformDependentValue<float> value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 0.65f,
            Phone = 0.8f
        };
        HAND_SCALE = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 0.75f,
            Phone = 1.4f
        };
        GAMEPLAY_SCALE = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 0.9f,
            Phone = 1.25f
        };
        GAMEPLAY_SCALE_LARGE = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 8f,
            Phone = 4.5f
        };
        BOX_SCALE = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 0.48f,
            Phone = 0.853f
        };
        HISTORY_SCALE = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 0.65f,
            Phone = 0.4f
        };
        MULLIGAN_SCALE = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 4f,
            Phone = 8f
        };
        COLLECTION_MANAGER_SCALE = value2;
        value2 = new PlatformDependentValue<float>(PlatformCategory.Screen) {
            PC = 4f,
            Phone = 8f
        };
        FORGE_SCALE = value2;
    }

    private void Awake()
    {
        SceneUtils.SetLayer(base.gameObject, GameLayer.Tooltip);
    }

    public float GetHeight()
    {
        return ((this.m_background.m_leftOrTop.GetComponent<Renderer>().bounds.size.z + this.m_background.m_middle.GetComponent<Renderer>().bounds.size.z) + this.m_background.m_rightOrBottom.GetComponent<Renderer>().bounds.size.z);
    }

    public float GetWidth()
    {
        return this.m_background.m_leftOrTop.GetComponent<Renderer>().bounds.size.x;
    }

    public void Initialize(string keywordName, string keywordText)
    {
        this.SetName(keywordName);
        this.SetBodyText(keywordText);
        base.gameObject.SetActive(true);
        this.m_name.UpdateNow();
        this.m_body.UpdateNow();
        float height = this.m_name.Height;
        float y = this.m_body.GetTextBounds().size.y;
        if (keywordText == string.Empty)
        {
            y = 0f;
        }
        float num3 = 1f;
        if ((this.m_initialBackgroundHeight == 0f) || (this.m_initialBackgroundScale == Vector3.zero))
        {
            this.m_initialBackgroundHeight = this.m_background.m_middle.GetComponent<Renderer>().bounds.size.z;
            this.m_initialBackgroundScale = this.m_background.m_middle.transform.localScale;
        }
        float num4 = (height + y) * num3;
        this.m_background.SetSize(new Vector3(this.m_initialBackgroundScale.x, (this.m_initialBackgroundScale.y * num4) / this.m_initialBackgroundHeight, this.m_initialBackgroundScale.z));
    }

    public bool IsTextRendered()
    {
        return (this.m_name.IsDone() && this.m_body.IsDone());
    }

    private void OnDestroy()
    {
        UnityEngine.Object.Destroy(this.m_name);
        this.m_name = null;
        UnityEngine.Object.Destroy(this.m_body);
        this.m_body = null;
        UnityEngine.Object.Destroy(this.m_background);
        this.m_background = null;
    }

    public void Reset()
    {
        base.transform.localScale = Vector3.one;
        base.transform.eulerAngles = Vector3.zero;
    }

    public void SetBodyText(string s)
    {
        this.m_body.Text = s;
    }

    public void SetName(string s)
    {
        this.m_name.Text = s;
    }

    public void SetScale(float newScale)
    {
        this.scaleToUse = newScale;
        base.transform.localScale = new Vector3(this.scaleToUse, this.scaleToUse, this.scaleToUse);
    }
}

