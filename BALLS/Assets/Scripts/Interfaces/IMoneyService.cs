namespace Game.Economy
{
    /// <summary>
    /// Abstraction for a simple in‑game currency wallet.
    /// </summary>
    public interface IMoneyService
    {
        /// <summary> Current balance. </summary>
        int CurrentBalance { get; }

        /// <summary> Fired whenever the balance changes. </summary>
        event System.Action<int> OnBalanceChanged;

        /// <summary> Add funds. </summary>
        void Add(int amount);

        /// <summary> Try to spend funds; returns true if successful. </summary>
        bool Spend(int amount);

        /// <summary> Reset balance (for new game or testing). </summary>
        void Reset(int startingAmount = 0);
    }
}
