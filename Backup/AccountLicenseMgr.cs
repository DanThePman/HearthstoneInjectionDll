using PegasusShared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AccountLicenseMgr
{
    private List<AccountLicensesChangedListener> m_accountLicensesChangedListeners = new List<AccountLicensesChangedListener>();
    private bool m_registeredForProfileNotices;
    private static AccountLicenseMgr s_instance;

    public static AccountLicenseMgr Get()
    {
        if (s_instance == null)
        {
            s_instance = new AccountLicenseMgr();
            ApplicationMgr.Get().WillReset += new System.Action(s_instance.WillReset);
        }
        if (!s_instance.m_registeredForProfileNotices)
        {
            NetCache.Get().RegisterNewNoticesListener(new NetCache.DelNewNoticesListener(s_instance.OnNewNotices));
            s_instance.m_registeredForProfileNotices = true;
        }
        return s_instance;
    }

    public List<AccountLicenseInfo> GetAllOwnedAccountLicenseInfo()
    {
        List<AccountLicenseInfo> list = new List<AccountLicenseInfo>();
        NetCache.NetCacheAccountLicenses netObject = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
        if (netObject != null)
        {
            foreach (AccountLicenseInfo info in netObject.AccountLicenses.Values)
            {
                if (this.OwnsAccountLicense(info))
                {
                    list.Add(info);
                }
            }
        }
        return list;
    }

    private void OnNewNotices(List<NetCache.ProfileNotice> newNotices)
    {
        NetCache.NetCacheAccountLicenses netObject = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
        if (netObject == null)
        {
            Debug.LogWarning("AccountLicenses.OnNewNotices netCacheAccountLicenses is null -- going to ack all ACCOUNT_LICENSE notices assuming NetCache is not yet loaded");
        }
        HashSet<long> set = new HashSet<long>();
        foreach (NetCache.ProfileNotice notice in newNotices)
        {
            if (notice.Type == NetCache.ProfileNotice.NoticeType.ACCOUNT_LICENSE)
            {
                NetCache.ProfileNoticeAcccountLicense license = notice as NetCache.ProfileNoticeAcccountLicense;
                if (netObject != null)
                {
                    if (!netObject.AccountLicenses.ContainsKey(license.License))
                    {
                        AccountLicenseInfo info = new AccountLicenseInfo {
                            License = license.License,
                            Flags_ = 0L,
                            CasId = 0L
                        };
                        netObject.AccountLicenses[license.License] = info;
                    }
                    if (license.CasID > netObject.AccountLicenses[license.License].CasId)
                    {
                        netObject.AccountLicenses[license.License].CasId = license.CasID;
                        if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.ACCOUNT_LICENSE_FLAGS)
                        {
                            netObject.AccountLicenses[license.License].Flags_ = (ulong) notice.OriginData;
                        }
                        else
                        {
                            object[] args = new object[] { notice.Origin, notice.OriginData, license.License, license.CasID };
                            Debug.LogWarning(string.Format("AccountLicenses.OnNewNotices unexpected notice origin {0} (data={1}) for license {2} casID {3}", args));
                        }
                        set.Add(license.License);
                    }
                }
                Network.AckNotice(notice.NoticeID);
            }
        }
        if (netObject != null)
        {
            List<AccountLicenseInfo> changedLicensesInfo = new List<AccountLicenseInfo>();
            foreach (long num in set)
            {
                if (netObject.AccountLicenses.ContainsKey(num))
                {
                    changedLicensesInfo.Add(netObject.AccountLicenses[num]);
                }
            }
            if (changedLicensesInfo.Count != 0)
            {
                foreach (AccountLicensesChangedListener listener in this.m_accountLicensesChangedListeners.ToArray())
                {
                    listener.Fire(changedLicensesInfo);
                }
            }
        }
    }

    public bool OwnsAccountLicense(AccountLicenseInfo accountLicenseInfo)
    {
        if (accountLicenseInfo == null)
        {
            return false;
        }
        return ((accountLicenseInfo.Flags_ & ((ulong) 1L)) == 1L);
    }

    public bool OwnsAccountLicense(long license)
    {
        NetCache.NetCacheAccountLicenses netObject = NetCache.Get().GetNetObject<NetCache.NetCacheAccountLicenses>();
        if (netObject == null)
        {
            return false;
        }
        if (!netObject.AccountLicenses.ContainsKey(license))
        {
            return false;
        }
        return this.OwnsAccountLicense(netObject.AccountLicenses[license]);
    }

    public bool RegisterAccountLicensesChangedListener(AccountLicensesChangedCallback callback)
    {
        return this.RegisterAccountLicensesChangedListener(callback, null);
    }

    public bool RegisterAccountLicensesChangedListener(AccountLicensesChangedCallback callback, object userData)
    {
        AccountLicensesChangedListener item = new AccountLicensesChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_accountLicensesChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_accountLicensesChangedListeners.Add(item);
        return true;
    }

    public bool RemoveAccountLicensesChangedListener(AccountLicensesChangedCallback callback)
    {
        return this.RemoveAccountLicensesChangedListener(callback, null);
    }

    public bool RemoveAccountLicensesChangedListener(AccountLicensesChangedCallback callback, object userData)
    {
        AccountLicensesChangedListener item = new AccountLicensesChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_accountLicensesChangedListeners.Remove(item);
    }

    private void WillReset()
    {
        this.m_accountLicensesChangedListeners.Clear();
    }

    public delegate void AccountLicensesChangedCallback(List<AccountLicenseInfo> changedLicensesInfo, object userData);

    private class AccountLicensesChangedListener : EventListener<AccountLicenseMgr.AccountLicensesChangedCallback>
    {
        public void Fire(List<AccountLicenseInfo> changedLicensesInfo)
        {
            base.m_callback(changedLicensesInfo, base.m_userData);
        }
    }
}

