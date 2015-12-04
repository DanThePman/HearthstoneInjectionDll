using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;

public class EntityDef : EntityBase
{
    [CompilerGenerated]
    private static Dictionary<string, int> <>f__switch$map3A;
    private string m_cardId;
    private string m_currLoadingPowerDefinition;
    private List<Power.PowerInfo> m_currLoadingPowerInfos;
    private List<string> m_entourageCardIDs = new List<string>();
    private string m_masterPower = string.Empty;
    private List<PowerHistoryInfo> m_powerHistoryInfoList = new List<PowerHistoryInfo>();
    private Map<string, Power> m_powers = new Map<string, Power>();
    protected TagSet m_referencedTags = new TagSet();
    protected Map<int, string> m_stringTags = new Map<int, string>();

    public EntityDef Clone()
    {
        EntityDef def = new EntityDef {
            m_cardId = this.m_cardId
        };
        def.ReplaceTags(base.m_tags);
        def.m_referencedTags.Replace(this.m_referencedTags);
        foreach (KeyValuePair<int, string> pair in this.m_stringTags)
        {
            def.m_stringTags.Add(pair.Key, pair.Value);
        }
        foreach (KeyValuePair<string, Power> pair2 in this.m_powers)
        {
            def.m_powers.Add(pair2.Key, pair2.Value);
        }
        def.m_masterPower = this.m_masterPower;
        return def;
    }

    private void FlushPower()
    {
        if (this.m_currLoadingPowerDefinition != null)
        {
            Power power = Power.Create(this.m_currLoadingPowerDefinition, this.m_currLoadingPowerInfos);
            if (this.m_powers.ContainsKey(power.GetDefinition()))
            {
                Debug.LogError(string.Format("Error loading card xml {0}, already contains power definition {1}", this.m_cardId, power.GetDefinition()));
            }
            else
            {
                this.m_powers.Add(power.GetDefinition(), power);
            }
        }
        this.m_currLoadingPowerDefinition = null;
        this.m_currLoadingPowerInfos = null;
    }

    public string GetArtistName()
    {
        string stringTag = base.GetStringTag(GAME_TAG.ARTISTNAME);
        if (stringTag == null)
        {
            return "ERROR: NO ARTIST NAME";
        }
        return stringTag;
    }

    public Power GetAttackPower()
    {
        return Power.GetDefaultAttackPower();
    }

    public string GetCardId()
    {
        return this.m_cardId;
    }

    public string GetCardTextInHand()
    {
        return TextUtils.TransformCardText(this, GAME_TAG.CARDTEXT_INHAND);
    }

    public TAG_CLASS GetClass()
    {
        return base.GetTag<TAG_CLASS>(GAME_TAG.CLASS);
    }

    public string GetDebugName()
    {
        string stringTag = base.GetStringTag(GAME_TAG.CARDNAME);
        if (stringTag != null)
        {
            return string.Format("[name={0} cardId={1} type={2}]", stringTag, this.m_cardId, base.GetCardType());
        }
        if (this.m_cardId != null)
        {
            return string.Format("[cardId={0} type={1}]", this.m_cardId, base.GetCardType());
        }
        return string.Format("UNKNOWN ENTITY [cardType={0}]", base.GetCardType());
    }

    public TAG_ENCHANTMENT_VISUAL GetEnchantmentBirthVisual()
    {
        return base.GetTag<TAG_ENCHANTMENT_VISUAL>(GAME_TAG.ENCHANTMENT_BIRTH_VISUAL);
    }

    public TAG_ENCHANTMENT_VISUAL GetEnchantmentIdleVisual()
    {
        return base.GetTag<TAG_ENCHANTMENT_VISUAL>(GAME_TAG.ENCHANTMENT_IDLE_VISUAL);
    }

    public List<string> GetEntourageCardIDs()
    {
        return this.m_entourageCardIDs;
    }

    public string GetFlavorText()
    {
        string stringTag = base.GetStringTag(GAME_TAG.FLAVORTEXT);
        if (stringTag == null)
        {
            return string.Empty;
        }
        return stringTag;
    }

    public string GetHowToEarnText(TAG_PREMIUM premium)
    {
        string stringTag;
        if (premium == TAG_PREMIUM.GOLDEN)
        {
            stringTag = base.GetStringTag(GAME_TAG.HOW_TO_EARN_GOLDEN);
            if (stringTag == null)
            {
                return string.Empty;
            }
            return stringTag;
        }
        stringTag = base.GetStringTag(GAME_TAG.HOW_TO_EARN);
        if (stringTag == null)
        {
            return string.Empty;
        }
        return stringTag;
    }

