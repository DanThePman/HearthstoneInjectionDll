using System;
using UnityEngine;

public class ArenaMedal : PegUIElement
{
    private int m_medal;
    public GameObject m_rankMedal;
    public const string MEDAL_NAME_PREFIX = "GLOBAL_ARENA_MEDAL_";
    public const string MEDAL_TEXTURE_PREFIX = "Medal_Key_";

    protected override void Awake()
    {
        base.Awake();
        this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.MedalOver));
        this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.MedalOut));
    }

    private string GetMedalName()
    {
        return GameStrings.Get("GLOBAL_ARENA_MEDAL_" + this.m_medal);
    }

    private string GetNextMedalName()
    {
        string key = "GLOBAL_ARENA_MEDAL_" + (this.m_medal + 1);
        string str2 = GameStrings.Get(key);
        if (str2 == key)
        {
            return string.Empty;
        }
        return str2;
    }

    private void MedalOut(UIEvent e)
    {
        base.gameObject.GetComponent<TooltipZone>().HideTooltip();
    }

    private void MedalOver(UIEvent e)
    {
        string medalName;
        string str2;
        bool flag = SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB);
        if (Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE) || flag)
        {
            medalName = this.GetMedalName();
            object[] args = new object[] { this.m_medal };
            str2 = GameStrings.Format("GLOBAL_MEDAL_ARENA_TOOLTIP_BODY", args);
        }
        else
        {
            medalName = GameStrings.Get("GLUE_TOURNAMENT_UNRANKED_MODE");
            str2 = GameStrings.Get("GLUE_TOURNAMENT_UNRANKED_DESC");
        }
        base.gameObject.GetComponent<TooltipZone>().ShowLayerTooltip(medalName, str2);
    }

    private void OnTextureLoaded(string assetName, UnityEngine.Object asset, object callbackData)
    {
        if (asset == null)
        {
            Debug.LogWarning(string.Format("ArenaMedal.OnTextureLoaded(): asset for {0} is null!", assetName));
        }
        else
        {
            Texture texture = asset as Texture;
            if (texture == null)
            {
                Debug.LogWarning(string.Format("ArenaMedal.OnTextureLoaded(): medalTexture for {0} is null (asset is not a texture)!", assetName));
            }
            else
            {
                this.m_rankMedal.GetComponent<Renderer>().material.mainTexture = texture;
            }
        }
    }

    public void SetMedal(int medal)
    {
        this.m_medal = medal;
        AssetLoader.Get().LoadTexture("Medal_Key_" + (medal + 1), new AssetLoader.ObjectCallback(this.OnTextureLoaded), null, false);
    }
}

