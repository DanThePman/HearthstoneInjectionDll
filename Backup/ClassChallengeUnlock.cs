using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class ClassChallengeUnlock : Reward
{
    [CustomEditField(Sections="Sounds", T=EditType.SOUND_PREFAB)]
    public string m_appearSound;
    [CustomEditField(Sections="Container")]
    public UIBObjectSpacing m_classFrameContainer;
    private List<GameObject> m_classFrames = new List<GameObject>();
    [CustomEditField(Sections="Text Settings")]
    public UberText m_headerText;

    protected override void Awake()
    {
        base.Awake();
        if (UniversalInputManager.UsePhoneUI == null)
        {
            base.m_rewardBanner.transform.localScale = (Vector3) (base.m_rewardBanner.transform.localScale * 8f);
        }
    }

    private void DestroyClassChallengeUnlock()
    {
        UnityEngine.Object.DestroyImmediate(base.gameObject);
    }

    protected override void HideReward()
    {
        base.HideReward();
        FullScreenFXMgr.Get().EndStandardBlurVignette(1f, new FullScreenFXMgr.EffectListener(this.DestroyClassChallengeUnlock));
        base.m_root.SetActive(false);
    }

    protected override void InitData()
    {
        base.SetData(new ClassChallengeUnlockData(), false);
    }

    private void OnClicked(Reward reward, object userData)
    {
        this.HideReward();
    }

    protected override void OnDataSet(bool updateVisuals)
    {
        if (updateVisuals)
        {
            ClassChallengeUnlockData data = base.Data as ClassChallengeUnlockData;
            if (data == null)
            {
                Debug.LogWarning(string.Format("ClassChallengeUnlock.OnDataSet() - Data {0} is not ClassChallengeUnlockData", base.Data));
            }
            else
            {
                List<string> list = new List<string>();
                List<string> list2 = new List<string>();
                foreach (DbfRecord record in GameDbf.AdventureMission.GetRecords())
                {
                    if (record.GetInt("REQ_WING_ID") == data.WingID)
                    {
                        int @int = record.GetInt("SCENARIO_ID");
                        DbfRecord record2 = GameDbf.Scenario.GetRecord(@int);
                        if (record2 == null)
                        {
                            Debug.LogError(string.Format("Unable to find Scenario record with ID: {0}", @int));
                        }
                        else if (record2.GetInt("MODE_ID") == 4)
                        {
                            string val = null;
                            if (record.TryGetAssetPath("CLASS_CHALLENGE_PREFAB_POPUP", out val))
                            {
                                string locString = record2.GetLocString("SHORT_NAME");
                                list.Add(val);
                                list2.Add(locString);
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("CLASS_CHALLENGE_PREFAB_POPUP not define for AdventureMission SCENARIO_ID: {0}", @int));
                            }
                        }
                    }
                }
                if (list.Count == 0)
                {
                    Debug.LogError(string.Format("Unable to find AdventureMission record with REQ_WING_ID: {0}.", data.WingID));
                }
                else
                {
                    string str3;
                    GameStrings.PluralNumber[] numberArray1 = new GameStrings.PluralNumber[1];
                    GameStrings.PluralNumber number = new GameStrings.PluralNumber {
                        m_number = list.Count
                    };
                    numberArray1[0] = number;
                    GameStrings.PluralNumber[] pluralNumbers = numberArray1;
                    this.m_headerText.Text = GameStrings.FormatPlurals("GLOBAL_REWARD_CLASS_CHALLENGE_HEADLINE", pluralNumbers, new object[0]);
                    if (list.Count > 0)
                    {
                        str3 = string.Join(", ", list2.ToArray());
                    }
                    else
                    {
                        str3 = string.Empty;
                    }
                    string source = GameDbf.Wing.GetRecord(data.WingID).GetLocString("CLASS_CHALLENGE_REWARD_SOURCE");
                    base.SetRewardText(str3, string.Empty, source);
                    foreach (string str5 in list)
                    {
                        GameObject child = AssetLoader.Get().LoadGameObject(FileUtils.GameAssetPathToName(str5), true, false);
                        if (child != null)
                        {
                            GameUtils.SetParent(child, this.m_classFrameContainer, false);
                            child.transform.localRotation = Quaternion.identity;
                            this.m_classFrameContainer.AddObject(child, true);
                            this.m_classFrames.Add(child);
                        }
                    }
                    this.m_classFrameContainer.UpdatePositions();
                    base.SetReady(true);
                    base.EnableClickCatcher(true);
                    base.RegisterClickListener(new Reward.OnClickedCallback(this.OnClicked));
                }
            }
        }
    }

    protected override void ShowReward(bool updateCacheValues)
    {
        base.m_root.SetActive(true);
        this.m_classFrameContainer.UpdatePositions();
        foreach (GameObject obj2 in this.m_classFrames)
        {
            obj2.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            object[] args = new object[] { "amount", new Vector3(0f, 0f, 540f), "time", 1.5f, "easeType", iTween.EaseType.easeOutElastic, "space", Space.Self };
            Hashtable hashtable = iTween.Hash(args);
            iTween.RotateAdd(obj2, hashtable);
        }
        FullScreenFXMgr.Get().StartStandardBlurVignette(1f);
    }
}

