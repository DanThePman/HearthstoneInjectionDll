using bnet.protocol.profanity;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class ProfanityAPI : BattleNetAPI
{
    private string m_localeName;
    private WordFilters m_wordFilters;
    private static readonly char[] REPLACEMENT_CHARS = new char[] { '!', '@', '#', '$', '%', '^', '&', '*' };

    public ProfanityAPI(BattleNetCSharp battlenet) : base(battlenet, "Profanity")
    {
    }

    private void DownloadCompletedCallback(byte[] data)
    {
        if (data == null)
        {
            base.ApiLog.LogWarning("Downloading of profanity data from depot failed!");
        }
        else
        {
            base.ApiLog.LogDebug("Downloading of profanity data completed");
            try
            {
                WordFilters filters = WordFilters.ParseFrom(data);
                this.m_wordFilters = filters;
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.ToString() };
                base.ApiLog.LogWarning("Failed to parse received data into protocol buffer. Ex  = {0}", args);
            }
            if ((this.m_wordFilters == null) || !this.m_wordFilters.IsInitialized)
            {
                base.ApiLog.LogWarning("WordFilters failed to initialize");
            }
        }
    }

    public string FilterProfanity(string unfiltered)
    {
        if (this.m_wordFilters == null)
        {
            return unfiltered;
        }
        string input = unfiltered;
        foreach (WordFilter filter in this.m_wordFilters.FiltersList)
        {
            if (filter.Type == "bad")
            {
                MatchCollection matchs = new Regex(filter.Regex, RegexOptions.IgnoreCase).Matches(input);
                if (matchs.Count != 0)
                {
                    IEnumerator enumerator = matchs.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            Match current = (Match) enumerator.Current;
                            if (current.Success)
                            {
                                char[] chArray = input.ToCharArray();
                                if (current.Index <= chArray.Length)
                                {
                                    int length = current.Length;
                                    if ((current.Index + current.Length) > chArray.Length)
                                    {
                                        length = chArray.Length - current.Index;
                                    }
                                    for (int i = 0; i < length; i++)
                                    {
                                        chArray[current.Index + i] = this.GetReplacementChar();
                                    }
                                    input = new string(chArray);
                                }
                            }
                        }
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable == null)
                        {
                        }
                        disposable.Dispose();
                    }
                }
            }
        }
        return input;
    }

    private char GetReplacementChar()
    {
        int index = UnityEngine.Random.Range(0, REPLACEMENT_CHARS.Length);
        return REPLACEMENT_CHARS[index];
    }

    public override void Initialize()
    {
        this.m_wordFilters = null;
        ResourcesAPI resources = base.m_battleNet.Resources;
        if (resources == null)
        {
            base.ApiLog.LogWarning("ResourcesAPI is not initialized! Unable to proceed.");
        }
        else
        {
            this.m_localeName = Localization.GetLocaleName();
            if (string.IsNullOrEmpty(this.m_localeName))
            {
                base.ApiLog.LogWarning("Unable to get Locale from Localization class");
                this.m_localeName = Localization.DEFAULT_LOCALE_NAME;
            }
            object[] args = new object[] { this.m_localeName };
            base.ApiLog.LogDebug("Locale is {0}", args);
            resources.LookupResource(new FourCC("BN"), new FourCC("apft"), new FourCC(this.m_localeName), new ResourcesAPI.ResourceLookupCallback(this.ResouceLookupCallback), null);
        }
    }

    private void ResouceLookupCallback(ContentHandle contentHandle, object userContext)
    {
        if (contentHandle == null)
        {
            base.ApiLog.LogWarning("BN resource look up failed unable to proceed");
        }
        else
        {
            object[] args = new object[] { contentHandle.Region, contentHandle.Usage, contentHandle.Sha256Digest };
            base.ApiLog.LogDebug("Lookup done Region={0} Usage={1} SHA256={2}", args);
            base.m_battleNet.LocalStorage.GetFile(contentHandle, new LocalStorageAPI.DownloadCompletedCallback(this.DownloadCompletedCallback));
        }
    }
}