    public Power GetMasterPower()
    {
        if (!this.m_masterPower.Equals(string.Empty) && this.m_powers.ContainsKey(this.m_masterPower))
        {
            return this.m_powers[this.m_masterPower];
        }
        return Power.GetDefaultMasterPower();
    }

    public string GetName()
    {
        string stringTag = base.GetStringTag(GAME_TAG.CARDNAME);
        if (stringTag != null)
        {
            return stringTag;
        }
        return this.GetDebugName();
    }

    public PowerHistoryInfo GetPowerHistoryInfo(int effectIndex)
    {
        if (effectIndex < 0)
        {
            return null;
        }
        foreach (PowerHistoryInfo info2 in this.m_powerHistoryInfoList)
        {
            if (info2.GetEffectIndex() == effectIndex)
            {
                return info2;
            }
        }
        return null;
    }

    public TAG_RACE GetRace()
    {
        return base.GetTag<TAG_RACE>(GAME_TAG.CARDRACE);
    }

    public string GetRaceText()
    {
        if (!base.HasTag(GAME_TAG.CARDRACE))
        {
            return string.Empty;
        }
        return GameStrings.GetRaceName(this.GetRace());
    }

    public TAG_RARITY GetRarity()
    {
        return base.GetTag<TAG_RARITY>(GAME_TAG.RARITY);
    }

    public override int GetReferencedTag(int tag)
    {
        return this.m_referencedTags.GetTag(tag);
    }

    public TagSet GetReferencedTags()
    {
        return this.m_referencedTags;
    }

    public override string GetStringTag(int tag)
    {
        string str;
        this.m_stringTags.TryGetValue(tag, out str);
        return str;
    }

    public static string GetTagString(XmlNode node, int tag, Locale loc)
    {
        XmlElement element = null;
        GAME_TAG game_tag = (GAME_TAG) tag;
        if (game_tag == GAME_TAG.ARTISTNAME)
        {
            element = node[Localization.DEFAULT_LOCALE_NAME];
        }
        else
        {
            foreach (Locale locale in Localization.GetLoadOrder(loc, false))
            {
                string str = locale.ToString();
                element = node[str];
                if (element != null)
                {
                    break;
                }
            }
        }
        if (element != null)
        {
            return TextUtils.DecodeWhitespaces(element.InnerText);
        }
        return null;
    }

    public static XmlElement LoadCardXmlFromAsset(string cardId, UnityEngine.Object asset)
    {
        if (asset == null)
        {
            Debug.LogWarning(string.Format("EntityDef.LoadCardXmlFromAsset() - null cardAsset given for cardId \"{0}\"", cardId));
            return null;
        }
        TextAsset xmlAsset = asset as TextAsset;
        return LoadCardXmlFromAsset(cardId, xmlAsset);
    }

    public static XmlElement LoadCardXmlFromAsset(string cardId, TextAsset xmlAsset)
    {
        if (xmlAsset == null)
        {
            Debug.LogWarning(string.Format("EntityDef.LoadCardXmlFromAsset() - asset for cardId \"{0}\" was not a card xml", cardId));
            return null;
        }
        return XmlUtils.LoadXmlDocFromTextAsset(xmlAsset)["Entity"];
    }

    public bool LoadDataFromCardXml(XmlReader reader)
    {
        this.m_powerHistoryInfoList = new List<PowerHistoryInfo>();
        while ((reader.NodeType != XmlNodeType.Element) || (reader.Name != "Entity"))
        {
            if (!reader.Read())
            {
                break;
            }
        }
        if (reader.EOF)
        {
            return false;
        }
        int depth = reader.Depth;
        this.m_cardId = reader["CardID"];
        bool flag = false;
        while (flag || reader.Read())
        {
            flag = false;
            if (reader.NodeType == XmlNodeType.Element)
            {
                if ((reader.Depth <= depth) || reader.EOF)
                {
                    break;
                }
                if ((this.m_currLoadingPowerDefinition != null) && !reader.Name.Equals("PlayRequirement"))
                {
                    this.FlushPower();
                }
                string name = reader.Name;
                if (name != null)
                {
                    int num2;
                    if (<>f__switch$map3A == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(7);
                        dictionary.Add("Tag", 0);
                        dictionary.Add("ReferencedTag", 1);
                        dictionary.Add("MasterPower", 2);
                        dictionary.Add("Power", 3);
                        dictionary.Add("PlayRequirement", 4);
                        dictionary.Add("EntourageCard", 5);
                        dictionary.Add("TriggeredPowerHistoryInfo", 6);
                        <>f__switch$map3A = dictionary;
                    }
                    if (<>f__switch$map3A.TryGetValue(name, out num2))
                    {
                        switch (num2)
                        {
                            case 0:
                            {
                                flag = this.ReadTag(reader);
                                continue;
                            }
                            case 1:
                            {
                                this.ReadReferencedTag(reader);
                                continue;
                            }
                            case 2:
                            {
                                this.ReadMasterPower(reader);
                                continue;
                            }
                            case 3:
                            {
                                this.ReadPower(reader);
                                continue;
                            }
                            case 4:
                            {
                                this.ReadPlayRequirement(reader);
                                continue;
                            }
                            case 5:
                            {
                                this.ReadEntourage(reader);
                                continue;
                            }
                            case 6:
                            {
                                this.ReadTriggeredPowerHistoryInfo(reader);
                                continue;
                            }
                        }
                    }
                }
                Debug.LogError(string.Format("EntityDef.LoadDataFromCardXml() - Unrecognized element \"{0}\" in card xml {1}", reader.Name, this.m_cardId));
            }
        }
        this.FlushPower();
        return true;
    }

