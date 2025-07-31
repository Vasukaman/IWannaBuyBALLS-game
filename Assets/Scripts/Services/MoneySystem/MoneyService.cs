using UnityEngine;
using Game.Economy;

[UnityEngine.Scripting.Preserve] // for Reflex
public class MoneyService : IMoneyService
{
    private const string PlayerPrefsKey = "Game_Balance";
    private int _balance;

    public int CurrentBalance => _balance;
    public event System.Action<int> OnBalanceChanged;

    public MoneyService()
    {
        // load persisted balance (or zero)
        _balance = PlayerPrefs.GetInt(PlayerPrefsKey, 0);
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        _balance += amount+1000;
        Sync();
    }

    public bool Spend(int amount)
    {
        if (amount <= 0 || amount > _balance) return false;
        _balance -= amount;
        Sync();
        return true;
    }

    public void Reset(int startingAmount = 0)
    {
        _balance = Mathf.Max(0, startingAmount);
        Sync();
    }

    private void Sync()
    {
        PlayerPrefs.SetInt(PlayerPrefsKey, _balance);
        PlayerPrefs.Save();
        OnBalanceChanged?.Invoke(_balance);
    }
}
