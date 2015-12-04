using PegasusShared;
using PegasusUtil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using WTCG.BI;

public class StoreManager
{
    [CompilerGenerated]
    private static Predicate<Network.BundleItem> <>f__am$cache47;
    [CompilerGenerated]
    private static Predicate<Network.BundleItem> <>f__am$cache48;
    private static readonly long CHALLENGE_CANCEL_STATUS_REQUEST_DELAY_TICKS;
    private static readonly string DEFAULT_CURRENCY_FORMAT;
    public static readonly int DEFAULT_SECONDS_BEFORE_AUTO_CANCEL = 600;
    public static readonly int DEFAULT_SECONDS_BEFORE_AUTO_CANCEL_THIRD_PARTY = 60;
    private static readonly long EARLY_STATUS_REQUEST_DELAY_TICKS;
    public static readonly PlatformDependentValue<bool> HAS_THIRD_PARTY_APP_STORE;
    private MoneyOrGTAPPTransaction m_activeMoneyOrGTAPPTransaction;
    private List<AuthorizationExitListener> m_authExitListeners;
    private bool m_battlePayAvailable;
    private Map<string, Network.Bundle> m_bundles;
    private List<Achievement> m_completedAchieves;
    private bool m_configLoaded;
    private long m_configRequestDelayTicks;
    private HashSet<long> m_confirmedTransactionIDs;
    private Currency m_currency;
    private StoreType m_currentStoreType;
    private bool m_featuresReady;
    private bool m_firstMoneyOrGTAPPTransactionSet;
    private long m_goldCostArena;
    private Map<int, Network.GoldCostBooster> m_goldCostBooster;
    private bool m_initComplete;
    private long m_lastCancelRequestTime;
    private long m_lastConfigRequestTime;
    private long m_lastStatusRequestTime;
    private bool m_licenseAchievesListenerRegistered;
    private bool m_noticesReady;
    private bool m_openWhenLastEventFired;
    private bool m_outOfSessionThirdPartyTransaction;
    private List<NetCache.ProfileNoticePurchase> m_outstandingPurchesNotices;
    private TransactionStatus m_previousStatusBeforeAutoCancel;
    private string m_requestedThirdPartyProductId;
    private bool m_shouldAutoCancelThirdPartyTransaction;
    private ShowStoreData m_showStoreData;
    private TransactionStatus m_status;
    private List<StatusChangedListener> m_statusChangedListeners;
    private long m_statusRequestDelayTicks;
    private List<StoreAchievesListener> m_storeAchievesListeners;
    private StoreChallengePrompt m_storeChallengePrompt;
    private StoreDoneWithBAM m_storeDoneWithBAM;
    private List<StoreHiddenListener> m_storeHiddenListeners;
    private StoreLegalBAMLinks m_storeLegalBAMLinks;
    private StorePurchaseAuth m_storePurchaseAuth;
    private Map<StoreType, Store> m_stores;
    private StoreSendToBAM m_storeSendToBAM;
    private List<StoreShownListener> m_storeShownListeners;
    private StoreSummary m_storeSummary;
    private List<SuccessfulPurchaseAckListener> m_successfulPurchaseAckListeners;
    private List<SuccessfulPurchaseListener> m_successfulPurchaseListeners;
    private long m_ticksBeforeAutoCancel;
    private long m_ticksBeforeAutoCancelThirdParty;
    private HashSet<long> m_transactionIDsConclusivelyHandled;
    private HashSet<long> m_transactionIDsThatReloadedNotices;
    private bool m_waitingToShowStore;
    private static readonly long MAX_REQUEST_DELAY_TICKS;
    private static readonly long MIN_CONFIG_REQUEST_DELAY_TICKS;
    private static readonly long MIN_STATUS_REQUEST_DELAY_TICKS;
    public const int NO_ITEM_COUNT_REQUIREMENT = 0;
    public const int NO_PRODUCT_DATA_REQUIREMENT = 0;
    private readonly PlatformDependentValue<string> s_adventureStorePrefab;
    private static readonly Map<AdventureDbId, ProductType> s_adventureToProductMap;
    private readonly PlatformDependentValue<string> s_arenaStorePrefab;
    private static readonly Map<Currency, string> s_currencySpecialFormats;
    private static readonly Map<Currency, Locale> s_currencyToLocaleMap;
    public static readonly PlatformDependentValue<GeneralStoreMode> s_defaultStoreMode;
    private static StoreManager s_instance;
    private readonly PlatformDependentValue<string> s_storeChallengePromptPrefab;
    private readonly PlatformDependentValue<string> s_storeDoneWithBAMPrefab;
    private readonly PlatformDependentValue<string> s_storeLegalBAMLinksPrefab;
    private readonly PlatformDependentValue<string> s_storePrefab;
    private readonly PlatformDependentValue<string> s_storePurchaseAuthPrefab;
    private readonly PlatformDependentValue<string> s_storeSendToBAMPrefab;
    private readonly PlatformDependentValue<string> s_storeSummaryPrefab;
    private static readonly int UNKNOWN_TRANSACTION_ID;

    static StoreManager()
    {
        PlatformDependentValue<GeneralStoreMode> value2 = new PlatformDependentValue<GeneralStoreMode>(PlatformCategory.Screen) {
            PC = GeneralStoreMode.CARDS,
            Phone = GeneralStoreMode.NONE
        };
        s_defaultStoreMode = value2;
        MAX_REQUEST_DELAY_TICKS = 0x165a0bc00L;
        MIN_CONFIG_REQUEST_DELAY_TICKS = 0x23c34600L;
        MIN_STATUS_REQUEST_DELAY_TICKS = 0x1ad27480L;
        EARLY_STATUS_REQUEST_DELAY_TICKS = 0x2faf080L;
        CHALLENGE_CANCEL_STATUS_REQUEST_DELAY_TICKS = 0x989680L;
        UNKNOWN_TRANSACTION_ID = -1;
        Map<Currency, Locale> map = new Map<Currency, Locale>();
        map.Add(Currency.USD, Locale.enUS);
        map.Add(Currency.GBP, Locale.enGB);
        map.Add(Currency.KRW, Locale.koKR);
        map.Add(Currency.EUR, Locale.frFR);
        map.Add(Currency.RUB, Locale.ruRU);
        map.Add(Currency.ARS, Locale.esMX);
        map.Add(Currency.CLP, Locale.esMX);
        map.Add(Currency.MXN, Locale.esMX);
        map.Add(Currency.BRL, Locale.ptBR);
        map.Add(Currency.AUD, Locale.enUS);
        map.Add(Currency.CPT, Locale.zhCN);
        map.Add(Currency.TPT, Locale.zhTW);
        s_currencyToLocaleMap = map;
        DEFAULT_CURRENCY_FORMAT = "{0:C2}";
        Map<Currency, string> map2 = new Map<Currency, string>();
        map2.Add(Currency.KRW, "{0:C0}");
        map2.Add(Currency.TPT, "{0:C0}");
        s_currencySpecialFormats = map2;
        Map<AdventureDbId, ProductType> map3 = new Map<AdventureDbId, ProductType>();
        map3.Add(AdventureDbId.NAXXRAMAS, ProductType.PRODUCT_TYPE_NAXX);
        map3.Add(AdventureDbId.BRM, ProductType.PRODUCT_TYPE_BRM);
        map3.Add(AdventureDbId.LOE, ProductType.PRODUCT_TYPE_LOE);
        s_adventureToProductMap = map3;
        s_instance = null;
        PlatformDependentValue<bool> value3 = new PlatformDependentValue<bool>(PlatformCategory.OS) {
            PC = false,
            Mac = false,
            iOS = true,
            Android = true
        };
        HAS_THIRD_PARTY_APP_STORE = value3;
    }

