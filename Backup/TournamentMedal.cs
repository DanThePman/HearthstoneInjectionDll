using System;
using System.Collections.Generic;
using UnityEngine;

public class TournamentMedal : PegUIElement
{
    public GameObject m_banner;
    public List<Transform> m_evenStarBones;
    public GameObject m_glowPlane;
    public UberText m_legendIndex;
    private MedalInfoTranslator m_medal;
    public List<Transform> m_oddStarBones;
    public GameObject m_rankMedal;
    public UberText m_rankNumber;
    public Material_MobileOverride m_starEmptyMaterial;
    public Material m_starFilledMaterial;
    public GameObject m_starPrefab;
    private List<GameObject> m_stars = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        if (base.gameObject.GetComponent<TooltipZone>() != null)
        {
            this.AddEventListener(UIEventType.ROLLOVER, new UIEvent.Handler(this.MedalOver));
            this.AddEventListener(UIEventType.ROLLOUT, new UIEvent.Handler(this.MedalOut));
        }
    }

    public TranslatedMedalInfo GetMedal()
    {
        return this.m_medal.GetCurrentMedal();
    }

    private void MedalOut(UIEvent e)
    {
        base.gameObject.GetComponent<TooltipZone>().HideTooltip();
    }

    public void MedalOver(UIEvent e)
    {
        string bodytext = string.Empty;
        bool flag = SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB);
        if (Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE) || flag)
        {
            string rankPercentile = SeasonEndDialog.GetRankPercentile(this.m_medal.GetCurrentMedal().rank);
            if (this.m_medal.GetCurrentMedal().rank == 0)
            {
                bodytext = GameStrings.Format("GLOBAL_MEDAL_TOOLTIP_BODY_LEGEND", new object[0]);
            }
            else if (rankPercentile.Length > 0)
            {
                object[] args = new object[] { rankPercentile, this.m_medal.GetCurrentMedal().nextMedalName };
                bodytext = GameStrings.Format("GLOBAL_MEDAL_TOURNAMENT_TOOLTIP_BODY", args);
            }
            else
            {
                object[] objArray2 = new object[] { this.m_medal.GetCurrentMedal().nextMedalName };
                bodytext = GameStrings.Format("GLOBAL_MEDAL_TOOLTIP_BODY", objArray2);
            }
        }
        base.gameObject.GetComponent<TooltipZone>().ShowLayerTooltip(this.m_medal.GetCurrentMedal().name, bodytext);
    }

    private void OnDestroy()
    {
        foreach (GameObject obj2 in this.m_stars)
        {
            UnityEngine.Object.Destroy(obj2);
        }
    }

    private void OnTextureLoaded(string assetName, UnityEngine.Object asset, object callbackData)
    {
        if (asset == null)
        {
            Debug.LogWarning(string.Format("TournamentMedal.OnTextureLoaded(): asset for {0} is null!", assetName));
        }
        else
        {
            Texture texture = asset as Texture;
            if (texture == null)
            {
                Debug.LogWarning(string.Format("TournamentMedal.OnTextureLoaded(): medalTexture for {0} is null (asset is not a texture)!", assetName));
            }
            else
            {
                this.m_rankMedal.GetComponent<Renderer>().enabled = true;
                this.m_rankMedal.GetComponent<Renderer>().material.mainTexture = texture;
                TranslatedMedalInfo currentMedal = this.m_medal.GetCurrentMedal();
                int rank = currentMedal.rank;
                if (rank == 0)
                {
                    this.m_legendIndex.gameObject.SetActive(true);
                    if (currentMedal.legendIndex == 0)
                    {
                        this.m_legendIndex.Text = string.Empty;
                    }
                    else if (currentMedal.legendIndex == -1)
                    {
                        this.m_legendIndex.Text = string.Empty;
                    }
                    else
                    {
                        this.m_legendIndex.Text = currentMedal.legendIndex.ToString();
                    }
                }
                else
                {
                    this.m_banner.SetActive(true);
                    this.m_rankNumber.Text = rank.ToString();
                    this.m_legendIndex.gameObject.SetActive(false);
                    this.m_legendIndex.Text = string.Empty;
                }
                if ((bool) callbackData)
                {
                    this.UpdateStars(currentMedal.earnedStars, currentMedal.totalStars);
                }
            }
        }
    }

    public void SetMedal(NetCache.NetCacheMedalInfo medal)
    {
        this.SetMedal(new MedalInfoTranslator(medal), true);
    }

    public void SetMedal(MedalInfoTranslator medal, bool showStars)
    {
        this.m_banner.SetActive(false);
        this.m_medal = medal;
        TranslatedMedalInfo currentMedal = this.m_medal.GetCurrentMedal();
        this.m_rankMedal.GetComponent<Renderer>().enabled = false;
        AssetLoader.Get().LoadTexture(currentMedal.textureName, new AssetLoader.ObjectCallback(this.OnTextureLoaded), showStars, false);
    }

    public void SetMedal(NetCache.NetCacheMedalInfo medal, bool showStars)
    {
        this.SetMedal(new MedalInfoTranslator(medal), showStars);
    }

    private void UpdateStars(int numEarned, int numTotal)
    {
        List<Transform> evenStarBones;
        int num = 0;
        if ((numTotal % 2) == 0)
        {
            evenStarBones = this.m_evenStarBones;
            if (numTotal == 2)
            {
                num = 1;
            }
        }
        else
        {
            evenStarBones = this.m_oddStarBones;
            if (numTotal == 3)
            {
                num = 1;
            }
            else if (numTotal == 1)
            {
                num = 2;
            }
        }
        for (int i = 0; i < numTotal; i++)
        {
            GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.m_starPrefab);
            item.transform.parent = base.transform;
            item.layer = base.gameObject.layer;
            this.m_stars.Add(item);
            item.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            item.transform.position = evenStarBones[i + num].position;
            if (i < numEarned)
            {
                item.GetComponent<Renderer>().material = this.m_starFilledMaterial;
            }
            else
            {
                item.GetComponent<Renderer>().material = (Material) this.m_starEmptyMaterial;
            }
        }
    }
}

