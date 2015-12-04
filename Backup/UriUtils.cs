using System;
using System.Net;
using System.Runtime.InteropServices;

public class UriUtils
{
    public static bool GetHostAddress(string hostName, out string address)
    {
        if (GetHostAddressAsIp(hostName, out address))
        {
            return true;
        }
        try
        {
            if (GetHostAddressByDns(hostName, out address))
            {
                return true;
            }
        }
        catch (Exception exception)
        {
            throw exception;
        }
        return false;
    }

    public static bool GetHostAddressAsIp(string hostName, out string address)
    {
        IPAddress address2;
        address = string.Empty;
        if (IPAddress.TryParse(hostName, out address2))
        {
            address = address2.ToString();
            return true;
        }
        return false;
    }

    public static bool GetHostAddressByDns(string hostName, out string address)
    {
        address = string.Empty;
        try
        {
            foreach (IPAddress address2 in Dns.GetHostEntry(hostName).AddressList)
            {
                address = address2.ToString();
                return true;
            }
        }
        catch (Exception exception)
        {
            throw exception;
        }
        address = hostName;
        return false;
    }
}

