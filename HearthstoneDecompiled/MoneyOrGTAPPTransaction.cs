using System;
using System.Runtime.CompilerServices;

public class MoneyOrGTAPPTransaction
{
    public static readonly BattlePayProvider? UNKNOWN_PROVIDER;

    public MoneyOrGTAPPTransaction(long id, string productID, BattlePayProvider? provider, bool isGTAPP)
    {
        this.ID = id;
        this.ProductID = productID;
        this.IsGTAPP = isGTAPP;
        this.Provider = provider;
        this.ClosedStore = false;
    }

    public override bool Equals(object obj)
    {
        MoneyOrGTAPPTransaction transaction = obj as MoneyOrGTAPPTransaction;
        if (transaction == null)
        {
            return false;
        }
        bool flag = false;
        if (this.Provider.HasValue && transaction.Provider.HasValue)
        {
            flag = this.Provider.Value == transaction.Provider.Value;
        }
        else
        {
            flag = true;
        }
        return (((transaction.ID == this.ID) && (transaction.ProductID == this.ProductID)) && flag);
    }

    public override int GetHashCode()
    {
        return (this.ID.GetHashCode() * this.ProductID.GetHashCode());
    }

    public bool ShouldShowMiniSummary()
    {
        return (((StoreManager.HAS_THIRD_PARTY_APP_STORE != null) && (ApplicationMgr.GetAndroidStore() != AndroidStore.BLIZZARD)) || this.ClosedStore);
    }

    public override string ToString()
    {
        object[] args = new object[] { this.ID, this.ProductID, this.IsGTAPP, !this.Provider.HasValue ? "UNKNOWN" : this.Provider.Value.ToString() };
        return string.Format("[MoneyOrGTAPPTransaction: ID={0},ProductID='{1}',IsGTAPP={2},Provider={3}]", args);
    }

    public bool ClosedStore { get; set; }

    public long ID { get; private set; }

    public bool IsGTAPP { get; private set; }

    public string ProductID { get; private set; }

    public BattlePayProvider? Provider { get; private set; }
}

