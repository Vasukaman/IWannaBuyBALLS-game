// Filename: BallData.cs
using System;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// The pure data model (the "Brain") for a Ball. Contains all state and logic
    /// that is independent of the Unity engine.
    /// </summary>
    public class BallData
    {
        public event Action<int> OnPriceChanged;

        public int CurrentPrice { get; private set; }
        public int BasePrice { get; private set; }

        public BallData(int basePrice)
        {
            BasePrice = basePrice;
            CurrentPrice = basePrice;
        }

        public void SetPrice(int newPrice)
        {
            int oldPrice = CurrentPrice;
            CurrentPrice = Math.Max(1, newPrice);

            if (CurrentPrice != oldPrice)
            {
                OnPriceChanged?.Invoke(CurrentPrice);
            }
        }

        public void ModifyPrice(Func<int, int> modifier) => SetPrice(modifier(CurrentPrice));
        public void AddPrice(int amount) => ModifyPrice(old => old + amount);
        public void MultiplyPrice(float multiplier) => ModifyPrice(old => (int)Math.Round(old * multiplier));
        public void SubtractPrice(int amount) => ModifyPrice(old => old - amount);
        public void ResetToBase() => SetPrice(BasePrice);
    }
}