    public static EntityDef LoadFromAsset(string cardId, TextAsset xmlAsset, bool overrideCardId = false)
    {
        if (xmlAsset == null)
        {
            Debug.LogWarning(string.Format("EntityDef.LoadFromAsset() - asset for cardId \"{0}\" was not a card xml", cardId));
            return null;
        }
        EntityDef def = new EntityDef();
        using (StringReader reader = new StringReader(xmlAsset.text))
        {
            using (XmlReader reader2 = XmlReader.Create(reader))
            {
                def.LoadDataFromCardXml(reader2);
            }
        }
        if (overrideCardId)
        {
            def.m_cardId = cardId;
        }
        return def;
    }

    private void ReadEntourage(XmlReader reader)
    {
        this.m_entourageCardIDs.Add(reader["cardID"]);
    }

    private void ReadMasterPower(XmlReader reader)
    {
        if (!string.IsNullOrEmpty(this.m_masterPower))
        {
            Debug.Log(string.Format("Error loading card xml {0}, multiple MasterPower definitions", this.m_cardId));
        }
        else
        {
            this.m_masterPower = reader.ReadElementContentAsString();
        }
    }

    private void ReadPlayRequirement(XmlReader reader)
    {
        string str = reader["reqID"];
        string str2 = reader["param"];
        if (string.IsNullOrEmpty(str))
        {
            Debug.LogError("PlayRequirement is missing requirement ID");
        }
        else
        {
            int num2;
            Power.PowerInfo info;
            int num = Convert.ToInt32(str);
            if (string.IsNullOrEmpty(str2))
            {
                num2 = 0;
            }
            else
            {
                num2 = Convert.ToInt32(str2);
            }
            info.reqId = (PlayErrors.ErrorType) num;
            info.param = num2;
            this.m_currLoadingPowerInfos.Add(info);
        }
    }

    private void ReadPower(XmlReader reader)
    {
        string str = reader["definition"];
        this.m_currLoadingPowerDefinition = str;
        this.m_currLoadingPowerInfos = new List<Power.PowerInfo>();
    }

    private void ReadReferencedTag(XmlReader reader)
    {
        string str = reader["enumID"];
        int tag = Convert.ToInt32(str);
        int val = Convert.ToInt32(reader["value"]);
        this.SetReferencedTag(tag, val);
    }

    private bool ReadTag(XmlReader reader)
    {
        string str = reader["enumID"];
        string str2 = reader["type"];
        bool flag = false;
        int tag = Convert.ToInt32(str);
        if (str2.Equals("String"))
        {
            string str3 = reader.ReadElementContentAsString();
            this.m_stringTags[tag] = str3;
            return flag;
        }
        int tagValue = Convert.ToInt32(reader["value"]);
        base.SetTag(tag, tagValue);
        return flag;
    }

    private void ReadTriggeredPowerHistoryInfo(XmlReader reader)
    {
        string str = reader["effectIndex"];
        string str2 = reader["showInHistory"];
        int index = Convert.ToInt32(str);
        bool show = Convert.ToBoolean(str2);
        this.m_powerHistoryInfoList.Add(new PowerHistoryInfo(index, show));
    }

    public void SetCardId(string cardId)
    {
        this.m_cardId = cardId;
    }

    public void SetReferencedTag(GAME_TAG enumTag, int val)
    {
        this.SetReferencedTag((int) enumTag, val);
    }

    public void SetReferencedTag(int tag, int val)
    {
        this.m_referencedTags.SetTag(tag, val);
    }

    public void SetStringTag(GAME_TAG enumTag, string val)
    {
        this.SetStringTag((int) enumTag, val);
    }

    public void SetStringTag(int tag, string val)
    {
        this.m_stringTags[tag] = val;
    }

    public override string ToString()
    {
        return this.GetDebugName();
    }
}