    public StoreManager()
    {
        PlatformDependentValue<string> value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "StoreMain",
            Phone = "StoreMain_phone"
        };
        this.s_storePrefab = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "StorePurchaseAuth",
            Phone = "StorePurchaseAuth_phone"
        };
        this.s_storePurchaseAuthPrefab = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "StoreSummary",
            Phone = "StoreSummary_phone"
        };
        this.s_storeSummaryPrefab = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "StoreSendToBAM",
            Phone = "StoreSendToBAM_phone"
        };
        this.s_storeSendToBAMPrefab = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "StoreDoneWithBAM",
            Phone = "StoreDoneWithBAM_phone"
        };
        this.s_storeDoneWithBAMPrefab = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "StoreChallengePrompt",
            Phone = "StoreChallengePrompt_phone"
        };
        this.s_storeChallengePromptPrefab = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "StoreLegalBAMLinks",
            Phone = "StoreLegalBAMLinks_phone"
        };
        this.s_storeLegalBAMLinksPrefab = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "ArenaStore",
            Phone = "ArenaStore_phone"
        };
        this.s_arenaStorePrefab = value2;
        value2 = new PlatformDependentValue<string>(PlatformCategory.Screen) {
            PC = "AdventureStore",
            Phone = "AdventureStore_phone"
        };
        this.s_adventureStorePrefab = value2;
        this.m_ticksBeforeAutoCancel = DEFAULT_SECONDS_BEFORE_AUTO_CANCEL * 0x989680L;
        this.m_ticksBeforeAutoCancelThirdParty = DEFAULT_SECONDS_BEFORE_AUTO_CANCEL_THIRD_PARTY * 0x989680L;
        this.m_configRequestDelayTicks = MIN_CONFIG_REQUEST_DELAY_TICKS;
        this.m_bundles = new Map<string, Network.Bundle>();
        this.m_goldCostBooster = new Map<int, Network.GoldCostBooster>();
        this.m_transactionIDsThatReloadedNotices = new HashSet<long>();
        this.m_transactionIDsConclusivelyHandled = new HashSet<long>();
        this.m_stores = new Map<StoreType, Store>();
        this.m_statusChangedListeners = new List<StatusChangedListener>();
        this.m_successfulPurchaseAckListeners = new List<SuccessfulPurchaseAckListener>();
        this.m_successfulPurchaseListeners = new List<SuccessfulPurchaseListener>();
        this.m_authExitListeners = new List<AuthorizationExitListener>();
        this.m_storeShownListeners = new List<StoreShownListener>();
        this.m_storeHiddenListeners = new List<StoreHiddenListener>();
        this.m_storeAchievesListeners = new List<StoreAchievesListener>();
        this.m_statusRequestDelayTicks = MIN_STATUS_REQUEST_DELAY_TICKS;
        this.m_confirmedTransactionIDs = new HashSet<long>();
        this.m_outstandingPurchesNotices = new List<NetCache.ProfileNoticePurchase>();
        this.m_completedAchieves = new List<Achievement>();
    }

    private void ActivateStoreCover()
    {
        Store currentStore = this.GetCurrentStore();
        if (currentStore != null)
        {
            currentStore.ActivateCover(true);
        }
    }

    private bool AlreadyOwnsProduct(ProductType product, int productData)
    {
        switch (product)
        {
            case ProductType.PRODUCT_TYPE_BOOSTER:
            case ProductType.PRODUCT_TYPE_DRAFT:
                return false;

            case ProductType.PRODUCT_TYPE_NAXX:
            case ProductType.PRODUCT_TYPE_BRM:
            case ProductType.PRODUCT_TYPE_LOE:
                return AdventureProgressMgr.Get().OwnsWing(productData);

            case ProductType.PRODUCT_TYPE_CARD_BACK:
                return CardBackManager.Get().IsCardBackOwned(productData);

            case ProductType.PRODUCT_TYPE_HERO:
            {
                string cardID = GameUtils.TranslateDbIdToCardId(productData);
                return CollectionManager.Get().IsCardInCollection(cardID, new CardFlair(TAG_PREMIUM.NORMAL));
            }
        }
        Debug.LogWarning(string.Format("StoreManager.AlreadyOwnsProduct() unknown product {0} productData {1}", product, productData));
        return false;
    }

    private bool AutoCancelPurchaseIfNeeded(long now)
    {
        if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
        {
            if ((now - this.m_lastCancelRequestTime) < this.m_ticksBeforeAutoCancelThirdParty)
            {
                return false;
            }
        }
        else if ((now - this.m_lastCancelRequestTime) < this.m_ticksBeforeAutoCancel)
        {
            return false;
        }
        return this.AutoCancelPurchaseIfPossible();
    }

    private bool AutoCancelPurchaseIfPossible()
    {
        if (this.m_activeMoneyOrGTAPPTransaction != null)
        {
            if (!this.m_activeMoneyOrGTAPPTransaction.Provider.HasValue)
            {
                return false;
            }
            if (((BattlePayProvider) this.m_activeMoneyOrGTAPPTransaction.Provider.Value) != BattlePayProvider.BP_PROVIDER_BLIZZARD)
            {
                if ((this.Status == TransactionStatus.WAIT_THIRD_PARTY_RECEIPT) && this.m_shouldAutoCancelThirdPartyTransaction)
                {
                    Log.Rachelle.Print("StoreManager.AutoCancelPurchaseIfPossible() canceling Third-Party purchase", new object[0]);
                    Log.Yim.Print("StoreManager.AutoCancelPurchaseIfPossible() canceling Third-Party purchase", new object[0]);
                    this.m_previousStatusBeforeAutoCancel = this.Status;
                    this.Status = TransactionStatus.AUTO_CANCELING;
                    this.m_lastCancelRequestTime = DateTime.Now.Ticks;
                    Network.CancelThirdPartyPurchase(CancelPurchase.ThirdPartyCancelReason.USER_CANCELING_TO_UNBLOCK);
                    return true;
                }
                return false;
            }
            switch (this.Status)
            {
                case TransactionStatus.IN_PROGRESS_MONEY:
                case TransactionStatus.IN_PROGRESS_GOLD_GTAPP:
                case TransactionStatus.WAIT_METHOD_OF_PAYMENT:
                case TransactionStatus.WAIT_CONFIRM:
                case TransactionStatus.WAIT_RISK:
                case TransactionStatus.CHALLENGE_SUBMITTED:
                case TransactionStatus.CHALLENGE_CANCELED:
                    Log.Rachelle.Print("StoreManager.AutoCancelPurchaseIfPossible() canceling Blizzard purchase", new object[0]);
                    this.Status = TransactionStatus.AUTO_CANCELING;
                    this.m_lastCancelRequestTime = DateTime.Now.Ticks;
                    Network.CancelBlizzardPurchase(true);
                    return true;
            }
        }
        return false;
    }

    public bool CanBuyBoosterWithGold(int boosterDbId)
    {
        SpecialEventType type;
        DbfRecord record = GameDbf.Booster.GetRecord(boosterDbId);
        if (record == null)
        {
            return false;
        }
        string val = string.Empty;
        if (!record.TryGetString("BUY_WITH_GOLD_EVENT", out val))
        {
            return false;
        }
        if (!EnumUtils.TryGetEnum<SpecialEventType>(val, out type))
        {
            return false;
        }
        return ((type == SpecialEventType.IGNORE) || SpecialEventManager.Get().IsEventActive(type, false));
    }

    private bool CanBuyBundle(Network.Bundle bundleToBuy)
    {
        if (bundleToBuy == null)
        {
            Debug.LogWarning("Null bundle passed to CanBuyBundle!");
            return false;
        }
        if ((AchieveManager.Get() == null) || !AchieveManager.Get().IsReady())
        {
            Debug.LogWarning(string.Format("Attempting to buy bundle {0}, but AchieveManager is not ready yet!", bundleToBuy.ProductID));
            return false;
        }
        if (bundleToBuy.Items.Count < 1)
        {
            Debug.LogWarning(string.Format("Attempting to buy bundle {0}, which does not contain any items!", bundleToBuy.ProductID));
            return false;
        }
        bool productExists = false;
        foreach (Network.Bundle bundle in this.GetAvailableBundlesForProduct(bundleToBuy.Items[0].Product, out productExists, 0, 0))
        {
            if (bundle.ProductID == bundleToBuy.ProductID)
            {
                return true;
            }
        }
        Debug.LogWarning(string.Format("Attempting to buy bundle {0}, which is not available!", bundleToBuy.ProductID));
        return false;
    }

    private bool CanBuyProduct(ProductType product, int productData)
    {
        if ((AchieveManager.Get() == null) || !AchieveManager.Get().IsReady())
        {
            Debug.LogWarning(string.Format("Waiting for AchieveManager to check if we already own product {0} of type {1}, cannot purchase!", productData, product));
            return false;
        }
        if (this.AlreadyOwnsProduct(product, productData))
        {
            Debug.LogWarning(string.Format("Already own product {0} of type {1}, cannot purchase again!", productData, product));
            return false;
        }
        return true;
    }

    private void CancelBlizzardPurchase()
    {
        Log.Rachelle.Print("StoreManager.CancelBlizzardPurchase()", new object[0]);
        this.Status = TransactionStatus.USER_CANCELING;
        this.m_lastCancelRequestTime = DateTime.Now.Ticks;
        Network.CancelBlizzardPurchase(false);
    }

    public void CancelThirdPartyPurchase(CancelPurchase.ThirdPartyCancelReason reason)
    {
        Log.Rachelle.Print(string.Format("StoreManager.CancelThirdPartyPurchase(): reason={0}", reason), new object[0]);
        this.Status = TransactionStatus.USER_CANCELING;
        this.m_lastCancelRequestTime = DateTime.Now.Ticks;
        Network.CancelThirdPartyPurchase(reason);
    }

    private void CompletePurchaseFailure(PurchaseErrorSource source, MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, string thirdPartyID, bool removeThirdPartyReceipt = true)
    {
        if (removeThirdPartyReceipt)
        {
            this.NotifyMobileGamePurchaseResponse(thirdPartyID, false);
        }
        if (this.IsCurrentStoreLoaded())
        {
            if (source == PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE)
            {
                this.ActivateStoreCover();
                this.m_storePurchaseAuth.ShowPurchaseMethodFailure(moneyOrGTAPPTransaction, failDetails, this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType));
            }
            else if (source == PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
            {
                this.ActivateStoreCover();
                this.m_storePurchaseAuth.ShowPreviousPurchaseFailure(moneyOrGTAPPTransaction, failDetails, this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType));
            }
            else if (!this.m_storePurchaseAuth.CompletePurchaseFailure(moneyOrGTAPPTransaction, failDetails))
            {
                Debug.LogWarning(string.Format("StoreManager.CompletePurchaseFailure(): purchased failed ({0}) but the store authorization window has been closed.", failDetails));
                this.DeactivateStoreCover();
            }
        }
    }

    private void ConfirmActiveMoneyTransaction(long id)
    {
        <ConfirmActiveMoneyTransaction>c__AnonStorey348 storey = new <ConfirmActiveMoneyTransaction>c__AnonStorey348 {
            id = id
        };
        if ((this.m_activeMoneyOrGTAPPTransaction == null) || ((this.m_activeMoneyOrGTAPPTransaction.ID != UNKNOWN_TRANSACTION_ID) && (this.m_activeMoneyOrGTAPPTransaction.ID != storey.id)))
        {
            Debug.LogWarning(string.Format("StoreManager.ConfirmActiveMoneyTransaction(id={0}) does not match active money transaction '{1}'", storey.id, this.m_activeMoneyOrGTAPPTransaction));
        }
        Log.Rachelle.Print(string.Format("ConfirmActiveMoneyTransaction() {0}", storey.id), new object[0]);
        Predicate<NetCache.ProfileNoticePurchase> match = new Predicate<NetCache.ProfileNoticePurchase>(storey.<>m__1A1);
        List<NetCache.ProfileNoticePurchase> list = this.m_outstandingPurchesNotices.FindAll(match);
        this.m_outstandingPurchesNotices.RemoveAll(match);
        foreach (NetCache.ProfileNoticePurchase purchase in list)
        {
            Network.AckNotice(purchase.NoticeID);
        }
        this.m_confirmedTransactionIDs.Add(storey.id);
        this.m_activeMoneyOrGTAPPTransaction = null;
    }

    private void DeactivateStoreCover()
    {
        Store currentStore = this.GetCurrentStore();
        if (currentStore != null)
        {
            currentStore.ActivateCover(false);
        }
    }

    public static void DestroyInstance()
    {
        Store store = s_instance.GetStore(StoreType.GENERAL_STORE);
        if (store != null)
        {
            store.Hide();
            UnityEngine.Object.DestroyObject(store);
        }
        if ((AchieveManager.Get() != null) && (s_instance != null))
        {
            AchieveManager.Get().RemoveActiveAchievesUpdatedListener(new AchieveManager.ActiveAchievesUpdatedCallback(s_instance.OnAchievesUpdated));
            AchieveManager.Get().RemoveLicenseAddedAchievesUpdatedListener(new AchieveManager.LicenseAddedAchievesUpdatedCallback(s_instance.OnLicenseAddedAchievesUpdated));
        }
        s_instance = null;
    }

    public bool DoZeroCostTransactionIfPossible(StoreType storeType, Store.ExitCallback exitCallback, object userData, ProductType product, int productData = 0, int numItems = 0)
    {
        if (this.m_waitingToShowStore)
        {
            Log.Rachelle.Print("StoreManager.DoZeroCostTransactionIfPossible(): already waiting to show store", new object[0]);
            return false;
        }
        bool productExists = false;
        List<Network.Bundle> list = this.GetAvailableBundlesForProduct(product, out productExists, productData, numItems);
        Network.Bundle bundle = null;
        foreach (Network.Bundle bundle2 in list)
        {
            if (bundle2.IsFree())
            {
                bundle = bundle2;
                break;
            }
        }
        if (bundle == null)
        {
            return false;
        }
        ZeroCostTransactionData data = new ZeroCostTransactionData(bundle);
        this.m_currentStoreType = storeType;
        this.m_showStoreData.m_exitCallback = exitCallback;
        this.m_showStoreData.m_exitCallbackUserData = userData;
        this.m_showStoreData.m_isTotallyFake = true;
        this.m_showStoreData.m_storeProduct = product;
        this.m_showStoreData.m_storeProductData = (bundle.Items.Count != 1) ? productData : bundle.Items[0].ProductData;
        this.m_showStoreData.m_storeMode = GeneralStoreMode.NONE;
        this.m_showStoreData.m_zeroCostTransactionData = data;
        this.ShowStoreWhenLoaded();
        return true;
    }

    private void FireStatusChangedEventIfNeeded()
    {
        bool isOpen = this.IsOpen();
        if (this.m_openWhenLastEventFired != isOpen)
        {
            foreach (StatusChangedListener listener in this.m_statusChangedListeners.ToArray())
            {
                listener.Fire(isOpen);
            }
            this.m_openWhenLastEventFired = isOpen;
        }
    }

    public string FormatCostBundle(Network.Bundle bundle)
    {
        if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
        {
            string mobileProductId = string.Empty;
            if (ApplicationMgr.GetAndroidStore() == AndroidStore.GOOGLE)
            {
                mobileProductId = bundle.GooglePlayID;
            }
            else if (ApplicationMgr.GetAndroidStore() == AndroidStore.AMAZON)
            {
                mobileProductId = bundle.AmazonID;
            }
            string localizedProductPrice = StoreMobilePurchase.GetLocalizedProductPrice(mobileProductId);
            if (localizedProductPrice.Length > 0)
            {
                return localizedProductPrice;
            }
        }
        return this.FormatCostText(bundle.Cost);
    }

    public string FormatCostText(double cost)
    {
        return FormatCostText(cost, this.m_currency);
    }

    public static string FormatCostText(double cost, Currency currency)
    {
        string str2;
        if (!s_currencyToLocaleMap.ContainsKey(currency))
        {
            Error.AddFatalLoc("GLOBAL_ERROR_CURRENCY_INVALID", new object[0]);
        }
        Locale locale = s_currencyToLocaleMap[currency];
        if (!s_currencySpecialFormats.TryGetValue(currency, out str2))
        {
            str2 = DEFAULT_CURRENCY_FORMAT;
        }
        CultureInfo provider = CultureInfo.CreateSpecificCulture(Localization.ConvertLocaleToDotNet(locale));
        switch (locale)
        {
            case Locale.koKR:
                provider.NumberFormat.CurrencySymbol = "B";
                break;

            case Locale.ruRU:
                provider.NumberFormat.CurrencySymbol = string.Format(" {0}", provider.NumberFormat.CurrencySymbol);
                break;

            case Locale.zhTW:
                provider.NumberFormat.CurrencyGroupSeparator = string.Empty;
                break;
        }
        object[] args = new object[] { cost };
        return string.Format(provider, str2, args);
    }

    public static StoreManager Get()
    {
        if (s_instance == null)
        {
            s_instance = new StoreManager();
        }
        return s_instance;
    }

    public static ProductType GetAdventureProductType(AdventureDbId adventure)
    {
        ProductType type = ProductType.PRODUCT_TYPE_UNKNOWN;
        if (s_adventureToProductMap.TryGetValue(adventure, out type))
        {
            return type;
        }
        return ProductType.PRODUCT_TYPE_UNKNOWN;
    }

    public List<Network.Bundle> GetAllBundlesForProduct(ProductType product, int productData = 0, int numItemsRequired = 0)
    {
        List<Network.Bundle> list = new List<Network.Bundle>();
        foreach (Network.Bundle bundle in this.m_bundles.Values)
        {
            if ((numItemsRequired != 0) && (bundle.Items.Count != numItemsRequired))
            {
                continue;
            }
            bool flag = false;
            foreach (Network.BundleItem item in bundle.Items)
            {
                if ((item.Product == product) && ((productData == 0) || (item.ProductData == productData)))
                {
                    flag = true;
                    break;
                }
            }
            if (flag && SpecialEventManager.Get().IsEventActive(bundle.ProductEvent, true))
            {
                list.Add(bundle);
            }
        }
        return list;
    }

    private long GetArenaGoldCostNoGTAPP()
    {
        if (this.m_goldCostArena == 0)
        {
            Debug.LogWarning("StoreManager.GetArenaGoldCostNoGTAPP(): don't have a gold price for arena purchases");
        }
        return this.m_goldCostArena;
    }

    public bool GetAvailableAdventureBundle(AdventureDbId adventure, out Network.Bundle bundle)
    {
        bool flag;
        return this.GetAvailableAdventureBundle(adventure, out bundle, out flag);
    }

    public bool GetAvailableAdventureBundle(AdventureDbId adventure, out Network.Bundle bundle, out bool productExists)
    {
        ProductType adventureProductType = GetAdventureProductType(adventure);
        if (adventureProductType == ProductType.PRODUCT_TYPE_UNKNOWN)
        {
            bundle = null;
            productExists = false;
            return false;
        }
        return this.GetAvailableAdventureBundle(adventureProductType, out bundle, out productExists);
    }

    public bool GetAvailableAdventureBundle(ProductType product, out Network.Bundle bundle, out bool productExists)
    {
        bundle = null;
        bool flag = false;
        List<Network.Bundle> list = null;
        switch (product)
        {
            case ProductType.PRODUCT_TYPE_NAXX:
                list = this.GetAvailableBundlesForProduct(product, out flag, 5, 0);
                break;

            case ProductType.PRODUCT_TYPE_BRM:
                list = this.GetAvailableBundlesForProduct(product, out flag, 10, 0);
                break;

            case ProductType.PRODUCT_TYPE_LOE:
                list = this.GetAvailableBundlesForProduct(product, out flag, 14, 0);
                break;
        }
        productExists = flag;
        if (list != null)
        {
            foreach (Network.Bundle bundle2 in list)
            {
                int count = bundle2.Items.Count;
                if (count != 0)
                {
                    if (bundle == null)
                    {
                        bundle = bundle2;
                    }
                    else if (bundle.Items.Count <= count)
                    {
                        bundle = bundle2;
                    }
                }
            }
        }
        return (bundle != null);
    }

    public List<Network.Bundle> GetAvailableBundlesForProduct(ProductType product, out bool productExists, int productData = 0, int numItemsRequired = 0)
    {
        productExists = false;
        bool flag = false;
        List<Network.Bundle> list = new List<Network.Bundle>();
        foreach (Network.Bundle bundle in this.m_bundles.Values)
        {
            if ((numItemsRequired != 0) && (bundle.Items.Count != numItemsRequired))
            {
                continue;
            }
            bool flag2 = false;
            bool flag3 = false;
            foreach (Network.BundleItem item in bundle.Items)
            {
                if (item.Product == product)
                {
                    productExists = true;
                }
                flag3 = this.AlreadyOwnsProduct(item.Product, item.ProductData);
                if (flag3)
                {
                    flag = true;
                    break;
                }
                if ((item.Product == product) && ((productData == 0) || (item.ProductData == productData)))
                {
                    flag2 = true;
                    break;
                }
            }
            if ((flag2 && !flag3) && SpecialEventManager.Get().IsEventActive(bundle.ProductEvent, true))
            {
                list.Add(bundle);
            }
        }
        if ((list.Count == 0) && !flag)
        {
            productExists = false;
        }
        return list;
    }

    private long GetBoosterGoldCostNoGTAPP(int boosterId)
    {
        if (this.m_goldCostBooster.ContainsKey(boosterId))
        {
            return this.m_goldCostBooster[boosterId].Cost;
        }
        Debug.LogWarning(string.Format("StoreManager.GetBoosterGoldCostNoGTAPP(): don't have a gold price for booster type {0}", boosterId));
        return 0L;
    }

    public Network.Bundle GetBundle(string productID)
    {
        if (this.m_bundles.ContainsKey(productID))
        {
            return this.m_bundles[productID];
        }
        Debug.LogWarning(string.Format("StoreManager.GetBundle(): don't have a bundle for productID '{0}'", productID));
        return null;
    }

    public Store GetCurrentStore()
    {
        return this.GetStore(this.m_currentStoreType);
    }

    public long GetGoldCostNoGTAPP(NoGTAPPTransactionData noGTAPPTransactionData)
    {
        if (noGTAPPTransactionData == null)
        {
            Debug.LogWarning("StoreManager.GetGoldPriceNoGTAPP(): noGTAPPTransactionData is null!");
            return 0L;
        }
        long boosterGoldCostNoGTAPP = 0L;
        ProductType product = noGTAPPTransactionData.Product;
        if (product == ProductType.PRODUCT_TYPE_BOOSTER)
        {
            boosterGoldCostNoGTAPP = this.GetBoosterGoldCostNoGTAPP(noGTAPPTransactionData.ProductData);
        }
        else if (product == ProductType.PRODUCT_TYPE_DRAFT)
        {
            boosterGoldCostNoGTAPP = this.GetArenaGoldCostNoGTAPP();
        }
        else
        {
            Debug.LogWarning(string.Format("StoreManager.GetGoldPriceNoGTAPP(): don't have a no-GTAPP gold price for product {0} data {1}", noGTAPPTransactionData.Product, noGTAPPTransactionData.ProductData));
            boosterGoldCostNoGTAPP = 0L;
        }
        return (boosterGoldCostNoGTAPP * noGTAPPTransactionData.Quantity);
    }

    public bool GetHeroBundle(int heroDbId, out Network.Bundle heroBundle)
    {
        bool productExists = false;
        List<Network.Bundle> list = this.GetAvailableBundlesForProduct(ProductType.PRODUCT_TYPE_HERO, out productExists, 0, 0);
        heroBundle = null;
        if (!productExists)
        {
            return false;
        }
        foreach (Network.Bundle bundle in list)
        {
            foreach (Network.BundleItem item in bundle.Items)
            {
                if ((item.Product == ProductType.PRODUCT_TYPE_HERO) && (item.ProductData == heroDbId))
                {
                    heroBundle = bundle;
                    break;
                }
            }
        }
        return true;
    }

    public bool GetHeroBundle(string heroId, out Network.Bundle heroBundle)
    {
        DbfRecord record = GameDbf.Card.GetRecord("NOTE_MINI_GUID", heroId);
        if (record == null)
        {
            heroBundle = null;
            return false;
        }
        return this.GetHeroBundle(record.GetId(), out heroBundle);
    }

    private long GetIncreasedRequestDelayTicks(long currentRequestDelayTicks, long minimumDelayTicks)
    {
        if (currentRequestDelayTicks < minimumDelayTicks)
        {
            return minimumDelayTicks;
        }
        long num = currentRequestDelayTicks * 2L;
        return Math.Min(num, MAX_REQUEST_DELAY_TICKS);
    }

    public Network.Bundle GetLowestCostUnownedBundle(ProductType product, int productData, int numItemsRequired = 0)
    {
        List<Network.Bundle> list = Get().GetAllBundlesForProduct(product, productData, numItemsRequired);
        Network.Bundle bundle = null;
        foreach (Network.Bundle bundle2 in list)
        {
            if (((numItemsRequired == 0) || (bundle2.Items.Count == numItemsRequired)) && !this.IsProductAlreadyOwned(bundle2))
            {
                if (bundle == null)
                {
                    bundle = bundle2;
                }
                else if (bundle.Cost > bundle2.Cost)
                {
                    bundle = bundle2;
                }
            }
        }
        return bundle;
    }

    private string GetMultiItemProductName(List<Network.BundleItem> items)
    {
        HashSet<ProductType> productsInItemList = this.GetProductsInItemList(items);
        if (productsInItemList.Contains(ProductType.PRODUCT_TYPE_NAXX))
        {
            object[] args = new object[] { items.Count };
            return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_NAXX_WING_BUNDLE", args);
        }
        if (productsInItemList.Contains(ProductType.PRODUCT_TYPE_BRM))
        {
            if (productsInItemList.Contains(ProductType.PRODUCT_TYPE_CARD_BACK))
            {
                return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_BRM_PRESALE_BUNDLE");
            }
            object[] objArray2 = new object[] { items.Count };
            return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_BRM_WING_BUNDLE", objArray2);
        }
        if (productsInItemList.Contains(ProductType.PRODUCT_TYPE_LOE))
        {
            object[] objArray3 = new object[] { items.Count };
            return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_LOE_WING_BUNDLE", objArray3);
        }
        if (productsInItemList.Contains(ProductType.PRODUCT_TYPE_HERO))
        {
            if (<>f__am$cache47 == null)
            {
                <>f__am$cache47 = obj => obj.Product == ProductType.PRODUCT_TYPE_HERO;
            }
            Network.BundleItem item = items.Find(<>f__am$cache47);
            if (item != null)
            {
                return this.GetSingleItemProductName(item);
            }
        }
        else if (productsInItemList.Contains(ProductType.PRODUCT_TYPE_BOOSTER) && productsInItemList.Contains(ProductType.PRODUCT_TYPE_CARD_BACK))
        {
            if (<>f__am$cache48 == null)
            {
                <>f__am$cache48 = obj => (obj.Product == ProductType.PRODUCT_TYPE_BOOSTER) && (obj.ProductData == 10);
            }
            if (items.Find(<>f__am$cache48) != null)
            {
                return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_TGT_PRESALE_BUNDLE");
            }
        }
        string str = string.Empty;
        foreach (Network.BundleItem item3 in items)
        {
            str = str + string.Format("[Product={0},ProductData={1},Quantity={2}],", item3.Product, item3.ProductData, item3.Quantity);
        }
        Debug.LogWarning(string.Format("StoreManager.GetMultiItemProductName(): don't know how to format product name for items '{0}'", str));
        return string.Empty;
    }

    private NetCache.NetCacheFeatures GetNetCacheFeatures()
    {
        if (!this.FeaturesReady)
        {
            return null;
        }
        NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
        if (netObject == null)
        {
            this.FeaturesReady = false;
        }
        return netObject;
    }

    public int GetNonPreorderItemCount(List<Network.BundleItem> items)
    {
        int num = 0;
        foreach (Network.BundleItem item in items)
        {
            if (item.Product != ProductType.PRODUCT_TYPE_CARD_BACK)
            {
                num++;
            }
        }
        return num;
    }

    public string GetProductName(List<Network.BundleItem> items)
    {
        if (items.Count == 1)
        {
            Network.BundleItem item = items[0];
            return this.GetSingleItemProductName(item);
        }
        return this.GetMultiItemProductName(items);
    }

    public string GetProductQuantityText(ProductType product, int productData, int quantity)
    {
        string str = string.Empty;
        ProductType type = product;
        if (type != ProductType.PRODUCT_TYPE_BOOSTER)
        {
            if (type == ProductType.PRODUCT_TYPE_DRAFT)
            {
                object[] objArray2 = new object[] { quantity, GameStrings.Get("GLUE_STORE_PRODUCT_NAME_FORGE_TICKET") };
                return GameStrings.Format("GLUE_STORE_SUMMARY_ITEM_ORDERED", objArray2);
            }
            Debug.LogWarning(string.Format("StoreManager.GetProductQuantityText(): don't know how to format quantity for product {0} (data {1})", product, productData));
            return str;
        }
        object[] args = new object[] { quantity };
        return GameStrings.Format("GLUE_STORE_QUANTITY_PACK", args);
    }

    public HashSet<ProductType> GetProductsInBundle(Network.Bundle bundle)
    {
        if (bundle == null)
        {
            return new HashSet<ProductType>();
        }
        return this.GetProductsInItemList(bundle.Items);
    }

    public HashSet<ProductType> GetProductsInItemList(List<Network.BundleItem> items)
    {
        HashSet<ProductType> set = new HashSet<ProductType>();
        foreach (Network.BundleItem item in items)
        {
            set.Add(item.Product);
        }
        return set;
    }

    private string GetSingleItemProductName(Network.BundleItem item)
    {
        string name = string.Empty;
        switch (item.Product)
        {
            case ProductType.PRODUCT_TYPE_BOOSTER:
            {
                string locString = GameDbf.Booster.GetRecord(item.ProductData).GetLocString("NAME");
                object[] args = new object[] { item.Quantity, locString };
                return GameStrings.Format("GLUE_STORE_PRODUCT_NAME_PACK", args);
            }
            case ProductType.PRODUCT_TYPE_DRAFT:
                return GameStrings.Get("GLUE_STORE_PRODUCT_NAME_FORGE_TICKET");

            case ProductType.PRODUCT_TYPE_NAXX:
            case ProductType.PRODUCT_TYPE_BRM:
            case ProductType.PRODUCT_TYPE_LOE:
                return AdventureProgressMgr.Get().GetWingName(item.ProductData);

            case ProductType.PRODUCT_TYPE_CARD_BACK:
            {
                DbfRecord record = GameDbf.CardBack.GetRecord(item.ProductData);
                if (record != null)
                {
                    name = record.GetLocString("NAME");
                }
                return name;
            }
            case ProductType.PRODUCT_TYPE_HERO:
            {
                EntityDef entityDef = DefLoader.Get().GetEntityDef(item.ProductData);
                if (entityDef != null)
                {
                    name = entityDef.GetName();
                }
                return name;
            }
        }
        Debug.LogWarning(string.Format("StoreManager.GetSingleItemProductName(): don't know how to format name for bundle product {0}", item.Product));
        return name;
    }

    public Store GetStore(StoreType storeType)
    {
        Store store = null;
        this.m_stores.TryGetValue(storeType, out store);
        return store;
    }

    public string GetTaxText()
    {
        Currency currency = this.m_currency;
        switch (currency)
        {
            case Currency.USD:
                return GameStrings.Get("GLUE_STORE_SUMMARY_TAX_DISCLAIMER_USD");

            case Currency.KRW:
                break;

            default:
                if ((currency != Currency.CPT) && (currency != Currency.TPT))
                {
                    return GameStrings.Get("GLUE_STORE_SUMMARY_TAX_DISCLAIMER");
                }
                break;
        }
        return string.Empty;
    }

    public void GetThirdPartyPurchaseStatus(string transactionID)
    {
        Network.GetThirdPartyPurchaseStatus(transactionID);
    }

    public int GetTotalBoostersAcquired()
    {
        NetCache.NetCacheBoosterTallies netObject = NetCache.Get().GetNetObject<NetCache.NetCacheBoosterTallies>();
        if ((netObject == null) || (netObject.BoosterTallies == null))
        {
            return 0;
        }
        int num = 0;
        foreach (NetCache.BoosterTally tally in netObject.BoosterTallies)
        {
            num += tally.Count;
        }
        return num;
    }

    private void HandleFailedRiskError(PurchaseErrorSource source)
    {
        bool flag = TransactionStatus.CHALLENGE_CANCELED == this.Status;
        this.Status = TransactionStatus.READY;
        if (flag)
        {
            Log.Rachelle.Print("HandleFailedRiskError for canceled transaction", new object[0]);
            if (this.m_activeMoneyOrGTAPPTransaction != null)
            {
                this.ConfirmActiveMoneyTransaction(this.m_activeMoneyOrGTAPPTransaction.ID);
            }
            this.DeactivateStoreCover();
        }
        else if (this.IsCurrentStoreLoaded() && this.GetCurrentStore().IsShown())
        {
            this.m_storePurchaseAuth.Hide();
            this.ActivateStoreCover();
            this.m_storeSendToBAM.Show(this.m_activeMoneyOrGTAPPTransaction, StoreSendToBAM.BAMReason.NEED_PASSWORD_RESET, string.Empty, source == PurchaseErrorSource.FROM_PREVIOUS_PURCHASE);
        }
    }

    private void HandlePurchaseError(PurchaseErrorSource source, Network.PurchaseErrorInfo.ErrorType purchaseErrorType, string purchaseErrorCode, string thirdPartyID, bool isGTAPP)
    {
        if ((this.IsConclusiveState(purchaseErrorType) && (this.m_activeMoneyOrGTAPPTransaction != null)) && this.m_transactionIDsConclusivelyHandled.Contains(this.m_activeMoneyOrGTAPPTransaction.ID))
        {
            object[] objArray1 = new object[] { this.m_activeMoneyOrGTAPPTransaction, purchaseErrorType };
            Log.Rachelle.Print("HandlePurchaseError already handled purchase error for conclusive state on transaction (Transaction: {0}, current purchaseErrorType = {1})", objArray1);
            return;
        }
        object[] args = new object[] { source, purchaseErrorType, purchaseErrorCode, thirdPartyID };
        Log.Yim.Print(string.Format("HandlePurchaseError source={0} purchaseErrorType={1} purchaseErrorCode={2} thirdPartyID={3}", args), new object[0]);
        string failDetails = string.Empty;
        bool removeThirdPartyReceipt = true;
        if (((source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE) && this.ReloadNoticesIfNecessary(purchaseErrorType)) && ((this.Status == TransactionStatus.UNKNOWN) && (source == PurchaseErrorSource.FROM_STATUS_OR_PURCHASE_RESPONSE)))
        {
            Log.Rachelle.Print("StoreManager.HandPurchaseError(): force reloading notices for very first status", new object[0]);
            this.NoticesReady = false;
        }
        Network.PurchaseErrorInfo.ErrorType type = purchaseErrorType;
        switch ((type + 1))
        {
            case Network.PurchaseErrorInfo.ErrorType.SUCCESS:
                Debug.LogWarning("StoreManager.HandlePurchaseError: purchase error is UNKNOWN, taking no action on this purchase");
                return;

            case Network.PurchaseErrorInfo.ErrorType.STILL_IN_PROGRESS:
                if (source != PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE)
                {
                    if (isGTAPP)
                    {
                        NetCache.Get().RefreshNetObject<NetCache.NetCacheGoldBalance>();
                    }
                    this.HandlePurchaseSuccess(new PurchaseErrorSource?(source), this.m_activeMoneyOrGTAPPTransaction, thirdPartyID);
                    break;
                }
                Debug.LogWarning("StoreManager.HandlePurchaseError: received SUCCESS from payment method purchase error.");
                break;

            case Network.PurchaseErrorInfo.ErrorType.INVALID_BNET:
                if (source != PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE)
                {
                    if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
                    {
                        this.Status = !isGTAPP ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP;
                    }
                    return;
                }
                Debug.LogWarning("StoreManager.HandlePurchaseError: received STILL_IN_PROGRESS from payment method purchase error.");
                return;

            case Network.PurchaseErrorInfo.ErrorType.SERVICE_NA:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_BNET_ID");
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.PURCHASE_IN_PROGRESS:
                if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
                {
                    if (this.Status != TransactionStatus.UNKNOWN)
                    {
                        this.BattlePayAvailable = false;
                    }
                    this.Status = TransactionStatus.UNKNOWN;
                }
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_NO_BATTLEPAY");
                this.CompletePurchaseFailure(source, this.m_activeMoneyOrGTAPPTransaction, failDetails, thirdPartyID, true);
                return;

            case Network.PurchaseErrorInfo.ErrorType.DATABASE:
                if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
                {
                    this.Status = !isGTAPP ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP;
                }
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_IN_PROGRESS");
                this.CompletePurchaseFailure(source, this.m_activeMoneyOrGTAPPTransaction, failDetails, thirdPartyID, true);
                return;

            case Network.PurchaseErrorInfo.ErrorType.INVALID_QUANTITY:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_DATABASE");
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.DUPLICATE_LICENSE:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_QUANTITY");
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.REQUEST_NOT_SENT:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_LICENSE");
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.NO_ACTIVE_BPAY:
                if ((source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE) && (this.Status != TransactionStatus.UNKNOWN))
                {
                    this.BattlePayAvailable = false;
                }
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_NO_BATTLEPAY");
                if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
                {
                    removeThirdPartyReceipt = false;
                }
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.FAILED_RISK:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_NO_ACTIVE_BPAY");
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.CANCELED:
                this.HandleFailedRiskError(source);
                return;

            case Network.PurchaseErrorInfo.ErrorType.WAIT_MOP:
                if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
                {
                    this.Status = TransactionStatus.READY;
                }
                return;

            case Network.PurchaseErrorInfo.ErrorType.WAIT_CONFIRM:
                Log.Rachelle.Print("StoreManager.HandlePurchaseError: Status is WAIT_MOP.. this probably shouldn't be happening.", new object[0]);
                if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
                {
                    if (this.Status != TransactionStatus.UNKNOWN)
                    {
                        this.Status = TransactionStatus.WAIT_METHOD_OF_PAYMENT;
                        return;
                    }
                    Log.Rachelle.Print(string.Format("StoreManager.HandlePurchaseError: Status is WAIT_MOP, previous Status was UNKNOWN, source = {0}", source), new object[0]);
                }
                return;

            case Network.PurchaseErrorInfo.ErrorType.WAIT_RISK:
                if ((source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE) && (this.Status == TransactionStatus.UNKNOWN))
                {
                    Log.Rachelle.Print(string.Format("StoreManager.HandlePurchaseError: Status is WAIT_CONFIRM, previous Status was UNKNOWN, source = {0}. Going to try to cancel the purchase.", source), new object[0]);
                    this.CancelBlizzardPurchase();
                }
                return;

            case Network.PurchaseErrorInfo.ErrorType.PRODUCT_NA:
                if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
                {
                    Log.Rachelle.Print("StoreManager.HandlePurchaseError: Waiting for client to respond to Risk challenge", new object[0]);
                    if (this.Status != TransactionStatus.UNKNOWN)
                    {
                        if ((this.Status == TransactionStatus.CHALLENGE_SUBMITTED) || (this.Status == TransactionStatus.CHALLENGE_CANCELED))
                        {
                            Log.Rachelle.Print(string.Format("StoreManager.HandlePurchaseError: Status = {0}; ignoring WAIT_RISK purchase error info", this.Status), new object[0]);
                        }
                        else
                        {
                            this.Status = TransactionStatus.WAIT_RISK;
                        }
                        return;
                    }
                    Log.Rachelle.Print(string.Format("StoreManager.HandlePurchaseError: Status is WAIT_RISK, previous Status was UNKNOWN, source = {0}", source), new object[0]);
                }
                return;

            case Network.PurchaseErrorInfo.ErrorType.RISK_TIMEOUT:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_PRODUCT_NA");
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.PRODUCT_ALREADY_OWNED:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_CHALLENGE_TIMEOUT");
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.WAIT_THIRD_PARTY_RECEIPT:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_PRODUCT_ALREADY_OWNED");
                goto Label_05C7;

            case Network.PurchaseErrorInfo.ErrorType.PRODUCT_EVENT_HAS_ENDED:
                this.OnThirdPartyPurchaseApproved();
                return;

            case (Network.PurchaseErrorInfo.ErrorType.RISK_TIMEOUT | Network.PurchaseErrorInfo.ErrorType.PURCHASE_IN_PROGRESS):
                failDetails = GameStrings.Get("GLUE_STORE_PRODUCT_EVENT_HAS_ENDED");
                goto Label_05C7;

            default:
                switch (type)
                {
                    case Network.PurchaseErrorInfo.ErrorType.BP_GENERIC_FAIL:
                    case Network.PurchaseErrorInfo.ErrorType.BP_RISK_ERROR:
                    case Network.PurchaseErrorInfo.ErrorType.BP_PAYMENT_AUTH:
                    case Network.PurchaseErrorInfo.ErrorType.BP_PROVIDER_DENIED:
                        if (isGTAPP)
                        {
                            failDetails = GameStrings.Get("GLUE_STORE_FAIL_GOLD_GENERIC");
                            goto Label_05C7;
                        }
                        this.HandleSendToBAMError(source, StoreSendToBAM.BAMReason.GENERIC_PAYMENT_FAIL, purchaseErrorCode);
                        if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
                        {
                            this.CompletePurchaseFailure(source, this.m_activeMoneyOrGTAPPTransaction, failDetails, thirdPartyID, true);
                        }
                        return;

                    case Network.PurchaseErrorInfo.ErrorType.BP_INVALID_CC_EXPIRY:
                        if (isGTAPP)
                        {
                            failDetails = GameStrings.Get("GLUE_STORE_FAIL_GOLD_GENERIC");
                            goto Label_05C7;
                        }
                        this.HandleSendToBAMError(source, StoreSendToBAM.BAMReason.CREDIT_CARD_EXPIRED, string.Empty);
                        return;

                    case Network.PurchaseErrorInfo.ErrorType.BP_NO_VALID_PAYMENT:
                        if (source != PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE)
                        {
                            if (!isGTAPP)
                            {
                                this.HandleSendToBAMError(source, StoreSendToBAM.BAMReason.NO_VALID_PAYMENT_METHOD, string.Empty);
                                return;
                            }
                            failDetails = GameStrings.Get("GLUE_STORE_FAIL_GOLD_GENERIC");
                        }
                        else
                        {
                            Debug.LogWarning("StoreManager.HandlePurchaseError: received BP_NO_VALID_PAYMENT from payment method purchase error.");
                        }
                        goto Label_05C7;

                    case Network.PurchaseErrorInfo.ErrorType.BP_PURCHASE_BAN:
                        failDetails = GameStrings.Get("GLUE_STORE_FAIL_PURCHASE_BAN");
                        goto Label_05C7;

                    case Network.PurchaseErrorInfo.ErrorType.BP_SPENDING_LIMIT:
                        if (isGTAPP)
                        {
                            failDetails = GameStrings.Get("GLUE_STORE_FAIL_GOLD_GENERIC");
                        }
                        else
                        {
                            failDetails = GameStrings.Get("GLUE_STORE_FAIL_SPENDING_LIMIT");
                        }
                        goto Label_05C7;

                    case Network.PurchaseErrorInfo.ErrorType.BP_PARENTAL_CONTROL:
                        failDetails = GameStrings.Get("GLUE_STORE_FAIL_PARENTAL_CONTROL");
                        goto Label_05C7;

                    case Network.PurchaseErrorInfo.ErrorType.BP_THROTTLED:
                        failDetails = GameStrings.Get("GLUE_STORE_FAIL_THROTTLED");
                        goto Label_05C7;

                    case Network.PurchaseErrorInfo.ErrorType.BP_THIRD_PARTY_BAD_RECEIPT:
                    case Network.PurchaseErrorInfo.ErrorType.BP_THIRD_PARTY_RECEIPT_USED:
                        failDetails = GameStrings.Get("GLUE_STORE_FAIL_THIRD_PARTY_BAD_RECEIPT");
                        goto Label_05C7;

                    case Network.PurchaseErrorInfo.ErrorType.BP_PRODUCT_UNIQUENESS_VIOLATED:
                        this.HandleSendToBAMError(source, StoreSendToBAM.BAMReason.PRODUCT_UNIQUENESS_VIOLATED, string.Empty);
                        return;

                    case Network.PurchaseErrorInfo.ErrorType.BP_REGION_IS_DOWN:
                        failDetails = GameStrings.Get("GLUE_STORE_FAIL_REGION_IS_DOWN");
                        goto Label_05C7;
                }
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_GENERAL");
                goto Label_05C7;
        }
        return;
    Label_05C7:
        if (source != PurchaseErrorSource.FROM_PREVIOUS_PURCHASE)
        {
            bool flag2 = TransactionStatus.UNKNOWN == this.Status;
            this.Status = TransactionStatus.READY;
            if (flag2)
            {
                if (this.IsConclusiveState(purchaseErrorType) && removeThirdPartyReceipt)
                {
                    this.NotifyMobileGamePurchaseResponse(thirdPartyID, false);
                }
                return;
            }
        }
        this.CompletePurchaseFailure(source, this.m_activeMoneyOrGTAPPTransaction, failDetails, thirdPartyID, removeThirdPartyReceipt);
    }

    private void HandlePurchaseSuccess(PurchaseErrorSource? source, MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string thirdPartyID)
    {
        PaymentMethod method;
        Network.Bundle bundle;
        this.Status = TransactionStatus.READY;
        if (moneyOrGTAPPTransaction == null)
        {
            method = PaymentMethod.GOLD_NO_GTAPP;
            bundle = null;
        }
        else
        {
            method = !moneyOrGTAPPTransaction.IsGTAPP ? PaymentMethod.MONEY : PaymentMethod.GOLD_GTAPP;
            bundle = this.GetBundle(moneyOrGTAPPTransaction.ProductID);
        }
        bool flag = false;
        bool flag2 = false;
        if (bundle != null)
        {
            foreach (Network.BundleItem item in bundle.Items)
            {
                switch (item.Product)
                {
                    case ProductType.PRODUCT_TYPE_CARD_BACK:
                        flag = true;
                        break;

                    case ProductType.PRODUCT_TYPE_HERO:
                        flag2 = true;
                        break;
                }
            }
        }
        if (((method != PaymentMethod.GOLD_NO_GTAPP) && (AchieveManager.Get() != null)) && AchieveManager.Get().HasIncompletePurchaseAchieves())
        {
            flag = true;
        }
        if (flag && (AchieveManager.Get() != null))
        {
            StoreAchievesData userData = new StoreAchievesData(bundle, method);
            AchieveManager.Get().UpdateActiveAchieves(new AchieveManager.ActiveAchievesUpdatedCallback(this.OnAchievesUpdated), userData);
        }
        if (flag2)
        {
            NetCache.Get().RefreshNetObject<NetCache.NetCacheFavoriteHeroes>();
        }
        foreach (SuccessfulPurchaseListener listener in this.m_successfulPurchaseListeners.ToArray())
        {
            listener.Fire(bundle, method);
        }
        this.NotifyMobileGamePurchaseResponse(thirdPartyID, true);
        if (this.IsCurrentStoreLoaded())
        {
            if (source.HasValue && (((PurchaseErrorSource) source.Value) == PurchaseErrorSource.FROM_PREVIOUS_PURCHASE))
            {
                this.ActivateStoreCover();
                this.m_storePurchaseAuth.ShowPreviousPurchaseSuccess(moneyOrGTAPPTransaction, this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType));
            }
            else
            {
                Store currentStore = this.GetCurrentStore();
                switch (method)
                {
                    case PaymentMethod.GOLD_GTAPP:
                    case PaymentMethod.GOLD_NO_GTAPP:
                        currentStore.OnGoldSpent();
                        break;

                    default:
                        currentStore.OnMoneySpent();
                        break;
                }
                this.m_storePurchaseAuth.CompletePurchaseSuccess(moneyOrGTAPPTransaction);
            }
        }
    }

    private void HandleSendToBAMError(PurchaseErrorSource source, StoreSendToBAM.BAMReason reason, string errorCode)
    {
        this.Status = TransactionStatus.READY;
        if (this.IsCurrentStoreLoaded() && this.GetCurrentStore().IsShown())
        {
            this.m_storePurchaseAuth.Hide();
            this.ActivateStoreCover();
            this.m_storeSendToBAM.Show(this.m_activeMoneyOrGTAPPTransaction, reason, errorCode, source == PurchaseErrorSource.FROM_PREVIOUS_PURCHASE);
        }
    }

    private void HandleZeroCostLicensePurchaseMethod(Network.PurchaseMethod method)
    {
        if (method.PurchaseError.Error != Network.PurchaseErrorInfo.ErrorType.STILL_IN_PROGRESS)
        {
            Debug.LogWarning(string.Format("StoreManager.HandleZeroCostLicensePurchaseMethod() FAILED error={0}", method.PurchaseError.Error));
            this.Status = TransactionStatus.READY;
        }
        else
        {
            Log.Rachelle.Print("StoreManager.HandleZeroCostLicensePurchaseMethod succeeded, refreshing achieves", new object[0]);
            AchieveManager.Get().UpdateActiveAchieves(null);
        }
    }

    public bool HasOutstandingPurchaseNotices(ProductType product)
    {
        foreach (NetCache.ProfileNoticePurchase purchase in this.m_outstandingPurchesNotices.ToArray())
        {
            Network.Bundle bundle = this.GetBundle(purchase.ProductID);
            if (bundle != null)
            {
                foreach (Network.BundleItem item in bundle.Items)
                {
                    if (item.Product == product)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool HaveProductsToSell()
    {
        return (((this.m_bundles.Count > 0) || (this.m_goldCostBooster.Count > 0)) || (this.m_goldCostArena > 0L));
    }

    public void Heartbeat()
    {
        if (this.m_initComplete)
        {
            long ticks = DateTime.Now.Ticks;
            this.RequestStatusIfNeeded(ticks);
            this.RequestConfigIfNeeded(ticks);
            this.AutoCancelPurchaseIfNeeded(ticks);
        }
    }

    private void HideAllPurchasePopups()
    {
        if (this.m_storePurchaseAuth != null)
        {
            this.m_storePurchaseAuth.Hide();
        }
        if (this.m_storeSummary != null)
        {
            this.m_storeSummary.Hide();
        }
        if (this.m_storeSendToBAM != null)
        {
            this.m_storeSendToBAM.Hide();
        }
        if (this.m_storeLegalBAMLinks != null)
        {
            this.m_storeLegalBAMLinks.Hide();
        }
        if (this.m_storeDoneWithBAM != null)
        {
            this.m_storeDoneWithBAM.Hide();
        }
        if (this.m_storeChallengePrompt != null)
        {
            this.m_storeChallengePrompt.Hide();
        }
    }

    public void HideArenaStore()
    {
        Store store = this.GetStore(StoreType.ARENA_STORE);
        if (store != null)
        {
            store.Hide();
            this.HideAllPurchasePopups();
        }
    }

    public void Init()
    {
        NetCache.Get().RegisterFeatures(new NetCache.NetCacheCallback(this.OnNetCacheFeaturesReady));
        if (!this.m_initComplete)
        {
            this.m_lastConfigRequestTime = 0L;
            this.m_lastStatusRequestTime = 0L;
            this.m_configRequestDelayTicks = MIN_CONFIG_REQUEST_DELAY_TICKS;
            this.m_statusRequestDelayTicks = MIN_STATUS_REQUEST_DELAY_TICKS;
            SceneMgr.Get().RegisterSceneUnloadedEvent(new SceneMgr.SceneUnloadedCallback(this.OnSceneUnloaded));
            BnetChallengeMgr.Get().AddChallengeListener(new BnetChallengeMgr.ChallengeCallback(this.OnChallengeReceived));
            Network network = Network.Get();
            network.RegisterNetHandler(BattlePayStatusResponse.PacketID.ID, new Network.NetHandler(this.OnBattlePayStatusResponse), null);
            network.RegisterNetHandler(BattlePayConfigResponse.PacketID.ID, new Network.NetHandler(this.OnBattlePayConfigResponse), null);
            network.RegisterNetHandler(PegasusUtil.PurchaseMethod.PacketID.ID, new Network.NetHandler(this.OnPurchaseMethod), null);
            network.RegisterNetHandler(PegasusUtil.PurchaseResponse.PacketID.ID, new Network.NetHandler(this.OnPurchaseResponse), null);
            network.RegisterNetHandler(CancelPurchaseResponse.PacketID.ID, new Network.NetHandler(this.OnPurchaseCanceledResponse), null);
            network.RegisterNetHandler(PurchaseWithGoldResponse.PacketID.ID, new Network.NetHandler(this.OnPurchaseViaGoldResponse), null);
            network.RegisterNetHandler(PegasusUtil.ThirdPartyPurchaseStatusResponse.PacketID.ID, new Network.NetHandler(this.OnThirdPartyPurchaseStatusResponse), null);
            NetCache.NetCacheProfileNotices netObject = NetCache.Get().GetNetObject<NetCache.NetCacheProfileNotices>();
            if (netObject != null)
            {
                this.OnNewNotices(netObject.Notices);
            }
            NetCache.Get().RegisterNewNoticesListener(new NetCache.DelNewNoticesListener(this.OnNewNotices));
            NetCache.Get().RegisterGoldBalanceListener(new NetCache.DelGoldBalanceListener(this.OnGoldBalanceChanged));
            this.m_initComplete = true;
            AssetLoader.Get().LoadGameObject((string) this.s_storePrefab, new AssetLoader.GameObjectCallback(this.OnGeneralStoreLoaded), null, false);
            ApplicationMgr.Get().WillReset += new System.Action(this.WillReset);
        }
    }

    public bool IsBattlePayFeatureEnabled()
    {
        NetCache.NetCacheFeatures netCacheFeatures = this.GetNetCacheFeatures();
        if (netCacheFeatures == null)
        {
            return false;
        }
        return (netCacheFeatures.Store.Store && netCacheFeatures.Store.BattlePay);
    }

    public bool IsBoosterPreorderActive(int boosterDbId, out Network.Bundle preOrderBundle)
    {
        foreach (Network.Bundle bundle in this.GetAllBundlesForProduct(ProductType.PRODUCT_TYPE_BOOSTER, boosterDbId, 0))
        {
            if (bundle.IsPreOrder())
            {
                preOrderBundle = bundle;
                return true;
            }
        }
        preOrderBundle = null;
        return false;
    }

    public bool IsBuyWithGoldFeatureEnabled()
    {
        NetCache.NetCacheFeatures netCacheFeatures = this.GetNetCacheFeatures();
        if (netCacheFeatures == null)
        {
            return false;
        }
        return (netCacheFeatures.Store.Store && netCacheFeatures.Store.BuyWithGold);
    }

    public bool IsChinaCustomer()
    {
        return (this.m_currency == Currency.CPT);
    }

    private bool IsConclusiveState(Network.PurchaseErrorInfo.ErrorType errorType)
    {
        Network.PurchaseErrorInfo.ErrorType type = errorType;
        switch (type)
        {
            case Network.PurchaseErrorInfo.ErrorType.WAIT_MOP:
            case Network.PurchaseErrorInfo.ErrorType.WAIT_CONFIRM:
            case Network.PurchaseErrorInfo.ErrorType.WAIT_RISK:
            case Network.PurchaseErrorInfo.ErrorType.WAIT_THIRD_PARTY_RECEIPT:
                break;

            default:
                switch ((type + 1))
                {
                    case Network.PurchaseErrorInfo.ErrorType.SUCCESS:
                    case Network.PurchaseErrorInfo.ErrorType.INVALID_BNET:
                        break;

                    case Network.PurchaseErrorInfo.ErrorType.STILL_IN_PROGRESS:
                        goto Label_0042;

                    default:
                        goto Label_0042;
                }
                break;
        }
        return false;
    Label_0042:
        return true;
    }

    private bool IsCurrentStoreLoaded()
    {
        Store currentStore = this.GetCurrentStore();
        if ((currentStore == null) || !currentStore.IsReady())
        {
            return false;
        }
        if (this.m_storePurchaseAuth == null)
        {
            return false;
        }
        if (this.m_storeSummary == null)
        {
            return false;
        }
        if (this.m_storeSendToBAM == null)
        {
            return false;
        }
        if (this.m_storeLegalBAMLinks == null)
        {
            return false;
        }
        if (this.m_storeDoneWithBAM == null)
        {
            return false;
        }
        if (this.m_storeChallengePrompt == null)
        {
            return false;
        }
        return true;
    }

    public bool IsEuropeanCustomer()
    {
        return ((this.m_currency == Currency.GBP) || ((this.m_currency == Currency.RUB) || (this.m_currency == Currency.EUR)));
    }

    public bool IsKoreanCustomer()
    {
        return (this.m_currency == Currency.KRW);
    }

    public bool IsNorthAmericanCustomer()
    {
        return ((this.m_currency == Currency.USD) || ((this.m_currency == Currency.ARS) || ((this.m_currency == Currency.CLP) || ((this.m_currency == Currency.MXN) || ((this.m_currency == Currency.BRL) || ((this.m_currency == Currency.AUD) || (this.m_currency == Currency.SGD)))))));
    }

    public bool IsOpen()
    {
        if (!this.NoticesReady)
        {
            return false;
        }
        if (!this.IsStoreFeatureEnabled())
        {
            return false;
        }
        if (!this.BattlePayAvailable)
        {
            return false;
        }
        if (!this.ConfigLoaded)
        {
            return false;
        }
        if (!this.HaveProductsToSell())
        {
            return false;
        }
        return (TransactionStatus.UNKNOWN != this.Status);
    }

    public bool IsProductAlreadyOwned(Network.Bundle bundle)
    {
        if (bundle != null)
        {
            foreach (Network.BundleItem item in bundle.Items)
            {
                if (this.AlreadyOwnsProduct(item.Product, item.ProductData))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsPromptShowing()
    {
        return ((((((this.m_storePurchaseAuth != null) && this.m_storePurchaseAuth.IsShown()) || ((this.m_storeSummary != null) && this.m_storeSummary.IsShown())) || (((this.m_storeSendToBAM != null) && this.m_storeSendToBAM.IsShown()) || ((this.m_storeLegalBAMLinks != null) && this.m_storeLegalBAMLinks.IsShown()))) || ((this.m_storeDoneWithBAM != null) && this.m_storeDoneWithBAM.IsShown())) || ((this.m_storeChallengePrompt != null) && this.m_storeChallengePrompt.IsShown()));
    }

    public bool IsShown()
    {
        Store currentStore = this.GetCurrentStore();
        return ((currentStore != null) && currentStore.IsShown());
    }

    public bool IsShownOrWaitingToShow()
    {
        return (this.IsWaitingToShow() || this.IsShown());
    }

    public bool IsStoreFeatureEnabled()
    {
        NetCache.NetCacheFeatures netCacheFeatures = this.GetNetCacheFeatures();
        if (netCacheFeatures == null)
        {
            return false;
        }
        return netCacheFeatures.Store.Store;
    }

    public bool IsWaitingToShow()
    {
        return this.m_waitingToShowStore;
    }

    private void Load(StoreType storeType)
    {
        if (this.GetCurrentStore() == null)
        {
            switch (storeType)
            {
                case StoreType.GENERAL_STORE:
                    AssetLoader.Get().LoadGameObject((string) this.s_storePrefab, new AssetLoader.GameObjectCallback(this.OnGeneralStoreLoaded), null, false);
                    break;

                case StoreType.ARENA_STORE:
                    AssetLoader.Get().LoadGameObject((string) this.s_arenaStorePrefab, new AssetLoader.GameObjectCallback(this.OnArenaStoreLoaded), null, false);
                    break;

                case StoreType.ADVENTURE_STORE:
                    AssetLoader.Get().LoadGameObject((string) this.s_adventureStorePrefab, new AssetLoader.GameObjectCallback(this.OnAdventureStoreLoaded), null, false);
                    break;
            }
            AssetLoader.Get().LoadGameObject((string) this.s_storePurchaseAuthPrefab, new AssetLoader.GameObjectCallback(this.OnStorePurchaseAuthLoaded), null, false);
            AssetLoader.Get().LoadGameObject((string) this.s_storeSummaryPrefab, new AssetLoader.GameObjectCallback(this.OnStoreSummaryLoaded), null, false);
            AssetLoader.Get().LoadGameObject((string) this.s_storeSendToBAMPrefab, new AssetLoader.GameObjectCallback(this.OnStoreSendToBAMLoaded), null, false);
            AssetLoader.Get().LoadGameObject((string) this.s_storeDoneWithBAMPrefab, new AssetLoader.GameObjectCallback(this.OnStoreDoneWithBAMLoaded), null, false);
            AssetLoader.Get().LoadGameObject((string) this.s_storeChallengePromptPrefab, new AssetLoader.GameObjectCallback(this.OnStoreChallengePromptLoaded), null, false);
            AssetLoader.Get().LoadGameObject((string) this.s_storeLegalBAMLinksPrefab, new AssetLoader.GameObjectCallback(this.OnStoreLegalBAMLinksLoaded), null, false);
        }
    }

    public void NotifyMobileGamePurchaseResponse(string thirdPartyID, bool isSuccess)
    {
        Log.Yim.Print("NotifyMobileGamePurchaseResponse(" + thirdPartyID + ", " + (!isSuccess ? "false" : "true") + ")", new object[0]);
        if (((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD)) && !string.IsNullOrEmpty(thirdPartyID))
        {
            StoreMobilePurchase.GamePurchaseStatusResponse(thirdPartyID, isSuccess);
        }
    }

    private void OnAchievesUpdated(object userData)
    {
        this.m_completedAchieves = AchieveManager.Get().GetNewCompletedAchieves();
        this.ShowCompletedAchieve();
        StoreAchievesData data = userData as StoreAchievesData;
        foreach (StoreAchievesListener listener in this.m_storeAchievesListeners.ToArray())
        {
            listener.Fire(data.Bundle, data.MethodOfPayment);
        }
    }

    private void OnAdventureStoreLoaded(string name, GameObject go, object callbackData)
    {
        AdventureStore store = this.OnStoreLoaded<AdventureStore>(go, StoreType.ADVENTURE_STORE);
        if (store != null)
        {
            this.SetupLoadedStore(store);
        }
    }

    private void OnArenaStoreLoaded(string name, GameObject go, object callbackData)
    {
        ArenaStore store = this.OnStoreLoaded<ArenaStore>(go, StoreType.ARENA_STORE);
        if (store != null)
        {
            this.SetupLoadedStore(store);
        }
    }

    private void OnAuthExit()
    {
        foreach (AuthorizationExitListener listener in this.m_authExitListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private void OnBattlePayConfigResponse()
    {
        Network.BattlePayConfig battlePayConfigResponse = Network.GetBattlePayConfigResponse();
        if (!battlePayConfigResponse.Available)
        {
            this.BattlePayAvailable = false;
        }
        else
        {
            this.m_configRequestDelayTicks = MIN_CONFIG_REQUEST_DELAY_TICKS;
            Log.Rachelle.Print(string.Format("StoreManager reset CONFIG delay, now waiting {0} seconds between requests", this.m_configRequestDelayTicks / 0x989680L), new object[0]);
            this.BattlePayAvailable = true;
            this.m_currency = battlePayConfigResponse.Currency;
            this.m_ticksBeforeAutoCancel = battlePayConfigResponse.SecondsBeforeAutoCancel * 0x989680L;
            this.m_ticksBeforeAutoCancelThirdParty = DEFAULT_SECONDS_BEFORE_AUTO_CANCEL_THIRD_PARTY * 0x989680L;
            this.m_bundles.Clear();
            if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
            {
                StoreMobilePurchase.ClearProductList();
            }
            foreach (Network.Bundle bundle in battlePayConfigResponse.Bundles)
            {
                if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
                {
                    string str = string.Empty;
                    if (!string.IsNullOrEmpty(str))
                    {
                        Log.Yim.Print("bundle.ProductId=" + bundle.ProductID + " thirdPartyID=" + str, new object[0]);
                        StoreMobilePurchase.AddProductById(str);
                        this.m_bundles.Add(bundle.ProductID, bundle);
                    }
                }
                else
                {
                    this.m_bundles.Add(bundle.ProductID, bundle);
                }
            }
            this.m_goldCostBooster.Clear();
            foreach (Network.GoldCostBooster booster in battlePayConfigResponse.GoldCostBoosters)
            {
                this.m_goldCostBooster.Add(booster.Id, booster);
            }
            this.m_goldCostArena = battlePayConfigResponse.GoldCostArena;
            if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
            {
                StoreMobilePurchase.ValidateAllProducts(MobileCallbackManager.Get().name);
            }
            if ((HAS_THIRD_PARTY_APP_STORE == null) || (ApplicationMgr.GetAndroidStore() == AndroidStore.BLIZZARD))
            {
                this.ConfigLoaded = true;
            }
        }
    }

    private void OnBattlePayStatusResponse()
    {
        Network.BattlePayStatus battlePayStatusResponse = Network.GetBattlePayStatusResponse();
        this.BattlePayAvailable = battlePayStatusResponse.BattlePayAvailable;
        bool flag = false;
        switch (battlePayStatusResponse.State)
        {
            case Network.BattlePayStatus.PurchaseState.READY:
                this.Status = TransactionStatus.READY;
                flag = true;
                break;

            case Network.BattlePayStatus.PurchaseState.CHECK_RESULTS:
            {
                bool isGTAPP = Currency.XSG == battlePayStatusResponse.CurrencyType;
                bool tryToResolvePreviousTransactionNotices = this.IsConclusiveState(battlePayStatusResponse.PurchaseError.Error);
                this.SetActiveMoneyOrGTAPPTransaction(battlePayStatusResponse.TransactionID, battlePayStatusResponse.ProductID, battlePayStatusResponse.Provider, isGTAPP, tryToResolvePreviousTransactionNotices);
                this.HandlePurchaseError(PurchaseErrorSource.FROM_STATUS_OR_PURCHASE_RESPONSE, battlePayStatusResponse.PurchaseError.Error, battlePayStatusResponse.PurchaseError.ErrorCode, battlePayStatusResponse.ThirdPartyID, isGTAPP);
                flag = battlePayStatusResponse.PurchaseError.Error != Network.PurchaseErrorInfo.ErrorType.STILL_IN_PROGRESS;
                break;
            }
            case Network.BattlePayStatus.PurchaseState.ERROR:
                Debug.LogWarning("StoreManager.OnBattlePayStatusResponse(): Error getting status. Check with Rachelle.");
                flag = false;
                break;

            default:
                Debug.LogError(string.Format("StoreManager.OnBattlePayStatusResponse(): unknown state {0}", battlePayStatusResponse.State));
                flag = false;
                break;
        }
        if (this.BattlePayAvailable && flag)
        {
            this.m_statusRequestDelayTicks = MIN_STATUS_REQUEST_DELAY_TICKS;
            Log.Rachelle.Print(string.Format("StoreManager reset STATUS delay, now waiting {0} seconds between requests", this.m_statusRequestDelayTicks / 0x989680L), new object[0]);
        }
    }

    private void OnChallengeCancel(ulong challengeID)
    {
        this.Status = TransactionStatus.CHALLENGE_CANCELED;
        Network.Get().CancelChallenge(challengeID);
        this.m_lastStatusRequestTime = DateTime.Now.Ticks;
        this.m_statusRequestDelayTicks = CHALLENGE_CANCEL_STATUS_REQUEST_DELAY_TICKS;
    }

    private void OnChallengeReceived(BnetChallengeMgr.ChallengeInfo challengeInfo, object userData)
    {
        this.m_statusRequestDelayTicks = MIN_STATUS_REQUEST_DELAY_TICKS;
        Log.Rachelle.Print(string.Format("StoreManager updated STATUS delay due to receiving a challenge, now waiting {0} seconds between requests", this.m_statusRequestDelayTicks / 0x989680L), new object[0]);
        this.ActivateStoreCover();
        if (this.m_storeChallengePrompt.Show(challengeInfo.Type, challengeInfo.ID, challengeInfo.IsRetry))
        {
            this.m_storePurchaseAuth.Hide();
        }
        else
        {
            Debug.LogWarning(string.Format("StoreManager.OnChallengeReceived(): received challenge {0} while we have an active challenge; auto-canceling", challengeInfo.ID));
            Network.Get().CancelChallenge(challengeInfo.ID);
        }
    }

    private void OnChallengeSubmit(ulong challengeID, string answer)
    {
        this.m_storePurchaseAuth.Show(this.m_activeMoneyOrGTAPPTransaction, this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType), false);
        this.m_lastStatusRequestTime = DateTime.Now.Ticks;
        this.m_statusRequestDelayTicks = EARLY_STATUS_REQUEST_DELAY_TICKS;
        Log.Rachelle.Print(string.Format("StoreManager updating STATUS delay due to challenge answer submit, now waiting {0} seconds between requests", this.m_statusRequestDelayTicks / 0x989680L), new object[0]);
        this.Status = TransactionStatus.CHALLENGE_SUBMITTED;
        Network.Get().AnswerChallenge(challengeID, answer);
    }

    private void OnDoneWithBAM()
    {
        this.DeactivateStoreCover();
    }

    private void OnGeneralStoreLoaded(string name, GameObject go, object callbackData)
    {
        GeneralStore store = this.OnStoreLoaded<GeneralStore>(go, StoreType.GENERAL_STORE);
        if (store != null)
        {
            this.SetupLoadedStore(store);
        }
    }

    private void OnGoldBalanceChanged(NetCache.NetCacheGoldBalance balance)
    {
        Store currentStore = this.GetCurrentStore();
        if ((currentStore != null) && currentStore.IsShown())
        {
            currentStore.OnGoldBalanceChanged(balance);
        }
    }

    private void OnLicenseAddedAchievesUpdated(List<Achievement> activeLicenseAddedAchieves, object userData)
    {
        if ((this.Status == TransactionStatus.WAIT_ZERO_COST_LICENSE) && (activeLicenseAddedAchieves.Count <= 0))
        {
            Log.Rachelle.Print("StoreManager.OnLicenseAddedAchievesUpdated(): done waiting for licenses!", new object[0]);
            if (this.IsCurrentStoreLoaded())
            {
                this.m_storePurchaseAuth.CompletePurchaseSuccess(null);
            }
            this.Status = TransactionStatus.READY;
        }
    }

    private void OnNetCacheFeaturesReady()
    {
        this.FeaturesReady = true;
    }

    private void OnNewNotices(List<NetCache.ProfileNotice> newNotices)
    {
        List<long> list = new List<long>();
        foreach (NetCache.ProfileNotice notice in newNotices)
        {
            if (notice.Type == NetCache.ProfileNotice.NoticeType.PURCHASE)
            {
                if (notice.Origin == NetCache.ProfileNotice.NoticeOrigin.PURCHASE_CANCELED)
                {
                    Log.Rachelle.Print(string.Format("StoreManager.OnNewNotices() ack'ing purchase canceled notice for bpay ID {0}", notice.OriginData), new object[0]);
                    list.Add(notice.NoticeID);
                }
                else if (this.m_confirmedTransactionIDs.Contains(notice.OriginData))
                {
                    Log.Rachelle.Print(string.Format("StoreManager.OnNewNotices() ack'ing purchase notice for already confirmed bpay ID {0}", notice.OriginData), new object[0]);
                    list.Add(notice.NoticeID);
                }
                else
                {
                    NetCache.ProfileNoticePurchase item = notice as NetCache.ProfileNoticePurchase;
                    Log.Rachelle.Print(string.Format("StoreManager.OnNewNotices() adding outstanding purchase notice for bpay ID {0}", notice.OriginData), new object[0]);
                    this.m_outstandingPurchesNotices.Add(item);
                }
            }
        }
        foreach (long num in list)
        {
            Network.AckNotice(num);
        }
        if (!this.NoticesReady)
        {
            this.NoticesReady = true;
            if (this.Status == TransactionStatus.READY)
            {
                this.ResolveFirstMoneyOrGTAPPTransactionIfPossible();
            }
        }
    }

    private void OnPurchaseCanceledResponse()
    {
        Network.PurchaseCanceledResponse purchaseCanceledResponse = Network.GetPurchaseCanceledResponse();
        switch (purchaseCanceledResponse.Result)
        {
            case Network.PurchaseCanceledResponse.CancelResult.SUCCESS:
                Log.Rachelle.Print("StoreManager.OnPurchaseCanceledResponse(): purchase successfully canceled.", new object[0]);
                NetCache.Get().ReloadNetObject<NetCache.NetCacheProfileNotices>();
                this.m_lastStatusRequestTime = DateTime.Now.Ticks;
                this.ConfirmActiveMoneyTransaction(purchaseCanceledResponse.TransactionID);
                this.Status = TransactionStatus.READY;
                this.m_shouldAutoCancelThirdPartyTransaction = false;
                this.m_requestedThirdPartyProductId = null;
                this.m_previousStatusBeforeAutoCancel = TransactionStatus.UNKNOWN;
                this.m_outOfSessionThirdPartyTransaction = false;
                break;

            case Network.PurchaseCanceledResponse.CancelResult.NOT_ALLOWED:
            {
                Debug.LogWarning("StoreManager.OnPurchaseCanceledResponse(): cancel purchase is not allowed right now.");
                bool isGTAPP = Currency.XSG == purchaseCanceledResponse.CurrencyType;
                this.SetActiveMoneyOrGTAPPTransaction(purchaseCanceledResponse.TransactionID, purchaseCanceledResponse.ProductID, MoneyOrGTAPPTransaction.UNKNOWN_PROVIDER, isGTAPP, true);
                this.Status = !isGTAPP ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP;
                if (this.m_previousStatusBeforeAutoCancel != TransactionStatus.UNKNOWN)
                {
                    this.Status = this.m_previousStatusBeforeAutoCancel;
                    this.m_previousStatusBeforeAutoCancel = TransactionStatus.UNKNOWN;
                }
                break;
            }
            case Network.PurchaseCanceledResponse.CancelResult.NOTHING_TO_CANCEL:
                if (this.m_activeMoneyOrGTAPPTransaction != null)
                {
                    TransactionStatus previousStatusBeforeAutoCancel = !this.m_activeMoneyOrGTAPPTransaction.IsGTAPP ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP;
                    if (this.m_previousStatusBeforeAutoCancel != TransactionStatus.UNKNOWN)
                    {
                        previousStatusBeforeAutoCancel = this.m_previousStatusBeforeAutoCancel;
                        this.m_previousStatusBeforeAutoCancel = TransactionStatus.UNKNOWN;
                    }
                    Debug.LogWarning(string.Format("StoreManager.OnPurchaseCanceledResponse(): nothing to cancel, setting status to {0}", previousStatusBeforeAutoCancel));
                    this.Status = previousStatusBeforeAutoCancel;
                    break;
                }
                Debug.LogWarning("StoreManager.OnPurchaseCanceledResponse(): nothing to cancel and m_activeMoneyOrGTAPPTransaction is null.. no choice but to set Status to UNKNOWN");
                this.Status = TransactionStatus.UNKNOWN;
                break;
        }
    }

    private void OnPurchaseMethod()
    {
        Network.PurchaseMethod purchaseMethodResponse = Network.GetPurchaseMethodResponse();
        if (purchaseMethodResponse.IsZeroCostLicense)
        {
            this.HandleZeroCostLicensePurchaseMethod(purchaseMethodResponse);
        }
        else
        {
            bool isGTAPP = Currency.XSG == purchaseMethodResponse.Currency;
            this.SetActiveMoneyOrGTAPPTransaction(purchaseMethodResponse.TransactionID, purchaseMethodResponse.ProductID, 1, isGTAPP, false);
            bool flag2 = false;
            if (purchaseMethodResponse.PurchaseError != null)
            {
                if (purchaseMethodResponse.PurchaseError.Error != Network.PurchaseErrorInfo.ErrorType.BP_NO_VALID_PAYMENT)
                {
                    this.HandlePurchaseError(PurchaseErrorSource.FROM_PURCHASE_METHOD_RESPONSE, purchaseMethodResponse.PurchaseError.Error, purchaseMethodResponse.PurchaseError.ErrorCode, string.Empty, isGTAPP);
                    return;
                }
                flag2 = true;
            }
            this.ActivateStoreCover();
            if (isGTAPP)
            {
                this.OnSummaryConfirm(purchaseMethodResponse.ProductID, purchaseMethodResponse.Quantity, null);
            }
            else
            {
                string walletName;
                if (flag2)
                {
                    walletName = null;
                }
                else if (purchaseMethodResponse.UseEBalance)
                {
                    walletName = GameStrings.Get("GLUE_STORE_BNET_BALANCE");
                }
                else
                {
                    walletName = purchaseMethodResponse.WalletName;
                }
                Store currentStore = this.GetCurrentStore();
                if ((currentStore == null) || !currentStore.IsShown())
                {
                    this.AutoCancelPurchaseIfPossible();
                }
                else
                {
                    if (this.m_storePurchaseAuth != null)
                    {
                        this.m_storePurchaseAuth.Hide();
                    }
                    this.Status = TransactionStatus.WAIT_CONFIRM;
                    this.m_storeSummary.Show(purchaseMethodResponse.ProductID, purchaseMethodResponse.Quantity, walletName);
                }
            }
        }
    }

    private void OnPurchaseResponse()
    {
        Network.PurchaseResponse purchaseResponse = Network.GetPurchaseResponse();
        bool isGTAPP = Currency.XSG == purchaseResponse.CurrencyType;
        this.SetActiveMoneyOrGTAPPTransaction(purchaseResponse.TransactionID, purchaseResponse.ProductID, MoneyOrGTAPPTransaction.UNKNOWN_PROVIDER, isGTAPP, false);
        this.HandlePurchaseError(PurchaseErrorSource.FROM_STATUS_OR_PURCHASE_RESPONSE, purchaseResponse.PurchaseError.Error, purchaseResponse.PurchaseError.ErrorCode, purchaseResponse.ThirdPartyID, isGTAPP);
    }

    private void OnPurchaseResultAcknowledged(bool success, MoneyOrGTAPPTransaction moneyOrGTAPPTransaction)
    {
        PaymentMethod method;
        Network.Bundle bundle;
        if (moneyOrGTAPPTransaction == null)
        {
            method = PaymentMethod.GOLD_NO_GTAPP;
            bundle = null;
        }
        else
        {
            if (moneyOrGTAPPTransaction.ID > 0L)
            {
                this.m_transactionIDsConclusivelyHandled.Add(moneyOrGTAPPTransaction.ID);
            }
            method = !moneyOrGTAPPTransaction.IsGTAPP ? PaymentMethod.MONEY : PaymentMethod.GOLD_GTAPP;
            bundle = this.GetBundle(moneyOrGTAPPTransaction.ProductID);
        }
        if (method != PaymentMethod.GOLD_NO_GTAPP)
        {
            this.ConfirmActiveMoneyTransaction(moneyOrGTAPPTransaction.ID);
        }
        if (success)
        {
            foreach (SuccessfulPurchaseAckListener listener in this.m_successfulPurchaseAckListeners.ToArray())
            {
                listener.Fire(bundle, method);
            }
        }
        this.DeactivateStoreCover();
        Store currentStore = this.GetCurrentStore();
        if (this.m_currentStoreType == StoreType.ADVENTURE_STORE)
        {
            currentStore.Close();
        }
        if (!this.BattlePayAvailable && (this.m_currentStoreType == StoreType.GENERAL_STORE))
        {
            currentStore.Close();
        }
    }

    private void OnPurchaseViaGoldResponse()
    {
        Network.PurchaseViaGoldResponse purchaseWithGoldResponse = Network.GetPurchaseWithGoldResponse();
        string failDetails = string.Empty;
        switch (purchaseWithGoldResponse.Error)
        {
            case Network.PurchaseViaGoldResponse.ErrorType.SUCCESS:
                NetCache.Get().RefreshNetObject<NetCache.NetCacheGoldBalance>();
                this.HandlePurchaseSuccess(null, null, string.Empty);
                return;

            case Network.PurchaseViaGoldResponse.ErrorType.INSUFFICIENT_GOLD:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_NOT_ENOUGH_GOLD");
                break;

            case Network.PurchaseViaGoldResponse.ErrorType.PRODUCT_NA:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_PRODUCT_NA");
                break;

            case Network.PurchaseViaGoldResponse.ErrorType.FEATURE_NA:
                failDetails = GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC");
                break;

            case Network.PurchaseViaGoldResponse.ErrorType.INVALID_QUANTITY:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_QUANTITY");
                break;

            default:
                failDetails = GameStrings.Get("GLUE_STORE_FAIL_GENERAL");
                break;
        }
        this.Status = TransactionStatus.READY;
        this.m_storePurchaseAuth.CompletePurchaseFailure(null, failDetails);
    }

    private void OnSceneUnloaded(SceneMgr.Mode prevMode, Scene prevScene, object userData)
    {
        this.UnloadAndFreeMemory();
    }

    private void OnSendToBAMCancel(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction)
    {
        if (moneyOrGTAPPTransaction != null)
        {
            this.ConfirmActiveMoneyTransaction(moneyOrGTAPPTransaction.ID);
        }
        this.DeactivateStoreCover();
    }

    private void OnSendToBAMLegal(StoreLegalBAMLinks.BAMReason reason)
    {
        this.DeactivateStoreCover();
    }

    private void OnSendToBAMLegalCancel()
    {
        this.DeactivateStoreCover();
    }

    private void OnSendToBAMOkay(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, StoreSendToBAM.BAMReason reason)
    {
        if (moneyOrGTAPPTransaction != null)
        {
            this.ConfirmActiveMoneyTransaction(moneyOrGTAPPTransaction.ID);
        }
        if (reason == StoreSendToBAM.BAMReason.PAYMENT_INFO)
        {
            this.OnDoneWithBAM();
        }
        else
        {
            this.m_storeDoneWithBAM.Show();
        }
    }

    private void OnStoreBuyWithGoldNoGTAPP(NoGTAPPTransactionData noGTAPPtransactionData, object userData)
    {
        if ((noGTAPPtransactionData != null) && this.CanBuyProduct(noGTAPPtransactionData.Product, noGTAPPtransactionData.ProductData))
        {
            this.ActivateStoreCover();
            this.m_storePurchaseAuth.Show(null, this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType), false);
            this.Status = TransactionStatus.IN_PROGRESS_GOLD_NO_GTAPP;
            Network.PurchaseViaGold(noGTAPPtransactionData.Quantity, noGTAPPtransactionData.Product, noGTAPPtransactionData.ProductData);
        }
    }

    private void OnStoreBuyWithGTAPP(string productID, int quantity, object userData)
    {
        if (this.CanBuyBundle(this.GetBundle(productID)))
        {
            this.ActivateStoreCover();
            this.SetActiveMoneyOrGTAPPTransaction((long) UNKNOWN_TRANSACTION_ID, productID, 1, true, false);
            this.Status = TransactionStatus.WAIT_METHOD_OF_PAYMENT;
            Network.GetPurchaseMethod(productID, quantity, Currency.XSG);
        }
    }

    private void OnStoreBuyWithMoney(string productID, int quantity, object userData)
    {
        Network.Bundle bundleToBuy = this.GetBundle(productID);
        if (this.CanBuyBundle(bundleToBuy))
        {
            this.ActivateStoreCover();
            if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
            {
                if (bundleToBuy.IsFree())
                {
                    Debug.LogWarning("Attempting to purchase a free bundle!  This should not be possible, and you would be charged by the Third Party Provider price if we allowed it.");
                    this.DeactivateStoreCover();
                }
                else
                {
                    BattlePayProvider provider = BattlePayProvider.BP_PROVIDER_BLIZZARD;
                    this.SetActiveMoneyOrGTAPPTransaction((long) UNKNOWN_TRANSACTION_ID, productID, new BattlePayProvider?(provider), false, false);
                    this.m_storePurchaseAuth.Show(this.m_activeMoneyOrGTAPPTransaction, this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType), false);
                    this.Status = TransactionStatus.WAIT_THIRD_PARTY_INIT;
                    Network.BeginThirdPartyPurchase(provider, productID, quantity);
                    Log.Yim.Print(string.Concat(new object[] { "Network.BeginThirdPartyPurchase(", provider, ", ", productID, ", ", quantity, ")" }), new object[0]);
                }
            }
            else
            {
                this.SetActiveMoneyOrGTAPPTransaction((long) UNKNOWN_TRANSACTION_ID, productID, 1, false, false);
                this.Status = TransactionStatus.WAIT_METHOD_OF_PAYMENT;
                Network.GetPurchaseMethod(productID, quantity, this.m_currency);
            }
        }
    }

    private void OnStoreChallengePromptLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            Debug.LogError("StoreManager.OnStoreChallengePromptLoaded(): go is null!");
        }
        else
        {
            this.m_storeChallengePrompt = go.GetComponent<StoreChallengePrompt>();
            if (this.m_storeChallengePrompt == null)
            {
                Debug.LogError("StoreManager.OnStoreChallengePromptLoaded(): go has no StoreChallengePrompt component");
            }
            else
            {
                this.m_storeChallengePrompt.Hide();
                this.m_storeChallengePrompt.RegisterSubmitListener(new StoreChallengePrompt.SubmitListener(this.OnChallengeSubmit));
                this.m_storeChallengePrompt.RegisterCancelListener(new StoreChallengePrompt.CancelListener(this.OnChallengeCancel));
                this.OnStoreComponentReady(null);
            }
        }
    }

    private void OnStoreComponentReady(object userData)
    {
        if (this.m_waitingToShowStore && this.IsCurrentStoreLoaded())
        {
            this.ShowStore();
        }
    }

    private void OnStoreDoneWithBAMLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            Debug.LogError("StoreManager.OnStoreDoneWithBAMLoaded(): go is null!");
        }
        else
        {
            this.m_storeDoneWithBAM = go.GetComponent<StoreDoneWithBAM>();
            if (this.m_storeDoneWithBAM == null)
            {
                Debug.LogError("StoreManager.OnStoreDoneWithBAMLoaded(): go has no StoreDoneWithBAM component");
            }
            else
            {
                this.m_storeDoneWithBAM.Hide();
                this.m_storeDoneWithBAM.RegisterOkayListener(new StoreDoneWithBAM.ButtonPressedListener(this.OnDoneWithBAM));
                this.OnStoreComponentReady(null);
            }
        }
    }

    private void OnStoreExit(bool authorizationBackButtonPressed, object userData)
    {
        if (this.m_activeMoneyOrGTAPPTransaction != null)
        {
            this.m_activeMoneyOrGTAPPTransaction.ClosedStore = true;
        }
        ulong challengeID = this.m_storeChallengePrompt.HideChallenge();
        if (challengeID != 0)
        {
            this.OnChallengeCancel(challengeID);
        }
        else
        {
            this.AutoCancelPurchaseIfPossible();
        }
        this.DeactivateStoreCover();
        this.HideAllPurchasePopups();
        foreach (StoreHiddenListener listener in this.m_storeHiddenListeners.ToArray())
        {
            listener.Fire();
        }
    }

    private void OnStoreInfo(object userData)
    {
        this.ActivateStoreCover();
        this.m_storeSendToBAM.Show(null, StoreSendToBAM.BAMReason.PAYMENT_INFO, string.Empty, false);
    }

    private void OnStoreLegalBAMLinksLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            Debug.LogError("StoreManager.OnStoreLegalBAMLinksLoaded(): go is null!");
        }
        else
        {
            this.m_storeLegalBAMLinks = go.GetComponent<StoreLegalBAMLinks>();
            if (this.m_storeLegalBAMLinks == null)
            {
                Debug.LogError("StoreManager.OnStoreLegalBAMLinksLoaded(): go has no StoreLegalBAMLinks component");
            }
            else
            {
                this.m_storeLegalBAMLinks.Hide();
                this.m_storeLegalBAMLinks.RegisterSendToBAMListener(new StoreLegalBAMLinks.SendToBAMListener(this.OnSendToBAMLegal));
                this.m_storeLegalBAMLinks.RegisterCancelListener(new StoreLegalBAMLinks.CancelListener(this.OnSendToBAMLegalCancel));
                this.OnStoreComponentReady(null);
            }
        }
    }

    private T OnStoreLoaded<T>(GameObject go, StoreType storeType) where T: Store
    {
        if (go == null)
        {
            Debug.LogError(string.Format("StoreManager.OnStoreLoaded<{0}>(): go is null!", typeof(T)));
            return null;
        }
        T component = go.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError(string.Format("StoreManager.OnStoreLoaded<{0}>(): go has no {1} component!", typeof(T), typeof(T)));
            return null;
        }
        this.m_stores[storeType] = component;
        return component;
    }

    private void OnStorePurchaseAuthLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            Debug.LogError("StoreManager.OnStorePurchaseAuthLoaded(): go is null!");
        }
        else
        {
            this.m_storePurchaseAuth = go.GetComponent<StorePurchaseAuth>();
            if (this.m_storePurchaseAuth == null)
            {
                Debug.LogError("StoreManager.OnStorePurchaseAuthLoaded(): go has no StorePurchaseAuth component");
            }
            else
            {
                this.m_storePurchaseAuth.Hide();
                this.m_storePurchaseAuth.RegisterAckPurchaseResultListener(new StorePurchaseAuth.AckPurchaseResultListener(this.OnPurchaseResultAcknowledged));
                this.m_storePurchaseAuth.RegisterExitListener(new StorePurchaseAuth.ExitListener(this.OnAuthExit));
                this.OnStoreComponentReady(null);
            }
        }
    }

    private void OnStoreSendToBAMLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            Debug.LogError("StoreManager.OnStoreSendToBAMLoaded(): go is null!");
        }
        else
        {
            this.m_storeSendToBAM = go.GetComponent<StoreSendToBAM>();
            if (this.m_storeSendToBAM == null)
            {
                Debug.LogError("StoreManager.OnStoreSendToBAMLoaded(): go has no StoreSendToBAM component");
            }
            else
            {
                this.m_storeSendToBAM.Hide();
                this.m_storeSendToBAM.RegisterOkayListener(new StoreSendToBAM.DelOKListener(this.OnSendToBAMOkay));
                this.m_storeSendToBAM.RegisterCancelListener(new StoreSendToBAM.DelCancelListener(this.OnSendToBAMCancel));
                this.OnStoreComponentReady(null);
            }
        }
    }

    private void OnStoreShown()
    {
        foreach (StoreShownListener listener in this.m_storeShownListeners.ToArray())
        {
            listener.Fire();
        }
        if (this.TransactionInProgress())
        {
            this.ActivateStoreCover();
            bool enableBackButton = this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType);
            if ((this.Status == TransactionStatus.WAIT_ZERO_COST_LICENSE) && (this.m_currentStoreType == StoreType.ADVENTURE_STORE))
            {
                enableBackButton = true;
            }
            BattlePayProvider provider = BattlePayProvider.BP_PROVIDER_BLIZZARD;
            if (((this.Status == TransactionStatus.WAIT_THIRD_PARTY_RECEIPT) && this.m_outOfSessionThirdPartyTransaction) && (provider != ((BattlePayProvider) this.m_activeMoneyOrGTAPPTransaction.Provider)))
            {
                this.m_storePurchaseAuth.ShowPurchaseLocked(this.m_activeMoneyOrGTAPPTransaction, enableBackButton, TransactionStatus.WAIT_ZERO_COST_LICENSE == this.Status, delegate (bool showHelp) {
                    if (showHelp)
                    {
                        Application.OpenURL(NydusLink.GetSupportLink("outstanding-purchase", false));
                    }
                    this.GetCurrentStore().Close();
                });
            }
            else
            {
                this.m_storePurchaseAuth.Show(this.m_activeMoneyOrGTAPPTransaction, enableBackButton, TransactionStatus.WAIT_ZERO_COST_LICENSE == this.Status);
            }
        }
        else if (this.m_outstandingPurchesNotices.Count != 0)
        {
            NetCache.ProfileNoticePurchase purchase = this.m_outstandingPurchesNotices[0];
            bool isGTAPP = Currency.XSG == purchase.CurrencyType;
            Log.Rachelle.Print(string.Format("StoreManager.OnStoreShown() m_outstandingPurchesNoticesCount={0}. First one: {1}", this.m_outstandingPurchesNotices.Count, purchase), new object[0]);
            this.SetActiveMoneyOrGTAPPTransaction(purchase.OriginData, purchase.ProductID, MoneyOrGTAPPTransaction.UNKNOWN_PROVIDER, isGTAPP, true);
            Network.PurchaseErrorInfo.ErrorType purchaseErrorType = (purchase.Origin != NetCache.ProfileNotice.NoticeOrigin.PURCHASE_COMPLETE) ? ((Network.PurchaseErrorInfo.ErrorType) ((int) purchase.Data)) : Network.PurchaseErrorInfo.ErrorType.SUCCESS;
            this.ActivateStoreCover();
            this.HandlePurchaseError(PurchaseErrorSource.FROM_PREVIOUS_PURCHASE, purchaseErrorType, string.Empty, string.Empty, isGTAPP);
        }
    }

    private void OnStoreSummaryLoaded(string name, GameObject go, object callbackData)
    {
        if (go == null)
        {
            Debug.LogError("StoreManager.OnStoreSummaryLoaded(): go is null!");
        }
        else
        {
            this.m_storeSummary = go.GetComponent<StoreSummary>();
            if (this.m_storeSummary == null)
            {
                Debug.LogError("StoreManager.OnStoreSummaryLoaded(): go has no StoreSummary component");
            }
            else
            {
                this.m_storeSummary.Hide();
                this.m_storeSummary.RegisterConfirmListener(new StoreSummary.ConfirmCallback(this.OnSummaryConfirm));
                this.m_storeSummary.RegisterCancelListener(new StoreSummary.CancelCallback(this.OnSummaryCancel));
                this.m_storeSummary.RegisterInfoListener(new StoreSummary.InfoCallback(this.OnSummaryInfo));
                this.m_storeSummary.RegisterPaymentAndTOSListener(new StoreSummary.PaymentAndTOSCallback(this.OnSummaryPaymentAndTOS));
                this.OnStoreComponentReady(null);
            }
        }
    }

    private void OnSummaryCancel(object userData)
    {
        this.CancelBlizzardPurchase();
        this.DeactivateStoreCover();
    }

    private void OnSummaryConfirm(string productID, int quantity, object userData)
    {
        this.m_storePurchaseAuth.Show(this.m_activeMoneyOrGTAPPTransaction, this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType), false);
        this.m_lastStatusRequestTime = DateTime.Now.Ticks;
        this.m_statusRequestDelayTicks = EARLY_STATUS_REQUEST_DELAY_TICKS;
        Log.Rachelle.Print(string.Format("StoreManager updating STATUS delay due to summary confirm, now waiting {0} seconds between requests", this.m_statusRequestDelayTicks / 0x989680L), new object[0]);
        this.Status = !this.m_activeMoneyOrGTAPPTransaction.IsGTAPP ? TransactionStatus.IN_PROGRESS_MONEY : TransactionStatus.IN_PROGRESS_GOLD_GTAPP;
        Network.ConfirmPurchase();
    }

    private void OnSummaryInfo(object userData)
    {
        this.ActivateStoreCover();
        this.AutoCancelPurchaseIfPossible();
        this.m_storeSendToBAM.Show(null, StoreSendToBAM.BAMReason.EULA_AND_TOS, string.Empty, false);
    }

    private void OnSummaryPaymentAndTOS(object userData)
    {
        this.AutoCancelPurchaseIfPossible();
        this.m_storeLegalBAMLinks.Show();
    }

    private void OnThirdPartyPurchaseApproved()
    {
        Log.Yim.Print("OnThirdPartyPurchaseApproved", new object[0]);
        TransactionStatus status = this.Status;
        this.Status = TransactionStatus.WAIT_THIRD_PARTY_RECEIPT;
        if ((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD))
        {
            if (this.m_activeMoneyOrGTAPPTransaction == null)
            {
                Debug.LogWarning("StoreManager.OnThirdPartyPurchaseApproved() but m_activeMoneyOrGTAPPTransaction is null");
            }
            else if (UNKNOWN_TRANSACTION_ID == this.m_activeMoneyOrGTAPPTransaction.ID)
            {
                Debug.LogWarning("StoreManager.OnThirdPartyPurchaseApproved() but m_activeMoneyOrGTAPPTransaction ID is UNKNOWN_TRANSACTION_ID");
            }
            else if ((this.m_activeMoneyOrGTAPPTransaction.Provider.HasValue && (((BattlePayProvider) this.m_activeMoneyOrGTAPPTransaction.Provider.Value) != BattlePayProvider.BP_PROVIDER_APPLE)) && ((((BattlePayProvider) this.m_activeMoneyOrGTAPPTransaction.Provider.Value) != BattlePayProvider.BP_PROVIDER_GOOGLE_PLAY) && (((BattlePayProvider) this.m_activeMoneyOrGTAPPTransaction.Provider.Value) != BattlePayProvider.BP_PROVIDER_AMAZON)))
            {
                Debug.LogWarning(string.Format("StoreManager.OnThirdPartyPurchaseApproved() active transaction is not an Third Party Provider transaction, so Third Party Provider shouldn't be servicing it (m_activeMoneyOrGTAPPTransaction = {0})", this.m_activeMoneyOrGTAPPTransaction));
            }
            else if (this.GetBundle(this.m_activeMoneyOrGTAPPTransaction.ProductID) == null)
            {
                Debug.LogWarning(string.Format("StoreManager.OnThirdPartyPurchaseApproved() but bundle is null (m_activeMoneyOrGTAPPTransaction = {0})", this.m_activeMoneyOrGTAPPTransaction));
            }
            else
            {
                if (this.IsCurrentStoreLoaded())
                {
                    this.ActivateStoreCover();
                }
                BattlePayProvider provider = BattlePayProvider.BP_PROVIDER_BLIZZARD;
                this.SetActiveMoneyOrGTAPPTransaction(this.m_activeMoneyOrGTAPPTransaction.ID, this.m_activeMoneyOrGTAPPTransaction.ProductID, new BattlePayProvider?(provider), false, false);
                if (this.IsCurrentStoreLoaded())
                {
                    this.m_storePurchaseAuth.Show(this.m_activeMoneyOrGTAPPTransaction, this.ShouldEnablePurchaseAuthBackButton(this.m_currentStoreType), false);
                }
                if (status == TransactionStatus.WAIT_THIRD_PARTY_INIT)
                {
                    string message = this.m_activeMoneyOrGTAPPTransaction.ID.ToString();
                    BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_INFO, BIReport.TelemetryEvent.EVENT_THIRD_PARTY_PURCHASE_REQUEST, 0, message);
                }
                else
                {
                    Debug.LogWarning(string.Format("StoreManager.OnThirdPartyPurchaseApproved() previous Status was {0}, expected {1} (m_activeMoneyOrGTAPPTransaction = {2}", status, TransactionStatus.WAIT_THIRD_PARTY_INIT, this.m_activeMoneyOrGTAPPTransaction));
                    Log.Yim.Print("Previous ongoing purchase detected, expecting third party receipt..", new object[0]);
                    Log.Yim.Print("m_activeMoneyOrGTAPPTransaction = " + this.m_activeMoneyOrGTAPPTransaction, new object[0]);
                    this.m_outOfSessionThirdPartyTransaction = true;
                    if (BattlePayProvider.BP_PROVIDER_BLIZZARD == ((BattlePayProvider) this.m_activeMoneyOrGTAPPTransaction.Provider.Value))
                    {
                        this.m_requestedThirdPartyProductId = string.Empty;
                        string str2 = this.m_activeMoneyOrGTAPPTransaction.ID + "|" + this.m_requestedThirdPartyProductId;
                        BIReport.Get().Report_Telemetry(Telemetry.Level.LEVEL_INFO, BIReport.TelemetryEvent.EVENT_THIRD_PARTY_PURCHASE_RECEIPT_REQUEST, 0, str2);
                        StoreMobilePurchase.WaitingOnThirdPartyReceipt(this.m_requestedThirdPartyProductId);
                    }
                }
            }
        }
        else
        {
            this.m_outOfSessionThirdPartyTransaction = true;
        }
    }

    private void OnThirdPartyPurchaseStatusResponse()
    {
        Network.ThirdPartyPurchaseStatusResponse thirdPartyPurchaseStatusResponse = Network.GetThirdPartyPurchaseStatusResponse();
        Debug.Log(string.Format("StoreManager.OnThirdPartyPurchaseStatusResponse(): ThirdPartyID='{0}', Status={1} -- receipt can be removed from client now", thirdPartyPurchaseStatusResponse.ThirdPartyID, thirdPartyPurchaseStatusResponse.Status));
        switch (thirdPartyPurchaseStatusResponse.Status)
        {
            case Network.ThirdPartyPurchaseStatusResponse.PurchaseStatus.NOT_FOUND:
                break;

            case Network.ThirdPartyPurchaseStatusResponse.PurchaseStatus.SUCCEEDED:
            case Network.ThirdPartyPurchaseStatusResponse.PurchaseStatus.FAILED:
                Log.Rachelle.Print(string.Format("StoreManager.OnThirdPartyPurchaseStatusResponse(): ThirdPartyID='{0}', Status={1} -- receipt can be removed from client now", thirdPartyPurchaseStatusResponse.ThirdPartyID, thirdPartyPurchaseStatusResponse.Status), new object[0]);
                break;

            case Network.ThirdPartyPurchaseStatusResponse.PurchaseStatus.IN_PROGRESS:
                Log.Rachelle.Print(string.Format("StoreManager.OnThirdPartyPurchaseStatusResponse(): ThirdPartyID='{0}' still in progress, leave receipt on client", thirdPartyPurchaseStatusResponse.ThirdPartyID), new object[0]);
                break;

            default:
                Debug.LogWarning(string.Format("StoreManager.OnThirdPartyPurchaseStatusResponse(): unexpected Status {0} received for third party ID '{1}'", thirdPartyPurchaseStatusResponse.Status, thirdPartyPurchaseStatusResponse.ThirdPartyID));
                break;
        }
        StoreMobilePurchase.ThirdPartyPurchaseStatus(thirdPartyPurchaseStatusResponse.ThirdPartyID, thirdPartyPurchaseStatusResponse.Status);
    }

    public bool RegisterAuthorizationExitListener(AuthorizationExitCallback callback)
    {
        return this.RegisterAuthorizationExitListener(callback, null);
    }

    public bool RegisterAuthorizationExitListener(AuthorizationExitCallback callback, object userData)
    {
        AuthorizationExitListener item = new AuthorizationExitListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_authExitListeners.Contains(item))
        {
            return false;
        }
        this.m_authExitListeners.Add(item);
        return true;
    }

    public bool RegisterStatusChangedListener(StatusChangedCallback callback)
    {
        return this.RegisterStatusChangedListener(callback, null);
    }

    public bool RegisterStatusChangedListener(StatusChangedCallback callback, object userData)
    {
        StatusChangedListener item = new StatusChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_statusChangedListeners.Contains(item))
        {
            return false;
        }
        this.m_statusChangedListeners.Add(item);
        return true;
    }

    public bool RegisterStoreAchievesListener(StoreAchievesCallback callback)
    {
        return this.RegisterStoreAchievesListener(callback, null);
    }

    public bool RegisterStoreAchievesListener(StoreAchievesCallback callback, object userData)
    {
        StoreAchievesListener item = new StoreAchievesListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_storeAchievesListeners.Contains(item))
        {
            this.m_storeAchievesListeners.Add(item);
        }
        return false;
    }

    public bool RegisterStoreHiddenListener(StoreHiddenCallback callback)
    {
        return this.RegisterStoreHiddenListener(callback, null);
    }

    public bool RegisterStoreHiddenListener(StoreHiddenCallback callback, object userData)
    {
        StoreHiddenListener item = new StoreHiddenListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (!this.m_storeHiddenListeners.Contains(item))
        {
            this.m_storeHiddenListeners.Add(item);
        }
        return false;
    }

    public bool RegisterStoreShownListener(StoreShownCallback callback)
    {
        return this.RegisterStoreShownListener(callback, null);
    }

    public bool RegisterStoreShownListener(StoreShownCallback callback, object userData)
    {
        StoreShownListener item = new StoreShownListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_storeShownListeners.Contains(item))
        {
            return false;
        }
        this.m_storeShownListeners.Add(item);
        return true;
    }

    public bool RegisterSuccessfulPurchaseAckListener(SuccessfulPurchaseAckCallback callback)
    {
        return this.RegisterSuccessfulPurchaseAckListener(callback, null);
    }

    public bool RegisterSuccessfulPurchaseAckListener(SuccessfulPurchaseAckCallback callback, object userData)
    {
        SuccessfulPurchaseAckListener item = new SuccessfulPurchaseAckListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_successfulPurchaseAckListeners.Contains(item))
        {
            return false;
        }
        this.m_successfulPurchaseAckListeners.Add(item);
        return true;
    }

    public bool RegisterSuccessfulPurchaseListener(SuccessfulPurchaseCallback callback)
    {
        return this.RegisterSuccessfulPurchaseListener(callback, null);
    }

    public bool RegisterSuccessfulPurchaseListener(SuccessfulPurchaseCallback callback, object userData)
    {
        SuccessfulPurchaseListener item = new SuccessfulPurchaseListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        if (this.m_successfulPurchaseListeners.Contains(item))
        {
            return false;
        }
        this.m_successfulPurchaseListeners.Add(item);
        return true;
    }

    private bool ReloadNoticesIfNecessary(Network.PurchaseErrorInfo.ErrorType errorType)
    {
        if ((this.m_activeMoneyOrGTAPPTransaction != null) && this.m_transactionIDsThatReloadedNotices.Contains(this.m_activeMoneyOrGTAPPTransaction.ID))
        {
            return false;
        }
        if (!this.IsConclusiveState(errorType))
        {
            return false;
        }
        if (this.m_activeMoneyOrGTAPPTransaction != null)
        {
            this.m_transactionIDsThatReloadedNotices.Add(this.m_activeMoneyOrGTAPPTransaction.ID);
        }
        object[] args = new object[] { errorType };
        Log.Rachelle.Print("StoreManager.ReloadNoticesIfNecessary: errorType = {0}, RELOADING", args);
        NetCache.Get().ReloadNetObject<NetCache.NetCacheProfileNotices>();
        return true;
    }

    public bool RemoveAuthorizationExitListener(AuthorizationExitCallback callback)
    {
        return this.RemoveAuthorizationExitListener(callback, null);
    }

    public bool RemoveAuthorizationExitListener(AuthorizationExitCallback callback, object userData)
    {
        AuthorizationExitListener item = new AuthorizationExitListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_authExitListeners.Remove(item);
    }

    public bool RemoveStatusChangedListener(StatusChangedCallback callback)
    {
        return this.RemoveStatusChangedListener(callback, null);
    }

    public bool RemoveStatusChangedListener(StatusChangedCallback callback, object userData)
    {
        StatusChangedListener item = new StatusChangedListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_statusChangedListeners.Remove(item);
    }

    public bool RemoveStoreAchievesListener(StoreAchievesCallback callback)
    {
        return this.RemoveStoreAchievesListener(callback, null);
    }

    public bool RemoveStoreAchievesListener(StoreAchievesCallback callback, object userData)
    {
        StoreAchievesListener item = new StoreAchievesListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_storeAchievesListeners.Remove(item);
    }

    public bool RemoveStoreHiddenListener(StoreHiddenCallback callback)
    {
        return this.RemoveStoreHiddenListener(callback, null);
    }

    public bool RemoveStoreHiddenListener(StoreHiddenCallback callback, object userData)
    {
        StoreHiddenListener item = new StoreHiddenListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_storeHiddenListeners.Remove(item);
    }

    public bool RemoveStoreShownListener(StoreShownCallback callback)
    {
        return this.RemoveStoreShownListener(callback, null);
    }

    public bool RemoveStoreShownListener(StoreShownCallback callback, object userData)
    {
        StoreShownListener item = new StoreShownListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_storeShownListeners.Remove(item);
    }

    public bool RemoveSuccessfulPurchaseAckListener(SuccessfulPurchaseAckCallback callback)
    {
        return this.RemoveSuccessfulPurchaseAckListener(callback, null);
    }

    public bool RemoveSuccessfulPurchaseAckListener(SuccessfulPurchaseAckCallback callback, object userData)
    {
        SuccessfulPurchaseAckListener item = new SuccessfulPurchaseAckListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_successfulPurchaseAckListeners.Remove(item);
    }

    public bool RemoveSuccessfulPurchaseListener(SuccessfulPurchaseCallback callback)
    {
        return this.RemoveSuccessfulPurchaseListener(callback, null);
    }

    public bool RemoveSuccessfulPurchaseListener(SuccessfulPurchaseCallback callback, object userData)
    {
        SuccessfulPurchaseListener item = new SuccessfulPurchaseListener();
        item.SetCallback(callback);
        item.SetUserData(userData);
        return this.m_successfulPurchaseListeners.Remove(item);
    }

    private void RemoveThirdPartyReceipt(string transactionID)
    {
        if (((HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD)) && !string.IsNullOrEmpty(transactionID))
        {
            StoreMobilePurchase.FinishTransactionForId(transactionID);
        }
    }

    private void RequestConfigIfNeeded(long now)
    {
        if (!this.ConfigLoaded && ((now - this.m_lastConfigRequestTime) >= this.m_configRequestDelayTicks))
        {
            this.m_configRequestDelayTicks = this.GetIncreasedRequestDelayTicks(this.m_configRequestDelayTicks, MIN_CONFIG_REQUEST_DELAY_TICKS);
            Log.Rachelle.Print(string.Format("StoreManager updated CONFIG delay, now waiting {0} seconds between requests", this.m_configRequestDelayTicks / 0x989680L), new object[0]);
            this.m_lastConfigRequestTime = now;
            Network.RequestBattlePayConfig();
        }
    }

    private void RequestStatusIfNeeded(long now)
    {
        if (this.ConfigLoaded)
        {
            bool flag = (((this.Status == TransactionStatus.UNKNOWN) || (this.Status == TransactionStatus.IN_PROGRESS_MONEY)) || ((this.Status == TransactionStatus.IN_PROGRESS_GOLD_GTAPP) || (this.Status == TransactionStatus.CHALLENGE_SUBMITTED))) || (TransactionStatus.CHALLENGE_CANCELED == this.Status);
            if ((!this.BattlePayAvailable || flag) && ((now - this.m_lastStatusRequestTime) >= this.m_statusRequestDelayTicks))
            {
                this.m_statusRequestDelayTicks = this.GetIncreasedRequestDelayTicks(this.m_statusRequestDelayTicks, MIN_STATUS_REQUEST_DELAY_TICKS);
                Log.Rachelle.Print(string.Format("StoreManager updated STATUS delay, now waiting {0} seconds between requests", this.m_statusRequestDelayTicks / 0x989680L), new object[0]);
                this.m_lastStatusRequestTime = now;
                Network.RequestBattlePayStatus();
            }
        }
    }

    private void ResolveFirstMoneyOrGTAPPTransactionIfPossible()
    {
        if (((this.m_firstMoneyOrGTAPPTransactionSet && this.NoticesReady) && (this.m_activeMoneyOrGTAPPTransaction != null)) && (this.m_outstandingPurchesNotices.Find(obj => obj.OriginData == this.m_activeMoneyOrGTAPPTransaction.ID) == null))
        {
            Log.Rachelle.Print(string.Format("StoreManager.ResolveFirstMoneyTransactionIfPossible(): no outstanding notices for transaction {0}; setting m_activeMoneyOrGTAPPTransaction = null", this.m_activeMoneyOrGTAPPTransaction), new object[0]);
            this.m_activeMoneyOrGTAPPTransaction = null;
        }
    }

    private void SetActiveMoneyOrGTAPPTransaction(long id, string productID, BattlePayProvider? provider, bool isGTAPP, bool tryToResolvePreviousTransactionNotices)
    {
        MoneyOrGTAPPTransaction transaction = new MoneyOrGTAPPTransaction(id, productID, provider, isGTAPP);
        bool flag = true;
        if (this.m_activeMoneyOrGTAPPTransaction != null)
        {
            if (transaction.Equals(this.m_activeMoneyOrGTAPPTransaction))
            {
                flag = !this.m_activeMoneyOrGTAPPTransaction.Provider.HasValue && provider.HasValue;
            }
            else if (UNKNOWN_TRANSACTION_ID != this.m_activeMoneyOrGTAPPTransaction.ID)
            {
                object[] args = new object[] { id, productID, isGTAPP, !provider.HasValue ? "UNKNOWN" : provider.Value.ToString(), this.m_activeMoneyOrGTAPPTransaction };
                Debug.LogWarning(string.Format("StoreManager.SetActiveMoneyOrGTAPPTransaction(id={0},productID='{1}',isGTAPP={2},provider={3}) does not match active money or GTAPP transaction '{4}'", args));
            }
        }
        if (flag)
        {
            Log.Rachelle.Print(string.Format("SetActiveMoneyOrGTAPPTransaction() {0}", transaction), new object[0]);
            Log.Yim.Print(string.Format("SetActiveMoneyOrGTAPPTransaction() {0}", transaction), new object[0]);
            this.m_activeMoneyOrGTAPPTransaction = transaction;
        }
        if (!this.m_firstMoneyOrGTAPPTransactionSet)
        {
            this.m_firstMoneyOrGTAPPTransactionSet = true;
            if (tryToResolvePreviousTransactionNotices)
            {
                this.ResolveFirstMoneyOrGTAPPTransactionIfPossible();
            }
        }
    }

    private void SetupLoadedStore(Store store)
    {
        if (store != null)
        {
            store.Hide();
            store.RegisterBuyWithMoneyListener(new Store.BuyWithMoneyCallback(this.OnStoreBuyWithMoney));
            store.RegisterBuyWithGoldGTAPPListener(new Store.BuyWithGoldGTAPPCallback(this.OnStoreBuyWithGTAPP));
            store.RegisterBuyWithGoldNoGTAPPListener(new Store.BuyWithGoldNoGTAPPCallback(this.OnStoreBuyWithGoldNoGTAPP));
            store.RegisterExitListener(new Store.ExitCallback(this.OnStoreExit));
            store.RegisterInfoListener(new Store.InfoCallback(this.OnStoreInfo));
            store.RegisterReadyListener(new Store.ReadyCallback(this.OnStoreComponentReady));
            this.OnStoreComponentReady(null);
        }
    }

    private bool ShouldEnablePurchaseAuthBackButton(StoreType storeType)
    {
        if (this.m_currentStoreType != StoreType.ARENA_STORE)
        {
            return false;
        }
        return true;
    }

    private void ShowCompletedAchieve()
    {
        bool enabled = this.m_completedAchieves.Count == 0;
        if (this.m_currentStoreType == StoreType.GENERAL_STORE)
        {
            ((GeneralStore) this.GetCurrentStore()).EnableClickCatcher(enabled);
        }
        if (!enabled)
        {
            Achievement quest = this.m_completedAchieves[0];
            this.m_completedAchieves.RemoveAt(0);
            QuestToast.ShowQuestToast(userData => this.ShowCompletedAchieve(), true, quest, false);
        }
    }

    private void ShowStore()
    {
        if (!this.m_licenseAchievesListenerRegistered)
        {
            AchieveManager.Get().RegisterLicenseAddedAchievesUpdatedListener(new AchieveManager.LicenseAddedAchievesUpdatedCallback(this.OnLicenseAddedAchievesUpdated));
            this.m_licenseAchievesListenerRegistered = true;
        }
        if ((this.Status == TransactionStatus.READY) && AchieveManager.Get().HasActiveLicenseAddedAchieves())
        {
            this.Status = TransactionStatus.WAIT_ZERO_COST_LICENSE;
        }
        Store currentStore = this.GetCurrentStore();
        bool flag = true;
        switch (this.m_currentStoreType)
        {
            case StoreType.GENERAL_STORE:
                if (!this.IsOpen())
                {
                    Debug.LogWarning("StoreManager.ShowStore(): Cannot show general store.. Store is not open");
                    if (this.m_showStoreData.m_exitCallback != null)
                    {
                        this.m_showStoreData.m_exitCallback(false, this.m_showStoreData.m_exitCallbackUserData);
                    }
                    flag = false;
                    break;
                }
                ((GeneralStore) currentStore).SetMode(this.m_showStoreData.m_storeMode);
                break;

            case StoreType.ARENA_STORE:
                currentStore.RegisterExitListener(this.m_showStoreData.m_exitCallback, this.m_showStoreData.m_exitCallbackUserData);
                break;

            case StoreType.ADVENTURE_STORE:
            {
                if (!this.IsOpen())
                {
                    Debug.LogWarning("StoreManager.ShowStore(): Cannot show adventure store.. Store is not open");
                    if (this.m_showStoreData.m_exitCallback != null)
                    {
                        this.m_showStoreData.m_exitCallback(false, this.m_showStoreData.m_exitCallbackUserData);
                    }
                    flag = false;
                    break;
                }
                AdventureStore store3 = (AdventureStore) currentStore;
                if (store3 != null)
                {
                    store3.SetAdventureProduct(this.m_showStoreData.m_storeProduct, this.m_showStoreData.m_storeProductData);
                }
                if (this.m_showStoreData.m_exitCallback != null)
                {
                    currentStore.RegisterExitListener(this.m_showStoreData.m_exitCallback, this.m_showStoreData.m_exitCallbackUserData);
                }
                break;
            }
        }
        if (flag)
        {
            currentStore.Show(new Store.DelOnStoreShown(this.OnStoreShown), this.m_showStoreData.m_isTotallyFake);
        }
        if ((this.IsOpen() && (this.Status == TransactionStatus.READY)) && (this.m_showStoreData.m_zeroCostTransactionData != null))
        {
            this.Status = TransactionStatus.WAIT_ZERO_COST_LICENSE;
            this.ActivateStoreCover();
            this.m_storePurchaseAuth.Show(null, true, true);
            Network.GetPurchaseMethod(this.m_showStoreData.m_zeroCostTransactionData.Bundle.ProductID, 1, this.m_currency);
        }
        this.m_waitingToShowStore = false;
    }

    private void ShowStoreWhenLoaded()
    {
        this.m_waitingToShowStore = true;
        if (!this.IsCurrentStoreLoaded())
        {
            this.Load(this.m_currentStoreType);
        }
        else
        {
            this.ShowStore();
        }
    }

    public void StartAdventureTransaction(ProductType product, int productData, Store.ExitCallback exitCallback, object exitCallbackUserData)
    {
        if (this.m_waitingToShowStore)
        {
            Log.Rachelle.Print("StoreManager.StartAdventureTransaction(): already waiting to show store", new object[0]);
        }
        else if (this.CanBuyProduct(product, productData))
        {
            this.m_currentStoreType = StoreType.ADVENTURE_STORE;
            this.m_showStoreData.m_exitCallback = exitCallback;
            this.m_showStoreData.m_exitCallbackUserData = exitCallbackUserData;
            this.m_showStoreData.m_isTotallyFake = false;
            this.m_showStoreData.m_storeProduct = product;
            this.m_showStoreData.m_storeProductData = productData;
            this.m_showStoreData.m_zeroCostTransactionData = null;
            this.ShowStoreWhenLoaded();
        }
    }

    public void StartArenaTransaction(Store.ExitCallback exitCallback, object exitCallbackUserData, bool isTotallyFake)
    {
        if (this.m_waitingToShowStore)
        {
            Log.Rachelle.Print("StoreManager.StartArenaTransaction(): already waiting to show store", new object[0]);
        }
        else
        {
            this.m_currentStoreType = StoreType.ARENA_STORE;
            this.m_showStoreData.m_exitCallback = exitCallback;
            this.m_showStoreData.m_exitCallbackUserData = null;
            this.m_showStoreData.m_isTotallyFake = isTotallyFake;
            this.m_showStoreData.m_storeProduct = ProductType.PRODUCT_TYPE_UNKNOWN;
            this.m_showStoreData.m_storeProductData = 0;
            this.m_showStoreData.m_zeroCostTransactionData = null;
            this.ShowStoreWhenLoaded();
        }
    }

    public void StartGeneralTransaction()
    {
        this.StartGeneralTransaction((GeneralStoreMode) s_defaultStoreMode);
    }

    public void StartGeneralTransaction(GeneralStoreMode mode)
    {
        if (this.m_waitingToShowStore)
        {
            Log.Rachelle.Print("StoreManager.StartGeneralTransaction(): already waiting to show store", new object[0]);
        }
        else
        {
            this.m_currentStoreType = StoreType.GENERAL_STORE;
            this.m_showStoreData.m_exitCallback = null;
            this.m_showStoreData.m_exitCallbackUserData = null;
            this.m_showStoreData.m_isTotallyFake = false;
            this.m_showStoreData.m_storeProduct = ProductType.PRODUCT_TYPE_UNKNOWN;
            this.m_showStoreData.m_storeProductData = 0;
            this.m_showStoreData.m_storeMode = mode;
            this.m_showStoreData.m_zeroCostTransactionData = null;
            this.ShowStoreWhenLoaded();
        }
    }

    public bool TransactionInProgress()
    {
        return (((((this.Status == TransactionStatus.IN_PROGRESS_MONEY) || (this.Status == TransactionStatus.IN_PROGRESS_GOLD_GTAPP)) || ((this.Status == TransactionStatus.IN_PROGRESS_GOLD_NO_GTAPP) || (this.Status == TransactionStatus.WAIT_THIRD_PARTY_RECEIPT))) || (this.Status == TransactionStatus.WAIT_THIRD_PARTY_INIT)) || (TransactionStatus.WAIT_ZERO_COST_LICENSE == this.Status));
    }

    public void UnloadAndFreeMemory()
    {
        foreach (KeyValuePair<StoreType, Store> pair in this.m_stores)
        {
            if (pair.Value != null)
            {
                UnityEngine.Object.Destroy(pair.Value);
            }
        }
        this.m_stores.Clear();
        if (this.m_storePurchaseAuth != null)
        {
            UnityEngine.Object.Destroy(this.m_storePurchaseAuth.gameObject);
            this.m_storePurchaseAuth = null;
        }
        if (this.m_storeSummary != null)
        {
            UnityEngine.Object.Destroy(this.m_storeSummary.gameObject);
            this.m_storeSummary = null;
        }
        if (this.m_storeSendToBAM != null)
        {
            UnityEngine.Object.Destroy(this.m_storeSendToBAM.gameObject);
            this.m_storeSendToBAM = null;
        }
        if (this.m_storeLegalBAMLinks != null)
        {
            UnityEngine.Object.Destroy(this.m_storeLegalBAMLinks.gameObject);
            this.m_storeLegalBAMLinks = null;
        }
        if (this.m_storeDoneWithBAM != null)
        {
            UnityEngine.Object.Destroy(this.m_storeDoneWithBAM.gameObject);
            this.m_storeDoneWithBAM = null;
        }
        if (this.m_storeChallengePrompt != null)
        {
            UnityEngine.Object.Destroy(this.m_storeChallengePrompt.gameObject);
            this.m_storeChallengePrompt = null;
        }
    }

    private void WillReset()
    {
        ApplicationMgr.Get().WillReset -= new System.Action(this.WillReset);
        Network network = Network.Get();
        network.RemoveNetHandler(BattlePayStatusResponse.PacketID.ID, new Network.NetHandler(this.OnBattlePayStatusResponse));
        network.RemoveNetHandler(BattlePayConfigResponse.PacketID.ID, new Network.NetHandler(this.OnBattlePayConfigResponse));
        network.RemoveNetHandler(PegasusUtil.PurchaseMethod.PacketID.ID, new Network.NetHandler(this.OnPurchaseMethod));
        network.RemoveNetHandler(PegasusUtil.PurchaseResponse.PacketID.ID, new Network.NetHandler(this.OnPurchaseResponse));
        network.RemoveNetHandler(CancelPurchaseResponse.PacketID.ID, new Network.NetHandler(this.OnPurchaseCanceledResponse));
        network.RemoveNetHandler(PurchaseWithGoldResponse.PacketID.ID, new Network.NetHandler(this.OnPurchaseViaGoldResponse));
        network.RemoveNetHandler(PegasusUtil.ThirdPartyPurchaseStatusResponse.PacketID.ID, new Network.NetHandler(this.OnThirdPartyPurchaseStatusResponse));
        StoreMobilePurchase.Reset();
        DestroyInstance();
    }

    private bool BattlePayAvailable
    {
        get
        {
            return this.m_battlePayAvailable;
        }
        set
        {
            this.m_battlePayAvailable = value;
            this.FireStatusChangedEventIfNeeded();
        }
    }

    private bool ConfigLoaded
    {
        get
        {
            return this.m_configLoaded;
        }
        set
        {
            this.m_configLoaded = value;
            this.FireStatusChangedEventIfNeeded();
        }
    }

    private bool FeaturesReady
    {
        get
        {
            return this.m_featuresReady;
        }
        set
        {
            this.m_featuresReady = value;
            this.FireStatusChangedEventIfNeeded();
        }
    }

    private bool NoticesReady
    {
        get
        {
            return this.m_noticesReady;
        }
        set
        {
            this.m_noticesReady = value;
            this.FireStatusChangedEventIfNeeded();
        }
    }

    private TransactionStatus Status
    {
        get
        {
            return this.m_status;
        }
        set
        {
            if ((this.m_lastCancelRequestTime == 0) && (this.m_status == TransactionStatus.UNKNOWN))
            {
                this.m_lastCancelRequestTime = DateTime.Now.Ticks;
            }
            this.m_status = value;
            this.FireStatusChangedEventIfNeeded();
        }
    }

    [CompilerGenerated]
    private sealed class <ConfirmActiveMoneyTransaction>c__AnonStorey348
    {
        internal long id;

        internal bool <>m__1A1(NetCache.ProfileNoticePurchase obj)
        {
            return (obj.OriginData == this.id);
        }
    }

    public delegate void AuthorizationExitCallback(object userData);

    private class AuthorizationExitListener : EventListener<StoreManager.AuthorizationExitCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    private enum PurchaseErrorSource
    {
        FROM_PURCHASE_METHOD_RESPONSE,
        FROM_STATUS_OR_PURCHASE_RESPONSE,
        FROM_PREVIOUS_PURCHASE
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ShowStoreData
    {
        public bool m_isTotallyFake;
        public Store.ExitCallback m_exitCallback;
        public object m_exitCallbackUserData;
        public ProductType m_storeProduct;
        public int m_storeProductData;
        public GeneralStoreMode m_storeMode;
        public StoreManager.ZeroCostTransactionData m_zeroCostTransactionData;
    }

    public delegate void StatusChangedCallback(bool isOpen, object userData);

    private class StatusChangedListener : EventListener<StoreManager.StatusChangedCallback>
    {
        public void Fire(bool isOpen)
        {
            base.m_callback(isOpen, base.m_userData);
        }
    }

    public delegate void StoreAchievesCallback(Network.Bundle bundle, PaymentMethod paymentMethod, object userData);

    private class StoreAchievesData
    {
        public StoreAchievesData(Network.Bundle bundle, PaymentMethod paymentMethod)
        {
            this.Bundle = bundle;
            this.MethodOfPayment = paymentMethod;
        }

        public Network.Bundle Bundle { get; private set; }

        public PaymentMethod MethodOfPayment { get; private set; }
    }

    private class StoreAchievesListener : EventListener<StoreManager.StoreAchievesCallback>
    {
        public void Fire(Network.Bundle bundle, PaymentMethod paymentMethod)
        {
            base.m_callback(bundle, paymentMethod, base.m_userData);
        }
    }

    public delegate void StoreHiddenCallback(object userData);

    private class StoreHiddenListener : EventListener<StoreManager.StoreHiddenCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public delegate void StoreShownCallback(object userData);

    private class StoreShownListener : EventListener<StoreManager.StoreShownCallback>
    {
        public void Fire()
        {
            base.m_callback(base.m_userData);
        }
    }

    public delegate void SuccessfulPurchaseAckCallback(Network.Bundle bundle, PaymentMethod purchaseMethod, object userData);

    private class SuccessfulPurchaseAckListener : EventListener<StoreManager.SuccessfulPurchaseAckCallback>
    {
        public void Fire(Network.Bundle bundle, PaymentMethod paymentMethod)
        {
            base.m_callback(bundle, paymentMethod, base.m_userData);
        }
    }

    public delegate void SuccessfulPurchaseCallback(Network.Bundle bundle, PaymentMethod purchaseMethod, object userData);

    private class SuccessfulPurchaseListener : EventListener<StoreManager.SuccessfulPurchaseCallback>
    {
        public void Fire(Network.Bundle bundle, PaymentMethod paymentMethod)
        {
            base.m_callback(bundle, paymentMethod, base.m_userData);
        }
    }

    private enum TransactionStatus
    {
        UNKNOWN,
        IN_PROGRESS_MONEY,
        IN_PROGRESS_GOLD_GTAPP,
        IN_PROGRESS_GOLD_NO_GTAPP,
        READY,
        WAIT_ZERO_COST_LICENSE,
        WAIT_METHOD_OF_PAYMENT,
        WAIT_THIRD_PARTY_INIT,
        WAIT_THIRD_PARTY_RECEIPT,
        WAIT_CONFIRM,
        WAIT_RISK,
        CHALLENGE_SUBMITTED,
        CHALLENGE_CANCELED,
        USER_CANCELING,
        AUTO_CANCELING
    }

    private class ZeroCostTransactionData
    {
        public ZeroCostTransactionData(Network.Bundle bundle)
        {
            this.Bundle = bundle;
        }

        public Network.Bundle Bundle { get; private set; }
    }
}

