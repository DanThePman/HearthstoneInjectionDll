using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.XPath;

public class Power
{
    private string mDefinition = string.Empty;
    private PlayErrors.PlayRequirementInfo mPlayRequirementInfo = new PlayErrors.PlayRequirementInfo();
    private static Power s_defaultAttackPower;
    private static Power s_defaultMasterPower;

    public static Power Create(string definition, List<PowerInfo> infos)
    {
        Power power = new Power {
            mDefinition = definition
        };
        List<PlayErrors.ErrorType> requirements = new List<PlayErrors.ErrorType>();
        if ((infos != null) && (infos.Count > 0))
        {
            foreach (PowerInfo info in infos)
            {
                PlayErrors.ErrorType reqId = info.reqId;
                int param = info.param;
                PlayErrors.ErrorType type2 = reqId;
                switch (type2)
                {
                    case PlayErrors.ErrorType.REQ_TARGET_MAX_ATTACK:
                        power.mPlayRequirementInfo.paramMaxAtk = param;
                        goto Label_012E;

                    case PlayErrors.ErrorType.REQ_TARGET_WITH_RACE:
                        power.mPlayRequirementInfo.paramRace = param;
                        goto Label_012E;

                    case PlayErrors.ErrorType.REQ_NUM_MINION_SLOTS:
                        power.mPlayRequirementInfo.paramNumMinionSlots = param;
                        goto Label_012E;

                    default:
                        switch (type2)
                        {
                            case PlayErrors.ErrorType.REQ_MINION_CAP_IF_TARGET_AVAILABLE:
                                power.mPlayRequirementInfo.paramNumMinionSlotsWithTarget = param;
                                goto Label_012E;

                            case PlayErrors.ErrorType.REQ_MINIMUM_ENEMY_MINIONS:
                                power.mPlayRequirementInfo.paramMinNumEnemyMinions = param;
                                goto Label_012E;

                            default:
                                if (type2 != PlayErrors.ErrorType.REQ_TARGET_MIN_ATTACK)
                                {
                                    if (type2 == PlayErrors.ErrorType.REQ_MINIMUM_TOTAL_MINIONS)
                                    {
                                        break;
                                    }
                                    if (type2 == PlayErrors.ErrorType.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    power.mPlayRequirementInfo.paramMinAtk = param;
                                }
                                goto Label_012E;
                        }
                        power.mPlayRequirementInfo.paramMinNumTotalMinions = param;
                        goto Label_012E;
                }
                power.mPlayRequirementInfo.paramMinNumFriendlyMinions = param;
            Label_012E:
                requirements.Add(reqId);
            }
        }
        power.mPlayRequirementInfo.requirementsMap = PlayErrors.GetRequirementsMap(requirements);
        return power;
    }

    public static Power GetDefaultAttackPower()
    {
        if (s_defaultAttackPower == null)
        {
            s_defaultAttackPower = new Power();
            List<PlayErrors.ErrorType> requirements = new List<PlayErrors.ErrorType> { 11, 3 };
            s_defaultAttackPower.mPlayRequirementInfo.requirementsMap = PlayErrors.GetRequirementsMap(requirements);
        }
        return s_defaultAttackPower;
    }

    public static Power GetDefaultMasterPower()
    {
        if (s_defaultMasterPower == null)
        {
            s_defaultMasterPower = new Power();
            List<PlayErrors.ErrorType> requirements = new List<PlayErrors.ErrorType>();
            s_defaultMasterPower.mPlayRequirementInfo.requirementsMap = PlayErrors.GetRequirementsMap(requirements);
        }
        return s_defaultMasterPower;
    }

    public string GetDefinition()
    {
        return this.mDefinition;
    }

    public PlayErrors.PlayRequirementInfo GetPlayRequirementInfo()
    {
        return this.mPlayRequirementInfo;
    }

    public static Power LoadFromXml(XmlElement rootElement)
    {
        Power power = new Power {
            mDefinition = rootElement.GetAttribute("definition")
        };
        XPathNavigator navigator = rootElement.CreateNavigator();
        XPathExpression expr = navigator.Compile("./PlayRequirement");
        XPathNodeIterator iterator = navigator.Select(expr);
        List<PlayErrors.ErrorType> requirements = new List<PlayErrors.ErrorType>();
        while (iterator.MoveNext())
        {
            int num;
            XmlElement node = (XmlElement) ((IHasXmlNode) iterator.Current).GetNode();
            if (int.TryParse(node.GetAttribute("reqID"), out num))
            {
                PlayErrors.ErrorType item = (PlayErrors.ErrorType) num;
                requirements.Add(item);
                switch (item)
                {
                    case PlayErrors.ErrorType.REQ_TARGET_MIN_ATTACK:
                    {
                        if (!int.TryParse(node.GetAttribute("param"), out power.mPlayRequirementInfo.paramMinAtk))
                        {
                            Log.Rachelle.Print(string.Format("Unable to read play requirement param minAtk for power {0}.", power.GetDefinition()), new object[0]);
                        }
                        continue;
                    }
                    case PlayErrors.ErrorType.REQ_TARGET_MAX_ATTACK:
                    {
                        if (!int.TryParse(node.GetAttribute("param"), out power.mPlayRequirementInfo.paramMaxAtk))
                        {
                            Log.Rachelle.Print(string.Format("Unable to read play requirement param maxAtk for power {0}.", power.GetDefinition()), new object[0]);
                        }
                        continue;
                    }
                    case PlayErrors.ErrorType.REQ_TARGET_WITH_RACE:
                    {
                        if (!int.TryParse(node.GetAttribute("param"), out power.mPlayRequirementInfo.paramRace))
                        {
                            Log.Rachelle.Print(string.Format("Unable to read play requirement param race for power {0}.", power.GetDefinition()), new object[0]);
                        }
                        continue;
                    }
                    case PlayErrors.ErrorType.REQ_NUM_MINION_SLOTS:
                    {
                        if (!int.TryParse(node.GetAttribute("param"), out power.mPlayRequirementInfo.paramNumMinionSlots))
                        {
                            Log.Rachelle.Print(string.Format("Unable to read play requirement param num minion slots for power {0}.", power.GetDefinition()), new object[0]);
                        }
                        continue;
                    }
                    case PlayErrors.ErrorType.REQ_MINION_CAP_IF_TARGET_AVAILABLE:
                    {
                        if (!int.TryParse(node.GetAttribute("param"), out power.mPlayRequirementInfo.paramNumMinionSlotsWithTarget))
                        {
                            Log.Rachelle.Print(string.Format("Unable to read play requirement param num minion slots with target for power {0}.", power.GetDefinition()), new object[0]);
                        }
                        continue;
                    }
                    case PlayErrors.ErrorType.REQ_MINIMUM_ENEMY_MINIONS:
                    {
                        if (!int.TryParse(node.GetAttribute("param"), out power.mPlayRequirementInfo.paramMinNumEnemyMinions))
                        {
                            Log.Rachelle.Print(string.Format("Unable to read play requirement param num enemy minions for power {0}.", power.GetDefinition()), new object[0]);
                        }
                        continue;
                    }
                    case PlayErrors.ErrorType.REQ_MINIMUM_TOTAL_MINIONS:
                    {
                        if (!int.TryParse(node.GetAttribute("param"), out power.mPlayRequirementInfo.paramMinNumTotalMinions))
                        {
                            Log.Rachelle.Print(string.Format("Unable to read play requirement param num total minions for power {0}.", power.GetDefinition()), new object[0]);
                        }
                        continue;
                    }
                }
                if ((item == PlayErrors.ErrorType.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS) && !int.TryParse(node.GetAttribute("param"), out power.mPlayRequirementInfo.paramMinNumFriendlyMinions))
                {
                    Log.Rachelle.Print(string.Format("Unable to read play requirement param num friendly minions for power {0}.", power.GetDefinition()), new object[0]);
                }
            }
        }
        power.mPlayRequirementInfo.requirementsMap = PlayErrors.GetRequirementsMap(requirements);
        return power;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PowerInfo
    {
        public PlayErrors.ErrorType reqId;
        public int param;
    }
}

