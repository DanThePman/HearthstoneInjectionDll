using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class StoreMobilePurchase
{
    public static void AddProductById(string mobileProductId)
    {
        AddProductToAllProductsList(mobileProductId);
    }

    private static void AddProductToAllProductsList(string mobileProductId)
    {
    }

    private static void ClearMobileStoreProducts()
    {
    }

    public static void ClearProductList()
    {
        ClearMobileStoreProducts();
    }

    private static bool DeviceIsWaitingForPurchase()
    {
        return false;
    }

    public static void DismissNextReceipt(bool consume)
    {
        DismissReceipt(consume);
    }

    private static void DismissReceipt(bool consume)
    {
    }

    public static void FinishTransactionForId(string transactionId)
    {
        Debug.LogError("FinishTransactionForId should be not be called. Handled natively on mobile platforms.");
    }

    private static void FinishTransactionId(string transactionId)
    {
    }

    public static void GamePurchaseStatusResponse(string transactionId, bool isSuccess)
    {
        OnGamePurchaseStatusResponse(transactionId, isSuccess);
    }

    private static int GetLastPurchaseStatusCode()
    {
        return 3;
    }

    private static string GetLastPurchaseStatusDescription()
    {
        return string.Empty;
    }

    public static string GetLocalizedPrice(double price)
    {
        return LocalizedPrice(price);
    }

    public static string GetLocalizedProductPrice(string mobileProductId)
    {
        return LocalizedProductPriceById(mobileProductId);
    }

    public static IntPtr GetNextReceipt()
    {
        return NextReceipt();
    }

    private static int GetNumReceiptsAvailable()
    {
        return 0;
    }

    public static double GetProductPrice(string mobileProductId)
    {
        return ProductPriceById(mobileProductId);
    }

    public static PURCHASE_STATUS_CODE GetStatusCodeOfLastPurchase()
    {
        return (PURCHASE_STATUS_CODE) GetLastPurchaseStatusCode();
    }

    public static string GetStatusDescriptionOfLastPurchase()
    {
        return GetLastPurchaseStatusDescription();
    }

    private static string GetThirdPartyUserId()
    {
        return string.Empty;
    }

    public static bool IsWaitingForPurchase()
    {
        return DeviceIsWaitingForPurchase();
    }

    private static string LocalizedPrice(double price)
    {
        return string.Empty;
    }

    private static string LocalizedProductPriceById(string mobileProductId)
    {
        return string.Empty;
    }

    private static IntPtr NextReceipt()
    {
        return IntPtr.Zero;
    }

    public static int NumReceiptsAvailable()
    {
        return GetNumReceiptsAvailable();
    }

    private static void OnGamePurchaseStatusResponse(string transactionId, bool isSuccess)
    {
    }

    private static void OnReset()
    {
    }

    private static void OnThirdPartyPurchaseStatus(string transactionId, int status)
    {
    }

    private static double ProductPriceById(string mobileProductId)
    {
        return -1.0;
    }

    public static void PurchaseProductById(string mobileProductId)
    {
        RequestPurchaseByProductId(mobileProductId, string.Empty);
    }

    public static void PurchaseProductById(string mobileProductId, string transactionId)
    {
        RequestPurchaseByProductId(mobileProductId, transactionId);
    }

    private static int ReceiptForTransactionId(string transactionId, out IntPtr data)
    {
        data = IntPtr.Zero;
        return 0;
    }

    private static string ReceiptStringForTransactionId(string transactionId)
    {
        return string.Empty;
    }

    private static void RequestPurchaseByProductId(string mobileProductId, string transactionId)
    {
    }

    private static void RequestThirdPartyReceipt(string mobileProductId)
    {
    }

    public static void Reset()
    {
        if ((StoreManager.HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
        {
            OnReset();
        }
    }

    private static void SetBattleNetGameAccountIdAndRegion(ulong gameAccountId, int gameRegion)
    {
    }

    public static void SetGameAccountIdAndRegion(ulong gameAccountId, int gameRegion)
    {
        Log.Yim.Print(string.Concat(new object[] { "SetGameAccountIdAndRegion(", gameAccountId, ", ", gameRegion, ")" }), new object[0]);
        SetBattleNetGameAccountIdAndRegion(gameAccountId, gameRegion);
    }

    private static void StoreTransactionStatusNotReady()
    {
    }

    public static void ThirdPartyPurchaseStatus(string transactionId, Network.ThirdPartyPurchaseStatusResponse.PurchaseStatus status)
    {
        OnThirdPartyPurchaseStatus(transactionId, (int) status);
    }

    public static string ThirdPartyUserId()
    {
        return GetThirdPartyUserId();
    }

    public static void TransactionStatusNotReady()
    {
        StoreTransactionStatusNotReady();
    }

    public static void ValidateAllProducts(string gameObjectName)
    {
        ValidateProducts(gameObjectName);
    }

    private static void ValidateProducts(string gameObjectName)
    {
    }

    public static void WaitingOnThirdPartyReceipt(string mobileProductId)
    {
        RequestThirdPartyReceipt(mobileProductId);
    }

    public enum PURCHASE_STATUS_CODE
    {
        PURCHASE_SUCCESSFUL,
        PURCHASE_FAILED,
        PURCHASE_RECOVERED,
        PURCHASE_NOT_AVAILABLE
    }
}

